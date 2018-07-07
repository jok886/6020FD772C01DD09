#define TEMP_REMOVE
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GameNet
{
    /*----------------------------\GameLib\Platform\PFDefine\df\Struct.h --------------------------------------*/

    class DFStructs // 对应于\GameLib\Platform\PFDefine\df\Struct.h
    {



        //////////////////////////////////////////////////////////////////////////////////////////////////////
        //道具索引
        public static byte PROP_DOUBLE = 0; //双倍积分卡
        public static byte PROP_FOURDOLD = 1; //四倍积分卡
        public static byte PROP_NEGAGIVE = 2; //负分清零
        public static byte PROP_FLEE = 3; //清逃跑率
        public static byte PROP_BUGLE = 4; //小喇叭
        public static byte PROP_KICK = 5; //防踢卡
        public static byte PROP_SHIELD = 6; //护身符
        public static byte PROP_MEMBER_1 = 7; //会员道具
        public static byte PROP_MEMBER_2 = 8; //会员道具
        public static byte PROP_MEMBER_3 = 9; //会员道具
        public static byte PROP_MEMBER_4 = 10; //会员道具
        public static byte PROP_MEMBER_5 = 11; //会员道具
        public static byte PROP_MEMBER_6 = 12; //会员道具
        public static byte PROP_MEMBER_7 = 13; //会员道具
        public static byte PROP_MEMBER_8 = 14; //会员道具

        public static byte[] g_PropTypeList =
        {
            PROP_DOUBLE,
            PROP_FOURDOLD,
            PROP_NEGAGIVE,
            PROP_FLEE,
            PROP_BUGLE,
            PROP_KICK,
            PROP_SHIELD,
            PROP_MEMBER_1,
            PROP_MEMBER_2,
            PROP_MEMBER_3,
            PROP_MEMBER_4,
            PROP_MEMBER_5,
            PROP_MEMBER_6,
            PROP_MEMBER_7,
            PROP_MEMBER_8,
        };

        public static int PROPERTY_COUNT = g_PropTypeList.Length; //道具数目
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct systemtime
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    };

    //游戏类型结构
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagGameType
    {
        public ushort wJoinID; //挂接索引
        public ushort wSortID; //排序索引
        public ushort wTypeID; //类型索引
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_TYPE)] public byte[] szTypeName; //种类名字
    };

    //游戏名称结构
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagGameKind
    {
        public ushort wTypeID; //类型号码
        public ushort wJoinID; //挂接索引
        public ushort wSortID; //排序号码
        public ushort wKindID; //名称号码
        public ushort wGameID; //模块索引
        public uint dwOnLineCount; //在线人数
        public uint dwFullCount; //满员人数
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_KIND)] public byte[] szKindName; //游戏名字
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MODULE)] public byte[] szProcessName; //进程名字
    };


    //游戏房间列表结构
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagGameServer
    {
        public ushort wKindID; //名称索引
        public ushort wNodeID; //节点索引
        public ushort wSortID; //排序索引
        public ushort wServerID; //房间索引
        //public ushort                            wServerKind;                        //房间类型
        public ushort wServerType; //房间类型
        public ushort wServerPort; //房间端口
        public long lCellScore; //单元积分
        public long lEnterScore; //进入积分
        public uint dwServerRule; //房间规则
        public uint dwOnLineCount; //在线人数
        public uint dwAndroidCount; //机器人数
        public uint dwFullCount; //满员人数
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] szServerAddr; //房间名称
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_SERVER)] public byte[] szServerName; //房间名称
    };

    //比赛报名
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagSignupMatchInfo
    {
        public ushort wServerID; //房间标识
        public uint dwMatchID; //比赛标识
        public uint dwMatchNO; //比赛场次
    };

    //比赛信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagGameMatch
    {
        //基本信息
        public ushort wServerID; //房间标识
        public uint dwMatchID; //比赛标识
        public uint dwMatchNO; //比赛场次	
        public byte cbMatchType; //比赛类型
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] szMatchName; //比赛名称

        //比赛信息
        public byte cbMemberOrder; //会员等级
        public byte cbMatchFeeType; //扣费类型
        public long lMatchFee; //比赛费用	
        public long lMatchEnterScore; //准入金币

        //比赛信息
        public ushort wStartUserCount; //开赛人数
        public ushort wMatchPlayCount; //比赛局数

        //比赛奖励
        public ushort wRewardCount; //奖励人数

        //比赛时间
        systemtime MatchStartTime; //开始时间
        systemtime MatchEndTime; //结束时间	
    };

    //在线信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagOnLineInfoKind
    {
        public ushort wKindID; //类型标识
        public uint dwOnLineCount; //在线人数
    };

    //在线信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagOnLineInfoServer
    {
        public ushort wServerID; //房间标识
        public uint dwOnLineCount; //在线人数
    };

    //////////////////////////////////////////////////////////////////////////////////
    //用户信息

    //桌子状态
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagTableStatus
    {
        public byte cbTableLock; //锁定标志
        public byte cbPlayStatus; //游戏标志
    };

    //用户状态
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagUserStatus
    {
        public ushort wTableID; //桌子索引
        public ushort wChairID; //椅子位置
        public byte cbUserStatus; //用户状态
    };

    //用户属性
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagUserAttrib
    {
        public byte cbCompanion; //用户关系
    };

    //用户积分
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagUserScore
    {
        //积分信息
        public long lScore; //用户分数
        public long lGrade; //用户成绩
        public long lInsure; //用户银行

        //输赢信息
        public uint dwWinCount; //胜利盘数
        public uint dwLostCount; //失败盘数
        public uint dwDrawCount; //和局盘数
        public uint dwFleeCount; //逃跑盘数

        //全局信息
        public uint dwUserMedal; //用户奖牌
        public uint dwExperience; //用户经验
        public uint lLoveLiness; //用户魅力
    };

    //用户积分
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagMobileUserScore
    {
        //积分信息
        public long lScore; //用户分数

        //输赢信息
        public uint dwWinCount; //胜利盘数
        public uint dwLostCount; //失败盘数
        public uint dwDrawCount; //和局盘数
        public uint dwFleeCount; //逃跑盘数

        //全局信息
        public uint dwExperience; //用户经验
    };


    //道具使用
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagUsePropertyInfo
    {
        public ushort wPropertyCount; //道具数目
        public ushort dwValidNum; //有效数字
        public uint dwEffectTime; //生效时间
    };


    //用户道具
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagUserProperty
    {
        public ushort wPropertyUseMark; //道具标示

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SocketDefines.MAX_PT_MARK,
            ArraySubType = UnmanagedType.Struct)] public tagUsePropertyInfo[] PropertyInfo; //使用信息   
    };

    //道具包裹
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagPropertyPackage
    {
        public ushort wTrumpetCount; //小喇叭数
        public ushort wTyphonCount; //大喇叭数
    };

    //时间信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagTimeInfo
    {
        public uint dwEnterTableTimer; //进出桌子时间
        public uint dwLeaveTableTimer; //离开桌子时间
        public uint dwStartGameTimer; //开始游戏时间
        public uint dwEndGameTimer; //离开游戏时间
    };

    //用户信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagUserInfo
    {
        //基本属性
        public uint dwUserID; //用户 I D
        public uint dwGameID; //游戏 I D
        public uint dwGroupID; //社团 I D

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)] public byte[] szNickName;//用户昵称

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_GROUP_NAME)] public byte[] szGroupName; //社团名字

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_UNDER_WRITE)] public byte[] szUnderWrite; //个性签名

        //WQ add
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_USER_NOTE)] public byte[] szHeadHttp;
        public uint dwClientAddr; //连接地址
        public uint dwPlayCount;
        public systemtime RegisterDate;

        // for HideSeek WangHu
        public byte cbTeamType;
        public byte cbModelIndex;
        public byte cbChoosedModelIndexOfTagger;

        //头像信息
        public ushort wFaceID; //头像索引
        public uint dwCustomID; //自定标识

        //用户资料
        public byte cbGender; //用户性别
        public byte cbMemberOrder; //会员等级
        public byte cbMasterOrder; //管理等级

        //用户状态
        public ushort wTableID; //桌子索引
        public ushort wLastTableID; //游戏桌子
        public ushort wChairID; //椅子索引
        public byte cbUserStatus; //用户状态
        public byte cbLastUserStatus; //上一次用户状态

        //积分信息
        public long lScore; //用户分数
        public long lGrade; //用户成绩
        public long lInsureScore; //用户银行
        public long lGameGold; //用户元宝

        //游戏信息
        public uint lWinCount; //胜利盘数
        public uint lLostCount; //失败盘数
        public uint lDrawCount; //和局盘数
        public uint lFleeCount; //逃跑盘数
        public uint lExperience; //用户经验
        public uint lLoveLiness; //用户魅力

        //时间信息
        tagTimeInfo TimerInfo;

        public void Init()
        {
            szNickName = new byte[SocketDefines.LEN_NICKNAME];
            szGroupName = new byte[SocketDefines.LEN_GROUP_NAME];
            szUnderWrite = new byte[SocketDefines.LEN_UNDER_WRITE];
            ///szLogonIP = new byte[SocketDefines.LEN_ACCOUNTS];
            szHeadHttp = new byte[SocketDefines.LEN_USER_NOTE];
        }
    };

    //用户基本信息结构
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagUserInfoHead
    {
        //用户属性
        public uint dwGameID; //游戏 I D
        public uint dwUserID; //用户 I D
        public uint dwGroupID; //社团 I D

        //头像信息
        public ushort wFaceID; //头像索引
        public uint dwCustomID; //自定标识

        //用户属性
        public byte cbGender; //用户性别
        public byte cbMemberOrder; //会员等级
        public byte cbMasterOrder; //管理等级

        //用户状态
        public ushort wTableID; //桌子索引
        public ushort wChairID; //椅子索引
        public byte cbUserStatus; //用户状态

        //积分信息
        public long lScore; //用户分数
        public long lGrade; //用户成绩
        public long lInsure; //用户银行

        //游戏信息
        public uint dwWinCount; //胜利盘数
        public uint dwLostCount; //失败盘数
        public uint dwDrawCount; //和局盘数
        public uint dwFleeCount; //逃跑盘数
        public uint dwUserMedal; //用户奖牌
        public uint dwExperience; //用户经验
        public uint lLoveLiness; //用户魅力

        //WQ add
        public uint dwClientAddr;						//连接地址
        public uint dwPlayCount;
        public systemtime RegisterDate;

        // for HideSeek WangHu
        public byte cbTeamType;
        public byte cbModelIndex;
    };

    //头像信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagCustomFaceInfo
    {
        public uint dwDataSize; //数据大小

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SocketDefines.FACE_CX*SocketDefines.FACE_CY,
            ArraySubType = UnmanagedType.U4)] public uint[] dwCustomFace; //图片信息
    };

    //用户信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagUserRemoteInfo
    {
        //用户信息
        public uint dwUserID; //用户标识
        public uint dwGameID; //游戏标识

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)] public byte[] szNickName;
        //用户昵称

        //等级信息
        public byte cbGender; //用户性别
        public byte cbMemberOrder; //会员等级
        public byte cbMasterOrder; //管理等级

        //位置信息
        public ushort wKindID; //类型标识
        public ushort wServerID; //房间标识

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_SERVER)] public byte[] szGameServer;
        //房间位置
    };


    //等级配置
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagGrowLevelConfig
    {
        public ushort wLevelID; //等级 I D
        public uint dwExperience; //相应经验
    };

    //等级参数
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagGrowLevelParameter
    {
        public ushort wCurrLevelID; //当前等级
        public uint dwExperience; //当前经验
        public uint dwUpgradeExperience; //下级经验
        public long lUpgradeRewardGold; //升级奖励
        public long lUpgradeRewardIngot; //升级奖励
    };


    //////////////////////////////////////////////////////////////////////////////////

    // for HideSeek
    //大厅子项
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagGameLobby
    {
        public ushort wLobbyID; //大厅标识
        public ushort wLobbyPort; //端口

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] szServerAddr; //服务地址

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] szServerName; //服务器名
    };

    //广场子项
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagGamePlaza
    {
        public ushort wPlazaID; //广场标识
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] szServerAddr; //服务地址
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] szServerName; //服务器名
    };

    //级别子项
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagLevelItem
    {
        public uint lLevelScore; //级别积分
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] szLevelName; //级别描述
    };

    //会员子项
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagMemberItem
    {
        public byte cbMemberOrder; //等级标识
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] szMemberName; //等级名字
    };

    //管理子项
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagMasterItem
    {
        public byte cbMasterOrder; //等级标识
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] szMasterName; //等级名字
    };

    //列表子项
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagColumnItem
    {
        byte[] cbColumnWidth; //列表宽度
        byte[] cbDataDescribe; //字段类型
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] szColumnName; //列表名字
    };

    //地址信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagAddressInfo
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] szAddress; //服务地址
    };

    //数据信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagDataBaseParameter
    {
        public ushort wDataBasePort; //数据库端口
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] szDataBaseAddr; //数据库地址
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] szDataBaseUser; //数据库用户
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] szDataBasePass; //数据库密码
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] szDataBaseName; //数据库名字
    };

    //房间配置
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagServerOptionInfo
    {
        //挂接属性
        public ushort wKindID; //挂接类型
        public ushort wNodeID; //挂接节点
        public ushort wSortID; //排列标识

        //税收配置
        public ushort wRevenueRatio; //税收比例
        public long lServiceScore; //服务费用

        //房间配置
        public long lRestrictScore; //限制积分
        public long lMinTableScore; //最低积分
        public long lMinEnterScore; //最低积分
        public long lMaxEnterScore; //最高积分

        //会员限制
        byte cbMinEnterMember; //最低会员
        byte cbMaxEnterMember; //最高会员

        //房间属性
        public uint dwServerRule; //房间规则

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_SERVER)] public byte[] szServerName;
        //房间名称
    };


    //////////////////////////////////////////////////////////////////////////////////
    //比赛信息

    //赛事信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagMatchInfo
    {

        //public byte szTitle[4][64];						//信息标题
        //Lin: here change to byte[256]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] szTitle; //信息标题
        public ushort wGameCount; //游戏局数
    };

    //提示信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagMatchWaitTip
    {
        public long lScore; //当前积分
        public ushort wRank; //当前名次
        public ushort wCurTableRank; //本桌名次
        public ushort wUserCount; //当前人数
        public ushort wPlayingTable; //游戏桌数

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_SERVER)] public byte[] szMatchName;
        //比赛名称
    };

    //比赛结果
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagMatchResult
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] szDescribe; //得奖描述
        public uint dwGold; //金币奖励
        public uint dwMedal; //奖牌奖励
        public uint dwExperience; //经验奖励
    };

    //比赛描述
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagMatchDesc
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] szTitle; //信息标题
        //public byte szTitle[4][16];						//信息标题
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] szDescribe; //描述内容
        //public byte szDescribe[4][64];					//描述内容
        public uint crTitleColor; //标题颜色
        public uint crDescribeColor; //描述颜色
    };

    //////////////////////////////////////////////////////////////////////////////////
    //排行榜
    //排行榜项
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagRankItem
    {
        public uint dwUserID; //用户ID
        public long lScore; //用户积分

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME,
            ArraySubType = UnmanagedType.U2)] public ushort[] szNickname; //用户昵称
    };


    /*----------------------------\GameLib\Platform\PFDefine\df\Struct.h --------------------------------------*/

    /*----------------------------\GameLib\Platform\PFDefine\df\Define.h --------------------------------------*/

    public class SocketDefines //对应于\GameLib\Platform\PFDefine\df\Define.h
    {
        //////////////////////////////////////////////////////////////////////////////////
        //数值定义
        //头像大小
        public const int FACE_CX = 48; //头像宽度
        public const int FACE_CY = 48; //头像高度

        //长度定义
        public const int LEN_LESS_ACCOUNTS = 6; //最短帐号
        public const int LEN_LESS_NICKNAME = 6; //最短昵称
        public const int LEN_LESS_PASSWORD = 6; //最短密码

        //人数定义
        public const int MAX_CHAIR = 100; //最大椅子
        public const int MAX_TABLE = 512; //最大桌子
        public const int MAX_COLUMN = 32; //最大列表
        public const int MAX_ANDROID = 256; //最大机器
        public const int MAX_PROPERTY = 128; //最大道具
        public const int MAX_WHISPER_USER = 16; //最大私聊
        public const int MAX_CHAIR_GENERAL = 8; //最大椅子

        //列表定义
        public const int MAX_KIND = 128; //最大类型
        public const int MAX_SERVER = 1024; //最大房间

        //参数定义
        public const int INVALID_CHAIR = 0xFFFF; //无效椅子
        public const int INVALID_TABLE = 0xFFFF; //无效桌子

        //税收定义
        public const int REVENUE_BENCHMARK = 0; //税收起点
        public const int REVENUE_DENOMINATOR = 1000; //税收分母

        //////////////////////////////////////////////////////////////////////////////////
        //系统参数

        //积分类型

        //游戏状态
        public const int GAME_STATUS_FREE = 0; //空闲状态

        // for HideSeek
        public const int GAME_STATUS_HIDE = 1;

        public const int GAME_STATUS_PLAY = 100; //游戏状态
        public const int GAME_STATUS_WAIT = 200; //等待状态
        public const int GAME_STATUS_END  = 201;

        //系统参数
        public const int LEN_USER_CHAT = 128; //聊天长度
        public const int TIME_USER_CHAT = 1; //聊天间隔
        public const int TRUMPET_MAX_CHAR = 128; //喇叭长度

        //////////////////////////////////////////////////////////////////////////////////
        //索引质数

        //列表质数
        public const int PRIME_TYPE = 11; //种类数目
        public const int PRIME_KIND = 53; //类型数目
        public const int PRIME_NODE = 101; //节点数目
        public const int PRIME_PAGE = 53; //自定数目
        public const int PRIME_SERVER = 1009; //房间数目

        //人数质数
        public const int PRIME_SERVER_USER = 503; //房间人数
        public const int PRIME_ANDROID_USER = 503; //机器人数
        public const int PRIME_PLATFORM_USER = 100003; //平台人数

        //////////////////////////////////////////////////////////////////////////////////
        //数据长度

        //资料数据
        public const int LEN_MD5 = 33; //加密密码
        public const int LEN_USERNOTE = 32; //备注长度
        public const int LEN_ACCOUNTS = 32; //帐号长度
        public const int LEN_NICKNAME = 32; //昵称长度
        public const int LEN_PASSWORD = 33; //密码长度
        public const int LEN_GROUP_NAME = 32; //社团名字
        public const int LEN_UNDER_WRITE = 32; //个性签名
        public const int LEN_SIGIN = 5; //签到天数
        public const int LEN_BEGINNER = 32; //新手活动长度
        public const int LEN_ADDRANK = 50; //新手活动长度

        public const int LEN_TOKEN = 512;  //加密字符串长度

        //长度宏定义
        public const int NAME_LEN = 32; //名字长度
        public const int PASS_LEN = 33; //密码长度
        public const int EMAIL_LEN = 32; //邮箱长度
        public const int GROUP_LEN = 32; //社团长度
        public const int COMPUTER_ID_LEN = 33; //机器序列
        public const int UNDER_WRITE_LEN = 32; //个性签名

        //数据长度
        public const int LEN_QQ = 16; //Q Q 号码
        public const int LEN_EMAIL = 33; //电子邮件
        public const int LEN_USER_NOTE = 256; //用户备注
        public const int LEN_SEAT_PHONE = 33; //固定电话
        public const int LEN_MOBILE_PHONE = 12; //移动电话
        public const int LEN_PASS_PORT_ID = 19; //证件号码
        public const int LEN_COMPELLATION = 16; //真实名字
        public const int LEN_DWELLING_PLACE = 128; //联系地址

        //机器标识
        public const int LEN_NETWORK_ID = 13; //网卡长度
        public const int LEN_MACHINE_ID = 33; //序列长度

        //长度宏定义
        public const int LEN_TYPE = 32; //种类长度
        public const int LEN_KIND = 32; //类型长度
        public const int LEN_STATION = 32; //站点长度
        public const int LEN_SERVER = 32; //房间长度
        public const int LEN_MODULE = 32; //进程长度

        public const int MAX_MATCH_DESC = 4;
        //////////////////////////////////////////////////////////////////////////////////

        //用户关系
        public const int CP_NORMAL = 0; //未知关系
        public const int CP_FRIEND = 1; //好友关系
        public const int CP_DETEST = 2; //厌恶关系
        public const int CP_SHIELD = 3; //屏蔽聊天

        //////////////////////////////////////////////////////////////////////////////////

        //性别定义
        public const int GENDER_FEMALE = 0; //女性性别
        public const int GENDER_MANKIND = 1; //男性性别

        //////////////////////////////////////////////////////////////////////////////////

        //游戏模式
        public const int GAME_GENRE_GOLD = 0x0001; //金币类型
        public const int GAME_GENRE_SCORE = 0x0002; //点值类型
        public const int GAME_GENRE_MATCH = 0x0004; //比赛类型
        public const int GAME_GENRE_EDUCATE = 0x0008; //训练类型

        //分数模式
        public const int SCORE_GENRE_NORMAL = 0x0100; //普通模式
        public const int SCORE_GENRE_POSITIVE = 0x0200; //非负模式

        //扣费类型
        public const int MATCH_FEE_TYPE_GOLD = 0x00; //扣费类型
        public const int MATCH_FEE_TYPE_MEDAL = 0x01; //扣费类型

        //比赛类型
        public const int MATCH_TYPE_LOCKTIME = 0x00; //定时类型
        public const int MATCH_TYPE_IMMEDIATE = 0x01; //即时类型

        //////////////////////////////////////////////////////////////////////////////////

        //用户状态
        public const int US_NULL = 0x00; //没有状态
        public const int US_FREE = 0x01; //站立状态
        public const int US_SIT = 0x02; //坐下状态
        public const int US_READY = 0x03; //同意状态
        public const int US_LOOKON = 0x04; //旁观状态
        public const int US_PLAYING = 0x05; //游戏状态
        public const int US_OFFLINE = 0x06; //断线状态

        //////////////////////////////////////////////////////////////////////////////////

        //比赛状态
        public const int MS_NULL = 0x00; //没有状态
        public const int MS_SIGNUP = 0x01; //报名状态
        public const int MS_MATCHING = 0x02; //比赛状态
        public const int MS_OUT = 0x03; //淘汰状态

        //////////////////////////////////////////////////////////////////////////////////

        //房间规则
        public const int SRL_LOOKON = 0x00000001; //旁观标志
        public const int SRL_OFFLINE = 0x00000002; //断线标志
        public const int SRL_SAME_IP = 0x00000004; //同网标志

        //房间规则
        public const int SRL_ROOM_CHAT = 0x00000100; //聊天标志
        public const int SRL_GAME_CHAT = 0x00000200; //聊天标志
        public const int SRL_WISPER_CHAT = 0x00000400; //私聊标志
        public const int SRL_HIDE_USER_INFO = 0x00000800; //隐藏标志

        //////////////////////////////////////////////////////////////////////////////////
        //列表数据

        //无效属性
        public const int UD_NULL = 0; //无效子项
        public const int UD_IMAGE = 100; //图形子项
        public const int UD_CUSTOM = 200; //自定子项

        //基本属性
        public const int UD_GAME_ID = 1; //游戏标识
        public const int UD_USER_ID = 2; //用户标识
        public const int UD_NICKNAME = 3; //用户昵称

        //扩展属性
        public const int UD_GENDER = 10; //用户性别
        public const int UD_GROUP_NAME = 11; //社团名字
        public const int UD_UNDER_WRITE = 12; //个性签名

        //状态信息
        public const int UD_TABLE = 20; //游戏桌号
        public const int UD_CHAIR = 21; //椅子号码

        //积分信息
        public const int UD_SCORE = 30; //用户分数
        public const int UD_GRADE = 31; //用户成绩
        public const int UD_USER_MEDAL = 32; //用户经验
        public const int UD_EXPERIENCE = 33; //用户经验
        public const int UD_LOVELINESS = 34; //用户魅力
        public const int UD_WIN_COUNT = 35; //胜局盘数
        public const int UD_LOST_COUNT = 36; //输局盘数
        public const int UD_DRAW_COUNT = 37; //和局盘数
        public const int UD_FLEE_COUNT = 38; //逃局盘数
        public const int UD_PLAY_COUNT = 39; //总局盘数

        //积分比率
        public const int UD_WIN_RATE = 40; //用户胜率
        public const int UD_LOST_RATE = 41; //用户输率
        public const int UD_DRAW_RATE = 42; //用户和率
        public const int UD_FLEE_RATE = 43; //用户逃率
        public const int UD_GAME_LEVEL = 44; //游戏等级

        //扩展信息
        public const int UD_NOTE_INFO = 50; //用户备注
        public const int UD_LOOKON_USER = 51; //旁观用户

        //图像列表
        public const int UD_IMAGE_FLAG = (UD_IMAGE + 1); //用户标志
        public const int UD_IMAGE_GENDER = (UD_IMAGE + 2); //用户性别
        public const int UD_IMAGE_STATUS = (UD_IMAGE + 3); //用户状态

        //////////////////////////////////////////////////////////////////////////////////
        //数据库定义

        public const int DB_ERROR = -1; //处理失败
        public const int DB_SUCCESS = 0; //处理成功
        public const int DB_NEEDMB = 18; //处理失败

        //////////////////////////////////////////////////////////////////////////////////
        //道具标示
        public const int PT_USE_MARK_DOUBLE_SCORE = 0x0001; //双倍积分
        public const int PT_USE_MARK_FOURE_SCORE = 0x0002; //四倍积分
        public const int PT_USE_MARK_GUARDKICK_CARD = 0x0010; //防踢道具
        public const int PT_USE_MARK_POSSESS = 0x0020; //附身道具 

        public const int MAX_PT_MARK = 4; //标识数目

        //有效范围
        public const int VALID_TIME_DOUBLE_SCORE = 3600; //有效时间
        public const int VALID_TIME_FOUR_SCORE = 3600; //有效时间
        public const int VALID_TIME_GUARDKICK_CARD = 3600; //防踢时间
        public const int VALID_TIME_POSSESS = 3600; //附身时间
        public const int VALID_TIME_KICK_BY_MANAGER = 3600; //游戏时间 

        //////////////////////////////////////////////////////////////////////////////////
        //设备类型
        public const int DEVICE_TYPE_PC = 0x00; //PC
        public const int DEVICE_TYPE_ANDROID = 0x10; //Android
        public const int DEVICE_TYPE_ITOUCH = 0x20; //iTouch
        public const int DEVICE_TYPE_IPHONE = 0x40; //iPhone
        public const int DEVICE_TYPE_IPAD = 0x80; //iPad

        /////////////////////////////////////////////////////////////////////////////////
        //手机定义

        //视图模式
        public const int VIEW_MODE_ALL = 0x0001; //全部可视
        public const int VIEW_MODE_PART = 0x0002; //部分可视

        //信息模式
        public const int VIEW_INFO_LEVEL_1 = 0x0010; //部分信息
        public const int VIEW_INFO_LEVEL_2 = 0x0020; //部分信息
        public const int VIEW_INFO_LEVEL_3 = 0x0040; //部分信息
        public const int VIEW_INFO_LEVEL_4 = 0x0080; //部分信息

        //其他配置
        public const int RECVICE_GAME_CHAT = 0x0100; //接收聊天
        public const int RECVICE_ROOM_CHAT = 0x0200; //接收聊天
        public const int RECVICE_ROOM_WHISPER = 0x0400; //接收私聊

        //行为标识
        public const int BEHAVIOR_LOGON_NORMAL = 0x0000; //普通登录
        public const int BEHAVIOR_LOGON_IMMEDIATELY = 0x1000; //立即登录

        /////////////////////////////////////////////////////////////////////////////////
        //处理结果
        public const int RESULT_ERROR = -1; //处理错误
        public const int RESULT_SUCCESS = 0; //处理成功
        public const int RESULT_FAIL = 1; //处理失败

        /////////////////////////////////////////////////////////////////////////////////
        //变化原因
        public const int SCORE_REASON_WRITE = 0; //写分变化
        public const int SCORE_REASON_INSURE = 1; //银行变化
        public const int SCORE_REASON_PROPERTY = 2; //道具变化
        public const int SCORE_REASON_MATCH_FEE = 3; //比赛报名
        public const int SCORE_REASON_MATCH_QUIT = 4; //比赛退赛

        /////////////////////////////////////////////////////////////////////////////////

        //登录房间失败原因
        public const int LOGON_FAIL_SERVER_INVALIDATION = 200; //房间失效
    }

    /*----------------------------\GameLib\Platform\PFDefine\df\Define.h --------------------------------------*/


    /*----------------------------\GameLib\Platform\PFDefine\msg\CMD_LogonServer.h--------------------------------------*/
    //////////////////////////////////////////////////////////////////////////////////
    //登录命令
    class MsgDefine
    {
        public const int MDM_GP_LOGON = 1; //广场登录

        //登录模式
        public const int SUB_GP_LOGON_GAMEID = 1; //I D 登录
        public const int SUB_GP_LOGON_ACCOUNTS = 2; //帐号登录

        public const int SUB_GP_REGISTER_ACCOUNTS = 3; //注册帐号

        public const int SUB_GP_LOGON_VISITOR = 5; //游客登录

        //登录结果
        public const int SUB_GP_LOGON_SUCCESS = 100; //登录成功
        public const int SUB_GP_LOGON_FAILURE = 101; //登录失败
        public const int SUB_GP_LOGON_FINISH = 102; //登录完成
        public const int SUB_GP_VALIDATE_MBCARD = 103; //登录失败
        public const int SUB_GP_VALIDATE_PASSPORT = 104; //登录失败  
        public const int SUB_GP_VERIFY_RESULT = 105; //验证结果
        public const int SUB_GP_MATCH_SIGNUPINFO = 106; //报名信息
        public const int SUB_GP_GROWLEVEL_CONFIG = 107; //等级配置

        //升级提示
        public const int SUB_GP_UPDATE_NOTIFY = 200; //升级提示


        //////////////////////////////////////////////////////////////////////////////////
        //

        public const int MB_VALIDATE_FLAGS = 0x01; //效验密保
        public const int LOW_VER_VALIDATE_FLAGS = 0x02; //效验低版本

        //////////////////////////////////////////////////////////////////////////////////
        //携带信息 CMD_GP_LogonSuccess

        public const int DTP_GP_GROUP_INFO = 1; //社团信息
        public const int DTP_GP_MEMBER_INFO = 2; //会员信息
        public const int DTP_GP_UNDER_WRITE = 3; //个性签名
        public const int DTP_GP_STATION_URL = 4; //主页信息

        //////////////////////////////////////////////////////////////////////////////////
        //列表命令

        public const int MDM_GP_SERVER_LIST = 2; //列表信息

        //获取命令
        public const int SUB_GP_GET_LIST = 1; //获取列表
        public const int SUB_GP_GET_SERVER = 2; //获取房间
        public const int SUB_GP_GET_ONLINE = 3; //获取在线
        public const int SUB_GP_GET_COLLECTION = 4; //获取收藏


        //列表信息
        public const int SUB_GP_LIST_TYPE = 100; //类型列表
        public const int SUB_GP_LIST_KIND = 101; //种类列表
        public const int SUB_GP_LIST_NODE = 102; //节点列表
        public const int SUB_GP_LIST_PAGE = 103; //定制列表
        public const int SUB_GP_LIST_SERVER = 104; //房间列表
        public const int SUB_GP_LIST_MATCH = 105; //比赛列表
        public const int SUB_GP_VIDEO_OPTION = 106; //视频配置

        //WQ add. for HideSeek
        public const int SUB_GP_LIST_LOBBY = 107; //大厅列表

        //完成信息
        public const int SUB_GP_LIST_FINISH = 200; //发送完成
        public const int SUB_GP_SERVER_FINISH = 201; //房间完成

        //在线信息
        public const int SUB_GR_KINE_ONLINE = 300; //类型在线
        public const int SUB_GR_SERVER_ONLINE = 301; //房间在线
        public const int SUB_GR_ONLINE_FINISH = 302; //在线完成


        //////////////////////////////////////////////////////////////////////////////////
        //服务命令

        public const int MDM_GP_USER_SERVICE = 3; //用户服务

        //账号服务
        public const int SUB_GP_MODIFY_MACHINE = 100; //修改机器
        public const int SUB_GP_MODIFY_LOGON_PASS = 101; //修改密码
        public const int SUB_GP_MODIFY_INSURE_PASS = 102; //修改密码
        public const int SUB_GP_MODIFY_UNDER_WRITE = 103; //修改签名
        public const int SUB_GP_MODIFY_ACCOUNTS = 104; //修改帐号
        public const int SUB_GP_MODIFY_SPREADER = 105; //修改推荐人

        //WQ add
        //代理
        public const int SUB_GP_ADDDEL_SPREADER = 106;                                  //增加/删除推荐人身份
        public const int SUB_GP_QUERY_SPREADERS_INFO = 107;									//查询代理人列表
        //内购
        public const int SUB_GP_ADD_PAYMENT = 115; //游戏内购
        public const int SUB_GP_QUERY_CHILDREN_PAYMENT_INFO = 125; //名下用户交易信息
        public const int SUB_GP_QUERY_PREPAYID = 126; //查询微信PrepayID相关信息
        public const int SUB_GP_UPLOAD_PAY_INFO = 127;//客户端微信支付消息
        //企业提现
        public const int SUB_GP_ADD_ENTERPRISE_PAYMENT = 128;

        // for HideSeek
        public const int SUB_GP_BOUGHT_TAGGER_MODEL = 129;

        public const int SUB_GP_QUERY_NICKNAME = 116; //查询用户昵称
        public const int SUB_GP_NICKNAME_INFO = 117; //查询用户昵称返回命令
        public const int SUB_GP_TRANSFER_DIAMOND =  118; //转房卡
        public const int SUB_GP_TRANSFER_DIAMOND_RESULT =  119; //转房卡返回命令

        //修改头像
        public const int SUB_GP_USER_FACE_INFO = 120; //头像信息
        public const int SUB_GP_SYSTEM_FACE_INFO = 121; //系统头像
        public const int SUB_GP_CUSTOM_FACE_INFO = 122; //自定头像

        //WQ 商品
        public const int SUB_GP_ADDSHOPITEM = 150;            //购买商品
        public const int SUB_GP_ADDSHOPITEM_RESULT = 151;     //购买商品返回命令

        //WQ 钻石金币兑换
        public const int SUB_GP_EXCHANGESCORE = 154;            
        public const int SUB_GP_EXCHANGESCORE_RESULT = 155;    

        //比赛服务
        public const int SUB_GP_MATCH_SIGNUP = 200; //比赛报名
        public const int SUB_GP_MATCH_UNSIGNUP = 201; //取消报名
        public const int SUB_GP_MATCH_SIGNUP_RESULT = 202; //报名结果
        public const int SUB_GP_MATCH_AWARD_LIST = 203; //比赛奖励
        
        //lin : add
        public const int SUB_GP_MATCH_TOP_LIST = 204;           //查询比赛积分排行
        public const int SUB_GP_TOP_PLAYERS_INFO_RESOULT = 205; //比赛积分排行返回命令

        //签到服务
        public const int SUB_GP_CHECKIN_QUERY = 220; //查询签到
        public const int SUB_GP_CHECKIN_INFO = 221; //签到信息
        public const int SUB_GP_CHECKIN_DONE = 222; //执行签到
        public const int SUB_GP_CHECKIN_RESULT = 223; //签到结果
        public const int SUB_GP_CHECKIN_AWARD = 224; //签到奖励

        //WQ add
        //抽奖服务
        public const int SUB_GP_RAFFLE_DONE = 230;
        public const int SUB_GP_RAFFLE_RESULT = 231;

        // for HideSeek
        //大厅聊天
        public const int SUB_GP_LOBBY_CHAT= 232;

        //新手引导
        public const int SUB_GP_BEGINNER_QUERY = 240; //新手引导签到
        public const int SUB_GP_BEGINNER_INFO = 241; //新手引导信息
        public const int SUB_GP_BEGINNER_DONE = 242; //新手引导执行
        public const int SUB_GP_BEGINNER_RESULT = 243; //新手引导结果

        //低保服务
        public const int SUB_GP_BASEENSURE_LOAD = 260; //加载低保
        public const int SUB_GP_BASEENSURE_TAKE = 261; //领取低保
        public const int SUB_GP_BASEENSURE_PARAMETER = 262; //低保参数
        public const int SUB_GP_BASEENSURE_RESULT = 263; //低保结果


        //个人资料
        public const int SUB_GP_USER_INDIVIDUAL = 301; //个人资料
        public const int SUB_GP_QUERY_INDIVIDUAL = 302; //查询信息
        public const int SUB_GP_MODIFY_INDIVIDUAL = 303; //修改资料
        public const int SUB_GP_QUERY_ACCOUNTINFO = 304; //个人信息
        public const int SUB_GP_QUERY_INGAME_SEVERID = 305; //游戏状态

        //银行服务
        public const int SUB_GP_USER_SAVE_SCORE = 400; //存款操作
        public const int SUB_GP_USER_TAKE_SCORE = 401; //取款操作
        public const int SUB_GP_USER_TRANSFER_SCORE = 402; //转账操作
        public const int SUB_GP_USER_INSURE_INFO = 403; //银行资料
        public const int SUB_GP_QUERY_INSURE_INFO = 404; //查询银行
        public const int SUB_GP_USER_INSURE_SUCCESS = 405; //银行成功
        public const int SUB_GP_USER_INSURE_FAILURE = 406; //银行失败
        public const int SUB_GP_QUERY_USER_INFO_REQUEST = 407; //查询用户
        public const int SUB_GP_QUERY_USER_INFO_RESULT = 408; //用户信息


        //自定义字段查询 公告
        public const int SUB_GP_QUERY_PUBLIC_NOTICE = 500; //自定义字段查询
        public const int SUB_GP_PUBLIC_NOTICE = 501;

        //设置推荐人结果
        public const int SUB_GP_SPREADER_RESOULT = 520; //设置推荐人结果


        //WQ add
        //查询代理人列表结果
        public const int SUB_GP_SPREADERS_INFO_RESOULT = 521;

        //WQ add
        //内购
        public const int SUB_GP_ADD_PAYMENT_RESULT = 530;
        //名下用户交易信息
        public const int SUB_GP_CHILDREN_PAYMENT_INFO_RESULT = 531;
        //企业提现
        public const int SUB_GP_ADD_ENTERPRISE_PAYMENT_RESULT = 532;

        // for HideSeek
        public const int SUB_GP_BOUGHT_TAGGER_MODEL_RESULT = 533;

        //赚金排行榜
        public const int SUB_GP_ADDRANK_GET_AWARD_INFO = 540; //获得奖励信息
        public const int SUB_GP_ADDRANK_BACK_AWARD_INFO = 541; //返回奖励信息
        public const int SUB_GP_ADDRANK_GET_RANK = 542; //获得排行
        public const int SUB_GP_ADDRANK_BACK_RANK = 543; //返回排行

        //游戏记录
        public const int SUB_GP_GAME_RECORD_LIST = 550;
        public const int SUB_GP_GAME_RECORD_TOTAL = 551;

        //客服信息
        public const int SUB_GP_KEFU = 600; //客服信息
        public const int SUB_GP_KEFU_RESULT = 601; //客服信息结果

        //邮件信息
        public const int SUB_GP_MESSAGE_LIST = 700; //邮件列表
        public const int SUB_GP_MESSAGE_LIST_RESULT = 701; //邮件列表返回
        public const int SUB_GP_MESSAGE_AWARD = 702; //获取排行榜奖励
        public const int SUB_GP_MESSAGE_AWARD_RESULT = 703; //获取排行榜结果

        //操作结果
        public const int SUB_GP_OPERATE_SUCCESS = 900; //操作成功
        public const int SUB_GP_OPERATE_FAILURE = 901; //操作失败

        //话费兑换
        public const int SUB_GP_EXCHANGEHUAFEI_GET_LIST_INFO = 1000; //获取兑换话费列表
        public const int SUB_GP_EXCHANGEHUAFEI_BACK = 1001; //兑换话费列表返回

        //商城数据
        public const int SUB_GP_SHOPINFO_GET_LIST_INFO = 1100; //获取商城列表
        public const int SUB_GP_SHOPINFO_BACK = 1101; //商城列表返回

        //////////////////////////////////////////////////////////////////////////////////
        //携带信息 CMD_GP_UserIndividual

        public const int DTP_GP_UI_NICKNAME = 1; //用户昵称
        public const int DTP_GP_UI_USER_NOTE = 2; //用户说明
        public const int DTP_GP_UI_UNDER_WRITE = 3; //个性签名
        public const int DTP_GP_UI_QQ = 4; //Q Q 号码
        public const int DTP_GP_UI_EMAIL = 5; //电子邮件
        public const int DTP_GP_UI_SEAT_PHONE = 6; //固定电话
        public const int DTP_GP_UI_MOBILE_PHONE = 7; //移动电话
        public const int DTP_GP_UI_COMPELLATION = 8; //真实名字
        public const int DTP_GP_UI_DWELLING_PLACE = 9; //联系地址
        public const int DTP_GP_UI_HEAD_HTTP = 10; //头像
        public const int DTP_GP_UI_IP = 11; //IP
        public const int DTP_GP_UI_CHANNEL = 12; //渠道号


        //////////////////////////////////////////////////////////////////////////////////
        //远程服务

        public const int MDM_GP_REMOTE_SERVICE = 4; //远程服务

        //查找服务
        public const int SUB_GP_C_SEARCH_DATABASE = 100; //数据查找
        public const int SUB_GP_C_SEARCH_CORRESPOND = 101; //协调查找

        //时间奖励
        public const int SUB_GP_C_TIME_AWARD_CHECK = 110; //时间奖励信息查询
        public const int SUB_GP_C_TIME_AWARD_GET = 111; //时间奖励领取

        //查找服务
        public const int SUB_GP_S_SEARCH_DATABASE = 200; //数据查找
        public const int SUB_GP_S_SEARCH_CORRESPOND = 201; //协调查找


        //时间奖励
        public const int SUB_GP_S_TIME_AWARD_CHECK = 210; //时间奖励信息查询结果
        public const int SUB_GP_S_TIME_AWARD_GET = 211; //时间奖励领取结果



        //登录模式
        public const int SUB_MB_LOGON_GAMEID = 2; //I D 登录
        public const int SUB_MB_REGISTER_ACCOUNTS = 3; //注册帐号

        //登录结果
        public const int SUB_MB_LOGON_SUCCESS = 100; //登录成功
        public const int SUB_MB_LOGON_FAILURE = 101; //登录失败

        //升级提示
        public const int SUB_MB_UPDATE_NOTIFY = 200; //升级提示

        //////////////////////////////////////////////////////////////////////////////////
        //列表命令

        public const int MDM_MB_SERVER_LIST = 101; //列表信息

        //列表信息
        public const int SUB_MB_LIST_TYPE = 100; //游戏类型
        public const int SUB_MB_LIST_KIND = 101; //种类列表
        public const int SUB_MB_LIST_NODE = 102; //节点列表
        public const int SUB_MB_LIST_PAGE = 103; //定制列表
        public const int SUB_MB_LIST_SERVER = 104; //房间列表
        public const int SUB_MB_LIST_MATCH = 105; //比赛列表
        public const int SUB_MB_LIST_FINISH = 200; //列表完成
    }

    //获取在线
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_GetOnline
    {
        public ushort wServerCount; //房间数目
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.MAX_SERVER, ArraySubType = UnmanagedType.U2)] public ushort[] wOnLineServerID; //房间标识
    };


    //类型在线
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_KindOnline
    {
        public ushort wKindCount; //类型数目
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.MAX_KIND, ArraySubType = UnmanagedType.Struct)] public tagOnLineInfoKind[] OnLineInfoKind; //类型在线
    };

    //房间在线
    struct CMD_GP_ServerOnline
    {
        public ushort wServerCount; //房间数目
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.MAX_SERVER, ArraySubType = UnmanagedType.Struct)] public tagOnLineInfoServer[] OnLineInfoServer; //房间在线
    };


    //修改密码
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_ModifyLogonPass
    {
        public uint dwUserID; //用户 I D

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szDesPassword;
        //用户密码

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szScrPassword;
        //用户密码
    };

    //修改密码
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_ModifyInsurePass
    {
        public uint dwUserID; //用户 I D

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szDesPassword;
        //用户密码

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szScrPassword;
        //用户密码
    };


    //修改推荐人
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_ModifySpreader
    {
        public uint dwUserID; //用户 I D

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)]
        public byte[] szPassword; //用户密码

        public uint dwSpreaderID;//推荐人ID
        ///[MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szSpreader; 

        public void Init()
        {
            szPassword = new byte[SocketDefines.LEN_PASSWORD];
        }
    };

    //WQ add
    //增加/删除推荐人身份
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_AddDelSpreader
    {
        public uint dwUserID;                     //用户 I D

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)]
        public byte[] szPassword;           //用户密码

        public uint dwSpreaderID;                 //推荐人ID

        public uint dwParentSpreaderID;           //上级代理ID

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] szSpreaderRealName;           //推荐人姓名

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        //public byte[] szSpreaderIDCardNo;           //推荐人身份证号

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] szSpreaderTelNum;           

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] szSpreaderWeiXinAccount;

        public ushort wSpreaderLevel;                    //推荐人等级

        public byte bIsAdd;                            //增加/删除？

        public void Init()
        {
            szPassword = new byte[SocketDefines.LEN_PASSWORD];
            szSpreaderRealName = new byte[32];
            ///szSpreaderIDCardNo = new byte[32];
            szSpreaderTelNum = new byte[32];
            szSpreaderWeiXinAccount = new byte[32];
        }
    };
    //查询代理人列表
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_QuerySpreadersInfo
    {
        public uint dwUserID;                     //用户 I D

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)]
        public byte[] szPassword;           //用户密码

        public void Init()
        {
            szPassword = new byte[SocketDefines.LEN_PASSWORD];
        }
    };

    //WQ add
    //内购
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_AddPayment
    {
        public uint dwUserID;                     //用户 I D

        public uint dwPayment;         //购买金额（元）
        public uint dwBoughtDiamond;     //购买到的钻石数

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)]
        public byte[] szPassword;           //用户密码

        public void Init()
        {
            szPassword = new byte[SocketDefines.LEN_PASSWORD];
        }
    };
    //名下用户交易信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_QueryChildrenPaymentInfo
    {
        public uint dwUserID;                     //用户 I D

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)]
        public byte[] szPassword;           //用户密码

        public void Init()
        {
            szPassword = new byte[SocketDefines.LEN_PASSWORD];
        }
    };
    //企业提现
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_AddEnterprisePayment
    {
        public uint dwUserID;                     //用户 I D

        public uint dwEnterprisePayment;         //提现金额（分）

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)]
        public byte[] szPassword;           //用户密码

        public void Init()
        {
            szPassword = new byte[SocketDefines.LEN_PASSWORD];
        }
    };

    // for HideSeek
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_BoughtTaggerModel
    {
        public uint dwUserID;               //用户 I D

        public uint dwPayment;              //付款金额
        public byte cbPaymentType;          //付款类型：0-金币，1-钻石，2-现金
        public ushort wBoughtModelIndex;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)]
        public byte[] szPassword;           //用户密码

        public void Init()
        {
            szPassword = new byte[SocketDefines.LEN_PASSWORD];
        }
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_QueryPrePayID
    {
        public uint dwUserID;                          //用户ID
        public uint dwShopItemID;                     //商品 I D
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GP_PrePayIDInfo
    {
        public byte cbStatusCode;					//获取成功与否的信息（1： 成功，0： 失败）
        //微信支付返回的PrePayID (wx2017082214271068e0d93b9d0885872954这是测试返回的id，大于32为位，暂为64，
        //微信调起支付接口文档中写类型为String（32）,而统一下单接口返回的文档中PrePayID的类型为String（64）)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] szPrePayID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] szNonceStr;                   //微信支付下单用的随机字符串
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] szTimeStamp;              //微信支付下单用的时间戳
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] szSign;                       //微信支付下单用的签名字段
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] szTradeNo;						//微信支付下单用的签名字段
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_ClientPayInfoResoult
    {
        public byte cbSuccessState;                    //支付状态（成功or失败）
        public uint dwInsureGold;                 //支付之后钻石数
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] szMsg;						//支付返回信息
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_ClientPayInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] szTradeNo;             //微信支付  (商户)订单号、、客户端拿不到微信订单号，只能拿到成功消息
        public uint dwUserID;                     //用户ID
        public byte cbSuccessState;                    //支付状态（成功or失败）
    };

    //查询比赛积分排行列表
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GR_QueryTopNum
    {
        public uint dwTopCount;                     //查询排行前多少的玩家
    };

    //查询昵称
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GR_QueryNickName
    {
        public uint dwUserID;                     //查询玩家的ID
    };

    //转房卡（钻石）
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GR_TransferDiamond
    {
        public uint dwLocalID;                    //玩家ID
        public uint dwUserID;                     //被转玩家的ID
        public uint dwDiamondNum;                 //房卡数量
    };

    //商品信息 WQ
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GP_ShopItemInfo
    {
        public uint dwUserID;                  //用户标识
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ADDRANK)]
        public byte[] szUID;                    //用户UID（SDK）
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_USERNOTE)]
        public byte[] szOrderID;                  //订单号
        public ushort wItemID;                  //商品ID
        public ushort wAmount;                  //商品金额
        public ushort wCount;                   //商品个数

        public void Init()
        {
            szUID = new byte[SocketDefines.LEN_ADDRANK];
            szOrderID = new byte[SocketDefines.LEN_USERNOTE];
        }
    };

    //购买商品信息反馈
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_ShopItemInfoResult
    {
        public byte cbSuccess;                          //购买成功
        public ushort wItemID;                           //商品ID
        public uint dwFinalInsureScore;                   //充值后拥有钻石
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] szDescribeString;                    //描述

        public void Init()
        {
            szDescribeString = new byte[256];
        }
    };
    //钻石金币兑换 WQ
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GP_ExchangScoreInfo
    {
        public uint dwUserID;                   //用户标识
        public byte cbItemID;                   //商品ID
        public ushort wAmount;                  //商品金额
        public byte cbExchangeType;             //消费类型   0-金币转钻石，1-钻石转金币
    };

    //钻石金币兑换反馈
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_ExchangScoreInfoResult
    {
        public byte cbSuccess;                          //购买成功
        public byte cbItemID;                           //商品ID
        public uint dwFinalScore;                       //当前金币
        public uint dwFinalInsure;                      //当前钻石
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] szDescribeString;                 //描述

        public void Init()
        {
            szDescribeString = new byte[256];
        }
    };

    //修改头像
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_CustomFaceInfo
    {
        public uint dwUserID; //用户 I D
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szPassword; //用户密码

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MACHINE_ID)] public byte[] szMachineID;
        //机器序列

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.FACE_CX*SocketDefines.FACE_CY,
            ArraySubType = UnmanagedType.U4)] public uint[] dwCustomFace; //图片信息
    };


    //////////////////////////////////////////////////////////////////////////////////
    //银行资料
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_UserInsureInfo
    {
        public ushort wRevenueTake; //税收比例
        public ushort wRevenueTransfer; //税收比例
        public ushort wServerID; //房间标识
        public long lUserScore; //用户金币
        public long lUserInsure; //银行金币
        public long lTransferPrerequisite; //转账条件
    };

    //存入金币
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_UserSaveScore
    {
        public uint dwUserID; //用户 I D
        public long lSaveScore; //存入金币

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MACHINE_ID)] public byte[] szMachineID;
        //机器序列
    };

    //提取金币
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_UserTakeScore
    {
        public uint dwUserID; //用户 I D
        public long lTakeScore; //提取金币
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MD5)] public byte[] szPassword; //银行密码

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MACHINE_ID)] public byte[] szMachineID;
        //机器序列
    };

    //转账金币
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_UserTransferScore
    {
        public uint dwUserID; //用户 I D
        public byte cbByNickName; //昵称赠送
        public long lTransferScore; //转账金币
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MD5)] public byte[] szPassword; //银行密码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)] public byte[] szNickName; //目标用户

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MACHINE_ID)] public byte[] szMachineID;
        //机器序列
    };

    //银行成功
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_UserInsureSuccess
    {
        public uint dwUserID; //用户 I D
        public long lUserScore; //用户金币
        public long lUserInsure; //银行金币
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] szDescribeString; //描述消息
    };

    //银行失败
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_UserInsureFailure
    {
        public uint lResultCode; //错误代码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] szDescribeString; //描述消息
    };

    //提取结果
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_UserTakeResult
    {
        public uint dwUserID; //用户 I D
        public long lUserScore; //用户金币
        public long lUserInsure; //银行金币
    };

    //查询银行
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_QueryInsureInfo
    {
        public uint dwUserID; //用户 I D
    };

    //查询用户
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_QueryUserInfoRequest
    {
        public byte cbByNickName; //昵称赠送
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)] public byte[] szNickName; //目标用户
    };

    //用户信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GP_UserTransferUserInfo
    {
        public uint dwTargetGameID; //目标用户
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)] public byte[] szNickName; //目标用户
    };

    //个人信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GP_UserAccountInfo
    {
        //属性资料
        public ushort wFaceID; //头像标识
        public uint dwUserID; //用户标识
        public uint dwGameID; //游戏标识
        public uint dwGroupID; //社团标识
        public uint dwCustomID; //自定索引
        public uint dwUserMedal; //用户奖牌
        public uint dwExperience; //经验数值
        public uint dwLoveLiness; //用户魅力
        public uint dwSpreaderID; //推广ID
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MD5)] public byte[] szPassword; //登录密码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ACCOUNTS)] public byte[] szAccounts; //登录帐号
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)] public byte[] szNickName; //用户昵称

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_GROUP_NAME)] public byte[] szGroupName;
        //社团名字

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ACCOUNTS)] public byte[] szLogonIp; //登录IP

        //用户成绩
        public long lUserScore; //用户游戏币
        public long lUserInsure; //用户银行

        //用户资料
        public byte cbGender; //用户性别
        public byte cbMoorMachine; //锁定机器

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_UNDER_WRITE)] public byte[] szUnderWrite;
        //个性签名

        //会员资料
        public byte cbMemberOrder; //会员等级
        public systemtime MemberOverDate; //到期时间
    };

    //游戏状态
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GP_UserInGameServerID
    {
        public uint dwUserID; //用户 I D
    };

    //操作成功
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GP_InGameSeverID
    {
        public uint LockKindID;
        public uint LockServerID;
    };

    //////////////////////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////////////////////
    //比赛报名
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_MatchSignup
    {
        //比赛信息
        public ushort wServerID; //房间标识
        public uint dwMatchID; //比赛标识
        public uint dwMatchNO; //比赛场次

        //用户信息
        public uint dwUserID; //用户标识
        [MarshalAs(UnmanagedType.Struct, SizeConst = SocketDefines.LEN_MD5)] public byte[] szPassword; //登录密码

        //机器信息
        [MarshalAs(UnmanagedType.Struct, SizeConst = SocketDefines.LEN_MACHINE_ID)] public byte[] szMachineID; //机器序列
    };

    //取消报名
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_MatchUnSignup
    {
        //比赛信息
        public ushort wServerID; //房间标识
        public uint dwMatchID; //比赛标识
        public uint dwMatchNO; //比赛场次

        //用户信息
        public uint dwUserID; //用户标识
        [MarshalAs(UnmanagedType.Struct, SizeConst = SocketDefines.LEN_MD5)] public byte[] szPassword; //登录密码

        //机器信息
        [MarshalAs(UnmanagedType.Struct, SizeConst = SocketDefines.LEN_MACHINE_ID)] public byte[] szMachineID; //机器序列
    };

    //报名结果
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_MatchSignupResult
    {
        public byte bSignup; //报名标识
        public byte bSuccessed; //成功标识

        //WQ add
        public systemtime MatchSignupStartTime;
        public systemtime MatchSignupEndTime;
        public systemtime MatchStartTime;
        public systemtime MatchEndTime;
        [MarshalAs(UnmanagedType.Struct, SizeConst = 128)] public byte[] szDescribeString; //描述信息
    };

    //报名结果
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_MatchGetAward
    {
        public uint dwUserID;
        public uint dwMatchID; //比赛标识
        public uint dwMatchNO; //比赛场次	
    };

#if !TEMP_REMOVE
    //排行信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagMatchAwardkInfo
     {
        public ushort MatchRank;                         //比赛名次
        public long RewardGold;                           //奖励金币
        public uint RewardMedal;                      //奖励元宝
        public uint RewardExperience;                 //奖励经验
         void StreamValue(datastream& kData, bool bSend)
         {
             Stream_VALUE(MatchRank);
             Stream_VALUE(RewardGold);
             Stream_VALUE(RewardMedal);
             Stream_VALUE(RewardExperience);
         }
     };
    //比赛奖励
   
    class CMD_GR_MatchAwardList
     {
        public uint dwMatchID;                            //比赛标识
        public uint dwMatchNO;                            //比赛场次
        public List<tagMatchAwardkInfo> kAwards;

        public void StreamValue(datastream& kData, bool bSend)
         {
             Stream_VALUE(dwMatchID);
             Stream_VALUE(dwMatchNO);
             StructVecotrMember(tagMatchAwardkInfo, kAwards);
         }
     };
#endif
    //////////////////////////////////////////////////////////////////////////////////
    //游戏记录
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_GetGameRecordList
    {
        public uint dwUserID;
    };

    //游戏记录
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_GetGameTotalRecord
    {
        public uint dwUserID;
        public uint dwRecordID;
    };

#if !TEMP_REMOVE
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagPrivateRandRecordChild
     {
        public uint dwKindID;
        public uint dwVersion;
        public int iRecordID;
        public int iRecordChildID;
        public std::vector<int> kScore;
        public systemtime kPlayTime;
        public datastream kRecordGame;
        public std::string kUserDefine;
         tagPrivateRandRecordChild()
         {
             iRecordID = 0;
             iRecordChildID = 0;
         }
         void StreamValue(datastream& kData, bool bSend)
         {
             Stream_VALUE(iRecordID);
             Stream_VALUE(iRecordChildID);
             Stream_VALUE(kScore);
             Stream_VALUE(kRecordGame);
             Stream_VALUE_SYSTEMTIME(kPlayTime);
             Stream_VALUE(kUserDefine);
             Stream_VALUE(dwKindID);
             Stream_VALUE(dwVersion);
         }
     };

     //用户一轮总输赢
     struct tagPrivateRandTotalRecord
     {
         tagPrivateRandTotalRecord()
         {
             iRoomNum = 0;
             iRecordID = 0;
         }
         uint dwKindID;
         uint dwVersion;
         int iRoomNum;
         int iRecordID;
         std::vector<int> kScore;
         std::vector<int> kUserID;
         std::vector<std::string> kNickName;
         systemtime kPlayTime;
         std::string kUserDefine;

         std::vector<tagPrivateRandRecordChild> kRecordChild;

         void StreamValue(datastream& kData, bool bSend)
         {
             Stream_VALUE(iRoomNum);
             Stream_VALUE(kUserID);
             Stream_VALUE(kNickName);
             Stream_VALUE(kScore);
             Stream_VALUE_SYSTEMTIME(kPlayTime);
             StructVecotrMember(tagPrivateRandRecordChild, kRecordChild);
             Stream_VALUE(iRecordID);
             Stream_VALUE(kUserDefine);
             Stream_VALUE(dwKindID);
             Stream_VALUE(dwVersion);
         }
     };
     struct tagPrivateRandTotalRecordList
     {
         uint dwUserID;
         std::vector<tagPrivateRandTotalRecord> kList;

         void StreamValue(datastream& kData, bool bSend)
         {
             Stream_VALUE(dwUserID);
             StructVecotrMember(tagPrivateRandTotalRecord, kList);
         }
     };
#endif

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_BackAddBankAwardInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ADDRANK*3, ArraySubType = UnmanagedType.I4)]
        //int kRewardGold[3][LEN_ADDRANK];
        public int[] kRewardGold;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ADDRANK*3, ArraySubType = UnmanagedType.I4)] public int[] kRewardType;
        //int kRewardType[3][LEN_ADDRANK];				
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_GetAddBank
    {
        public uint dwUserID; //用户 I D
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szPassword; //用户密码
        public int iRankIdex;
    };

#if !TEMP_REMOVE
    struct CMD_GP_BackAddBank
     {
         int iRankIdex;
         std::vector<std::string> kNickName;
         std::vector<int> kUserID;
         std::vector<int> kFaceID;
         std::vector<int> kCustomID;
         std::vector<int> kUserPoint;

         void StreamValue(datastream& kData, bool bSend)
         {
             Stream_VALUE(iRankIdex);
             Stream_VALUE(kNickName);
             Stream_VALUE(kUserID);
             Stream_VALUE(kFaceID);
             Stream_VALUE(kCustomID);
             Stream_VALUE(kUserPoint);
         }
     };

     struct CMD_GP_GetExchangeHuaFei
     {
         uint dwUserID;                     //用户 I D
         byte szPassword[LEN_PASSWORD];          //用户密码
     };

     struct CMD_GP_BackExchangeHuaFei
     {
         std::vector<int> kExchangeID;            //兑换id
         std::vector<int> kUseType;             //兑换道具类型
         std::vector<int> kUseNum;                  //兑换道具个数
         std::vector<int> kGetType;             //兑换商品类型
         std::vector<int> kGetNum;                  //兑换商品个数
         std::vector<std::string> kGoodsName;               //兑换商品名称
         std::vector<std::string> kExchangeDesc;        //兑换描述
         std::vector<std::string> kImgIcon;             //图标名称
         std::vector<int> kFlag;                //标记

         void StreamValue(datastream& kData, bool bSend)
         {
             Stream_VALUE(kExchangeID);
             Stream_VALUE(kUseType);
             Stream_VALUE(kUseNum);
             Stream_VALUE(kGetType);
             Stream_VALUE(kGetNum);
             Stream_VALUE(kGoodsName);
             Stream_VALUE(kExchangeDesc);
             Stream_VALUE(kImgIcon);
             Stream_VALUE(kFlag);
         }
     };
     struct CMD_GP_GetShopInfo
     {
         uint dwUserID;                     //用户 I D
         byte szPassword[LEN_PASSWORD];          //用户密码
     };

     struct CMD_GP_BackShopInfo
     {
         std::vector<int> kItemID;                    //商品id
         std::vector<int> kItemType;              //商品类型
         std::vector<int> kOrderID_IOS;               //商品订单号 苹果
         std::vector<int> kOrderID_Android;           //商品订单号 安卓
         std::vector<int> kPrice;                     //商品价格
         std::vector<int> kGoodsNum;              //商品数量
         std::vector<std::string> kItemTitle;                 //标题
         std::vector<std::string> kItemDesc;              //描述
         std::vector<std::string> kItemIcon;              //图标名称
         std::vector<std::string> kItemName;              //商品名称

         void StreamValue(datastream& kData, bool bSend)
         {
             Stream_VALUE(kItemID);
             Stream_VALUE(kItemType);
             Stream_VALUE(kOrderID_IOS);
             Stream_VALUE(kOrderID_Android);
             Stream_VALUE(kPrice);
             Stream_VALUE(kGoodsNum);
             Stream_VALUE(kItemTitle);
             Stream_VALUE(kItemDesc);
             Stream_VALUE(kItemIcon);
             Stream_VALUE(kItemName);
         }
     };
#endif

    //////////////////////////////////////////////////////////////////////////////////

    //查询信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_QueryIndividual
    {
        public uint dwUserID; //用户 I D
    };

    //查询信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_QueryAccountInfo
    {
        public uint dwUserID; //用户 I D
    };

    //个人资料
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_UserIndividual
    {
        public uint dwUserID; //用户 I D
    };
    
    //修改资料
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_ModifyIndividual
    {
        public byte cbGender; //用户性别
        public uint dwUserID; //用户 I D
        public ushort wModCost;//修改花费
        public byte cbModCosttType;//花费类型：0-金币，1-钻石
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szPassword; //用户密码
    };

    //操作成功
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GP_SpreaderResoult
    {
        //WQ add
        public byte cbOperateType;					//操作类型：0-绑定代理，1-增加代理，2-删除代理
        public uint dwBindSpreaderId;               //绑定操作：绑定的代理ID

        public uint lResultCode; //操作代码
        public long lScore;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] szDescribeString; //成功消息
    };

    //WQ add
    //查询代理人
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SpreaderInfoItem
    {
        public uint SpreaderId;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] RealName;
        ///char RealName[32];

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        //public byte[] IDCardNo;
        ///char IDCardNo[32];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] TelNum;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] WeiXinAccount;

        public ushort SpreaderLevel;
        public uint ParentID;

        public void Init()
        {
            RealName = new byte[32];
            //IDCardNo = new byte[32];
            TelNum = new byte[32];
            WeiXinAccount = new byte[32];
        }

        public void StreamValue(byte[] kData, int offset, int dataSize)
        {
            //int offset = 0;

            SpreaderId = System.BitConverter.ToUInt32(kData, offset);
            offset += sizeof(uint);

            RealName = new byte[32];
            Buffer.BlockCopy(kData, offset, RealName, 0, 32);
            offset += 32;
            string str = Encoding.Default.GetString(RealName);

            //IDCardNo = new byte[32];
            //Buffer.BlockCopy(kData, offset, IDCardNo, 0, 32);
            //offset += 32;
            //str = Encoding.Default.GetString(IDCardNo);

            TelNum = new byte[32];
            Buffer.BlockCopy(kData, offset, TelNum, 0, 32);
            offset += 32;
            str = Encoding.Default.GetString(TelNum);

            WeiXinAccount = new byte[32];
            Buffer.BlockCopy(kData, offset, WeiXinAccount, 0, 32);
            offset += 32;
            str = Encoding.Default.GetString(WeiXinAccount);

            SpreaderLevel = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            ParentID = System.BitConverter.ToUInt32(kData, offset);
            offset += sizeof(uint);

            //return offset;
        }
    };
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_SpreadersInfoResoult
    {
        public uint lResultCode;                       //操作代码

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] szDescribeString;
        ///char szDescribeString[64];              //成功消息

        public ushort wPacketIdx;                            //当前包序号
        public ushort wPacketNum;							//分包发送的总包数

        public ushort wItemCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GlobalUserInfo.Max_Spreaders_Num_PerSend)]
        public SpreaderInfoItem[] SpreaderInfoItems;//代理人列表
        ///SpreaderInfoItem SpreaderInfoItems[Max_Spreaders_Num_PerSend];//代理人列表

        //public void Init()
        //{
        //    szDescribeString = new byte[64];

        //    SpreaderInfoItems = new SpreaderInfoItem[GlobalUserInfo.Max_Spreaders_Num_PerSend];
        //    for (int i = 0; i < SpreaderInfoItems.Length; i++)
        //    {
        //        SpreaderInfoItems[i].Init();
        //    }
        //}

        public void StreamValue(byte[] kData, int dataSize)
        {
            int offset = 0;

            lResultCode = System.BitConverter.ToUInt32(kData, offset);
            offset += sizeof(uint);

            szDescribeString = new byte[64];
            Buffer.BlockCopy(kData, offset, szDescribeString, 0, 64);
            offset += 64;
            string str = Encoding.Default.GetString(szDescribeString);

            wPacketIdx = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            wPacketNum = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            wItemCount = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            SpreaderInfoItems = new SpreaderInfoItem[GlobalUserInfo.Max_Spreaders_Num_PerSend];
            //for (int i = 0; i < SpreaderInfoItems.Length; i++)
            //{
            //    SpreaderInfoItems[i].Init();
            //}

            int i = 0;
            //Buffer.BlockCopy(kData, offset, SpreaderInfoItems, 0, dataSize-offset);
            int SpreaderInfoItemSize = Marshal.SizeOf(typeof(SpreaderInfoItem));
            while ( offset <= dataSize - SpreaderInfoItemSize)
            {
                SpreaderInfoItems[i].StreamValue(kData, offset, SpreaderInfoItemSize);
                offset += SpreaderInfoItemSize;
                i++;
            }
        }
    };

    //WQ add
    //内购
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_AddPaymentResult
    {
        public uint lResultCode;                       //操作代码
        public uint dwFinalInsureScore;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] szDescribeString;             //成功消息               
    };
    //名下用户交易信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PaymentInfoItem
    {
        public uint UserId;
        public uint Payment;
        public double PaymentGrantRate;

        public void StreamValue(byte[] kData, int offset, int dataSize)
        {
            UserId = System.BitConverter.ToUInt32(kData, offset);
            offset += sizeof(uint);

            Payment = System.BitConverter.ToUInt32(kData, offset);
            offset += sizeof(uint);

            PaymentGrantRate = System.BitConverter.ToDouble(kData, offset);
            offset += sizeof(double);
        }
     };
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_ChildrenPaymentInfoResult
    {
        public uint lResultCode;                       //操作代码

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] szDescribeString;

        public double dTotalGrantOfChildrenBuy;			    //名下用户充值获得的提成总额（元）
        public double dExtraCash;							//获得的额外金额(元)：比如删除直属下级代理获得对方的剩余金额
        public double dCashedOut;                           //已提现金额（元）
        public double dTotalLeftCash;						//剩余金额（元）

        public ushort wPacketIdx;                            //当前包序号
        public ushort wPacketNum;							//分包发送的总包数

        public ushort wItemCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GlobalUserInfo.Max_PaymentInfo_Num_PerSend)]
        public PaymentInfoItem[] PaymentInfoItems;          //交易信息

        public void StreamValue(byte[] kData, int dataSize)
        {
            int offset = 0;

            lResultCode = System.BitConverter.ToUInt32(kData, offset);
            offset += sizeof(uint);

            szDescribeString = new byte[64];
            Buffer.BlockCopy(kData, offset, szDescribeString, 0, 64);
            offset += 64;
            string str = Encoding.Default.GetString(szDescribeString);

            dTotalGrantOfChildrenBuy = System.BitConverter.ToDouble(kData, offset);
            offset += sizeof(double);

            dExtraCash = System.BitConverter.ToDouble(kData, offset);
            offset += sizeof(double);

            dCashedOut = System.BitConverter.ToDouble(kData, offset);
            offset += sizeof(double);

            dTotalLeftCash = System.BitConverter.ToDouble(kData, offset);
            offset += sizeof(double);

            wPacketIdx = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            wPacketNum = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            wItemCount = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            PaymentInfoItems = new PaymentInfoItem[GlobalUserInfo.Max_PaymentInfo_Num_PerSend];

            int i = 0;
            int PaymentInfoItemSize = Marshal.SizeOf(typeof(PaymentInfoItem));
            while (offset <= dataSize - PaymentInfoItemSize)
            {
                //byte[] pInfoItem = new byte[PaymentInfoItemSize];
                //Buffer.BlockCopy(kData, offset, pInfoItem, 0, PaymentInfoItemSize);
                //PaymentInfoItem pPaymentInfoItem = (tagGameKind)StructConverterByteArray.BytesToStruct(pInfoItem, typeof(PaymentInfoItem));

                PaymentInfoItems[i].StreamValue(kData, offset, PaymentInfoItemSize);
                offset += PaymentInfoItemSize;
                i++;
            }
        }
    };
    //企业提现
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_AddEnterprisePaymentResult
    {
        public uint dwUserID;                           //用户标识

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)]
        public byte[] szRealName;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ACCOUNTS)]
        public byte[] szOpenid;

        public uint lResultCode;                        //操作代码
        public uint dwEnterprisePayment;                //提现金额（分）
        public uint dwTotalGrant;                       //总金额（分）

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] szDescribeString;             //成功消息               
    };

    // for HideSeek
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_BoughtTaggerModelResult
    {
        public uint lResultCode;                       //操作代码

        public long lUserScore;                         //用户金币
        public long lUserInsure;                        //用户银行
        public ushort wBoughtModelIndex;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] szDescribeString;             //成功消息               
    };

    //lin add
    //查询比赛积分排行
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TopPlayersInfoItem
    {

        public uint dwUserID;                         //用户标识
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)]
        public byte[] strNickName;                  //用户昵称
        public int iExperience;                      //用户经验
        //public long lTotalScore;                      //所有比赛积分
        //public ushort wWinCount;                         //胜利场次
        //public ushort wDrawCount;                            //平局场次
        //public ushort wLoseCount;							//失败场次
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_TopPlayersInfoResoult
    {
        public uint lResultCode;                        //操作代码

        public ushort wItemCount;                       //排行人数

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GlobalUserInfo.Max_TopPlayers_Num)]
        public TopPlayersInfoItem[] TopPlayersInfoItems;//排行榜列表

        public void StreamValue(byte[] kData, int dataSize)
        {
            int offset = 0;

            lResultCode = System.BitConverter.ToUInt32(kData, offset);
            offset += sizeof(uint);

            wItemCount = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            TopPlayersInfoItems = new TopPlayersInfoItem[wItemCount];

            byte[] tempBuf;
            var typeITem = typeof (TopPlayersInfoItem);
            int ItemSize = Marshal.SizeOf(typeITem);
            for (int j = 0; j < wItemCount; j++)
            {
                tempBuf = new byte[ItemSize];
                Buffer.BlockCopy(kData, offset, tempBuf, 0, ItemSize);
                TopPlayersInfoItems[j] = (TopPlayersInfoItem)StructConverterByteArray.BytesToStruct(tempBuf, typeITem);
                offset += ItemSize;
            }
        }
    };

    //查询昵称返回
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_NickNameInfo_Resoult
    {
        public byte cbSuccess;                          //查询是否成功
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)]
        public byte[] szNickName; //用户昵称 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] szDescribeString; //描述消息
    };

    //转房卡返回
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_Transfer_Diamonds_Resoutl
    {
        public byte cbSuccess; //转房卡是否成功
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] szDescribeString; //描述消息
    };

    //操作成功
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_OperateSuccess
    {
        public uint lResultCode; //操作代码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] szDescribeString; //成功消息
    };

    //操作失败
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_OperateFailure
    {
        public uint lResultCode; //错误代码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] szDescribeString; //描述消息
    };

    //签到奖励
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct DBO_GP_CheckInReward
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_SIGIN, ArraySubType = UnmanagedType.I8)] public long[] lRewardGold; //奖励金额

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_SIGIN)] public byte[] lRewardType;
        //奖励类型 1金币 2道具

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_SIGIN)] public byte[] lRewardDay; //奖励天数 
    };

    //查询签到
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_CheckInQueryInfo
    {
        public uint dwUserID; //用户标识
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szPassword; //登录密码

        public void Init()
        {
            szPassword = new byte[SocketDefines.LEN_PASSWORD];
        }
    };

    //签到信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_CheckInInfo
    {
        public ushort wSeriesDate; //连续日期
        public ushort wAwardDate; //物品日期
        public byte bTodayChecked; //签到标识
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_SIGIN, ArraySubType = UnmanagedType.I8)] public long[] lRewardGold; //奖励金额

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_SIGIN)] public byte[] lRewardType;//奖励类型 1金币 2道具

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_SIGIN)] public byte[] lRewardDay; //奖励天数 
    };

    //执行签到
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_CheckInDone
    {
        public uint dwUserID; //用户标识
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szPassword; //登录密码

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MACHINE_ID)] public byte[] szMachineID;//机器序列

        public void Init()
        {
            szPassword = new byte[SocketDefines.LEN_PASSWORD];
            szMachineID = new byte[SocketDefines.LEN_MACHINE_ID];
        }
    };

    //签到结果
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_CheckInResult
    {
        public byte bType; //是否是达到天数领取物品
        public byte bSuccessed; //成功标识
        public long lScore; //当前金币
        public ushort wSeriesDate;	//连续日期
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] szNotifyContent; //提示内容
    };


    //WQ add，抽奖
    //执行抽奖
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_RaffleDone
    {
        public uint dwUserID; //用户标识
        public uint dwRaffleGold;//抽到的钻石

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)]
        public byte[] szPassword; //登录密码

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MACHINE_ID)]
        public byte[] szMachineID;//机器序列

        public void Init()
        {
            szPassword = new byte[SocketDefines.LEN_PASSWORD];
            szMachineID = new byte[SocketDefines.LEN_MACHINE_ID];
        }
    };
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_RaffleResult
    {
        public byte bSuccessed; //成功标识
        public long lScore; //当前金币

        public uint dwRaffleCount;//已抽奖次数
        public uint dwPlayCount;//已打场次

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] szNotifyContent; //提示内容
    };
    // for HideSeek
    //大厅聊天数据
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GP_CHAT
    {
        public uint dwUserID; //用户标识

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)]
        public byte[] szNickName; //用户昵称

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] szChatData;


        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] szHeadHttp;

        public void Init()
        {
            szNickName = new byte[SocketDefines.LEN_NICKNAME];
            szChatData = new byte[128];
            szHeadHttp = new byte[SocketDefines.LEN_USER_NOTE];
        }
    }

    //////////////////////////////////////////////////////////////////////////////////
    //新手活动
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_BeginnerQueryInfo
    {
        public uint dwUserID; //用户标识
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szPassword; //登录密码
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_BeginnerInfo
    {
        public enum AwardType
        {
            Type_Gold = 1,
            Type_Phone = 2,
        };

        public ushort wSeriesDate; //连续日期
        public byte bTodayChecked; //签到标识
        public byte bLastCheckIned; //签到标识
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_BEGINNER, ArraySubType = UnmanagedType.I8)] public long[] lRewardGold; //奖励金额

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_BEGINNER)] public byte[] lRewardType;
        //奖励类型 1金币 2道具
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_BeginnerDone
    {
        public uint dwUserID; //用户标识
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szPassword; //登录密码

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MACHINE_ID)] public byte[] szMachineID;
        //机器序列
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_BeginnerResult
    {
        public byte bSuccessed; //成功标识
        public long lAwardCout; //奖励数量
        public long lAwardType; //奖励类型
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] szNotifyContent; //提示内容
    };

    //领取低保
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_BaseEnsureTake
    {
        public uint dwUserID; //用户 I D
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szPassword; //登录密码

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MACHINE_ID)] public byte[] szMachineID;
        //机器序列
    };

    //低保参数
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_BaseEnsureParamter
    {
        public long lScoreCondition; //游戏币条件
        public long lScoreAmount; //游戏币数量
        public byte cbTakeTimes; //领取次数	
    };

    //低保结果
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_BaseEnsureResult
    {
        public byte bSuccessed; //成功标识
        public long lGameScore; //当前游戏币
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] szNotifyContent; //提示内容
    };

    //自定义字段查询 公告
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_QueryNotice
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)] public byte[] szKeyName; //关键字
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_PublicNotice
    {
        public uint lResultCode; //操作代码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)] public byte[] szDescribeString; //成功消息
    };


    //////////////////////////////////////////////////////////////////////////////////

    //序列长度
    //帐号登录
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_LogonAccounts
    {
        //登录信息
        public uint dwPlazaVersion; //广场版本

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MACHINE_ID)] public byte[] szMachineID;
        //机器序列//LEN_MACHINE_ID

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MD5)] public byte[] szPassword;
        //登录密码//LEN_MD5

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ACCOUNTS)] public byte[] szAccounts;
        //登录帐号//LEN_ACCOUNTS

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ADDRANK)] public byte[] szUid;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ACCOUNTS)] public byte[] szOpenid;

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_TOKEN)]public byte[] szToken;

        // for Match Time
        public ushort wKindID;

        public uint cbValidateFlags; //校验标识

        public void Init()
        {
            szMachineID = new byte[SocketDefines.LEN_MACHINE_ID];
            szPassword = new byte[SocketDefines.LEN_MD5];

            szAccounts = new byte[SocketDefines.LEN_ACCOUNTS];
            szUid = new byte[SocketDefines.LEN_ADDRANK];
            szOpenid = new byte[SocketDefines.LEN_ACCOUNTS];
            //szToken = new byte[SocketDefines.LEN_TOKEN];
        }
    };

    //登录失败
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct DBR_GP_LogonError
    {
        public uint lErrorCode; //错误代码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] szErrorDescribe; //错误消息
    };


    //I D登录
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_LogonByUserID
    {
        //登录信息
        public uint dwGameID; //游戏 I D

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MD5)] public byte[] szPassword;
        //登录密码//LEN_MD5

        public byte cbValidateFlags; //校验标识
    };

    //注册帐号
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_RegisterAccounts
    {
        //系统信息
        public uint dwPlazaVersion; //广场版本

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MACHINE_ID)] public byte[] szMachineID;
        //机器序列

        //密码变量
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MD5)] public byte[] szLogonPass; //登录密码
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_MD5)] public byte[] szInsurePass; //银行密码

        //注册信息
        public ushort wFaceID; //头像标识
        public ushort wChannleId;  //渠道ID 
        public byte cbGender; //用户性别
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ACCOUNTS)] public byte[] szAccounts; //登录帐号

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ADDRANK)]
        public byte[] szUid;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ACCOUNTS)]
        public byte[] szOpenid;

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_TOKEN)]
        //public byte[] szToken;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)] public byte[] szNickName; //用户昵称
        public uint dwSpreaderID;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ACCOUNTS)] public byte[] szSpreader; //推荐帐号

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASS_PORT_ID)] public byte[] szPassPortID;
        //证件号码

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_COMPELLATION)] public byte[] szCompellation;
        //真实名字

        // for Match Time
        public ushort wKindID;

        public byte cbValidateFlags; //校验标识

        public void Init()
        {
            szAccounts = new byte[SocketDefines.LEN_ACCOUNTS];
            szUid = new byte[SocketDefines.LEN_ADDRANK];
            szOpenid = new byte[SocketDefines.LEN_ACCOUNTS];
            //szToken = new byte[SocketDefines.LEN_TOKEN];

            szNickName = new byte[SocketDefines.LEN_NICKNAME];
            szLogonPass = new byte[SocketDefines.LEN_MD5];
            szCompellation = new byte[SocketDefines.LEN_COMPELLATION];
            szInsurePass = new byte[SocketDefines.LEN_MD5];
            szMachineID = new byte[SocketDefines.LEN_MACHINE_ID];
            szPassPortID = new byte[SocketDefines.LEN_PASS_PORT_ID];
            //szSpreader = new byte[SocketDefines.LEN_ACCOUNTS];
        }

    };

    //游客登录
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_VisitorLogon
    {
        public ushort wFaceID; //头像标识
        public byte cbGender; //用户性别
        public uint dwPlazaVersion; //广场版本
        public byte cbValidateFlags; //校验标识
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)] public byte[] szNickName; //用户昵称
        public uint dwSpreaderID;//[MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)] public byte[] szSpreader; //推广人名
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szPassWord; //登录密码

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)] public byte[] szPassWordBank;
        //登录密码
    };


    //登陆成功
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_LogonSuccess
    {
        public ushort wFaceID; //头像标识
        public uint dwUserID; //用户 I D
        public uint dwGameID; //游戏 I D
        public uint dwGroupID; //社团标识
        public uint dwCustomID; //自定标识
        public uint dwUserMedal; //用户奖牌
        public uint dwExperience; //经验数值
        public uint dwLoveLiness; //用户魅力
        public uint dwSpreaderID; //推广ID
        public byte cbInsureEnabled; //银行开通

        public long lUserScore; //用户金币
        public long lUserInsure; //用户银行

        //用户信息
        public byte cbGender; //用户性别
        public byte cbMoorMachine; //锁定机器
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ACCOUNTS)] public byte[] szAccounts; //登录帐号
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_ACCOUNTS)] public byte[] szNickName; //用户昵称

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_GROUP_NAME)] public byte[] szGroupName;
        //社团名字

        //配置信息
        public byte cbShowServerStatus; //显示服务器状态

        //WQ add,for Match Time
        public systemtime MatchStartTime;
        public systemtime MatchEndTime;

        // for签到
        public ushort wSeriesDate;

        // 已打场次,for抽奖
        public uint dwPlayCount;

        //WQ add,抽奖记录
        public uint dwRaffleCount;
        public uint dwPlayCountPerRaffle;

        // 代理
        public int iSpreaderLevel;  //代理等级： -1:不是代理人

        // for HideSeek:查询警察模型库
        public long lModelIndex0;

        public byte cbGPIsForAppleReview;

        //WQ 头像Http
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_USER_NOTE)]
        public byte[] szHeadHttp;

        //WQ add,公告信息
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_USER_NOTE)]
        public byte[] szPublicNotice;
    };

    //修改玩家信息 WQ
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GP_ModUserInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)]
        public byte[] szNickName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_USER_NOTE)]
        public byte[] szHeadHttp;
        public void Init()
        {
            szNickName = new byte[SocketDefines.LEN_NICKNAME];
            szHeadHttp = new byte[SocketDefines.LEN_USER_NOTE];
        }
    }
    //列表配置
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_GP_ListConfig
    {
        public byte bShowOnLineCount; //显示人数
    };

    /*----------------------------\GameLib\Platform\PFDefine\msg\CMD_LogonServer.h--------------------------------------*/
}