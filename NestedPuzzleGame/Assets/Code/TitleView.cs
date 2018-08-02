using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using MiniJSON;


public class TitleView : MonoBehaviour {
	[SerializeField] private Text versionLeft;
	[SerializeField] private Text versionRight;
	public void EnterGame() {
		Director.TransitionManager.PlayTransition (() => { SceneManager.LoadScene ("WorldSelectScene");}, 0.2f, Director.TransitionManager.FadeToBlack(),  Director.TransitionManager.FadeOut());
	}

	void Start() {
		var manifest = (TextAsset) Resources.Load("UnityCloudBuildManifest.json");
		if (manifest != null)
		{
			var manifestDict = Json.Deserialize(manifest.text) as Dictionary<string,object>;
			versionRight.text = manifestDict ["buildNumber"].ToString ();
		}
	}
}
