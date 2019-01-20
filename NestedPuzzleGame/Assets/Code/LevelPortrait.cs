using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LevelPortrait : MonoBehaviour {
	[SerializeField] private MeshRenderer frontPicture;
	[SerializeField] private MeshRenderer backPicture;
	[SerializeField] private GameObject picturePivot;
	[SerializeField] private GameObject picturePanel;
	[SerializeField] private SpriteRenderer frame;
	[SerializeField] private GameObject masterPuzzlePivot;
	[SerializeField] private TextMesh masterPuzzleAmountText;
	[SerializeField] private MeshRenderer collectableNormal;
	[SerializeField] private MeshRenderer collectableBack;
	
	public int levelIndex;

	void Start() {
		var level = Director.LevelDatabase.levels [levelIndex];

		var pictureSize = GetPictureSize();

		if (levelIndex == 0) {
			masterPuzzlePivot.SetActive(true);
			masterPuzzlePivot.transform.localPosition = new Vector3(0,0.5f+pictureSize.y/2f,0);
			masterPuzzleAmountText.text = LevelSelectView.amountOfMasterPieces + "/"+"10";
			picturePanel.SetActive(false);
			var key = Director.Instance.WorldIndex.ToString () + "_0";
			if (Director.Instance.MasterPuzzleImages.ContainsKey (key)) {
				Texture2D goalTexture = Director.Instance.MasterPuzzleImages [key];
				frontPicture.material.mainTexture = goalTexture;
			} else {
				SetMasterPuzzleTexture ();
			}
		} else {
			masterPuzzlePivot.SetActive(false);
			picturePanel.SetActive(true);
		}

		picturePivot.transform.localScale = pictureSize;
		picturePanel.transform.localPosition = new Vector3(0,1.4f+pictureSize.y/2f,0);
		frame.size = pictureSize*(1f/frame.transform.localScale.x)+new Vector3(0.1f,0.1f,0);
		var levelSaveNormal = Director.SaveData.GetLevelSaveDataEntry(Director.Instance.WorldIndex.ToString() + "_" + levelIndex.ToString() + "_" + false.ToString());
		var levelSaveAlternative = Director.SaveData.GetLevelSaveDataEntry(Director.Instance.WorldIndex.ToString() + "_" + levelIndex.ToString() + "_" + true.ToString());
		collectableNormal.material.color = Color.black;
		collectableBack.material.color = Color.black;
		if (levelSaveNormal.completed) {
			frontPicture.material.mainTexture = level.picture;
			if (levelSaveNormal.gotCollectable) {
				collectableNormal.material.color = Color.white;
			}
		}
		if (levelSaveAlternative.completed) {
			backPicture.material.mainTexture = level.picture;
			if (levelSaveAlternative.gotCollectable) {
				collectableBack.material.color = Color.white;
			}
		}
	}

	public Vector3 GetPictureSize() {
		var level = Director.LevelDatabase.levels [levelIndex];
		
		var scale = 12;
		var aspectRatio = (float)level.picture.height/level.picture.width;
		var pictureSize = new Vector3(scale, scale*aspectRatio, 1);
		
		return pictureSize;
	}
	
	public void SetMasterPuzzleTexture() {
		StartCoroutine (SetMasterPuzzleTextureCr());
	}

	private IEnumerator SetMasterPuzzleTextureCr() {
		var key = Director.Instance.WorldIndex.ToString () + "_0";
		SceneManager.LoadScene ("LevelScene", LoadSceneMode.Additive);
		var currentLevelIndex = Director.Instance.LevelIndex;
		Director.Instance.LevelIndex = 0;
		yield return null;
		var camera = GameObject.Find ("CameraPivot");
		camera.SetActive (false);
		var gameUI = GameObject.Find ("GameUI");
		gameUI.SetActive (false);
		var gameBoard = GameObject.Find ("GameBoard").GetComponent<GameBoard> ();
		gameBoard.MoveEntireScene (new Vector3 (1000, 1000, 0));
		for (int i = 0; i < 10; i++) {
			yield return null;
		}
		yield return SceneManager.UnloadSceneAsync ("LevelScene");
		Director.Instance.LevelIndex = currentLevelIndex;
		Texture2D goalTexture = Director.Instance.MasterPuzzleImages [key];
		frontPicture.material.mainTexture = goalTexture;
	}

	public void PlayLevel() {
		Director.Instance.LevelIndex = levelIndex;
		Director.Instance.IsAlternativeLevel = false;
		Director.TransitionManager.PlayTransition (() => { SceneManager.LoadScene ("LevelScene");}, 0.1f, Director.TransitionManager.FadeToBlack(),  Director.TransitionManager.FadeOut());
	}

	public void PlayAlternativeLevel() {
		Director.Instance.LevelIndex = levelIndex;
		Director.Instance.IsAlternativeLevel = true;
		Director.TransitionManager.PlayTransition (() => { SceneManager.LoadScene ("LevelScene");}, 0.1f, Director.TransitionManager.FadeToBlack(),  Director.TransitionManager.FadeOut());
	}
}
