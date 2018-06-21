using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SubPuzzle : MonoBehaviour {
	[SerializeField] private GameObject piece;
	[SerializeField] private Camera puzzleCamera;
	[SerializeField] private Texture goalTexture;
	[SerializeField] private GameObject background;
	[SerializeField] private SubPuzzleButton subPuzzleButton;

	private RenderTexture puzzleCameraTexture;

	public Piece draggablePiece;
	public Vector3 draggablePieceOffset;
	private GameBoard gameBoard;
	private Vector2 pictureVector;

	private List<SnapablePoint> snapablePoints = new List<SnapablePoint>();
	private RenderTexture puzzleTexture;

	private List<PuzzlePivot> puzzlePivots = new List<PuzzlePivot>();
	private PuzzlePivot activePuzzlePivot;

	private SubPuzzle activeSubPuzzle;
	private List<SubPuzzle> subPuzzles = new List<SubPuzzle>();
	public SubPuzzle parentSubPuzzle;

	private int subPuzzleLayer = 0;
	void Start () {
		puzzleCameraTexture = new RenderTexture (170, 256, 24, RenderTextureFormat.ARGB32);
		puzzleCamera.targetTexture = puzzleCameraTexture;
	}

	public void Initialize(GameBoard gameBoard, Vector2 pictureVector, int layer) {
		this.gameBoard = gameBoard;
		subPuzzleButton.gameBoard = gameBoard;
		subPuzzleButton.subPuzzle = this;
		this.pictureVector = pictureVector;
		subPuzzleLayer = layer;
	}

	public void ActivateSubPuzzle() {
		subPuzzleButton.gameObject.SetActive (false);
	}

	public void DeactivateSubPuzzle() {
		subPuzzleButton.gameObject.SetActive (true);
	}

	public void SpawnSubPuzzle() {
		var pivot = new GameObject ();
		pivot.transform.parent = background.transform;
		pivot.transform.localPosition = new Vector3(0,0,0);
		StartCoroutine (SpawnExtraPivots(pivot, GameBoard.NumberOfPivots));
	}

	private void SpawnPieces(PuzzlePivot pivot, Texture texture, Vector2 scale, Vector2 pictureVector) {
		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 3; j++) {
				var id = i.ToString () + j.ToString ();
				var newSnapablePoint = new SnapablePoint (id, new Vector3 ((i-1)*2+1, (j-1)*2, 0));
				snapablePoints.Add(newSnapablePoint);

				var pieceObject = GameObject.Instantiate (piece).GetComponent<Piece>();
				pieceObject.transform.parent = pivot.pivot.transform;
				pieceObject.transform.localPosition = new Vector3 (0, 0, 0);
				pieceObject.id = id;
				pieceObject.gameBoard = gameBoard;

				pieceObject.GetComponent<MeshRenderer> ().material.SetTextureScale ("_MainTex",scale);
				pieceObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(i*scale.x,j*scale.y)+pictureVector);
				pieceObject.GetComponent<MeshRenderer> ().material.SetTexture ("_MainTex", texture);

				pivot.pieces.Add (pieceObject);
			}
		}
	}

	private void ScramblePiecePosition(List<Piece> pieces) {
		foreach (var piece in pieces) {
			var randomX = UnityEngine.Random.Range (-100,100)*0.01f;
			var randomY = UnityEngine.Random.Range (-200,200)*0.01f;
			piece.transform.localPosition = new Vector3 (randomX,randomY,piece.transform.position.z);
		}
	}

	private void ArrangePiecePosition(List<Piece> pieces) {
		var offset = new Vector3 (-1.5f,-3,0);
		int i = 0;
		foreach (var piece in pieces) {
			var x = i%2;
			var y = Mathf.RoundToInt(i/2);
			piece.transform.localPosition = new Vector3 (x*3,y*3.3f,piece.transform.position.z)+offset;
			i += 1;
		}
	}

	private IEnumerator SpawnExtraPivots(GameObject pivot, int numberOfPivots) {
		// Wait 3 frames
		for (int i = 0; i < 3; i++) {
			yield return null;
		}

		for (int i = 0; i < numberOfPivots; i++) {
			SpawnExtraPivot (pivot);
			yield return null;
		}
		if (subPuzzleLayer < GameBoard.NumberOfLayers) {
			ArrangePiecePosition (activePuzzlePivot.pieces);

			gameBoard.transform.localScale = new Vector3 (gameBoard.transform.localScale.x * 2f, gameBoard.transform.localScale.y * 2f, gameBoard.transform.localScale.z);

			var dist = gameBoard.transform.position - activePuzzlePivot.pieces [0].gameObject.transform.position;
			gameBoard.transform.position += dist + new Vector3 (0, 1.5f, 0);

			foreach (var piece in activePuzzlePivot.pieces) {
				var subPuzzle = GameObject.Instantiate (gameBoard.subPuzzlePrefab).GetComponent<SubPuzzle> ();
				subPuzzle.Initialize (gameBoard, new Vector2 (0, 0), subPuzzleLayer + 1);
				subPuzzle.transform.parent = piece.transform.parent;
				subPuzzle.transform.localPosition = piece.transform.localPosition+new Vector3(0,-0.5f,0);
				subPuzzle.parentSubPuzzle = this;
				subPuzzle.SpawnSubPuzzle ();
				subPuzzle.completedSubPuzzle += () => {
					piece.gameObject.SetActive(true);

					if (AlllSubPuzzlesComplete()) {
						background.GetComponent<MeshRenderer> ().enabled = true;
					}
				};
				subPuzzles.Add (subPuzzle);
			}

			// Wait 3 frames
			for (int i = 0; i < 3; i++) {
				yield return null;
			}
			foreach (var piece in activePuzzlePivot.pieces) {
				piece.gameObject.SetActive (false);
			}
			background.GetComponent<MeshRenderer> ().enabled = false;
			SetActiveSubPuzzle (subPuzzles [0]);
		}
	}

	private bool AlllSubPuzzlesComplete() {
		foreach (var subPuzzle in subPuzzles) {
			if (subPuzzle.gameObject.activeSelf) {
				return false;
			}
		}
		return true;
	}

	public Action completedSubPuzzle;
	public void WasDone() {
		gameObject.SetActive (false);
		if (completedSubPuzzle != null) {
			completedSubPuzzle ();
		}
	}

	private void SetActiveSubPuzzle(SubPuzzle newActiveSubPuzzle) {
		foreach (var subPuzzle in subPuzzles) {
			subPuzzle.DeactivateSubPuzzle();
		}
		gameBoard.activeSubPuzzle = newActiveSubPuzzle;
		activeSubPuzzle = newActiveSubPuzzle;
		newActiveSubPuzzle.ActivateSubPuzzle();
	}

	private void SpawnExtraPivot(GameObject pivot) {
		HideAllPuzzlePivots ();
		var snapShot = new Texture2D (170, 256, TextureFormat.ARGB32,false);
		Graphics.CopyTexture (puzzleCameraTexture, snapShot);
		var puzzlePivot = new PuzzlePivot (pivot);
		puzzlePivots.Add (puzzlePivot);
		SpawnPieces (puzzlePivot, snapShot, new Vector2 (0.5f,0.33f), Vector2.zero);
		ScramblePiecePosition (puzzlePivot.pieces);
		activePuzzlePivot = puzzlePivot;
	}

	private PuzzlePivot GetNextPuzzlePivot() {
		PuzzlePivot result = null;
		foreach (var puzzlePivot in puzzlePivots) {
			if (puzzlePivot == activePuzzlePivot) {
				return result;
			}
			result = puzzlePivot;
		}
		return result;
	}

	private void HideAllPuzzlePivots() {
		foreach (var puzzlePivot in puzzlePivots) {
			puzzlePivot.pivot.SetActive (false);
		}
	}

	public SnapablePoint GetPointWithinRadius(Vector3 point, float radius) {
		foreach (var snapablePoint in snapablePoints) {
			var dist = snapablePoint.position - point;
			if (dist.magnitude < radius) {
				return snapablePoint;
			}
		}
		return null;
	}

	public bool CheckForWin() {
		foreach(var piece in activePuzzlePivot.pieces) {
			var snapablePoint = GetPointWithinRadius (piece.transform.localPosition, 0.2f);
			if (snapablePoint == null || snapablePoint.id != piece.id) {
				return false;
			}
		}
		return true;
	}

	public bool SetupNextPuzzlePivot() {
		var nextPuzzlePivot = GetNextPuzzlePivot();
		if (nextPuzzlePivot != null) {
			activePuzzlePivot = nextPuzzlePivot;
			HideAllPuzzlePivots ();
			activePuzzlePivot.pivot.SetActive (true);
			return true;
		}
		return false;
	}

	public class SnapablePoint {
		public readonly string id;
		public readonly Vector3 position;

		public SnapablePoint(string id, Vector3 position) {
			this.id = id;
			this.position = position;
		}
	}

	public class PuzzlePivot {
		public GameObject pivot;
		public List<Piece> pieces = new List<Piece>();

		public PuzzlePivot(GameObject parent) {
			pivot = new GameObject();
			pivot.transform.parent = parent.transform;
			pivot.transform.localPosition = new Vector3(0,0,0);
		}
	}
}
