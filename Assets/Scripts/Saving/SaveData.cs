using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class SaveData {
    public WorldSaveData WorldSaveData { get; set; }
    public TilemapSaveData LandscapeTilemap { get; set; }
    public TilemapSaveData FoundationTilemap { get; set; }
    public CharactersSaveData CharactersSaveData{ get; set; }
    public SaveData () { }

}

[Serializable]
public class WorldSaveData {
    public int width;
    public int height;
    public WorldSaveData (World world) {
        width = world.Width;
        height = world.Height;
    }
}

[Serializable]
public class TilemapSaveData {

    public Dictionary<SerializableVector3Int, string> tilemapToSave { get; set; }
    public TilemapSaveData (Tilemap tilemap, int tilemapheight) {
        tilemapToSave = new Dictionary<SerializableVector3Int, string> ();
        for (int x = 0; x < WorldController.Instance.World.Width; x++) {
            for (int y = 0; y < WorldController.Instance.World.Height; y++) {
                Vector3Int tilePos = new Vector3Int (x, y, tilemapheight);
                if (tilemap.GetTile (tilePos) != null) { //Checks that the tile exists
                    if (tilemap.GetTile (tilePos).name.ToString()=="") { //Checks if the tile is a RULETILE, Eliminates Floors that use single sprites, and takes their sprite as name
                        tilemapToSave.Add (tilePos, tilemap.GetSprite (tilePos).name.ToString ());
                    } else {
                        tilemapToSave.Add (tilePos, tilemap.GetTile (tilePos).name.ToString ());
                    }
                }
            }
        }
    }
}
[Serializable]
public class CharactersSaveData{
    public Dictionary<SerializableVector3Int, CharacterData> characters;
    public CharactersSaveData(){
        characters = new Dictionary<SerializableVector3Int, CharacterData>();
        foreach (Character c in WorldController.Instance.World.characters)
        {
            characters.Add(c.currTile, new CharacterData(c));
        }
    }
}
[Serializable]
public class CharacterData{
    public string name;
    public float speed;
    public float buildspeed;

    public CharacterData(Character c){
        name = c.name;
        speed = c.speed;
        buildspeed = c.buildtime;
    }
}