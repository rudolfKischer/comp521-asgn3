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
        Vector3 goalPos = goal.transform.position;
        Vector3 direction = goalPos - transform.position;
        direction.y = 0;
        direction.Normalize();
        transform.position += direction * speed * Time.deltaTime;
    }


}
