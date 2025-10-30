using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class UserSettingsManager : MonoBehaviour
{
    public static UserSettingsManager Instance { get; private set; }

    string path = "";
    UserSettings currentSettings;

    public int[] Resolution
    {
        get => currentSettings.resolution;
        set
        {
            int width = value.Length > 0 ? Mathf.Max(value[0], 1024) : currentSettings.resolution[0];
            int height = value.Length >= 2 ? Mathf.Max(value[1], 576) : width * 9 / 16;
            currentSettings.resolution = new int[2] { width, height };
            Screen.SetResolution(width, height, true);
        }
    }

    public int VSyncCount
    {
        get => currentSettings.vSyncCount;
        set
        {
            int count = Mathf.Clamp(value, 0, 2);
            currentSettings.vSyncCount = count;
            QualitySettings.vSyncCount = count;
        }
    }

    public float HorizontalSensitivityMultiplier
    {
        get => currentSettings.horizontalSensitivityMultiplier;
        set
        {
            float multiplier = Mathf.Clamp(value, 0.5f, 2f);
            currentSettings.horizontalSensitivityMultiplier = multiplier;
        }
    }

    public float VerticalSensitivityMultiplier
    {
        get => currentSettings.verticalSensitivityMultiplier;
        set
        {
            float multiplier = Mathf.Clamp(value, 0.5f, 2f);
            currentSettings.verticalSensitivityMultiplier = multiplier;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        path = Path.Combine(Application.persistentDataPath, "user_settings.json");
        currentSettings = new UserSettings();
        if (File.Exists(path)) LoadSettings();
        SaveSettings();
    }

    void OnApplicationQuit() => SaveSettings();

    void OnApplicationPause(bool pause)
    {
        if (pause) SaveSettings();
    }

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(currentSettings, prettyPrint: true);
        File.WriteAllText(path, json);
    }

    void LoadSettings()
    {
        string json = File.ReadAllText(path);
        UserSettings settings = JsonUtility.FromJson<UserSettings>(json);
        // Make sure the settings are valid before actually using them for the game
        Resolution = new int[2] { settings.resolution[0], settings.resolution[1] };
        VSyncCount = settings.vSyncCount;
        HorizontalSensitivityMultiplier = settings.horizontalSensitivityMultiplier;
        VerticalSensitivityMultiplier = settings.verticalSensitivityMultiplier;
    }
}
