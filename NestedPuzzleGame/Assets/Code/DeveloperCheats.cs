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

	public static int NumberOfPivots = -1;
	[CommandHandler(Description="Determine how deep each puzzle is.")]
	private static void SetNumberOfPivots(int numberOfPivots) {
		NumberOfPivots = numberOfPivots;
		ReloadLevelInternal ();
	}

	public static int NumberOfLayers = -1;
	[CommandHandler(Description="Determine how wide each puzzle is.")]
	private static void SetNumberOfLayers(int numberOfLayers) {
		NumberOfLayers = numberOfLayers;
		ReloadLevelInternal ();
	}

	public static int NumberOfPiecesOnX = -2;
	public static int NumberOfPiecesOnY = -2;
	[CommandHandler(Description="Determine how many pieces per puzzle.")]
	private static void SetNumberOfPieces(int numberOfPiecesOnX, int numberOfPiecesOnY) {
		NumberOfPiecesOnX = numberOfPiecesOnX;
		NumberOfPiecesOnY = numberOfPiecesOnY;
		ReloadLevelInternal ();
	}
}
