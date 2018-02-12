using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using quicksdk;
using UnityEngine.SceneManagement;

public class EventHandle : QuickSDKListener
{

    public static int m_channelID;
    public static string m_kUid;
    public static string m_kAccount;
    public static string m_token;

    private ushort m_ItemID;
    private ushort m_Amount;
    private ushort m_Count;

    private void Init()
    {
        m_channelID = 0;
        m_kUid = "Uid";
        m_kAccount = "Name_";
        m_token = "token";
    }
    void showLog(string title, string message)
    {
        Debug.Log("title: " + title + ", message: " + message);
    }
    // Use this for initialization
    void Start()
    {
        Debug.Log("lyy  start ");

        QuickSDK.getInstance().setListener(this);

        Init();
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {

    //    }
    //}

    public void onLogin()
    {
        QuickSDK.getInstance().login();
    }

    public void onLogout()
    {
        QuickSDK.getInstance().logout();
    }


    public void onPay(int goodId)
    {
        OrderInfo orderInfo = new OrderInfo();
        GameRoleInfo gameRoleInfo = new GameRoleInfo();
        DateTime dt = DateTime.Now;
        orderInfo.cpOrderID = dt.Ticks.ToString();
        orderInfo.cpOrderID += (MersenneTwister.MT19937.Int63() % 100).ToString("D2");
        switch (QuickSDK.getInstance().channelType())
        {
            case 134:   //QuickGame渠道
                GetGoodsByID(goodId, 134,ref orderInfo);
                orderInfo.callbackUrl = "http://callback.sdk.quicksdk.net/callback/134/72063088805951804403573663362240";

                gameRoleInfo.gameRoleBalance = "0";
                gameRoleInfo.gameRoleID = "000001";
                gameRoleInfo.gameRoleLevel = "1";
                gameRoleInfo.gameRoleName = "Nmae";
                gameRoleInfo.partyName = "XX会";
                gameRoleInfo.serverID = "1";
                gameRoleInfo.serverName = "服务器";
                gameRoleInfo.vipLevel = "1";
                gameRoleInfo.roleCreateTime = "roleCreateTime";
                break;
            case 153:    //QuickGame_Appstore渠道
                GetGoodsByID(goodId, 153,ref orderInfo);
                orderInfo.callbackUrl = "http://callback.sdk.quicksdk.net/callback/153/72063088805951804403573663362240";

                gameRoleInfo.gameRoleBalance = "0";
                gameRoleInfo.gameRoleID = "000001";
                gameRoleInfo.gameRoleLevel = "1";
                gameRoleInfo.gameRoleName = "Nmae";
                gameRoleInfo.partyName = "XX会";
                gameRoleInfo.serverID = "1";
                gameRoleInfo.serverName = "服务器";
                gameRoleInfo.vipLevel = "1";
                gameRoleInfo.roleCreateTime = "roleCreateTime";
                break;
            case 9999: //IOS母包
                GetGoodsByID(goodId, 9999,ref orderInfo);
                orderInfo.callbackUrl = "";

                gameRoleInfo.gameRoleBalance = "0";
                gameRoleInfo.gameRoleID = "000001";
                gameRoleInfo.gameRoleLevel = "1";
                gameRoleInfo.gameRoleName = "Nmae";
                gameRoleInfo.partyName = "XX会";
                gameRoleInfo.serverID = "1";
                gameRoleInfo.serverName = "服务器";
                gameRoleInfo.vipLevel = "1";
                gameRoleInfo.roleCreateTime = "roleCreateTime";
                break;
            case 0: //Android母包
                GetGoodsByID(goodId, 0,ref orderInfo);
                orderInfo.callbackUrl = "";

                gameRoleInfo.gameRoleBalance = "0";
                gameRoleInfo.gameRoleID = "000001";
                gameRoleInfo.gameRoleLevel = "1";
                gameRoleInfo.gameRoleName = "Nmae";
                gameRoleInfo.partyName = "XX会";
                gameRoleInfo.serverID = "1";
                gameRoleInfo.serverName = "服务器";
                gameRoleInfo.vipLevel = "1";
                gameRoleInfo.roleCreateTime = "roleCreateTime";
                break;
        }

        m_ItemID = (ushort)goodId;
        m_Amount = (ushort)orderInfo.amount;
        m_Count = (ushort)orderInfo.count;
        QuickSDK.getInstance().pay(orderInfo, gameRoleInfo);
    }

    private void GetGoodsByID(int goodId, int channle, ref OrderInfo orderInfo)
    {
        if (channle == 153)
        {
            if (goodId == 1)
                orderInfo.goodsID = "CSIO_001";
            else if (goodId == 2)
                orderInfo.goodsID = "CSIO_002";
            else if (goodId == 3)
                orderInfo.goodsID = "CSIO_003";
        }
        else
            orderInfo.goodsID = goodId.ToString();

        switch (goodId)
        {
            case 1:
                orderInfo.count = 10;
                orderInfo.amount = 0.1;
                break;
            case 2:
                orderInfo.count = 20;
                orderInfo.amount = 0.2;
                break;
            case 3:
                orderInfo.count = 30;
                orderInfo.amount = 0.3;
                break;
        }
        orderInfo.goodsName = "钻石";
        orderInfo.goodsDesc = orderInfo.count + "个钻石"; //停用的，不用给值
        orderInfo.quantifier = "个";  //停用的，不用给值
        orderInfo.extrasParams = goodId + "&" + orderInfo.count + "&" + GameNet.GlobalUserInfo.getUserID();
        orderInfo.price = 0.1f;  //停用的，不用给值
    }
    public void onCreatRole()
    {
        //注：GameRoleInfo的字段，如果游戏有的参数必须传，没有则不用传
        GameRoleInfo gameRoleInfo = new GameRoleInfo();

        gameRoleInfo.gameRoleBalance = "0";
        gameRoleInfo.gameRoleID = "000001";
        gameRoleInfo.gameRoleLevel = "1";
        gameRoleInfo.gameRoleName = "Name";
        gameRoleInfo.partyName = "XX会";
        gameRoleInfo.serverID = "1";
        gameRoleInfo.serverName = "服务器";
        gameRoleInfo.vipLevel = "1";

        gameRoleInfo.roleCreateTime = "roleCreateTime";//UC与1881渠道必传，值为10位数时间戳

        gameRoleInfo.gameRoleGender = "男";//360渠道参数
        gameRoleInfo.gameRolePower = "38";//360渠道参数，设置角色战力，必须为整型字符串
        gameRoleInfo.partyId = "1100";//360渠道参数，设置帮派id，必须为整型字符串

        gameRoleInfo.professionId = "11";//360渠道参数，设置角色职业id，必须为整型字符串
        gameRoleInfo.profession = "法师";//360渠道参数，设置角色职业名称
        gameRoleInfo.partyRoleId = "1";//360渠道参数，设置角色在帮派中的id
        gameRoleInfo.partyRoleName = "帮主"; //360渠道参数，设置角色在帮派中的名称
        gameRoleInfo.friendlist = "无";//360渠道参数，设置好友关系列表，格式请参考：http://open.quicksdk.net/help/detail/aid/190

        QuickSDK.getInstance().createRole(gameRoleInfo);//创建角色
    }

    public void onEnterGame()
    {
        //注：GameRoleInfo的字段，如果游戏有的参数必须传，没有则不用传
        GameRoleInfo gameRoleInfo = new GameRoleInfo();

        gameRoleInfo.gameRoleBalance = "0";
        gameRoleInfo.gameRoleID = "000001";
        gameRoleInfo.gameRoleLevel = "1";
        gameRoleInfo.gameRoleName = "Name";
        gameRoleInfo.partyName = "XX会";
        gameRoleInfo.serverID = "1";
        gameRoleInfo.serverName = "服务器";
        gameRoleInfo.vipLevel = "1";
        gameRoleInfo.roleCreateTime = "roleCreateTime";//UC与1881渠道必传，值为10位数时间戳

        gameRoleInfo.gameRoleGender = "男";//360渠道参数
        gameRoleInfo.gameRolePower = "38";//360渠道参数，设置角色战力，必须为整型字符串
        gameRoleInfo.partyId = "1100";//360渠道参数，设置帮派id，必须为整型字符串

        gameRoleInfo.professionId = "11";//360渠道参数，设置角色职业id，必须为整型字符串
        gameRoleInfo.profession = "法师";//360渠道参数，设置角色职业名称
        gameRoleInfo.partyRoleId = "1";//360渠道参数，设置角色在帮派中的id
        gameRoleInfo.partyRoleName = "帮主"; //360渠道参数，设置角色在帮派中的名称
        gameRoleInfo.friendlist = "无";//360渠道参数，设置好友关系列表，格式请参考：http://open.quicksdk.net/help/detail/aid/190


        QuickSDK.getInstance().enterGame(gameRoleInfo);//开始游戏
        //Application.LoadLevel("scene4");
    }

    public void onUpdateRoleInfo()
    {
        //注：GameRoleInfo的字段，如果游戏有的参数必须传，没有则不用传
        GameRoleInfo gameRoleInfo = new GameRoleInfo();

        gameRoleInfo.gameRoleBalance = "0";
        gameRoleInfo.gameRoleID = "000001";
        gameRoleInfo.gameRoleLevel = "1";
        gameRoleInfo.gameRoleName = "Name";
        gameRoleInfo.partyName = "XX会";
        gameRoleInfo.serverID = "1";
        gameRoleInfo.serverName = "服务器";
        gameRoleInfo.vipLevel = "1";
        gameRoleInfo.roleCreateTime = "roleCreateTime";//UC与1881渠道必传，值为10位数时间戳

        gameRoleInfo.gameRoleGender = "男";//360渠道参数
        gameRoleInfo.gameRolePower = "38";//360渠道参数，设置角色战力，必须为整型字符串
        gameRoleInfo.partyId = "1100";//360渠道参数，设置帮派id，必须为整型字符串

        gameRoleInfo.professionId = "11";//360渠道参数，设置角色职业id，必须为整型字符串
        gameRoleInfo.profession = "法师";//360渠道参数，设置角色职业名称
        gameRoleInfo.partyRoleId = "1";//360渠道参数，设置角色在帮派中的id
        gameRoleInfo.partyRoleName = "帮主"; //360渠道参数，设置角色在帮派中的名称
        gameRoleInfo.friendlist = "无";//360渠道参数，设置好友关系列表，格式请参考：http://open.quicksdk.net/help/detail/aid/190

        QuickSDK.getInstance().updateRole(gameRoleInfo);
    }

    //public void onNext()
    //{
    //    //Application.LoadLevel("scene3");
    //}

    public void onExit()
    {
        if (QuickSDK.getInstance().isChannelHasExitDialog())
        {
            QuickSDK.getInstance().exit();
        }
        else
        {
            //游戏调用自身的退出对话框，点击确定后，调用QuickSDK的exit()方法
            QuickSDK.getInstance().exit();
        }
    }

    public void onExitCancel()
    {
         
    }
    public void onExitConfirm()
    {
        QuickSDK.getInstance().exit();
    }

    public void onShowToolbar()
    {
        QuickSDK.getInstance().showToolBar(ToolbarPlace.QUICK_SDK_TOOLBAR_BOT_LEFT);
    }

    public void onHideToolbar()
    {
        QuickSDK.getInstance().hideToolBar();
    }

    public void onEnterUserCenter()
    {
        QuickSDK.getInstance().callFunction(FuncType.QUICK_SDK_FUNC_TYPE_ENTER_USER_CENTER);
    }

    public void onEnterBBS()
    {
        QuickSDK.getInstance().callFunction(FuncType.QUICK_SDK_FUNC_TYPE_ENTER_BBS);
    }
    public void onEnterCustomer()
    {
        QuickSDK.getInstance().callFunction(FuncType.QUICK_SDK_FUNC_TYPE_ENTER_CUSTOMER_CENTER);
    }
    public void onUserId()
    {
        string uid = QuickSDK.getInstance().userId();
        showLog("userId", uid);
    }
    public void onChannelType()
    {
        int type = QuickSDK.getInstance().channelType();
        showLog("channelType", "" + type);
    }
    public void onFuctionSupport(int type)
    {
        bool supported = QuickSDK.getInstance().isFunctionSupported((FuncType)type);
        showLog("fuctionSupport", supported ? "yes" : "no");
    }
    public void onGetConfigValue(string key)
    {
        string value = QuickSDK.getInstance().getConfigValue(key);
        showLog("onGetConfigValue", key + ": " + value);
    }

    public void onOk()
    {

    }

    public void onPauseGame()
    {
        Time.timeScale = 0;
        QuickSDK.getInstance().callFunction(FuncType.QUICK_SDK_FUNC_TYPE_PAUSED_GAME);
    }

    public void onResumeGame()
    {
        Time.timeScale = 1;
    }

    //************************************************************以下是需要实现的回调接口*************************************************************************************************************************
    //callback
    public override void onInitSuccess()
    {
        showLog("onInitSuccess", "");
        //QuickSDK.getInstance ().login (); //如果游戏需要启动时登录，需要在初始化成功之后调用
    }

    public override void onInitFailed(ErrorMsg errMsg)
    {
        GameSceneUIHandler.ShowLog("游戏初始化失败，请重新启动");
        showLog("onInitFailed", "msg: " + errMsg.errMsg);
    }

    public override void onLoginSuccess(UserInfo userInfo)
    {
        showLog("onLoginSuccess", "uid: " + userInfo.uid + " ,username: " + userInfo.userName + " ,userToken: " + userInfo.token + ", msg: " + userInfo.errMsg);
        //SceneManager.LoadScene("GameHall");
        m_channelID = QuickSDK.getInstance().channelType();
        m_kUid = userInfo.uid;
        m_kAccount = userInfo.userName;
        m_token = userInfo.token;

        LogIn.hnManager.LogOn();
    }

    public override void onSwitchAccountSuccess(UserInfo userInfo)
    {
        //切换账号成功，清除原来的角色信息，使用获取到新的用户信息，回到进入游戏的界面，不需要再次调登录
        showLog("onLoginSuccess", "uid: " + userInfo.uid + " ,username: " + userInfo.userName + " ,userToken: " + userInfo.token + ", msg: " + userInfo.errMsg);
        SceneManager.LoadScene("GameLand");
    }

    public override void onLoginFailed(ErrorMsg errMsg)
    {
        showLog("onLoginFailed", "msg: " + errMsg.errMsg);
    }

    public override void onLogoutSuccess()
    {
        showLog("onLogoutSuccess", "");
        //注销成功后回到登陆界面
        Init();
        SceneManager.LoadScene("GameLand");
    }

    public override void onPaySuccess(PayResult payResult)
    {
        showLog("onPaySuccess", "orderId: " + payResult.orderId + ", cpOrderId: " + payResult.cpOrderId + " ,extraParam" + payResult.extraParam);
        onUpdateRoleInfo();
        //GameNet.CMD_GP_ShopItemInfo kNetInfo = new GameNet.CMD_GP_ShopItemInfo();
        //kNetInfo.Init();

        //Buffer.BlockCopy(System.Text.Encoding.Default.GetBytes(m_kUid), 0, kNetInfo.szUID, 0, Encoding.Default.GetBytes(m_kUid).Length);
        //Buffer.BlockCopy(System.Text.Encoding.Default.GetBytes(payResult.cpOrderId), 0, kNetInfo.szOrderID, 0, Encoding.Default.GetBytes(payResult.cpOrderId).Length);
        //kNetInfo.wItemID = m_ItemID; 
        //kNetInfo.wAmount = m_Amount;
        //kNetInfo.wCount = m_Count;
        //GameNet.UserInfo.getInstance().queryAddShopItem(kNetInfo);
        GameNet.UserInfo.getInstance().reqAccountInfo(5);    //5s之后向服务器请求最新用户数据
        GameSceneUIHandler.ShowLog("购买成功，请耐心等待");
    }

    public override void onPayCancel(PayResult payResult)
    {
        showLog("onPayCancel", "orderId: " + payResult.orderId + ", cpOrderId: " + payResult.cpOrderId + " ,extraParam" + payResult.extraParam);
    }

    public override void onPayFailed(PayResult payResult)
    {
        showLog("onPayFailed", "orderId: " + payResult.orderId + ", cpOrderId: " + payResult.cpOrderId + " ,extraParam" + payResult.extraParam);
    }

    public override void onExitSuccess()
    {
        showLog("onExitSuccess", "");
        //退出成功的回调里面调用  QuickSDK.getInstance ().exitGame ();  即可实现退出游戏，杀进程。为避免与渠道发生冲突，请不要使用  Application.Quit ();
        Init();
        QuickSDK.getInstance().exitGame();
    }
}

