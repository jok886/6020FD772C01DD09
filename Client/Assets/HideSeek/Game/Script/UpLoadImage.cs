using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpLoadImage : MonoBehaviour
{
    public GameObject texture;

    void Start()
    {
        StartCoroutine(LoadTexture("small.jpg"));
    }
    //打开相册    
    public void OpenGallery()
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        string nameObj = gameObject.name;
        jo.Call("OpenGallery", nameObj);
    }

    //打开相机
    public void OpenCamera()
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        string nameObj = gameObject.name;
        jo.Call("TakePhoto", nameObj);
    }
    public void messgae(string str)
    {
        Debug.Log("--------------- messgae ---------------------");
#if UNITY_ANDROID
        //在Android插件中通知Unity开始去指定路径中找图片资源
        StartCoroutine(LoadTexture(str));
#endif

    }

    IEnumerator LoadTexture(string name)
    {
        Debug.Log("-------------------LoadTexture Start-----------------------");
        //注解1
        string path = "file://" + Application.persistentDataPath + "/" + name;
        path = "file://" + Application.dataPath + "/" + name;
        WWW www = new WWW(path);
        while (!www.isDone) { }
        yield return www;
        Debug.Log("-------------------LoadTexture End-----------------------");
        int len = www.bytes.Length;
        Debug.Log("----------------- " + len);
        //为贴图赋值
        texture.GetComponent<Image>().sprite = Sprite.Create(www.texture, new Rect(0, 0, 200, 200), new Vector2(0.5f, 0.5f));
    }
}
