using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameExitToLand : MonoBehaviour {

    private EventHandle eventHandle;
    // Use this for initialization
    void Start()
    {
        eventHandle = GameObject.FindObjectOfType<EventHandle>();
    }

    // Update is called once per frame
    //void Update()
    //{

    //}
    public void Exit()
    {
        //Application.Quit();
        eventHandle.onLogout();
    }
}
