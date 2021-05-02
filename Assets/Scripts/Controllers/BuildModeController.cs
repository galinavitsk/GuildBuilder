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
	String buildModeTile;

	// Start is called before the first frame update
	void Start () {

	}

	public void Add_Job_Build_Wall(Vector3Int tilePos, string buildModeObjectType){
Job j = new Job (tilePos, buildModeObjectType, (theJob) => Build_Wall (theJob.tilePos, theJob.objectType)); //Create job, pass the method that needs to be run once the job is complete
							WorldController.Instance.World.jobQueue.Enqueue (j);

	}

	// Update is called once per frame
	public void DoBuild (int start_x, int start_y, int end_x, int end_y, string buildModeIsObject, string buildModeObjectType, string floorType) {
		Vector3 pos = Input.mousePosition;
		pos = pos.Round (1);
		Tilemap tilemapFoundation = WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ();
		if (buildModeIsObject == "ground") {
			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, y, 0));
					if (tilePos.x < WorldController.Instance.World.Width && tilePos.y < WorldController.Instance.World.Height && tilePos.x >= 0 && tilePos.y >= 0) {

						TileBase tile = Resources.Load<RuleTile> ("Images/Landscape/" + buildModeObjectType);
						WorldController.Instance.tilemapLandscape.SetTile (tilePos, tile);
					}
				}
			}
		} else if (buildModeIsObject == "WallOnly") {

			if (buildModeObjectType.Contains ("Wall") == true) {
				for (int x = start_x; x <= end_x; x++) {
					for (int y = start_y; y <= end_y; y++) {

						Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, y, 1));
						if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos.x, tilePos.y) == false ||
							WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos) == true) {
							Debug.LogError ("Can't place a " + buildModeObjectType + " here");

						} else {
							Add_Job_Build_Wall(tilePos,buildModeObjectType);

						}
					}
				}
			}
			if (buildModeObjectType.Contains ("Door") == true || buildModeObjectType.Contains ("Window") == true) {
				Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (end_x, end_y, 2));
				if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos.x, tilePos.y) == false ||
					WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos) == true) {
						if(WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos) == true && WorldController.Instance.World.jobQueue.JobPositonsWallCheck(tilePos)){
							Debug.Log("ADDING DOOR JOB OVER FUTURE WALL");
							WorldController.Instance.World.jobQueue.JobQueRemoveAt(tilePos);
							Job j = new Job (tilePos, buildModeObjectType, (theJob) => Build_DoorWindow (theJob.tilePos, theJob.objectType)); //Create job, pass the method that needs to be run once the job is complete
							WorldController.Instance.World.jobQueue.Enqueue (j);
						}
						else{
							Debug.LogError ("!!!!BuildModeController::Can't place a " + buildModeObjectType + " here");
						}
				} else {
					Debug.Log("ADDING DOOR JOB OVER CURRENT WALL");
					Job j = new Job (tilePos, buildModeObjectType, (theJob) => Build_DoorWindow (theJob.tilePos, theJob.objectType)); //Create job, pass the method that needs to be run once the job is complete
					WorldController.Instance.World.jobQueue.Enqueue (j);

				}
			}

		} else if (buildModeIsObject == "Room") {
			for (int x = start_x; x <= end_x; x++) {
				Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, start_y, 0));//Goes along bottom wall
				if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos.x, tilePos.y) == false ||
					WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos) == true) {
					Debug.LogError ("BuildModeController::Can't place a " + buildModeObjectType + " here: START_Y");

				} else {
							Add_Job_Build_Wall(tilePos,buildModeObjectType);

				}
			}
			for (int y = start_y; y <= end_y; y++) {
				Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (start_x, y, 0));
				if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos.x, tilePos.y) == false ||
					WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos) == true) {
					Debug.LogError ("BuildModeController::Can't place a " + buildModeObjectType + " here: START_X");

				} else {
							Add_Job_Build_Wall(tilePos,buildModeObjectType);

				}
			}

			for (int x = start_x; x <= end_x; x++) {
				Vector3Int tilePos2 = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, end_y, 0));//Goes along top wall
				if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos2.x, tilePos2.y) == false ||
					WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos2) == true) {
					Debug.LogError ("BuildModeController::Can't place a " + buildModeObjectType + " here: END_Y");

				} else {
							Add_Job_Build_Wall(tilePos2,buildModeObjectType);
				}
			}
			
			for (int y = start_y; y <= end_y; y++) {
				Vector3Int tilePos2 = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (end_x, y, 0));
				if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos2.x, tilePos2.y) == false ||
					WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos2) == true) {
					Debug.LogError ("BuildModeController::Can't place a " + buildModeObjectType + " here:END_X");
				} else {
							Add_Job_Build_Wall(tilePos2,buildModeObjectType);

				}
			}
			for (int x = start_x + 1; x <= end_x - 1; x++) {
				for (int y = start_y + 1; y <= end_y - 1; y++) {
					Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, y, 0));
					if (y == 1 || x == 1 || x == WorldController.Instance.World.Width - 2 || y == WorldController.Instance.World.Height - 2) { Build_Wall (tilePos, "Wall"); } else {

						if (WorldController.Instance.World.IsInstalledObjectPlacementValid (buildModeObjectType, tilePos.x, tilePos.y) == false ||
							WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos) == true) {
							Debug.LogError ("Can't place a " + buildModeObjectType + " here");

						} else {

							Job j = new Job (tilePos, "Floor_" + floorType, (theJob) => Build_Floor (theJob.tilePos, theJob.objectType)); //Create job, pass the method that needs to be run once the job is complete
							WorldController.Instance.World.jobQueue.Enqueue (j);

						}

					}
				}
			}
		}
	}

	public void DoBuldoze (int start_x, int start_y, int end_x, int end_y, string buildModeIsObject, string buildModeObjectType) {
		Vector3 pos = Input.mousePosition;
		pos = pos.Round (1);
		Tilemap tilemapFoundation = WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ();
		Tilemap tilemapLandscape = WorldController.Instance.tilemapLandscape.GetComponent<Tilemap> ();

		if (buildModeIsObject == "ground") {
			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					Vector3Int tilePos = WorldController.Instance.tilemapLandscape.WorldToCell (new Vector3Int (x, y, 0));
					if (tilePos.x < WorldController.Instance.World.Width && tilePos.y < WorldController.Instance.World.Height && tilePos.x >= 0 && tilePos.y >= 0) {

						TileBase tile = Resources.Load<RuleTile> ("Images/Landscape/Grass_Generic");
						WorldController.Instance.tilemapLandscape.SetTile (tilePos, tile);

					}
				}
			}
		} else if (buildModeIsObject == "WallOnly" || buildModeIsObject == "Room") {
			TileBase tile = Resources.Load<RuleTile> ("Images/Landscape/Grass_Generic");
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
	public void Build_Test_Room () {
		int l = WorldController.Instance.World.Width / 2 - 5;
		int b = WorldController.Instance.World.Height / 2 - 5;
		for (int x = l - 5; x < l + 15; x++) {
			for (int y = b - 5; y < b + 15; y++) {
				if (x == l || x == (l + 9) || y == b || y == (b + 9)) {
					if (x != (l + 9) && y != (b + 4)) {
						if (WorldController.Instance.World.IsInstalledObjectPlacementValid ("Wall_Wood", x, y)) { Build_Wall (new Vector3Int (x, y, 0), "Wall_Wood"); }
					}
				}
			}
		}
		//Path_TileGraph tileGraph = new Path_TileGraph(WorldController.Instance.World);
	}
	void Build_DoorWindow (Vector3Int tilePos, string objectType) {
		WorldController.Instance.World.jobQueue.JobPositonsRemove (tilePos);
		WorldController.Instance.PlaceInstalledObject (tilePos, objectType);
		if (WorldController.Instance.tilemapLandscape.GetSprite (tilePos) == null ||
			WorldController.Instance.tilemapLandscape.GetSprite (tilePos).name.ToString ().Contains ("Floor_") == false) {
			TileBase tile = Resources.Load<AdvancedRuleTile> ("Images/InstalledObjects/Floor_Wood_01");
			WorldController.Instance.tilemapLandscape.SetTile (tilePos, tile);
		}
		Debug.Log("DOOR BUILT AT:"+tilePos.x+"_"+tilePos.y);
	}
	void Build_Floor (Vector3Int tilePos, string floor) {

		TileBase tile = Resources.Load<AdvancedRuleTile> ("Images/InstalledObjects/" + floor);
		if (WorldController.Instance.World.jobQueue.JobPositonsContains (tilePos)) {
			WorldController.Instance.World.jobQueue.JobPositonsRemove (tilePos);
		}
		WorldController.Instance.tilemapLandscape.SetTile (tilePos, tile);
		WorldController.Instance.tilemapFoundation.SetTile (tilePos, null);
		InstalledObject floorobject = WorldController.Instance.World.InstalledObjectPrototypes[floor].Clone ();
		WorldController.Instance.World.foundationGameMap.Add (tilePos, floorobject);
	}
	void Build_Wall (Vector3Int tilePos, string objectType) {
		WorldController.Instance.World.jobQueue.JobPositonsRemove (tilePos);
		TileBase tile = Resources.Load<RuleTile> ("Images/Landscape/Dirt");
		Tilemap tilemapFoundation = WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ();
		Tilemap tilemapLandscape = WorldController.Instance.tilemapLandscape.GetComponent<Tilemap> ();
		if ((tilemapFoundation.GetSprite (tilePos) == null)) {
			WorldController.Instance.PlaceInstalledObject (tilePos, objectType);
			tilemapLandscape.SetTile (tilePos, tile);
		} else {
			Debug.LogError ("Trying to place an object where one already exists");
		}

	}

}