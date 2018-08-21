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

	private List<GameObject> piecePivots = new List<GameObject>();

	internal override IEnumerator SpawnPieces(Texture texture) {
		yield return internalPuzzlePivot.SpawnPieces (texture);
		internalPuzzlePivot.ScramblePiecePosition ();
		yield return null;

		// Create new piecePivots and assign pieces
		var nPiecesInPivot = internalPuzzlePivot.pieces.Count / 2;
		for (int i = 0; i < 2; i++) {
			var piecePivot = new GameObject ();
			piecePivot.transform.parent = pivot.transform;
			piecePivot.transform.localPosition = new Vector3 (0,0,-1);
			for (int j = 0; j <nPiecesInPivot; j++) {
				internalPuzzlePivot.pieces [i*nPiecesInPivot+j].transform.parent = piecePivot.transform;
			}
			piecePivots.Add (piecePivot);
		}

		// Side A
		subPuzzle.SetBackgroundColor(0);
		HideAllPiecePivots();
		// Take picture of background Quad
		var cachePosA = subPuzzle.BackgroundQuad.transform.localPosition;
		subPuzzle.BackgroundQuad.transform.localPosition = new Vector3 (0, 0, 0);
		yield return null;
		aSideNoPiecesTexture = subPuzzle.TakeSnapShot ();
		subPuzzle.BackgroundQuad.transform.localPosition = cachePosA;
		yield return null;

		// Take picture of A side
		HideAllPiecePivots();
		piecePivots [1].SetActive (true);
		yield return null;
		var aSidePicture = subPuzzle.TakeSnapShot ();

		// Side B
		subPuzzle.SetBackgroundColor(1);
		HideAllPiecePivots();
		// Take picture of background Quad
		var cachePosB = subPuzzle.BackgroundQuad.transform.localPosition;
		subPuzzle.BackgroundQuad.transform.localPosition = new Vector3 (0, 0, 0);
		yield return null;
		bSideNoPiecesTexture = subPuzzle.TakeSnapShot ();
		subPuzzle.BackgroundQuad.transform.localPosition = cachePosB;
		yield return null;

		// Take picture of B side
		HideAllPiecePivots();
		piecePivots [0].SetActive (true);
		yield return null;
		var bSidePicture = subPuzzle.TakeSnapShot ();
		
		// Spawn actual pieces and set pictures for A and B side
		yield return base.SpawnPieces (null);
		SetTextureForPieces (aSidePicture, true);
		SetTextureForPieces (bSidePicture, false);

		internalPuzzlePivot.pivot.gameObject.transform.localPosition = new Vector3 (0,0,-5);
		HideAllPiecePivots();


		// Setup KeyPieceDictionary for A and B side
		int k = 0;
		foreach (var snapablePoint in snapablePoints) {
			pieces[k].transform.localPosition = new Vector3(snapablePoint.position.x,snapablePoint.position.y,pieces[k].transform.localPosition.z);
			snapablePoint.piece = pieces[k];
			k++;
		}

		aGoalKeyPieceDictionary = KeyPieceDictionary.SetupKeyPieceDictionary (this, pieces);
		
		k = 0;
		foreach (var snapablePoint in snapablePoints) {
			pieces[k].transform.localEulerAngles += new Vector3(0,180,0);
			k++;
		}
		
		bGoalKeyPieceDictionary = KeyPieceDictionary.SetupKeyPieceDictionary (this, pieces);

		foreach (var snapablePoint in snapablePoints) {
			snapablePoint.piece = null;
		}
	}

	private void HideAllPiecePivots() {
		foreach(var piecePivot in piecePivots) {
			piecePivot.SetActive (false);
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
			piecePivots [1].SetActive (true);
			
			SetTextureForPieces (aSideNoPiecesTexture);
			aSideComplete = true;
		}

		if (bGoalKeyPieceDictionary.IsPiecesPlacedCorrectly(this)) {
			piecePivots [0].SetActive (true);
			
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
