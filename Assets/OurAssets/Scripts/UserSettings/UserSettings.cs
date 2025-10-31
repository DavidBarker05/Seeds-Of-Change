[System.Serializable]
public class UserSettings
{
    public int[] resolution = new int[2];
    public int vSyncCount;

    public UserSettings()
    {
        resolution = new int[2] { 1920, 1080 };
        vSyncCount = 1;
    }

    public UserSettings(UserSettings other)
    {
        resolution = other.resolution;
        vSyncCount = other.vSyncCount;
    }
}
