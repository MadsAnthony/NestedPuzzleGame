using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelView : MonoBehaviour {
	[SerializeField] private Camera mainCamera;
	[SerializeField] private GameObject menuObject;
	public void OpenPauseMenu() {
		menuObject.SetActive(!menuObject.activeSelf);
	}
	
	public void ClosePauseMenu() {
		menuObject.SetActive(false);
	}
	
	public void GotoLevelSelect() {
		Director.TransitionManager.PlayTransition (() => { SceneManager.LoadScene ("LevelSelectScene");}, 0.2f, Director.TransitionManager.FadeToBlack(),  Director.TransitionManager.FadeOut());
	}

	private bool isCollectableLayerOn;
	public void ToggleCollectableLayer() {
		isCollectableLayerOn = !isCollectableLayerOn;
		if (isCollectableLayerOn) {
			mainCamera.cullingMask |= (1 << LayerMask.NameToLayer("CollectableLayer"));
		} else {
			mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("CollectableLayer"));
		}
	}
}
