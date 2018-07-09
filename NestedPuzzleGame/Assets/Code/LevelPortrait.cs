using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortrait : MonoBehaviour {
	[SerializeField] private int levelIndex;

	void Start() {
		var level = Director.LevelDatabase.levels [levelIndex];

		var scale = 12;
		var aspectRatio = (float)level.picture.height/level.picture.width;
		var pictureSize = new Vector3(scale, scale*aspectRatio, 1);

		transform.localScale = pictureSize;

		if (Director.SaveData.GetLevelSaveDataEntry (levelIndex.ToString ()) != null) {
			GetComponent<MeshRenderer>().material.mainTexture = level.picture;
		}
	}

	void OnMouseDown() {
		Director.Instance.LevelIndex = levelIndex;
		SceneManager.LoadScene ("LevelScene");
	}
}
