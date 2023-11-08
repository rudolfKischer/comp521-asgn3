using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PathingTerrain : MonoBehaviour
{
    [SerializeField] 
    private Vector3 dimensions = new Vector3(10, 1, 10);

    private void OnValidate()
    {
        transform.localScale = dimensions;
        transform.position = new Vector3(0, -dimensions.y / 2.0f, 0);
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
