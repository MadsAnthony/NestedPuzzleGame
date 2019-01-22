using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectView : MonoBehaviour {
	[SerializeField] private Text worldLabel;
	[SerializeField] private GameObject levelPortraitPrefab;
	[SerializeField] private AnimationCurve easeInOutCurve;

	public int currentLevelIndex;
	private bool isRotating;
	private List<LevelPortrait> listOfPortraits = new List<LevelPortrait>();
	public static int amountOfMasterPieces;
	private Vector3 mousePosDownInWorld;
	private Vector3 mousePosUpInWorld;

	private void Awake() {
		amountOfMasterPieces = Director.GetAmountOfMasterPieces();
		worldLabel.text = "World "+(Director.Instance.WorldIndex + 1).ToString ();
	}

	private void Start() {
		float yPos = -50;
		int i = 0;
		foreach (var level in Director.LevelDatabase.levels) {
			var levelPortraitObject = Instantiate (levelPortraitPrefab, transform);
			var levelPortrait = levelPortraitObject.GetComponent<LevelPortrait> ();
			levelPortrait.levelIndex = i;
			var pictureHeight = levelPortrait.GetPictureSize().y;
			if (i == 0) {
				levelPortraitObject.transform.localPosition = new Vector3 (0, yPos, 0);
				yPos -= 10;
			} else {
				yPos -= pictureHeight*0.5f;
				levelPortraitObject.transform.localPosition = new Vector3 (0, yPos, 0);
			}
			
			listOfPortraits.Add (levelPortrait);
			
			yPos -= pictureHeight*0.5f;
			yPos -= 5;
			i++;
		}

		currentLevelIndex = Director.Instance.LevelIndex;
		if (currentLevelIndex>=0) {
			StartCoroutine(InitialFlow());
		}
	}

	private void Update() {
		if (Input.GetMouseButtonDown (0)) {
			mousePosDownInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mousePosDownInWorld.z = -1;
		}
		if (Input.GetMouseButtonUp (0)) {
			mousePosUpInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mousePosDownInWorld.z = -1;

			MouseUp ();
		}
	}

	private void MouseUp() {
		var dist = mousePosDownInWorld - mousePosUpInWorld;
		var horiziontalDot = Vector3.Dot (Vector3.down, dist);
		if (Mathf.Abs(horiziontalDot) > 1) {
			if (horiziontalDot < 0) {
				MoveLeft ();
			} else {
				MoveRight ();
			}
			return;
		}

		var hits = Physics.RaycastAll(mousePosDownInWorld, Vector3.forward, 100);
		if (hits.Length > 0) {
			var levelPortrait = hits [0].transform.GetComponentInParent<LevelPortrait> ();
			if (levelPortrait != null) {
				if (hits [0].transform.name.Contains ("Front")) {
					levelPortrait.PlayLevel ();
				}
				if (hits [0].transform.name.Contains ("Back")) {
					levelPortrait.PlayAlternativeLevel ();
				}
			}
		}
	}

	private IEnumerator InitialFlow() {
		yield return MoveToLevelCr(currentLevelIndex, 1);
		if (Director.Instance.IsAlternativeLevel) {
			Rotate();
		} else {
			if ((Director.Instance.levelExitState & LevelExitState.LevelCompleted) != 0) {
				yield return new WaitForSeconds(1);
				MoveRight();
			}
			if ((Director.Instance.levelExitState & LevelExitState.GotCollectable) != 0) {
				listOfPortraits [0].SetMasterPuzzleTexture ();
			}
		}
		

		Director.Instance.levelExitState = LevelExitState.None;
	}

	public void GoBack() {
		Director.TransitionManager.PlayTransition (() => { SceneManager.LoadScene ("WorldSelectScene");}, 0.2f, Director.TransitionManager.FadeToBlack(),  Director.TransitionManager.FadeOut());
	}

	public void SwapLevelsUp() {
		if (currentLevelIndex <= 1) return;
		SwapLevels(currentLevelIndex - 1);
	}

	public void SwapLevelsDown() {
		if (currentLevelIndex >= listOfPortraits.Count - 1) return;
		SwapLevels(currentLevelIndex + 1);
	}
	
	public void SwapLevels(int nextLevelIndex) {
		if (currentLevelIndex == 0) return;
		
		var currentLevel = listOfPortraits[currentLevelIndex];
		var nextLevel = listOfPortraits[nextLevelIndex];

		var dir = (nextLevelIndex > currentLevelIndex)? 1 : -1;
		var currentLevelNewPos = currentLevel.transform.localPosition + new Vector3(0, -nextLevel.GetPictureSize().y-5,0)*dir;
		var nextLevelNewPos = nextLevel.transform.localPosition + new Vector3(0, currentLevel.GetPictureSize().y+5,0)*dir;
		
		StartCoroutine(AnimateTo(currentLevel.gameObject, currentLevelNewPos, 0.05f));
		StartCoroutine(AnimateTo(nextLevel.gameObject, nextLevelNewPos, 0.05f));

		listOfPortraits[currentLevelIndex] = nextLevel;
		listOfPortraits[nextLevelIndex] = currentLevel;
	}

	public void MoveLeft() {
		if (currentLevelIndex == 1 && amountOfMasterPieces == 0) return;
		currentLevelIndex -= 1;
		currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, listOfPortraits.Count - 1);
		StartCoroutine(MoveToLevelCr(currentLevelIndex, 0.05f));
	}

	public void MoveRight() {
		currentLevelIndex += 1;
		currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, listOfPortraits.Count - 1);
		StartCoroutine(MoveToLevelCr(currentLevelIndex, 0.05f));
	}

	public void Rotate() {
		if (currentLevelIndex < 0 || isRotating) return;
		var currentPicture = listOfPortraits [currentLevelIndex];
		var endRotation = currentPicture.transform.localEulerAngles+new Vector3 (0, 180, 0);
		StartCoroutine(RotateTo(currentPicture.gameObject, endRotation, 0.05f));
	}

	private IEnumerator MoveToLevelCr(int levelIndex, float timeIncrement) {
		var endPosition = gameObject.transform.position - listOfPortraits[currentLevelIndex].transform.position;
		yield return AnimateTo(gameObject, endPosition, timeIncrement);
	}

	private IEnumerator AnimateTo(GameObject gameObject, Vector3 endPosition, float timeIncrement) {
		var startPosition = gameObject.transform.localPosition;
		float time = 0;
		while (time < 1) {
			time += timeIncrement;
			var t = easeInOutCurve.Evaluate (time);
			gameObject.transform.localPosition = (startPosition * (1 - t)) + endPosition * t;
			yield return null;
		}
	}

	private IEnumerator RotateTo(GameObject gameObject, Vector3 endRotation, float timeIncrement) {
		isRotating = true;
		var startRotation = gameObject.transform.localEulerAngles;
		float time = 0;
		while (time < 1) {
			time += timeIncrement;
			var t = easeInOutCurve.Evaluate (time);
			gameObject.transform.localEulerAngles = (startRotation * (1 - t)) + endRotation * t;
			yield return null;
		}
		isRotating = false;
	}
}
