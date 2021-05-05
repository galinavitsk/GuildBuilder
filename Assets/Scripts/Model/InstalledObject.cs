using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public enum ENTERABILITY { Yes, Never, Soon }

public class InstalledObject {

    public Vector3Int tile { get; protected set; } //BASE tile, but in practice large objects may occupy multiple tiles

    //ObjectType queried by visual system to know what sprite to render for this object
    public string objectType { get; protected set; }

    //multiplier for movement, value of 2 means twice as slow(half speed)
    //SPECIAL: IF movementCost=0 then the tile is impassable(e.g. a wall)
    public float movementCost;
    int width;
    int height;
    public string sprite { get; set; }

    Action<InstalledObject> cbOnChanged;
    public Func<int, int, bool> funcPositionValidation;

    public Dictionary<string, float> installedObjectParamenters;
    public Action<InstalledObject, float> updateActions;
    public Func<InstalledObject, ENTERABILITY> IsEnterable;

    public void Update (float deltaTime) {
        if (updateActions != null) {
            updateActions (this, deltaTime);
        }
    }
    public InstalledObject () { }

    protected InstalledObject (InstalledObject proto) {
        this.sprite = proto.sprite;
        this.objectType = proto.objectType;
        this.movementCost = proto.movementCost;
        this.width = proto.width;
        this.height = proto.height;
        this.funcPositionValidation = proto.funcPositionValidation;
        this.installedObjectParamenters = new Dictionary<string, float> (proto.installedObjectParamenters);
        if (proto.updateActions != null) {

            this.updateActions = (Action<InstalledObject, float>) proto.updateActions.Clone ();
        }
        if (proto.IsEnterable != null) {
            this.IsEnterable = (Func<InstalledObject, ENTERABILITY>) proto.IsEnterable.Clone ();
        }
    }

    virtual public InstalledObject Clone () {
        return new InstalledObject (this);
    }
    public InstalledObject (string objectType, string SpriteName, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeightbor = false) {
        this.objectType = objectType;
        this.movementCost = movementCost;
        this.width = width;
        this.height = height;
        this.sprite = SpriteName;
        this.funcPositionValidation += this.IsValidPosition;
        this.funcPositionValidation += this.DoorValidPosition;
        this.installedObjectParamenters = new Dictionary<string, float> ();
        this.IsEnterable+=this.EnterCheck;
        return;
    }

    static public InstalledObject PlaceInstance (InstalledObject proto, Vector3Int tile_position) {
        if (proto.funcPositionValidation (tile_position.x, tile_position.y) == false) {
            Debug.Log ("Tile position:" + tile_position.x + "_" + tile_position.y);
            Debug.LogError ("Invalid Function Position");
            return null;
        }
        InstalledObject obj = proto.Clone ();
        obj.tile = tile_position;
        return obj;

    }

    public void RegisterOnChangedCallback (Action<InstalledObject> callbackFunc) {
        cbOnChanged += callbackFunc;
    }
    public void UnregisterOnChangedCallback (Action<InstalledObject> callbackFunc) {
        cbOnChanged -= callbackFunc;
    }

    bool IsValidPosition (int x, int y) {
        if (WorldController.Instance.World.foundationGameMap.ContainsKey (new Vector3Int (x, y, 0)) == true ||
            x >= WorldController.Instance.World.Width - 1 || y >= WorldController.Instance.World.Height - 1 || x < 1 || y < 1) { return false; }

        return true;
    }
    bool DoorValidPosition (int x, int y) {
        if (objectType == "Door" || objectType == "Window") {
            Tilemap tilemapFoundation = WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ();
            Tilemap tilemapJobs=WorldController.Instance.tilemapJobs.GetComponent<Tilemap>();
            Dictionary<Vector3Int, InstalledObject> foundationGameMap = WorldController.Instance.World.foundationGameMap;
            Vector3Int tile_to_check=new Vector3Int(x,y,0);
            Vector3Int north_tile=new Vector3Int(x,y+1,0);
            Vector3Int south_tile=new Vector3Int(x,y-1,0);
            Vector3Int east_tile=new Vector3Int(x+1,y,0);
            Vector3Int west_tile=new Vector3Int(x-1,y,0);
            // Debug.Log ("Placing Door at position:" + x + "_" + y);
            //            Debug.Log (tilemapFoundation.GetTile (new Vector3Int (x - 1, y, 0)).name.ToString ().Contains ("Wall"));
            //IF There is already a wall there OR the tile is empty THEN check tiles around to check if they are walls/doors OR if they contain wall/door job
            if ((foundationGameMap.ContainsKey(tile_to_check) && tilemapFoundation.GetTile(tile_to_check).name.ToString().Contains("Wall")) ||
            foundationGameMap.ContainsKey(tile_to_check)==false){
                //VERTICAL TILE CHECK NORTH+SOUTH
                if(
                    (
                        (tilemapFoundation.GetTile(north_tile)!=null && (tilemapFoundation.GetTile(north_tile).name.ToString().Contains("Wall")||(tilemapFoundation.GetTile(north_tile).name.ToString().Contains("Door"))))
                        ||
                        (tilemapJobs.GetTile(north_tile)!=null && (tilemapJobs.GetTile(north_tile).name.ToString().Contains("Wall")||tilemapJobs.GetTile(north_tile).name.ToString().Contains("Door")))
                    )
                    &&
                    (
                        (tilemapFoundation.GetTile(south_tile)!=null && (tilemapFoundation.GetTile(south_tile).name.ToString().Contains("Wall")||tilemapFoundation.GetTile(south_tile).name.ToString().Contains("Door")))
                        ||
                        (tilemapJobs.GetTile(south_tile)!=null && (tilemapJobs.GetTile(south_tile).name.ToString().Contains("Wall")||tilemapJobs.GetTile(south_tile).name.ToString().Contains("Door")))
                    )
                ){
                    return true;
                }
                else if(
                    (
                        (tilemapFoundation.GetTile(west_tile)!=null && (tilemapFoundation.GetTile(west_tile).name.ToString().Contains("Wall")||tilemapFoundation.GetTile(west_tile).name.ToString().Contains("Door")))
                        ||
                        (tilemapJobs.GetTile(west_tile)!=null && (tilemapJobs.GetTile(west_tile).name.ToString().Contains("Wall")||tilemapJobs.GetTile(west_tile).name.ToString().Contains("Door")))
                    )
                    &&
                    (
                        (tilemapFoundation.GetTile(east_tile)!=null && (tilemapFoundation.GetTile(east_tile).name.ToString().Contains("Wall")||tilemapFoundation.GetTile(east_tile).name.ToString().Contains("Door")))
                        ||
                        (tilemapJobs.GetTile(east_tile)!=null && (tilemapJobs.GetTile(east_tile).name.ToString().Contains("Wall")||tilemapJobs.GetTile(east_tile).name.ToString().Contains("Door")))
                    )
                ){
                    return true;
                }

            }
        }
        return IsValidPosition (x, y);
    }

    public ENTERABILITY EnterCheck (InstalledObject installedO) {
        if (movementCost == 0) { return ENTERABILITY.Never; }
        if (objectType.Contains ("Door") && IsEnterable != null) {
            return IsEnterable (this);
        }
        return ENTERABILITY.Yes;

    }
}