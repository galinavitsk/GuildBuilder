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
                        tiles[x, y].Type = LandTile.TileType.Generic;
                        break;
                }
                if (x != 0 || y != 0) {
                    //FIXME:WHY WON"T THIS WORK
                    neighbors = new Dictionary<LandTile.TileType, string[, ]> { };
                    neighbors = determineNeighbors (neighbors, x, y);
                    tiles[x, y].Type = NeighborCheckInitial (x, y, neighbors);
                    LandTile.TileType tile_under = LandTile.TileType.Empty;
                    LandTile.TileType tile_left = LandTile.TileType.Empty;
                    LandTile.TileType tile_top = LandTile.TileType.Empty;
                    LandTile.TileType tile_right = LandTile.TileType.Empty;
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
                    List<LandTile.TileType> validLeftEdge = new List<LandTile.TileType> { };
                    List<LandTile.TileType> validBottomEdge = new List<LandTile.TileType> { };
                    List<LandTile.TileType> validRightEdge = new List<LandTile.TileType> { };
                    List<LandTile.TileType> validTopEdge = new List<LandTile.TileType> { };
                    List<LandTile.TileType> valid = new List<LandTile.TileType> { };

                    if (tiles[x, y].Type == LandTile.TileType.ErrorTile) {
                        //Activated when a tile gets assigned as an error tile.
                        if (tiles[x - 1, y].BasicType == tiles[x, y - 1].BasicType) {
                            //IF Left and Under Tile are both same basic type
                            if (tiles[x - 1, y].BasicType == LandTile.TileBasicType.Grass) {
                                tiles[x, y].Type = LandTile.TileType.FullGrass;
                                tiles[x, y - 1].Type = ReassignUnderTile (tiles[x, y].Type, x, (y - 1), neighbors);
                                tiles[x - 1, y].Type = ReassignLeftTile (tiles[x, y].Type, (x - 1), y, neighbors);
                            }
                            if (tiles[x - 1, y].BasicType == LandTile.TileBasicType.Dirt) {
                                tiles[x, y].Type = LandTile.TileType.FullDirt;

                                tiles[x, y - 1].Type = ReassignUnderTile (tiles[x, y].Type, x, (y - 1), neighbors);
                                tiles[x - 1, y].Type = ReassignLeftTile (tiles[x, y].Type, (x - 1), y, neighbors);
                            }
                        } else {
                            tiles[x, y].Type = LandTile.TileType.Generic;
                            tiles[x, y - 1].Type = ReassignUnderTile (tiles[x, y].Type, x, (y - 1), neighbors);
                            tiles[x - 1, y].Type = ReassignLeftTile (tiles[x, y].Type, (x - 1), y, neighbors);
                            //If the left tile and under tiles are NOT the same basic type
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

        LandTile.TileType tile_under = LandTile.TileType.Empty;
        LandTile.TileType tile_left = LandTile.TileType.Empty;
        if (y > 0) { //skip check if y is 0
            if (tiles[x, y - 1].Type == LandTile.TileType.ErrorTile) { tile_under = LandTile.TileType.Empty; } else {
                //Debug.Log ("Tile Under :" + tiles[x, y - 1].Type);
                tile_under = tiles[x, y - 1].Type;
            }
        } else if (y == 0) {
            tile_under = LandTile.TileType.Empty;
        }
        if (x > 0) { //skip check if x is 0
            if (tiles[x - 1, y].Type == LandTile.TileType.ErrorTile) { tile_left = LandTile.TileType.Empty; } else {
                tile_left = tiles[x - 1, y].Type;
            }
        } else if (x == 0) {
            tile_left = LandTile.TileType.Empty;
        }

        List<LandTile.TileType> validLeftEdge = new List<LandTile.TileType> { };
        validLeftEdge = leftCheck (tile_left, siblings);
        List<LandTile.TileType> validBottomEdge = new List<LandTile.TileType> { };
        validBottomEdge = underCheck (tile_under, siblings);
        List<LandTile.TileType> valid = new List<LandTile.TileType> { };
        if (validLeftEdge.Count > 0 && (y == 0 || tile_under == LandTile.TileType.Empty)) {
            foreach (var a in validLeftEdge) {
                valid.Add (a);
            }
        }
        if (validBottomEdge.Count > 0 && (x == 0 || tile_left == LandTile.TileType.Empty)) {
            foreach (var a in validBottomEdge) {
                valid.Add (a);
            }
        }
        if (validBottomEdge.Count > 0 && validLeftEdge.Count > 0) {
            foreach (var a in validBottomEdge) {
                if (validLeftEdge.Contains (a)) {
                    valid.Add (a);

                }
            }
        }
        if (valid.Count != 0) {
            int random = UnityEngine.Random.Range (0, valid.Count);
            //Debug.Log ("Valid Count:" + valid.Count + "   Random:" + random + " Result:" + valid[random]);
            tiles[x, y].Type = valid[random];
        } else {
            Debug.Log ("There has been an Error Generating a tile");
            tiles[x, y].Type = LandTile.TileType.ErrorTile;
        }
        return tiles[x, y].Type;

    }

    public LandTile.TileType fourNeighborCheck (int x, int y, LandTile.TileType checkedtile, Dictionary<LandTile.TileType, string[, ]> siblings) {

        //pulls the right requirements for the tile to the left of the current tile
        LandTile.TileType tile_under = LandTile.TileType.Empty;
        LandTile.TileType tile_left = LandTile.TileType.Empty;
        LandTile.TileType tile_top = LandTile.TileType.Empty;
        LandTile.TileType tile_right = LandTile.TileType.Empty;
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
        List<LandTile.TileType> validLeftEdge = new List<LandTile.TileType> { };
        validLeftEdge = leftCheck (tile_left, siblings);
        List<LandTile.TileType> validBottomEdge = new List<LandTile.TileType> { };
        validBottomEdge = underCheck (tile_under, siblings);
        List<LandTile.TileType> validRightEdge = new List<LandTile.TileType> { };
        validRightEdge = rightCheck (tile_right, siblings);
        List<LandTile.TileType> validTopEdge = new List<LandTile.TileType> { };
        validTopEdge = overCheck (tile_top, siblings);
        List<LandTile.TileType> valid = new List<LandTile.TileType> { };

        if (valid.Count != 0) {
            int random = UnityEngine.Random.Range (0, valid.Count);
            Debug.Log ("Valid Count:" + valid.Count + "   Random:" + random + " Result:" + valid[random]);
            tiles[x, y].Type = valid[random];
        } else {
            Debug.Log ("There has been an Error Generating a tile");
            tiles[x, y].Type = LandTile.TileType.ErrorTile;
        }

        return tiles[x, y].Type;
    }

    public List<LandTile.TileType> leftCheck (LandTile.TileType tile_left, Dictionary<LandTile.TileType, string[, ]> siblings) {

        List<LandTile.TileType> validLeftEdge = new List<LandTile.TileType> { };
        string[, ] leftRequirements = siblings[tile_left]; //Pulls out requirements for the tile to the left
        string[] rightEdge = new string[] { leftRequirements[1, 0], leftRequirements[1, 1], leftRequirements[1, 2] }; //pulls out the right edge requirements of tile_left
        foreach (KeyValuePair<LandTile.TileType, string[, ]> tileType in siblings) {
            string[, ] testerTile = tileType.Value; //Pulls out tile to test
            string[] testLeftEdge = new string[] { testerTile[3, 2], testerTile[3, 1], testerTile[3, 0] }; //pulls the left posibilites

            if (rightEdge.SequenceEqual (testLeftEdge) && tile_left != LandTile.TileType.Empty) {
                validLeftEdge.Add (tileType.Key);
            }
        }
        return validLeftEdge;
    }
    public List<LandTile.TileType> underCheck (LandTile.TileType tile_under, Dictionary<LandTile.TileType, string[, ]> siblings) {
        List<LandTile.TileType> validBottomEdge = new List<LandTile.TileType> { };
        //Debug.Log ("Tile_under Type:" + tile_under);
        string[, ] underRequirements = siblings[tile_under];
        string[] topEdge = new string[] { underRequirements[0, 0], underRequirements[0, 1], underRequirements[0, 2] };

        foreach (KeyValuePair<LandTile.TileType, string[, ]> tileType in siblings) {
            string[, ] testerTile = tileType.Value; //Pulls out tile to test
            string[] testBottomEdge = new string[] { testerTile[2, 2], testerTile[2, 1], testerTile[2, 0] }; //pulls the left posibilites

            if (topEdge.SequenceEqual (testBottomEdge) && tile_under != LandTile.TileType.Empty) {
                validBottomEdge.Add (tileType.Key);
            }
        }
        return validBottomEdge;
    }
    public List<LandTile.TileType> overCheck (LandTile.TileType tile_over, Dictionary<LandTile.TileType, string[, ]> siblings) {
        List<LandTile.TileType> validTopEdge = new List<LandTile.TileType> { };
        string[, ] overRequirements = siblings[tile_over];
        string[] bottomEdge = new string[] { overRequirements[2, 2], overRequirements[2, 1], overRequirements[2, 0] };

        foreach (KeyValuePair<LandTile.TileType, string[, ]> tileType in siblings) {
            string[, ] testerTile = tileType.Value; //Pulls out tile to test
            string[] testTopEdge = new string[] { testerTile[0, 0], testerTile[0, 1], testerTile[0, 2] }; //pulls the left posibilites

            if (bottomEdge.SequenceEqual (testTopEdge) && tile_over != LandTile.TileType.Empty) {
                validTopEdge.Add (tileType.Key);
            }
        }
        return validTopEdge;
    }
    public List<LandTile.TileType> rightCheck (LandTile.TileType tile_right, Dictionary<LandTile.TileType, string[, ]> siblings) {
        List<LandTile.TileType> validRightEdge = new List<LandTile.TileType> { };
        string[, ] rightRequirements = siblings[tile_right];
        string[] leftEdge = new string[] { rightRequirements[1, 0], rightRequirements[1, 1], rightRequirements[1, 2] };

        foreach (KeyValuePair<LandTile.TileType, string[, ]> tileType in siblings) {
            string[, ] testerTile = tileType.Value; //Pulls out tile to test
            string[] testRightEdge = new string[] { testerTile[3, 2], testerTile[3, 1], testerTile[3, 0] }; //pulls the left posibilites

            if (leftEdge.SequenceEqual (testRightEdge) && tile_right != LandTile.TileType.Empty) {
                validRightEdge.Add (tileType.Key);
            }
        }
        return validRightEdge;
    }

    public LandTile.TileType ReassignLeftTile (LandTile.TileType tile_right, int x, int y, Dictionary<LandTile.TileType, string[, ]> siblings) {

        List<LandTile.TileType> validLeftEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validBottomEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validTopEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validRightEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> valid = new List<LandTile.TileType> { };

        List<LandTile.TileType> temp = new List<LandTile.TileType> { };
        validRightEdge = rightCheck (tile_right, siblings);
        if (x > 0) {
            //Checks if there is a tile to the left
            validLeftEdge = leftCheck (tiles[x, y].Type, siblings);
        }
        if (y > 0) {
            validBottomEdge = underCheck (tiles[x, y].Type, siblings);
        }
        if (y < height) {
            validTopEdge = overCheck (tiles[x, y].Type, siblings);
        }
        if (validRightEdge.Count > 0) {
            foreach (var a in validRightEdge) {
                temp.Add (a);
            }
        }
        Debug.Log ("RIGHTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validLeftEdge.Count > 0) {
            foreach (var a in validLeftEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
            temp.Clear ();
            temp = valid.ToList ();
            valid.Clear ();
        }
        Debug.Log ("LEFTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validBottomEdge.Count > 0) {
            foreach (var a in validBottomEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
            temp.Clear ();
            temp = valid.ToList ();
            valid.Clear ();

        }
        Debug.Log ("BOTTOMEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validTopEdge.Count > 0) {
            foreach (var a in validTopEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
        }
        Debug.Log ("TOPEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (valid.Count == 0) {
            valid = temp.ToList ();
        }
        int random = UnityEngine.Random.Range (0, valid.Count);
        //Debug.Log ("Valid Count:" + valid.Count + "   Random:" + random + " Result:" + valid[random]);
        tiles[x, y].Type = valid[random];
        return tiles[x, y].Type;

    }

    public LandTile.TileType ReassignUnderTile (LandTile.TileType tile_under, int x, int y, Dictionary<LandTile.TileType, string[, ]> siblings) {

        List<LandTile.TileType> validLeftEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validBottomEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validTopEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> validRightEdge = new List<LandTile.TileType> { };
        List<LandTile.TileType> valid = new List<LandTile.TileType> { };

        List<LandTile.TileType> temp = new List<LandTile.TileType> { };
        validTopEdge = overCheck (tile_under, siblings);
        if (x > 0) {
            validLeftEdge = leftCheck (tiles[x, y].Type, siblings);
        }
        if (y > 0) {
            validBottomEdge = underCheck (tiles[x, y].Type, siblings);
        }
        if (x < width) {
            validRightEdge = rightCheck (tiles[x, y].Type, siblings);
        }
        if (validTopEdge.Count > 0) {
            foreach (var a in validTopEdge) {
                temp.Add (a);
            }
        }
        Debug.Log ("RU TOPEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validLeftEdge.Count > 0) {
            foreach (var a in validLeftEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
            Debug.Log ("FIRST RU LEFTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
            temp.Clear ();
            temp = valid.ToList ();
            valid.Clear ();
        }
        Debug.Log ("RU LEFTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);

        if (validBottomEdge.Count > 0) {
            foreach (var a in validBottomEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
            temp.Clear ();
            temp = valid.ToList ();
            valid.Clear ();
        }
        Debug.Log ("RU BOTTOMEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (validRightEdge.Count > 0) {
            foreach (var a in validRightEdge) {
                if (temp.Contains (a)) {
                    valid.Add (a);
                }
            }
        }
        Debug.Log ("RU RIGHTEDGE Valid Count:" + valid.Count + "   Temp Count:" + temp.Count);
        if (valid.Count == 0) {
            valid = temp.ToList ();
        }
        int random = UnityEngine.Random.Range (0, valid.Count);
        Debug.Log ("Valid Count:" + valid.Count + "   Temp Count:" + temp.Count + "  Random:" + random);
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