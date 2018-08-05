using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
	[SerializeField] private MeshRenderer pieceRenderer;
	[SerializeField] private MeshRenderer collectableLayerRenderer;
	[SerializeField] private GameObject backdrop;
	[SerializeField] private AnimationCurve pieceJiggleCurve;
	[SerializeField] private AnimationCurve pieceMoveCurve;
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

	public void Jiggle() {
		if (isRotating) return;
		StartCoroutine(RotateCr (gameObject, new Vector3 (0, 0, 45), 0.2f));
	}

	private bool isRotating;
	private IEnumerator RotateCr(GameObject gameObject, Vector3 endRotation, float sec) {
		isRotating = true;
		var startRotation = gameObject.transform.localEulerAngles;
		float t = 0;
		while (t < 1) {
			t += (1/sec)*Time.deltaTime;
			var value = pieceJiggleCurve.Evaluate (t);
			gameObject.transform.localEulerAngles = (startRotation * (1 - value)) + endRotation * value;
			yield return null;
		}
		gameObject.transform.localEulerAngles = startRotation;
		isRotating = false;
	}

	public void Move(Vector3 endPosition, Action callback) {
		if (isMoving) return;
		var distance = gameObject.transform.localPosition - endPosition;
		StartCoroutine (MoveCr (gameObject, endPosition, Mathf.Max(0.1f*distance.magnitude,0.5f), callback));
	}

	public void Move(Vector3 endPosition, float sec, Action callback) {
		if (isMoving) return;
		StartCoroutine (MoveCr (gameObject, endPosition, sec, callback));
	}

	private bool isMoving;
	private IEnumerator MoveCr(GameObject gameObject, Vector3 endPosition, float sec, Action callback) {
		isMoving = true;
		var startPosition = gameObject.transform.localPosition;
		startPosition.z = 0.5f;
		endPosition.z = startPosition.z;
		float t = 0;
		while (t < 1) {
			t += (1/sec)*Time.deltaTime;
			var value = pieceMoveCurve.Evaluate (t);
			gameObject.transform.localPosition = (startPosition * (1 - value)) + endPosition * value;
			yield return null;
		}
		if (callback != null) {
			callback ();
		}
		isMoving = false;
	}
}
