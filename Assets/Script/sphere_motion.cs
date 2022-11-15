using UnityEngine;
using System.Collections;

public class sphere_motion : MonoBehaviour
{
    bool pressed = false;
    bool sphere_move = false;
    Vector3 offset;

    // Use this for initialization
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pressed = true;

            // .ScreenPointToRay() 返回摄像机通过屏幕点的光线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // .Cross 两个向量的叉乘
            // transform.position - ray.origin 返回射线的起点到物体的向量
            // .magnitude 返回向量的长度
            if (Vector3.Cross(ray.direction, transform.position - ray.origin).magnitude < 2.5f)
                sphere_move = true;
            else
                sphere_move = false;
            offset = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        }
        if (Input.GetMouseButtonUp(0))
            pressed = false;

        if (pressed)
        {
            if (sphere_move)
            {
                Vector3 mouse = Input.mousePosition;
                // 鼠标位置 - 偏移量Offset = 移动的距离
                mouse -= offset;

                // mouse.z = Camera.main.WorldToScreenPoint(transform.position).z;
                transform.position = Camera.main.ScreenToWorldPoint(mouse);
            }
            else
            // 当鼠标没有聚焦在球上时，照相机绕y轴旋转
            {
                float h = 2.0f * Input.GetAxis("Mouse X");
                Camera.main.transform.RotateAround(new Vector3(0, 0, 0), Vector3.up, h);
            }
        }
    }
}
