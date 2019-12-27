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

    public World (int width = 20, int height = 20) {
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
                    tiles[x, y].Type = NeighborCheckInitial (x, y, neighbors);
                    LandTile.TileBasicType tile_to_the_south = LandTile.TileBasicType.Empty;
                    LandTile.TileBasicType tile_to_the_west = LandTile.TileBasicType.Empty;

                    if (tiles[x, y].Type == LandTile.TileType.ErrorTile) {
                        //Activated when a tile gets assigned as an error tile.
                        if (y > 0) { //skip check if y is 0, means there is no tile under this tile
                            tile_to_the_south = tiles[x, y - 1].BasicType;
                        } else if (y == 0) {
                            tile_to_the_south = LandTile.TileBasicType.Empty;
                        }
                        if (x > 0) { //skip check if x is 0, means there is no tile to the west of this tile
                            tile_to_the_west = tiles[x - 1, y].BasicType;
                        } else if (x == 0) {
                            tile_to_the_west = LandTile.TileBasicType.Empty;
                        }

                        if (tile_to_the_west == tile_to_the_south) {
                            //IF Left and Under Tile are both same basic type, will not be triggered if either of them are empty.
                            if (tile_to_the_south == LandTile.TileBasicType.Grass) {
                                tiles[x, y].Type = LandTile.TileType.FullGrass;
                                tiles[x, y - 1].Type = reassignSouthTile (tiles[x, y].Type, x, (y - 1), neighbors);
                                tiles[x - 1, y].Type = reassignWestTile (tiles[x, y].Type, (x - 1), y, neighbors);
                            }
                            if (tile_to_the_south == LandTile.TileBasicType.Dirt) {
                                tiles[x, y].Type = LandTile.TileType.FullDirt;
                                tiles[x, y - 1].Type = reassignSouthTile (tiles[x, y].Type, x, (y - 1), neighbors);
                                tiles[x - 1, y].Type = reassignWestTile (tiles[x, y].Type, (x - 1), y, neighbors);
                            }
                        } else if (tile_to_the_south == LandTile.TileBasicType.Empty) { //If Only West Tile Exists
                            tiles[x, y].Type = LandTile.TileType.Generic;
                            tiles[x - 1, y].Type = reassignWestTile (tiles[x, y].Type, (x - 1), y, neighbors);
                        } else if (tile_to_the_west == LandTile.TileBasicType.Empty) { //If Only West Tile Exists
                            tiles[x, y].Type = LandTile.TileType.Generic;
                            tiles[x, y - 1].Type = reassignSouthTile (tiles[x, y].Type, x, (y - 1), neighbors);
                        } else {
                            //                            Debug.Log ("Trying to regenerate error Tile_" + x + "_" + y);
                            tiles[x, y].Type = LandTile.TileType.Generic;
                            tiles[x, y - 1].Type = reassignSouthTile (tiles[x, y].Type, x, (y - 1), neighbors);
                            tiles[x - 1, y].Type = reassignWestTile (tiles[x, y].Type, (x - 1), y, neighbors);
                        }
                    }
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

    public void PlaceFurniture (string objectType, LandTile t) {
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

    public LandTile.TileType NeighborCheckInitial (int x, int y, Dictionary<LandTile.TileType, string[, ]> siblings) {

        LandTile.TileType tile_to_the_south = LandTile.TileType.Empty;
        LandTile.TileType tile_to_the_west = LandTile.TileType.Empty;
        if (y > 0) { //skip check if y is 0
            if (tiles[x, y - 1].Type == LandTile.TileType.ErrorTile) { tile_to_the_south = LandTile.TileType.Empty; } else {
                //Debug.Log ("Tile Under :" + tiles[x, y - 1].Type);
                tile_to_the_south = tiles[x, y - 1].Type;
            }
        } else {
            tile_to_the_south = LandTile.TileType.Empty;
        }
        if (x > 0) { //skip check if x is 0
            if (tiles[x - 1, y].Type == LandTile.TileType.ErrorTile) { tile_to_the_west = LandTile.TileType.Empty; } else {
                tile_to_the_west = tiles[x - 1, y].Type;
            }
        } else {
            tile_to_the_west = LandTile.TileType.Empty;
        }

        List<LandTile.TileType> validWestEdge = new List<LandTile.TileType> { };
        validWestEdge = westTileCheck (tile_to_the_west, siblings); //Gives tile[x,y] possibilites based ONLY on WEST Tile
        List<LandTile.TileType> validSouthEdge = new List<LandTile.TileType> { };
        validSouthEdge = southTileCheck (tile_to_the_south, siblings); //Gives tile[x,y] possibilites based ONLY on WEST Tile
        List<LandTile.TileType> valid = new List<LandTile.TileType> { };
        if (validWestEdge.Count > 0 && (y == 0 || tile_to_the_south == LandTile.TileType.Empty)) { //If only WEST Tile Exists
            foreach (var a in validWestEdge) {
                valid.Add (a);
            }
        }
        if (validSouthEdge.Count > 0 && (x == 0 || tile_to_the_west == LandTile.TileType.Empty)) { //If only SOUTH Tile Exists
            foreach (var a in validSouthEdge) {
                valid.Add (a);
            }
        }
        if (validSouthEdge.Count > 0 && validWestEdge.Count > 0) { //if BOTH SOUTH & WEST Tiles Exist
            //Goes through all the tiles in validSouthEdge and checks if validWestEdge has that tile, if it does adds tile to VALID array
            foreach (var a in validSouthEdge) {
                if (validWestEdge.Contains (a)) {
                    valid.Add (a);

                }
            }
        }
        if (valid.Count != 0) {
            int random = UnityEngine.Random.Range (0, valid.Count);
            //Debug.Log ("Valid Count:" + valid.Count + "   Random:" + random + " Result:" + valid[random]);
            tiles[x, y].Type = valid[random];
        } else {
            //            Debug.Log ("There has been an Error Generating a tile, Tile_" + x + "_" + y);
            tiles[x, y].Type = LandTile.TileType.ErrorTile;
        }
        return tiles[x, y].Type;

    }

    public List<LandTile.TileType> westTileCheck (LandTile.TileType tile_to_the_west, Dictionary<LandTile.TileType, string[, ]> siblings) {
        List<LandTile.TileType> validWestEdge = new List<LandTile.TileType> { };
        string[, ] westReq = siblings[tile_to_the_west]; //Pulls out all requirements for the tile to the west
        string[] eastEdgeReq = new string[] { westReq[1, 0], westReq[1, 1], westReq[1, 2] }; //pulls out the west edge requirements of tile_to_the_west
        foreach (KeyValuePair<LandTile.TileType, string[, ]> tileType in siblings) {
            string[, ] testerTile = tileType.Value; //Pulls out tile to test
            string[] testWestEdge = new string[] { testerTile[3, 2], testerTile[3, 1], testerTile[3, 0] }; //pulls the left posibilites

            if (eastEdgeReq.SequenceEqual (testWestEdge) && tile_to_the_west != LandTile.TileType.Empty) {
                validWestEdge.Add (tileType.Key);
            }
        }
        return validWestEdge;
    }
    public List<LandTile.TileType> southTileCheck (LandTile.TileType tile_to_the_south, Dictionary<LandTile.TileType, string[, ]> siblings) {
        List<LandTile.TileType> validSouthEdge = new List<LandTile.TileType> { };
        string[, ] southReq = siblings[tile_to_the_south];
        string[] northEdgeReq = new string[] { southReq[0, 0], southReq[0, 1], southReq[0, 2] };

        foreach (KeyValuePair<LandTile.TileType, string[, ]> tileType in siblings) {
            string[, ] testerTile = tileType.Value; //Pulls out tile to test
            string[] testSouthEdge = new string[] { testerTile[2, 2], testerTile[2, 1], testerTile[2, 0] }; //pulls the left posibilites

            if (northEdgeReq.SequenceEqual (testSouthEdge) && tile_to_the_south != LandTile.TileType.Empty) {
                validSouthEdge.Add (tileType.Key);
            }
        }
        return validSouthEdge;
    }
    public List<LandTile.TileType> northTileCheck (LandTile.TileType tile_to_the_north, Dictionary<LandTile.TileType, string[, ]> siblings) {
        List<LandTile.TileType> validNorthEdge = new List<LandTile.TileType> { };
        string[, ] northReq = siblings[tile_to_the_north];
        string[] southEdgeReq = new string[] { northReq[2, 2], northReq[2, 1], northReq[2, 0] };

        foreach (KeyValuePair<LandTile.TileType, string[, ]> tileType in siblings) {
            string[, ] testerTile = tileType.Value; //Pulls out tile to test
            string[] testNorthEdge = new string[] { testerTile[0, 0], testerTile[0, 1], testerTile[0, 2] }; //pulls the left posibilites

            if (southEdgeReq.SequenceEqual (testNorthEdge) && tile_to_the_north != LandTile.TileType.Empty) {
                validNorthEdge.Add (tileType.Key);
            }
        }
        return validNorthEdge;
    }
    public List<LandTile.TileType> eastTileCheck (LandTile.TileType tile_to_the_east, Dictionary<LandTile.TileType, string[, ]> siblings) {
        List<LandTile.TileType> validEastEdge = new List<LandTile.TileType> { };
        string[, ] eastReq = siblings[tile_to_the_east];
        string[] westEdgeReq = new string[] { eastReq[3, 2], eastReq[3, 1], eastReq[3, 0] };

        foreach (KeyValuePair<LandTile.TileType, string[, ]> tileType in siblings) {
            string[, ] testerTile = tileType.Value; //Pulls out tile to test
            string[] testEastEdge = new string[] { testerTile[1, 0], testerTile[1, 1], testerTile[1, 2] }; //pulls the left posibilites

            if (westEdgeReq.SequenceEqual (testEastEdge) && tile_to_the_east != LandTile.TileType.Empty) {
                validEastEdge.Add (tileType.Key);
            }
        }
        return validEastEdge;
    }

    public LandTile.TileType reassignWestTile (LandTile.TileType tile_to_the_east, int x, int y, Dictionary<LandTile.TileType, string[, ]> siblings) {
        //      Debug.Log ("Checking tile Tile_" + x + "_" + y);
        List<LandTile.TileType> validWestEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validSouthEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validNorthEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validEastEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> valid = new List<LandTile.TileType> { };

        List<LandTile.TileType> temp = new List<LandTile.TileType> { };
        validEastEdge = eastTileCheck (tile_to_the_east, siblings);

        if (x > 0) {
            validWestEdge = westTileCheck (tiles[x - 1, y].Type, siblings);

        }
        if (y > 0) {
            validSouthEdge = southTileCheck (tiles[x, y - 1].Type, siblings);
        }
        if (y < height - 1) {
            validNorthEdge = northTileCheck (tiles[x, y + 1].Type, siblings);
            //            Debug.Log ("ValidNorthEdge:" + (string.Join (",", validNorthEdge)));
        }

        if (validEastEdge.Count > 0) {
            foreach (var a in validEastEdge) {
                temp.Add (a);
            }
        }
        //Debug.Log ("RU TOPEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validWestEdge.Count > 0) {
            foreach (var a in validWestEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
            //Debug.Log ("FIRST RU LEFTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
            temp.Clear ();
            temp = valid.ToList ();
            valid.Clear ();
        }
        //Debug.Log ("RU LEFTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);

        if (validSouthEdge.Count > 0) {
            foreach (var a in validSouthEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
            temp.Clear ();
            temp = valid.ToList ();
            valid.Clear ();
        }
        //Debug.Log ("RU BOTTOMEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validNorthEdge.Count > 0) {
            foreach (var a in validNorthEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
        }
        //Debug.Log ("RU RIGHTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (valid.Count == 0) {
            valid = temp.ToList ();
        }
        int random = UnityEngine.Random.Range (0, valid.Count);
        //Debug.Log ("Valid Count:" + valid.Count + "   Random:" + random + " Result:" + valid[random]);
        tiles[x, y].Type = valid[random];
        return tiles[x, y].Type;

    }
    public LandTile.TileType reassignEastTile (LandTile.TileType tile_to_the_west, int x, int y, Dictionary<LandTile.TileType, string[, ]> siblings) {
        //      Debug.Log ("Checking tile Tile_" + x + "_" + y);
        List<LandTile.TileType> validWestEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validSouthEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validNorthEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validEastEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> valid = new List<LandTile.TileType> { };

        List<LandTile.TileType> temp = new List<LandTile.TileType> { };
        validWestEdge = westTileCheck (tile_to_the_west, siblings);

        if (x < width - 1) {
            validEastEdge = eastTileCheck (tiles[x + 1, y].Type, siblings);

        }
        if (y > 0) {
            validSouthEdge = southTileCheck (tiles[x, y - 1].Type, siblings);
        }
        if (y < height - 1) {
            validNorthEdge = northTileCheck (tiles[x, y + 1].Type, siblings);
            //   Debug.Log ("ValidNorthEdge:" + (string.Join (",", validNorthEdge)));
        }

        if (validWestEdge.Count > 0) {
            foreach (var a in validWestEdge) {
                temp.Add (a);
            }
        }
        //Debug.Log ("RU TOPEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validEastEdge.Count > 0) {
            foreach (var a in validEastEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
            //Debug.Log ("FIRST RU LEFTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
            temp.Clear ();
            temp = valid.ToList ();
            valid.Clear ();
        }
        //Debug.Log ("RU LEFTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);

        if (validSouthEdge.Count > 0) {
            foreach (var a in validSouthEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
            temp.Clear ();
            temp = valid.ToList ();
            valid.Clear ();
        }
        //Debug.Log ("RU BOTTOMEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validNorthEdge.Count > 0) {
            foreach (var a in validNorthEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
        }
        //Debug.Log ("RU RIGHTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (valid.Count == 0) {
            valid = temp.ToList ();
        }
        int random = UnityEngine.Random.Range (0, valid.Count);
        //Debug.Log ("Valid Count:" + valid.Count + "   Random:" + random + " Result:" + valid[random]);
        tiles[x, y].Type = valid[random];
        return tiles[x, y].Type;

    }
    public LandTile.TileType reassignSouthTile (LandTile.TileType tile_to_the_south, int x, int y, Dictionary<LandTile.TileType, string[, ]> siblings) {
        //        Debug.Log ("Regenerating South tile Tile_" + x + "_" + y);

        List<LandTile.TileType> validWestEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validSouthEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validNorthEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validEastEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> valid = new List<LandTile.TileType> { };

        List<LandTile.TileType> temp = new List<LandTile.TileType> { };

        validNorthEdge = northTileCheck (tile_to_the_south, siblings); //generates possibilites based on the tile to the North Of current tile
        if (x > 0) {
            validWestEdge = westTileCheck (tiles[x - 1, y].Type, siblings);

        }
        if (y > 0) {
            validSouthEdge = southTileCheck (tiles[x, y - 1].Type, siblings);
        }
        if (x < width - 1) {
            if (tiles[x + 1, y].Type != LandTile.TileType.Empty) {
                validEastEdge = eastTileCheck (tiles[x + 1, y].Type, siblings);
            }
        }
        if (validNorthEdge.Count > 0) {
            foreach (var a in validNorthEdge) {
                temp.Add (a);
            }
        }
        //Debug.Log ("RU TOPEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validWestEdge.Count > 0) {
            foreach (var a in validWestEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
            //Debug.Log ("FIRST RU LEFTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
            temp.Clear ();
            temp = valid.ToList ();
            valid.Clear ();
        }
        //Debug.Log ("RU LEFTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);

        if (validSouthEdge.Count > 0) {
            foreach (var a in validSouthEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
            temp.Clear ();
            temp = valid.ToList ();
            valid.Clear ();
        }
        //Debug.Log ("RU BOTTOMEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validEastEdge.Count > 0) {
            foreach (var a in validEastEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
        }
        //Debug.Log ("RU RIGHTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (valid.Count == 0) {
            valid = temp.ToList ();
        }
        int random = UnityEngine.Random.Range (0, valid.Count);
        //Debug.Log ("Valid Count:" + valid.Count + "   Temp Count:" + temp.Count + "  Random:" + random);
        tiles[x, y].Type = valid[random];
        return tiles[x, y].Type;

    }
    public LandTile.TileType reassignNorthTile (LandTile.TileType tile_to_the_north, int x, int y, Dictionary<LandTile.TileType, string[, ]> siblings) {
        //      Debug.Log ("Regenerating North tile Tile_" + x + "_" + y);

        List<LandTile.TileType> validWestEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validSouthEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validNorthEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validEastEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> valid = new List<LandTile.TileType> { };

        List<LandTile.TileType> temp = new List<LandTile.TileType> { };

        validSouthEdge = southTileCheck (tile_to_the_north, siblings); //generates possibilites based on the tile to the North Of current tile
        if (x > 0) {
            validWestEdge = westTileCheck (tiles[x - 1, y].Type, siblings);

        }
        if (y > 0) {
            validNorthEdge = northTileCheck (tiles[x, y + 1].Type, siblings);
        }
        if (x < width - 1) {
            if (tiles[x + 1, y].Type != LandTile.TileType.Empty) {
                validEastEdge = eastTileCheck (tiles[x + 1, y].Type, siblings);
            }
        }
        if (validSouthEdge.Count > 0) {
            foreach (var a in validSouthEdge) {
                temp.Add (a);
            }
        }
        //Debug.Log ("RU TOPEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validWestEdge.Count > 0) {
            foreach (var a in validWestEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
            //Debug.Log ("FIRST RU LEFTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
            temp.Clear ();
            temp = valid.ToList ();
            valid.Clear ();
        }
        //Debug.Log ("RU LEFTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);

        if (validNorthEdge.Count > 0) {
            foreach (var a in validNorthEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
            temp.Clear ();
            temp = valid.ToList ();
            valid.Clear ();
        }
        //Debug.Log ("RU BOTTOMEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validEastEdge.Count > 0) {
            foreach (var a in validEastEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
        }
        //Debug.Log ("RU RIGHTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (valid.Count == 0) {
            valid = temp.ToList ();
        }
        int random = UnityEngine.Random.Range (0, valid.Count);
        //Debug.Log ("Valid Count:" + valid.Count + "   Temp Count:" + temp.Count + "  Random:" + random);
        tiles[x, y].Type = valid[random];
        return tiles[x, y].Type;

    }

    public Dictionary<LandTile.TileType, string[, ]> determineNeighbors (Dictionary<LandTile.TileType, string[, ]> siblings, int x, int y) {
        siblings.Add (LandTile.TileType.Empty, new string[4, 3] { { "emp", "emp", "emp" }, { "emp", "emp", "emp" }, { "emp", "emp", "emp" }, { "emp", "emp", "emp" } });

        siblings.Add (LandTile.TileType.Generic, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" } });

        siblings.Add (LandTile.TileType.FullGrass, new string[4, 3] { { "gr", "gr", "gr" }, { "gr", "gr", "gr" }, { "gr", "gr", "gr" }, { "gr", "gr", "gr" } });
        siblings.Add (LandTile.TileType.TopGrass, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gr" }, { "gr", "gr", "gen" } });
        siblings.Add (LandTile.TileType.BottomGrass, new string[4, 3] { { "gr", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gr" } });
        siblings.Add (LandTile.TileType.RightGrass, new string[4, 3] { { "gr", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gr" } });
        siblings.Add (LandTile.TileType.LeftGrass, new string[4, 3] { { "gen", "gr", "gr" }, { "gr", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.BLGrass, new string[4, 3] { { "gen", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.BRGrass, new string[4, 3] { { "gr", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gr" } });
        siblings.Add (LandTile.TileType.TLGrass, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.TRGrass, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gen" } });
        //DIRT
        siblings.Add (LandTile.TileType.FullDirt, new string[4, 3] { { "di", "di", "di" }, { "di", "di", "di" }, { "di", "di", "di" }, { "di", "di", "di" } });
        siblings.Add (LandTile.TileType.TopDirt, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "di", "di" }, { "di", "di", "di" }, { "di", "di", "gen" } });
        siblings.Add (LandTile.TileType.BottomDirt, new string[4, 3] { { "di", "di", "di" }, { "di", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "di" } });
        siblings.Add (LandTile.TileType.RightDirt, new string[4, 3] { { "di", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "di" }, { "di", "di", "di" } });
        siblings.Add (LandTile.TileType.LeftDirt, new string[4, 3] { { "gen", "di", "di" }, { "di", "di", "di" }, { "di", "di", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.BLDirt, new string[4, 3] { { "gen", "di", "di" }, { "di", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.BRDirt, new string[4, 3] { { "di", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "di" } });
        siblings.Add (LandTile.TileType.TLDirt, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "di", "di" }, { "di", "di", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.TRDirt, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "di" }, { "di", "di", "gen" } });

        siblings.Add (LandTile.TileType.Grass_S, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Grass_NS, new string[4, 3] { { "gen", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Grass_N, new string[4, 3] { { "gen", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Grass_E, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Grass_EW, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_W, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Grass_NE, new string[4, 3] { { "gen", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Grass_NES, new string[4, 3] { { "gen", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Grass_ES, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Grass_ESW, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_NESW, new string[4, 3] { { "gen", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_SW, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_NSW, new string[4, 3] { { "gen", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_NW, new string[4, 3] { { "gen", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gen" } });

        siblings.Add (LandTile.TileType.Grass_IC_TL, new string[4, 3] { { "gen", "gr", "gr" }, { "gr", "gr", "gr" }, { "gr", "gr", "gr" }, { "gr", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_IC_BL, new string[4, 3] { { "gr", "gr", "gr" }, { "gr", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gr", "gr" } });
        siblings.Add (LandTile.TileType.Grass_IC_TR, new string[4, 3] { { "gr", "gr", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gr" }, { "gr", "gr", "gr" } });
        siblings.Add (LandTile.TileType.Grass_IC_BR, new string[4, 3] { { "gr", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gr" } });

        siblings.Add (LandTile.TileType.Grass_Dia_SE, new string[4, 3] { { "gr", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gr" } });
        siblings.Add (LandTile.TileType.Grass_Full_NES_T, new string[4, 3] { { "gen", "gr", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Grass_Full_NES_B, new string[4, 3] { { "gen", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gr", "gr" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Grass_Dia_NE, new string[4, 3] { { "gen", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_Full_ESW_L, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_Full_NEW_L, new string[4, 3] { { "gen", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_Full_ESW_R, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_Full_NEW_R, new string[4, 3] { { "gr", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gr" } });
        siblings.Add (LandTile.TileType.Grass_Dia_SW, new string[4, 3] { { "gen", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_Full_NSW_T, new string[4, 3] { { "gen", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_Full_NSW_B, new string[4, 3] { { "gr", "gr", "gen" }, { "gen", "gen", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gr" } });
        siblings.Add (LandTile.TileType.Grass_Dia_NW, new string[4, 3] { { "gen", "gr", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_Exit_Top, new string[4, 3] { { "gen", "gr", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gr" }, { "gr", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_Exit_Right, new string[4, 3] { { "gr", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gr" } });
        siblings.Add (LandTile.TileType.Grass_Exit_Left, new string[4, 3] { { "gen", "gr", "gr" }, { "gr", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_Exit_Bottom, new string[4, 3] { { "gr", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gr", "gen" }, { "gen", "gr", "gr" } });
        siblings.Add (LandTile.TileType.Grass_Full_SW_NE, new string[4, 3] { { "gen", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gen" } });
        siblings.Add (LandTile.TileType.Grass_Full_NW_SE, new string[4, 3] { { "gr", "gr", "gen" }, { "gen", "gr", "gr" }, { "gr", "gr", "gen" }, { "gen", "gr", "gr" } });

        siblings.Add (LandTile.TileType.Dirt_S, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_NS, new string[4, 3] { { "gen", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_N, new string[4, 3] { { "gen", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_E, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_EW, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_W, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_NE, new string[4, 3] { { "gen", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_NES, new string[4, 3] { { "gen", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_ES, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_ESW, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_NESW, new string[4, 3] { { "gen", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_SW, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_NSW, new string[4, 3] { { "gen", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_NW, new string[4, 3] { { "gen", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "gen" } });

        siblings.Add (LandTile.TileType.Dirt_IC_TL, new string[4, 3] { { "gen", "di", "di" }, { "di", "di", "di" }, { "di", "di", "di" }, { "di", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_IC_BL, new string[4, 3] { { "di", "di", "di" }, { "di", "di", "di" }, { "di", "di", "gen" }, { "gen", "di", "di" } });
        siblings.Add (LandTile.TileType.Dirt_IC_TR, new string[4, 3] { { "di", "di", "gen" }, { "gen", "di", "di" }, { "di", "di", "di" }, { "di", "di", "di" } });
        siblings.Add (LandTile.TileType.Dirt_IC_BR, new string[4, 3] { { "di", "di", "di" }, { "di", "di", "gen" }, { "gen", "di", "di" }, { "di", "di", "di" } });

        siblings.Add (LandTile.TileType.Dirt_Dia_SE, new string[4, 3] { { "di", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "di" } });
        siblings.Add (LandTile.TileType.Dirt_Full_NES_T, new string[4, 3] { { "gen", "di", "gen" }, { "gen", "di", "di" }, { "di", "di", "gen" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_Full_NES_B, new string[4, 3] { { "gen", "di", "di" }, { "di", "di", "gen" }, { "gen", "di", "di" }, { "gen", "gen", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_Dia_NE, new string[4, 3] { { "gen", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "di" }, { "di", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_Full_ESW_L, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "di", "di" }, { "di", "di", "gen" }, { "gen", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_Full_NEW_L, new string[4, 3] { { "gen", "di", "di" }, { "di", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_Full_ESW_R, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "di" }, { "di", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_Full_NEW_R, new string[4, 3] { { "di", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "di" } });
        siblings.Add (LandTile.TileType.Dirt_Dia_SW, new string[4, 3] { { "gen", "di", "di" }, { "di", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_Full_NSW_T, new string[4, 3] { { "gen", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "di" }, { "di", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_Full_NSW_B, new string[4, 3] { { "di", "di", "gen" }, { "gen", "gen", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "di" } });
        siblings.Add (LandTile.TileType.Dirt_Dia_NW, new string[4, 3] { { "gen", "di", "gen" }, { "gen", "di", "di" }, { "di", "di", "gen" }, { "gen", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_Exit_Top, new string[4, 3] { { "gen", "di", "gen" }, { "gen", "di", "di" }, { "di", "di", "di" }, { "di", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_Exit_Right, new string[4, 3] { { "di", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "di" }, { "di", "di", "di" } });
        siblings.Add (LandTile.TileType.Dirt_Exit_Left, new string[4, 3] { { "gen", "di", "di" }, { "di", "di", "di" }, { "di", "di", "gen" }, { "gen", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_Exit_Bottom, new string[4, 3] { { "di", "di", "di" }, { "di", "di", "gen" }, { "gen", "di", "gen" }, { "gen", "di", "di" } });
        siblings.Add (LandTile.TileType.Dirt_Full_SW_NE, new string[4, 3] { { "gen", "di", "di" }, { "di", "di", "gen" }, { "gen", "di", "di" }, { "di", "di", "gen" } });
        siblings.Add (LandTile.TileType.Dirt_Full_NW_SE, new string[4, 3] { { "di", "di", "gen" }, { "gen", "di", "di" }, { "di", "di", "gen" }, { "gen", "di", "di" } });

        return siblings;
    }
}