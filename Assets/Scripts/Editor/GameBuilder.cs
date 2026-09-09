using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor
{
    public static class GameBuilder
    {
        [MenuItem("Space Defender/Build/Build Windows Standalone", false, 30)]
        public static void BuildWindowsStandalone()
        {
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Windows");
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            var buildPath = Path.Combine(outputDir, "SpaceDefender.exe");
            var scenes = new[] { "Assets/Scenes/SampleScene.unity" };

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"[GameBuilder] Starting StandaloneWindows64 build to: {buildPath}...");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
                Debug.Log(
                    $"[GameBuilder] Build SUCCEEDED! Total size: {summary.totalSize / (1024 * 1024):F1} MB at {buildPath}");
            else
                Debug.LogError($"[GameBuilder] Build FAILED with {summary.totalErrors} errors.");
        }
    }
}