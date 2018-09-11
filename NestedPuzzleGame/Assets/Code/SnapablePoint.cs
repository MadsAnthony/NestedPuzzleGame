using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapablePoint {
	public readonly string id;
	public readonly Vector3 position;
	public readonly Vector3 initialPieceTextureOffset;
	public Piece piece;

	public SnapablePoint(string id, Vector3 position, Vector3 initialPieceTextureOffset) {
		this.id = id;
		this.position = position;
		this.initialPieceTextureOffset = initialPieceTextureOffset;
	}

	public static SnapablePoint GetSnapablePointFromRelativePosition(PuzzlePivot puzzlePivot, SnapablePoint currentSnapablePoint, Vector2 relativePosition) {
		var index = puzzlePivot.snapablePoints.IndexOf(currentSnapablePoint);
		int piecesOnY = (int)puzzlePivot.numberOfPieces.y;

		var newX = index + relativePosition.x*piecesOnY;
		var newY = index + relativePosition.y;

		if (newX < 0 || newX >= puzzlePivot.snapablePoints.Count) return null;
		if (newY < 0 || newY >= puzzlePivot.snapablePoints.Count) return null;

		if (newY < 0 || newY >= puzzlePivot.snapablePoints.Count) return null;

		if (Mathf.FloorToInt(newY/piecesOnY) != Mathf.FloorToInt((float)index/piecesOnY)) return null;

		var newIndex = (int)(newX + relativePosition.y);

		return puzzlePivot.snapablePoints[newIndex];
	}

	public static Vector2 GetRelativePosition(PuzzlePivot puzzlePivot, int currentIndex, int otherIndex) {
		int piecesOnY = (int)puzzlePivot.numberOfPieces.y;

		var currentX = Mathf.FloorToInt((float)currentIndex/piecesOnY);
		var currentY = currentIndex % piecesOnY;

		var otherX = Mathf.FloorToInt((float)otherIndex/piecesOnY);
		var otherY = otherIndex % piecesOnY;

		return new Vector2(otherX-currentX, otherY-currentY);
	}

	public static SnapablePoint GetSnapablePointWithPieceId(PuzzlePivot puzzlePivot, string id) {
		foreach (var snapablePoint in puzzlePivot.snapablePoints) {
			if (snapablePoint.piece != null && snapablePoint.piece.id == id) {
				return snapablePoint;
			}
		}

		return null;
	}

	public static SnapablePoint GetSnapablePointFromDirection(PuzzlePivot puzzlePivot, SnapablePoint currentSnapablePoint, Direction direction) {
		var index = puzzlePivot.snapablePoints.IndexOf(currentSnapablePoint);
		int piecesOnY = (int)puzzlePivot.numberOfPieces.y;

		if (direction == Direction.Right) {
			var newIndex = index + piecesOnY;
			if (newIndex < 0 || newIndex >= puzzlePivot.snapablePoints.Count) return null;
			return puzzlePivot.snapablePoints[newIndex];
		}
		if (direction == Direction.Left) {
			var newIndex = index - piecesOnY;
			if (newIndex < 0 || newIndex >= puzzlePivot.snapablePoints.Count) return null;
			return puzzlePivot.snapablePoints[newIndex];
		}
		if (direction == Direction.Up) {
			var newIndex = index + 1;
			if (newIndex%piecesOnY == 0) return null;
			if (newIndex < 0 || newIndex >= puzzlePivot.snapablePoints.Count) return null;
			return puzzlePivot.snapablePoints[newIndex];
		}
		if (direction == Direction.Down) {
			var newIndex = index - 1;
			if (index%piecesOnY == 0) return null;
			if (newIndex < 0 || newIndex >= puzzlePivot.snapablePoints.Count) return null;
			return puzzlePivot.snapablePoints[newIndex];
		}
		return null;
	}
}
