using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	Camera camera;

	[SerializeField] bool IgnoreCameraMaterial;
	public Material cameraMaterial;

	public AnimationCurve showLayerCurve;
	public AnimationCurve hideLayerCurve;
	// Use this for initialization
	void Start () {
		if (IgnoreCameraMaterial) return;
		cameraMaterial.SetFloat ("_Transparency", 0);
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void CameraShake() {
		StartCoroutine(Shake());
	}

	public bool IsLayerVisible() {
		return cameraMaterial.GetFloat("_Transparency")>0.1f;
	}
	public void ShowLayer() {
		StartCoroutine(ShowLayerCr(showLayerCurve));
	}

	public void HideLayer() {
		StartCoroutine(ShowLayerCr(hideLayerCurve));
	}

	IEnumerator ShowLayerCr(AnimationCurve animationCurve, float duration = 0.3f) {
		float t = 0;
		while (t<1) {
			t += (1 / ((duration))) * Time.deltaTime;

			float evalT = animationCurve.Evaluate (t);

			cameraMaterial.SetFloat ("_Transparency", evalT);

			yield return null;
		}
	}

	IEnumerator Shake() {
		float x = 0;
		while (true) {
			x += 0.4f;
			transform.eulerAngles = new Vector3 (0,0,4*Mathf.Sin(x));
			if (x > 4) {
				break;
			}
			yield return null;
		}
		transform.eulerAngles = new Vector3 (0,0,0);
	}
}
