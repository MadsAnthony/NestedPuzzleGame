using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIUtils : MonoBehaviour {
	public void GotoWorldSelectScene() {
		SceneManager.LoadScene ("WorldSelectScene");
	}

	public void GotoLevelScene(int i) {
		Director.Instance.LevelIndex = i;
		SceneManager.LoadScene ("LevelScene");
	}

	public void RestartLevel() {
		SceneManager.LoadScene ("LevelScene");
	}
}
