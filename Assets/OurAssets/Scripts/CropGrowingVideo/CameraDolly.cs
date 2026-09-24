using UnityEngine;
using UnityEngine.Events;

public class CameraDolly : MonoBehaviour
{
    [SerializeField]
    Camera m_Camera;
    [SerializeField]
    Transform m_Start;
    [SerializeField]
    Transform m_Control;
    [SerializeField]
    Transform m_End;
    [SerializeField]
    AnimationCurve m_AnimationCurve;
    [Header("Editor Preview Only")]
    [SerializeField, Range(0f, 1f)]
    float m_PreviewT;
    [SerializeField]
    int m_PathResolution = 20;
    [SerializeField]
    float m_GizmoDirectionLength = 0.5f;

    public float StartTime => m_AnimationCurve.keys[0].time;
    public float EndTime => m_AnimationCurve.keys[^1].time;
    public float MovementDuration => EndTime - StartTime;
    public bool IsPlaying { get; private set; }

    public float CurrentTime { get; private set; }

    public void ResetDolly()
    {
        CurrentTime = StartTime;
        IsPlaying = false;
        m_Camera.transform.SetPositionAndRotation(m_Start.position, m_Start.rotation);
    }

    public void StartDolly() => IsPlaying = true;

    public void UpdateDolly(float deltaTime)
    {
        if (!IsPlaying) return;
        CurrentTime += deltaTime;
        bool finished = CurrentTime >= EndTime;
        if (finished) CurrentTime = EndTime;
        (Vector3 position, Quaternion rotation) = GetPoseAtTime(CurrentTime);
        m_Camera.transform.SetPositionAndRotation(position, rotation);
        if (finished) IsPlaying = false;
    }

    (Vector3 position, Quaternion rotation) GetPoseAtTime(float time)
    {
        float t = m_AnimationCurve.Evaluate(time);
        Vector3 position = QuadraticBezier(m_Start.position, m_Control.position, m_End.position, t);
        Quaternion rotation = Quaternion.Slerp(m_Start.rotation, m_End.rotation, t);
        return (position, rotation);
    }

    static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }

    void OnDrawGizmosSelected()
    {
        if (m_Start == null || m_Control == null || m_End == null || m_AnimationCurve == null) return;
        if (m_AnimationCurve.keys.Length < 2) return;
        Gizmos.color = Color.yellow;
        Vector3 prev = m_Start.position;
        for (int i = 1; i <= m_PathResolution; i++)
        {
            float t = i / (float)m_PathResolution;
            Vector3 point = QuadraticBezier(m_Start.position, m_Control.position, m_End.position, t);
            Gizmos.DrawLine(prev, point);
            prev = point;
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(m_Control.position, 0.15f);
        Gizmos.DrawLine(m_Start.position, m_Control.position);
        Gizmos.DrawLine(m_Control.position, m_End.position);
        for (int i = 0; i <= m_PathResolution; i++)
        {
            float time = Mathf.Lerp(StartTime, EndTime, i / (float)m_PathResolution);
            (Vector3 pos, Quaternion rot) = GetPoseAtTime(time);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pos, 0.08f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pos, pos + rot * Vector3.forward * m_GizmoDirectionLength);
        }
        float previewTime = Mathf.Lerp(StartTime, EndTime, m_PreviewT);
        (Vector3 previewPos, Quaternion previewRot) = GetPoseAtTime(previewTime);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(previewPos, 0.12f);
        Gizmos.DrawLine(previewPos, previewPos + previewRot * Vector3.forward * m_GizmoDirectionLength * 1.5f);
        if (m_Camera != null)
        {
            Gizmos.matrix = Matrix4x4.TRS(previewPos, previewRot, Vector3.one);
            Gizmos.DrawFrustum(Vector3.zero, m_Camera.fieldOfView, m_Camera.farClipPlane * 0.1f,
                m_Camera.nearClipPlane, m_Camera.aspect);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}