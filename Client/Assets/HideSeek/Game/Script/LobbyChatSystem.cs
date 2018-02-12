using GameNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.BWF;

public class LobbyChatSystem : MonoBehaviour
{
    private List<GameObject> listChatTextObj;
    public GameObject m_chatInputField;
    public GameObject m_texts;

    private List<string> listChatText = new List<string>();
    private List<string> listChatName = new List<string>();

    //private string m_chatText = "";

    private const int LobbyChatDataLenght = 20;

    private float m_fDeltaTime = 5.0f;

    void Start()
    {
        listChatTextObj = new List<GameObject>();

        m_fDeltaTime = 5.0f;

        //var b = System.Text.Encoding.Default.GetBytes(LoginScene.m_strServerIP);
        //m_kGPLobbyChatMission = new CGPLobbyChatMission(b, LoginScene.m_nLobbyServerPort);
    }

    void Update()
    {
        m_fDeltaTime += Time.deltaTime;
        if(m_fDeltaTime > 5.0f)
        {
            m_fDeltaTime = 0f;

            CGPLobbyMission kGPLobbyMission = CGPLobbyMission.GetInstance();
            if (kGPLobbyMission != null)
            {
                if (!kGPLobbyMission.isAlive())
                {
                    kGPLobbyMission.SendChatData("StartChatConnect!");
                }

                if (!kGPLobbyMission.isLobbyChatSystemSetted())
                {
                    kGPLobbyMission.setLobbyChatSystem(this);
                }
            }
        }
    }

    public void OnChatButtonClick()
    {
        if(m_chatInputField != null)
        {
            m_chatInputField.SetActive(!m_chatInputField.activeSelf);
        }

        if (m_texts != null)
        {
            m_texts.SetActive(!m_texts.activeSelf);
        }

        //GameObject ChatInputField = transform.FindChild("ChatInputField").gameObject;
        //ChatInputField.SetActive(!ChatInputField.activeSelf);
        //GameObject Texts = transform.FindChild("Texts").gameObject;
        //Texts.SetActive(!Texts.activeSelf);
    }

    ////获取文本框中的文字
    //public void GetChatData(string str)
    //{
    //    m_chatText = "";
    //    m_chatText = str;
    //}

    //发送聊天文字
    public void SendChatData(string chatText)
    {
        if (BWFManager.isReady && BWFManager.Contains(chatText))
        {
            chatText = BWFManager.ReplaceAll(chatText);
        }
        if (chatText.Length > LobbyChatDataLenght)
        {
            GameSceneUIHandler.ShowLog("文字长度过长，请重新输入！");
        }
        else if (chatText.Length == 0)
        {
            GameSceneUIHandler.ShowLog("输入信息不能为空！");
        }
        else
        {
            //var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
            //if (kernel == null) { return; }
            //Buffer.BlockCopy(temp, 0, data, 0, temp.Length);
            //kernel.SendChatData(data);

            CGPLobbyMission kGPLobbyMission = CGPLobbyMission.GetInstance();
            if (kGPLobbyMission != null)
            {
                if (!kGPLobbyMission.isLobbyChatSystemSetted())
                {
                    kGPLobbyMission.setLobbyChatSystem(this);
                }

                kGPLobbyMission.SendChatData(chatText);
            }
        }
        //m_chatText = "";
    }
    //显示聊天文字
    public void ShowChatPanel(int headindex, string szNickName, string szChatData)
    {
        ShowChatText(headindex, szNickName, szChatData);
    }
    private void ShowChatText(int headindex, string name, string str)
    {
        if (str.StartsWith("StartChatConnect!"))
            return;
        Loom.QueueOnMainThread(()=>
        {
            GameObject loadObj = null;
            GameObject Content = GameObject.Find("Canvas/Chat/Texts/Scroll View/Viewport/Content");
            loadObj = Instantiate(Content.transform.Find("User0").gameObject);
            float curAnchoredPosY = loadObj.GetComponent<RectTransform>().anchoredPosition.y;
            loadObj.transform.SetParent(Content.transform, false);
            loadObj.transform.Find("UserImage").GetComponent<Image>().overrideSprite = UserInfoWin.GetInstance.GetHeadImage(headindex);
            loadObj.transform.Find("UserName").GetComponent<Text>().text = name;
            loadObj.transform.Find("TextBack/Text").GetComponent<Text>().text = str;
            loadObj.SetActive(true);
            if (listChatTextObj.Count <= 30)
            {
                listChatTextObj.Add(loadObj);
            }
            else
            {
                listChatTextObj.RemoveAt(0);
                listChatTextObj.Add(loadObj);
            }
            listChatTextObj.Reverse();
            for (int i = 0; i < listChatTextObj.Count; i++)
            {
                RectTransform Rect = listChatTextObj[i].transform.GetComponent<RectTransform>();
                //Set Pos
                Vector3 anchoredPos = Rect.anchoredPosition;
                anchoredPos.y = curAnchoredPosY;
                Rect.anchoredPosition = anchoredPos;
                curAnchoredPosY += 80;
            }
            //if (listChatTextObj.Count >= 3)
            {
                RectTransform Rect = Content.transform.GetComponent<RectTransform>();
                Rect.sizeDelta = new Vector3(Rect.sizeDelta.x, 80 * listChatTextObj.Count, 0);
            }
        });



        //if (listChatText.Count == 6)
        //{
        //    listChatName.RemoveAt(5);
        //    listChatName.Insert(0, name);
        //    listChatText.RemoveAt(5);
        //    listChatText.Insert(0, str);
        //}
        //else
        //{
        //    listChatText.Insert(0, str);
        //    listChatName.Insert(0, name);
        //}
        //Loom.QueueOnMainThread(() =>
        //{
        //    for (int i = 0; i < listChatName.Count; i++)
        //    {
        //        if (listChatTextObj != null && listChatTextObj[i] != null && listChatName[i] != null && listChatText[i] != null)
        //        {
        //            if (!listChatTextObj[0].gameObject.transform.parent.gameObject.activeSelf)
        //                listChatTextObj[0].gameObject.transform.parent.gameObject.SetActive(true);
        //            Text nam = listChatTextObj[i].GetComponent<Text>();
        //            Text lan = listChatTextObj[i].transform.Find("Text").GetComponent<Text>();
        //            nam.text = listChatName[i];
        //            lan.text = ": " + listChatText[i];
        //        }
        //    }
        //});
    }
    //清空文字聊天框
    public void InputFieldClear()
    {
        transform.FindChild("ChatInputField").GetComponent<InputField>().text = "";
    }

    public void onSubGPLobbyChat(CMD_GP_CHAT pNetInfo)
    {
        uint dwUserID = pNetInfo.dwUserID;

        string szNickName = GlobalUserInfo.GBToUtf8(pNetInfo.szNickName);
        string szChatData = Encoding.UTF8.GetString(pNetInfo.szChatData);
        int headindex = int.Parse(GlobalUserInfo.GBToUtf8(pNetInfo.szHeadHttp));
        //Debug.Log("onSubGPLobbyChat:接收到的数据：headindex=" + headindex + ", szNickName=" + szNickName + ": " + szChatData);

        ShowChatPanel(headindex, szNickName, szChatData);
    }
}
