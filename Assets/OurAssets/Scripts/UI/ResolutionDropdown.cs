using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class ResolutionDropdown : MonoBehaviour
{
    readonly string[] resolutions = {
        "1280x720",
        "1920x1080",
        "2560x1440",
        "3840x2160"
    };
    readonly List<string> resList = new List<string>();

    TMP_Dropdown dropdown;

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        resList.AddRange(resolutions);
    }

    void OnEnable()
    {
        if (UserSettingsManager.Instance == null) return;
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.ClearOptions();
        string resolution = $"{UserSettingsManager.Instance.Resolution[0]}x{UserSettingsManager.Instance.Resolution[1]}";
        if (!resList.Contains<string>(resolution)) resList.Insert(0, "CUSTOM");
        dropdown.AddOptions(resList);
        dropdown.value = resList[0] == "CUSTOM" ? 0 : resList.IndexOf(resolution);
        dropdown.onValueChanged.AddListener(ChangeResolution);
    }

    void ChangeResolution(int index)
    {
        string resolution = dropdown.options[index].text;
        if (resList.Contains<string>("CUSTOM") && resolution != "CUSTOM")
        {
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.ClearOptions();
            resList.RemoveAt(0);
            dropdown.AddOptions(resList);
            dropdown.value = resList.IndexOf(resolution);
            dropdown.onValueChanged.AddListener(ChangeResolution);
        }
        if (UserSettingsManager.Instance == null) return;
        int width = resolution switch
        {
            "1280x720" => 1280,
            "1920x1080" => 1920,
            "2560x1440" => 2560,
            "3840x2160" => 3840,
            _ => UserSettingsManager.Instance.Resolution[0]
        };
        int height = resolution switch
        {
            "1280x720" => 720,
            "1920x1080" => 1080,
            "2560x1440" => 1440,
            "3840x2160" => 2160,
            _ => UserSettingsManager.Instance.Resolution[1]
        };
        UserSettingsManager.Instance.Resolution = new int[2] { width, height };
    }
}
