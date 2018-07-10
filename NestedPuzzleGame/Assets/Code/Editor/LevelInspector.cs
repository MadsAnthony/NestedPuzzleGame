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

			var selectedNodeAsset = GetNodeAsset (selectableNodeId);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField ("SelectionId:");
			EditorGUILayout.LabelField (selectionId);
			EditorGUILayout.EndHorizontal ();

			selectedNodeAsset.numberOfPivots = EditorGUILayout.IntField ("Number of Pivots:", selectedNodeAsset.numberOfPivots);
			selectedNodeAsset.numberOfPieces = EditorGUILayout.Vector2Field ("Number of Pieces:", selectedNodeAsset.numberOfPieces);

			EditorGUILayout.EndVertical ();
			if(GUILayout.Button("Add Node")) {
				myTarget.subPuzzleNodes.Add(new LevelAsset.SubPuzzleNode(selectedNodeAsset.id+"-"+GetChildrenNodes(selectedNodeAsset.id).Count));
				reconstruct = true;
			}
		}

		var tmpMousePos = Event.current.mousePosition;
		tmpMousePos -= windowOffset;
		tmpMousePos -= windowSize * 0.5f;
		if (Event.current.type == EventType.MouseDown && IsPositionWithinWindow(Event.current.mousePosition)) {
			if (editorMode == EditorMode.Select) {
				var mousePosInWindow = new Vector3(tmpMousePos.x/50,-tmpMousePos.y/50,0);
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

		nodeAssetDictionary = ConstructDictionary ();
		nodeDictionary = new Dictionary<string,LevelEditorNode>();
		var rootNode = nodeAssetDictionary [""][0];
		SpawnNode (rootNode, new Vector2(0,4));

		LevelEditorNode selectedNode;
		if (!string.IsNullOrEmpty (selectableNodeId)) {
			nodeDictionary.TryGetValue (selectableNodeId, out selectedNode);
			if (selectedNode != null) {
				var renderer = selectedNode.GetComponent<MeshRenderer> ();
				var tempMaterial = new Material (renderer.sharedMaterial);
				tempMaterial.color = Color.green;
				renderer.sharedMaterial = tempMaterial;
			}
		}
	}

	private GameObject levelEditorNode;
	private Dictionary<string,List<LevelAsset.SubPuzzleNode>> nodeAssetDictionary;
	private Dictionary<string,LevelEditorNode> nodeDictionary;

	private void SpawnNode(LevelAsset.SubPuzzleNode subPuzzleNode, Vector2 pos) {
		var nodeGameObject = GameObject.Instantiate (levelEditorNode);
		nodeGameObject.transform.parent = rootContainer.transform;
		nodeGameObject.transform.localPosition = new Vector2(pos.x,pos.y);
		nodeGameObject.GetComponent<LevelEditorNode> ().id = subPuzzleNode.id;
		nodeDictionary.Add (subPuzzleNode.id,nodeGameObject.GetComponent<LevelEditorNode> ());

		if (nodeAssetDictionary.ContainsKey(subPuzzleNode.id)) {
			float x = pos.x-0.75f;
			float y = pos.y - 1.5f;
			foreach (var node in nodeAssetDictionary[subPuzzleNode.id]) {
				SpawnNode (node,new Vector2(x,y));
				x += 1.5f;
			}
		}
	}

	private LevelAsset.SubPuzzleNode GetNodeAsset(string id) {
		var nodeList = GetNodeAndSiblings (id);
		if (nodeList != null) {
			foreach (var node in nodeList) {
				if (node.id == id) {
					return node;
				}
			}
		}

		return null;
	}

	private List<LevelAsset.SubPuzzleNode> GetNodeAndSiblings(string id) {
		var parentId = GetParentId (id);
		List<LevelAsset.SubPuzzleNode> nodeList = new List<LevelAsset.SubPuzzleNode>();
		nodeAssetDictionary.TryGetValue (parentId, out nodeList);
		return nodeList;
	}

	private List<LevelAsset.SubPuzzleNode> GetChildrenNodes(string id) {
		List<LevelAsset.SubPuzzleNode> nodeList;
		nodeAssetDictionary.TryGetValue (id, out nodeList);
		if (nodeList != null) {
			return nodeList;
		}
		return new List<LevelAsset.SubPuzzleNode>();
	}

	private string GetParentId(string id) {
		string parentId = String.Empty;
		var idPath = id.Split('-');
		for (int i = 0; i<idPath.Length-1; i++) {
			if (i > 0) {
				parentId += "-";
			}
			parentId += idPath [i];
		}
		return parentId;
	}

	private Dictionary<string,List<LevelAsset.SubPuzzleNode>> ConstructDictionary() {
		LevelAsset myTarget = (LevelAsset)target;
		var result = new Dictionary<string,List<LevelAsset.SubPuzzleNode>> ();
		foreach (var node in myTarget.subPuzzleNodes) {
			string parentId = GetParentId (node.id);

			if (!result.ContainsKey(parentId)) {
				var newList = new List<LevelAsset.SubPuzzleNode> ();
				newList.Add (node);
				result.Add (parentId,newList);
			} else {
				result [parentId].Add (node);
			}
		}

		return result;
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
