using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TerrainLoader : MonoBehaviour {

	private GameObject _rover = null;
	private GameObject _globalGround = null;

	// Use this for initialization
	void Start () {
		_globalGround = GameObject.Find("Ground");
	}
	
	// Update is called once per frame
	void Update () {
		GameObject rover = GetRover();
		if (false && rover) {  // disabled for now until we get back to work on this

			var allTerrains = _globalGround.GetComponentsInChildren(typeof(Terrain));
			// Debug.Log(string.Format("_globalGround: {0}  allTerrains.size: {1}", _globalGround, allTerrains.Length));

			Component terrainContainingRover = FindTerrainHoldingRover(rover, allTerrains);
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

	static Component FindTerrainHoldingRover(GameObject rover, Component[] allTerrains) {
		var roverWorldPosition = rover.transform.position;

		foreach(var terrain in allTerrains) {
			var terrainWorldPosition = terrain.transform.position;
			var meshRenderer = terrain.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
			var meshCollider = terrain.GetComponent(typeof(MeshCollider)) as MeshCollider;
			var meshBoundsMin = meshCollider.bounds.min;
			var meshBoundsMax = meshCollider.bounds.max;

			var terrainDebugInfo = new StringBuilder();
			bool containedX = meshBoundsMin.x <= roverWorldPosition.x && roverWorldPosition.x <= meshBoundsMax.x;
			bool containedZ = meshBoundsMin.z <= roverWorldPosition.z && roverWorldPosition.z <= meshBoundsMax.z;
			bool contained = containedX && containedZ;
			// Debug.Log(string.Format(
			// 	"Terrain {0} has {1} bounds: {2}   roverWorldPosition: {3}   contained: {4}",
			// 	terrain.name, meshCollider.bounds.size, meshCollider.bounds, roverWorldPosition, contained));

			return terrain;
		}
		return null;
	}
}
