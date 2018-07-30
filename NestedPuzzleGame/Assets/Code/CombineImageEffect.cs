using UnityEngine;

[ExecuteInEditMode]
public class CombineImageEffect : MonoBehaviour {
	public Material EffectMaterial;

	[Range(0,50)]
	public int Iterations;
	[Range(0,4)]
	public int DownRes;
	void OnRenderImage(RenderTexture src, RenderTexture dst) {
		Graphics.Blit (src, dst, EffectMaterial);
	}
}
