using System.Collections;
using System.Collections.Generic;
using GameNet;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour,IEventSystemHandler {
    public Toggle agree;
    public GameObject tip;
    //public GameObject NowWindow;
    //private Animation anim;
    
   
    private void Start()
    {
        //  anim = NowWindow.GetComponent<Animation>();
        
    }

    public void LandToHall() {
        if (agree.isOn)
        {
            
            //SceneManager.LoadScene("GameHall");
        }
        else
        {
            tip.SetActive(true);
        }
    }
    
    public void HallToLand() {
        SceneManager.LoadScene("GameLand");
    }

    public void HallToPlay()
    {
        
        //SceneManager.LoadScene("GamePlay");
    }

    public void PlayToHall()
    {
        SceneManager.LoadScene("GameHall");
    }

    public void Quit()
    {
      //  anim.Play();
        StartCoroutine(RealQuit());
    }

    IEnumerator RealQuit()
    {
        yield return new WaitForSeconds(0.8f);
        Application.Quit();
    }
}
