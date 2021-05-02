using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue {
    // Start is called before the first frame update

    protected LinkedList<Job> jobQueue;
    protected List<JobPositionItem> jobPositionQue;
    Action<Job> cbJobCreated;
    public JobQueue () {
        jobQueue = new LinkedList<Job> ();
        jobPositionQue = new List<JobPositionItem>();
    }
    public void Enqueue (Job j) {
        Debug.Log("ENQUEUE START:"+jobQueue.Count);
        jobQueue.AddLast (j);
        jobPositionQue.Add (new JobPositionItem(j.tilePos,j.objectType));
        if (cbJobCreated != null) {
            cbJobCreated (j);
        }
        Debug.Log("ENQUEUE EMD:"+jobQueue.Count);
        //TODO:Call callbacks
    }
    public Job Dequeue(){
        Debug.Log("DEQUEUE:"+jobQueue.Count);
        if (jobQueue.Count == 0) { return null;}
        Job job= jobQueue.First.Value;
        jobQueue.RemoveFirst();
        Debug.Log("DEQUEUE:"+jobQueue.Count);
        return job;
    }
    public bool JobPositonsContains (Vector3Int tilePos) {
        return jobPositionQue.FindIndex(position=>position.tilePos==tilePos)!=-1;
    }
    public bool JobPositonsWallCheck(Vector3Int tilePos){
        try{
            return jobPositionQue.Find(position=>position.tilePos==tilePos).objectType.Contains("Wall");
        }
        catch{return false;}
    }
    public void JobPositonsRemove (Vector3Int tilePos) {
        try{
        jobPositionQue.RemoveAt (jobPositionQue.FindIndex(position=>position.tilePos==tilePos));
            Debug.Log("JobQueue::Removing job position, position:"+tilePos.x+"_"+tilePos.y);
        }
        catch{
            Debug.LogError("JobQueue::Couldn't remove a job position, position:"+tilePos.x+"_"+tilePos.y);
        }
    }
    
    public void JobQueRemoveAt(Vector3Int tilePos){
        try{
            foreach (var node in jobQueue)
            {
                if(node.tilePos==tilePos){
                    jobQueue.Remove(node);

                }
            }
            JobPositonsRemove(tilePos);
        }catch{}

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