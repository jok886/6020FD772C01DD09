using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using GameNet;
using System;
using System.Net;
using System.IO;
using Crosstales.BWF;

public class LogIn : MonoBehaviour
{
    public static HNGameManager hnManager;
    public Toggle AgreeToggle;
    public GameObject AgreeTips;
    public static GameObject CustomNameWindow;

    private EventHandle eventHandle;

    private bool m_bIsIpv6;

    // Use this for initialization
    void Start()
    {
        //Check ipv6
        m_bIsIpv6 = false;
        if (gameObject.name == "LandButton")
        {
            AddressFamily family = AddressFamily.InterNetwork;
            //ipv4转ipv6 - 苹果ios
#if !UNITY_EDITOR && UNITY_IPHONE
            IPAddress[] address = Dns.GetHostAddresses("www.baidu.com");
            if (address[0].AddressFamily == AddressFamily.InterNetworkV6)
            {
                family = AddressFamily.InterNetworkV6;
                Debug.Log("Is InterNetworkV6");
            }
            else
            {
                family = AddressFamily.InterNetwork;
                Debug.Log("Is InterNetwork");
            }
#endif
            if (family == AddressFamily.InterNetworkV6)
            {
                m_bIsIpv6 = true;

                Image image = gameObject.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = Resources.Load("LandButtons/Button_GameIn", typeof(Sprite)) as Sprite;
                }
            }
        }

        var dddb = Loom.Current;
        hnManager = GameObject.FindObjectOfType<HNGameManager>();
        eventHandle = GameObject.FindObjectOfType<EventHandle>();
        //CustomNameWindow= GameObject.Find("Canvas/CustomNameWindow").gameObject;
        CustomNameWindow = gameObject.transform.parent.transform.Find("CustomNameWindow").gameObject;
    }

    public void LogOn()
    {
        if (/*AgreeToggle.isOn*/true)
        {
            if (gameObject.name == "Button_YouKe" || m_bIsIpv6)
            {
                HNGameManager.bWeChatLogonIn = false;
                hnManager.LogOn();
            }
            else
            {
                HNGameManager.bWeChatLogonIn = true;
                eventHandle.onLogin();
            }

        }
        else
        {
            AgreeTips.SetActive(true);
        }
    }
    public static void ShowNickNameForRegisterWin(bool visible)
    {
        CustomNameWindow.SetActive(visible);
        CustomNameWindow.transform.Find("UserName").GetComponent<InputField>().text = LoginScene.m_kNickName;
    }
    public void SetNickNameForRegister(string str)
    {
        if (!BWFManager.Contains(str))
        {
            CustomNameWindow.transform.Find("UserName").GetComponent<InputField>().text = str;
            LoginScene.m_kNickName = str;
        }
        else
        {
            GameSceneUIHandler.ShowLog("该昵称包含敏感词汇，无法使用");
            CustomNameWindow.transform.Find("UserName").GetComponent<InputField>().text = LoginScene.m_kNickName;
        }
    }
    public void SetRandomNickNameForRegister()
    {
        string str = CreateNickName.GetInstance.RandomName();
        CustomNameWindow.transform.Find("UserName").GetComponent<InputField>().text = str;
        LoginScene.m_kNickName = str;
    }
    public void RegisterLogin()
    {
        if (hnManager.login != null)
            hnManager.login.RegisterAccount();
    }
}
