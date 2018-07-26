using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

public class SaveData {
	const string LEVEL_PROGRESS_ID = "LEVEL_PROGRESS";
	public Dictionary<string,object> LevelProgress {
		get { 
			string jsonString = PlayerPrefs.GetString (LEVEL_PROGRESS_ID);
			if (!String.IsNullOrEmpty(jsonString)) {
				return Json.Deserialize (jsonString) as Dictionary<string,object>;
			} else {
				return new Dictionary<string,object> ();
			}
		}
		set {
			PlayerPrefs.SetString (LEVEL_PROGRESS_ID, Json.Serialize (value)); 
		}
	}

	public LevelSaveData GetLevelSaveDataEntry(string id) {
		object value;
		if (LevelProgress.TryGetValue (id, out value)) {
			return UnityEngine.JsonUtility.FromJson<LevelSaveData> (MiniJSON.Json.Serialize (value));
		} else {
			return null;
		}
	}
}

[System.Serializable]
public class LevelSaveData {
	public bool completed;
	public bool gotCollectable;

	public LevelSaveData(bool completed, bool gotCollectable) {
		this.completed = completed;
		this.gotCollectable = gotCollectable;
	}
}
