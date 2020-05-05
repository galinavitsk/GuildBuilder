using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InstalledObjectActions {
    public static void Door_UpdateAction (InstalledObject door, float deltaTime) {
        Debug.Log ("Door_UpdateAction");
        Tilemap tilemapFoundation = WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ();
        if (door.installedObjectParamenters["is_opening"] == 1) {
            door.installedObjectParamenters["openess"] += deltaTime;
            if (door.installedObjectParamenters["openess"] >= 1) { //if the door is open, set door to start closing
                door.installedObjectParamenters["is_opening"] = 0;
            }
        } else {
            door.installedObjectParamenters["openess"] -= deltaTime;
        }
        // Debug.Log (door.installedObjectParamenters["openess"]);
        if (door.installedObjectParamenters["openess"] >= 0f) {
            CustomTileBase ctile = (CustomTileBase) ScriptableObject.CreateInstance (typeof (CustomTileBase));
            Debug.Log (tilemapFoundation.GetSprite (door.tile).name);
            String spriteName = tilemapFoundation.GetSprite (door.tile).name.Substring (0, tilemapFoundation.GetSprite (door.tile).name.Length - 2);
            Debug.Log (spriteName);
            if (door.installedObjectParamenters["openess"] <= 0.25f) {
                Debug.Log ("0.25 " + tilemapFoundation.GetSprite (door.tile).name);
                ctile.sprite = GameObject.FindObjectOfType<InstalledObjectSpriteController> ().installedObjectSprites[spriteName + "_1"];
            } else if (door.installedObjectParamenters["openess"] <= 0.5f) {
                Debug.Log ("0.5 " + tilemapFoundation.GetSprite (door.tile).name);
                ctile.sprite = GameObject.FindObjectOfType<InstalledObjectSpriteController> ().installedObjectSprites[spriteName + "_2"];
            } else if (door.installedObjectParamenters["openess"] <= 0.75f) {
                Debug.Log ("0.75 " + tilemapFoundation.GetSprite (door.tile).name);
                ctile.sprite = GameObject.FindObjectOfType<InstalledObjectSpriteController> ().installedObjectSprites[spriteName + "_3"];
            } else if (door.installedObjectParamenters["openess"] >= 0.75f) {
                Debug.Log ("1 " + tilemapFoundation.GetSprite (door.tile).name);
                ctile.sprite = GameObject.FindObjectOfType<InstalledObjectSpriteController> ().installedObjectSprites[spriteName + "_4"];
            }
            TileBase tile = (TileBase) ctile;
            tilemapFoundation.SetTile (door.tile, tile);
        }
        door.installedObjectParamenters["openess"] = Mathf.Clamp01 (door.installedObjectParamenters["openess"]);
        // Debug.Log ("CLAMP " + door.installedObjectParamenters["openess"]);
    }
    public static ENTERABILITY Door_IsEnterable (InstalledObject door) {
        door.installedObjectParamenters["is_opening"] = 1;
        if (door.installedObjectParamenters["openess"] >= 1) {
            return ENTERABILITY.Yes;
        } else { return ENTERABILITY.Soon; }
    }
}