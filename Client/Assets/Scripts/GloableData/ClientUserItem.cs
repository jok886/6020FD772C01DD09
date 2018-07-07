using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

//用户数组
//using ClientUserItemVector = System.Collections.Generic.List<CClientUserItem>;
//typedef std::vector<CClientUserItem *> ClientUserItemVector;
namespace GameNet
{

    //服务状态
    public enum enServiceStatus
    {
        ServiceStatus_Unknow,           //未知状态
        ServiceStatus_Entering,         //进入状态
        ServiceStatus_Validate,         //验证状态
        ServiceStatus_RecvInfo,         //读取状态
        ServiceStatus_ServiceIng,       //服务状态
        ServiceStatus_NetworkDown,      //中断状态
    };

    //游戏退出代码
    public enum enGameExitCode
    {
        GameExitCode_Normal,            //正常退出
        GameExitCode_CreateFailed,      //创建失败
        GameExitCode_Timeout,           //定时到时
        GameExitCode_Shutdown,          //断开连接
    };

    //房间退出代码
    public enum enServerExitCode
    {
        ServerExitCode_Normal,          //正常退出
        ServerExitCode_Shutdown,        //断开连接
    };

    public interface IServerItemSink
    {
      
        //登陆信息
	//请求失败
	void onGRRequestFailure(string sDescribeString);
	//登陆成功
void OnGRLogonSuccess(byte[] data, int dataSize) ;
	//登陆失败
	void OnGRLogonFailure(long lErrorCode, string sDescribeString);
	//登陆完成
	void OnGRLogonFinish();
	//更新通知
	void OnGRUpdateNotify(byte cbMustUpdate, string sDescribeString);

	//配置信息
	//列表配置
	void OnGRConfigColumn();
	//房间配置
	void OnGRConfigServer();
	//道具配置
	void OnGRConfigProperty();
	//玩家权限配置
	void OnGRConfigUserRight() ;
	//配置完成
	void OnGRConfigFinish() ;

	//用户信息
	//用户进入
	void OnGRUserEnter(IClientUserItem pIClientUserItem);
	//用户更新
	void OnGRUserUpdate(IClientUserItem pIClientUserItem);
	//用户删除
	void OnGRUserDelete(IClientUserItem pIClientUserItem);

	//框架消息
	//用户邀请
	void OnGFUserInvite(string szMessage);
	//用户邀请失败
	void OnGFUserInviteFailure(string szMessage);
	//房间退出
	void OnGFServerClose(string szMessage);
	//创建游戏内核
	bool CreateKernel();
	//启动游戏
	bool StartGame(bool bIsReconnect = false);
	//心跳包
	void HeartTick();
	//网络错误
	void onEventTCPSocketError(Exception errorCode) ;
};

    //用户接口
    public interface IClientUserItem
    {


        //属性接口

        //重要等级
        long GetImportOrder();
        //用户信息
        tagUserInfo GetUserInfo();
        void SetUserInfo(tagUserInfo uInfo);
        //自定头像
        tagCustomFaceInfo GetCustomFaceInfo();
        void SetCustomFaceInfo(tagCustomFaceInfo fInfo);
        //道具包裹
        tagPropertyPackage GetPropertyPackage();

        //头像信息

        //头像索引
        ushort GetFaceID();
        //自定索引
        uint GetCustomID();

        //属性信息
        //用户性别
        byte GetGender();
        //用户标识
        uint GetUserID();
        //游戏标识
        uint GetGameID();
        //社团标识
        uint GetGroupID();
        //用户昵称
        byte[] GetNickName();
        //社团名字
        byte[] GetGroupName();
        //个性签名
        byte[] GetUnderWrite();

        //经验魅力
        //用户奖牌
        // uint GetUserMedal();
        //经验数值
        uint GetUserExperience();
        //魅力数值
        long GetUserLoveLiness();

        //等级信息
        //会员等级
        byte GetMemberOrder();
        //管理等级
        byte GetMasterOrder();

        //用户状态
        //用户桌子
        ushort GetTableID();
        //用户椅子
        ushort GetChairID();
        //用户状态
        byte GetUserStatus();
        //用户数
        // uint GetActiveUserCount() ;

        // for HideSeek WangHu
        PlayerTeam.PlayerTeamType GetTeamType();
        byte GetModelIndex();

        //游戏信息
        //积分数值
        long GetUserScore();
        //成绩数值
        long GetUserGrade();
        //银行数值
        long GetUserInsure();

        //游戏信息
        //胜利盘数
        uint GetUserWinCount();
        //失败盘数
        uint GetUserLostCount();
        //和局盘数
        uint GetUserDrawCount();
        //逃跑盘数
        uint GetUserFleeCount();
        //游戏局数
        uint GetUserPlayCount();

        //比率信息
        //用户胜率
        float GetUserWinRate();
        //用户输率
        float GetUserLostRate();
        //用户和率
        float GetUserDrawRate();
        //用户逃率
        float GetUserFleeRate();

        //用户关系
        //获取关系
        byte GetUserCompanion();
        //设置关系
        void SetUserCompanion(byte cbCompanion);

        //用户备注
        //获取备注
        byte[] GetUserNoteInfo();
        //设置备注
        void SetUserNoteInfo(byte[] pszUserNote);
    };

    //信息接口
    public interface IStringMessageSink
    {



        //事件消息

        //进入事件
        bool InsertUserEnter(byte[] pszUserName);
        //离开事件
        bool InsertUserLeave(byte[] pszUserName);
        //断线事件
        bool InsertUserOffLine(byte[] pszUserName);

        //字符消息

        //普通消息(窗口输出)
        bool InsertNormalString(byte[] pszString);
        //系统消息(窗口输出)
        bool InsertSystemString(byte[] pszString);
        //系统消息(窗口输出)
        bool InsertSystemStringScript(byte[] pszString);
        //提示消息(对话框方式??)0:确认 1:确认,取消
        int InsertPromptString(byte[] pszString, int iButtonType);

        //定制消息

        //喜报消息
        bool InsertGladString(byte[] pszContent, byte[] pszNickName, byte[] pszNum, uint colText, uint colName,
            uint colNum);

//	//定制消息
//	 bool InsertCustomString(byte[] pszString, uint crColor);
//	//定制消息
//	 bool InsertCustomString(byte[] pszString, uint crColor, uint crBackColor);
    };

    //用户信息
    public class CClientUserItem : IClientUserItem
    {

        //属性变量
        public tagUserInfo m_UserInfo; //用户信息
        public tagCustomFaceInfo m_CustomFaceInfo; //自定头像
        public tagPropertyPackage m_PropertyPackage; //道具包裹

        //扩展属性
        public byte m_cbCompanion; //用户关系
        public byte[] m_szUserNote = new byte[SocketDefines.LEN_USERNOTE]; //用户备注

        //函数定义

        //构造函数
        public CClientUserItem()
        {
            //设置变量
            m_cbCompanion = SocketDefines.CP_NORMAL;

            m_UserInfo.wTableID = SocketDefines.INVALID_TABLE;
            m_UserInfo.wChairID = SocketDefines.INVALID_CHAIR;
        }

        //属性接口
        //重要等级
        public long GetImportOrder()
        {
            //构造等级
            long nOrder = 0;
            if (m_cbCompanion == SocketDefines.CP_FRIEND) nOrder += 1000;
            if (m_UserInfo.cbMemberOrder != 0) nOrder += m_UserInfo.cbMemberOrder*100L;
            if (m_UserInfo.cbMasterOrder != 0) nOrder += m_UserInfo.cbMasterOrder*10000L;

            return nOrder;
        }

        //用户信息
        public void SetUserInfo(tagUserInfo uInfo)
        {
            m_UserInfo = uInfo;
        }
        public tagUserInfo GetUserInfo()
        {
            return m_UserInfo;
        }

        //自定头像
        public void SetCustomFaceInfo(tagCustomFaceInfo fInfo)
        {
            m_CustomFaceInfo = fInfo;
        }
        public tagCustomFaceInfo GetCustomFaceInfo()
        {
            return m_CustomFaceInfo;
        }

        //道具包裹
        public tagPropertyPackage GetPropertyPackage()
        {
            return m_PropertyPackage;
        }

        //头像信息
        //头像索引
        public ushort GetFaceID()
        {
            return m_UserInfo.wFaceID;
        }

        //自定索引
        public uint GetCustomID()
        {
            return m_UserInfo.dwCustomID;
        }

        //属性信息
        //用户性别
        public byte GetGender()
        {
            return m_UserInfo.cbGender;
        }

        //用户标识
        public uint GetUserID()
        {
            return m_UserInfo.dwUserID;
        }

        //游戏标识
        public uint GetGameID()
        {
            return m_UserInfo.dwGameID;
        }

        //社团标识
        public uint GetGroupID()
        {
            return m_UserInfo.dwGroupID;
        }

        //用户昵称
        public byte[] GetNickName()
        {
            return m_UserInfo.szNickName;
        }

//社团名字
        public byte[] GetGroupName()
        {
            return m_UserInfo.szGroupName;
        }

        //个性签名
        public byte[] GetUnderWrite()
        {
            return m_UserInfo.szUnderWrite;
        }

        //经验魅力
        //用户奖牌
//	 uint GetUserMedal() { return m_UserInfo.dwUserMedal; }
        //经验数值
        public uint GetUserExperience()
        {
            return m_UserInfo.lExperience;
        }

        //魅力数值
        public long GetUserLoveLiness()
        {
            return m_UserInfo.lLoveLiness;
        }

//等级信息
        //会员等级
        public byte GetMemberOrder()
        {
            return m_UserInfo.cbMemberOrder;
        }

        //管理等级
        public byte GetMasterOrder()
        {
            return m_UserInfo.cbMasterOrder;
        }

        //用户状态
        //用户桌子
        public ushort GetTableID()
        {
            return m_UserInfo.wTableID;
        }

        //用户椅子
        public ushort GetChairID()
        {
            return m_UserInfo.wChairID;
        }

        //用户状态
        public byte GetUserStatus()
        {
            return m_UserInfo.cbUserStatus;
        }

        // for HideSeek WangHu
        public PlayerTeam.PlayerTeamType GetTeamType()
        {
            PlayerTeam.PlayerTeamType teamType = (PlayerTeam.PlayerTeamType)m_UserInfo.cbTeamType;
            return teamType;
        }
        public byte GetModelIndex()
        {
            byte cbModelIndex = m_UserInfo.cbModelIndex;
            return cbModelIndex;
        }
        

        //积分信息
        //积分数值
        public long GetUserScore()
        {
            return m_UserInfo.lScore;
        }

        //成绩数值
        public long GetUserGrade()
        {
            return m_UserInfo.lGrade;
        }

        //银行数值
        public long GetUserInsure()
        {
            return m_UserInfo.lInsureScore;
        }

        //游戏信息
        //胜利盘数
        public uint GetUserWinCount()
        {
            return m_UserInfo.lWinCount;
        }

        //失败盘数
        public uint GetUserLostCount()
        {
            return m_UserInfo.lLostCount;
        }

        //和局盘数
        public uint GetUserDrawCount()
        {
            return m_UserInfo.lDrawCount;
        }

        //逃跑盘数
        public uint GetUserFleeCount()
        {
            return m_UserInfo.lFleeCount;
        }

        //游戏局数
        public uint GetUserPlayCount()
        {
            return m_UserInfo.lWinCount + m_UserInfo.lLostCount + m_UserInfo.lDrawCount + m_UserInfo.lFleeCount;
        }

        //比率信息
        //用户胜率
        public float GetUserWinRate()
        {
            long lPlayCount = GetUserPlayCount();
            if (lPlayCount != 0L) return (float) (m_UserInfo.lWinCount*100.0f/(float) lPlayCount);

            return 0.0f;
        }

        //用户输率
        public float GetUserLostRate()
        {
            long lPlayCount = GetUserPlayCount();
            if (lPlayCount != 0L) return (float) (m_UserInfo.lLostCount*100.0f/(float) lPlayCount);

            return 0.0f;
        }

        //用户和率
        public float GetUserDrawRate()
        {
            long lPlayCount = GetUserPlayCount();
            if (lPlayCount != 0L) return (float) (m_UserInfo.lDrawCount*100.0f/(float) lPlayCount);

            return 0.0f;
        }

        //用户逃率
        public float GetUserFleeRate()
        {
            long lPlayCount = GetUserPlayCount();
            if (lPlayCount != 0L) return (float) (m_UserInfo.lFleeCount*100.0f/(float) lPlayCount);

            return 0.0f;

        }

        //用户关系
        //获取关系
        public byte GetUserCompanion()
        {
            return m_cbCompanion;
        }

        //设置关系
        public void SetUserCompanion(byte cbCompanion)
        {
            m_cbCompanion = cbCompanion;
        }

        //用户备注
        //设置备注
        public void SetUserNoteInfo(byte[] pszUserNote)
        {
            //效验参数
            if (pszUserNote == null) return;

            //设置备注
            Buffer.BlockCopy(pszUserNote, 0, m_szUserNote, 0, pszUserNote.Length);
            //strncpy(m_szUserNote, pszUserNote, countarray(m_szUserNote));
        }

        //获取备注
        public byte[] GetUserNoteInfo()
        {
            return m_szUserNote;
        }
    };

//////////////////////////////////////////////////////////////////////////////////

//用户管理
    public class CPlazaUserManager
    {
        //变量定义

        protected List<CClientUserItem> m_UserItemActive; //活动数组
        protected List<CClientUserItem> m_PoolItem; //删除数组

        //组件接口

        protected IUserManagerSink m_pIUserManagerSink; //通知接口
        //IUserInformation *				m_pIUserInformation;		//用户信息

        //函数定义
        //构造函数
        public CPlazaUserManager()
        {
            //组件接口
            m_pIUserManagerSink = null;
            m_UserItemActive = new List<CClientUserItem>();
            m_PoolItem = new List<CClientUserItem>();
        }


        void addPool(CClientUserItem pIClientUserItem)
        {
            if (m_PoolItem.Contains(pIClientUserItem))
            {
                return;
            }
          
            m_PoolItem.Add(pIClientUserItem);
        }

        CClientUserItem getPool()
        {
            CClientUserItem pClient = null;
            if (m_PoolItem.Count > 0)
            {
                pClient = m_PoolItem.Last();
                m_PoolItem.RemoveAt(m_PoolItem.Count - 1);
                return pClient;
            }
            pClient = new CClientUserItem();
            m_UserItemActive.Add(pClient);
            return pClient;
        }

        //配置接口

        ////设置接口
        // bool SetUserInformation(IUserInformation * pIUserInformation);
        //设置接口
        public bool SetUserManagerSink(IUserManagerSink pIUserManagerSink)
        {
            m_pIUserManagerSink = pIUserManagerSink;
            return true;
        }

        //管理接口
        //重置用户
        public bool ResetUserItem()
        {
            for (int i = 0; i < (int) m_UserItemActive.Count; i++)
            {
                if (m_pIUserManagerSink != null)
                    m_pIUserManagerSink.OnUserItemDelete(m_UserItemActive[i]);

            }
            for (int i = 0; i < (int) m_UserItemActive.Count; i++)
            {
                addPool(m_UserItemActive[i]);

            }
            //设置变量
            m_UserItemActive.Clear();

            return true;
        }

        //删除用户
        public bool DeleteUserItem(IClientUserItem pIClientUserItem)
        {
            //查找用户
            CClientUserItem pUserItemActive = null;
            for (int i = 0, l = (int) m_UserItemActive.Count; i < l; i++)
            {
                pUserItemActive = m_UserItemActive[i];
                if (pIClientUserItem == pUserItemActive)
                {
                    //删除用户
                    m_UserItemActive.RemoveAt(i);
                    //m_UserItemActive.erase(m_UserItemActive.begin() + i);
                    addPool(pUserItemActive);

                    //删除通知
                    if (m_pIUserManagerSink != null)
                        m_pIUserManagerSink.OnUserItemDelete(pUserItemActive);

                    //设置数据
                    pUserItemActive.m_cbCompanion = SocketDefines.CP_NORMAL;
                    pUserItemActive.m_UserInfo = new tagUserInfo();
                    pUserItemActive.m_UserInfo.Init();
                    //zeromemory(&pUserItemActive.m_UserInfo, sizeof(tagUserInfo));
                    return true;
                }
            }

            //错误断言
            Debug.Assert(false, "DeleteUserItem failed");

            return false;
        }

        //增加用户
        public IClientUserItem ActiveUserItem(tagUserInfo UserInfo, tagCustomFaceInfo CustomFaceInfo)
        {
            //变量定义
            CClientUserItem pClientUserItem = (CClientUserItem) SearchUserByUserID(UserInfo.dwUserID);
            if (pClientUserItem == null)
            {
                pClientUserItem = new CClientUserItem();
                if (pClientUserItem == null) return null;

                //插入用户
                m_UserItemActive.Add(pClientUserItem);
            }


            pClientUserItem.m_szUserNote[0] = 0;
            pClientUserItem.m_cbCompanion = SocketDefines.CP_NORMAL;

            //设置数据
            pClientUserItem.m_UserInfo = UserInfo;
            pClientUserItem.m_CustomFaceInfo = CustomFaceInfo;
            //memcpy(&pClientUserItem.m_UserInfo, &UserInfo, sizeof(UserInfo));
            //memcpy(&pClientUserItem.m_CustomFaceInfo, &CustomFaceInfo, sizeof(CustomFaceInfo));

            //更新通知

            if (m_pIUserManagerSink != null) m_pIUserManagerSink.OnUserItemAcitve(pClientUserItem);


            return pClientUserItem;
        }

        //更新接口
        //更新积分
        public bool UpdateUserItemScore(IClientUserItem pIClientUserItem, tagUserScore pUserScore)
        {
            Debug.Log("更新积分");
            //效验参数
            //Debug.Assert(pUserScore != null);
            Debug.Assert(pIClientUserItem != null);

            //获取用户
            tagUserInfo pUserInfo = pIClientUserItem.GetUserInfo();

            //以往数据
            tagUserScore UserScore = new tagUserScore();
            UserScore.lScore = pUserInfo.lScore;
            UserScore.lInsure = pUserInfo.lInsureScore;
            UserScore.dwWinCount = pUserInfo.lWinCount;
            UserScore.dwLostCount = pUserInfo.lLostCount;
            UserScore.dwDrawCount = pUserInfo.lDrawCount;
            UserScore.dwFleeCount = pUserInfo.lFleeCount;
            UserScore.dwExperience = pUserInfo.lExperience;

            //设置数据
            pUserInfo.lScore = pUserScore.lScore;
            pUserInfo.lInsureScore = pUserScore.lInsure;
            pUserInfo.lWinCount = pUserScore.dwWinCount;
            pUserInfo.lLostCount = pUserScore.dwLostCount;
            pUserInfo.lDrawCount = pUserScore.dwDrawCount;
            pUserInfo.lFleeCount = pUserScore.dwFleeCount;
            pUserInfo.lExperience = pUserScore.dwExperience;
            pIClientUserItem.SetUserInfo(pUserInfo);
            //通知更新
            Debug.Assert(m_pIUserManagerSink != null);
            if (m_pIUserManagerSink != null)
                m_pIUserManagerSink.OnUserItemUpdate(pIClientUserItem,ref UserScore);

            return true;
        }

        //更新状态
        public bool UpdateUserItemStatus(IClientUserItem pIClientUserItem, tagUserStatus pUserStatus)
        {
            //效验参数
            //ASSERT(pUserStatus != 0);
            Debug.Assert(pIClientUserItem != null);

            //获取用户
            tagUserInfo pUserInfo = pIClientUserItem.GetUserInfo();

            //以往数据
            tagUserStatus tUserStatus = new tagUserStatus();
            tUserStatus.wTableID = pUserInfo.wTableID;
            tUserStatus.wChairID = pUserInfo.wChairID;
            tUserStatus.cbUserStatus = pUserInfo.cbUserStatus;

            //设置数据
            pUserInfo.wTableID = pUserStatus.wTableID;
            pUserInfo.wChairID = pUserStatus.wChairID;
            pUserInfo.cbUserStatus = pUserStatus.cbUserStatus;
            pIClientUserItem.SetUserInfo(pUserInfo);
            //通知更新
            Debug.Assert(m_pIUserManagerSink != null);
            if (m_pIUserManagerSink != null)
                m_pIUserManagerSink.OnUserItemUpdate(pIClientUserItem, ref tUserStatus);


            return true;
        }

        //更新属性
        public bool UpdateUserItemAttrib(IClientUserItem pIClientUserItem, tagUserAttrib pUserAttrib)
        {
            Debug.Assert(pIClientUserItem != null);

            //以往数据
            tagUserAttrib UserAttrib = new tagUserAttrib();
            UserAttrib.cbCompanion = pIClientUserItem.GetUserCompanion();

            //设置数据
            pIClientUserItem.SetUserCompanion(pUserAttrib.cbCompanion);

            //通知更新
            Debug.Assert(m_pIUserManagerSink != null);
            if (m_pIUserManagerSink != null)
                m_pIUserManagerSink.OnUserItemUpdate(pIClientUserItem,ref UserAttrib);

            return true;
        }

        //更新头像
        public bool UpdateUserCustomFace(IClientUserItem pIClientUserItem, uint dwCustomID,
            tagCustomFaceInfo CustomFaceInfo)
        {
            //获取用户
            tagUserInfo pUserInfo = pIClientUserItem.GetUserInfo();
            pIClientUserItem.SetCustomFaceInfo(CustomFaceInfo);

            //设置变量
            pUserInfo.dwCustomID = dwCustomID;
            pIClientUserItem.SetUserInfo(pUserInfo);
            //pCustomFaceInfo = CustomFaceInfo;

            //通知更新
            Debug.Assert(m_pIUserManagerSink != null);
            if (m_pIUserManagerSink != null)
                m_pIUserManagerSink.OnUserFaceUpdate(pIClientUserItem);

            return true;
        }


        //查找接口
        //枚举用户
        public IClientUserItem EnumUserItem(ushort wEnumIndex)
        {
            if (wEnumIndex >= m_UserItemActive.Count) return null;
            return m_UserItemActive[wEnumIndex];
        }

        //查找用户
        public IClientUserItem SearchUserByUserID(uint dwUserID)
        {
            //用户搜索
            for (int i = 0, l = (int) m_UserItemActive.Count; i < l; i++)
            {
                CClientUserItem pClientUserItem = m_UserItemActive[i];
                if (pClientUserItem.m_UserInfo.dwUserID == dwUserID) return pClientUserItem;
            }

            return null;
        }

        //查找用户
        public IClientUserItem SearchUserByGameID(uint dwGameID)
        {
            //用户搜索
            for (int i = 0, l = (int) m_UserItemActive.Count; i < l; i++)
            {
                CClientUserItem pClientUserItem = m_UserItemActive[i];
                if (pClientUserItem.m_UserInfo.dwGameID == dwGameID) return pClientUserItem;
            }

            return null;
        }

        //查找用户
        public IClientUserItem SearchUserByNickName(byte[] pszNickName)
        {
            //用户搜索
            for (int i = 0, l = (int) m_UserItemActive.Count; i < l; i++)
            {
                CClientUserItem pClientUserItem = m_UserItemActive[i];
                byte[] pszTempNickName = pClientUserItem.GetNickName();
                if (pszNickName == pszTempNickName) return pClientUserItem;
            }
            return null;
        }

        //获得人数
        public uint GetActiveUserCount()
        {
            return (uint) m_UserItemActive.Count;
        }
    };
}
