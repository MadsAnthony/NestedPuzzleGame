using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class SubPuzzle : MonoBehaviour {
	[SerializeField] private GameObject piece;
	[SerializeField] private Camera puzzleCamera;
	[SerializeField] private Camera puzzleCameraCollectable;
	[SerializeField] private GameObject background;
	[SerializeField] private GameObject backgroundQuad;
	[SerializeField] private SubPuzzleButton subPuzzleButton;
	[SerializeField] private GameObject collectable;

	private RenderTexture puzzleCameraTexture;
	private RenderTexture puzzleCameraCollectableTexture;
	
	private GameBoard gameBoard;

	private RenderTexture puzzleTexture;

	private List<PuzzlePivot> puzzlePivots = new List<PuzzlePivot>();
	private PuzzlePivot activePuzzlePivot;
	
	private List<SubPuzzle> subPuzzles = new List<SubPuzzle>();
	public SubPuzzle parentSubPuzzle;

	public int subPuzzleLayer = 0;
	public string id;
	public LevelAsset.SubPuzzleNode nodeAsset;
	private Vector2 textureSize;
	private TextureFormat textureFormat = TextureFormat.ARGB32;
	private RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;
	private Vector2 sizeOfPicture;
	private int additionalPieces;
	
	public void Initialize(GameBoard gameBoard, string id, int layer, Vector2 sizeOfPicture) {
		if (Director.Instance.IsAlternativeLevel) {
			additionalPieces = 1;
		}
		
		this.gameBoard = gameBoard;
		subPuzzleButton.gameBoard = gameBoard;
		subPuzzleButton.subPuzzle = this;
		subPuzzleLayer = layer;
		this.sizeOfPicture = sizeOfPicture;
		this.id = id;
		nodeAsset = LevelAssetHelper.GetNodeAsset (gameBoard.nodeAssetDictionary, id);

		textureSize = new Vector2 (sizeOfPicture.x,sizeOfPicture.y)*120;
		backgroundQuad.transform.localScale = new Vector3(sizeOfPicture.x,sizeOfPicture.y,1);
		puzzleCameraTexture = new RenderTexture ((int)textureSize.x, (int)textureSize.y, 24, renderTextureFormat);
		puzzleCamera.targetTexture = puzzleCameraTexture;
		puzzleCamera.orthographicSize = sizeOfPicture.y/2;
		puzzleCameraCollectableTexture = new RenderTexture ((int)textureSize.x, (int)textureSize.y, 24, renderTextureFormat);
		puzzleCameraCollectable.targetTexture = puzzleCameraCollectableTexture;
		puzzleCameraCollectable.orthographicSize = sizeOfPicture.y/2;
	}

	public void ActivateSubPuzzle() {
		subPuzzleButton.gameObject.SetActive (false);
	}

	public void DeactivateSubPuzzle() {
		subPuzzleButton.gameObject.SetActive (true);
	}

	public void SpawnSubPuzzle() {
		StartCoroutine (SpawnExtraPivots(background, nodeAsset.numberOfPivots));
	}

	
	private void SpawnPieces(PuzzlePivot pivot, Texture texture) {
		int piecesOnX = (int)nodeAsset.numberOfPieces.x+additionalPieces;
		int piecesOnY = (int)nodeAsset.numberOfPieces.y+additionalPieces;

		var scale = new Vector2(1f/piecesOnX, 1f/piecesOnY);

		float pieceZPosition = -1;
		for (int i = 0; i < piecesOnX; i++) {
			for (int j = 0; j < piecesOnY; j++) {
				var id = i.ToString () + j.ToString ();

				var midPointX = sizeOfPicture.x*(scale.x*(i+0.5f)-0.5f);
				var midPointY = sizeOfPicture.y*(scale.y*(j+0.5f)-0.5f);
				var newSnapablePoint = new SnapablePoint (id, new Vector3 (midPointX, midPointY, -1));
				pivot.snapablePoints.Add(newSnapablePoint);

				var pieceObject = GameObject.Instantiate (piece).GetComponent<Piece>();
				pieceObject.transform.parent = pivot.pivot.transform;
				pieceObject.transform.localPosition = new Vector3 (0, 0, pieceZPosition);
				pieceObject.transform.localScale = new Vector3(sizeOfPicture.x*scale.x,sizeOfPicture.y*scale.y,1);
				pieceObject.id = id;
				
				pieceObject.PieceRenderer.material.SetTextureScale ("_MainTex",scale);
				pieceObject.PieceRenderer.material.SetTextureOffset("_MainTex", new Vector2(i*scale.x,j*scale.y));
				pieceObject.PieceRenderer.material.SetTexture ("_MainTex", texture);

				pieceObject.CollectableLayerRenderer.gameObject.SetActive(false);
				pivot.pieces.Add (pieceObject);
				
				pieceZPosition += -0.1f;
			}
		}
	}

	private void SetupCollectableLayerForPieces(PuzzlePivot pivot){
		int piecesOnX = (int)nodeAsset.numberOfPieces.x+additionalPieces;
		int piecesOnY = (int)nodeAsset.numberOfPieces.y+additionalPieces;

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
			shuffledPieces[i].transform.localPosition = snapablePoint.position;
			snapablePoint.piece = shuffledPieces[i];
			i++;
		}
		
		var importantPieces = GetImportantPieces(pivot);
		collectableKeyPieceDictionary = SetupKeyPieceDictionary(pivot, importantPieces);
		
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
	
	private void ScramblePiecePosition(List<Piece> pieces) {
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

	private void ArrangePiecePosition(List<Piece> pieces) {
		var offset = new Vector3 (-1.5f,-4,0);
		int i = 0;
		
		int piecesOnX = (int)GameBoard.numberOfPieces.x;
		
		foreach (var piece in pieces) {
			var x = i%piecesOnX;
			var y = Mathf.FloorToInt((float)i/piecesOnX);
			var space = new Vector3(x,y,0)*0.5f;
			piece.transform.localPosition = space+new Vector3 (x*piece.transform.localScale.x,y*piece.transform.localScale.y,piece.transform.localPosition.z)+offset;
			i += 1;
		}
	}

	public void SetupMasterPuzzle() {
		foreach (var piece in activePuzzlePivot.pieces) {
			piece.gameObject.SetActive(false);
		}
		
		var amountOfMasterPieces = Director.GetAmountOfMasterPieces();
		foreach (var piece in activePuzzlePivot.pieces) {
			if (amountOfMasterPieces > 0) {
				piece.gameObject.SetActive(true);
			}
			amountOfMasterPieces--;
		}
	}
	
	private IEnumerator SpawnExtraPivots(GameObject pivot, int numberOfPivots) {
		var newPuzzlePivots = new List<PuzzlePivot>();
		for (int i = 0; i < numberOfPivots; i++) {
			var newPuzzlePivot = SpawnExtraPivot(pivot);
			newPuzzlePivots.Add(newPuzzlePivot);

			if (nodeAsset.collectable.isActive && i == numberOfPivots-1) {
				SpawnCollectable(newPuzzlePivot);
			}
		}
		
		// Wait 3 frames
		for (int i = 0; i < 3; i++) {
			yield return null;
		}

		foreach (var newPuzzlePivot in newPuzzlePivots) {
			SetupExtraPivot (newPuzzlePivot);
			yield return null;
		}

		var childrenNodes = LevelAssetHelper.GetChildrenNodes (gameBoard.nodeAssetDictionary, id);
		if (childrenNodes.Count>0) {
			var newSubPuzzleLayer = subPuzzleLayer+1;
			ArrangePiecePosition (activePuzzlePivot.pieces);
			gameBoard.ZoomToLayer (newSubPuzzleLayer);

			int n = 0;
			foreach (var childrenNode in childrenNodes) {
				var piece = activePuzzlePivot.pieces[n];
				var scale = 4;
				var aspectRatio = piece.transform.localScale.y/piece.transform.localScale.x;
				var pictureSize = new Vector2(scale, scale*aspectRatio);
				
				var subPuzzle = GameObject.Instantiate (gameBoard.subPuzzlePrefab).GetComponent<SubPuzzle> ();
				subPuzzle.Initialize (gameBoard, childrenNode.id, newSubPuzzleLayer, pictureSize);
				subPuzzle.transform.parent = piece.transform.parent;
				subPuzzle.transform.localPosition = piece.transform.localPosition;
				subPuzzle.parentSubPuzzle = this;
				subPuzzle.SpawnSubPuzzle ();
				subPuzzle.ActivateSubPuzzle ();
				subPuzzle.completedSubPuzzle += () => {
					piece.gameObject.SetActive(true);

					if (AlllSubPuzzlesComplete()) {
						backgroundQuad.GetComponent<MeshRenderer> ().enabled = true;
					}
					SetActiveSubPuzzle (subPuzzle);
				};
				subPuzzles.Add (subPuzzle);
				n++;
			}

			// Wait 3 frames
			for (int i = 0; i < 3; i++) {
				yield return null;
			}
			for (int i = 0; i < n; i++) {
				activePuzzlePivot.pieces[i].gameObject.SetActive (false);
			}
			backgroundQuad.GetComponent<MeshRenderer> ().enabled = false;


			var camera = GameObject.Find("Main Camera");
			var newPos = camera.transform.position-subPuzzles [0].gameObject.transform.position;
			newPos.z = 0;
			gameBoard.transform.position += newPos;
			SetActiveSubPuzzle (subPuzzles [0], false);
		}
	}

	private bool AlllSubPuzzlesComplete() {
		foreach (var subPuzzle in subPuzzles) {
			if (subPuzzle.gameObject.activeSelf) {
				return false;
			}
		}
		return true;
	}

	public Action completedSubPuzzle;
	public void WasDone() {
		gameObject.SetActive (false);
		if (completedSubPuzzle != null) {
			completedSubPuzzle ();
		}
	}
	public void DeactivateAllSubPuzzles() {
		foreach (var subPuzzle in subPuzzles) {
			subPuzzle.DeactivateSubPuzzle ();
		}
	}

	private void SetActiveSubPuzzle(SubPuzzle newActiveSubPuzzle, bool deactivateOthers = true) {
		if (deactivateOthers) {
			foreach (var subPuzzle in subPuzzles) {
				subPuzzle.DeactivateSubPuzzle ();
			}
		}
		gameBoard.activeSubPuzzle = newActiveSubPuzzle;
		newActiveSubPuzzle.ActivateSubPuzzle();
	}

	public Texture snapShot;
	public Texture snapShotCollectableLayer;
	private void TakeSnapShot(PuzzlePivot pivot) {
		snapShot = new Texture2D ((int)textureSize.x, (int)textureSize.y, textureFormat, false);
		Graphics.CopyTexture (puzzleCameraTexture, snapShot);

		if (pivot.collectableObject != null) {
			snapShotCollectableLayer = new Texture2D((int) textureSize.x, (int) textureSize.y, textureFormat, false);
			Graphics.CopyTexture(puzzleCameraCollectableTexture, snapShotCollectableLayer);
			pivot.collectableObject.SetActive(false);
		}
	}

	private PuzzlePivot SpawnExtraPivot(GameObject pivot) {
		var puzzlePivot = new PuzzlePivot(pivot);
		return puzzlePivot;
	}

	private void SpawnCollectable(PuzzlePivot puzzlePivot) {
		puzzlePivot.collectableObject = GameObject.Instantiate(collectable);
		puzzlePivot.collectableObject.transform.parent = puzzlePivot.pivot.transform;
		puzzlePivot.collectableObject.transform.localPosition = new Vector3(nodeAsset.collectable.position.x,nodeAsset.collectable.position.y,-2);
		puzzlePivot.collectableObject.transform.localScale = new Vector3(nodeAsset.collectable.scale.x,nodeAsset.collectable.scale.y,1);
	}

	private void SetupExtraPivot(PuzzlePivot puzzlePivot) {
		HideAllPuzzlePivots ();
		TakeSnapShot(puzzlePivot);
		
		puzzlePivots.Add (puzzlePivot);
		SpawnPieces (puzzlePivot, snapShot);
		if (puzzlePivot.collectableObject != null) {
			SetupCollectableLayerForPieces(puzzlePivot);
		}

		ScramblePiecePosition (puzzlePivot.pieces);
		activePuzzlePivot = puzzlePivot;
	}

	private PuzzlePivot GetNextPuzzlePivot() {
		PuzzlePivot result = null;
		foreach (var puzzlePivot in puzzlePivots) {
			if (puzzlePivot == activePuzzlePivot) {
				return result;
			}
			result = puzzlePivot;
		}
		return result;
	}

	private void HideAllPuzzlePivots() {
		foreach (var puzzlePivot in puzzlePivots) {
			puzzlePivot.pivot.SetActive (false);
		}
	}

	public SnapablePoint GetPointWithinRadius(Vector3 point, float radius) {
		foreach (var snapablePoint in activePuzzlePivot.snapablePoints) {
			point.z = snapablePoint.position.z;
			var dist = snapablePoint.position - point;
			if (dist.magnitude < radius) {
				return snapablePoint;
			}
		}
		return null;
	}

	public bool CheckForWin() {
		foreach(var piece in activePuzzlePivot.pieces) {
			var snapablePoint = GetPointWithinRadius (piece.transform.localPosition, 0.2f);
			if (snapablePoint == null || snapablePoint.id != piece.id) {
				return false;
			}
		}
		return true;
	}

	public void CheckForCollectable() {
		if (!LevelView.IsCollectableLayerOn || gameBoard.hasCollectedCollectable || activePuzzlePivot.collectableObject == null) return;
		if (activePuzzlePivot.collectableObject.activeSelf) return;
		var collectableSnapablePoint = GetSnapablePointWithPieceId(activePuzzlePivot, collectableKeyPieceDictionary.keyPiece.id);

		if (collectableSnapablePoint != null) {
			foreach (var piecePairValue in collectableKeyPieceDictionary.pieceDictionary) {
				var snapablePoint = GetSnapablePointFromRelativePosition(collectableSnapablePoint, piecePairValue.Key);
				if (snapablePoint == null) return;
				var snapablePiece = snapablePoint.piece;
				if (snapablePiece == null || snapablePiece.id != piecePairValue.Value) return;
			}

			var offset = collectableKeyPieceDictionary.originalPosition-collectableSnapablePoint.piece.transform.localPosition;
			offset.z = 0;
			activePuzzlePivot.collectableObject.transform.localPosition -= offset;
			activePuzzlePivot.collectableObject.SetActive(true);

			foreach(var piece in activePuzzlePivot.pieces) {
				piece.CollectableLayerRenderer.gameObject.SetActive(false);
			}
		}
	}

	public bool SetupNextPuzzlePivot() {
		var nextPuzzlePivot = GetNextPuzzlePivot();
		if (nextPuzzlePivot != null) {
			activePuzzlePivot = nextPuzzlePivot;
			HideAllPuzzlePivots ();
			activePuzzlePivot.pivot.SetActive (true);
			return true;
		}
		return false;
	}

	private KeyPieceDictionary collectableKeyPieceDictionary;

	private KeyPieceDictionary SetupKeyPieceDictionary(PuzzlePivot pivot, List<Piece> pieces){
		var keyPiece = pieces[0];
		pieces.Remove(keyPiece);
		var keyPieceDictionary = new KeyPieceDictionary(keyPiece);
		
		var keyPieceSnapablePoint = GetSnapablePointWithPieceId(pivot, keyPiece.id);
		var keyPieceId = pivot.snapablePoints.IndexOf(keyPieceSnapablePoint);
		
		foreach (var piece in pieces) {
			var snapablePoint = GetSnapablePointWithPieceId(pivot, piece.id);

			var relativePos = GetRelativePosition(keyPieceId, pivot.snapablePoints.IndexOf(snapablePoint));
			keyPieceDictionary.pieceDictionary.Add(relativePos,piece.id);
		}

		return keyPieceDictionary;
	}

	private SnapablePoint GetSnapablePointFromRelativePosition(SnapablePoint currentSnapablePoint, Vector2 relativePosition) {
		var index = activePuzzlePivot.snapablePoints.IndexOf(currentSnapablePoint);
		int piecesOnY = (int)nodeAsset.numberOfPieces.y+additionalPieces;

		var newX = index + relativePosition.x*piecesOnY;
		var newY = index + relativePosition.y;
		
		if (newX < 0 || newX >= activePuzzlePivot.snapablePoints.Count) return null;
		if (newY < 0 || newY >= activePuzzlePivot.snapablePoints.Count) return null;
		
		if (newY < 0 || newY >= activePuzzlePivot.snapablePoints.Count) return null;
		
		if (Mathf.FloorToInt(newY/piecesOnY) != Mathf.FloorToInt((float)index/piecesOnY)) return null;

		var newIndex = (int)(newX + relativePosition.y);
		
		return activePuzzlePivot.snapablePoints[newIndex];
	}
	
	private Vector2 GetRelativePosition(int currentIndex, int otherIndex) {
		int piecesOnY = (int)nodeAsset.numberOfPieces.y+additionalPieces;

		var currentX = Mathf.FloorToInt((float)currentIndex/piecesOnY);
		var currentY = currentIndex % piecesOnY;
		
		var otherX = Mathf.FloorToInt((float)otherIndex/piecesOnY);
		var otherY = otherIndex % piecesOnY;
		
		return new Vector2(otherX-currentX, otherY-currentY);
	}
	
	private SnapablePoint GetSnapablePointWithPieceId(PuzzlePivot pivot, string id) {
		foreach (var snapablePoint in pivot.snapablePoints) {
			if (snapablePoint.piece != null && snapablePoint.piece.id == id) {
				return snapablePoint;
			}
		}
		
		return null;
	}
	
	public SnapablePoint GetSnapablePointFromDirection(SnapablePoint currentSnapablePoint, Direction direction) {
		var index = activePuzzlePivot.snapablePoints.IndexOf(currentSnapablePoint);
		int piecesOnY = (int)nodeAsset.numberOfPieces.y+additionalPieces;
		
		if (direction == Direction.Right) {
			var newIndex = index + piecesOnY;
			if (newIndex < 0 || newIndex >= activePuzzlePivot.snapablePoints.Count) return null;
			return activePuzzlePivot.snapablePoints[newIndex];
		}
		if (direction == Direction.Left) {
			var newIndex = index - piecesOnY;
			if (newIndex < 0 || newIndex >= activePuzzlePivot.snapablePoints.Count) return null;
			return activePuzzlePivot.snapablePoints[newIndex];
		}
		if (direction == Direction.Up) {
			var newIndex = index + 1;
			if (newIndex%piecesOnY == 0) return null;
			if (newIndex < 0 || newIndex >= activePuzzlePivot.snapablePoints.Count) return null;
			return activePuzzlePivot.snapablePoints[newIndex];
		}
		if (direction == Direction.Down) {
			var newIndex = index - 1;
			if (index%piecesOnY == 0) return null;
			if (newIndex < 0 || newIndex >= activePuzzlePivot.snapablePoints.Count) return null;
			return activePuzzlePivot.snapablePoints[newIndex];
		}
		return null;
	}
	
	public class SnapablePoint {
		public readonly string id;
		public readonly Vector3 position;
		public Piece piece;

		public SnapablePoint(string id, Vector3 position) {
			this.id = id;
			this.position = position;
		}
	}

	public class PuzzlePivot {
		public GameObject pivot;
		public List<Piece> pieces = new List<Piece>();
		public List<SnapablePoint> snapablePoints = new List<SnapablePoint>();
		public GameObject collectableObject;

		public PuzzlePivot(GameObject parent) {
			pivot = new GameObject();
			pivot.transform.parent = parent.transform;
			pivot.transform.localPosition = new Vector3(0,0,0);
		}
	}

	public class KeyPieceDictionary {
		public readonly Piece keyPiece;
		public readonly Vector3 originalPosition;
		public Dictionary<Vector2, string> pieceDictionary = new Dictionary<Vector2, string>();
		
		public KeyPieceDictionary(Piece keyPiece) {
			this.keyPiece = keyPiece;
			originalPosition = keyPiece.transform.localPosition;
		}
	}
}

public enum Direction
{
	Up,
	Right,
	Down,
	Left
}