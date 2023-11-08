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



    [SerializeField, Range(5, 100)]
    private int numHumans = 5;
    [SerializeField, Range(5, 100)]
    private int numChairs = 10;          
    [SerializeField, Range(0.001f, 1.0f)]
    private float gridSparsity = 1.0f;
    [SerializeField]
    private float terrain_coverage = 0.8f;
    [SerializeField]
    private bool displayGrid = true;

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



    void SpawnAgents() {
        ClearAgents();

        gridSpacing = (int)(Mathf.Ceil(Mathf.Sqrt((numHumans + numChairs) * (1.0f / gridSparsity))));

        humans = InstatiateAgents(humanPrefab, numHumans);
        for (int i = 0; i < humans.Count; i++) {
            humans[i].GetComponent<Human>().goal = goalObject;
        }
        chairs = InstatiateAgents(chairPrefab, numChairs);

        // distibute humans and chairs over the area of the terrain
        // assume the terrain is a axis aligned cube with any dimensions

        // get the dimensions of the terrain
        Vector3 terrainDimensions = terrainObject.transform.localScale;
        Vector3 terrainPosition = terrainObject.transform.position;

        Vector2 terrainXRange = new Vector2(-terrainDimensions.x / 2.0f, terrainDimensions.x / 2.0f) * terrain_coverage;
        Vector2 terrainZRange = new Vector2(-terrainDimensions.z / 2.0f, terrainDimensions.z / 2.0f) * terrain_coverage;


        float horizontalSpacing = (terrainXRange.y - terrainXRange.x) / gridSpacing;
        float verticalSpacing = (terrainZRange.y - terrainZRange.x) / gridSpacing;
        float min_spaceing = Mathf.Min(horizontalSpacing, verticalSpacing);
        //scale the agents to fit the grid spacing
        for (int i = 0; i < humans.Count; i++) {
            humans[i].transform.localScale = new Vector3(min_spaceing, min_spaceing, min_spaceing);
        }
        for (int i = 0; i < chairs.Count; i++) {
            chairs[i].transform.localScale = new Vector3(min_spaceing, min_spaceing, min_spaceing);
        }

        GridAgentDistribution(global_agents, terrainZRange, terrainXRange, new Vector2(gridSpacing, gridSpacing));



    }

    //need to to clear agents on when we exit play mode


    void OnDestroy()
    {
        ClearAgents();
    }

private void ClearAgents() {
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
    }
    #endif


}
