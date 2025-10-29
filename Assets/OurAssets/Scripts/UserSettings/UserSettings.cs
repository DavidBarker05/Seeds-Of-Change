[System.Serializable]
public class UserSettings
{
    public int[] resolution = new int[2];
    public int vSyncCount;
    public float horizontalSensitivityMultiplier;
    public float verticalSensitivityMultiplier;

    public UserSettings()
    {
        resolution = new int[2] { 1920, 1080 };
        vSyncCount = 1;
        horizontalSensitivityMultiplier = 1f;
        verticalSensitivityMultiplier = 1f;
    }

    public UserSettings(UserSettings other)
    {
        resolution = other.resolution;
        vSyncCount = other.vSyncCount;
        horizontalSensitivityMultiplier = other.horizontalSensitivityMultiplier;
        verticalSensitivityMultiplier = other.verticalSensitivityMultiplier;
    }
}
