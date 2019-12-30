using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Job {

    Vector3Int tilePos;
    float jobTime;

    Action<Job> cbJobComplete;
    Action<Job> cbJobCancelled;

    public Job (Vector3Int tile, Action<Job> cbJobComplete, float jobTime = 1f) {
        this.tilePos = tile;
        this.cbJobComplete += cbJobComplete;
    }
    public void RegisterJobCompleteCallback (Action<Job> cb) {
        this.cbJobComplete += cb;
    }
    public void RegisterJobCancelledCallback (Action<Job> cb) {
        this.cbJobComplete += cb;
    }
    public void DoWork (float workTIme) {
        jobTime -= workTIme;
        if (jobTime <= 0) {
            if (cbJobComplete != null) { cbJobComplete (this); }
        }
    }
    public void CancelJob (float workTIme) {

        if (cbJobCancelled != null) { cbJobCancelled (this); }
    }
}