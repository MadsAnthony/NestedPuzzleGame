using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public abstract class PuzzlePivot {
	public GameBoard gameboard;
	public GameObject pivot;
	public List<Piece> pieces = new List<Piece>();
	public List<SnapablePoint> snapablePoints = new List<SnapablePoint>();
	public GameObject collectableObject;
	public SubPuzzle subPuzzle;
	public Vector2 numberOfPieces;
	private Vector2 sizeOfPicture;
	private KeyPieceDictionary goalKeyPieceDictionary;

	public PuzzlePivot(GameObject parent, Vector2 sizeOfPicture, Vector2 numberOfPieces, GameBoard gameboard, SubPuzzle subPuzzle) {
		pivot = new GameObject();
		pivot.transform.parent = parent.transform;
		pivot.transform.localPosition = new Vector3(0,0,0);
		this.gameboard = gameboard;
		this.subPuzzle = subPuzzle;
		this.sizeOfPicture = sizeOfPicture;
		this.numberOfPieces = numberOfPieces;
	}

	public SnapablePoint GetPointWithinRadius(Vector3 point, float radius) {
		foreach (var snapablePoint in snapablePoints) {
			point.z = snapablePoint.position.z;
			var dist = snapablePoint.position - point;
			if (dist.magnitude < radius) {
				return snapablePoint;
			}
		}
		return null;
	}

	internal void SetPieceAsToHighestDepth(Piece piece) {
		if (!pieces.Contains (piece)) return;
		pieces = pieces.OrderBy (x => x.transform.localPosition.z).Reverse().ToList();
		pieces.Remove (piece);
		pieces.Add (piece);
		SetDepthOfPieces ();
	}

	public void SetDepthOfPieces() {
		float pieceZPosition = 0;
		foreach (var piece in pieces)  {
			piece.transform.localPosition = new Vector3(piece.transform.localPosition.x,piece.transform.localPosition.y,pieceZPosition);
			pieceZPosition += -0.1f;
		}
	}

	public void SetupGoalKeypieceDictionary() {
		int i = 0;
		foreach (var snapablePoint in snapablePoints) {
			pieces[i].transform.localPosition = new Vector3(snapablePoint.position.x,snapablePoint.position.y,pieces[i].transform.localPosition.z);
			snapablePoint.piece = pieces[i];
			i++;
		}

		goalKeyPieceDictionary = KeyPieceDictionary.SetupKeyPieceDictionary (this, pieces);

		foreach (var snapablePoint in snapablePoints) {
			snapablePoint.piece = null;
		}
	}

	internal virtual bool CheckForWin() {
		return goalKeyPieceDictionary.IsPiecesPlacedCorrectly (this);
	}

	internal virtual IEnumerator SpawnPieces(Texture texture) {
		int piecesOnX = (int)numberOfPieces.x;
		int piecesOnY = (int)numberOfPieces.y;

		var scale = new Vector2(1f/piecesOnX, 1f/piecesOnY);

		float pieceZPosition = -1;
		for (int i = 0; i < piecesOnX; i++) {
			for (int j = 0; j < piecesOnY; j++) {
				var id = i.ToString () + j.ToString ();

				var midPointX = sizeOfPicture.x*(scale.x*(i+0.5f)-0.5f);
				var midPointY = sizeOfPicture.y*(scale.y*(j+0.5f)-0.5f);
				var newSnapablePoint = new SnapablePoint (id, new Vector3 (midPointX, midPointY, -1), new Vector2(i*scale.x,j*scale.y));
				snapablePoints.Add(newSnapablePoint);

				var pieceObject = GameObject.Instantiate (gameboard.PiecePrefab).GetComponent<Piece>();
				pieceObject.transform.parent = pivot.transform;
				pieceObject.transform.localPosition = new Vector3 (0, 0, pieceZPosition);
				pieceObject.transform.localScale = new Vector3(sizeOfPicture.x*scale.x,sizeOfPicture.y*scale.y,1);
				pieceObject.id = id;
				pieceObject.puzzlePivot = this;

				pieceObject.PieceRenderer.material.SetTextureScale ("_MainTex", scale);
				pieceObject.PieceRenderer.material.SetTextureOffset("_MainTex", newSnapablePoint.initialPieceTextureOffset);
				pieceObject.PieceRendererList = new List<GameObject> ();
				pieceObject.PieceRendererList.Add (pieceObject.PieceRenderer.gameObject);

				pieceObject.CollectableLayerRenderer.gameObject.SetActive(false);
				pieces.Add (pieceObject);

				pieceObject.outline = PieceOutlineHelper.GenerateMeshOutline(pieceObject.gameObject);
				pieceObject.outline.SetActive (false);

				pieceZPosition += -0.1f;
			}
		}
		if (texture != null) {
			SetTextureForPiecesRenderer (texture);
		}

		SetupGoalKeypieceDictionary ();
		yield break;
	}

	public void SetTextureForPiecesRenderer(Texture texture, int index = 0) {
		foreach(var pieceObject in pieces) {
			pieceObject.PieceRendererList [index].GetComponent<MeshRenderer>().material.SetTexture ("_MainTex", texture);
		}
	}

	public int SpawnExtraPieceRenderer(Action<GameObject> modifyPieceRenderer = null) {
		foreach (var piece in pieces) {
			var newRenderer = piece.SpawnExtraRenderer ();
			if (modifyPieceRenderer != null) {
				modifyPieceRenderer (newRenderer);
			}
		}
		return pieces [0].PieceRendererList.Count ()-1;
	}

	internal void AssignToSnapablePoint(Piece piece, SnapablePoint snapablePoint) {
		snapablePoint.piece = piece;
		snapablePoint.piece.transform.localPosition = new Vector3(snapablePoint.position.x, snapablePoint.position.y, 0);
		snapablePoint.piece.Backdrop.SetActive (false);
		gameboard.CheckForWin ();
	}

	public void ScramblePiecePosition() {
		foreach (var piece in pieces) {
			var x = sizeOfPicture.x * 0.5f;
			var y = sizeOfPicture.y * 0.5f;
			var pieceScaleX = piece.transform.localScale.x*0.5f;
			var pieceScaleY = piece.transform.localScale.x*0.5f;


			var randomX = UnityEngine.Random.Range ((-x+pieceScaleX)*100,(x-pieceScaleX)*100)*0.01f;
			var randomY = UnityEngine.Random.Range ((-y+pieceScaleY)*100,(y-pieceScaleY)*100)*0.01f;
			piece.transform.localPosition = new Vector3 (randomX,randomY,piece.transform.localPosition.z);
		}
	}

	public abstract void SetPiecePosition ();
	public abstract void PieceClicked (Piece piece, Vector3 mousePosInWorld);
	public abstract void TouchReleased ();
	public abstract void CustomUpdate();
}
