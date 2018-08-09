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
	[SerializeField] private Material outlineMaterial;

	public SubPuzzle parentSubPuzzle;
	public Action completedSubPuzzle;
	public Texture snapShot;
	public int subPuzzleLayer = 0;
	public string id;
	public LevelAsset.SubPuzzleNode nodeAsset;
	public PuzzlePivot ActivePuzzlePivot { get { return activePuzzlePivot; }}

	private RenderTexture puzzleCameraTexture;
	private GameBoard gameBoard;
	private RenderTexture puzzleTexture;
	private List<PuzzlePivot> puzzlePivots = new List<PuzzlePivot>();
	private PuzzlePivot activePuzzlePivot;
	private List<SubPuzzle> subPuzzles = new List<SubPuzzle>();

	private Vector2 textureSize;
	private TextureFormat textureFormat = TextureFormat.ARGB32;
	private RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;
	private Vector2 sizeOfPicture;
	private CollectableHelper collectableHelper;

	public void Initialize(GameBoard gameBoard, string id, int layer, Vector2 sizeOfPicture) {
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
		collectableHelper = new CollectableHelper (puzzleCameraCollectable,sizeOfPicture, textureSize, renderTextureFormat, textureFormat);
	}

	public void ActivateSubPuzzle() {
		subPuzzleButton.gameObject.SetActive (false);
	}

	public void DeactivateSubPuzzle() {
		subPuzzleButton.gameObject.SetActive (true);
	}

	public void SpawnSubPuzzle() {
		StartCoroutine (SpawnExtraPivots(background, nodeAsset.puzzlePivots.Count));
	}

	
	private void SpawnPieces(PuzzlePivot pivot, Texture texture) {
		int piecesOnX = (int)pivot.numberOfPieces.x;
		int piecesOnY = (int)pivot.numberOfPieces.y;

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

				pieceObject.outline = PieceOutlineHelper.GenerateMeshOutline(pieceObject.gameObject);
				pieceObject.outline.SetActive (false);

				pieceZPosition += -0.1f;
			}
		}
	}

	private void ArrangePiecePosition(List<Piece> pieces) {
		var offset = new Vector3 (-1.5f,-4,0);
		int i = 0;
		
		int piecesOnX = (int)activePuzzlePivot.numberOfPieces.x;
		
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
			var newPuzzlePivot = SpawnExtraPivot(pivot,nodeAsset.puzzlePivots[i].type);
			var additionalPieces = 0;
			if (Director.Instance.IsAlternativeLevel) {
				additionalPieces = 1;
			}
			newPuzzlePivot.numberOfPieces = nodeAsset.puzzlePivots[i].numberOfPieces+new Vector2(1,1)*additionalPieces;
			newPuzzlePivots.Add(newPuzzlePivot);

			if (nodeAsset.collectable.isActive && i == numberOfPivots-1) {
				collectableHelper.SpawnCollectable(newPuzzlePivot,nodeAsset.collectable.position,nodeAsset.collectable.scale);
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


			var camera = GameObject.Find("CameraPivot/Main Camera");
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

	private void TakeSnapShot(PuzzlePivot pivot) {
		snapShot = new Texture2D ((int)textureSize.x, (int)textureSize.y, textureFormat, false);
		Graphics.CopyTexture (puzzleCameraTexture, snapShot);

		if (pivot.collectableObject != null) {
			collectableHelper.TakeCollectableSnapshot ();
			pivot.collectableObject.SetActive(false);
		}
	}

	private PuzzlePivot SpawnExtraPivot(GameObject pivot, PuzzlePivotType type) {
		PuzzlePivot puzzlePivot = null;
		if (type == PuzzlePivotType.jigsaw) {
			puzzlePivot = new JigsawPuzzlePivot(pivot, sizeOfPicture, gameBoard);
		}
		if (type == PuzzlePivotType.sliding) {
			puzzlePivot = new SlidingPuzzlePivot(pivot, sizeOfPicture, gameBoard);
		}
		if (type == PuzzlePivotType.rotating) {
			puzzlePivot = new RotatingPuzzlePivot(pivot, sizeOfPicture, gameBoard);
		}

		return puzzlePivot;
	}

	private void SetupExtraPivot(PuzzlePivot puzzlePivot) {
		SetBackgroundColor (puzzlePivots.Count);

		HideAllPuzzlePivots ();
		TakeSnapShot(puzzlePivot);

		puzzlePivots.Add (puzzlePivot);
		SpawnPieces (puzzlePivot, snapShot);
		if (puzzlePivot.collectableObject != null) {
			collectableHelper.SetupCollectableLayerForPieces(puzzlePivot);
		}

		puzzlePivot.SetPiecePosition();
		activePuzzlePivot = puzzlePivot;
		activePuzzlePivot.SetDepthOfPieces ();
	}

	public void CheckForCollectable() {
		collectableHelper.CheckForCollectable (activePuzzlePivot, gameBoard.hasCollectedCollectable);
	}

	private void SetBackgroundColor(int pivotIndex) {
		if (pivotIndex == 0) {
			backgroundQuad.GetComponent<MeshRenderer> ().material.SetVector ("_v1", new Vector4 (1, 0, 0, 0));
			backgroundQuad.GetComponent<MeshRenderer> ().material.SetVector ("_v2", new Vector4 (0, 1, 0, 0));
			backgroundQuad.GetComponent<MeshRenderer> ().material.SetVector ("_v3", new Vector4 (0, 0, 1, 0));
		} else {
			backgroundQuad.GetComponent<MeshRenderer> ().material.SetVector ("_v1", new Vector4 (0, 0, 1, 0));
			backgroundQuad.GetComponent<MeshRenderer> ().material.SetVector ("_v2", new Vector4 (1, 0, 0, 0));
			backgroundQuad.GetComponent<MeshRenderer> ().material.SetVector ("_v3", new Vector4 (0, 1, 0, 0));
		}
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

	public bool CheckForWin() {
		foreach(var piece in activePuzzlePivot.pieces) {
			var snapablePoint = activePuzzlePivot.GetPointWithinRadius (piece.transform.localPosition, 0.2f);
			if (snapablePoint == null || snapablePoint.id != piece.id) {
				return false;
			}
		}
		return true;
	}

	public bool SetupNextPuzzlePivot() {
		var nextPuzzlePivot = GetNextPuzzlePivot();
		if (nextPuzzlePivot != null) {
			activePuzzlePivot = nextPuzzlePivot;
			HideAllPuzzlePivots ();
			activePuzzlePivot.pivot.SetActive (true);
			SetBackgroundColor (puzzlePivots.IndexOf(activePuzzlePivot));
			return true;
		}
		return false;
	}
}
