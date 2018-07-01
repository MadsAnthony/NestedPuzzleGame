using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundDefinition {

	public Sound.SoundData sound;
	public List<Sound.SoundData> Additional = new List <Sound.SoundData>();

	public void Play() {
		if (Additional.Count == 0) {
			Play (sound);
		} else {
			Play (Additional [Random.Range (0, Additional.Count)]);
		}
	}

	public void Play(Sound.SoundData sound) {
		AudioSource audio = Director.Instance.GetComponent<AudioSource> ();
		audio.PlayOneShot (sound.Clip,sound.Volume);
	}
}
