using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RotatingPuzzlePivot : CombinedPuzzlePivot {
	
	public RotatingPuzzlePivot(GameObject parent, Vector2 sizeOfPicture, Vector2 numberOfPieces, GameBoard gameboard, SubPuzzle subPuzzle) : base(parent, sizeOfPicture, numberOfPieces, gameboard, subPuzzle) {
		
	}

	public override void SetPiecePosition() {
		int i = 0;
		foreach(var piece in pieces) {
			var randomNumber = Random.Range(0, 100);
			if (randomNumber > 50) {
				piece.transform.localEulerAngles += new Vector3(0,180,0);
			}
			AssignToSnapablePoint (piece, snapablePoints [i]);
			i++;
		}
	}

	internal override void SetPiecePivotsExtraRenderer (List<PiecePivot> piecePivots) {
		for (int i = 1; i<piecePivots.Count; i++) {
			piecePivots[i].pieceRendererIndex = SpawnExtraPieceRenderer ((pieceRenderer) => {
				pieceRenderer.transform.localEulerAngles = new Vector3(0,180,0);
				pieceRenderer.transform.localPosition = new Vector3(0,0,0.2f);
			});
		}
	}

	internal override void ScramblePiecesAndAssignToSnapablePoints (int piecePivotIndex, int pieceRendererIndex) {
		var pivot = TopPivot ();
		int i = 0;
		foreach (var snapablePoint in TopPivot().snapablePoints) {
			pivot.pieces[i].transform.localPosition = new Vector3(snapablePoint.position.x,snapablePoint.position.y,pivot.pieces[i].transform.localPosition.z);
			pivot.pieces[i].transform.localEulerAngles = new Vector3 (0, 180 * piecePivotIndex, 0);
			snapablePoint.piece = pivot.pieces[i];
			i++;
		}
	}

	internal override void PiecePivotPieceClicked(Piece piece, Vector3 mousePosInWorld) {
		foreach (var snapablePoint in snapablePoints) {
			if (snapablePoint != null && snapablePoint.piece == piece) {
				snapablePoint.piece.Rotate(snapablePoint.piece.transform.localEulerAngles+new Vector3(0,180,0),0.2f,() => gameboard.CheckForWin ());
			}
		}
	}
}
