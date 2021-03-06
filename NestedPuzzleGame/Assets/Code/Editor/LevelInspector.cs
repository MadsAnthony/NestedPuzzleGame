﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Rotorz.ReorderableList;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(LevelAsset))]
public class LevelInspector : Editor {
	private RenderTexture editorRenderTexture;
	private Vector2 windowOffset = new Vector2(20,200);
	private Vector2 windowSize = new Vector2(500,500);
	private Color windowBackgroundColor = new Color (1,1,1,1);

	private Vector2 startMousePos;
	private Vector2 mousePos;
	private EditorMode editorMode;
	private string selectableNodeId;
	private int selectablePuzzlePivotId;

	public override void OnInspectorGUI() {
		if (EditorApplication.isPlaying) return;
		bool reconstruct = false;
		LevelAsset myTarget = (LevelAsset)target;

		var editorModeCached = editorMode;
		string[] editorModeOptions = {"Select", "Add"};
		if (GUILayout.Button ("Play Level")) {
			DestroyLevel ();
			EditorApplication.isPlaying = false;
			EditorSceneManager.OpenScene ("Assets/Scenes/LevelScene.unity");
			var gameBoard = GameObject.Find ("GameBoard").GetComponent<GameBoard> ();
			gameBoard.levelOverride = myTarget;
			EditorApplication.isPlaying = true;
		}
		myTarget.isMasterPuzzle = EditorGUILayout.Toggle("IsMasterPuzzle:", myTarget.isMasterPuzzle);
		editorMode = (EditorMode)EditorGUILayout.Popup ("Mode", (int)editorMode, editorModeOptions);
		myTarget.picture = EditorGUILayout.ObjectField ("GoalTexture", myTarget.picture, typeof(Texture), false) as Texture;
		if (GUILayout.Button ("Clear Nodes")) {
			myTarget.subPuzzleNodes.Clear ();
			myTarget.subPuzzleNodes.Add (new LevelAsset.SubPuzzleNode("0"));
			reconstruct = true;
		}

		if (editorModeCached != editorMode) {
			reconstruct = true;
		}

		if (cameraGameObject != null) {
			EditorGUI.DrawPreviewTexture (new Rect (0+windowOffset.x, 0+windowOffset.y, windowSize.x, windowSize.y), editorRenderTexture);
		}

		var selectionId = "0";
		if (editorMode == EditorMode.Select && !string.IsNullOrEmpty(selectableNodeId)) {
			selectionId = selectableNodeId;
		}
		if (!string.IsNullOrEmpty (selectableNodeId)) {
			EditorGUILayout.BeginVertical();
			GUILayout.Space (windowOffset.y+windowSize.y);

			var selectedNodeAsset = LevelAssetHelper.GetNodeAsset (nodeAssetDictionary, selectableNodeId);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField ("SelectionId:");
			EditorGUILayout.LabelField (selectionId);
			EditorGUILayout.EndHorizontal ();

			var puzzlePivotAsset = selectedNodeAsset.puzzlePivots [selectablePuzzlePivotId];
			puzzlePivotAsset.numberOfPieces = EditorGUILayout.Vector2Field ("Number of Pieces:", puzzlePivotAsset.numberOfPieces);
			puzzlePivotAsset.type = (PuzzlePivotType)EditorGUILayout.EnumPopup ("Type:", puzzlePivotAsset.type);
			selectedNodeAsset.collectable.isActive = EditorGUILayout.Toggle("Has Collectable:", selectedNodeAsset.collectable.isActive);
			if (selectedNodeAsset.collectable.isActive) {
				selectedNodeAsset.collectable.position = EditorGUILayout.Vector2Field ("Position of collectable:", selectedNodeAsset.collectable.position);
				selectedNodeAsset.collectable.scale = EditorGUILayout.Vector2Field ("Scale of collectable:", selectedNodeAsset.collectable.scale);
			}

			EditorGUILayout.EndVertical ();
			if(GUILayout.Button("Add PuzzlePivot")) {
				selectedNodeAsset.puzzlePivots.Add(new LevelAsset.PuzzlePivot());
				reconstruct = true;
			}
			if(GUILayout.Button("Add Node")) {
				myTarget.subPuzzleNodes.Add(new LevelAsset.SubPuzzleNode(selectedNodeAsset.id+"-"+LevelAssetHelper.GetChildrenNodes(nodeAssetDictionary,selectedNodeAsset.id).Count));
				reconstruct = true;
			}
		}

		var tmpMousePos = Event.current.mousePosition;
		tmpMousePos -= windowOffset;
		tmpMousePos -= windowSize * 0.5f;
		if (Event.current.type == EventType.MouseDown && IsPositionWithinWindow(Event.current.mousePosition)) {
			if (editorMode == EditorMode.Select) {
				var mousePosInWindow = new Vector3(tmpMousePos.x/50,-tmpMousePos.y/50,0);
				var hits = Physics.RaycastAll (cameraGameObject.transform.position+mousePosInWindow, Vector3.forward, 200);
				selectableNodeId = String.Empty;
				if (hits.Length > 0) {
					if (hits [0].collider.gameObject.GetComponent<LevelEditorNode> () != null) {
						selectableNodeId = hits[0].collider.gameObject.GetComponent<LevelEditorNode>().nodeId;
					}
					if (hits [0].collider.gameObject.GetComponent<LevelEditorPuzzlePivot> () != null) {
						var levelEditorPuzzlePivot = hits [0].collider.gameObject.GetComponent<LevelEditorPuzzlePivot> ();
						selectableNodeId = levelEditorPuzzlePivot.parent.nodeId;
						selectablePuzzlePivotId = levelEditorPuzzlePivot.parent.puzzlePivots.IndexOf(levelEditorPuzzlePivot);
					}
				}
				reconstruct = true;
			}

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

		levelEditorNode = Resources.Load ("LevelEditorNode") as GameObject;
		levelEditorPuzzlePivot = Resources.Load ("levelEditorPuzzlePivot") as GameObject;

		nodeAssetDictionary = LevelAssetHelper.ConstructDictionary (myTarget.subPuzzleNodes);
		nodeDictionary = new Dictionary<string,LevelEditorNode>();
		var rootNode = nodeAssetDictionary [""][0];
		SpawnNode (rootNode, new Vector2(0,4));

		LevelEditorNode selectedNode;
		if (!string.IsNullOrEmpty (selectableNodeId)) {
			nodeDictionary.TryGetValue (selectableNodeId, out selectedNode);
			if (selectedNode != null) {
				var pivot = selectedNode.puzzlePivots [selectablePuzzlePivotId];
				var renderer = pivot.GetComponent<MeshRenderer> ();
				var tempMaterial = new Material (renderer.sharedMaterial);
				tempMaterial.color = Color.green;
				renderer.sharedMaterial = tempMaterial;
			}
		}
	}

	private GameObject levelEditorNode;
	private GameObject levelEditorPuzzlePivot;
	private Dictionary<string,List<LevelAsset.SubPuzzleNode>> nodeAssetDictionary;
	private Dictionary<string,LevelEditorNode> nodeDictionary;

	private void SpawnNode(LevelAsset.SubPuzzleNode subPuzzleNode, Vector2 pos) {
		var nodeGameObject = GameObject.Instantiate (levelEditorNode);
		nodeGameObject.transform.parent = rootContainer.transform;
		nodeGameObject.transform.localPosition = new Vector2(pos.x,pos.y);
		nodeGameObject.GetComponent<LevelEditorNode> ().nodeId = subPuzzleNode.id;
		nodeDictionary.Add (subPuzzleNode.id,nodeGameObject.GetComponent<LevelEditorNode> ());

		int i = 0;
		foreach (var puzzlePivotAsset in subPuzzleNode.puzzlePivots) {
			var puzzlePivotObject = GameObject.Instantiate (levelEditorPuzzlePivot);
			var puzzlePivot = puzzlePivotObject.GetComponent<LevelEditorPuzzlePivot> ();
			puzzlePivot.transform.parent = rootContainer.transform;
			puzzlePivot.transform.localPosition = new Vector3(pos.x+0.3f*i,pos.y-0.3f*i,i);
			puzzlePivot.parent = nodeGameObject.GetComponent<LevelEditorNode> ();
			nodeGameObject.GetComponent<LevelEditorNode> ().puzzlePivots.Add (puzzlePivot);
			i++;
		}

		if (nodeAssetDictionary.ContainsKey(subPuzzleNode.id)) {
			float x = pos.x-0.75f;
			float y = pos.y - 1.5f;
			foreach (var node in nodeAssetDictionary[subPuzzleNode.id]) {
				SpawnNode (node,new Vector2(x,y));
				x += 1.5f;
			}
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
