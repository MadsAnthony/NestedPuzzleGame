using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class SubPuzzleButton : MonoBehaviour {
	public GameBoard gameBoard;
	public SubPuzzle subPuzzle;

	public void Click() {
		gameBoard.transform.localScale *= GameBoard.ZoomScale;
		var camera = GameObject.Find("CameraPivot/Main Camera");
		var newPos = camera.transform.position-subPuzzle.transform.position;
		gameBoard.transform.localScale *= 1/GameBoard.ZoomScale;

		gameBoard.ZoomIn (gameBoard.transform.position+new Vector3(newPos.x, newPos.y,0));
		gameBoard.SetActiveSubPuzzle (subPuzzle);
		subPuzzle.DeactivateAllSubPuzzles ();
	}
}
