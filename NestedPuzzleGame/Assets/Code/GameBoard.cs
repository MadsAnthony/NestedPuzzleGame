using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Opencoding.CommandHandlerSystem;

public class GameBoard : MonoBehaviour {
	[SerializeField] public SubPuzzle subPuzzlePrefab;
	[SerializeField] private AnimationCurve easeInOutCurve;
	[SerializeField] private GameObject goalPicture;

	public Piece draggablePiece;
	public Vector3 draggablePieceOffset;

	private RenderTexture puzzleTexture;

	public SubPuzzle activeSubPuzzle;
	private GameObject goalPictureObject;
	private Vector3 startScale;
	public static float ZoomScale;
	public bool hasCollectedCollectable;

	private LevelAsset level;

	private int additionalPieces;
	public Dictionary<string,List<LevelAsset.SubPuzzleNode>> nodeAssetDictionary;
	void Start () {
		if (Director.Instance.IsAlternativeLevel) {
			additionalPieces = 1;
		}
		level = Director.LevelDatabase.levels [Director.Instance.LevelIndex];
		
		numberOfPieces = level.numberOfPieces+new Vector2(additionalPieces,additionalPieces);
		
		ZoomScale = numberOfPieces.x;

		startScale = transform.localScale;
		nodeAssetDictionary = LevelAssetHelper.ConstructDictionary (level.subPuzzleNodes);

		StartCoroutine (SpawnInitialSubPuzzle ());
	}

	private IEnumerator SpawnInitialSubPuzzle() {
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

		var subPuzzle = GameObject.Instantiate (subPuzzlePrefab).GetComponent<SubPuzzle>();
		subPuzzle.transform.parent = transform;
		subPuzzle.Initialize(this, "0" , 0, pictureSize);
		subPuzzle.transform.parent = transform;
		subPuzzle.transform.localPosition = new Vector3 (0, 0, 0);
		subPuzzle.SpawnSubPuzzle ();


		// Wait 3 frames
		for (int i = 0; i < 3; i++) {
			yield return null;
		}

		SetActiveSubPuzzle (subPuzzle);
		goalPictureObject.SetActive (false);
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
				if (!LevelView.IsCollectableLayerOn) {
					var piece = hit.collider.GetComponent<Piece>();
					if (piece != null) {
						var snapablePoint = activeSubPuzzle.GetPointWithinRadius(piece.transform.localPosition, 0.2f);
						if (snapablePoint != null)
						{
							snapablePoint.piece = null;
						}

						var offSet = piece.transform.position - mousePosInWorld;
						offSet.z = 0;
						draggablePieceOffset = offSet;
						draggablePiece = piece;
						break;
					}
				}

				if (hit.collider.name.Contains("Star")) {
					hasCollectedCollectable = true;
					hit.collider.gameObject.SetActive(false);
				}
			}
		}

		if (draggablePiece != null) {
			var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			draggablePiece.transform.position = new Vector3(mousePos.x,mousePos.y,draggablePiece.transform.position.z)+draggablePieceOffset;
		}

		if (Input.GetMouseButtonUp (0) && (draggablePiece != null)) {
			var snapablePoint = activeSubPuzzle.GetPointWithinRadius (draggablePiece.transform.localPosition, 0.2f);
			if (snapablePoint != null) {
				draggablePiece.transform.localPosition = new Vector3(snapablePoint.position.x, snapablePoint.position.y, draggablePiece.transform.localPosition.z);
				snapablePoint.piece = draggablePiece;
			}

			draggablePiece = null;

			var isDone = activeSubPuzzle.CheckForWin ();
			if (isDone) {
				var hasMorePuzzles = activeSubPuzzle.SetupNextPuzzlePivot ();
				if (!hasMorePuzzles) {
					activeSubPuzzle.WasDone ();
					if (activeSubPuzzle.parentSubPuzzle != null) {
						transform.localScale *= 1/GameBoard.ZoomScale;
						var camera = GameObject.Find("Main Camera");
						var newPos = camera.transform.position-activeSubPuzzle.parentSubPuzzle.transform.position;
						transform.localScale *= GameBoard.ZoomScale;

						ZoomOut (transform.position+newPos);
					} else {
						goalPictureObject.SetActive (true);
						var tempDict = Director.SaveData.LevelProgress;
						var id = Director.Instance.LevelIndex.ToString() + "_" + Director.Instance.IsAlternativeLevel.ToString();
						var levelSave = Director.SaveData.GetLevelSaveDataEntry(id);
						if (levelSave == null) {
							tempDict[id] = new LevelSaveData(true, hasCollectedCollectable);
						} else {
							if (hasCollectedCollectable){
								levelSave.gotCollectable = true;
							}
							tempDict[id] = levelSave;
						}

						Director.SaveData.LevelProgress = tempDict;
						Director.TransitionManager.PlayTransition (() => { SceneManager.LoadScene ("LevelSelectScene");}, 0.2f, Director.TransitionManager.FadeToBlack(),  Director.TransitionManager.FadeOut());
					}
					activeSubPuzzle = activeSubPuzzle.parentSubPuzzle;
				}
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
}
