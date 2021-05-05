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
    public float speed = 2f; //Tiles per second;
    public string name;
    public float buildtime;

    Job myJob;
    Action<Character> cbCharacterMoved;
    public void Update (float deltaTime) {
        Update_DoJob (deltaTime, buildtime);
        Update_DoMovement (deltaTime);
    }

    void Update_DoJob (float deltaTime, float buildtime) {
        if (myJob == null) {
            //Get a new job
            //TODO:Check if the job is reachable
            myJob = WorldController.Instance.World.jobQueue.Dequeue();
            Dictionary<Vector3Int, InstalledObject> foundationGameMap=WorldController.Instance.World.foundationGameMap;
            if(myJob==null){
                //jobQueue was empty so exit out of doing the job
                return;
            }
            Vector3Int jobposition=myJob.tilePos;
            //north,east,south,west
            Vector3Int [] tile_positions={new Vector3Int(myJob.tilePos.x,myJob.tilePos.y+1,0),new Vector3Int(myJob.tilePos.x+1,myJob.tilePos.y,0),new Vector3Int(myJob.tilePos.x,myJob.tilePos.y-1,0),new Vector3Int(myJob.tilePos.x-1,myJob.tilePos.y,0)};
            
            bool[] reachable={find_If_Tile_Job_Is_Rechable(tile_positions[0]),find_If_Tile_Job_Is_Rechable(tile_positions[1]),find_If_Tile_Job_Is_Rechable(tile_positions[2]),find_If_Tile_Job_Is_Rechable(tile_positions[3])};
         
            try{
                    if(foundationGameMap.ContainsKey(myJob.tilePos)){
                        if(foundationGameMap[myJob.tilePos]!=null &&
                            foundationGameMap[myJob.tilePos].IsEnterable!=null &&
                            foundationGameMap[myJob.tilePos].IsEnterable(foundationGameMap[myJob.tilePos])==ENTERABILITY.Yes)
                        {
                            jobposition=myJob.tilePos;
                        }
                        else{
                            if(jobposition.y<WorldController.Instance.World.Height-1 &&
                            reachable[0] &&
                            (
                            foundationGameMap.ContainsKey(tile_positions[0])==false ||
                            foundationGameMap[tile_positions[0]].IsEnterable(foundationGameMap[tile_positions[0]])==ENTERABILITY.Yes)
                            ){
                                jobposition=tile_positions[0];
                            }
                            else if(jobposition.y<WorldController.Instance.World.Height+1 &&
                            reachable[2] &&
                            (
                            foundationGameMap.ContainsKey(tile_positions[2])==false ||
                            foundationGameMap[tile_positions[2]].IsEnterable(foundationGameMap[tile_positions[2]])==ENTERABILITY.Yes)
                            ){
                                jobposition=tile_positions[2];
                            }
                            else if(jobposition.x<WorldController.Instance.World.Width+1 &&
                            reachable[1] &&
                            (
                            foundationGameMap.ContainsKey(tile_positions[1])==false ||
                            foundationGameMap[tile_positions[1]].IsEnterable(foundationGameMap[tile_positions[1]])==ENTERABILITY.Yes)
                            ){
                                jobposition=tile_positions[1];
                            }
                            else if(jobposition.x<WorldController.Instance.World.Width-1 &&
                            reachable[3] &&
                            (
                            foundationGameMap.ContainsKey(tile_positions[3])==false ||
                            foundationGameMap[tile_positions[3]].IsEnterable(foundationGameMap[tile_positions[3]])==ENTERABILITY.Yes)
                            ){
                                jobposition=tile_positions[3];
                            }
                        }
                    }
            }
            catch{
                AbandonJob();
            }
            
            if (myJob != null) {
                destTile = jobposition;
                myJob.RegisterJobCompleteCallback (onJobEnded);
                myJob.RegisterJobCancelledCallback (onJobEnded);
            }
            
        }
        //Movement code
        if (currTile == destTile) {
            if (myJob != null) {
                myJob.DoWork (deltaTime, buildtime);
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
                nextTile = path_AStar.GetNextTile ();
            }
            nextTile = path_AStar.GetNextTile (); //removes it from the path list
            if (nextTile == currTile) {
                Debug.Log ("Update_DoMovement::NextTile is curr tile ?");
            }
        }

        float totalDisToTravel = Vector3Int.Distance (currTile, nextTile); //total distance from A to B
        float movementCost = 1;
        ENTERABILITY enterability = ENTERABILITY.Yes;
        if (WorldController.Instance.World.foundationGameMap.ContainsKey (nextTile) == true) {
            movementCost = WorldController.Instance.World.foundationGameMap[nextTile].movementCost;
            try{
            enterability = WorldController.Instance.World.foundationGameMap[nextTile].IsEnterable (WorldController.Instance.World.foundationGameMap[nextTile]);
            }
            catch{enterability=ENTERABILITY.Never;}
             if(movementCost>0 && enterability==ENTERABILITY.Never){
                enterability=ENTERABILITY.Yes;
            }
        }
        if (enterability == ENTERABILITY.Never) {
            Debug.LogError ("Character " + name + " was trying to enter an unwalkable tile");
            nextTile = currTile;
            path_AStar = null;
            return;
        } else if (enterability == ENTERABILITY.Soon) {
            //Have to wait to enter the tile, this is likely a door
            //No bailing on the movement/path, but we do return now and don't actually process the movement;
            return;
        }
        float distanceThisFrame = speed / movementCost * deltaTime; //how much distance can character tavel this update
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
    public Character (Vector3Int tile, float speed, string name, float buildtime) {
        currTile = destTile = nextTile = tile;
        this.speed = speed;
        this.name = name;
        this.buildtime = buildtime;
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
        //Job completed or cancelled
        //if (job != myJob) { Debug.LogError ("Character being told about a job that isn't his. Forgot to unregister something"); return; }
        myJob = null;
    }

    bool find_If_Tile_Job_Is_Rechable(Vector3Int jobTile){
        Path_AStar path_to_job=new Path_AStar(WorldController.Instance.World, currTile,jobTile);
        if(path_to_job.Length()==0){
           return false;
        }
        else{return true;}
    }

}