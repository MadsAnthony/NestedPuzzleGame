using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
	public string id;
	public GameBoard gameBoard;

	void OnMouseDown() {
		var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		var offSet = transform.position - mousePos;
		offSet.z = 0;
		gameBoard.draggablePieceOffset = offSet;
		gameBoard.draggablePiece = this;
	}
}
