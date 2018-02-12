#define WEIXINLOGIN
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GameNet;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace GameNet
{

    public class LoginScene : IGPLoginMissionSink
    {
#if true
        public static string m_kAccount = "Account_";//WtoY1d_1Fvw52yhXOof-V-MRHRd5eE 1级代理， WtoY1d_1EgfAyUyVsMjo8CIqzAln3Q 0级代理
        public static string m_kUid= "Uid";
        public static string m_kOpenid = "Openid";
        public static ushort m_ChannelID = 0;
        public static string m_kPssword = "WeiXinPassword";
        public static string m_kNickName = "Name_";
        public static bool m_bMale;
        //public static string m_headURL = "0";
        public static string m_strServerIP = "127.0.0.1";    //101.132.137.47  139.224.115.49
        public static int m_nLogonServerPort = 10081; 
#else
        private string m_kAccount = "WeiXinCMM4";
        private string m_kPssword = "WeiXinPassword";
        private string m_kNickName = "CMM4";
#endif
        private CGPLoginMission m_kLoginMission;

        public LoginScene()
        {
//#if WEIXINLOGIN && (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
//            //Social.appKey = "59534811734be402f1000429";
//#else

//#endif
            Random.InitState((int)DateTime.Now.Ticks);
            var randV = (int)(Random.Range(1, 1000000000));
            m_kAccount = m_kAccount + randV;
            //m_kNickName = m_kNickName + randV;
            m_kNickName = CreateNickName.GetInstance.RandomName();
            Debug.Log("--------------------- " + m_kNickName);

            var dddb = Loom.Current;
            var b = System.Text.Encoding.Default.GetBytes(LoginScene.m_strServerIP);
            m_kLoginMission = new CGPLoginMission(b, LoginScene.m_nLogonServerPort);
            m_kLoginMission.setMissionSink(this);
        }
        /*----------------IGPLoginMissionSink------------------------*/

        public void onGPLoginSuccess()
        {
            UnityEngine.Debug.Log("--IGPLoginMissionSink----------onGPLoginSuccess call---------");
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

            Loom.QueueOnMainThread(() =>
            {
                if (PlayerPrefs.HasKey("LocalHumanInfo"))  //登陆成功清除之前断线位置信息
                {
                    PlayerPrefs.DeleteKey("LocalHumanInfo");
                    PlayerPrefs.Save();
                }

                //PlayerPrefs.SetString("Accounts", Encoding.Default.GetString(pGlobalUserData.szAccounts));
                //PlayerPrefs.SetString("Accounts", Encoding.Default.GetString(pGlobalUserData.szPassword));
                LogIn.ShowNickNameForRegisterWin(false);
                Debug.Log("Go to Hall Scene1");
                SceneManager.LoadScene("GameHall");

            });
        }

        public void onGPLoginComplete()
        {
            //Loom.QueueOnMainThread(() =>
            //{
            //    Debug.Log("Go to Hall Scene2");
            //    SceneManager.LoadScene("GameHall");
            //});
        }

        public bool onGPUpdateNotify(byte cbMustUpdate, byte[] szDescription)
        {
            return false;
        }

        public void onGPLoginFailure(uint iErrorCode, byte[] szDescription)
        {
            if (iErrorCode == 3 || iErrorCode == 1)  //账号未注册
            {
                Loom.QueueOnMainThread(() => { LogIn.ShowNickNameForRegisterWin(true); });
                //RegisterAccount();
            }
            else if (iErrorCode == 7)   //注册冲突问题，需要重新修改
            {
                string strDescribe = GlobalUserInfo.GBToUtf8(szDescription);
                GameSceneUIHandler.ShowLog(strDescribe);
            }
            else
            {
                Debug.Log(Encoding.Default.GetString(szDescription));
                //TimeManager::Instance().addCerterTimeCB(TIME_CALLBACK(HNScenceManager::InHomeScence, HNScenceManager::pInstance()), 3.0f);
                Loom.QueueOnMainThread(() =>
                {
                    SceneManager.LoadScene("GameLand");
                });
            }
            Loom.QueueOnMainThread(() =>
            {
                if (PlayerPrefs.HasKey("ChoosedModelIndex"))
                {
                    PlayerPrefs.DeleteKey("ChoosedModelIndex");
                    PlayerPrefs.Save();
                }
            });
        }

        public void onGPError(Exception errorCode)
        {
            Debug.Log("NetError : " + errorCode.Message);
            Loom.QueueOnMainThread(() =>
            {
                SceneManager.LoadScene("GameLand");
            });
        }

        public void RegisterAccount()
        {
            //lin: 微信第三方注册还未实现！！

            //m_kAccount = "Account_652595469";
            //m_kUid = "Uid";
            //m_kOpenid = "Openid";
            //m_kPssword = "WeiXinPassword                  ";
            //m_kNickName = "慷慨阿姆斯特朗炮";

            CMD_GP_RegisterAccounts kRegister = new CMD_GP_RegisterAccounts();
            kRegister.Init();
            kRegister.dwPlazaVersion = DF.shared().GetPlazaVersion();
            kRegister.cbValidateFlags = MsgDefine.MB_VALIDATE_FLAGS | MsgDefine.LOW_VER_VALIDATE_FLAGS;
            kRegister.cbGender = (byte)(m_bMale ? 1 : 0);
            kRegister.wFaceID = 0;
            kRegister.wChannleId = m_ChannelID;
            var tempBuf = Encoding.Default.GetBytes(m_kAccount);
            Buffer.BlockCopy(tempBuf, 0, kRegister.szAccounts, 0, tempBuf.Length);

            tempBuf = System.Text.Encoding.Default.GetBytes(m_kUid);
            Buffer.BlockCopy(tempBuf, 0, kRegister.szUid, 0, tempBuf.Length);
            tempBuf = System.Text.Encoding.Default.GetBytes(m_kOpenid);
            Buffer.BlockCopy(tempBuf, 0, kRegister.szOpenid, 0, tempBuf.Length);

            tempBuf = Encoding.Default.GetBytes(m_kPssword);
            Buffer.BlockCopy(tempBuf, 0, kRegister.szLogonPass, 0, tempBuf.Length);
            tempBuf = Encoding.UTF8.GetBytes(m_kNickName);  //不能使用GetEncoding(936)，会导致数据库存的是乱码 在服务端进行UTF8转GB编码处理
            Buffer.BlockCopy(tempBuf, 0, kRegister.szNickName, 0, tempBuf.Length);
            //kRegister.szAccounts = Encoding.Default.GetBytes (m_kAccount);
            //kRegister.szLogonPass = Encoding.Default.GetBytes(m_kPssword);
            //kRegister.szNickName = Encoding.Default.GetBytes(m_kNickName);
            //strncpy(kRegister.szAccounts, kAccounts.c_str(), kAccounts.size());
            //strncpy(kRegister.szLogonPass, m_kPssword.c_str(), m_kPssword.size());
            //std::string kNickName = (m_kWeiXinUserInfo.nickname);
            //strncpy(kRegister.szNickName, kNickName.c_str(), kNickName.size());
            m_kLoginMission.registerServer(kRegister);
            if (HNGameManager.bWeChatLogonIn == false)
            {
                //游客登陆
                Loom.QueueOnMainThread(() =>
                {
                    PlayerPrefs.SetString("UserName", m_kAccount);
                    PlayerPrefs.SetString("Uid", m_kUid);
                    PlayerPrefs.SetString("Openid", m_kOpenid);
                    PlayerPrefs.SetString("Psd", m_kPssword);
                    PlayerPrefs.SetString("NickName", m_kNickName);
                    PlayerPrefs.SetInt("Sex", m_bMale ? 1 : 0);
                    //PlayerPrefs.SetString("HeadURL", m_headURL);
                    PlayerPrefs.Save();
                });
            }
        }

        /*----------------IGPLoginMissionSink------------------------*/
        //Login
        public void LogOn()
        {
            bool bHasAccount = false;
            if (HNGameManager.bWeChatLogonIn)
            {
                bHasAccount = PlayerPrefs.GetString("UserName_WX") == EventHandle.m_kAccount &&
                    PlayerPrefs.GetString("Uid_WX") == EventHandle.m_kUid;
            }
            else
            {
                bHasAccount = PlayerPrefs.HasKey("UserName");
            }
            if (HNGameManager.bWeChatLogonIn)  //SDK登录
            {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
             Loom.QueueOnMainThread(() =>
            {
                m_kAccount = EventHandle.m_kAccount;
                m_kUid = EventHandle.m_kUid;
                m_kNickName = CreateNickName.GetInstance.RandomName();
                m_ChannelID = (ushort)EventHandle.m_channelID;

                PlayerPrefs.SetString("UserName_WX", m_kAccount);
                PlayerPrefs.SetString("Uid_WX", m_kUid);
                PlayerPrefs.SetString("Openid_WX", m_kOpenid);
                PlayerPrefs.SetString("Psd_WX", m_kPssword);
                PlayerPrefs.SetString("NickName_WX", m_kNickName);
                PlayerPrefs.SetInt("Sex_WX", m_bMale ? 1 : 0);
                PlayerPrefs.Save();

                var loginAccount = new CMD_GP_LogonAccounts();
                loginAccount.Init();
                loginAccount.dwPlazaVersion = DF.shared().GetPlazaVersion();
                loginAccount.cbValidateFlags = MsgDefine.MB_VALIDATE_FLAGS | MsgDefine.LOW_VER_VALIDATE_FLAGS;

                var tempBuf = System.Text.Encoding.Default.GetBytes(m_kAccount);
                Buffer.BlockCopy(tempBuf, 0, loginAccount.szAccounts, 0, tempBuf.Length);
                tempBuf = System.Text.Encoding.Default.GetBytes(m_kUid);
                Buffer.BlockCopy(tempBuf, 0, loginAccount.szUid, 0, tempBuf.Length);
                tempBuf = System.Text.Encoding.Default.GetBytes(m_kOpenid);
                Buffer.BlockCopy(tempBuf, 0, loginAccount.szOpenid, 0, tempBuf.Length);

                tempBuf = System.Text.Encoding.Default.GetBytes(m_kPssword);
                Buffer.BlockCopy(tempBuf, 0, loginAccount.szPassword, 0, tempBuf.Length);
                m_kLoginMission.loginAccount(loginAccount);
            });               
#endif
            }
            else   //PC游客登录，仅供测试
            {
                //游客登陆，或者有历史账号信息
                //GameManagerBaseNet pTemp = GameManagerBaseNet.InstanceBase();

                if (bHasAccount)  //存在PC账号记录
                {

                    m_kAccount = PlayerPrefs.GetString("UserName", m_kAccount);
                    m_kUid = PlayerPrefs.GetString("Uid", m_kUid);
                    m_kOpenid = PlayerPrefs.GetString("Openid", m_kOpenid);
                    m_kPssword = PlayerPrefs.GetString("Psd", m_kPssword);
                    m_kNickName = PlayerPrefs.GetString("NickName", m_kNickName);
                    m_bMale = PlayerPrefs.GetInt("Sex", m_bMale ? 1 : 0) == 1;

                }

                var loginAccount = new CMD_GP_LogonAccounts();
                loginAccount.Init();
                loginAccount.dwPlazaVersion = DF.shared().GetPlazaVersion();
                loginAccount.cbValidateFlags = MsgDefine.MB_VALIDATE_FLAGS | MsgDefine.LOW_VER_VALIDATE_FLAGS;
                var tempBuf = System.Text.Encoding.Default.GetBytes(m_kAccount);
                Buffer.BlockCopy(tempBuf, 0, loginAccount.szAccounts, 0, tempBuf.Length);

                tempBuf = System.Text.Encoding.Default.GetBytes(m_kUid);
                Buffer.BlockCopy(tempBuf, 0, loginAccount.szUid, 0, tempBuf.Length);
                tempBuf = System.Text.Encoding.Default.GetBytes(m_kOpenid);
                Buffer.BlockCopy(tempBuf, 0, loginAccount.szOpenid, 0, tempBuf.Length);

                tempBuf = System.Text.Encoding.Default.GetBytes(m_kPssword);
                Buffer.BlockCopy(tempBuf, 0, loginAccount.szPassword, 0, tempBuf.Length);
                m_kLoginMission.loginAccount(loginAccount);

                ////mChen add, to set pGlobalUserData.szHeadHttp
                //GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
                //tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
                /////string strHeadimgurl = "http:\\s1";
                //var kHeadimgurl = Encoding.Default.GetBytes(m_headURL);
                //Buffer.BlockCopy(kHeadimgurl, 0, pGlobalUserData.szHeadHttp, 0, kHeadimgurl.Length);
                //pGlobalUserInfo.upPlayerInfo();
            }
        }
    }
}
