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
  [SerializeField]
  private bool displayGridTiles = true;

  //need to keep track of if a cell is occupied
  [SerializeField]
  private bool[] occupiedCells;

  private int PointIndex(int x, int z) {
    return (int)(x + z * numOfPointsX);
  }

  private Vector2 IndexCoords(int index) {
    int x = index % numOfPointsX;
    int z = index / numOfPointsX;
    return new Vector2(x, z);
  }





  private Vector2 GetXRange() {
    Collider collider = GetComponent<Collider>();
    float minX = collider.bounds.min.x;
    float maxX = collider.bounds.max.x;
    float diff = maxX - minX;
    float step = diff / (numOfPointsX + 1) * 0.5f;
    float newMinX = minX + step;
    float newMaxX = maxX - step;
    return new Vector2(newMinX, newMaxX);
  }

  private Vector2 GetZRange() {
    Collider collider = GetComponent<Collider>();
    float minZ = collider.bounds.min.z;
    float maxZ = collider.bounds.max.z;
    float diff = maxZ - minZ;
    float step = diff / (numOfPointsZ + 1) * 0.5f;
    float newMinZ = minZ + step;
    float newMaxZ = maxZ - step;
    return new Vector2(newMinZ, newMaxZ);
  }

  private float GetXStep() {
    Vector2 xRange = GetXRange();
    float minX = xRange.x;
    float maxX = xRange.y;
    return (maxX - minX) / (numOfPointsX - 1);
  }

  private float GetZStep() {
    Vector2 zRange = GetZRange();
    float minZ = zRange.x;
    float maxZ = zRange.y;
    return (maxZ - minZ) / (numOfPointsZ - 1);
  }

  private void CreateEdges() {
    allEdges.Clear();

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


  private void CreatePoints() {
    // get edge points of the object
    Vector2 xRange = GetXRange();
    Vector2 zRange = GetZRange();
    float minX = xRange.x;
    float maxX = xRange.y;
    float minZ = zRange.x;
    float maxZ = zRange.y;

    //There should always be points on each end of the bound
    float xStep = GetXStep();
    float zStep = GetZStep();

    //add points to the nav mesh, need to convert to single array
    points = new Vector3[numOfPointsX * numOfPointsZ];
    for (int z = 0; z < numOfPointsZ; z++) {
      for (int x = 0; x < numOfPointsX; x++) {
        points[PointIndex(x, z)] = new Vector3(minX + x * xStep, 0, minZ + z * zStep);
      }
    }
    //note that the index of each grid point , index = x + z * numOfPointsX
    // so the index of the point at (x,z) is x + z * numOfPointsX
  }

  //Given a point, we need to be able to get the index of the point that it maps to
  //on the grid
  //if the point is outside of the grid, return -1
  private int GetPointIndex(Vector3 point) {
    // use the collider to get the bounds of the object
    Collider collider = GetComponent<Collider>();
    float minX = collider.bounds.min.x;
    float maxX = collider.bounds.max.x;
    float minZ = collider.bounds.min.z;
    float maxZ = collider.bounds.max.z;

    //check if point is outside of the grid
    if (point.x < minX || point.x > maxX || point.z < minZ || point.z > maxZ) {
      return -1;
    }

    //get the index of the point
    float xStep = GetXStep();
    float zStep = GetZStep();
    int x = (int)((point.x - minX) / xStep);
    int z = (int)((point.z - minZ) / zStep);
    return PointIndex(x, z);
  }

  //Given a point, we want to mark that cell as occupied
  public void OccupyCell(Vector3 point) {
    int index = GetPointIndex(point);
    if (index != -1) {
      occupiedCells[index] = true;
    } else {
      Debug.LogWarning("Point " + point + " is outside of the grid.");
    }
  }

  //given a list GameObjects, we want to mark the cells that they are in as occupied, using their transform
  public void OccupyCells(List<GameObject> objects) {
    // clear occupied cells
    for (int i = 0; i < occupiedCells.Length; i++) {
      occupiedCells[i] = false;
    }
    foreach (GameObject obj in objects) {
      //do it for each point in the axis aligned bounding box of the objects collider
      Collider collider = obj.GetComponent<Collider>();
      if (collider == null) {
        Debug.LogWarning("Object " + obj + " does not have a collider.");
        continue;
      }
      // we want the 2 corner points of the collider
      Vector3 minBound = collider.bounds.min;
      Vector3 maxBound = collider.bounds.max;

      // we want to convert these corner points to indices
      // then because these are axis aligned
      // we can just iterate through the points between the min and max x and z values
      // and mark those cells as occupied

      int minIndex = GetPointIndex(minBound);
      int maxIndex = GetPointIndex(maxBound);
      Vector2 minCoords = IndexCoords(minIndex);
      Vector2 maxCoords = IndexCoords(maxIndex);

      for (int z = (int)minCoords.y; z <= (int)maxCoords.y; z++) {
        for (int x = (int)minCoords.x; x <= (int)maxCoords.x; x++) {
          int occupiedIndex = PointIndex(x, z);
          if (occupiedIndex >= 0 && occupiedIndex < occupiedCells.Length) {
            occupiedCells[occupiedIndex] = true;
          }
        }
      }
    }
  }
  
  public void SetNumPoints(int x, int z) {
    numOfPointsX = x;
    numOfPointsZ = z;
  }

  public void SetGridSize(float size) {
    //do some calculations to figure out how many points we need to get grid tile to be size x size
    // use collider bounds
    Collider collider = GetComponent<Collider>();
    float minX = collider.bounds.min.x;
    float maxX = collider.bounds.max.x;
    float minZ = collider.bounds.min.z;
    float maxZ = collider.bounds.max.z;

    numOfPointsX = (int)((maxX - minX) / size) + 1;
    numOfPointsZ = (int)((maxZ - minZ) / size) + 1;

  }

  public void Setup() {
    //create the points
    CreatePoints();
    CreateEdges();
    if (occupiedCells == null || occupiedCells.Length != points.Length) {
      occupiedCells = new bool[points.Length];
    }
  }

  void Start()
  {
    Setup();
  }

  # if UNITY_EDITOR

  private void OnValidate()
  {
    //create the points
    if (linkNumZtoX) {
      numOfPointsZ = numOfPointsX;
    }
    Setup();
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

  void DrawTiles() {
    //draw very thin gzmos cube for tiles, centered on each point, that is the size of the tile
    for (int i = 0; i < this.points.Length; i++) {
      Gizmos.color = Color.white;
      if (occupiedCells[i]) {
        Gizmos.color = Color.red;
      }
      Vector3 point = this.points[i];
      Gizmos.DrawCube(point, new Vector3(GetXStep(), 0.01f, GetZStep()));
      //draw outline of this cube
      Gizmos.color = Color.black;
      Gizmos.DrawLine(point + new Vector3(-GetXStep() / 2.0f, 0, -GetZStep() / 2.0f), point + new Vector3(GetXStep() / 2.0f, 0, -GetZStep() / 2.0f));
      Gizmos.DrawLine(point + new Vector3(GetXStep() / 2.0f, 0, -GetZStep() / 2.0f), point + new Vector3(GetXStep() / 2.0f, 0, GetZStep() / 2.0f));
      Gizmos.DrawLine(point + new Vector3(GetXStep() / 2.0f, 0, GetZStep() / 2.0f), point + new Vector3(-GetXStep() / 2.0f, 0, GetZStep() / 2.0f));
      Gizmos.DrawLine(point + new Vector3(-GetXStep() / 2.0f, 0, GetZStep() / 2.0f), point + new Vector3(-GetXStep() / 2.0f, 0, -GetZStep() / 2.0f));
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

    
    //draw very thin gzmos cube for tiles, centered on each point, that is the size of the tile
    if (this.points != null && displayGridTiles) {
      DrawTiles();
    }
    

  }
}
