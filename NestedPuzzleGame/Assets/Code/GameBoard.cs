using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Opencoding.CommandHandlerSystem;

public class GameBoard : MonoBehaviour {
	[SerializeField] public SubPuzzle subPuzzlePrefab;
	[SerializeField] private AnimationCurve easeInOutCurve;
	[SerializeField] private GameObject goalPicture;
	[SerializeField] private Texture goalTexture;

	public Piece draggablePiece;
	public Vector3 draggablePieceOffset;

	private RenderTexture puzzleTexture;

	public SubPuzzle activeSubPuzzle;
	private GameObject goalPictureObject;
	private Vector3 startScale;
	public static float ZoomScale = 3;

	private LevelAsset level;

	void Start () {
		level = Director.LevelDatabase.levels [Director.Instance.LevelIndex];

		GameBoard.numberOfLayers = level.numberOfLayers;
		GameBoard.numberOfPivots = level.numberOfPivots;
		GameBoard.numberOfPieces = level.numberOfPieces;

		startScale = transform.localScale;
		StartCoroutine (SpawnInitialSubPuzzle ());
	}

	private IEnumerator SpawnInitialSubPuzzle() {
		var scale = 4;
		var aspectRatio = (float)level.picture.height/level.picture.width;
		var pictureSize = new Vector2(scale, scale*aspectRatio);

		goalPictureObject = GameObject.Instantiate (goalPicture);
		goalPictureObject.transform.parent = transform;

		goalPictureObject.transform.localScale = new Vector3 (pictureSize.x,pictureSize.y,goalPictureObject.transform.localScale.z);
		goalPictureObject.transform.localPosition = new Vector3 (0,1.5f,-1);
		goalPictureObject.GetComponent<MeshRenderer> ().material.SetTexture ("_MainTex", level.picture);

		var subPuzzle = GameObject.Instantiate (subPuzzlePrefab).GetComponent<SubPuzzle>();
		subPuzzle.transform.parent = transform;
		subPuzzle.Initialize(this, 0, pictureSize);
		subPuzzle.transform.parent = transform;
		subPuzzle.transform.localPosition = new Vector3 (0, 0, 0);
		subPuzzle.SpawnSubPuzzle ();


		// Wait 3 frames
		for (int i = 0; i < 3; i++) {
			yield return null;
		}

		SetActiveSubPuzzle (subPuzzle);
		goalPictureObject.SetActive (false);
	}

	public void SetActiveSubPuzzle(SubPuzzle newActiveSubPuzzle) {
		activeSubPuzzle = newActiveSubPuzzle;
		newActiveSubPuzzle.ActivateSubPuzzle();
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
					activeSubPuzzle.WasDone ();
					if (activeSubPuzzle.parentSubPuzzle != null) {
						transform.localScale *= 1/GameBoard.ZoomScale;
						var camera = GameObject.Find("Main Camera");
						var newPos = camera.transform.position-activeSubPuzzle.parentSubPuzzle.transform.position;
						transform.localScale *= GameBoard.ZoomScale;

						ZoomOut (transform.position+newPos);
					} else {
						goalPictureObject.SetActive (true);
						Director.TransitionManager.PlayTransition (() => { SceneManager.LoadScene ("LevelSelectScene");}, 0.2f, Director.TransitionManager.FadeToBlack(),  Director.TransitionManager.FadeOut());
					}
					activeSubPuzzle = activeSubPuzzle.parentSubPuzzle;
				}
			}
		}
	}

	public void ZoomToLayer(int layerNumber) {
		transform.localScale = startScale*Mathf.Pow(ZoomScale,layerNumber);
	}

	public static int numberOfLayers;
	public static int NumberOfLayers { 
		get 
		{
			if (DeveloperCheats.NumberOfLayers >= 0) {
				return DeveloperCheats.NumberOfLayers;
			}
			return numberOfLayers;
		}
	}

	public static int numberOfPivots;
	public static int NumberOfPivots { 
		get 
		{
			if (DeveloperCheats.NumberOfPivots > 0) {
				return DeveloperCheats.NumberOfPivots;
			}
			return numberOfPivots;
		}
	}

	public static Vector2 numberOfPieces;
	public static Vector2 NumberOfPieces { 
		get 
		{
			if (DeveloperCheats.NumberOfPiecesOnX > 0 || DeveloperCheats.NumberOfPiecesOnY>0) {
				return new Vector2(DeveloperCheats.NumberOfPiecesOnX,DeveloperCheats.NumberOfPiecesOnY);
			}
			return numberOfPieces;
		}
	}

	public IEnumerator ZoomOut() {
		yield return new WaitForSeconds (0.5f);
		StartCoroutine(AnimateTo (gameObject, new Vector3 (0,-1f)));
		StartCoroutine(ScaleTo (gameObject, gameObject.transform.localScale*(1/ZoomScale)));
	}

	public void ZoomOut(Vector3 pos) {
		StartCoroutine(AnimateTo (gameObject, new Vector3(pos.x,pos.y,gameObject.transform.position.z)));
		StartCoroutine(ScaleTo (gameObject, gameObject.transform.localScale*1/ZoomScale));
	}

	public void ZoomIn(Vector3 pos) {
		StartCoroutine(AnimateTo (gameObject, new Vector3(pos.x,pos.y,gameObject.transform.position.z)));
		StartCoroutine(ScaleTo (gameObject, gameObject.transform.localScale*ZoomScale));
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
}
