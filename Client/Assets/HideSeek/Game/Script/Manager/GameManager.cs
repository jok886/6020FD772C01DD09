using UnityEngine;
using System;
using System.Collections;

using GameNet;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public enum GameSingleMultiType
{
    SingleGame,
    MultiGame_Skynet,
    MultiGame_WangHu
}

public class GameManager : MonoBehaviour
{

    // 单例
    private static GameManager _instance = null;
    public static GameManager GetInstance()
    {
        if (!_instance)
        {
            _instance = (GameManager)GameObject.FindObjectOfType(typeof(GameManager));
            //if (!_instance)
            //{
            //    Debug.LogError("There needs to be one active GameManager script on a GameObject in your scene.");
            //}
        }

        return _instance;
    }

    public const float s_deltaTime = 0.03f;

    private static byte s_cbSendClientCount = 0;
    private static float s_fDeltaTimeSinceLastSubHeartBeat = 0f;

    // Network
    public static NetWorkClient s_NetWorkClient = null;
    public static object LockObj { get; set; }

    ///static public bool s_isSingleGame = true;
    static public GameSingleMultiType s_gameSingleMultiType = GameSingleMultiType.MultiGame_WangHu;

    public static int HIDESEEK_WAIT_GAME_TIME = 8;
    public static int HIDESEEK_HIDING_TIME = 45;
    public static int HIDESEEK_GAME_PLAY_TIME = 200;
    public static int s_singleGameStatus;

    public bool m_bSettedNetCB = false;
    private bool m_bSentDeadInfo;
    public CMD_C_HideSeek_ClientPlayersInfo m_playersInfo = new CMD_C_HideSeek_ClientPlayersInfo();
    public static bool m_bHasCreatedAIs = false;

    //出生位置列表
    public static List<GameObject> ListTaggerPosition = new List<GameObject>();  //储存实时地图中的辅助点位置
    public static List<GameObject> ListHiderPosition = new List<GameObject>();
    public static List<int> ListRandomNumT = new List<int>();    //存储ListTaggerPosition的下标
    public static List<int> ListRandomNumH = new List<int>();
    public static Dictionary<int, int> RandomNumT = new Dictionary<int, int>();  //储存ListTaggerPosition是否已被使用信息(0-未使用，1-已使用)
    public static Dictionary<int, int> RandomNumH = new Dictionary<int, int>();
    public static bool isDeadView = false;
    private GameObject[] Door;
    public GameObject CameraPoint;
    public GameObject LookatPoint;

    private HNGameManager m_hnGameManager = null;

    void Awake()
    {
        Debug.Log("GameManager Awake()!!!");

        //添加道具脚本
        if (gameObject.GetComponent<InventoryManager>() == null)
            gameObject.AddComponent<InventoryManager>();
        if (LockObj == null)
        {
            LockObj = new object();
        }

        m_hnGameManager = GameObject.FindObjectOfType<HNGameManager>();

        m_hnGameManager.SetLoading(false);

        GameObjectsManager.GetInstance().CreateHumanTeams();

        //if (s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        //{
        //    InitSingleGame();
        //}

        GameObjectsManager.GetInstance().ListBornPositionInit();
        GameObjectsManager.GetInstance().RandomGameObjects();

        if (UIManager.GetInstance() != null)
            UIManager.GetInstance().UpdateUIInfo();   //进入游戏更新界面UI

        ///m_playersInfo = new CMD_C_HideSeek_ClientPlayersInfo();
        m_playersInfo.HumanInfoItem = new PlayerInfoItem();
        m_playersInfo.AIInfoItems = new PlayerInfoItem[HNMJ_Defines.GAME_PLAYER];
        m_playersInfo.PlayerEventItems = new PlayerEventItem[HNMJ_Defines.GAME_PLAYER];
    }

    public void InitSingleGame()
    {
        //单人模式关闭聊天系统
        if (ControlManager.GetInstance() != null)
            ControlManager.GetInstance().SetChatSystemEnable(false);

        HNGameManager.m_iLocalChairID = 0;
        //GameObjectsManager.s_LocalHumanTeamType = PlayerTeam.PlayerTeamType.TaggerTeam;
        GameObjectsManager.s_LocalHumanTeamType = (PlayerTeam.PlayerTeamType)UnityEngine.Random.Range(0, 2);

        // Create Local Human
        PlayerTeam localTeam = GameObjectsManager.GetInstance().GetPlayerTeam(GameObjectsManager.s_LocalHumanTeamType);
        //if (localTeam != null)
        //{
        //    byte cbModelIndex = (byte)(MersenneTwister.MT19937.Int63() % 255);
        //    localTeam.AddAPlayer(false, HNGameManager.m_iLocalChairID, cbModelIndex);
        //}
        if (localTeam != null)
        {
            byte cbModelIndex = (byte)PlayerPrefs.GetInt("ChoosedModelIndex");
            localTeam.AddAPlayer(false, HNGameManager.m_iLocalChairID, cbModelIndex);
        }

        // Create AIs
        byte cbAIId = 0;
        for (PlayerTeam.PlayerTeamType teamType = PlayerTeam.PlayerTeamType.TaggerTeam; teamType < PlayerTeam.PlayerTeamType.MaxTeamNum; teamType++)
        {
            PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(teamType);
            if (team == null)
            {
                continue;
            }

            int nAINum = 0;
            if (GameObjectsManager.s_LocalHumanTeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
            {
                nAINum = 1;
                if (teamType == PlayerTeam.PlayerTeamType.HideTeam)
                {
                    nAINum = 5;
                }
            }
            else
            {
                nAINum = 4;
                if (teamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                {
                    nAINum = 2;
                }
            }
            for (int i = 0; i < nAINum; i++)
            {
                byte cbModelIndex = (byte)UnityEngine.Random.Range(0, 255); //(byte)(MersenneTwister.MT19937.Int63() % 255);
                team.AddAPlayer(true, HNGameManager.m_iLocalChairID, cbModelIndex, cbAIId);
                cbAIId++;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("GameManager Start()!!!");

        //Loom.QueueOnMainThread(() => { m_hnGameManager.SetLoading(false); }, 0.5f);

        m_bSettedNetCB = false;
        m_bSentDeadInfo = false;
        //m_bHasCreatedAIs = false;

        Door = GameObject.FindGameObjectsWithTag("Door");
        CameraPoint = GameObject.Find("ClassRoomScene/HelpObjs/CameraPoints/PosPoint");
        LookatPoint = GameObject.Find("ClassRoomScene/HelpObjs/CameraPoints/LookAtPoint");
        if (s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {
            ControlManager.GetInstance().Init();
        }
        else
        {
            // MultiGame

            //StartConnecting();
            ControlManager.GetInstance().Init();
            //GameManager.s_NetWorkClient.TryToJoinMatch();
        }

        // for HideSeek WangHu
        InitGame();
        if (m_hnGameManager != null)
        {
            if (s_gameSingleMultiType == GameSingleMultiType.SingleGame)
            {
                m_hnGameManager.OnGameStatus(SocketDefines.GAME_STATUS_WAIT);
            }
            else
            {
                byte Gamestate = CServerItem.get().GetGameStatus();
                m_hnGameManager.OnGameStatus(Gamestate);
            }

        }
        s_fDeltaTimeSinceLastSubHeartBeat = 0f;

        GameObjectsManager.GetInstance().PlayerEventList.Clear();

        //Exp UI
        if (m_hnGameManager.Exp != null && !m_hnGameManager.Exp.activeSelf)
            m_hnGameManager.Exp.SetActive(true);
    }

    // for HideSeek WangHu
    public void InitGame()
    {
        InitMainCamera();
        InitPlayersPos();
    }
    private void InitMainCamera()
    {
        //Main Camera
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (localHuman != null && Camera.main != null && Camera.main.transform.parent != null)
        {
            //if (ControlManager.s_IsFirstPersonControl)
            //{
            //    Camera.main.transform.parent.SetParent(localHuman.gameObject.transform, false);
            //    Camera.main.transform.parent.transform.localPosition = Vector3.zero;
            //}
            GameObject PosPoint = GameObject.Find("PosPoint");
            GameObject LookAtPoint = GameObject.Find("LookAtPoint");
            if (PosPoint != null && LookAtPoint != null)
            {
                if (Camera.main.transform.parent != null)
                {
                    Camera.main.transform.parent.transform.position = PosPoint.transform.position;
                    Camera.main.transform.parent.transform.localEulerAngles = PosPoint.transform.localEulerAngles;
                    Camera.main.transform.LookAt(LookAtPoint.transform);
                    Camera.main.transform.localPosition = Vector3.zero;
                }
            }
            if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
            {
                float y = 0;
                float z = 0;
                GameObject ModelObj = localHuman.transform.GetChild(0).gameObject;
                if (ModelObj.transform.childCount == 0)
                {
                    y = ModelObj.gameObject.GetComponent<Renderer>().bounds.size.y;
                    z = ModelObj.gameObject.GetComponent<Renderer>().bounds.size.z;
                }
                else if (ModelObj.transform.childCount > 0)
                {
                    if (ModelObj.transform.GetChild(0).name == "CameraControl")
                    {
                        y = ModelObj.gameObject.GetComponent<Renderer>().bounds.size.y;
                        z = ModelObj.gameObject.GetComponent<Renderer>().bounds.size.z;
                    }
                    else
                    {
                        y = ModelObj.transform.GetChild(0).GetComponent<Renderer>().bounds.size.y;
                        z = ModelObj.transform.GetChild(0).GetComponent<Renderer>().bounds.size.z;
                    }
                }
                Camera.main.transform.localPosition = new Vector3(0, y * 1.5f + 1, -(z * 1.5f + 2));
                Camera.main.transform.localEulerAngles = new Vector3(30, 0, 0);
            }
            else if (localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
            {
                if (ControlManager.isPerson_1st)
                    Camera.main.transform.localPosition = new Vector3(0, 1.7f, 0);
                else
                    Camera.main.transform.localPosition = new Vector3(0, 2.4f, -3);
                Camera.main.transform.localEulerAngles = Vector3.zero;
            }
            //HNGameManager.CameraLocalPos = Camera.main.transform.localPosition;
        }
        ControlManager.GetInstance().ControlModelSwitch(ControlManager.CameraControlMode.FreeCameraMode);
    }
    public void InitPlayersPos()
    {
        for (PlayerTeam.PlayerTeamType teamType = PlayerTeam.PlayerTeamType.TaggerTeam; teamType < PlayerTeam.PlayerTeamType.MaxTeamNum; teamType++)
        {
            PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(teamType);
            if (team == null)
            {
                continue;
            }

            List<int> ListRandomNum = new List<int>();
            Dictionary<int, int> RandomNum = new Dictionary<int, int>();
            int TypeCount = 0;
            if (teamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                TypeCount = ListTaggerPosition.Count;
            else
                TypeCount = ListHiderPosition.Count;
            while (RandomNum.Count != TypeCount)
            {
                int index = (int)(MersenneTwister.MT19937.Int63() % TypeCount);
                if (!ListRandomNum.Contains(index))
                {
                    ListRandomNum.Add(index);
                    RandomNum.Add(index, 0);
                }
            }
            for (int i = 0; i < team.GetPlayerNum(); i++)
            {
                PlayerBase playerBase = GameObjectsManager.GetInstance().GetPlayer(teamType, i);
                if (playerBase != null)
                {
                    if (playerBase.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                    {
                        if (RandomNum.Count > 0)
                        {
                            if (playerBase.IsAI)
                            {
                                for (int j = 0; j < RandomNum.Count; j++)
                                {
                                    if (RandomNum[ListRandomNum[j]] == 0)
                                    {
                                        playerBase.gameObject.transform.position = ListTaggerPosition[ListRandomNum[j]].transform.position;
                                        playerBase.gameObject.transform.localEulerAngles = ListTaggerPosition[ListRandomNum[j]].transform.localEulerAngles;
                                        RandomNum[ListRandomNum[j]] = 1;
                                        break;
                                    }
                                    else
                                        continue;
                                }
                            }
                            else
                            {
                                playerBase.gameObject.transform.position = ListTaggerPosition[playerBase.ChairID].transform.position;
                                playerBase.gameObject.transform.localEulerAngles = ListTaggerPosition[playerBase.ChairID].transform.localEulerAngles;
                                if (playerBase.IsLocalHuman())
                                    playerBase.gameObject.transform.position += new Vector3(0, 0.5f, 0);
                                RandomNum[playerBase.ChairID] = 1;
                            }
                        }
                    }
                    else
                    {
                        if (RandomNum.Count > 0)
                        {
                            if (playerBase.IsAI)
                            {
                                for (int j = 0; j < RandomNum.Count; j++)
                                {
                                    if (RandomNum[ListRandomNum[j]] == 0)
                                    {
                                        playerBase.gameObject.transform.position = ListHiderPosition[ListRandomNum[j]].transform.position;
                                        playerBase.gameObject.transform.localEulerAngles = ListHiderPosition[ListRandomNum[j]].transform.localEulerAngles;
                                        RandomNum[ListRandomNum[j]] = 1;
                                        break;
                                    }
                                    else
                                        continue;
                                }
                            }
                            else
                            {
                                playerBase.gameObject.transform.position = ListHiderPosition[playerBase.ChairID].transform.position;
                                playerBase.gameObject.transform.localEulerAngles = ListHiderPosition[playerBase.ChairID].transform.localEulerAngles;
                                if (playerBase.IsLocalHuman())
                                    playerBase.gameObject.transform.position += new Vector3(0, 0.5f, 0);
                                RandomNum[playerBase.ChairID] = 1;
                            }
                        }

                    }
                }
            }
        }
    }

    private void StartConnecting()
    {
        if (GameManager.s_NetWorkClient == null)
        {
            GameManager.s_NetWorkClient = new NetWorkClient();
            GameManager.LockObj = new object();

            GameManager.s_NetWorkClient.UserName = String.Empty; //PlayerPrefs.GetString("username");//String.Empty; //
                                                                 //GameManager.s_NetWorkClient.UserName = String.Empty;
            GameManager.s_NetWorkClient.PassWord = String.Empty; //PlayerPrefs.GetString("password");//String.Empty; //
            GameManager.s_NetWorkClient.DisplayName = PlayerPrefs.GetString("displayname");
            ///GameManager.s_NetWorkClient.StartConnect();
        }
    }

    // for HideSeek WangHu
    private void SetPlayerInfoItem(ref PlayerInfoItem tmpPlayerInfoItem, PlayerBase playerBase)
    {
        //Set Pos
        Vector3 playerPos = playerBase.transform.position;
        Vector3 eulerAngles = playerBase.transform.eulerAngles;
        tmpPlayerInfoItem.posX = (int)(playerPos.x * 100.0f);// BitConverter.ToInt64(BitConverter.GetBytes((double)playerPos.x), 0);
        tmpPlayerInfoItem.posY = (int)(playerPos.y * 100.0f);//BitConverter.ToInt64(BitConverter.GetBytes((double)playerPos.y), 0);
        tmpPlayerInfoItem.posZ = (int)(playerPos.z * 100.0f);//BitConverter.ToInt64(BitConverter.GetBytes((double)playerPos.z), 0);
        tmpPlayerInfoItem.angleX = (int)eulerAngles.x;// BitConverter.ToInt64(BitConverter.GetBytes((double)eulerAngles.x), 0);
        tmpPlayerInfoItem.angleY = (int)eulerAngles.y;//BitConverter.ToInt64(BitConverter.GetBytes((double)eulerAngles.y), 0);
        tmpPlayerInfoItem.angleZ = (int)eulerAngles.z;//BitConverter.ToInt64(BitConverter.GetBytes((double)eulerAngles.z), 0);

        //Set cbHP
        tmpPlayerInfoItem.cbHP = (byte)(playerBase.Hp >= 0 ? playerBase.Hp : 0);

        tmpPlayerInfoItem.cbIsValid = 1;

        //to be done
        //tmpPlayerInfoItem.moveSpeed = playerBase.GetCurSpeed();
    }
    private void SendClientPlayersInfo_WangHu()
    {
        byte Gamestate = SocketDefines.GAME_STATUS_FREE;
        enServiceStatus serviceStatus = enServiceStatus.ServiceStatus_Unknow;
        if (CServerItem.get() == null)
        {
            // Log
            Debug.Log("SendClientPlayersInfo_WangHu: return for CServerItem.get() == null");

            return;
        }
        else
        {
            Gamestate = CServerItem.get().GetGameStatus();
            serviceStatus = CServerItem.get().GetServiceStatus();
        }

        if (Gamestate != SocketDefines.GAME_STATUS_HIDE && Gamestate != SocketDefines.GAME_STATUS_PLAY)
        {
            // Log
            if (Gamestate != SocketDefines.GAME_STATUS_WAIT)
            {
                Debug.Log("SendClientPlayersInfo_WangHu: return for Gamestate = " + Gamestate);
            }

            return;
        }

        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        if (kernel == null)
        {
            //Debug.Log("SendClientPlayersInfo_WangHu: return for CServerItem.get().GetClientKernelSink() == null");

            // fix在调用StartOrStopGameSceneHeartBeat(true)前断线（在LocalUserEnter里面），导致游戏时间一直没有更新，但没有跳出断线UI
            //如：有人在团灭前断线，重连回来（已经下一局）还在原来的房间，时间一直没有更新，但没有跳出断线UI
            if (serviceStatus == enServiceStatus.ServiceStatus_ServiceIng)
            {
                s_fDeltaTimeSinceLastSubHeartBeat += Time.deltaTime;
                if (s_fDeltaTimeSinceLastSubHeartBeat > 8f)
                {
                    Debug.Log("SendClientPlayersInfo_WangHu: kernel==null, s_fDeltaTimeSinceLastSubHeartBeat > 8f, serviceStatus = " + serviceStatus);

                    s_fDeltaTimeSinceLastSubHeartBeat = 0f;

                    if (m_hnGameManager != null)
                    {
                        Debug.LogError("SendClientPlayersInfo_WangHu: kernel==null, s_fDeltaTimeSinceLastSubHeartBeat>8f cause LeaveGameToHall");

                        //强制用户离开 
                        m_hnGameManager.m_cbGameEndReason = HNMJ_Defines.GER_USER_LEAVE;
                        m_hnGameManager.StartOrStopGameSceneHeartBeat(false);
                        m_hnGameManager.LeaveRoom();
                        m_hnGameManager.LeaveGameToHall();
                        ///m_hnGameManager.StartOrStopGameSceneHeartBeat(true);
                    }
                }
            }

            return;
        }
        else
        {
            s_fDeltaTimeSinceLastSubHeartBeat = 0f;
        }

        //if (!m_bSettedNetCB)
        //{
        //    m_bSettedNetCB = true;

        //    // for HideSeek WangHu
        //    kernel.addNetCB(HNMJ_Defines.SUB_S_HideSeek_HeartBeat, kernel, OnSubHeartBeat_WangHu, "OnSubHeartBeat_WangHu");
        //    kernel.addNetCB(HNMJ_Defines.SUB_S_HideSeek_AICreateInfo, kernel, OnSubAICreateInfo_WangHu, "OnSubAICreateInfot_WangHu");
        //}



        IClientUserItem pMeItem = CServerItem.get().GetMeUserItem();
        if (pMeItem == null)
        {
            // fix在游戏阶段最后一秒断线，卡死在重连中
            //游戏阶段最后一秒断线，然后就卡死在重连中，一直进这里
            if (serviceStatus == enServiceStatus.ServiceStatus_ServiceIng)
            {
                s_fDeltaTimeSinceLastSubHeartBeat += Time.deltaTime;
                if (s_fDeltaTimeSinceLastSubHeartBeat > 8f)
                {
                    Debug.Log("SendClientPlayersInfo_WangHu: pMeItem==null, s_fDeltaTimeSinceLastSubHeartBeat > 8f");

                    s_fDeltaTimeSinceLastSubHeartBeat = 0f;

                    if (m_hnGameManager != null)
                    {
                        Debug.LogError("SendClientPlayersInfo_WangHu: pMeItem==null, s_fDeltaTimeSinceLastSubHeartBeat>8f cause LeaveGameToHall");

                        //强制用户离开 
                        m_hnGameManager.m_cbGameEndReason = HNMJ_Defines.GER_USER_LEAVE;
                        m_hnGameManager.StartOrStopGameSceneHeartBeat(false);
                        m_hnGameManager.LeaveRoom();
                        m_hnGameManager.LeaveGameToHall();
                        ///m_hnGameManager.StartOrStopGameSceneHeartBeat(true);
                    }
                }
            }

            return;
        }
        int nUserStatus = pMeItem.GetUserStatus();
        //Debug.Log("SendClientPlayersInfo_WangHu:LocalPlayer 当前状态： " + nUserStatus);
        if (nUserStatus == SocketDefines.US_LOOKON)
        {
            //旁观用户
            return;
        }

        //Debug.Log("SendClientPlayersInfo_WangHu:nUserStatus="+ nUserStatus);

        //bool bIsOffline = (nUserStatus == SocketDefines.US_OFFLINE || nUserStatus == SocketDefines.US_NULL
        bool bIsPlaying = (nUserStatus == SocketDefines.US_PLAYING);
        if (!bIsPlaying)
        {
            return;
        }

        // Player Items -------------------------------

        //m_playersInfo.AIInfoItems = new PlayerInfoItem[HNMJ_Defines.GAME_PLAYER];
        //int nMyAINumOfTagger = GameObjectsManager.GetInstance().GetAINum(PlayerTeam.PlayerTeamType.TaggerTeam, HNGameManager.m_iLocalChairID);
        //int nMyAINumOfHide = GameObjectsManager.GetInstance().GetAINum(PlayerTeam.PlayerTeamType.HideTeam, HNGameManager.m_iLocalChairID);
        //m_playersInfo.wPlayerItemCount = (ushort)(1 + nMyAINumOfTagger + nMyAINumOfHide);

        // Set LocalHuman
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (localHuman != null)
        {
            //if (!localHuman.IsDead() || !m_bSentDeadInfo)
            {
                //if (localHuman.IsDead())
                //{
                //    m_bSentDeadInfo = true;
                //}

                // temp add
                m_playersInfo.HumanInfoItem.cbTeamType = (byte)localHuman.TeamType;
                m_playersInfo.HumanInfoItem.wChairId = (ushort)localHuman.ChairID;

                m_playersInfo.HumanInfoItem.cbAIId = HNMJ_Defines.INVALID_AI_ID;

                SetPlayerInfoItem(ref m_playersInfo.HumanInfoItem, localHuman);

                //if (GameObjectsManager.GetInstance().KilledPlayer != 0)
                //{
                //    m_playersInfo.HumanInfoItem.cbKilledPlayer = GameObjectsManager.GetInstance().KilledPlayer;
                //    m_playersInfo.HumanInfoItem.cbKilledAIIdx = GameObjectsManager.GetInstance().KilledAIIdx;

                //    GameObjectsManager.GetInstance().KilledPlayer = 0;
                //}
            }
        }

        // Local AIs
        int nAIItemIdx = 0;
        for (PlayerTeam.PlayerTeamType teamType = PlayerTeam.PlayerTeamType.TaggerTeam; teamType < PlayerTeam.PlayerTeamType.MaxTeamNum; teamType++)
        {
            PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(teamType);
            if (team == null)
            {
                continue;
            }

            int nPlayerNum = team.GetPlayerNum();
            //Debug.Log("-----------------" + nPlayerNum);
            for (int i = 0; i < nPlayerNum; i++)
            {
                PlayerBase player = team.GetPlayer(i);

                if (player == null)
                {
                    continue;
                }

                //Local AI
                if (player.IsAI && player.ChairID == HNGameManager.m_iLocalChairID)
                {
                    // temp add
                    m_playersInfo.AIInfoItems[nAIItemIdx].wChairId = (ushort)player.ChairID;

                    m_playersInfo.AIInfoItems[nAIItemIdx].cbTeamType = (byte)player.TeamType;
                    m_playersInfo.AIInfoItems[nAIItemIdx].cbAIId = player.AIId;

                    SetPlayerInfoItem(ref m_playersInfo.AIInfoItems[nAIItemIdx], player);

                    nAIItemIdx++;
                }
            }
        }
        m_playersInfo.wAIItemCount = (ushort)nAIItemIdx;

        // Event Items -------------------------------

        ///m_playersInfo.PlayerEventItems = new PlayerEventItem[HNMJ_Defines.GAME_PLAYER];
        int nEventItemIdx = 0;
        foreach (PlayerEventItem eventItem in GameObjectsManager.GetInstance().PlayerEventList)
        {
            m_playersInfo.PlayerEventItems[nEventItemIdx] = eventItem;

            nEventItemIdx++;
        }
        GameObjectsManager.GetInstance().PlayerEventList.Clear();
        m_playersInfo.wEventItemCount = (ushort)nEventItemIdx;

        // Send
        byte[] buf = m_playersInfo.ToBytes();// StructConverterByteArray.StructToBytes(m_playersInfo);
        //var buf =  StructConverterByteArray.StructToBytes(m_playersInfo);
        //int nLen = buf.Length;
        //ushort dataSizeOfPlayerInfo = (ushort)(m_playersInfo.wPlayerItemCount * Marshal.SizeOf(typeof(PlayerInfoItem)));
        //ushort dataSizeOfEventInfo = (ushort)(m_playersInfo.wEventItemCount * Marshal.SizeOf(typeof(PlayerEventItem)));
        //ushort dataSize = (ushort)(sizeof(ushort) + sizeof(ushort) + dataSizeOfPlayerInfo + dataSizeOfEventInfo);
        kernel.SendSocketData(HNMJ_Defines.SUB_C_HIDESEEK_PLAYERS_INFO, buf, (ushort)buf.Length);
    }
    public void OnSubHeartBeat_WangHu(byte[] pBuffer, ushort wDataSize)
    {
        s_fDeltaTimeSinceLastSubHeartBeat = 0f;

        byte Gamestate = SocketDefines.GAME_STATUS_FREE;
        if (CServerItem.get() != null)
        {
            Gamestate = CServerItem.get().GetGameStatus();
        }
        if (Gamestate != SocketDefines.GAME_STATUS_HIDE && Gamestate != SocketDefines.GAME_STATUS_PLAY)
        {
            return;
        }

        CMD_S_HideSeek_HeartBeat pHeartBeatMsg = new CMD_S_HideSeek_HeartBeat();
        pHeartBeatMsg.StreamValue(pBuffer, wDataSize);

        // Event Items -------------------------------
        for (int i = 0; i < pHeartBeatMsg.nEventItemCount; i++)
        {
            PlayerEventItem eventItem = pHeartBeatMsg.PlayerEventItems[i];

            PlayerBase.PlayerEventKind eventKind = (PlayerBase.PlayerEventKind)eventItem.cbEventKind;
            switch (eventKind)
            {
                case PlayerBase.PlayerEventKind.Pick:
                    Loom.QueueOnMainThread(() =>
                    {
                        PlayerBase playerBase = GameObjectsManager.GetInstance().GetPlayerByChairID((PlayerTeam.PlayerTeamType)eventItem.cbTeamType, (int)eventItem.wChairId, eventItem.cbAIId);
                        if (playerBase != null)
                        {
                            playerBase.PlayPickupAnim();

                            //if (m_hnGameManager != null)
                            //{
                            //    m_hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_EFFECT_PickObj);
                            //}
                        }
                    });
                    break;

                case PlayerBase.PlayerEventKind.DeadByDecHp:
                    Loom.QueueOnMainThread(() =>
                    {
                        PlayerBase deadPlayer = GameObjectsManager.GetInstance().GetPlayerByChairID((PlayerTeam.PlayerTeamType)eventItem.cbTeamType, (int)eventItem.wChairId, eventItem.cbAIId);
                        PlayerBase killer = GameObjectsManager.GetInstance().GetPlayerByChairID((PlayerTeam.PlayerTeamType)eventItem.nCustomData0, (int)eventItem.nCustomData1, (byte)eventItem.nCustomData2);
                        if (deadPlayer != null)
                        {
                            deadPlayer.SyncDead(eventKind, killer);
                            //var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                            //if (kernel != null)  //联机
                            //{
                            //    String[] str = kernel.getPlayerByChairID(eventItem.nCustomData1).GetNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                            //    UIManager.GetInstance().ShowMiddleTips(str[0] + "找到了" + deadPlayer.name);
                            //}
                        }
                    });
                    break;
                case PlayerBase.PlayerEventKind.DeadByPicked:
                    Loom.QueueOnMainThread(() =>
                    {
                        PlayerBase deadPlayer = GameObjectsManager.GetInstance().GetPlayerByChairID((PlayerTeam.PlayerTeamType)eventItem.cbTeamType, (int)eventItem.wChairId, eventItem.cbAIId);
                        PlayerBase killer = GameObjectsManager.GetInstance().GetPlayerByChairID((PlayerTeam.PlayerTeamType)eventItem.nCustomData0, (int)eventItem.nCustomData1, (byte)eventItem.nCustomData2);
                        if (deadPlayer != null)
                        {
                            deadPlayer.SyncDead(eventKind, killer);
                            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                            if (kernel != null)  //联机
                            {
                                if (killer.IsAI)
                                {
                                    //String[] str = killer.gameObject.name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                                    //String[] pickedStr = deadPlayer.gameObject.name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                                    UIManager.GetInstance().ShowMiddleTips(killer.gameObject.name + " 找到了: " + deadPlayer.gameObject.name);
                                }
                                else
                                {
                                    if (s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu && killer.IsLocalHuman())
                                        kernel.SendAwardData(10, 0);  //找到一个人奖励

                                    String[] str = kernel.getPlayerByChairID(eventItem.nCustomData1).GetNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                                    //String[] pickedStr = deadPlayer.gameObject.name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                                    UIManager.GetInstance().ShowMiddleTips(str[0] + " 找到了: " + deadPlayer.gameObject.name);
                                }
                            }
                        }
                    });
                    break;
                case PlayerBase.PlayerEventKind.DeadByBoom:
                    Loom.QueueOnMainThread(() =>
                    {
                        PlayerBase deadPlayer = GameObjectsManager.GetInstance().GetPlayerByChairID((PlayerTeam.PlayerTeamType)eventItem.cbTeamType, (int)eventItem.wChairId, eventItem.cbAIId);
                        PlayerBase killer = GameObjectsManager.GetInstance().GetPlayerByChairID((PlayerTeam.PlayerTeamType)eventItem.nCustomData0, (int)eventItem.nCustomData1, (byte)eventItem.nCustomData2);
                        if (deadPlayer != null)
                        {
                            deadPlayer.SyncDead(eventKind, killer);
                            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                            if (kernel != null)  //联机
                            {
                                if (killer.IsAI)
                                {
                                    //String[] str = killer.gameObject.name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                                    //String[] pickedStr = deadPlayer.gameObject.name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                                    UIManager.GetInstance().ShowMiddleTips(killer.gameObject.name + " 找到了: " + deadPlayer.gameObject.name);
                                }
                                else
                                {
                                    if (s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu && killer.IsLocalHuman())
                                        kernel.SendAwardData(10, 0);  //找到一个人奖励

                                    String[] str = kernel.getPlayerByChairID(eventItem.nCustomData1).GetNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                                    //String[] pickedStr = deadPlayer.gameObject.name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                                    UIManager.GetInstance().ShowMiddleTips(str[0] + " 找到了: " + deadPlayer.gameObject.name);
                                }
                            }
                        }
                    });
                    break;

                case PlayerBase.PlayerEventKind.Boom:
                    Loom.QueueOnMainThread(() =>
                    {
                        bool bIsLocalPlayer = (eventItem.cbTeamType == (byte)GameObjectsManager.s_LocalHumanTeamType && eventItem.wChairId == (ushort)HNGameManager.m_iLocalChairID);
                        if (!bIsLocalPlayer)
                        {
                            //Boom特效
                            GameObject loadObj = Resources.Load("Player/Prefabs/Invenrtory/FX Comic Explosion 1 Large BOOM") as GameObject;
                            GameObject BoomFX = Instantiate(loadObj);
                            Vector3 vBoomPos;
                            vBoomPos.x = (float)eventItem.nCustomData0;
                            vBoomPos.y = (float)eventItem.nCustomData1;
                            vBoomPos.z = (float)eventItem.nCustomData2;
                            BoomFX.transform.position = vBoomPos;

                            if (m_hnGameManager != null)
                            {
                                m_hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_EFFECT_Boom);
                            }
                        }
                    });
                    break;

                case PlayerBase.PlayerEventKind.GetInventory:
                    Loom.QueueOnMainThread(() =>
                    {
                        byte nInventoryId = (byte)eventItem.nCustomData0;
                        byte nInventoryType = (byte)eventItem.nCustomData1;
                        bool bIsLocalPlayer = (eventItem.cbTeamType == (byte)GameObjectsManager.s_LocalHumanTeamType && eventItem.wChairId == (ushort)HNGameManager.m_iLocalChairID);
                        if (bIsLocalPlayer)
                        {
                            Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
                            if (localHuman == null)
                                return;
                            switch((InventoryManager.InventoryType)nInventoryType)
                            {
                                case InventoryManager.InventoryType.Inventory_Blood:
                                    {
                                        InventoryManager.GetInstane.InventoryRemove(nInventoryId);
                                        UIManager.GetInstance().ShowMiddleTips("你拾取了生命道具！！！");
                                    }
                                    break;
                                case InventoryManager.InventoryType.Inventory_Boom:
                                    {
                                        InventoryManager.HaveBoom = true;
                                        InventoryManager.GetInstane.InventoryRemove(nInventoryId);
                                        UIManager.GetInstance().ShowMiddleTips("你拾取了炸弹！！！");
                                        ControlManager.GetInstance().BoomButton.gameObject.SetActive(true); 
                                    }
                                    break;
                                case InventoryManager.InventoryType.Inventory_Key:
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
                                            if (playerBase != null)
                                            {
                                                if (DirectionKey.GetInstance != null)
                                                {
                                                    DirectionKey.GetInstance.Deal(localHuman.gameObject, playerBase.gameObject);
                                                    InventoryManager.GetInstane.InventoryRemove(nInventoryId);
                                                    UIManager.GetInstance().ShowMiddleTips("你拾取了钥匙道具！！！");
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case InventoryManager.InventoryType.Inventory_Search:
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
                                                InventoryManager.GetInstane.InventoryRemove(nInventoryId);
                                                UIManager.GetInstance().ShowMiddleTips("有一个玩家变成了" + playerBase.gameObject.name);
                                            }
                                        }
                                    }
                                    break;
                                case InventoryManager.InventoryType.Inventory_Speed:
                                    {
                                        InventoryManager.GetInstane.InventoryRemove(nInventoryId);
                                        UIManager.GetInstance().ShowMiddleTips("你拾取了加速道具！！！");
                                        ControlManager.GetInstance().SpeedChange(ControlManager.s_speed * 3);
                                        StartCoroutine(InventoryManager.GetInstane.InventorySpeed());
                                    }
                                    break;
                            }
                        }
                        else
                            InventoryManager.GetInstane.SyncInventoryRemove(nInventoryId);
                    });
                    break;

                case PlayerBase.PlayerEventKind.DecHp:
                    //通过服务端的HP数据来同步
                    //Loom.QueueOnMainThread(() =>
                    //{
                    //    PlayerBase playerBase = GameObjectsManager.GetInstance().GetPlayerByChairID((PlayerTeam.PlayerTeamType)eventItem.cbTeamType, (int)eventItem.wChairId, eventItem.cbAIId);
                    //    if (playerBase != null)
                    //    {
                    //        playerBase.DecHP();
                    //    }
                    //});
                    break;
            }
        }

        // Player Items -------------------------------
        for (int i = 0; i < pHeartBeatMsg.nPlayerItemCount; i++)
        {
            PlayerInfoItem playerInfoItem = pHeartBeatMsg.PlayerInfoItems[i];

            PlayerBase playerBase = GameObjectsManager.GetInstance().GetPlayerByChairID((PlayerTeam.PlayerTeamType)playerInfoItem.cbTeamType, (int)playerInfoItem.wChairId, playerInfoItem.cbAIId);

            if (playerBase == null)
            {
                continue;
            }

            if (playerBase.IsDead())
            {
                continue;
            }

            // Hp同步
            playerBase.Hp = (int)playerInfoItem.cbHP;

            // Dead同步
            if (playerInfoItem.cbHP <= 0)
            {
                Loom.QueueOnMainThread(() =>
                {
                    if (playerBase != null)
                    {
                        playerBase.MakeDead();
                    }
                });
            }

            if (playerInfoItem.wChairId == HNGameManager.m_iLocalChairID)
            {
                // Is local Human or local AI
            }
            else
            {
                //// Hp同步
                //playerBase.Hp = (int)playerInfoItem.cbHP;

                // 位置同步
                Loom.QueueOnMainThread(() =>
                {
                    if (playerBase == null)
                    {
                        return;
                    }

                    Vector3 playerPos;
                    playerPos.x = (float)BitConverter.ToInt32(BitConverter.GetBytes(playerInfoItem.posX), 0) / 100.0f;
                    playerPos.y = (float)BitConverter.ToInt32(BitConverter.GetBytes(playerInfoItem.posY), 0) / 100.0f;
                    playerPos.z = (float)BitConverter.ToInt32(BitConverter.GetBytes(playerInfoItem.posZ), 0) / 100.0f;

                    //Vector3 vec3Delta = playerPos - playerBase.transform.position;
                    //playerBase.CurMoveSpeed = vec3Delta.magnitude / 0.01f;
                    //if (vec3Delta.magnitude > 0f)
                    //{
                    //    Debug.Log("moved");
                    //}
                    playerBase.transform.position = playerPos;

                    Vector3 eulerAngles;
                    eulerAngles.x = (float)BitConverter.ToInt32(BitConverter.GetBytes(playerInfoItem.angleX), 0);
                    eulerAngles.y = (float)BitConverter.ToInt32(BitConverter.GetBytes(playerInfoItem.angleY), 0);
                    eulerAngles.z = (float)BitConverter.ToInt32(BitConverter.GetBytes(playerInfoItem.angleZ), 0);
                    playerBase.transform.eulerAngles = eulerAngles;

                });
            }
        }
    }

    // for HideSeek
    //AI分配信息
    public void OnSubAICreateInfo_WangHu(byte[] pBuffer, ushort wDataSize)
    {
        if (m_bHasCreatedAIs)
        {
            Debug.LogWarning("OnSubAICreateInfo_WangHu: but HasCreatedAIs");
            return;
        }
        m_bHasCreatedAIs = true;

        CMD_GF_S_AICreateInfoItems AICreateInfoItems = new CMD_GF_S_AICreateInfoItems();
        AICreateInfoItems.StreamValue(pBuffer, wDataSize);
        Debug.Log("OnSubAICreateInfo_WangHu:wItemCount=" + AICreateInfoItems.wItemCount);

        lock (HNGameManager.LockObjOfLoadScene)
        {
            Loom.QueueOnMainThread(() =>
            {
                for (int i = 0; i < AICreateInfoItems.wItemCount; i++)
                {
                    AICreateInfoItem aiInfoItem = AICreateInfoItems.AICreateInfoItems[i];

                    PlayerTeam.PlayerTeamType teamType = (PlayerTeam.PlayerTeamType)aiInfoItem.cbTeamType;
                    PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(teamType);

                    PlayerBase playerAI = team.GetPlayerByChairID((int)aiInfoItem.wChairId, aiInfoItem.cbAIId);
                    if (playerAI == null)
                    {
                        team.AddAPlayer(true, (int)aiInfoItem.wChairId, aiInfoItem.cbModelIdx, aiInfoItem.cbAIId);
                    }
                }
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        ControlManager.GetInstance().GetAxisFromETC();
        if (s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {
            LocalGameServer.GetInstance().Heartbeat();
            ControlManager.GetInstance().DetectLocalControl();
        }
        else
        {
            // MultiGame

            //if (GameManager.s_NetWorkClient.MultiGameState == NetWorkClient.GameState.Game_Running)
            //if (GameManager.s_NetWorkClient.UserLoginState == NetWorkClient.LoginState.LoginSuccess)
            //{
            //    if (GameManager.s_NetWorkClient.MultiGameState == NetWorkClient.GameState.Game_NullState)
            //    {
            //        GameObjectsManager.GetInstance().CreateGameObjects();
            //        ControlManager.GetInstance().Init();

            //        GameManager.s_NetWorkClient.FindMatch();
            //    }
            //}

            LocalGameServer.GetInstance().Heartbeat();
            ControlManager.GetInstance().DetectLocalControl();

            // for HideSeek WangHu
            if (s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
            {
                s_cbSendClientCount++;
                //if (s_cbSendClientCount >= 2)
                {
                    s_cbSendClientCount = 0;
                    SendClientPlayersInfo_WangHu();
                }

            }
        }
    }

    private void FixedUpdate()
    {
        //if (IsMultiGame())
        //{
        //    // MultiGame
        //    if (GameManager.s_NetWorkClient.MultiGameState == NetWorkClient.GameState.Game_Running)
        //    {
        //        _pingDeltaTime += Time.deltaTime;
        //        if (_pingDeltaTime > 0.5f)
        //        {
        //            _pingDeltaTime = 0.0f;
        //            StartCoroutine("PingMethod");
        //        }
        //    }
        //}

        if (s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {
            int fastFrameNum = 5;
            while ((fastFrameNum-- > 0) && ControlManager.GetInstance().HaveServerMsg())
            {
                ControlManager.GetInstance().CustomUpdate();
                GameObjectsManager.GetInstance().CustomUpdate();
            }
        }
        else if (s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
        {
            int fastFrameNum = 5;
            while ((fastFrameNum-- > 0) && ControlManager.GetInstance().HaveServerMsg())
            {
                ControlManager.GetInstance().CustomUpdate();
                GameObjectsManager.GetInstance().CustomUpdate();
            }
        }
        else
        {
            // GameSingleMultiType.MultiGame_Skynet

            //if (GameManager.s_NetWorkClient.MultiGameState == NetWorkClient.GameState.Game_Match && GameManager.s_NetWorkClient.TheUserMatchState == NetWorkClient.UserMatchState.AllPlayersJoinedRoom)
            //{
            //    GameManager.s_NetWorkClient.TheUserMatchState = NetWorkClient.UserMatchState.WaitForStart;
            //    SprotoType.waitforstart.request reqToSend = new SprotoType.waitforstart.request();
            //    reqToSend.readyFrame = 0;
            //    NetSender.Send<Protocol.waitforstart>(reqToSend);
            //    ControlManager.GetInstance().GameFrameNum = 0;
            //}

            if (GameManager.s_NetWorkClient.MultiGameState == NetWorkClient.GameState.Game_Running)
            {
                int fastFrameNum = 5;
                while ((fastFrameNum-- > 0) && ControlManager.GetInstance().HaveServerMsg())
                {
                    ControlManager.GetInstance().CustomUpdate();
                    GameObjectsManager.GetInstance().CustomUpdate();
                }
            }
        }
    }
    private void LateUpdate()
    {
        //限制相机旋转角度
        Func<float, float> CheckAngle = (value) =>
       {
           float angle = value - 180;
           if (angle > 0)
               return angle - 180;
           return angle + 180;
       };
        if (CheckAngle(Camera.main.transform.localEulerAngles.x) < -20)
        {
            Camera.main.transform.localEulerAngles = new Vector3(-20, 0, 0);
        }
        if (CheckAngle(Camera.main.transform.localEulerAngles.x) > 50)
        {
            Camera.main.transform.localEulerAngles = new Vector3(50, 0, 0);
        }
        //限制相机高度
        if (Camera.main.transform.parent != null)
        {
            if (ControlManager.m_Up && Camera.main.transform.parent.position.y < 35)
            {

                Camera.main.transform.parent.Translate(new Vector3(0, 1, 0) * Time.deltaTime * 10);
            }
            else if (ControlManager.m_Down && Camera.main.transform.parent.position.y > (0 - Camera.main.transform.localPosition.y) + 0.8)
            {
                Camera.main.transform.parent.Translate(new Vector3(0, -1, 0) * Time.deltaTime * 10);
            }
        }
    }
    void WindowFunction()
    {
        GUILayout.Label("This is a draggable window!");

        //窗口的拖条(drag-strip),坐标相对于窗口的左上角  
        GUI.DragWindow(new Rect(0, 0, 150, 20));
    }

    void WinFuncOfPickupPos(int windowID)
    {
        //GUI.contentColor = Color.green;
        //GUILayout.Label("Test label...");

        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }
        Vector3 pickupPosOfScreen = Input.mousePosition;

        if (pickupPosOfScreen.magnitude > 0.01f)
        {
            GUILayout.Label("pickupPosOfScreen1: " + pickupPosOfScreen);

            GUI.contentColor = Color.green;
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(500, 500, 100, 100), "pickupPosOfScreen2：" + pickupPosOfScreen);

            Debug.Log("pickupPosOfScreen3=" + pickupPosOfScreen);
        }

        GUI.DragWindow();
    }

    void OnGUI()
    {
        return;

        GUILayout.Window(0, new Rect(25, 25, 100, 100), WinFuncOfPickupPos, "My Window");

        Vector3 pickupPosOfScreen = new Vector3();
#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount <= 0)
        {
            return;
        }
        else if (Input.touches[0].phase != TouchPhase.Began)
        {
            return;
        }
        pickupPosOfScreen = Input.GetTouch(0).position;
#else
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }
        pickupPosOfScreen = Input.mousePosition;
#endif
        if (pickupPosOfScreen.magnitude > 0.01f)
        {
            GUILayout.Label("pickupPosOfScreen1: " + pickupPosOfScreen);
            GUI.Label(new Rect(500, 500, 100, 100), "pickupPosOfScreen2：" + pickupPosOfScreen);
            Debug.Log("pickupPosOfScreen3=" + pickupPosOfScreen);
        }
    }

    //临时房门控制
    public void OpenDoor()
    {
        if (Door != null)
        {
            for (int i = 0; i < Door.Length; i++)
            {
                if (Door[i] != null && Door[i].activeSelf)
                    Door[i].SetActive(false);
            }
        }

    }
    public void CloseDoor()
    {
        if (Door != null)
        {
            for (int i = 0; i < Door.Length; i++)
            {
                if (Door[i] != null)
                    Door[i].SetActive(true);
            }
        }
    }
    //切换所有躲藏玩家显隐
    public void SetAllHiderVisible(bool bVisible)
    {
        GameObject temp = null;
        temp = GameObject.Find("Player");
        if (temp != null)
        {
            GameObject hideTeam = temp.transform.Find("HideTeam").gameObject;
            if (hideTeam != null)
            {
                for (int i = 0; i < hideTeam.transform.childCount; i++)
                {
                    PlayerBase hider = hideTeam.transform.GetChild(i).gameObject.GetComponent<PlayerBase>();
                    if (hider != null)
                        hider.SetGameObjVisible(bVisible);
                }
            }
        }
    }
    public bool SetLookOn()
    {
        if (CServerItem.get() == null)
        {
            return false;
        }

        IClientUserItem pMeItem = CServerItem.get().GetMeUserItem();
        byte Gamestate = CServerItem.get().GetGameStatus();
        if (pMeItem == null)
        {
            return false;
        }
        int nStatus = pMeItem.GetUserStatus();
        //Debug.Log("当前状态： " + nStatus);
        if (nStatus == SocketDefines.US_LOOKON)
        {
            m_hnGameManager.PlayBGM();
            if (ControlManager.GetInstance() != null)
            {
                ControlManager.GetInstance().ControlModelSwitch(ControlManager.CameraControlMode.LookOnMode);
                if (UIManager.GetInstance() != null)
                {
                    UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(false);
                    if (Gamestate == SocketDefines.GAME_STATUS_HIDE)
                        UIManager.GetInstance().ShowTopTips("躲藏阶段...", true);
                    else if (Gamestate == SocketDefines.GAME_STATUS_PLAY)
                        UIManager.GetInstance().ShowTopTips("游戏阶段...", true);
                }
                SetAllHiderVisible(true);
                OpenDoor();
                return true;
            }
            else
            {
                Debug.Log("无法找到单例");
                return false;
            }
        }
        return false;
    }
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireCube(new Vector3(Screen.height / 2, Screen.width / 2, 0), new Vector3(Screen.height*0.6f, Screen.width*0.4f, 0));
    //}
}