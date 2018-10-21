using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;


public class FlightDirector_Mission : MonoBehaviour {

	private NetworkManager _networkManager = null;
	private NetworkClient _lastNetworkClient = null;

	// Use this for initialization
	void Start () {
		_networkManager = GetComponent<NetworkManager>() as NetworkManager;
	}
	
	// Update is called once per frame
	void Update () {
		// if (_networkManager == null) {
		// 	_networkManager = GetComponent<NetworkManager>() as NetworkManager;
		// } else {
		// 	if (_networkManager.client != null && _networkManager.client != _lastNetworkClient) {
		// 		_lastNetworkClient = _networkManager.client;
		// 		RegisterMessageHandlers(_lastNetworkClient);
		// 	}
		// }
	}

	public void RegisterMessageHandlers(NetworkClient networkClient) {
		networkClient.RegisterHandler(NetworkMsgIds.GenericJson, OnMsgGenericJson);
		networkClient.RegisterHandler(NetworkMsgIds.NamedLocationUpdate, OnMsgNamedLocationUpdate);

		// networkClient.RegisterHandler(NetworkMsgIds.CommsAudioMessage, OnMsgCommsAudioMessage);
	}

	void OnMsgGenericJson(NetworkMessage msg) {
		StringMessage strMsg = msg.ReadMessage<StringMessage>();
	}

	public class NamedLocationUpdateMsg : MessageBase {
		public string name;
	};

	void OnMsgNamedLocationUpdate(NetworkMessage msg) {
		NamedLocationUpdateMsg namedLocationUpdate = msg.ReadMessage<NamedLocationUpdateMsg>();

		Debug.Log(string.Format("Current location: {0}", namedLocationUpdate.name));

		var currentLocationGO = GameObject.Find("CurrentLocation");
		var currentLocationUIText = currentLocationGO.GetComponent(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
		currentLocationUIText.text = "Current location: " + namedLocationUpdate.name;
	}
}
