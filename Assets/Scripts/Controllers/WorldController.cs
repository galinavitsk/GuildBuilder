using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class WorldController : MonoBehaviour {
    public static WorldController Instance { get; protected set; }
    public World World { get; protected set; }
    public Tilemap tilemapLandscape;
    public Tilemap tilemapFoundation;
    static bool loadWorld = false;

    //public Tilemap tilemap;
    //sprites for walls, windows, doors
    // Start is called before the first frame update
    void OnEnable () {

        if (Instance != null) {
            Debug.LogError ("There should not be two world controllers.");
        }

        Instance = this;
        if (loadWorld == true) {
            FindObjectOfType<SaveManager> ().Load ();
            loadWorld = false;
        } else {
            CreateEmptyWorld ();
            Debug.Log ("Creating a character");
            Character c = World.CreateCharacter (new Vector3Int (World.Width / 2, World.Height / 2, 0), 5, "Caleb", 5f);
        }

    }

    // Update is called once per frame
    void Update () {
        //TODO: speed controls, pause unpause, etc
        World.Update (Time.deltaTime);
    }

    public void CreateEmptyWorld (int width = 20, int height = 20) {
        World = new World (width, height);
        Camera.main.transform.position = new Vector3 (World.Width / 2, World.Height / 2, Camera.main.transform.position.z);
    }
    public void PlaceInstalledObject (Vector3Int tile_position, string buildModeObjectType) {
        InstalledObject object_to_place = World.InstalledObjectPrototypes[buildModeObjectType];
        object_to_place = InstalledObject.PlaceInstance (object_to_place, tilemapFoundation.GetTile (tile_position), tile_position);
        TileBase tile = GameObject.FindObjectOfType<InstalledObjectSpriteController> ().GetTileBase (buildModeObjectType, tile_position);
        if (tile == null) { Debug.LogError ("Something went wrong"); } else {
            tilemapFoundation.SetTile (tile_position, tile);
            World.InvalidateTileGraph ();
            if(object_to_place!=null){
            World.objectsGameMap.Add (tile_position, object_to_place);}
        }

    }
    public void NewWorld () {
        SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
        CreateEmptyWorld ();
    }

    public void SaveWorld () {
        FindObjectOfType<SaveManager> ().Save ();
    }
    public void LoadWorld () {
        loadWorld = true;
        SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
    }
}