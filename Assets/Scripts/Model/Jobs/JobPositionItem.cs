using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobPositionItem {

    public Vector3Int tilePos;
    public string objectType;

    public JobPositionItem (Vector3Int tile, string objectType) {
        this.tilePos = tile;
        this.objectType = objectType;
    }

}