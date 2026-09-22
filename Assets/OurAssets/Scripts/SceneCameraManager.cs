using UnityEngine;
using UnityEngine.InputSystem;

public class SceneCameraManager : MonoBehaviour
{
    public static SceneCameraManager Instance { get; private set; }

    [SerializeField]
    Camera m_Camera;
    [SerializeField]
    Transform[] m_CameraTransforms;

    int m_CurrentCameraTransform;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        if (Instance != this) return;
        MoveCameraToIndex(0);
        InputManagerScript.Instance.AddTrailerHotkeyAction(2, GoToPrevCamera);
        InputManagerScript.Instance.AddTrailerHotkeyAction(3, GoToNextCamera);
    }

    void OnDestroy()
    {
        if (Instance != this) return;
        InputManagerScript.Instance.RemoveTrailerHotkeyAction(2, GoToPrevCamera);
        InputManagerScript.Instance.RemoveTrailerHotkeyAction(3, GoToNextCamera);
    }

    void MoveCameraToIndex(int index)
    {
        if (m_CameraTransforms.Length == 0 || index < 0 || index >= m_CameraTransforms.Length) return;
        m_CurrentCameraTransform = index;
        m_Camera.transform.SetPositionAndRotation(m_CameraTransforms[index].position, m_CameraTransforms[index].rotation);
    }

    void GoToPrevCamera(InputAction.CallbackContext ctx) => MoveCameraToIndex(m_CurrentCameraTransform - 1);

    void GoToNextCamera(InputAction.CallbackContext ctx) => MoveCameraToIndex(m_CurrentCameraTransform + 1);
}
