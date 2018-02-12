using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace GameNet
{
    public class ParameterDefines
    {
        //配置信息
        public const string REG_GOBAL_OPTION = "GolbalOption"; //全局信息


        //////////////////////////////////////////////////////////////////////////////////
        //枚举定义

        //邀请模式
        public const int INVITE_MODE_ALL = 0; //全部显示
        public const int INVITE_MODE_NONE = 1; //全不显示
        public const int INVITE_MODE_FRIEND = 2; //好友显示

        //消息模式
        public const int MESSAGE_MODE_ALL = 0; //全部显示
        public const int MESSAGE_MODE_DETEST = 1; //屏蔽厌恶
        public const int MESSAGE_MODE_FRIEND = 2; //社团好友
        public const int MESSAGE_MODE_NONE = 3; //全不显示

        //界面动作
        public const int ACTION_ORIENTATION = 0; //定位用户
        public const int ACTION_SEND_WHISPER = 1; //发送私聊
        public const int ACTION_SHOW_USER_INFO = 2; //用户信息
        public const int ACTION_SEARCH_TABLE = 3; //寻找位置
        public const int ACTION_SHOW_SEARCH_DLG = 4; //查找界面

        //////////////////////////////////////////////////////////////////////////////////
    }




    //游戏参数
    public class CParameterGame
    {
        //友元定义

        //胜率限制
        public ushort m_wMinWinRate; //最低胜率
        public bool m_bLimitWinRate; //限制胜率

        //逃率限制

        public ushort m_wMaxFleeRate; //最高逃跑
        public bool m_bLimitFleeRate; //限制断线

        //积分限制

        public long m_lMaxGameScore; //最高分数 
        public long m_lMinGameScore; //最低分数
        public bool m_bLimitGameScore; //限制分数

        //属性变量
        protected byte[] m_szRegKeyName = new byte[16]; //注册项名

        //函数定义
        //构造函数
        public CParameterGame()
        {
            //默认参数
            DefaultParameter();
        }

        //功能函数
        //加载参数
        public void LoadParameter()
        {
            return;
            //配置表项
#if false
    //胜率限制
            m_wMinWinRate = (word)MyUserDefault::getInstance()->getIntegerForKey(m_szRegKeyName, "MinWinRate", m_wMinWinRate);
            m_bLimitWinRate = MyUserDefault::getInstance()->getBoolForKey(m_szRegKeyName, "LimitWinRate", m_bLimitWinRate);

            //逃率限制
            m_wMaxFleeRate = (word)MyUserDefault::getInstance()->getIntegerForKey(m_szRegKeyName, "MaxFleeRate", m_wMaxFleeRate);
            m_bLimitFleeRate = MyUserDefault::getInstance()->getBoolForKey(m_szRegKeyName, "LimitFleeRate", m_bLimitFleeRate);

            //积分限制
            m_lMaxGameScore = MyUserDefault::getInstance()->getIntegerForKey(m_szRegKeyName, "MaxGameScore", m_lMaxGameScore);
            m_lMinGameScore = MyUserDefault::getInstance()->getIntegerForKey(m_szRegKeyName, "MinGameScore", m_lMinGameScore);
            m_bLimitGameScore = MyUserDefault::getInstance()->getBoolForKey(m_szRegKeyName, "LimitGameScore", m_bLimitGameScore);
#endif
        }

        //保存参数
        public void SaveParameter()
        {
            return;
            //配置表项
#if false
    //胜率限制
            MyUserDefault::getInstance()->setIntegerForKey(m_szRegKeyName, "MinWinRate", m_wMinWinRate);
            MyUserDefault::getInstance()->setBoolForKey(m_szRegKeyName, "LimitWinRate", m_bLimitWinRate);

            //逃率限制
            MyUserDefault::getInstance()->setIntegerForKey(m_szRegKeyName, "MaxFleeRate", m_wMaxFleeRate);
            MyUserDefault::getInstance()->setBoolForKey(m_szRegKeyName, "LimitFleeRate", m_bLimitFleeRate);

            //积分限制
            MyUserDefault::getInstance()->setIntegerForKey(m_szRegKeyName, "MaxGameScore", m_lMaxGameScore);
            MyUserDefault::getInstance()->setIntegerForKey(m_szRegKeyName, "MinGameScore", m_lMinGameScore);
            MyUserDefault::getInstance()->setBoolForKey(m_szRegKeyName, "LimitGameScore", m_bLimitGameScore);
#endif
        }

        //默认参数
        public void DefaultParameter()
        {
            //胜率限制
            m_wMinWinRate = 0;
            m_bLimitWinRate = false;

            //逃率限制
            m_wMaxFleeRate = 5000;
            m_bLimitFleeRate = false;

            //积分限制
            m_bLimitGameScore = false;
            m_lMaxGameScore = 2147483647L;
            m_lMinGameScore = -2147483646L;
        }

        //配置函数

        //配置参数
        protected bool InitParameter(byte[] pszProcessName)
        {
            //构造键名
            ushort wNameIndex = 0;
            while (wNameIndex < ((m_szRegKeyName.Length) - 1))
            {
                //终止判断
                if (pszProcessName[wNameIndex] == 0) break;
                if (pszProcessName[wNameIndex] == '.') break;

                //设置字符
                ushort wCurrentIndex = wNameIndex++;
                m_szRegKeyName[wCurrentIndex] = pszProcessName[wCurrentIndex];
            }

            //设置变量
            m_szRegKeyName[wNameIndex] = 0;

            return true;
        }


    };

    //////////////////////////////////////////////////////////////////////////////////

    //房间参数
    public class CParameterServer
    {
        //配置变量
        public bool m_bTakePassword; //密码标志
        public byte[] m_szPassword = new byte[SocketDefines.LEN_PASSWORD]; //桌子密码

        //函数定义
        //构造函数
        public CParameterServer()
        {
            DefaultParameter();
        }

        //配置函数
        //默认参数
        public void DefaultParameter()
        {
            //配置变量
            m_bTakePassword = false;
        }
    };
    
    //全局参数
    class CParameterGlobal
    {
        //静态函数
        //静态变量
        private static CParameterGlobal __gParameterGlobal = null; //全局配置
        //获取对象
        public static CParameterGlobal shared()
        {
            if (__gParameterGlobal == null)
                __gParameterGlobal = new CParameterGlobal();
            return __gParameterGlobal;
        }

        public static void purge()
        {
            if (__gParameterGlobal != null)
                __gParameterGlobal = null;

        }

        //登录配置
        public bool m_bLogonAuto; //自动登录
        public bool m_bWriteCookie; //写入甜饼
        public bool m_bRemberPassword; //记住密码

        //模式配置
        public byte m_cbInviteMode; //邀请模式
        public byte m_cbMessageMode; //消息模式
        public byte m_cbActionHitAutoJoin; //自动加入
        public byte m_cbActionLeftDoubleList; //双击列表

        //时间配置

        public ushort m_wIntermitTime; //中断时间
        public ushort m_wOnLineCountTime; //人数时间

        //房间配置
        public bool m_bLimitDetest; //限制厌恶
        public bool m_bLimitSameIP; //限制地址
        public bool m_bNotifyUserInOut; //进出消息
        public bool m_bNotifyFriendCome; //好友提示

        //系统配置

        public bool m_bAllowSound; //允许声音
        public bool m_bAllowBackMusic; //允许背景音乐
        public bool m_bAutoSitDown; //自动坐下
        public bool m_bAutoShowWhisper; //显示私聊
        public bool m_bSaveWhisperChat; //保存私聊
        public bool m_bSendWhisperByEnter; //回车发送

        //配置组件

        protected Dictionary<ushort, CParameterGame> m_ParameterGameMap; //游戏配置
        protected Dictionary<ushort, CParameterServer> m_ParameterServerMap; //房间配置

        //显示配置
        public bool m_bShowServerStatus; //显示服务器状态

        //函数定义
        //构造函数
        public CParameterGlobal()
        {
            m_ParameterGameMap = new Dictionary<ushort, CParameterGame>();
            m_ParameterServerMap = new Dictionary<ushort, CParameterServer>();
            //默认参数
            DefaultParameter();
        }

        //配置函数
        //加载参数
        public void LoadParameter()
        {
#if false
    //自动登录
	m_bLogonAuto=MyUserDefault::getInstance()->getBoolForKey(REG_GOBAL_OPTION,"LogonAuto",m_bLogonAuto);
	m_bWriteCookie=MyUserDefault::getInstance()->getBoolForKey(REG_GOBAL_OPTION,"WriteCookie",m_bWriteCookie);

	//邀请模式
	m_cbInviteMode=(byte)MyUserDefault::getInstance()->getIntegerForKey(REG_GOBAL_OPTION,"InviteMode",m_cbInviteMode);
	switch (m_cbInviteMode)
	{
	case INVITE_MODE_NONE:
	case INVITE_MODE_FRIEND: { break; }
	default: { m_cbInviteMode=INVITE_MODE_ALL; }
	}

	//消息模式
	m_cbMessageMode=(byte)MyUserDefault::getInstance()->getIntegerForKey(REG_GOBAL_OPTION,"MessageMode",m_cbMessageMode);
	switch (m_cbMessageMode)
	{
	case MESSAGE_MODE_ALL:
	case MESSAGE_MODE_FRIEND: { break; }
	default: { m_cbMessageMode=MESSAGE_MODE_DETEST; }
	}

	//自动加入
	m_cbActionHitAutoJoin=(byte)MyUserDefault::getInstance()->getIntegerForKey(REG_GOBAL_OPTION,"ActionHitAutoJoin",m_cbActionHitAutoJoin);
	switch (m_cbActionHitAutoJoin)
	{
	case ACTION_SHOW_SEARCH_DLG: { break; }
	default: { m_cbActionHitAutoJoin=ACTION_SEARCH_TABLE; }
	}

	//双击列表
	m_cbActionLeftDoubleList=(byte)MyUserDefault::getInstance()->getIntegerForKey(REG_GOBAL_OPTION,"ActionLeftDoubleList",m_cbActionLeftDoubleList);
	switch (m_cbActionLeftDoubleList)
	{
	case ACTION_ORIENTATION:
	case ACTION_SHOW_USER_INFO: { break; }
	default: { m_cbActionLeftDoubleList=ACTION_SEND_WHISPER; }
	}

	//房间配置
	m_bLimitDetest=MyUserDefault::getInstance()->getBoolForKey(REG_GOBAL_OPTION,"LimitDetest",m_bLimitDetest);
	m_bLimitSameIP=MyUserDefault::getInstance()->getBoolForKey(REG_GOBAL_OPTION,"LimitSameIP",m_bLimitSameIP);
	m_bNotifyUserInOut=MyUserDefault::getInstance()->getBoolForKey(REG_GOBAL_OPTION,"NotifyUserInOut",m_bNotifyUserInOut);
	m_bNotifyFriendCome=MyUserDefault::getInstance()->getBoolForKey(REG_GOBAL_OPTION,"NotifyFriendCome",m_bNotifyFriendCome);

	//系统配置
	m_bAllowSound=MyUserDefault::getInstance()->getBoolForKey(REG_GOBAL_OPTION,"AllowSound",m_bAllowSound);
	m_bAllowBackMusic=MyUserDefault::getInstance()->getBoolForKey(REG_GOBAL_OPTION,"AllowBackMusic",m_bAllowBackMusic);
	m_bAutoSitDown=MyUserDefault::getInstance()->getBoolForKey(REG_GOBAL_OPTION,"AutoSitDown",m_bAutoSitDown);
	m_bAutoShowWhisper=MyUserDefault::getInstance()->getBoolForKey(REG_GOBAL_OPTION,"AutoShowWhisper",m_bAutoShowWhisper);
	m_bSaveWhisperChat=MyUserDefault::getInstance()->getBoolForKey(REG_GOBAL_OPTION,"SaveWhisperChat",m_bSaveWhisperChat);
	m_bSendWhisperByEnter=MyUserDefault::getInstance()->getBoolForKey(REG_GOBAL_OPTION,"SendWhisperByEnter",m_bSendWhisperByEnter);
#endif
        }

        //保存参数
        void SaveParameter()
        {
#if false
    //控制变量
	{
		//自动登录
		MyUserDefault::getInstance()->setBoolForKey(REG_GOBAL_OPTION,"LogonAuto",m_bLogonAuto);
		MyUserDefault::getInstance()->setBoolForKey(REG_GOBAL_OPTION,"WriteCookie",m_bWriteCookie);

		//邀请模式
		MyUserDefault::getInstance()->setIntegerForKey(REG_GOBAL_OPTION,"InviteMode",m_cbInviteMode);
		
		//消息模式
		MyUserDefault::getInstance()->setIntegerForKey(REG_GOBAL_OPTION,"MessageMode",m_cbMessageMode);
		
		//自动加入
		MyUserDefault::getInstance()->setIntegerForKey(REG_GOBAL_OPTION,"ActionHitAutoJoin",m_cbActionHitAutoJoin);
		
		//双击列表
		MyUserDefault::getInstance()->setIntegerForKey(REG_GOBAL_OPTION,"ActionLeftDoubleList",m_cbActionLeftDoubleList);
		
		//房间配置
		MyUserDefault::getInstance()->setBoolForKey(REG_GOBAL_OPTION,"LimitDetest",m_bLimitDetest);
		MyUserDefault::getInstance()->setBoolForKey(REG_GOBAL_OPTION,"LimitSameIP",m_bLimitSameIP);
		MyUserDefault::getInstance()->setBoolForKey(REG_GOBAL_OPTION,"NotifyUserInOut",m_bNotifyUserInOut);
		MyUserDefault::getInstance()->setBoolForKey(REG_GOBAL_OPTION,"NotifyFriendCome",m_bNotifyFriendCome);

		//系统配置
		MyUserDefault::getInstance()->setBoolForKey(REG_GOBAL_OPTION,"AllowSound",m_bAllowSound);
		MyUserDefault::getInstance()->setBoolForKey(REG_GOBAL_OPTION,"AllowBackMusic",m_bAllowBackMusic);
		MyUserDefault::getInstance()->setBoolForKey(REG_GOBAL_OPTION,"AutoSitDown",m_bAutoSitDown);
		MyUserDefault::getInstance()->setBoolForKey(REG_GOBAL_OPTION,"AutoShowWhisper",m_bAutoShowWhisper);
		MyUserDefault::getInstance()->setBoolForKey(REG_GOBAL_OPTION,"SaveWhisperChat",m_bSaveWhisperChat);
		MyUserDefault::getInstance()->setBoolForKey(REG_GOBAL_OPTION,"SendWhisperByEnter",m_bSendWhisperByEnter);

	}

	
	CParameterGameMap::iterator GameIter = m_ParameterGameMap.begin();
	for (; GameIter != m_ParameterGameMap.end(); ++GameIter)
	{
		GameIter->second->SaveParameter();
	}
#endif
        }

        //默认参数
        void DefaultParameter()
        {
            //登录配置
            m_bLogonAuto = false;
            m_bWriteCookie = true;
            m_bRemberPassword = false;

            //时间配置
            m_wIntermitTime = 0;
            m_wOnLineCountTime = 0;

            //房间配置
            m_cbInviteMode = ParameterDefines.INVITE_MODE_ALL;
            m_cbMessageMode = ParameterDefines.MESSAGE_MODE_ALL;
            m_cbActionHitAutoJoin = ParameterDefines.ACTION_SEARCH_TABLE;
            m_cbActionLeftDoubleList = ParameterDefines.ACTION_SEND_WHISPER;

            //房间配置
            m_bLimitDetest = false;
            m_bLimitSameIP = false;
            m_bNotifyUserInOut = false;
            m_bNotifyFriendCome = true;

            //系统配置
            m_bAllowSound = true;
            m_bAllowBackMusic = true;
            m_bAutoSitDown = true;
            m_bSaveWhisperChat = true;
            m_bAutoShowWhisper = false;
            m_bSendWhisperByEnter = true;
        }

        //游戏配置
        //游戏配置
        public CParameterGame GetParameterGame(tagGameKind pGameKind)
        {
            //寻找现存

            if (m_ParameterGameMap.ContainsKey(pGameKind.wKindID))
            {
                return m_ParameterGameMap[pGameKind.wKindID];
            }

            //创建对象

            //创建对象
            CParameterGame pParameterGame = new CParameterGame();

            //配置对象
            //		pParameterGame->InitParameter(pGameKind->szProcessName);

            //加载参数
            pParameterGame.LoadParameter();

            //设置数组
            m_ParameterGameMap[pGameKind.wKindID] = pParameterGame;

            return pParameterGame;


        }

        //房间配置
        public CParameterServer GetParameterServer(tagGameServer pGameServer)
        {
            //寻找现存

            if (m_ParameterServerMap.ContainsKey(pGameServer.wServerID))
            {
                return m_ParameterServerMap[pGameServer.wServerID];
            }


            //创建对象
            CParameterServer pParameterServer = new CParameterServer();

            //设置数组
            m_ParameterServerMap[pGameServer.wServerID] = pParameterServer;

            return pParameterServer;
        }
    };
}
