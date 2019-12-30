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

		if (Input.GetMouseButtonDown (0) || Input.GetMouseButtonDown (1)) {
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

		if (Input.GetMouseButtonUp (0)) { //Stop Drag
			Vector3 pos = Input.mousePosition;
			pos = pos.Round (1);
			Tilemap tilemapFoundation = WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ();

			if (buildModeIsObject == "ground") {
				for (int x = start_x; x <= end_x; x++) {
					for (int y = start_y; y <= end_y; y++) {
						Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, y, 0));
						if (tilePos.x < WorldController.Instance.World.Width && tilePos.y < WorldController.Instance.World.Height && tilePos.x >= 0 && tilePos.y >= 0) {

							TileBase tile = Resources.Load<RuleTile> ("Images/Landscape/" + buildModeTile);
							WorldController.Instance.tilemapLandscape.SetTile (tilePos, tile);
						}
					}
				}
			} else if (buildModeIsObject == "WallOnly") {

				if (buildModeObjectType.Contains ("Wall") == true) {
					for (int x = start_x; x <= end_x; x++) {
						for (int y = start_y; y <= end_y; y++) {
							Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, y, 0));
							if ((tilemapFoundation.GetSprite (tilePos) == null) ||
								(tilemapFoundation.GetSprite (tilePos).name.ToString ().Contains ("Floor_")) ||
								(tilemapFoundation.GetTile (tilePos).name.ToString ().Contains ("Wall"))) {
								WorldController.Instance.PlaceInstalledObject (tilePos, buildModeObjectType);

							} else {
								Debug.LogError ("Trying to place an object where one already exists");
							}
						}
					}
				}
				if (buildModeObjectType.Contains ("Door") == true || buildModeObjectType.Contains ("Window") == true) {
					Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (end_x, end_y, 0));
					WorldController.Instance.PlaceInstalledObject (tilePos, buildModeObjectType);
					if (WorldController.Instance.tilemapLandscape.GetSprite (tilePos).name.ToString ().Contains ("Floor_") == false) {
						CustomTileBase tile = (CustomTileBase) ScriptableObject.CreateInstance (typeof (CustomTileBase));
						tile.sprite = WorldController.Instance.installedObjectSprites["Floor_Wood"];
						WorldController.Instance.tilemapLandscape.SetTile (tilePos, tile);
					}
				}

			} else if (buildModeIsObject == "Room") {
				for (int x = start_x; x <= end_x; x++) {
					Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, start_y, 0));
					WorldController.Instance.PlaceInstalledObject (tilePos, buildModeObjectType);
					tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, end_y, 0));
					WorldController.Instance.PlaceInstalledObject (tilePos, buildModeObjectType);
				}
				for (int y = start_y; y <= end_y; y++) {
					Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (start_x, y, 0));
					WorldController.Instance.PlaceInstalledObject (tilePos, buildModeObjectType);
					tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (end_x, y, 0));
					WorldController.Instance.PlaceInstalledObject (tilePos, buildModeObjectType);
				}
				for (int x = start_x + 1; x <= end_x - 1; x++) {
					for (int y = start_y + 1; y <= end_y - 1; y++) {
						Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, y, 0));
						CustomTileBase tile = (CustomTileBase) ScriptableObject.CreateInstance (typeof (CustomTileBase));
						tile.sprite = WorldController.Instance.installedObjectSprites["Floor_" + floorType];
						if (y == 1 || x == 1 || x == WorldController.Instance.World.Width - 2 || y == WorldController.Instance.World.Height - 2) { WorldController.Instance.PlaceInstalledObject (tilePos, buildModeObjectType); } else {
							WorldController.Instance.tilemapLandscape.SetTile (tilePos, tile);
							tilemapFoundation.SetTile (tilePos, null);
						}
					}
				}

			}

		}
		if (Input.GetMouseButtonUp (1)) { //BULDOZING 

			Vector3 pos = Input.mousePosition;
			pos = pos.Round (1);
			Tilemap tilemapFoundation = WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ();
			Tilemap tilemapLandscape = WorldController.Instance.tilemapLandscape.GetComponent<Tilemap> ();
			if (buildModeIsObject == "ground") {
				for (int x = start_x; x <= end_x; x++) {
					for (int y = start_y; y <= end_y; y++) {
						Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, y, 0));
						if (tilePos.x < WorldController.Instance.World.Width && tilePos.y < WorldController.Instance.World.Height && tilePos.x >= 0 && tilePos.y >= 0) {

							TileBase tile = Resources.Load<RuleTile> ("Images/Landscape/Generic");
							WorldController.Instance.tilemapLandscape.SetTile (tilePos, tile);
						}
					}
				}
			} else if (buildModeIsObject == "WallOnly" || buildModeIsObject == "Room") {
				TileBase tile = Resources.Load<RuleTile> ("Images/Landscape/Dirt");
				for (int x = start_x; x <= end_x; x++) {
					for (int y = start_y; y <= end_y; y++) {
						Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, y, 0));
						if ((tilemapFoundation.GetTile (tilePos) == null ||
								tilemapFoundation.GetTile (tilePos).name.ToString ().Contains ("Wall") == true ||
								tilemapFoundation.GetSprite (tilePos).name.ToString ().Contains ("Door") == true)) {
							tilemapFoundation.SetTile (tilePos, null);
							tilemapLandscape.SetTile (tilePos, tile);
							if ((tilemapLandscape.GetSprite (tilePos).name.ToString ().Contains ("Floor_")) == true) {

								tilemapLandscape.SetTile (tilePos, tile);
							}

						} else {
							Debug.LogError ("Trying to place an object where one already exists");
						}
					}
				}

			} else if (buildModeIsObject == "Furniture") {
				Debug.Log ("Placing Furniture");
			}

		}
	}

	public void OpenLandScapeMenu (string objectType) {

	}
	public void SetMode_BuildGround (string objectType) {
		buildModeIsObject = "ground";
		buildModeTile = objectType;
	}

	public void SetMode_BuildInstalledObject (string objectType) {
		buildModeIsObject = "WallOnly";
		buildModeObjectType = objectType;
	}
	public void SetMode_BuildRoom (string roomType) {
		String[] room = roomType.Split (',');
		buildModeIsObject = "Room";
		buildModeObjectType = room[0];
		floorType = room[1];
	}
	public void RandomizeAllLandscapeTiles () {
		Tilemap tilemapLandscape = WorldController.Instance.tilemapLandscape.GetComponent<Tilemap> ();
        tilemapLandscape.ClearAllTiles();
        WorldController.Instance.World.RandomizeTiles();
    }

}