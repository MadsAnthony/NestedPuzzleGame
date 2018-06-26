using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubPuzzleButton : MonoBehaviour {
	public GameBoard gameBoard;
	public SubPuzzle subPuzzle;

	void OnMouseDown() {
		gameBoard.transform.localScale *= GameBoard.ZoomScale;
		var camera = GameObject.Find("Main Camera");
		var newPos = camera.transform.position-subPuzzle.transform.position;
		gameBoard.transform.localScale *= 1/GameBoard.ZoomScale;

		gameBoard.ZoomIn (gameBoard.transform.position+new Vector3(newPos.x, newPos.y,0));
		gameBoard.SetActiveSubPuzzle (subPuzzle);
		subPuzzle.DeactivateAllSubPuzzles ();
	}
}
