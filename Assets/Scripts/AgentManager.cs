using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;




public class AgentManager : MonoBehaviour
{
    //Want to have a list of agents

    public static AgentManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    

    [SerializeField]
    private GameObject humanPrefab;
    [SerializeField]
    private GameObject chairPrefab;
    [SerializeField]
    private GameObject terrainObject;
    [SerializeField]
    private GameObject goalObject;


    private int gridSpacing;



    [SerializeField, Range(1, 300)]
    private int numHumans = 5;
    [SerializeField]
    private bool lockChairMovement = true;

    [SerializeField, Range(1, 300)]
    private int numChairs = 10;          
    [SerializeField, Range(0.001f, 1.0f)]
    private float gridSparsity = 1.0f;
    [SerializeField]
    private float terrain_coverage = 0.8f;
    [SerializeField]
    private bool displayGrid = true;

    [SerializeField]
    private float humanTimeToCrossTerrain = 10.0f;

    // Navigation Terrain
    [SerializeField]
    private GameObject navigationTerrain;
    private GridNavMesh navMesh;

    [SerializeField]
    private bool lockNavMesh = true;
    [SerializeField, Range(0.001f, 1.0f)]
    private float navResolution = 0.1f;

    [SerializeField]
    private bool humanObstacles = true;





    private List<GameObject> humans;
    private List<GameObject> chairs;
    private List<GameObject> global_agents = new List<GameObject>();
    private List<Vector2> gridPoints = new List<Vector2>();
    
    private List<GameObject> InstatiateAgents(GameObject prefab, int numAgents) {
        List<GameObject> agents = new List<GameObject>();
        for (int i = 0; i < numAgents; i++) {
            GameObject agent = Instantiate(prefab);
            agent.transform.parent = transform;
            agents.Add(agent);
            global_agents.Add(agent);
            // shrink size by a very small amount to avoid overlapping agents
            agent.transform.localScale = 0.95f * agent.transform.localScale;

        }
        return agents;
    }

    private Vector2[,] PointGrid(Vector2 yRange, Vector2 xRange, Vector2 spaces) {
        // Create a list of points that are spaced out over the range
        Vector2 spacing = new Vector2( (xRange.y - xRange.x) / spaces.x, (yRange.y - yRange.x) / spaces.y);
        Vector2[,] points = new Vector2[(int)spaces.x,(int)spaces.y];
        for (int y = 0; y < spaces.y; y++) {
            for (int x = 0; x < spaces.x; x++) {
                points[x,y] = new Vector2(x * spacing.x + xRange.x, y * spacing.y + yRange.x);
            }
        }

        return points;
        
    }

    private List<Vector2> GridPoints(Vector2 yRange, Vector2 xRange, Vector2 spaces) {
        // Create a list of points that are spaced out over the range
        List<Vector2> points = new List<Vector2>();
        Vector2[,] pointGrid = PointGrid(yRange, xRange, spaces);
        for (int y = 0; y < spaces.y; y++) {
            for (int x = 0; x < spaces.x; x++) {
                points.Add(pointGrid[x,y]);
            }
        }
        return points;
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private float MidpointHeight(GameObject agent) {
      //use collider to get the height of the agent
      Vector3 globalScale = agent.transform.lossyScale;
      Collider agentCollider = agent.GetComponent<Collider>();
      if (agentCollider == null) {
        return globalScale.y / 2.0f;
      }
      return agentCollider.bounds.size.y * globalScale.y / 2.0f;
    }

    private void DistributeAgents(List<GameObject> agents, List<Vector2> points) {
        if (agents.Count > points.Count) {
            Debug.LogWarning("Not enough points to distribute all agents.");
            return;
        }

        ShuffleList(points);
        for (int i = 0; i < agents.Count; i++) {
            float midpointHeight = MidpointHeight(agents[i]);
            agents[i].transform.position = new Vector3(points[i].x, midpointHeight , points[i].y);
        }
    }

    private void GridAgentDistribution(List<GameObject> agents, Vector2 yRange, Vector2 xRange, Vector2 spacing) {
        // Create a list of points that are spaced out over the range

        gridPoints = GridPoints(yRange, xRange, spacing);
        DistributeAgents(agents, gridPoints);
    }

    private int GetGridSpacing() {
      return (int)(Mathf.Ceil(Mathf.Sqrt((numHumans + numChairs) * (1.0f / gridSparsity))));
    }

    private Vector2[] GetTerrainRanges() {
        // get the dimensions of the terrain
        Vector3 terrainDimensions = terrainObject.transform.localScale;

        Vector2 terrainXRange = new Vector2(-terrainDimensions.x / 2.0f, terrainDimensions.x / 2.0f) * terrain_coverage;
        Vector2 terrainZRange = new Vector2(-terrainDimensions.z / 2.0f, terrainDimensions.z / 2.0f) * terrain_coverage;

        return new Vector2[] {terrainXRange, terrainZRange};
    }

    private float GetMinSpacing(Vector2 terrainXRange, Vector2 terrainZRange) {
        // get the dimensions of the terrain
        gridSpacing = GetGridSpacing();
        float horizontalSpacing = (terrainXRange.y - terrainXRange.x) / gridSpacing;
        float verticalSpacing = (terrainZRange.y - terrainZRange.x) / gridSpacing;
        return Mathf.Min(horizontalSpacing, verticalSpacing);

    }

    private float GetHumanSpeed() {
        Vector3 terrainDimensions = terrainObject.transform.lossyScale;
        //get the x and z components of the terrain dimensions
        float terrainX = terrainDimensions.x;
        float terrainZ = terrainDimensions.z;
        //get the x and z components of the human colliders aabb bounds
        // get the first human in the list
        if (humans.Count == 0) {
            return 0.0f;
        }
        GameObject human = humans[0];
        Collider humanCollider = human.GetComponent<Collider>();
        Vector3 humanBounds = humanCollider.bounds.size;
        float humanX = humanBounds.x;
        float humanZ = humanBounds.z;
        //set the speed of the human so that it takes 10 seconds to cross the terrain
        float speed = (terrainX + terrainZ) / (humanTimeToCrossTerrain * (humanX + humanZ));
        return speed;

    }

    void SpawnAgents() {
        ClearAgents();

        gridSpacing = GetGridSpacing();

        humans = InstatiateAgents(humanPrefab, numHumans);
        float speed = GetHumanSpeed();
        for (int i = 0; i < humans.Count; i++) {
            Human humanComponent = humans[i].GetComponent<Human>();
            humanComponent.goal = goalObject;
            //we want to set the speed, so that the human takes 10 seconds to cross the terrain
            //get the dimensions of the terrain
            humans[i].GetComponent<Human>().speed = speed;
        }
        chairs = InstatiateAgents(chairPrefab, numChairs);
        if (lockChairMovement) {
            //lock rigid body
            for (int i = 0; i < chairs.Count; i++) {
                Rigidbody rigidBody = chairs[i].GetComponent<Rigidbody>();
                rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            }
            // set chair to be half od humans speed
            for (int i = 0; i < chairs.Count; i++) {
                chairs[i].GetComponent<Agent>().speed = speed * 0.66f;
            }
        }

        // distibute humans and chairs over the area of the terrain
        // assume the terrain is a axis aligned cube with any dimensions

        Vector2[] terrainRanges = GetTerrainRanges();
        Vector2 terrainXRange = terrainRanges[0];
        Vector2 terrainZRange = terrainRanges[1];
        float min_spaceing = GetMinSpacing(terrainXRange, terrainZRange);
        //scale the agents to fit the grid spacing
        for (int i = 0; i < humans.Count; i++) {
            humans[i].transform.localScale = min_spaceing * Vector3.one;
        }
        for (int i = 0; i < chairs.Count; i++) {
            chairs[i].transform.localScale = min_spaceing * Vector3.one;
        }

        //shrink the goal aswell
        goalObject.transform.localScale = min_spaceing * Vector3.one;

        GridAgentDistribution(global_agents, terrainZRange, terrainXRange, new Vector2(gridSpacing, gridSpacing));

        

    }

    public List<Vector3> GetPath(int start, int end) {

        // get the path from the A* solver
        AStarSolver solver = AStarSolver.Instance;
        List<int> path = solver.Solve(start, end);

        // convert the path from a list of indices to a list of points
        List<Vector3> pathPoints = new List<Vector3>();
        foreach (int index in path) {
            pathPoints.Add(navMesh.GetPoint(index));
        }
        pathPoints.Add(navMesh.GetPoint(end));

        return pathPoints;


    }

    public List<Vector3> GetPath(Vector3 start, Vector3 end) {
        int startIndex = navMesh.GetPointIndex(start);
        int endIndex = navMesh.GetPointIndex(end);
        startIndex = navMesh.GetClosestUnoccupiedTowardsPoint(startIndex, end);
        endIndex = navMesh.GetClosestUnoccupiedTowardsPoint(endIndex, start);
        if (startIndex == -1) {
            Debug.Log("No unoccupied point found.");
            return new List<Vector3>();
        }

        return GetPath(startIndex, endIndex);
    }



    public void SetPathHuman(GameObject human) {
      Vector3 start = human.transform.position;
      Vector3 end = human.GetComponent<Human>().goal.transform.position;
      List<Vector3> pathPoints = GetPath(start, end);

      // set the path of the human
      human.GetComponent<Human>().SetPath(pathPoints);
    }

    public void SetPathChair(GameObject chair) {
      Vector3 start = chair.transform.position;
      GameObject closestPlayer = GetClosestHuman(start);
      Vector3 end = closestPlayer.transform.position;
      List<Vector3> pathPoints = GetPath(start, end);
      chair.GetComponent<Agent>().SetPath(pathPoints);
    }

    public void SetHumanPaths() {
        for (int i = 0; i < humans.Count; i++) {
            Human humanComponent = humans[i].GetComponent<Human>();
            if (humanComponent.dirtyPath) {
                SetPathHuman(humans[i]);
                humanComponent.dirtyPath = false;
            }
        }
    }

    public void SetSolverGraph() {
        AStarSolver solver = AStarSolver.Instance;
        if (solver == null) {
          Debug.Log("No A* solver found.");
          return;
        }
        List<int>[] edges = navMesh.GetReducedEdges();
        Vector3[] points = navMesh.GetPoints();
        solver.SetGraph(points, edges);
    }

    public void MarkPathedCells(List<Vector3> path) {
        List<int> pathIndices = new List<int>();
        foreach (Vector3 point in path) {
            pathIndices.Add(navMesh.GetPointIndex(point));
        }
        navMesh.MarkPathedCells(pathIndices);
    }

    public void MarkAllHumansPaths() {
        navMesh.ClearPathedCells();
        for (int i = 0; i < humans.Count; i++) {
            //Mark pathed cells from human path
            Human humanComponent = humans[i].GetComponent<Human>();
            List<Vector3> path = humanComponent.GetPath();
            if (path != null && path.Count > 0) {
                MarkPathedCells(path);
            }
        }
    }

    private GameObject GetClosestHuman(Vector3 position) {
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;
        foreach (GameObject player in humans) {
            float distance = Vector3.Distance(position, player.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestPlayer = player;
            }
        }
        return closestPlayer;
    }


    private void SetAllChairPaths() {
      // set the path of each chair to the closest player
      for (int i = 0; i < chairs.Count; i++) {
        Agent chairComponent = chairs[i].GetComponent<Agent>();
        //if the path is not dirty, skip
        if (!chairComponent.dirtyPath) {
          continue;
        }
        SetPathChair(chairs[i]);
        chairComponent.dirtyPath = false;
      }

      // display the path of each chair
      for (int i = 0; i < chairs.Count; i++) {
        //mark pathed cells
        Agent chairComponent = chairs[i].GetComponent<Agent>();
        List<Vector3> path = chairComponent.GetPath();
        if (path != null && path.Count > 0) {
            MarkPathedCells(path);
        }
      }
    }


    // every update, we need to update which cells are occupied

    void Start()
    {
        humans = new List<GameObject>();
        chairs = new List<GameObject>();


        this.navMesh =  navigationTerrain.GetComponent<GridNavMesh>();
        if (navMesh == null) {
            Debug.Log("No navigation mesh found.");
            return;

        }
        //get min spacing
        Vector2[] terrainRanges = GetTerrainRanges();
        if (lockNavMesh) {
            navMesh.SetGridSize(GetMinSpacing(terrainRanges[0], terrainRanges[1]) * (1 - navResolution));
        }
        navMesh.Setup();

        //set the solver graph
        navMesh.ClearOccupiedCells();
        navMesh.OccupyCells(global_agents);
        SetSolverGraph();
        SetHumanPaths();
        MarkAllHumansPaths();

        //set the nav collision bound to hald the width of a human

        //wait for a little bit to spawn the agents
        IEnumerator WaitAndSpawnAgents() {
            yield return new WaitForSeconds(0.1f);
            SpawnAgents();
        }
        StartCoroutine(WaitAndSpawnAgents());



    }



    void Update()
    { 
        if (!goalObject.GetComponent<Goal>().inPlay) {
            for (int i = 0; i < humans.Count; i++) {
                humans[i].GetComponent<Human>().SetPath(new List<Vector3>());
            }
            for (int i = 0; i < chairs.Count; i++) {
                chairs[i].GetComponent<Agent>().SetPath(new List<Vector3>());
            }
            // freexe their velocity
            for (int i = 0; i < global_agents.Count; i++) {
                Rigidbody rigidBody = global_agents[i].GetComponent<Rigidbody>();
                rigidBody.velocity = Vector3.zero;
            }
            return;
        }
        // 
        if (humans.Count != 0) {
            //set bound to half width of human
            // use human collider
            Collider humanCollider = humans[0].GetComponent<Collider>();
            if (humanCollider != null) {
                navMesh.navCollisionBound = 0.25f * humanCollider.bounds.size.x;
            }
        }

        float humanWidth = 0.0f;
        if (humans.Count > 0) {
            Collider humanCollider = humans[0].GetComponent<Collider>();
            if (humanCollider != null) {
                humanWidth = humanCollider.bounds.size.x;
            }
        }

        if (lockChairMovement) {
          // dont pathfind for chairs
        }
        navMesh.ClearOccupiedCells();
        navMesh.OccupyCells(humans, 0.0f);
        navMesh.OccupyCells(chairs, -humanWidth * 0.7f);
        SetSolverGraph();
        SetAllChairPaths();



        navMesh.ClearOccupiedCells();
        //we want to set the bound dist to be 0 on humans
        // and then on chairs we want to set it to the width of a human collider
        // this is so that the path planning goes far out of its way to avoid chairs
        // but not humans
        float humanBoundDist = -humanWidth * 0.75f;
        float chairBoundDist = 0.0f;

        navMesh.OccupyCells(humans, humanBoundDist);
        navMesh.OccupyCells(chairs, chairBoundDist);
        SetSolverGraph();
        SetHumanPaths();
        MarkAllHumansPaths();




        // if the goal is not in play
        // clear the players paths

    }

    //need to to clear agents on when we exit play mode


    void OnDestroy()
    {
        ClearAgents();
    }

private void ClearAgents() {
    for (int i = transform.childCount - 1; i >= 0; --i)
    {
        GameObject child = transform.GetChild(i).gameObject;
        // if the child is an agent has tag 'Agent' destroy it
        if (child.CompareTag("Agent"))
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

    }
    foreach (var agent in global_agents) {
        if (agent != null) {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying) {
                Destroy(agent);
            } else {
                DestroyImmediate(agent);
            }
#else
            Destroy(agent);
#endif
        }
    }
    global_agents.Clear();
}



    #if UNITY_EDITOR
    void DisplayGrid() {
        Gizmos.color = Color.red;
        Vector3 terrainPosition = terrainObject.transform.position;
        float terrainYLevel = terrainPosition.y + terrainObject.transform.localScale.y / 2.0f;

        foreach (Vector2 point in gridPoints) {
          Vector3 gizmoPoint = new Vector3(point.x, terrainYLevel, point.y);
          Gizmos.DrawSphere(gizmoPoint, 0.1f);
        }
    }

    void OnDrawGizmos()
    {
      if (terrainObject != null && gridPoints != null && displayGrid) {
        DisplayGrid();
      }
    }

    #endif


    #if UNITY_EDITOR

    void OnEnable()
    {
        UnityEditor.EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
    }

    void OnDisable()
    {
        UnityEditor.EditorApplication.playModeStateChanged -= HandleOnPlayModeChanged;
    }

    void OnValidate()
    {
      if (Application.isPlaying) return;
      UnityEditor.EditorApplication.delayCall += DelayedValidation;
        
    }

    private void DelayedValidation(){
      if (this != null)
      {
        UnityEditor.Undo.RecordObject(this, "AgentManager Validation");
        //destroy all children aswell

        SpawnAgents();
        UnityEditor.EditorUtility.SetDirty(this);

      }

    }

    private void HandleOnPlayModeChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state == UnityEditor.PlayModeStateChange.EnteredEditMode)
        {
            // Clean up when returning to edit mode, if necessary.
            ClearAgents();
        }

        // clear agents when entering play mode aswell
        if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode)
        {
            // Clean up when returning to edit mode, if necessary.
            ClearAgents();
        }

    }
    #endif


}
