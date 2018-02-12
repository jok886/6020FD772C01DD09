using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using GameNet;
using System;

public class UIManager : MonoBehaviour
{
    // 单例
    private static UIManager _instance = null;
    public static UIManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = (UIManager)GameObject.FindObjectOfType(typeof(UIManager));
            if (!_instance)
            {
                ///Debug.LogError("There needs to be one active UIManager script on a GameObject in your scene.");
            }
        }
        return _instance;
    }

    private GameObject _loseUIPopup;
    private Text _pickupTooFarText;
    private GameObject _pickupWrongUI;

    // Multi UI
    public GameObject m_Canvas;
    private GameObject _matchStartCountDownUI;
    private Text _matchStartCountDownText;
    private GameObject _joinMatchButton;
    public GameObject _leaveMatchButton;
    public GameObject _backMatchButton;
    public GameObject _showRoomUsersBtn;
    public GameObject _MiddleTips;
    public GameObject _ManualControlTips;
    public GameObject _StatusTips;
    public GameObject ControlSpriteR;
    public GameObject LosePopup;
    public GameObject WinPopup;
    public GameObject _Glod;

    public static GameObject TimeTip;
    private Text HideCount;
    private Text TaggerCount;
    public static int TimeLeft = 0;
    //public static float coldTime = 0;

    private HNGameManager m_hnGameManager = null;

    void Awake()
    {
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log("UIManager Start()");
        if (m_hnGameManager == null)
            m_hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
        m_Canvas = GameObject.Find("CanvasGamePaly_demo");
        _loseUIPopup = GameObject.Find("LosePopup");
        HideLoseUI();

        GameObject pickupTooFarTextObj = GameObject.Find("PickupTooFarText");
        //_pickupTooFarText = pickupTooFarTextObj.GetComponent<Text>();
        HidePickupTooFarText();

        _pickupWrongUI = GameObject.Find("PickupWrongUI");
        HidePickupWrongUI();


        // Multi UI
        _matchStartCountDownUI = GameObject.Find("MatchStartCountDown");
        //WQ changge
        //_matchStartCountDownText = _matchStartCountDownUI.GetComponent<Text>();
        //_matchStartCountDownUI.active = false;

        //_joinMatchButton = GameObject.Find("JoinMatchButton");
        //_joinMatchButton.active = false;
        _leaveMatchButton = GameObject.Find("LeaveMatchButton");
        if (_leaveMatchButton != null)
            _leaveMatchButton.SetActive(true);
        _showRoomUsersBtn = GameObject.Find("ShowRoomUsersBtn");
        if (_showRoomUsersBtn != null)
            _showRoomUsersBtn.SetActive(false);
        _backMatchButton = m_Canvas.transform.Find("BackMatchButton").gameObject;
        if (_backMatchButton != null)
            _backMatchButton.SetActive(true);
        _MiddleTips = m_Canvas.transform.Find("MiddleTips").gameObject;
        if (_MiddleTips != null)
            _MiddleTips.SetActive(false);
        _ManualControlTips = m_Canvas.transform.Find("ManualControlTips").gameObject;
        if (_ManualControlTips != null)
            _ManualControlTips.SetActive(false);
        _Glod = m_Canvas.transform.Find("Glod").gameObject;
        if (_Glod != null)
            _Glod.SetActive(true);

        _StatusTips = m_Canvas.transform.Find("StatusTips").gameObject;
        if (_StatusTips != null)
            _StatusTips.SetActive(false);

        TimeTip = m_Canvas.transform.Find("Time/TimeCount").gameObject;
        HideCount = m_Canvas.transform.Find("Time/UserCountBackground/HideCount").GetComponent<Text>();
        TaggerCount = m_Canvas.transform.Find("Time/UserCountBackground/TaggerCount").GetComponent<Text>();

        ControlSpriteR = m_Canvas.transform.Find("ControlSpriteR").gameObject;
        Sprite ctrName = Resources.Load<Sprite>("UI/Joystick/Conctrol");
        if (ControlSpriteR != null)
            ControlSpriteR.GetComponent<Image>().sprite = ctrName;
        //GameStatusDeal();
        //输赢窗口
        LosePopup = m_Canvas.transform.Find("LosePopup").gameObject;
        WinPopup = m_Canvas.transform.Find("WinPopup").gameObject;
    }

    //public void GameStatusDeal()
    //{
    //    byte Gamestate = CServerItem.get().GetGameStatus();
    //    Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
    //    switch (Gamestate)
    //    {
    //        case SocketDefines.GAME_STATUS_WAIT:
    //            Loom.QueueOnMainThread(() =>
    //            {
    //                if (localHuman != null && localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
    //                {
    //                    if (UIManager.GetInstance() != null)
    //                    {
    //                        UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(false);
    //                    }
    //                    else
    //                        Debug.Log("无法获取单例");
    //                }
    //                else if (localHuman != null && localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
    //                {
    //                    if (UIManager.GetInstance() != null)
    //                    {
    //                        UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(true);
    //                        //UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart0").GetComponent<Image>().overrideSprite = Resources.Load("UI/Hearts/Pink", typeof(Sprite)) as Sprite;
    //                    }
    //                    else
    //                        Debug.Log("无法获取单例");
    //                }
    //                else
    //                    Debug.Log("无法获取单例");
    //            });
    //            break;
    //        case SocketDefines.GAME_STATUS_HIDE:
    //            Loom.QueueOnMainThread(() =>
    //            {
    //                if (localHuman != null && localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
    //                {
    //                    if (UIManager.GetInstance() != null)
    //                    {
    //                        UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(false);
    //                    }
    //                    else
    //                        Debug.Log("无法获取单例");
    //                }
    //                else if (localHuman != null && localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
    //                {
    //                    if (UIManager.GetInstance() != null)
    //                    {
    //                        UIManager.GetInstance().m_Canvas.transform.Find("Hearts").gameObject.SetActive(true);
    //                        //UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart0").GetComponent<Image>().overrideSprite = Resources.Load("UI/Hearts/Pink", typeof(Sprite)) as Sprite;
    //                    }
    //                    else
    //                        Debug.Log("无法获取单例");
    //                }
    //                else
    //                    Debug.Log("无法获取单例");

    //                if (UIManager.GetInstance() != null)
    //                {
    //                    if (UIManager.GetInstance()._backMatchButton != null)
    //                        UIManager.GetInstance()._backMatchButton.SetActive(true);
    //                }
    //                else
    //                {
    //                    Debug.Log("无法获取单例");
    //                }
    //            });
    //            break;
    //        case SocketDefines.GAME_STATUS_PLAY:
    //            break;
    //        default:
    //            break;
    //    }
    //}
    public void OnJoinMatchButtonClick()
    {
        //_joinMatchButton.active = false;
        if (_leaveMatchButton != null)
            _leaveMatchButton.SetActive(true);
        if (_showRoomUsersBtn != null)
            _showRoomUsersBtn.SetActive(true);

        GameManager.s_NetWorkClient.TryToJoinMatch();
    }

    public void OnLeaveMatchButtonClick()
    {
        if (_leaveMatchButton != null)
            _leaveMatchButton.SetActive(false);
        if (_showRoomUsersBtn != null)
            _showRoomUsersBtn.SetActive(false);

        if (m_hnGameManager == null)
        {
            m_hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
        }

        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {
            m_hnGameManager.StopSingleGame();
            m_hnGameManager.LeaveGameToHall();
            return;
        }

        int nStatus = SocketDefines.US_NULL;
        IClientUserItem pMeItem = null;
        if (CServerItem.get() != null)
        {
            pMeItem = CServerItem.get().GetMeUserItem();
        }
        if (pMeItem != null)
        {
            nStatus = pMeItem.GetUserStatus();
        }
        if (true)//if (nStatus == SocketDefines.US_LOOKON)
        {
            m_hnGameManager.PopLeaveRoomWindow();
        }
        else
        {
            //_joinMatchButton.active = true;
            m_hnGameManager.DismissBtnClicked();
            if (GameManager.s_NetWorkClient != null)
                GameManager.s_NetWorkClient.LeaveMatchToRoom();
        }

        HideMatchStartCountDown();
        HideWaitToJoinNextMatchLabel();

    }
    public void OnShowRoomUsersBtnClick()
    {
    }

    private void ShowLoseUI()
    {
        if (_loseUIPopup != null)
        {
            _loseUIPopup.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            _loseUIPopup.SetActive(true);
        }
    }

    private void HideLoseUI()
    {
        if (_loseUIPopup != null)
        {
            _loseUIPopup.SetActive(false);
        }
    }

    public void ShowPickupTooFarText()
    {
        //_pickupTooFarText.enabled = true;
        Invoke("HidePickupTooFarText", 1f);
    }

    private void HidePickupTooFarText()
    {
        //_pickupTooFarText.enabled = false;
    }

    public void ShowPickupWrongUI(Vector3 pickupPos)
    {
        if (_pickupWrongUI != null)
        {
            _pickupWrongUI.transform.position = pickupPos;
            _pickupWrongUI.transform.forward = Camera.main.transform.forward;
            _pickupWrongUI.SetActive(true);
            Invoke("HidePickupWrongUI", 0.6f);
        }
    }

    private void HidePickupWrongUI()
    {
        if (_pickupWrongUI != null)
            _pickupWrongUI.SetActive(false);
    }

    public void ShowMatchStartCountDown(float value)
    {
        if (GameManager.s_NetWorkClient.TheUserMatchState == NetWorkClient.UserMatchState.PlayingMatch)
        {
            if (GameManager.s_NetWorkClient.MultiGameState == NetWorkClient.GameState.Game_Match)
            {
                _matchStartCountDownText.text = string.Format("Start CD:{0}", value.ToString());
            }
            else if (GameManager.s_NetWorkClient.MultiGameState == NetWorkClient.GameState.Game_Running)
            {
                _matchStartCountDownText.text = string.Format("Match CD:{0}", value.ToString());
            }
            Debug.Log("ShowMatchStartCountDown:" + _matchStartCountDownText.text);
            //_matchStartCountDownText.text = value.ToString();
            _matchStartCountDownUI.SetActive(true);
        }
        else
        {
            HideMatchStartCountDown();
        }
    }

    public void HideMatchStartCountDown()
    {
        if (_matchStartCountDownUI != null)
            _matchStartCountDownUI.SetActive(false);
    }

    public void ShowWaitToJoinNextMatchLabel()
    {
        if (GameManager.s_NetWorkClient.TheUserMatchState == NetWorkClient.UserMatchState.WaitToJoinNextMatch)
        {
            _matchStartCountDownText.text = "Wait To Join Next Match";
            if (_matchStartCountDownUI != null)
                _matchStartCountDownUI.SetActive(true);
        }
        else
        {
            HideWaitToJoinNextMatchLabel();
        }
    }

    public void HideWaitToJoinNextMatchLabel()
    {
        if (_matchStartCountDownUI)
            _matchStartCountDownUI.SetActive(false);
    }

    // Update is called once per frame
    int taggerCount = 0;
    int hideCount = 0;
    void Update()
    {
        if (TaggerCount != null)
            TaggerCount.text = "0";
        if (HideCount != null)
            HideCount.text = "0";
        for (PlayerTeam.PlayerTeamType teamType = PlayerTeam.PlayerTeamType.TaggerTeam; teamType < PlayerTeam.PlayerTeamType.MaxTeamNum; teamType++)
        {
            PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(teamType);
            if (team == null)
                continue;
            for (int i = 0; i < team.GetPlayerNum(); i++)
            {
                PlayerBase playerBase = GameObjectsManager.GetInstance().GetPlayer(teamType, i);
                if (playerBase != null && playerBase.Hp != 0)
                {
                    if (teamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                        taggerCount++;
                    else if (teamType == PlayerTeam.PlayerTeamType.HideTeam)
                        hideCount++;

                    ShowPlayerTopHeadInfo(playerBase);
                }
            }
            TaggerCount.text = taggerCount.ToString();
            HideCount.text = hideCount.ToString();
        }

        taggerCount = 0;
        hideCount = 0;
    }
    public void OnApplicationQuit()
    {
        Debug.Log("quit application now");
        if (GameManager.s_NetWorkClient != null)
        {
            GameManager.s_NetWorkClient.DisConnect();
        }
    }
    //根据队伍类型显隐相应UI
    public void ShowHideUI(Human localHuman)
    {
        if (localHuman != null)
        {
            if (localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
            {
                m_Canvas.transform.Find("Hearts").gameObject.SetActive(true);
            }
            else if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
            {
                m_Canvas.transform.Find("Hearts").gameObject.SetActive(false);
            }
        }
    }
    //Tips
    private Coroutine showMiddleTipsCoroutine = null;
    public void ShowMiddleTips(string str, float time = 1.5f)
    {
        Loom.QueueOnMainThread(() =>
        {
            if (showMiddleTipsCoroutine != null)
            {
                StopCoroutine(showMiddleTipsCoroutine);
                showMiddleTipsCoroutine = null;
                _MiddleTips.SetActive(false);
            }
            _MiddleTips.transform.Find("Text").GetComponent<Text>().text = str;
            _MiddleTips.SetActive(true);
            showMiddleTipsCoroutine = StartCoroutine(MiddleTips(time));
        });
    }
    public void ClearMiddleTips()
    {
        _MiddleTips.transform.Find("Text").GetComponent<Text>().text = "";
        _MiddleTips.SetActive(false);
    }
    private IEnumerator MiddleTips(float time)
    {
        yield return new WaitForSeconds(time);
        _MiddleTips.SetActive(false);
    }

    public void ShowJSPopup(int index)
    {
        StartCoroutine(JSPopup(index));
    }
    private IEnumerator JSPopup(int index)
    {
        if (index == 0)
            LosePopup.SetActive(true);
        else
            WinPopup.SetActive(true);
        yield return new WaitForSeconds(2);
        LosePopup.SetActive(false);
        WinPopup.SetActive(false);
    }
    public void ShowTopTips(string str, bool visible)
    {
        _StatusTips.GetComponent<Text>().text = str;
        if (visible)
            _StatusTips.SetActive(true);
        else
            _StatusTips.SetActive(false);
    }

    public void ShowManualControlTips(string str, bool visible)
    {
        _ManualControlTips.SetActive(visible);
        _ManualControlTips.GetComponent<Text>().text = str;
    }

    public void ShowWinOrLose()
    {
        //判断输赢
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(PlayerTeam.PlayerTeamType.HideTeam);
        if (localHuman != null && team != null)
        {
            bool HideWin = false;
            for (int i = 0; i < team.GetPlayerNum(); i++)
            {
                PlayerBase playerBase = GameObjectsManager.GetInstance().GetPlayer(PlayerTeam.PlayerTeamType.HideTeam, i);
                if (playerBase.Hp != 0)
                {
                    HideWin = true;
                    break;
                }
            }
            if (!HideWin)
            {
                if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                    ShowJSPopup(0);
                else
                    ShowJSPopup(1);
            }
            else
            {
                if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                {
                    var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                    if (kernel != null)
                        kernel.SendAwardData(10, 0);  //找到一个人奖励
                    ShowJSPopup(1);
                }
                else
                    ShowJSPopup(0);
            }
        }
    }
    public void HiderGameScore(int fTime)
    {
        if (fTime == 150 || fTime == 100 || fTime == 50)
        {
            PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(PlayerTeam.PlayerTeamType.HideTeam);
            if (team != null)
            {
                for (int i = 0; i < team.GetPlayerNum(); i++)
                {
                    PlayerBase playerBase = GameObjectsManager.GetInstance().GetPlayer(PlayerTeam.PlayerTeamType.HideTeam, i);
                    if (playerBase.Hp != 0)
                    {
                        playerBase.GameScore += 50;
                    }
                }
            }
        }
    }
    //显示头顶信息
    public void ShowPlayerTopHeadInfo(PlayerBase playerBase)
    {
        if(GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {
            if (GameManager.s_singleGameStatus == SocketDefines.GAME_STATUS_HIDE || GameManager.s_singleGameStatus == SocketDefines.GAME_STATUS_PLAY)
            {
                Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
                if (localHuman != null)
                {
                    //本地玩家为警察时不显示躲藏者信息
                    if (localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam && playerBase.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                    {
                        playerBase.SetTopInfoVisible(false);
                        return;
                    }
                    goto TopHeadInfo;
                }
                return;
            }
            else
                return;
        }
        else
        {
            if (CServerItem.get() == null)
                return;
            byte Gamestate = CServerItem.get().GetGameStatus();
            if (Gamestate == SocketDefines.GAME_STATUS_HIDE || Gamestate == SocketDefines.GAME_STATUS_PLAY)
            {
                Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
                if (localHuman != null)
                {
                    //本地玩家为警察时不显示躲藏者信息
                    if (localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam && playerBase.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                    {
                        playerBase.SetTopInfoVisible(false);
                        return;
                    }
                    goto TopHeadInfo;
                }
                else
                {
                    IClientUserItem pMeItem = CServerItem.get().GetMeUserItem();
                    if (pMeItem == null)
                    {
                        return;
                    }

                    int nStatus = pMeItem.GetUserStatus();
                    if (nStatus == SocketDefines.US_LOOKON)
                    {
                        goto TopHeadInfo;
                    }
                }
            }
            else
                return;
        }

        TopHeadInfo:
        {
            GameObject InfoPanelObj = playerBase.gameObject.transform.Find("InfoPanel").gameObject;
            if (InfoPanelObj != null)
            {
                TextMesh nameText = InfoPanelObj.transform.Find("Name").GetComponent<TextMesh>();
                TextMesh levelText = InfoPanelObj.transform.Find("Level").GetComponent<TextMesh>();
                if (playerBase.IsAI)
                {

                    nameText.text = playerBase.gameObject.name;
                    nameText.color = Color.white;

                    if (levelText.text == "")
                        levelText.text = "等级：" + UnityEngine.Random.Range(0, 100);
                    levelText.color = Color.white;
                }
                else
                {
                    var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                    string strNN = "";
                    if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                        strNN = GlobalUserInfo.getNickName();
                    else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                    {
                        if (kernel != null)
                            strNN = kernel.getPlayerByChairID(playerBase.ChairID).GetNickName();
                    }
                    if (strNN != "")
                    {
                        String[] str = strNN.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                        nameText.text = str[0];
                        nameText.color = Color.white;
                        if (kernel != null)
                        {
                            tagUserInfo userInfo = (tagUserInfo)kernel.getPlayerByChairID(playerBase.ChairID).GetUserInfo();
                            levelText.text = "等级：" + (userInfo.lExperience / 100).ToString();
                        }
                        else
                            levelText.text = "等级：" + (GlobalUserInfo.getUserExp() / 100).ToString();
                        levelText.color = Color.white;
                    }
                }
                if (InfoPanelObj != null)
                    InfoPanelObj.transform.LookAt(Camera.main.transform);
            }
            return;
        }
    }
    //更新UI信息
    public void UpdateUIInfo()
    {
        if (_Glod != null)
        {
            _Glod.transform.Find("GlodCount").GetComponent<Text>().text = GlobalUserInfo.getUserScore().ToString();
        }
    }

    public void StartPlayerJionDealInGaming(byte cbChairID)
    {
        StartCoroutine(PlayerJionDealInGaming(cbChairID));
    }
    IEnumerator PlayerJionDealInGaming(byte cbChairID)
    {
        while (CServerItem.get() == null) { }
        yield return null;
        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
        {
            Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
            if (localHuman != null)
            {
                byte Gamestate = CServerItem.get().GetGameStatus();
                if (Gamestate == SocketDefines.GAME_STATUS_PLAY)
                {
                    ControlManager.GetInstance().ControlModelSwitch(ControlManager.CameraControlMode.PlayerViewMode);
                    if (localHuman != null)
                    {
                        if (localHuman.ChairID == cbChairID)
                        {
                            if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                            {
                                GameSceneUIHandler.ShowLog("躲藏队");
                                m_Canvas.transform.Find("Hearts").gameObject.SetActive(false);
                                var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                                if (kernel != null && !m_hnGameManager.isReconnect)   //只有非断线重连加入游戏才能获得隐身效果
                                {
                                    ShowMiddleTips("当前为隐身状态，在隐身效果结束前躲起来！", 3);
                                    kernel.SendInventoryConsumption((byte)ControlManager.InventoryItemID.Stealth, 0);  //不消耗费用
                                }
                                if (m_hnGameManager.isReconnect)
                                {
                                    Button StealthButton = m_Canvas.transform.Find("Btn/StealthButton").gameObject.GetComponent<Button>();
                                    if (StealthButton != null)
                                    {
                                        StealthButton.interactable = true;
                                        Text timeText = StealthButton.gameObject.transform.Find("TimeLeft").GetComponent<Text>();
                                        if (timeText != null)
                                            timeText.text = "";
                                    }
                                }
                            }
                            else
                            {
                                GameSceneUIHandler.ShowLog("搜查队");
                                ShowMiddleTips("您被分配到搜查队，开始寻找躲起来的物品吧！", 3);
                                m_Canvas.transform.Find("Hearts").gameObject.SetActive(true);
                            }
                        }
                    }
                    _showRoomUsersBtn.SetActive(true);
                    GameManager.GetInstance().SetAllHiderVisible(true);
                }
            }
        }
    }
    #region 道具使用冷却
    public Coroutine coldTimeCoroutine = null;
    //道具冷却时间
    public void StartColdTime(Button Btn, float time)
    {
        coldTimeCoroutine = StartCoroutine(ColdTime(Btn, time));
    }
    public void ResetColdTime()
    {
        if (coldTimeCoroutine != null)
        {
            StopCoroutine(coldTimeCoroutine);
            coldTimeCoroutine = null;
        }
        GameObject StealthButton = m_Canvas.transform.Find("Btn/StealthButton").gameObject;
        Button button = StealthButton.GetComponent<Button>();
        if (button != null)
            button.interactable = true;
        Image image = StealthButton.transform.FindChild("TimeImage").GetComponent<Image>();
        if (image != null)
            image.fillAmount = 0;
        Text timeText = StealthButton.gameObject.transform.Find("TimeLeft").GetComponent<Text>();
        if (timeText != null)
            timeText.text = "";
    }
    IEnumerator ColdTime(Button Btn, float time)
    {
        //Debug.LogWarning("--------- " + time);
        float t = time;
        Image image = Btn.gameObject.transform.FindChild("TimeImage").GetComponent<Image>();
        while (t > 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            t -= Time.deltaTime;
            if (image != null)
                image.fillAmount = (t / 60f);
            Btn.interactable = false;

        }
        if (t <= 0)
            if (image != null)
                image.fillAmount = (t / 60f);
        Btn.interactable = true;
    }
    #endregion
    public void StartSingleStealthTime(byte time)
    {
        StartCoroutine(SingleStealthTime(time));
    }
    IEnumerator SingleStealthTime(byte time)
    {
        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
            ControlManager.GetInstance().StealthTime(time);
        }
        ControlManager.GetInstance().StealthTime(255);
        ControlManager.GetInstance().Stealth(true);
    }

}
