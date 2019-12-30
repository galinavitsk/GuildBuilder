using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InstalledObject {
    public TileBase tile { get; protected set; } //BASE tile, but in practice large objects may occupy multiple tiles

    //ObjectType queried by visual system to know what sprite to render for this object
    public string objectType { get; protected set; }

    //multiplier for movement, value of 2 means twice as slow(half speed)
    //SPECIAL: IF movementCost=0 then the tile is impassable(e.g. a wall)
    float movementCost;
    int width;
    int height;
    public string sprite { get; protected set; }

    Action<InstalledObject> cbOnChanged;
    Func<int, int, bool> funcPositionValidation;
    protected InstalledObject () { }

    static public InstalledObject CreatePrototype (string objectType, string SpriteName, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeightbor = false) {
        InstalledObject obj = new InstalledObject ();
        obj.objectType = objectType;
        obj.movementCost = movementCost;
        obj.width = width;
        obj.height = height;
        obj.sprite = SpriteName;
        obj.funcPositionValidation = obj.IsValidPosition;
        return obj;
    }

    static public InstalledObject PlaceInstance (InstalledObject proto, TileBase tile, Vector3Int tile_position) {
        if (proto.funcPositionValidation (tile_position.x, tile_position.y) == false) {
            Debug.Log ("Tile position:" + tile_position.x + "_" + tile_position.y);
            Debug.LogError ("Invalid Function Position");
            return null;
        }
        InstalledObject obj = new InstalledObject ();
        if (proto.objectType == "Door") { proto.DoorValidPosition (tile_position.x, tile_position.y, obj); } else { obj.sprite = proto.sprite; }
        obj.objectType = proto.objectType;
        obj.movementCost = proto.movementCost;
        obj.width = proto.width;
        obj.height = proto.height;

        obj.tile = tile;

        return obj;

    }

    public void RegisterOnChangedCallback (Action<InstalledObject> callbackFunc) {
        cbOnChanged += callbackFunc;
    }
    public void UnregisterOnChangedCallback (Action<InstalledObject> callbackFunc) {
        cbOnChanged -= callbackFunc;
    }

    public bool IsValidPosition (int x, int y) {
        if (x >= WorldController.Instance.World.Width - 1 || y >= WorldController.Instance.World.Height - 1 || x < 1 || y < 1) { return false; }
        //TODO: Implement Position validation
        return true;
    }
    public void DoorValidPosition (int x, int y, InstalledObject obj) {
        if (IsValidPosition (x, y) == true) { //If general position is valid
            Tilemap tilemapFoundation = WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ();
            Debug.Log ("Placing Door at position:" + x + "_" + y);
            //            Debug.Log (tilemapFoundation.GetTile (new Vector3Int (x - 1, y, 0)).name.ToString ().Contains ("Wall"));
            if (tilemapFoundation.GetTile (new Vector3Int (x - 1, y, 0)) != null &&
                tilemapFoundation.GetTile (new Vector3Int (x + 1, y, 0)) != null &&
                tilemapFoundation.GetTile (new Vector3Int (x - 1, y, 0)).name.ToString ().Contains ("Wall") == true &&
                tilemapFoundation.GetTile (new Vector3Int (x + 1, y, 0)).name.ToString ().Contains ("Wall") == true) {
                Debug.Log ("Placing Door with sprite EW");
                obj.sprite = "Door_EW";
            } else if (
                tilemapFoundation.GetTile (new Vector3Int (x, y - 1, 0)) != null &&
                tilemapFoundation.GetTile (new Vector3Int (x, y + 1, 0)) != null &&
                tilemapFoundation.GetTile (new Vector3Int (x, y - 1, 0)).name.ToString ().Contains ("Wall") == true &&
                tilemapFoundation.GetTile (new Vector3Int (x, y + 1, 0)).name.ToString ().Contains ("Wall") == true) {
                obj.sprite = "Door_NS";
            }
            else{
                Debug.LogError("Can't place Door here");
            }
        }

    }

}