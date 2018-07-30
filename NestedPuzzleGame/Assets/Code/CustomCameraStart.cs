using UnityEngine;

[ExecuteInEditMode]
public class CustomCameraStart : MonoBehaviour {
	public string GlobalTextureName;

	public void Start() {
		var renderTexture = new RenderTexture (Screen.width, Screen.height, 1);
		GetComponent<Camera> ().targetTexture = renderTexture;
		Shader.SetGlobalTexture (GlobalTextureName, renderTexture);
	}
}
