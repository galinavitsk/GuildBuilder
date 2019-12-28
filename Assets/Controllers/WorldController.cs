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
    Dictionary<Tile, GameObject> tileGameObjectMap;
    Dictionary<Furniture, GameObject> furnitureGameObjectMap;
    public TileBase tileGrass;
    public TileBase tileDirt;
    public TileBase tileGeneric;
    public Tilemap tilemapLandscape;
    public Tilemap tilemapFurniture;

    //public Tilemap tilemap;
    Dictionary<string, Sprite> furnitureSprites; //sprites for walls, windows, doors
    // Start is called before the first frame update
    void Start () {

        furnitureSprites = new Dictionary<string, Sprite> ();
        Sprite[] furnituresprites = Resources.LoadAll<Sprite> ("Images/Furniture/");

        if (Instance != null) {
            Debug.LogError ("There should not be two world controllers.");
        }
        Instance = this;
        World = new World ();
        tileGameObjectMap = new Dictionary<Tile, GameObject> (); //Maps which GO is rendering which LandTile
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject> ();
        BoundsInt area = new BoundsInt (new Vector3Int (0, 0, 0), new Vector3Int (World.Width, World.Height, 1));
        Debug.Log ("ARea position: " + area.position);
        TileBase[] tileArray = new TileBase[area.size.x * area.size.y * area.size.z];

        for (int index = 0; index < tileArray.Length; index++) {
            tileArray[index] = UnityEngine.Random.Range (0, 2) == 0 ? tileGrass : tileDirt;
        }
        Debug.Log (tileArray.Length);
        tilemapLandscape = tilemapLandscape.GetComponent<Tilemap> ();
        tilemapLandscape.SetTilesBlock (area, tileArray);
    }

    // Update is called once per frame
    void Update () { }

    public void PlaceFurniture (Vector3Int tile_position, string buildModeObjectType) {
        Debug.Log ("Building a:" + buildModeObjectType);
        Furniture object_to_place = World.furniturePrototypes[buildModeObjectType];
        TileBase tile = Resources.Load<RuleTile> ("Images/Furniture/" + object_to_place.sprite);
        tilemapFurniture.SetTile (tile_position, tile);
    }

}