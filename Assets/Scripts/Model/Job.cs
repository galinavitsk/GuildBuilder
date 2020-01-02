using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job {

    public Vector3Int tilePos;
    public string objectType;
    float jobTime;

    Action<Job> cbJobComplete;
    Action<Job> cbJobCancelled;
    Action<Job> cbJobCreated;

    public Job (Vector3Int tile, string objectType, Action<Job> cbJobComplete, float jobTime = 1f) {
        this.tilePos = tile;
        this.objectType = objectType;
        this.cbJobComplete += cbJobComplete;
        this.jobTime = jobTime;
    }
    public void RegisterJobCompleteCallback (Action<Job> cb) {
        this.cbJobComplete += cb;
    }
    public void RegisterJobCancelledCallback (Action<Job> cb) {
        this.cbJobCancelled += cb;
    }
    public void DoWork (float workTIme) {
        jobTime -= workTIme;
        if (jobTime <= 0) {
            if (cbJobComplete != null) {
                cbJobComplete (this);
            }
        }
    }
    public void CancelJob () {

        if (cbJobCancelled != null) {
            cbJobCancelled (this);
        }
    }
}