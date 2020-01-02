using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Edge<T> {
    public Path_Node<T> node;
    
    //cost to traverse this edge, i.e cost to ENTER the tile

    public float cost;

}