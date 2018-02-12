using GameNet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerList : MonoBehaviour
{
    public List<GameObject> TaggerList = new List<GameObject>();
    public List<GameObject> HideList = new List<GameObject>();

    public void ShowPlayerList()
    {
        for (PlayerTeam.PlayerTeamType teamType = PlayerTeam.PlayerTeamType.TaggerTeam; teamType < PlayerTeam.PlayerTeamType.MaxTeamNum; teamType++)
        {
            PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(teamType);
            for (int i = 0; i < team.GetPlayerNum(); i++)
            {
                PlayerBase playerBase = GameObjectsManager.GetInstance().GetPlayer(teamType, i);
                if (playerBase != null)
                {
                    if (playerBase.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                    {
                        TaggerList[i].transform.Find("NumInfo").GetComponent<Text>().text = (i + 1).ToString();
                        string name = "";
                        string state = "";
                        if (playerBase.IsAI)
                        {
                            //String[] str = playerBase.gameObject.name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            //name = str[0] + "_" + str[1] + "_" + str[2];
                            name = playerBase.gameObject.name;
                        }
                        else
                        {
                            if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                            {
                                String[] str = GlobalUserInfo.getNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                                name = str[0];
                            }
                            else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                            {
                                var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                                string strNN = kernel.getPlayerByChairID(playerBase.ChairID).GetNickName();
                                if (strNN != "")
                                {
                                    String[] str = strNN.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                                    name = str[0];
                                }
                                else
                                {
                                    name = playerBase.gameObject.name;
                                    state = "离开";
                                }
                            }
                        }
                        TaggerList[i].transform.Find("NameInfo").GetComponent<Text>().text = name;
                        if (playerBase.Hp != 0)
                            TaggerList[i].transform.Find("State").GetComponent<Text>().text = "";
                        else
                            TaggerList[i].transform.Find("State").GetComponent<Text>().text = "失败";
                        if (state == "离开")
                            TaggerList[i].transform.Find("State").GetComponent<Text>().text = "离开";
                        TaggerList[i].SetActive(true);
                    }
                    else
                    {
                        HideList[i].transform.Find("NumInfo").GetComponent<Text>().text = (i + 1).ToString();
                        string name = "";
                        string state = "";
                        if (playerBase.IsAI)
                        {
                            //String[] str = playerBase.gameObject.name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            //name = str[0] + "_" + str[1] + "_" + str[2];
                            name = "机器人";
                        }
                        else
                        {
                            if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                            {
                                String[] str = GlobalUserInfo.getNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                                name = str[0];
                            }
                            else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                            {
                                var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                                string strNN = kernel.getPlayerByChairID(playerBase.ChairID).GetNickName();
                                if (strNN != "")
                                {
                                    String[] str = strNN.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                                    name = str[0];
                                }
                                else
                                {
                                    name = playerBase.gameObject.name;
                                    state = "离开";
                                }
                            }
                        }
                        HideList[i].transform.Find("NameInfo").GetComponent<Text>().text = name;
                        if (playerBase.Hp != 0)
                            HideList[i].transform.Find("State").GetComponent<Text>().text = "";
                        else
                            HideList[i].transform.Find("State").GetComponent<Text>().text = "被发现";
                        if (state == "离开")
                            HideList[i].transform.Find("State").GetComponent<Text>().text = "离开";
                        HideList[i].SetActive(true);
                    }
                }
            }
        }
    }
    public void HidePlayerList()
    {
        //GameObject TaggerListContent = TaggerList.transform.Find("Scroll View/Viewport/Content").gameObject;
        //GameObject HideListContent = HideList.transform.Find("Scroll View/Viewport/Content").gameObject;
        //for (int i = 1; i < TaggerListContent.transform.childCount; i++)
        //{
        //    TaggerListContent.transform.GetChild(i).gameObject.SetActive(false);
        //    Destroy(TaggerListContent.transform.GetChild(i).gameObject);
        //}
        //for (int i = 1; i < HideListContent.transform.childCount; i++)
        //{
        //    HideListContent.transform.GetChild(i).gameObject.SetActive(false);
        //    Destroy(HideListContent.transform.GetChild(i).gameObject);
        //}
        for (int i = 0; i < TaggerList.Count; i++)
        {
            TaggerList[i].transform.Find("NumInfo").GetComponent<Text>().text = "";
            TaggerList[i].transform.Find("NameInfo").GetComponent<Text>().text = "";
            TaggerList[i].transform.Find("State").GetComponent<Text>().text = "";
            TaggerList[i].SetActive(false);
        }
        for (int i = 0; i < HideList.Count; i++)
        {
            HideList[i].transform.Find("NumInfo").GetComponent<Text>().text = "";
            HideList[i].transform.Find("NameInfo").GetComponent<Text>().text = "";
            HideList[i].transform.Find("State").GetComponent<Text>().text = "";
            HideList[i].SetActive(false);
        }
    }
}
