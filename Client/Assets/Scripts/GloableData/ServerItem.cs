using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace GameNet
{
    /*---------------\GameLib\Platform\PFDefine\df\DF.h------------------*/

    //用户属性
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagUserAttribute
    {
        //用户属性
        public uint dwUserID;                         //用户标识
        public ushort wTableID;                          //桌子号码
        public ushort wChairID;                          //椅子号码

        //权限属性
        public uint dwUserRight;                      //用户权限
        public uint dwMasterRight;                        //管理权限
    };

    //游戏属性
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagGameAttribute
    {
        public ushort wKindID;                           //类型标识
        public ushort wChairCount;                       //椅子数目
        public uint dwClientVersion;                  //游戏版本
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_KIND)]
        public byte[] szGameName;              //游戏名字
    };

    //房间属性
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagServerAttribute
    {
        public ushort wKindID;                           //类型标识
        public ushort wServerID;                         //房间规则
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_SERVER)]
        public byte[] szServerName;          //房间名称
        public ushort wAVServerPort;                     //视频端口
        public uint dwAVServerAddr;                       //视频地址
        public ushort wServerType;                       //房间类型

        public ushort wTableCount;                       //桌子数目
        public ushort wChairCount;                       //椅子数目
    };

    //////////////////////////////////////////////////////////////////////////
    // 辅助定义
    //////////////////////////////////////////////////////////////////////////
    class DF
    {
        protected byte[] mGameName;
        protected int mClientVersion;
        protected int mKindID;
        protected int mGamePlayers;
        private static DF sInstance = new DF();
        public static DF shared()
        {
            return sInstance;
        }

        static byte[] MD5Encrypt(byte[] pszSourceData)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] sPassuint = md5.ComputeHash(pszSourceData);

            return sPassuint;
        }

        public DF()
        {
            mClientVersion = (0);
            mKindID = (0);
            mGamePlayers = (0);
        }

        public void init(int iKindID, int iPlayers, int iClientVersion, byte[] sGameName)
        {
            mKindID = iKindID;
            mGamePlayers = iPlayers;
            mClientVersion = iClientVersion;
            mGameName = sGameName;
        }

        public byte GetDeviceType()
        {
#if UNITY_IOS
            return SocketDefines.DEVICE_TYPE_IPHONE;
#elif UNITY_ANDROID
            return SocketDefines.DEVICE_TYPE_ANDROID;
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN
            return SocketDefines.DEVICE_TYPE_PC;
#endif
        }
        public byte[] GetMachineID()
        {
            return null;
        }

        public byte[] GetMobilePhone()
        {
            return null;
        }

        public uint GetPlazaVersion()
        {
            return GameServerDefines.VERSION_PLAZA;
        }

        public uint GetFrameVersion()
        {
            return GameServerDefines.VERSION_FRAME;
        }

        public ushort GetPlazzKindID()
        {
            return GetGameKindID();
        }

        public uint GetGameVersion()
        {
            return (uint)mClientVersion;
        }

        public ushort GetGameKindID()
        {
            return (ushort)mKindID;
        }

        public uint GetGamePlayer()
        {
            return (uint)mGamePlayers;
        }

        byte[] GetGameName(byte[] szGameName)// [LEN_KIND]
        {
            //lin: unused function
            return null;
        }
    };

    /*---------------\GameLib\Platform\PFDefine\df\DF.h------------------*/
    /*---------------\GameLib\Kernel\kernel\server\IServerMatchSink.h------------------*/
    public interface IServerMatchSink
    {
        void OnSocketSubMatchFee(CMD_GR_Match_Fee pNetInfo);
        void OnSocketSubMatchNum(CMD_GR_Match_Num pNetInfo);
        void OnSocketSubMatchInfo(CMD_GR_Match_Info pNetInfo);
        void OnSocketSubMatchWaitTip(bool bShow, CMD_GR_Match_Wait_Tip pNetInfo);
        void OnSocketSubMatchResult(CMD_GR_MatchResult pNetInfo);
        void OnSocketSubMatchStatus(byte pNetInfo);
        void OnSocketSubMatchGoldUpdate(CMD_GR_MatchGoldUpdate pNetInfo);
        void OnSocketSubMatchEliminate();
        void OnSocketSubMatchJoinResolt(bool bSucess);
    };
    /*---------------\GameLib\Kernel\kernel\server\IServerMatchSink.h------------------*/

    public interface IServerPrivateSink
    {
        void OnSocketSubPrivateInfo(CMD_GR_Private_Info pNetInfo);
        void OnSocketSubPrivateCreateSuceess(CMD_GR_Create_Private_Sucess pNetInfo);
        void OnSocketSubPrivateRoomInfo(CMD_GF_Private_Room_Info pNetInfo);
        void OnSocketSubPrivateEnd(CMD_GF_Private_End_Info pNetInfo);
        void OnSocketSubPrivateDismissInfo(CMD_GF_Private_Dismiss_Info pNetInfo);
    };

    public interface IUserManagerSink
    {
        void OnUserItemAcitve(IClientUserItem pIClientUserItem);
        void OnUserItemDelete(IClientUserItem pIClientUserItem);
        void OnUserFaceUpdate(IClientUserItem pIClientUserItem);
        void OnUserItemUpdate(IClientUserItem pIClientUserItem, ref tagUserScore LastScore);
        void OnUserItemUpdate(IClientUserItem pIClientUserItem, ref tagUserStatus LastStatus);
        void OnUserItemUpdate(IClientUserItem pIClientUserItem, ref tagUserAttrib UserAttrib);
    };

    public interface IClientKernelSink
    {

        //控制接口
        //启动游戏
        void clearInfo();
        //启动游戏
        bool SetupGameClient();
        //重置游戏
        void ResetGameClient();
        //关闭游戏
        void CloseGameClient();

        //框架事件
        //系统滚动消息
        bool OnGFTableMessage(byte[] szMessage);
        //比赛信息
        bool OnGFMatchInfo(tagMatchInfo pMatchInfo);
        //比赛等待提示
        bool OnGFMatchWaitTips(tagMatchWaitTip pMatchWaitTip);
        //比赛结果
        bool OnGFMatchResult(tagMatchResult pMatchResult);

        bool OnGFPlayerReady(int nChairID);

        //游戏事件
        //旁观消息
        bool OnEventLookonMode(byte[] data, int dataSize);
        //场景消息
        bool OnEventSceneMessage(byte cbGameStatus, bool bLookonUser, byte[] data, int dataSize);
        //场景消息
        bool OnEventGameMessage(int sub, byte[] data, int dataSize);

        //用户事件
        //用户进入
        void OnEventUserEnter(IClientUserItem pIClientUserItem, bool bLookonUser);
        //用户离开
        void OnEventUserLeave(IClientUserItem pIClientUserItem, bool bLookonUser);
        //用户积分
        void OnEventUserScore(IClientUserItem pIClientUserItem, bool bLookonUser);
        //用户状态
        void OnEventUserStatus(IClientUserItem pIClientUserItem, bool bLookonUser);
        //用户属性
        void OnEventUserAttrib(IClientUserItem pIClientUserItem, bool bLookonUser);
        //用户头像
        void OnEventCustomFace(IClientUserItem pIClientUserItem, bool bLookonUser);

        //私人房
        void OnSocketSubPrivateRoomInfo(CMD_GF_Private_Room_Info pNetInfo);
        void OnSocketSubPrivateEnd(CMD_GF_Private_End_Info pNetInfo);
        void OnSocketSubPrivateDismissInfo(CMD_GF_Private_Dismiss_Info pNetInfo);

        bool RevTalkFile(byte[] data, int dataSize);

        //mChen：聊天消息
        bool OnSubUserChatIndex(byte[] data, int dataSize);
        bool OnSubUserExpressionIndex(byte[] data, int dataSize);
    };

    //查找桌子
    public struct tagFindTable
    {
        public bool bOneNull;                          //一个空位
        public bool bTwoNull;                          //两个空位
        public bool bAllNull;                          //全空位置
        public bool bNotFull;                          //不全满位
        public bool bFilterPass;                       //过滤密码
        public ushort wBeginTableID;                     //开始索引
        public ushort wResultTableID;                        //结果桌子
        public ushort wResultChairID;                        //结果椅子
    };

    //房间
    public interface IServerItem
    {

        ISocketEngine GetServerItemSocketEngine();
        //配置接口

        //设置接口
        bool SetServerItemSink(IServerItemSink pIServerItemSink);
        //设置接口
        bool SetStringMessageSink(IStringMessageSink pIStringMessageSink);
        //设置接口
        bool SetServerMatchSink(IServerMatchSink pIServerMatchSink);
        //设置接口
        bool SetServerPrivateSink(IServerPrivateSink pIServerMatchSink);

        //连接接口

        //连接服务器
        bool ConnectServer(CGameServerItem pGameServerItem, ushort wAVServerPort, uint dwAVServerAddr);
        //连接断开
        bool IntermitConnect(bool force);

        //属性接口

        //用户属性
        tagUserAttribute GetUserAttribute();
        //房间属性
        tagServerAttribute GetServerAttribute();
        //服务状态
        enServiceStatus GetServiceStatus();
        //是否服务状态
        bool IsService();
        //设置状态
        void SetServiceStatus(enServiceStatus ServiceStatus);
        //自己状态
        bool IsPlayingMySelf();
        //用户接口

        //自己位置
        IClientUserItem GetMeUserItem();
        //游戏用户
        IClientUserItem GetTableUserItem(ushort wChariID);
        //查找用户
        IClientUserItem SearchUserByUserID(uint dwUserID);
        //查找用户
        IClientUserItem SearchUserByGameID(uint dwGameID);
        //查找用户
        IClientUserItem SearchUserByNickName(byte[] szNickName);

        //网络接口

        //发送函数
        bool SendSocketData(ushort wMainCmdID, ushort wSubCmdID);
        //发送函数
        bool SendSocketData(ushort wMainCmdID, ushort wSubCmdID, byte[] pData, ushort wDataSize);

        //mChen
        void sendMatchSignupCheck();//mChen test
        void sendMacthFree();

        //动作处理

        //执行快速加入
        bool PerformQuickSitDown();
        //执行旁观
        bool PerformLookonAction(ushort wTableID, ushort wChairID);
        //执行起立
        bool PerformStandUpAction();
        //执行坐下
        bool PerformSitDownAction(ushort wTableID, ushort wChairID, bool bEfficacyPass, byte[] psw = null);
        //执行购买
        bool PerformBuyProperty(byte cbRequestArea, byte[] pszNickName, ushort wItemCount, ushort wPropertyIndex);

        //桌子消息

        //获得空桌子
        int GetSpaceTableId();
        //获得对应桌子的空椅子
        int GetSpaceChairId(int tableId);
        //获得总桌子数
        int GetTotalTableCount();
        //获得游戏服务
        //////////////////////////////////////////////////////////////////////////
        // 框架消息

        //游戏已准备好
        void OnGFGameReady();
        //游戏关闭
        void OnGFGameClose(int iExitCode);

        //设置内核接口
        void SetClientKernelSink(IClientKernelSink pIClientKernelSink);
        //獲取內核接口
        IClientKernelSink GetClientKernelSink();
        //获取游戏状态
        byte GetGameStatus();
        //设置游戏状态
        void SetGameStatus(byte cbGameStatus);

        bool SendUserReady(byte[] data, ushort dataSize);
        bool SendCreaterUserPressedStart(byte[] data, ushort dataSize);
        //////////////////////////////////////////////////////////////////////////
        // Socket消息

        // 发送数据
        bool GFSendData(int main, int sub, byte[] data, int size);


        //IServerItem create();
        //   void destory();
        //   IServerItem get();
    };

    public class CServerItem : IServerItem, IUserManagerSink, ISocketEngineSink
    {
        static int __gServerItemRefCount = 0;
        static IServerItem __gServerItem = null;
        public static IServerItem create()
        {
            if (__gServerItemRefCount == 0)
            {
                __gServerItem = new CServerItem();
            }

            __gServerItemRefCount++;
            Debug.Log("IServerItem::create ref: " + __gServerItemRefCount);

            if (__gServerItemRefCount > 1)
            {
                Debug.LogError("CServerItem::create __gServerItemRefCount=" + __gServerItemRefCount);
            }

            return __gServerItem;
        }

        public static void destory()
        {
            if (__gServerItemRefCount > 0)
            {
                __gServerItemRefCount--;
                if (__gServerItemRefCount <= 0)
                {
                    __gServerItemRefCount = 0;
                    __gServerItem = null;
                }
            }
            Debug.Log("IServerItem::destory ref: " + __gServerItemRefCount);
        }

        public static IServerItem get()
        {
            return __gServerItem;
        }

        public CServerItem()
        {
            m_wReqTableID = (SocketDefines.INVALID_TABLE);
            m_wReqChairID = (SocketDefines.INVALID_CHAIR);
            mFindTableID = (SocketDefines.INVALID_TABLE);
            mIsGameReady = (false);
            m_pMeUserItem = null;
            mIServerItemSink = null;
            mIServerMatchSink = null;
            mIServerPrivateSink = null;
            mIStringMessageSink = null;
            mSocketEngine = null;
            mServiceStatus = (enServiceStatus.ServiceStatus_Unknow);
            mIClientKernelSink = null;
            mGameStatus = (SocketDefines.GAME_STATUS_FREE);

            mIsQuickSitDown = false;

            mSocketEngine = new CSocketEngine();
            mUserManager = new CPlazaUserManager();
            mUserManager.SetUserManagerSink(this);
            mSocketEngine.setSocketEngineSink(this);
            mServerAttribute = new tagServerAttribute();
            m_TableFrame = new CTableViewFrame();

            hnGameManager = null;
        }
        public ISocketEngine GetServerItemSocketEngine()
        {
            return mSocketEngine;
        }
        //////////////////////////////////////////////////////////////////////////
        // IServerItem
        //////////////////////////////////////////////////////////////////////////


        //配置接口
        //设置接口
        public bool SetServerItemSink(IServerItemSink pIServerItemSink)
        {
            mIServerItemSink = pIServerItemSink;
            return true;
        }
        //设置接口
        public bool SetStringMessageSink(IStringMessageSink pIStringMessageSink)
        {
            mIStringMessageSink = pIStringMessageSink;
            return true;
        }
        //设置接口
        public bool SetServerMatchSink(IServerMatchSink pIServerMatchSink)
        {
            mIServerMatchSink = pIServerMatchSink;
            return true;
        }
        //设置接口
        public bool SetServerPrivateSink(IServerPrivateSink pIServerMatchSink)
        {
            mIServerPrivateSink = pIServerMatchSink;
            return true;
        }
        //配置房间
        public bool SetServerAttribute(CGameServerItem pGameServerItem, ushort wAVServerPort, uint dwAVServerAddr)
        {
            //房间属性
            CGameKindItem pGameKindItem = pGameServerItem.m_pGameKindItem;
#if false
            mGameKind = pGameKindItem.m_GameKind;
            mGameServer = pGameServerItem.m_GameServer;
#else
            var buf = StructConverterByteArray.StructToBytes(pGameKindItem.m_GameKind);
            mGameKind = (tagGameKind)StructConverterByteArray.BytesToStruct(buf, typeof(tagGameKind));
            buf = StructConverterByteArray.StructToBytes(pGameServerItem.m_GameServer);
            mGameServer = (tagGameServer)StructConverterByteArray.BytesToStruct(buf, typeof(tagGameServer));
#endif
            //memcpy(&mGameKind, &pGameKindItem.m_GameKind, sizeof(mGameKind));
            //memcpy(&mGameServer, &pGameServerItem.m_GameServer, sizeof(mGameServer));

            mServerAttribute.wKindID = mGameServer.wKindID;
            mServerAttribute.wServerID = mGameServer.wServerID;
            mServerAttribute.wServerType = mGameServer.wServerType;

            mServerAttribute.wAVServerPort = wAVServerPort;
            mServerAttribute.dwAVServerAddr = dwAVServerAddr;
            mServerAttribute.szServerName = new byte[SocketDefines.LEN_SERVER];
            Buffer.BlockCopy(mGameServer.szServerName, 0, mServerAttribute.szServerName, 0, SocketDefines.LEN_SERVER);
            //strncpy(mServerAttribute.szServerName, mGameServer.szServerName, LEN_SERVER);
#if true
            CParameterGlobal pParameterGlobal = CParameterGlobal.shared();
            //加载配置
            mParameterGame = pParameterGlobal.GetParameterGame(mGameKind);
            mParameterServer = pParameterGlobal.GetParameterServer(mGameServer);
#endif
            return true;
        }

        //连接接口
        //初始化房间
        public bool ConnectServer(CGameServerItem pGameServerItem, ushort wAVServerPort, uint dwAVServerAddr)
        {
            // temp hack: fix比赛时间未到参加导致的mServiceStatus==ServiceStatus_Entering，从而引起之后任何模式登陆都报System_Tips_1
            if (true)//if (pGameServerItem.m_GameServer.wServerType == GAME_GENRE_MATCH) 
            {
                IntermitConnect(true);
            }

            //IntermitConnect(true);
            if (mServiceStatus != enServiceStatus.ServiceStatus_Unknow &&
                mServiceStatus != enServiceStatus.ServiceStatus_NetworkDown)
            {
                if (mIStringMessageSink != null)
                    mIStringMessageSink.InsertPromptString(Encoding.Default.GetBytes("您的网络不稳定，请稍后再试！"), GameServerDefines.DLG_MB_OK);//"System_Tips_1"
                return false;
            }

            //房间属性
            SetServerAttribute(pGameServerItem, wAVServerPort, dwAVServerAddr);

            //关闭判断
            if (mServerAttribute.wServerID == 0)
            {
                SetServiceStatus(enServiceStatus.ServiceStatus_Unknow);
                if (mIStringMessageSink != null)
                    mIStringMessageSink.InsertPromptString(Encoding.Default.GetBytes("System_Tips_2"), GameServerDefines.DLG_MB_OK);
                return false;
            }

            //memcpy(&mGameServer.szServerAddr, "219.82.50.83", sizeof(mGameServer.szServerAddr));//mChen hack

            //创建组件
            Debug.LogFormat("Connect {0} port : {1}", Encoding.Default.GetString(mGameServer.szServerAddr), mGameServer.wServerPort);
            if (!mSocketEngine.connect(Encoding.Default.GetString(mGameServer.szServerAddr), mGameServer.wServerPort))
            {
                if (mIStringMessageSink != null)
                    mIStringMessageSink.InsertPromptString(Encoding.Default.GetBytes("System_Tips_3"), GameServerDefines.DLG_MB_OK);
                return false;
            }

            //设置状态
            SetServiceStatus(enServiceStatus.ServiceStatus_Entering);
            return true;
        }
        //中断连接
        public bool IntermitConnect(bool force)
        {
            Debug.Log("CServerItem::IntermitConnect1");
            if (mServiceStatus == enServiceStatus.ServiceStatus_Unknow ||
                mServiceStatus == enServiceStatus.ServiceStatus_NetworkDown)
                return false;
            //设置状态
            Debug.Log("CServerItem::IntermitConnect2");
            SetServiceStatus(enServiceStatus.ServiceStatus_NetworkDown);

            OnGFGameClose(0);

            if (mSocketEngine != null)
            {
                mSocketEngine.disconnect();
            }

            if (mUserManager != null)
            {
                mUserManager.ResetUserItem();
            }

            m_wReqTableID = SocketDefines.INVALID_TABLE;
            m_wReqChairID = SocketDefines.INVALID_CHAIR;
            mFindTableID = SocketDefines.INVALID_TABLE;
            mIsGameReady = false;
            m_pMeUserItem = null;

            mUserManager.ResetUserItem();

            return true;
        }


        //属性接口

        //用户属性
        public tagUserAttribute GetUserAttribute()
        {
            return mUserAttribute;
        }


        //房间属性
        public tagServerAttribute GetServerAttribute()
        {
            return mServerAttribute;
        }
        //服务状态
        public enServiceStatus GetServiceStatus()
        {
            return mServiceStatus;
        }
        //是否服务状态
        public bool IsService()
        {
            return GetServiceStatus() == enServiceStatus.ServiceStatus_ServiceIng;
        }
        //设置状态
        public void SetServiceStatus(enServiceStatus ServiceStatus)
        {
            //设置变量
            mServiceStatus = ServiceStatus;
        }
        //自己状态
        public bool IsPlayingMySelf()
        {
            return ((m_pMeUserItem != null) && (m_pMeUserItem.GetUserStatus() == SocketDefines.US_PLAYING));
        }

        //用户接口

        //自己位置
        public ushort GetMeChairID()
        {
            if (m_pMeUserItem == null) return SocketDefines.INVALID_CHAIR;
            return m_pMeUserItem.GetChairID();
        }
        //自己位置
        public IClientUserItem GetMeUserItem()
        {
            return m_pMeUserItem;
        }
        //游戏用户
        public IClientUserItem GetTableUserItem(ushort wChairID)
        {
            return mUserManager.EnumUserItem(wChairID);
        }
        //查找用户
        public IClientUserItem SearchUserByUserID(uint dwUserID)
        {
            return mUserManager.SearchUserByUserID(dwUserID);
        }
        //查找用户
        public IClientUserItem SearchUserByGameID(uint dwGameID)
        {
            return mUserManager.SearchUserByGameID(dwGameID);
        }
        //查找用户
        public IClientUserItem SearchUserByNickName(byte[] szNickName)
        {
            return mUserManager.SearchUserByNickName(szNickName);
        }
        //用户数
        public uint GetActiveUserCount()
        {
            return mUserManager.GetActiveUserCount();
        }

        //桌子接口

        //获取对应桌子是否锁的状态
        public bool GetTableLockState(int tableId)
        {
            return m_TableFrame.GetLockerFlag((ushort)tableId);

        }
        //桌子是否游戏
        public bool GetTableGameState(int tableId)
        {
            return m_TableFrame.GetPlayFlag((ushort)tableId);

        }

        //网络接口
        //发送函数
        public bool SendSocketData(ushort wMainCmdID, ushort wSubCmdID)
        {
            return SendSocketData(wMainCmdID, wSubCmdID, null, 0);
        }
        //发送函数
        public bool SendSocketData(ushort wMainCmdID, ushort wSubCmdID, byte[] data, ushort dataSize)
        {
            return mSocketEngine.send(wMainCmdID, wSubCmdID, data, dataSize);
        }

        //网络命令
        //发送登录
        public bool SendLogonPacket()
        {
            //变量定义
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

            //变量定义
            CMD_GR_LogonUserID LogonUserID = new CMD_GR_LogonUserID();
            LogonUserID.Init();

            LogonUserID.wKindID = mGameKind.wKindID;
            //游戏版本
            LogonUserID.dwProcessVersion = DF.shared().GetGameVersion();

            LogonUserID.dwPlazaVersion = DF.shared().GetPlazaVersion();

            //登录信息
            LogonUserID.dwUserID = pGlobalUserData.dwUserID;
            LogonUserID.szPassword = pGlobalUserData.szPassword;
            //strncpy(LogonUserID.szPassuint, pGlobalUserData.szPassuint, countarray(LogonUserID.szPassuint));

            //发送数据
            var buf = StructConverterByteArray.StructToBytes(LogonUserID);
            SendSocketData(GameServerDefines.MDM_GR_LOGON, GameServerDefines.SUB_GR_LOGON_USERID, buf, (ushort)buf.Length);
            return true;
        }
        //发送规则
        public bool SendUserRulePacket()
        {
            //发送数据
            return true;
        }
        //发送旁观
        public bool SendLookonPacket(ushort wTableID, ushort wChairID)
        {
            return true;
        }
        //发送坐下
        public bool SendSitDownPacket(ushort wTableID, ushort wChairID, byte[] lpszPassuint = null)
        {
            if (!IsService())
                return false;

            //自己状态
            if (m_pMeUserItem.GetUserStatus() >= SocketDefines.US_PLAYING)
            {
                //提示消息
                if (wTableID != m_pMeUserItem.GetTableID() && wChairID != m_pMeUserItem.GetChairID())
                {
                    if (mIStringMessageSink != null)
                        mIStringMessageSink.InsertPromptString(Encoding.Default.GetBytes("InGame"), GameServerDefines.DLG_MB_OK);
                    return false;
                }
            }

            CMD_GR_UserSitDown UserSitReq = new CMD_GR_UserSitDown();
            UserSitReq.Init();

            UserSitReq.wTableID = wTableID;
            UserSitReq.wChairID = wChairID;
            if (lpszPassuint != null)
            {
                Buffer.BlockCopy(lpszPassuint, 0, UserSitReq.szTablePass, 0, lpszPassuint.Length);
            }
            //发送数据包
            var buf = StructConverterByteArray.StructToBytes(UserSitReq);
            SendSocketData(GameServerDefines.MDM_GR_USER, GameServerDefines.SUB_GR_USER_SITDOWN, buf, (ushort)buf.Length);
            return true;
        }
        //发送起立
        public bool SendStandUpPacket(ushort wTableID, ushort wChairID, byte cbForceLeave)
        {
            CMD_GR_UserStandUp UserStandUp = new CMD_GR_UserStandUp();

            //构造数据
            UserStandUp.wTableID = wTableID;
            UserStandUp.wChairID = wChairID;
            UserStandUp.cbForceLeave = cbForceLeave;

            //发送数据
            var buf = StructConverterByteArray.StructToBytes(UserStandUp);
            SendSocketData(GameServerDefines.MDM_GR_USER, GameServerDefines.SUB_GR_USER_STANDUP, buf, (ushort)buf.Length);
            return true;
        }
        //发送表情
        public bool SendExpressionPacket(uint dwTargetUserID, ushort wItemIndex)
        {
            return true;
        }
        //发送聊天
        public bool SendUserChatPacket(uint dwTargetUserID, byte[] pszChatString, uint dwColor)
        {
            return true;
        }

        //动作处理
        //查找桌子
        public bool FindGameTable(ref tagFindTable FindInfo)
        {
#if true
            //变量定义
            ushort wNullCount = 0;
            ITableView pTableView = null;
            uint wChairUser = m_TableFrame.GetChairCount();
            uint wMeTableID = m_pMeUserItem.GetTableID();

            //搜索桌子
            for (uint i = 0; i < m_TableFrame.GetTableCount(); i++)
            {
                FindInfo.wResultTableID = (ushort)((FindInfo.wBeginTableID + i) % m_TableFrame.GetTableCount());
                if (wMeTableID != FindInfo.wResultTableID)
                {
                    //获取桌子
                    pTableView = m_TableFrame.GetTableViewItem(FindInfo.wResultTableID);
                    Debug.Assert(pTableView != null);

                    //判断是否开始游戏
                    if (pTableView.GetPlayFlag() == true) continue;

                    //过滤密码
                    bool bTablePass = pTableView.GetLockerFlag();
                    if ((FindInfo.bFilterPass == true) && (bTablePass == true)) continue;

                    //寻找空位置
                    wNullCount = pTableView.GetNullChairCount(ref FindInfo.wResultChairID, m_pMeUserItem.GetUserID());

                    //判断数目
                    if (wNullCount > 0)
                    {
                        //效验规则
                        byte[] strDescribe = new byte[256];
                        //char strDescribe[256];
                        if (EfficacyTableRule(FindInfo.wResultTableID, FindInfo.wResultChairID, false, strDescribe) == false)
                        {
                            continue;
                        }

                        if ((FindInfo.bNotFull == true) && (wNullCount < wChairUser)) return true;
                        if ((FindInfo.bOneNull == true) && (wNullCount == 1)) return true;
                        if ((FindInfo.bTwoNull == true) && (wNullCount == 2)) return true;
                        if ((FindInfo.bAllNull == true) && (wNullCount == wChairUser)) return true;
                    }
                }
            }

            //设置数据
            FindInfo.wResultTableID = SocketDefines.INVALID_TABLE;
            FindInfo.wResultChairID = SocketDefines.INVALID_CHAIR;
#endif
            return false;

        }
        //执行快速加入
        public bool PerformQuickSitDown()
        {
            if (!IsService())
                return false;

            //自己状态
            if (m_pMeUserItem.GetUserStatus() >= SocketDefines.US_PLAYING)
            {
                if (mIStringMessageSink != null)
                    mIStringMessageSink.InsertPromptString(Encoding.Default.GetBytes("InGame"), GameServerDefines.DLG_MB_OK);
            }

            //先搜索桌子不全空的
            tagFindTable FindInfo = new tagFindTable();
            FindInfo.bAllNull = false;
            FindInfo.bFilterPass = true;
            FindInfo.bNotFull = true;
            FindInfo.bOneNull = true;
            FindInfo.bTwoNull = (m_TableFrame.GetChairCount() != 2);
            FindInfo.wBeginTableID = (ushort)(mFindTableID + 1);
            FindInfo.wResultTableID = SocketDefines.INVALID_TABLE;
            FindInfo.wResultChairID = SocketDefines.INVALID_CHAIR;
            bool bSuccess = FindGameTable(ref FindInfo);
            mFindTableID = FindInfo.wResultTableID;

            //搜索全部游戏桌
            if (bSuccess == false)
            {
                FindInfo.bAllNull = true;
                FindInfo.bFilterPass = true;
                FindInfo.bNotFull = true;
                FindInfo.bOneNull = true;
                FindInfo.bTwoNull = true;
                FindInfo.wBeginTableID = (ushort)(mFindTableID + 1);
                FindInfo.wResultTableID = SocketDefines.INVALID_TABLE;
                FindInfo.wResultChairID = SocketDefines.INVALID_CHAIR;
                bSuccess = FindGameTable(ref FindInfo);
                mFindTableID = FindInfo.wResultTableID;
            }

            //结果判断
            if (bSuccess == true)
            {
                //自动坐下
                PerformSitDownAction(mFindTableID, FindInfo.wResultChairID, false);
            }
            else
            {
                if (mIStringMessageSink != null)
                    mIStringMessageSink.InsertPromptString(Encoding.Default.GetBytes("No Table"), GameServerDefines.DLG_MB_OK);
            }

            return true;
        }
        //执行旁观
        public bool PerformLookonAction(ushort wTableID, ushort wChairID)
        {
            if (!IsService())
                return false;


            //状态过滤
            if (mServiceStatus != enServiceStatus.ServiceStatus_ServiceIng) return false;
            if ((m_wReqTableID == wTableID) && (m_wReqChairID == wChairID)) return false;

            //自己状态
            if (m_pMeUserItem.GetUserStatus() >= SocketDefines.US_PLAYING)
            {
                //提示消息
                if (mIStringMessageSink != null)
                    mIStringMessageSink.InsertPromptString(Encoding.Default.GetBytes(">=US_PLAYING"), GameServerDefines.DLG_MB_OK);
                return false;
            }
#if false
            //权限判断
            if (CUserRight::CanLookon(mUserAttribute.dwUserRight) == false)
            {
                //提示消息
                if (mIStringMessageSink!=null)
                    mIStringMessageSink.InsertPromptString(Encoding.Default.GetBytes("CanLookon == false"), GameServerDefines.DLG_MB_OK);
                return false;
            }
#endif
            //清理界面
            if ((m_wReqTableID != SocketDefines.INVALID_TABLE) && (m_wReqChairID != SocketDefines.INVALID_CHAIR) && (m_wReqChairID != SocketDefines.MAX_CHAIR))
            {
                IClientUserItem pIClientUserItem = m_TableFrame.GetClientUserItem(m_wReqTableID, m_wReqChairID);
                if (pIClientUserItem == m_pMeUserItem) m_TableFrame.SetClientUserItem(m_wReqTableID, m_wReqChairID, null);
            }

            //设置变量
            m_wReqTableID = wTableID;
            m_wReqChairID = wChairID;
            mFindTableID = SocketDefines.INVALID_TABLE;

            //设置界面
            m_TableFrame.VisibleTable(wTableID);

            Debug.Log("CServerItem::PerformLookonAction send\n");
            //发送命令
            SendLookonPacket(wTableID, wChairID);

            return true;
        }
        //执行起立
        public bool PerformStandUpAction()
        {
            if (!IsService())
                return false;

            //状态过滤
            if (mServiceStatus != enServiceStatus.ServiceStatus_ServiceIng) return false;

            //设置界面
            Debug.Log("CServerItem::PerformStandUpAction send\n");

            //发送命令
            if (m_pMeUserItem != null)
            {
                SendStandUpPacket(m_pMeUserItem.GetTableID(), m_pMeUserItem.GetChairID(), 1);
            }

            return true;
        }
        //执行坐下
        public bool PerformSitDownAction(ushort wTableID, ushort wChairID, bool bEfficacyPass, byte[] psw = null)
        {
            if (!IsService())
                return false;

            //状态过滤
            if (mServiceStatus != enServiceStatus.ServiceStatus_ServiceIng) return false;
            if ((m_wReqTableID != SocketDefines.INVALID_TABLE) && (m_wReqTableID == wTableID)) return false;
            if ((m_wReqChairID != SocketDefines.INVALID_CHAIR) && (m_wReqChairID == wChairID)) return false;

            //密码判断
            //char szPassword[LEN_PASSWORD] = { 0 };
            byte[] szPassword = new byte[SocketDefines.LEN_PASSWORD];

            //设置变量
            m_wReqTableID = 0;
            m_wReqChairID = 0;
            mFindTableID = SocketDefines.INVALID_TABLE;


            Debug.LogFormat("CServerItem::PerformSitDownAction send... wTableID ={0}  wCharID ={1} \n,", wTableID, wChairID);
            //发送命令
            SendSitDownPacket(SocketDefines.INVALID_TABLE, SocketDefines.INVALID_CHAIR, szPassword);

            return true;
        }
        //执行购买
        public bool PerformBuyProperty(byte cbRequestArea, byte[] pszNickName, ushort wItemCount, ushort wPropertyIndex)
        {
            if (!IsService())
                return false;

            return true;
        }

        //内部函数

        static uint dwChatTime = 0;
        //聊天效验
        bool EfficacyUserChat(byte[] pszChatString, ushort wExpressionIndex)
        {
            byte cbMemberOrder = m_pMeUserItem.GetMemberOrder();
            byte cbMasterOrder = m_pMeUserItem.GetMasterOrder();
#if false
            //权限判断
            if (CUserRight::CanRoomChat(mUserAttribute.dwUserRight) == false)
            {
                return false;
            }
#endif
            //速度判断
            uint dwCurrentTime = (uint)DateTime.Now.Second;
            if ((cbMasterOrder == 0) && ((dwCurrentTime - dwChatTime) <= SocketDefines.TIME_USER_CHAT))
            {
                return false;
            }

            //设置变量
            dwChatTime = dwCurrentTime;
            return true;
        }
        //桌子效验
        protected bool EfficacyTableRule(ushort wTableID, ushort wChairID, bool bReqLookon, byte[] strDescribe)
        {
            //状态过滤
            if (mServiceStatus != enServiceStatus.ServiceStatus_ServiceIng) return false;
            if (wTableID >= m_TableFrame.GetTableCount()) return false;
            if (wChairID >= m_TableFrame.GetChairCount()) return false;

            //变量定义
            CParameterGlobal pParameterGlobal = CParameterGlobal.shared();

            //变量定义
            ITableView pITableView = m_TableFrame.GetTableViewItem(wTableID);
            //IClientUserItem * pITableUserItem=pITableView.GetClientUserItem(wChairID);

            //变量定义
            bool bGameStart = pITableView.GetPlayFlag();

            //游戏状态
            if ((bGameStart == true) && (bReqLookon == false))
            {
                Debug.Log(strDescribe + ("  Game alerdy start, you not come in!"));
                return false;
            }

            //其他判断
            if ((bReqLookon == false) && (m_pMeUserItem.GetMasterOrder() == 0))
            {
                //规则判断
                for (ushort i = 0; i < m_TableFrame.GetChairCount(); i++)
                {
                    //获取用户
                    IClientUserItem pITableUserItem = pITableView.GetClientUserItem(i);
                    if ((pITableUserItem == null) || (pITableUserItem == m_pMeUserItem)) continue;

                    //厌恶玩家
                    if (pParameterGlobal.m_bLimitDetest == true)
                    {
                        if (pITableUserItem.GetUserCompanion() == SocketDefines.CP_DETEST)
                        {
                            //设置提示
                            Debug.Log(strDescribe + (" System_Tips_15 ") + pITableUserItem.GetNickName());
                            return false;
                        }
                    }

                    //胜率效验
                    if (mParameterGame.m_bLimitWinRate == true)
                    {
                        if (((ushort)(pITableUserItem.GetUserWinRate() * 1000L)) < mParameterGame.m_wMinWinRate)
                        {
                            Debug.Log(strDescribe + ("System_Tips_16") + pITableUserItem.GetNickName());
                            return false;
                        }
                    }

                    //逃率效验
                    if (mParameterGame.m_bLimitFleeRate)
                    {
                        if (((ushort)(pITableUserItem.GetUserFleeRate() * 1000L)) < mParameterGame.m_wMaxFleeRate)
                        {
                            Debug.Log(strDescribe + ("System_Tips_17") + pITableUserItem.GetNickName());
                            return false;
                        }
                    }

                    //积分效验
                    if (mParameterGame.m_bLimitGameScore)
                    {
                        //最高积分
                        if (pITableUserItem.GetUserScore() > mParameterGame.m_lMaxGameScore)
                        {
                            Debug.Log(strDescribe + ("System_Tips_18") + pITableUserItem.GetNickName());
                            return false;
                        }

                        //最低积分
                        if (pITableUserItem.GetUserScore() < mParameterGame.m_lMinGameScore)
                        {
                            Debug.Log(strDescribe + ("System_Tips_19") + pITableUserItem.GetNickName());
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        //获得空的桌子id
        public int GetSpaceTableId()
        {
            int tableCount = m_TableFrame.GetTableCount();
            for (ushort i = 0; i < tableCount; i++)
            {
                ITableView tableView = m_TableFrame.GetTableViewItem(i);
                if (tableView != null)
                {
                    for (byte j = 0; j < m_TableFrame.GetChairCount(); j++)
                    {
                        IClientUserItem userItem = tableView.GetClientUserItem(j);
                        if (userItem == null)
                        {
                            return i;
                        }
                    }
                }
            }
            return SocketDefines.INVALID_TABLE;
        }
        //获得空的椅子
        public int GetSpaceChairId(int tableId)
        {
            ITableView tableView = m_TableFrame.GetTableViewItem((ushort)tableId);
            if (tableView != null)
            {
                for (byte j = 0; j < m_TableFrame.GetChairCount(); j++)
                {
                    IClientUserItem userItem = tableView.GetClientUserItem(j);
                    if (userItem == null)
                    {
                        return j;
                    }
                }
            }
            return SocketDefines.INVALID_CHAIR;
        }
        //获得桌子总数量
        public int GetTotalTableCount()
        {
            return m_TableFrame.GetTableCount();
        }
        //获得游戏服务
        //////////////////////////////////////////////////////////////////////////
        // IUserManagerSink
        //////////////////////////////////////////////////////////////////////////
        public void OnUserItemAcitve(IClientUserItem pIClientUserItem)
        {
            //判断自己
            if (m_pMeUserItem == null)
            {
                m_pMeUserItem = pIClientUserItem;
            }

            //设置桌子
            byte cbUserStatus = pIClientUserItem.GetUserStatus();
            if ((cbUserStatus >= SocketDefines.US_SIT) && (cbUserStatus != SocketDefines.US_LOOKON))
            {
                ushort wTableID = pIClientUserItem.GetTableID();
                ushort wChairID = pIClientUserItem.GetChairID();
                m_TableFrame.SetClientUserItem(wTableID, wChairID, pIClientUserItem);
            }

            //提示信息
            CParameterGlobal pParameterGlobal = CParameterGlobal.shared();
            if ((pParameterGlobal.m_bNotifyUserInOut == true) && (mServiceStatus == enServiceStatus.ServiceStatus_ServiceIng))
            {
                if (mIStringMessageSink != null)
                    mIStringMessageSink.InsertUserEnter(pIClientUserItem.GetNickName());
            }

            if (mIServerItemSink != null)
                mIServerItemSink.OnGRUserEnter(pIClientUserItem);

            if (mIClientKernelSink != null)
                mIClientKernelSink.OnEventUserEnter(pIClientUserItem,
                    pIClientUserItem.GetUserStatus() == SocketDefines.US_LOOKON);
        }

        public void OnUserItemDelete(IClientUserItem pIClientUserItem)
        {//变量定义
            ushort wLastTableID = pIClientUserItem.GetTableID();
            ushort wLastChairID = pIClientUserItem.GetChairID();
            uint dwLeaveUserID = pIClientUserItem.GetUserID();

            //变量定义
            CParameterGlobal pParameterGlobal = CParameterGlobal.shared();


            if (mIServerItemSink != null)
                mIServerItemSink.OnGRUserDelete(pIClientUserItem);

            //提示信息
            if ((pParameterGlobal.m_bNotifyUserInOut == true) && (mServiceStatus == enServiceStatus.ServiceStatus_ServiceIng))
            {
                if (mIStringMessageSink != null)
                    mIStringMessageSink.InsertUserLeave(pIClientUserItem.GetNickName());
            }

            if (mIClientKernelSink != null)
                mIClientKernelSink.OnEventUserLeave(pIClientUserItem
                , pIClientUserItem.GetUserStatus() == SocketDefines.US_LOOKON);

            if (m_pMeUserItem == pIClientUserItem)
            {
                m_pMeUserItem = null;
            }

        }

        public void OnUserFaceUpdate(IClientUserItem pIClientUserItem)
        {
            //变量定义
            tagUserInfo pUserInfo = pIClientUserItem.GetUserInfo();
            tagCustomFaceInfo pCustomFaceInfo = pIClientUserItem.GetCustomFaceInfo();


            if (mIServerItemSink != null)
                mIServerItemSink.OnGRUserUpdate(pIClientUserItem);

            //更新桌子
            if ((pUserInfo.wTableID != SocketDefines.INVALID_TABLE) && (pUserInfo.cbUserStatus != SocketDefines.US_LOOKON))
            {
                m_TableFrame.UpdateTableView(pUserInfo.wTableID);
            }
        }
        public void OnUserItemUpdate(IClientUserItem pIClientUserItem, ref tagUserScore LastScore)
        {//变量定义
            tagUserInfo pUserInfo = pIClientUserItem.GetUserInfo();
            tagUserInfo pMeUserInfo = m_pMeUserItem.GetUserInfo();


            //房间界面通知
            if (pIClientUserItem == m_pMeUserItem)
            {
                //变量定义
                GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
                tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
                tagUserInsureInfo pUserInsureData = pGlobalUserInfo.GetUserInsureInfo();

                //设置变量
                pUserInsureData.lUserScore += pIClientUserItem.GetUserScore() - LastScore.lScore;
                pUserInsureData.lUserInsure += pIClientUserItem.GetUserInsure() - LastScore.lInsure;
                pGlobalUserData.lUserScore = pUserInsureData.lUserScore;
                pGlobalUserData.lUserInsure = pUserInsureData.lUserInsure;
            }

            if (mIServerItemSink != null)
                mIServerItemSink.OnGRUserUpdate(pIClientUserItem);

            //游戏通知
            if ((pMeUserInfo.wTableID != SocketDefines.INVALID_TABLE) && (pUserInfo.wTableID == pMeUserInfo.wTableID))
            {
                if (mIClientKernelSink != null)
                    mIClientKernelSink.OnEventUserScore(pIClientUserItem
                    , pIClientUserItem.GetUserStatus() == SocketDefines.US_LOOKON);
            }

        }

        public void OnUserItemUpdate(IClientUserItem pIClientUserItem, ref tagUserStatus LastStatus)
        {
            //变量定义
            tagUserInfo pUserInfo = pIClientUserItem.GetUserInfo();
            tagUserInfo pMeUserInfo = m_pMeUserItem.GetUserInfo();
            ushort wNowTableID = pIClientUserItem.GetTableID(), wLastTableID = LastStatus.wTableID;
            ushort wNowChairID = pIClientUserItem.GetChairID(), wLastChairID = LastStatus.wChairID;
            byte cbNowStatus = pIClientUserItem.GetUserStatus(), cbLastStatus = LastStatus.cbUserStatus;

            // 更新界面上的 分数
            if (mIServerItemSink != null)
            {
                //Debug.Log("CServerItem::OnUserItemUpdate_2");
                mIServerItemSink.OnGRUserUpdate(pIClientUserItem);
            }


            //桌子离开
            if ((wLastTableID != SocketDefines.INVALID_TABLE) && ((wLastTableID != wNowTableID) || (wLastChairID != wNowChairID)))
            {
                //Debug.Log("CServerItem::OnUserItemUpdate_3");
                IClientUserItem pITableUserItem = m_TableFrame.GetClientUserItem(wLastTableID, wLastChairID);
                if (pITableUserItem == pIClientUserItem) m_TableFrame.SetClientUserItem(wLastTableID, wLastChairID, null);
            }

            //桌子加入
            if ((wNowTableID != SocketDefines.INVALID_TABLE) && (cbNowStatus != SocketDefines.US_LOOKON) && ((wNowTableID != wLastTableID) || (wNowChairID != wLastChairID)))
            {
                //Debug.Log("CServerItem::OnUserItemUpdate_4");
                //厌恶判断（黑名单）
                if (pUserInfo.dwUserID != pMeUserInfo.dwUserID && cbNowStatus == SocketDefines.US_SIT && pMeUserInfo.wTableID == wNowTableID)
                {
                }
                m_TableFrame.SetClientUserItem(wNowTableID, wNowChairID, pIClientUserItem);
            }

            //桌子状态
            if ((m_TableFrame.GetChairCount() < SocketDefines.MAX_CHAIR) && (wNowTableID != SocketDefines.INVALID_TABLE) && (wNowTableID == wLastTableID) && (wNowChairID == wLastChairID))
            {
                //Debug.Log("CServerItem::OnUserItemUpdate_5");
                m_TableFrame.UpdateTableView(wNowTableID);
            }

            //离开通知
            if ((wLastTableID != SocketDefines.INVALID_TABLE) && ((wNowTableID != wLastTableID) || (wNowChairID != wLastChairID)))
            {
                //Debug.Log("CServerItem::OnUserItemUpdate_6");
                //游戏通知
                if ((pMeUserInfo.wTableID != SocketDefines.INVALID_TABLE) && (pUserInfo.wTableID == pMeUserInfo.wTableID))
                {
                    if (mIClientKernelSink != null)
                        mIClientKernelSink.OnEventUserStatus(pIClientUserItem
                        , pIClientUserItem.GetUserStatus() == SocketDefines.US_LOOKON);
                }
            }

            //加入处理
            if ((wNowTableID == SocketDefines.INVALID_TABLE) && ((wNowTableID != wLastTableID) || (wNowChairID != wLastChairID)))
            {
                // temp hack: fix 比赛模式一局结束后，因为服务端的standup行为导致客户端退出到大厅 
                //但会产生一个私人（其他）模式的bug：解散房间后，客户端没有退回到大厅
                ///return;
                if (GetServerAttribute().wServerType == SocketDefines.GAME_GENRE_MATCH)
                {
                    return;
                }

                if (m_pMeUserItem == pIClientUserItem)
                {
                    //设置变量
                    m_wReqTableID = SocketDefines.INVALID_TABLE;
                    m_wReqChairID = SocketDefines.INVALID_CHAIR;
                    OnGFGameClose(0);//mChen comment
                }
            }
            //加入处理
            if ((wNowTableID != SocketDefines.INVALID_TABLE) && ((wNowTableID != wLastTableID) || (wNowChairID != wLastChairID)))
            {
                //自己判断
                if (m_pMeUserItem == pIClientUserItem)
                {
                    //设置变量
                    m_wReqTableID = SocketDefines.INVALID_TABLE;
                    m_wReqChairID = SocketDefines.INVALID_CHAIR;
                    //启动进程
                    if (mIServerItemSink == null || !mIServerItemSink.StartGame())
                    {
                        OnGFGameClose((int)enGameExitCode.GameExitCode_CreateFailed);
                        return;
                    }
                }

                //游戏通知
                if ((m_pMeUserItem != pIClientUserItem) && (pMeUserInfo.wTableID == wNowTableID))
                {
                    //Debug.Log("CServerItem::OnUserItemUpdate_8");
                    if (mIClientKernelSink != null)
                        mIClientKernelSink.OnEventUserEnter(pIClientUserItem,
                        pIClientUserItem.GetUserStatus() == SocketDefines.US_LOOKON);
                }

                return;
            }

            //状态改变
            if ((wNowTableID != SocketDefines.INVALID_TABLE) && (wNowTableID == wLastTableID) && (pMeUserInfo.wTableID == wNowTableID))
            {
                //Debug.Log("CServerItem::OnUserItemUpdate_9\n");
                //游戏通知
                tagUserStatus UserStatus = new tagUserStatus();
                UserStatus.wTableID = wNowTableID;
                UserStatus.wChairID = wNowChairID;
                UserStatus.cbUserStatus = cbNowStatus;

                if (mIClientKernelSink != null)
                    mIClientKernelSink.OnEventUserStatus(pIClientUserItem,
                    pIClientUserItem.GetUserStatus() == SocketDefines.US_LOOKON);

                return;
            }
        }

        public void OnUserItemUpdate(IClientUserItem pIClientUserItem, ref tagUserAttrib UserAttrib)
        {
            //变量定义
            ushort wMeTableID = m_pMeUserItem.GetTableID();
            ushort wUserTableID = pIClientUserItem.GetTableID();

            if (mIServerItemSink != null)
                mIServerItemSink.OnGRUserUpdate(pIClientUserItem);

            //通知游戏
            if ((wMeTableID != SocketDefines.INVALID_TABLE) && (wMeTableID == wUserTableID))
            {
                //变量定义
                //tagUserAttrib UserAttrib;
                UserAttrib.cbCompanion = pIClientUserItem.GetUserCompanion();

                //发送通知


                if (mIClientKernelSink != null)
                    mIClientKernelSink.OnEventUserScore(pIClientUserItem,
                    pIClientUserItem.GetUserStatus() == SocketDefines.US_LOOKON);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // ISocketEngineSink
        //////////////////////////////////////////////////////////////////////////
        public void onEventTCPSocketLink()
        {
            SendLogonPacket();
        }

        public void onEventTCPSocketShut()
        {
            if (mIServerItemSink != null)
                mIServerItemSink.OnGFServerClose(("CServerItem::onEventTCPSocketShut()"));

            OnGFGameClose(0);
        }

        public void onEventTCPSocketError(Exception errorCode)
        {
            Debug.Log("ServerItem:onEventTCPSocketError: " + errorCode.ToString());

            if (mIServerItemSink != null)
                mIServerItemSink.onEventTCPSocketError(errorCode);
        }

        public bool onEventTCPSocketRead(int main, int sub, byte[] data, int dataSize)
        {

            //Debug.Log("CServerItem.onEventTCPSocketRead" + main + " sub " + sub);
            switch (main)
            {
                //登录消息
                case GameServerDefines.MDM_GR_LOGON:
                    return OnSocketMainLogon(sub, data, dataSize);
                //配置信息
                case GameServerDefines.MDM_GR_CONFIG:
                    return OnSocketMainConfig(sub, data, dataSize);
                //用户信息
                case GameServerDefines.MDM_GR_USER:
                    return OnSocketMainUser(sub, data, dataSize);
                //状态信息
                case GameServerDefines.MDM_GR_STATUS:
                    return OnSocketMainStatus(sub, data, dataSize);
                //系统消息
                case GameServerDefines.MDM_CM_SYSTEM:
                    return OnSocketMainSystem(sub, data, dataSize);
                //游戏消息
                case GameServerDefines.MDM_GF_GAME:
                //框架消息
                case GameServerDefines.MDM_GF_FRAME:
                    return OnSocketMainGameFrame(main, sub, data, dataSize);
                //比赛消息
                case GameServerDefines.MDM_GR_MATCH:
                    return OnSocketMainMatch(sub, data, dataSize);
                //私人场消息
                case GameServerDefines.MDM_GR_PRIVATE:
                    return OnSocketMainPrivate(sub, data, dataSize);
            }

            Debug.Assert(false, "CServerItem:onEventTCPSocketRead");

            return true;
        }

        public bool onEventTCPHeartTick()
        {
            if (mIServerItemSink != null)
            {
                mIServerItemSink.HeartTick();
            }
            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        //登录消息
        public bool OnSocketMainLogon(int sub, byte[] data, int dataSize)
        {
            switch (sub)
            {
                //登录成功
                case GameServerDefines.SUB_GR_LOGON_SUCCESS: return OnSocketSubLogonSuccess(data, dataSize);
                //登录失败
                case GameServerDefines.SUB_GR_LOGON_FAILURE: return OnSocketSubLogonFailure(data, dataSize);
                //登录完成
                case GameServerDefines.SUB_GR_LOGON_FINISH: return OnSocketSubLogonFinish(data, dataSize);
                //更新提示
                case GameServerDefines.SUB_GR_UPDATE_NOTIFY: return OnSocketSubUpdateNotify(data, dataSize);
            }

            return true;
        }
        //登录成功
        bool OnSocketSubLogonSuccess(byte[] data, int dataSize)
        {
            mIsQuickSitDown = false;

            //设置状态
            SetServiceStatus(enServiceStatus.ServiceStatus_RecvInfo);

            if (mIServerItemSink != null)
                mIServerItemSink.OnGRLogonSuccess(data, dataSize);

            return true;
        }
        //登录失败
        bool OnSocketSubLogonFailure(byte[] data, int dataSize)
        {
            CMD_GR_LogonError pGameServer = (CMD_GR_LogonError)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GR_LogonError));
            if (mIStringMessageSink != null)
            {
                mIStringMessageSink.InsertSystemString(pGameServer.szErrorDescribe);
            }

            //关闭连接
            IntermitConnect(true);
            return true;
        }
        //登录完成
        bool OnSocketSubLogonFinish(byte[] data, int dataSize)
        {
            //设置状态
            SetServiceStatus(enServiceStatus.ServiceStatus_ServiceIng);

            mUserAttribute.dwUserID = m_pMeUserItem.GetUserID();
            mUserAttribute.wChairID = SocketDefines.INVALID_CHAIR;
            mUserAttribute.wTableID = SocketDefines.INVALID_TABLE;


            if (mIServerItemSink != null)
                mIServerItemSink.OnGRLogonFinish();

            return true;
        }
        //更新提示
        bool OnSocketSubUpdateNotify(byte[] data, int dataSize)
        {
            IntermitConnect(true);

            if (mIServerItemSink != null)
            {
                mIServerItemSink.OnGRUpdateNotify(0, "当前是老版本，请更新到最新版本！");
            }

            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        //配置信息
        bool OnSocketMainConfig(int sub, byte[] data, int dataSize)
        {
            switch (sub)
            {
                //列表配置
                case GameServerDefines.SUB_GR_CONFIG_COLUMN: return OnSocketSubConfigColumn(data, dataSize);
                //房间配置
                case GameServerDefines.SUB_GR_CONFIG_SERVER: return OnSocketSubConfigServer(data, dataSize);
                //道具配置
                case GameServerDefines.SUB_GR_CONFIG_PROPERTY: return OnSocketSubConfigOrder(data, dataSize);
                //配置玩家权限	
                case GameServerDefines.SUB_GR_CONFIG_USER_RIGHT: return OnSocketSubConfigMmber(data, dataSize);
                //配置完成
                case GameServerDefines.SUB_GR_CONFIG_FINISH: return OnSocketSubConfigFinish(data, dataSize);
            }

            //错误断言
            Debug.Assert(false, "OnSocketMainConfig msg error");
            return true;
        }
        //列表配置
        bool OnSocketSubConfigColumn(byte[] data, int dataSize)
        {
            if (mIServerItemSink != null)
                mIServerItemSink.OnGRConfigColumn();

            return true;
        }
        //房间配置
        bool OnSocketSubConfigServer(byte[] data, int dataSize)
        {
            if (dataSize < Marshal.SizeOf(typeof(CMD_GR_ConfigServer))) return false;

            //消息处理
            CMD_GR_ConfigServer pConfigServer = (CMD_GR_ConfigServer)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GR_ConfigServer));

            mServerAttribute.wTableCount = pConfigServer.wTableCount;
            mServerAttribute.wChairCount = pConfigServer.wChairCount;

            if (!m_TableFrame.ConfigTableFrame(
                mServerAttribute.wTableCount,
                mServerAttribute.wChairCount,
                mServerAttribute.wServerID))
            {
                IntermitConnect(false);
                return false;
            }

            if (mIServerItemSink != null)
                mIServerItemSink.OnGRConfigServer();

            return true;
        }
        //道具配置
        bool OnSocketSubConfigOrder(byte[] data, int dataSize)
        {
            if (mIServerItemSink != null)
                mIServerItemSink.OnGRConfigProperty();

            return true;
        }
        //配置玩家权限	
        bool OnSocketSubConfigMmber(byte[] data, int dataSize)
        {
            if (mIServerItemSink != null)
                mIServerItemSink.OnGRConfigUserRight();

            return true;
        }
        //配置完成
        bool OnSocketSubConfigFinish(byte[] data, int dataSize)
        {
            if (mIServerItemSink != null)
                mIServerItemSink.OnGRConfigFinish();
            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        //用户信息
        bool OnSocketMainUser(int sub, byte[] data, int dataSize)
        {

            switch (sub)
            {
                //请求坐下失败
                case GameServerDefines.SUB_GR_SIT_FAILED:
                    return OnSocketSubRequestFailure(data, dataSize);
                //用户进入
                case GameServerDefines.SUB_GR_USER_ENTER:
                    return OnSocketSubUserEnter(data, dataSize);
                //用户积分
                case GameServerDefines.SUB_GR_USER_SCORE:
                    return OnSocketSubUserScore(data, dataSize);
                //用户状态
                case GameServerDefines.SUB_GR_USER_STATUS:
                    return OnSocketSubUserStatus(data, dataSize);
                //用户聊天
                case GameServerDefines.SUB_GR_USER_CHAT:
                    return OnSocketSubUserChat(data, dataSize);
                //用户表情
                case GameServerDefines.SUB_GR_USER_EXPRESSION:
                    return OnSocketSubExpression(data, dataSize);
                //用户私聊
                case GameServerDefines.SUB_GR_WISPER_CHAT:
                    return OnSocketSubWisperUserChat(data, dataSize);
                //私聊表情
                case GameServerDefines.SUB_GR_WISPER_EXPRESSION:
                    return OnSocketSubWisperExpression(data, dataSize);

                //道具成功
                case GameServerDefines.SUB_GR_PROPERTY_SUCCESS:
                    return OnSocketSubPropertySuccess(data, dataSize);
                //道具失败
                case GameServerDefines.SUB_GR_PROPERTY_FAILURE:
                    return OnSocketSubPropertyFailure(data, dataSize);
                //道具效应
                case GameServerDefines.SUB_GR_PROPERTY_EFFECT:
                    return OnSocketSubPropertyEffect(data, dataSize);
                //礼物消息
                case GameServerDefines.SUB_GR_PROPERTY_MESSAGE:
                    return OnSocketSubPropertyMessage(data, dataSize);
                //喇叭消息
                case GameServerDefines.SUB_GR_PROPERTY_TRUMPET:
                    return OnSocketSubPropertyTrumpet(data, dataSize);
                //喜报消息
                case GameServerDefines.SUB_GR_GLAD_MESSAGE:
                    return OnSocketSubGladMessage(data, dataSize);
            }

            return true;
        }

        //请求失败
        bool OnSocketSubRequestFailure(byte[] data, int dataSize)
        {
            //变量定义
            CMD_GR_RequestFailure pRequestFailure = new CMD_GR_RequestFailure();
            pRequestFailure.StreamValue(data, dataSize);
            //CMD_GR_RequestFailure pRequestFailure = (CMD_GR_RequestFailure)StructConverterByteArray.BytesToStruct(data,typeof(CMD_GR_RequestFailure));

            //效验参数
            if (dataSize <= (dataSize - (pRequestFailure.szDescribeString.Length))) return false;
            //if (dataSize <= (Marshal.SizeOf(typeof(CMD_GR_RequestFailure)) - (pRequestFailure.szDescribeString.Length))) return false;

            if (mIStringMessageSink != null)
            {
                mIStringMessageSink.InsertSystemString(pRequestFailure.szDescribeString);
            }
            return true;
        }

        public void FakeUserLeave(int recordIndex)
        {
            for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
            {
                tagUserInfo kUserInfo = new tagUserInfo();
                kUserInfo.Init();
                PlayBackStorage.GetInstance().InitUserInfo(recordIndex, ref kUserInfo, i);
                //刪除用户
                IClientUserItem pIClientUserItem = mUserManager.SearchUserByUserID(kUserInfo.dwUserID);
                //删除用户
                mUserManager.DeleteUserItem(pIClientUserItem);

                //获取对象
                CServerListData pServerListData = CServerListData.shared();

                //人数更新
                pServerListData.SetServerOnLineCount(mServerAttribute.wServerID, mUserManager.GetActiveUserCount());
            }
        }

        public void FakeUserEnter(int recordIndex)
        {
            tagCustomFaceInfo CustomFaceInfo = new tagCustomFaceInfo();
            for (int i = 0; i < HNMJ_Defines.GAME_PLAYER; i++)
            {
                tagUserInfo kUserInfo = new tagUserInfo();
                kUserInfo.Init();
                PlayBackStorage.GetInstance().InitUserInfo(recordIndex, ref kUserInfo, i);
                //激活用户
                IClientUserItem pIClientUserItem = mUserManager.SearchUserByUserID(kUserInfo.dwUserID);
                pIClientUserItem = mUserManager.ActiveUserItem(kUserInfo, CustomFaceInfo);
            }
        }

        //用户进入
        bool OnSocketSubUserEnter(byte[] data, int dataSize)
        {
            //int n = sizeof(tagUserInfoHead);
            //变量定义
            tagUserInfo kUserInfo = new tagUserInfo();
            kUserInfo.Init();
            tagCustomFaceInfo CustomFaceInfo = new tagCustomFaceInfo();

            //变量定义
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

            //变量定义

            CParameterGlobal pParameterGlobal = CParameterGlobal.shared();

            tagUserInfoHead pUserInfoHead =
                (tagUserInfoHead)StructConverterByteArray.BytesToStruct(data, typeof(tagUserInfoHead));

            kUserInfo.dwUserID = pUserInfoHead.dwUserID;
            kUserInfo.wTableID = pUserInfoHead.wTableID;
            kUserInfo.wChairID = pUserInfoHead.wChairID;
            kUserInfo.cbUserStatus = pUserInfoHead.cbUserStatus;
            kUserInfo.wFaceID = pUserInfoHead.wFaceID;
            kUserInfo.dwCustomID = pUserInfoHead.dwCustomID;
            kUserInfo.cbGender = pUserInfoHead.cbGender;
            kUserInfo.cbMemberOrder = pUserInfoHead.cbMemberOrder;
            kUserInfo.cbMasterOrder = pUserInfoHead.cbMasterOrder;
            kUserInfo.dwGameID = pUserInfoHead.dwGameID;
            kUserInfo.dwGroupID = pUserInfoHead.dwGroupID;
            kUserInfo.lLoveLiness = pUserInfoHead.lLoveLiness;
            kUserInfo.lScore = pUserInfoHead.lScore;
            kUserInfo.lGameGold = pUserInfoHead.lGrade;
            kUserInfo.lInsureScore = pUserInfoHead.lInsure;
            kUserInfo.lWinCount = pUserInfoHead.dwWinCount;
            kUserInfo.lLostCount = pUserInfoHead.dwLostCount;
            kUserInfo.lDrawCount = pUserInfoHead.dwDrawCount;
            kUserInfo.lFleeCount = pUserInfoHead.dwFleeCount;
            kUserInfo.lExperience = pUserInfoHead.dwExperience;

            //获取对象
            CServerListData pServerListData = CServerListData.shared();
            byte[] pDataBuffer = null;
            tagDataDescribe DataDescribe = new tagDataDescribe();
            var desSize = Marshal.SizeOf(typeof(tagUserInfoHead));
            byte[] PacketBuffer = new byte[dataSize - desSize];
            Buffer.BlockCopy(data, desSize, PacketBuffer, 0, dataSize - desSize);
            //CRecvPacketHelper RecvPacket(pUserInfoHead + 1, dataSize - sizeof(tagUserInfoHead));
            CRecvPacketHelper RecvPacket = new CRecvPacketHelper(PacketBuffer, (ushort)PacketBuffer.Length);

            //WQ add
            kUserInfo.dwClientAddr = pUserInfoHead.dwClientAddr;            //最后登录ip
            kUserInfo.dwPlayCount = pUserInfoHead.dwPlayCount;
            kUserInfo.RegisterDate = pUserInfoHead.RegisterDate;

            // for HideSeek WangHu
            kUserInfo.cbTeamType = pUserInfoHead.cbTeamType;
            kUserInfo.cbModelIndex = pUserInfoHead.cbModelIndex;
            ///kUserInfo.cbMapIndexRand = pUserInfoHead.cbMapIndexRand;
            ///pGlobalUserData.cbMapIndexRand = kUserInfo.cbMapIndexRand;

            if (m_pMeUserItem != null && m_pMeUserItem.GetUserID() == pUserInfoHead.dwUserID)
            {
                pGlobalUserData.dwPlayCount = pUserInfoHead.dwPlayCount;
            }

            //扩展信息
            while (true)
            {
                pDataBuffer = RecvPacket.GetData(ref DataDescribe);
                if (DataDescribe.wDataDescribe == CRecvPacketHelper.DTP_NULL) break;
                switch (DataDescribe.wDataDescribe)
                {
                    case GameServerDefines.DTP_GR_NICK_NAME: //用户昵称
                        {
                            //ASSERT(pDataBuffer != NULL);
                            //ASSERT(DataDescribe.wDataSize <= sizeof(kUserInfo.szNickName));
                            if (DataDescribe.wDataSize <= kUserInfo.szNickName.Length)
                            {
                                Buffer.BlockCopy(pDataBuffer, 0, kUserInfo.szNickName, 0, DataDescribe.wDataSize);
                                //memcpy(&kUserInfo.szNickName, pDataBuffer, DataDescribe.wDataSize);
                                kUserInfo.szNickName[kUserInfo.szNickName.Length - 1] = 0;
                            }
                            break;
                        }
                    case GameServerDefines.DTP_GR_GROUP_NAME: //用户社团
                        {
                            //ASSERT(pDataBuffer != NULL);
                            //ASSERT(DataDescribe.wDataSize <= sizeof(kUserInfo.szGroupName));
                            if (DataDescribe.wDataSize <= (kUserInfo.szGroupName.Length))
                            {
                                Buffer.BlockCopy(pDataBuffer, 0, kUserInfo.szGroupName, 0, DataDescribe.wDataSize);
                                //memcpy(&kUserInfo.szGroupName, pDataBuffer, DataDescribe.wDataSize);
                                kUserInfo.szGroupName[(kUserInfo.szGroupName.Length) - 1] = 0;
                            }
                            break;
                        }
                    case GameServerDefines.DTP_GR_UNDER_WRITE: //个性签名
                        {
                            //ASSERT(pDataBuffer != NULL);
                            //ASSERT(DataDescribe.wDataSize <= sizeof(kUserInfo.szUnderWrite));
                            if (DataDescribe.wDataSize <= (kUserInfo.szUnderWrite.Length))
                            {
                                Buffer.BlockCopy(pDataBuffer, 0, kUserInfo.szUnderWrite, 0, DataDescribe.wDataSize);
                                //memcpy(kUserInfo.szUnderWrite, pDataBuffer, DataDescribe.wDataSize);
                                kUserInfo.szUnderWrite[(kUserInfo.szUnderWrite.Length) - 1] = 0;
                            }
                            break;
                        }

                    // for headPic
                    case GameServerDefines.DTP_GR_HEAD_HTTP:  //微信头像
                        {
                            //ASSERT(pDataBuffer != NULL);
                            //ASSERT(DataDescribe.wDataSize <= sizeof(kUserInfo.szHeadHttp));
                            if (DataDescribe.wDataSize <= (kUserInfo.szHeadHttp.Length))
                            {
                                Buffer.BlockCopy(pDataBuffer, 0, kUserInfo.szHeadHttp, 0, DataDescribe.wDataSize);
                                ///memcpy(&kUserInfo.szHeadHttp, pDataBuffer, DataDescribe.wDataSize);

                                //string strTmp = Encoding.Default.GetString(kUserInfo.szHeadHttp);
                                //string strName = Encoding.Default.GetString(kUserInfo.szNickName);
                                //Debug.Log("mChen OnSocketSubUserEnter:szHeadHttp="+ strTmp + ", strName=" + kUserInfo.szNickName);

                                kUserInfo.szHeadHttp[(kUserInfo.szHeadHttp.Length) - 1] = 0;
                            }
                            break;
                        }
                }
            }
            //激活用户
            IClientUserItem pIClientUserItem = mUserManager.SearchUserByUserID(kUserInfo.dwUserID);
            pIClientUserItem = mUserManager.ActiveUserItem(kUserInfo, CustomFaceInfo);

            //人数更新
            if (pServerListData != null)
                pServerListData.SetServerOnLineCount(mServerAttribute.wServerID, mUserManager.GetActiveUserCount());

            return true;
        }

        //用户积分
        bool OnSocketSubUserScore(byte[] data, int dataSize)
        {
            //效验参数

            if (dataSize < Marshal.SizeOf(typeof(CMD_GR_UserScore))) return false;

            //寻找用户
            CMD_GR_UserScore pUserScore = (CMD_GR_UserScore)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GR_UserScore));
            IClientUserItem pIClientUserItem = mUserManager.SearchUserByUserID(pUserScore.dwUserID);

            //用户判断
            if (pIClientUserItem == null) return true;

            //变量定义
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

            mUserManager.UpdateUserItemScore(pIClientUserItem, pUserScore.UserScore);


            return true;
        }
        //用户状态
        bool OnSocketSubUserStatus(byte[] data, int dataSize)
        {
            if (dataSize < Marshal.SizeOf(typeof(CMD_GR_UserStatus))) return false;

            //处理数据
            CMD_GR_UserStatus pUserStatus = (CMD_GR_UserStatus)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GR_UserStatus));

            IClientUserItem pIClientUserItem = mUserManager.SearchUserByUserID(pUserStatus.dwUserID);
            if (pIClientUserItem == null) return true;

            tagUserStatus UserStatus = new tagUserStatus();
            UserStatus.wTableID = pUserStatus.UserStatus.wTableID;
            UserStatus.wChairID = pUserStatus.UserStatus.wChairID;
            UserStatus.cbUserStatus = pUserStatus.UserStatus.cbUserStatus;

            //设置状态
            if (UserStatus.cbUserStatus == SocketDefines.US_NULL)
            {
                //删除用户
                mUserManager.DeleteUserItem(pIClientUserItem);

                //获取对象
                CServerListData pServerListData = CServerListData.shared();

                //人数更新
                pServerListData.SetServerOnLineCount(mServerAttribute.wServerID, mUserManager.GetActiveUserCount());
            }
            else
            {
                //更新用户
                mUserManager.UpdateUserItemStatus(pIClientUserItem, UserStatus);
            }

            return true;
        }
        //用户聊天
        bool OnSocketSubUserChatIndex(byte[] data, int dataSize) //WQ add
        {
            if (mIClientKernelSink != null)
            {
                return mIClientKernelSink.OnSubUserChatIndex(data, dataSize);
            }

            return true;
        }

        bool OnSocketSubUserChat(byte[] data, int dataSize)
        {

            return true;

        }
        //用户表情
        bool OnSocketSubUserExpressionIndex(byte[] data, int dataSize) //WQ add
        {
            if (mIClientKernelSink != null)
            {
                return mIClientKernelSink.OnSubUserExpressionIndex(data, dataSize);
            }

            return true;
        }

        bool OnSocketSubExpression(byte[] data, int dataSize)
        {
            return true;
        }
        //用户私聊
        bool OnSocketSubWisperUserChat(byte[] data, int dataSize)
        {
            return true;
        }
        //私聊表情
        bool OnSocketSubWisperExpression(byte[] data, int dataSize)
        {
            return true;
        }
        //道具成功
        bool OnSocketSubPropertySuccess(byte[] data, int dataSize)
        {
            return true;
        }
        //道具失败
        bool OnSocketSubPropertyFailure(byte[] data, int dataSize)
        {
            return true;
        }
        //道具效应
        bool OnSocketSubPropertyEffect(byte[] data, int dataSize)
        {
            return true;
        }
        //道具消息
        bool OnSocketSubPropertyMessage(byte[] data, int dataSize)
        {
            return true;
        }
        //道具喇叭
        bool OnSocketSubPropertyTrumpet(byte[] data, int dataSize)
        {
            return true;
        }
        //喜报消息
        bool OnSocketSubGladMessage(byte[] data, int dataSize)
        {
            return true;
        }

        // for HideSeek
        bool OnSocketSubInventoryCreate(byte[] data, int dataSize)
        {
            //生成道具
            Loom.QueueOnMainThread(() =>
            {
                //if (InventoryManager.GetInstane != null)
                //    InventoryManager.GetInstane.InventoryInit();
                if (hnGameManager == null)
                {
                    hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
                }
                hnGameManager.OnSocketSubInventoryCreate();

            });

            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        //状态信息
        public bool OnSocketMainStatus(int sub, byte[] data, int dataSize)
        {
            //Debug.Log("CServerItem.OnSocketMainStatus");
            switch (sub)
            {
                //桌子信息
                case GameServerDefines.SUB_GR_TABLE_INFO: return OnSocketSubStatusTableInfo(data, dataSize);
                //桌子状态
                case GameServerDefines.SUB_GR_TABLE_STATUS: return OnSocketSubStatusTableStatus(data, dataSize);
            }

            return true;
        }
        //桌子信息
        bool OnSocketSubStatusTableInfo(byte[] data, int dataSize)
        {
            //变量定义
            return true;

        }
        //桌子状态
        bool OnSocketSubStatusTableStatus(byte[] data, int dataSize)
        {
            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        //银行消息
        bool OnSocketMainInsure(int sub, byte[] data, int dataSize)
        {
            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        //管理消息
        bool OnSocketMainManager(int sub, byte[] data, int dataSize)
        {
            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        //系统消息
        bool OnSocketMainSystem(int sub, byte[] data, int dataSize)
        {
            //Debug.Log("CServerItem.OnSocketMainSystem" + sub);
            switch (sub)
            {
                //系统消息
                case GameServerDefines.SUB_CM_SYSTEM_MESSAGE: return OnSocketSubSystemMessage(data, dataSize);
            }

            //错误断言
            Debug.Assert(false, "CServerItem.OnSocketMainSystem");

            return true;
        }
        //系统消息
        bool OnSocketSubSystemMessage(byte[] data, int wDataSize)
        {
            CMD_CM_SystemMessage pSystemMessage = new CMD_CM_SystemMessage();
            pSystemMessage.StreamValue(data, wDataSize);
            //CMD_CM_SystemMessage pSystemMessage = (CMD_CM_SystemMessage)StructConverterByteArray.BytesToStruct(data, typeof(CMD_CM_SystemMessage));

            //效验参数
            int wHeadSize = wDataSize - (pSystemMessage.szString.Length);//Marshal.SizeOf(typeof(CMD_CM_SystemMessage)) - (pSystemMessage.szString.Length);
            if ((wDataSize <= wHeadSize) || (wDataSize != (wHeadSize + pSystemMessage.wLength))) return false;

            ushort wType = pSystemMessage.wType;

            //关闭处理
            if ((wType & GameServerDefines.SMT_CLOSE_LINK) != 0)
            {
                if (mIStringMessageSink != null)
                {
                    mIStringMessageSink.InsertSystemString(pSystemMessage.szString);
                }

                // for HideSeek
                Debug.LogWarning("OnSocketSubSystemMessage: SMT_CLOSE_LINK: LeaveGameToHall");
                Loom.QueueOnMainThread(() =>
                {
                    if (hnGameManager == null)
                    {
                        hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
                    }
                    if (hnGameManager != null)
                    {
                        hnGameManager.m_cbGameEndReason = HNMJ_Defines.GER_USER_LEAVE;
                        hnGameManager.StartOrStopGameSceneHeartBeat(false);
                        //hnGameManager.LeaveRoom();
                        hnGameManager.LeaveGameToHall();
                    }
                });

                OnGFGameClose(0);
            }

            //显示消息
            if ((wType & GameServerDefines.SMT_CHAT) != 0)
            {
                if (mIStringMessageSink != null)
                {
                    mIStringMessageSink.InsertSystemString(pSystemMessage.szString);
                }
            }

            //关闭游戏
            if ((wType & GameServerDefines.SMT_CLOSE_GAME) != 0)
            {
                if (mIStringMessageSink != null)
                {
                    mIStringMessageSink.InsertSystemString(pSystemMessage.szString);
                }

                // for HideSeek: fix继续游戏报钻石不够创建房间:强制用户离开
                Debug.LogWarning("OnSocketSubSystemMessage: SMT_CLOSE_GAME: LeaveGameToHall");
                Loom.QueueOnMainThread(() =>
                {
                    if (hnGameManager == null)
                    {
                        hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
                    }
                    if (hnGameManager != null)
                    {
                        hnGameManager.m_cbGameEndReason = HNMJ_Defines.GER_USER_LEAVE;
                        hnGameManager.StartOrStopGameSceneHeartBeat(false);
                        //hnGameManager.LeaveRoom();
                        hnGameManager.LeaveGameToHall();
                    }
                });

                OnGFGameClose(0);

            }

            //弹出消息
            if ((wType & GameServerDefines.SMT_EJECT) != 0)
            {
                if (mIStringMessageSink != null)
                {
                    mIStringMessageSink.InsertPromptString(pSystemMessage.szString, 0);
                }
            }

            //关闭房间
            if ((wType & GameServerDefines.SMT_CLOSE_ROOM) != 0)
            {
                if (mIStringMessageSink != null)
                {
                    mIStringMessageSink.InsertSystemString(pSystemMessage.szString);
                }

                // for HideSeek: fix“您的帐号在另一地方进入了此游戏房间，您被迫离开”导致的一个人进两个房间:强制用户离开 
                Debug.LogWarning("OnSocketSubSystemMessage: SMT_CLOSE_ROOM: LeaveGameToHall");
                Loom.QueueOnMainThread(() =>
                {
                    if (hnGameManager == null)
                    {
                        hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
                    }
                    if (hnGameManager != null)
                    {
                        hnGameManager.m_cbGameEndReason = HNMJ_Defines.GER_USER_LEAVE;
                        hnGameManager.StartOrStopGameSceneHeartBeat(false);
                        hnGameManager.LeaveRoom();
                        hnGameManager.LeaveGameToHall();
                    }
                });

                OnGFGameClose(0);
            }

            return true;
        }
        //动作消息
        bool OnSocketSubActionMessage(byte[] data, int dataSize)
        {
            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        //设置内核接口
        public void SetClientKernelSink(IClientKernelSink pIClientKernelSink)
        {
            if (mIClientKernelSink != null)
            {
                mIClientKernelSink.clearInfo();
            }
            mIClientKernelSink = pIClientKernelSink;
        }

        public IClientKernelSink GetClientKernelSink()
        {
            return mIClientKernelSink;
        }

        public bool SendUserReady(byte[] data, ushort dataSize)
        {
            if (m_pMeUserItem == null) return false;
            return SendSocketData(GameServerDefines.MDM_GF_FRAME, GameServerDefines.SUB_GF_USER_READY, null, 0);
        }
        public bool SendCreaterUserPressedStart(byte[] data, ushort dataSize)
        {
            if (m_pMeUserItem == null) return false;
            return SendSocketData(GameServerDefines.MDM_GF_FRAME, GameServerDefines.SUB_GF_CREATER_PRESS_START, null, 0);
        }


        //游戏消息,框架消息
        bool OnSocketMainGameFrame(int main, int sub, byte[] data, int dataSize)
        {
            if (dataSize > Packet.SOCKET_TCP_PACKET) return false;
            //Debug.Log("OnSocketMainGameFrame1 " + main + " sub " + sub);
            //游戏消息
            if (main == GameServerDefines.MDM_GF_GAME)
            {
                //效验状态

                if (mIClientKernelSink == null)
                {
                    Debug.LogWarning("OnSocketMainGameFrame MDM_GF_GAME: mIClientKernelSink==null cause return false");
                    return false;
                }

                if (sub == HNMJ_Defines.SUB_S_HideSeek_AICreateInfo)
                {
                    Debug.Log("OnSocketMainGameFrame2 main= " + main + ", sub= " + sub);
                }
                ///Debug.Log("OnSocketMainGameFrame2 main= " + main + ", sub= " + sub);
                return mIClientKernelSink.OnEventGameMessage(sub, data, dataSize);
            }
            //Debug.Log("OnSocketMainGameFrame3 main= " + main + ", sub= " + sub);
            //内核处理
            if (main == GameServerDefines.MDM_GF_FRAME)
            {
                //Debug.Log("CServerItem.MDM_GF_FRAME" + sub);
                switch (sub)
                {
                    //WQ add
                    case GameServerDefines.SUB_GF_USER_CHAT_INDEX:            //用户聊天
                        {
                            return OnSocketSubUserChatIndex(data, dataSize);
                        }
                    case GameServerDefines.SUB_GF_USER_EXPRESSION_INDEX:          //用户聊天
                        {
                            return OnSocketSubUserExpressionIndex(data, dataSize);
                        }

                    // for HideSeek
                    case GameServerDefines.SUB_GF_INVENTORY_CREATE:
                        {
                            return OnSocketSubInventoryCreate(data, dataSize);
                        }

                    case GameServerDefines.SUB_GF_USER_CHAT:          //用户聊天
                        {
                            return OnSocketSubUserChat(data, dataSize);
                        }
                    case GameServerDefines.SUB_GR_TABLE_TALK:         //用户聊天
                        {
                            return OnSocketSubUserTalk(data, dataSize);
                        }
                    case GameServerDefines.SUB_GF_USER_EXPRESSION:    //用户表情
                        {
                            return OnSocketSubExpression(data, dataSize);
                        }
                    case GameServerDefines.SUB_GF_GAME_STATUS:        //游戏状态
                        {
                            //Debug.Log("OnSocketMainGameFrame3 " + main + " sub " + sub);
                            return OnSocketSubGameStatus(data, dataSize);
                        }
                    case GameServerDefines.SUB_GF_GAME_SCENE:         //游戏场景
                        {
                            //Debug.Log("OnSocketMainGameFrame4 " + main + " sub " + sub);
                            return OnSocketSubGameScene(data, dataSize);
                        }
                    case GameServerDefines.SUB_GF_LOOKON_STATUS:      //旁观状态
                        {
                            return OnSocketSubLookonStatus(data, dataSize);
                        }
                    case GameServerDefines.SUB_GF_SYSTEM_MESSAGE:     //系统消息
                        {
                            //Debug.Log("OnSocketMainGameFrame5 " + main + " sub " + sub);
                            return OnSocketSubSystemMessage(data, dataSize);
                        }
                    case GameServerDefines.SUB_GF_ACTION_MESSAGE:     //动作消息
                        {
                            return OnSocketSubActionMessage(data, dataSize);
                        }
                    case GameServerDefines.SUB_GF_USER_READY:         //用户准备
                        {
                            //Debug.Log("OnSocketMainGameFrame6 " + main + " sub " + sub);

                            ////to show player ready UI
                            //ushort wChairId = BitConverter.ToUInt16(data, 0);
                            //Loom.QueueOnMainThread(() =>
                            //{
                            //    if (mIClientKernelSink != null)
                            //        mIClientKernelSink.OnGFPlayerReady(wChairId);
                            //});
                            //return true;

                            if (m_pMeUserItem == null || m_pMeUserItem.GetUserStatus() >= SocketDefines.US_READY)
                                return true;
                            SendUserReady(null, 0);
                            if (mIClientKernelSink != null)
                                mIClientKernelSink.OnGFMatchWaitTips(new tagMatchWaitTip());
                            return true;
                        }
                    case GameServerDefines.SUB_GR_MATCH_INFO:             //比赛信息
                        {
                            if (mIClientKernelSink == null)
                                return true;

                            return true;
                        }
                    case GameServerDefines.SUB_GR_MATCH_WAIT_TIP:         //等待提示
                        {
                            //Debug.Log("OnSocketMainGameFrame7 " + main + " sub " + sub);
                            if (mIClientKernelSink == null)
                                return true;

                            //设置参数
                            if (dataSize == 0)
                            {
                                mIClientKernelSink.OnGFMatchWaitTips(new tagMatchWaitTip());
                            }

                            return true;
                        }
                    case GameServerDefines.SUB_GR_MATCH_RESULT:           //比赛结果
                        {
                            //设置参数
                            if (mIClientKernelSink == null)
                                return true;


                            return true;
                        }
                }

                return true;
            }

            return false;
        }
        //用户聊天
        bool OnSocketSubUserTalk(byte[] data, int dataSize)
        {
            if (mIClientKernelSink != null)
            {
                return mIClientKernelSink.RevTalkFile(data, dataSize);
            }
            return true;
        }
        //游戏状态
        bool OnSocketSubGameStatus(byte[] data, int dataSize)
        {
            if (dataSize != Marshal.SizeOf(typeof(CMD_GF_GameStatus))) return false;

            //消息处理
            CMD_GF_GameStatus pGameStatus = (CMD_GF_GameStatus)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GF_GameStatus));

            Loom.QueueOnMainThread(() =>
            {
                if (hnGameManager == null)
                {
                    hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
                }
                if (hnGameManager != null)
                {
                    SetGameStatus(pGameStatus.cbGameStatus);
                    hnGameManager.OnGameStatus(pGameStatus.cbGameStatus);
                    hnGameManager.HideOfflineWaitingUI();
                }
            });

            return true;
        }
        //游戏场景
        bool OnSocketSubGameScene(byte[] data, int dataSize)
        {
            Debug.Log("flow->CClientKernel::OnSocketSubGameScene1");
            if (m_pMeUserItem == null) return true;

            if (mIClientKernelSink == null)
                return true;
            //场景处理
            bool bLookonUser = (m_pMeUserItem.GetUserStatus() == SocketDefines.US_LOOKON);
            Debug.Log("flow.CClientKernel::OnSocketSubGameScene3");
            return mIClientKernelSink.OnEventSceneMessage(mGameStatus, bLookonUser, data, dataSize);
        }
        //旁观状态
        bool OnSocketSubLookonStatus(byte[] data, int dataSize)
        {
            return true;
        }
        //获取状态
        public byte GetGameStatus()
        {
            return mGameStatus;
        }
        //设置状态
        public void SetGameStatus(byte cbGameStatus)
        {
            mGameStatus = cbGameStatus;
        }
        //////////////////////////////////////////////////////////////////////////
        //比赛消息
        bool OnSocketMainMatch(int sub, byte[] data, int dataSize)
        {
            switch (sub)
            {
                //费用查询
                case GameServerDefines.SUB_GR_MATCH_FEE: return OnSocketSubMatchFee(data, dataSize);
                case GameServerDefines.SUB_GR_MATCH_NUM: return OnSocketSubMatchNum(data, dataSize);
                case GameServerDefines.SUB_GR_MATCH_INFO: return OnSocketSubMatchInfo(data, dataSize);
                case GameServerDefines.SUB_GR_MATCH_WAIT_TIP: return OnSocketSubWaitTip(data, dataSize);
                case GameServerDefines.SUB_GR_MATCH_RESULT: return OnSocketSubMatchResult(data, dataSize);
                case GameServerDefines.SUB_GR_MATCH_STATUS: return OnSocketSubMatchStatus(data, dataSize);
                case GameServerDefines.SUB_GR_MATCH_GOLDUPDATE: return OnSocketSubMatchGoldUpdate(data, dataSize);
                case GameServerDefines.SUB_GR_MATCH_ELIMINATE: return OnSocketSubMatchEliminate(data, dataSize);
                case GameServerDefines.SUB_GR_MATCH_JOIN_RESOULT: return OnSocketSubMatchJoinResoult(data, dataSize);
            }
            return true;
        }

        public void sendMatchSignupCheck() //mChen test
        {
            bool bInGamePhase = false;
            var buf = BitConverter.GetBytes(bInGamePhase);
            SendSocketData(GameServerDefines.MDM_GR_MATCH, GameServerDefines.SUB_GR_MATCH_SIGNUP_CHECK, buf, (ushort)buf.Length);
        }

        public void sendMacthFree()
        {
            var buf = BitConverter.GetBytes(m_kMacthCost.lMatchFee);
            SendSocketData(GameServerDefines.MDM_GR_MATCH, GameServerDefines.SUB_GR_MATCH_FEE, buf, (ushort)buf.Length);
        }

        void sendExitMacth()
        {
            SendSocketData(GameServerDefines.MDM_GR_MATCH, GameServerDefines.SUB_GR_LEAVE_MATCH);
        }
        //比赛费用
        bool OnSocketSubMatchFee(byte[] data, int dataSize)
        {
            CMD_GR_Match_Fee pNetInfo = (CMD_GR_Match_Fee)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GR_Match_Fee));

            m_kMacthCost = pNetInfo;

            if (pNetInfo.lMatchFee > 0)
            {
                if (mIServerMatchSink != null)
                {
                    mIServerMatchSink.OnSocketSubMatchFee(pNetInfo);
                }
                //WQ add
                else
                {
                    if (mIStringMessageSink != null)
                        mIStringMessageSink.InsertPromptString(pNetInfo.szNotifyContent, GameServerDefines.DLG_MB_OK);
                    sendMacthFree();
                }
            }

            return true;
        }
        //参赛人数
        bool OnSocketSubMatchNum(byte[] data, int dataSize)
        {
            if (dataSize != Marshal.SizeOf(typeof(CMD_GR_Match_Num))) return false;

            CMD_GR_Match_Num pNetInfo = (CMD_GR_Match_Num)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GR_Match_Num));

            if (mIServerMatchSink != null)
            {
                mIServerMatchSink.OnSocketSubMatchNum(pNetInfo);
            }

            return true;
        }

        bool OnSocketSubMatchInfo(byte[] data, int dataSize)
        {
            if (dataSize != Marshal.SizeOf(typeof(CMD_GR_Match_Info))) return false;

            CMD_GR_Match_Info pNetInfo = (CMD_GR_Match_Info)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GR_Match_Info));

            if (mIServerMatchSink != null)
            {
                mIServerMatchSink.OnSocketSubMatchInfo(pNetInfo);
            }

            return true;
        }

        bool OnSocketSubWaitTip(byte[] data, int dataSize)
        {
            if (dataSize != Marshal.SizeOf(typeof(CMD_GR_Match_Wait_Tip)) && dataSize != 0) return false;

            if (mIServerMatchSink != null)
            {
                if (dataSize != 0)
                {
                    CMD_GR_Match_Wait_Tip pNetInfo = (CMD_GR_Match_Wait_Tip)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GR_Match_Wait_Tip));
                    mIServerMatchSink.OnSocketSubMatchWaitTip(true, pNetInfo);
                }
                else
                {
                    mIServerMatchSink.OnSocketSubMatchWaitTip(true, new CMD_GR_Match_Wait_Tip());
                }
            }

            return true;
        }

        bool OnSocketSubMatchResult(byte[] data, int dataSize)
        {
            var typeValue = typeof(CMD_GR_MatchResult);
            if (dataSize != Marshal.SizeOf(typeValue)) return false;

            CMD_GR_MatchResult pNetInfo = (CMD_GR_MatchResult)StructConverterByteArray.BytesToStruct(data, typeValue);

            if (mIServerMatchSink != null)
            {
                mIServerMatchSink.OnSocketSubMatchResult(pNetInfo);
            }

            return true;
        }

        bool OnSocketSubMatchStatus(byte[] data, int dataSize)
        {
            if (dataSize != sizeof(byte)) return false;

            if (mIServerMatchSink != null)
            {
                mIServerMatchSink.OnSocketSubMatchStatus(data[0]);
            }

            return true;
        }

        bool OnSocketSubMatchGoldUpdate(byte[] data, int dataSize)
        {
            var typeValue = typeof(CMD_GR_MatchGoldUpdate);
            if (dataSize != Marshal.SizeOf(typeValue)) return false;

            CMD_GR_MatchGoldUpdate pNetInfo = (CMD_GR_MatchGoldUpdate)StructConverterByteArray.BytesToStruct(data, typeValue);

            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

            pGlobalUserData.lUserScore = pNetInfo.lCurrGold;

            pGlobalUserInfo.upPlayerInfo();

            if (mIServerMatchSink != null)
            {
                mIServerMatchSink.OnSocketSubMatchGoldUpdate(pNetInfo);
            }

            return true;
        }

        bool OnSocketSubMatchEliminate(byte[] data, int dataSize)
        {
            if (mIServerMatchSink != null)
            {
                mIServerMatchSink.OnSocketSubMatchEliminate();
            }

            return true;
        }

        bool OnSocketSubMatchJoinResoult(byte[] data, int dataSize)
        {
            var typeValue = typeof(CMD_GR_Match_JoinResoult);
            if (dataSize != Marshal.SizeOf(typeValue)) return false;

            CMD_GR_Match_JoinResoult pNetInfo = (CMD_GR_Match_JoinResoult)StructConverterByteArray.BytesToStruct(data, typeValue);

            if (mIServerMatchSink != null)
            {
                mIServerMatchSink.OnSocketSubMatchJoinResolt(pNetInfo.wSucess == 1);
            }
            return true;
        }


        //////////////////////////////////////////////////////////////////////////
        //私人场消息
        bool OnSocketMainPrivate(int sub, byte[] data, int dataSize)
        {
            //Debug.Log("CServerItem.OnSocketMainPrivate" + sub);
            switch (sub)
            {
                //费用查询
                case GameServerDefines.SUB_GR_PRIVATE_INFO: return OnSocketSubPrivateInfo(data, dataSize);
                case GameServerDefines.SUB_GR_CREATE_PRIVATE_SUCESS: return OnSocketSubPrivateCreateSuceess(data, dataSize);
                case GameServerDefines.SUB_GF_PRIVATE_ROOM_INFO: return OnSocketSubPrivateRoomInfo(data, dataSize);
                case GameServerDefines.SUB_GF_PRIVATE_END: return OnSocketSubPrivateEnd(data, dataSize);
                case GameServerDefines.SUB_GR_PRIVATE_DISMISS: return OnSocketSubPrivateDismissInfo(data, dataSize);
            }
            return true;
        }

        bool OnSocketSubPrivateInfo(byte[] data, int dataSize)
        {
            CMD_GR_Private_Info pNetInfo = (CMD_GR_Private_Info)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GR_Private_Info));

            if (mIServerPrivateSink != null)
            {
                mIServerPrivateSink.OnSocketSubPrivateInfo(pNetInfo);
            }

            return true;
        }

        bool OnSocketSubPrivateCreateSuceess(byte[] data, int dataSize)
        {
            var typeValue = typeof(CMD_GR_Create_Private_Sucess);
            if (dataSize != Marshal.SizeOf(typeValue)) return false;

            CMD_GR_Create_Private_Sucess pNetInfo = (CMD_GR_Create_Private_Sucess)StructConverterByteArray.BytesToStruct(data, typeValue);

            if (mIServerPrivateSink != null)
            {
                mIServerPrivateSink.OnSocketSubPrivateCreateSuceess(pNetInfo);
            }

            return true;
        }

        bool OnSocketSubPrivateRoomInfo(byte[] data, int dataSize)
        {
            //datastream kDataStream(data, dataSize);
            CMD_GF_Private_Room_Info kNetInfo = new CMD_GF_Private_Room_Info();
            kNetInfo.StreamValue(data, dataSize);

            if (mIServerPrivateSink != null)
            {
                mIServerPrivateSink.OnSocketSubPrivateRoomInfo(kNetInfo);
            }
            if (mIClientKernelSink != null)
            {
                mIClientKernelSink.OnSocketSubPrivateRoomInfo(kNetInfo);
            }
            return true;
        }

        bool OnSocketSubPrivateEnd(byte[] data, int dataSize)
        {
            var typeValue = typeof(CMD_GF_Private_End_Info);
            if (dataSize != Marshal.SizeOf(typeValue))
            {
                Debug.LogError("OnSocketSubPrivateEnd: incorrect dataSize");
                return false;
            }

            CMD_GF_Private_End_Info kNetInfo = (CMD_GF_Private_End_Info)StructConverterByteArray.BytesToStruct(data, typeValue);
            //CMD_GF_Private_End_Info kNetInfo = new CMD_GF_Private_End_Info();
            //kNetInfo.StreamValue(data, false,dataSize);

            // for HideSeek
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            pGlobalUserData.cbMapIndexRand = kNetInfo.cbMapIndex;
            pGlobalUserData.wRandseed = kNetInfo.wRandseed;
            pGlobalUserData.wRandseedForRandomGameObject = kNetInfo.wRandseedForRandomGameObject;
            pGlobalUserData.wRandseedForInventory = kNetInfo.wRandseedForInventory;
            //pGlobalUserData.lEncodedInventoryList = kNetInfo.lEncodedInventoryList;
            Array.Copy(kNetInfo.sInventoryList, pGlobalUserData.sInventoryList, kNetInfo.sInventoryList.Length);

            GameManager.m_bHasCreatedAIs = false;

            Loom.QueueOnMainThread(() =>
            {
                if (hnGameManager == null)
                {
                    hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
                }
                if (hnGameManager != null)
                {
                    hnGameManager.m_cbGameEndReason = kNetInfo.cbEndReason;
                    hnGameManager.StartOrStopGameSceneHeartBeat(false);

                    ////判断输赢
                    if (UIManager.GetInstance() != null)
                    {
                        UIManager.GetInstance().ShowWinOrLose();
                    }
                    //Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
                    //PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(PlayerTeam.PlayerTeamType.HideTeam);
                    //if (localHuman != null && team != null)
                    //{
                    //    bool HideWin = false;
                    //    for (int i = 0; i < team.GetPlayerNum(); i++)
                    //    {
                    //        PlayerBase playerBase = GameObjectsManager.GetInstance().GetPlayer(PlayerTeam.PlayerTeamType.HideTeam, i);
                    //        if (playerBase.Hp != 0)
                    //        {
                    //            HideWin = true;
                    //            break;
                    //        }
                    //    }
                    //    if (!HideWin)
                    //    {
                    //        if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                    //        {
                    //            if (UIManager.GetInstance() != null)
                    //                UIManager.GetInstance().ShowJSPopup(0);
                    //        }
                    //        else
                    //        {
                    //            if (UIManager.GetInstance() != null)
                    //                UIManager.GetInstance().ShowJSPopup(1);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                    //        {
                    //            if (UIManager.GetInstance() != null)
                    //                UIManager.GetInstance().ShowJSPopup(1);
                    //        }
                    //        else
                    //        {
                    //            if (UIManager.GetInstance() != null)
                    //                UIManager.GetInstance().ShowJSPopup(0);
                    //        }
                    //    }
                    //}

                    GameObjectsManager.GetInstance().ClearPlayers();
                    if (InventoryManager.GetInstane != null)
                        InventoryManager.GetInstane.DestoryInventoryObjects();  //删除参与未使用道具
                    if (mIClientKernelSink != null)
                    {
                        mIClientKernelSink.clearInfo();
                    }
                    Loom.QueueOnMainThread(() =>
                    {
                        if (kNetInfo.cbEndReason == HNMJ_Defines.GER_NORMAL)
                        {
                            hnGameManager.PlayAgain();
                        }
                    }, 3.0f);//delay to call PlayAgain,保证所有客户端都结束后才调用PlayAgain，防止误RemovePlayers
                }
                else
                {
                    Debug.LogError("OnSocketSubPrivateEnd:mIClientKernelSink==null, 导致没同步cbMapIndexRand和lEncodedInventoryList");
                }

                if (mIServerPrivateSink != null)
                {
                    mIServerPrivateSink.OnSocketSubPrivateEnd(kNetInfo);
                }
                if (mIClientKernelSink != null)
                {
                    mIClientKernelSink.OnSocketSubPrivateEnd(kNetInfo);
                }
                else
                {
                    Debug.LogError("OnSocketSubPrivateEnd:mIClientKernelSink==null, 导致没调用showFinalJieSuanInfo");
                }
            });


            return true;
        }

        bool OnSocketSubPrivateDismissInfo(byte[] data, int dataSize)
        {
            var typeValue = typeof(CMD_GF_Private_Dismiss_Info);
            if (dataSize != Marshal.SizeOf(typeValue)) return false;

            CMD_GF_Private_Dismiss_Info pNetInfo = (CMD_GF_Private_Dismiss_Info)StructConverterByteArray.BytesToStruct(data, typeValue);

            if (mIServerPrivateSink != null)
            {
                mIServerPrivateSink.OnSocketSubPrivateDismissInfo(pNetInfo);
            }
            if (mIClientKernelSink != null)
            {
                mIClientKernelSink.OnSocketSubPrivateDismissInfo(pNetInfo);
            }
            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        // 框架消息
        //游戏已准备好
        public void OnGFGameReady()
        {
            //变量定义
            if (m_pMeUserItem != null)
            {
                ushort wTableID = m_pMeUserItem.GetTableID();
                ushort wChairID = m_pMeUserItem.GetChairID();
                mUserAttribute.wChairID = wChairID;
                mUserAttribute.wTableID = wTableID;
            }

            //配置完成
            if (mIClientKernelSink != null)
            {
                mIClientKernelSink.SetupGameClient();
            }
            CMD_GF_GameOption GameOption = new CMD_GF_GameOption();

            //构造数据
            GameOption.dwFrameVersion = GameServerDefines.VERSION_FRAME;
            //GameOption.cbAllowLookon = gGlobalUnits.m_bAllowLookon;
            GameOption.cbAllowLookon = 0;
            GameOption.dwClientVersion = DF.shared().GetGameVersion();

            //发送数据
            var buf = StructConverterByteArray.StructToBytes(GameOption);
            SendSocketData(GameServerDefines.MDM_GF_FRAME, GameServerDefines.SUB_GF_GAME_OPTION, buf, (ushort)buf.Length);

            mIsGameReady = true;
        }
        //游戏关闭
        public void OnGFGameClose(int iExitCode)
        {
            //效验状态

            Debug.LogWarning("OnGFGameClose iExitCode = " + iExitCode);

            mIsGameReady = false;

            //变量定义
            //if (m_pMeUserItem != null)
            //{
            //    ushort wTableID = m_pMeUserItem.GetTableID();
            //    ushort wChairID = m_pMeUserItem.GetChairID();
            //    byte cbUserStatus = m_pMeUserItem.GetUserStatus();
            //}
            mUserAttribute.wChairID = SocketDefines.INVALID_CHAIR;
            mUserAttribute.wTableID = SocketDefines.INVALID_TABLE;

            if (iExitCode == (int)enServiceStatus.ServiceStatus_NetworkDown)
            {
                mServiceStatus = enServiceStatus.ServiceStatus_NetworkDown;
            }

            m_TableFrame.SetTableStatus(false);

            if (mIClientKernelSink != null)
            {
                mIClientKernelSink.CloseGameClient();
                mIClientKernelSink = null;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Socket消息
        // 发送数据
        public bool GFSendData(int main, int sub, byte[] data, int size)
        {
            return SendSocketData((ushort)main, (ushort)sub, data, (ushort)size);
        }



        //////////////////////////////////////////////////////////////////////////
        // 数据
        //////////////////////////////////////////////////////////////////////////

        //辅助变量
        ushort m_wReqTableID;                     //请求桌子
        ushort m_wReqChairID;                     //请求位置
        ushort mFindTableID;                      //查找桌子
        bool mIsGameReady;                      //游戏是否准备好
        bool mIsQuickSitDown;                       //是否快速加入操作

        CMD_GR_Match_Fee m_kMacthCost;
        //用户
        IClientUserItem m_pMeUserItem;
        CPlazaUserManager mUserManager;

        //房间属性
        enServiceStatus mServiceStatus;
        tagGameKind mGameKind;                      //类型信息
        tagGameServer mGameServer;                  //房间信息
        tagUserAttribute mUserAttribute;                    //用户属性
        tagServerAttribute mServerAttribute;                //房间属性

        //配置参数
        CParameterGame mParameterGame;                 //游戏配置
        CParameterServer mParameterServer;             //房间配置

        //桌子
        CTableViewFrame m_TableFrame;                   //桌子框架

        //接口
        IServerItemSink mIServerItemSink;              //房间接口
        IServerMatchSink mIServerMatchSink;                //比赛接口
        IServerPrivateSink mIServerPrivateSink;            //私人场接口
        IStringMessageSink mIStringMessageSink;            //信息接口

        IClientKernelSink mIClientKernelSink;              //内核接口
        byte mGameStatus;                                   //游戏状态
                                                            // 网络连接
        ISocketEngine mSocketEngine;

        HNGameManager hnGameManager;

    };
}
