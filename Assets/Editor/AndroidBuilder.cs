using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AndroidBuilder
{
	private const string PackageName = "com.shishkabob.cardwars";
	private const string OutputDir = "Builds/Android";
	private const string ApkName = "CardWars.apk";

	private static readonly string[] StreamingAssetsToShelve =
	{
		"StreamingAssets/CharacterFaceAnim",
	};

	[MenuItem("Build/Android APK")]
	public static void BuildAPK()
	{
		PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageName);
		PlayerSettings.Android.bundleVersionCode = Mathf.Max(1, PlayerSettings.Android.bundleVersionCode);
		PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
		PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
		PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);

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

		List<string> shelved = ShelveLargeStreamingAssets();
		try
		{
			Debug.Log("[AndroidBuilder] Building APK to " + outputPath);
			BuildPipeline.BuildPlayer(options);
			Debug.Log("[AndroidBuilder] Build finished: " + outputPath);
		}
		finally
		{
			RestoreLargeStreamingAssets(shelved);
		}
	}

	private static string GetShelfRoot()
	{
		string projectRoot = Path.GetDirectoryName(Application.dataPath);
		return Path.Combine(projectRoot, "AndroidBuildShelf");
	}

	private static List<string> ShelveLargeStreamingAssets()
	{
		List<string> moved = new List<string>();
		string shelfRoot = GetShelfRoot();
		Directory.CreateDirectory(shelfRoot);

		bool changed = false;
		foreach (string relativePath in StreamingAssetsToShelve)
		{
			string assetPath = Path.Combine(Application.dataPath, relativePath);
			if (!Directory.Exists(assetPath))
			{
				continue;
			}

			string shelved = Path.Combine(shelfRoot, relativePath.Replace('/', '_'));
			if (Directory.Exists(shelved))
			{
				Directory.Delete(shelved, true);
			}
			Directory.Move(assetPath, shelved);

			string metaPath = assetPath + ".meta";
			string shelvedMeta = shelved + ".meta";
			if (File.Exists(metaPath))
			{
				if (File.Exists(shelvedMeta))
				{
					File.Delete(shelvedMeta);
				}
				File.Move(metaPath, shelvedMeta);
			}

			moved.Add(relativePath);
			Debug.Log("[AndroidBuilder] Shelved " + relativePath + " to " + shelved);
			changed = true;
		}

		if (changed)
		{
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}
		return moved;
	}

	private static void RestoreLargeStreamingAssets(List<string> shelvedPaths)
	{
		if (shelvedPaths == null || shelvedPaths.Count == 0)
		{
			return;
		}

		string shelfRoot = GetShelfRoot();
		foreach (string relativePath in shelvedPaths)
		{
			string assetPath = Path.Combine(Application.dataPath, relativePath);
			string shelved = Path.Combine(shelfRoot, relativePath.Replace('/', '_'));
			if (!Directory.Exists(shelved))
			{
				continue;
			}

			if (Directory.Exists(assetPath))
			{
				Directory.Delete(assetPath, true);
			}
			Directory.Move(shelved, assetPath);

			string metaPath = assetPath + ".meta";
			string shelvedMeta = shelved + ".meta";
			if (File.Exists(shelvedMeta))
			{
				if (File.Exists(metaPath))
				{
					File.Delete(metaPath);
				}
				File.Move(shelvedMeta, metaPath);
			}
			Debug.Log("[AndroidBuilder] Restored " + relativePath);
		}

		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
	}
}
