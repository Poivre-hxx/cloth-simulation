using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ball_move : MonoBehaviour
{
    bool pressed = false;
    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            pressed = true;
            offset = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        } else if (Input.GetMouseButtonUp(0)) {
            pressed = false;
        }

        if (pressed) {
            Vector3 mouse = Input.mousePosition;
            mouse -= offset;
            transform.position = Camera.main.ScreenToWorldPoint(mouse);
        }
        
    }
}
