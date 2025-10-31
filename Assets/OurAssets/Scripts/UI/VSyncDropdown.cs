using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class VSyncDropdown : MonoBehaviour
{
    TMP_Dropdown dropdown;

    void Awake() => dropdown = GetComponent<TMP_Dropdown>();

    void OnEnable()
    {
        if (UserSettingsManager.Instance == null) return;
        int vSyncMode = UserSettingsManager.Instance.VSyncCount;
        int vSyncClamp = Mathf.Clamp(vSyncMode, 0, 2);
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.value = vSyncClamp;
        dropdown.onValueChanged.AddListener(ChangeVsync);
        UserSettingsManager.Instance.VSyncCount = vSyncClamp;
    }

    void ChangeVsync(int index) => UserSettingsManager.Instance.VSyncCount = index;
}
