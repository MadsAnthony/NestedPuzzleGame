using Rotorz.ReorderableList;
using UnityEditor;

[CustomEditor(typeof(LevelDatabaseDatabase))]
public class LevelDatabaseDatabaseInspector : Editor {
	public override void OnInspectorGUI()
	{
		ReorderableListGUI.Title("Level Databases");
		ReorderableListGUI.ListField(serializedObject.FindProperty("levelDatabases"));

		serializedObject.ApplyModifiedProperties();
	}
}
