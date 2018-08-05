using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SlidingPuzzlePivot : PuzzlePivot {
	private Piece blankPiece;
	public SlidingPuzzlePivot(GameObject parent, Vector2 sizeOfPicture, GameBoard gameboard) : base(parent, sizeOfPicture, gameboard) {
	}

	public override void SetPiecePosition() {
		var shuffledPieces = pieces.OrderBy(a => Random.Range(0,100));
		int i = 0;
		foreach(var piece in shuffledPieces) {
			AssignToSnapablePoint (piece, snapablePoints [i]);
			i++;
		}

		blankPiece = pieces [0];
		blankPiece.PieceRenderer.enabled = false;
		blankPiece.Backdrop.SetActive (false);
	}

	public override void PieceClicked(Piece piece, Vector3 mousePosInWorld) {
		bool didMove = false;
		var piecePos = piece.transform.localPosition;

		var currentSnapablePoint = SnapablePoint.GetSnapablePointWithPieceId (this, piece.id);
		var snapablePointRight = SnapablePoint.GetSnapablePointFromRelativePosition (this, currentSnapablePoint, new Vector2 (1,0));
		var snapablePointLeft = SnapablePoint.GetSnapablePointFromRelativePosition (this, currentSnapablePoint, new Vector2 (-1,0));
		var snapablePointUp = SnapablePoint.GetSnapablePointFromRelativePosition (this, currentSnapablePoint, new Vector2 (0,1));
		var snapablePointDown = SnapablePoint.GetSnapablePointFromRelativePosition (this, currentSnapablePoint, new Vector2 (0,-1));

		float sec = 0.2f;
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

		if (!didMove) {
			piece.Jiggle ();
		}
	}

	public override void TouchReleased() {
	}

	public override void CustomUpdate() {
	}
}
