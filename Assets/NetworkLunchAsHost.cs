using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class NetworkLunchAsHost : MonoBehaviour {

    NetworkManager networkManager;
    
    // Use this for initialization
    void Start () {
        networkManager = GetComponent<NetworkManager>();
        networkManager.StartHost();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
