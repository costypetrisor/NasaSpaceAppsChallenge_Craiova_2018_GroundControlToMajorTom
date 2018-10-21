using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverFollow : MonoBehaviour {

    // The target we are following
    [SerializeField]
    private Transform target;

    // The offset
    [SerializeField]
    private Vector3 offset;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {
        if (target != null) {
            transform.position = target.position + offset;
            transform.forward = target.forward;
        }
    }
}
