using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class JobSpriteController : MonoBehaviour {
    public Tilemap tilemapJobs;
    // Start is called before the first frame update
    void Start () {
        WorldController.Instance.World.jobQueue.RegisterJobCreationCallback (OnJobCreated);
    }

    void OnJobCreated (Job j) {
        TileBase tile_to_build = GameObject.FindObjectOfType<InstalledObjectSpriteController> ().GetTileBase (j.objectType,j.tilePos);
        tilemapJobs.GetComponent<Tilemap> ().SetTile (j.tilePos, tile_to_build);
        tilemapJobs.GetComponent<Tilemap> ().SetColor (j.tilePos, new Color (1.0f, 1.0f, 1.0f, 0.5f));
        j.RegisterJobCompleteCallback (OnJobEnded);
        j.RegisterJobCancelledCallback (OnJobEnded);

    }
    void OnJobEnded (Job j) {
        tilemapJobs.GetComponent<Tilemap> ().SetTile (j.tilePos, null);
        //TODO:Delete building sprites

    }
    // Update is called once per frame
    void Update () {

    }
}