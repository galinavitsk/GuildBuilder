using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class InstalledObjectSpriteController : MonoBehaviour {
    public Dictionary<string, Sprite> installedObjectSprites { get; protected set; }
    void OnEnable () {
        LoadSprites ();
    }
    public TileBase GetTileBase (string objectType, Vector3Int tilePos) {
        Debug.Log (objectType);
        TileBase tile = null;
        if (objectType == "Door" || objectType == "Window") { objectType = IsRotatable (objectType, tilePos); }
        if (objectType == null) { Debug.LogError ("Passed a null objectType"); } else {
            if (installedObjectSprites == null) {
                LoadSprites ();
            }
            if (installedObjectSprites.ContainsKey (objectType) == false) { //IF sprite doesn't exist then it's likely a RuleTile(see: dirt/grass/walls)
                tile = Resources.Load<RuleTile> ("Images/InstalledObjects/" + objectType);
                if (tile == null) {
                    tile = Resources.Load<AdvancedRuleTile> ("Images/InstalledObjects/" + objectType);
                }
            }
            if (installedObjectSprites.ContainsKey (objectType) == true) { //IF sprite exists
                CustomTileBase ctile = (CustomTileBase) ScriptableObject.CreateInstance (typeof (CustomTileBase));;
                ctile.sprite = installedObjectSprites[objectType];
                ctile.name = objectType;
                tile = (TileBase) ctile;
            }

        }
        return tile;

    }

    void LoadSprites () {
        //Debug.Log("LoadSprites");
        installedObjectSprites = new Dictionary<string, Sprite> ();
        Sprite[] installedObjectSpritesNames = Resources.LoadAll<Sprite> ("Images/InstalledObjects/");
        foreach (var s in installedObjectSpritesNames) {
            installedObjectSprites.Add (s.name.ToString (), s);
        }
    }

    string IsRotatable (string objectType, Vector3Int tilePos) {
        int x = tilePos.x;
        int y = tilePos.y;
        if (objectType == "Door" || objectType == "Window") { //If general position is valid
            Tilemap tilemapFoundation = WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ();
            if (tilemapFoundation.GetTile (new Vector3Int (x - 1, y, 0)) != null &&
                tilemapFoundation.GetTile (new Vector3Int (x + 1, y, 0)) != null &&
                tilemapFoundation.GetTile (new Vector3Int (x - 1, y, 0)).name.ToString ().Contains ("Wall") == true &&
                tilemapFoundation.GetTile (new Vector3Int (x + 1, y, 0)).name.ToString ().Contains ("Wall") == true) {
                return objectType + "_EW_1"; //assigns horizontal Sprite
            } else if (
                tilemapFoundation.GetTile (new Vector3Int (x, y - 1, 0)) != null &&
                tilemapFoundation.GetTile (new Vector3Int (x, y + 1, 0)) != null &&
                tilemapFoundation.GetTile (new Vector3Int (x, y - 1, 0)).name.ToString ().Contains ("Wall") == true &&
                tilemapFoundation.GetTile (new Vector3Int (x, y + 1, 0)).name.ToString ().Contains ("Wall") == true) {
                return objectType + "_NS_1"; //assigns vertical sprite
            } else {
                Debug.LogError ("Can't place Door here Sprite Pass"); //Throws an error as you can't place a door
                return null;
            }
        }
        return null;
    }

}