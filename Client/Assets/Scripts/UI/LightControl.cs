using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameNet;
public class LightControl : MonoBehaviour {
    public GameObject[] ObjectToController;
	// Use this for initialization
	void Start () {
        if (HNGameManager.GameType == HNPrivateScenceBase.GAME_TYPE_13Shui)
        {
            for (int i = 0; i < ObjectToController.Length; i++)
            {
                ObjectToController[i].SetActive(false);
            }
        }
        else if (HNGameManager.GameType == HNPrivateScenceBase.GAME_TYPE_JianDe)
        {
            for (int i = 0; i < ObjectToController.Length; i++)
            {
                ObjectToController[i].SetActive(true);
            }
        }
    }
}
