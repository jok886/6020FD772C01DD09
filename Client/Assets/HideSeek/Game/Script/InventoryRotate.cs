using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryRotate : MonoBehaviour
{
    private float _RotationSpeed;
    void Start()
    {
        _RotationSpeed = 10;
    }
    void Update()
    {
        transform.Rotate(Vector3.down * _RotationSpeed, Space.World); //物体自转
    }
}
