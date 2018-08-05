using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CollectableHelper {

	private GameObject collectablePrefab;
	private Texture snapShotCollectableLayer;
	private Camera collectableCamera;
	private RenderTexture puzzleCameraCollectableTexture;
	private Vector2 textureSize;
	private TextureFormat textureFormat;
	private KeyPieceDictionary collectableKeyPieceDictionary;

	public CollectableHelper(Camera collectableCamera, Vector2 sizeOfPicture, Vector2 textureSize, RenderTextureFormat renderTextureFormat, TextureFormat textureFormat) {
		collectablePrefab = (GameObject)Resources.Load ("Star");
		this.collectableCamera = collectableCamera;
		this.textureSize = textureSize;
		this.textureFormat = textureFormat;

		puzzleCameraCollectableTexture = new RenderTexture ((int)textureSize.x, (int)textureSize.y, 24, renderTextureFormat);
		collectableCamera.targetTexture = puzzleCameraCollectableTexture;
		collectableCamera.orthographicSize = sizeOfPicture.y/2;
	}

	public void SpawnCollectable(PuzzlePivot puzzlePivot, Vector2 position, Vector2 scale) {
		puzzlePivot.collectableObject = GameObject.Instantiate(collectablePrefab);
		puzzlePivot.collectableObject.transform.parent = puzzlePivot.pivot.transform;
		puzzlePivot.collectableObject.transform.localPosition = new Vector3(position.x,position.y,-2);
		puzzlePivot.collectableObject.transform.localScale = new Vector3(scale.x,scale.y,1);
	}

	public void TakeCollectableSnapshot() {
		snapShotCollectableLayer = new Texture2D((int) textureSize.x, (int) textureSize.y, textureFormat, false);
		Graphics.CopyTexture(puzzleCameraCollectableTexture, snapShotCollectableLayer);
	}

	public void CheckForCollectable(PuzzlePivot puzzlePivot, bool hasCollectedCollectable) {
		if (!LevelView.IsCollectableLayerOn || hasCollectedCollectable || puzzlePivot.collectableObject == null) return;
		if (puzzlePivot.collectableObject.activeSelf) return;
		var collectableSnapablePoint = SnapablePoint.GetSnapablePointWithPieceId(puzzlePivot, collectableKeyPieceDictionary.keyPiece.id);

		if (collectableSnapablePoint != null) {
			foreach (var piecePairValue in collectableKeyPieceDictionary.pieceDictionary) {
				var snapablePoint = SnapablePoint.GetSnapablePointFromRelativePosition(puzzlePivot, collectableSnapablePoint, piecePairValue.Key);
				if (snapablePoint == null) return;
				var snapablePiece = snapablePoint.piece;
				if (snapablePiece == null || snapablePiece.id != piecePairValue.Value) return;
			}

			var offset = collectableKeyPieceDictionary.originalPosition-collectableSnapablePoint.piece.transform.localPosition;
			offset.z = 0;
			puzzlePivot.collectableObject.transform.localPosition -= offset;
			puzzlePivot.collectableObject.SetActive(true);

			foreach(var piece in puzzlePivot.pieces) {
				piece.CollectableLayerRenderer.gameObject.SetActive(false);
			}
		}
	}

	public void SetupCollectableLayerForPieces(PuzzlePivot pivot){
		int piecesOnX = (int)pivot.numberOfPieces.x;
		int piecesOnY = (int)pivot.numberOfPieces.y;

		var scale = new Vector2(1f/piecesOnX, 1f/piecesOnY);

		var shuffledPieces = pivot.pieces.OrderBy( x => UnityEngine.Random.Range(0, pivot.pieces.Count) ).ToList( );
		int i = 0;
		foreach (var piece in shuffledPieces) {
			var x = Mathf.FloorToInt((float)i/piecesOnY);
			var y = i%piecesOnY;
			piece.CollectableLayerRenderer.gameObject.SetActive(true);
			piece.CollectableLayerRenderer.material.SetTextureScale ("_MainTex",scale);
			piece.CollectableLayerRenderer.material.SetTextureOffset("_MainTex", new Vector2(x*scale.x,y*scale.y));
			piece.CollectableLayerRenderer.material.SetTexture ("_MainTex", snapShotCollectableLayer);
			i++;
		}

		i = 0;
		foreach (var snapablePoint in pivot.snapablePoints) {
			shuffledPieces[i].transform.localPosition = new Vector3(snapablePoint.position.x,snapablePoint.position.y,shuffledPieces[i].transform.localPosition.z);
			snapablePoint.piece = shuffledPieces[i];
			i++;
		}

		var importantPieces = GetImportantPieces(pivot);
		collectableKeyPieceDictionary = KeyPieceDictionary.SetupKeyPieceDictionary(pivot, importantPieces);

		foreach (var snapablePoint in pivot.snapablePoints) {
			snapablePoint.piece = null;
		}
	}

	private List<Piece> GetImportantPieces(PuzzlePivot pivot) {
		var result = new List<Piece>();
		var collectablePosition = new Vector2(pivot.collectableObject.transform.position.x-pivot.collectableObject.transform.localScale.x/2f,
			pivot.collectableObject.transform.position.y-pivot.collectableObject.transform.localScale.y/2f);
		var collectableScale = new Vector2(pivot.collectableObject.transform.localScale.x, pivot.collectableObject.transform.localScale.y);
		var collectableRect = new Rect(collectablePosition, collectableScale);

		foreach (var snapablePoint in pivot.snapablePoints) {
			var piecePosition = new Vector2(snapablePoint.piece.transform.position.x-snapablePoint.piece.transform.localScale.x/2f,
				snapablePoint.piece.transform.position.y-snapablePoint.piece.transform.localScale.y/2f);
			var pieceScale = new Vector2(snapablePoint.piece.transform.localScale.x, snapablePoint.piece.transform.localScale.y);
			var pieceRect = new Rect(piecePosition, pieceScale);

			if (pieceRect.Overlaps(collectableRect)) {
				result.Add(snapablePoint.piece);
			}
		}
		return result;
	}
}
