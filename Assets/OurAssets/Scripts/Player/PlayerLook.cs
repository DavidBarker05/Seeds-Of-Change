using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField]
    Camera cam;
    [SerializeField, Min(0f)]
    float mouseHorizontalSensitivity = 0.2f;
    [SerializeField, Min(0f)]
    float mouseVerticalSensitivity = 0.225f;
    [SerializeField, Min(0f)]
    float controllerHorizontalSensitivity = 180f;
    [SerializeField, Min(0f)]
    float controllerVerticalSensitivity = 202.5f;
    [SerializeField, Range(-90f, 0f)]
    float minVerticalAngle = -80f;
    [SerializeField, Range(0f, 90f)]
    float maxVerticalAngle = 80f;

    float pitch = 0f;

    void Update()
    {
        if (GameManager.Instance?.IsPaused ?? false) return;
        float mX = InputManagerScript.Instance?.LookInput.x ?? 0f;
        float mY = InputManagerScript.Instance?.LookInput.y ?? 0f;
        float hSens = (InputManagerScript.Instance?.CurrentLookDevice is Mouse) ? mouseHorizontalSensitivity : (controllerHorizontalSensitivity * Time.deltaTime);
        float vSens = (InputManagerScript.Instance?.CurrentLookDevice is Mouse) ? mouseVerticalSensitivity : (controllerVerticalSensitivity * Time.deltaTime);
        float yaw = mX * hSens;// * (UserSettingsManager.Instance?.UserSettings.sensitivityMultiplier ?? 1);
        pitch -= mY * vSens;// * (UserSettingsManager.Instance?.UserSettings.sensitivityMultiplier ?? 1);
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        if (cam != null) cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        transform.Rotate(Vector3.up, yaw);
    }
}
