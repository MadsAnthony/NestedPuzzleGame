using System.Collections;
using System.Collections.Generic;
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
	void Start () {
		puzzleCameraTexture = new RenderTexture (170, 256, 24, RenderTextureFormat.ARGB32);
		puzzleCamera.targetTexture = puzzleCameraTexture;
	}

	public void Initialize(GameBoard gameBoard, Vector2 pictureVector) {
		this.gameBoard = gameBoard;
		subPuzzleButton.gameBoard = gameBoard;
		subPuzzleButton.subPuzzle = this;
		this.pictureVector = pictureVector;
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

		var puzzlePivot = new PuzzlePivot (pivot);
		puzzlePivots.Add (puzzlePivot);
		SpawnPieces (puzzlePivot, goalTexture, new Vector2 (0.5f*0.5f,0.33f*0.33f), pictureVector);
		ScramblePiecePosition (puzzlePivot.pieces);
		activePuzzlePivot = puzzlePivot;
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
			var randomX = Random.Range (-100,100)*0.01f;
			var randomY = Random.Range (-200,200)*0.01f;
			piece.transform.localPosition = new Vector3 (randomX,randomY,piece.transform.position.z);
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
