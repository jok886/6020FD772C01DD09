using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GameNet;
using UnityEngine;
using UnityEngine.UI;

public class TransferDiamond : MonoBehaviour
{

    public Text UserID;
    public Text DiamondCount;
    public GameObject TipsWindows;
    public GameObject ConfirmWindows;
    private bool bCloseGiveWindow = false;

    void OnEnable()
    {
        bCloseGiveWindow = false;
        TipsWindows.SetActive(false);
        ConfirmWindows.SetActive(false);
        UserID.text = "";
        DiamondCount.text = "";
    }

    public void QueryNickName()
    {
        uint userId;
        uint diamondCount;
        bool bSuccess = uint.TryParse(UserID.text, out userId);
        if (bSuccess == false)
        {
            ShowErrorTips("用户ID号输入有误，请重新输入！");
            return;
        }
        else if(userId == GlobalUserInfo.getUserID())
        {
            ShowErrorTips("不能向自己转移房卡，请重新输入！");
            return;
        }
        bSuccess = uint.TryParse(DiamondCount.text, out diamondCount);
        if (bSuccess)
        {
            if (diamondCount <= GlobalUserInfo.getUserInsure())
            {
                GameNet.UserInfo.getInstance().queryNickName(userId, this);
            }
            else
            {
                ShowErrorTips("转钻石数大于拥有钻石数，请重新输入！");
                return;
            }
        }
        else
        {
            ShowErrorTips("转钻石数输入有误，请重新输入！");
            return;
        }
    }

    public void ShowNickNameInfo(string nickName)
    {
        ConfirmWindows.SetActive(true);
        ConfirmWindows.transform.Find("Tips_Name/Name").GetComponent<Text>().text = nickName;
        ConfirmWindows.transform.Find("Tips_ID/Name").GetComponent<Text>().text = UserID.text;
        ConfirmWindows.transform.Find("Tips_Diamond/DiamondCount").GetComponent<Text>().text = DiamondCount.text;
    }

    public void ShowErrorTips(string errorStr, bool bCloseAllWindow= false)
    {
        TipsWindows.SetActive(true);
        TipsWindows.transform.Find("Tips").GetComponent<Text>().text = errorStr;
        bCloseGiveWindow = bCloseAllWindow;
    }

    public void TransferDiamonds()
    {
        GameNet.UserInfo.getInstance().TransferDiamonds(uint.Parse(DiamondCount.text));
    }

    public void CloseTipsWindow()
    {
        if (bCloseGiveWindow)
        {
            gameObject.SetActive(false);
        }
        else
        {
            TipsWindows.SetActive(false);
        }
    }
}
