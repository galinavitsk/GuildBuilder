using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class MouseController : MonoBehaviour {
	Vector3 lastFramePosition;
	Vector3 currFramePosition;
	Vector3 dragStartPosition;
	public GameObject circleCursorPrefab;
	public GameObject mainMenu;
	String buildModeTile = "Generic";
	string buildModeObjectType;
	List<GameObject> dragPreviewGameObjects;
	String buildModeIsObject = "ground";
	String floorType = "Wood";
	int start_x;
	int end_x;
	int start_y;
	int end_y;

	// Start is called before the first frame update
	void Start () {
		dragPreviewGameObjects = new List<GameObject> ();
	}

	// Update is called once per frame
	void Update () {
		currFramePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		currFramePosition.z = 0;

		UpdateDragging ();
		UpdateCameraMovement ();

	}

	void UpdateCameraMovement () {

		if (Input.GetMouseButton (2)) //Middle mouse button
		{
			Vector3 diff = lastFramePosition - currFramePosition;
			Camera.main.transform.Translate (diff);
		}

		lastFramePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		lastFramePosition.z = 0;
		Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis ("Mouse ScrollWheel");
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 3f, 25f);

	}

	void UpdateDragging () {
		//Do Not Activate If mouse is over UI element
		if (EventSystem.current.IsPointerOverGameObject ()) {
			return;
		}

		if (Input.GetMouseButtonDown (0) || Input.GetMouseButtonDown (1)) {
			dragStartPosition = currFramePosition;
		}

		start_x = Mathf.RoundToInt (dragStartPosition.x);
		end_x = Mathf.RoundToInt (currFramePosition.x);
		start_y = Mathf.RoundToInt (dragStartPosition.y);
		end_y = Mathf.RoundToInt (currFramePosition.y);

		// We may be dragging in the "wrong" direction, so flip things if needed.
		if (end_x < start_x) {
			int tmp = end_x;
			end_x = start_x;
			start_x = tmp;
		}
		if (end_y < start_y) {
			int tmp = end_y;
			end_y = start_y;
			start_y = tmp;
		}

		// Clean up old drag previews
		while (dragPreviewGameObjects.Count > 0) {
			GameObject go = dragPreviewGameObjects[0];
			dragPreviewGameObjects.RemoveAt (0);
			Destroy (go);
		}

		if (Input.GetMouseButton (0) || Input.GetMouseButton (1)) {
			// Display a preview of the drag area
			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					TileBase t = WorldController.Instance.tilemapLandscape.GetTile (new Vector3Int (x, y, 0));
					if (t != null) { //LandTile Changing Mode
						GameObject go = (GameObject) Instantiate (circleCursorPrefab, new Vector3 (x, y, 0), Quaternion.identity);
						dragPreviewGameObjects.Add (go);
					}
				}
			}
		}

		if (Input.GetMouseButtonUp (0)) { //Stop to Build
			GameObject.FindObjectOfType<BuildModeController> ().DoBuild (start_x, start_y, end_x, end_y);

		}
		if (Input.GetMouseButtonUp (1)) { //BULDOZING 
			GameObject.FindObjectOfType<BuildModeController> ().DoBuldoze (start_x, start_y, end_x, end_y);

		}

	}

	public void OpenLandScapeMenu (string objectType) {

	}
	public void RandomizeAllLandscapeTiles () {
		Tilemap tilemapLandscape = WorldController.Instance.tilemapLandscape.GetComponent<Tilemap> ();
		tilemapLandscape.ClearAllTiles ();
		WorldController.Instance.World.RandomizeTiles ();
	}

}