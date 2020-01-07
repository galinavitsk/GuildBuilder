using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using UnityEngine;

public class Path_AStar {
    // Start is called before the first frame update
    Queue<Vector3Int> path;
    public Path_AStar (World world, Vector3Int Tilestart, Vector3Int Tileend) {
        if (world.tileGraph == null) { world.tileGraph = new Path_TileGraph (world); }
        Dictionary<Vector3Int, Path_Node<Vector3Int>> nodes = world.tileGraph.nodes;
        if (nodes.ContainsKey (Tilestart) == false) {
            Debug.LogError ("Path_AStar::Starting tile is not in the list of nodes!");
            return;
        }
        if (nodes.ContainsKey (Tileend) == false) { Debug.LogError ("Path_AStar::Ending tile is not in the list of nodes!"); return; }
        Path_Node<Vector3Int> start = nodes[Tilestart];
        Path_Node<Vector3Int> goal = nodes[Tileend];

        List<Path_Node<Vector3Int>> ClosedSet = new List<Path_Node<Vector3Int>> ();
        SimplePriorityQueue<Path_Node<Vector3Int>> OpenSet = new SimplePriorityQueue<Path_Node<Vector3Int>> ();
        OpenSet.Enqueue (start, 0);

        Dictionary<Path_Node<Vector3Int>, Path_Node<Vector3Int>> Came_From = new Dictionary<Path_Node<Vector3Int>, Path_Node<Vector3Int>> ();
        Dictionary<Path_Node<Vector3Int>, float> g_score = new Dictionary<Path_Node<Vector3Int>, float> ();
        Dictionary<Path_Node<Vector3Int>, float> f_score = new Dictionary<Path_Node<Vector3Int>, float> ();

        foreach (Path_Node<Vector3Int> n in nodes.Values) {
            g_score[n] = Mathf.Infinity;
        }
        g_score[start] = 0;
        foreach (Path_Node<Vector3Int> n in nodes.Values) {
            f_score[n] = Mathf.Infinity;
        }
        f_score[start] = heuristic_cost_estimate (start.data, goal.data);
        while (OpenSet.Count > 0) {
            Path_Node<Vector3Int> current = OpenSet.Dequeue ();
            if (current == goal) {
                reconstruct_Path (Came_From, goal);
                return;
            }
            ClosedSet.Add (current);
            foreach (Path_Edge<Vector3Int> neighbor in current.edges) {
                if (ClosedSet.Contains (neighbor.node) == true) { continue; }
                float tentative_g_score = g_score[current] + (dist_between (current, neighbor.node) * neighbor.cost);
                if (OpenSet.Contains (neighbor.node) && tentative_g_score >= g_score[neighbor.node]) {
                    continue;
                }
                Came_From[neighbor.node] = current;
                g_score[neighbor.node] = tentative_g_score;
                f_score[neighbor.node] = g_score[neighbor.node] + (heuristic_cost_estimate (neighbor.node.data, goal.data));
                if (OpenSet.Contains (neighbor.node) == false) {
                    OpenSet.Enqueue (neighbor.node, f_score[neighbor.node]);
                }
                else{
                    OpenSet.UpdatePriority(neighbor.node,f_score[neighbor.node]);
                }
            }

        }
        return;

    }

    void reconstruct_Path (Dictionary<Path_Node<Vector3Int>, Path_Node<Vector3Int>> Came_From, Path_Node<Vector3Int> current) {
        Queue<Vector3Int> total_path = new Queue<Vector3Int> ();
        total_path.Enqueue (current.data); //final step in the path is the goal
        while (Came_From.ContainsKey (current)) {
            //Came_From is a map where the key/value relation is some_node=>we_got_there_from_this_node
            current = Came_From[current];
            total_path.Enqueue (current.data);
        }
        path = new Queue<Vector3Int> (total_path.Reverse ());
    }

    float dist_between (Path_Node<Vector3Int> a, Path_Node<Vector3Int> b) {
        //Debug.Log(a.data);
        //Debug.Log(b.data);
        if (Mathf.Abs (a.data.x - b.data.x) + Mathf.Abs (a.data.y - b.data.y) == 1) { return 1f; }
        if (Mathf.Abs (a.data.x - b.data.x) == 1 && Mathf.Abs (a.data.y - b.data.y) == 1) { return 1.41421356237f; }
        return heuristic_cost_estimate (a.data, b.data);
    }

    float heuristic_cost_estimate (Vector3Int start, Vector3Int goal) {
        return Vector3Int.Distance (start, goal);
    }

    public Vector3Int GetNextTile () {
        return path.Dequeue ();
    }
    public int Length () {
        if (path == null) { return 0; } else { return path.Count (); }
    }
}