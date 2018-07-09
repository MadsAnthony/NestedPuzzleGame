using Rotorz.ReorderableList;
using UnityEditor;

[CustomEditor(typeof(LevelDatabase))]
public class LevelDatabaseInspector : Editor {
	public override void OnInspectorGUI()
	{
		ReorderableListGUI.Title("Levels");
		ReorderableListGUI.ListField(serializedObject.FindProperty("levels"));

		serializedObject.ApplyModifiedProperties();
	}
}
