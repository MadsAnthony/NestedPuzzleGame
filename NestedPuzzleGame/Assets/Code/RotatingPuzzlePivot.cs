using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RotatingPuzzlePivot : PuzzlePivot {
	public RotatingPuzzlePivot(GameObject parent, Vector2 sizeOfPicture, GameBoard gameboard) : base(parent, sizeOfPicture, gameboard) {
	}

	public override void SetPiecePosition() {
		int i = 0;
		foreach(var piece in pieces) {
			AssignToSnapablePoint (piece, snapablePoints [i]);
			i++;
		}
	}

	public override void PieceClicked(Piece piece, Vector3 mousePosInWorld) {
		var currentSnapablePoint = SnapablePoint.GetSnapablePointWithPieceId (this, piece.id);
		var snapablePoints = new List<SnapablePoint> ();
		snapablePoints.Add (currentSnapablePoint);
		snapablePoints.Add(SnapablePoint.GetSnapablePointFromRelativePosition (this, currentSnapablePoint, new Vector2 (1,0)));
		snapablePoints.Add(SnapablePoint.GetSnapablePointFromRelativePosition (this, currentSnapablePoint, new Vector2 (-1,0)));
		snapablePoints.Add(SnapablePoint.GetSnapablePointFromRelativePosition (this, currentSnapablePoint, new Vector2 (0,1)));
		snapablePoints.Add(SnapablePoint.GetSnapablePointFromRelativePosition (this, currentSnapablePoint, new Vector2 (0,-1)));

		foreach (var snapablePoint in snapablePoints) {
			if (snapablePoint != null && snapablePoint.piece != null) {
				snapablePoint.piece.Rotate(snapablePoint.piece.transform.localEulerAngles+new Vector3(0,180,0),0.2f);
			}
		}
	}

	public override void TouchReleased() {
	}

	public override void CustomUpdate() {
	}
}
