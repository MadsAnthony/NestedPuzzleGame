using System;
using UnityEngine;
using System.Collections.Generic;

public class LevelAssetHelper {
	public static LevelAsset.SubPuzzleNode GetNodeAsset(Dictionary<string,List<LevelAsset.SubPuzzleNode>> nodeAssetDictionary, string id) {
		var nodeList = GetNodeAndSiblings (nodeAssetDictionary, id);
		if (nodeList != null) {
			foreach (var node in nodeList) {
				if (node.id == id) {
					return node;
				}
			}
		}

		return null;
	}

	public static List<LevelAsset.SubPuzzleNode> GetNodeAndSiblings(Dictionary<string,List<LevelAsset.SubPuzzleNode>> nodeAssetDictionary, string id) {
		var parentId = GetParentId (id);
		List<LevelAsset.SubPuzzleNode> nodeList = new List<LevelAsset.SubPuzzleNode>();
		nodeAssetDictionary.TryGetValue (parentId, out nodeList);
		return nodeList;
	}

	public static List<LevelAsset.SubPuzzleNode> GetChildrenNodes(Dictionary<string,List<LevelAsset.SubPuzzleNode>> nodeAssetDictionary, string id) {
		List<LevelAsset.SubPuzzleNode> nodeList;
		nodeAssetDictionary.TryGetValue (id, out nodeList);
		if (nodeList != null) {
			return nodeList;
		}
		return new List<LevelAsset.SubPuzzleNode>();
	}

	public static string GetParentId(string id) {
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

	public static Dictionary<string,List<LevelAsset.SubPuzzleNode>> ConstructDictionary(List<LevelAsset.SubPuzzleNode> subPuzzleNodes) {
		var result = new Dictionary<string,List<LevelAsset.SubPuzzleNode>> ();
		foreach (var node in subPuzzleNodes) {
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
}
