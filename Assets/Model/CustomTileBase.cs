using System;
using UnityEngine.Tilemaps;
using UnityEngine;

public class CustomTileBase : TileBase {
    public Sprite sprite;

    // Docs: https://docs.unity3d.com/ScriptReference/Tilemaps.TileBase.GetTileData.html

    public override void GetTileData (Vector3Int position, ITilemap tilemap, ref TileData tileData) {
        tileData.sprite = sprite;
    }
}