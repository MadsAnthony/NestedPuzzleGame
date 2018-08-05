using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class JigsawPuzzlePivot : PuzzlePivot {
	private Piece draggablePiece;
	private Vector3 draggablePieceOffset;
	private SnapablePoint draggablePiecePrevSnapablePoint;

	public JigsawPuzzlePivot(GameObject parent, Vector2 sizeOfPicture, GameBoard gameboard) : base(parent, sizeOfPicture, gameboard) {
	}

	public override void SetPiecePosition() {
		ScramblePiecePosition ();
	}

	public override void PieceClicked(Piece piece, Vector3 mousePosInWorld) {
		var snapablePoint = GetPointWithinRadius(piece.transform.localPosition, 0.3f);
		if (snapablePoint != null && snapablePoint.piece == piece) {
			draggablePiecePrevSnapablePoint = snapablePoint;
			snapablePoint.piece = null;
		} else {
			draggablePiecePrevSnapablePoint = null;
		}

		piece.outline.SetActive(true);
		piece.Backdrop.SetActive (true);
		var offSet = piece.transform.position - mousePosInWorld;
		offSet.z = 0;
		draggablePieceOffset = offSet;
		draggablePiece = piece;
		SetPieceAsToHighestDepth(draggablePiece);
	}

	public override void TouchReleased() {
		if (draggablePiece != null) {
			DraggablePieceReleased ();
		}
	}

	private void DraggablePieceReleased() {
		var snapablePoint = GetPointWithinRadius (draggablePiece.transform.localPosition, 0.3f);
		if (snapablePoint != null) {
			if (snapablePoint.piece != null) {
				if (draggablePiecePrevSnapablePoint != null) {
					var snapablePointPiece = snapablePoint.piece;
					var prevSnapablePoint = draggablePiecePrevSnapablePoint;
					snapablePoint.piece.Move (prevSnapablePoint.position, () => {
						AssignToSnapablePoint (snapablePointPiece, prevSnapablePoint);
					});
					AssignToSnapablePoint (draggablePiece, snapablePoint);
				} else {
					draggablePiece.Jiggle ();
				}
			} else {
				AssignToSnapablePoint (draggablePiece, snapablePoint);
			}

		}
		draggablePiece.outline.SetActive (false);
		draggablePiece = null;
	}

	public override void CustomUpdate() {
		if (draggablePiece != null) {
			var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			draggablePiece.transform.position = new Vector3(mousePos.x,mousePos.y,draggablePiece.transform.position.z)+draggablePieceOffset;
		}
	}
}
