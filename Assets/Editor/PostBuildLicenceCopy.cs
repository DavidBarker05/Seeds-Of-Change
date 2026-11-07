using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class PostBuildLicenceCopy : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    static readonly string s_ThirdPartyAssetsFolder = "ThirdPartyAssets";
    static readonly string s_LicencesFile = "LICENSES.txt";

    public void OnPostprocessBuild(BuildReport report)
    {
        string tpaPath = Path.Combine(Application.dataPath, s_ThirdPartyAssetsFolder);
        string buildPath = Path.GetDirectoryName(report.summary.outputPath);
        CopyFile(s_LicencesFile, tpaPath, buildPath);
    }

    void CopyFile(string fileName, string fromDir, string toDir)
    {
        string fromPath = Path.Combine(fromDir, fileName);
        if (!Directory.Exists(toDir)) Directory.CreateDirectory(toDir);
        string toPath = Path.Combine(toDir, fileName);
        if (!File.Exists(fromPath))
        {
            Debug.LogError($"{fileName} doesn't exist within {fromDir}");
            return;
        }
        try
        {
            File.Copy(fromPath, toPath, overwrite: true);
        }
        catch (IOException ex)
        {
            Debug.LogError($"Post-build copy failed for {fileName}: {ex.Message}");
        }
    }
}
