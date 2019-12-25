using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World {
    LandTile[, ] tiles;
    Dictionary<string, Furniture> furniturePrototypes;
    Dictionary<LandTile.TileType, string[, ]> neighbors;
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

    public World (int width = 5, int height = 5) {
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
                        tiles[x, y].Type = LandTile.TileType.Generic;
                        break;
                }
                if (x != 0 || y != 0) {
                    neighbors = new Dictionary<LandTile.TileType, string[, ]> { };
                    neighbors = determineNeighbors (neighbors, x, y);
                    //tiles[x,y].Type=NeighborCheck(x,y,neighbors)
                    tiles[x, y].Type = CheckNeighbors (x, y);
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

        LandTile.TileType[] under = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.FullGrass, LandTile.TileType.FullDirt }; //List of valid tiles if there is only tile under the tile
        LandTile.TileType[] left = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.FullGrass, LandTile.TileType.FullDirt }; //list of valid tiles if there is only tile left
        List<LandTile.TileType> valid = new List<LandTile.TileType> { LandTile.TileType.Generic, LandTile.TileType.FullGrass, LandTile.TileType.FullDirt };
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
            if (tile_under == LandTile.TileType.Generic) {
                under = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.BottomGrass, LandTile.TileType.BottomDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.BRGrass, LandTile.TileType.BRDirt };
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
                under = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.BRGrass, LandTile.TileType.BLGrass, LandTile.TileType.BRDirt, LandTile.TileType.BLDirt };
            } else if (tile_under == LandTile.TileType.TopDirt) {
                under = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.BRGrass, LandTile.TileType.BLGrass, LandTile.TileType.BRDirt, LandTile.TileType.BLDirt };
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
                under = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.BottomGrass, LandTile.TileType.BottomDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.BRGrass, LandTile.TileType.BRDirt };
            } else if (tile_under == LandTile.TileType.TRDirt) {
                under = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.BottomGrass, LandTile.TileType.BottomDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.BRGrass, LandTile.TileType.BRDirt };
            } else if (tile_under == LandTile.TileType.TLGrass) {
                under = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.BottomGrass, LandTile.TileType.BottomDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.BRGrass, LandTile.TileType.BRDirt };
            } else if (tile_under == LandTile.TileType.TLDirt) {
                under = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.BottomGrass, LandTile.TileType.BottomDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.BRGrass, LandTile.TileType.BRDirt };
            }
        }

        if (tile_left != LandTile.TileType.Empty) {
            if (tile_left == LandTile.TileType.Generic) {
                left = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.LeftGrass, LandTile.TileType.LeftDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.TLGrass, LandTile.TileType.TLDirt };
            }
            if (tile_left == LandTile.TileType.Generic) {
                left = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.LeftDirt, LandTile.TileType.LeftGrass, LandTile.TileType.BLDirt, LandTile.TileType.BLGrass };
            } else if (tile_left == LandTile.TileType.FullDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.FullDirt, LandTile.TileType.RightDirt };
            } else if (tile_left == LandTile.TileType.FullGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.FullGrass, LandTile.TileType.RightGrass };
            } else if (tile_left == LandTile.TileType.RightGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.LeftGrass, LandTile.TileType.LeftDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.TLGrass, LandTile.TileType.TLDirt };
            } else if (tile_left == LandTile.TileType.RightDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.LeftGrass, LandTile.TileType.LeftDirt, LandTile.TileType.BLGrass, LandTile.TileType.BLDirt, LandTile.TileType.TLGrass, LandTile.TileType.TLDirt };
            } else if (tile_left == LandTile.TileType.LeftGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.FullGrass, LandTile.TileType.RightGrass };
            } else if (tile_left == LandTile.TileType.LeftDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.FullDirt, LandTile.TileType.RightDirt };
            } else if (tile_left == LandTile.TileType.TopGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.TRGrass, LandTile.TileType.TopGrass };
            } else if (tile_left == LandTile.TileType.TopDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.TRDirt, LandTile.TileType.TopDirt };
            } else if (tile_left == LandTile.TileType.BottomGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.BRGrass, LandTile.TileType.BottomGrass };
            } else if (tile_left == LandTile.TileType.BottomDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.BRDirt, LandTile.TileType.BottomDirt };
            } else if (tile_left == LandTile.TileType.TRGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.BLGrass, LandTile.TileType.LeftGrass, LandTile.TileType.LeftDirt, LandTile.TileType.TLGrass, LandTile.TileType.BLDirt, LandTile.TileType.TLDirt };
            } else if (tile_left == LandTile.TileType.TLGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.TopGrass, LandTile.TileType.TRGrass };
            } else if (tile_left == LandTile.TileType.BRGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.BLGrass, LandTile.TileType.LeftGrass, LandTile.TileType.LeftDirt, LandTile.TileType.TLGrass, LandTile.TileType.BLDirt, LandTile.TileType.TLDirt };
            } else if (tile_left == LandTile.TileType.BLGrass) {
                left = new LandTile.TileType[] { LandTile.TileType.BottomGrass, LandTile.TileType.BRGrass };
            } else if (tile_left == LandTile.TileType.TRDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.BLGrass, LandTile.TileType.LeftGrass, LandTile.TileType.LeftDirt, LandTile.TileType.TLGrass, LandTile.TileType.BLDirt, LandTile.TileType.TLDirt };
            } else if (tile_left == LandTile.TileType.TLDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.TopDirt, LandTile.TileType.TRDirt };
            } else if (tile_left == LandTile.TileType.BRDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.Generic, LandTile.TileType.BLGrass, LandTile.TileType.LeftGrass, LandTile.TileType.LeftDirt, LandTile.TileType.TLGrass, LandTile.TileType.BLDirt, LandTile.TileType.TLDirt };
            } else if (tile_left == LandTile.TileType.BLDirt) {
                left = new LandTile.TileType[] { LandTile.TileType.BottomDirt, LandTile.TileType.BRDirt };
            }
        }

        int underx;
        int lefty;
        valid.Clear ();
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
        if (valid.Count > 0) { tiles[x, y].Type = valid[UnityEngine.Random.Range (0, valid.Count)]; } else { tiles[x, y].Type = valid.FirstOrDefault (); }
        valid.Clear ();
        //tiles[x, y].Type = left[UnityEngine.Random.Range (0, left.Length)];
        Debug.Log ("Checked Tile: Tile_" + x + "_" + y + " New Type:" + tiles[x, y].Type);
        return tiles[x, y].Type;
    }

    public LandTile.TileType NeighborCheck(int x, int y, Dictionary<LandTile.TileType,string[,]> siblings){
        string [,] neighborRequirements=siblings[tile[x,y]];//Pulls neighbor requirements for the current tile in terms of a 4,3 array
        

    }
    public Dictionary<LandTile.TileType, string[, ]> determineNeighbors (Dictionary<LandTile.TileType, string[, ]> siblings, int x, int y) {

        siblings.Add (LandTile.TileType.FullGrass, new string[4, 3] { { "grass", "grass", "grass" }, { "grass", "grass", "grass" }, { "grass", "grass", "grass" }, { "grass", "grass", "grass" } });
        siblings.Add (LandTile.TileType.TopGrass, new string[4, 3] { { "generic", "generic", "generic" }, { "generic", "grass", "grass" }, { "grass", "grass", "grass" }, { "grass", "grass", "generic" } });
        siblings.Add (LandTile.TileType.BottomGrass, new string[4, 3] { { "grass", "grass", "generic" }, { "generic", "generic", "generic" }, { "generic", "grass", "grass" }, { "grass", "grass", "grass" } });
        siblings.Add (LandTile.TileType.RightGrass, new string[4, 3] { { "grass", "grass", "generic" }, { "generic", "generic", "generic" }, { "generic", "grass", "grass" }, { "grass", "grass", "grass" } });
        siblings.Add (LandTile.TileType.LeftGrass, new string[4, 3] { { "generic", "grass", "grass" }, { "grass", "grass", "grass" }, { "grass", "grass", "generic" }, { "generic", "generic", "generic" } });
        siblings.Add (LandTile.TileType.BLGrass, new string[4, 3] { { "generic", "grass", "grass" }, { "grass", "grass", "generic" }, { "generic", "generic", "generic" }, { "generic", "generic", "generic" } });
        siblings.Add (LandTile.TileType.BRGrass, new string[4, 3] { { "grass", "grass", "generic" }, { "generic", "generic", "generic" }, { "generic", "generic", "generic" }, { "generic", "grass", "grass" } });
        siblings.Add (LandTile.TileType.TLGrass, new string[4, 3] { { "generic", "generic", "generic" }, { "generic", "grass", "grass" }, { "grass", "grass", "generic" }, { "generic", "generic", "generic" } });
        siblings.Add (LandTile.TileType.TRGrass, new string[4, 3] { { "generic", "generic", "generic" }, { "generic", "generic", "generic" }, { "generic", "grass", "grass" }, { "grass", "grass", "generic" } });
        //DIRT
        siblings.Add (LandTile.TileType.FullDirt, new string[4, 3] { { "dirt", "dirt", "dirt" }, { "dirt", "dirt", "dirt" }, { "dirt", "dirt", "dirt" }, { "dirt", "dirt", "dirt" } });
        siblings.Add (LandTile.TileType.TopDirt, new string[4, 3] { { "generic", "generic", "generic" }, { "generic", "dirt", "dirt" }, { "dirt", "dirt", "dirt" }, { "dirt", "dirt", "generic" } });
        siblings.Add (LandTile.TileType.BottomDirt, new string[4, 3] { { "dirt", "dirt", "generic" }, { "generic", "generic", "generic" }, { "generic", "dirt", "dirt" }, { "dirt", "dirt", "dirt" } });
        siblings.Add (LandTile.TileType.RightDirt, new string[4, 3] { { "dirt", "dirt", "generic" }, { "generic", "generic", "generic" }, { "generic", "dirt", "dirt" }, { "dirt", "dirt", "dirt" } });
        siblings.Add (LandTile.TileType.LeftDirt, new string[4, 3] { { "generic", "dirt", "dirt" }, { "dirt", "dirt", "dirt" }, { "dirt", "dirt", "generic" }, { "generic", "generic", "generic" } });
        siblings.Add (LandTile.TileType.BLDirt, new string[4, 3] { { "generic", "dirt", "dirt" }, { "dirt", "dirt", "generic" }, { "generic", "generic", "generic" }, { "generic", "generic", "generic" } });
        siblings.Add (LandTile.TileType.BRDirt, new string[4, 3] { { "dirt", "dirt", "generic" }, { "generic", "generic", "generic" }, { "generic", "generic", "generic" }, { "generic", "dirt", "dirt" } });
        siblings.Add (LandTile.TileType.TLDirt, new string[4, 3] { { "generic", "generic", "generic" }, { "generic", "dirt", "dirt" }, { "dirt", "dirt", "generic" }, { "generic", "generic", "generic" } });
        siblings.Add (LandTile.TileType.TRDirt, new string[4, 3] { { "generic", "generic", "generic" }, { "generic", "generic", "generic" }, { "generic", "dirt", "dirt" }, { "dirt", "dirt", "generic" } });
       
        /* 
        Grass_S,
        Grass_NS,
        Grass_N,
        Grass_E,
        Grass_EW,
        Grass_W,
        Grass,
        Grass_NE,
        Grass_NES,
        Grass_SE,
        Grass_ESW,
        Grass_NESW,
        Grass_NEW,
        Grass_SW,
        Grass_NSW,
        Grass_NW,
        Grass_Dia_SE,
        Grass_Full_NES_T,
        Grass_Full_NES_B,
        Grass_Dia_NE,
        Grass_Full_ESW_L,
        Grass_IC_TL,
        Grass_IC_BL,
        Grass_Full_NEW_L,
        Grass_ESW_R,
        Grass_IC_TR,
        Grass_IC_BR,
        Grass_Full_NEW_R,
        Grass_Dia_SW,
        Grass_Full_NSW_T,
        Grass_Full_NSW_B,
        Grass_Dia_NW,
        Grass_Exit_Top,
        Grass_Exit_Right,
        Grass_Exit_Left,
        Grass_Exit_Bottom,
        Grass_Full_SW_NE,
        Grass_Full_NW_SE, */

        return siblings;
    }
}