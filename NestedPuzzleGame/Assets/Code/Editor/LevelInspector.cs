using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Rotorz.ReorderableList;

[CustomEditor(typeof(LevelAsset))]
public class LevelInspector : Editor {
	private RenderTexture editorRenderTexture;
	private Vector2 windowOffset = new Vector2(20,100);
	private Vector2 windowSize = new Vector2(500,500);
	private Color windowBackgroundColor = new Color (1,1,1,1);

	private Vector2 startMousePos;
	private Vector2 mousePos;
	private EditorMode editorMode;
	private string selectableNodeId;

	public override void OnInspectorGUI() {
		bool reconstruct = false;
		LevelAsset myTarget = (LevelAsset)target;

		var editorModeCached = editorMode;
		string[] editorModeOptions = {"Select", "Add"};
		editorMode = (EditorMode)EditorGUILayout.Popup ("Mode", (int)editorMode, editorModeOptions);

		if (editorModeCached != editorMode) {
			reconstruct = true;
		}

		if (cameraGameObject != null) {
			EditorGUI.DrawPreviewTexture (new Rect (0+windowOffset.x, 0+windowOffset.y, windowSize.x, windowSize.y), editorRenderTexture);
		}

		var tmpMousePos = Event.current.mousePosition;
		tmpMousePos -= windowOffset;
		tmpMousePos -= windowSize * 0.5f;
		if (Event.current.type == EventType.MouseDown && IsPositionWithinWindow(Event.current.mousePosition)) {
			if (editorMode == EditorMode.Select) {
				var mousePosInWindow = new Vector3(tmpMousePos.x/50,tmpMousePos.y/50,0);
				var a = Physics.RaycastAll (cameraGameObject.transform.position+mousePosInWindow, Vector3.forward, 200);
				selectableNodeId = String.Empty;
				if (a.Length > 0) {
					selectableNodeId = a[0].collider.gameObject.GetComponent<LevelEditorNode>().id;
				}
				reconstruct = true;
			} /*else if (editorMode == EditorMode.Add) {
				if (pieceType == PieceType.Tile) {
					if (Event.current.button == 0) {
						if (!myTarget.Tiles.Exists(x => { return x.Pos == mousePosInGrid; })) {
							myTarget.Tiles.Add(new TileData(mousePosInGrid));
							reconstruct = true;
						}
					}

					if (Event.current.button == 1) {
						var posibleTile = myTarget.Tiles.Find(x => { return x.Pos == mousePosInGrid; });
						if (posibleTile != null) {
							myTarget.Tiles.Remove(posibleTile);
							reconstruct = true;
						}
					}
				} else {
					var posibleTile = myTarget.Tiles.Find(x => { return x.Pos == mousePosInGrid; });
					if (Event.current.button == 0) {
						if (posibleTile != null && posibleTile.Pieces.Count == 0) {
							posibleTile.Pieces.Add(new PieceData(pieceType));
							reconstruct = true;
						}
					}
					if (Event.current.button == 1) {
						if (posibleTile != null && posibleTile.Pieces.Count != 0){
							posibleTile.Pieces = new List<PieceData>();
							reconstruct = true;
						}
					}
				}
			}*/

		}

		if (Event.current.button == 2) {
			if (Event.current.type == EventType.MouseDown) {
				mousePos = Event.current.mousePosition;
			}
			if (Event.current.type == EventType.MouseDrag) {
				var mouseDir = (mousePos - Event.current.mousePosition) * 0.02f;
				cameraGameObject.transform.position += new Vector3 (mouseDir.x, -mouseDir.y, 0);
				mousePos = Event.current.mousePosition;
			}
		}
		if (reconstruct) {
			DestroyLevel ();
			ConstructLevel();
		}

		var selectionId = "0";
		if (editorMode == EditorMode.Select && !string.IsNullOrEmpty(selectableNodeId)) {
			selectionId = selectableNodeId;
		}
		EditorGUILayout.BeginVertical();

		GUILayout.Space (windowOffset.y+windowSize.y);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField ("SelectionId:");
		EditorGUILayout.LabelField (selectionId);
		EditorGUILayout.EndHorizontal ();

		var value = 0;
		EditorGUILayout.IntField (value);
		EditorGUILayout.EndVertical ();
		if(GUILayout.Button("Add Node")) {
			myTarget.subPuzzleNodes.Add(new LevelAsset.SubPuzzleNode("0"));
		}

		EditorUtility.SetDirty (myTarget);
	}

	private LevelAsset.SubPuzzleNode GetSubPuzzleNode(LevelAsset levelAsset) {
		return levelAsset.subPuzzleNodes[0];
	}

	bool IsPositionWithinWindow(Vector2 pos) {
		return 	pos.x > windowOffset.x && pos.x < windowOffset.x+windowSize.x &&
				pos.y > windowOffset.y && pos.y < windowOffset.y+windowSize.y;
	}
	
	private void OnEnable() {
		if (Application.isPlaying) {
			DestroyLevel ();
		} else {
			ConstructLevel ();
		}
	}

	private void OnDisable() {
		DestroyLevel ();
	}

	private GameObject cameraGameObject;
	private GameObject rootContainer;
	private void ConstructLevel() {
		LevelAsset myTarget = (LevelAsset)target;
		rootContainer = new GameObject();
		rootContainer.transform.position = new Vector3 (0,0,0);
		
		cameraGameObject = new GameObject();
		cameraGameObject.transform.position = new Vector3 (0,0,-100);
		var camera = cameraGameObject.AddComponent<Camera> ();
		cameraGameObject.hideFlags = HideFlags.HideAndDontSave;
		camera.orthographic = true;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = windowBackgroundColor;
		editorRenderTexture = Resources.Load ("EditorRenderTexture") as RenderTexture;
		camera.targetTexture = editorRenderTexture;


		var levelEditorNode = Resources.Load ("LevelEditorNode") as GameObject;
		var nodeGameObject = GameObject.Instantiate (levelEditorNode);

		nodeGameObject.transform.parent = rootContainer.transform;
		nodeGameObject.transform.localPosition = Vector3.zero;
		nodeGameObject.GetComponent<LevelEditorNode> ().id = "1";
		SetHideFlagsRecursively(rootContainer);

		if (!String.IsNullOrEmpty(selectableNodeId)) {
			var renderer = nodeGameObject.GetComponent<MeshRenderer> ();
			var tempMaterial = new Material(renderer.sharedMaterial);
			tempMaterial.color = Color.green;
			renderer.sharedMaterial = tempMaterial;
		}
	}

	public void SetHideFlagsRecursively(GameObject gameObject) {
		gameObject.hideFlags = HideFlags.HideAndDontSave;
		foreach (Transform child in gameObject.transform) {
			SetHideFlagsRecursively(child.gameObject);
		}
	}
	
	void AssignObjectToGrid(GameObject newObject, int x, int z) {
		newObject.transform.parent = rootContainer.transform;
		newObject.transform.localEulerAngles = new Vector3 (0,0,0);
		newObject.transform.localPosition = new Vector3 (x*2,0,z*2);
	}
	
	private void DestroyLevel() {
		GameObject.DestroyImmediate (rootContainer);
		GameObject.DestroyImmediate (cameraGameObject);
	}

	public enum EditorMode {
		Select,
		Add
	};
}
