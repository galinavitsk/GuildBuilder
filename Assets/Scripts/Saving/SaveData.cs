using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class SaveData {
    public TilemapSaveData LandscapeTilemap { get; set; }
    public SaveData () { }

}

[Serializable]
public class TilemapSaveData {

    public Dictionary<SerializableVector3Int, string> tilemapToSave { get; set; }
    public TilemapSaveData (Tilemap tilemap) {
        BoundsInt area = new BoundsInt (new Vector3Int (0, 0, 0), new Vector3Int (WorldController.Instance.World.Width, WorldController.Instance.World.Height, 1));
        for (int x = 0; x < WorldController.Instance.World.Width; x++) {
            for (int y = 0; y < WorldController.Instance.World.Height; y++) {
                Vector3Int tilePos = new Vector3Int (x, y, 0);
                if (tilemap.GetTile (tilePos) != null) { //Checks that the tile exists
                    if (tilemap.GetTile (tilePos).name != "") { //Checks if the tile is a RULETILE, Eliminates Floors that use single sprites, and takes their sprite as name
                        tilemapToSave.Add (tilePos,tilemap.GetSprite (tilePos).name.ToString ());
                    } else {
                        tilemapToSave.Add (tilePos,tilemap.GetTile (tilePos).name.ToString ());
                    }
                }
            }

        }
    }
}