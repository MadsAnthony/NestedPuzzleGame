using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound {
	
	[System.Serializable]
	public class SoundData {
		[SerializeField]
		public AudioClip Clip;
		[SerializeField]
		public float Volume = 1.0f;
		[SerializeField]
		public float Probability = 1.0f;
		[SerializeField]
		public bool Loop = false;
	}

}
