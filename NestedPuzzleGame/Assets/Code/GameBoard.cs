using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Opencoding.CommandHandlerSystem;

public class GameBoard : MonoBehaviour {
	
	[SerializeField] private GameObject zoomOutPivot;
	[SerializeField] private SubPuzzle subPuzzlePrefab;
	[SerializeField] private AnimationCurve easeInOutCurve;

	public Piece draggablePiece;
	public Vector3 draggablePieceOffset;

	private RenderTexture puzzleTexture;

	private SubPuzzle activeSubPuzzle;

	void Start () {
		Application.targetFrameRate = 60;
		CommandHandlers.RegisterCommandHandlers(typeof(GameBoard));

		zoomOutPivot.SetActive (false);


		var subPuzzle = GameObject.Instantiate (subPuzzlePrefab).GetComponent<SubPuzzle>();
		subPuzzle.SetGameBoard(this);
		subPuzzle.transform.parent = transform;
		subPuzzle.transform.localPosition = new Vector3 (1, 0, 0);
		subPuzzle.SpawnSubPuzzle ();
		activeSubPuzzle = subPuzzle;
		subPuzzle.ActivateSubPuzzle ();

		var subPuzzle2 = GameObject.Instantiate (subPuzzlePrefab).GetComponent<SubPuzzle>();
		subPuzzle2.SetGameBoard(this);
		subPuzzle2.transform.parent = transform;
		subPuzzle2.transform.localPosition = new Vector3 (6, 0, 0);
		subPuzzle2.SpawnSubPuzzle ();
	}

	public void SetActiveSubPuzzle(SubPuzzle subPuzzle) {
		activeSubPuzzle = subPuzzle;
		activeSubPuzzle.ActivateSubPuzzle ();
	}

	void Update() {
		if (draggablePiece != null) {
			var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			draggablePiece.transform.position = new Vector3(mousePos.x,mousePos.y,draggablePiece.transform.position.z)+draggablePieceOffset;
		}

		if (Input.GetMouseButtonUp (0) && (draggablePiece != null)) {
			var snapablePoint = activeSubPuzzle.GetPointWithinRadius (draggablePiece.transform.localPosition, 0.2f);
			if (snapablePoint != null) {
				draggablePiece.transform.localPosition = snapablePoint.position;
			}

			draggablePiece = null;

			var isDone = activeSubPuzzle.CheckForWin ();
			if (isDone) {
				var hasMorePuzzles = activeSubPuzzle.SetupNextPuzzlePivot ();
				if (!hasMorePuzzles) {
					StartCoroutine (ZoomOut ());
				}
			}
		}
	}

	public IEnumerator ZoomOut() {
		zoomOutPivot.SetActive (true);
		yield return new WaitForSeconds (0.5f);
		StartCoroutine(AnimateTo (gameObject, new Vector3 (-1.5f,-3,0)));
		StartCoroutine(ScaleTo (gameObject, new Vector3 (0.5f,0.5f,0)));
	}

	public void ZoomIn(Vector3 pos) {
		StartCoroutine(AnimateTo (gameObject, new Vector3(-pos.x,pos.y,gameObject.transform.position.z)));
		StartCoroutine(ScaleTo (gameObject, new Vector3 (1f,1f,0)));
	}

	private IEnumerator AnimateTo(GameObject gameObject, Vector3 endPosition) {
		var startPosition = gameObject.transform.position;
		float time = 0;
		while (time < 1) {
			time += 0.01f;
			var t = easeInOutCurve.Evaluate (time);
			gameObject.transform.position = (startPosition * (1 - t)) + endPosition * t;
			yield return null;
		}
	}

	private IEnumerator ScaleTo(GameObject gameObject, Vector3 endScale) {
		var startScale = gameObject.transform.localScale;
		startScale.z = 1;
		endScale.z = 1;
		float time = 0;
		while (time < 1) {
			time += 0.01f;
			var t = easeInOutCurve.Evaluate (time);
			gameObject.transform.localScale = (startScale * (1 - t)) + endScale * t;
			yield return null;
		}
	}

	private static void ReloadLevelInternal() {
		SceneManager.LoadScene ("Boot");
	}

	[CommandHandler(Description="Will reload the level.")]
	private static void ReloadLevel() {
		ReloadLevelInternal ();
	}

	public static int NumberOfPivots = 1;
	[CommandHandler(Description="Determine how many layers should be used for a puzzle.")]
	private static void SetNumberOfPivots(int numberOfPivots) {
		NumberOfPivots = numberOfPivots;
		ReloadLevelInternal ();
	}
}
