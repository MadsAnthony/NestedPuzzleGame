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

	private class PiecePivot {
		public GameObject pivot;
		public Texture2D picture;
		public Texture2D pictureNoPieces;
		public KeyPieceDictionary goalKeyPieceDictionary;
	}

	private List<PiecePivot> piecePivots = new List<PiecePivot>();

	internal override IEnumerator SpawnPieces(Texture texture) {
		yield return internalPuzzlePivot.SpawnPieces (texture);
		internalPuzzlePivot.ScramblePiecePosition ();
		yield return null;

		piecePivots = CreatePiecePivotList();

		yield return TakeSnapShotsOfPiecePivots (piecePivots);

		yield return base.SpawnPieces (null);
		for (int i = 0; i<piecePivots.Count; i++) {
			SetTextureForPieces (piecePivots[i].picture, i<1);
		}

		for (int i = 0; i < piecePivots.Count; i++) {
			piecePivots[i].goalKeyPieceDictionary = SetupKeyPieceDictionary (new Vector3 (0, 180 * i, 0));
		}

		internalPuzzlePivot.pivot.gameObject.transform.localPosition = new Vector3 (0,0,-5);
		HideAllPiecePivots();
	}

	private void HideAllPiecePivots() {
		foreach(var piecePivot in piecePivots) {
			piecePivot.pivot.SetActive (false);
		}
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

	private KeyPieceDictionary SetupKeyPieceDictionary(Vector3 angle) {
		int k = 0;
		foreach (var snapablePoint in snapablePoints) {
			pieces[k].transform.localPosition = new Vector3(snapablePoint.position.x,snapablePoint.position.y,pieces[k].transform.localPosition.z);
			pieces[k].transform.localEulerAngles = angle;
			snapablePoint.piece = pieces[k];
			k++;
		}
		var goalKeyPieceDictionary = KeyPieceDictionary.SetupKeyPieceDictionary (this, pieces);

		foreach (var snapablePoint in snapablePoints) {
			snapablePoint.piece = null;
		}

		return goalKeyPieceDictionary;
	}

	private KeyPieceDictionary aGoalKeyPieceDictionary;
	private KeyPieceDictionary bGoalKeyPieceDictionary;

	public override void PieceClicked(Piece piece, Vector3 mousePosInWorld) {
		bool allPiecePivotsCompleted = true;
		foreach (var piecePivot in piecePivots) {
			if (!piecePivot.pivot.activeSelf) {
				allPiecePivotsCompleted = false;
			}
		}
		if (allPiecePivotsCompleted) return;

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
		int i = 0;
		foreach (var piecePivot in piecePivots) {
			if (piecePivot.goalKeyPieceDictionary.IsPiecesPlacedCorrectly (this)) {
				piecePivot.pivot.SetActive (true);
				SetTextureForPieces (piecePivot.pictureNoPieces, i<1);
			}
			i++;
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
