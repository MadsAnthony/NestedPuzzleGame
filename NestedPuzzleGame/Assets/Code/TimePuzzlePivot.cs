using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TimePuzzlePivot : CombinedPuzzlePivot {
	PuzzlePivot topPuzzlePivot;
	public TimePuzzlePivot(GameObject parent, Vector2 sizeOfPicture, Vector2 numberOfPieces, GameBoard gameboard, SubPuzzle subPuzzle) : base(parent, sizeOfPicture, numberOfPieces, gameboard, subPuzzle) {
		topPuzzlePivot = new JigsawPuzzlePivot (parent, sizeOfPicture, numberOfPieces, gameboard, subPuzzle);
		topPuzzlePivot.subPuzzle = subPuzzle;
	}

	public override void SetPiecePosition() {
		topPuzzlePivot.ScramblePiecePosition ();
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

	internal override void ScramblePiecesAndAssignToSnapablePoints (int piecePivotIndex, int pieceRendererIndex) {
		var pivot = TopPivot ();

		var shuffledPieces = pivot.pieces.OrderBy( x => UnityEngine.Random.Range(0, pivot.pieces.Count) ).ToList( );
		int i = 0;
		foreach (var piece in shuffledPieces) {
			var snapablePoint = pivot.snapablePoints [i];
			piece.PieceRendererList[pieceRendererIndex].GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", snapablePoint.initialPieceTextureOffset);
			piece.transform.localPosition = new Vector3(snapablePoint.position.x,snapablePoint.position.y,pivot.pieces[i].transform.localPosition.z);
			snapablePoint.piece = piece;
			i++;
		}
	}

	internal override void PiecePivotPieceClicked(Piece piece, Vector3 mousePosInWorld) {
		
	}

	private float t;
	public override void CustomUpdate() {
		base.CustomUpdate ();

		if (allPiecePivotsCompleted) return;

		t += Time.deltaTime*4;
		foreach (var piece in topPuzzlePivot.pieces) {
			var color = piece.PieceRendererList[1].GetComponent<MeshRenderer>().material.color;
			color.a = (Mathf.Sin(t)+1)/2;
			piece.PieceRendererList[1].GetComponent<MeshRenderer>().material.color = color;
		}
	}

}
