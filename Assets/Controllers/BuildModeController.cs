using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class BuildModeController : MonoBehaviour {
	String buildModeTile = "Generic";
	string buildModeObjectType;
	String buildModeIsObject = "ground";
	String floorType = "Wood";

	// Start is called before the first frame update
	void Start () {

	}

	// Update is called once per frame
	public void DoBuild (int start_x, int start_y, int end_x, int end_y) {
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
						if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos.x, tilePos.y) == false ||
							WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos) == true) {
							Debug.LogError ("Can't place a " + buildModeObjectType + " here");

						} else {

							Job j = new Job (tilePos, buildModeObjectType, (theJob) => Build_Wall (tilePos)); //Create job, pass the method that needs to be run once the job is complete
							WorldController.Instance.World.jobQueue.Enqueue (j);

						}
					}
				}
			}
			if (buildModeObjectType.Contains ("Door") == true || buildModeObjectType.Contains ("Window") == true) {
				Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (end_x, end_y, 0));

				if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos.x, tilePos.y) == false ||
					WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos) == true) {
					Debug.LogError ("Can't place a " + buildModeObjectType + " here");

				} else {

					Job j = new Job (tilePos, buildModeObjectType, (theJob) => Build_DoorWindow (tilePos)); //Create job, pass the method that needs to be run once the job is complete
					WorldController.Instance.World.jobQueue.Enqueue (j);

				}
			}

		} else if (buildModeIsObject == "Room") {
			for (int x = start_x; x <= end_x; x++) {
				Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, start_y, 0));

				if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos.x, tilePos.y) == false || WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos) == true) {
					Debug.LogError ("Can't place a " + buildModeObjectType + " here");

				} else {

					Job j = new Job (tilePos, buildModeObjectType, (theJob) => Build_Wall (tilePos)); //Create job, pass the method that needs to be run once the job is complete
					WorldController.Instance.World.jobQueue.Enqueue (j);

				}
				Vector3Int tilePos2 = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, end_y, 0));
				if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos2.x, tilePos2.y) == false || WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos2) == true) {
					Debug.LogError ("Can't place a " + buildModeObjectType + " here");

				} else {
					Job j = new Job (tilePos2, buildModeObjectType, (theJob) => Build_Wall (tilePos2)); //Create job, pass the method that needs to be run once the job is complete
					WorldController.Instance.World.jobQueue.Enqueue (j);
				}
			}
			for (int y = start_y; y <= end_y; y++) {
				Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (start_x, y, 0));
				if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos.x, tilePos.y) == false || WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos) == true) {
					Debug.LogError ("Can't place a " + buildModeObjectType + " here");

				} else {

					Job j = new Job (tilePos, buildModeObjectType, (theJob) => Build_Wall (tilePos)); //Create job, pass the method that needs to be run once the job is complete
					WorldController.Instance.World.jobQueue.Enqueue (j);

				}
				Vector3Int tilePos2 = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (end_x, y, 0));
				if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos2.x, tilePos2.y) == false || WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos2) == true) {
					Debug.LogError ("Can't place a " + buildModeObjectType + " here");

				} else {

					Job j = new Job (tilePos2, buildModeObjectType, (theJob) => Build_Wall (tilePos2)); //Create job, pass the method that needs to be run once the job is complete
					WorldController.Instance.World.jobQueue.Enqueue (j);

				}
			}
			for (int x = start_x + 1; x <= end_x - 1; x++) {
				for (int y = start_y + 1; y <= end_y - 1; y++) {
					Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, y, 0));
					CustomTileBase tile = (CustomTileBase) ScriptableObject.CreateInstance (typeof (CustomTileBase));
					tile.sprite = GameObject.FindObjectOfType<InstalledObjectSpriteController> ().installedObjectSprites["Floor_" + floorType];
					if (y == 1 || x == 1 || x == WorldController.Instance.World.Width - 2 || y == WorldController.Instance.World.Height - 2) { Build_Wall (tilePos); } else {

						if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos.x, tilePos.y) == false || WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos) == true) {
							Debug.LogError ("Can't place a " + buildModeObjectType + " here");

						} else {

							Job j = new Job (tilePos, tile.sprite.name.ToString (), (theJob) => Build_Floor (tilePos, tile)); //Create job, pass the method that needs to be run once the job is complete
							WorldController.Instance.World.jobQueue.Enqueue (j);

						}

					}
				}
			}
		}
	}

	public void DoBuldoze (int start_x, int start_y, int end_x, int end_y) {
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
		tilemapLandscape.ClearAllTiles ();
		WorldController.Instance.World.RandomizeTiles ();
	}

	public void Build_Test_Room () {
		buildModeObjectType = "Wall";
		int l = WorldController.Instance.World.Width / 2 - 5;
		int b = WorldController.Instance.World.Height / 2 - 5;
		for (int x = l - 5; x < l + 15; x++) {
			for (int y = b - 5; y < b + 15; y++) {
				if (x == l || x == (l + 9) || y == b || y == (b + 9)) {
					if (x != (l + 9) && y != (b + 4)) { Build_Wall (new Vector3Int (x, y, 0)); }
				}
			}
		}
	}
	void Build_DoorWindow (Vector3Int tilePos) {
		WorldController.Instance.World.jobQueue.JobPositonsRemove (tilePos);
		WorldController.Instance.PlaceInstalledObject (tilePos, buildModeObjectType);
		if (WorldController.Instance.tilemapLandscape.GetSprite (tilePos).name.ToString ().Contains ("Floor_") == false) {
			CustomTileBase tile = (CustomTileBase) ScriptableObject.CreateInstance (typeof (CustomTileBase));
			tile.sprite = GameObject.FindObjectOfType<InstalledObjectSpriteController> ().installedObjectSprites["Floor_Wood"];
			WorldController.Instance.tilemapLandscape.SetTile (tilePos, tile);
		}
	}
	void Build_Floor (Vector3Int tilePos, TileBase tile) {
		WorldController.Instance.World.jobQueue.JobPositonsRemove (tilePos);
		WorldController.Instance.tilemapLandscape.SetTile (tilePos, tile);
		WorldController.Instance.tilemapFoundation.SetTile (tilePos, null);
	}

	void Build_Wall (Vector3Int tilePos) {
		WorldController.Instance.World.jobQueue.JobPositonsRemove (tilePos);
		TileBase tile = Resources.Load<RuleTile> ("Images/Landscape/Dirt");
		Tilemap tilemapFoundation = WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ();
		Tilemap tilemapLandscape = WorldController.Instance.tilemapLandscape.GetComponent<Tilemap> ();
		if ((tilemapFoundation.GetSprite (tilePos) == null)) {
			WorldController.Instance.PlaceInstalledObject (tilePos, buildModeObjectType);
			tilemapLandscape.SetTile (tilePos, tile);

		} else {
			Debug.LogError ("Trying to place an object where one already exists");
		}

	}

}