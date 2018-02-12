using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAdaptation : MonoBehaviour {
    float devHeight = 1.8f;
    float devWidth = 3.2f;
    void Start()
    {
        //根据相机类型判定分别处理
        if (this.GetComponent<Camera>().orthographic)
        {
            float orthographicSize = this.GetComponent<Camera>().orthographicSize;

            float aspectRatio = Screen.width * 1.0f / Screen.height;
            float cameraWidth = orthographicSize * 2 * aspectRatio;
            
            //宽度适配
            if (cameraWidth < devWidth)
            {
                orthographicSize = devWidth / (2 * aspectRatio);
                //Debug.Log("new orthographicSize = " + orthographicSize);
                this.GetComponent<Camera>().orthographicSize = orthographicSize;
            }
        }
        if(!this.GetComponent<Camera>().orthographic)
        {
            float aspectRatio = Screen.width * 1.0f / Screen.height;
            if(aspectRatio<(devWidth/ devHeight))
            {
                //Debug.Log("宽度适配");
                float viewAspect = 16.0f / 9.0f;
                float newFOV = (viewAspect * 40.0f) / this.GetComponent<Camera>().aspect;
                this.GetComponent<Camera>().fieldOfView = newFOV;
            }
        }
        
        
    }
}
