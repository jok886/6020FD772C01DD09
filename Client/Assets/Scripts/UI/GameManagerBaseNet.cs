using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameNet
{
    class GameManagerBaseNet: IServerItemSink,IServerListDataSink, IStringMessageSink
    {
        static GameManagerBaseNet ms_pkInstanceBase;
        private static HNGameManager hnManager;
        
        public static GameManagerBaseNet InstanceBase()
        {
            if (ms_pkInstanceBase==null)
            {
                ms_pkInstanceBase = new GameManagerBaseNet();
                hnManager = GameObject.FindObjectOfType<HNGameManager>();
            }
            return ms_pkInstanceBase;
        }

        private GameManagerBaseNet()
        {
            mServerItem = CServerItem.create();
            mCServerItem = (CServerItem) mServerItem;
            mServerItem.SetServerItemSink(this);
            mServerItem.SetStringMessageSink(this);

            //cocos2d::Director::getInstance()->getScheduler()->schedule(CC_CALLBACK_1(GameManagerBase::upDataTime, this), this, 0.0f, false, "GameManagerBase");
        }

        public CGameServerItem SearchGameServer(int iServerID)
        {
            return CServerListData.shared().SearchGameServer((ushort)iServerID);
        }

        public bool connectGameServerByKindID(ushort wKindID)
        {
            CGameServerItem pMinPlayerCoutServer = CServerListData.getGameServerByKind(wKindID);
            if (pMinPlayerCoutServer!=null)
            {
                connectGameServer(pMinPlayerCoutServer);
            }
            else
            {
                Debug.Assert(false, "GameManager::connectGameServerByKindID");
            }
            return true;
        }

        public bool connectGameServerByKindIDAndServerType(ushort wKindID, ushort wServerType) //mChen add
        {
            CGameServerItem pMinPlayerCoutServer = CServerListData.getGameServerByKindAndServerType(wKindID, wServerType);
            if (pMinPlayerCoutServer!=null)
            {
                connectGameServer(pMinPlayerCoutServer);
            }
            else
            {
                Debug.Assert(false, "GameManager::connectGameServerByKindIDAndServerType");
            }
            return true;
        }

        public bool connectGameServerByServerID(int iServerID)
        {
            CGameServerItem pServer = CServerListData.shared().SearchGameServer((ushort)iServerID);
            if (pServer!=null)
            {
                connectGameServer(pServer);
            }
            else
            {
                Debug.Assert(false, "GameManager::connectGameServerByServerID");
            }
            return true;
        }

        public bool connectGameServer(CGameServerItem pGameServerItem)
        {
            Debug.Log("GameManagerBase::connectGameServer");
            if (mServerItem.IsService())
            {
                disconnectServer();
            }

            Debug.Log("GameManagerBase::connectGameServer connectServer");
            mGameServerItem = pGameServerItem;

            connectServer();

            RestTick();

            return true;
        }

        public void onEnterTransitionDidFinish()
        {
            
        }

    //登陆信息
   
	//坐下失败
	public void onGRRequestFailure(string sDescribeString)
        {
            
        }
    //登陆成功
        public void OnGRLogonSuccess(byte[] data, int dataSize)
        {
            var typeValue = typeof (CMD_GR_LogonSuccess);
            if (dataSize != Marshal.SizeOf(typeValue)) return;

            //消息处理
            CMD_GR_LogonSuccess pLogonSuccess = (CMD_GR_LogonSuccess)StructConverterByteArray.BytesToStruct(data,typeValue);

            //玩家属性
            m_dwUserRight = pLogonSuccess.dwUserRight;
            m_dwMasterRight = pLogonSuccess.dwMasterRight;

#if UNITY_IPHONE
            bool bIsForAppleReview = (pLogonSuccess.cbIsForAppleReview != 0);
            if (hnManager != null)
            {
                hnManager.ShowHideRedForeground(bIsForAppleReview);
            }
#endif

            RestTick();
        }
        //登陆失败
        public void OnGRLogonFailure(long lErrorCode, string sDescribeString)
        {
           
        }

        public enReconnectStatus GetReconnectStatus()
        {
            return m_eInReconnect;
        }

        //登陆完成
        public void OnGRLogonFinish()
        {
            m_eInReconnect = enReconnectStatus.ReconnectStatus_NULL;
            closeClinet();
            //TimeManager::Instance().removeByFun(TIME_CALLBACK(GameManagerBase::closeClinet, this));

            ////mChen add, fix有人在团灭前断线，重连回来（已经下一局）还在原来的房间，时间一直没有更新，且没有跳出断线UI
            //Loom.QueueOnMainThread(() =>
            //{
            //    if (HNGameManager.bFakeServer == false)
            //    {
            //        if (hnManager != null)
            //        {
            //            hnManager.StartOrStopGameSceneHeartBeat(true);
            //        }
            //    }
            //});

            //mChen edit
            if (false)//if (HNMJGameScence::SERVER_TYPE == GAME_GENRE_MATCH && HNMJGameScence::IS_MATCH_SIGNUP)
            {
                //比赛报名
                CServerItem.get().sendMatchSignupCheck();
            }
            else
            {
                //重入判断
                if (mServerItem != null && (mServerItem.GetMeUserItem() != null) && (mServerItem.GetMeUserItem().GetUserStatus() >= SocketDefines.US_SIT))
                {
                    //启动进程
                    if (!StartGame(true))
                    {
                        mServerItem.OnGFGameClose((int)enGameExitCode.GameExitCode_CreateFailed);
                    }
                    return;
                }
                else if (m_eInReconnect == enReconnectStatus.ReconnectStatus_Connecting)
                {
                    if (mServerItem != null)
                        mServerItem.OnGFGameClose((int)enGameExitCode.GameExitCode_Normal);
                    return;
                }

                CB_GameLogonFinsh();
            }
        }

        //更新通知
        public void OnGRUpdateNotify(byte cbMustUpdate,string sDescribeString)
        {
            GameSceneUIHandler.ShowLog(sDescribeString);
        }

        public void CB_GameLogonFinsh()
        {
            if (mServerItem!=null)
            {
                if (!mServerItem.PerformQuickSitDown())
                {
                    //NoticeMsg::Instance().ShowTopMsgByScript("RoomFull");
                    Debug.Log("RoomFull");
                }
            }
        }

    //配置信息
    //列表配置
        public void OnGRConfigColumn()
        {
            
        }
    //房间配置
        public void OnGRConfigServer()
        {
            //创建桌子
            if (mCServerItem==null) return;
            ushort tChairCount = mCServerItem.GetServerAttribute().wChairCount;
        }
    //道具配置
    public void OnGRConfigProperty() { }
    //玩家权限配置
    public void OnGRConfigUserRight() { }
    //配置完成
    public void OnGRConfigFinish() { }

    //用户信息
  //用户进入
        public void OnGRUserEnter(IClientUserItem pIClientUserItem)
        {
            Debug.Log("GameManager::OnGRUserEnter: " +  GlobalUserInfo.GBToUtf8(pIClientUserItem.GetNickName()));
        }
    //用户更新
        public void OnGRUserUpdate(IClientUserItem pIClientUserItem)
        {
            
        }
    //用户删除
        public void OnGRUserDelete(IClientUserItem pIClientUserItem)
        {
            
        }

        //框架消息
	    //用户邀请
        public void OnGFUserInvite(string szMessage)
        {
            
        }

        //用户邀请失败
        public void OnGFUserInviteFailure(string szMessage)
        {
        }

        //房间退出
        public void OnGFServerClose(string szMessage)
        {
            if(hnManager != null)
            {
                Debug.Log("OnGFServerClose:GameType=" + HNGameManager.GameType);
                hnManager.m_bIsToHallByDisconnect = true;
                hnManager.m_bIsToHallFrom13Shui = (HNGameManager.GameType == HNPrivateScenceBase.GAME_TYPE_13Shui);
            }
        }

        //创建游戏内核
        public bool CreateKernel()
        {
            IClientKernelSink pKernelSink = CreateGame(mGameServerItem.m_GameServer.wKindID);
            if (pKernelSink==null)
            {
                GameNet.CServerItem.get().IntermitConnect(true);
                return false;
            }
            pKernelSink.clearInfo();
            CServerItem.get().SetClientKernelSink(pKernelSink);
            CServerItem.get().OnGFGameReady();

            return false;
        }

        public bool StartFakeGame(ushort wKindID)
        {
            IClientKernelSink pKernelSink = CreateGame(wKindID);
            pKernelSink.clearInfo();
            CServerItem.get().SetClientKernelSink(pKernelSink);
            return true;
        }

        public void FakeCloseGame()
        {
            if (mServerItem != null)
                mServerItem.OnGFGameClose((int)enGameExitCode.GameExitCode_Normal);
        }

    //启动游戏
        public bool StartGame(bool bIsReconnect=false)
        {
            Loom.QueueOnMainThread(() =>
            {
                float fDelayTimeOfUserEnter = 1.0f;
#if ApplyAutoReConnect
                //mChen add, for HideSeek
                if (bIsReconnect)
                {
                    //断线重连

                    Debug.Log("---------------------Disconnect back 1: ClearPlayers");
                    ///GameObjectsManager.GetInstance().ClearPlayers(false);
                    if (GameManager.GetInstance() != null)
                    {
                        GameManager.GetInstance().m_bSettedNetCB = false;
                    }
                    hnManager.isReconnect = true;
                    fDelayTimeOfUserEnter = 0f;
                }
                else
                {
                    ///hnManager.LoadHideSeekSceneOfWangHu();
                }
#else
                ///hnManager.LoadHideSeekSceneOfWangHu();
#endif

                ////mChen edit, for HideSeek: delay called to ensure scene is loaded before receive OnSocketSubUserEnter msg from server
                //Loom.QueueOnMainThread(() =>
                //{
                //    this.CreateKernel();
                //}, fDelayTimeOfUserEnter);
                this.CreateKernel();
            });
            ///this.CreateKernel();

            return true;
        }
        //完成通知
    public void OnGameItemFinish() { }
    //完成通知
    public void OnGameKindFinish(ushort wKindID) { }
    //更新通知
    public void OnGameItemUpdateFinish() { }

        //更新通知
        //插入通知
        public void OnGameItemInsert(CGameListItem pGameListItem) { }
        //更新通知
        public void OnGameItemUpdate(CGameListItem pGameListItem) { }
        //删除通知
        public void OnGameItemDelete(CGameListItem pGameListItem) { }

        /*------these two function must override in derived classes-------*/
        public virtual IClientKernelSink CreateGame(ushort wKindID)
        {
#if true
            //if (HNMJGameScence::KIND_ID == wKindID)
            {
                return new GameScene(hnManager);
            }
#endif
            return null;
        }

        public virtual void loadGameBaseData(ushort wKindID)
        {
            //if (HNMJGameScence::KIND_ID == wKindID)
            {
                DF.shared().init(wKindID, GameNet.GameScene.MAX_PLAYER, (int)GameScene.VERSION_CLIENT, Encoding.Default.GetBytes("Game"));
            }
        }
        /*------these two function must override in derived classes-------*/

        ///< 链接服务器
        public void connectServer()
        {
            if (mServerItem.IsService())
            {
                return;
            }

            RestTick();

            ///< 载入游戏基础数据
            loadGameBaseData(mGameServerItem.m_GameServer.wKindID);
            mServerItem.ConnectServer(mGameServerItem, 0, 0);
        }

        ///< 断开连接
        public void disconnectServer() //事件消息
        {
            if (mServerItem!=null)
            {
                if (CServerItem.get()!=null)
                {
                    CServerItem.get().IntermitConnect(true);
                }
            }
        }
        //进入事件
        public bool InsertUserEnter(byte[] pszUserName)
        {
            return true;
        }
        //离开事件
        public bool InsertUserLeave(byte[] pszUserName)
        {
            return true;
        }

        //断线事件
        public bool InsertUserOffLine(byte[] pszUserName)
        {
            return true;
        }

	//普通消息(窗口输出)
        public bool InsertNormalString(byte[] pszString)
        {
            //NoticeMsg::Instance().ShowTopMsg(utility::a_u8(pszString));
            string strLog = GlobalUserInfo.GBToUtf8(pszString);
            Debug.Log(strLog);
            GameSceneUIHandler.ShowLog(strLog);

            return true;
        }
        //系统消息(窗口输出)
        public bool InsertSystemString(byte[] pszString)
        {
            string strLog = Encoding.Default.GetString(pszString);
            Debug.Log(strLog);
            String[] str = strLog.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
            if (str.Length != 1)
            {
                int time = int.Parse(str[1]);
                if (time == 200)  //Play-200s
                {
                    Debug.Log("Play SOUND_TIPS_START_SEEK");
                    Loom.QueueOnMainThread(() => { hnManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_TIPS_START_SEEK); });
                }
                else if (time == 45 || time == 100) //Hide-45s,Play-100s,Play-45s
                {
                    Debug.Log("Play SOUND_TIPS_STARTOREND");
                    Loom.QueueOnMainThread(() => { hnManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_TIPS_STARTOREND); });
                    if (time == 45)
                    {
                        IClientUserItem pMeItem = CServerItem.get().GetMeUserItem();
                        byte Gamestate = CServerItem.get().GetGameStatus();
                        if (Gamestate == SocketDefines.GAME_STATUS_PLAY)
                            if (UIManager.GetInstance() != null)
                                UIManager.GetInstance().ShowMiddleTips("警察进入无敌状态");
                    }
                }
                else if (time == 5 || time == 4 || time == 3 || time == 2 || time == 1)
                {
                    Debug.Log("Play SOUND_TIME_SEC");
                    Loom.QueueOnMainThread(() => { hnManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_TIME_SEC); });
                }
            }
            strLog = GlobalUserInfo.GBToUtf8(pszString);
            GameSceneUIHandler.ShowLog(strLog);

            return true;
        }
        //系统消息(窗口输出)
        public bool InsertSystemStringScript(byte[] pszString)
        {
            string strLog = GlobalUserInfo.GBToUtf8(pszString);
            Debug.Log(strLog);

            GameSceneUIHandler.ShowLog(strLog);

            return true;
        }
        //提示消息(对话框方式??)0:确认 1:确认,取消
        public int InsertPromptString(byte[] pszString, int iButtonType)
        {
            string strLog = GlobalUserInfo.GBToUtf8(pszString);
            
            GameSceneUIHandler.ShowLog(strLog);
            return 1;
        }

        public bool InsertGladString(byte[] pszContent, byte[] pszNickName, byte[] pszNum, uint colText, uint colName,
            uint colNum)
        {
            return true;
        }

        //心跳包
        public void HeartTick()
        {
            if (CServerItem.get() == null)
            {
                return;
            }
            RestTick();
        }

        public void onEventTCPSocketError(Exception errorCode)
        {
            ///UserInfo.getInstance().checkInGameServer();
            Debug.Log("GameManagerBaseNet:onEventTCPSocketError: " + errorCode.ToString());

            //TimeManager::Instance().addCerterTimeCB(TIME_CALLBACK(GameManagerBase::closeClinet, this), 3.0f);
            Loom.QueueOnMainThread(() =>
            {
                Loom.QueueOnMainThread(closeClinet, 3.0f);
            });
        }

        public void closeClinet()
        {
            if (mServerItem!=null)
            {
                mServerItem.OnGFGameClose((int)enGameExitCode.GameExitCode_Normal);
            }
        }

        public void RestTick()
        {
            m_eInReconnect = enReconnectStatus.ReconnectStatus_NULL;
            m_fHeatTickTime = 0;
        }

        public void StartGameReconnect()
        {
            if (CServerItem.get().IsService())
            {
                Debug.Log("GameManagerBase::StartGameReconnect() IServerItem::get()->IsService() false!");
                m_eInReconnect = enReconnectStatus.ReconnectStatus_DisConnect;
                disconnectServer();
            }
        }

        public void upDataTime(float fTime)
        {
            if (CServerItem.get()==null || mGameServerItem == null)
            {
                return;
            }
            if (CServerItem.get().IsService() && m_eInReconnect == enReconnectStatus.ReconnectStatus_NULL)
            {
                m_fHeatTickTime += fTime;
            }
            if (m_eInReconnect == enReconnectStatus.ReconnectStatus_DisConnect)
            {
                if (CServerItem.get().IsService())
                {
                    disconnectServer();
                }
                else
                {
                    m_eInReconnect = enReconnectStatus.ReconnectStatus_Connecting;
                    connectServer();
                }
            }
            if (m_fHeatTickTime > 15.0f)
            {
                m_fHeatTickTime = 0;
                if (m_eInReconnect == enReconnectStatus.ReconnectStatus_NULL)
                {
                    //NoticeMsg::Instance().ShowNoticeMsgByScript("NetReconnectError");
                    m_eInReconnect = enReconnectStatus.ReconnectStatus_DisConnect;
                    disconnectServer();
                }
                else
                {
                    m_eInReconnect = enReconnectStatus.ReconnectStatus_NULL;

                    if (mServerItem!=null)
                    {
                        mServerItem.OnGFGameClose((int)enGameExitCode.GameExitCode_Normal);
                    }
                }
            }
        }

    //服务状态
    public enum enReconnectStatus
    {
        ReconnectStatus_NULL,
        ReconnectStatus_DisConnect,
        ReconnectStatus_Connecting,
    };

	enReconnectStatus m_eInReconnect;
    float m_fHeatTickTime;

    CGameServerItem mGameServerItem;
    IServerItem mServerItem;
    CServerItem mCServerItem;

    uint m_dwUserRight;                        //用户权限
    uint m_dwMasterRight;					//管理权限
};


}
