using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class constaints : MonoBehaviour
{
    const int count = 5;
    // Start is called before the first frame update
    void Start()
    {
       GameObject balls = GameObject.Find("balls");
       GameObject shpere = balls.transform.Find("sphere").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
