﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SaveManager : MonoBehaviour {
    // Start is called before the first frame update
    void Start () {

    }

    // Update is called once per frame
    void Update () {

    }

    public void Save () {
        BinaryFormatter bf = new BinaryFormatter ();
        FileStream file = File.Open (Application.persistentDataPath + "/" + "SaveTest.dat", FileMode.Create);
        SaveData data = new SaveData ();
        data.WorldSaveData = new WorldSaveData (WorldController.Instance.World);
        data.LandscapeTilemap = new TilemapSaveData (WorldController.Instance.tilemapLandscape);
        data.FoundationTilemap = new TilemapSaveData (WorldController.Instance.tilemapFoundation);
        data.CharactersSaveData = new CharactersSaveData ();
        bf.Serialize (file, data);
        file.Close ();
    }

    public void Load () {
        BinaryFormatter bf = new BinaryFormatter ();
        FileStream file = File.Open (Application.persistentDataPath + "/" + "SaveTest.dat", FileMode.Open);
        SaveData data = (SaveData) bf.Deserialize (file);
        WorldController.Instance.CreateEmptyWorld (data.WorldSaveData.width, data.WorldSaveData.height);
        LoadTilemaps (data);
        LoadCharacters (data);
        file.Close ();
    }

    public void LoadTilemaps (SaveData data) {
        Dictionary<SerializableVector3Int, string> landscapeTiles = data.LandscapeTilemap.tilemapToSave;
        foreach (SerializableVector3Int tilePos in landscapeTiles.Keys) {
            if (landscapeTiles[tilePos].Contains ("Floor") == true) {
                TileBase tile = GameObject.FindObjectOfType<InstalledObjectSpriteController> ().GetTileBase (landscapeTiles[tilePos], tilePos);
                WorldController.Instance.tilemapLandscape.SetTile (tilePos, tile);
            } else {
                WorldController.Instance.tilemapLandscape.SetTile (tilePos, Resources.Load<RuleTile> ("Images/Landscape/" + landscapeTiles[tilePos]));
            }
        }
        //Debug.Log ("Loading Foundation Tilemap");
        Dictionary<SerializableVector3Int, string> foundationTiles = data.FoundationTilemap.tilemapToSave;
        Dictionary<Vector3Int, string> afterwallplacements = new Dictionary<Vector3Int, string> ();
        foreach (SerializableVector3Int tilePos in foundationTiles.Keys) {
            if (foundationTiles[tilePos] == "Wall") {
                WorldController.Instance.PlaceInstalledObject (tilePos, foundationTiles[tilePos]);
            } else {
                afterwallplacements.Add (tilePos, foundationTiles[tilePos]);
            }
        }
        foreach (Vector3Int tilePos in afterwallplacements.Keys) {
            WorldController.Instance.PlaceInstalledObject (tilePos, afterwallplacements[tilePos]);
        }
    }

    public void LoadCharacters (SaveData data) {
        foreach (SerializableVector3Int tilePos in data.CharactersSaveData.characters.Keys) {
            WorldController.Instance.World.CreateCharacter (tilePos, data.CharactersSaveData.characters[tilePos].speed, data.CharactersSaveData.characters[tilePos].name, data.CharactersSaveData.characters[tilePos].buildspeed);
        }
    }
}