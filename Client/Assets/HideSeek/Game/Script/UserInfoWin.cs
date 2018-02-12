using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameNet;
using System;
using Crosstales.BWF;
using System.Text;

public class UserInfoWin : MonoBehaviour
{
    private static UserInfoWin _instance = null;
    public static UserInfoWin GetInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<UserInfoWin>();
            }
            return _instance;
        }
    }

    private GameObject Canvas;
    private string strName = null;
    private int headIndex = 0;  //使用自定义头像
    void Start()
    {
        if (Canvas == null)
            Canvas = GameObject.Find("Canvas");
        UpdateInfo();
    }

    //void Update()
    //{

    //}
    public void OnClickUserHead()
    {
        UpdateInfo();
    }
    public void GetUserNickName(string str)
    {
        if (str != null && str != "")
        {
            if (str.Length <= 10)
                strName = str;
            else
                GameSceneUIHandler.ShowLog("昵称过长，请使用较短昵称");
        }
    }
    public void GetRandomUserNickName()
    {
        string name = CreateNickName.GetInstance.RandomName();
        Canvas.transform.Find("Window/UserEditorWindow/UserInfo/UserNameField").GetComponent<InputField>().text = name;
        strName = name;
    }
    public void GetUserHeadIdex(int index)
    {
        if (index == 0)
        {
            headIndex = (headIndex + 1) % 5;
        }
        else if (index == 1)
        {
            headIndex = (headIndex - 1) >= 0 ? (headIndex - 1) : (headIndex - 1) + 5;
        }
        Canvas.transform.Find("Window/UserEditorWindow/UserInfo/UserImage/UserImage").GetComponent<Image>().overrideSprite = GetHeadImage(headIndex);
    }
    public void SetUserHeadIndex(int index)
    {
        headIndex = index;
        Canvas.transform.Find("Window/UserEditorWindow/UserInfo/UserImage/UserImage").GetComponent<Image>().overrideSprite = GetHeadImage(headIndex);
    }
    public void SetUserInfo()
    {
        GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
        tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

        CMD_GP_ModUserInfo pData = new CMD_GP_ModUserInfo();
        pData.Init();

        if (strName == null)
        {
            Buffer.BlockCopy(pGlobalUserData.szNickName, 0, pData.szNickName, 0, pGlobalUserData.szNickName.Length);
        }
        else
        {
            byte[] szNickName = Encoding.GetEncoding(936).GetBytes(strName);
            Buffer.BlockCopy(szNickName, 0, pData.szNickName, 0, szNickName.Length);
        }
        if (headIndex == -1)  //预留自定义头像,使用自定义头像时headIndex需置为-1
        {
            Buffer.BlockCopy(pGlobalUserData.szHeadHttp, 0, pData.szHeadHttp, 0, pGlobalUserData.szHeadHttp.Length);
        }
        else
        {
            byte[] szHeadIndex = Encoding.GetEncoding(936).GetBytes(headIndex.ToString());
            Buffer.BlockCopy(szHeadIndex, 0, pData.szHeadHttp, 0, szHeadIndex.Length);
        }

        if (!BWFManager.Contains(strName))
        {
            bool isNameEquals = true;
            bool isHeadEquals = true;
            string name1 = Encoding.Default.GetString(pData.szNickName);
            string name2 = Encoding.Default.GetString(pGlobalUserData.szNickName);
            string head1 = Encoding.Default.GetString(pData.szHeadHttp);
            string head2 = Encoding.Default.GetString(pGlobalUserData.szHeadHttp);
            if (name1.Length != name2.Length)
                isNameEquals = false;
            else
            {
                for (int i = 0; i < name1.Length; i++)
                {
                    if (name1[i] == name2[i])
                        continue;
                    else
                    {
                        isNameEquals = false;
                        break;
                    }
                }
            }
            if (head1.Length != head2.Length)
                isHeadEquals = false;
            else
            {
                for (int i = 0; i < head1.Length; i++)
                {
                    if (head1[i] == head2[i])
                        continue;
                    else
                    {
                        isHeadEquals = false;
                        break;
                    }
                }
            }
            if (!isNameEquals || !isHeadEquals)
            {
                UserInfo.getInstance().modeUserInfo(pData);
                //修改玩资料后，临时变量清空
                strName = null;
                Canvas.transform.Find("Window/UserEditorWindow/UserInfo/UserNameField").GetComponent<InputField>().text = "";
            }
            else if (isNameEquals)
                GameSceneUIHandler.ShowLog("您当前正在使用该昵称！");
        }
        else
            GameSceneUIHandler.ShowLog("该昵称包含敏感词汇，无法使用");
    }
    public Sprite GetHeadImage(int index)
    {
        string path = "UI/UserImage/User_0" + index;
        return Resources.Load(path, typeof(Sprite)) as Sprite;
    }
    public void UpdateInfo()
    {
        GameObject userInfoWindow = Canvas.transform.Find("Window/UserWindow").gameObject;
        userInfoWindow.transform.Find("UserInfo/User/UserText").GetComponent<Text>().text = GlobalUserInfo.getNickName();
        userInfoWindow.transform.Find("UserInfo/ID/IDText").GetComponent<Text>().text = GlobalUserInfo.getUserID().ToString();
        userInfoWindow.transform.Find("UserInfo/LV/LVText").GetComponent<Text>().text = (GlobalUserInfo.getUserExp() / 100).ToString();
        userInfoWindow.transform.Find("UserInfo/GameNum/GameNumText").GetComponent<Text>().text = "0";
        userInfoWindow.transform.Find("UserImage/UserMask/UserImage").GetComponent<Image>().overrideSprite = GetHeadImage(int.Parse(GlobalUserInfo.GBToUtf8(GlobalUserInfo.getHeadHttp())));
        Canvas.transform.Find("Window/UserEditorWindow/UserInfo/UserImage/UserImage").GetComponent<Image>().overrideSprite = GetHeadImage(int.Parse(GlobalUserInfo.GBToUtf8(GlobalUserInfo.getHeadHttp())));
        headIndex = int.Parse(GlobalUserInfo.GBToUtf8(GlobalUserInfo.getHeadHttp()));
    }
    public void ShowTipsWindow()
    {
        GameObject TipsWin = gameObject.transform.Find("TipsWindow").gameObject;
        Text text = TipsWin.transform.Find("Text").GetComponent<Text>();
        if (strName != null)
            text.text = "信息修改将扣除1钻石！";
        else
            text.text = "确认修改信息？";
    }
}
