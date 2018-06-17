using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Opencoding.CommandHandlerSystem;

public class GameBoard : MonoBehaviour {

	[SerializeField] private GameObject piece;
	[SerializeField] private Camera puzzleCamera;
	[SerializeField] private RenderTexture puzzleCameraTexture;
	[SerializeField] private Texture goalTexture;

	public Piece draggablePiece;
	public Vector3 draggablePieceOffset;

	private List<SnapablePoint> snapablePoints = new List<SnapablePoint>();
	private Vector3 startOffset = new Vector3(-1,0,0);
	private RenderTexture puzzleTexture;

	private List<PuzzlePivot> puzzlePivots = new List<PuzzlePivot>();
	private PuzzlePivot activePuzzlePivot;
	void Start () {
		Application.targetFrameRate = 60;

		var puzzlePivot = new PuzzlePivot ();
		puzzlePivots.Add (puzzlePivot);
		SpawnPieces (puzzlePivot, goalTexture);
		ScramblePiecePosition (puzzlePivot.pieces);
		activePuzzlePivot = puzzlePivot;
		StartCoroutine (SpawnExtraPivots(NumberOfPivots));

		CommandHandlers.RegisterCommandHandlers(typeof(GameBoard));
	}

	private void SpawnPieces(PuzzlePivot pivot, Texture texture) {
		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 3; j++) {
				var id = i.ToString () + j.ToString ();
				var newSnapablePoint = new SnapablePoint (id, new Vector3 (i * 2, j * 2, 0) + startOffset);
				snapablePoints.Add(newSnapablePoint);

				var pieceObject = GameObject.Instantiate (piece).GetComponent<Piece>();
				pieceObject.transform.position = new Vector3 (i+0.2f,j*2+0.2f,0)+startOffset;
				pieceObject.id = id;
				pieceObject.gameBoard = this;
				pieceObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(i*0.5f,j*0.33f));
				pieceObject.GetComponent<MeshRenderer> ().material.SetTexture ("_MainTex", texture);
				pieceObject.transform.parent = pivot.pivot.transform;
				pivot.pieces.Add (pieceObject);
			}
		}
	}

	private void ScramblePiecePosition(List<Piece> pieces) {
		foreach (var piece in pieces) {
			var randomX = Random.Range (-100,100)*0.01f;
			var randomY = Random.Range (0,400)*0.01f;
			piece.transform.position = new Vector3 (randomX,randomY,piece.transform.position.z);
		}
	}

	private IEnumerator SpawnExtraPivots(int numberOfPivots) {
		// Wait 3 frames
		for (int i = 0; i < 3; i++) {
			yield return null;
		}

		for (int i = 0; i < numberOfPivots; i++) {
			SpawnExtraPivot ();
			yield return null;
		}
	}

	private void SpawnExtraPivot() {
		HideAllPuzzlePivots ();
		var snapShot = new Texture2D (170, 256,TextureFormat.ARGB32,false);
		Graphics.CopyTexture (puzzleCameraTexture, snapShot);
		var puzzlePivot = new PuzzlePivot ();
		puzzlePivots.Add (puzzlePivot);
		SpawnPieces (puzzlePivot, snapShot);
		ScramblePiecePosition (puzzlePivot.pieces);
		activePuzzlePivot = puzzlePivot;
	}

	void Update() {
		if (draggablePiece != null) {
			var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			draggablePiece.transform.position = new Vector3(mousePos.x,mousePos.y,draggablePiece.transform.position.z)+draggablePieceOffset;
		}

		if (Input.GetMouseButtonUp (0) && (draggablePiece != null)) {
			var snapablePoint = GetPointWithinRadius (draggablePiece.transform.position, 0.2f);
			if (snapablePoint != null) {
				draggablePiece.transform.position = snapablePoint.position;
			}

			draggablePiece = null;

			var isDone = CheckForWin ();
			if (isDone) {
				var nextPuzzlePivot = GetNextPuzzlePivot();
				if (nextPuzzlePivot != null) {
					activePuzzlePivot = nextPuzzlePivot;
					HideAllPuzzlePivots ();
					activePuzzlePivot.pivot.SetActive (true);
				}
			}
		}
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

	private SnapablePoint GetPointWithinRadius(Vector3 point, float radius) {
		foreach (var snapablePoint in snapablePoints) {
			var dist = snapablePoint.position - point;
			if (dist.magnitude < radius) {
				return snapablePoint;
			}
		}
		return null;
	}

	private bool CheckForWin() {
		foreach(var piece in activePuzzlePivot.pieces) {
			var snapablePoint = GetPointWithinRadius (piece.transform.position, 0.2f);
			if (snapablePoint == null || snapablePoint.id != piece.id) {
				return false;
			}
		}
		return true;
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

		public PuzzlePivot() {
			pivot = new GameObject();
		}
	}

	private static void ReloadLevelInternal() {
		SceneManager.LoadScene ("Boot");
	}

	[CommandHandler(Description="Will reload the level.")]
	private static void ReloadLevel() {
		ReloadLevelInternal ();
	}

	private static int NumberOfPivots = 1;
	[CommandHandler(Description="Determine how many layers should be used for a puzzle.")]
	private static void SetNumberOfPivots(int numberOfPivots) {
		NumberOfPivots = numberOfPivots;
		ReloadLevelInternal ();
	}
}
