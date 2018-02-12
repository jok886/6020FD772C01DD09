#define NEW_CARDS
#define FPS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GameNet;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.Events;

public class HNGameManager : MonoBehaviour
{
    private static HNGameManager _instance = null;
    public static HNGameManager GetInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<HNGameManager>();
            }
            return _instance;
        }
    }

    public static string ItemsListText = "";
    public static string NickNameListText = "";
    public static List<Item> listItem = new List<Item>();
    public static List<Tagger> listTagger = new List<Tagger>();
    public static List<string> taggerPrefabNames = new List<string>();
    public static List<string> taggerPrefabFileNames = new List<string>();
    public static List<string> taggerPrefabPath = new List<string>();
    public static List<string> hidePrefabNames = new List<string>();
    public static List<string> hidePrefabFileNames = new List<string>();
    public static List<string> hidePrefabPath = new List<string>();

    //public static bool gameManagerPos = false;
    public static bool playerTeamPos = false;
    public bool localEnter = false;
    public bool playAgain = false;
    public bool isReconnect = false;

    private bool m_bPressStartButton = false;

    //控制方案
    public static byte ControlCase;
    //相机位置
    public static Vector3 CameraLocalPos = Vector3.zero;

    //Exp
    public GameObject Exp;

    //Loading
    public GameObject Loading;

    //ChatSystem
    public ChatSystem chatSystem;

    //mChen add, for HideSeek
    public GameObject PlayerTeamObj;
    public GameObject CameraControl;
    public static object LockObjOfLoadScene { get; set; }

    public static bool bWeChatLogonIn = true;
    public Sprite[] NextOrFinalImage;

    private Sprite[] HeadImage = new Sprite[4];
    private GameObject[] MyUserScore = new GameObject[HNMJ_Defines.GAME_PLAYER];
    public static byte GameType = HNPrivateScenceBase.GAME_TYPE_Null;        //游戏类型：13水或者建德
    int[] tempnum = new int[4];                                             //玩家总分  
    public int totalcount = 0, nowcount = 0;                                       //局数
    public int TempRoomId = 0;                                                     //房间号
    public uint CreateUserID;
    public bool m_bRoomStartGame = false;

    public int[] WinCount_MJ = new int[HNMJ_Defines.GAME_PLAYER];             //储存每个玩家的胜场数目(胡牌数目)
    public int[] MaxScore_MJ = new int[HNMJ_Defines.GAME_PLAYER];           //储存每个玩家在本次游戏中单局最高分
    public int[] TotalScore_MJ = new int[HNMJ_Defines.GAME_PLAYER];         //储存每个玩家在本次游戏中的总分
    public Sprite[] GenderSprites;
    public AudioSource BackgroundAudioSource;
    public AudioManager AudioPlayGManager;
    public Dictionary<int, List<AudioClip>> cardClip;
    public AudioSource GlobalEffectAudioSource = null;
    private AudioSource HuSoundClipAudioSource = null;


    public GameObject FocusUI;
#if UNITY_IPHONE
    [DllImport("__Internal")]  
    private static extern void StartIOSRecord();
    
    [DllImport("__Internal")]
    private static extern void EndIOSRecord();
    
    [DllImport("__Internal")]  
    private static extern void PlayIOSSound();
#endif

#if UNITY_IPHONE
	//[DllImport("__Internal")]  
	//private static extern void requestProduct();
#endif


    //一小局结算界面
    public GameObject FinalUI;
    public Text HuPaiUI;
    public Text ChengBaoUI;
    public Text ScoreUI;

    public GameObject LeftTipsObj;
    public GameObject LiuJuObj;

    //表情预定义语句
    public GameObject EmoJiAndLanUI;
    private GameObject EmojiUI;
    private GameObject LansUI;

    public GameObject SpeakingUI;
    public MeshRenderer ThreeDLight;

    //public int ServerType;

    //Sound
    public Single m_musicVolume = 1.0f;
    public Single m_soundEffectVolume = 1.0f;
    public AudioManager.Sound_Place m_soundPlace;// = AudioManager.Sound_Place.MeiCheng;

    //规则设置
    public long m_baseScore = 1;            //底分
    public byte m_cbPlayCoutIdex = 0;       //局数  
    public byte m_cbPlayCostTypeIdex = 1;   //支付方式：0-房主支付， 1-平均支付
    public byte m_PlayerCount = 4;

    //mChen add, for Match Time
    public systemtime m_matchStartTime;
    public systemtime m_matchEndTime;

    private float m_offlineWaitTime = 0;
    private GameObject m_breakLineWindow;

    //mChen add, 解散房间倒计时
    private Coroutine m_timeDownOfDismissRoom;
    public GameObject m_timeDownObjOfDismissRoom;

    private GameObject m_readyButton;

    private GameObject m_redForeground;

    private Font m_fontOrange;
    private Font m_fontBlue;

    public struct PlayerUI
    {
        public GameObject SeatNode;
        public GameObject CardNodeRoot;
        ///public GameObject CardCpgNode;
        public List<GameObject> CardCpgList;
        public Transform[] CardNode;
        public ExpAnimation ExpUVAnimTexture;
        public GameObject LansObj;
        public GameObject VoiceUI;
        public GameObject CurHightLight;
        public GameObject FlowerNode;
        public AudioSource Audio;
        public bool AnimEnd;

        public GameObject m_breakBackObj;
        public GameObject m_playerReadyObj;
        public GameObject m_huScoreObj;
        public GameObject m_huInfoObj;

        public void CloneFrom(PlayerUI other)
        {
            CardNodeRoot = other.CardNodeRoot;
            ///CardCpgNode = other.CardCpgNode;
            SeatNode = other.SeatNode;
            CardCpgList = new List<GameObject>(other.CardCpgList);
            CardNode = new Transform[other.CardNode.Length];
            Array.Copy(other.CardNode, 0, CardNode, 0, other.CardNode.Length);
            ExpUVAnimTexture = other.ExpUVAnimTexture;
            LansObj = other.LansObj;
            VoiceUI = other.VoiceUI;
            CurHightLight = other.CurHightLight;
            FlowerNode = other.FlowerNode;
            Audio = other.Audio;
            AnimEnd = other.AnimEnd;

            m_breakBackObj = other.m_breakBackObj;
            m_playerReadyObj = other.m_playerReadyObj;
            m_huScoreObj = other.m_huScoreObj;
            m_huInfoObj = other.m_huInfoObj;
        }
    }

    public bool bEnteredGameScene = false;
    private Camera gameSceneCamera;
    //game scene UI:
    private Text RoomId;
    private Text LeftCardLabel;
    private Text PlayCoutLabel;
    private Text BaseScoreTxt;
    private Text PlayerCountTxt;

    private GameObject gameplayUI;
    private GameObject gameplayMaJiang;

    public GameObject gameplayUIETC;
    public GameObject gamePlayUI_HT;
    private GameObject InputManager;

    public LoginScene login;
    private HomeScene homeScene;
    private GameManagerBaseNet managerBaseNet;
    private HNPrivateScenceBase privateScence;

    PlayerUI[] m_allPlayersUI = new PlayerUI[HNMJ_Defines.GAME_PLAYER];//0 -- East 1 -- sourth 2 -- west 3-- north
    //private Dictionary<int, int> m_PlayerIndex2PlayerUI = new Dictionary<int, int>(HNMJ_Defines.GAME_PLAYER);  
    public static int m_iLocalChairID = -1;

    //mChen add
    public bool m_bIsToHallByDisconnect;
    private GameObject m_offlineWaitingImage;
    public bool m_bIsToHallFrom13Shui;

    private HNMJPlayer[] hnPlayers = new HNMJPlayer[HNMJ_Defines.GAME_PLAYER];
#if NEW_CARDS
    private Color[] preCardColors = new Color[4];
    public enum CardUsageEnum
    {
        Card_Null,
        Card_Hand1,//chairID 0玩家手牌
        Card_Hand2,//chairID 1玩家手牌
        Card_Hand3,//chairID 2玩家手牌
        Card_Hand4,//chairID 3玩家手牌
        Card_Out,
        Card_Chi,
        Card_Peng,
        Card_Gang,
        Card_Hu,
        Card_Max
    };

    public class CardUsage
    {
        public GameObject cardObj;
        public CardUsageEnum usage;//0: null 1: hand card 2: out card 3:chi 4: peng 5:gang

        public CardUsage(GameObject o, CardUsageEnum u)
        {
            cardObj = o;
            usage = u;
        }
    }

    private GameObject lastOutCard;
    private Dictionary<int, List<CardUsage>> cardsAll = new Dictionary<int, List<CardUsage>>(HNMJ_Defines.MAX_REPERTORY);
    //麻将值， peng牌存储起来，  后面可能杠牌 
    private Dictionary<int, GameObject> PengList = new Dictionary<int, GameObject>();
#else
#endif
    //打出牌的最大数量
    const int OutCardNum = 27;
    //打出牌的位置信息
    private Dictionary<int, List<Transform>> outCardTransforms = new Dictionary<int, List<Transform>>();

    private Transform GetRootObject(Transform childObject)
    {
        if (childObject.parent == null)
        {
            return childObject;
        }
        return GetRootObject(childObject.parent);
    }

    public static GameObject SliderL;
    public static GameObject SliderR;
    public static GameObject TextL;
    public static GameObject TextR;
    public static GameObject MapIndex;
    // Use this for initialization
    void Start()
    {
        SliderL = GameObject.Find("CanvasGamePaly_demo/TempTest/SliderL");
        SliderR = GameObject.Find("CanvasGamePaly_demo/TempTest/SliderR");
        TextL = GameObject.Find("CanvasGamePaly_demo/TempTest/TextL");
        TextR = GameObject.Find("CanvasGamePaly_demo/TempTest/TextR");
        MapIndex = GameObject.Find("CanvasGamePaly_demo/TempTest/MapIndex");

        //读取场景物品文本信息
        ItemsListText = (Resources.Load("ItemsList") as TextAsset).text;
        ItemListManager.GetInstance.LoadAndDeserialize();
        listItem = ItemListManager.GetInstance.items.ItemList;
        listTagger = ItemListManager.GetInstance.items.TaggerList;


        //读取昵称库文本信息
        NickNameListText= (Resources.Load("NickNameList") as TextAsset).text;
        NickNameListManager.GetInstance.LoadAndDeserialize();

        for (int i = 0; i < listTagger.Count; i++)
        {
            taggerPrefabFileNames.Add(listTagger[i].FileName);
            taggerPrefabNames.Add(listTagger[i].Name);
            taggerPrefabPath.Add(listTagger[i].Path);
        }

        ControlCase = 0;


        Application.targetFrameRate = 30;

        //mChen add, for HideSeek
        DontDestroyOnLoad(PlayerTeamObj);
        DontDestroyOnLoad(CameraControl);
        if (LockObjOfLoadScene == null)
        {
            LockObjOfLoadScene = new object();
        }

        bWeChatLogonIn = true;
        m_bIsToHallByDisconnect = false;


        if (m_timeDownObjOfDismissRoom != null)
        {
            m_timeDownObjOfDismissRoom.SetActive(false);
        }
        m_timeDownOfDismissRoom = null;

        ///DontDestroyOnLoad(FocusUI);

        DontDestroyOnLoad(this);
        managerBaseNet = GameManagerBaseNet.InstanceBase();
        login = new LoginScene();
        homeScene = new HomeScene();
        privateScence = new HNPrivateScenceBase(this);

        gameplayUI = GameObject.Find("CanvasGamePlay");
        gameplayUIETC = GameObject.Find("CanvasEasyTouchControls");
        gamePlayUI_HT = GameObject.Find("CanvasGamePaly_demo");
        InputManager = GameObject.Find("InputManager");

        ////初始化背景音乐音量
        //gameplayUI.transform.Find("Window/SetupWindow/Music/Slider").GetComponent<Slider>().value = 0.5f;
        //BackgroundAudioSource.volume = 0.5f;

        for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
        {
            Transform trans = gameplayUI.transform.Find("Users/UserBack0" + (i + 1) + "/User/Score");
            if (trans != null)
            {
                MyUserScore[i] = trans.gameObject;
            }
            else
            {
                MyUserScore[i] = null;
            }
            ///MyUserScore[i]=gameplayUI.transform.Find("Users/UserBack0" + (i + 1) + "/User/Score").gameObject;
        }

        //gameplayUI.SetActive(false);
        DontDestroyOnLoad(gameplayUI);
        DontDestroyOnLoad(gameplayUIETC);
        DontDestroyOnLoad(gamePlayUI_HT);
        DontDestroyOnLoad(InputManager);

        RoomId = gameplayUI.transform.Find("RoomInfo/RoomId").GetComponent<Text>();
        LeftCardLabel = gameplayUI.transform.Find("RoomInfo/LeftCard").GetComponent<Text>();
        PlayerCountTxt = gameplayUI.transform.Find("RoomInfo/PlayerCount").GetComponent<Text>();
        LeftCardLabel.text = LeftCardNum.ToString();
        PlayCoutLabel = gameplayUI.transform.Find("RoomInfo/BoardNumber").GetComponent<Text>();
        //gameplayMaJiang = GameObject.Find("mahjongTable");
        BaseScoreTxt = gameplayUI.transform.Find("RoomInfo/BottomMark").GetComponent<Text>();

        m_offlineWaitingImage = gameplayUI.transform.Find("WaitingImage_Table").gameObject;

        for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
        {
            m_allPlayersUI[i].CardNode = new Transform[HNMJ_Defines.MAX_COUNT];
            for (int j = 0; j < HNMJ_Defines.MAX_COUNT; j++)
            {
                if (m_allPlayersUI[i].CardNodeRoot != null)
                {
                    m_allPlayersUI[i].CardNode[j] = m_allPlayersUI[i].CardNodeRoot.transform.Find("Dragon_Blank" + j);
                    m_allPlayersUI[i].CardNode[j].gameObject.SetActive(false);
                }
                else
                {
                    m_allPlayersUI[i].CardNode[j] = null;
                }
            }

            m_allPlayersUI[i].CardCpgList = new List<GameObject>();
            for (int k = 1; k <= HNMJ_Defines.MAX_WEAVE; k++)
            {
                if (m_allPlayersUI[i].CardNodeRoot == null)
                {
                    continue;
                }

                GameObject cpgGameObj = m_allPlayersUI[i].CardNodeRoot.transform.Find("cpg" + k).gameObject;
                for (int j = 0; j < 4; j++)
                {
                    cpgGameObj.transform.Find("Dragon_Blank" + j).gameObject.SetActive(false);
                }
                m_allPlayersUI[i].CardCpgList.Add(cpgGameObj);
            }

            outCardTransforms.Add(i, new List<Transform>(OutCardNum));
            for (int j = 0; j < OutCardNum; j++)
            {
                if (m_allPlayersUI[i].CardNodeRoot == null)
                {
                    continue;
                }

                outCardTransforms[i].Add(m_allPlayersUI[i].CardNodeRoot.transform.Find("out/Dragon_Blank" + j));
                outCardTransforms[i][j].gameObject.SetActive(false);
            }

            //m_allPlayersUI[i].FlowerNode = m_allPlayersUI[i].CardNodeRoot.transform.Find("flower").gameObject;
        }

        EmojiUI = EmoJiAndLanUI.transform.Find("ExpressionScrollView").gameObject;
        LansUI = EmoJiAndLanUI.transform.Find("WordsScrollView").gameObject;

        SpeakingUI.SetActive(false);

        m_breakLineWindow = gameplayUI.transform.Find("Window/BreakLineWindow").gameObject;

        //m_readyButton = gameplayUI.transform.Find("ReadyButton").gameObject;
        m_readyButton = gamePlayUI_HT.transform.Find("ReadyButton").gameObject;

        m_redForeground = gameplayUI.transform.Find("ImageRed").gameObject;

        //Font
        m_fontOrange = Resources.Load<Font>("Fonts/MyFont_orange");
        m_fontBlue = Resources.Load<Font>("Fonts/MyFont_blue");

        m_soundPlace = AudioManager.Sound_Place.MeiCheng;

        //Exp
        Exp = gameplayUI.transform.Find("Experience").gameObject;
        //Loading
        Loading = gamePlayUI_HT.transform.Find("Loading").gameObject;
        //ChatSystem
        chatSystem = GameObject.FindObjectOfType<ChatSystem>();


#if UNITY_ANDROID || UNITY_IOS

        //设置目标FPS
        Application.targetFrameRate = 30;

        //关闭垂直同步
        QualitySettings.vSyncCount = 0;

        //关闭抗锯齿
        QualitySettings.antiAliasing = 0;
        
        //关闭实时阴影
        QualitySettings.shadows = ShadowQuality.Disable;

        //防止锁屏
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //Debug.logger.logEnabled = false;//发布版移除log
#endif

#if UNITY_IPHONE
		//requestProduct();
#endif

    }



    //Sound
    private void ResetVolume()
    {
        if (BackgroundAudioSource != null)
        {
            BackgroundAudioSource.volume = m_musicVolume;
        }

        for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
        {
            if (m_allPlayersUI[i].Audio != null)
            {
                m_allPlayersUI[i].Audio.volume = m_soundEffectVolume;
            }
        }

        if (GlobalEffectAudioSource != null)
        {
            GlobalEffectAudioSource.volume = m_soundEffectVolume;
        }
    }
    public void OnMusicVolumeChange(Slider slider)
    {
        m_musicVolume = slider.value;
        gameplayUI.transform.Find("Window/SetupWindow/Music/Slider").GetComponent<Slider>().value = slider.value;  //13水和麻将的背景音乐条同步
        ///AudioSource bgMusic = gameObject.GetComponent<AudioSource>();
        if (BackgroundAudioSource != null)
        {
            BackgroundAudioSource.volume = m_musicVolume;
        }
    }
    public void OnSoundEffectVolumeChange(Slider slider)
    {
        m_soundEffectVolume = slider.value;

        for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
        {
            if (m_allPlayersUI[i].Audio != null)
            {
                m_allPlayersUI[i].Audio.volume = m_soundEffectVolume;
            }
        }

        if (GlobalEffectAudioSource != null)
        {
            GlobalEffectAudioSource.volume = m_soundEffectVolume;
        }

        if (HuSoundClipAudioSource != null)
        {
            HuSoundClipAudioSource.volume = m_soundEffectVolume;
        }
    }
    public void OnSoundPlaceChange1(Toggle toggle)
    {
        if (toggle.isOn)
        {
            m_soundPlace = AudioManager.Sound_Place.DaTong;
        }
    }
    public void OnSoundPlaceChange2(Toggle toggle)
    {
        if (toggle.isOn)
        {
            m_soundPlace = AudioManager.Sound_Place.MeiCheng;
        }
    }
    public void OnSoundPlaceChange3(Toggle toggle)
    {
        if (toggle.isOn)
        {
            m_soundPlace = AudioManager.Sound_Place.NanFeng;
        }
    }
    public void OnSoundPlaceChange4(Toggle toggle)
    {
        if (toggle.isOn)
        {
            m_soundPlace = AudioManager.Sound_Place.ShouCang;
        }
    }

    public GameObject getGamePlayUIRoot()
    {
        return gameplayUI;
    }

    public GameObject getGamePlayMaJiangRoot()
    {
        return gameplayMaJiang;
    }

    private int getChairIdFromIndex(int userIndex)
    {
        int iChairId = -1;
        if (m_iLocalChairID >= 0)
        {
            iChairId = (m_iLocalChairID - userIndex + HNMJ_Defines.GAME_PLAYER) % HNMJ_Defines.GAME_PLAYER;
        }
        return iChairId;

        //if (m_PlayerIndex2PlayerUI.Count != 4) //if (m_PlayerIndex2PlayerUI.Count!= HNMJ_Defines.GAME_PLAYER)
        //{
        //    return -1;
        //}
        //return m_PlayerIndex2PlayerUI[userIndex];
    }

    IEnumerator HeaderImageLoadAndShow(int chairID, string headURL)
    {
        //lin: 暂时全部使用自己的头像
        ///WWW www = new WWW(LoginScene.m_headURL);
        ///
        //Debug.Log("mChen HeaderImageLoadAndShow1:headURL=" + headURL);

        WWW www = new WWW(headURL);
        yield return www;

        Texture2D tex = www.texture;

        m_allPlayersUI[chairID].SeatNode.transform.Find("User").GetComponent<Image>().sprite = Sprite.Create(
            tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        HeadImage[chairID] = m_allPlayersUI[chairID].SeatNode.transform.Find("User").GetComponent<Image>().sprite;
        Debug.Log("head loaded");
    }
    //更新玩家头像旁边的分数显示

    public void UpDatePlayerScore()
    {
        Loom.QueueOnMainThread(() =>
        {
            for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
            {
                if (GameType == HNPrivateScenceBase.GAME_TYPE_JianDe)
                {
                    //m_allPlayersUI[index].SeatNode.transform.Find("User/Score").GetComponent<Text>().text = "当前分数:" + TotalScore_MJ[i];
                    MyUserScore[i].GetComponent<Text>().text = "当前分数:" + TotalScore_MJ[(i + m_iLocalChairID) % 4];

                }
            }
        });

        //========================================
    }
    public void ResetPlayerCount()
    {
        m_PlayerCount = 4;
    }
    public void ZeroPlayerScore()
    {
        Loom.QueueOnMainThread(() =>
        {
            for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
            {
                Transform trans = gameplayUI.transform.Find("Users/UserBack0" + (i + 1) + "/User/Score");
                if (trans == null)
                {
                    continue;
                }

                gameplayUI.transform.Find("Users/UserBack0" + (i + 1) + "/User/Score").GetComponent<Text>().text = "当前分数:0";
            }
        });
    }
    public void UpdateUserInfo(int userIndex, string name, string id, int gender, string headHttp)
    {
        var iChairId = getChairIdFromIndex(userIndex);
        if (iChairId == -1)
        {
            return;
        }
        Debug.Log("UpdateUserInfo " + name);

        if (m_allPlayersUI[iChairId].SeatNode != null)
        {
            if (name == "" && id == "0")
            {
                m_allPlayersUI[iChairId].SeatNode.SetActive(false);
            }
            else
            {
                m_allPlayersUI[iChairId].SeatNode.SetActive(true);
            }
            ///m_allPlayersUI[iChairId].SeatNode.SetActive(true);
            m_allPlayersUI[iChairId].SeatNode.transform.Find("User/Name").GetComponent<Text>().text = name;
            m_allPlayersUI[iChairId].SeatNode.transform.Find("User/ID").GetComponent<Text>().text = id;
            m_allPlayersUI[iChairId].SeatNode.transform.Find("User/Sex").GetComponent<Image>().sprite = GenderSprites[gender % 2];//0 nv 2 nan
        }

        if (hnPlayers[iChairId] == null)
        {
            Debug.Log(" Load headImage " + headHttp);

            if (CServerItem.get() != null)
            {
                var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                if (kernel == null)
                    return;
                hnPlayers[iChairId] = kernel.getPlayerByChairID(iChairId);
            }
#if (UNITY_IPHONE || UNITY_ANDROID)  && !UNITY_EDITOR
            ///StartCoroutine(HeaderImageLoadAndShow(iChairId, headHttp));
#endif
        }
    }

    public void LocalUserEnter(int localChairID)
    {
        Debug.Log("User: enter " + localChairID);
        if (!playAgain)
            UIManager.GetInstance().ShowManualControlTips("等待房主开始游戏···", true);
        m_iLocalChairID = localChairID;

        if (HNGameManager.bFakeServer == false)
        {
            StartOrStopGameSceneHeartBeat(true);
        }

        for (int iChairId = 0; iChairId < HNMJ_Defines.GAME_PLAYER; iChairId++)
        {
            int iUserIdex = (m_iLocalChairID - iChairId + HNMJ_Defines.GAME_PLAYER) % HNMJ_Defines.GAME_PLAYER;
            int iUserBackIdex = (HNMJ_Defines.GAME_PLAYER - iUserIdex) % HNMJ_Defines.GAME_PLAYER + 1;
            String sUIName = String.Format("Users/UserBack0{0}", iUserBackIdex);

            Transform trans = gameplayUI.transform.FindChild(sUIName);
            if (trans == null)
            {
                m_allPlayersUI[iChairId].SeatNode = null;
                continue;
            }

            m_allPlayersUI[iChairId].SeatNode = gameplayUI.transform.FindChild(sUIName).gameObject;


            float localEulerAngles = ((HNMJ_Defines.GAME_PLAYER - localChairID) % HNMJ_Defines.GAME_PLAYER) * 90;
            //getGamePlayMaJiangRoot().transform.localEulerAngles = new Vector3(0, localEulerAngles, 0);
            //getGamePlayMaJiangRoot().transform.Rotate(0, localEulerAngles, 0);

            m_allPlayersUI[iChairId].ExpUVAnimTexture = m_allPlayersUI[iChairId].SeatNode.transform.Find("User/Exp").GetComponent<ExpAnimation>();
            m_allPlayersUI[iChairId].LansObj = m_allPlayersUI[iChairId].SeatNode.transform.Find("User/Word").gameObject;
            m_allPlayersUI[iChairId].VoiceUI = m_allPlayersUI[iChairId].SeatNode.transform.Find("User/Voices").gameObject;
            m_allPlayersUI[iChairId].CurHightLight = m_allPlayersUI[iChairId].SeatNode.transform.Find("HighEffect").gameObject;
            m_allPlayersUI[iChairId].Audio = m_allPlayersUI[iChairId].SeatNode.GetComponent<AudioSource>();

            //mChen add, fix离线标志位置不对的bug
            m_allPlayersUI[iChairId].m_breakBackObj = m_allPlayersUI[iChairId].SeatNode.transform.Find("User/BreakBack").gameObject;
            m_allPlayersUI[iChairId].m_playerReadyObj = m_allPlayersUI[iChairId].SeatNode.transform.Find("User/ReadyBack").gameObject;
            m_allPlayersUI[iChairId].m_huScoreObj = m_allPlayersUI[iChairId].SeatNode.transform.Find("User/HuScore").gameObject;
            m_allPlayersUI[iChairId].m_huInfoObj = m_allPlayersUI[iChairId].SeatNode.transform.Find("User/HuInfo").gameObject;
        }

        //断线重连回来，“下一局”按钮
        if (CServerItem.get() != null)
        {
            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
            if (kernel == null)
                return;
            var localPlayer = kernel.getPlayerByChairID(m_iLocalChairID);
            if (localPlayer != null)
            {
                uint nRoomPlayCout = GetRoomPlayCout();
                int nStatus = localPlayer.GetUserStatus();
                if (nRoomPlayCout > 0 && nStatus == SocketDefines.US_SIT)
                {
                    var smallFinal = FinalUI.transform.Find("SmallFinal");
                    if (smallFinal != null)
                    {
                        if (!smallFinal.gameObject.activeSelf)
                        {
                            ShowFinalLabel(true);
                        }
                    }
                }
            }
        }
        localEnter = true;  //标记localHuman已进入游戏，供断线重连使用
    }

    IEnumerator TimeCountDown(Material timeMat)
    {
        Debug.Log("Start Time Count down!!!-----------------");
        float timePassed = 0.0f;
        while (timePassed < 16.0f)
        {
            timeMat.SetFloat("_AnimaTime", timePassed);
            yield return new WaitForSeconds(1.0f);
            timePassed += 1.0f;
        }
        Debug.Log("TimeCountDown Time Count down!!!-----------------");
    }

    private Coroutine timeDown;
    private static string[] ModeStr = { "_EMMODE_EAST", "_EMMODE_SOUTH", "_EMMODE_WEST", "_EMMODE_NORTH", "_EMMODE_NONE" };
    private static string[] ArrowStr =
    {
        "_ARROWDIR_NONE", "_ARROWDIR_RIGHT", "_ARROWDIR_UP", "_ARROWDIR_LEFT"
    };

    public void setCurrentPlayer(int chairID)
    {
        foreach (var playerUi in m_allPlayersUI)
        {
            if (playerUi.CurHightLight != null)
            {
                playerUi.CurHightLight.SetActive(false);
            }
        }
        if (m_allPlayersUI[chairID].CurHightLight != null)
        {
            m_allPlayersUI[chairID].CurHightLight.SetActive(true);
        }

        if (chairID == m_iLocalChairID)
        {
            bSendingCards = false;
        }

        for (int i = 0; i < 5; i++)
        {
            if (i == chairID)
            {
                ThreeDLight.material.EnableKeyword(ModeStr[i]);
            }
            else
            {
                ThreeDLight.material.DisableKeyword(ModeStr[i]);
            }
        }

    }

    public GameObject getUserSeatRoot(int chairID)
    {
        return m_allPlayersUI[chairID].SeatNode;
    }

    public GameObject getUserCardRoot(int chairID)
    {
        return m_allPlayersUI[chairID].CardNodeRoot;
    }

    //dataIndex -2: null ||    -1 ：out内找 || 0 - max : 从 hand  里面找 (chairID标示 hand usage)
    public GameObject getOutCard(int cardValue, int dataIndex, int chairID, CardUsageEnum usage = CardUsageEnum.Card_Out)
    {
        return null;
    }

    public void SetOutCardObj(int chairID, int cardIndex, GameObject obj)
    {
    }

    //重置手牌中麻将的数量
    public void ResetCardCountMap(int chairID)
    {
        for (int i = 0; i < HNMJ_Defines.MAX_COUNT; i++)
        {
            AllCurrentCards[chairID, i] = null;
        }

    }

    //隐藏其他玩家空白手牌
    public void HideOtherPlayersCard()
    {
        foreach (var mPlayer in m_allPlayersUI)
        {
            foreach (var transform1 in mPlayer.CardNode)
            {
                transform1.gameObject.SetActive(false);
            }
        }
    }

    public GameObject getHandCardObj(int cardInt, int dataIndex, int chairID)
    {
        foreach (var cardUsage in cardsAll[cardInt])
        {
            if (cardUsage.usage == CardUsageEnum.Card_Null)
            {
                cardUsage.usage = (CardUsageEnum)(chairID + 1);//hand card
                cardUsage.cardObj.tag = "Index" + dataIndex;
                cardUsage.cardObj.layer = (chairID == m_iLocalChairID ? 8 : 0);
                cardUsage.cardObj.SetActive(true);
                return cardUsage.cardObj;
            }
        }
        Debug.Assert(false,
            "Should not come here " + cardInt + " data index" + dataIndex + " " + cardsAll[cardInt][0].usage +
            cardsAll[cardInt][1].usage + cardsAll[cardInt][2].usage + cardsAll[cardInt][3].usage);
        return null;
    }
    static Color caiShenColor = new Color(0.98f, 0.86f, 0.37f);
    public GameObject setPlayerCardObj(int chairID, int dataIndex, int cardIndex, int cardValue, bool bShowCard = false)
    {
#if NEW_CARDS
        GameObject handCard;
        if (bShowCard)
        {
            var rotation = m_allPlayersUI[chairID].CardNode[cardIndex].rotation;
            rotation = rotation * Quaternion.Euler(new Vector3(-90, 0, 0));
            var newPos = m_allPlayersUI[chairID].CardNode[cardIndex].position;
            newPos.y -= 0.0145f;
            handCard = getHandCardObj(cardValue, dataIndex, chairID);
            handCard.transform.position = newPos;
            handCard.transform.rotation = rotation;
            ///handCard.transform.SetPositionAndRotation(newPos, rotation);
            handCard.layer = 0;
        }
        else
        {
            handCard = getHandCardObj(cardValue, dataIndex, chairID);
            if (bFakeServer == false)
            {
                handCard.transform.position = m_allPlayersUI[chairID].CardNode[cardIndex].position;
                handCard.transform.rotation = m_allPlayersUI[chairID].CardNode[cardIndex].rotation;
                ///handCard.transform.SetPositionAndRotation(m_allPlayersUI[chairID].CardNode[cardIndex].position,m_allPlayersUI[chairID].CardNode[cardIndex].rotation);
                if (cardValue == 47)//财神
                {
                    handCard.GetComponent<Renderer>().material.SetColor("_MainColor", caiShenColor);
                }
            }
            else
            {
                handCard.transform.position = m_allPlayersUI[chairID].CardNode[cardIndex].position;
                handCard.transform.rotation = m_allPlayersUI[chairID].CardNode[cardIndex].rotation;
                ///handCard.transform.SetPositionAndRotation( m_allPlayersUI[chairID].CardNode[cardIndex].position, m_allPlayersUI[chairID].CardNode[cardIndex].rotation);
                if (chairID != m_iLocalChairID)
                {
                    switch (chairID)
                    {
                        case 0:
                            handCard.transform.localEulerAngles = new Vector3(0, 180, 0);
                            break;
                        case 1:
                            handCard.transform.localEulerAngles = new Vector3(0, -90, 0);
                            break;
                        case 2:
                            handCard.transform.localEulerAngles = new Vector3(0, 0, 0);
                            break;
                        case 3:

                            handCard.transform.localEulerAngles = new Vector3(0, 90, 0);
                            break;
                    }
                    if (cardValue == 47)//财神
                    {
                        handCard.GetComponent<Renderer>().material.SetColor("_MainColor", Color.white);
                    }
                }
                else
                {
                    if (cardValue == 47)//财神
                    {
                        handCard.GetComponent<Renderer>().material.SetColor("_MainColor", caiShenColor);
                    }
                }
            }
        }

        AllCurrentCards[chairID, dataIndex] = handCard.transform;
        ///Debug.Log("dataIndex cards" + dataIndex);

        return handCard;
#else
#endif
    }
    public void ShowSetting()
    {
        if (GameType == HNPrivateScenceBase.GAME_TYPE_JianDe)
        {
            gameplayUI.transform.Find("Window/SetupWindow").gameObject.SetActive(true);
        }
    }

    //mChen add, for解散房间倒计时
    public void StartTimeDownOfDismissRoom()
    {
        if (m_timeDownObjOfDismissRoom == null)
        {
            return;
        }

        m_timeDownObjOfDismissRoom.SetActive(true);

        if (m_timeDownOfDismissRoom != null)
        {
            Debug.Log("Stop TimeDownOfDismissRoom-----------------");
            StopCoroutine(m_timeDownOfDismissRoom);
        }

        m_timeDownOfDismissRoom = StartCoroutine(TimeDownOfDismissRoom());
    }
    public void StopTimeDownOfDismissRoom()
    {
        if (m_timeDownObjOfDismissRoom != null)
        {
            m_timeDownObjOfDismissRoom.SetActive(false);
        }
        if (m_timeDownOfDismissRoom != null)
        {
            StopCoroutine(m_timeDownOfDismissRoom);
        }
    }
    IEnumerator TimeDownOfDismissRoom()
    {
        Debug.Log("Start TimeDownOfDismissRoom-----------------");

        if (m_timeDownObjOfDismissRoom != null)
        {
            int timePassed = 0;
            int waitTime = 30;
            while (timePassed < 30)
            {
                int timeToShow = waitTime - timePassed;
                m_timeDownObjOfDismissRoom.GetComponent<Text>().text = timeToShow.ToString();
                yield return new WaitForSeconds(1.0f);
                timePassed++;
            }

            m_timeDownObjOfDismissRoom.SetActive(false);
        }

        Debug.Log("End TimeDownOfDismissRoom-----------------");
    }


    //根据玩家人数显隐公共部分UI
    public void ShowHideUIByPlayerCount_Common()
    {
        gameplayUI.transform.Find("Window/Finals/BigFinal/FinalBackImage4").gameObject.SetActive(true);
        gameplayUI.transform.Find("Window/Finals/BigFinal/FinalBackImage3").gameObject.SetActive(true);
        gameplayUI.transform.Find("Window/Finals/BigFinal/FinalBackImage1").gameObject.SetActive(true);

        bool bShow = true;
        if (m_PlayerCount == 3)
        {
            bShow = false;
        }
        if (m_iLocalChairID == 0)
        {
            gameplayUI.transform.Find("Window/Finals/BigFinal/FinalBackImage4").gameObject.SetActive(bShow);
        }
        else if (m_iLocalChairID == 1)
        {
            gameplayUI.transform.Find("Window/Finals/BigFinal/FinalBackImage3").gameObject.SetActive(bShow);
        }
        else if (m_iLocalChairID == 2)
        {
            gameplayUI.transform.Find("Window/Finals/BigFinal/FinalBackImage1").gameObject.SetActive(bShow);
        }

    }

    public void LogOn()
    {
        //Debug.Log("HHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH");
        if (login == null)
            login = new LoginScene();
        login.LogOn();
        ///UserInfo.getInstance().checkInGameServer();
	}

    public void CreateRoom()
    {
        //Debug.Log("創建麻將房間");
        if (GameType == HNPrivateScenceBase.GAME_TYPE_Null || GameType == HNPrivateScenceBase.GAME_TYPE_JianDe)
        {
            Debug.Log("創建麻将房間,改变GameType");
            GameType = HNPrivateScenceBase.GAME_TYPE_JianDe;

            privateScence.Button_CreateRoom();
        }
    }

    public void JoinRoom(int iRoomId)
    {
        //ZY add
        if (GameType == HNPrivateScenceBase.GAME_TYPE_Null || GameType == HNPrivateScenceBase.GAME_TYPE_JianDe)
        {
            Debug.Log("加入mj房間,改变GameType");
            GameType = HNPrivateScenceBase.GAME_TYPE_JianDe;

            privateScence.Button_JoinRoom(iRoomId);
        }
    }

    public void JoinRace_MJ()
    {
        if (GameType == HNPrivateScenceBase.GAME_TYPE_Null || GameType == HNPrivateScenceBase.GAME_TYPE_JianDe)
        {
            Debug.Log("加入麻将比赛,改变GameType");
            GameType = HNPrivateScenceBase.GAME_TYPE_JianDe;
            privateScence.Button_JoinRace();
        }
    }

    //mChen add, for HideSeek
    public void LoadHideSeekSceneOfWangHu()
    {
        //Log
        if (!bEnteredGameScene)
        {
            Debug.Log("LoadHideSeekSceneOfWangHu");
        }
        else
        {
            Debug.LogError("LoadHideSeekSceneOfWangHu error for bEnteredGameScene = true");
        }

        //if (!bEnteredGameScene)
        {
            lock (HNGameManager.LockObjOfLoadScene)
            {
                GameManager.s_gameSingleMultiType = GameSingleMultiType.MultiGame_WangHu;
                //进入游戏场景显示CanvasEasyTouchControls和CanvasGamePaly_demo
                gameplayUIETC.SetActive(true);
                gamePlayUI_HT.SetActive(true);
                SetLoading(true);

                //SceneManager.LoadScene("mainScene");// mainScene demo_01 
                GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
                tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
                GameObjectsManager.GetInstance().LoadMap(pGlobalUserData.cbMapIndexRand);

                EnterGamePlayScene();
            }
        }
    }
    public void LoadSingGame()
    {
        GameType = HNPrivateScenceBase.GAME_TYPE_JianDe;

        GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
        tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

        GameManager.s_gameSingleMultiType = GameSingleMultiType.SingleGame;

        //MersenneTwister.MT19937.Seed((ulong)DateTime.Now.Ticks);
        pGlobalUserData.cbMapIndexRandForSingleGame = (byte)UnityEngine.Random.Range(0, 255); //(byte)(MersenneTwister.MT19937.Int63() % 255);
        //进入游戏场景显示CanvasEasyTouchControls和CanvasGamePaly_demo
        gameplayUIETC.SetActive(true);
        gamePlayUI_HT.SetActive(true);
        SetLoading(true);

        //SceneManager.LoadScene("mainScene");
        GameObjectsManager.GetInstance().LoadMap(pGlobalUserData.cbMapIndexRandForSingleGame); 
    }

    public void ShowEmojiOrLan(bool bEmoji)
    {
        EmoJiAndLanUI.SetActive(true);
        if (bEmoji)
        {
            EmojiUI.SetActive(true);
            EmoJiAndLanUI.transform.Find("Title/ExpressionToggle").GetComponent<Toggle>().isOn = true;
            LansUI.SetActive(false);
            EmoJiAndLanUI.transform.Find("Title/WordToggle").GetComponent<Toggle>().isOn = false;
        }
        else
        {
            EmojiUI.SetActive(false);
            EmoJiAndLanUI.transform.Find("Title/ExpressionToggle").GetComponent<Toggle>().isOn = false;
            LansUI.SetActive(true);
            EmoJiAndLanUI.transform.Find("Title/WordToggle").GetComponent<Toggle>().isOn = true;
        }
    }

    public void HideEmojiOrLan()
    {
        EmoJiAndLanUI.SetActive(false);
    }

    public void EnterGamePlayScene()
    {
        //表情初始化
        EmojiUI = EmoJiAndLanUI.transform.Find("ExpressionScrollView").gameObject;
        LansUI = EmoJiAndLanUI.transform.Find("WordsScrollView").gameObject;
        LansUI.SetActive(false);
        EmojiUI.SetActive(true);

        //Offline
        if (m_breakLineWindow != null)
        {
            m_breakLineWindow.SetActive(false);
        }
        for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
        {
            ShowOfflineUI(i, false);
            ShowPlayerReadyUI(i, false);
        }

        gameSceneCamera = null;
        gameplayUI.SetActive(true);

        bEnteredGameScene = true;
        //gameplayMaJiang.GetComponent<Animation>().enabled = false;
        SetRoomUI(true);
        //m_cbGameEndReason = HNMJ_Defines.GER_NOT_END;
        //if(bFakeServer == false)
        //    StartOrStopGameSceneHeartBeat(true);
    }

    public void SetRoomInfo(ref CMD_GF_Private_Room_Info roomInfo)
    {
        RoomId.text = roomInfo.dwRoomNum.ToString();
        PlayCoutLabel.text = (roomInfo.dwPlayCout + " / " + roomInfo.dwPlayTotal);

        m_PlayerCount = roomInfo.PlayerCount;
        PlayerCountTxt.text = m_PlayerCount + "人";

        m_baseScore = roomInfo.lBaseScore;
        BaseScoreTxt.text = m_baseScore.ToString();
        CreateUserID = roomInfo.dwCreateUserID;
        m_bRoomStartGame = (roomInfo.bStartGame != 0);

        //ZY add 根据人数显隐UI
        ShowHideUIByPlayerCount_Common();

        //林： 显示或隐藏邀请按钮
        gameplayUI.transform.Find("InviteButton")
            .gameObject.SetActive(roomInfo.bStartGame == 0 && PlayerPrefs.GetInt("PubOrPrivate", (int)RoomType.Type_Private) == (int)RoomType.Type_Private &&
                                  (hnPlayers[m_iLocalChairID].GetUserID() == roomInfo.dwCreateUserID));
    }

    public bool IsMeRoomCreator()
    {
        bool bIsMeRoomCreator = false;
        if (m_iLocalChairID >= 0 && hnPlayers[m_iLocalChairID]!=null)
        {
            bIsMeRoomCreator = (hnPlayers[m_iLocalChairID].GetUserID() == CreateUserID);
        }

        return bIsMeRoomCreator;
    }
    public void ShowReadyButton(bool bShow)
    {
        if (m_readyButton != null)
        {
            m_readyButton.SetActive(bShow);
        }
    }

    public void HideFocus()
    {
        return;

        FocusUI.SetActive(false);
    }

    public void ShowFocus(Vector3 pos)
    {
        return;

        FocusUI.transform.position = pos;
        FocusUI.SetActive(true);
    }

    public void SetOtherPlayersCard(int chairID, int cardStartIndex, int iHandcardCount, bool bNewCardIn)
    {
        Debug.Log("User: " + " trueidex : " + chairID + " count: " + iHandcardCount);
        int count = iHandcardCount;
        if (bNewCardIn)
        {
            count -= 1;
        }

        for (int i = 0; i < HNMJ_Defines.MAX_COUNT; i++)
        {
            m_allPlayersUI[chairID].CardNode[i].gameObject.SetActive(false);
        }

        for (int i = cardStartIndex; i < cardStartIndex + count; i++)
        {
            m_allPlayersUI[chairID].CardNode[i].gameObject.SetActive(true);
        }

        if (bNewCardIn)
        {
            m_allPlayersUI[chairID].CardNode[HNMJ_Defines.MAX_COUNT - 1].gameObject.SetActive(true);
        }
    }


    private int LeftCardNum = 136;

    public void SetLeftCard(int cardNum, bool bReconnect = false, bool bGameEndLeftCards = false)
    {
    }

    private Coroutine LeftTipsCoroutine = null;

    IEnumerator showAndHideLeftTips()
    {
        //Sound
        //PlaySoundEffect(0, (int)AudioManager.Sound_Effect_Defines.SOUND_EFFECT_Left26Cards);

        LeftTipsObj.SetActive(true);
        yield return new WaitForSecondsRealtime(2.0f);
        LeftTipsObj.SetActive(false);
        LeftTipsCoroutine = null;
    }

    public void DismissBtnClicked()
    {
        privateScence.Button_DismissPrivate();
    }

    public byte m_cbGameEndReason = HNMJ_Defines.GER_NOT_END;

    public uint GetRoomPlayCout()
    {
        uint uRoomPlayCout = 0;
        if (privateScence != null)
        {
            uRoomPlayCout = privateScence.GetRoomPlayCout();
        }

        return uRoomPlayCout;
    }

    public void PopDismissWindow(uint dwPlayCout)
    {
        if (dwPlayCout == 0)
        {
            var panel = gameplayUI.transform.Find("Window/DismissPanel");
            Text text = panel.Find("PromptText").GetComponent<Text>();
            text.text = "解散房间不扣房卡，是否解散？";

            var buttonOk = panel.Find("ButtonOk");
            buttonOk.gameObject.SetActive(true);

            panel.gameObject.SetActive(true);

            Debug.Log("PopDismissWindow==========Setbull");
        }
        else
        {
            //if (PlayerPrefs.GetInt("PubOrPrivate") == (int)RoomType.Type_Public)
            //{
            //    var panel = gameplayUI.transform.Find("Window/DismissPanel");
            //    Text text = panel.Find("Text").GetComponent<Text>();
            //    text.text = "比赛模式不允许解散房间！";

            //    var buttonOk = panel.Find("ButtonOk");
            //    buttonOk.gameObject.SetActive(false);

            //    panel.gameObject.SetActive(true);
            //    return;
            //}

            var panelWin = gameplayUI.transform.Find("Window/DisbandWindow");
            panelWin.gameObject.SetActive(true);

            StartTimeDownOfDismissRoom();

            privateScence.Button_SureeDismissPrivate();
        }
    }

    public void PopLeaveRoomWindow()
    {
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if(PlayerPrefs.GetInt("PubOrPrivate") == (int)RoomType.Type_Private && localHuman != null && !localHuman.IsDead() && !IsMeRoomCreator())
        {
            var panel = gameplayUI.transform.Find("Window/LeaveRoomPanel");
            Text text = panel.Find("PromptText").GetComponent<Text>();
            text.text = "退出房间会扣除50金币，是否确定退出？";
            panel.gameObject.SetActive(true);
        }
        else
        {
            LeaveRoom();
        }
    }
    public void LeaveRoom()
    {
        //if (PlayerPrefs.GetInt("PubOrPrivate") == (int)RoomType.Type_Public)//比赛  
        {
            if (CServerItem.get() != null)
            {
                var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                if (kernel != null)
                {
                    m_cbGameEndReason = HNMJ_Defines.GER_USER_LEAVE;
                    kernel.ExitGameBase();
                }

                //mChen add, for HideSeek
                CServerItem.get().IntermitConnect(true);
            }

            //mChen add, for HideSeek:强制离开,fix有人在团灭前断线，重连回来（已经下一局）点离开房间按钮没反应
            LeaveGameToHall();
        }
    }

    public void DismissPrivate()
    {
        //if (ServerType != SocketDefines.GAME_GENRE_MATCH)
        {
            privateScence.Button_SureeDismissPrivate();
        }

        var panel = gameplayUI.transform.Find("Window/DismissPanel");
        panel.gameObject.SetActive(false);
    }

    public void DismissPrivateNot()
    {
        privateScence.Button_DismissPrivateNot();
        //var panel = gameplayUI.transform.Find("Window/DismissPanel");
        //panel.gameObject.SetActive(false);
    }

    public void OnSocketSubPrivateDismissInfo(CMD_GF_Private_Dismiss_Info pNetInfo)
    {
        if (CServerItem.get() == null)
        {
            return;
        }

        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        if (kernel == null)
        {
            return;
        }

        //lin: 第一局未开始，任意一个即可解散房间，无需显示解散界面
        if (kernel.getPlayCount() < 1)
        {
            GameType = HNPrivateScenceBase.GAME_TYPE_Null;
            return;
        }
        if (kernel.getPlayCount() == 1)
        {
            GameType = HNPrivateScenceBase.GAME_TYPE_Null;
        }

        var panel = gameplayUI.transform.Find("Window/DisbandWindow");
        if (pNetInfo.dwDissUserCout == 0)
        {
            panel.gameObject.SetActive(false);
            Debug.Log("OnSocketSubPrivateDismissInfo1");

            StopTimeDownOfDismissRoom();

            return;
        }
        else
        {
            if (panel.gameObject.active == false)
            {
                panel.gameObject.SetActive(true);

                StartTimeDownOfDismissRoom();
            }
        }
        Debug.Log("OnSocketSubPrivateDismissInfo2");
        int[] kChairID = new int[HNMJ_Defines.GAME_PLAYER];// { 1, 1, 1, 1 };
        for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
        {
            kChairID[i] = 1;
        }
        bool bShowSelfAction = true;
        int iIdex = 0;
        String kName = "";

        HNMJPlayer pLocalPlayer = kernel.getLocalPlayer();
        HNMJPlayer pPlayer = null;
        uint nChairId = 0;

        Sprite agreeSprite = Resources.Load<Sprite>("Disband/Agree");
        Sprite refuseSprite = Resources.Load<Sprite>("Disband/Refuse");
        Sprite waitingSprite = Resources.Load<Sprite>("Disband/Waiting");

        //Dissmiss Users
        for (int i = 0; i < (int)pNetInfo.dwDissUserCout; i++)
        {
            nChairId = pNetInfo.dwDissChairID[i];
            pPlayer = hnPlayers[nChairId];
            kChairID[pNetInfo.dwDissChairID[i]] = 0;
            if (pPlayer != null)
            {
                kName = pPlayer.GetNickName();
            }
            if (pPlayer == pLocalPlayer)
            {
                bShowSelfAction = false;
            }

            if (i == 0)
            {
                var userNameOfDismiss = panel.Find("DisbandText/UserName");
                userNameOfDismiss.GetComponent<Text>().text = kName;
            }

            String sNameUserImage = String.Format("UserInfos/UserImage{0}", iIdex + 1);
            var userImagePanel = panel.Find(sNameUserImage);
#if (UNITY_IPHONE || UNITY_ANDROID)  && !UNITY_EDITOR
            userImagePanel.gameObject.GetComponent<Image>().sprite = HeadImage[nChairId];
#endif
            //name
            if (userImagePanel != null)
            {
                var nameText = userImagePanel.Find("Name").GetComponent<Text>();
                nameText.text = kName;
                //state
                var stateImage = userImagePanel.Find("StateImage").GetComponent<Image>();
                stateImage.sprite = agreeSprite;
            }

            iIdex++;
        }

        //NotAgre Users
        for (int i = 0; i < (int)pNetInfo.dwNotAgreeUserCout; i++)
        {
            nChairId = pNetInfo.dwNotAgreeChairID[i];
            pPlayer = hnPlayers[nChairId];
            kChairID[pNetInfo.dwNotAgreeChairID[i]] = 0;
            if (pPlayer != null)
            {
                kName = pPlayer.GetNickName();
            }
            if (pPlayer == pLocalPlayer)
            {
                bShowSelfAction = false;
            }
            String sNameUserImage = String.Format("UserInfos/UserImage{0}", iIdex + 1);
            var userImagePanel = panel.Find(sNameUserImage);
#if (UNITY_IPHONE || UNITY_ANDROID)  && !UNITY_EDITOR
            userImagePanel.gameObject.GetComponent<Image>().sprite = HeadImage[nChairId];
#endif
            //name
            var nameText = userImagePanel.Find("Name").GetComponent<Text>();
            nameText.text = kName;
            //state
            var stateImage = userImagePanel.Find("StateImage").GetComponent<Image>();
            stateImage.sprite = refuseSprite;

            iIdex++;
        }

        //Wait users
        for (int i = 0; i < 4; i++)
        {
            if (kChairID[i] == 0)
            {
                continue;
            }

            nChairId = (uint)i;
            pPlayer = hnPlayers[nChairId];
            if (pPlayer == null)
            {
                continue;
            }
            else
            {
                kName = pPlayer.GetNickName();
            }

            String sNameUserImage = String.Format("UserInfos/UserImage{0}", iIdex + 1);
            var userImagePanel = panel.Find(sNameUserImage);
#if (UNITY_IPHONE || UNITY_ANDROID)  && !UNITY_EDITOR
            userImagePanel.gameObject.GetComponent<Image>().sprite = HeadImage[nChairId];
#endif
            if (userImagePanel != null)
            {
                //name
                var nameText = userImagePanel.Find("Name").GetComponent<Text>();
                nameText.text = kName;
                //state
                var stateImage = userImagePanel.Find("StateImage").GetComponent<Image>();
                stateImage.sprite = waitingSprite;
            }

            iIdex++;
        }

        //Buttons
        var buttonsPanel = panel.Find("Buttons");
        buttonsPanel.gameObject.SetActive(bShowSelfAction);
        //WaitingText
        var waitingPanel = panel.Find("WaitingText");
        waitingPanel.gameObject.SetActive(!bShowSelfAction);
    }

    bool CanSendCard()
    {
        if (UIState != GameUIState.UI_Idle)
        {
            return false;
        }

        if (CServerItem.get() == null)
        {
            return false;
        }

        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        if (kernel == null)
        {
            return false;
        }

        var localPlayer = hnPlayers[m_iLocalChairID];
        if (localPlayer == null)
        {
            return false;
        }

        return !bSendingCards && (kernel.GetCurrentPlayerID() == m_iLocalChairID) && (localPlayer.AnimCount() == 0);
    }

    void selectCard(GameObject objSelected, bool bSelect)
    {
    }

    private void UpdateOfflineUI()
    {
        if (bEnteredGameScene)
        {
            for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
            {
                if (hnPlayers[i] != null)
                {
                    int nStatus = hnPlayers[i].GetUserStatus();
                    bool bIsOffline = (nStatus == SocketDefines.US_OFFLINE);// || nStatus == SocketDefines.US_NULL);
                    ShowOfflineUI(i, bIsOffline);
                }
            }
        }
    }

    private void UpdatePlayerReadyUI()
    {
        if (bEnteredGameScene)
        {
            uint nRoomPlayCout = GetRoomPlayCout();
            for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
            {
                if (hnPlayers[i] != null)
                {
                    int nStatus = hnPlayers[i].GetUserStatus();
                    bool bIsReady = (nStatus == SocketDefines.US_READY);// || nStatus == SocketDefines.US_NULL);

#if ADD_READY_STEP

                    //Ready Button
                    if (i == m_iLocalChairID && m_readyButton != null)
                    {
                        if (nStatus == SocketDefines.US_SIT && nRoomPlayCout==0)
                        {
                            m_readyButton.SetActive(true);
                            m_readyButton.SetActive(false);
                        }
                        else
                        {
                            m_readyButton.SetActive(false);
                        }
                    }

                    ////“下一局”按钮
                    //if(i == m_iLocalChairID && nRoomPlayCout > 0 && nStatus == SocketDefines.US_SIT)
                    //{
                    //    var smallFinal = FinalUI.transform.Find("SmallFinal");
                    //    if(smallFinal != null)
                    //    {
                    //        if(!smallFinal.gameObject.active)
                    //        {
                    //            ShowFinalLabel(true);
                    //        }
                    //    }
                    //}

                    bool bShow = bIsReady && (nRoomPlayCout >= 0);
#else
                    bool bShow = bIsReady && (nRoomPlayCout > 0);
#endif
                    ShowPlayerReadyUI(i, bShow);

                }
            }
        }

#if ADD_READY_STEP
        else
        {
            //Ready Button
            if ( m_readyButton != null)
            {
                m_readyButton.SetActive(false);
            }
        }
#endif

    }

    //mChen add, for HideSeek
    public void UpdateReadyButton()
    {
        if (GameManager.s_gameSingleMultiType != GameSingleMultiType.SingleGame && bEnteredGameScene && !m_bPressStartButton && CServerItem.get() != null)
        {
            byte Gamestate = CServerItem.get().GetGameStatus();
            if (Gamestate == SocketDefines.GAME_STATUS_FREE)  //游戏开始前断线处理
            {
                int nRoomType = PlayerPrefs.GetInt("PubOrPrivate");
                if (nRoomType == (int)RoomType.Type_Private && IsMeRoomCreator() && !playAgain)
                {
                    ShowReadyButton(true);
                    return;
                }
            }
        }

        if (GameManager.s_gameSingleMultiType != GameSingleMultiType.SingleGame && bEnteredGameScene && CServerItem.get() != null)
        {
            int nRoomType = PlayerPrefs.GetInt("PubOrPrivate");
            if (nRoomType == (int)RoomType.Type_Public || !IsMeRoomCreator() || playAgain)
            {
                ShowReadyButton(false);
                return;
            }
        }
    }

    public void ShowHideRedForeground(bool bIsShow)
    {
        Loom.QueueOnMainThread(() =>
        {
            if (m_redForeground != null)
            {
                m_redForeground.SetActive(bIsShow);
            }
        });
    }

    private GameObject movingObj = null;
    private float yOri = 5.447499f;
    private float yUp = 5.55f;
    private Vector3 oriPos;

#if FPS
    public Text FpsText;
    public float fpsMeasuringDelta = 2.0f;

    private float timePassed;
    private int m_FrameCount = 0;
    private float m_FPS = 0.0f;
#endif
    void Update()
    {
#if true
        m_FrameCount = m_FrameCount + 1;
        timePassed = timePassed + Time.deltaTime;

        if (timePassed > fpsMeasuringDelta)
        {
            m_FPS = m_FrameCount / timePassed;
            FpsText.text = m_FPS.ToString();
            timePassed = 0.0f;
            m_FrameCount = 0;
        }
#endif
        UpdateOfflineUI();
        UpdatePlayerReadyUI();
        UpdateReadyButton();

        //var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        if (bEnteredGameScene && UIState != GameUIState.UI_Starting && UIState != GameUIState.UI_Max)
        {
            //if(kernel!=null)
            {
                for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
                {
                    if (hnPlayers[i] != null)
                    {
                        hnPlayers[i].UpdateAnim();
                    }
                }
            }
        }

#if ApplyAutoReConnect
        CheckInGameServerAfterOffline();
#endif

        #region 经验更新
        //ExperienceSystem();
        #endregion

    }
    public void L(Slider s)
    {
        if (ControlManager.GetInstance() != null && ControlManager.GetInstance()._etcJoystickL != null && ControlManager.GetInstance()._etcJoystickR != null)
        {
            ControlManager.GetInstance()._etcJoystickL.axisX.speed = s.value * 10;
            ControlManager.GetInstance()._etcJoystickL.axisY.speed = s.value * 10;
            TextL.GetComponent<Text>().text = (s.value * 10).ToString();
        }
    }
    public void R(Slider s)
    {
        if (ControlManager.GetInstance() != null && ControlManager.GetInstance()._etcJoystickL != null && ControlManager.GetInstance()._etcJoystickR != null)
        {
            ControlManager.GetInstance()._etcJoystickR.axisX.speed = s.value * 200;
            ControlManager.GetInstance()._etcJoystickR.axisY.speed = s.value * 200;
            TextR.GetComponent<Text>().text = (s.value * 200).ToString();
        }
    }

    public void CheckInGameServerWhenToHallByDisconnect()
    {
        return;
        //大厅的断线重连检测

        if (m_bIsToHallByDisconnect)
        {
            m_offlineWaitTime += Time.deltaTime;

            if (m_offlineWaitTime > 3.0f)
            {
                m_offlineWaitTime = 0f;
                UserInfo.getInstance().checkInGameServer();
            }

            if (m_offlineWaitingImage != null)
            {
                m_offlineWaitingImage.SetActive(true);

                if (m_bIsToHallFrom13Shui)
                {
                }
                else
                {
                    Transform imageMJTrans = m_offlineWaitingImage.transform.Find("Image_MJ");
                    if (imageMJTrans != null)
                    {
                        imageMJTrans.gameObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
            if (m_offlineWaitingImage != null)
            {
                m_offlineWaitingImage.SetActive(false);
            }
        }
    }
    public void HideOfflineWaitingUI()
    {
        Loom.QueueOnMainThread(() =>
        {
            if (m_offlineWaitingImage != null)
            {
                m_offlineWaitingImage.SetActive(false);
            }
        });
    }

    public void CheckInGameServerAfterOffline()
    {
        //断线重连检测

        enServiceStatus serviceStatus = enServiceStatus.ServiceStatus_Unknow;
        byte Gamestate = SocketDefines.GAME_STATUS_FREE;
        if (CServerItem.get() != null)
        {
            serviceStatus = CServerItem.get().GetServiceStatus();
            Gamestate = CServerItem.get().GetGameStatus();

            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        }

        //if (kernel != null && kernel.getGameState() == GameScene.MJState.HNMJ_STATE_PLAYING && bEnteredGameScene && serviceStatus == enServiceStatus.ServiceStatus_NetworkDown)//UIState != GameUIState.UI_Starting)
        //if(GameManagerBaseNet.InstanceBase().GetReconnectStatus() == GameManagerBaseNet.enReconnectStatus.ReconnectStatus_DisConnect)
        if (bEnteredGameScene && m_cbGameEndReason == HNMJ_Defines.GER_NOT_END /*&& m_bRoomStartGame*/ && serviceStatus != enServiceStatus.ServiceStatus_ServiceIng)//kernel==null
        {
            //mChen comment:因为服务器在游戏结束时会让离线玩家强制离开
            ////mChen add, 游戏快结束的时候断线，直接回到大厅，fix重连回来已经第二轮的bug
            //if ( (Gamestate == SocketDefines.GAME_STATUS_PLAY && UIManager.TimeLeft < 20) || (Gamestate == SocketDefines.GAME_STATUS_END) )
            //{
            //    Debug.Log("游戏快结束的时候断线，直接回到大厅,TimeLeft=" + UIManager.TimeLeft);

            //    //mChen add, for HideSeek:强制离开
            //    if (CServerItem.get() != null)
            //    {
            //        CServerItem.get().IntermitConnect(true);
            //    }
            //    LeaveGameToHall(false);
            //    return;
            //}


            ///var localPlayer = hnPlayers[m_iLocalChairID];
            ///if ( localPlayer != null && (localPlayer.getUserItem()==null || localPlayer.GetUserStatus() == SocketDefines.US_OFFLINE) )
            {
                m_offlineWaitTime += Time.deltaTime;

                if (m_offlineWaitTime > 3.0f)
                {
                    m_offlineWaitTime = 0f;

                    UserInfo.getInstance().checkInGameServer();

                    if (Gamestate == SocketDefines.GAME_STATUS_FREE && !m_bPressStartButton)  //游戏开始前断线处理
                    {
                        int nRoomType = PlayerPrefs.GetInt("PubOrPrivate");
                        if (nRoomType == (int)RoomType.Type_Private && IsMeRoomCreator())
                        {
                            ShowReadyButton(true);
                        }
                    }
                }
            }

            if (m_breakLineWindow != null && !m_breakLineWindow.activeSelf)
            {
                m_breakLineWindow.SetActive(true);

                GameObjectsManager.GetInstance().SaveOffLineInfo();
                playerTeamPos = false;
                gamePlayUI_HT.transform.FindChild("Hearts").GetComponent<HpUI>().enabled = false;
                //String[] strName = GlobalUserInfo.getNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                //if (strName.Length > 0)
                //{
                //    Loom.QueueOnMainThread(() =>
                //    {
                //        ChatSystem.GetInstance.ShowChatText("通知", strName[0] + " 离开了房间！");
                //    });
                //}
                chatSystem.TextClear();
                //断线时删除Human
                GameObjectsManager.GetInstance().ClearPlayers(false);
            }
        }
        else
        {
            if (Gamestate == SocketDefines.GAME_STATUS_FREE)  //游戏开始前断线处理
            {
                //int nRoomType = PlayerPrefs.GetInt("PubOrPrivate");
                //if (nRoomType == (int)RoomType.Type_Private)
                //{
                //    //SetLoading(true);
                //}
                IClientUserItem pMeItem = null;
                if(CServerItem.get() != null)
                {
                    pMeItem = CServerItem.get().GetMeUserItem();
                }
                if (pMeItem == null)
                {
                    //mChen add, for HideSeek:去除重连提示UI
                    if (m_breakLineWindow != null && m_breakLineWindow.activeSelf)
                    {
                        m_breakLineWindow.SetActive(false);
                    }

                    return;
                }
                int nStatus = pMeItem.GetUserStatus();
                if (nStatus == SocketDefines.US_LOOKON)  //判断是否为旁观
                {
                    SetLoading(false);
                }
            }
            else
            {
                if (m_breakLineWindow != null && m_breakLineWindow.activeSelf)
                {
                    if (Gamestate == SocketDefines.GAME_STATUS_PLAY || Gamestate == SocketDefines.GAME_STATUS_HIDE)
                        StartCoroutine(OffLineEnd(m_breakLineWindow));
                    //m_breakLineWindow.SetActive(false);
                }
            }
        }
    }
    float t = 0.0f;
    bool success = true;
    IEnumerator OffLineEnd(GameObject m_breakLineWindow)
    {
        t = 0.0f;
        yield return new WaitUntil(() =>
        {
            t += Time.deltaTime;
            if (t > 5)
            {
                success = false;
                return true;
            }
            return playerTeamPos == true && localEnter == true;
        });
        if (success)
        {
            gamePlayUI_HT.transform.FindChild("Hearts").GetComponent<HpUI>().enabled = true;
            GameObjectsManager.GetInstance().LoadOffLineInfo();
            playerTeamPos = false;
            localEnter = false;
            SetLoading(false);
            m_breakLineWindow.SetActive(false);
        }
        success = true;
    }

    static Dictionary<byte, int> cardMap = new Dictionary<byte, int>();

    public static int GetCardValueInt(byte cardByte)
    {
        if (cardMap.ContainsKey(cardByte))
        {
            return cardMap[cardByte];
        }
        int cbValue = ((byte)cardByte & HNMJLogic_Defines.MASK_VALUE);
        int cbColor = (((byte)cardByte & HNMJLogic_Defines.MASK_COLOR) >> 4) + 1;

        cardMap.Add(cardByte, cbColor * 10 + cbValue);
        return cardMap[cardByte];
    }

    public static byte GetCardValueByte(int cardInt)
    {
        foreach (var i in cardMap)
        {
            if (i.Value == cardInt)
            {
                return i.Key;
            }
        }
        return 0;
    }

    private bool bSendingCards = false;

    void SendCard(GameObject card)
    {
    }

    public enum BtnTypes
    {
        Btn_Guo,
        Btn_Peng,/*
        Btn_Gang,*/
        Btn_Hu,
        Btn_Chi_Left,
        Btn_Chi_Center,
        Btn_Chi_Right,
        Btn_Max
    }


    public void SetChiCardPanel(GameObject ChiNode, BtnTypes type, byte cbActionCard)
    {
    }

    public void SetLogicBtnChi(int chiNum, int[] chiType, byte cbActionCard)
    {
    }

    public void SetGangPanel(GameObject GangObj, byte cbActionCard)
    {
    }

    public void SetLogicBtnsGang(bool bHaveChiPanel, byte[] cbGangCards)
    {
    }


    public void SetLogicBtns(int index, BtnTypes types, byte cbActionCard = 0)
    {
    }

    public void SetHuaCardObj(int chairID, byte huaCardValue, int huaIndex)
    {
    }

    public static int CalArrowDir(int selfChairID, int providerID)
    {
        if (selfChairID == providerID)
        {
            return 0;//self
        }
        var nextId = getNextPlayerChairID(selfChairID);
        if (nextId == providerID)
        {
            return 1; // right
        }
        nextId = getNextPlayerChairID(nextId);
        if (nextId == providerID)
        {
            return 2; // up
        }
        else
        {
            return 3;// left
        }
    }

    public void SetWeaveCards(CMD_WeaveItem[] allWeaves, int chairID)
    {
    }

    public void ResetMovingCard()
    {
        if (movingObj != null)
        {
            movingObj.transform.position = oriPos;
            selectCard(movingObj, false);
            movingObj = null;
        }
    }

    public void ResetLocalHandCardStuff(byte[] cardsBytes)
    {
    }
    //bReconnect 当值为true时，说明是重连进来，此时m_OutCardList内不存在，全部通过getOutCard函数获取obj
    //lin: 重连时， weaveIndex才有用！！！
    public void SetChiPengGangObj(int chairID, int[] cardValue, int type, int iArrowDir, bool bReconnect = false, int weaveIndex = -1) // 1: peng 2: gang 3: left chi 4: center chi 5: right chi
    {
    }

    private static String[] HuStr = { "流局", "胡（自摸）", "被胡（自摸）", "胡（抢杠）", "被胡", "胡（起手胡）", "被胡（起手胡）" };
    //private static String[] HuStr = { "流局", "胡（自摸）", " 被胡（自摸）", "胡（接炮）", "被胡（放炮）", "胡（起手胡）", "被胡（起手胡）" };

    public void ShowFinalLabel(bool bShow, int[] finalInfo = null, String HuStr = null)
    {
        if (bFakeServer)
            return;
        StopTimeDownOfDismissRoom();

        FinalUI.SetActive(bShow);
        if (bShow)
        {
            if (HuStr != null)
            {
                HuPaiUI.text = HuStr;
                if (HuStr == "流局")
                {
                    LiuJuObj.SetActive(true);
                }
            }

            if (finalInfo != null)
            {
                ChengBaoUI.text = finalInfo[0].ToString();
                ScoreUI.text = finalInfo[1].ToString();
            }

            FinalUI.transform.Find("SmallFinal").gameObject.SetActive(true);
            FinalUI.transform.Find("BigFinal").gameObject.SetActive(false);

            //隐藏按钮，等动画播放完毕再显示
            FinalUI.transform.Find("SmallFinal/NextButton").gameObject.SetActive(false);
        }
    }

    public void ReturnFromBigFinalToHallScene()
    {
        if (CServerItem.get() != null)
        {
            CServerItem.get().PerformStandUpAction();
        }

        Debug.Log("ReturnFromBigFinalToHallScene " + m_cbGameEndReason);
        if (m_cbGameEndReason == HNMJ_Defines.GER_DISMISS)
        {
            FinalUI.SetActive(true);
        }
        else
        {
            FinalUI.SetActive(false);
        }
        if (SceneManager.GetActiveScene().name == "GameHall")
        {
            /*switch (m_cbGameEndReason)
            {
                case 2:
                    FinalUI.SetActive(false);
                    break;
                case 4:
                    FinalUI.SetActive(false);
                    break;
            }*/
            FinalUI.SetActive(false);
        }
        else
        {
            LeaveGameToHall();
        }

        m_cbGameEndReason = HNMJ_Defines.GER_NOT_END;
        m_bIsToHallByDisconnect = false;
    }

    private Coroutine bigFinalCor = null;
    IEnumerator showBigFinalCoroutine()
    {
        if (GameScene.KIND_ID == GameScene.KIND_ID_JianDe)
        {
            bool bHaveAnim = false;
            do
            {
                bHaveAnim = false;
                foreach (var hnmjPlayer in hnPlayers)
                {
                    if (hnmjPlayer != null && hnmjPlayer.AnimCount() > 0)
                    {
                        bHaveAnim = true;
                        break;
                    }
                }
                yield return null;
            } while (bHaveAnim);
        }
        FinalUI.SetActive(true);
        FinalUI.transform.Find("SmallFinal").gameObject.SetActive(false);
        FinalUI.transform.Find("BigFinal").gameObject.SetActive(/*true*/false);

        Debug.Log("ShowBigFinalLabel -------------------");
        //正常退出和解散时设置回默认值
        ///HNGameManager.GameType = HNPrivateScenceBase.GAME_TYPE_Null;
        bigFinalCor = null;
    }

    public void ShowBigFinalLabel()
    {
        if (bigFinalCor == null)
        {
            bigFinalCor = StartCoroutine(showBigFinalCoroutine());
        }
    }

    public String[] sActionTag = { "自摸次数", "接炮次数", "点炮次数", "杠次数" };
    public void ShowBigFinalLabel(bool bShow, FinalJieSuanInfo[] finalJieSuanInfo)
    {
    }

    public void OnClickUserHead(int iGamePlayerIdex)
    {
    }

    IEnumerator nextGameCoroutine()
    {
        yield return null;

        if (CServerItem.get() != null)
        {
            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
            kernel.HNMJButton_Ready();
        }
        nextGameCor = null;
    }

    private Coroutine nextGameCor = null;
    public void NextGame()
    {
        if (nextGameCor == null)
        {
            nextGameCor = StartCoroutine(nextGameCoroutine());
        }
    }

    public void OnPressReadyButton()
    {
        if (CServerItem.get() != null)
        {
            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
            kernel.sendReady();
        }
    }

    public void OnPressStartButton()
    {
        if (CServerItem.get() != null)
        {
            CServerItem.get().SendCreaterUserPressedStart(null, 0);
            m_bPressStartButton = true;
        }
    }

    public void ShowExpression(int chairID, int expIndex)
    {
        m_allPlayersUI[chairID].ExpUVAnimTexture.gameObject.SetActive(true);
        m_allPlayersUI[chairID].ExpUVAnimTexture.ExpAnimationPlay(expIndex,
            () => { m_allPlayersUI[chairID].ExpUVAnimTexture.gameObject.SetActive(false); });
    }

    private static string[] LansStr =
    {
        "动作快点啦",                       //SOUND_KuaiDian
        "再不给我碰，我就打八对子了",       //SOUND_ZaiBuGeiWoPeng
        "叫扫，这么没风头的",               //SOUND_JiaoSao
        "还吃？抓到财神不告诉你",           //SOUND_HaiChi
        "想吃？没有的",                     //SOUND_XiangChi
        "财神呢？财神死哪里去了？",         //SOUND_CaiShenNe
        "下家是个人为，要吃的为",           //SOUND_YaoChiDe
        "下次再也不坐你下家了",             //SOUND_BuZuoNiXiaJiaLe
        "不来了，不来了，你什么风头啊",     //SOUND_BuLaiLe
        "这个麻将吃不消打，下次再来过",     //SOUND_ChiBuXiaoDa
        "这么好的牌都没得糊",               //SOUND_MeiHu
    };

    public void ShowLans(int chairID, int expIndex)
    {
        m_allPlayersUI[chairID].LansObj.SetActive(true);
        m_allPlayersUI[chairID].LansObj.transform.GetComponentInChildren<Text>().text = LansStr[expIndex];
        StopCoroutine("HideLansUI");
        StartCoroutine("HideLansUI", chairID);

        int iSoundID = (int)AudioManager.Sound_Defines.SOUND_ChatSound_Start + expIndex;
        HNMJPlayer pPlayer = hnPlayers[chairID];
        PlaySoundClipByGender(chairID, pPlayer.GetGender(), iSoundID);
    }

    IEnumerator HideLansUI(int chariId)
    {
        yield return new WaitForSeconds(1.5f);
        m_allPlayersUI[chariId].LansObj.SetActive(false);
    }

    public void EmoJiBtnClicked(int emojiID)
    {
        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        kernel.Button_EmosShow(emojiID);
        HideEmojiOrLan();
    }

    public void LanBtnClicked(int lansID)
    {
        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        kernel.Button_LansShow(lansID);
        HideEmojiOrLan();
    }

    public void ToogleEmoji(bool bIsOn)
    {
        EmojiUI.SetActive(bIsOn);
        LansUI.SetActive(!bIsOn);
    }

    public void ToogleLans(bool bIson)
    {
        LansUI.SetActive(bIson);
        EmojiUI.SetActive(!bIson);
    }

    public void StartRecord(BaseEventData data)
    {
#if UNITY_ANDROID
        bRecording = true;

        var jc = new AndroidJavaObject("com.wingjoy.audioplugin.AudioUtils");
        jc.CallStatic("StartSoundRecord");
        SpeakingUI.SetActive(true);
#elif UNITY_IPHONE
		bRecording = true;
		SpeakingUI.SetActive(true);
		StartIOSRecord();
#endif

    }

    private bool bRecording = false;
    public void EndRecording(BaseEventData eventData)
    {
#if UNITY_ANDROID
        var jc = new AndroidJavaObject("com.wingjoy.audioplugin.AudioUtils");
        jc.CallStatic("EndRecord");
        //SpeakEnd(soundStr);

        //AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        //AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        //jo.CallStatic("EndSoundRecord");
#elif UNITY_IPHONE
		EndIOSRecord();
#endif
        ResetVolume();
    }

    public void SpeakEnd(string soundFile)
    {
        if (bRecording)
        {
            bRecording = false;
        }
        else
        {
            return;
        }

        SpeakingUI.SetActive(false);

#if UNITY_ANDROID || UNITY_IPHONE
        string sound = System.IO.Path.Combine(Application.persistentDataPath , soundFile);
        Debug.Log("Persister path " + Application.persistentDataPath + " sound file " + sound);
#if true
        FileStream fs = new FileStream(sound, FileMode.Open);
        byte[] bytes = new byte[fs.Length];
        BinaryReader br = new BinaryReader(fs);
        br.Read(bytes, 0, bytes.Length);
        br.Close();
#else
        StreamReader sr = new StreamReader(sound);
        var soundData = sr.ReadToEnd();
        Debug.Log(soundData);
        //Debug.Log("SpeakEnd receive : " + soundData.Length);
        byte[] bytes = Encoding.UTF8.GetBytes(soundData);
        sr.Close();
#endif
        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        kernel.sendTalkFile(bytes);
#endif
        //AndroidJNI.DetachCurrentThread();
    }

    private int m_curSpeakID;
    public void UserTalk(int chairID, byte[] soundData)
    {
        Debug.Log("UserTalk receive : " + chairID + "DATA LEN " + soundData.Length);
#if UNITY_ANDROID
        m_curSpeakID = chairID;
        string sound = System.IO.Path.Combine(Application.persistentDataPath, "ReceivedSound.amr");
        FileStream fs = new FileStream(sound, FileMode.Create, FileAccess.Write);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(soundData);
        bw.Flush();
        bw.Close();

        var jc = new AndroidJavaObject("com.wingjoy.audioplugin.AudioUtils");
        jc.CallStatic("PlaySound");
        StartCoroutine(IOSStopPlaySound(chairID));

#elif UNITY_IPHONE
         //m_curSpeakID = chairID;
       
        string sound = System.IO.Path.Combine(Application.persistentDataPath, "ReceivedSound.amr");
        Debug.Log("Persister path " + Application.persistentDataPath + " sound file " + sound);

        FileStream fs = new FileStream(sound, FileMode.Create, FileAccess.Write);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(soundData);
        bw.Flush();
        bw.Close();
        
		PlayIOSSound();

		StartCoroutine(IOSStopPlaySound(chairID));
#endif
        m_allPlayersUI[chairID].VoiceUI.SetActive(true);
    }

    //#if UNITY_IPHONE
    IEnumerator IOSStopPlaySound(int chiarID)
    {
        yield return new WaitForSeconds(1.5f);
        m_allPlayersUI[chiarID].VoiceUI.SetActive(false);
    }
    //#endif

    public void OtherTalkEnd()
    {
        Debug.Log("chair talk end " + m_curSpeakID);
        m_allPlayersUI[m_curSpeakID].VoiceUI.SetActive(false);
        //AndroidJNI.DetachCurrentThread();
    }

    public void PaymentResult(string productId)
    {
        if (productId == "0")
        {
            //buy failed
        }
        else
        {
            //buy success
            switch (productId)
            {
                case "Xinan_01":
                    {
                        UserInfo.getInstance().AddPayment(6, 8);
                    }
                    break;
                case "Xinan_02":
                    {
                        UserInfo.getInstance().AddPayment(18, 24);
                    }
                    break;
                case "Xinan_03":
                    {
                        UserInfo.getInstance().AddPayment(30, 40);
                    }
                    break;
                case "Xinan_04":
                    {
                        UserInfo.getInstance().AddPayment(68, 92);
                    }
                    break;
                case "Xinan_05":
                    {
                        UserInfo.getInstance().AddPayment(128, 175);
                    }
                    break;
                case "Xinan_06":
                    {
                        UserInfo.getInstance().AddPayment(328, 450);
                    }
                    break;
                default:
                    {
                        Debug.Log("PaymentResult error: incorrct productId=" + productId);
                    }
                    break;
            }
        }
    }

    public void ResetPlayersUIInfo()
    {
        for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
        {
            if (m_allPlayersUI[i].SeatNode != null)
            {
                m_allPlayersUI[i].SeatNode.SetActive(false || bFakeServer);
            }

            ShowOfflineUI(i, false);
            ShowPlayerReadyUI(i, false);
        }

        var dismissPanel = gameplayUI.transform.Find("Window/DismissPanel");
        if (dismissPanel != null)
        {
            dismissPanel.gameObject.SetActive(false);
        }


        var disbandWindow = gameplayUI.transform.Find("Window/DisbandWindow");
        if (disbandWindow != null)
        {
            disbandWindow.gameObject.SetActive(false);
        }

        StopTimeDownOfDismissRoom();
    }

    void SetRoomUI(bool bEnable)
    {
        //bEnable = true;

        Debug.Log("SetRoomUI" + bEnable);
        GameObject RoomInfoTemp = gameplayUI.transform.Find("RoomInfo").gameObject;
        gameplayUI.transform.Find("FunctionButtons").gameObject.SetActive(bEnable && !bFakeServer);
        gameplayUI.transform.Find("PlayBackUI").gameObject.SetActive(bEnable && bFakeServer);

        if (GameType == HNPrivateScenceBase.GAME_TYPE_JianDe)
        {
            gameplayUI.transform.Find("RoomInfo/LeftCard").gameObject.GetComponent<Text>().enabled = true;
            gameplayUI.transform.Find("RoomInfo/PlayerCount").gameObject.GetComponent<Text>().enabled = false;

            String CardPic = "RoomInfo/roomInfo";
            Image image = RoomInfoTemp.GetComponent<Image>();
            image.overrideSprite = Resources.Load(CardPic, typeof(Sprite)) as Sprite;
        }

        if (PlayerPrefs.GetInt("PubOrPrivate") == (int)RoomType.Type_Private)
        {
            RoomInfoTemp.SetActive(bEnable);
        }
        else
        {
            //躲猫猫欢乐模式测试版本显示房间号  WQ
            RoomInfoTemp.SetActive(/*false*/bEnable);
        }
        if (Loading != null)
        {
            //SetLoading(true);
        }
    }

    public void DefaultState()
    {
        LeftTipsObj.SetActive(false);
        if (LeftTipsCoroutine != null)
        {
            StopCoroutine(LeftTipsCoroutine);
            LeftTipsCoroutine = null;
        }
        LiuJuObj.SetActive(false);
        bigFinalCor = null;
        bResetLoaclCardsBeforeGameEnd = false;
        bSendingCards = false;
        UIState = GameUIState.UI_Max;
        leaveGameCoroutine = null;
        resetGameCoroutine = null;
        nextGameCor = null;
        StopTimeDownOfDismissRoom();

        //gameplayMaJiang.GetComponent<Animation>().enabled = false;
        //gameplayMaJiang.transform.FindChild("mahjongTilesOK").gameObject.SetActive(false);

        EmoJiAndLanUI.SetActive(false);
        ShowFinalLabel(false);
        ///FocusUI.SetActive(false);

        return;
    }

    public void DefaultState_HideSeek()
    {
        if (InventoryManager.GetInstane != null)
            InventoryManager.GetInstane.DestoryInventoryObjects();  //删除残余道具
        ShowReadyButton(false);  //隐藏解散时未隐藏的加入按键
        if (UIManager.GetInstance() != null)
        {
            //if (UIManager.GetInstance().m_Canvas != null)
            //{
            //    UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart0").GetComponent<Image>().sprite = Resources.Load("UI/Hearts/Pink", typeof(Sprite)) as Sprite;
            //    UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart1").GetComponent<Image>().sprite = Resources.Load("UI/Hearts/Pink", typeof(Sprite)) as Sprite;
            //    UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart2").GetComponent<Image>().sprite = Resources.Load("UI/Hearts/Pink", typeof(Sprite)) as Sprite;
            //    UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart3").GetComponent<Image>().sprite = Resources.Load("UI/Hearts/Pink", typeof(Sprite)) as Sprite;
            //}
            UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(false);
            UIManager.GetInstance().m_Canvas.transform.Find("Btn/StealthButton").GetComponent<Button>().interactable = true;
            if (UIManager.TimeTip != null)
            {
                UIManager.TimeTip.GetComponent<Text>().text = "";
            }
            UIManager.GetInstance().ResetColdTime(); //初始化重置CD时间
            UIManager.GetInstance().ClearMiddleTips();
        }

        //gamePlayUI_HT.transform.Find("LockButton").GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/Red", typeof(Sprite)) as Sprite;
        //gamePlayUI_HT.transform.Find("LockButton/Image").GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/Gold", typeof(Sprite)) as Sprite;
        gamePlayUI_HT.transform.Find("Btn/LockButton").GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/UnLock", typeof(Sprite)) as Sprite;

        ControlManager.GetInstance().RemoveListener();
        if (Loading != null)
        {
            SetLoading(false);
        }
        //游戏内聊天信息清零
        chatSystem.TextClear();
        //游戏内BGM关闭
        StopBGM();
        //清除断线信息，防止临近结束时断线,之前的断线信息仍存在
        if (PlayerPrefs.HasKey("LocalHumanInfo"))
        {
            PlayerPrefs.DeleteKey("PlayerPrefs");
            PlayerPrefs.Save();
        }
        GameObjectsManager.GetInstance().ClearPlayers();
        //初始化重置单机游戏状态
        if (GameManager.s_singleGameStatus != SocketDefines.GAME_STATUS_FREE)
            GameManager.s_singleGameStatus = SocketDefines.GAME_STATUS_FREE;
    }

    private Coroutine leaveGameCoroutine = null;
    IEnumerator leaveCoroutine(bool bLeaveInHeartBeat)
    {
        Debug.Log("leaveCoroutine 1");

        if (bFakeServer)
        {
            if (FakeGameCoroutine != null)
            {
                StopCoroutine(FakeGameCoroutine);
                FakeGameCoroutine = null;
                Array.Clear(hnPlayers, 0, GameScene.MAX_PLAYER);
            }
            GameManagerBaseNet.InstanceBase().FakeCloseGame();
            ((CServerItem)CServerItem.get()).FakeUserLeave(PlayBackStorage.GetInstance().GetCurrentRecordIdx());
            PlayBackPauseOrPlay.SetActive(false);
            //StopAllCoroutines();
        }

        yield return null;

        bFakeServer = false;
        Debug.Log("leaveCoroutine 4");

        ResetPlayersUIInfo();//to hide dismiss panel
        SetRoomUI(false);


        if (FinalUI != null)
        {
            if (m_cbGameEndReason == HNMJ_Defines.GER_NORMAL)
            {
                FinalUI.SetActive(false);
            }
            else if (m_cbGameEndReason == HNMJ_Defines.GER_DISMISS)
            {
                FinalUI.SetActive(true);
                FinalUI.transform.Find("SmallFinal").gameObject.SetActive(false);
                FinalUI.transform.Find("BigFinal").gameObject.SetActive(/*true*/false);
                PlayBackStorage.GetInstance().StopRecord();
                Debug.Log("Dismiss End");
            }
        }
        LiuJuObj.SetActive(false);
        bEnteredGameScene = false;
        Array.Clear(hnPlayers, 0, GameScene.MAX_PLAYER);
        gameplayUI.transform.Find("InviteButton").gameObject.SetActive(false);
        leaveGameCoroutine = null;
        if (bLeaveInHeartBeat == false)
        {
            StartOrStopGameSceneHeartBeat(false);
        }
        m_breakLineWindow.SetActive(false);
        SceneManager.LoadScene("GameHall");
        StopAllCoroutines();
    }
    public void LeaveGameToHall(bool bLeaveInHeartBeat = false)
    {
        Debug.LogWarning("LeaveGameToHall");

        playAgain = false;
        isReconnect = false;
        DefaultState_HideSeek();

        //Exp UI
        if (Exp != null && Exp.activeSelf)
            Exp.SetActive(false);

        m_bPressStartButton = false;  //返回大厅，startGame重新置为false
        GameManager.m_bHasCreatedAIs = false;
        //DestroyPlayerTeam();

        if (bFakeServer)
        {
            Time.timeScale = 1.0f;
        }
        //ZY add
        ResetPlayerCount();
        ZeroPlayerScore();
        if (leaveGameCoroutine == null)
        {
            leaveGameCoroutine = StartCoroutine(leaveCoroutine(bLeaveInHeartBeat));
        }
        ShowHideRedForeground(false);

        ///PlayAgain();
    }

    public void LeaveGameToLand()
    {
        bEnteredGameScene = false;
        SceneManager.LoadScene("GameLand");

        ShowHideRedForeground(false);
    }

    private Coroutine resetGameCoroutine = null;
    IEnumerator resetCoroutine()
    {
        bool bHaveAnim = false;
        do
        {
            bHaveAnim = false;
            foreach (var hnmjPlayer in hnPlayers)
            {
                if (hnmjPlayer != null && hnmjPlayer.AnimCount() > 0)
                {
                    bHaveAnim = true;
                    break;
                }
            }
            yield return null;
        } while (bHaveAnim);
        ///ResetPlayersUIInfo();//to hide dismiss panel
        SetRoomUI(true);
        ///bEnteredGameScene = false;
        Array.Clear(hnPlayers, 0, GameScene.MAX_PLAYER);
        ///SceneManager.LoadScene("GameHall");
        resetGameCoroutine = null;
        yield return new WaitForSeconds(0.3f);

        if (FinalUI != null)
        {
            FinalUI.SetActive(false);
        }

    }
    public void ResetGameAfterDisconnect()
    {
        if (resetGameCoroutine == null)
        {
            resetGameCoroutine = StartCoroutine(resetCoroutine());
        }
    }


    #region "Audio"
    public void PlayCardClipByGender(int chairID, int iGender, int nCardIntValue)
    {
        Debug.Log("--------------PlayCardClipByGender: chairID=" + chairID + ", nCardIntValue=" + nCardIntValue);

        m_allPlayersUI[chairID].Audio.clip = AudioPlayGManager.getCardClipByGender(iGender % 2, GetCardValueInt((byte)nCardIntValue));
        m_allPlayersUI[chairID].Audio.Play();
    }

    //0 : peng 1: chi 2:gang 3: hu 4: zimo
    public void PlaySoundClipByGender(int chairID, int iGender, int iSoundID)
    {
        Debug.Log("--------------PlaySoundClipByGender: chairID=" + chairID + ", iSoundID=" + iSoundID);

        //use HuSoundClipAudioSource to fix 覆盖bug
        if (iSoundID == (int)AudioManager.Sound_Defines.SOUND_HU || iSoundID == (int)AudioManager.Sound_Defines.SOUND_ZIMO)
        {
            if (HuSoundClipAudioSource == null)
            {
                HuSoundClipAudioSource = gameObject.AddComponent<AudioSource>();//GlobalEffectAudioSource = UnityEngine.Object.Instantiate<AudioSource>(); //new AudioSource();
                HuSoundClipAudioSource.volume = m_soundEffectVolume;
            }
            HuSoundClipAudioSource.clip = AudioPlayGManager.getSoundByGender(iGender % 2, iSoundID);
            HuSoundClipAudioSource.Play();
        }
        else
        {
            m_allPlayersUI[chairID].Audio.clip = AudioPlayGManager.getSoundByGender(iGender % 2, iSoundID);
            m_allPlayersUI[chairID].Audio.Play();
        }
    }

    public void PlaySoundEffect(int chairID, int iEffectId)
    {
        if (true)///if(chairID == -1)
        {
            //use global effect
            if (GlobalEffectAudioSource == null)
            {
                GlobalEffectAudioSource = gameObject.AddComponent<AudioSource>();//GlobalEffectAudioSource = UnityEngine.Object.Instantiate<AudioSource>(); //new AudioSource();
                GlobalEffectAudioSource.volume = m_soundEffectVolume;
            }
            GlobalEffectAudioSource.clip = AudioPlayGManager.getEffectClip(iEffectId);
            GlobalEffectAudioSource.Play();
        }
        else
        {
            m_allPlayersUI[chairID].Audio.clip = AudioPlayGManager.getEffectClip(iEffectId);
            m_allPlayersUI[chairID].Audio.Play();
        }
    }
    public void PlayBGM()
    {
        int iBGMId = 0;
        //if (BackgroundAudioSource==null)
        //{
        //    BackgroundAudioSource = gameObject.AddComponent<AudioSource>();
        //    BackgroundAudioSource.volume = m_musicVolume;
        //}
        iBGMId = (int)(MersenneTwister.MT19937.Int63() % (int)AudioManager.Sound_BGM_Difines.BGM_Num);
        BackgroundAudioSource.Stop();
        BackgroundAudioSource.clip = AudioPlayGManager.getBGMClip(iBGMId);
        BackgroundAudioSource.loop = true;
        BackgroundAudioSource.Play();
        //StartCoroutine(BGMPlayCallBack(BackgroundAudioSource));
    }
    public void StopBGM()
    {
        BackgroundAudioSource.Stop();
    }
    IEnumerator BGMPlayCallBack(AudioSource bgm)
    {
        bgm.Play();
        yield return new WaitForSeconds(bgm.clip.length);
        PlayBGM();
    }
    #endregion

    public void SetZhuang(int chairID)
    {
        for (int i = 0; i < GameScene.MAX_PLAYER; i++)
        {
            if (i == chairID)
            {
                m_allPlayersUI[chairID].SeatNode.transform.Find("User/Banker").gameObject.SetActive(true);
            }
            else
            {
                m_allPlayersUI[chairID].SeatNode.transform.Find("User/Banker").gameObject.SetActive(false);
            }
        }
    }

    public void ShowOfflineUI(int chairID, bool bIsOffline)
    {
        if (chairID < 0 || chairID >= GameScene.MAX_PLAYER)
        {
            return;
        }

        if (m_allPlayersUI[chairID].m_breakBackObj == null && m_allPlayersUI[chairID].SeatNode != null)
        {
            m_allPlayersUI[chairID].m_breakBackObj = m_allPlayersUI[chairID].SeatNode.transform.Find("User/BreakBack").gameObject;
        }

        if (m_allPlayersUI[chairID].m_breakBackObj != null)
        {
            m_allPlayersUI[chairID].m_breakBackObj.SetActive(bIsOffline);
        }
    }

    public void ShowPlayerReadyUI(int chairID, bool bShow)
    {
        if (chairID < 0 || chairID >= GameScene.MAX_PLAYER)
        {
            return;
        }

        if (m_allPlayersUI[chairID].m_playerReadyObj == null && m_allPlayersUI[chairID].SeatNode != null)
        {
            m_allPlayersUI[chairID].m_playerReadyObj = m_allPlayersUI[chairID].SeatNode.transform.Find("User/ReadyBack").gameObject;
        }

        if (m_allPlayersUI[chairID].m_playerReadyObj != null)
        {
            m_allPlayersUI[chairID].m_playerReadyObj.SetActive(bShow);
        }
    }

    public void ShowHuInfoUI(int chairID, string strHuInfo, bool bShow, bool bNormalEnd = true)
    {
    }

    public void ShowHuScoreUI(int chairID, int score, bool bShow)
    {
    }

    public void GameStartProcedure(int saizi, int bankerUer, byte[] cardPrimeArray)
    {
        Debug.Log("Start game start!!!");

        StartCoroutine(StartGameCoroutine(saizi, bankerUer, cardPrimeArray));
    }

    public static int getNextPlayerChairID(int chairID)
    {
        return ((chairID - 1) + GameScene.MAX_PLAYER) % GameScene.MAX_PLAYER;
    }

    public void StartKaiPai()
    {
        UIState = GameUIState.UI_Starting;
    }

    IEnumerator StartGameCoroutine(int saizi, int bankerUser, byte[] cardPrimeArray)
    {
        Random.InitState((int)DateTime.Now.Ticks);
        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();

        int iStartIndex = (HNMJ_Defines.MAX_REPERTORY / HNMJ_Defines.GAME_PLAYER * bankerUser + saizi * 2) % HNMJ_Defines.MAX_REPERTORY;
        int weaveCount = 0;
        int curPlayer = bankerUser;
        for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
        {
            ResetCardCountMap(i);//重置牌数量map
        }
        for (int j = 0; j < GameScene.MAX_PLAYER; j++)
        {
            var buf = new byte[HNMJ_Defines.MAX_COUNT - 1];
            Buffer.BlockCopy(cardPrimeArray, curPlayer * HNMJ_Defines.MAX_COUNT, buf, 0, buf.Length);
            yield return new WaitForSeconds(0.2f);
            curPlayer = getNextPlayerChairID(curPlayer);

            iStartIndex = (iStartIndex + 4) % HNMJ_Defines.MAX_REPERTORY;
        }

        curTableIndex = (iStartIndex + (HNMJ_Defines.MAX_WEAVE - 2) * 16) % HNMJ_Defines.MAX_REPERTORY;//(cardOriIndexInTable + 48 - iOrderIndex * 3) % HNMJ_Defines.MAX_REPERTORY

        kernel.HNMJButtonAction_ShowCard();

        UIState = GameUIState.UI_Idle;
    }

    enum GameUIState
    {
        UI_Starting,
        UI_Playing,
        UI_Idle,
        UI_Max
    }
    //private bool bIsStarting = false; // 是否正在发牌
    GameUIState UIState = GameUIState.UI_Max;
    private int curTableIndex = 0;//当前麻将索引

    public void NewCardIn(int chairID, byte cardValueByte)
    {
    }

    IEnumerator BuHuaCoroutine(int chairID, byte cardValueByte)
    {
        Debug.Log("BuHuaCoroutine start" + GetCardValueInt(cardValueByte));

        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        var player = hnPlayers[(chairID)];
        int iIndex = player.ReplaceLogic(cardValueByte) - 1;
        SetHuaCardObj(chairID, cardValueByte, iIndex);
        //if (chairID == m_iLocalChairID)
        if (true)
        {
            ResetCardCountMap(chairID);
            player.showHandCard();
        }
        player.AnimDone();
        yield return null;
    }

    public void BuHua(int chairID, byte cardValueByte)
    {
        //StartCoroutine(BuHuaCoroutine(chairID, cardValueByte));
        StartCoroutine(BuHuaCoroutine(chairID, cardValueByte));
    }

    Transform[,] AllCurrentCards = new Transform[HNMJ_Defines.GAME_PLAYER, HNMJ_Defines.MAX_COUNT];

    public void ChaPai(int chariID, int kongIndex, int iInsertIndex, int iStartIndex, byte outcardByte)
    {
    }

    public void ShowLeftOutCard(int chariID, byte cardByte, int cardOutIndex)
    {
    }

    public void SendLastCard(int chairID, int cardIndex, int dataIndex, byte outcardByte)
    {
    }

    public void RemoveGangCardObjInQiangGangHu(byte cardByte)
    {
    }

    public void RemoveHandOutCardObj(byte cardByte)
    {
    }

    public void AnimEnd(int chairID)
    {
        m_allPlayersUI[chairID].AnimEnd = true;
    }

    public void SetAllCardsColor(Color newColor)
    {
    }

    public void smileButton()
    {
        gameplayUI.transform.Find("Window/Expression&WordWindow").gameObject.SetActive(true);
        Transform tg = gameplayUI.transform.Find("Window/Expression&WordWindow/ToggleGroup");
        tg.gameObject.SetActive(true);
        if (GameType == HNPrivateScenceBase.GAME_TYPE_13Shui)
        {
            tg.gameObject.SetActive(false);
        }
    }

    public static bool BReceivedHeartBeatMsg = true;
    private Coroutine heartBeatCoroutine = null;

    IEnumerator HeartBeat(float fTime)
    {
        while (BReceivedHeartBeatMsg)
        {
            BReceivedHeartBeatMsg = false;
            //Debug.Log("BReceivedHeartBeatMsg received");

            yield return new WaitForSecondsRealtime(fTime);
        }
        Debug.Log("BReceivedHeartBeatMsg receive failed!!!");

        Debug.LogError("disconnect9: HeartBeat cause disconnect!!!");
        ///yield break;

        CServerItem.get().IntermitConnect(true);
        //CServerItem.get().GetServerItemSocketEngine().disconnect();
        //CServerItem.get().SetServiceStatus(enServiceStatus.ServiceStatus_NetworkDown);
#if ApplyAutoReConnect
        if (m_cbGameEndReason != HNMJ_Defines.GER_NOT_END)//if (m_cbGameEndReason != HNMJ_Defines.GER_NOT_END || !m_bRoomStartGame)
        {
            Debug.LogError("HeartBeat LeaveGameToHall! m_cbGameEndReason="+ m_cbGameEndReason);
            LeaveGameToHall(true);
        }
#else
        LeaveGameToHall(true);

        Debug.Log("HeartBeat:GameType=" + GameType);
        m_bIsToHallByDisconnect = true;
        m_bIsToHallFrom13Shui = (GameType == HNPrivateScenceBase.GAME_TYPE_13Shui);
#endif

        //断线后发送，会得到异常，退出游戏界面
        // CServerItem.get().GetServerItemSocketEngine().send(Packet.MDM_KN_COMMAND, Packet.SUB_KN_DETECT_SOCKET, null, 0);

        heartBeatCoroutine = null;
    }

    void OnApplicationPause(bool bPause)
    {
        if (bEnteredGameScene)
        {
            if (bFakeServer == false)
            {
                Debug.Log("StartOrStopGameSceneHeartBeat received " + bPause);
                StartOrStopGameSceneHeartBeat(!bPause);
            }
        }
    }

    public void StartOrStopGameSceneHeartBeat(bool bStart)
    {
        ///return;

        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {
            return;
        }

        Debug.Log("StartOrStopGameSceneHeartBeat bStart= " + bStart);
        if (bStart)
        {
            if (heartBeatCoroutine == null)
            {
                BReceivedHeartBeatMsg = true;
                heartBeatCoroutine = StartCoroutine(HeartBeat(8.0f));
            }
        }
        else
        {
            if (heartBeatCoroutine != null)
            {
                StopCoroutine(heartBeatCoroutine);
                heartBeatCoroutine = null;
                BReceivedHeartBeatMsg = false;
            }
        }
    }
    public void ResetGameType()
    {
        GameType = HNPrivateScenceBase.GAME_TYPE_Null;
    }

    public void PlayAgain()
    {
        playAgain = true;
        ZeroPlayerScore();
        UserInfo.getInstance().reqAccountInfo();

        //ResetGameType();
        //ResetGameAfterDisconnect();

        if (CServerItem.get() != null)
        {
            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
            if (kernel != null)
            {
                kernel.defaultState();
            }

            CMD_GR_Again_Private kNetInfo = new CMD_GR_Again_Private();
            kNetInfo.Init();
            var buf = StructConverterByteArray.StructToBytes(kNetInfo);
            CServerItem.get().SendSocketData(GameServerDefines.MDM_GR_PRIVATE, GameServerDefines.SUB_GR_RIVATE_AGAIN, buf, (ushort)buf.Length);

            //重新载入新场景
            if(m_cbGameEndReason == HNMJ_Defines.GER_NORMAL)
            //if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
            {
                GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
                tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
                if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                {
                    pGlobalUserData.cbMapIndexRandForSingleGame = (byte)UnityEngine.Random.Range(0, 255); //(byte)(MersenneTwister.MT19937.Int63() % 255);
                    GameObjectsManager.GetInstance().LoadMap(pGlobalUserData.cbMapIndexRandForSingleGame);
                }
                else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                    GameObjectsManager.GetInstance().LoadMap(pGlobalUserData.cbMapIndexRand);

                DefaultState_HideSeek();
            }
        }
        else
        {
            Debug.LogError("PlayAgain:CServerItem.get()==null");
        }

        //int nRoomType = PlayerPrefs.GetInt("PubOrPrivate");
        //if (nRoomType == (int)RoomType.Type_Private && IsMeRoomCreator())
        //{
        //    ShowReadyButton(true);
        //}

        m_cbGameEndReason = HNMJ_Defines.GER_NOT_END;

        return;
    }

    IEnumerator showCardCoroutine(int chairID, byte[] cardData)
    {
        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        while (bResetLoaclCardsBeforeGameEnd == false)
        {
            yield return null;
        }
        for (int i = 0; i < cardData.Length; i++)
        {
            byte pTemp = cardData[i];
            ///Debug.Log(hnPlayers[chairID].getIdex() + "showCard player : card value: " + pTemp + " at index: " + i);
            if (pTemp > 0)
            {
                pTemp = (byte)HNGameManager.GetCardValueInt(pTemp);
                setPlayerCardObj(chairID, i, i + hnPlayers[chairID].getWeaveCount() * 3, pTemp, true);
                // WidgetFun::setImagic(pNode, utility::toString(kImagicFront, (int)cbColor, (int)cbValue, ".png"), false);
            }
        }
        hnPlayers[chairID].AnimDone();
    }
    public void GameEndShowCard(int chairID, byte[] cardData)
    {
        StartCoroutine(showCardCoroutine(chairID, cardData));
    }

    public bool bResetLoaclCardsBeforeGameEnd = false;

    public static void WechatPay(ref CMD_GP_PrePayIDInfo payInfo)
    {
        Debug.Log("get prepay id " + Encoding.Default.GetString(payInfo.szPrePayID));
        Debug.Log("szTimeStamp" + "--:" + Encoding.Default.GetString(payInfo.szTimeStamp));
        Debug.Log("szNonceStr" + "--:" + Encoding.Default.GetString(payInfo.szNonceStr));
        Debug.Log("szSign" + "--:" + Encoding.Default.GetString(payInfo.szSign));
        if (payInfo.cbStatusCode == 0)
        {
            GameSceneUIHandler.ShowLog("获取交易信息失败，请稍后再试！！");
            return;
        }
        // 参数  
        string[] mObject = new string[4];
        mObject[0] = Encoding.Default.GetString(payInfo.szPrePayID);
        mObject[1] = Encoding.Default.GetString(payInfo.szNonceStr);
        mObject[2] = Encoding.Default.GetString(payInfo.szTimeStamp);
        mObject[3] = Encoding.Default.GetString(payInfo.szSign);
        Loom.QueueOnMainThread(() =>
        {

#if UNITY_ANDROID && !UNITY_EDITOR
            //AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            //AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            //jo.Call("PayShopItems", mObject);
#elif UNITY_IOS
			//PayShopItems(mObject[0],mObject[1],mObject[2],mObject[3]);
#endif
        });

    }
#if UNITY_IPHONE
	//[DllImport("__Internal")]  
	//private static extern void PayShopItems(string prePayID,string nonceStr,string timestamp,string sign);
#endif

    public void PayResponse(string payResult)
    {
        Debug.Log("pay called back" + payResult);
        if (payResult == "Canceld")
        {
            GameSceneUIHandler.ShowLog("用户取消微信支付！");
        }
        else if (payResult == "Failed")
        {
            GameSceneUIHandler.ShowLog("微信支付失败，请稍后再试！");
        }
        else if (payResult == "Success")
        {
            UserInfo.getInstance().ClientPayInfo();
        }
    }

    public void SaveUserInfo(ref PlayBackStorage.UserInfoStorage[] users)
    {
        var player = hnPlayers[m_iLocalChairID];
        users[0] = new PlayBackStorage.UserInfoStorage(player.GetNickName(),
            player.GetGender(), player.GetUserID());
        var curPlayerChairID = getNextPlayerChairID(m_iLocalChairID);
        int iIndex = 1;
        while (curPlayerChairID != m_iLocalChairID)
        {
            player = hnPlayers[curPlayerChairID];
            users[iIndex++] = new PlayBackStorage.UserInfoStorage(player.GetNickName(),
            player.GetGender(), player.GetUserID());
            curPlayerChairID = getNextPlayerChairID(curPlayerChairID);
        }
    }

    void SetRecordRoomInfo(int idx)
    {
        var recordStruct = PlayBackStorage.GetInstance().GetRecord(idx);

        RoomId.text = recordStruct.roomID.ToString();
        PlayCoutLabel.text = (recordStruct.iMatchIdx + " / " + recordStruct.totalCount);

        m_baseScore = 1;
    }

    public static bool bFakeServer = false;//回放时为true，无需断线检测
    private bool bPausePlayBack;
    public Sprite[] PauseOrPlay;
    public void StartFakeReply(int iRecordIdx, int imatchIdx)
    {
        //进入游戏场景
        bFakeServer = true;
        GameType = HNPrivateScenceBase.GAME_TYPE_JianDe;
        GameManagerBaseNet.InstanceBase().StartFakeGame(GameScene.KIND_ID_JianDe);

        var btnShowOrHide = PlayBackPauseOrPlay.GetComponent<Button>();
        btnShowOrHide.onClick.RemoveAllListeners();
        var btn = PlayBackPauseOrPlay.transform.Find("PauseOrPlay").GetComponent<Button>();
        btnShowOrHide.onClick.AddListener(() =>
        {
            btn.gameObject.SetActive(!btn.gameObject.activeSelf);
        });

        btn.onClick.RemoveAllListeners();
        btn.GetComponentInChildren<Image>().sprite = PauseOrPlay[0];
        btn.onClick.AddListener(() =>
        {
            if (btn.GetComponentInChildren<Image>().sprite == PauseOrPlay[0])
            {
                Time.timeScale = 0.0f;
                btn.GetComponentInChildren<Image>().sprite = PauseOrPlay[1];
            }
            else
            {
                Time.timeScale = 1.0f;
                btn.GetComponentInChildren<Image>().sprite = PauseOrPlay[0];
            }
        });

        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();

        ((CServerItem)CServerItem.get()).FakeUserEnter(iRecordIdx);
        for (int i = 0; i < 4; i++)
        {
            hnPlayers[i] = kernel.getPlayerByChairID(i);
        }
        PlayBackStorage.GetInstance().SetMatchIdx(iRecordIdx, imatchIdx);
        SetRecordRoomInfo(iRecordIdx);
        FakeGameCoroutine = StartCoroutine(FakeTickGame(iRecordIdx));
    }

    private Coroutine FakeGameCoroutine = null;
    public Slider RecordSlider;
    public GameObject PlayBackPauseOrPlay;

    IEnumerator FakeTickGame(int iRecordIdx)
    {
        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        var RecordRate = 0.0f;
        do
        {
            var bNoAnim = true;
            do
            {
                bNoAnim = true;
                for (int i = 0; i < 4; i++)
                {
                    if (hnPlayers[i].AnimCount() > 0)
                    {
                        bNoAnim = false;
                    }
                }
                yield return new WaitForSeconds(0.05f);
            } while (!bNoAnim);
            PlayBackStorage.GameMsgEvent gameMsg;
            RecordRate = PlayBackStorage.GetInstance().TickGameMsg(out gameMsg);
            RecordSlider.value = RecordRate;
            if (RecordRate != 1.0f)
            {
                Debug.Log("show msg: " + gameMsg.sub);
                kernel.OnEventGameMessage(gameMsg.sub, gameMsg.data, gameMsg.dataSize);
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                break;
            }
        } while (true);
        Debug.Log("Record Reach end!");
    }

    //mChen add, for HideSeek
    private Coroutine waitCoroutine = null;
    private Coroutine hideCoroutine = null;
    private Coroutine playCoroutine = null;
    private Coroutine playAgainCoroutine = null;
    public void OnGameStatus(byte cbGameStatus)
    {
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        switch (cbGameStatus)
        {
            case SocketDefines.GAME_STATUS_FREE:
                Debug.Log("OnSocketSubGameStatus:cbGameStatus= GAME_STATUS_FREE");
                Loom.QueueOnMainThread(() =>
                {
                    if (UIManager.GetInstance() != null)
                    {
                        UIManager.GetInstance().ShowTopTips("", false);
                        //UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart0").GetComponent<Image>().overrideSprite = Resources.Load("UI/Hearts/Pink", typeof(Sprite)) as Sprite;
                    }
                });
                break;

            case SocketDefines.GAME_STATUS_WAIT:
                Debug.Log("OnSocketSubGameStatus:cbGameStatus= GAME_STATUS_WAIT");
                Loom.QueueOnMainThread(() =>
                {
                    //开始播放BGM
                    PlayBGM();
                    UIManager.GetInstance().ShowManualControlTips("", false);
                    if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                    {
                        SetLoading(false);    //显示Loading界面
                        waitCoroutine = StartCoroutine(WaitCoroutine_SingleGame(GameManager.HIDESEEK_WAIT_GAME_TIME));
                        if (GameManager.GetInstance() != null)
                        {
                            if (UIManager.GetInstance() != null)
                                UIManager.GetInstance().m_Canvas.transform.Find("Btn/PersonButton").gameObject.SetActive(false);
                            GameManager.GetInstance().InitSingleGame();
                        }
                    }
                    if (UIManager.GetInstance() != null)
                    {
                        UIManager.GetInstance().ShowTopTips("等待游戏开始...", true);
                        if (localHuman != null && localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                        {
                            UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(false);
                            UIManager.GetInstance().m_Canvas.transform.Find("Btn/PersonButton").gameObject.SetActive(false);
                        }
                        else if (localHuman != null && localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                        {
                            UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(false);
                            UIManager.GetInstance().m_Canvas.transform.Find("Btn/PersonButton").gameObject.SetActive(false);
                        }
                    }
                    if (GameManager.GetInstance() != null)
                    {
                        GameManager.GetInstance().CloseDoor();
                    }
                    else
                        Debug.Log("无法获取UIManager单例");
                });
                break;

            case SocketDefines.GAME_STATUS_HIDE:
                Debug.Log("OnSocketSubGameStatus:cbGameStatus= GAME_STATUS_HIDE");
                UIManager.GetInstance().ShowManualControlTips("", false);
                if (localHuman != null && localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                {
                    GameSceneUIHandler.ShowLog("躲藏队");
                    UIManager.GetInstance().ShowMiddleTips("在倒计时结束前躲起来！", 3);
                }
                else
                {
                    GameSceneUIHandler.ShowLog("搜查队");
                    UIManager.GetInstance().ShowMiddleTips("记住场景中大致物品摆放而寻找！", 3);
                }
                Loom.QueueOnMainThread(() =>
                {
                    if (Loading != null)
                    {
                        SetLoading(false);
                    }
                    if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                    {
                        SetLoading(false);  //隐藏Loading界面
                        hideCoroutine = StartCoroutine(HideCoroutine_SingleGame(GameManager.HIDESEEK_HIDING_TIME));

                        //if (GameManager.GetInstance() != null)
                        //{
                        //    GameManager.GetInstance().InitSingleGame();
                        //}
                    }

                    ////旁观用户强制显示躲藏者
                    //IClientUserItem pMeItem = CServerItem.get().GetMeUserItem();
                    //if (pMeItem != null)
                    //{
                    //    int nStatus = pMeItem.GetUserStatus();
                    //    if (nStatus == SocketDefines.US_LOOKON)
                    //    {
                    //        Loom.QueueOnMainThread(() =>
                    //        {
                    //            if (GameManager.GetInstance() != null)
                    //            {
                    //                GameManager.GetInstance().SetHiderVisible(true);
                    //            }
                    //        }, 2); //延时两秒执行
                    //    }
                    //}

                    if (GameManager.GetInstance() != null)
                    {
                        if (GameManager.GetInstance().SetLookOn())
                        {
                            if (!UIManager.GetInstance()._showRoomUsersBtn.activeSelf)
                                UIManager.GetInstance()._showRoomUsersBtn.SetActive(true);
                            return;
                        }
                    }
                    else
                        Debug.Log("无法获取GameManager单例");

                    if (localHuman != null && localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                    {
                        if (UIManager.GetInstance() != null)
                        {
                            UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(false);
                            if (localHuman.Hp != 0)
                            {
                                UIManager.GetInstance().m_Canvas.transform.Find("Btn/ObjectSwitch").gameObject.SetActive(true);  //模型切换
                                UIManager.GetInstance().m_Canvas.transform.Find("Btn/StealthButton").gameObject.SetActive(false);
                            }
                        }
                        else
                            Debug.Log("无法获取UIManager单例");
                        ControlManager.GetInstance().ControlModelSwitch(ControlManager.CameraControlMode.PlayerViewMode);  //切换为玩家视角
                        Loom.QueueOnMainThread(()=>
                        {
                            if (GameManager.GetInstance() != null)
                            {
                                GameManager.GetInstance().SetAllHiderVisible(true);
                            }
                            else
                            {
                                Debug.Log("无法获取GameManager单例");
                            }
                        }, 2); //延时两秒执行
                    }
                    else if (localHuman != null && localHuman.Hp != 0 && localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                    {
                        if (UIManager.GetInstance() != null)
                        {
                            UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(true);
                        }
                        else
                            Debug.Log("无法获取UIManager单例");
                        //ControlManager.GetInstance().ControlModelSwitch(ControlManager.CameraControlMode.FreeCamereMode);  //切换为摄像机模式
                        if (GameManager.GetInstance() != null)
                        {
                            GameManager.GetInstance().SetAllHiderVisible(false);
                        }
                        else
                        {
                            Debug.Log("无法获取GameManager单例");
                        }
                    }
                    else
                        Debug.Log("无法获取单例");


                    if (UIManager.GetInstance() != null)
                    {
                        if (UIManager.GetInstance()._backMatchButton != null)
                            UIManager.GetInstance()._backMatchButton.SetActive(true);
                        UIManager.GetInstance()._showRoomUsersBtn.SetActive(true);
                        UIManager.GetInstance().ShowTopTips("躲藏阶段...", true);
                    }
                    else
                    {
                        Debug.Log("无法获取UIManager单例");
                    }

                    //GameObjectsManager.GetInstance().EnableLocalAI(PlayerTeam.PlayerTeamType.HideTeam, true);
                });
                break;

            case SocketDefines.GAME_STATUS_PLAY:
                Debug.Log("OnSocketSubGameStatus:cbGameStatus= GAME_STATUS_PLAY");
                UIManager.GetInstance().ShowManualControlTips("", false);
                UIManager.TimeLeft = GameManager.HIDESEEK_GAME_PLAY_TIME;//fix有时断线时，TimeLeft的值不对的bug

                Loom.QueueOnMainThread(() =>
                {
                    ////生成道具
                    //InventoryManager.GetInstane.InventoryInit();

                    if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                    {
                        if (UIManager.GetInstance() != null)
                            UIManager.GetInstance().m_Canvas.transform.Find("Btn/PersonButton").gameObject.SetActive(true);
                        playCoroutine = StartCoroutine(PlayCoroutine_SingleGame(GameManager.HIDESEEK_GAME_PLAY_TIME));
                    }
                    //mChen note:加两层Loom是为了保证这个在GameScene::CreatePlayer->team.AddAPlayer之后调用
                    //           目前是通过修改服务端来解决：在SUB_GF_GAME_OPTION中将SendUserInfoPacket移到send SUB_GF_GAME_STATUS之前
                    //Loom.QueueOnMainThread(() =>
                    //{
                    if (GameManager.GetInstance() != null)
                    {
                        GameManager.GetInstance().OpenDoor();
                        GameManager.GetInstance().SetAllHiderVisible(true);  //显示躲藏者
                        if (GameManager.GetInstance().SetLookOn())
                        {
                            UIManager.GetInstance().m_Canvas.transform.Find("Btn/PersonButton").gameObject.SetActive(false);
                            if (!UIManager.GetInstance()._showRoomUsersBtn.activeSelf)
                                UIManager.GetInstance()._showRoomUsersBtn.SetActive(true);
                            SetLoading(false);
                            return;
                        }

                    }
                    else
                        Debug.Log("无法获取GameManager单例");
                    if (UIManager.GetInstance() != null)
                    {
                        UIManager.GetInstance().ShowTopTips("游戏阶段...", true);
                    }
                    else
                    {

                    }
                    if (localHuman != null && localHuman.Hp != 0 && localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                    {
                        if (UIManager.GetInstance() != null)
                        {
                            UIManager.GetInstance().m_Canvas.transform.Find("Btn/PersonButton").gameObject.SetActive(true);
                            UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(true);
                        }
                        ControlManager.GetInstance().ControlModelSwitch(ControlManager.CameraControlMode.PlayerViewMode);  //切换为玩家模式
                    }
                    else if(localHuman != null && localHuman.Hp != 0 && localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                    {
                        if (UIManager.GetInstance() != null)
                        {
                            UIManager.GetInstance().m_Canvas.transform.Find("Btn/PersonButton").gameObject.SetActive(false);
                            UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(false);
                            if (localHuman.Hp != 0)
                            {
                                UIManager.GetInstance().m_Canvas.transform.Find("Btn/ObjectSwitch").gameObject.SetActive(true);  //模型切换
                                UIManager.GetInstance().m_Canvas.transform.Find("Btn/StealthButton").gameObject.SetActive(true);
                            }
                        }
                    }
                    ControlManager.m_Down = false;
                    ControlManager.m_Up = false;

                    GameObjectsManager.GetInstance().EnableLocalAI(PlayerTeam.PlayerTeamType.TaggerTeam, true);

                    //});
                });
                break;

            case SocketDefines.GAME_STATUS_END:
                UIManager.GetInstance().ShowManualControlTips("", false);
                Debug.Log("OnSocketSubGameStatus:cbGameStatus= GAME_STATUS_END");
                Loom.QueueOnMainThread(() =>
                {
                    if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                    {
                        UIManager.GetInstance().ShowWinOrLose();
                        StopSingleGame();
                        playAgainCoroutine = StartCoroutine(PlayAgain_SingleGame(2f));
                    }
                    //游戏中聊天信息清零
                    chatSystem.TextClear();
                });
                break;

            default:
                Debug.Log("OnSocketSubGameStatus:cbGameStatus= " + cbGameStatus);
                break;
        }
    }
    public void StopSingleGame() 
    {
        GameObjectsManager.GetInstance().ClearPlayers();
        StopSingleGameCoroutine();
    }
    public void PlayAgainSingleGame()
    {
        //gamePlayUI_HT.transform.Find("LockButton").GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/Red", typeof(Sprite)) as Sprite;
        //gamePlayUI_HT.transform.Find("LockButton/Image").GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/Gold", typeof(Sprite)) as Sprite;
        gamePlayUI_HT.transform.Find("Btn/LockButton").GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/UnLock", typeof(Sprite)) as Sprite;

        playAgainCoroutine = StartCoroutine(PlayAgain_SingleGame(2f));
    }
    private void StopSingleGameCoroutine()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        if (playAgainCoroutine != null)
        {
            StopCoroutine(playAgainCoroutine);
            playAgainCoroutine = null;
        }
    }
    IEnumerator WaitCoroutine_SingleGame(float fTime)
    {
        GameManager.s_singleGameStatus = SocketDefines.GAME_STATUS_WAIT;
        while (fTime >= 0f)
        {
            string strMsg = String.Format("Wait time [{0}]", fTime);
            GameSceneUIHandler.ShowLog(strMsg);
            if (fTime == 5 || fTime == 4 || fTime == 3 || fTime == 2 || fTime == 1)
            {
                PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_TIME_SEC);
            }
            yield return new WaitForSecondsRealtime(1.0f);

            fTime = fTime - 1.0f;
        }
        GameManager.s_singleGameStatus = SocketDefines.GAME_STATUS_HIDE;
        OnGameStatus(SocketDefines.GAME_STATUS_HIDE);
    }
    IEnumerator HideCoroutine_SingleGame(float fTime)
    {
        while (fTime >= 0f)
        {
            string strMsg = String.Format("Hide time [{0}]", fTime);
            GameSceneUIHandler.ShowLog(strMsg);
            if (fTime == 45)
            {
                PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_TIPS_STARTOREND);
            }
            else if (fTime == 5 || fTime == 4 || fTime == 3 || fTime == 2 || fTime == 1)
            {
                PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_TIME_SEC);
            }
            yield return new WaitForSecondsRealtime(1.0f);

            fTime = fTime - 1.0f;
        }
        GameManager.s_singleGameStatus = SocketDefines.GAME_STATUS_PLAY;
        OnGameStatus(SocketDefines.GAME_STATUS_PLAY);
    }
    IEnumerator PlayCoroutine_SingleGame(float fTime)
    {
        while (fTime >= 0f)
        {
            string strMsg = String.Format("Play time [{0}]", fTime);
            GameSceneUIHandler.ShowLog(strMsg);
            if (fTime == 45)
            {
                UIManager.GetInstance().ShowMiddleTips("警察进入无敌状态");
                PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_TIPS_STARTOREND);
            }
            else if (fTime == 100)
            {
                PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_TIPS_STARTOREND);
            }
            else if (fTime == 200)
            {
                PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_TIPS_START_SEEK);
            }
            else if (fTime == 5 || fTime == 4 || fTime == 3 || fTime == 2 || fTime == 1)
            {
                PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_TIME_SEC);
            }
            yield return new WaitForSecondsRealtime(1.0f);
            if (fTime == GameManager.HIDESEEK_GAME_PLAY_TIME || fTime == 100)
                if (InventoryManager.GetInstane != null)
                    InventoryManager.GetInstane.InventoryInit();
            fTime = fTime - 1.0f;
        }
        GameManager.s_singleGameStatus = SocketDefines.GAME_STATUS_END;
        OnGameStatus(SocketDefines.GAME_STATUS_END);
    }
    IEnumerator PlayAgain_SingleGame(float fTime)
    {
        yield return new WaitForSecondsRealtime(fTime);
        GameManager.s_singleGameStatus = SocketDefines.GAME_STATUS_WAIT;
        OnGameStatus(SocketDefines.GAME_STATUS_WAIT);
    }
    public void OnSocketGFSubAICreate(byte[] data, ushort wDataSize)
    {
        Loom.QueueOnMainThread(() =>
        {
            StartCoroutine(CreateAIsCoroutine(data, wDataSize));
        });
    }

    IEnumerator CreateAIsCoroutine(byte[] data, ushort wDataSize)
    {
        GameManager gameMgr = GameManager.GetInstance();
        while (gameMgr == null)
        {
            yield return new WaitForSecondsRealtime(1.0f);

            gameMgr = GameManager.GetInstance();
        }

        gameMgr.OnSubAICreateInfo_WangHu(data, wDataSize);
    }
    public void OnSocketSubInventoryCreate()
    {
        Loom.QueueOnMainThread(() =>
        {
            StartCoroutine(CreateInventoryCoroutine());
        });

    }
    IEnumerator CreateInventoryCoroutine()
    {
        //直到场景加载完
        InventoryManager inventoryManager = InventoryManager.GetInstane;
        while (inventoryManager == null)
        {
            yield return new WaitForSecondsRealtime(1.0f);

            inventoryManager = InventoryManager.GetInstane;
        }

        inventoryManager.InventoryInit();

    }

    public void OnApplicationQuit() 
    {
        Debug.Log("HNGameManager:quit application now");

        StartOrStopGameSceneHeartBeat(false);
        LeaveRoom();
        LeaveGameToHall(); 
        //CServerItem.get().IntermitConnect(true);

    }

    //经验系统
    public void ExperienceSystem()
    {
        Image ExpNum = Exp.transform.Find("Background/Fill").GetComponent<Image>();
        Text ExpNumTxt = Exp.transform.Find("Background/Level").GetComponent<Text>();
        //Debug.Log("------ExperienceSystem  当前经验值：" + GlobalUserInfo.getUserExp().ToString());
        if (ExpNum != null)
            ExpNum.fillAmount = (GlobalUserInfo.getUserExp() % 100) / 100.0f;
        if (ExpNumTxt != null)
        {
            ExpNumTxt.text = String.Format("等级: {0}", GlobalUserInfo.getUserExp() / 100);
        }
    }
    #region  聊天系统
    //显示聊天文字
    public void ShowChatPanel(int id, byte status, string str)
    {
        chatSystem.ShowChatPanel(id, status, str);
    }
    #endregion
    #region  Loading画面切换
    public void SetLoading(bool visible)
    {
        if(visible)
        {
            gamePlayUI_HT.GetComponent<Canvas>().sortingOrder = 9;
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            int mapLoadingIndex = 0;
            if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                mapLoadingIndex = pGlobalUserData.cbMapIndexRandForSingleGame % (int)CreateObjectManager.MapType.MapNum;
            else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                mapLoadingIndex = pGlobalUserData.cbMapIndexRand % (int)CreateObjectManager.MapType.MapNum;
            Debug.Log("----------------------mapLoadingIndex: " + mapLoadingIndex);
            Loading.GetComponent<Image>().overrideSprite = Resources.Load("UI/LoadingImage/"+ mapLoadingIndex, typeof(Sprite)) as Sprite;
        }
        else
            gamePlayUI_HT.GetComponent<Canvas>().sortingOrder = 1;
        Loading.SetActive(visible);
    }
    #endregion
    #region 微信分享模块
#if UNITY_ANDROID
    static string urlShare = "http://www.cat-fun.com/";
    static string urlEmpty = "";
#elif UNITY_IOS
    static string urlShare = "http://www.cat-fun.com/";
	static string urlEmpty = "";
#else
    static string urlShare = "";
    static string urlEmpty = "";
#endif
    public static void Share(Platform p)
    {
        Social.ShareDelegate sharecallback =
            delegate (Platform platform, int stCode, string errorMsg)
            {
                if (stCode == Social.SUCCESS)
                {
                    Debug.Log("分享成功");
                }
                else
                {

                    Debug.Log("分享失败 : " + errorMsg);
                }
            };

        //CaptureScreenshot2(new Rect(0, 0, Screen.width, Screen.height));
        //imagePath如果为本地文件 只支持 Application.persistentDataPath下的文件
        //例如 Application.persistentDataPath + "/" +"你的文件名"
        //如果想分享 Assets/Resouces的下的 icon.png 请前使用 Resources.Load() 和 FileStream 写到 Application.persistentDataPath下
        if (File.Exists(Application.persistentDataPath + "/ShareIcon.png") == false)
        {
            Texture2D icon = Resources.Load<Texture2D>("UI/Icons/icon_share");
            // 然后将这些纹理数据，成一个png图片文件  
            byte[] bytes = icon.EncodeToPNG();
            string filename = Application.persistentDataPath + "/Sceenshot.png";
            File.WriteAllBytes(filename, bytes);
        }
        Social.DirectShare(p, "快来玩开心躲猫猫吧！", Application.persistentDataPath + "/ShareIcon.png", "开心躲猫猫", urlShare, sharecallback);
    }

    //public static void ShareMatchResult()
    //{
    //    Social.ShareDelegate sharecallback =
    //        delegate (Platform platform, int stCode, string errorMsg)
    //        {
    //            if (stCode == Social.SUCCESS)
    //            {
    //                Debug.Log("分享战绩成功");
    //            }
    //            else
    //            {

    //                Debug.Log("分享战绩失败 : " + errorMsg);
    //            }
    //        };

    //    CaptureScreenshot(new Rect(0, 0, Screen.width, Screen.height));

    //    Social.DirectShare(Platform.WEIXIN, "快来膜拜我的战绩吧！哈哈哈！", Application.persistentDataPath + "/Sceenshot.png", "新安棋牌", urlEmpty, sharecallback);
    //}

    //static Texture2D CaptureScreenshot(Rect rect)
    //{
    //    // 先创建一个的空纹理，大小可根据实现需要来设置  
    //    Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);

    //    // 读取屏幕像素信息并存储为纹理数据，  
    //    screenShot.ReadPixels(rect, 0, 0);
    //    screenShot.Apply();

    //    // 然后将这些纹理数据，成一个png图片文件  
    //    byte[] bytes = screenShot.EncodeToPNG();
    //    string filename = Application.persistentDataPath + "/Sceenshot.png";
    //    System.IO.File.WriteAllBytes(filename, bytes);
    //    Debug.Log(string.Format("截屏了一张图片: {0}", filename));

    //    // 最后，我返回这个Texture2d对象，这样我们直接，所这个截图图示在游戏中，当然这个根据自己的需求的。  
    //    return screenShot;
    //}

    //public void InviteFriends()
    //{
    //    Social.ShareDelegate sharecallback =
    //        delegate (Platform platform, int stCode, string errorMsg)
    //        {
    //            if (stCode == Social.SUCCESS)
    //            {
    //                Debug.Log("邀请已发出！");
    //            }
    //            else
    //            {

    //                Debug.Log("邀请失败: " + errorMsg);
    //            }
    //        };

    //    //imagePath如果为本地文件 只支持 Application.persistentDataPath下的文件
    //    //例如 Application.persistentDataPath + "/" +"你的文件名"
    //    //如果想分享 Assets/Resouces的下的 icon.png 请前使用 Resources.Load() 和 FileStream 写到 Application.persistentDataPath下
    //    if (File.Exists(Application.persistentDataPath + "/ShareIcon.png") == false)
    //    {
    //        Texture2D icon = Resources.Load<Texture2D>("Icons/icon_share");
    //        // 然后将这些纹理数据，成一个png图片文件  
    //        byte[] bytes = icon.EncodeToPNG();
    //        string filename = Application.persistentDataPath + "/Sceenshot.png";
    //        File.WriteAllBytes(filename, bytes);
    //    }
    //    if (GameType == HNPrivateScenceBase.GAME_TYPE_JianDe)
    //    {
    //        Social.DirectShare(Platform.WEIXIN, string.Format("底分：{0}，{1}局，4人开，别犹豫，就等你啦！", m_baseScore, totalcount), Application.persistentDataPath + "/ShareIcon.png", string.Format("新安棋牌【麻将房间号：{0}】", TempRoomId), urlShare, sharecallback);
    //    }
    //    else
    //    {
    //        Social.DirectShare(Platform.WEIXIN, string.Format("底分：{0}，{1}局，4人开，别犹豫，就等你啦！", m_baseScore, totalcount), Application.persistentDataPath + "/ShareIcon.png", string.Format("新安棋牌【十三水房间号：{0}】", TempRoomId), urlShare, sharecallback);
    //    }
    //}
    #endregion
}
