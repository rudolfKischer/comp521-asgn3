using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GridNavMesh : NavMesh<Vector3>
{

  //The nav mesh should cover the width and length (x and z) of the object its attached to
  [SerializeField, Range(2, 100)]
  private int numOfPointsX = 10;
  [SerializeField, Range(2, 100)]
  private int numOfPointsZ = 10;
  [SerializeField]
  private bool linkNumZtoX = true;

  //Debug
  [SerializeField]
  private bool displayGridPoints = true;
  [SerializeField]
  private bool displayGridEdges = true;



  private int PointIndex(int x, int z) {
    return (int)(x + z * numOfPointsX);
  }


  private void CreatePoints() {
    // get edge points of the object
    allEdges.Clear();
    Collider collider = GetComponent<Collider>();
    float minX = collider.bounds.min.x;
    float maxX = collider.bounds.max.x;
    float minZ = collider.bounds.min.z;
    float maxZ = collider.bounds.max.z;

    //There should always be points on each end of the bound
    float xStep = (maxX - minX) / (numOfPointsX - 1);
    float zStep = (maxZ - minZ) / (numOfPointsZ - 1);

    //add points to the nav mesh, need to convert to single array
    points = new Vector3[numOfPointsX * numOfPointsZ];
    for (int z = 0; z < numOfPointsZ; z++) {
      for (int x = 0; x < numOfPointsX; x++) {
        points[PointIndex(x, z)] = new Vector3(x * xStep + minX, 0, z * zStep + minZ);
      }
    }
    //note that the index of each grid point , index = x + z * numOfPointsX
    // so the index of the point at (x,z) is x + z * numOfPointsX

    // create edges
    SetEdges(new List<int>[numOfPointsX * numOfPointsZ]);
    for (int i = 0; i < numOfPointsX * numOfPointsZ; i++) {
      edges[i] = new List<int>();
    }
    // we want to connect each point to the points around it
    // this includes the points to the left, right, up, and down, and the diagonals
    // we only want to add each edge once
    //so we do horizontals, then verticals, then diagonals

    // horizontals
    for (int z = 0; z < numOfPointsZ; z++) {
      for (int x = 0; x < numOfPointsX - 1; x++) {
        // add edge to both points=
        AddEdge(new Vector2(PointIndex(x, z), PointIndex(x + 1, z)));
      }
    }

    //draw verticals
    for (int z = 0; z < numOfPointsZ - 1; z++) {
      for (int x = 0; x < numOfPointsX; x++) {
        // add edge to both points
        AddEdge(new Vector2(PointIndex(x, z), PointIndex(x, z + 1)));
      }
    }

    //draw diagonals
    for (int z = 0; z < numOfPointsZ - 1; z++) {
      for (int x = 0; x < numOfPointsX - 1; x++) {
        // add edge to both points
        AddEdge(new Vector2(PointIndex(x, z), PointIndex(x + 1, z + 1)));
        AddEdge(new Vector2(PointIndex(x + 1, z), PointIndex(x, z + 1)));
      }
    }

  }

  void Start()
  {
    //create the points
    CreatePoints();
  }

  # if UNITY_EDITOR

  private void OnValidate()
  {
    //create the points
    if (linkNumZtoX) {
      numOfPointsZ = numOfPointsX;
    }
    CreatePoints();
  }

  #endif

  void DrawGridPoints() {
    //draw the points
    foreach (Vector3 point in this.points) {
      Gizmos.DrawSphere(point, 0.1f);
    }
  }

  void DrawGridLines() {
    //draw the edges
    // make edge colour black
    Gizmos.color = Color.black;
    for (int i = 0; i < this.allEdges.Count; i++) {
      Vector3 point1 = this.points[(int)this.allEdges[i].x];
      Vector3 point2 = this.points[(int)this.allEdges[i].y];
      Gizmos.DrawLine(point1, point2);
    }
  }


  void OnDrawGizmos()
  {
    if (this.points != null && displayGridPoints) {
      DrawGridPoints();
    }

    //draw the edges
    // make edge colour black
    Gizmos.color = Color.black;
    if (this.edges != null && displayGridEdges) {
      DrawGridLines();
    }
  }
}
