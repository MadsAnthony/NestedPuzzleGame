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
	[SerializeField] private GameObject zoomOutPivot;
	[SerializeField] private AnimationCurve easeInOutCurve;

	public Piece draggablePiece;
	public Vector3 draggablePieceOffset;

	private List<SnapablePoint> snapablePoints = new List<SnapablePoint>();
	private Vector3 startOffset = new Vector3(-1,0,0);
	private RenderTexture puzzleTexture;

	private List<PuzzlePivot> puzzlePivots = new List<PuzzlePivot>();
	private PuzzlePivot activePuzzlePivot;
	void Start () {
		Application.targetFrameRate = 60;

		zoomOutPivot.SetActive (false);
		var puzzlePivot = new PuzzlePivot (gameObject);
		puzzlePivots.Add (puzzlePivot);
		SpawnPieces (puzzlePivot, goalTexture, new Vector2 (0.5f*0.5f,0.33f*0.33f));
		ScramblePiecePosition (puzzlePivot.pieces);
		activePuzzlePivot = puzzlePivot;
		StartCoroutine (SpawnExtraPivots(NumberOfPivots));

		CommandHandlers.RegisterCommandHandlers(typeof(GameBoard));
	}

	private void SpawnPieces(PuzzlePivot pivot, Texture texture, Vector2 scale) {
		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 3; j++) {
				var id = i.ToString () + j.ToString ();
				var newSnapablePoint = new SnapablePoint (id, new Vector3 (i * 2, j * 2, 0) + startOffset);
				snapablePoints.Add(newSnapablePoint);

				var pieceObject = GameObject.Instantiate (piece).GetComponent<Piece>();
				pieceObject.transform.position = new Vector3 (i+0.2f,j*2+0.2f,0)+startOffset;
				pieceObject.id = id;
				pieceObject.gameBoard = this;

				pieceObject.GetComponent<MeshRenderer> ().material.SetTextureScale ("_MainTex",scale);
				pieceObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(i*scale.x,j*scale.y));
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
		var puzzlePivot = new PuzzlePivot (gameObject);
		puzzlePivots.Add (puzzlePivot);
		SpawnPieces (puzzlePivot, snapShot, new Vector2 (0.5f,0.33f));
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
				} else {
					StartCoroutine (ZoomOut ());
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
