#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Automation script to compile the Unity scene into a standalone Windows 64-bit executable.
/// </summary>
public class BuildScript
{
    [MenuItem("Infested City Run/Build Windows Executable")]
    public static void PerformBuild()
    {
        string[] scenes = { "Assets/Scenes/GameplayScene.unity" };
        string buildPath = "Build/InfestedCityRun.exe";

        Debug.Log("Starting Infested City Run Standalone Windows 64-bit Build...");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Standalone Build Succeeded! Output directory: " + summary.outputPath);
            Debug.Log("Total size: " + summary.totalSize + " bytes");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Standalone Build Failed! Check Unity Editor logs for compile details.");
        }
    }
}
#endif
