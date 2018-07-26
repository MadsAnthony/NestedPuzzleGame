using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortrait : MonoBehaviour {
	[SerializeField] private int levelIndex;
	[SerializeField] private MeshRenderer frontPicture;
	[SerializeField] private MeshRenderer backPicture;
	[SerializeField] private GameObject picturePivot;
	[SerializeField] private GameObject picturePanel;
	[SerializeField] private MeshRenderer collectable;

	void Start() {
		var level = Director.LevelDatabase.levels [levelIndex];

		var scale = 12;
		var aspectRatio = (float)level.picture.height/level.picture.width;
		var pictureSize = new Vector3(scale, scale*aspectRatio, 1);

		picturePivot.transform.localScale = pictureSize;
		picturePanel.transform.localPosition = new Vector3(0,1.2f+pictureSize.y/2f,0);
		var levelSaveNormal = Director.SaveData.GetLevelSaveDataEntry(levelIndex.ToString() + "_" + false.ToString());
		var levelSaveAlternative = Director.SaveData.GetLevelSaveDataEntry(levelIndex.ToString() + "_" + true.ToString());
		if (levelSaveNormal != null) {
			frontPicture.material.mainTexture = level.picture;
			if (levelSaveNormal.gotCollectable) {
				collectable.material.color = Color.white;
			}
		}
		if (levelSaveAlternative != null) {
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
