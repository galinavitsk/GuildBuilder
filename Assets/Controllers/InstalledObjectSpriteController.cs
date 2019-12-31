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
    void Start () {
LoadSprites();
    }
    public TileBase GetTileBase (string objectType) {
        TileBase tile = null;

        //if (objectType == "Door" || objectType == "Window") { objectType = IsRotatable (objectType); }
        if (objectType == null) { Debug.LogError ("Passed a null objectType"); } else {

            if (installedObjectSprites.ContainsKey (objectType) == false) { //IF sprite doesn't exist then it's likely a RuleTile(see: dirt/grass/walls)
                tile = Resources.Load<RuleTile> ("Images/InstalledObjects/" + objectType);
            }
            if (installedObjectSprites.ContainsKey (objectType) == true) { //IF sprite exists

                CustomTileBase ctile = (CustomTileBase) ScriptableObject.CreateInstance (typeof (CustomTileBase));;
                ctile.sprite = installedObjectSprites[objectType];
                tile = (TileBase) ctile;
            }

        }
        return tile;

    }

    void LoadSprites () {
        installedObjectSprites = new Dictionary<string, Sprite> ();
        Sprite[] installedObjectSpritesNames = Resources.LoadAll<Sprite> ("Images/InstalledObjects/");
        foreach (var s in installedObjectSpritesNames) {
            installedObjectSprites.Add (s.name.ToString (), s);
        }
    }

}