using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPieceDictionary {
	public readonly Piece keyPiece;
	public readonly Vector3 originalPosition;
	public Dictionary<Vector2, string> pieceDictionary = new Dictionary<Vector2, string>();

	private KeyPieceDictionary(Piece keyPiece) {
		this.keyPiece = keyPiece;
		originalPosition = keyPiece.transform.localPosition;
	}

	public static KeyPieceDictionary SetupKeyPieceDictionary(PuzzlePivot pivot, List<Piece> pieces){
		var keyPiece = pieces[0];
		var keyPieceDictionary = new KeyPieceDictionary(keyPiece);

		var keyPieceSnapablePoint = SnapablePoint.GetSnapablePointWithPieceId(pivot, keyPiece.id);
		var keyPieceId = pivot.snapablePoints.IndexOf(keyPieceSnapablePoint);

		foreach (var piece in pieces) {
			var snapablePoint = SnapablePoint.GetSnapablePointWithPieceId(pivot, piece.id);

			var relativePos = SnapablePoint.GetRelativePosition(pivot, keyPieceId, pivot.snapablePoints.IndexOf(snapablePoint));
			keyPieceDictionary.pieceDictionary.Add(relativePos,piece.FullId);
		}

		return keyPieceDictionary;
	}

	public bool IsPiecesPlacedCorrectly(PuzzlePivot puzzlePivot) {
		var collectableSnapablePoint = SnapablePoint.GetSnapablePointWithPieceId(puzzlePivot, keyPiece.id);
		if (collectableSnapablePoint == null) return false;

		foreach (var piecePairValue in pieceDictionary) {
			var snapablePoint = SnapablePoint.GetSnapablePointFromRelativePosition(puzzlePivot, collectableSnapablePoint, piecePairValue.Key);
			if (snapablePoint == null) return false;
			var snapablePiece = snapablePoint.piece;
			if (snapablePiece == null || snapablePiece.FullId != piecePairValue.Value) return false;
		}

		return true;
	}
}
