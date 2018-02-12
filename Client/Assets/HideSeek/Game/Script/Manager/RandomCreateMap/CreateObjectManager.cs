using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameNet;
using System.Linq;

public class CreateObjectManager : MonoBehaviour
{
    public enum GameObjectType
    {
        LargeObject,
        MiddleObject,
        SmallObject
    }
    public enum MapType
    {
        Military = 0,
        Office,
        Port,
        ClassRoom,
        Town,

        MapNum
    }
    //public List<Vector3> m_GameObjectPosition = new List<Vector3>();
    //public List<Vector3> m_GameObjectQuaternion = new List<Vector3>();
    private List<Item> m_GameObjectItemList = new List<Item>();
    private List<GameObject> m_GameObjectL = new List<GameObject>();
    private List<GameObject> m_GameObjectM = new List<GameObject>();
    private List<GameObject> m_GameObjectS = new List<GameObject>();
    private List<GameObject> m_GameObjectLChoose = new List<GameObject>();
    private List<GameObject> m_GameObjectMChoose = new List<GameObject>();
    private List<GameObject> m_GameObjectSChoose = new List<GameObject>();
    private List<string> m_NameL = new List<string>();
    private List<string> m_NameM = new List<string>();
    private List<string> m_NameS = new List<string>();
    private List<string> m_NameLChoose = new List<string>();
    private List<string> m_NameMChoose = new List<string>();
    private List<string> m_NameSChoose = new List<string>();
    private List<string> m_PathL = new List<string>();
    private List<string> m_PathM = new List<string>();
    private List<string> m_PathS = new List<string>();
    private List<string> m_PathLChoose = new List<string>();
    private List<string> m_PathMChoose = new List<string>();
    private List<string> m_PathSChoose = new List<string>();

    void Awake()
    {
        Debug.Log("CreateObjectManager Awake()"); 
        ItemListManager.GetInstance.LoadAndDeserialize();
        m_GameObjectItemList = ItemListManager.GetInstance.items.ItemList;
        for (int i = 0; i < m_GameObjectItemList.Count; i++)
        {
            Scheme schemeTemp = null;
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            string mapname = "";
            if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                mapname = ItemListManager.GetInstance.GetMapName(pGlobalUserData.cbMapIndexRandForSingleGame);
            else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                mapname = ItemListManager.GetInstance.GetMapName(pGlobalUserData.cbMapIndexRand);
            //string mapname = ItemListManager.GetInstance.GetMapName(pGlobalUserData.cbMapIndexRand);
            if (mapname != m_GameObjectItemList[i].Map)
                continue;
            else
                schemeTemp = m_GameObjectItemList[i].SchemeItem;
            for (int k = 0; k < schemeTemp.ModelNameList.Count; k++)
            {
                String[] str = schemeTemp.ModelNameList[k].FileName.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                switch (Convert.ToInt32(str[str.Length - 1]))
                {
                    case 0: //大
                        m_NameL.Add(schemeTemp.ModelNameList[k].FileName);
                        m_PathL.Add(schemeTemp.ModelNameList[k].Path);
                        break;
                    case 1: //中
                        m_NameM.Add(schemeTemp.ModelNameList[k].FileName);
                        m_PathM.Add(schemeTemp.ModelNameList[k].Path);
                        break;
                    case 2: //小
                        m_NameS.Add(schemeTemp.ModelNameList[k].FileName);
                        m_PathS.Add(schemeTemp.ModelNameList[k].Path);
                        break;
                }
            }
        }
        GameObject[] tempL = GameObject.FindGameObjectsWithTag("BigPoint").OrderBy(g => g.transform.GetSiblingIndex()).ToArray();
        for (int i = 0; i < tempL.Length; i++)
        {
            m_GameObjectL.Add(tempL[i]);
        }
        GameObject[] tempM = GameObject.FindGameObjectsWithTag("MiddlePoint").OrderBy(g => g.transform.GetSiblingIndex()).ToArray();
        for (int i = 0; i < tempM.Length; i++)
        {
            m_GameObjectM.Add(tempM[i]);
        }
        GameObject[] tempS = GameObject.FindGameObjectsWithTag("SmallPoint").OrderBy(g => g.transform.GetSiblingIndex()).ToArray();
        for (int i = 0; i < tempS.Length; i++)
        {
            m_GameObjectS.Add(tempS[i]);
        }
    }
    private string RandomMapGameObjectPositionCase()
    {
        string str = "";
        int temp = (int)(MersenneTwister.MT19937.Int63() % 4);
        switch (temp)
        {
            case 0: //方案1
                str = "case0";
                break;
            case 1: //方案2
                str = "case1";
                break;
            case 2: //方案3
                str = "case2";
                break;
            case 3: //方案4
                str = "case3";
                break;
            case 4: //方案5
                str = "case4";
                break;
        }
        return str;
    }
    public void RandomGameObject(GameObjectType type)
    {
        List<GameObject> RandomObject = new List<GameObject>();
        switch (type)
        {
            case GameObjectType.LargeObject:  //大物体随机选择
                if (m_GameObjectL.Count != 0 && m_GameObjectL.Count != 1)
                {
                    while (RandomObject.Count < (m_GameObjectL.Count/* * 2 / 3*/))
                    {
                        RandomAgainL:
                        {
                            int index = (int)(MersenneTwister.MT19937.Int63() % m_GameObjectL.Count);
                            if (!RandomObject.Contains(m_GameObjectL[index])/* && RandomObject.Count < (m_GameObjectL.Count * 2 / 3)*/)
                                RandomObject.Add(m_GameObjectL[index]);
                            else
                                goto RandomAgainL;
                        }
                    }
                    for (int i = 0; i < RandomObject.Count; i++)
                        m_GameObjectLChoose.Add(RandomObject[i]);
                }
                break;
            case GameObjectType.MiddleObject:  //中物体随机选择
                if (m_GameObjectM.Count != 0 && m_GameObjectM.Count != 1)
                {
                    while (RandomObject.Count < (m_GameObjectM.Count/* * 2 / 3*/))
                    {
                        RandomAgainM:
                        {
                            int index = (int)(MersenneTwister.MT19937.Int63() % m_GameObjectM.Count);
                            if (!RandomObject.Contains(m_GameObjectM[index])/* && RandomObject.Count < (m_GameObjectM.Count * 2 / 3)*/)
                                RandomObject.Add(m_GameObjectM[index]);
                            else
                                goto RandomAgainM;
                        }
                    }
                    for (int i = 0; i < RandomObject.Count; i++)
                        m_GameObjectMChoose.Add(RandomObject[i]);
                }
                break;
            case GameObjectType.SmallObject:  //小物体随机选择
                if (m_GameObjectS.Count != 0 && m_GameObjectS.Count != 1)
                {
                    while (RandomObject.Count < (m_GameObjectS.Count/* * 2 / 3*/))
                    {
                        RandomAgainS:
                        {
                            int index = (int)(MersenneTwister.MT19937.Int63() % m_GameObjectS.Count);
                            if (!RandomObject.Contains(m_GameObjectS[index])/* && RandomObject.Count < (m_GameObjectS.Count * 2 / 3)*/)
                                RandomObject.Add(m_GameObjectS[index]);
                            else
                                goto RandomAgainS;
                        }
                    }
                    for (int i = 0; i < RandomObject.Count; i++)
                        m_GameObjectSChoose.Add(RandomObject[i]);
                }
                break;
        }
    }
    public void RandomObjectName(GameObjectType type)
    {
        List<string> RandomName = new List<string>();
        List<string> RandomPath = new List<string>();
        switch (type)
        {
            case GameObjectType.LargeObject:  //大物体随机选择
                while (RandomName.Count < (m_GameObjectL.Count/* * 2 / 3*/))
                {
                    //RandomAgainL:
                    //{
                    //    int index = (int)(MersenneTwister.MT19937.Int63() % m_NameL.Count);
                    //    if (!RandomName.Contains(m_NameL[index])/* && RandomName.Count < (m_GameObjectL.Count * 2 / 3)*/)
                    //        RandomName.Add(m_NameL[index]);
                    //    else
                    //        goto RandomAgainL;
                    //}
                    int index = (int)(MersenneTwister.MT19937.Int63() % m_NameL.Count);
                    RandomName.Add(m_NameL[index]);
                    RandomPath.Add(m_PathL[index]);
                }
                for (int i = 0; i < RandomName.Count; i++)
                {
                    m_NameLChoose.Add(RandomName[i]);
                    m_PathLChoose.Add(RandomPath[i]);
                }
                break;
            case GameObjectType.MiddleObject:  //中物体随机选择
                while (RandomName.Count < (m_GameObjectM.Count/* * 2 / 3*/))
                {
                    //RandomAgainM:
                    //{
                    //    int index = (int)(MersenneTwister.MT19937.Int63() % m_NameM.Count);
                    //    if (!RandomName.Contains(m_NameM[index])/* && RandomName.Count < (m_GameObjectM.Count * 2 / 3)*/)
                    //        RandomName.Add(m_NameM[index]);
                    //    else
                    //        goto RandomAgainM;
                    //}
                    int index = (int)(MersenneTwister.MT19937.Int63() % m_NameM.Count);
                    RandomName.Add(m_NameM[index]);
                    RandomPath.Add(m_PathM[index]);
                }
                for (int i = 0; i < RandomName.Count; i++)
                {
                    m_NameMChoose.Add(RandomName[i]);
                    m_PathMChoose.Add(RandomPath[i]);
                }

                break;
            case GameObjectType.SmallObject:  //小物体随机选择
                while (RandomName.Count < (m_GameObjectS.Count /** 2 / 3*/))
                {
                    //RandomAgainS:
                    //{
                    //    int index = (int)(MersenneTwister.MT19937.Int63() % m_NameS.Count);
                    //    //if (!RandomName.Contains(m_NameS[index])/* && RandomName.Count < (m_GameObjectS.Count * 2 / 3)*/)
                    //        RandomName.Add(m_NameS[index]);
                    //    else
                    //        goto RandomAgainS;
                    //}
                    int index = (int)(MersenneTwister.MT19937.Int63() % m_NameS.Count);
                    RandomName.Add(m_NameS[index]);
                    RandomPath.Add(m_PathS[index]);
                }
                for (int i = 0; i < RandomName.Count; i++)
                {
                    m_NameSChoose.Add(RandomName[i]);
                    m_PathSChoose.Add(RandomPath[i]);
                }

                break;
        }
    }
    public void CreateGameObject()
    {
        for (int i = 0; i < m_GameObjectLChoose.Count; i++)
            CreateObject.GetInstance.InstanceObject(m_NameLChoose[i], m_PathLChoose[i],m_GameObjectLChoose[i].transform.position, m_GameObjectLChoose[i].transform.localEulerAngles);
        for (int i = 0; i < m_GameObjectMChoose.Count; i++)
            CreateObject.GetInstance.InstanceObject(m_NameMChoose[i], m_PathMChoose[i],m_GameObjectMChoose[i].transform.position, m_GameObjectMChoose[i].transform.localEulerAngles);
        for (int i = 0; i < m_GameObjectSChoose.Count; i++)
            CreateObject.GetInstance.InstanceObject(m_NameSChoose[i], m_PathSChoose[i],m_GameObjectSChoose[i].transform.position, m_GameObjectSChoose[i].transform.localEulerAngles);
    }
}
