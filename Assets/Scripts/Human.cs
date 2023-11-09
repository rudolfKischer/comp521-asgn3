using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour
{
    [SerializeField]
    public GameObject goal;
    [SerializeField]
    public float speed = 1.0f;


    //slowly move towards goal with only x and z velociy * speed

    private void Start()
    {
        //set goal to a random point on the nav mesh

    }

    private void Update()
    {
        //move towards goal
        // if the goal is not enabled then return
        //get goal component
        Goal goalComponent = goal.GetComponent<Goal>();
        //if goal component in play is false then return
        if (!goalComponent.inPlay) {
            return;
        }

        Vector3 goalPos = goal.transform.position;
        Vector3 direction = goalPos - transform.position;
        direction.y = 0;
        direction.Normalize();
        //use rigid body velocity to move
        // make sure to only set the velocity in the x and z directions
        // need to leave the y velocity alone
        GetComponent<Rigidbody>().velocity = direction * speed;

    }


}
