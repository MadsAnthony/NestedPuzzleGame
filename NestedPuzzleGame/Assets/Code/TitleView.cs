using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TitleView : MonoBehaviour {
	public void EnterGame() {
		Director.TransitionManager.PlayTransition (() => { SceneManager.LoadScene ("LevelSelectScene");}, 0.2f, Director.TransitionManager.FadeToBlack(),  Director.TransitionManager.FadeOut());
	}
}
