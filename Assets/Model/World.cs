﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World {
    Tile[, ] tiles;
    public Dictionary<string, InstalledObject> InstalledObjectPrototypes { get; protected set; }
    int width;
    int height;
    Action<InstalledObject> cbInstalledObjectCreated;
    //int iniChance = 50; //0-100
    int birthLimit = 4; //1-8
    int deathLimit = 4; //1-8
    //int numR = 5;

    public int Width {
        get {
            return width;
        }
    }
    public int Height {
        get {
            return height;
        }
    }
    //TODO: Create dedicated job queue class later on
   public Queue<Job> jobQueue;
    public List<Vector3Int> jobPositions;

    public World (int width = 20, int height = 20) {
        this.width = width;
        this.height = height;
        CreateInstalledObjectPrototypes ();
        jobQueue = new Queue<Job>();
        jobPositions = new List<Vector3Int>();
    }

    public void RandomizeTiles () {
        BoundsInt area = new BoundsInt (new Vector3Int (0, 0, 0), new Vector3Int (width, height, 1));
        TileBase tileGrass = Resources.Load<RuleTile> ("Images/Landscape/Grass");
        TileBase tileDirt = Resources.Load<RuleTile> ("Images/Landscape/Dirt");

        TileBase[] tileArray = new TileBase[area.size.x * area.size.y * area.size.z]; //create the tiles array
        Tilemap tilemapLandscape = WorldController.Instance.tilemapLandscape.GetComponent<Tilemap> ();
        WorldController.Instance.tilemapLandscape.GetComponent<Tilemap> ().ClearAllTiles ();
        WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ().ClearAllTiles ();
        //WorldController.Instance.tilemapFurniture.GetComponent<Tilemap> ().ClearAllTiles ();
        for (int index = 0; index < tileArray.Length; index++) {
            tileArray[index] = UnityEngine.Random.Range (0, 2) == 0 ? tileGrass : tileDirt;
        }
        tilemapLandscape.SetTilesBlock (area, tileArray); //Actually set the tiles onto the tilemapLandscape */
        /* 
                int[, ] tileGenMap = new int[width, height];
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        tileGenMap[x, y] = UnityEngine.Random.Range (1, 101) < iniChance ? 1 : 0; //1:Grass 0:Dirt
                    }
                }

                for (int i = 0; i < numR; i++) {
                    tileGenMap = genTilePos (tileGenMap);
                }
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        if (tileGenMap[x, y] == 1) {
                            tilemapLandscape.SetTile(new Vector3Int(x, y, 1), tileGrass);
                        } else {
                            tilemapLandscape.SetTile(new Vector3Int(x, y, 1), tileDirt);
                        }
                    }
                } */
    }
    public int[, ] genTilePos (int[, ] oldMap) {
        int[, ] newmap = new int[width, height];
        int neighb;
        BoundsInt myB = new BoundsInt (-1, -1, 0, 3, 3, 1);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                neighb = 0;
                foreach (var b in myB.allPositionsWithin) {
                    if (b.x == 0 && b.y == 0) continue;
                    if (x + b.x >= 0 && x + b.x < width && y + b.y >= 0 && y + b.y < height) {
                        neighb += oldMap[x + b.x, y + b.y];
                    } else {
                        neighb++;
                    }
                }
                if (oldMap[x, y] == 1) {
                    if (neighb < deathLimit) { newmap[x, y] = 0; } else {
                        newmap[x, y] = 1;
                    }
                }
                if (oldMap[x, y] == 0) {
                    if (neighb < birthLimit) { newmap[x, y] = 1; } else {
                        newmap[x, y] = 0;
                    }
                }
            }
        }
        return newmap;
    }

    void CreateInstalledObjectPrototypes () {
        InstalledObjectPrototypes = new Dictionary<string, InstalledObject> ();

        InstalledObjectPrototypes.Add ("Floor_Wood", InstalledObject.CreatePrototype ("Floor_Wood", "Floor_Wood", 1, 1, 1, true));
        InstalledObjectPrototypes.Add ("Wall", InstalledObject.CreatePrototype ("Wall", "Wall", 0, 1, 1, true));
        InstalledObjectPrototypes.Add ("Door", InstalledObject.CreatePrototype ("Door", "Door", 0, 1, 1, false));
    }

    public void RegisterInstalledObjectCreated (Action<InstalledObject> callbackfunc) {
        cbInstalledObjectCreated += callbackfunc;
    }

    public void UnregisterInstalledObjectCreated (Action<InstalledObject> callbackfunc) {
        cbInstalledObjectCreated -= callbackfunc;
    }

    public bool IsInstalledObjectPlacementValid(string objType, int x, int y){
        return InstalledObjectPrototypes[objType].funcPositionValidation(x, y);
    }
}