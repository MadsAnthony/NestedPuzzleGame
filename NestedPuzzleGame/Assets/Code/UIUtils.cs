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

	public void MoveObjectRight(GameObject gameObject) {
		gameObject.transform.localPosition += new Vector3(-3,0,0);
	}

	public void MoveObjectLeft(GameObject gameObject) {
		gameObject.transform.localPosition += new Vector3(3,0,0);
	}

	public void RestartLevel() {
		SceneManager.LoadScene ("LevelScene");
	}
}
