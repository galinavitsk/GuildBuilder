using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Furniture {
    public Tile tile { get; protected set; } //BASE tile, but in practice large objects may occupy multiple tiles

    //ObjectType queried by visual system to know what sprite to render for this object
    public string objectType { get; protected set; }

    //multiplier for movement, value of 2 means twice as slow(half speed)
    //SPECIAL: IF movementCost=0 then the tile is impassable(e.g. a wall)
    float movementCost;
    int width;
    int height;
   public string sprite{ get; protected set; }


    Action<Furniture> cbOnChanged;
    protected Furniture () { }

    static public Furniture CreatePrototype (string objectType, string SpriteName, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeightbor = false) {
        Furniture obj = new Furniture ();
        obj.objectType = objectType;
        obj.movementCost = movementCost;
        obj.width = width;
        obj.height = height;
        obj.sprite = SpriteName;
        return obj;
    }

    static public Furniture PlaceInstance (Furniture proto, Tile tile) {
        Furniture obj = new Furniture ();
        obj.objectType = proto.objectType;
        obj.movementCost = proto.movementCost;
        obj.width = proto.width;
        obj.height = proto.height;
        obj.tile = tile;

        if (tile.PlaceObject (obj) == false) {
            return null;
        }

        return obj;

    }

    public void RegisterOnChangedCallback (Action<Furniture> callbackFunc) {
        cbOnChanged += callbackFunc;
    }
    public void UnregisterOnChangedCallback (Action<Furniture> callbackFunc) {
        cbOnChanged -= callbackFunc;
    }

}