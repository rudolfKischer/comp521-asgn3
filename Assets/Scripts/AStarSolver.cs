using UnityEngine;
using System.Collections.Generic;



public class AStarSolver : MonoBehaviour
{
    //lets make A star solver a singleton
    public static AStarSolver Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // if there is already an instance of AStarSolver, destroy this one
            Destroy(this);
        }
    }


    // Given a Graph and a start and end node, find the shortest path between the two nodes
    // using the A* algorithm
    // Returns a list of nodes that make up the shortest path
    // the list is ordered from start to end

    // The solver works by setting the graph were solving on once
    // then we can make multiple calls to the Solve method with different start and end nodes

    // we need to be able to keep a list of explored nodes
    // we also need a way to calculate the distance between two nodes
    // for simplicity sake we each node will be a point in space
    // so we can use the distance formula to calculate the distance between two nodes

    // ========NODES =========
    protected Vector3[] points; // the points in space that make up the nodes
    protected bool[] explored;
    protected float[] cost; // cost of getting to each node, not that this will update as we explore
    protected float[] heuristic; // heuristic cost of getting to the end node from each node
    protected int[] from; // the node we came from to get to this node, used to reconstruct the path


    // ========EDGES =========
    //edges are defined as follows:
    // we have an array of lists
    // the slot we index into, is the node were looking at, this index corresponds to the index of the node in the points array
    // the list at that index, contains the indexes of the nodes that are connected to the node at the index of the list
    protected List<int>[] edges;




    // we are going to want a priority queue to keep track of which nodes we should explore next

    //we also need to store the cost of getting to each node
    // as well as the heuristic cost of getting to the end node
    //we need a way to keep track of which nodes we have explored
    // its messy to have a bunch of arrays, but its slower to use classes 

    //========Search Data Structures========
    private int start = -1;
    private int end = -1;
    PriorityQueue<int, float> openList = new PriorityQueue<int, float>();

    public void Start() {
        SetGraph(new Vector3[0], new List<int>[0]);
    }

    public void SetGraph(Vector3[] points, List<int>[] edges)
    {
        this.points = points;
        this.edges = edges;
        explored = new bool[points.Length];
        cost = new float[points.Length];
        heuristic = new float[points.Length];
        from = new int[points.Length];
    }

    // Heuristic and cost functions

    private float Hfunc(int node)
    {
        return Vector3.Distance(points[node], points[end]);
    }

    private float Gfunc(int node, int fromNode)
    {
        return Vector3.Distance(points[node], points[fromNode]) + cost[fromNode];
    }

    private float Ffunc(int node)
    {
        return cost[node] + heuristic[node];
    }

    private List<int> ReconstructPath(int start, int end)
    {
        List<int> path = new List<int>();
        int current = end;
        while (current != start)
        {
            path.Add(current);
            current = from[current];
        }
        path.Add(start);
        path.Reverse();
        return path;
    }


    // The solver returns an ordered list of nodes that make up the shortest path
    // the list is ordered from start to end
    public List<int> Solve(int _start, int _end) {

        this.start = _start;
        this.end = _end;

        openList.Clear();
        //reset Data structures
        for (int i = 0; i < explored.Length; i++)
        {
            explored[i] = false;
            cost[i] = float.MaxValue;
            heuristic[i] = Hfunc(i);
            from[i] = -1;
        }


        //update start nodes heuristic
        heuristic[start] = Hfunc(start);
        cost[start] = 0;

        int closestExploredNode = -1;

        // add the start node to the open list
        openList.Enqueue(start, Ffunc(start));

        while (openList.Count > 0)
        {
            // get the node with the lowest F value
            int current = openList.Dequeue();

            // if we have found the end node, we are done
            if (current == end)
            {
                return ReconstructPath(start, end);
            }

            // mark the node as explored
            explored[current] = true;

            // for each node connected to the current node
            foreach (int neighbor in edges[current])
            {
                // if we have already explored the neighbor, skip it
                if (explored[neighbor])
                {
                    continue;
                }

                // calculate the cost of getting to the neighbor from the current node
                float newCost = Gfunc(neighbor, current);

                // if we have not explored the neighbor, or the new cost is less than the current cost
                // update the cost and heuristic of the neighbor
                // and add the neighbor to the open list
                if (newCost < cost[neighbor])
                {
                    cost[neighbor] = newCost;
                    from[neighbor] = current;
                    //update closest explore node
                    if (closestExploredNode == -1 || Vector3.Distance(points[neighbor], points[end]) < Vector3.Distance(points[closestExploredNode], points[end]))
                    {
                        closestExploredNode = neighbor;
                    }

                    if (!openList.Contains(neighbor))
                    {
                        openList.Enqueue(neighbor, Ffunc(neighbor));
                    } else {
                        openList.UpdatePriority(neighbor, Ffunc(neighbor));
                    }
                }
            }
        }

        //if we cant find a path we still want to move towards the goal so we will return a path to the closes point we found
        if (closestExploredNode != -1)
        {
            return ReconstructPath(start, closestExploredNode);
        }
        else
        {
            return new List<int>();
        }







    }
    

}