using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
	[SerializeField] private MeshRenderer pieceRenderer;
	[SerializeField] private MeshRenderer collectableLayerRenderer;
	public GameObject outline;
	
	public MeshRenderer PieceRenderer {
		get { return pieceRenderer; }
	}
	public MeshRenderer CollectableLayerRenderer {
		get { return collectableLayerRenderer; }
	}
	
	public string id;
}
