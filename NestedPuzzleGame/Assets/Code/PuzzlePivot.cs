using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class PuzzlePivot {
	public GameBoard gameboard;
	public GameObject pivot;
	public List<Piece> pieces = new List<Piece>();
	public List<SnapablePoint> snapablePoints = new List<SnapablePoint>();
	public GameObject collectableObject;
	public Vector2 numberOfPieces;
	private Vector2 sizeOfPicture;

	public PuzzlePivot(GameObject parent, Vector2 sizeOfPicture, GameBoard gameboard) {
		pivot = new GameObject();
		pivot.transform.parent = parent.transform;
		pivot.transform.localPosition = new Vector3(0,0,0);
		this.gameboard = gameboard;
	}

	public SnapablePoint GetPointWithinRadius(Vector3 point, float radius) {
		foreach (var snapablePoint in snapablePoints) {
			point.z = snapablePoint.position.z;
			var dist = snapablePoint.position - point;
			if (dist.magnitude < radius) {
				return snapablePoint;
			}
		}
		return null;
	}

	internal void SetPieceAsToHighestDepth(Piece piece) {
		if (!pieces.Contains (piece)) return;
		pieces = pieces.OrderBy (x => x.transform.localPosition.z).Reverse().ToList();
		pieces.Remove (piece);
		pieces.Add (piece);
		SetDepthOfPieces ();
	}

	public void SetDepthOfPieces() {
		float pieceZPosition = 0;
		foreach (var piece in pieces)  {
			piece.transform.localPosition = new Vector3(piece.transform.localPosition.x,piece.transform.localPosition.y,pieceZPosition);
			pieceZPosition += -0.1f;
		}
	}

	internal void AssignToSnapablePoint(Piece piece, SnapablePoint snapablePoint) {
		snapablePoint.piece = piece;
		snapablePoint.piece.transform.localPosition = new Vector3(snapablePoint.position.x, snapablePoint.position.y, 0);
		snapablePoint.piece.Backdrop.SetActive (false);
		gameboard.CheckForWin ();
	}

	internal void ScramblePiecePosition() {
		foreach (var piece in pieces) {
			var x = sizeOfPicture.x * 0.5f;
			var y = sizeOfPicture.y * 0.5f;
			var pieceScaleX = piece.transform.localScale.x*0.5f;
			var pieceScaleY = piece.transform.localScale.x*0.5f;


			var randomX = UnityEngine.Random.Range ((-x+pieceScaleX)*100,(x-pieceScaleX)*100)*0.01f;
			var randomY = UnityEngine.Random.Range ((-y+pieceScaleY)*100,(y-pieceScaleY)*100)*0.01f;
			piece.transform.localPosition = new Vector3 (randomX,randomY,piece.transform.localPosition.z);
		}
	}

	public abstract void SetPiecePosition ();
	public abstract void PieceClicked (Piece piece, Vector3 mousePosInWorld);
	public abstract void TouchReleased ();
	public abstract void CustomUpdate();
}
