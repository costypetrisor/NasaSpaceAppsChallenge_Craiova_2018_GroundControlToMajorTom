using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;


/// This is supposed to be our FlightDirector network manager
public class FlightDirectorNetworkManager : GameNetworkManager {

	public override void OnClientConnect(NetworkConnection connection)
    {
		base.OnClientConnect(connection);

		var missionGO = GameObject.Find("Mission");
		var missionBehavior = missionGO.GetComponent<FlightDirector_Mission>();
		missionBehavior.RegisterMessageHandlers(client);
    }

	public virtual void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		base.OnServerAddPlayer(conn, playerControllerId);
		// var player = (GameObject)GameObject.Instantiate(base.playerPrefab, base.playerSpawnPos, Quaternion.identity);
		// NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

		var rover = GameObject.Find("carRoot(Clone)");
		if (rover != null) {
			AudioListener[] audioListeners = rover.GetComponentsInChildren<AudioListener>();
			foreach (var audioListener in audioListeners) {
				audioListener.enabled = false;
			}
		}
	}
}
