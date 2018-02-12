using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RockBall.Crypto;
using SprotoType;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GameNet;

public enum MatchType
{
    Normal_1v1 = 1,
    Normal_2v2,
    Normal_3v3
}

public class NetWorkClient {

    //use for Login flow
    public enum UserLoginState
    {
        NullState = -1,
        LoginStart,
        LoginPublicServerKey,
        LoginCheckSecret,
        LoginRegistResult,
        LoginCheckTokenResult,
        LoginCheckTokenRetry,
        LoginSuccess,
        LoginFailed
    }

    //use for match msg (send or receive matching msg)
    public enum MatchMsgType
    {
        Match_FindRoom = 1,
        Match_PlayerJoinedRoom,
        Match_TryToJoinMatch,
        //Match_CancelMatching,
        //Match_ReMatching
        Match_CountDownTime,
        Match_LeaveMatchToRoom
        //Match_ConfirmAll,
        //Match_PlayerSelect,
        //Match_OnePlayerConfirm,
        //Match_LoadScenePercent,
        //Match_QuitGame
    }

    //Synchronization with server state, use for reconnect 
    public enum UserMatchState
    {
        NullState = -1,
        FindRoom = 4,  //same to server
        JoinedRoom = 5,
        PlayingMatch = 6,
        WaitToJoinNextMatch = 7
        //AllPlayersJoinedRoom,
        //WaitForStart
        //User_MatchWaitForConfirm,
        //User_PlayerSelecting,
        //User_LoadingScene,
        //User_PlayingGame
    }

    //use for multi game
    public enum GameState
    {
        Game_NullState,
        Game_Login,
        Game_Match,
        //Game_Matching,
        //Game_Ready,
        //Game_WaitForStart,
        Game_Running,
        Game_Over
    }

    //players info in one match
    public class MultiPlayerInfo
    {
        public long PlayIndexInGlobal; // player index in the match
        public string PlayerName; // player display name
        public int PlayerId { get; set; } // user selected player id
        public int SkinId { get; set; } // user selected player's skin id
        public int LoadPercent; // loading scene percent

        public PlayerTeam.PlayerTeamType ThePlayerTeamType;
        public int PlayerIndexInTeam;
    }

    public event EventHandler QuitMatchingMsg; // match succeed but not confirm match
    public event EventHandler ReMatchMsg; // match succeed and confirm the match, but someone in match didnot confirm
    public event EventHandler AllUserConfirmMsg; // all users in the game clicked the confirm button
    public event EventHandler PlayerSelectMsg;  //one user changed his selection in player select ui

    public UserLoginState TheUserLoginState { get; set; } //login flow state
    //private byte[] _secretKeyStr;
    private byte[] _challengeStr; // challenge string received from server for login
    private UInt64 _privateKey; //dh64 private key
    private UInt64 _publicKey; // dh64 public key
    private UInt64 _secretKey; // dh64 secret key created by private key and server public key
    public string UserName { get; set; }//unique user id 
    public string PassWord { get; set; }//password
    public string DisplayName { get; set; }//display name
    private static string challengeSuccess = "100"; // challenge success key
    private static string challengeFailed = "400"; // challenge failed key

    public UserMatchState TheUserMatchState;//save the user's state, overwrite it with server state when reconnect
    public List<MultiPlayerInfo> AllPlayers;//all player info in the match

    public GameState MultiGameState { get; set; }//multigame state
    
    public bool CheckConnectVar = false;//var for check network

    private Thread _dispatchSocketthread; //dispatch socket msg thread
    private Thread _checkNetworkThread; //check network thread, started when NetWorkClient init
    private Thread _sendHeartBeatThread; // send and receive heartbeat msg thread


    //constructor, init NetCore,NetSender,NetReceiver, add receiver handler, start check network thread
    public NetWorkClient()
    {
        var forInit = Loom.Current;//to initialize Loom in mainThread

        TheUserLoginState = UserLoginState.NullState;
        TheUserMatchState = UserMatchState.NullState;
        MultiGameState = GameState.Game_NullState;

        AllPlayers = new List<MultiPlayerInfo>();

       // Debug.Log("init netclient");
        NetCore.Init();
        NetCore.SocketDisconnectHandler += Reconnect;
        NetSender.Init();
        NetReceiver.Init();

        SetupNetReceiverHandler();

        StartConnect();

        //mChen
        //_checkNetworkThread = new Thread(CheckHeartBeat) { IsBackground = true };
        ////防止后台现成。相反需要后台线程就设为false
        //_checkNetworkThread.Start();
        //while (_checkNetworkThread.IsAlive == false)
        //{
        //}
    }

    private void SetupNetReceiverHandler()
    {
        NetReceiver.AddHandler<Protocol.handshake>((_) =>
        {
            Debug.Log("handshake");
            SprotoType.handshake.request req = _ as handshake.request;
            switch (TheUserLoginState)
            {
                case UserLoginState.LoginStart:
                    {
                        //Debug.Log("challenge receive ");
                        _challengeStr = Convert.FromBase64String(req.msg);

                        DH64 dhforKey = new DH64();
                        dhforKey.KeyPair(out _privateKey, out _publicKey);
                        SprotoType.handshake.request clientkey = new handshake.request();

                        clientkey.msg = Convert.ToBase64String(BitConverter.GetBytes(_publicKey));

                        clientkey.socketfd = req.socketfd;
                        NetSender.Send<Protocol.handshake>(clientkey);

                        Debug.Log("challengeStr" + _challengeStr + " ori " + req.msg + " fd " + clientkey.socketfd);
                        TheUserLoginState = UserLoginState.LoginPublicServerKey;
                    }
                    break;
                case UserLoginState.LoginPublicServerKey:
                    {
                        //string serverKey = System.Text.Encoding.Default.GetString(Convert.FromBase64String(req.msg));
                        byte[] ServerKey = Convert.FromBase64String(req.msg);

                        ulong serverKey = BitConverter.ToUInt64(ServerKey, 0);
                        _secretKey = DH64.Secret(_privateKey, serverKey);
                        SprotoType.handshake.request challengeMsg = new handshake.request();

                        challengeMsg.msg =
                            Convert.ToBase64String(
                                BitConverter.GetBytes(
                                    HMac64.hmac(
                                        BitConverter.ToUInt64(_challengeStr, 0),
                                        _secretKey)));

                        challengeMsg.socketfd = req.socketfd;
                        NetSender.Send<Protocol.handshake>(challengeMsg);
                        TheUserLoginState = UserLoginState.LoginCheckSecret;
                    }
                    break;
                case UserLoginState.LoginCheckSecret:
                    {
                        if (Convert.ToBase64String(Encoding.UTF8.GetBytes(challengeSuccess)) == req.msg)
                        {
                            if (UserName != string.Empty)
                            {
                                Debug.Log("login " + UserName);
                                string token = string.Format("{0}:{1}",
                                    Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName)),
                                    Convert.ToBase64String(Encoding.UTF8.GetBytes(PassWord)));

                                SprotoType.handshake.request tokenMsg = new handshake.request
                                {
                                    msg = DesCrypt.desencode(_secretKey, token),
                                    socketfd = req.socketfd
                                };

                                NetSender.Send<Protocol.handshake>(tokenMsg);

                                TheUserLoginState = UserLoginState.LoginCheckTokenResult;
                            }
                            else
                            {
                                Debug.Log("register");
                                DisplayName = RandomName.GetRandomName();
                                string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(DisplayName));
                                SprotoType.handshake.request tokenMsg = new handshake.request
                                {
                                    msg = DesCrypt.desencode(_secretKey, token),
                                    socketfd = req.socketfd
                                };

                                NetSender.Send<Protocol.handshake>(tokenMsg);
                                TheUserLoginState = UserLoginState.LoginRegistResult;
                            }
                        }
                        else if (Convert.ToBase64String(Encoding.UTF8.GetBytes(challengeFailed)) == req.msg)
                        {
                            Debug.Log("sercet incorrect!");
                            //error handle
                            TheUserLoginState = UserLoginState.LoginFailed;
                        }
                    }
                    break;
                case UserLoginState.LoginRegistResult:
                    {
                        var msg = Encoding.UTF8.GetString(Convert.FromBase64String(req.msg));
                        var msgArray = msg.Split(':');
                        UserName = Encoding.UTF8.GetString(Convert.FromBase64String(msgArray[0]));
                        PassWord = Encoding.UTF8.GetString(Convert.FromBase64String(msgArray[1]));

                        Loom.QueueOnMainThread(() =>
                        {
                            //save registed info
                            PlayerPrefs.SetString("username", UserName);
                            //GameManager.NetWorkClient.UserName = String.Empty;
                            PlayerPrefs.SetString("password", PassWord);
                            PlayerPrefs.SetString("displayname", DisplayName);
                            Debug.Log("saving name " + UserName + " : " + PassWord);
                            PlayerPrefs.Save();
                        });
                        Debug.Log("name " + UserName + " : " + PassWord);

                        //Thread.Sleep(5);
                        TheUserLoginState = UserLoginState.LoginSuccess;
                    }
                    break;
                case UserLoginState.LoginCheckTokenResult:
                case UserLoginState.LoginSuccess:
                    {
                        if (Convert.ToBase64String(Encoding.UTF8.GetBytes(challengeSuccess)) == req.msg)
                        {
                            //Thread.Sleep(5);
                            Debug.Log("token right");

                            TheUserLoginState = UserLoginState.LoginSuccess;

                            // mChen
                            //if(MultiGameState == GameState.Game_NullState)
                            //{
                            //    FindMatch();
                            //}
                        }
                        else if (Convert.ToBase64String(Encoding.UTF8.GetBytes(challengeFailed)) == req.msg)
                        {
                            //error handle
                            TheUserLoginState = UserLoginState.LoginFailed;
                        }

                    }
                    break;
            }
            //NetSender.Send<Protocol.handshake>(req);
            //Debug.Log(TheUserLoginState.ToString());
            return null;
        });

        NetReceiver.AddHandler<Protocol.matchMsg>((_) =>
        {
            SprotoType.matchMsg.request req = _ as matchMsg.request;
            
            switch (req.matchMsgType)
            {
                case (Int64)MatchMsgType.Match_PlayerJoinedRoom:
                    int playerTeamType = Convert.ToInt32(req.matchInfo);
                    TheUserMatchState = UserMatchState.JoinedRoom;
                    Debug.Log("NetReceiver: matchMsg: MatchMsgType.Match_PlayerJoinRoom: playerTeamType=" + playerTeamType);
                    break;

                case (Int64)MatchMsgType.Match_CountDownTime:
                    float countDowmTime = Convert.ToSingle(req.matchInfo) / 100;
                    Debug.Log("NetReceiver: matchMsg: MatchMsgType.Match_CountDownTime =" + countDowmTime);
                    // UI
                    Loom.QueueOnMainThread(() =>
                    {
                        UIManager.GetInstance().ShowMatchStartCountDown(countDowmTime);
                    });
                    break;

                case (Int64)MatchMsgType.Match_TryToJoinMatch:
                    int result = Convert.ToInt32(req.matchInfo);
                    Debug.Log("NetReceiver: matchMsg: MatchMsgType.Match_TryToJoinMatch: result=" + result);
                    MultiGameState = GameState.Game_Match;
                    if (result == 0)
                    {
                        TheUserMatchState = UserMatchState.WaitToJoinNextMatch;
                        Loom.QueueOnMainThread(() =>
                        {
                            UIManager.GetInstance().ShowWaitToJoinNextMatchLabel();
                        });   
                    }
                    else if (result == 1)
                    {
                        TheUserMatchState = UserMatchState.PlayingMatch;
                        Loom.QueueOnMainThread(() =>
                        {
                            UIManager.GetInstance().HideWaitToJoinNextMatchLabel();
                        });
                    }
                    else
                    {
                        Debug.Log("error TryToJoinMatch result from Server" + result);
                        throw new Exception("error TryToJoinMatch result from Server");
                    }
                    break;

                //case (Int64)MatchMsgType.Match_CancelMatching:
                //    if (QuitMatchingMsg != null)
                //    {
                //        QuitMatchingMsg(this, EventArgs.Empty);
                //    }
                //    break;

                default:
                    Debug.Log("error match type" + req.matchMsgType);
                    throw new Exception("error msg type");
            }
            return null;
        });

        NetReceiver.AddHandler<Protocol.usersInfoMsg>((_) =>
        {
            if (MultiGameState == GameState.Game_NullState)
            {
                return null;
            }
            Debug.Log("get usersInfoMsg");

            SprotoType.usersInfoMsg.request req = _ as SprotoType.usersInfoMsg.request;
            GameObjectsManager.s_LocalPlayerIndexInGlobal = (int)req.curUserGlobalIndex;
            //Debug.Log("get player index in global: " + GameObjectsManager.s_LocalPlayerIndexInGlobal);

            //Create players
            Loom.QueueOnMainThread(() =>
            {
                AllPlayers.Clear();
                GameObjectsManager.GetInstance().ClearPlayers();

                Debug.Log("received usersInfoMsg:");
                foreach (SprotoType.UserInfo serverPlayerinfo in req.userArray)
                {
                    PlayerTeam.PlayerTeamType teamType = (PlayerTeam.PlayerTeamType)serverPlayerinfo.playerTeamType;
                    PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(teamType);
                    int userState = (int)serverPlayerinfo.userState;

                    Debug.Log("     UserIndexInGlobal=" + serverPlayerinfo.playerIndex + " UserName=" + serverPlayerinfo.username + " userState=" + userState + " TeamType=" + teamType);

                    if (serverPlayerinfo.playerIndex == req.curUserGlobalIndex)
                    {
                        // is local player

                        if (userState == (int)UserMatchState.PlayingMatch)
                        {
                            GameObjectsManager.s_LocalHumanTeamType = teamType;
                        }
                    }

                    int playerIndexInTeam = -1;
                    if(userState == (int)UserMatchState.PlayingMatch)
                    {
                        // create a new player

                        playerIndexInTeam = team.AddAPlayer(false);
                    }

                    // set AllPlayers
                    AllPlayers.Add(new MultiPlayerInfo
                    {
                        PlayIndexInGlobal = serverPlayerinfo.playerIndex,
                        PlayerName = serverPlayerinfo.username,
                        SkinId = 0,
                        PlayerId = 1,
                        ThePlayerTeamType = teamType,
                        PlayerIndexInTeam = playerIndexInTeam
                    });
                }

            });

            //TheUserMatchState = UserMatchState.AllPlayersJoinedRoom;
            return null;
        });

        NetReceiver.AddHandler<Protocol.waitforstart>((_) =>
        {
            Debug.Log("Second half start!!");
            var res = _ as SprotoType.waitforstart.request;
            //mChen
            //switch (GameManager.GetInstance().TheGamePrePeriod)
            //{
            //    case GamePeriod.Opening:
            //        break;
            //    case GamePeriod.FirstHalf:
            //        break;
            //    case GamePeriod.Halftime:
            //        break;
            //    case GamePeriod.CutsceneAfterScoredOr24SecondsViolation:
            //        break;
            //    case GamePeriod.WairForGameToContinue:
            //        break;
            //    case GamePeriod.SecondHalf:
            //        GameManager.GetInstance().HalfTimeReadyFrame = res.readyFrame;
            //        break;
            //    case GamePeriod.Ending:
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}

            return null;
        });

        NetReceiver.AddHandler<Protocol.gameTick>((_) =>
        {
            //Debug.Log("response called");
            if (false)//mChen if (GameManager.TheGameMode > GameMode.MultiGame_3v3)
            {
                //when single mode, do not handle this server tick
                return null;
            }

            if (MultiGameState == GameState.Game_Match)
            {
                MultiGameState = GameState.Game_Running;

                Loom.QueueOnMainThread(() =>
                {
                    UIManager.GetInstance().HideMatchStartCountDown();

                });

                //mChen
                _sendHeartBeatThread = new Thread(SendHeartBeatMsg) { IsBackground = true };
                //防止后台现成。相反需要后台线程就设为false
                _sendHeartBeatThread.Start();
                while (_sendHeartBeatThread.IsAlive == false)
                {
                }
            }

            SprotoType.gameTick.request res = _ as gameTick.request;
            {
                // bool bOnlyFrameInfo = true;

                if (res.HasButtonMsg)
                {
                    //mChen
                    //foreach (var btnMsg in res.buttonMsg)
                    //{
                    //    ButtonInfo msg = new ButtonInfo
                    //    {
                    //        TeamType =
                    //            (PlayerTeamType)((btnMsg.playerIndex) / GameManager.TeamPlayerNum),
                    //        PlayerIndexInTeam = (btnMsg.playerIndex) % GameManager.TeamPlayerNum,
                    //        FrameNum = res.frame,
                    //        Action = (ButtonAction)btnMsg.btnAction,
                    //        Type = (ButtonType)btnMsg.btnType
                    //    };

                    //    if (btnMsg.HasBtnValueX)
                    //    {
                    //        msg.Value.x = (float)BitConverter.ToDouble(BitConverter.GetBytes(btnMsg.btnValueX), 0);
                    //        msg.Value.y = (float)BitConverter.ToDouble(BitConverter.GetBytes(btnMsg.btnValueY), 0); ;
                    //    }
                    //    ControlManager.GetInstance().AddControlInfo(msg);
                    //    //Debug.Log("Button msg");
                    //}

                    // bOnlyFrameInfo = false;
                }

                //_controllerMsg.Clear();
                if (res.HasControlMsg)
                {
                    //Debug.Log("controller called");
                    //List < ControlMsg > tempList = new List<ControlMsg>();

                    foreach (var resControl in res.controlMsg)
                    {
                        //mChen
                        //DPadInfo msg = new DPadInfo
                        //{
                        //    FrameNum = res.frame,
                        //    TeamType = (PlayerTeamType)(resControl.playerIndex / GameManager.TeamPlayerNum),
                        //    PlayerIndexInTeam = resControl.playerIndex % GameManager.TeamPlayerNum,
                        //    //H = Convert.ToSingle(resControl.x),
                        //    //V = Convert.ToSingle(resControl.y)
                        //    H = (float)BitConverter.ToDouble(BitConverter.GetBytes(resControl.x), 0),
                        //    V = (float)BitConverter.ToDouble(BitConverter.GetBytes(resControl.y), 0)
                        //};

                        //ControlManager.GetInstance().AddControlInfo(msg);
#if false
                        if (ControlManager.GetInstance().GameFrameNum % 150 == 1)
                        {
                            GameManager.GetInstance().InfoStr += string.Format("\n dpad info {0} {1} at frame {2}",
                                msg.H, msg.V, ControlManager.GetInstance().GameFrameNum);
                        }
#endif
                        // Debug.LogFormat("dpad msg frame {0} : h {1} v {2} ", res.frame, msg.H, msg.V);

                    }


                    // bOnlyFrameInfo = false;
                }

                if (res.HasPlayersGamePlayMsg)
                {
                    Loom.QueueOnMainThread(() =>
                    {
                        foreach (var playerGamePlayMsg in res.playersGamePlayMsg)
                        {
                            int playerIndex = (int)playerGamePlayMsg.playerIndex;
                            float posX = (float)BitConverter.ToDouble(BitConverter.GetBytes(playerGamePlayMsg.posX), 0);
                            float posY = (float)BitConverter.ToDouble(BitConverter.GetBytes(playerGamePlayMsg.posY), 0);
                            float posZ = (float)BitConverter.ToDouble(BitConverter.GetBytes(playerGamePlayMsg.posZ), 0);
                            float angleX = (float)BitConverter.ToDouble(BitConverter.GetBytes(playerGamePlayMsg.angleX), 0);
                            float angleY = (float)BitConverter.ToDouble(BitConverter.GetBytes(playerGamePlayMsg.angleY), 0);
                            float angleZ = (float)BitConverter.ToDouble(BitConverter.GetBytes(playerGamePlayMsg.angleZ), 0);
                            //Debug.Log("playerInfoMsg:" + playerGamePlayMsg.playerIndex + " pos:" + posX + posY + posZ);

                            if (playerIndex == GameObjectsManager.s_LocalPlayerIndexInGlobal)
                            {

                            }
                            else
                            {
                                PlayerTeam.PlayerTeamType teamType = AllPlayers[playerIndex].ThePlayerTeamType;
                                int playerIndexInTeam = AllPlayers[playerIndex].PlayerIndexInTeam;
                                PlayerBase player = GameObjectsManager.GetInstance().GetPlayer(teamType, playerIndexInTeam);
                                if (player != null)
                                {
                                    player.transform.position = new Vector3(posX, posY, posZ);
                                    player.transform.eulerAngles = new Vector3(angleX, angleY, angleZ);
                                }
                            }
                        }
                    });

                }

                //添加一般帧信息
                //if (bOnlyFrameInfo)
                {
                    //mChen ControlManager.GetInstance().AddControlInfo(res.frame);
                }
            }
            return null;
        });

        NetReceiver.AddHandler<Protocol.restoreStates>((_) =>
        {

            var req = _ as SprotoType.restoreStates.request;
            TheUserMatchState = (UserMatchState)req.serverState;
            Debug.Log("resotre user state now " + TheUserMatchState.ToString());
            var infoArray = req.stateInfo.Split(':');

            switch (req.serverState)
            {
                case (int)UserMatchState.FindRoom:
                    ParseFindMatchMsg(infoArray);
                    TheUserMatchState = UserMatchState.FindRoom;
                    //mChen Loom.QueueOnMainThread(GameUIManager.GetInstance().UIChangeToMatching);
                    //GameUIManager.GetInstance().UIChangeToMatching();//go to matching ui
                    break;
            }
            return null;
        });

    }

    public static string HostStr = "skynet.lightacting.com";
    //connect thread function
    public void StartConnectInThread()
    {
        //NetCore.Connect(HostStr, 8888, ConnectedCallback);
        NetCore.Connect(LoginScene.m_strServerIP, 8887, ConnectedCallback);//192.168.1.108 192.168.40.32
        //Debug.Log("Connect thread exit");
    }

    // start connect to server in another thread
    public void StartConnect()
    {

        TheUserLoginState = UserLoginState.LoginStart;

        if (_dispatchSocketthread != null && _dispatchSocketthread.IsAlive)
        {
            _dispatchSocketthread.Abort();
            _dispatchSocketthread = null;
        }

        if (_sendHeartBeatThread != null && _sendHeartBeatThread.IsAlive)
        {
            _sendHeartBeatThread.Abort();
            _sendHeartBeatThread = null;
        }
        CheckConnectVar = true;
        var startThread = new Thread(StartConnectInThread) { IsBackground = true };
        //防止后台现成。相反需要后台线程就设为false
        startThread.Start();
        while (startThread.IsAlive == false)
        {
        }
    }

    //disconnect from server when exit application
    public void DisConnect()
    {
        //mChen GameManager.GameReady = false;

        QuitApplicaion();

        Debug.Log("quit message send");
        if (IsConnected())
        {
            //NetSender.Send<Protocol.quit>();
            NetCore.Disconnect();
        }

        if (_dispatchSocketthread != null && _dispatchSocketthread.IsAlive)
        {
            Debug.Log("abort athread");
            _dispatchSocketthread.Abort();
        }

        if (_sendHeartBeatThread != null && _sendHeartBeatThread.IsAlive)
        {
            _sendHeartBeatThread.Abort();
        }

        if (_checkNetworkThread != null && _checkNetworkThread.IsAlive)
        {
            _checkNetworkThread.Abort();
        }

    }

    //whether socket is connected
    public bool IsConnected()
    {
        return NetCore.bConnected;
    }
    
    //socket dispatch thread function
    public void SocketThread()
    {
        while(NetCore.bConnected)
        {
            //Debug.Log("test");
            NetCore.Dispatch();
            Thread.Sleep(5);
        }
        Debug.Log("socket thread exit");
    }

    //send find match msg to server
    public void FindRoom()
    {
        Debug.Log("FindRoom called");

        MultiGameState = GameState.Game_Match;
        TheUserMatchState = UserMatchState.FindRoom;

        SprotoType.matchMsg.request req = new SprotoType.matchMsg.request();
        req.matchMsgType = (int)MatchMsgType.Match_FindRoom;
        //mChen
        //switch (GameManager.TheGameMode)
        //{
        //    case GameMode.MultiGame_1v1:
        //        req.matchInfo = ((int)MatchType.Normal_1v1).ToString();
        //        break;
        //    case GameMode.MultiGame_2v2:
        //        req.matchInfo = ((int)MatchType.Normal_2v2).ToString();
        //        break;
        //    case GameMode.MultiGame_3v3:
        //        req.matchInfo = ((int)MatchType.Normal_3v3).ToString();
        //        break;
        //}
        req.matchInfo = ((int)MatchType.Normal_1v1).ToString();

        NetSender.Send<Protocol.matchMsg>(req);
    }

    public void TryToJoinMatch()
    {
        Debug.Log("TryToJoinMatch called");
        //TheUserMatchState = UserMatchState.JoinedRoom;

        SprotoType.matchMsg.request req = new SprotoType.matchMsg.request();
        req.matchMsgType = (int)MatchMsgType.Match_TryToJoinMatch;
        NetSender.Send<Protocol.matchMsg>(req);
    }

    public void LeaveMatchToRoom()
    {
        Debug.Log("LeaveMatchToRoom called");

        if(TheUserMatchState==UserMatchState.PlayingMatch || TheUserMatchState==UserMatchState.WaitToJoinNextMatch)
        {
            MultiGameState = GameState.Game_Match;
            TheUserMatchState = UserMatchState.JoinedRoom;

            SprotoType.matchMsg.request req = new SprotoType.matchMsg.request();
            req.matchMsgType = (int)MatchMsgType.Match_LeaveMatchToRoom;
            NetSender.Send<Protocol.matchMsg>(req);
        }
    }

    //send and receive heart beat msg thread function
    public void SendHeartBeatMsg()
    {
		CheckConnectVar = true;
        while (NetCore.bConnected)
        {
            Debug.Log("heart beat send");

            Loom.QueueOnMainThread(() =>
            {
                Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
                Vector3 localPlayerPos = localHuman.transform.position;
                Vector3 eulerAngles = localHuman.transform.eulerAngles;
                SprotoType.heartbeat.request req = new heartbeat.request
                {
                    posX = BitConverter.ToInt64(BitConverter.GetBytes((double)localPlayerPos.x), 0),
                    posY = BitConverter.ToInt64(BitConverter.GetBytes((double)localPlayerPos.y), 0),
                    posZ = BitConverter.ToInt64(BitConverter.GetBytes((double)localPlayerPos.z), 0),

                    angleX = BitConverter.ToInt64(BitConverter.GetBytes((double)eulerAngles.x), 0),
                    angleY = BitConverter.ToInt64(BitConverter.GetBytes((double)eulerAngles.y), 0),
                    angleZ = BitConverter.ToInt64(BitConverter.GetBytes((double)eulerAngles.z), 0)
                };
                //req.playerPosMsg = new SprotoType.PlayerPosMsg();
                //req.playerPosMsg.playerIndex = (int)-1;
                //req.playerPosMsg.posX = BitConverter.ToInt64(BitConverter.GetBytes((double)localPlayerPos.x), 0);
                //req.playerPosMsg.posY = BitConverter.ToInt64(BitConverter.GetBytes((double)localPlayerPos.y), 0);
                //req.playerPosMsg.posZ = BitConverter.ToInt64(BitConverter.GetBytes((double)localPlayerPos.z), 0);

                NetSender.Send<Protocol.heartbeat>(req, (_) =>
                {
                    CheckConnectVar = true;
                });
            });

            Thread.Sleep(5000);//5s
        }
        Debug.Log("SendHeartBeatMsg thread exit");
    }

    //check network connect function, when not receive server reply in last 5s, reconnect to server
	public void CheckHeartBeat()
	{
        //return;

		while (true) {
			if (CheckConnectVar) {
				CheckConnectVar = false;
			} else {
				//reconnect to server
				Debug.Log ("re start connect at thread " + Thread.CurrentThread.ManagedThreadId);
                
                Loom.QueueOnMainThread(()=>{
                    if (SceneManager.GetActiveScene().buildIndex != 0)
                    {
                        Debug.Log("When lost connection, return to main scene, if you want close this feature,  annotate these codes");
                        SceneManager.LoadScene(0);
                    }
                });
				NetCore.ReConnect ();
			}

			Thread.Sleep (6000);
		}
	}

    //connected callback function, start dispatch thread when connected
    private void ConnectedCallback()
    {
        NetCore.Enabled = true;
        CheckConnectVar = true;
        Debug.Log("Connected to server!");

        _dispatchSocketthread = new Thread(SocketThread) { IsBackground = true };
        //防止后台现成。相反需要后台线程就设为false
        _dispatchSocketthread.Start();
        while (_dispatchSocketthread.IsAlive == false)
        {
        }
    }

    //Parse matching State resotre info
    public int ParseFindMatchMsg(string[] infoArray)
    {
        //mChen
        //switch (Convert.ToInt32 (infoArray [0])) {
        //case 1:
        //	GameManager.TheGameMode = GameMode.MultiGame_1v1;
        //	GameManager.TeamPlayerNum = 1;
        //	break;
        //case 2:
        //	GameManager.TheGameMode = GameMode.MultiGame_2v2;
        //	GameManager.TeamPlayerNum = 2;
        //	break;
        //case 3:
        //	GameManager.TheGameMode = GameMode.MultiGame_3v3;
        //	GameManager.TeamPlayerNum = 3;
        //	break;
        //}
        return 1;
    }

    //send quit match msg to server
    public void QuitApplicaion()
    {
        Reset();
        //mChen
        //ControlManager.GetInstance().Reset();
        //if (GameManager.IsMultiGame())
        //{
        //    matchMsg.request req = new matchMsg.request();
        //    req.matchMsgType = (int)NetWorkClient.MatchMsgType.Match_QuitGame;
        //    NetSender.Send<Protocol.matchMsg>(req);
        //}
    }

    //reconnect fucntion
    void Reconnect(object obj, EventArgs e)
    {
        StartConnect();
    }

    //reset gameState
    public void Reset()
    {
        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_Skynet)
        {
            //NetCore.Reset();
            MultiGameState = GameState.Game_NullState;

            //SprotoType.quit.request req = new SprotoType.quit.request();
            NetSender.Send<Protocol.quit>(null);
        }
    }

    public class RandomName
    {
        private static string _firstName =
            @"赵,钱,孙,李,周,吴,郑,王,冯,陈,褚,卫,蒋,            沈,韩,杨,朱,秦,尤,许,何,吕,施,张,孔,曹,严,华,金,魏,陶,姜, 戚,谢,邹,喻,            柏,水,窦,章,云,苏,潘,葛,奚,范,彭,郎,鲁,韦,昌,马,苗,凤,花,方,俞,任,袁,柳,            丰,鲍,史,唐, 费,廉,岑,薛,雷,贺,倪,汤,滕,殷,罗,毕,郝,邬,安,常,乐,于,时,            傅,皮,卞,齐,康,伍,余,元,卜,顾,孟,平,黄, 和,穆,萧,尹,姚,邵,湛,汪,祁,毛,            禹,狄,米,贝,明,臧,计,伏,成,戴,谈,宋,茅,庞,熊,纪,舒,屈,项,祝,董,梁, 杜,            阮,蓝,闵,席,季,麻,强,贾,路,娄,危,江,童,颜,郭,梅,盛,林,刁,钟,徐,丘,骆,高,            夏,蔡,田,樊,胡,凌,霍, 虞,万,支,柯,昝,管,卢,莫,经,房,裘,缪,干,解,应,宗,丁,            宣,贲,邓,郁,单,杭,洪,包,诸,左,石,崔,吉,钮,龚, 程,嵇,邢,滑,裴,陆,荣,翁,荀,            羊,於,惠,甄,麴,家,封,芮,羿,储,靳,汲,邴,糜,松,井,段,富,巫,乌,焦,巴,弓, 牧,            隗,山,谷,车,侯,宓,蓬,全,郗,班,仰,秋,仲,伊,宫,宁,仇,栾,暴,甘,钭,厉,戌,祖,            武,符,刘,景,詹,束,龙, 叶,幸,司,韶,郜,黎,蓟,薄,印,宿,白,怀,蒲,邰,从,鄂,索,            咸,籍,赖,卓,蔺,屠,蒙,池,乔,阴,郁,胥,能,苍,双, 闻,莘,党,翟,谭,贡,劳,逢,姬,            申,扶,堵,冉,宰,郦,雍,郤,璩,桑,桂,濮,牛,寿,通,边,扈,燕,冀,郏,浦,尚,农, 温,            别,庄,晏,柴,瞿,阎,充,慕,连,茹,习,宦,艾,鱼,容,向,古,易,慎,戈,廖,庾,终,暨,            居,衡,步,都,耿,满,弘, 匡,国,文,寇,广,禄,阙,东,欧,殳,沃,利,蔚,越,菱,隆,师,            巩,厍,聂,晃,勾,敖,融,冷,訾,辛,阚,那,简,饶,空, 曾,毋,沙,乜,养,鞠,须,丰,巢,            关,蒯,相,查,后,荆,红,游,竺,权,逯,盖,益,桓,公, 万俟,司马,上官,欧阳,夏侯,            诸葛,闻人,东方,赫连,皇甫,尉迟,公羊,澹台,公冶,宗政,濮阳,淳于,单于,太叔,            申屠,公孙,仲孙,轩辕,令狐,钟离,宇文,长孙,慕容,司徒,司空";
        private static string _lastName = @"努,迪,立,林,维,吐,丽,新,涛,米,亚,克,湘,明,            白,玉,代,孜,霖,霞,加,永,卿,约,小,刚,光,峰,春,基,木,国,娜,晓,兰,阿,伟,英,元,            音,拉,亮,玲,木,兴,成,尔,远,东,华,旭,迪,吉,高,翠,莉,云,华,军,荣,柱,科,生,昊,            耀,汤,胜,坚,仁,学,荣,延,成,庆,音,初,杰,宪,雄,久,培,祥,胜,梅,顺,涛,西,库,康,            温,校,信,志,图,艾,赛,潘,多,振,伟,继,福,柯,雷,田,也,勇,乾,其,买,姚,杜,关,陈,            静,宁,春,马,德,水,梦,晶,精,瑶,朗,语,日,月,星,河,飘,渺,星,空,如,萍,棕,影,南,北";

        private static System.Random _rnd = new System.Random((int) DateTime.Now.ToFileTimeUtc());

        public static string GetRandomName()
        {
            int namelength = 0;
            namelength = _rnd.Next(2, 4);
            _firstName = _firstName.Replace("\n", "");
            _firstName = _firstName.Replace("\r", "");
            _firstName = _firstName.Replace(" ", "");
            _lastName = _lastName.Replace("\r", "");
            _lastName = _lastName.Replace("\n", "");
            _lastName = _lastName.Replace(" ", "");
            string name = "";
            string[] FirstName = _firstName.Split(',');
            string[] LastName = _lastName.Split(',');
            if (namelength == 2)
            {
                name = FirstName[_rnd.Next(0, FirstName.Length)] + LastName[_rnd.Next(0, LastName.Length)];
            }
            else if (namelength == 3)
            {
                name = FirstName[_rnd.Next(0, FirstName.Length)] + LastName[_rnd.Next(0, LastName.Length)] + LastName[_rnd.Next(0, LastName.Length)];
            }
            return name;
        }
    }
}
