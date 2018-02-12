using UnityEngine;  
using System.Collections;  
  
public class CameraFieldofView : MonoBehaviour
{
    private float rate1 = 9.0f/16.0f;
    private float rate2;
    
    void Start()
    {
        rate2 = (float)Screen.height / (float)Screen.width;
        gameObject.GetComponent<Camera>().fieldOfView *= 1 + (rate2 - rate1)*2;
    }
}