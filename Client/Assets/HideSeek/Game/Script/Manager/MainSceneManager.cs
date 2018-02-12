using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using GameNet;

public class MainSceneManager : MonoBehaviour {

    private GameObject _connectingLabel;
    private Text _connectingLabelText;
    //private GameObject _joinGameButton;

    private int _connectingTimeCount;

    // Use this for initialization
    void Start()
    {
        _connectingLabel = GameObject.Find("Connecting");
        if(_connectingLabel)
        {
            _connectingLabelText = _connectingLabel.GetComponentInChildren<Text>();
        }

        //_joinGameButton = GameObject.Find("JoinGameButton");
        //_joinGameButton.active = false;

        GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
        tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
        //MersenneTwister.MT19937.Seed((ulong)pGlobalUserData.wRandseed);

        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {
            //GameObjectsManager.GetInstance().LoadMap(pGlobalUserData.cbMapIndexRand);
            GameObjectsManager.GetInstance().LoadMap(pGlobalUserData.cbMapIndexRandForSingleGame);
        }
        else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
        {
            GameObjectsManager.GetInstance().LoadMap(pGlobalUserData.cbMapIndexRand);
        }
        else
        {
            // GameSingleMultiType.MultiGame_Skynet

            StartConnecting();
        }
    }
	
    private void StartConnecting()
    {
        _connectingTimeCount = 0;

        if (GameManager.s_NetWorkClient == null)
        {
            GameManager.s_NetWorkClient = new NetWorkClient();
            if(GameManager.LockObj==null)
            {
                GameManager.LockObj = new object();
            }

            GameManager.s_NetWorkClient.UserName = String.Empty; //PlayerPrefs.GetString("username");//String.Empty; //
                                                                 //GameManager.s_NetWorkClient.UserName = String.Empty;
            GameManager.s_NetWorkClient.PassWord = String.Empty; //PlayerPrefs.GetString("password");//String.Empty; //
            GameManager.s_NetWorkClient.DisplayName = PlayerPrefs.GetString("displayname");
            ///GameManager.s_NetWorkClient.StartConnect();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {

        }
        else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
        {

        }
        else
        {
            // GameSingleMultiType.MultiGame_Skynet

            // Set Connecting text
            if (_connectingLabel)
            {
                _connectingTimeCount++;

                if (_connectingLabelText)
                {
                    if (_connectingTimeCount % 80 < 20)
                    {
                        _connectingLabelText.text = "Server Connecting";
                    }
                    else if (_connectingTimeCount % 80 < 40)
                    {
                        _connectingLabelText.text = "Server Connecting.";
                    }
                    else if (_connectingTimeCount % 80 < 60)
                    {
                        _connectingLabelText.text = "Server Connecting..";
                    }
                    else
                    {
                        _connectingLabelText.text = "Server Connecting...";
                    }
                }
            }

            if (GameManager.s_NetWorkClient.MultiGameState == NetWorkClient.GameState.Game_NullState && GameManager.s_NetWorkClient.TheUserLoginState == NetWorkClient.UserLoginState.LoginSuccess)
            {
                GameManager.s_NetWorkClient.FindRoom();
            }

            if(GameManager.s_NetWorkClient.TheUserMatchState == NetWorkClient.UserMatchState.JoinedRoom)
            {
                //if (_connectingTimeCount > 100)
                {
                    if (_connectingLabel.activeSelf)
                    {
                        _connectingLabel.SetActive(false);
                    }
                }
            }

        }
    }
}
