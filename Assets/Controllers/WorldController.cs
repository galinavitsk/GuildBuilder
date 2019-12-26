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
    Dictionary<string, Sprite> landscapeSprites;
    Dictionary<LandTile.TileType, string> landscapeSpritesNames;
    Dictionary<string, Sprite> furnitureSprites; //sprites for walls, windows, doors
    // Start is called before the first frame update
    void Start () {

        furnitureSprites = new Dictionary<string, Sprite> ();
        landscapeSprites = new Dictionary<string, Sprite> ();
        landscapeSpritesNames = new Dictionary<LandTile.TileType, string> ();
        Sprite[] furnituresprites = Resources.LoadAll<Sprite> ("Images/Furniture/");
        Sprite[] landscapesprites = Resources.LoadAll<Sprite> ("Images/Landscape/");
        
        populatelandscapeSpritesNames ();

        foreach (Sprite s in furnituresprites) {
            furnitureSprites[s.name] = s;
        }
        foreach (Sprite s in landscapesprites) {
            landscapeSprites[s.name] = s;
        }

        if (Instance != null) {
            Debug.LogError ("There should not be two world controllers.");
        }
        Instance = this;
        World = new World ();
        World.RegisterFurnitureCreated (OnFurnitureCreated);
        tileGameObjectMap = new Dictionary<LandTile, GameObject> (); //Maps which GO is rendering which LandTile
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject> ();

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
                } else if (tile_data.Type == LandTile.TileType.Generic) {
                    tile_sr.sprite = landscapeSprites["Generic"];
                }
            }
        }

        World.RandomizeTiles ();
    }

    // Update is called once per frame
    void Update () { }

    //CURRENTLY THIS FUNCTION IS NOT USED
    public void DestroyAllTileGameObjects () {
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
    void OnTileTypeChanged (LandTile tile_data) { //LANDSCAPE
        if (tileGameObjectMap.ContainsKey (tile_data) == false) {
            Debug.LogError ("tileGameObjectMap doesn't contain tile_data -- forget to add the tile to the dictionary? Or unregister a callback?");
            return;
        }
        GameObject tile_go = tileGameObjectMap[tile_data];
        if (tile_go == null) {
            Debug.LogError ("tileGameObjectMap returned null-- forget to add the tile to the dictionary? Or unregister a callback?");
            return;
        }
        AssignSpriteToLandTile (tile_data, tile_go);

    }

    void AssignSpriteToLandTile (LandTile tile_data, GameObject tile_go) {
        if (landscapeSpritesNames.ContainsKey (tile_data.Type) == true) { //Sometimes there is some bug here where full grass tile gets placed instead of anything else
            Debug.Log("Trying to assign this sprite:"+tile_data.Type);
            tile_go.GetComponent<SpriteRenderer> ().sprite = landscapeSprites[landscapeSpritesNames[tile_data.Type]];
        } else {
            Debug.LogError ("OnTileTypeChanged-Unrecognized tiletype:" + tile_data.Type);
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
        furnitureGameObjectMap.Add (obj, furniture_go);
        furniture_go.name = obj.objectType + "_" + obj.tile.X + "_" + obj.tile.Y;
        furniture_go.transform.position = new Vector3 (obj.tile.X, obj.tile.Y, -1);
        furniture_go.transform.SetParent (this.transform, true);
        furniture_go.AddComponent<SpriteRenderer> ().sprite = GetSpriteForFurniture (obj); // FIXME:currently assumes this is a wall
        obj.RegisterOnChangedCallback (OnFurnitureChanged);
    }

    Sprite GetSpriteForFurniture (Furniture obj) {

        //Debug.Log ("Obj.linkstoNeightbor" + obj.linksToNeightbor);
        if (obj.linksToNeightbor == false) {
            return furnitureSprites[obj.objectType];
        }
        string spriteName = obj.objectType + "_";
        int x = obj.tile.X;
        int y = obj.tile.Y;
        //check for neightbors NESW
        LandTile t;
        t = World.GetTileAt (x, y + 1);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType) {
            spriteName += "N";
        }
        t = World.GetTileAt (x + 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType) {
            spriteName += "E";
        }
        t = World.GetTileAt (x, y - 1);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType) {
            spriteName += "S";
        }
        t = World.GetTileAt (x - 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType) {
            spriteName += "W";
        }
        if (furnitureSprites.ContainsKey (spriteName) == false) {
            Debug.LogError ("GetSpriteForFurniture--furnitureSprites does not contain the key:" + spriteName);
            return null;
        }
        return furnitureSprites[spriteName];
    }
    void OnFurnitureChanged (Furniture obj) {
        if(furnitureGameObjectMap.ContainsKey(obj)==false){
            Debug.LogError("Trying to change visuals for furniture not in the map");
        }
        GameObject furn_obj=furnitureGameObjectMap[obj];
        furn_obj.GetComponent<SpriteRenderer> ().sprite = GetSpriteForFurniture (obj);
    }

    void populatelandscapeSpritesNames () {
        
        landscapeSpritesNames.Add (LandTile.TileType.ErrorTile, "ErrorTile");
        landscapeSpritesNames.Add (LandTile.TileType.Generic, "Generic");
        //Grass Tiles
        landscapeSpritesNames.Add (LandTile.TileType.FullGrass, "Grass_Full");
        landscapeSpritesNames.Add (LandTile.TileType.TopGrass, "Grass_Top");
        landscapeSpritesNames.Add (LandTile.TileType.BottomGrass, "Grass_Bottom");
        landscapeSpritesNames.Add (LandTile.TileType.RightGrass, "Grass_Right");
        landscapeSpritesNames.Add (LandTile.TileType.LeftGrass, "Grass_Left");
        landscapeSpritesNames.Add (LandTile.TileType.BLGrass, "Grass_BL");
        landscapeSpritesNames.Add (LandTile.TileType.BRGrass, "Grass_BR");
        landscapeSpritesNames.Add (LandTile.TileType.TLGrass, "Grass_TL");
        landscapeSpritesNames.Add (LandTile.TileType.TRGrass, "Grass_TR");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_S, "Grass_S");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_NS,"Grass_NS");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_N,"Grass_N");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_E,"Grass_E");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_EW,"Grass_EW");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_W,"Grass_W");
        landscapeSpritesNames.Add (LandTile.TileType.Grass,"Grass_");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_NE,"Grass_NE");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_NES,"Grass_NES");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_ES,"Grass_ES");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_ESW,"Grass_ESW");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_NESW,"Grass_NESW");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_NEW,"Grass_NEW");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_SW,"Grass_SW");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_NSW,"Grass_NSW");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_NW,"Grass_NW");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Dia_SE,"Grass_Dia_SE");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Full_NES_T,"Grass_Full_NES_T");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Full_NES_B,"Grass_Full_NES_B");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Dia_NE,"Grass_Dia_NE");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Full_ESW_L,"Grass_Full_ESW_L");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_IC_TL,"Grass_IC_TL");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_IC_BL,"Grass_IC_BL");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Full_NEW_L,"Grass_Full_NEW_L");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Full_ESW_R,"Grass_Full_ESW_R");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_IC_TR,"Grass_IC_TR");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_IC_BR,"Grass_IC_BR");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Full_NEW_R,"Grass_Full_NEW_R");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Dia_SW,"Grass_Dia_SW");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Full_NSW_T,"Grass_Full_NSW_T");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Full_NSW_B,"Grass_Full_NSW_B");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Dia_NW,"Grass_Dia_NW");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Exit_Top,"Grass_Exit_Top");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Exit_Right,"Grass_Exit_Right");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Exit_Left,"Grass_Exit_Left");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Exit_Bottom,"Grass_Exit_Bottom");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Full_SW_NE,"Grass_Full_SW_NE");
        landscapeSpritesNames.Add (LandTile.TileType.Grass_Full_NW_SE,"Grass_Full_NW_SE");
        //Dirt Tiles
        
        landscapeSpritesNames.Add (LandTile.TileType.FullDirt, "Dirt_Full");
        landscapeSpritesNames.Add (LandTile.TileType.TopDirt, "Dirt_Top");
        landscapeSpritesNames.Add (LandTile.TileType.BottomDirt, "Dirt_Bottom");
        landscapeSpritesNames.Add (LandTile.TileType.RightDirt, "Dirt_Right");
        landscapeSpritesNames.Add (LandTile.TileType.LeftDirt, "Dirt_Left");
        landscapeSpritesNames.Add (LandTile.TileType.BLDirt, "Dirt_BL");
        landscapeSpritesNames.Add (LandTile.TileType.BRDirt, "Dirt_BR");
        landscapeSpritesNames.Add (LandTile.TileType.TLDirt, "Dirt_TL");
        landscapeSpritesNames.Add (LandTile.TileType.TRDirt, "Dirt_TR");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_S, "Dirt_S");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_NS,"Dirt_NS");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_N,"Dirt_N");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_E,"Dirt_E");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_EW,"Dirt_EW");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_W,"Dirt_W");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt,"Dirt_");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_NE,"Dirt_NE");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_NES,"Dirt_NES");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_ES,"Dirt_ES");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_ESW,"Dirt_ESW");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_NESW,"Dirt_NESW");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_NEW,"Dirt_NEW");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_SW,"Dirt_SW");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_NSW,"Dirt_NSW");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_NW,"Dirt_NW");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Dia_SE,"Dirt_Dia_SE");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Full_NES_T,"Dirt_Full_NES_T");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Full_NES_B,"Dirt_Full_NES_B");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Dia_NE,"Dirt_Dia_NE");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Full_ESW_L,"Dirt_Full_ESW_L");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_IC_TL,"Dirt_IC_TL");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_IC_BL,"Dirt_IC_BL");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Full_NEW_L,"Dirt_Full_NEW_L");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Full_ESW_R,"Dirt_Full_ESW_R");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_IC_TR,"Dirt_IC_TR");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_IC_BR,"Dirt_IC_BR");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Full_NEW_R,"Dirt_Full_NEW_R");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Dia_SW,"Dirt_Dia_SW");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Full_NSW_T,"Dirt_Full_NSW_T");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Full_NSW_B,"Dirt_Full_NSW_B");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Dia_NW,"Dirt_Dia_NW");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Exit_Top,"Dirt_Exit_Top");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Exit_Right,"Dirt_Exit_Right");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Exit_Left,"Dirt_Exit_Left");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Exit_Bottom,"Dirt_Exit_Bottom");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Full_SW_NE,"Dirt_Full_SW_NE");
        landscapeSpritesNames.Add (LandTile.TileType.Dirt_Full_NW_SE,"Dirt_Full_NW_SE");
    }

}