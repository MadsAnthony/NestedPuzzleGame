using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIView : MonoBehaviour {
	
	void Start () {
		Director.UIManager.ActiveView = this;
		OnStart();
	}

	void OnDestroy() {
		OnRemoving();
	}

	protected virtual void OnStart() {
	}

	protected virtual void OnRemoving() {
	}
}
