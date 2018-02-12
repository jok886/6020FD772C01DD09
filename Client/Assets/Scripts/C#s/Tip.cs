using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class Tip : MonoBehaviour {
    public InputField input;
    public Text BindedInviteNum;
    public Text tip;
    public GameObject BindButton;

    float timer = 0;
    float now = 0;

    void Start()
    {
        if(tip != null)
        {
            tip.text = "";
        }

        CheckShowOrHideInput();
    }

    public void HandleBindResult(bool bIsSuccess, string strDescribe)
    {
        tip.text = strDescribe;

        CheckShowOrHideInput();
    }

    private void CheckShowOrHideInput()
    {
        GameNet.GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
        GameNet.tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
        if (pGlobalUserData.dwSpreaderID != 0)
        {
            //已经绑定代理人

            if (input != null)
            {
                input.transform.parent.gameObject.SetActive(false);
            }

            if (BindButton != null)
            {
                BindButton.SetActive(false);
            }

            if (BindedInviteNum != null)
            {
                BindedInviteNum.text = Surrogate.IdToInviteCode(pGlobalUserData.dwSpreaderID);
                BindedInviteNum.transform.parent.gameObject.SetActive(true);
            }
        }
        else
        {
            if (input != null)
            {
                input.transform.parent.gameObject.SetActive(true);
            }

            if (BindButton != null)
            {
                BindButton.SetActive(true);
            }

            if (BindedInviteNum != null)
            {
                BindedInviteNum.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (tip.text != "") {    
            timer+=Time.deltaTime;
            if (timer-now >= 3)
            {
                tip.text = "";
                now = timer;
            }
        }      
    }

    public void InviteHint() {
        if (input.text == "")
        {
            tip.text = "邀请码不能为空！";
            ///tip.color = Color.red;

            Debug.Log("Tip:InviteHint:邀请码不能为空");

        }
        else if (input.text.Length != 5)
        {
            tip.text = "邀请码必须是5位！";
            ///tip.color = Color.red;

            Debug.Log("Tip:InviteHint:邀请码必须是5位，input.text=" + input.text);
        }
        else
        {
            ///uint dwSpreaderID = uint.Parse(input.text);
            uint dwSpreaderID = Surrogate.InviteCodeToId(input.text);
            GameNet.UserInfo.getInstance().modifySpreader(dwSpreaderID);
        }
    }

    public void CheckHint() {
        if (input.text=="")
        {
            tip.text = "请输入回放码！";
            ///tip.color = Color.red;
        }
        else if (input.text != "123456")
        {
            tip.text = "请输入正确的回放码！";
            ///tip.color = Color.red;
        }
    }

}
