using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageEffect : MonoBehaviour {
    public Shader colorEffectShader;
    //public Color mainCol = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    //[Range(0.0f,10.0f)]
    //public float brightness;
    //[Range(0.0f, 10.0f)]
    //public float saturation;
    //[Range(0.0f, 10.0f)]
    //public float contrast;
    private Material effectMa;
    Material cusMaterial
    {
        get
        {
            if(effectMa == null) effectMa = new Material(colorEffectShader);
            return effectMa;
        }
    }
	// Use this for initialization
	void Start () {
		if(!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }
        if (!colorEffectShader && !colorEffectShader.isSupported) enabled = false;
	}

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Input.GetKey(KeyCode.A))
        {
            Destroy(GetComponent("ImageEffect"));
        }

        if (colorEffectShader)
        {
            //cusMaterial.SetColor("_MainColor", mainCol);
            //cusMaterial.SetFloat("_Brightness", brightness);
            //cusMaterial.SetFloat("_Saturation", saturation);
            //cusMaterial.SetFloat("_Contrast", contrast);
            Graphics.Blit(source, destination, cusMaterial);
        }

        else
        {
            Graphics.Blit(source, destination);
        }
    }

    void OnDisable()
    {
        if(cusMaterial) DestroyImmediate(cusMaterial);
    }

}
