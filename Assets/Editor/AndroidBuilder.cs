using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AndroidBuilder
{
	private const string PackageName = "com.shishkabob.cardwars";
	private const string OutputDir = "Builds/Android";
	private const string ApkName = "CardWars.apk";

	[MenuItem("Build/Android APK")]
	public static void BuildAPK()
	{
		PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageName);
		PlayerSettings.Android.bundleVersionCode = Mathf.Max(1, PlayerSettings.Android.bundleVersionCode);
		PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
		PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
		PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

		if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
		{
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
		}

		Directory.CreateDirectory(OutputDir);
		string outputPath = Path.Combine(OutputDir, ApkName);

		List<string> scenes = new List<string>();
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
		{
			if (scene.enabled)
			{
				scenes.Add(scene.path);
			}
		}

		BuildPlayerOptions options = new BuildPlayerOptions
		{
			scenes = scenes.ToArray(),
			locationPathName = outputPath,
			target = BuildTarget.Android,
			targetGroup = BuildTargetGroup.Android,
			options = BuildOptions.None,
		};

		Debug.Log("[AndroidBuilder] Building APK to " + outputPath);
		BuildPipeline.BuildPlayer(options);
		Debug.Log("[AndroidBuilder] Build finished: " + outputPath);
	}
}
