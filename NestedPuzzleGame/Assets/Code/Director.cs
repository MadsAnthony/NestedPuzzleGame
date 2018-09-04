using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : MonoBehaviour  {
	private static Director instance;
	private static bool hasBeenDestroyed;

	[SerializeField] private LevelDatabaseDatabase worldDatabase;
	[SerializeField] private SoundDatabase soundDatabase;

	private int worldIndex = 0;
	private int levelIndex = 1;
	private int prevLevelIndex = 1;
	public Dictionary<string, Texture2D> MasterPuzzleImages = new Dictionary<string, Texture2D>();
	public bool IsAlternativeLevel;
	public LevelExitState levelExitState = LevelExitState.None;
	public int LevelIndex 
	{
		get {return levelIndex;}
		set 
		{
			prevLevelIndex = levelIndex;
			levelIndex = value;
		}
	}
	public int PrevLevelIndex {
		get { return prevLevelIndex;}
	}

	public int WorldIndex  {
		get { return worldIndex; }
		set { worldIndex = value; }
	}

	private GameEventManager		gameEventManager;
	private UIManager 		 		uiManager;
	private TransitionManager		transitionManager;
	private SaveData				saveData;

	public static GameEventManager 		GameEventManager 	{get {return Instance.gameEventManager;}}
	public static LevelDatabase    		LevelDatabase 		{get {return WorldDatabase.levelDatabases[Instance.WorldIndex];}}
	public static LevelDatabaseDatabase WorldDatabase 		{get {return Instance.worldDatabase;}}
	public static UIManager    	   		UIManager			{get {return Instance.uiManager;}}
	public static TransitionManager		TransitionManager 	{get {return Instance.transitionManager;}}
	public static SoundDatabase			Sounds 				{get {return Instance.soundDatabase;}}
	public static SaveData				SaveData 			{get {return Instance.saveData;}}

	public static Director Instance
	{
		get
		{
			if (instance == null && !Director.hasBeenDestroyed)
			{
				var asset = (Director)Resources.Load ("Director", typeof(Director));
				instance = (Director)GameObject.Instantiate (asset);
				instance.Load();
			}
			return instance;
		}
	}

	void OnDestroy() {
		Director.hasBeenDestroyed = true;
	}

	void Load() {
		gameEventManager  = new GameEventManager();
		uiManager 		  = new UIManager();
		transitionManager = SetupTransitionManager();
		saveData 		  = new SaveData ();
	}

	void Start () {
		DontDestroyOnLoad (transform.gameObject);
		Application.targetFrameRate = 60;
		new DeveloperCheats ();
	}

	TransitionManager SetupTransitionManager() {
		var asset = (TransitionManager)Resources.Load ("TransitionManager", typeof(TransitionManager));
		return (TransitionManager)GameObject.Instantiate (asset);
	}

	public static void CameraShake() {
		GameObject.Find ("BaseCamera").GetComponentInParent<CameraManager>().CameraShake();
	}
	
	public static int GetAmountOfMasterPieces() {
		int amountOfPieces = 0;
		for (int i = 1; i < Director.LevelDatabase.levels.Count; i++) {
			var levelSaveNormal = Director.SaveData.GetLevelSaveDataEntry(Director.Instance.WorldIndex.ToString()+"_"+i.ToString() + "_" + false.ToString());
			var levelSaveAlternative = Director.SaveData.GetLevelSaveDataEntry(Director.Instance.WorldIndex.ToString()+"_"+i.ToString() + "_" + true.ToString());

			if (levelSaveNormal != null && levelSaveNormal.gotCollectable) {
				amountOfPieces++;
			}
			if (levelSaveAlternative != null && levelSaveAlternative.gotCollectable) {
				amountOfPieces++;
			}
		}

		return amountOfPieces;
	}
}
