#if UNITY_CLOUD_BUILD
using UnityEngine;
using System.Collections.Generic;
using MiniJSON;
using UnityEditor;

public class UnityCloudPreProcess {
	public static void PreExport() {
		var manifest = (TextAsset) Resources.Load("UnityCloudBuildManifest.json");
		if (manifest != null) {
			var manifestDict = Json.Deserialize(manifest.text) as Dictionary<string,object>;
			PlayerSettings.SetApplicationIdentifier (BuildTargetGroup.Android, manifestDict ["buildNumber"].ToString ());
			PlayerSettings.SetApplicationIdentifier (BuildTargetGroup.iOS, manifestDict ["buildNumber"].ToString ());
		}
	}

}
#endif