using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlManager : MonoBehaviour
{
    private float MaxMoveSpeed;
    private float deltaTime;
    private Vector3 deltaPos;
    private Vector3 oldPos;
    public static Vector3 moveDirection;
    public static Vector3 rotaDirection;
    void Start()
    {
        MaxMoveSpeed = 8f;
        deltaTime = 0.03f;
        moveDirection = new Vector3(0, 0, 0);
        rotaDirection = new Vector3(0, 0, 0);
    }

    void Update()
    {
        if (moveDirection.magnitude > 0f)
        {
            if (transform.position.magnitude <= 130)  //限制相机移动范围，暂定为130
            {
                oldPos = this.transform.position;
                deltaPos = moveDirection * MaxMoveSpeed * deltaTime;
                this.transform.position += deltaPos;
            }
            else
                this.transform.position = oldPos;
        }
        //if (rotaDirection.magnitude > 0f)
        //{
        //    this.transform.Rotate(Vector3.up * rotaDirection.x * 8, Space.Self);
        //}
    }
}
