using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour {
	[SerializeField] Image overlay;
	[SerializeField] Camera camera;

	public bool isPlayingTransition;
	void Start () {
		DontDestroyOnLoad (transform.gameObject);
		camera.transform.gameObject.SetActive(false);
	}

	public void PlayTransition(Action callInBetween = null, float waitTime = 0, IEnumerator transitionIn = null, IEnumerator transitionOut = null) {
		StartCoroutine(PlayTransitionCr(callInBetween, waitTime, transitionIn, transitionOut));
	}

	IEnumerator PlayTransitionCr(Action callInBetween, float waitTime, IEnumerator transitionIn, IEnumerator transitionOut = null) {
		isPlayingTransition = true;
		camera.transform.gameObject.SetActive (true);
		yield return transitionIn;
		if (callInBetween != null) {
			callInBetween ();
		}
		yield return new WaitForSeconds(waitTime);
		yield return transitionOut;
		camera.transform.gameObject.SetActive(false);
		isPlayingTransition = false;
	}

	public IEnumerator FadeToBlack() {
		yield return Fade(new Color(0,0,0,0),new Color(0,0,0,1),0.5f);
	}

	public IEnumerator FadeToColor(Color color,float time) {
		yield return Fade(overlay.color,color,time);
	}

	public IEnumerator FadeOut(float time= 0.5f) {
		yield return Fade(overlay.color,new Color(overlay.color.r,overlay.color.g,overlay.color.b,0),time);
	}

	IEnumerator Fade(Color startColor, Color endColor, float time) {
		float t = 0;
		while (true) {
			if (time > 0) {
				t += (1 / (time)) * Time.deltaTime;
			} else {
				t = 1;
			}
			overlay.color = Color.Lerp (startColor,endColor,t);
			if (t > 1) {
				break;
			}
			yield return null;
		}
	}
}

public enum TransitionTypes {
	//Transtion in
	FadeToBlack,

	//Transition out
	FadeOut,
}
