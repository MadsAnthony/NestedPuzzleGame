using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boot : MonoBehaviour {
	void Start () {
		Application.targetFrameRate = 60;
		var initDirector = Director.Instance;
		SceneManager.LoadScene ("TitleScene");
	}
}
