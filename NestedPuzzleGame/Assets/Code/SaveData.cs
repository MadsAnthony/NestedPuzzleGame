using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using System.Linq;

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

	public LevelSaveData GetCurrentLevelSaveDataEntry(out string id) {
		id = Director.Instance.WorldIndex.ToString() + "_" + Director.Instance.LevelIndex.ToString() + "_" + Director.Instance.IsAlternativeLevel.ToString();
		return Director.SaveData.GetLevelSaveDataEntry(id);
	}

	public LevelSaveData GetLevelSaveDataEntry(string id) {
		object value;
		if (LevelProgress.TryGetValue (id, out value)) {
			return UnityEngine.JsonUtility.FromJson<LevelSaveData> (MiniJSON.Json.Serialize (value));
		} else {
			return new LevelSaveData (false, false);
		}
	}

	public void SaveLevelSaveDataEntry(LevelSaveData levelSaveData, string id) {
		var tempDict = LevelProgress;
		tempDict [id] = levelSaveData;
		LevelProgress = tempDict;
	}
}

[System.Serializable]
public class LevelSaveData {
	public bool completed;
	public bool gotCollectable;
	public string piecePositions = "";

	public LevelSaveData(bool completed, bool gotCollectable) {
		this.completed = completed;
		this.gotCollectable = gotCollectable;
	}

	public Dictionary<string, object> GetPiecePositions() {
		if (String.IsNullOrEmpty(piecePositions)) return null;
		return Json.Deserialize (piecePositions) as Dictionary<string,object>;
	}

	public void SetPiecePositions(Dictionary<string, PieceStateData> newPiecePositions) {
		piecePositions = Json.Serialize (newPiecePositions);
	}

	[System.Serializable]
	public class PieceStateData {
		public Vector3 localPosition;
	}
}
