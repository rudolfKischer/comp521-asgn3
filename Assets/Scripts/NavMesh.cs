using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMesh<T> : MonoBehaviour
{

    //need an array of points
    [SerializeField]
    protected T[] points; //nodes

    //edges are an array of lists , each lists indexes, which correspond to the points that are connected to the point at the index of the list
    // which specifies which points are connected to the point at the index of the list
    protected List<int>[] edges; // use adjacency list

    protected List<Vector2> allEdges = new List<Vector2>();

    void Start() {
        points = new T[0];
        edges = new List<int>[0];
    }

    public T[] GetPoints() { return points; }
    public List<int>[] GetEdges() { return edges; }
    public void SetPoints(T[] points) { this.points = points; }
    public void SetEdges(List<int>[] edges) { this.edges = edges; }

    public void AddEdge(Vector2 edge) {
        // add edge to both points
        edges[(int)edge.x].Add((int)edge.y);
        edges[(int)edge.y].Add((int)edge.x);
        allEdges.Add(edge);
    }

    public void RemovEdge(Vector2 edge)
    {
        // remove edge from both points
        edges[(int)edge.x].Remove((int)edge.y);
        edges[(int)edge.y].Remove((int)edge.x);
    }

}