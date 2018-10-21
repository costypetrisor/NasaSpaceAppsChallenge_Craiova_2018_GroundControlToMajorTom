using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

/// This is supposed to be our common network manager
public class GameNetworkManager : NetworkManager {

    public override void OnClientConnect(NetworkConnection connection)
    {
		base.OnClientConnect(connection);

		RegisterMessageHandlers(client);
    }

    public override void OnClientDisconnect(NetworkConnection connection)
    {
		base.OnClientDisconnect(connection);
    }

	void RegisterMessageHandlers(NetworkClient networkClient) {
		var missionGO = GameObject.Find("Mission");
		var comms = missionGO.GetComponent<Comms>();
		networkClient.RegisterHandler(NetworkMsgIds.CommsAudioMessage, comms.OnMsgCommsAudioMessage);
	}
}
