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
	Tile.TileType buildModeTile = Tile.TileType.Generic;
	string buildModeObjectType;
	List<GameObject> dragPreviewGameObjects;
	bool buildModeIsObject = false;

	// Start is called before the first frame update
	void Start () {
		dragPreviewGameObjects = new List<GameObject> ();
	}

	// Update is called once per frame
	void Update () {
		currFramePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		currFramePosition.z = 0;

		//UpdateCursor();
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

		if (Input.GetMouseButtonDown (0)) {
			dragStartPosition = currFramePosition;
		}

		int start_x = Mathf.RoundToInt (dragStartPosition.x);
		int end_x = Mathf.RoundToInt (currFramePosition.x);
		int start_y = Mathf.RoundToInt (dragStartPosition.y);
		int end_y = Mathf.RoundToInt (currFramePosition.y);

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

		if (Input.GetMouseButton (0)) {
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

		if (Input.GetMouseButtonUp (0)) { //Stop Drag
			Vector3 pos = Input.mousePosition;
			pos = pos.Round (1);

			if (buildModeIsObject == false) {
				for (int x = start_x; x <= end_x; x++) {
					for (int y = start_y; y <= end_y; y++) {
						Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, y, 0));
						if (tilePos.x < WorldController.Instance.World.Width && tilePos.y < WorldController.Instance.World.Height && tilePos.x >= 0 && tilePos.y >= 0) {

							if (buildModeTile == Tile.TileType.Dirt) {
								WorldController.Instance.tilemapLandscape.SetTile (tilePos, WorldController.Instance.tileDirt);
							}
							if (buildModeTile == Tile.TileType.Grass) {
								WorldController.Instance.tilemapLandscape.SetTile (tilePos, WorldController.Instance.tileGrass);
							}
						}
					}
				}
			} else if (buildModeIsObject == true) {
				Tilemap tilemapFurniture = WorldController.Instance.tilemapFurniture.GetComponent<Tilemap> ();
				for (int x = start_x; x <= end_x; x++) {
					for (int y = start_y; y <= end_y; y++) {
						Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, y, 0));
						if (tilemapFurniture.GetSprite (tilePos) == null) {
							if (tilePos.x < WorldController.Instance.World.Width && tilePos.y < WorldController.Instance.World.Height && tilePos.x >= 0 && tilePos.y >= 0) {
								WorldController.Instance.PlaceFurniture (tilePos, buildModeObjectType);
							}
						} else { Debug.LogError ("Trying to place an object where one already exists"); }
					}
				}
				//TileBase t = WorldController.Instance.tilemapLandscape.GetTile(tilePos);
				//WorldController.Instance.World.PlaceFurniture (buildModeObjectType, t);
			}
		}
	}

	public void SetMode_BuildDirt () {
		buildModeIsObject = false;
		buildModeTile = Tile.TileType.Dirt;
	}

	public void SetMode_BuildGrass () {
		buildModeIsObject = false;
		buildModeTile = Tile.TileType.Grass;

	}
	public void SetMode_BuildInstalledObject (string objectType) {
		buildModeIsObject = true;
		buildModeObjectType = objectType;
	}

}