using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameNet;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.Linq;

public class GameObjectsManager
{
    // 单例
    private static GameObjectsManager _instance;
    public static GameObjectsManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new GameObjectsManager();
        }

        return _instance;
    }

    //private static int s_LocalHumanIndex = 0;
    public static int s_LocalPlayerIndexInGlobal { get; set; }
    public static PlayerTeam.PlayerTeamType s_LocalHumanTeamType = PlayerTeam.PlayerTeamType.TaggerTeam;//{ get { return (PlayerTeam.PlayerTeamType)(GameObjectsManager.s_LocalHumanIndex / (int)PlayerTeam.PlayerTeamType.MaxTeamNum); }  }

    private PlayerTeam[] _playerTeams;

    //mChen add, for HideSeek WangHu
    public List<PlayerEventItem> PlayerEventList = new List<PlayerEventItem>();

    public void CreateHumanTeams()
    {
        if (GameManager.LockObj == null)
        {
            GameManager.LockObj = new object();
        }

        lock (GameManager.LockObj)
        {
            if (_playerTeams == null)
            {
                _playerTeams = new PlayerTeam[(int)PlayerTeam.PlayerTeamType.MaxTeamNum];
                for (PlayerTeam.PlayerTeamType type = PlayerTeam.PlayerTeamType.TaggerTeam; type < PlayerTeam.PlayerTeamType.MaxTeamNum; type++)
                {
                    _playerTeams[(int)type] = new PlayerTeam(type);
                }
            }
        }
    }

    public void RemovePlayerByChairID(PlayerTeam.PlayerTeamType teamType, byte cbChairID, byte cbAIId = HNMJ_Defines.INVALID_AI_ID)
    {
        PlayerTeam team = GetPlayerTeam(teamType);
        if (team != null)
            team.RemovePlayerByChairID(cbChairID);
    }

    public void ClearPlayers(bool bClearAI = true)
    {
        RemovePlayers(bClearAI);

        //移除随机位置数据 WQ
        GameManager.RandomNumT.Clear();
        GameManager.RandomNumH.Clear();
        GameManager.ListRandomNumT.Clear();
        GameManager.ListRandomNumH.Clear();
        GameManager.ListHiderPosition.Clear();
        GameManager.ListTaggerPosition.Clear();
        GameManager.isDeadView = false;
        InventoryManager.HaveBoom = false;
        if (UIManager.GetInstance() != null)
            UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(false);
    }

    public void RemovePlayers(bool bRemoveAI = true)
    {
        if (Camera.main.gameObject != null && Camera.main.transform.parent != null)
        {
            GameObject PosPoint = GameObject.Find("PosPoint");
            GameObject LookAtPoint = GameObject.Find("LookAtPoint");
            if (PosPoint != null && LookAtPoint != null)
            {
                Camera.main.transform.parent.SetParent(null, false);
                Camera.main.transform.parent.transform.position = PosPoint.transform.position;
                Camera.main.transform.parent.transform.localEulerAngles = PosPoint.transform.localEulerAngles;
                Camera.main.transform.LookAt(LookAtPoint.transform);
            }
        }

        for (PlayerTeam.PlayerTeamType teamType = PlayerTeam.PlayerTeamType.TaggerTeam; teamType < PlayerTeam.PlayerTeamType.MaxTeamNum; teamType++)
        {
            PlayerTeam team = GetPlayerTeam(teamType);
            if (team != null)
            {
                team.RemovePlayers(bRemoveAI);
            }
        }

        //GameObject LocalTaggerObj = GameObject.Find("Player/TaggerTeam/LocalTagger");
        //GameObject.Destroy(LocalTaggerObj);

        //GameObject LocalHideObj = GameObject.Find("Player/HideTeam/LocalHide");
        //GameObject.Destroy(LocalHideObj);

        GameObject LocalHumanObj = GameObject.FindGameObjectWithTag("LocalHuman");
        GameObject.Destroy(LocalHumanObj);

        if (bRemoveAI)
        {
            GameObject TaggerTeam = GameObject.Find("Player/TaggerTeam");
            if (TaggerTeam != null)
            {
                for (int i = 0; i < TaggerTeam.transform.childCount; i++)
                {
                    GameObject.Destroy(TaggerTeam.transform.GetChild(i).gameObject);
                }
            }

            GameObject HideTeam = GameObject.Find("Player/HideTeam");
            if (HideTeam != null)
            {
                for (int i = 0; i < HideTeam.transform.childCount; i++)
                {
                    GameObject.Destroy(HideTeam.transform.GetChild(i).gameObject);
                }
            }
        }
    }

    public PlayerBase GetPlayer(PlayerTeam.PlayerTeamType teamType, int playerIndex)
    {
        PlayerBase player = null;

        if (_playerTeams != null && _playerTeams[(int)teamType] != null)
        {
            player = _playerTeams[(int)teamType].GetPlayer(playerIndex);
        }

        return player;
    }

    //mChen add, for HideSeek WangHu
    public PlayerBase GetPlayerByChairID(PlayerTeam.PlayerTeamType teamType, int nChairID, byte cbAIId = HNMJ_Defines.INVALID_AI_ID)
    {
        PlayerBase player = null;
        if (_playerTeams != null && _playerTeams[(int)teamType] != null)
        {
            player = _playerTeams[(int)teamType].GetPlayerByChairID(nChairID, cbAIId);
        }
        return player;
    }

    public PlayerBase GetPlayerByChairID(int nChaidID, byte cbAIId = HNMJ_Defines.INVALID_AI_ID)
    {
        PlayerBase player = null;

        for (PlayerTeam.PlayerTeamType teamType = PlayerTeam.PlayerTeamType.TaggerTeam; teamType < PlayerTeam.PlayerTeamType.MaxTeamNum; teamType++)
        {
            if (_playerTeams != null && _playerTeams[(int)teamType] != null)
            {
                player = _playerTeams[(int)teamType].GetPlayerByChairID(nChaidID, cbAIId);
                if (player != null)
                {
                    return player;
                }
            }
        }
        ///PlayerBase player = _playerTeams[(int)teamType].GetPlayerByChairID(nChaidID);

        return null;
    }
    public int GetAINum(PlayerTeam.PlayerTeamType teamType, int nChaidID)
    {
        int nAINum = 0;
        if (_playerTeams != null && _playerTeams[(int)teamType] != null)
        {
            nAINum = _playerTeams[(int)teamType].GetAINum(nChaidID);
        }

        return nAINum;
    }
    public Human GetLocalHuman()
    {
        PlayerBase localPlayer = null;
        localPlayer = GetPlayerByChairID(s_LocalHumanTeamType, HNGameManager.m_iLocalChairID);

        if (localPlayer == null)
        {
            return null;
        }

        return (Human)localPlayer;
    }

    public PlayerTeam GetPlayerTeam(PlayerTeam.PlayerTeamType teamType)
    {
        if (_playerTeams == null)
        {
            CreateHumanTeams();
        }

        PlayerTeam team = null;
        if (teamType != PlayerTeam.PlayerTeamType.MaxTeamNum)
        {
            team = _playerTeams[(int)teamType];
        }
        return team;
    }
    public int GetRealPlayerNum(PlayerTeam.PlayerTeamType teamType)
    {
        int num = 0;
        PlayerTeam team = null;
        if (teamType != PlayerTeam.PlayerTeamType.MaxTeamNum)
        {
            team = _playerTeams[(int)teamType];
            for (int i = 0; i < team.GetPlayerNum(); i++)
            {
                PlayerBase playerBase = GetPlayer(teamType, i);
                if (!playerBase.IsAI)
                    num++;
            }
        }
        return num;
    }
    public int GetPlayerMaxChairId(PlayerTeam.PlayerTeamType teamType)
    {
        int chairId = 0;
        PlayerTeam team = null;
        if (teamType != PlayerTeam.PlayerTeamType.MaxTeamNum)
        {
            team = _playerTeams[(int)teamType];
            for(int i=0;i<team.GetPlayerNum();i++)
            {
                PlayerBase playerBase = GetPlayer(teamType, i);
                if (!playerBase.IsAI)
                    chairId = playerBase.ChairID > chairId ? playerBase.ChairID : chairId;
            }
        }
        return chairId;
    }

    public void CustomUpdate()
    {
        // Humans
        if (_playerTeams != null)
        {
            for (PlayerTeam.PlayerTeamType teamType = PlayerTeam.PlayerTeamType.TaggerTeam; teamType < PlayerTeam.PlayerTeamType.MaxTeamNum; teamType++)
            {
                _playerTeams[(int)teamType].CustomUpdate();
            }
        }
    }
    public void RandomGameObjects()
    {
        GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
        tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
        MersenneTwister.MT19937.Seed((ulong)pGlobalUserData.wRandseedForRandomGameObject);

        CreateObjectManager temp = Camera.main.transform.gameObject.AddComponent<CreateObjectManager>();
        temp.RandomGameObject(CreateObjectManager.GameObjectType.LargeObject);
        temp.RandomGameObject(CreateObjectManager.GameObjectType.MiddleObject);
        temp.RandomGameObject(CreateObjectManager.GameObjectType.SmallObject);
        temp.RandomObjectName(CreateObjectManager.GameObjectType.LargeObject);
        temp.RandomObjectName(CreateObjectManager.GameObjectType.MiddleObject);
        temp.RandomObjectName(CreateObjectManager.GameObjectType.SmallObject);
        temp.CreateGameObject();
    }
    public void ListBornPositionInit()
    {
        GameManager.ListTaggerPosition.Clear();
        GameManager.ListHiderPosition.Clear();
        if (GameManager.ListTaggerPosition.Count == 0)
        {
            GameObject[] tempT = GameObject.FindGameObjectsWithTag("TaggerBornPoint").OrderBy(g => g.transform.GetSiblingIndex()).ToArray(); 
            for (int i = 0; i < tempT.Length; i++)
            {
                GameManager.ListTaggerPosition.Add(tempT[i]/*.transform.localPosition*/);
            }
            GameObject[] tempH = GameObject.FindGameObjectsWithTag("HiderBornPoint").OrderBy(g => g.transform.GetSiblingIndex()).ToArray(); 
            for (int i = 0; i < tempH.Length; i++)
            {
                GameManager.ListHiderPosition.Add(tempH[i]/*.transform.localPosition*/);
            }
        }
        //if (ListTaggerPosition.Count != 0 && ListHiderPosition.Count != 0)
        //    GameObjectsManager.GetInstance().PrefabsObjInit();
    }

    public void LoadMap(int nMapIndex)
    {
        CreateObjectManager.MapType mapType = (CreateObjectManager.MapType)(nMapIndex % (int)CreateObjectManager.MapType.MapNum);

        Debug.Log("LoadMap: nMapIndex=" + nMapIndex + " ,mapType="+ mapType);

        switch (mapType)
        {
            case CreateObjectManager.MapType.Military:
                if (SceneManager.GetActiveScene().name != "MilitaryScene_01")
                    SceneManager.LoadScene("MilitaryScene_01");
                //Transition.LoadLevel("MilitaryScene_01", 0f, Color.black);
                break;
            case CreateObjectManager.MapType.Office:
                if (SceneManager.GetActiveScene().name != "OfficeScene_01")
                    SceneManager.LoadScene("OfficeScene_01");
                //Transition.LoadLevel("OfficeScene_01", 0f, Color.black);
                break;
            case CreateObjectManager.MapType.Port:
                if (SceneManager.GetActiveScene().name != "SpotScene_01")
                    SceneManager.LoadScene("SpotScene_01");
                //Transition.LoadLevel("SpotScene_01", 0f, Color.black);
                break;
            case CreateObjectManager.MapType.ClassRoom:
                if (SceneManager.GetActiveScene().name != "ClassRoomScene_01")
                    SceneManager.LoadScene("ClassRoomScene_01");
                //Transition.LoadLevel("ClassRoomScene_01", 0f, Color.black);
                break;
            case CreateObjectManager.MapType.Town:
                if (SceneManager.GetActiveScene().name != "TownScene_01")
                    SceneManager.LoadScene("TownScene_01");
                //Transition.LoadLevel("TownScene_01", 0f, Color.black);
                break;
            default:
                Debug.LogError("该地图索引失败，无法载入!");
                break;
        }
    }

    public void EnableLocalAI(PlayerTeam.PlayerTeamType teamType, bool bEnable)
    {
        PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(teamType);
        if (team == null)
        {
            return;
        }

        int nPlayerNum = team.GetPlayerNum();
        for (int i = 0; i < nPlayerNum; i++)
        {
            PlayerBase player = team.GetPlayer(i);

            if (player == null)
            {
                continue;
            }

            if (!player.IsAI)
            {
                continue;
            }

            //Local AI
            if (player.ChairID == HNGameManager.m_iLocalChairID)
            {
                BehaviorDesigner.Runtime.BehaviorTree behaviorTree = player.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
                if (behaviorTree != null)
                {
                    behaviorTree.enabled = bEnable;
                }

                UnityEngine.AI.NavMeshAgent navMeshAgent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (navMeshAgent != null)
                {
                    navMeshAgent.enabled = bEnable;
                }
            }
        }
    }
    public void SaveOffLineInfo()
    {
        //断线储存本地玩家信息
        if (!PlayerPrefs.HasKey("LocalHumanInfo"))
        {
            Vector3 pos = Vector3.zero;
            Vector3 posEA = Vector3.zero;
            Vector3 controlcamerapos = Vector3.zero;
            Vector3 controlcameraposEA = Vector3.zero;
            Vector3 camerapos = Vector3.zero;
            Vector3 cameraposEA = Vector3.zero;
            int HP = 0;
            int Lock = 0;
            int Boom = 0;
            int ViewMode = 0;
            int isPerson_1st = 0;
            //float coldTime = 0;
            //int leftTime = 0;
            //string str = "";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (InventoryManager.HaveBoom) Boom = 1;
            else Boom = 0;
            if (ControlManager.isPerson_1st) isPerson_1st = 1;
            else isPerson_1st = 0;
            //coldTime = UIManager.coldTime;
            //leftTime = UIManager.TimeLeft;

            GameObject localHuman = GameObject.FindGameObjectWithTag("LocalHuman");
            if (localHuman != null)
            {
                //GameObject localHuman = Camera.main.transform.parent.transform.parent.gameObject;
                pos = localHuman.gameObject.transform.position;
                posEA = localHuman.gameObject.transform.localEulerAngles;
                if (Camera.main.transform.parent != null)
                {
                    controlcamerapos = Camera.main.transform.parent.transform.position;
                    controlcameraposEA = Camera.main.transform.parent.transform.localEulerAngles;
                }

                camerapos = Camera.main.transform.localPosition;
                //HNGameManager.CameraLocalPos= Camera.main.transform.localPosition;
                cameraposEA = Camera.main.transform.localEulerAngles;
                HP = localHuman.GetComponent<PlayerBase>().Hp;
                if (localHuman.GetComponent<PlayerBase>().m_Lock) Lock = 1;
                else Lock = 0;
                if (localHuman.transform.Find("CameraControl") != null && HP != 0) ViewMode = 0;
                else if (localHuman.transform.Find("CameraControl") == null /*&& HP != 0*/) ViewMode = 1;
                //else if (localHuman.transform.Find("CameraControl") == null && HP == 0) ViewMode = 2;

                sb.Append(pos.x); sb.Append("_"); sb.Append(pos.y); sb.Append("_"); sb.Append(pos.z); sb.Append("_");
                sb.Append(posEA.x); sb.Append("_"); sb.Append(posEA.y); sb.Append("_"); sb.Append(posEA.z); sb.Append("_");
                sb.Append(controlcamerapos.x); sb.Append("_"); sb.Append(controlcamerapos.y); sb.Append("_"); sb.Append(controlcamerapos.z); sb.Append("_");
                sb.Append(controlcameraposEA.x); sb.Append("_"); sb.Append(controlcameraposEA.y); sb.Append("_"); sb.Append(controlcameraposEA.z); sb.Append("_");
                sb.Append(camerapos.x); sb.Append("_"); sb.Append(camerapos.y); sb.Append("_"); sb.Append(camerapos.z); sb.Append("_");
                sb.Append(cameraposEA.x); sb.Append("_"); sb.Append(cameraposEA.y); sb.Append("_"); sb.Append(cameraposEA.z); sb.Append("_");
                sb.Append(HP); sb.Append("_"); sb.Append(Lock); sb.Append("_"); sb.Append(Boom); sb.Append("_");
                sb.Append(ViewMode); sb.Append("_"); sb.Append(isPerson_1st);
                //sb.Append("_"); sb.Append(coldTime); sb.Append("_");
                //sb.Append(leftTime);
                //str = pos.x + "_" + pos.y + "_" + pos.z + "_" +
                //    posEA.x + "_" + posEA.y + "_" + posEA.z + "_" +
                //    controlcamerapos.x + "_" + controlcamerapos.y + "_" + controlcamerapos.z + "_" +
                //    controlcameraposEA.x + "_" + controlcameraposEA.y + "_" + controlcameraposEA.z + "_" +
                //    camerapos.x + "_" + camerapos.y + "_" + camerapos.z + "_" +
                //    cameraposEA.x + "_" + cameraposEA.y + "_" + cameraposEA.z + "_" +
                //    HP + "_" + Lock + "_" + Boom + "_" + ViewMode + "_" + isPerson_1st;
                Debug.Log("玩家断线信息保存(存活): " + sb);
                PlayerPrefs.SetString("LocalHumanInfo", sb.ToString());
            }
            else  //死亡状态
            {
                if (Camera.main.transform.parent != null)
                {
                    controlcamerapos = Camera.main.transform.parent.transform.position;
                    controlcameraposEA = Camera.main.transform.parent.transform.localEulerAngles;
                }

                camerapos = Camera.main.transform.localPosition;
                //HNGameManager.CameraLocalPos = Camera.main.transform.localPosition;
                cameraposEA = Camera.main.transform.localEulerAngles;

                sb.Append(pos.x); sb.Append("_"); sb.Append(pos.y); sb.Append("_"); sb.Append(pos.z); sb.Append("_");
                sb.Append(posEA.x); sb.Append("_"); sb.Append(posEA.y); sb.Append("_"); sb.Append(posEA.z); sb.Append("_");
                sb.Append(controlcamerapos.x); sb.Append("_"); sb.Append(controlcamerapos.y); sb.Append("_"); sb.Append(controlcamerapos.z); sb.Append("_");
                sb.Append(controlcameraposEA.x); sb.Append("_"); sb.Append(controlcameraposEA.y); sb.Append("_"); sb.Append(controlcameraposEA.z); sb.Append("_");
                sb.Append(camerapos.x); sb.Append("_"); sb.Append(camerapos.y); sb.Append("_"); sb.Append(camerapos.z); sb.Append("_");
                sb.Append(cameraposEA.x); sb.Append("_"); sb.Append(cameraposEA.y); sb.Append("_"); sb.Append(cameraposEA.z); sb.Append("_");
                sb.Append(HP); sb.Append("_"); sb.Append(Lock); sb.Append("_"); sb.Append(Boom); sb.Append("_");
                sb.Append(ViewMode); sb.Append("_"); sb.Append(isPerson_1st);
                //sb.Append("_"); sb.Append(coldTime); sb.Append("_");
                //sb.Append(leftTime);
                //str = pos.x + "_" + pos.y + "_" + pos.z + "_" +
                //    posEA.x + "_" + posEA.y + "_" + posEA.z + "_" +
                //    controlcamerapos.x + "_" + controlcamerapos.y + "_" + controlcamerapos.z + "_" +
                //    controlcameraposEA.x + "_" + controlcameraposEA.y + "_" + controlcameraposEA.z + "_" +
                //    camerapos.x + "_" + camerapos.y + "_" + camerapos.z + "_" +
                //    cameraposEA.x + "_" + cameraposEA.y + "_" + cameraposEA.z + "_" +
                //    HP + "_" + Lock + "_" + Boom + "_" + ViewMode + "_" + isPerson_1st;
                Debug.Log("玩家断线信息保存(死亡): " + sb);
                PlayerPrefs.SetString("LocalHumanInfo", sb.ToString());
                PlayerPrefs.Save();
            }
            if (ControlManager.GetInstance() != null)
                ControlManager.GetInstance().SetETCUIControlEnable(false);
        }
    }
    public void LoadOffLineInfo()
    {
        Debug.Log("LoadOffLineInfo");

        GameObject localHuman = GameObject.FindGameObjectWithTag("LocalHuman");
        if (/*Camera.main.transform.parent.transform.parent*/localHuman != null && PlayerPrefs.HasKey("LocalHumanInfo"))
        {
            //GameObject localHuman = Camera.main.transform.parent.transform.parent.gameObject;
            string str = PlayerPrefs.GetString("LocalHumanInfo");
            Debug.Log("玩家重连信息设置: " + str);
            String[] info = str.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            if (int.Parse(info[19]) == 1)
                localHuman.GetComponent<PlayerBase>().m_Lock = true;
            else
                localHuman.GetComponent<PlayerBase>().m_Lock = false;
            if (int.Parse(info[22]) == 1) ControlManager.isPerson_1st = true;
            else ControlManager.isPerson_1st = false;
            //if (float.Parse(info[23]) > (int.Parse(info[24]) - UIManager.TimeLeft))
            //{
            //    //UIManager.GetInstance().StartColdTime(ControlManager.GetInstance().StealthButton, float.Parse(info[23]));
            //}

            if (localHuman.GetComponent<PlayerBase>().TeamType == PlayerTeam.PlayerTeamType.HideTeam)
            {
                if (HNGameManager.GetInstance.GlobalEffectAudioSource != null)  //重连时关闭音效
                    HNGameManager.GetInstance.GlobalEffectAudioSource.enabled = false;
                ControlManager.GetInstance().ControlModelSwitch(ControlManager.CameraControlMode.PlayerViewMode);
                localHuman.GetComponent<PlayerBase>().m_Lock = !localHuman.GetComponent<PlayerBase>().m_Lock;
                ControlManager.GetInstance().ClickLockButton();
                if (HNGameManager.GetInstance.GlobalEffectAudioSource != null)
                    HNGameManager.GetInstance.GlobalEffectAudioSource.enabled = true;
            }
            else if (localHuman.GetComponent<PlayerBase>().TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
            {
                ControlManager.GetInstance().ControlModelSwitch((ControlManager.CameraControlMode)int.Parse(info[21]));
            }
            if (int.Parse(info[20]) == 1)
            {
                ControlManager.GetInstance().BoomButton.gameObject.SetActive(true);
                InventoryManager.HaveBoom = true;
            }
            else
            {
                ControlManager.GetInstance().BoomButton.gameObject.SetActive(false);
                InventoryManager.HaveBoom = false;
            }
            localHuman.transform.position = new Vector3(float.Parse(info[0]), float.Parse(info[1]), float.Parse(info[2]));
            localHuman.transform.localEulerAngles = new Vector3(float.Parse(info[3]), float.Parse(info[4]), float.Parse(info[5]));
            if (Camera.main.transform.parent != null)
            {
                Camera.main.transform.parent.transform.position = new Vector3(float.Parse(info[6]), float.Parse(info[7]), float.Parse(info[8]));
                Camera.main.transform.parent.transform.localEulerAngles = new Vector3(float.Parse(info[9]), float.Parse(info[10]), float.Parse(info[11]));
            }
            //Camera.main.transform.localPosition = new Vector3(float.Parse(info[12]), float.Parse(info[13]), float.Parse(info[14]));
            Camera.main.transform.localEulerAngles = new Vector3(float.Parse(info[15]), float.Parse(info[16]), float.Parse(info[17]));
            if (int.Parse(info[18]) == 0)
            {
                //GameObject infoPanel = localHuman.transform.FindChild("InfoPanel").gameObject;
                //if (infoPanel != null) infoPanel.SetActive(false);
                //GameObject modelObj = localHuman.transform.GetChild(0).gameObject;
                //if (modelObj != null)
                //{
                //    if (modelObj.transform.childCount == 0)
                //    {
                //        modelObj.transform.GetComponent<Renderer>().enabled = false;
                //    }
                //    else if (modelObj.transform.childCount > 0)
                //    {
                //        for (int j = 0; j < modelObj.transform.childCount; j++)
                //            if (modelObj.transform.GetChild(j).GetComponent<Renderer>() != null)
                //                modelObj.transform.GetChild(j).GetComponent<Renderer>().enabled = false;
                //    }
                //}
                localHuman.GetComponent<PlayerBase>().SetGameObjRenderVisible(false);    //已死亡断线重连后先隐藏模型防止穿帮
                if (HNGameManager.GetInstance.GlobalEffectAudioSource != null)
                    HNGameManager.GetInstance.GlobalEffectAudioSource.enabled = false;
                localHuman.GetComponent<PlayerBase>().MakeDead(true);            //断线重连时不再重现复活按钮
                if (HNGameManager.GetInstance.GlobalEffectAudioSource != null)
                    HNGameManager.GetInstance.GlobalEffectAudioSource.enabled = true;
                if (UIManager.GetInstance() != null)
                {
                    UIManager.GetInstance().m_Canvas.transform.Find("Btn/ObjectSwitch").gameObject.SetActive(false);  //模型切换
                    UIManager.GetInstance().m_Canvas.transform.Find("Btn/StealthButton").gameObject.SetActive(false);
                }
            }
            else
            {
                localHuman.GetComponent<PlayerBase>().Hp = int.Parse(info[18]);
                if (UIManager.GetInstance() != null && UIManager.GetInstance().m_Canvas != null)
                {
                    if (localHuman.GetComponent<PlayerBase>().TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                        UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(true);
                    else
                    {
                        UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(false);
                        UIManager.GetInstance().m_Canvas.transform.Find("Btn/ObjectSwitch").gameObject.SetActive(true);  //模型切换
                        UIManager.GetInstance().m_Canvas.transform.Find("Btn/StealthButton").gameObject.SetActive(true);
                    }
                    for (int i = 1; i <= int.Parse(info[18]); i++)
                    {
                        if (i <= PlayerBase.MaxHp)
                            UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart" + (i - 1)).GetComponent<Image>().sprite = Resources.Load("UI/Hearts/Pink", typeof(Sprite)) as Sprite;
                    }
                    for (int i = int.Parse(info[18]) + 1; i <= 4; i++)
                    {
                        if (i <= PlayerBase.MaxHp)
                            UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart" + (i - 1)).GetComponent<Image>().sprite = Resources.Load("UI/Hearts/Grey", typeof(Sprite)) as Sprite;
                    }
                }
            }

            PlayerPrefs.DeleteKey("LocalHumanInfo");
            PlayerPrefs.Save();
            if (ControlManager.GetInstance() != null)
                ControlManager.GetInstance().SetETCUIControlEnable(true);
        }
    }

    public void SetLocalHumanForWard(Vector3 forward)   //设置玩家模型随摇杆旋转
    {
        Human localHuman = GetLocalHuman();
        Vector3 localEulerAngles = Vector3.zero;
        if (localHuman != null)
        {
            if (localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
            {
                if (localHuman.transform.Find("CameraControl") != null && !ControlManager.isPerson_1st)   //警察在玩家视角模式且第三人称才允许进入
                {
                    GameObject ModelObj = localHuman.transform.GetChild(0).transform.gameObject;
                    ModelObj.transform.LookAt(new Vector3(ModelObj.transform.position.x + forward.x, ModelObj.transform.position.y,
                        ModelObj.transform.position.z + forward.z));
                }
            }
        }
    }
}