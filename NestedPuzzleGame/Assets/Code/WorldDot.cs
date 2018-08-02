using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class WorldDot : MonoBehaviour {
	[SerializeField] private int worldIndex;
	[SerializeField] private TextMesh text;

	void Start() {
		text.text = (worldIndex+1).ToString ();
	}

	public void Click() {
		Director.Instance.LevelIndex = 1;
		Director.Instance.IsAlternativeLevel = false;
		Director.Instance.WorldIndex = worldIndex;
		GotoWorld ();
	}

	private void GotoWorld() {
		Director.TransitionManager.PlayTransition (() => { SceneManager.LoadScene ("LevelSelectScene");}, 0.2f, Director.TransitionManager.FadeToBlack(),  Director.TransitionManager.FadeOut());
	}
}
