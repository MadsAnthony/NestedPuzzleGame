using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {

	[SerializeField] private GameObject piece;
	[SerializeField] private Camera puzzleCamera;
	[SerializeField] private RenderTexture puzzleCameraTexture;
	[SerializeField] private Texture snapShot;

	public Piece draggablePiece;
	public Vector3 draggablePieceOffset;

	private List<Piece> pieces  = new List<Piece>();
	private List<Vector3> snapablePoints = new List<Vector3>();
	private Vector3 startOffset = new Vector3(-1,0,0);
	private RenderTexture puzzleTexture;
	void Start () {
		Application.targetFrameRate = 60;

		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 3; j++) {
				var newSnapablePoint = new Vector3 (i * 2, j * 2, 0) + startOffset;
				snapablePoints.Add(newSnapablePoint);
				var pieceObject = GameObject.Instantiate (piece).GetComponent<Piece>();
				pieceObject.transform.position = new Vector3 (i+0.2f,j*2+0.2f,0)+startOffset;
				pieceObject.gameBoard = this;
				pieceObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(i*0.5f,j*0.33f));
				pieces.Add (pieceObject);
			}
		}
	}

	int counter = 0;
	void Update() {
		counter++;
		if (counter == 10) {
			Graphics.CopyTexture (puzzleCameraTexture, snapShot);
			foreach (var piece in pieces) {
				piece.GetComponent<MeshRenderer> ().material.SetTexture ("_MainTex", snapShot);
			}
		}

		if (draggablePiece != null) {
			var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			draggablePiece.transform.position = new Vector3(mousePos.x,mousePos.y,draggablePiece.transform.position.z)+draggablePieceOffset;
		}

		if (Input.GetMouseButtonUp (0) && (draggablePiece != null)) {
			foreach (var snapablePoint in snapablePoints) {
				if (IsWithinRadius (snapablePoint, draggablePiece.transform.position, 0.2f)) {
					draggablePiece.transform.position = snapablePoint;
					break;
				}
			}

			draggablePiece = null;
		}
	}

	private bool IsWithinRadius(Vector3 pointA, Vector3 pointB, float radius) {
		var dist = pointA - pointB;
		return dist.magnitude < radius;
	}
}
