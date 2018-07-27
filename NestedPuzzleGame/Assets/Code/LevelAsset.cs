using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level", menuName = "Assets/New Level", order = 1)]
public class LevelAsset : ScriptableObject {
	
	public Texture picture;

	public bool isMasterPuzzle;
	public Vector2 numberOfPieces = new Vector2(1,1);
	public List<SubPuzzleNode> subPuzzleNodes = new List<SubPuzzleNode>();

	[Serializable]
	public class SubPuzzleNode {
		public string id;
		public int numberOfPivots = 1;
		public Vector2 numberOfPieces = new Vector2(1,1);
		
		public Collectable collectable = new Collectable();
		
		public SubPuzzleNode(string id) {
			this.id = id;
		}
	}

	[Serializable]
	public class Collectable {
		public bool isActive;
		public Vector2 position;
		public Vector2 scale = new Vector2(1,1);
	}
}
