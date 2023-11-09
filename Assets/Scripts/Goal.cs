using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{

    // The goal respawns if a human touches/ collides with it
    // it should respawn at a random point within the dimensions of the terrain
    // it should be disabled for a short time after respawning

    [SerializeField]
    private GameObject terrain;

    public bool inPlay = true;

    [SerializeField]
    private float respawnTime = 3.0f;

    private float timeSinceDespawn = 0.0f;

    private float spawnHeight = 1.0f;

    private void Respawn() {
        //enable the goal

        inPlay = true;
        timeSinceDespawn = 0.0f;
        // re-enable renderer and collider
        Collider collider = GetComponent<Collider>();
        collider.enabled = true;
        Renderer renderer = GetComponent<Renderer>();
        renderer.enabled = true;

        //set the position to a random point on the terrain
        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainScale = terrain.transform.lossyScale;
        Vector3 terrainMin = terrainPos - terrainScale / 2.0f;
        Vector3 terrainMax = terrainPos + terrainScale / 2.0f;
        Vector3 newPos = new Vector3(Random.Range(terrainMin.x, terrainMax.x), spawnHeight, Random.Range(terrainMin.z, terrainMax.z));
        transform.position = newPos;
    }

    private void Start()
    {
        Respawn();
    }

    private void Update()
    {

        //if the goal is disabled, increment time since respawn
        if (!inPlay) {
            timeSinceDespawn += Time.deltaTime;
        }

        if (timeSinceDespawn >= respawnTime) {
            Respawn();
        }
    }

    private void Despawn() {
        //disable the goal
        timeSinceDespawn = 0.0f;
        inPlay = false;
        //disable renderer and collider

        Collider collider = GetComponent<Collider>();
        collider.enabled = false;
        Renderer renderer = GetComponent<Renderer>();
        renderer.enabled = false;
    }


    //on collision
    private void OnCollisionEnter(Collision collision)
    {
        //if the collision is with a human
        if (collision.gameObject.GetComponent<Human>() != null)
        {
            //disable the goal
            Despawn();
        }
    }



}
