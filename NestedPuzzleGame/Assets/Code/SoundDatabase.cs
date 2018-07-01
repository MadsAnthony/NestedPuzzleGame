using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Sound/New SoundDatabase", order = 1)]
public class SoundDatabase : ScriptableObject {
	[GUIHeader("Gameplay")]
	public SoundDefinition tap;
}
	
public class GUIHeader : System.Attribute {
	public GUIHeader(string s) {
		title = s;
	}
	public string title;
}
