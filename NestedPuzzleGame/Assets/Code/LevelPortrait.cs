using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortrait : MonoBehaviour {
	[SerializeField] private int levelIndex;
	[SerializeField] private MeshRenderer frontPicture;
	[SerializeField] private MeshRenderer backPicture;

	void Start() {
		var level = Director.LevelDatabase.levels [levelIndex];

		var scale = 12;
		var aspectRatio = (float)level.picture.height/level.picture.width;
		var pictureSize = new Vector3(scale, scale*aspectRatio, 1);

		transform.localScale = pictureSize;

		if (Director.SaveData.GetLevelSaveDataEntry (levelIndex.ToString ()+"_"+false.ToString()) != null) {
			frontPicture.material.mainTexture = level.picture;
		}
		if (Director.SaveData.GetLevelSaveDataEntry (levelIndex.ToString ()+"_"+true.ToString()) != null) {
			backPicture.material.mainTexture = level.picture;
		}
	}

	public void PlayLevel() {
		Director.Instance.LevelIndex = levelIndex;
		Director.Instance.IsAlternativeLevel = false;
		SceneManager.LoadScene ("LevelScene");
	}

	public void PlayAlternativeLevel() {
		Director.Instance.LevelIndex = levelIndex;
		Director.Instance.IsAlternativeLevel = true;
		SceneManager.LoadScene ("LevelScene");
	}
}
