using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {

	[SerializeField] private GameObject piece;
	[SerializeField] private Camera puzzleCamera;
	[SerializeField] private RenderTexture puzzleCameraTexture;
	[SerializeField] private Texture goalTexture;
	[SerializeField] private Texture snapShot;

	public Piece draggablePiece;
	public Vector3 draggablePieceOffset;

	private List<SnapablePoint> snapablePoints = new List<SnapablePoint>();
	private Vector3 startOffset = new Vector3(-1,0,0);
	private RenderTexture puzzleTexture;

	private GameObject pivot1;
	private GameObject pivot2;

	private List<Piece> pieces1  = new List<Piece>();
	private List<Piece> pieces2  = new List<Piece>();
	void Start () {
		Application.targetFrameRate = 60;

		pivot1 = new GameObject ();
		SpawnPieces (pivot1, pieces1, goalTexture);
		ScramblePiecePosition (pieces1);
	}

	private void SpawnPieces(GameObject pivot, List<Piece> pieces, Texture texture) {
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
				pieceObject.transform.parent = pivot.transform;
				pieces.Add (pieceObject);
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

	int counter = 0;
	void Update() {
		counter++;
		if (counter == 5) {
			pivot1.SetActive (false);
			Graphics.CopyTexture (puzzleCameraTexture, snapShot);
			pivot2 = new GameObject ();
			SpawnPieces (pivot2, pieces2, snapShot);
			ScramblePiecePosition (pieces2);
		}

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
				pivot1.SetActive (true);
				pivot2.SetActive (false);
			}
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
		foreach(var piece in pieces2) {
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
}
