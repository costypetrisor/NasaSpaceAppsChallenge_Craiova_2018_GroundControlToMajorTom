using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class PopupNamedLocation : MonoBehaviour {
	List<string> currentlyActiveNamedLocations = new List<string>();

	GameObject _rover = null;
	GameObject _coordinatesUITextGO = null;

	// Use this for initialization
	void Start () {
		_coordinatesUITextGO = GameObject.Find("Coordinates");
	}
	
	// Update is called once per frame
	void Update () {
		UpdateCoordinatesUI();
	}

	void SendNamedLocationUpdate(string locationName) {
		var networkManager = GetComponent<NetworkManager>() as NetworkManager;
		var namedLocationUpdateMsg = new FlightDirector_Mission.NamedLocationUpdateMsg();
		namedLocationUpdateMsg.name = locationName;
		NetworkServer.SendToAll(NetworkMsgIds.NamedLocationUpdate, namedLocationUpdateMsg);
	}

	void OnTriggerEnter(Collider collider) {
		// Debug.Log(string.Format(
		// 	"-- PopupNamedLocation.OnTriggerEnter with {0}   ActiveList: {1} {2}",
		// 	collider.gameObject.name, currentlyActiveNamedLocations.Count, string.Join(", ", currentlyActiveNamedLocations.ToArray())
		// ));
		if (collider.CompareTag("NamedLocation")) {
			var namedLocation = collider.gameObject;

			GameObject canvas = GameObject.Find("Canvas");
			var namedLocationSpriteTransform = canvas.transform.Find("NamedLocationSprite");
			GameObject namedLocationSprite = null;
			if (namedLocationSpriteTransform == null) {
				var namedLocationSpritePrefab = Resources.Load("UI/NamedLocationSprite");
				namedLocationSprite = Instantiate(namedLocationSpritePrefab, canvas.transform) as GameObject;
				namedLocationSprite.name = "NamedLocationSprite";
			} else {
				namedLocationSprite = namedLocationSpriteTransform.gameObject;
			}

			var textComponent = namedLocationSprite.GetComponent(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
			textComponent.text = namedLocation.name;

			currentlyActiveNamedLocations.Add(namedLocation.name);

			SendNamedLocationUpdate(namedLocation.name);
		}
	}

	void OnTriggerExit(Collider collider) {
		// Debug.Log(string.Format("-- PopupNamedLocation.OnTriggerExit with {0}", collider.gameObject.name));
		if (collider.CompareTag("NamedLocation")) {
			var namedLocation = collider.gameObject;

			currentlyActiveNamedLocations.Remove(namedLocation.name);

			GameObject canvas = GameObject.Find("Canvas");
			var namedLocationSpriteTransform = canvas.transform.Find("NamedLocationSprite");
			if (namedLocationSpriteTransform != null) {
				var namedLocationSprite = namedLocationSpriteTransform.gameObject;

				var textComponent = namedLocationSprite.GetComponent(typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;

				// Debug.Log(string.Format(
				// 	"-- -- PopupNamedLocation.OnTriggerExit  collidedWith: {0}  currentTextComponent = {1} {2}   currentlyActiveNamedLocations= {3} {4}",
				// 	collider.gameObject.name, textComponent, textComponent != null ? textComponent.text : "<unknown",
				// 	currentlyActiveNamedLocations.Count, string.Join(", ", currentlyActiveNamedLocations.ToArray())
				// ));

				if (currentlyActiveNamedLocations.Count == 0) {
					Destroy(textComponent.gameObject);

					SendNamedLocationUpdate("");
				}
			}
		}
	}

	public GameObject GetRover() {
		if (_rover == null) {
			_rover = GameObject.Find("carRoot(Clone)");
		}
		return _rover;
	}

	void UpdateCoordinatesUI() {
		var rover = GetRover();
		var position = rover.transform.position;
		_coordinatesUITextGO.GetComponent<UnityEngine.UI.Text>().text = string.Format(
			"Coordinates: {0:000.00}, {1:000.00}", position.x, position.z);
	}
}
