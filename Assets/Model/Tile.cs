using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tile {
    public enum TileType {
        Grass,
        Dirt,
        Generic,
        Empty,
        ErrorTile
    }
    public enum TileBasicType { Generic, Grass, Dirt, Empty }
    
    TileType type = TileType.Generic;
   
    Action<Tile> cbTileTypeChanged;
    public TileType Type {
        get { return type; }
        set {
            TileType oldType = type;
            type = value;

            //Calls a function and sets this tile as a parameter
            if (cbTileTypeChanged != null && oldType != type) { cbTileTypeChanged (this); }
            
        }
    }


    LooseObject looseObject;
    public Furniture furniture { get; protected set; }

    public World world { get; protected set; }
    int x;
    public int X {
        get { return x; }
    }
    int y;
    public int Y {
        get { return y; }
    }

    public Tile (World world, int x, int y) {
        this.world = world;
        this.x = x;
        this.y = y;
    }

    //FUNCTIONS TO CALL WHEN TILE TYPE CHANGES
    public void RegisterTileTypeChangedCallback (Action<Tile> callback) {
        cbTileTypeChanged += callback;

    }
    public void UnregisterTileTypeChangedCallback (Action<Tile> callback) {
        cbTileTypeChanged -= callback;

    }

    public bool PlaceObject (Furniture objInstance) {
        if (objInstance == null) {
            furniture = null;
            return true;
        }
        if (furniture != null) {
            Debug.LogError ("Trying to assign an InstalledObject to a tile that already has one!");
            return false;
        }
        furniture = objInstance;
        return true;

    }

}