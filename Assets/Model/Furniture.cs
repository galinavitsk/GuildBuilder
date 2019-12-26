using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture {
    public LandTile tile { get; protected set; } //BASE tile, but in practice large objects may occupy multiple tiles

    //ObjectType queried by visual system to know what sprite to render for this object
    public string objectType { get; protected set; }

    //multiplier for movement, value of 2 means twice as slow(half speed)
    //SPECIAL: IF movementCost=0 then the tile is impassable(e.g. a wall)
    float movementCost;
    int width;
    int height;

    public bool linksToNeightbor { get; protected set; }

    Action<Furniture> cbOnChanged;
    protected Furniture () { }

    static public Furniture CreatePrototype (string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeightbor = false) {
        Furniture obj = new Furniture ();
        obj.objectType = objectType;
        obj.movementCost = movementCost;
        obj.width = width;
        obj.height = height;
        obj.linksToNeightbor = linksToNeightbor;
        return obj;
    }

    static public Furniture PlaceInstance (Furniture proto, LandTile tile) {
        Furniture obj = new Furniture ();
        obj.objectType = proto.objectType;
        obj.movementCost = proto.movementCost;
        obj.width = proto.width;
        obj.height = proto.height;
        obj.tile = tile;
        obj.linksToNeightbor = proto.linksToNeightbor;

        if (tile.PlaceObject (obj) == false) {
            return null;
        }

        if (obj.linksToNeightbor) {
            //if furniture links itself to its neighbors inform our neighbours that they have a new neighbors
            int x=tile.X;
            int y=tile.Y;
            LandTile t;
        t = tile.world.GetTileAt (x, y + 1);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType) {
            t.furniture.cbOnChanged(t.furniture);
        }
        t = tile.world.GetTileAt (x + 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType) {
            t.furniture.cbOnChanged(t.furniture);
        }
        t = tile.world.GetTileAt (x, y - 1);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType) {
            t.furniture.cbOnChanged(t.furniture);
        }
        t = tile.world.GetTileAt (x - 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType) {
            t.furniture.cbOnChanged(t.furniture);
        }
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