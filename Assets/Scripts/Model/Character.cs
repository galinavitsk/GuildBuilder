using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character {
    public float X {
        get { return Mathf.Lerp (currTile.x, nextTile.x, movementPercentage); }
    }
    public float Y {
        get { return Mathf.Lerp (currTile.y, nextTile.y, movementPercentage); }
    }
    public Vector3Int currTile { get; protected set; }
    Vector3Int nextTile;
    Vector3Int destTile; //if the character is not moving then destTile=currTile
    Path_AStar path_AStar;
    float movementPercentage; //goes from 0 to 1 as they move along the path
    float speed = 2f; //Tiles per second;

    Job myJob;
    Action<Character> cbCharacterMoved;
    public void Update (float deltaTime) {
        Update_DoJob (deltaTime);
        Update_DoMovement (deltaTime);
    }

    void Update_DoJob (float deltaTime) {
        if (myJob == null) {
            //Get a new job
            //TODO:Check if the job is reachable
            
            myJob = WorldController.Instance.World.jobQueue.Dequeue ();
            if (myJob != null) {
                destTile = myJob.tilePos;
                myJob.RegisterJobCompleteCallback (onJobEnded);
                myJob.RegisterJobCancelledCallback (onJobEnded);
            }
        }
        //Movement code
        if (currTile == destTile) {
            if (myJob != null) {
                myJob.DoWork (deltaTime);
            }
        }
    }

    public void AbandonJob () {
        nextTile = destTile = currTile;
        path_AStar = null;

        WorldController.Instance.World.jobQueue.Enqueue (myJob);
        myJob = null;
    }
    void Update_DoMovement (float deltaTime) {
        if (currTile == destTile) {
            path_AStar = null;
            return;
        }
        if (nextTile == null || nextTile == currTile) {
            //get the next tile from pathfinding
            if (path_AStar == null || path_AStar.Length () == 0) {
                path_AStar = new Path_AStar (WorldController.Instance.World, currTile, destTile);
                if (path_AStar.Length () == 0) {
                    Debug.LogError ("Path_AStar returned no path to destination");
                    AbandonJob ();
                    path_AStar = null;
                    return;
                }
            }
            nextTile = path_AStar.GetNextTile (); //removes it from the path list
            if (nextTile == currTile) {
                Debug.Log ("Update_DoMovement-Nexttile is curr tile ?");
            }
        }

        float totalDisToTravel = Vector3Int.Distance (currTile, nextTile); //total distance from A to B
        float distanceThisFrame = speed * deltaTime; //how much distance can character tavel this update
        float percantageThisFrame = distanceThisFrame / totalDisToTravel; //how much is that in percentage
        movementPercentage += percantageThisFrame; //increase percentage moved
        if (movementPercentage >= 1) {
            //Destination reached
            currTile = nextTile;
            movementPercentage = 0;

        }
        if (cbCharacterMoved != null) {
            cbCharacterMoved (this);
        }
    }
    public Character (Vector3Int tile) {
        currTile = destTile = nextTile = tile;
    }
    public void SetDestination (Vector3Int tile) {
        destTile = tile;
    }

    public void RegisterCharacterMovedCallback (Action<Character> cb) {
        cbCharacterMoved += cb;
    }
    public void UnegisterCharacterMovedCallback (Action<Character> cb) {
        cbCharacterMoved -= cb;
    }

    void onJobEnded (Job job) {
        //Job completed of cancelled
        //if (job != myJob) { Debug.LogError ("Character being told about a job that isn't his. Forgot to unregister something"); return; }
        myJob = null;
    }
}