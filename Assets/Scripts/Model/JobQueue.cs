using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue {
    // Start is called before the first frame update

    protected Queue<Job> jobQueue;
    protected List<Vector3Int> jobPositionQue;
    Action<Job> cbJobCreated;
    public JobQueue () {
        jobQueue = new Queue<Job> ();
        jobPositionQue = new List<Vector3Int>();
    }
    public void Enqueue (Job j) {
        jobQueue.Enqueue (j);
        jobPositionQue.Add (j.tilePos);
        if (cbJobCreated != null) {
            cbJobCreated (j);
        }
        //TODO:Call callbacks
    }
    public Job Dequeue(){
        if (jobQueue.Count == 0) { return null;}
        return jobQueue.Dequeue();
    }
    public bool JobPositonsContains (Vector3Int tilePos) {
        return jobPositionQue.Contains (tilePos);
    }
    public void JobPositonsRemove (Vector3Int tilePos) {
        jobPositionQue.Remove (tilePos);
    }
    public void RegisterJobCreationCallback (Action<Job> cb) {
        cbJobCreated += cb;
    }
    public void UnregisterJobCreationCallback (Action<Job> cb) {
        cbJobCreated -= cb;
    }
    public int Count () {
        return jobQueue.Count;
    }
}