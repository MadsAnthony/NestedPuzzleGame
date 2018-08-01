using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
	[SerializeField] private MeshRenderer pieceRenderer;
	[SerializeField] private MeshRenderer collectableLayerRenderer;
	[SerializeField] private GameObject backdrop;
	public GameObject outline;

	public GameObject Backdrop {
		get { return backdrop; }
	}
	public MeshRenderer PieceRenderer {
		get { return pieceRenderer; }
	}
	public MeshRenderer CollectableLayerRenderer {
		get { return collectableLayerRenderer; }
	}
	
	public string id;
}
