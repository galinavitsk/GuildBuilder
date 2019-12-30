using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldController : MonoBehaviour {
    public static WorldController Instance { get; protected set; }
    public World World { get; protected set; }
    public Tilemap tilemapLandscape;
    public Tilemap tilemapFoundation;

    //public Tilemap tilemap;
    public Dictionary<string, Sprite> installedObjectSprites { get; protected set; } //sprites for walls, windows, doors
    // Start is called before the first frame update
    void Start () {

        installedObjectSprites = new Dictionary<string, Sprite> ();
        Sprite[] installedObjectSpritesNames = Resources.LoadAll<Sprite> ("Images/InstalledObjects/");
        foreach (var s in installedObjectSpritesNames) {
            installedObjectSprites.Add (s.name.ToString (), s);
        }

        if (Instance != null) {
            Debug.LogError ("There should not be two world controllers.");
        }
        Instance = this;
        World = new World ();
        World.RandomizeTiles();
        //Center Camera
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);

    }

    // Update is called once per frame
    void Update () { }

    public void PlaceInstalledObject (Vector3Int tile_position, string buildModeObjectType) {
        InstalledObject object_to_place = World.InstalledObjectPrototypes[buildModeObjectType];
        object_to_place = InstalledObject.PlaceInstance (object_to_place, tilemapFoundation.GetTile (tile_position), tile_position);
        if (object_to_place.sprite == null) { } else {

            if (installedObjectSprites.ContainsKey (object_to_place.sprite) == false) { //IF sprite doesn't exist then it's likely a RuleTile(see: dirt/grass/walls)
                tilemapFoundation.SetTile (tile_position, Resources.Load<RuleTile> ("Images/InstalledObjects/" + object_to_place.sprite));
            }
            if (installedObjectSprites.ContainsKey (object_to_place.sprite) == true) { //IF sprite exists

                CustomTileBase tile = (CustomTileBase) ScriptableObject.CreateInstance (typeof (CustomTileBase));;
                tile.sprite = installedObjectSprites[object_to_place.sprite];
                tilemapFoundation.SetTile (tile_position, tile);
            }

        }
    }

}