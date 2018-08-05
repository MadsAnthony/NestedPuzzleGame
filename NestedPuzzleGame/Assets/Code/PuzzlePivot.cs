using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePivot {
	public GameObject pivot;
	public List<Piece> pieces = new List<Piece>();
	public List<SnapablePoint> snapablePoints = new List<SnapablePoint>();
	public GameObject collectableObject;
	public Vector2 numberOfPieces;

	public PuzzlePivot(GameObject parent) {
		pivot = new GameObject();
		pivot.transform.parent = parent.transform;
		pivot.transform.localPosition = new Vector3(0,0,0);
	}
}
