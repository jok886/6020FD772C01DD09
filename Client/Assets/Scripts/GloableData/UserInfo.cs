using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameNet
{ 
    //typedef void (cocos2d::Ref::* QYSEL_CallFunc)();
//#define QY_CALLFUNC_SELECTOR(_SELECTOR) static_cast<QYSEL_CallFunc>(&_SELECTOR)
    public delegate void QYSEL_CallFunc();
    struct UserCallInfo
    {
        //cocos2d::Ref* pPoint;
        public object pPoint;
        public QYSEL_CallFunc pFun;
    };

    class UserInfo : GlobalUserInfoSink, ICGUserInGameServerInfoSink, IGPSignInMissionSink
    {
        //列表数据
        private static UserInfo __gUserInfo = null;

        //静态函数
        //获取对象
        public static UserInfo getInstance()
        {
            if (__gUserInfo == null)
                __gUserInfo = new UserInfo();
            return __gUserInfo;
        }

        public UserInfo()
        {
            m_kUpPlayerInfoCB = new List<UserCallInfo>();
            m_kLoginSucessCB = new List<UserCallInfo>();
            m_kUserInGameServerInfo =
                new CGUserInGameServerInfo(System.Text.Encoding.Default.GetBytes(LoginScene.m_strServerIP), LoginScene.m_nLogonServerPort);
            m_kIndividualMission = new CGPIndividualMission(System.Text.Encoding.Default.GetBytes(LoginScene.m_strServerIP),
                LoginScene.m_nLogonServerPort);
            GlobalUserInfo.GetInstance().setSink(this);
            m_kUserInGameServerInfo.setMissionSink(this);

            m_kSignInMission = new CGPSignInMission(System.Text.Encoding.Default.GetBytes(LoginScene.m_strServerIP), LoginScene.m_nLogonServerPort);
            m_kRaffleMission = new CGPRaffleMission(System.Text.Encoding.Default.GetBytes(LoginScene.m_strServerIP), LoginScene.m_nLogonServerPort);

            if (m_hnManager == null)
            {
                Loom.QueueOnMainThread(() =>
                {
                    m_hnManager = GameObject.FindObjectOfType<HNGameManager>();
                });
            }
        }

        public void reqAccountInfo(float fWaiteTime = 0.0f)
        {
            if (fWaiteTime > 0.01f)
            {
                Loom.QueueOnMainThread(on_reqAccountInfo, fWaiteTime);
            }
            else
            {
                on_reqAccountInfo();
            }
        }

        public void reqIndividual()
        {
            m_kIndividualMission.query((int) getUserID());
        }

        public void on_reqAccountInfo()
        {
            m_kIndividualMission.queryAccountInfo();
        }

        public tagGlobalUserData getUserData()
        {
            return GlobalUserInfo.GetInstance().GetGlobalUserData();
        }

        public tagUserInsureInfo GetUserInsureInfo()
        {
            return GlobalUserInfo.GetInstance().GetUserInsureInfo();
        }

        public string getUserNicName()
        {
            return GlobalUserInfo.getNickName();
        }
        public string getAccounts()
        {
            return GlobalUserInfo.getAccounts();
        }

        public long getUserScore()
        {
            return GlobalUserInfo.getUserScore();
        }

        public long getUserInsure()
        {
            return GlobalUserInfo.getUserInsure();
        }

        public uint getUserID()
        {
            return GlobalUserInfo.getUserID();
        }

        public uint getGameID()
        {
            return GlobalUserInfo.getGameID();
        }

        public byte getGender()
        {
            return GlobalUserInfo.getGender();
        }

        //mChen add
        public void modifyIndivHeadHttp(float fWaiteTime)
        {
            if (fWaiteTime > 0.01f)
            {
                Loom.QueueOnMainThread(() =>
                {
                    Loom.QueueOnMainThread(modeHeadHttp, fWaiteTime);
                });
                ///TimeManager::Instance().addCerterTimeCB(TIME_CALLBACK(UserInfo::modeHeadHttp, this), fWaiteTime);
            }
            else
            {
                modeHeadHttp();
            }
        }
        void modeHeadHttp()
        {
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            m_kIndividualMission.modifyHeadHttp(pGlobalUserData.szHeadHttp);
        }
        public void modeUesrNickName(string name)
        {
            m_kIndividualMission.modifyName(name);
        }
        public void modeHeadHttp(string strHttp)
        {
            var kHttp = Encoding.Default.GetBytes(strHttp);

            m_kIndividualMission.modifyHeadHttp(kHttp);
        }
        public void modeUserInfo(CMD_GP_ModUserInfo pData)
        {
            m_kIndividualMission.modUserInfo(pData);
        }
        public byte[] getHeadHttp()
        {
            return GlobalUserInfo.getHeadHttp();
        }

        public string getUserChannel()
        {
            return GlobalUserInfo.getUserChannel();
        }

        public string getUserIP()
        {
            return Encoding.Default.GetString(GlobalUserInfo.GetInstance().GetGlobalUserData().szLogonIP);
        }

        public void setPhoneNumber(string strNumber)
        {
            m_kIndividualMission.modifyPhoneNumber(strNumber);
        }

        public string getPhoneNumber()
        {
            return GlobalUserInfo.GetInstance().getPhoneNumber();
        }

        public void checkInGameServer()
        {
            m_kUserInGameServerInfo.PerformInGameServerID((int) getUserID());
        }

        // 修改推广人
        public void modifySpreader(uint dwSpreaderID)
        {
            m_kIndividualMission.modifySpreader(dwSpreaderID);
        }

        // mChen add
        //增加/删除推荐人身份
        public void addDelSpreader(uint dwSpreaderID, string szSpreaderRealName, string szSpreaderTelNum, string szSpreaderWeiXinAccount, uint dwParentSpreaderID, ushort wSpreaderLevel, bool bIsAddSpreader)
        {
            m_kIndividualMission.addDelSpreader(dwSpreaderID, szSpreaderRealName, szSpreaderTelNum, szSpreaderWeiXinAccount, dwParentSpreaderID, wSpreaderLevel, bIsAddSpreader);
        }
        //查询代理人列表
        public void querySpreadersInfo(Surrogate cSurrogate)
        {
            m_kIndividualMission.querySpreadersInfo(cSurrogate);
        }

        //mChen add
        //游戏内购
        public void AddPayment(uint dwPayment, uint dwBoughtDiamond)
        {
            m_kIndividualMission.AddPayment(dwPayment, dwBoughtDiamond);
        }
        //名下用户交易信息
        public void queryChildrenPaymentInfo(Surrogate cSurrogate)
        {
            m_kIndividualMission.queryChildrenPaymentInfo(cSurrogate);
        }
        //企业提现
        public void AddEnterprisePay(uint dwPayment)
        {
            m_kIndividualMission.AddEnterprisePay(dwPayment);
        }

        //mChen add, for HideSeek
        public void BoughtTaggerModel(uint dwPayment, byte cbPaymentType, ushort wBoughtModelIndex)
        {
            m_kIndividualMission.BoughtTaggerModel(dwPayment, cbPaymentType, wBoughtModelIndex);
        }

        //微信购买钻石
        public void queryPrePayID(uint dwShopItem)
        {
            m_kIndividualMission.queryPrePayIDByShopID(dwShopItem);
        }

        public void queryAddShopItem(CMD_GP_ShopItemInfo kNetInfo)
        {
            m_kIndividualMission.queryAddShopItem(kNetInfo);
        }
        public void requestExchangeScore(byte itemId, byte exchangeType, int amount)   //
        {
            m_kIndividualMission.requestExchangeScore(itemId, exchangeType, amount);
        }
        //查询比赛排行榜列表
        public void queryTopPlayersInfo(MatchScore cMS)
        {
            m_kIndividualMission.queryTopPlayersInfo(cMS);
        }

        //查询用户昵称
        public void queryNickName(uint userID, TransferDiamond td)
        {
            m_kIndividualMission.queryUserNickNameByUserID(userID, td);
        }

        //转房卡
        public void TransferDiamonds(uint diamondCount)
        {
            m_kIndividualMission.transferDiamond(diamondCount);
        }

        public void ClientPayInfo()
        {
            m_kIndividualMission.uploadPayInfo();
        }

        public void addLoginSucessCB(object pPoint, QYSEL_CallFunc pFun)
        {
            foreach (var userCallInfo in m_kLoginSucessCB)
            {
                if (userCallInfo.pPoint == pPoint)
                {
                    Debug.Assert(false, "addLoginSucessCB : Already added this object ");
                    return;
                }
            }

            UserCallInfo kCallInfo;
            kCallInfo.pPoint = pPoint;
            kCallInfo.pFun = pFun;
            m_kLoginSucessCB.Add(kCallInfo);
        }

        public void addUpPlayerInfoCB(object pPoint, QYSEL_CallFunc pFun)
        {
            foreach (var userCallInfo in m_kUpPlayerInfoCB)
            {
                if (userCallInfo.pPoint == pPoint)
                {
                    Debug.Assert(false, "addLoginSucessCB : Already added this object ");
                    return;
                }
            }
            UserCallInfo kCallInfo;
            kCallInfo.pPoint = pPoint;
            kCallInfo.pFun = pFun;
            m_kUpPlayerInfoCB.Add(kCallInfo);
        }

        public void delCallByPoint(object pPoint)
        {
            foreach (var userCallInfo in m_kLoginSucessCB)
            {
                if (userCallInfo.pPoint == pPoint)
                {
                    m_kLoginSucessCB.Remove(userCallInfo);
                    break;
                }
            }

            foreach (var userCallInfo in m_kUpPlayerInfoCB)
            {
                if (userCallInfo.pPoint == pPoint)
                {
                    m_kUpPlayerInfoCB.Remove(userCallInfo);
                    break;
                }
            }
        }

        public void upPlayerInfo()
        {
            foreach (var userCallInfo in m_kUpPlayerInfoCB)
            {
                userCallInfo.pFun();
            }
        }

        public void LoginSucess()
        {
            foreach (var userCallInfo in m_kLoginSucessCB)
            {
                userCallInfo.pFun();
            }
            //checkInGameServer();
        }

        public void onUserInGameServerID(CMD_GP_InGameSeverID pNetInfo)
        {
            Debug.Log("onUserInGameServerID: " + pNetInfo.LockServerID + "," + pNetInfo.LockKindID);
            if (pNetInfo.LockServerID != 0)
            {
                //Debug.Log("connectGameServerByServerID " + pNetInfo.LockServerID);
                GameManagerBaseNet.InstanceBase().connectGameServerByServerID((int)pNetInfo.LockServerID);
                Debug.Log("断线重连onUserInGameServerID:" + pNetInfo.LockKindID);
                if (pNetInfo.LockKindID == GameScene.KIND_ID_JianDe)
                {
                    GameScene.KIND_ID = GameScene.KIND_ID_JianDe;
                    HNGameManager.GameType = HNPrivateScenceBase.GAME_TYPE_JianDe;
                }
                else if (pNetInfo.LockKindID == GameScene.KIND_ID_13Shui)
                {
                    Debug.LogError("13shui断线重连");
                    GameScene.KIND_ID = GameScene.KIND_ID_13Shui;
                    HNGameManager.GameType = HNPrivateScenceBase.GAME_TYPE_13Shui;
                }
            }
            else
            {
                GameScene.KIND_ID = 0;
                HNGameManager.GameType = HNPrivateScenceBase.GAME_TYPE_Null;

                //mChen add, for HideSeek:强制离开
                //玩家没被锁在GameScoreLocker表中
                Debug.LogError("onUserInGameServerID: 玩家没被锁在GameScoreLocker表中");
                if (m_hnManager != null)
                {
                    if (m_hnManager.bEnteredGameScene)
                    {
                        if (CServerItem.get() != null)
                        {
                            CServerItem.get().IntermitConnect(true);
                        }
                        Loom.QueueOnMainThread(() =>
                        { 
                            m_hnManager.LeaveGameToHall(false);
                        });
                    }
                }
                else
                {
                    Debug.LogError("onUserInGameServerID: m_hnManager==null");
                }
            }
        }

        //mChen add
        /// <CGPSignInMission>
        ///    
        public void QuerySignIn()
        {
            if(m_kSignInMission != null)
            {
                m_kSignInMission.query();
            }
            
        }
        public void DoneSignIn()
        {
            if (m_kSignInMission != null)
            {
                m_kSignInMission.done();
            }
        }
        /// </CGPSignInMission>
        /// 
        /// <IGPSignInMissionSink>
        ///         
        public virtual void onSignInQueryInfoResult(CMD_GP_CheckInInfo pNetInfo)
        {

        }

        public virtual void onSignInDoneResult(CMD_GP_CheckInResult pNetInfo)
        {

        }
        /// </IGPSignInMissionSink>
        /// 

        //mChen add
        public void DoneRaffle(uint dwRaffleGold)
        {
            if (m_kRaffleMission != null)
            {
                m_kRaffleMission.raffle(dwRaffleGold);
            }
        }
        List<UserCallInfo> m_kUpPlayerInfoCB;
        List<UserCallInfo> m_kLoginSucessCB;
        CGPIndividualMission m_kIndividualMission;
        CGUserInGameServerInfo m_kUserInGameServerInfo;

        CGPSignInMission m_kSignInMission;
        CGPRaffleMission m_kRaffleMission;

        private HNGameManager m_hnManager;
    };
}
