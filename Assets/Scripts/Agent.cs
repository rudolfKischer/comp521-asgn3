using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField]
    public GameObject goal;
    [SerializeField]
    public float speed = 1.0f;

    public bool dirtyPath = true;

    private float stalePathTime = 2.0f;
    private float stalePathTimer = 0.0f;



    

    private List<Vector3> path = new List<Vector3>();


    public void SetPath(List<Vector3> path)
    {
        this.path = path;
    }

    public List<Vector3> GetPath()
    {
        return path;
    }


    //slowly move towards goal with only x and z velociy * speed

    private void Start()
    {
        //set goal to a random point on the nav mesh

    }

    private void UpdatePath() {
        //strategy
        // add a raycast that poins in the direction the human is moving
        // if this raycast collides with anything
        // recalculate the pathh
        AStarSolver solver = AStarSolver.Instance;
        if (solver == null)
        {
            return;
        }

    }

    private void Update()
    {
        //add a random amount of time delta time to stale path timer
        // this is that they dont all change their path at the same time
        stalePathTimer += Time.deltaTime;



        if (stalePathTimer > stalePathTime)
        {
            stalePathTimer = 0.0f;
            float minStaleTime = 0.2f;
            float maxStaleTime = 0.3f;
            stalePathTime = Random.Range(minStaleTime, maxStaleTime);
            dirtyPath = true;
        }
        if (path.Count == 0)
        {
            return;
        }

        //get the closest point in the path
        Vector3 closestPoint = path[0];
        float closestDistance = Vector3.Distance(transform.position, closestPoint);
        int closestIndex = 0;
        for (int i = 1; i < path.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, path[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = path[i];
                closestIndex = i;
            }
        }

        //while the closest point is too close, pick the next point on the path i + 1
        float minDistance = 0.05f;
        //min distance should be equal to the collision radius of the human
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            minDistance = collider.bounds.extents.magnitude;
        }



        closestPoint = path[closestIndex];
        Vector3 direction = closestPoint - transform.position;
        direction.y = 0;
        direction.Normalize();
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = direction * speed;

        // give angular velocity to face the direction of movement
        Vector3 angularVelocity = Vector3.Cross(transform.forward, -direction);
        rb.angularVelocity = angularVelocity * 30.0f;






        if (closestDistance < minDistance && closestIndex < path.Count)
        {
            path.RemoveAt(closestIndex);
        }

    }

    //draw the current velocity direction as a vector
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //draw debug raycast
        Gizmos.DrawRay(transform.position + Vector3.up * GetComponent<Collider>().bounds.extents.y, transform.forward * GetComponent<Collider>().bounds.size.magnitude);
    }

    // if we collide with the goal then remove path
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == goal)
        {
            path.Clear();
        }
    }

}
