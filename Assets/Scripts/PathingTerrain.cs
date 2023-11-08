using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PathingTerrain : MonoBehaviour
{
    [SerializeField] 
    private Vector3 dimensions = new Vector3(10, 1, 10);

    [SerializeField]
    private float wallThickness = 0.1f;
    [SerializeField]
    private float wallHeight = 1.0f;

    private GameObject[] walls = new GameObject[4];


    private void CreateWalls(){


        

        //find child with name "Walls"
        Transform oldWallsParent = transform.Find("Walls");
        if (oldWallsParent != null)
        {
          //destroy it and all of its children
          DestroyImmediate(oldWallsParent.gameObject);
        }

        //create a new game object to be the parent of the walls
        GameObject wallsParent = new GameObject("Walls");
        wallsParent.transform.parent = transform;
        wallsParent.transform.localScale = Vector3.one;
        //set walls scale to identity

        //create walls around the terrain made of cubes
        float rotation = 0;
        for (int i = 0; i < 4; i++)
        {
            if (walls[i] != null) {
                DestroyImmediate(walls[i]);
            }
            walls[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walls[i].transform.parent = wallsParent.transform;

            //set the position and scale of the wall
            walls[i].transform.localScale = new Vector3(wallThickness, wallHeight, 1.0f);
            walls[i].transform.position = new Vector3(dimensions.x / 2.0f, 0, 0);
            walls[i].transform.localRotation = Quaternion.identity;
            walls[i].transform.RotateAround(transform.position, Vector3.up, rotation);
            //face the correct direction
            rotation += 90;
        }

    }

    private void OnValidate()
    {
        transform.localScale = dimensions;
        transform.position = new Vector3(0, -dimensions.y / 2.0f, 0);

        UnityEditor.EditorApplication.delayCall += CreateWalls;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
