using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RotatingPuzzlePivot : PuzzlePivot {
	private PuzzlePivot internalPuzzlePivot;
	private Texture2D noPiecesTexture;
	public RotatingPuzzlePivot(GameObject parent, Vector2 sizeOfPicture, GameBoard gameboard, SubPuzzle subPuzzle) : base(parent, sizeOfPicture, gameboard, subPuzzle) {
		internalPuzzlePivot = new JigsawPuzzlePivot (parent,sizeOfPicture,gameboard, subPuzzle);
		internalPuzzlePivot.numberOfPieces = new Vector2(2,2);
		internalPuzzlePivot.subPuzzle = subPuzzle;
	}

	public override void SetPiecePosition() {
		int i = 0;
		foreach(var piece in pieces) {
			AssignToSnapablePoint (piece, snapablePoints [i]);
			i++;
		}
	}

	internal override IEnumerator SpawnPieces(Texture texture) {
		// Take picture of background Quad
		var cachePos = subPuzzle.BackgroundQuad.transform.localPosition;
		subPuzzle.BackgroundQuad.transform.localPosition = new Vector3 (0, 0, 0);
		yield return null;
		noPiecesTexture = subPuzzle.TakeSnapShot ();
		subPuzzle.BackgroundQuad.transform.localPosition = cachePos;
		yield return null;

		// Take picture of A side
		yield return internalPuzzlePivot.SpawnPieces (texture);
		internalPuzzlePivot.ScramblePiecePosition ();
		internalPuzzlePivot.pieces[0].gameObject.SetActive (false);
		internalPuzzlePivot.pieces[1].gameObject.SetActive (false);
		yield return null;
		var snapShot = subPuzzle.TakeSnapShot ();

		// Take picture of B side
		subPuzzle.SetBackgroundColor(1);
		internalPuzzlePivot.pieces[0].gameObject.SetActive (true);
		internalPuzzlePivot.pieces[1].gameObject.SetActive (true);
		internalPuzzlePivot.pieces[2].gameObject.SetActive (false);
		internalPuzzlePivot.pieces[3].gameObject.SetActive (false);
		yield return null;
		var bSidePicture = subPuzzle.TakeSnapShot ();

		// Set pictures for A and B side
		yield return base.SpawnPieces (snapShot);
		SetTextureForPieces (bSidePicture, false);

		internalPuzzlePivot.pivot.gameObject.transform.localPosition = new Vector3 (0,0,-5);
		internalPuzzlePivot.pieces[0].gameObject.SetActive (false);
		internalPuzzlePivot.pieces[1].gameObject.SetActive (false);
		internalPuzzlePivot.pieces[2].gameObject.SetActive (false);
		internalPuzzlePivot.pieces[3].gameObject.SetActive (false);
	}

	public override void PieceClicked(Piece piece, Vector3 mousePosInWorld) {
		var currentSnapablePoint = SnapablePoint.GetSnapablePointWithPieceId (this, piece.id);
		var snapablePoints = new List<SnapablePoint> ();
		snapablePoints.Add (currentSnapablePoint);

		foreach (var snapablePoint in snapablePoints) {
			if (snapablePoint != null && snapablePoint.piece != null) {
				snapablePoint.piece.Rotate(snapablePoint.piece.transform.localEulerAngles+new Vector3(0,180,0),0.2f,() => gameboard.CheckForWin ());
			}
		}
	}

	internal override bool CheckForWin() {
		if (base.CheckForWin ()) {
			internalPuzzlePivot.pieces[2].gameObject.SetActive (true);
			internalPuzzlePivot.pieces[3].gameObject.SetActive (true);
			SetTextureForPieces (noPiecesTexture);
		}
		return false;
	}

	public override void TouchReleased() {
		internalPuzzlePivot.TouchReleased ();
	}

	public override void CustomUpdate() {
		internalPuzzlePivot.CustomUpdate ();
	}
}
