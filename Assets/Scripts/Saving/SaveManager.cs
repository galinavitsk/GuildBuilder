using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour {
    // Start is called before the first frame update
    void Start () {

    }

    // Update is called once per frame
    void Update () {

    }

    public void Save () {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/" + "SaveTest.dat",FileMode.Create);
        SaveData data = new SaveData();
        file.Close();
    }

    private void SaveTilemaps(SaveData data)
    {
        data.LandscapeTilemap = new TilemapSaveData(WorldController.Instance.tilemapLandscape);
    }
}