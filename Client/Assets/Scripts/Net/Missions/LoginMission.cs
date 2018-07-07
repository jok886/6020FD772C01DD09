

using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

using KIND_LIST = System.Collections.Generic.List<ushort>;
//typedef std::list<uint16>   KIND_LIST;
//typedef KIND_LIST::iterator KIND_ITER;
namespace GameNet
{

    //////////////////////////////////////////////////////////////////////////
    // 登陆提示
    //////////////////////////////////////////////////////////////////////////
    interface IGPLoginMissionSink
    {
        void onGPLoginSuccess();
        void onGPLoginComplete();
        bool onGPUpdateNotify(byte cbMustUpdate, byte[] szDescription);
        void onGPLoginFailure(uint iErrorCode,byte[] szDescription);
        void onGPError(Exception errorCode);
    }

    class CGPLoginMission : CSocketMission
    {
        const byte MISSION_LOGIN_ACCOUNT = 1;
        const byte MISSION_LOGIN_GAMEID = 2;
        const byte MISSION_REGISTER = 3;
        const byte MISSION_UPDATE_INFO = 4;
        const byte MISSION_SERVER_INFO = 5;
        const byte MISSION_LOGIN_VISITOR = 6;

        // 回调
        IGPLoginMissionSink mIGPLoginMissionSink;

        // 任务类型
        byte mMissionType;
        // 登陆数据
        CMD_GP_LogonAccounts mLoginAccount;
        // 登陆数据
        CMD_GP_LogonByUserID mLoginGameID;
        // 注册数据
        CMD_GP_RegisterAccounts mRegisterAccount;
        // 注册数据
        CMD_GP_VisitorLogon mVisitorAccount;

        private HNGameManager hnManager;

        // 更新类型
        //typedef std::list<uint16>   KIND_LIST;
        //typedef KIND_LIST::iterator KIND_ITER;

        KIND_LIST mKindList;
        KIND_LIST mKindWaitList;
        ///< 允许的类型
        /// 
        public CGPLoginMission(byte[] url, int port)
        {
            base.setUrl(url, port);
            mMissionType = 0;
            mIGPLoginMissionSink = null;
            mKindList = new KIND_LIST();
            mKindWaitList = new KIND_LIST();
        }


//////////////////////////////////////////////////////////////////////////
// 登陆
        // 设置回调接口
        public void setMissionSink(IGPLoginMissionSink pIGPLoginMissionSink)
        {
            mIGPLoginMissionSink = pIGPLoginMissionSink;
        }

        // 账号登陆
        public void loginAccount(CMD_GP_LogonAccounts loginAccount)
        {
            mLoginAccount = loginAccount;
            mMissionType = MISSION_LOGIN_ACCOUNT;
            start();
        }

        // 游客登陆
        public void loginVisitor(CMD_GP_VisitorLogon VisitorAccount)
        {
            mVisitorAccount = VisitorAccount;
            mVisitorAccount.dwPlazaVersion = DF.shared().GetPlazaVersion();
            mMissionType = MISSION_LOGIN_VISITOR;
            start();
        }

        // I D登陆
        public void loginGameID(CMD_GP_LogonByUserID LoginGameID)
        {

            mLoginGameID = LoginGameID;
            mMissionType = MISSION_LOGIN_ACCOUNT;
            start();
        }

        // 注册
        public void registerServer(CMD_GP_RegisterAccounts RegisterAccount)
        {
            mRegisterAccount = RegisterAccount;
            mMissionType = MISSION_REGISTER;
            start();
        }

        //更新人数
        public void updateOnlineInfo()
        {
            mMissionType = MISSION_UPDATE_INFO;
            start();
        }

        //更新类型
        public bool updateServerInfo(ushort kind)
        {
            // lin: temp not implemented
            if (mKindList.Contains(kind))
                return false;
            if (mKindWaitList.Contains(kind))
            {
                return false;
            }
            /* KIND_ITER it = std::find(mKindList.begin(), mKindList.end(), kind);

	if (it != mKindList.end())
	{
		return false;
	}

	it = std::find(mKindWaitList.begin(), mKindWaitList.end(), kind);

	if (it != mKindWaitList.end())
	{
		return false;
	}
    */
            mKindList.Add(kind);
            mMissionType = MISSION_SERVER_INFO;
            start();

            return true;

        }


        //////////////////////////////////////////////////////////////////////////
        /// 
        /// // 登陆
        public bool sendLoginVisitor(CMD_GP_VisitorLogon VisitorAccount)
        {
            GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            Buffer.BlockCopy(mVisitorAccount.szPassWord, 0, pGlobalUserData.szPassword, 0,
                mVisitorAccount.szPassWord.Length);

            //发送数据
            var buf = StructConverterByteArray.StructToBytes(mVisitorAccount);
            send(MsgDefine.MDM_GP_LOGON, MsgDefine.SUB_GP_LOGON_VISITOR, buf, buf.Length);
            return true;
        }

        //发送登陆信息
        public bool sendLoginAccount(CMD_GP_LogonAccounts LoginAccount)
        {
            GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            Buffer.BlockCopy(LoginAccount.szPassword, 0, pGlobalUserData.szPassword, 0, LoginAccount.szPassword.Length);

            // for Match Time
            Debug.Log("登錄設置kindid");
            LoginAccount.wKindID = GameScene.KIND_ID_JianDe;

            //发送数据
            //var buf = StructConverterByteArray.StructToBytes<CMD_GP_LogonAccounts>(LoginAccount);
            var buf = StructConverterByteArray.StructToBytes(LoginAccount);

            send(MsgDefine.MDM_GP_LOGON, MsgDefine.SUB_GP_LOGON_ACCOUNTS, buf, buf.Length);
            return true;
        }

        //发送登陆信息
        public bool sendLoginGameID(CMD_GP_LogonByUserID LoginGameID)
        {

            return true;
        }

        //发送注册信息
        public bool sendRegisterPacket(CMD_GP_RegisterAccounts RegisterAccount)
        {
            GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

            Buffer.BlockCopy(RegisterAccount.szLogonPass, 0, pGlobalUserData.szPassword, 0,
                RegisterAccount.szLogonPass.Length);

            // for Match Time
            Debug.Log("發送註冊信息");
            RegisterAccount.wKindID = GameScene.KIND_ID_JianDe;

            //发送数据
            var buf = StructConverterByteArray.StructToBytes(RegisterAccount);
            send(MsgDefine.MDM_GP_LOGON, MsgDefine.SUB_GP_REGISTER_ACCOUNTS, buf, buf.Length);
            return true;
        }

// 更新人数
        bool sendUpdateOnlineInfoPacket()
        {
            return true;
        }

// 更新类型房间列表
        void sendUpdateServerInfo()
        {

        }

//////////////////////////////////////////////////////////////////////////
// ISocketEngineSink
        public override void onEventTCPSocketLink()
        {
            switch (mMissionType)
            {
                // 登陆
                case MISSION_LOGIN_ACCOUNT:
                    sendLoginAccount(mLoginAccount);
                    break;
                case MISSION_LOGIN_VISITOR:
                    sendLoginVisitor(mVisitorAccount);
                    break;
                // 登陆
                case MISSION_LOGIN_GAMEID:
                    sendLoginGameID(mLoginGameID);
                    break;
                // 注册
                case MISSION_REGISTER:
                    sendRegisterPacket(mRegisterAccount);
                    break;
                // 更新人数
                case MISSION_UPDATE_INFO:
                    sendUpdateOnlineInfoPacket();
                    break;
                // 更新类型房间列表
                case MISSION_SERVER_INFO:
                    sendUpdateServerInfo();
                    break;
            }
        }

        void onEventTCPSocketShut()
        {
        }

        public void onEventTCPSocketError(Exception errorCode)
        {
            if (mIGPLoginMissionSink != null)
                mIGPLoginMissionSink.onGPError(errorCode);
        }

        public override bool onEventTCPSocketRead(int main, int sub, byte[] data, int size)
        {
            switch (main)
            {
                //case SUB_MB_LOGON_GAMEID:	return onSocketMainLogon(sub, data, size);
                case MsgDefine.MDM_GP_LOGON:
                    return onSocketMainLogon(sub, data, size);
                //case MsgDefine.MDM_GP_SERVER_LIST: return onSocketMainServerList(sub, data, size);
                case MsgDefine.MDM_GP_SERVER_LIST:
                    return onSocketMainServerList(sub, data, size);
                default:
                    break;
            }
            Assert.AreNotEqual(true, false, "event error");
            return false;
        }

        //////////////////////////////////////////////////////////////////////////
        // 登陆信息
        bool onSocketMainLogon(int sub, byte[] data, int size)
        {
            switch (sub)
            {
                //登录成功
                case MsgDefine.SUB_GP_LOGON_SUCCESS:
                    return onSocketSubLogonSuccess(data, size);
                //登录失败
                case MsgDefine.SUB_GP_LOGON_FAILURE:
                    return onSocketSubLogonFailure(data, size);
                //登录失败
                case MsgDefine.SUB_GP_VALIDATE_MBCARD:
                    return onSocketSubLogonValidateMBCard(data, size);
                //升级提示
                case MsgDefine.SUB_GP_UPDATE_NOTIFY:
                    return onSocketSubUpdateNotify(data, size);
                //登录完成
                case MsgDefine.SUB_GP_LOGON_FINISH:
                    return onSocketSubLogonFinish(data, size);
                case MsgDefine.SUB_GP_GROWLEVEL_CONFIG:
                    return true;
                case MsgDefine.SUB_GP_MATCH_SIGNUPINFO:
                    return onSocketSubMacthSignupInfo(data, size);
            }

            Assert.AreNotEqual(true, false, "");
            return false;
        }

//登录成功
        bool onSocketSubLogonSuccess(byte[] data, int size)
        {

            //登陆成功
            CMD_GP_LogonSuccess pData =
                (CMD_GP_LogonSuccess) StructConverterByteArray.BytesToStruct(data, typeof (CMD_GP_LogonSuccess));

            // for Match Time
            Loom.QueueOnMainThread(() =>
            {
                if (hnManager == null)
                {
                    hnManager = GameObject.FindObjectOfType<HNGameManager>();
                }
                hnManager.m_matchStartTime = pData.MatchStartTime;
                hnManager.m_matchEndTime = pData.MatchEndTime;
            });

            //变量定义
            GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

            pGlobalUserData.lUserScore = pData.lUserScore;
            pGlobalUserData.lUserInsure = pData.lUserInsure;

            // for签到
            pGlobalUserData.wSeriesDate = pData.wSeriesDate;

            // 已打场次,for抽奖
            pGlobalUserData.dwPlayCount = pData.dwPlayCount;

            //WQ add,抽奖记录
            pGlobalUserData.dwRaffleCount = pData.dwRaffleCount;
            pGlobalUserData.dwPlayCountPerRaffle = pData.dwPlayCountPerRaffle;

            // 代理
            pGlobalUserData.iSpreaderLevel = pData.iSpreaderLevel;  // -1:不是代理人

            // for HideSeek:查询警察模型库
            pGlobalUserData.lModelIndex0 = pData.lModelIndex0;
            
            pGlobalUserData.bGPIsForAppleReview = (pData.cbGPIsForAppleReview != 0);

            //WQ 头像Http
            string szHeadHttp = GlobalUserInfo.GBToUtf8(pData.szHeadHttp);
            Buffer.BlockCopy(pData.szHeadHttp, 0, pGlobalUserData.szHeadHttp, 0, pData.szHeadHttp.Length);

            //WQ add,公告信息
            string szPublicNotice = GlobalUserInfo.GBToUtf8(pData.szPublicNotice);
            Buffer.BlockCopy(pData.szPublicNotice, 0, pGlobalUserData.szPublicNotice, 0, pData.szPublicNotice.Length);

            //保存信息
            pGlobalUserData.wFaceID = pData.wFaceID;
            pGlobalUserData.cbGender = pData.cbGender;
            pGlobalUserData.dwUserID = pData.dwUserID;
            pGlobalUserData.dwGameID = pData.dwGameID;
            pGlobalUserData.dwSpreaderID = pData.dwSpreaderID;
            pGlobalUserData.dwExperience = pData.dwExperience;
            pGlobalUserData.cbInsureEnabled = pData.cbInsureEnabled;

            Debug.Log("onSocketSubLogonSuccess " + Encoding.Default.GetString(pData.szNickName));
            Buffer.BlockCopy(pData.szNickName, 0, pGlobalUserData.szNickName, 0, pData.szNickName.Length);
            // strcpy(pGlobalUserData.szNickName, utility::a_u8((char*)pData.szNickName).c_str());

            Buffer.BlockCopy(pData.szAccounts, 0, pGlobalUserData.szAccounts, 0, pData.szAccounts.Length);
            // strncpy(pGlobalUserData.szAccounts, ((char*)pData.szAccounts), countarray(pGlobalUserData.szAccounts));

            //金币信息
            pGlobalUserInfo.upPlayerInfo();

            if (mIGPLoginMissionSink != null)
                mIGPLoginMissionSink.onGPLoginSuccess();

            return true;
        }

//登录失败
        bool onSocketSubLogonFailure(byte[] data, int size)
        {
            DBR_GP_LogonError pNetInfo =
                (DBR_GP_LogonError) StructConverterByteArray.BytesToStruct(data, typeof (DBR_GP_LogonError));
            //显示消息
            if (mIGPLoginMissionSink != null)
                mIGPLoginMissionSink.onGPLoginFailure(pNetInfo.lErrorCode, (pNetInfo.szErrorDescribe));
            return true;

        }

//登录完成
        bool onSocketSubLogonFinish(byte[] data, int size)
        {
            stop();

            if (mIGPLoginMissionSink != null)
                mIGPLoginMissionSink.onGPLoginComplete();

            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

            pGlobalUserInfo.LoginSucess();
            return true;
        }

//登录完成
        bool onSocketSubMacthSignupInfo(byte[] data, int size)
        {
            //校验数据
            if (size%sizeof (ushort) != 0) return false;

            //提取数据		
            tagSignupMatchInfo tempInfo = new tagSignupMatchInfo();
            int structSize = Marshal.SizeOf(tempInfo);
            var buf = new byte[structSize];
            ushort wSignupCount = (ushort)(size / structSize);
            tagSignupMatchInfo[] pSignupMatchInfo=new tagSignupMatchInfo[wSignupCount];
            //获取对象
            //ASSERT(CServerListData::shared() != NULL);
            CServerListData pServerListData = CServerListData.shared();

            //查找房间
            CGameServerItem pGameServerItem = null;

            for (int i = 0; i < wSignupCount; i++)
            {
                Buffer.BlockCopy(data, i*structSize, buf, 0, structSize);
                pSignupMatchInfo[i] = (tagSignupMatchInfo)StructConverterByteArray.BytesToStruct(buf, typeof(tagSignupMatchInfo));

                //设置报名
                pGameServerItem = pServerListData.SearchGameServer(pSignupMatchInfo[i].wServerID);
                if (pGameServerItem != null && pGameServerItem.m_GameMatch.dwMatchID == pSignupMatchInfo[i].dwMatchID)
                {
                    pGameServerItem.m_bSignuped = true;
                }
            }

            return true;
        }

//升级提示
        bool onSocketSubUpdateNotify(byte[] data, int size)
        {
            GameSceneUIHandler.ShowLog("当前是老版本，请更新到最新版本！");

            return true;
        }

//登录失败(密保卡)
        bool onSocketSubLogonValidateMBCard(byte[] data, int size)
        {
            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        // 列表信息
        bool onSocketMainServerList(int sub, byte[] data, int size)
        {
            switch (sub)
            {
                case MsgDefine.SUB_GP_LIST_TYPE:
                    return onSocketListType(data, size);
                case MsgDefine.SUB_GP_LIST_KIND:
                    return onSocketListKind(data, size);
                case MsgDefine.SUB_GP_LIST_SERVER:
                    return onSocketListServer(data, size);
                case MsgDefine.SUB_GP_LIST_MATCH:
                    return onSocketListMatch(data, size);
                case MsgDefine.SUB_GP_LIST_FINISH:
                    return onSocketListFinish(data, size);
                case MsgDefine.SUB_GR_KINE_ONLINE:
                    return onSocketKindOnline(data, size);
                case MsgDefine.SUB_GR_SERVER_ONLINE:
                    return onSocketServerOnline(data, size);
                case MsgDefine.SUB_GR_ONLINE_FINISH:
                    return onSocketOnlineFinish(data, size);

                //WQ add. for HideSeek
                //大厅列表
                case MsgDefine.SUB_GP_LIST_LOBBY:
                    return onSocketListLobby(data, size);
            }

            return false;
        }

//种类信息
        bool onSocketListType(byte[] data, int size)
        {
            return true;
        }

//类型信息
        bool onSocketListKind(byte[] data, int size)
        {

            ////更新数据
            tagGameKind tempStruct = new tagGameKind();
            int itemSize = Marshal.SizeOf(tempStruct);

            if (size % itemSize != 0) return false;
            byte[] pNetInfo = new byte[itemSize];
            int iItemCount = size / itemSize;
            CServerListData pServerListData = CServerListData.shared();
            for (int i = 0; i < iItemCount; i++)
            {
                Buffer.BlockCopy(data, i*itemSize, pNetInfo, 0, itemSize);
                tagGameKind pGameKind = (tagGameKind)StructConverterByteArray.BytesToStruct(pNetInfo,typeof(tagGameKind));
                pServerListData.InsertGameKind(pGameKind);
            }
            return true;
        }

        //房间信息
        bool onSocketListServer(byte[] data, int size)
        {
            ////更新数据
            tagGameServer tempStruct = new tagGameServer();
            int itemSize = Marshal.SizeOf(tempStruct);

            if (size % itemSize != 0) return false;
            byte[] pNetInfo = new byte[itemSize];
            int iItemCount = size / itemSize;
            CServerListData pServerListData = CServerListData.shared();
            for (int i = 0; i < iItemCount; i++)
            {
                Buffer.BlockCopy(data, i * itemSize, pNetInfo, 0, itemSize);
                tagGameServer pGameServer = (tagGameServer)StructConverterByteArray.BytesToStruct(pNetInfo, typeof(tagGameServer));
                pServerListData.InsertGameServer(pGameServer);
            }

            return true;
        }

        //WQ add. for HideSeek
        //大厅列表
        bool onSocketListLobby(byte[] data, int size)
        {
            ////更新数据
            tagGameLobby tempStruct = new tagGameLobby();
            int itemSize = Marshal.SizeOf(tempStruct);

            if (size % itemSize != 0) return false;
            byte[] pNetInfo = new byte[itemSize];
            int iItemCount = size / itemSize;
            for (int i = 0; i < iItemCount; i++)
            {
                Buffer.BlockCopy(data, i * itemSize, pNetInfo, 0, itemSize);
                tagGameLobby sGameLobby = (tagGameLobby)StructConverterByteArray.BytesToStruct(pNetInfo, typeof(tagGameLobby));
                CServerListData.shared().InsertGameLobby(sGameLobby);
            }

            //Create CGPLobbyMission
            Loom.QueueOnMainThread(() =>
            {
                if (CServerListData.shared().GetGameLobbyCount() > 0)
                {
                    tagGameLobby gameLobby = CServerListData.shared().getARandGameLobby();
                    if (gameLobby.wLobbyPort != 0)
                    {
                        CGPLobbyMission kGPLobbyMission = CGPLobbyMission.CreateInstance(gameLobby.szServerAddr, gameLobby.wLobbyPort);
                        if (kGPLobbyMission != null && !kGPLobbyMission.isAlive())
                        {
                            kGPLobbyMission.SendChatData("StartChatConnect!");//只是为了连接上
                        }
                    }
                    else
                    {
                        Debug.LogError("onSocketListLobby: incorrect wLobbyPort=0");
                    }
                }
            });

            return true;
        }

        //比赛列表
        bool onSocketListMatch(byte[] data, int size)
        {
            tagGameMatch tempStruct = new tagGameMatch();
            int itemSize = Marshal.SizeOf(tempStruct);
           
            if (size % itemSize != 0) return false;

            //变量定义
            ushort wItemCount = (ushort)(size / itemSize);
  
            var buf = new byte[itemSize];

            tagGameMatch[] pGameMatch = new tagGameMatch[wItemCount];

            //获取对象
            CGameServerItem pGameServerItem = null;
            for (int i = 0; i < wItemCount; i++)
            {
                Buffer.BlockCopy(data, i * itemSize, buf, 0, itemSize);
                pGameMatch[i] = (tagGameMatch)StructConverterByteArray.BytesToStruct(buf, typeof(tagGameMatch));

                //更新数据
                pGameServerItem = CServerListData.shared().SearchGameServer(pGameMatch[i].wServerID);
                if (pGameServerItem != null)
                {
                    pGameServerItem.m_GameMatch = (tagGameMatch)StructConverterByteArray.BytesToStruct(buf, typeof(tagGameMatch));
                    //memcpy(&pGameServerItem.m_GameMatch, pGameMatch++, sizeof(pGameServerItem.m_GameMatch));
                }
            }

            return true;
        }

//列表完成
        bool onSocketListFinish(byte[] data, int size)
        {

            if (CServerListData.shared() != null)
            {
                CServerListData.shared().OnEventListFinish();
            }

            return true;
        }

//列表配置
        bool onSocketListConfig(byte[] data, int size)
        {
            CMD_GP_ListConfig pListConfig =
                (CMD_GP_ListConfig) StructConverterByteArray.BytesToStruct(data, typeof (CMD_GP_ListConfig));
            return true;
        }

//视频配置
        bool onSocketVideoOption(byte[] data, int size)
        {
            return true;
        }

//类型在线
        bool onSocketKindOnline(byte[] data, int size)
        {
            return true;
        }

//房间在线
        bool onSocketServerOnline(byte[] data, int size)
        {
            return true;
        }

//在线完成更新完成
        bool onSocketOnlineFinish(byte[] data, int size)
        {
            return true;
        }

    }
}


