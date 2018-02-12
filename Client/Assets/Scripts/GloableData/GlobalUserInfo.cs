using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GameNet
{
    //用户信息
    class tagGlobalUserData
    {
        public tagGlobalUserData()
        {
            szAccounts = new byte[SocketDefines.LEN_ACCOUNTS];
            szNickName = new byte[SocketDefines.LEN_NICKNAME];
            szPassword = new byte[SocketDefines.LEN_PASSWORD];
            szDynamicPass = new byte[SocketDefines.LEN_PASSWORD];
            szLogonIP = new byte[SocketDefines.LEN_ACCOUNTS];
            szUserChannel = new byte[SocketDefines.LEN_NICKNAME];
            szUnderWrite = new byte[SocketDefines.LEN_UNDER_WRITE];
            szGroupName = new byte[SocketDefines.LEN_GROUP_NAME];
            szHeadHttp=new byte[SocketDefines.LEN_USER_NOTE];//1024
            szPublicNotice = new byte[SocketDefines.LEN_USER_NOTE];

            iSpreaderLevel = -1;

            //mChen add, for HideSeek WangHu
            cbMapIndexRand = 100;
            cbMapIndexRandForSingleGame = 100;
            wRandseed = 128;
            wRandseedForRandomGameObject = 256;
            wRandseedForInventory = 512;
            //道具同步
            sInventoryList = new InventoryItem[InventoryManager.MAX_INVENTORY_NUM];

            bTodayChecked = false;
        }
        //基本资料
        public uint dwUserID;                         //用户 I D
        public uint dwGameID;                         //游戏 I D
        public uint dwUserMedal;                      //用户奖牌
        public uint dwExperience;                     //用户经验
        public uint dwLoveLiness;                     //用户魅力
        public uint dwSpreaderID;                     //推广ID
        public byte[] szAccounts;          //登录帐号
        public byte[] szNickName;          //用户昵称
        public byte[] szPassword;          //登录密码
        public byte[] szDynamicPass;       //动态密码
        public byte[] szLogonIP;           //登录IP
        public byte[] szUserChannel;       //渠道号

        //用户成绩
        public long lUserScore;                           //用户游戏币
        public long lUserInsure;                      //银行游戏币
        public long lUserIngot;                           //用户元宝
        public double dUserBeans;                          //用户游戏豆

        //mChen add, for签到
        public ushort wSeriesDate;
        public bool bTodayChecked;

        //mChen add, for抽奖
        public uint dwPlayCount;//已打场次
        public uint dwRaffleCount;//已抽奖次数
        public uint dwPlayCountPerRaffle;

        //mChen add, 代理
        public int iSpreaderLevel;  //代理等级： -1:不是代理人

        //mChen add, for HideSeek:查询警察模型库
        public long lModelIndex0;

        public bool bGPIsForAppleReview;

        //mChen add,公告信息
        public byte[] szPublicNotice;

        //mChen add, for HideSeek WangHu
        public byte cbMapIndexRand;
        public byte cbMapIndexRandForSingleGame;
        //for随机种子同步
        public ushort wRandseed;
        //地图随机物品生成
        public ushort wRandseedForRandomGameObject;
        //道具同步
        public ushort wRandseedForInventory;
        //public long lEncodedInventoryList;
        public InventoryItem[] sInventoryList;

        //扩展资料
        public byte cbGender;                          //用户性别
        public byte cbMoorMachine;                     //锁定机器
        public byte[] szUnderWrite;     //个性签名

        //社团资料
        public uint dwGroupID;                            //社团索引
        public byte[] szGroupName;       //社团名字

        //会员资料
        public byte cbMemberOrder;                     //会员等级
        public systemtime MemberOverDate;                      //到期时间

        //头像信息
        public ushort wFaceID;                           //头像索引
        public uint dwCustomID;                           //自定标识
        public tagCustomFaceInfo CustomFaceInfo;                       //自定头像
        public byte[] szHeadHttp;                  //http头像

        //配置信息
        public byte cbInsureEnabled;                   //银行使能

        public uint dwWinCount;                           //胜利盘数
        public uint dwLostCount;                      //失败盘数
        public uint dwDrawCount;                      //和局盘数					
        public uint dwFleeCount;                      //逃跑盘数
    };

    //扩展资料
    class tagIndividualUserData
    {
        public tagIndividualUserData()
        {
            szUserNote = new byte[SocketDefines.LEN_USER_NOTE];
            szCompellation = new byte[SocketDefines.LEN_COMPELLATION];
            szSeatPhone = new byte[SocketDefines.LEN_SEAT_PHONE];
            szMobilePhone = new byte[SocketDefines.LEN_MOBILE_PHONE];
            szQQ = new byte[SocketDefines.LEN_QQ];
            szEMail = new byte[SocketDefines.LEN_EMAIL];
            szDwellingPlace = new byte[SocketDefines.LEN_DWELLING_PLACE];
        }
        //用户信息
        public byte[] szUserNote;         //用户说明
        public byte[] szCompellation;  //真实名字

        //电话号码
        public byte[] szSeatPhone;       //固定电话
        public byte[] szMobilePhone;   //移动电话

        //联系资料
        public byte[] szQQ;                      //Q Q 号码
        public byte[] szEMail;                    //电子邮件
        public byte[] szDwellingPlace;//联系地址
    };

    //银行信息
    // struct tagUserInsureInfo
    // {
    // 	public ushort							wRevenueTake;						//税收比例
    // 	public ushort							wRevenueTransfer;					//税收比例
    // 	public ushort							wServerID;							//房间标识
    // 	SCORE							lUserScore;							//用户游戏币
    // 	SCORE							lUserInsure;						//银行游戏币
    // 	SCORE							lUserIngot;							//用户元宝
    // 	double							dUserBeans;							//用户游戏逗
    // 	SCORE							lTransferPrerequisite;				//转账条件
    // 	byte							cbInsureEnabled;					//银行使能条件
    // };

    //mChen add
    //代理人列表
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class tagSpreadersInfo
    {
        public ushort wItemCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GlobalUserInfo.Max_Spreaders_Num)]
        public SpreaderInfoItem[] SpreaderInfoItems;
        ///public SpreaderInfoItem SpreaderInfoItems[Max_Spreaders_Num_PerSend];

        public tagSpreadersInfo()
        {
            SpreaderInfoItems = new SpreaderInfoItem[GlobalUserInfo.Max_Spreaders_Num];
            for (int i = 0; i < SpreaderInfoItems.Length; i++)
            {
                SpreaderInfoItems[i].Init();
            }
        }
    };
    //名下用户交易信息
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class tagChildrenPaymentInfo
    {
        public double dTotalGrantOfChildrenBuy;			    //名下用户充值获得的提成总额（元）;   
        public double dExtraCash;							//获得的额外金额(元)：比如删除直属下级代理获得对方的剩余金额
        public double dCashedOut;                           //已提现金额（元）
        public double dTotalLeftCash;						//剩余金额（元）

        public ushort wItemCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GlobalUserInfo.Max_PaymentInfo_Num)]
        public PaymentInfoItem[] PaymentInfoItems;
    
        public tagChildrenPaymentInfo()
        {
            PaymentInfoItems = new PaymentInfoItem[GlobalUserInfo.Max_PaymentInfo_Num];
        }
    };

    //银行信息
    struct tagUserInsureInfo
    {
        public ushort wRevenueTake;                      //税收比例
        public ushort wRevenueTransfer;                  //税收比例
        public ushort wRevenueTransferMember;                //税收比例
        public ushort wServerID;                         //房间标识
        public long lUserScore;                           //用户游戏币
        public long lUserInsure;                      //银行游戏币
        public long lTransferPrerequisite;                //转帐条件
    };

    interface GlobalUserInfoSink
    {
        void upPlayerInfo();
        void LoginSucess();
    }

    //用户信息
    class GlobalUserInfo
    {
        //用户信息
        protected tagGlobalUserData m_GlobalUserData;                     //用户资料
        protected tagIndividualUserData m_IndividualUserData;                 //扩展资料
        protected tagUserInsureInfo m_UserInsureInfo;                     //银行资料

        protected tagGrowLevelParameter m_GrowLevelParameter;                 //用户等级信息

        //mChen add
        //查询代理人列表
        tagSpreadersInfo m_SpreadersInfo;                       //代理人列表
        public const int Max_Spreaders_Num_PerSend = 400;       //服务器每次发送的最大代理人个数  
        public const int Max_Spreaders_Num = 10000;
        //名下用户交易信息
        public const int Max_PaymentInfo_Num_PerSend = 800;         //服务器每次发送最大交易信息数 
        public const int Max_PaymentInfo_Num = 10000;
        tagChildrenPaymentInfo m_ChildrenPaymentInfo;

        //lin add
        public const int Max_TopPlayers_Num = 50;    //最大排行榜人数
        private TopPlayersInfoItem[] m_topPlayersInfo;

        protected static GlobalUserInfoSink m_pUserInfoSink;
        //静态变量
        protected static GlobalUserInfo m_pGlobalUserInfo;                      //用户信息

        //函数定义

        //构造函数
        private GlobalUserInfo()
        {
            m_GlobalUserData = new tagGlobalUserData();
            m_IndividualUserData = new tagIndividualUserData();
            m_UserInsureInfo = new tagUserInsureInfo();
            m_GrowLevelParameter = new tagGrowLevelParameter();

            m_SpreadersInfo = new tagSpreadersInfo();
            m_ChildrenPaymentInfo = new tagChildrenPaymentInfo();
        }

        //功能函数

        //mChen add
        public tagSpreadersInfo GetSpreadersInfo() { return m_SpreadersInfo; }
        public tagChildrenPaymentInfo GetChildrenPaymentInfo() { return m_ChildrenPaymentInfo; }
        //lin add
        public TopPlayersInfoItem[] GetTopPlayersInfo() { return m_topPlayersInfo; }

        //重置资料
        public void ResetUserInfoData()
        {
            m_GlobalUserData = new tagGlobalUserData();
            m_IndividualUserData = new tagIndividualUserData();
        }

        public static string GBToUtf8(byte[] bytes)
        {
            byte[] temp = Encoding.Convert(Encoding.GetEncoding(936), Encoding.UTF8, bytes);//"gb2312"
            string result = Encoding.UTF8.GetString(temp);
            return result;
        }
        //public static string GBToUtf8(string str)
        //{
        //    byte[] temp = Encoding.GetEncoding(936).GetBytes(str);
        //    byte[] temp1 = Encoding.Convert(Encoding.GetEncoding(936), Encoding.UTF8, temp);
        //    string result = Encoding.UTF8.GetString(temp1);
        //    return result;
        //}
        //public byte[] GBToUtf8(byte[] bytes)
        //{
        //    byte[] temp = Encoding.Convert(Encoding.GetEncoding(936), Encoding.UTF8, bytes);
        //    return temp;
        //}


        //用户资料
        public tagGlobalUserData GetGlobalUserData() { return m_GlobalUserData; }
        //扩展资料
        public tagIndividualUserData GetIndividualUserData() { return m_IndividualUserData; }
        //银行资料
        public tagUserInsureInfo GetUserInsureInfo() { return m_UserInsureInfo; }
        //用户等级
        public tagGrowLevelParameter GetUserGrowLevelParameter()
        {
            return m_GrowLevelParameter;
        }

        public string getPhoneNumber()
        {
            return Encoding.Default.GetString(m_IndividualUserData.szMobilePhone);
        }

        public static void setNickName(string kName)
        {
            GetInstance().GetGlobalUserData().szNickName = Encoding.GetEncoding(936).GetBytes(kName);
            GetInstance().upPlayerInfo();
        }

        public static void setAccounts(string kAccounts)
        {
            GetInstance().GetGlobalUserData().szAccounts = Encoding.Default.GetBytes(kAccounts);
            GetInstance().upPlayerInfo();
        }

        public static void setUserScore(long lScore)
        {
            GetInstance().GetGlobalUserData().lUserScore = lScore;
            GetInstance().upPlayerInfo();
        }

        public static void setUserInsure(long lScore)
        {
            GetInstance().GetGlobalUserData().lUserInsure = lScore;
            GetInstance().upPlayerInfo();
        }

        public static uint getUserID()
        {
            return GetInstance().GetGlobalUserData().dwUserID;
        }

        public static uint getGameID()
        {
            return GetInstance().GetGlobalUserData().dwGameID;
        }

        public static byte[] getPassword()
        {
            return GetInstance().GetGlobalUserData().szPassword;
        }

        public static byte getGender()
        {
            return GetInstance().GetGlobalUserData().cbGender;
        }
        public static string getNickName()
        {
            return GBToUtf8(GetInstance().GetGlobalUserData().szNickName);
            ///return Encoding.GetEncoding("gb2313").GetString(m_pGlobalUserInfo.GetGlobalUserData().szNickName);
        }

        public static byte[] getHeadHttp()
        {
            return GetInstance().GetGlobalUserData().szHeadHttp;/*Encoding.Default.GetString(m_pGlobalUserInfo.GetGlobalUserData().szHeadHttp);*/
        }
        public static string getUserChannel()
        {
            return Encoding.Default.GetString(GetInstance().GetGlobalUserData().szUserChannel);
        }

        public static string getAccounts()
        {
            return Encoding.Default.GetString(GetInstance().GetGlobalUserData().szAccounts);
        }

        public static long getUserScore()
        {
            if (m_pGlobalUserInfo != null && m_pGlobalUserInfo.GetGlobalUserData() != null)
            {
                return m_pGlobalUserInfo.GetGlobalUserData().lUserScore;
            }
            else
            {
                return 0;
            }
        }

        public static long getUserInsure()
        {
            if(m_pGlobalUserInfo!=null && m_pGlobalUserInfo.GetGlobalUserData()!=null)
            {
                return m_pGlobalUserInfo.GetGlobalUserData().lUserInsure;
            }
            else
            {
                return 0;
            }

        }
        public static long getUserExp()
        {
            if (m_pGlobalUserInfo != null && m_pGlobalUserInfo.GetGlobalUserData() != null)
            {
                return m_pGlobalUserInfo.GetGlobalUserData().dwExperience;
            }
            else
            {
                return 0;
            }
        }
        public void setSink(GlobalUserInfoSink pSink)
        {
            m_pUserInfoSink = pSink;
        }

        public void upPlayerInfo()
        {
            if (m_pUserInfoSink!=null)
            {
                m_pUserInfoSink.upPlayerInfo();
            }
        }
        public void LoginSucess()
        {
            if (m_pUserInfoSink!=null)
            {
                m_pUserInfoSink.LoginSucess();
            }
        }
        //静态函数
    
	//获取对象
        public static GlobalUserInfo GetInstance()
        {
            if (m_pGlobalUserInfo == null)
            {
                m_pGlobalUserInfo = new GlobalUserInfo();
            }
            return m_pGlobalUserInfo;
        }
    }
}
