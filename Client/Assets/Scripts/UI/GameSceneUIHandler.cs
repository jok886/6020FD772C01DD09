using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameNet;
using System;

public class GameSceneUIHandler : MonoBehaviour
{
    private HNGameManager hnManager;
    public GameObject DismissWindow;
    //Menu in GamePlayScene
    public GameObject MenusBG;// Menu Extend
    public GameObject DownMenus;
    public Sprite CloseMenusImage;// Menu Close State
    public Sprite ShowMenusImage;// Menu Show(Open) State

    public GameObject DisbandObj;
    public GameObject LeaveRoomObj;

    //system log
    static public Text m_serverLogText;
    static float m_fLogTimer = 0;

    // Use this for initialization
    void Start ()
	{
	    hnManager = FindObjectOfType<HNGameManager>();

        //Log框
        GameObject serverLogObj = GameObject.Find("ServerLog");
        if(serverLogObj != null)
        {
            serverLogObj.SetActive(true);
            DontDestroyOnLoad(serverLogObj.transform.parent.gameObject);

            m_serverLogText = serverLogObj.transform.Find("ServerLogText").GetComponent<Text>();
            if (m_serverLogText != null)
            {
                m_serverLogText.text = "";
            }
        }
    }

    private void UpdateLog()
    {
        if (m_serverLogText != null)
        {
            if(m_serverLogText.text != "")
            {
                m_fLogTimer += Time.deltaTime;
                m_serverLogText.transform.parent.gameObject.SetActive(true);
                if (m_fLogTimer > 3f)
                {
                    m_serverLogText.text = "";
                    m_fLogTimer = 0f;
                }
            }
            else
            {
                m_serverLogText.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    static public void ShowLog(string strLogText)
    {
        if (m_serverLogText != null)
        {
            String[] str = strLogText.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
            Loom.QueueOnMainThread(() =>
            {
                if (str.Length != 1)
                {
                    if (UIManager.TimeTip != null)
                    {
                        UIManager.TimeTip.GetComponent<Text>().text = str[1];
                        UIManager.TimeLeft = Int32.Parse(str[1]);
                    }
                    //玩家积分计算
                    if (UIManager.GetInstance() != null)
                        UIManager.GetInstance().HiderGameScore(UIManager.TimeLeft);
                }
                else
                {
                    m_serverLogText.text = strLogText;
                    m_fLogTimer = 0f;
                }

                //LogText.gameObject.SetActive(true);
            });
        }
    }

    private void Update()
    {
        UpdateLog();
    }

    public void DismissBtnClicked()
    {
        hnManager.DismissBtnClicked();
    }

    public void LeaveRoomBtnClicked()
    {
        hnManager.LeaveRoom();
    }

    public void DismissPrivate()
    {
        hnManager.DismissPrivate();
    }

    public void DismissPrivateNot()
    {
        hnManager.DismissPrivateNot();
    }

    public void ShowDismissWindow()
    {
        DismissWindow.SetActive(true);
    }

    public void ShowOrCloseMenus()
    {
        MenusBG.SetActive(!MenusBG.activeSelf);
        if(MenusBG.activeSelf)
        {
            DownMenus.GetComponent<Image>().sprite = ShowMenusImage;
        }else
        {
            DownMenus.GetComponent<Image>().sprite = CloseMenusImage;
        }
    }

    public void ShowMenus()
    {
        MenusBG.SetActive(true);
        DownMenus.GetComponent<Image>().sprite = ShowMenusImage;

        //设置解散和退出按钮的显示
        if (PlayerPrefs.GetInt("PubOrPrivate") == (int)RoomType.Type_Private)
        {
            //房卡模式

            if (hnManager.GetRoomPlayCout() == 0 && HNGameManager.m_iLocalChairID != 0)
            {
                //游戏未开始,并且不是房主，显示退出按钮

                if (DisbandObj != null)
                {
                    DisbandObj.SetActive(false);
                }
                if (LeaveRoomObj != null)
                {
                    LeaveRoomObj.SetActive(true);
                }
                return;
            }
        }
        if (DisbandObj != null)
        {
            DisbandObj.SetActive(true);
        }
        //if (LeaveRoomObj != null)
        //{
        //    LeaveRoomObj.SetActive(false);
        //}
    }


    //if click the button in menu ,make menu script change close
    public void CloseMenus()
    {
        MenusBG.SetActive(false);
        DownMenus.GetComponent<Image>().sprite = CloseMenusImage;
    }
}
