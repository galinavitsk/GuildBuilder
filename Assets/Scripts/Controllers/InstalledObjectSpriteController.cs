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
      //  Debug.Log (objectType);
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
        Vector3Int north_tile=new Vector3Int(x,y+1,0);
        Vector3Int south_tile=new Vector3Int(x,y-1,0);
        Vector3Int east_tile=new Vector3Int(x+1,y,0);
        Vector3Int west_tile=new Vector3Int(x-1,y,0);

        if (objectType == "Door" || objectType == "Window") { //If general position is valid
            Tilemap tilemapFoundation = WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ();//Tilemap of currently placed walls/doors
            Tilemap tilemapJobs=WorldController.Instance.tilemapJobs.GetComponent<Tilemap>();//Tilemap of currently existing jobs
            if (
                (
                    (
                        tilemapFoundation.GetTile (north_tile) != null
                        &&
                        (
                            tilemapFoundation.GetTile(north_tile).name.ToString().Contains("Wall")==true ||
                            tilemapFoundation.GetTile(north_tile).name.ToString().Contains("Door")==true
                        )
                    )
                    ||
                    (
                        tilemapJobs.GetTile (north_tile) != null
                        &&
                        (
                            tilemapJobs.GetTile(north_tile).name.ToString().Contains("Wall")==true ||
                            tilemapJobs.GetTile(north_tile).name.ToString().Contains("Door")==true
                        )
                    )
                )
                &&
                (
                    (
                        tilemapFoundation.GetTile (south_tile) != null
                        &&
                        (
                            tilemapFoundation.GetTile(south_tile).name.ToString().Contains("Wall")==true ||
                            tilemapFoundation.GetTile(south_tile).name.ToString().Contains("Door")==true
                        )
                    )
                    ||
                    (
                        tilemapJobs.GetTile (south_tile) != null
                        &&
                        (
                            tilemapJobs.GetTile(south_tile).name.ToString().Contains("Wall")==true ||
                            tilemapJobs.GetTile(south_tile).name.ToString().Contains("Door")==true
                        )
                    )
                )
            ){
                return objectType+"_NS_1"; //assigns horizontal Sprite
            } else if (
                (
                    (
                        tilemapFoundation.GetTile (west_tile) != null
                        &&
                        (
                            tilemapFoundation.GetTile(west_tile).name.ToString().Contains("Wall")==true ||
                            tilemapFoundation.GetTile(west_tile).name.ToString().Contains("Door")==true
                        )
                    )
                    ||
                    (
                        tilemapJobs.GetTile (west_tile) != null
                        &&
                        (
                            tilemapJobs.GetTile(west_tile).name.ToString().Contains("Wall")==true ||
                            tilemapJobs.GetTile(west_tile).name.ToString().Contains("Door")==true
                        )
                    )
                )
                &&
                (
                    (
                        tilemapFoundation.GetTile (east_tile) != null
                        &&
                        (
                            tilemapFoundation.GetTile(east_tile).name.ToString().Contains("Wall")==true ||
                            tilemapFoundation.GetTile(east_tile).name.ToString().Contains("Door")==true
                        )
                    )
                    ||
                    (
                        tilemapJobs.GetTile (east_tile) != null
                        &&
                        (
                            tilemapJobs.GetTile(east_tile).name.ToString().Contains("Wall")==true ||
                            tilemapJobs.GetTile(east_tile).name.ToString().Contains("Door")==true
                        )
                    )
                )
                ) {
                return objectType + "_EW_1"; //assigns vertical sprite
            } else {
                Debug.LogError ("Can't place Door here Sprite Pass"); //Throws an error as you can't place a door
                return null;
            }
        }
        return null;
    }

}