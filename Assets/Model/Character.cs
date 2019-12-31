using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character {
    public float X {
        get { return Mathf.Lerp (currTile.x, destTile.x, movementPercentage); }
    }
    public float Y {
        get { return Mathf.Lerp (currTile.y, destTile.y, movementPercentage); }
    }
    public Vector3Int currTile { get; protected set; }
    Vector3Int destTile; //if the character is not moving then destTile=currTile
    float movementPercentage; //goes from 0 to 1 as they move along the path
    float speed = 2f; //Tiles per second;

    Job myJob;
    Action<Character> cbCharacterMoved;
    public void Update (float deltaTime) {

        if (myJob == null) {
            //Get a new job
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
            return;
        }
        float totalDisToTravel = Vector3Int.Distance (currTile, destTile); //total distance from A to B
        float distanceThisFrame = speed * deltaTime; //how much distance can character tavel this update
        float percantageThisFrame = distanceThisFrame / totalDisToTravel; //how much is that in percentage
        movementPercentage += percantageThisFrame; //increase percentage moved
        if (movementPercentage >= 1) {
            //Destination reached
            currTile = destTile;
            movementPercentage = 0;

        }
        if (cbCharacterMoved != null) {
            cbCharacterMoved (this);
        }
    }
    public Character (Vector3Int tile) {
        currTile = destTile = tile;
    }
    public void SetDestination (Vector3Int tile) {
        if (IsNeighborTiles (tile, true) == false) {
            Debug.Log ("Character::SetDestination--Out destination isn't actually adjacent");
        }
        destTile = tile;
    }
    bool IsNeighborTiles (Vector3Int tile, bool diagOkay = false) {
        if (currTile.x == tile.x && (tile.y + 1 == currTile.y || currTile.y == tile.y - 1)) { return true; }
        if (currTile.y == tile.y && (tile.x + 1 == currTile.x || currTile.x == tile.x - 1)) { return true; }
        if (diagOkay == true) {
            if (currTile.x == tile.x + 1 && (tile.y + 1 == currTile.y || currTile.y == tile.y - 1)) { return true; }

            if (currTile.x == tile.x - 1 && (tile.y + 1 == currTile.y || currTile.y == tile.y - 1)) { return true; }
        }
        return false;
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