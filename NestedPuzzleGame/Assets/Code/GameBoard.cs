using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Opencoding.CommandHandlerSystem;

public class GameBoard : MonoBehaviour {
	[SerializeField] private GameObject gameUI;
	[SerializeField] public SubPuzzle subPuzzlePrefab;
	[SerializeField] private AnimationCurve easeInOutCurve;
	[SerializeField] private GameObject goalPicture;
	[SerializeField] private GameObject cameraPivot;
	[SerializeField] private GameObject piece;


	public GameObject PiecePrefab { get { return piece; }}
	private RenderTexture puzzleTexture;

	public SubPuzzle activeSubPuzzle;
	private GameObject goalPictureObject;
	private Vector3 startScale;
	public static float ZoomScale;
	public bool hasCollectedCollectable;

	private LevelAsset level;
	public SubPuzzle InitialSubPuzzle { get; private set; }

	private int additionalPieces;
	public Dictionary<string,List<LevelAsset.SubPuzzleNode>> nodeAssetDictionary;
	public LevelAsset levelOverride;

	void Start () {
		if (Director.Instance.IsAlternativeLevel) {
			additionalPieces = 1;
		}
		if (levelOverride == null) {
			level = Director.LevelDatabase.levels [Director.Instance.LevelIndex];
		} else {
			level = levelOverride;
		}
		
		numberOfPieces = level.numberOfPieces+new Vector2(additionalPieces,additionalPieces);
		
		ZoomScale = numberOfPieces.x;

		startScale = transform.localScale;
		nodeAssetDictionary = LevelAssetHelper.ConstructDictionary (level.subPuzzleNodes);

		StartCoroutine (SpawnInitialSubPuzzle ());
	}

	private IEnumerator SpawnInitialSubPuzzle() {
		cameraPivot.transform.localPosition = new Vector3 (1000,1000,0);
		var scale = 4;
		var aspectRatio = (float)level.picture.height/level.picture.width;
		var pictureSize = new Vector2(scale, scale*aspectRatio);

		goalPictureObject = GameObject.Instantiate (goalPicture);
		goalPictureObject.transform.parent = transform;

		goalPictureObject.transform.localScale = new Vector3 (pictureSize.x,pictureSize.y,goalPictureObject.transform.localScale.z);
		goalPictureObject.transform.localPosition = new Vector3 (0,0,-1);
		goalPictureObject.GetComponent<MeshRenderer> ().material.SetTexture ("_MainTex", level.picture);
		if (Director.Instance.IsAlternativeLevel) {
			goalPictureObject.GetComponent<MeshRenderer> ().material.SetVector ("_v1", new Vector4 (0, 0, 1, 0));
			goalPictureObject.GetComponent<MeshRenderer> ().material.SetVector ("_v2", new Vector4 (1, 0, 0, 0));
			goalPictureObject.GetComponent<MeshRenderer> ().material.SetVector ("_v3", new Vector4 (0, 1, 0, 0));
		}

		InitialSubPuzzle = GameObject.Instantiate (subPuzzlePrefab).GetComponent<SubPuzzle>();
		InitialSubPuzzle.transform.parent = transform;
		InitialSubPuzzle.Initialize(this, "0" , 0, pictureSize);
		InitialSubPuzzle.transform.parent = transform;
		InitialSubPuzzle.transform.localPosition = new Vector3 (0, 0, 0);
		InitialSubPuzzle.SpawnSubPuzzle ();


		// Wait 3 frames
		for (int i = 0; i < 3; i++) {
			yield return null;
		}

		SetActiveSubPuzzle (InitialSubPuzzle);
		goalPictureObject.SetActive (false);

		if (level.isMasterPuzzle) {
			yield return null;
			activeSubPuzzle.SetupMasterPuzzle();
		}

		cameraPivot.transform.localPosition = new Vector3 (0,0,0);

		if (level.isMasterPuzzle) {
			yield return null;

			LoadInitialSubPuzzlePiecePositions ();
		}
	}

	public void SetActiveSubPuzzle(SubPuzzle newActiveSubPuzzle) {
		activeSubPuzzle = newActiveSubPuzzle;
		newActiveSubPuzzle.ActivateSubPuzzle();
	}

	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			var mousePosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mousePosInWorld.z = -10;
			var hits = Physics.RaycastAll(mousePosInWorld, Vector3.forward, 100);
			var hitsList = new List<RaycastHit>(hits);
			hitsList = hitsList.OrderBy(x => x.transform.position.z).ToList();
			foreach (var hit in hitsList) {
				if (hit.collider.GetComponent<SubPuzzleButton>() != null) {
					var subPuzzleButton = hit.collider.GetComponent<SubPuzzleButton>();
					subPuzzleButton.Click();
				}
			}

			foreach (var hit in hitsList) {
				if (!LevelView.IsCollectableLayerOn) {
					var piece = hit.collider.GetComponent<Piece>();
					if (piece != null) {
						piece.puzzlePivot.PieceClicked (piece, mousePosInWorld);
						break;
					}
				}

				if (hit.collider.name.Contains("Star")) {
					hasCollectedCollectable = true;
					hit.collider.gameObject.SetActive(false);
					
					// Save
					string id;
					var levelSave = Director.SaveData.GetCurrentLevelSaveDataEntry(out id);
					if (levelSave.completed && hasCollectedCollectable && !levelSave.gotCollectable) {
						Director.Instance.levelExitState |= LevelExitState.GotCollectable;

						levelSave.gotCollectable = true;
						Director.SaveData.SaveLevelSaveDataEntry (levelSave,id);
					}
				}
			}
		}

		if (activeSubPuzzle != null && activeSubPuzzle.ActivePuzzlePivot != null) {
			activeSubPuzzle.ActivePuzzlePivot.CustomUpdate ();
		}

		if (Input.GetMouseButtonUp (0)) {
			activeSubPuzzle.ActivePuzzlePivot.TouchReleased ();
		}
	}

	public void CheckForWin() {
		if (activeSubPuzzle == null || activeSubPuzzle.ActivePuzzlePivot == null) return;
		var isDone = activeSubPuzzle.ActivePuzzlePivot.CheckForWin ();
		if (isDone) {
			var hasMorePuzzles = activeSubPuzzle.SetupNextPuzzlePivot ();
			if (!hasMorePuzzles) {
				activeSubPuzzle.WasDone ();
				if (activeSubPuzzle.parentSubPuzzle != null) {
					transform.localScale *= 1/GameBoard.ZoomScale;
					var camera = GameObject.Find("CameraPivot/Main Camera");
					var newPos = camera.transform.position-activeSubPuzzle.parentSubPuzzle.transform.position;
					transform.localScale *= GameBoard.ZoomScale;

					ZoomOut (transform.position+newPos);
				} else {
					goalPictureObject.SetActive (true);
					string id;
					var levelSave = Director.SaveData.GetCurrentLevelSaveDataEntry(out id);
					if (!levelSave.completed) {
						Director.Instance.levelExitState |= LevelExitState.LevelCompleted;
					}
					levelSave.completed = true;
					if (hasCollectedCollectable && !levelSave.gotCollectable){
						Director.Instance.levelExitState |= LevelExitState.GotCollectable;
						levelSave.gotCollectable = true;
					}
					Director.SaveData.SaveLevelSaveDataEntry (levelSave, id);

					Director.TransitionManager.PlayTransition (() => { SceneManager.LoadScene ("LevelSelectScene");}, 0.2f, Director.TransitionManager.FadeToBlack(),  Director.TransitionManager.FadeOut());
				}
				activeSubPuzzle = activeSubPuzzle.parentSubPuzzle;
			}
		}
	}

	public void ZoomToLayer(int layerNumber) {
		transform.localScale = startScale*Mathf.Pow(ZoomScale,layerNumber);
	}
	public static Vector2 numberOfPieces;

	public IEnumerator ZoomOut() {
		yield return new WaitForSeconds (0.5f);
		StartCoroutine(AnimateTo (gameObject, new Vector3 (0,-1f)));
		StartCoroutine(ScaleTo (gameObject, gameObject.transform.localScale*(1/ZoomScale)));
	}

	public void ZoomOut(Vector3 pos) {
		StartCoroutine(AnimateTo (gameObject, new Vector3(pos.x,pos.y,gameObject.transform.position.z)));
		StartCoroutine(ScaleTo (gameObject, gameObject.transform.localScale*1/ZoomScale));
	}

	public void ZoomIn(Vector3 pos) {
		StartCoroutine(AnimateTo (gameObject, new Vector3(pos.x,pos.y,gameObject.transform.position.z)));
		StartCoroutine(ScaleTo (gameObject, gameObject.transform.localScale*ZoomScale));
	}

	private IEnumerator AnimateTo(GameObject gameObject, Vector3 endPosition) {
		var startPosition = gameObject.transform.position;
		float time = 0;
		while (time < 1) {
			time += 0.01f;
			var t = easeInOutCurve.Evaluate (time);
			gameObject.transform.position = (startPosition * (1 - t)) + endPosition * t;
			yield return null;
		}
	}

	private IEnumerator ScaleTo(GameObject gameObject, Vector3 endScale) {
		var startScale = gameObject.transform.localScale;
		startScale.z = 1;
		endScale.z = 1;
		float time = 0;
		while (time < 1) {
			time += 0.01f;
			var t = easeInOutCurve.Evaluate (time);
			gameObject.transform.localScale = (startScale * (1 - t)) + endScale * t;
			yield return null;
		}
	}

	public void MoveEntireScene(Vector3 newPosition) {
		var addPositon = transform.position - newPosition;
		cameraPivot.transform.position = newPosition;
		transform.position += addPositon;
		gameUI.transform.position = newPosition;
	}

	private void SaveInitialSubPuzzlePiecePositions() {
		Dictionary<string, LevelSaveData.PieceStateData> piecePositions = new Dictionary<string, LevelSaveData.PieceStateData> ();
		foreach (var piece in InitialSubPuzzle.ActivePuzzlePivot.pieces) {
			var pieceStateData = new LevelSaveData.PieceStateData ();
			pieceStateData.localPosition = piece.transform.localPosition;
			piecePositions.Add (piece.id, pieceStateData);
		}

		string id;
		var levelSave = Director.SaveData.GetCurrentLevelSaveDataEntry(out id);
		levelSave.SetPiecePositions (piecePositions);
		Director.SaveData.SaveLevelSaveDataEntry (levelSave, id);
	}

	private void LoadInitialSubPuzzlePiecePositions() {
		string id;
		var levelSave = Director.SaveData.GetCurrentLevelSaveDataEntry(out id);
		var piecePositions = levelSave.GetPiecePositions ();
		if (piecePositions == null) return;
		foreach (var piece in InitialSubPuzzle.ActivePuzzlePivot.pieces) {
			if (!piece.gameObject.activeSelf) continue;
			var pieceStateDate = UnityEngine.JsonUtility.FromJson<LevelSaveData.PieceStateData> (MiniJSON.Json.Serialize (piecePositions [piece.id]));
			piece.transform.localPosition = pieceStateDate.localPosition;
			((JigsawPuzzlePivot)(InitialSubPuzzle.ActivePuzzlePivot)).AssignToClosestSnapablePoint (piece);
		}
	}

	private void OnDestroy() {
		if (level.isMasterPuzzle) {
			SaveInitialSubPuzzlePiecePositions ();

			var key = Director.Instance.WorldIndex.ToString () + "_0";
			var levelTexture = InitialSubPuzzle.TakeSnapShot ();
			if (Director.Instance.MasterPuzzleImages.ContainsKey (key)) {
				Director.Instance.MasterPuzzleImages [key] = levelTexture;
			} else {
				Director.Instance.MasterPuzzleImages.Add (key, levelTexture);
			}
		}
	}

}

[Flags]
public enum LevelExitState {
	None 			= 0,
	LevelCompleted 	= 1 << 0,
	GotCollectable	= 2 << 1
}
