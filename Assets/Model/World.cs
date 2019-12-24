using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World {
    LandTile[, ] tiles;
    Dictionary<string, Furniture> furniturePrototypes;
    int width;
    int height;
    Action<Furniture> cbFurnitureCreated;

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

    public World (int width = 10, int height = 10) {
        this.width = width;
        this.height = height;
        tiles = new LandTile[width, height];
        //Creates all the tiles once the world size is created
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                tiles[x, y] = new LandTile (this, x, y);
            }
        }
        Debug.Log ("World created with " + (width * height) + " tiles.");
        CreateInstalledObjectPrototypes ();
    }

    void CreateInstalledObjectPrototypes () {
        furniturePrototypes = new Dictionary<string, Furniture> ();
        furniturePrototypes.Add ("Wall", Furniture.CreatePrototype ("Wall", 0, 1, 1, true));
    }

    public void RandomizeTiles () {

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                switch (UnityEngine.Random.Range (0, 3)) {
                    case 0:
                        tiles[x, y].Type = LandTile.TileType.FullDirt;
                        break;
                    case 1:
                        tiles[x, y].Type = LandTile.TileType.FullGrass;
                        break;
                    default:
                        tiles[x, y].Type = LandTile.TileType.LowGrass;
                        break;
                }
                if (x != 0 || y != 0) {
                    //tiles[x, y].Type = CheckNeighbors (x, y);
                }
            }
        }
    }
    public LandTile GetTileAt (int x, int y) {
        if (x > width || x < 0) {
            Debug.LogError ("LandTile (" + x + "," + y + ") is out of range");
            return null;
        }
        return tiles[x, y];
    }

    public void PlaceInstalledObject (string objectType, LandTile t) {
        //TODO: Function assumes 1x1 tiles, and no rotation--Change this
        if (furniturePrototypes.ContainsKey (objectType) == false) {
            Debug.LogError ("furniturePrototypes doesn't contain a poroto for the key:" + objectType);
            return;
        }
        Furniture obj = Furniture.PlaceInstance (furniturePrototypes[objectType], t);
        if (obj == null) {
            //failed to place object
            return;
        }
        if (cbFurnitureCreated != null) {
            cbFurnitureCreated (obj);
        }

    }

    public void RegisterFurnitureCreated (Action<Furniture> callbackfunc) {
        cbFurnitureCreated += callbackfunc;
    }

    public void UnregisterFurnitureCreated (Action<Furniture> callbackfunc) {
        cbFurnitureCreated -= callbackfunc;
    }
    public LandTile.TileType CheckNeighbors (int x, int y) {
        //Debug.Log("InitialTileCheck, tile checked:"+x+"_"+y);
        LandTile.TileType tile_under = LandTile.TileType.Empty;
        LandTile.TileType tile_left = LandTile.TileType.Empty;
        LandTile.TileType tile_top = LandTile.TileType.Empty;
        LandTile.TileType tile_right = LandTile.TileType.Empty;

        LandTile.TileType[] under = new LandTile.TileType[] { LandTile.TileType.Empty }; //List of valid tiles if there is only tile under the tile
        LandTile.TileType[] left = new LandTile.TileType[] { LandTile.TileType.Empty }; //list of valid tiles if there is only tile left
        List<LandTile.TileType> valid = new List<LandTile.TileType> { LandTile.TileType.Empty };
        //Check tiles around LandTile [x,y]
        if (y > 0) { //skip check if y is 0
            tile_under = tiles[x, y - 1].Type;
        } else if (y == 0) {
            tile_under = LandTile.TileType.Empty;
        }
        if (x > 0) { //skip check if x is 0
            tile_left = tiles[x - 1, y].Type;
        } else if (x == 0) {
            tile_left = LandTile.TileType.Empty;
        }
        if (y < height - 1) { //skip check if y is height
            tile_top = tiles[x, y + 1].Type;
        } else if (y == height - 1) {
            tile_top = LandTile.TileType.Empty;
        }
        if (x < width - 1) { //skip check if x is wdith
            tile_right = tiles[x + 1, y].Type;
        } else if (x == width - 1) {
            tile_right = LandTile.TileType.Empty;
        }
        Debug.Log ("Checked Tile: Tile_" + x + "_" + y + " Old Type:" + tiles[x, y].Type + " under:" + tile_under + " left:" + tile_left);

        //Checks that the tile to the left of x,y exists
        if (tile_under != LandTile.TileType.Empty) { //Checks that the tile under x,y exists
            if (tile_under == LandTile.TileType.LowGrass) {
                under = new LandTile.TileType[] { LandTile.TileType.LowGrass, LandTile.TileType.BottomGrass, LandTile.TileType.BottomDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.BRGrass, LandTile.TileType.BRDirt };
            } else if (tile_under == LandTile.TileType.FullDirt) {
                under = new LandTile.TileType[] { LandTile.TileType.FullDirt, LandTile.TileType.TopDirt };
            } else if (tile_under == LandTile.TileType.FullGrass) {
                under = new LandTile.TileType[] { LandTile.TileType.FullGrass, LandTile.TileType.TopGrass };
            } else if (tile_under == LandTile.TileType.RightGrass) {
                under = new LandTile.TileType[] { LandTile.TileType.RightGrass, LandTile.TileType.TRGrass };
            } else if (tile_under == LandTile.TileType.RightDirt) {
                under = new LandTile.TileType[] { LandTile.TileType.RightDirt, LandTile.TileType.TRDirt };
            } else if (tile_under == LandTile.TileType.LeftGrass) {
                under = new LandTile.TileType[] { LandTile.TileType.LeftGrass, LandTile.TileType.TLGrass };
            } else if (tile_under == LandTile.TileType.LeftDirt) {
                under = new LandTile.TileType[] { LandTile.TileType.LeftDirt, LandTile.TileType.TLDirt };
            } else if (tile_under == LandTile.TileType.TopGrass) {
                under = new LandTile.TileType[] { LandTile.TileType.LowGrass, LandTile.TileType.BRGrass, LandTile.TileType.BLGrass, LandTile.TileType.BRDirt, LandTile.TileType.BLDirt };
            } else if (tile_under == LandTile.TileType.TopDirt) {
                under = new LandTile.TileType[] { LandTile.TileType.LowGrass, LandTile.TileType.BRGrass, LandTile.TileType.BLGrass, LandTile.TileType.BRDirt, LandTile.TileType.BLDirt };
            } else if (tile_under == LandTile.TileType.BottomGrass) {
                under = new LandTile.TileType[] { LandTile.TileType.FullGrass };
            } else if (tile_under == LandTile.TileType.BottomDirt) {
                under = new LandTile.TileType[] { LandTile.TileType.FullDirt };
            } else if (tile_under == LandTile.TileType.BRGrass) {
                under = new LandTile.TileType[] { LandTile.TileType.RightGrass };
            } else if (tile_under == LandTile.TileType.BRDirt) {
                under = new LandTile.TileType[] { LandTile.TileType.RightDirt };
            } else if (tile_under == LandTile.TileType.BLGrass) {
                under = new LandTile.TileType[] { LandTile.TileType.LeftGrass };
            } else if (tile_under == LandTile.TileType.BLDirt) {
                under = new LandTile.TileType[] { LandTile.TileType.LeftDirt };
            } else if (tile_under == LandTile.TileType.TRGrass) {
                under = new LandTile.TileType[] { LandTile.TileType.LowGrass, LandTile.TileType.BottomGrass, LandTile.TileType.BottomDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.BRGrass, LandTile.TileType.BRDirt };
            } else if (tile_under == LandTile.TileType.TRDirt) {
                under = new LandTile.TileType[] { LandTile.TileType.LowGrass, LandTile.TileType.BottomGrass, LandTile.TileType.BottomDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.BRGrass, LandTile.TileType.BRDirt };
            } else if (tile_under == LandTile.TileType.TLGrass) {
                under = new LandTile.TileType[] { LandTile.TileType.LowGrass, LandTile.TileType.BottomGrass, LandTile.TileType.BottomDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.BRGrass, LandTile.TileType.BRDirt };
            } else if (tile_under == LandTile.TileType.TLDirt) {
                under = new LandTile.TileType[] { LandTile.TileType.LowGrass, LandTile.TileType.BottomGrass, LandTile.TileType.BottomDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.BRGrass, LandTile.TileType.BRDirt };
            }
        }

        if (tile_left != LandTile.TileType.Empty) {
            if (tile_left == LandTile.TileType.LowGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.LowGrass, LandTile.TileType.LeftGrass, LandTile.TileType.LeftDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.TLGrass, LandTile.TileType.TLDirt };
            }
            if (tile_left == LandTile.TileType.LowGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.LowGrass, LandTile.TileType.LeftDirt, LandTile.TileType.LeftGrass, LandTile.TileType.BLDirt, LandTile.TileType.BLGrass };
            } else if (tile_left == LandTile.TileType.FullDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.FullDirt, LandTile.TileType.RightDirt };
            } else if (tile_left == LandTile.TileType.FullGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.FullGrass, LandTile.TileType.RightGrass };
            } else if (tile_left == LandTile.TileType.RightGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.LowGrass, LandTile.TileType.LeftGrass, LandTile.TileType.LeftDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.TLGrass, LandTile.TileType.TLDirt };
            } else if (tile_left == LandTile.TileType.RightDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.LowGrass, LandTile.TileType.LeftGrass, LandTile.TileType.LeftDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.TLGrass, LandTile.TileType.TLDirt };
            } else if (tile_left == LandTile.TileType.LeftGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.FullGrass, LandTile.TileType.RightGrass };
            } else if (tile_left == LandTile.TileType.LeftDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.FullDirt, LandTile.TileType.RightDirt };
            } else if (tile_left == LandTile.TileType.TopGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.TRGrass };
            } else if (tile_left == LandTile.TileType.TopDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.TRDirt };
            } else if (tile_left == LandTile.TileType.BottomGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.BRGrass };
            } else if (tile_left == LandTile.TileType.BottomDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.BRDirt };
            } else if (tile_left == LandTile.TileType.TRGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.LowGrass };
            } else if (tile_left == LandTile.TileType.TLGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.TopGrass };
            } else if (tile_left == LandTile.TileType.BRGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.LowGrass };
            } else if (tile_left == LandTile.TileType.BLGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.BottomGrass };
            } else if (tile_left == LandTile.TileType.TRDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.LowGrass };
            } else if (tile_left == LandTile.TileType.TLDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.TopDirt };
            } else if (tile_left == LandTile.TileType.BRDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.LowGrass };
            } else if (tile_left == LandTile.TileType.BLDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.BottomDirt };
            }
        }

        int underx;
        int lefty;

        if (tile_left != LandTile.TileType.Empty && tile_under != LandTile.TileType.Empty) { //if neither tile is empty, checks and adds equal tiletypes from under and left to valid
            for (underx = 0; underx < under.Length; underx++) {
                for (lefty = 0; lefty < left.Length; lefty++) {
                    if (under[underx] == left[lefty]) {
                        valid.Add (under[underx]);
                    }
                }
            }
        } else if ((tile_under == LandTile.TileType.Empty) && (tile_left != LandTile.TileType.Empty)) { //if under tile is empty, adds the entire left to valid
            for (lefty = 0; lefty < left.Length; lefty++) {
                valid.Add (left[lefty]);
            }
        } else if ((tile_left == LandTile.TileType.Empty) && (tile_under != LandTile.TileType.Empty)) { //if left tile is empty, adds the entire under to valid
            for (underx = 0; underx < under.Length; underx++) {
                valid.Add (under[underx]);
            }
        } else {
            Debug.LogError ("Something Went Wrong when checking neightbors");
        }

        valid.Remove (LandTile.TileType.Empty);
        foreach (var b in valid) {
            Debug.Log ("Valid:" + b);
        }
        if (valid.Count > 1) { tiles[x, y].Type = valid[UnityEngine.Random.Range (0, valid.Count)]; } else { tiles[x, y].Type = valid.FirstOrDefault (); }
        valid.Clear ();
        //tiles[x, y].Type = left[UnityEngine.Random.Range (0, left.Length)];
        Debug.Log ("Checked Tile: Tile_" + x + "_" + y + " New Type:" + tiles[x, y].Type);
        return tiles[x, y].Type;
    }
}