using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubPuzzleButton : MonoBehaviour {
	public GameBoard gameBoard;
	public SubPuzzle subPuzzle;

	private bool toggle = true;
	void OnMouseDown() {
		if (toggle) {
			gameBoard.ZoomIn (subPuzzle.transform.localPosition);
			gameBoard.SetActiveSubPuzzle (subPuzzle);
		} else {
			gameBoard.StartCoroutine (gameBoard.ZoomOut ());
		}
		toggle = !toggle;
	}
}
