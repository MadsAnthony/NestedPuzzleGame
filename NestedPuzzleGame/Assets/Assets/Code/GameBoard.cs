using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour {

	[SerializeField] private GameObject piece;

	public Piece draggablePiece;

	void Start () {
		Application.targetFrameRate = 60;
		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 3; j++) {
			var pieceObject = GameObject.Instantiate (piece).GetComponent<Piece>();
			pieceObject.transform.position = new Vector3 (i,j,0);
			pieceObject.gameBoard = this;
				pieceObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(i*0.5f,j*0.33f));
			}
		}
	}

	void Update() {
		if (draggablePiece != null) {
			var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			draggablePiece.transform.position = new Vector3(mousePos.x,mousePos.y,draggablePiece.transform.position.z);
		}

		if (Input.GetMouseButtonUp (0)) {
			draggablePiece = null;
		}
	}
}
