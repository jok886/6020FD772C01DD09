using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscApp : MonoBehaviour {

	//// Use this for initialization
	//void Start () {
		
	//}
	
	//// Update is called once per frame
	//void Update () {
		
	//}
    public void Esc()
    {
        Loom.QueueOnMainThread(() =>
        {
            PlayerPrefs.DeleteKey("UserName");
            PlayerPrefs.DeleteKey("Uid");
            PlayerPrefs.DeleteKey("Openid");
            PlayerPrefs.DeleteKey("Psd");
            PlayerPrefs.DeleteKey("NickName");
            PlayerPrefs.DeleteKey("Sex");
            PlayerPrefs.DeleteKey("HeadURL");

            PlayerPrefs.DeleteKey("UserName_WX");
            PlayerPrefs.DeleteKey("Uid_WX");
            PlayerPrefs.DeleteKey("Openid_WX");
            PlayerPrefs.DeleteKey("Psd_WX");
            PlayerPrefs.DeleteKey("NickName_WX");
            PlayerPrefs.DeleteKey("Sex_WX");
            PlayerPrefs.DeleteKey("HeadURL_WX");

            SceneManager.LoadScene("GameLand");
        });

        ///Application.Quit();
    }
}
