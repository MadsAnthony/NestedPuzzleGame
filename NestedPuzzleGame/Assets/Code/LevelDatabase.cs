using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Assets/New LevelDatabase", order = 1)]
public class LevelDatabase : ScriptableObject {
	public List<LevelAsset> levels = new List<LevelAsset>();
}
