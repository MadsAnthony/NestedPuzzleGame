using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level", menuName = "Assets/New Level", order = 1)]
public class LevelAsset : ScriptableObject {
	public Texture picture;

	public int numberOfLayers = 0;
	public int numberOfPivots = 1;
	public Vector2 numberOfPieces = new Vector2(1,1);
}
