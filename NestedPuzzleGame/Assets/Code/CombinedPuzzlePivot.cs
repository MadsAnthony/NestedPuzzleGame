﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class CombinedPuzzlePivot : PuzzlePivot {
	internal PuzzlePivot internalPuzzlePivot;

	public CombinedPuzzlePivot(GameObject parent, Vector2 sizeOfPicture, GameBoard gameboard, SubPuzzle subPuzzle) : base(parent, sizeOfPicture, gameboard, subPuzzle) {
		internalPuzzlePivot = new JigsawPuzzlePivot (parent,sizeOfPicture,gameboard, subPuzzle);
		internalPuzzlePivot.numberOfPieces = new Vector2(2,2);
		internalPuzzlePivot.subPuzzle = subPuzzle;
	}

	internal class PiecePivot {
		public GameObject pivot;
		public Texture2D picture;
		public Texture2D pictureNoPieces;
		public KeyPieceDictionary goalKeyPieceDictionary;
		public int pieceRendererIndex;
	}

	private List<PiecePivot> piecePivots = new List<PiecePivot>();

	private void HideAllPiecePivots() {
		foreach(var piecePivot in piecePivots) {
			piecePivot.pivot.SetActive (false);
		}
	}

	internal virtual PuzzlePivot TopPivot () {
		return this;
	}

	internal abstract void SetPiecePivotsExtraRenderer (List<PiecePivot> piecePivots);

	internal abstract void SetPiecePivotsGoal (List<PiecePivot> piecePivots);

	internal abstract void PiecePivotPieceClicked(Piece piece, Vector3 mousePosInWorld);

	internal override IEnumerator SpawnPieces(Texture texture) {
		yield return internalPuzzlePivot.SpawnPieces (texture);
		internalPuzzlePivot.ScramblePiecePosition ();
		yield return null;

		piecePivots = CreatePiecePivotList();

		yield return TakeSnapShotsOfPiecePivots (piecePivots);

		if (TopPivot () == this) {
			yield return base.SpawnPieces (null);
		} else {
			yield return TopPivot ().SpawnPieces (null);
		}

		SetPiecePivotsExtraRenderer (piecePivots);

		for (int i = 0; i<piecePivots.Count; i++) {
			TopPivot().SetTextureForPiecesRenderer (piecePivots[i].picture, piecePivots[i].pieceRendererIndex);
		}

		SetPiecePivotsGoal (piecePivots);

		internalPuzzlePivot.pivot.gameObject.transform.localPosition = new Vector3 (0,0,-5);
		HideAllPiecePivots();
	}

	private List<PiecePivot> CreatePiecePivotList() {
		var piecePivots = new List<PiecePivot> ();
		var nPiecesInPivot = internalPuzzlePivot.pieces.Count / 2;
		for (int i = 0; i < 2; i++) {
			var piecePivot = new PiecePivot ();
			var piecePivotGameObject = new GameObject ();
			piecePivot.pivot = piecePivotGameObject;

			piecePivotGameObject.transform.parent = pivot.transform;
			piecePivotGameObject.transform.localPosition = new Vector3 (0,0,-1);
			for (int j = 0; j <nPiecesInPivot; j++) {
				internalPuzzlePivot.pieces [i*nPiecesInPivot+j].transform.parent = piecePivotGameObject.transform;
			}
			piecePivots.Add (piecePivot);
		}
		return piecePivots;
	}

	private IEnumerator TakeSnapShotsOfPiecePivots(List<PiecePivot> piecePivots) {
		for (int i = 0; i<piecePivots.Count; i++) {
			yield return TakeSnapShotOfSide(i, piecePivots[i]);
		}
	}

	private IEnumerator TakeSnapShotOfBackgroundQuad(System.Action<Texture2D> snapShotAction) {
		var cachePosA = subPuzzle.BackgroundQuad.transform.localPosition;
		subPuzzle.BackgroundQuad.transform.localPosition = new Vector3 (0, 0, 0);
		yield return null;
		var sideNoPiecesTexture = subPuzzle.TakeSnapShot ();
		subPuzzle.BackgroundQuad.transform.localPosition = cachePosA;
		yield return null;
		snapShotAction(sideNoPiecesTexture);
	}

	private IEnumerator TakeSnapShotOfSide(int sideIndex, PiecePivot piecePivot) {
		subPuzzle.SetBackgroundColor(sideIndex);
		HideAllPiecePivots();
		// Take picture of background Quad
		Texture2D sideNoPiecesTexture = null;
		yield return TakeSnapShotOfBackgroundQuad ((picture) => sideNoPiecesTexture = picture);

		// Take picture of a side
		HideAllPiecePivots();
		piecePivots [sideIndex].pivot.SetActive (true);
		yield return null;
		var sidePicture = subPuzzle.TakeSnapShot ();

		piecePivot.picture = sidePicture;
		piecePivot.pictureNoPieces = sideNoPiecesTexture;
	}

	internal KeyPieceDictionary SetupKeyPieceDictionary(Vector3 angle) {
		var pivot = TopPivot ();

		int k = 0;
		foreach (var snapablePoint in pivot.snapablePoints) {
			pivot.pieces[k].transform.localPosition = new Vector3(snapablePoint.position.x,snapablePoint.position.y,pivot.pieces[k].transform.localPosition.z);
			pivot.pieces[k].transform.localEulerAngles = angle;
			snapablePoint.piece = pivot.pieces[k];
			k++;
		}
		var goalKeyPieceDictionary = KeyPieceDictionary.SetupKeyPieceDictionary (pivot, pivot.pieces);

		foreach (var snapablePoint in pivot.snapablePoints) {
			snapablePoint.piece = null;
		}

		return goalKeyPieceDictionary;
	}

	public override void PieceClicked(Piece piece, Vector3 mousePosInWorld) {
		bool allPiecePivotsCompleted = true;
		foreach (var piecePivot in piecePivots) {
			if (!piecePivot.pivot.activeSelf) {
				allPiecePivotsCompleted = false;
			}
		}
		if (allPiecePivotsCompleted) return;

		PiecePivotPieceClicked (piece, mousePosInWorld);
	}

	internal override bool CheckForWin() {
		int i = 0;
		foreach (var piecePivot in piecePivots) {
			if (piecePivot.goalKeyPieceDictionary.IsPiecesPlacedCorrectly (TopPivot())) {
				piecePivot.pivot.SetActive (true);
				SetTextureForPiecesRenderer (piecePivot.pictureNoPieces, piecePivots[i].pieceRendererIndex);
			}
			i++;
		}
		return internalPuzzlePivot.CheckForWin();
	}

	public override void TouchReleased() {
		internalPuzzlePivot.TouchReleased ();

		if (TopPivot() != this) {
			TopPivot ().TouchReleased ();
		}
	}

	public override void CustomUpdate() {
		internalPuzzlePivot.CustomUpdate ();

		if (TopPivot() != this) {
			TopPivot ().CustomUpdate ();
		}
	}
}
