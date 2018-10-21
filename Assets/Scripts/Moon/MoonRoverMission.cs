using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoonRoverMission : MonoBehaviour {

	public KeyCode SceneResetKey = KeyCode.Joystick1Button1;

	GameObject _rover = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(SceneResetKey)) {
			// SceneManager.LoadScene(0);
			// Debug.Log(string.Format("After scene reload MainCamera={0} {0}", Camera.main, Camera.main.name));

			var rover = GetRover();
			rover.transform.position = new Vector3(0.0f, 5.0f, 0.0f);
		}
	}

	public GameObject GetRover() {
		if (_rover == null) {
			_rover = GameObject.Find("carRoot(Clone)");
		}
		return _rover;
	}

}
