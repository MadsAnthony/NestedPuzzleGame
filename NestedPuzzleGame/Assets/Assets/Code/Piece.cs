using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
	public GameBoard gameBoard;
	void OnMouseDown() {
		gameBoard.draggablePiece = this;
	}

}
