using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Logos : MonoBehaviour
{

    public float logoShowTime = 1.0f;
    public Image LogoImage = null;
    [Range(0.001f, 0.1f)]
    public float transitionSpeed = 0.01f;


    // Update is called once per frame
    void Update()
    {
        if (LogoImage == null)
            SceneManager.LoadScene("GameLand");
        else if (Time.time >= (logoShowTime))
        {
            LogoImage.color = new Vector4(0.0f, 0.0f, 0.0f, LogoImage.color.a + transitionSpeed);

            //Debug.Log(LogoImage.color);
            if (LogoImage.color.a >= 1.0f)
            {
                //Debug.Log("1");
                SceneManager.LoadScene("GameLand");
            }
        }
    }
}
