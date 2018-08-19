using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RotatingPuzzlePivot : PuzzlePivot {
	private PuzzlePivot internalPuzzlePivot;
	private Texture2D aSideNoPiecesTexture;
	private Texture2D bSideNoPiecesTexture;
	public RotatingPuzzlePivot(GameObject parent, Vector2 sizeOfPicture, GameBoard gameboard, SubPuzzle subPuzzle) : base(parent, sizeOfPicture, gameboard, subPuzzle) {
		internalPuzzlePivot = new JigsawPuzzlePivot (parent,sizeOfPicture,gameboard, subPuzzle);
		internalPuzzlePivot.numberOfPieces = new Vector2(2,2);
		internalPuzzlePivot.subPuzzle = subPuzzle;
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

	private List<Piece> aPieces = new List<Piece>();
	private List<Piece> bPieces = new List<Piece>();
	internal override IEnumerator SpawnPieces(Texture texture) {
		// Take picture of background Quad
		var cachePosA = subPuzzle.BackgroundQuad.transform.localPosition;
		subPuzzle.BackgroundQuad.transform.localPosition = new Vector3 (0, 0, 0);
		yield return null;
		aSideNoPiecesTexture = subPuzzle.TakeSnapShot ();
		subPuzzle.BackgroundQuad.transform.localPosition = cachePosA;
		yield return null;

		// Take picture of A side
		yield return internalPuzzlePivot.SpawnPieces (texture);
		internalPuzzlePivot.ScramblePiecePosition ();
		internalPuzzlePivot.pieces[0].gameObject.SetActive (false);
		internalPuzzlePivot.pieces[1].gameObject.SetActive (false);
		aPieces.Add(internalPuzzlePivot.pieces[2]);
		aPieces.Add(internalPuzzlePivot.pieces[3]);
		yield return null;
		var snapShot = subPuzzle.TakeSnapShot ();

		// Take picture of B side
		subPuzzle.SetBackgroundColor(1);
		internalPuzzlePivot.pieces[0].gameObject.SetActive (true);
		internalPuzzlePivot.pieces[1].gameObject.SetActive (true);
		internalPuzzlePivot.pieces[2].gameObject.SetActive (false);
		internalPuzzlePivot.pieces[3].gameObject.SetActive (false);
		bPieces.Add(internalPuzzlePivot.pieces[0]);
		bPieces.Add(internalPuzzlePivot.pieces[1]);
		yield return null;
		var bSidePicture = subPuzzle.TakeSnapShot ();
		
		internalPuzzlePivot.pieces[0].gameObject.SetActive (false);
		internalPuzzlePivot.pieces[1].gameObject.SetActive (false);
		// Take picture of background Quad
		var cachePosB = subPuzzle.BackgroundQuad.transform.localPosition;
		subPuzzle.BackgroundQuad.transform.localPosition = new Vector3 (0, 0, 0);
		yield return null;
		bSideNoPiecesTexture = subPuzzle.TakeSnapShot ();
		subPuzzle.BackgroundQuad.transform.localPosition = cachePosB;
		yield return null;
		
		// Set pictures for A and B side
		yield return base.SpawnPieces (snapShot);
		SetTextureForPieces (bSidePicture, false);

		internalPuzzlePivot.pivot.gameObject.transform.localPosition = new Vector3 (0,0,-5);
		internalPuzzlePivot.pieces[0].gameObject.SetActive (false);
		internalPuzzlePivot.pieces[1].gameObject.SetActive (false);
		internalPuzzlePivot.pieces[2].gameObject.SetActive (false);
		internalPuzzlePivot.pieces[3].gameObject.SetActive (false);



		int i = 0;
		foreach (var snapablePoint in snapablePoints) {
			pieces[i].transform.localPosition = new Vector3(snapablePoint.position.x,snapablePoint.position.y,pieces[i].transform.localPosition.z);
			snapablePoint.piece = pieces[i];
			i++;
		}

		aGoalKeyPieceDictionary = KeyPieceDictionary.SetupKeyPieceDictionary (this, pieces);
		
		i = 0;
		foreach (var snapablePoint in snapablePoints) {
			pieces[i].transform.localEulerAngles += new Vector3(0,180,0);
			i++;
		}
		
		bGoalKeyPieceDictionary = KeyPieceDictionary.SetupKeyPieceDictionary (this, pieces);

		foreach (var snapablePoint in snapablePoints) {
			snapablePoint.piece = null;
		}
		
	}

	private KeyPieceDictionary aGoalKeyPieceDictionary;
	private KeyPieceDictionary bGoalKeyPieceDictionary;

	public override void PieceClicked(Piece piece, Vector3 mousePosInWorld) {
		if (aSideComplete && bSideComplete) return;
		var currentSnapablePoint = SnapablePoint.GetSnapablePointWithPieceId (this, piece.id);
		var snapablePoints = new List<SnapablePoint> ();
		snapablePoints.Add (currentSnapablePoint);

		foreach (var snapablePoint in snapablePoints) {
			if (snapablePoint != null && snapablePoint.piece != null) {
				snapablePoint.piece.Rotate(snapablePoint.piece.transform.localEulerAngles+new Vector3(0,180,0),0.2f,() => gameboard.CheckForWin ());
			}
		}
	}

	private bool aSideComplete = false;
	private bool bSideComplete = false;
	internal override bool CheckForWin() {
		if (aGoalKeyPieceDictionary.IsPiecesPlacedCorrectly(this)) {
			foreach (var aPiece in aPieces) {
				aPiece.gameObject.SetActive (true);
			}
			
			SetTextureForPieces (aSideNoPiecesTexture);
			aSideComplete = true;
		}

		if (bGoalKeyPieceDictionary.IsPiecesPlacedCorrectly(this)) {
			foreach (var bPiece in bPieces) {
				bPiece.gameObject.SetActive (true);
			}
			
			SetTextureForPieces (bSideNoPiecesTexture, false);
			bSideComplete = true;
		}
		return internalPuzzlePivot.CheckForWin();
	}

	public override void TouchReleased() {
		internalPuzzlePivot.TouchReleased ();
	}

	public override void CustomUpdate() {
		internalPuzzlePivot.CustomUpdate ();
	}
}
