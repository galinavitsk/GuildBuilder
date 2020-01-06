using System;
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
    Action<Character> cbCharacterCreated;
    //int iniChance = 50; //0-100
    int birthLimit = 4; //1-8
    int deathLimit = 4; //1-8
    //int numR = 5;
    public List<Character> characters;
    public Dictionary<Vector3Int, InstalledObject> objectsGameMap;

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
    public JobQueue jobQueue;
    public Path_TileGraph tileGraph;

    public World (int width = 20, int height = 20) {
        this.width = width;
        this.height = height;
        RandomizeTiles ();
        CreateInstalledObjectPrototypes ();
        jobQueue = new JobQueue ();
        characters = new List<Character> ();
        objectsGameMap = new Dictionary<Vector3Int, InstalledObject> ();

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
        tilemapLandscape.SetTilesBlock (area, tileArray); //Actually set the tiles onto the tilemapLandscape 

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

    public void Update (float deltaTime) {
        foreach (Character c in characters) {
            c.Update (deltaTime);
        }
        foreach (Vector3Int tilePos in objectsGameMap.Keys)
        {
            objectsGameMap[tilePos].Update(deltaTime);
        }
    }

    public Character CreateCharacter (Vector3Int tile, float speed, string name, float buildtime) {
        Character c = new Character (tile, speed, name, buildtime);
        characters.Add (c);
        if (cbCharacterCreated != null) { cbCharacterCreated (c); } else { //Debug.Log("cbCharacterCreates is null"); 
        }
        return c;
    }
    void CreateInstalledObjectPrototypes () {
        InstalledObjectPrototypes = new Dictionary<string, InstalledObject> ();
        //ObjectType, Sprite, movementCost, width, height, linkstoneighbor
        InstalledObjectPrototypes.Add ("Floor_Wood", new InstalledObject ("Floor_Wood", "Floor_Wood", 1, 1, 1, true));
        InstalledObjectPrototypes.Add ("Wall", new InstalledObject ("Wall", "Wall", 0, 1, 1, true));
        InstalledObjectPrototypes.Add ("Door",new  InstalledObject ("Door", "Door", 10, 1, 1, false));
        InstalledObjectPrototypes["Door"].installedObjectParamenters["openess"] = 0;
        InstalledObjectPrototypes["Door"].updateActions += InstalledObjectActions.Door_UpdateAction;
    }

    public void RegisterInstalledObjectCreated (Action<InstalledObject> callbackfunc) {
        cbInstalledObjectCreated += callbackfunc;
    }

    public void UnregisterInstalledObjectCreated (Action<InstalledObject> callbackfunc) {
        cbInstalledObjectCreated -= callbackfunc;
    }

    public void RegisterCharacterCreated (Action<Character> callbackfunc) {
        // Debug.Log("RegisterCharacterCreated");
        cbCharacterCreated += callbackfunc;
    }

    public void UnregisterCharacterCreated (Action<Character> callbackfunc) {
        cbCharacterCreated -= callbackfunc;
    }

    public bool IsInstalledObjectPlacementValid (string objType, int x, int y) {
        return InstalledObjectPrototypes[objType].funcPositionValidation (x, y);
    }

    public void InvalidateTileGraph () {
        //called whenever a change to the world changes the pathfinding info
        tileGraph = null;
    }
    public Vector3Int[] GetNeighbors (Vector3Int tile, bool diagOkay = false) {
        //is diagonal movement okay? is clipping corners okay?
        List<Vector3Int> ns = new List<Vector3Int> ();
        Vector3Int n;
        n = new Vector3Int (tile.x, tile.y + 1, 0);
        ns.Add (n); //N
        n = new Vector3Int (tile.x + 1, tile.y, 0);
        ns.Add (n); //E
        n = new Vector3Int (tile.x, tile.y - 1, 0);
        ns.Add (n); //S
        n = new Vector3Int (tile.x - 1, tile.y, 0);
        ns.Add (n); //W

        if (diagOkay == true) {
            n = new Vector3Int (tile.x + 1, tile.y + 1, 0);
            ns.Add (n); //NE
            n = new Vector3Int (tile.x + 1, tile.y - 1, 0);
            ns.Add (n); //SE
            n = new Vector3Int (tile.x - 1, tile.y - 1, 0);
            ns.Add (n); //SW
            n = new Vector3Int (tile.x - 1, tile.y + 1, 0);
            ns.Add (n); //NW
        }
        return ns.ToArray ();
    }

}