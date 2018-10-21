using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotOverview2D_CameraFollow : MonoBehaviour {

	private GameObject _rover = null;
	private GameObject RobotOverview2D = null;

	// Use this for initialization
	void Start () {
		RobotOverview2D = GameObject.Find("RobotOverview2D");
	}
	
	// Update is called once per frame
	void Update () {
		GameObject rover = GetRover();
		if (rover) {
			RobotOverview2D.transform.position = new Vector3(rover.transform.position.x, RobotOverview2D.transform.position.y, rover.transform.position.z);
		}
	}

	public GameObject GetRover() {
		if (_rover == null) {
			_rover = GameObject.Find("carRoot(Clone)");
			if (_rover != null) {
				Debug.Log(string.Format("Found Rover {0}", _rover));
			}
		}
		return _rover;
	}
}
