using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opencoding.CommandHandlerSystem;
using UnityEngine.SceneManagement;

public class DeveloperCheats {

	static DeveloperCheats() {
		CommandHandlers.RegisterCommandHandlers(typeof(DeveloperCheats));
	}

	[CommandHandler]
	private static void DeleteAllSaveData() {
		PlayerPrefs.DeleteAll ();
	}

	[CommandHandler]
	private static void GotoLevelSelectScene() {
		SceneManager.LoadScene ("LevelSelectScene");
	}

	private static void ReloadLevelInternal() {
		SceneManager.LoadScene ("LevelScene");
	}

	[CommandHandler(Description="Will reload the level.")]
	private static void ReloadLevel() {
		ReloadLevelInternal ();
	}

	[CommandHandler(Description="Null reference crash")]
	private static void NullReferenceCrash() {
		List<int> aList = null;
		var count = aList.Count;
	}
}
