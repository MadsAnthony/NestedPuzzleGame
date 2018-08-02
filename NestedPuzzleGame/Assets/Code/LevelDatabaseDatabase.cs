using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelDatabaseDatabase", menuName = "Assets/New LevelDatabaseDatabase", order = 1)]
public class LevelDatabaseDatabase : ScriptableObject {
	public List<LevelDatabase> levelDatabases = new List<LevelDatabase>();
}
