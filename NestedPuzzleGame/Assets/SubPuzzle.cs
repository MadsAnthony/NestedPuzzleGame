using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubPuzzle : MonoBehaviour {
	[SerializeField] private GameBoard gameBoard;

	private bool toggle = true;
	void OnMouseDown() {
		if (toggle) {
			gameBoard.ZoomIn (transform.localPosition);
		} else {
			gameBoard.StartCoroutine (gameBoard.ZoomOut ());
		}
		toggle = !toggle;
	}
}
