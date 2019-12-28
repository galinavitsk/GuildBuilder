using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World {
    Tile[, ] tiles;
    public Dictionary<string, Furniture> furniturePrototypes { get; protected set; }
    public Dictionary<Tile.TileType, string[, ]> neighbors { get; protected set; }
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
        CreateInstalledObjectPrototypes ();
    }

    void CreateInstalledObjectPrototypes () {
        furniturePrototypes = new Dictionary<string, Furniture> ();
        furniturePrototypes.Add ("Wall", Furniture.CreatePrototype ("Wall", "Wall", 0, 1, 1, true));
    }

    public void PlaceFurniture (string objectType, Tile t) {
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

    public Tile.TileType NeighborCheckInitial (int x, int y, Dictionary<Tile.TileType, string[, ]> siblings) {

        Tile.TileType tile_to_the_south = Tile.TileType.Empty;
        Tile.TileType tile_to_the_west = Tile.TileType.Empty;
        if (y > 0) { //skip check if y is 0
            if (tiles[x, y - 1].Type == Tile.TileType.ErrorTile) { tile_to_the_south = Tile.TileType.Empty; } else {
                //Debug.Log ("Tile Under :" + tiles[x, y - 1].Type);
                tile_to_the_south = tiles[x, y - 1].Type;
            }
        } else {
            tile_to_the_south = Tile.TileType.Empty;
        }
        if (x > 0) { //skip check if x is 0
            if (tiles[x - 1, y].Type == Tile.TileType.ErrorTile) { tile_to_the_west = Tile.TileType.Empty; } else {
                tile_to_the_west = tiles[x - 1, y].Type;
            }
        } else {
            tile_to_the_west = Tile.TileType.Empty;
        }

        List<Tile.TileType> validWestEdge = new List<Tile.TileType> { };
        validWestEdge = westTileCheck (tile_to_the_west, siblings); //Gives tile[x,y] possibilites based ONLY on WEST Tile
        List<Tile.TileType> validSouthEdge = new List<Tile.TileType> { };
        validSouthEdge = southTileCheck (tile_to_the_south, siblings); //Gives tile[x,y] possibilites based ONLY on WEST Tile
        List<Tile.TileType> valid = new List<Tile.TileType> { };
        if (validWestEdge.Count > 0 && (y == 0 || tile_to_the_south == Tile.TileType.Empty)) { //If only WEST Tile Exists
            foreach (var a in validWestEdge) {
                valid.Add (a);
            }
        }
        if (validSouthEdge.Count > 0 && (x == 0 || tile_to_the_west == Tile.TileType.Empty)) { //If only SOUTH Tile Exists
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
            tiles[x, y].Type = Tile.TileType.ErrorTile;
        }
        return tiles[x, y].Type;

    }

    public List<Tile.TileType> westTileCheck (Tile.TileType tile_to_the_west, Dictionary<Tile.TileType, string[, ]> siblings) {
        List<Tile.TileType> validWestEdge = new List<Tile.TileType> { };
        string[, ] westReq = siblings[tile_to_the_west]; //Pulls out all requirements for the tile to the west
        string[] eastEdgeReq = new string[] { westReq[1, 0], westReq[1, 1], westReq[1, 2] }; //pulls out the west edge requirements of tile_to_the_west
        foreach (KeyValuePair<Tile.TileType, string[, ]> tileType in siblings) {
            string[, ] testerTile = tileType.Value; //Pulls out tile to test
            string[] testWestEdge = new string[] { testerTile[3, 2], testerTile[3, 1], testerTile[3, 0] }; //pulls the left posibilites

            if (eastEdgeReq.SequenceEqual (testWestEdge) && tile_to_the_west != Tile.TileType.Empty) {
                validWestEdge.Add (tileType.Key);
            }
        }
        return validWestEdge;
    }
    public List<Tile.TileType> southTileCheck (Tile.TileType tile_to_the_south, Dictionary<Tile.TileType, string[, ]> siblings) {
        List<Tile.TileType> validSouthEdge = new List<Tile.TileType> { };
        string[, ] southReq = siblings[tile_to_the_south];
        string[] northEdgeReq = new string[] { southReq[0, 0], southReq[0, 1], southReq[0, 2] };

        foreach (KeyValuePair<Tile.TileType, string[, ]> tileType in siblings) {
            string[, ] testerTile = tileType.Value; //Pulls out tile to test
            string[] testSouthEdge = new string[] { testerTile[2, 2], testerTile[2, 1], testerTile[2, 0] }; //pulls the left posibilites

            if (northEdgeReq.SequenceEqual (testSouthEdge) && tile_to_the_south != Tile.TileType.Empty) {
                validSouthEdge.Add (tileType.Key);
            }
        }
        return validSouthEdge;
    }
    public List<Tile.TileType> northTileCheck (Tile.TileType tile_to_the_north, Dictionary<Tile.TileType, string[, ]> siblings) {
        List<Tile.TileType> validNorthEdge = new List<Tile.TileType> { };
        string[, ] northReq = siblings[tile_to_the_north];
        string[] southEdgeReq = new string[] { northReq[2, 2], northReq[2, 1], northReq[2, 0] };

        foreach (KeyValuePair<Tile.TileType, string[, ]> tileType in siblings) {
            string[, ] testerTile = tileType.Value; //Pulls out tile to test
            string[] testNorthEdge = new string[] { testerTile[0, 0], testerTile[0, 1], testerTile[0, 2] }; //pulls the left posibilites

            if (southEdgeReq.SequenceEqual (testNorthEdge) && tile_to_the_north != Tile.TileType.Empty) {
                validNorthEdge.Add (tileType.Key);
            }
        }
        return validNorthEdge;
    }
    public List<Tile.TileType> eastTileCheck (Tile.TileType tile_to_the_east, Dictionary<Tile.TileType, string[, ]> siblings) {
        List<Tile.TileType> validEastEdge = new List<Tile.TileType> { };
        string[, ] eastReq = siblings[tile_to_the_east];
        string[] westEdgeReq = new string[] { eastReq[3, 2], eastReq[3, 1], eastReq[3, 0] };

        foreach (KeyValuePair<Tile.TileType, string[, ]> tileType in siblings) {
            string[, ] testerTile = tileType.Value; //Pulls out tile to test
            string[] testEastEdge = new string[] { testerTile[1, 0], testerTile[1, 1], testerTile[1, 2] }; //pulls the left posibilites

            if (westEdgeReq.SequenceEqual (testEastEdge) && tile_to_the_east != Tile.TileType.Empty) {
                validEastEdge.Add (tileType.Key);
            }
        }
        return validEastEdge;
    }

    public Tile.TileType reassignWestTile (Tile.TileType tile_to_the_east, int x, int y, Dictionary<Tile.TileType, string[, ]> siblings) {
        //      Debug.Log ("Checking tile Tile_" + x + "_" + y);
        List<Tile.TileType> validWestEdge = new List<Tile.TileType> { };
        List<Tile.TileType> validSouthEdge = new List<Tile.TileType> { };
        List<Tile.TileType> validNorthEdge = new List<Tile.TileType> { };
        List<Tile.TileType> validEastEdge = new List<Tile.TileType> { };
        List<Tile.TileType> valid = new List<Tile.TileType> { };

        List<Tile.TileType> temp = new List<Tile.TileType> { };
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
    public Tile.TileType reassignEastTile (Tile.TileType tile_to_the_west, int x, int y, Dictionary<Tile.TileType, string[, ]> siblings) {
        //      Debug.Log ("Checking tile Tile_" + x + "_" + y);
        List<Tile.TileType> validWestEdge = new List<Tile.TileType> { };
        List<Tile.TileType> validSouthEdge = new List<Tile.TileType> { };
        List<Tile.TileType> validNorthEdge = new List<Tile.TileType> { };
        List<Tile.TileType> validEastEdge = new List<Tile.TileType> { };
        List<Tile.TileType> valid = new List<Tile.TileType> { };

        List<Tile.TileType> temp = new List<Tile.TileType> { };
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
    public Tile.TileType reassignSouthTile (Tile.TileType tile_to_the_north, int x, int y, Dictionary<Tile.TileType, string[, ]> siblings) {
        //        Debug.Log ("Regenerating South tile Tile_" + x + "_" + y);

        List<Tile.TileType> validWestEdge = new List<Tile.TileType> { };
        List<Tile.TileType> validSouthEdge = new List<Tile.TileType> { };
        List<Tile.TileType> validNorthEdge = new List<Tile.TileType> { };
        List<Tile.TileType> validEastEdge = new List<Tile.TileType> { };
        List<Tile.TileType> valid = new List<Tile.TileType> { };

        List<Tile.TileType> temp = new List<Tile.TileType> { };

        validNorthEdge = northTileCheck (tile_to_the_north, siblings); //generates possibilites based on the tile to the North Of current tile
        if (x > 0) {
            validWestEdge = westTileCheck (tiles[x - 1, y].Type, siblings);

        }
        if (y > 0) {
            validSouthEdge = southTileCheck (tiles[x, y - 1].Type, siblings);
        }
        if (x < width - 1) {
            if (tiles[x + 1, y].Type != Tile.TileType.Empty) {
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
    public Tile.TileType reassignNorthTile (Tile.TileType tile_to_the_south, int x, int y, Dictionary<Tile.TileType, string[, ]> siblings) {
        //      Debug.Log ("Regenerating North tile Tile_" + x + "_" + y);

        List<Tile.TileType> validWestEdge = new List<Tile.TileType> { };
        List<Tile.TileType> validSouthEdge = new List<Tile.TileType> { };
        List<Tile.TileType> validNorthEdge = new List<Tile.TileType> { };
        List<Tile.TileType> validEastEdge = new List<Tile.TileType> { };
        List<Tile.TileType> valid = new List<Tile.TileType> { };

        List<Tile.TileType> temp = new List<Tile.TileType> { };

        validSouthEdge = southTileCheck (tile_to_the_south, siblings); //generates possibilites based on the tile to the North Of current tile
        if (x > 0) {
            validWestEdge = westTileCheck (tiles[x - 1, y].Type, siblings);

        }
        if (y > 0) {
            validNorthEdge = northTileCheck (tiles[x, y + 1].Type, siblings);
        }
        if (x < width - 1) {
            if (tiles[x + 1, y].Type != Tile.TileType.Empty) {
                validEastEdge = eastTileCheck (tiles[x + 1, y].Type, siblings);
            }
        }
        if (validSouthEdge.Count > 0) {
            foreach (var a in validSouthEdge) {
                temp.Add (a);
            }
        }
        //  Debug.Log ("RU TOPEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
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
    public void TestFourNeighbors (Tile tile_data) {
        Debug.Log ("Reassigning Neighbors of Tile_" + tile_data.X + "_" + tile_data.Y);
        if (tile_data.Y < height - 1) { //if tile to the north exists
            reassignNorthTile (tile_data.Type, tile_data.X, tile_data.Y + 1, neighbors);
        }
        if (tile_data.Y > 0) {
            reassignSouthTile (tile_data.Type, tile_data.X, tile_data.Y - 1, neighbors);
        }
        if (tile_data.X < width - 1) {
            reassignEastTile (tile_data.Type, tile_data.X + 1, tile_data.Y, neighbors);
        }
        if (tile_data.X > 0) {
            reassignWestTile (tile_data.Type, tile_data.X - 1, tile_data.Y, neighbors);
        }

    }

    public Dictionary<Tile.TileType, string[, ]> determineNeighbors (Dictionary<Tile.TileType, string[, ]> siblings, int x, int y) {
        siblings.Add (Tile.TileType.Empty, new string[4, 3] { { "emp", "emp", "emp" }, { "emp", "emp", "emp" }, { "emp", "emp", "emp" }, { "emp", "emp", "emp" } });

        siblings.Add (Tile.TileType.Generic, new string[4, 3] { { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" }, { "gen", "gen", "gen" } });

        siblings.Add (Tile.TileType.Grass, new string[4, 3] { { "gr", "gr", "gr" }, { "gr", "gr", "gr" }, { "gr", "gr", "gr" }, { "gr", "gr", "gr" } });

        siblings.Add (Tile.TileType.Dirt, new string[4, 3] { { "di", "di", "di" }, { "di", "di", "di" }, { "di", "di", "di" }, { "di", "di", "di" } });

        return siblings;
    }
}