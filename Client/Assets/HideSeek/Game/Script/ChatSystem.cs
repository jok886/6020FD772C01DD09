using GameNet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.BWF;

public class ChatSystem : MonoBehaviour
{
    private const int ChatDataLenght = 20;
    private HNGameManager hnGameManager;
    private GameObject gamePlayUI_HT;
    public List<GameObject> listChatTextObj = new List<GameObject>();

    private List<string> listChatText = new List<string>();
    private List<string> listChatName = new List<string>();

    private static ChatSystem _instance = null;
    public static ChatSystem GetInstance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<ChatSystem>();
            return _instance;
        }
    }

    private string chat = "";
    void Start()
    {
        if (hnGameManager == null)
            hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
        if (gamePlayUI_HT == null)
            gamePlayUI_HT = GameObject.Find("CanvasGamePaly_demo");
        //if (listChatTextObj == null)
        //{
        //    listChatTextObj = new List<GameObject>();
        //    GameObject ChatTexts = gamePlayUI_HT.transform.Find("Chat/Texts").gameObject;
        //    if (ChatTexts.transform.childCount > 0)
        //    {
        //        for (int i = 0; i < ChatTexts.transform.childCount; i++)
        //        {
        //            GameObject chat = ChatTexts.transform.GetChild(i).gameObject;
        //            listChatTextObj.Add(chat);
        //        }
        //        listChatTextObj.Reverse();
        //    }
        //}

    }

    //void Update()
    //{

    //}

    //获取文本框中的文字
    public void GetChatData(string str)
    {
        chat = "";
        chat = str;
    }
    //发送聊天文字
    public void SendChatData(string str)
    {
        if (BWFManager.isReady && BWFManager.Contains(str))
            str = BWFManager.ReplaceAll(str);
        if (str.Length > ChatDataLenght)
        {
            GameSceneUIHandler.ShowLog("文字长度过长，请重新输入！");
        }
        else if (str.Length == 0)
        {
            GameSceneUIHandler.ShowLog("输入信息不能为空！");
        }
        else
        {
            var temp = Encoding.GetEncoding(936).GetBytes(str);
            //byte[] data = new byte[40];
            //Array.Clear(data, 0, 40);
            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
            if (kernel == null) { return; }
            //Buffer.BlockCopy(temp, 0, data, 0, temp.Length);
            kernel.SendChatData(temp);
        }
        chat = "";
    }
    //显示聊天文字
    public void ShowChatPanel(int id, byte status, string str)
    {
        //PlayerBase playerBase = GameObjectsManager.GetInstance().GetPlayerByChairID(id);
        //IClientUserItem pMeItem = CServerItem.get().GetMeUserItem();
        //if (pMeItem == null)
        //    return;
        //int nStatus = pMeItem.GetUserStatus();

        //if (nStatus == SocketDefines.US_LOOKON)  //旁观者
        //{
        //    if (playerBase.TeamType == PlayerTeam.PlayerTeamType.HideTeam || status == SocketDefines.US_LOOKON)
        //    {
        //        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        //        if (kernel != null)
        //        {
        //            String[] st = kernel.getPlayerByChairID(id).GetNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        //            ShowChatText(st[0], str);
        //        }
        //    }
        //}
        //else  //玩家
        //{
        //    Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        //    if (localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
        //    {
        //        if (playerBase.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam && status != SocketDefines.US_LOOKON)
        //        {
        //            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        //            if (kernel != null)
        //            {
        //                String[] st = kernel.getPlayerByChairID(id).GetNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        //                ShowChatText(st[0], str);
        //            }
        //        }
        //    }
        //    else if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
        //    {
        //        if (playerBase.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
        //        {
        //            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        //            if (kernel != null)
        //            {
        //                String[] st = kernel.getPlayerByChairID(id).GetNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        //                ShowChatText(st[0], str);
        //            }
        //        }
        //    }
        //}
        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        if (kernel != null)
        {
            String[] st = kernel.getPlayerByChairID(id).GetNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            ShowChatText(st[0], str);
        }

    }
    public void ShowChatText(string name, string str)
    {
        if (name.Length >= 4)
            name = name[0] + "..." + name[name.Length - 1];
        if (listChatText.Count == 6)
        {
            listChatName.RemoveAt(5);
            listChatName.Insert(0, name);
            listChatText.RemoveAt(5);
            listChatText.Insert(0, str);
        }
        else
        {
            listChatText.Insert(0, str);
            listChatName.Insert(0, name);
        }
        Loom.QueueOnMainThread(() =>
        {
            for (int i = 0; i < listChatName.Count; i++)
            {
                if (listChatTextObj != null && listChatTextObj[i] != null && listChatName[i] != null && listChatText[i] != null)
                {
                    if (!listChatTextObj[0].gameObject.transform.parent.gameObject.activeSelf)
                        listChatTextObj[0].gameObject.transform.parent.gameObject.SetActive(true);
                    Text nam = listChatTextObj[i].GetComponent<Text>();
                    Text lan = listChatTextObj[i].transform.Find("Text").GetComponent<Text>();
                    nam.text = listChatName[i];
                    lan.text = ": " + listChatText[i];
                }
            }
        });
    }
    //清空文字聊天框
    public void InputFieldClear()
    {
        gamePlayUI_HT.transform.Find("Chat/ChatInputField").GetComponent<InputField>().text = "";
    }
    public void TextClear()
    {
        listChatText.Clear();
        listChatName.Clear();
        for (int i=0;i< listChatTextObj.Count;i++)
        {
            Text nam = listChatTextObj[i].GetComponent<Text>();
            Text lan = listChatTextObj[i].transform.Find("Text").GetComponent<Text>();
            nam.text = "";
            lan.text = "";
        }
    }
}
