using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenAdaptation : MonoBehaviour {
    public Camera[] cameras;

    public float width = 854f;

    public float height = 480f;

    // Use this for initialization

    void Start()
    {

        foreach (Camera c in cameras)

        {

            if (c != null)

            {

                c.aspect = width / height;

            }

        }

    }
}
