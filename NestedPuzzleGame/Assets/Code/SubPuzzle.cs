using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SubPuzzle : MonoBehaviour {
	[SerializeField] private GameObject piece;
	[SerializeField] private Camera puzzleCamera;
	[SerializeField] private Texture goalTexture;
	[SerializeField] private GameObject background;
	[SerializeField] private GameObject backgroundQuad;
	[SerializeField] private SubPuzzleButton subPuzzleButton;

	private RenderTexture puzzleCameraTexture;
	
	private GameBoard gameBoard;

	private List<SnapablePoint> snapablePoints = new List<SnapablePoint>();
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
	void Start () {
		
	}

	public void Initialize(GameBoard gameBoard, string id, int layer, Vector2 sizeOfPicture) {
		this.gameBoard = gameBoard;
		subPuzzleButton.gameBoard = gameBoard;
		subPuzzleButton.subPuzzle = this;
		subPuzzleLayer = layer;
		this.sizeOfPicture = sizeOfPicture;
		this.id = id;
		nodeAsset = LevelAssetHelper.GetNodeAsset (gameBoard.nodeAssetDictionary, id);

		textureSize = new Vector2 (sizeOfPicture.x,sizeOfPicture.y)*120;
		puzzleCamera.orthographicSize = sizeOfPicture.y/2;
		backgroundQuad.transform.localScale = new Vector3(sizeOfPicture.x,sizeOfPicture.y,1);
		puzzleCameraTexture = new RenderTexture ((int)textureSize.x, (int)textureSize.y, 24, renderTextureFormat);
		puzzleCamera.targetTexture = puzzleCameraTexture;
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
		int piecesOnX = (int)nodeAsset.numberOfPieces.x;
		int piecesOnY = (int)nodeAsset.numberOfPieces.y;

		var scale = new Vector2(1f/piecesOnX, 1f/piecesOnY);

		for (int i = 0; i < piecesOnX; i++) {
			for (int j = 0; j < piecesOnY; j++) {
				var id = i.ToString () + j.ToString ();

				var midPointX = sizeOfPicture.x*(scale.x*(i+0.5f)-0.5f);
				var midPointY = sizeOfPicture.y*(scale.y*(j+0.5f)-0.5f);
				var newSnapablePoint = new SnapablePoint (id, new Vector3 (midPointX, midPointY, -1));
				snapablePoints.Add(newSnapablePoint);

				var pieceObject = GameObject.Instantiate (piece).GetComponent<Piece>();
				pieceObject.transform.parent = pivot.pivot.transform;
				pieceObject.transform.localPosition = new Vector3 (0, 0, -1);
				pieceObject.transform.localScale = new Vector3(sizeOfPicture.x*scale.x,sizeOfPicture.y*scale.y,1);
				pieceObject.id = id;
				pieceObject.gameBoard = gameBoard;

				pieceObject.GetComponent<MeshRenderer> ().material.SetTextureScale ("_MainTex",scale);
				pieceObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(i*scale.x,j*scale.y));
				pieceObject.GetComponent<MeshRenderer> ().material.SetTexture ("_MainTex", texture);

				pivot.pieces.Add (pieceObject);
			}
		}
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
			var y = Mathf.RoundToInt(i/piecesOnX);
			var space = new Vector3(x,y,0)*0.5f;
			piece.transform.localPosition = space+new Vector3 (x*piece.transform.localScale.x,y*piece.transform.localScale.y,piece.transform.localPosition.z)+offset;
			i += 1;
		}
	}

	private IEnumerator SpawnExtraPivots(GameObject pivot, int numberOfPivots) {
		// Wait 3 frames
		for (int i = 0; i < 3; i++) {
			yield return null;
		}

		for (int i = 0; i < numberOfPivots; i++) {
			SpawnExtraPivot (pivot);
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
	private void SpawnExtraPivot(GameObject pivot) {
		HideAllPuzzlePivots ();
		snapShot = new Texture2D ((int)textureSize.x, (int)textureSize.y, textureFormat, false);
		Graphics.CopyTexture (puzzleCameraTexture, snapShot);
		var puzzlePivot = new PuzzlePivot (pivot);
		puzzlePivots.Add (puzzlePivot);
		SpawnPieces (puzzlePivot, snapShot);
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
		foreach (var snapablePoint in snapablePoints) {
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

	public class SnapablePoint {
		public readonly string id;
		public readonly Vector3 position;

		public SnapablePoint(string id, Vector3 position) {
			this.id = id;
			this.position = position;
		}
	}

	public class PuzzlePivot {
		public GameObject pivot;
		public List<Piece> pieces = new List<Piece>();

		public PuzzlePivot(GameObject parent) {
			pivot = new GameObject();
			pivot.transform.parent = parent.transform;
			pivot.transform.localPosition = new Vector3(0,0,0);
		}
	}
}
