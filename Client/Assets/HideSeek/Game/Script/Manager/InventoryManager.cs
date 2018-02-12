using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameNet;
using Random = UnityEngine.Random;

public class InventoryManager : MonoBehaviour
{
    public const int MAX_INVENTORY_NUM = 16;

    public enum InventoryType
    {
        Inventory_Blood = 0,
        Inventory_Boom,
        Inventory_Key,
        Inventory_Search,
        Inventory_Speed,

        Inventory_Type_Num
    }

    private string[] m_inventoryName = 
    {
        "Inventory_Blood",
        "Inventory_Boom",
        "Inventory_Key",
        "Inventory_Search",
        "Inventory_Speed"
    };

    public struct InventoryObjItem
    {
        public byte id;
        public GameObject obj;
        public bool bUsed;
        public bool bTaked;
    };

    private static InventoryManager _instance = null;
    public static InventoryManager GetInstane
    {
        get
        {
            if (_instance == null)
                _instance = (InventoryManager)GameObject.FindObjectOfType(typeof(InventoryManager));
                //_instance = new InventoryManager();
            return _instance;
        }
    }
    public static bool HaveBoom = false;
    public static List<GameObject> ListInventoryPoint = new List<GameObject>();
    private static List<InventoryObjItem> ListInventory = new List<InventoryObjItem>();  //道具列表

    private void Awake()
    {
    }
    private void Update()
    {
        //道具使用
        CheckInventoryUse();
        if (ControlManager.GetInstance().BoomButton != null)
        {
            if (InventoryManager.HaveBoom)
                ControlManager.GetInstance().BoomButton.gameObject.SetActive(true);
            else
                ControlManager.GetInstance().BoomButton.gameObject.SetActive(false);
        }
    }
    public void InventoryInit()
    {
        DestoryInventoryObjects();
        InventoryPointInit();
        Invoke("InventoryCreate", 0);
    }
    public void SyncInventoryRemove(byte id)
    {
        //Log
        //Debug.LogWarning("SyncInventoryRemove:");
        //for (int i = 0; i < ListInventory.Count; i++)
        //{
        //    Debug.LogWarning("      index=" + i+ ", name=" + ListInventory[i].name );
        //}

        for (int i = 0; i < ListInventory.Count; i++)
        {
            if (ListInventory[i].id == id)
            {
                if (ListInventory[i].obj == null)
                    return;
                Debug.LogWarning("SyncInventoryRemove: remove" + ListInventory[i].obj.name + ", i=" + i + ", id=" + id);

                GameObject.Destroy(ListInventory[i].obj);
                InventoryObjItem temp = ListInventory[i];
                temp.bUsed = true;
                ListInventory[i] = temp;// ListInventory[i].bUsed = true;//struct是传值类型，所以下标取出来的是副本
                //ListInventory.RemoveAt(i);

                return;
            }
        }
       
        Debug.LogError("SyncInventoryRemove error: id=" + id + ", ListInventory.Count=" + ListInventory.Count);
    }
    public void InventoryRemove(int index)
    {
        //Log
        //Debug.LogWarning("InventoryRemove:");
        //for (int i = 0; i < ListInventory.Count; i++)
        //{
        //    Debug.LogWarning("      index=" + i + ", name=" + ListInventory[i].name);
        //}

        if (index >= 0 && index < ListInventory.Count)
        {
            if (ListInventory[index].obj == null)
                return;
            Debug.LogWarning("InventoryRemove: remove" + ListInventory[index].obj.name + ", index=" + index);

            GameObject.Destroy(ListInventory[index].obj);
            InventoryObjItem temp = ListInventory[index];
            temp.bUsed = true;
            ListInventory[index] = temp;// ListInventory[i].bUsed = true;//struct是传值类型，所以下标取出来的是副本
            //ListInventory.RemoveAt(index);
        }
        else
        {
            Debug.LogError("InventoryRemove: incorrect index=" + index + ", ListInventory.Count=" + ListInventory.Count);
        }
    }
    //检测道具使用
    private void CheckInventoryUse()
    {
        Human localHuman = null;
        if (GameObjectsManager.GetInstance() != null)
            localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (localHuman != null && localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
        {
            if (ListInventory.Count != 0 && ListInventory != null)
            {
                for (int i = 0; i < ListInventory.Count; i++)
                {
                    if (ListInventory[i].obj != null && !ListInventory[i].bUsed)
                    {
                        String[] str = ListInventory[i].obj.name.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                        switch (str[1])
                        {
                            case "Search":
                                if (Math.Abs(localHuman.gameObject.transform.position.x - ListInventory[i].obj.transform.position.x) < 1 &&
                                    Math.Abs(localHuman.gameObject.transform.position.z - ListInventory[i].obj.transform.position.z) < 1 &&
                                    !ListInventory[i].bTaked)
                                {
                                    PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(PlayerTeam.PlayerTeamType.HideTeam);
                                    PlayerBase playerBase = null;
                                    if (team.GetPlayerNum() != 0 && localHuman.Hp != 0)
                                    {
                                        int count = 0;
                                        while (true)
                                        {
                                            int index = (int)(MersenneTwister.MT19937.Int63() % team.GetPlayerNum());
                                            playerBase = GameObjectsManager.GetInstance().GetPlayer(PlayerTeam.PlayerTeamType.HideTeam, index);
                                            if (playerBase.Hp > 0)
                                                break;
                                            if (count > 50)  //防止没有存活导致死循环
                                            {
                                                playerBase = null;
                                                break;
                                            }
                                            count++;
                                        }
                                        if (playerBase != null)
                                        {
                                            InventoryObjItem temp = ListInventory[i];
                                            temp.bTaked = true;
                                            ListInventory[i] = temp;

                                            if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                                            {
                                                InventoryRemove(i);
                                                UIManager.GetInstance().ShowMiddleTips("有一个玩家变成了" + playerBase.gameObject.name);
                                            }

                                            if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                                            {
                                                // GetInventory event sync
                                                PlayerEventItem getInventoryEvent = new PlayerEventItem();
                                                getInventoryEvent.cbTeamType = (byte)GameObjectsManager.s_LocalHumanTeamType;
                                                getInventoryEvent.wChairId = (ushort)HNGameManager.m_iLocalChairID;
                                                getInventoryEvent.cbAIId = HNMJ_Defines.INVALID_AI_ID;
                                                getInventoryEvent.cbEventKind = (byte)PlayerBase.PlayerEventKind.GetInventory;
                                                getInventoryEvent.nCustomData0 = (Int32)ListInventory[i].id;
                                                getInventoryEvent.nCustomData1 = (Int32)InventoryType.Inventory_Search;
                                                GameObjectsManager.GetInstance().PlayerEventList.Add(getInventoryEvent);
                                            }
                                        }
                                    }
                                }
                                break;
                            case "Boom":
                                if (Math.Abs(localHuman.gameObject.transform.position.x - ListInventory[i].obj.transform.position.x) < 1 &&
                                    Math.Abs(localHuman.gameObject.transform.position.z - ListInventory[i].obj.transform.position.z) < 1 &&
                                    !ListInventory[i].bTaked)
                                {
                                    if (!HaveBoom && localHuman.Hp != 0)
                                    {
                                        InventoryObjItem temp = ListInventory[i];
                                        temp.bTaked = true;
                                        ListInventory[i] = temp;

                                        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                                        {
                                            HaveBoom = true;
                                            InventoryRemove(i);
                                            //GameSceneUIHandler.ShowLog("你拾取了炸弹！！！");
                                            UIManager.GetInstance().ShowMiddleTips("你拾取了炸弹！！！");
                                            ControlManager.GetInstance().BoomButton.gameObject.SetActive(true);
                                        }

                                        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                                        {
                                            // GetInventory event sync
                                            PlayerEventItem getInventoryEvent = new PlayerEventItem();
                                            getInventoryEvent.cbTeamType = (byte)GameObjectsManager.s_LocalHumanTeamType;
                                            getInventoryEvent.wChairId = (ushort)HNGameManager.m_iLocalChairID;
                                            getInventoryEvent.cbAIId = HNMJ_Defines.INVALID_AI_ID;
                                            getInventoryEvent.cbEventKind = (byte)PlayerBase.PlayerEventKind.GetInventory;
                                            getInventoryEvent.nCustomData0 = (Int32)ListInventory[i].id;
                                            getInventoryEvent.nCustomData1 = (Int32)InventoryType.Inventory_Boom;
                                            GameObjectsManager.GetInstance().PlayerEventList.Add(getInventoryEvent);
                                        }
                                    }
                                }
                                break;
                            case "Blood":
                                if (Math.Abs(localHuman.gameObject.transform.position.x - ListInventory[i].obj.transform.position.x) < 1 &&
                                    Math.Abs(localHuman.gameObject.transform.position.z - ListInventory[i].obj.transform.position.z) < 1 &&
                                    !ListInventory[i].bTaked)
                                {
                                    if (localHuman.Hp < PlayerBase.MaxHp && localHuman.Hp != 0)
                                    {
                                        InventoryObjItem temp = ListInventory[i];
                                        temp.bTaked = true;
                                        ListInventory[i] = temp;

                                        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                                        {
                                            localHuman.Hp++;  //生命增加
                                            InventoryRemove(i);
                                            UIManager.GetInstance().ShowMiddleTips("你拾取了生命道具！！！");
                                        }

                                        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                                        {

                                            // GetInventory event sync
                                            PlayerEventItem getInventoryEvent = new PlayerEventItem();
                                            getInventoryEvent.cbTeamType = (byte)GameObjectsManager.s_LocalHumanTeamType;
                                            getInventoryEvent.wChairId = (ushort)HNGameManager.m_iLocalChairID;
                                            getInventoryEvent.cbAIId = HNMJ_Defines.INVALID_AI_ID;
                                            getInventoryEvent.cbEventKind = (byte)PlayerBase.PlayerEventKind.GetInventory;
                                            getInventoryEvent.nCustomData0 = (Int32)ListInventory[i].id;
                                            getInventoryEvent.nCustomData1 = (Int32)InventoryType.Inventory_Blood;
                                            GameObjectsManager.GetInstance().PlayerEventList.Add(getInventoryEvent);

                                            // AddHp event
                                            PlayerEventItem addHpEvent = new PlayerEventItem();
                                            addHpEvent.cbTeamType = (byte)GameObjectsManager.s_LocalHumanTeamType;
                                            addHpEvent.wChairId = (ushort)HNGameManager.m_iLocalChairID;
                                            addHpEvent.cbAIId = HNMJ_Defines.INVALID_AI_ID;
                                            addHpEvent.cbEventKind = (byte)PlayerBase.PlayerEventKind.AddHp;
                                            GameObjectsManager.GetInstance().PlayerEventList.Add(addHpEvent);
                                        }
                                    }
                                }
                                break;
                            case "Speed":
                                if (Math.Abs(localHuman.gameObject.transform.position.x - ListInventory[i].obj.transform.position.x) < 1 &&
                                    Math.Abs(localHuman.gameObject.transform.position.z - ListInventory[i].obj.transform.position.z) < 1 &&
                                    !ListInventory[i].bTaked)
                                {
                                    if (ControlManager.GetInstance()._etcJoystickL.axisX.speed == ControlManager.s_speed && localHuman.Hp != 0 && localHuman.m_isGrounded)
                                    {
                                        InventoryObjItem temp = ListInventory[i];
                                        temp.bTaked = true;
                                        ListInventory[i] = temp;

                                        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                                        {
                                            InventoryRemove(i);
                                            UIManager.GetInstance().ShowMiddleTips("你拾取了加速道具！！！");
                                            ControlManager.GetInstance().SpeedChange(ControlManager.s_speed * 3);
                                            StartCoroutine(InventorySpeed());
                                        }

                                        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                                        {
                                            // GetInventory event sync
                                            PlayerEventItem getInventoryEvent = new PlayerEventItem();
                                            getInventoryEvent.cbTeamType = (byte)GameObjectsManager.s_LocalHumanTeamType;
                                            getInventoryEvent.wChairId = (ushort)HNGameManager.m_iLocalChairID;
                                            getInventoryEvent.cbAIId = HNMJ_Defines.INVALID_AI_ID;
                                            getInventoryEvent.cbEventKind = (byte)PlayerBase.PlayerEventKind.GetInventory;
                                            getInventoryEvent.nCustomData0 = (Int32)ListInventory[i].id;
                                            getInventoryEvent.nCustomData1 = (Int32)InventoryType.Inventory_Speed;
                                            GameObjectsManager.GetInstance().PlayerEventList.Add(getInventoryEvent);
                                        }
                                    }
                                }
                                break;
                            case "Key":
                                if (Math.Abs(localHuman.gameObject.transform.position.x - ListInventory[i].obj.transform.position.x) < 1 &&
                                    Math.Abs(localHuman.gameObject.transform.position.z - ListInventory[i].obj.transform.position.z) < 1 &&
                                    !ListInventory[i].bTaked)
                                {
                                    //Debug.Log("--------Key1");
                                    if (localHuman.transform.FindChild("Inventory_Arrow") == null && localHuman.Hp != 0)
                                    {
                                        PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(PlayerTeam.PlayerTeamType.HideTeam);
                                        PlayerBase playerBase = null;
                                        if (team.GetPlayerNum() != 0)
                                        {
                                            int count = 0;
                                            while (true)
                                            {
                                                int index = (int)(MersenneTwister.MT19937.Int63() % team.GetPlayerNum());
                                                playerBase = GameObjectsManager.GetInstance().GetPlayer(PlayerTeam.PlayerTeamType.HideTeam, index);
                                                if (playerBase.Hp > 0)
                                                    break;
                                                if (count > 50)  //防止没有存活导致死循环
                                                {
                                                    playerBase = null;
                                                    localHuman = null;
                                                    break;
                                                }
                                                count++;
                                            }
                                            if (playerBase != null && localHuman != null)
                                            {
                                                if (DirectionKey.GetInstance != null)
                                                {
                                                    InventoryObjItem temp = ListInventory[i];
                                                    temp.bTaked = true;
                                                    ListInventory[i] = temp;

                                                    if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                                                    {
                                                        DirectionKey.GetInstance.Deal(localHuman.gameObject, playerBase.gameObject);
                                                        InventoryRemove(i);
                                                        UIManager.GetInstance().ShowMiddleTips("你拾取了钥匙道具！！！");
                                                    }

                                                    if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                                                    {
                                                        // GetInventory event sync
                                                        PlayerEventItem getInventoryEvent = new PlayerEventItem();
                                                        getInventoryEvent.cbTeamType = (byte)GameObjectsManager.s_LocalHumanTeamType;
                                                        getInventoryEvent.wChairId = (ushort)HNGameManager.m_iLocalChairID;
                                                        getInventoryEvent.cbAIId = HNMJ_Defines.INVALID_AI_ID;
                                                        getInventoryEvent.cbEventKind = (byte)PlayerBase.PlayerEventKind.GetInventory;
                                                        getInventoryEvent.nCustomData0 = (Int32)ListInventory[i].id;
                                                        getInventoryEvent.nCustomData1 = (Int32)InventoryType.Inventory_Key;
                                                        GameObjectsManager.GetInstance().PlayerEventList.Add(getInventoryEvent);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
    //使用炸弹
    public void BoomUse()
    {
        if (HaveBoom)
        {
            GameObject loadObj = Resources.Load("Player/Prefabs/Invenrtory/Inventory_Boom") as GameObject;
            GameObject BoomObj = Instantiate(loadObj);
            BoomObj.AddComponent<Boom>();
            GameObject ModelObj = gameObject.transform.parent.transform.parent.transform.Find("Model").gameObject;
            Renderer _renderer = ModelObj.transform.GetChild(0).GetComponent<Renderer>();
            BoomObj.transform.position = ModelObj.transform.position + ModelObj.transform.forward * 1.5f + ModelObj.transform.up * _renderer.bounds.size.y;
            BoomObj.transform.localScale = Vector3.one;
            BoomObj.AddComponent<BoxCollider>();
            Rigidbody Ri = BoomObj.AddComponent<Rigidbody>();
            Ri.useGravity = true;
            Ri.AddForce(ModelObj.transform.forward * 500);
            HaveBoom = false;
            ControlManager.GetInstance().BoomButton.gameObject.SetActive(false);
            //GameSceneUIHandler.ShowLog("你使用了炸弹！！！");
            UIManager.GetInstance().ShowMiddleTips("你使用了炸弹！！！");
        }
    }
    private void InventoryPointInit()
    {
        ListInventoryPoint.Clear();
        if (ListInventoryPoint.Count == 0)
        {
            GameObject[] temp = GameObject.FindGameObjectsWithTag("InventoryPoint");
            if (temp != null)
                for (int i = 0; i < temp.Length; i++)
                    ListInventoryPoint.Add(temp[i]);
        }
    }
    public IEnumerator InventorySpeed()
    {
        yield return new WaitForSeconds(5f);
        ControlManager.GetInstance().SpeedChange(ControlManager.s_speed);
    }
    void InventoryCreate()
    {
        Debug.LogWarning("InventoryCreate: ListInventoryPoint.Count = " + ListInventoryPoint.Count);
        if (ListInventoryPoint.Count > MAX_INVENTORY_NUM)
        {
            Debug.LogError("InventoryCreate: incorrect ListInventoryPoint.Count = " + ListInventoryPoint.Count);
            return;
        }

        ListInventory.Clear();
        GameObject InventoryObject = GameObject.Find("InventoryObject");
        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {
            for (int i = 0; i < ListInventoryPoint.Count; i++)
            {
                if (ListInventoryPoint[i] != null)
                {
                    int index = Random.Range(0, 100) % m_inventoryName.Length;
                    GameObject loadObj = Resources.Load("Player/Prefabs/Invenrtory/" + m_inventoryName[index]) as GameObject;
                    GameObject temp = Instantiate(loadObj);

                    temp.transform.SetParent(InventoryObject.transform, false);
                    temp.transform.position = ListInventoryPoint[i].transform.position;
                    temp.name = m_inventoryName[index];

                    InventoryObjItem item = new InventoryObjItem();
                    item.id = (byte)i;
                    item.obj = temp;
                    item.bUsed = false;
                    item.bTaked = false;
                    ListInventory.Add(item);
                }
            }
        }
        else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
        {
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            MersenneTwister.MT19937.Seed((ulong)pGlobalUserData.wRandseedForInventory);

            for (int i = 0; i < ListInventoryPoint.Count; i++)
            {
                InventoryType inventoryType = (InventoryType)pGlobalUserData.sInventoryList[i].cbType;

                if (ListInventoryPoint[i] != null && pGlobalUserData.sInventoryList[i].cbUsed == 0)
                {
                    //Debug.LogWarning("InventoryCreate: inventoryType=" + inventoryType + " i=" + i);
                    if (inventoryType >= InventoryType.Inventory_Type_Num)
                    {
                        Debug.LogError("InventoryCreate: incorrect inventoryType=" + inventoryType + " i=" + i);
                    }

                    int index = (int)inventoryType;// (int)(MersenneTwister.MT19937.Int63() % m_inventoryName.Length);
                    GameObject loadObj = Resources.Load("Player/Prefabs/Invenrtory/" + m_inventoryName[index]) as GameObject;
                    GameObject temp = Instantiate(loadObj);

                    temp.transform.SetParent(InventoryObject.transform, false);
                    temp.transform.position = ListInventoryPoint[i].transform.position;
                    temp.name = m_inventoryName[index];

                    InventoryObjItem item = new InventoryObjItem();
                    item.id = (byte)i;
                    item.obj = temp;
                    item.bUsed = false;
                    item.bTaked = false;
                    ListInventory.Add(item);
                }
            }
        }

    }
    public void DestoryInventoryObjects()
    {
        GameObject temp = GameObject.Find("InventoryObject");
        if (temp != null)
        {
            for(int i = 0; i < temp.transform.childCount; i++)
            {
                GameObject Obj = temp.transform.GetChild(i).gameObject;
                Destroy(Obj);
            }
            ListInventory.Clear();
            //CancelInvoke();
        }
    }
}
