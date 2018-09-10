using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TimePuzzlePivot : CombinedPuzzlePivot {
	PuzzlePivot topPuzzlePivot;
	public TimePuzzlePivot(GameObject parent, Vector2 sizeOfPicture, GameBoard gameboard, SubPuzzle subPuzzle) : base(parent, sizeOfPicture, gameboard, subPuzzle) {
		topPuzzlePivot = new JigsawPuzzlePivot (parent,sizeOfPicture,gameboard, subPuzzle);
		topPuzzlePivot.numberOfPieces = new Vector2(2,2);
		topPuzzlePivot.subPuzzle = subPuzzle;
	}

	public override void SetPiecePosition() {
		int i = 0;
		foreach(var piece in pieces) {
			AssignToSnapablePoint (piece, snapablePoints [i]);
			i++;
		}
	}

	internal override PuzzlePivot TopPivot () {
		return topPuzzlePivot;
	}

	internal override void SetPiecePivotsExtraRenderer (List<PiecePivot> piecePivots) {
		for (int i = 1; i<piecePivots.Count; i++) {
			piecePivots[i].pieceRendererIndex = topPuzzlePivot.SpawnExtraPieceRenderer ((pieceRenderer) => {
				pieceRenderer.transform.localPosition = new Vector3(0,0,-0.05f*i);
			});
		}
	}

	internal override void SetPiecePivotsGoal (List<PiecePivot> piecePivots) {
		for (int i = 0; i < piecePivots.Count; i++) {
			piecePivots[i].goalKeyPieceDictionary = SetupKeyPieceDictionary (new Vector3 (0, 0, 0));
		}
	}

	internal override void PiecePivotPieceClicked(Piece piece, Vector3 mousePosInWorld) {
		
	}

	private float t;
	public override void CustomUpdate() {
		base.CustomUpdate ();

		foreach (var piece in topPuzzlePivot.pieces) {
			t += Time.deltaTime;
			var color = piece.PieceRendererList[1].GetComponent<MeshRenderer>().material.color;
			color.a = (Mathf.Sin(t)+1)/2;
			piece.PieceRendererList[1].GetComponent<MeshRenderer>().material.color = color;
		}
	}

}
