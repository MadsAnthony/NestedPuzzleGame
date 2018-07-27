using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelPortrait : MonoBehaviour {
	[SerializeField] private int levelIndex;
	[SerializeField] private MeshRenderer frontPicture;
	[SerializeField] private MeshRenderer backPicture;
	[SerializeField] private GameObject picturePivot;
	[SerializeField] private GameObject picturePanel;
	[SerializeField] private GameObject masterPuzzlePivot;
	[SerializeField] private TextMesh masterPuzzleAmountText;
	[SerializeField] private MeshRenderer collectableNormal;
	[SerializeField] private MeshRenderer collectableBack;

	void Start() {
		var level = Director.LevelDatabase.levels [levelIndex];

		var scale = 12;
		var aspectRatio = (float)level.picture.height/level.picture.width;
		var pictureSize = new Vector3(scale, scale*aspectRatio, 1);

		if (levelIndex == 0) {
			masterPuzzlePivot.SetActive(true);
			masterPuzzlePivot.transform.localPosition = new Vector3(0,0.3f+pictureSize.y/2f,0);
			masterPuzzleAmountText.text = LevelSelectView.amountOfMasterPieces + "/"+"10";
			picturePanel.SetActive(false);
		} else {
			masterPuzzlePivot.SetActive(false);
			picturePanel.SetActive(true);
		}
		
		picturePivot.transform.localScale = pictureSize;
		picturePanel.transform.localPosition = new Vector3(0,1.2f+pictureSize.y/2f,0);
		var levelSaveNormal = Director.SaveData.GetLevelSaveDataEntry(levelIndex.ToString() + "_" + false.ToString());
		var levelSaveAlternative = Director.SaveData.GetLevelSaveDataEntry(levelIndex.ToString() + "_" + true.ToString());
		collectableNormal.material.color = Color.black;
		collectableBack.material.color = Color.black;
		if (levelSaveNormal != null) {
			frontPicture.material.mainTexture = level.picture;
			if (levelSaveNormal.gotCollectable) {
				collectableNormal.material.color = Color.white;
			}
		}
		if (levelSaveAlternative != null) {
			backPicture.material.mainTexture = level.picture;
			if (levelSaveAlternative.gotCollectable) {
				collectableBack.material.color = Color.white;
			}
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
