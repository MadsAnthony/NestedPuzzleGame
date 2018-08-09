using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SlidingPuzzlePivot : PuzzlePivot {
	private Piece blankPiece;
	public SlidingPuzzlePivot(GameObject parent, Vector2 sizeOfPicture, GameBoard gameboard) : base(parent, sizeOfPicture, gameboard) {
	}

	public override void SetPiecePosition() {
		int i = 0;
		foreach(var piece in pieces) {
			AssignToSnapablePoint (piece, snapablePoints [i]);
			i++;
		}

		blankPiece = pieces [0];
		blankPiece.PieceRenderer.enabled = false;
		blankPiece.PieceRendererBack.enabled = false;
		blankPiece.Backdrop.SetActive (false);

		int randomDir = 0;
		for (int j = 0; j<pieces.Count*5; j++) {
			var dirList = new List<Direction> ();
			var addRandom = (int)Random.Range (0, 2);
			randomDir += addRandom;
			randomDir %= 4;
			var blankSnapablePoint = SnapablePoint.GetSnapablePointWithPieceId (this, blankPiece.id);
			var snapablePoint = SnapablePoint.GetSnapablePointFromDirection(this, blankSnapablePoint, (Direction)randomDir);
			if (snapablePoint != null) {
				MovePiece (snapablePoint.piece,0);
			}
		}
	}

	public override void PieceClicked(Piece piece, Vector3 mousePosInWorld) {
		var didMove = MovePiece (piece, 0.2f);

		if (!didMove) {
			piece.Jiggle ();
		}
	}

	private bool MovePiece(Piece piece, float sec) {
		bool didMove = false;
		var piecePos = piece.transform.localPosition;

		var currentSnapablePoint = SnapablePoint.GetSnapablePointWithPieceId (this, piece.id);
		var snapablePointRight = SnapablePoint.GetSnapablePointFromRelativePosition (this, currentSnapablePoint, new Vector2 (1,0));
		var snapablePointLeft = SnapablePoint.GetSnapablePointFromRelativePosition (this, currentSnapablePoint, new Vector2 (-1,0));
		var snapablePointUp = SnapablePoint.GetSnapablePointFromRelativePosition (this, currentSnapablePoint, new Vector2 (0,1));
		var snapablePointDown = SnapablePoint.GetSnapablePointFromRelativePosition (this, currentSnapablePoint, new Vector2 (0,-1));
		if (snapablePointRight != null && snapablePointRight.piece == blankPiece) {
			piece.Move (blankPiece.transform.localPosition, sec, () => { AssignToSnapablePoint (piece, snapablePointRight);});
			blankPiece.Move (piecePos, sec, () => { AssignToSnapablePoint (blankPiece, currentSnapablePoint);});
			didMove = true;
		}
		if (snapablePointLeft 	!= null && snapablePointLeft.piece 	== blankPiece) {
			piece.Move (blankPiece.transform.localPosition, sec, () => { AssignToSnapablePoint (piece, snapablePointLeft);});
			blankPiece.Move (piecePos, sec, () => { AssignToSnapablePoint (blankPiece, currentSnapablePoint);});
			didMove = true;
		}
		if (snapablePointUp 	!= null && snapablePointUp.piece 	== blankPiece) {
			piece.Move (blankPiece.transform.localPosition, sec, () => { AssignToSnapablePoint (piece, snapablePointUp);});
			blankPiece.Move (piecePos, sec, () => { AssignToSnapablePoint (blankPiece, currentSnapablePoint);});
			didMove = true;
		}
		if (snapablePointDown 	!= null && snapablePointDown.piece 	== blankPiece) {
			piece.Move (blankPiece.transform.localPosition, sec, () => { AssignToSnapablePoint (piece, snapablePointDown);});
			blankPiece.Move (piecePos, sec, () => { AssignToSnapablePoint (blankPiece, currentSnapablePoint);});
			didMove = true;
		}

		return didMove;
	}

	public override void TouchReleased() {
	}

	public override void CustomUpdate() {
	}
}
