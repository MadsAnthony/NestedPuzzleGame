using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;

public class LevelView : MonoBehaviour {
	[SerializeField] private Camera mainCamera;
	[SerializeField] private GameObject menuObject;
	[SerializeField] private GameBoard gameboard;
	[SerializeField] private CombineImageEffect combineImageEffect;

	private void Start() {
		DisableCollectableLayer();
	}
	
	public void OpenPauseMenu() {
		menuObject.SetActive(!menuObject.activeSelf);
	}
	
	public void ClosePauseMenu() {
		menuObject.SetActive(false);
	}
	
	public void GotoLevelSelect() {
		Director.TransitionManager.PlayTransition (() => { SceneManager.LoadScene ("LevelSelectScene");}, 0.2f, Director.TransitionManager.FadeToBlack(),  Director.TransitionManager.FadeOut());
	}

	public static bool IsCollectableLayerOn;
	public void ToggleCollectableLayer() {
		IsCollectableLayerOn = !IsCollectableLayerOn;
		if (IsCollectableLayerOn) {
			gameboard.activeSubPuzzle.CheckForCollectable();
			mainCamera.gameObject.GetComponent<Grayscale>().enabled = true;
			mainCamera.gameObject.GetComponent<VignetteAndChromaticAberration>().enabled = true;
			mainCamera.gameObject.GetComponent<BlurOptimized>().enabled = true;
			combineImageEffect.EffectMaterial.SetFloat("_HideCameraTwo",1);
		} else {
			DisableCollectableLayer();
		}
	}

	private void DisableCollectableLayer() {
		mainCamera.gameObject.GetComponent<Grayscale>().enabled = false;
		mainCamera.gameObject.GetComponent<VignetteAndChromaticAberration>().enabled = false;
		mainCamera.gameObject.GetComponent<BlurOptimized>().enabled = false;
		combineImageEffect.EffectMaterial.SetFloat("_HideCameraTwo",0);
	}
}
