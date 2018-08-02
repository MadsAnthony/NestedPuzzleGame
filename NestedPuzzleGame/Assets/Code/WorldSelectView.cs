using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class WorldSelectView : MonoBehaviour {
	private Vector3 mousePosDownInWorld;
	private Vector3 mousePosUpInWorld;

	private void Update() {
		if (Input.GetMouseButtonDown (0)) {
			mousePosDownInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mousePosDownInWorld.z = -1;
		}
		if (Input.GetMouseButtonUp (0)) {
			mousePosUpInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mousePosDownInWorld.z = -1;

			MouseUp ();
		}
	}

	private void MouseUp() {
		var hits = Physics.RaycastAll(mousePosDownInWorld, Vector3.forward, 100);
		if (hits.Length > 0) {
			var worldDot = hits [0].transform.GetComponentInParent<WorldDot> ();
			if (worldDot != null) {
				worldDot.Click ();
			}
		}
	}

	public void GoBack() {
		Director.TransitionManager.PlayTransition (() => { SceneManager.LoadScene ("TitleScene");}, 0.2f, Director.TransitionManager.FadeToBlack(),  Director.TransitionManager.FadeOut());
	}
}
