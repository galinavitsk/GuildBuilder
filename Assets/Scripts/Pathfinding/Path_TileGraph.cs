﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Path_TileGraph {
    //This class constructs a simple path-finding compatable graph of the world. Each tile is a node. Each Walkable neighbour from a tile is linked via an edge connection
    public Dictionary<Vector3Int, Path_Node<Vector3Int>> nodes;
    public Path_TileGraph (World world) {
        nodes = new Dictionary<Vector3Int, Path_Node<Vector3Int>> ();
        //Loop through all tiles of the world, build a node for each tile
        //Not creating nodes for tiles that are NON walkable(i.e walls, water, windows, etc)
        for (int x = 0; x < world.Width; x++) {
            for (int y = 0; y < world.Height; y++) {
                Vector3Int tilePos = new Vector3Int (x, y, 0);
                //Creates nodes FOR EVERY tile on the map

                Path_Node<Vector3Int> node = new Path_Node<Vector3Int> ();
                node.data = tilePos;
                nodes.Add (tilePos, node);
            }
        }
        int edgesCount = 0;
        //Now loop through all the nodes, and create the edges
        foreach (Vector3Int t in nodes.Keys) {
            //Get a list of neighbors of the tile, if neighbor is walkable create an edge to the relevant node
            Path_Node<Vector3Int> node = nodes[t];
            List<Path_Edge<Vector3Int>> edges = new List<Path_Edge<Vector3Int>> ();
            Vector3Int[] neighbors = world.GetNeighbors (t, true);
            for (int i = 0; i < neighbors.Length; i++)
            {
                float movementSpeed = 1f;
                //Get movement Speed based on the tile on tilemap Walkable layer
                if (WorldController.Instance.World.foundationGameMap.ContainsKey(neighbors[i]) == true)
                {
                    movementSpeed = WorldController.Instance.World.foundationGameMap[neighbors[i]].movementCost;
                }
                if (WorldController.Instance.World.foundationGameMap.ContainsKey (neighbors[i]) == false ||
                    (WorldController.Instance.World.foundationGameMap.ContainsKey (neighbors[i]) == true && movementSpeed > 0)) {
                    if (isClippingCorner (nodes[t].data, neighbors[i])) { continue; }
                    Path_Edge<Vector3Int> e = new Path_Edge<Vector3Int> ();
                    e.cost = movementSpeed;
                    if (nodes.ContainsKey (neighbors[i]) == true) {
                        e.node = nodes[neighbors[i]];
                        edges.Add (e);
                        edgesCount++;
                    }

                }
            }
            node.edges = edges.ToArray ();
        }
    }
    bool isClippingCorner (Vector3Int curr, Vector3Int neigh) {
        //If the movement from curr to neigh is diagonal(eg, NE)
        //Then check to mke sure we aren't clipping, that (N and E are both walkable.)
        if (Mathf.Abs (curr.x - neigh.x) + Mathf.Abs (curr.y - neigh.y) == 2) {
            //Diagonal
            int dX = curr.x - neigh.x;
            int dY = curr.y - neigh.y;
            if (WorldController.Instance.World.foundationGameMap.ContainsKey (new Vector3Int (curr.x - dX, curr.y, curr.z)) == true &&
                WorldController.Instance.World.foundationGameMap[new Vector3Int (curr.x - dX, curr.y, curr.z)].movementCost == 0) {
                //Tile to the west or east is not walkable
                return true;
            }
            if (WorldController.Instance.World.foundationGameMap.ContainsKey (new Vector3Int (curr.x, curr.y - dY, curr.z)) == true &&
                WorldController.Instance.World.foundationGameMap[new Vector3Int (curr.x, curr.y - dY, curr.z)].movementCost == 0) {
                //Tile to the north of south is not walkable
                return true;
            }
        }

        return false;
    }
}