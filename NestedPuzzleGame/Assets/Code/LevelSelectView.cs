using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectView : MonoBehaviour {
	[SerializeField] private List<GameObject> listOfLevels;
	[SerializeField] private AnimationCurve easeInOutCurve;

	public int currentLevelIndex;
	private bool isRotating;
	public static int amountOfMasterPieces;

	private void Awake() {
		amountOfMasterPieces = Director.GetAmountOfMasterPieces();
	}

	private void Start() {
		currentLevelIndex = Director.Instance.LevelIndex;
		if (currentLevelIndex>=0) {
			StartCoroutine(InitialFlow());
		}
	}

	private void Update() {
		if (Input.GetMouseButtonDown (0)) {
			var mousePosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mousePosInWorld.z = -1;
			var hits = Physics.RaycastAll(mousePosInWorld, Vector3.forward, 100);
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
		}
		

		Director.Instance.levelExitState = LevelExitState.None;
	}
	
	public void MoveLeft() {
		if (currentLevelIndex == 1 && amountOfMasterPieces == 0) return;
		currentLevelIndex -= 1;
		currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, listOfLevels.Count - 1);
		StartCoroutine(MoveToLevelCr(currentLevelIndex, 0.05f));
	}

	public void MoveRight() {
		currentLevelIndex += 1;
		currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, listOfLevels.Count - 1);
		StartCoroutine(MoveToLevelCr(currentLevelIndex, 0.05f));
	}

	public void Rotate() {
		if (currentLevelIndex < 0 || isRotating) return;
		var currentPicture = listOfLevels [currentLevelIndex];
		var endRotation = currentPicture.transform.localEulerAngles+new Vector3 (0, 180, 0);
		StartCoroutine(RotateTo(currentPicture, endRotation, 0.05f));
	}

	private IEnumerator MoveToLevelCr(int levelIndex, float timeIncrement) {
		var endPosition = gameObject.transform.position - listOfLevels[currentLevelIndex].transform.position;
		yield return AnimateTo(gameObject, endPosition, timeIncrement);
	}

	private IEnumerator AnimateTo(GameObject gameObject, Vector3 endPosition, float timeIncrement) {
		var startPosition = gameObject.transform.position;
		float time = 0;
		while (time < 1) {
			time += timeIncrement;
			var t = easeInOutCurve.Evaluate (time);
			gameObject.transform.position = (startPosition * (1 - t)) + endPosition * t;
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
