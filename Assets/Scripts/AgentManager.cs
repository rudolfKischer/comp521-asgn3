using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class AgentManager : MonoBehaviour
{
    //Want to have a list of agents

    [SerializeField]
    private GameObject humanPrefab;
    [SerializeField]
    private GameObject chairPrefab;
    [SerializeField]
    private GameObject terrainObject;
    [SerializeField]
    private int gridSpacing = 10;



    [SerializeField]
    private int numHumans = 5;
    [SerializeField]
    private int numChairs = 10;          

    private List<GameObject> humans;
    private List<GameObject> chairs;

    
    private List<GameObject> InstatiateAgents(GameObject prefab, int numAgents) {
        List<GameObject> agents = new List<GameObject>();
        for (int i = 0; i < numAgents; i++) {
            GameObject agent = Instantiate(prefab);
            agents.Add(agent);
        }
        return agents;
    }

    private List<Vector2> GridPoints(Vector2 yRange, Vector2 xRange, Vector2 spacing) {
        // Create a list of points that are spaced out over the range
        List<Vector2> points = new List<Vector2>();
        for (float y = yRange.x; y <= yRange.y; y += spacing.y) {
            for (float x = xRange.x; x <= xRange.y; x += spacing.x) {
                points.Add(new Vector2(x, y));
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

    private void DistributeAgents(List<GameObject> agents, List<Vector2> points) {
        if (agents.Count > points.Count) {
            Debug.LogWarning("Not enough points to distribute all agents.");
        }

        ShuffleList(points);
        for (int i = 0; i < agents.Count; i++) {
            agents[i].transform.position = new Vector3(points[i].x, 0, points[i].y);
        }
    }

    private void GridAgentDistribution(List<GameObject> agents, Vector2 yRange, Vector2 xRange, Vector2 spacing) {
        // Create a list of points that are spaced out over the range
        List<Vector2> gridPoints = GridPoints(yRange, xRange, spacing);
        DistributeAgents(agents, gridPoints);
    }

    void Start() {

        humans = InstatiateAgents(humanPrefab, numHumans);
        chairs = InstatiateAgents(chairPrefab, numChairs);

        // distibute humans and chairs over the area of the terrain
        // assume the terrain is a axis aligned cube with any dimensions

        // get the dimensions of the terrain
        Vector3 terrainDimensions = terrainObject.transform.localScale;
        Vector3 terrainPosition = terrainObject.transform.position;
        Vector2 terrainXRange = new Vector2(-terrainDimensions.x / 2.0f, terrainDimensions.x / 2.0f);
        Vector2 terrainYRange = new Vector2(-terrainDimensions.y / 2.0f, terrainDimensions.y / 2.0f);


        // distribute humans and chairs over the terrain
        GridAgentDistribution(humans, terrainYRange, terrainXRange, new Vector2(gridSpacing, gridSpacing));

    }


    // On start we randomly spawn n number of humans
    // On start we randomly spawn n number of chairs
    // they should be distributed randomly around the map, without overlapping


}
