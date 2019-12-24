using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class WorldController : MonoBehaviour {
    public static WorldController Instance { get; protected set; }
    public World World { get; protected set; }
    Dictionary<LandTile, GameObject> tileGameObjectMap;
    Dictionary<Furniture, GameObject> furnitureGameObjectMap;
    Dictionary<string,Sprite> landscapeSprites;
    Dictionary<LandTile.TileType,string> landscapeSpritesNames;
	Dictionary<string,Sprite> foundationSprites;//sprites for walls, windows, doors
    // Start is called before the first frame update
    void Start () {

        foundationSprites = new Dictionary<string, Sprite>();
        landscapeSprites = new Dictionary<string, Sprite>();
        landscapeSpritesNames = new Dictionary<LandTile.TileType, string>();
        populatelandscapeSpritesNames();
        Sprite[] foundationsprites = Resources.LoadAll<Sprite>("Images/Foundation/");
        Sprite[] landscapesprites = Resources.LoadAll<Sprite>("Images/Landscape/");

        foreach (Sprite s in foundationsprites)
        {
            foundationSprites[s.name]=s;
        }
        foreach (Sprite s in landscapesprites)
        {
            landscapeSprites[s.name]=s;
        }

        if (Instance != null) {
            Debug.LogError ("There should not be two world controllers.");
        }
        Instance = this;
        World = new World ();
        World.RegisterFurnitureCreated (OnFurnitureCreated);
        tileGameObjectMap = new Dictionary<LandTile, GameObject> (); //Maps which GO is rendering which LandTile
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        //Create a GameObject for each tile so they show up visually
        for (int x = 0; x < World.Width; x++) {
            for (int y = 0; y < World.Height; y++) {
                GameObject tile_go = new GameObject ();
                LandTile tile_data = World.GetTileAt (x, y);
                tileGameObjectMap.Add (tile_data, tile_go);
                tile_go.name = "Tile_" + x + "_" + y;
                tile_go.transform.position = new Vector3 (tile_data.X, tile_data.Y, 0);
                tile_go.transform.SetParent (this.transform, true);
                SpriteRenderer tile_sr = tile_go.AddComponent<SpriteRenderer> ();
                tile_data.RegisterTileTypeChangedCallback (OnTileTypeChanged);
                if (tile_data.Type == LandTile.TileType.FullGrass) {
                    tile_sr.sprite = landscapeSprites["Grass_Full"];
                } else if (tile_data.Type == LandTile.TileType.FullDirt) {
                    tile_sr.sprite = landscapeSprites["Dirt_Full"];
                }
                else if (tile_data.Type == LandTile.TileType.LowGrass) {
                    tile_sr.sprite = landscapeSprites["LowGrass"];
                }
            }
        }
        
        World.RandomizeTiles ();
    }

    // Update is called once per frame
    void Update () { }

    //CURRENTLY THIS FUNCTION IS NOT USED
    void DestroyAllTileGameObjects () {
        //Destroys all visual tiles, not the actual data, used if changing levels/floors
        while (tileGameObjectMap.Count > 0) {
            LandTile tile_data = tileGameObjectMap.Keys.First ();
            GameObject tile_go = tileGameObjectMap[tile_data];
            tileGameObjectMap.Remove (tile_data);
            //Register callback so the gameobject gets updated when tile type changes
            tile_data.UnregisterTileTypeChangedCallback (OnTileTypeChanged);
            Destroy (tile_go);

        }
    }
    void OnTileTypeChanged (LandTile tile_data) {//LANDSCAPE
        if (tileGameObjectMap.ContainsKey (tile_data) == false) {
            Debug.LogError ("tileGameObjectMap doesn't contain tile_data -- forget to add the tile to the dictionary? Or unregister a callback?");
            return;
        }
        GameObject tile_go = tileGameObjectMap[tile_data];
        if (tile_go == null) {
            Debug.LogError ("tileGameObjectMap returned null-- forget to add the tile to the dictionary? Or unregister a callback?");
            return;
        }
        AssignSpriteToLandTile(tile_data,tile_go);
        
    }

    void AssignSpriteToLandTile(LandTile tile_data,GameObject tile_go){
        if(landscapeSpritesNames.ContainsKey(tile_data.Type)==true){//Sometimes there is some bug here where full grass tile gets placed instead of anything else
            //Debug.Log("Trying to assign this sprite:"+tile_data.Type);
            tile_go.GetComponent<SpriteRenderer> ().sprite = landscapeSprites[landscapeSpritesNames[tile_data.Type]];
        }        
        else {
            Debug.LogError ("OnTileTypeChanged-Unrecognized tiletype:"+tile_data.Type);
        }
    }
    public LandTile GetTileAtWorldCoord (Vector3 coord) {
        int x = Mathf.RoundToInt (coord.x);
        int y = Mathf.RoundToInt (coord.y);

        return WorldController.Instance.World.GetTileAt (x, y);
    }

    public void OnFurnitureCreated (Furniture obj) {
		// Create a visual GameObject linked to this data.

		// FIXME:Does not consider multi-tile objects nor rotated objects

		GameObject furniture_go = new GameObject ();
		furnitureGameObjectMap.Add ( obj, furniture_go );
		furniture_go.name = obj.objectType + "_" + obj.tile.X + "_" + obj.tile.Y;
		furniture_go.transform.position = new Vector3( obj.tile.X, obj.tile.Y, -1);
		furniture_go.transform.SetParent(this.transform, true);
		furniture_go.AddComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(obj);	// FIXME:currently assumes this is a wall
		obj.RegisterOnChangedCallback( OnFurnitureChanged );
    }

    Sprite GetSpriteForFurniture(Furniture obj){
        
        Debug.Log("Obj.linkstoNeightbor"+obj.linksToNeightbor);
        if(obj.linksToNeightbor==false){
        return foundationSprites[obj.objectType];
        }
        string spriteName = obj.objectType+"_";
        int x=obj.tile.X;
        int y=obj.tile.Y;
        //check for neightbors NESW
        LandTile t;
        t=World.GetTileAt(x,y+1);
        if(t!=null && t.installedObject!=null && t.installedObject.objectType==obj.objectType){
            spriteName +="N";
        }
        t=World.GetTileAt(x+1,y);
        if(t!=null && t.installedObject!=null && t.installedObject.objectType==obj.objectType){
            spriteName +="E";
        }
        t=World.GetTileAt(x,y-1);
        if(t!=null && t.installedObject!=null && t.installedObject.objectType==obj.objectType){
            spriteName +="S";
        }
        t=World.GetTileAt(x-1,y);
        if(t!=null && t.installedObject!=null && t.installedObject.objectType==obj.objectType){
            spriteName +="W";
        }
        if(foundationSprites.ContainsKey(spriteName)==false){
            Debug.LogError("GetSpriteForFurniture--foundationSprites does not contain the key:"+spriteName);
            return null;
        }
        return foundationSprites[spriteName];
    }
    void OnFurnitureChanged (Furniture obj) {
        Debug.LogError ("OnFurnitureChanged not implemented");
    }

    void populatelandscapeSpritesNames( ){
        landscapeSpritesNames.Add(LandTile.TileType.LowGrass,"LowGrass");
        //Grass Tiles
        landscapeSpritesNames.Add(LandTile.TileType.FullGrass,"Grass_Full");
        landscapeSpritesNames.Add(LandTile.TileType.TopGrass,"Grass_Top");
        landscapeSpritesNames.Add(LandTile.TileType.BottomGrass,"Grass_Bottom");
        landscapeSpritesNames.Add(LandTile.TileType.RightGrass,"Grass_Right");
        landscapeSpritesNames.Add(LandTile.TileType.LeftGrass,"Grass_Left");
        landscapeSpritesNames.Add(LandTile.TileType.BLGrass,"Grass_BL");
        landscapeSpritesNames.Add(LandTile.TileType.BRGrass,"Grass_BR");
        landscapeSpritesNames.Add(LandTile.TileType.TLGrass,"Grass_TL");
        landscapeSpritesNames.Add(LandTile.TileType.TRGrass,"Grass_TR");
        //Dirt Tiles
        landscapeSpritesNames.Add(LandTile.TileType.FullDirt,"Dirt_Full");
        landscapeSpritesNames.Add(LandTile.TileType.TopDirt,"Dirt_Top");
        landscapeSpritesNames.Add(LandTile.TileType.BottomDirt,"Dirt_Bottom");
        landscapeSpritesNames.Add(LandTile.TileType.RightDirt,"Dirt_Right");
        landscapeSpritesNames.Add(LandTile.TileType.LeftDirt,"Dirt_Left");
        landscapeSpritesNames.Add(LandTile.TileType.BLDirt,"Dirt_BL");
        landscapeSpritesNames.Add(LandTile.TileType.BRDirt,"Dirt_BR");
        landscapeSpritesNames.Add(LandTile.TileType.TLDirt,"Dirt_TL");
        landscapeSpritesNames.Add(LandTile.TileType.TRDirt,"Dirt_TR");
    }
}