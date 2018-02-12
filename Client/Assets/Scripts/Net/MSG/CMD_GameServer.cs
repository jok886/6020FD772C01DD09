using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GameNet
{

    //////////////////////////////////////////////////////////////////////////////////
    //登录命令
    public class GameServerDefines
    {
        public const uint PRODUCT_VER = 6; //产品版本
        //模块版本
        static uint PROCESS_VERSION(int cbMainVer, int cbSubVer, int cbBuildVer)
        {
            return (uint)(
                (((byte)(PRODUCT_VER)) << 24) +
                (((byte)(cbMainVer)) << 16) +
                ((byte)(cbSubVer) << 8) +
                (byte)(cbBuildVer));
        }

        //程序版本
        public static uint VERSION_FRAME = PROCESS_VERSION(6, 1, 3);            //框架版本
        public static uint VERSION_PLAZA = PROCESS_VERSION(9, 1, 3);            //大厅版本
        public static uint VERSION_MOBILE_ANDROID = PROCESS_VERSION(6, 1, 3);       //手机版本
        public static uint VERSION_MOBILE_IOS = PROCESS_VERSION(6, 1, 3);			//手机版本

        public const int DLG_MB_OK = 0x1;
        public const int DLG_MB_CANCEL = 0x2;
        public const int DLG_MB_YES = 0x4;
        public const int DLG_MB_NO = 0x8;

        public const int MDM_CM_SYSTEM = 1000; //系统命令

        public const int SUB_CM_SYSTEM_MESSAGE = 1; //系统消息
        public const int SUB_CM_ACTION_MESSAGE = 2; //动作消息
        public const int SUB_CM_DOWN_LOAD_MODULE = 3; //下载消息

        //类型掩码
        public const int SMT_CHAT = 0x0001; //聊天消息
        public const int SMT_EJECT = 0x0002; //弹出消息
        public const int SMT_GLOBAL = 0x0004; //全局消息
        public const int SMT_PROMPT = 0x0008; //提示消息
        public const int SMT_TABLE_ROLL = 0x0010; //滚动消息

        //控制掩码
        public const int SMT_CLOSE_ROOM = 0x0100; //关闭房间
        public const int SMT_CLOSE_GAME = 0x0200; //关闭游戏
        public const int SMT_CLOSE_LINK = 0x0400; //中断连接

        //////////////////////////////////////////////////////////////////////////////////

        public const int MDM_GR_LOGON = 1; //登录信息

        //登录模式
        public const int SUB_GR_LOGON_USERID = 1; //I D 登录
        public const int SUB_GR_LOGON_MOBILE = 2; //手机登录
        public const int SUB_GR_LOGON_ACCOUNTS = 3; //帐户登录

        //登录结果
        public const int SUB_GR_LOGON_SUCCESS = 100; //登录成功
        public const int SUB_GR_LOGON_FAILURE = 101; //登录失败
        public const int SUB_GR_LOGON_FINISH = 102; //登录完成

        //升级提示
        public const int SUB_GR_UPDATE_NOTIFY = 200; //升级提示

        //////////////////////////////////////////////////////////////////////////////////
        ///   //////////////////////////////////////////////////////////////////////////////////
        //配置命令

        public const int MDM_GR_CONFIG = 2; //配置信息

        public const int SUB_GR_CONFIG_COLUMN = 100; //列表配置
        public const int SUB_GR_CONFIG_SERVER = 101; //房间配置
        public const int SUB_GR_CONFIG_PROPERTY = 102; //道具配置
        public const int SUB_GR_CONFIG_FINISH = 103; //配置完成
        public const int SUB_GR_CONFIG_USER_RIGHT = 104; //玩家权限
        //////////////////////////////////////////////////////////////////////////////////
        /// //////////////////////////////////////////////////////////////////////////////////

        public const int MDM_GR_USER = 3; //用户信息

        //用户动作
        public const int SUB_GR_USER_RULE = 1; //用户规则
        public const int SUB_GR_USER_LOOKON = 2; //旁观请求
        public const int SUB_GR_USER_SITDOWN = 3; //坐下请求
        public const int SUB_GR_USER_STANDUP = 4; //起立请求
        public const int SUB_GR_USER_INVITE = 5; //用户邀请
        public const int SUB_GR_USER_INVITE_REQ = 6; //邀请请求
        public const int SUB_GR_USER_REPULSE_SIT = 7; //拒绝玩家坐下
        public const int SUB_GR_USER_KICK_USER = 8; //踢出用户
        public const int SUB_GR_USER_INFO_REQ = 9; //请求用户信息
        public const int SUB_GR_USER_CHAIR_REQ = 10; //请求更换位置
        public const int SUB_GR_USER_CHAIR_INFO_REQ = 11; //请求椅子用户信息

        //用户状态
        public const int SUB_GR_USER_ENTER = 100; //用户进入
        public const int SUB_GR_USER_SCORE = 101; //用户分数
        public const int SUB_GR_USER_STATUS = 102; //用户状态
        public const int SUB_GR_SIT_FAILED = 103; //请求失败


        //聊天命令
        public const int SUB_GR_USER_CHAT = 201; //聊天消息
        public const int SUB_GR_USER_EXPRESSION = 202; //表情消息
        public const int SUB_GR_WISPER_CHAT = 203; //私聊消息
        public const int SUB_GR_WISPER_EXPRESSION = 204; //私聊表情
        public const int SUB_GR_COLLOQUY_CHAT = 205; //会话消息
        public const int SUB_GR_COLLOQUY_EXPRESSION = 206; //会话表情

        //道具命令
        public const int SUB_GR_PROPERTY_BUY = 300; //购买道具
        public const int SUB_GR_PROPERTY_SUCCESS = 301; //道具成功
        public const int SUB_GR_PROPERTY_FAILURE = 302; //道具失败
        public const int SUB_GR_PROPERTY_MESSAGE = 303; //道具消息
        public const int SUB_GR_PROPERTY_EFFECT = 304; //道具效应
        public const int SUB_GR_PROPERTY_TRUMPET = 305; //喇叭消息

        public const int SUB_GR_GLAD_MESSAGE = 400; //喜报消息

        //////////////////////////////////////////////////////////////////////////////////
        //状态命令

        public const int MDM_GR_STATUS = 4; //状态信息

        public const int SUB_GR_TABLE_INFO = 100; //桌子信息
        public const int SUB_GR_TABLE_STATUS = 101; //桌子状态

        //////////////////////////////////////////////////////////////////////////////////
        /// //////////////////////////////////////////////////////////////////////////////////
        //比赛命令

        public const int MDM_GR_MATCH = 9; //比赛命令

        public const int SUB_GR_MATCH_FEE = 400; //报名费用
        public const int SUB_GR_MATCH_NUM = 401; //等待人数
        public const int SUB_GR_LEAVE_MATCH = 402; //退出比赛
        public const int SUB_GR_MATCH_INFO = 403; //比赛信息
        public const int SUB_GR_MATCH_WAIT_TIP = 404; //等待提示
        public const int SUB_GR_MATCH_RESULT = 405; //比赛结果
        public const int SUB_GR_MATCH_STATUS = 406; //比赛状态
        public const int SUB_GR_MATCH_GOLDUPDATE = 409; //金币更新
        public const int SUB_GR_MATCH_ELIMINATE = 410; //比赛淘汰
        public const int SUB_GR_MATCH_JOIN_RESOULT = 411; //加入结果

        public const int SUB_GR_MATCH_SIGNUP_CHECK = 412; //mChen test;报名Check

        //////////////////////////////////////////////////////////////////////////////////
        //私人场命令

        public const int MDM_GR_PRIVATE = 10; //比赛命令

        public const int SUB_GR_PRIVATE_INFO = 401; //私人场信息
        public const int SUB_GR_CREATE_PRIVATE = 402; //创建私人场
        public const int SUB_GR_CREATE_PRIVATE_SUCESS = 403; //创建私人场成功
        public const int SUB_GR_JOIN_PRIVATE = 404; //加入私人场
        public const int SUB_GF_PRIVATE_ROOM_INFO = 405; //私人场房间信息
        public const int SUB_GR_PRIVATE_DISMISS = 406; //私人场请求解散
        public const int SUB_GF_PRIVATE_END = 407; //私人场结算
        public const int SUB_GR_RIVATE_AGAIN = 408; //创建私人场



        public const int MAX_PRIVATE_ACTION = 8;


        //////////////////////////////////////////////////////////////////////////////////
        //框架命令

        public const int MDM_GF_FRAME = 100; //框架命令

        //////////////////////////////////////////////////////////////////////////////////
        //框架命令

        //用户命令
        public const int SUB_GF_GAME_OPTION = 1;            //游戏配置
        public const int SUB_GF_USER_READY = 2;             //用户准备
        public const int SUB_GF_LOOKON_CONFIG = 3;          //旁观配置
        //mChen add, for HideSeek
        public const int SUB_GF_CREATER_PRESS_START = 4;    //房主是否点击开始按钮
        public const int SUB_GF_INVENTORY_CREATE = 5;	    //道具生成同步：服务器通知客户端生成道具

        //聊天命令
        public const int SUB_GF_USER_CHAT_INDEX = 8; //用户聊天 mChen add
        public const int SUB_GF_USER_EXPRESSION_INDEX = 9; //会话表情 mChen add
        public const int SUB_GF_USER_CHAT = 10; //用户聊天
        public const int SUB_GF_USER_EXPRESSION = 11; //用户表情
        public const int SUB_GR_TABLE_TALK = 12; //用户聊天


        //游戏信息
        public const int SUB_GF_GAME_STATUS = 100; //游戏状态
        public const int SUB_GF_GAME_SCENE = 101; //游戏场景
        public const int SUB_GF_LOOKON_STATUS = 102; //旁观状态


        //系统消息
        public const int SUB_GF_SYSTEM_MESSAGE = 200; //系统消息
        public const int SUB_GF_ACTION_MESSAGE = 201; //动作消息

        //////////////////////////////////////////////////////////////////////////////////
        //游戏命令

        public const int MDM_GF_GAME = 200; //游戏命令

        //////////////////////////////////////////////////////////////////////////////////
        //携带信息

        //其他信息
        public const int DTP_GR_TABLE_PASSWORD = 1; //桌子密码

        //用户属性
        public const int DTP_GR_NICK_NAME = 10; //用户昵称
        public const int DTP_GR_GROUP_NAME = 11; //社团名字
        public const int DTP_GR_UNDER_WRITE = 12; //个性签名
        public const int DTP_GR_HEAD_HTTP = 13;	//微信头像http地址

        //附加信息
        public const int DTP_GR_USER_NOTE = 20; //用户备注
        public const int DTP_GR_CUSTOM_FACE = 21; //自定头像

        //////////////////////////////////////////////////////////////////////////////////

        //请求错误
        public const int REQUEST_FAILURE_NORMAL = 0; //常规原因
        public const int REQUEST_FAILURE_NOGOLD = 1; //金币不足
        public const int REQUEST_FAILURE_NOSCORE = 2; //积分不足
        public const int REQUEST_FAILURE_PASSWORD = 3; //密码错误

        //////////////////////////////////////////////////////////////////////////////////

    }


    //系统消息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_CM_SystemMessage
    {
        public ushort wType;                             //消息类型
        public ushort wLength;                           //消息长度
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] szString;                        //消息内容

        public void StreamValue(byte[] kData, int dataSize)
        {
            int offset = 0;

            wType = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            wLength = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            int nStrSize = dataSize - offset;
            szString = new byte[nStrSize];
            Buffer.BlockCopy(kData, offset, szString, 0, nStrSize);
        }
    };

    //
    //房间 ID 登录
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_LogonUserID
    {
        public uint dwPlazaVersion;                       //广场版本
        public uint dwFrameVersion;                       //框架版本
        public uint dwProcessVersion;                 //进程版本

        //登录信息
        public uint dwUserID;                         //用户 I D
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MD5)]
        public byte[] szPassword;               //登录密码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MACHINE_ID)]
        public byte[] szMachineID;       //机器序列
        public ushort wKindID;                           //类型索引

        public void Init()
        {
            szPassword = new byte[SocketDefines.LEN_MD5];
            szMachineID = new byte[SocketDefines.LEN_MACHINE_ID];
        }
    };

    //登录成功消息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_LogonSuccess
    {
        //	unsigned int					dwUserID;							//用户 I D
        public uint dwUserRight;                      //用户权限
        public uint dwMasterRight;                        //管理权限

        public byte cbIsForAppleReview;
    };

    //登录失败
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_LogonError
    {
        public uint lErrorCode;                            //错误代码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] szErrorDescribe;              //错误消息
    };

    //列表配置
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_ConfigColumn
    {
        public byte cbColumnCount;                     //列表数目
        [MarshalAs(UnmanagedType.ByValArray,SizeConst = SocketDefines.MAX_COLUMN,ArraySubType = UnmanagedType.Struct)]
        tagColumnItem[] ColumnItem;               //列表描述
    };

    //房间配置
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_ConfigServer
    {
        //房间属性
        public ushort wTableCount;                       //桌子数目
        public ushort wChairCount;                       //椅子数目

        //房间配置
        public ushort wServerType;                       //房间类型
        public uint dwServerRule;                     //房间规则
    };
    /*
    //道具配置
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_ConfigProperty
    {
        public byte cbPropertyCount;                   //道具数目
        public tagPropertyInfo PropertyInfo[MAX_PROPERTY];         //道具描述
    };
    */
    //玩家权限
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_ConfigUserRight
    {
        public uint dwUserRight;                      //玩家权限
    };


    //起立请求
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_UserStandUp
    {
        public ushort wTableID;                          //桌子位置
        public ushort wChairID;                          //椅子位置
        public byte cbForceLeave;                      //强行离开
    };

    //用户分数
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_UserScore
    {
        public uint dwUserID;                         //用户标识
        public tagUserScore UserScore;                         //积分信息
    };
    //lin: 涉及到datastream， c++中是std：：vector，暂时跳过
    //用户语音聊天
#if true
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GR_C_TableTalk
    {
        public byte cbChairID;                         //座位
        //datastream kDataStream;                     //语音数据
        //void StreamValue(datastream& kData, bool bSend)
        //{
        //    Stream_VALUE(cbChairID);
        //    Stream_VALUE(kDataStream);
        //}
    };
#endif
    //请求坐下
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_UserSitDown
    {
        public ushort wTableID;                          //桌子位置
        public ushort wChairID;                          //椅子位置
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.PASS_LEN)]
        public byte[] szTablePass;             //桌子密码

        public void Init()
        {
            szTablePass = new byte[SocketDefines.PASS_LEN];
        }
    };

    //用户状态
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_UserStatus
    {
        public uint dwUserID;                         //用户标识
        public tagUserStatus UserStatus;                           //用户状态
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IPC_GF_UserInfo
    {
        public byte cbCompanion;                       //用户关系
        public tagUserInfoHead UserInfoHead;                       //用户信息
    };

    //请求失败
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_RequestFailure
    {
        public uint lErrorCode;                            //错误代码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] szDescribeString;             //描述信息

        public void StreamValue(byte[] kData, int dataSize)
        {
            int offset = 0;

            lErrorCode = System.BitConverter.ToUInt32(kData, offset);
            offset += sizeof(uint);

            int nStrSize = dataSize - offset;
            szDescribeString = new byte[nStrSize];
            Buffer.BlockCopy(kData, offset, szDescribeString, 0, nStrSize);
        }
    };



    //桌子信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_TableInfo
    {
        public ushort wTableCount;                       //桌子数目
        public tagTableStatus TableStatusArray;               //桌子状态
    };

    //桌子状态
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_TableStatus
    {
        public ushort wTableID;                          //桌子号码
        public tagTableStatus TableStatus;                     //桌子状态
    };

    //费用提醒
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_Match_Fee
    {
        public long lMatchFee;                            //报名费用
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] szNotifyContent;              //提示内容
    };


    //费用提醒
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_Match_JoinResoult
    {
        public ushort wSucess;
    };

    //比赛人数
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_Match_Num
    {
        public uint dwWaitting;                           //等待人数
        public uint dwTotal;                          //开赛人数
    };

    //赛事信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_Match_Info
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] szTitle;                      //信息标题  szTitle[4][64]
        public ushort wGameCount;                            //游戏局数
        public ushort wRank;                             //当前名次
    };

    //提示信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_Match_Wait_Tip
    {
        public long lScore;                               //当前积分
        public ushort wRank;                             //当前名次
        public ushort wCurTableRank;                     //本桌名次
        public ushort wUserCount;                            //当前人数
        public ushort wCurGameCount;                     //当前局数
        public ushort wGameCount;                            //总共局数
        public ushort wPlayingTable;                     //游戏桌数
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_SERVER)]
        public byte[] szMatchName; //比赛名称
    };

    //比赛结果
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_MatchResult
    {
        public long lGold;                                //金币奖励
        public uint dwIngot;                          //元宝奖励
        public uint dwExperience;                     //经验奖励
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] szDescribe;                   //得奖描述
    };
    //最多描述

    //金币更新
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_MatchGoldUpdate
    {
        public long lCurrGold;                            //当前金币
        public long lCurrIngot;                           //当前元宝
        public uint dwCurrExprience;                  //当前经验
    };

    //私人场信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_Private_Info
    {
        public ushort wKindID;
        public long lCostGold;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] bPlayCout;                          //玩家局数
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4,ArraySubType = UnmanagedType.I8)]
        public long[] lPlayCost;                         //消耗点数
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        //public byte[] szGameType;                       //游戏类型szGameType[4][32]
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        //public byte[] szGameRule;                       //游戏规则szGameRule[4][32]

        //mChen add,金币场模拟比赛场
        public byte cbMatchPlayCout;
        public systemtime MatchStartTime;                //游戏开始日期
        public systemtime MatchEndTime;                     //游戏结束日期
    };

    public enum RoomType
    {
        Type_Private = 0,
        Type_Public,
    };

    //创建房间
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_Create_Private
    {
        public byte cbGameType;                             //游戏类型
        public byte bPlayCoutIdex;                          //游戏局数
        public byte bGameTypeIdex;                          //游戏类型
        public uint bGameRuleIdex;                          //游戏规则

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)]
        public byte[] stHttpChannel;//http获取

        public byte cbPlayCostTypeIdex;                             //费用，0-房主支付， 1-平均支付
        public long lBaseScore;
        public byte PlayerCount;                            //游戏人数

        //mChen add, for HideSeek
        public byte cbChoosedMapIndex;
        public byte cbChoosedModelIndex;
    };

    //创建房间
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_Create_Private_Sucess
    {
        public long lCurSocre;                                //当前剩余
        public uint dwRoomNum;                                //房间ID
    };

    //创建房间
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_Join_Private
    {
        public uint dwRoomNum;                                //房间ID
        public byte cbGameTypeIdex; //mChen add

        //mChen add, for HideSeek
        public byte cbChoosedModelIndex;
    };

    //私人场房间信息
    public struct CMD_GF_Private_Room_Info
    {
        public byte bPlayCoutIdex;     //玩家局数0 1，  8 或者16局
        public byte bGameTypeIdex;     //游戏类型
        public uint bGameRuleIdex;        //游戏规则

        public byte bStartGame;
        public uint dwPlayCout;           //游戏局数
        public uint dwRoomNum;
        public uint dwCreateUserID;
        public uint dwPlayTotal;      //总局数

        //mChen add
        public byte cbRoomType;
        public long lBaseScore;
        //ZY add
        public byte PlayerCount;

        public List<int> kWinLoseScore;

        public void StreamValue(byte[] kData, int dataSize)
        {
#if true
            kWinLoseScore = new List<int>();
            int offset = 0;
            bPlayCoutIdex = kData[offset++];
            bGameTypeIdex = kData[offset++];
            bGameRuleIdex = BitConverter.ToUInt32(kData, offset);
            offset += 4;
            bStartGame = kData[offset++];
            dwPlayCout = BitConverter.ToUInt32(kData, offset);
            offset += 4;
            dwRoomNum = BitConverter.ToUInt32(kData, offset);
            offset += 4;
            dwCreateUserID = BitConverter.ToUInt32(kData, offset);
            offset += 4;
            dwPlayTotal = BitConverter.ToUInt32(kData, offset);
            offset += 4;

            cbRoomType = kData[offset++];
            lBaseScore = BitConverter.ToInt64(kData, offset);
            long test = BitConverter.ToUInt32(kData, offset);
            offset += 8;
            PlayerCount = kData[offset++];
            while (offset <= dataSize - sizeof(int))
            {
                kWinLoseScore.Add(BitConverter.ToInt32(kData,offset));
                offset += 4;
            }
#else
            Stream_VALUE(bPlayCoutIdex);
            Stream_VALUE(bGameTypeIdex);
            Stream_VALUE(bGameRuleIdex);
            Stream_VALUE(bStartGame);
            Stream_VALUE(dwPlayCout);
            Stream_VALUE(dwRoomNum);
            Stream_VALUE(dwCreateUserID);
            Stream_VALUE(dwPlayTotal);
            Stream_VALUE(kWinLoseScore);
#endif
        }
    };

    //解散房间
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_Dismiss_Private
    {
        public byte bDismiss;          //解散
    };

    //重新加入
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GR_Again_Private
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)]
        public byte[] stHttpChannel;

        public void Init()
        {
            stHttpChannel = new byte[SocketDefines.LEN_NICKNAME];
            Array.Clear(stHttpChannel, 0, stHttpChannel.Length);
        }
    };


    //私人场解散信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GF_Private_Dismiss_Info
    {
        public uint dwDissUserCout;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.MAX_CHAIR,ArraySubType = UnmanagedType.U4)]
        public uint[] dwDissChairID;
        public uint dwNotAgreeUserCout;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.MAX_CHAIR, ArraySubType = UnmanagedType.U4)]
        public uint[] dwNotAgreeChairID;
    };

    //私人场结算信息
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GF_Private_End_Info
    {
        public byte cbEndReason;

        //mChen add, for HideSeek
        public byte cbMapIndex;
        //for随机种子同步
        public ushort wRandseed;
        //地图随机物品生成
        public ushort wRandseedForRandomGameObject;
        //道具同步
        public ushort wRandseedForInventory;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = InventoryManager.MAX_INVENTORY_NUM)]
        public InventoryItem[] sInventoryList;

        //public List<Int64> lPlayerWinLose;
        //public List<byte> lPlayerAction;

        /*
        public void StreamValue(byte[] kData, bool bSend, int dataSize)
        {
#if true
            //lPlayerWinLose = new List<Int64>();
            //lPlayerAction = new List<byte>();

            int offset = 0;
            cbEndReason = kData[offset++];

            //mChen add, for HideSeek
            cbMapIndex = kData[offset];
            offset += Marshal.SizeOf(cbMapIndex);
            wRandseed = BitConverter.ToUInt16(kData, offset);
            offset += Marshal.SizeOf(wRandseed);
            wRandseedForRandomGameObject = BitConverter.ToUInt16(kData, offset);
            offset += Marshal.SizeOf(wRandseedForRandomGameObject);
            wRandseedForInventory = BitConverter.ToUInt16(kData, offset);
            offset += Marshal.SizeOf(wRandseedForInventory);

            sInventoryList = new InventoryItem[InventoryManager.MAX_INVENTORY_NUM];
            Buffer.BlockCopy(kData, offset, sInventoryList, 0, sInventoryList.Length);

            //int nSize = BitConverter.ToInt32(kData, offset);
            //offset += 4;
            //while (offset <= dataSize - sizeof(Int64) && lPlayerWinLose.Count < nSize)
            //{
            //    lPlayerWinLose.Add(BitConverter.ToInt64(kData, offset));
            //    offset += 8;
            //}

            //nSize = BitConverter.ToInt32(kData, offset);
            //offset += 4;
            //while (offset <= dataSize - sizeof(byte))
            //{
            //    lPlayerAction.Add(kData[offset++]);
            //}
#else
            Stream_VALUE(lPlayerWinLose);
            Stream_VALUE(lPlayerAction);
#endif
        }
        */
    };

    //游戏配置
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GF_GameOption
    {
        public byte cbAllowLookon;                     //旁观标志
        public uint dwFrameVersion;                       //框架版本
        public uint dwClientVersion;                  //游戏版本
    };

    //mChen add
    //////////////////////////////////////////////////////////////////////////
    //用户聊天
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GF_C_UserChatIdx
    {
        public ushort wItemIndex;                            //文字索引
        public ushort wSendUserID;                       //发送用户
    };

    //用户表情
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GF_C_UserExpressionIdx
    {
        public ushort wItemIndex;                            //表情索引
        public ushort wSendUserID;                       //发送用户
    };

    //struct CMD_GF_C_UserChat
    //{
    //	public ushort							wChatLength;						//信息长度
    //	public uint							dwChatColor;						//信息颜色
    //	public uint							dwTargetUserID;						//目标用户
    //	TCHAR							szChatString[LEN_USER_CHAT];		//聊天信息
    //};

    //struct CMD_GF_C_UserExpression
    //{
    //	public ushort							wItemIndex;							//表情索引
    //	public uint							dwTargetUserID;						//目标用户
    //};
    //////////////////////////////////////////////////////////////////////////

    //游戏环境
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GF_GameStatus
    {
        public byte cbGameStatus;                      //游戏状态
        public byte cbAllowLookon;                     //旁观标志
    };

    //////////////////////////////////////////////////////////////////////////////////
}
