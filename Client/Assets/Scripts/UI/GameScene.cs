using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
//using UnityEngine.PostProcessing;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Collections;

namespace GameNet
{
    public class FinalJieSuanInfo
    {
        public String strNickName;
        public uint nId;
        public String[] sActionTxt = new String[5];
        public int nTotalScore;

        public FinalJieSuanInfo()
        {
            strNickName = "";
        }
    };

    class GameScene : GameBase

    {
        public int myruntime = 0;
        public static int KIND_ID; //游戏ID mChen const static int KIND_ID = 310; 311 312
        public static int SERVER_TYPE = SocketDefines.GAME_GENRE_EDUCATE; //mChen add, 房间类型 GAME_GENRE_EDUCATE, GAME_GENRE_MATCH, GAME_GENRE_GOLD
        public static bool IS_MATCH_SIGNUP; //mChen add,比赛报名

        public static uint VERSION_SERVER = GameServerDefines.VERSION_MOBILE_IOS; //程序版本
        public static uint VERSION_CLIENT = GameServerDefines.VERSION_MOBILE_IOS;
        public static int MAX_PLAYER = HNMJ_Defines.GAME_PLAYER;

        public const int KIND_ID_JianDe = 311;
        public const int KIND_ID_13Shui = 312;

        //语言 = 888；表情 = 999
        public const int TYPE_LANS = 888;
        public const int TYPE_EMOS = 999;

        private GameObject m_fxPiao = null;
        private GameObject m_fxBaoTou = null;


        public enum MJState
        {
            HNMJ_STATE_NULL,
            HNMJ_STATE_READY,
            HNMJ_STATE_XIAO_HU,
            HNMJ_STATE_PLAYING,
        };

        public GameScene(HNGameManager ma) : base(ma)
        {
            init();
        }

        public virtual bool init()
        {
            //mChen add, clear UI info, fix UI bug after 解散房间
            Loom.QueueOnMainThread(() =>
            {
                hnManager.ResetPlayersUIInfo();
                hnManager.DefaultState();
            });

            initPrivate();

            initButton();
            initNet();
            initTouch();

            initLanguageAndEmoji(); //add Language And Emoji
            for (int i = 0; i < MAX_PLAYER; i++)
            {
                m_pPlayer[i] = new HNMJPlayer(i, hnManager);
            }
            m_pLocal = m_pPlayer[0];
            return true;
        }

        void initPrivate()
        {
            m_bEnterGetUserInfo = true;
        }

        public void EnterScence()
        {
            defaultState();
        }

        public void HideAll()
        {
            defaultPrivateState();
        }

        public void defaultState()
        {
            //m_pTouchCardNode = NULL;

            for (int i = 0; i < MAX_PLAYER; i++)
            {
                m_pPlayer[i].defaultState();
                m_pPlayer[i].EndGame();
            }
            bSelfActioning = false;

            defaultPrivateState();
            defaultPlayerActionState();

            Loom.QueueOnMainThread(() =>
            {
                hnManager.DefaultState();
            });
        }

        public void defaultPlayerActionState()
        {

        }

        public void showSaiZi(uint iValue)
        {
            ushort wSice1 = (ushort)(iValue >> 16);
            ushort wSice2 = (ushort)(iValue);
        }

        public string getStringHuRight(uint kValue)
        {
            int[] dwRight = {
                HNMJLogic_Defines.CHR_BA_DUI                      ,									//八对X2
		        HNMJLogic_Defines.CHR_PIAO_CAI_YI                 ,									//飘财X4
		        HNMJLogic_Defines.CHR_PIAO_CAI_ER                 ,									//二次飘财X8
		        HNMJLogic_Defines.CHR_PIAO_CAI_SAN                ,									//三次飘财X16
		        HNMJLogic_Defines.CHR_GANG_KAI                    ,									//杠开X2
		        HNMJLogic_Defines.CHR_GANG_BAO                    ,									//杠暴
		        HNMJLogic_Defines.CHR_PIAO_GANG                   ,									//飘杠
		        HNMJLogic_Defines.CHR_GANG_PIAO                   ,									//杠飘
		        HNMJLogic_Defines.CHR_SHI_SAN_BU_DA               ,									//十三不搭X4
		        HNMJLogic_Defines.CHR_QING_YI_SE                  ,									//清一色X10
		        HNMJLogic_Defines.CHR_QING_FENG_ZI                ,                                  //清风子X20
		        HNMJLogic_Defines.CHR_QIANG_GANG_HU               ,									//抢杠胡X6
		        HNMJLogic_Defines.CHR_SHI_SAN_BU_DA_QIANG_GANG    ,									//十三不搭抢杠
		        HNMJLogic_Defines.CHR_QIANG_PIAO_GANG             ,									//抢飘杠
		        HNMJLogic_Defines.CHR_BA_DUI_ZI_PIAO_CAI          ,									//八对子飘财
		        HNMJLogic_Defines.CHR_QING_YI_SE_PIAO_CAI         ,									//清一色飘财
		        HNMJLogic_Defines.CHR_QING_BA_DUI_PIAO_CAI        ,									//清八对飘财
                HNMJLogic_Defines.CHR_BAO_TOU                     									//暴头X2
	        };

            String[] pszRight =
            {
                " 八对",
                " 飘财",
                " 二次飘财",
                " 三次飘财",
                " 杠开",
                " 杠暴",
                " 飘杠",
                " 杠飘",
                " 十三不搭",
                " 清一色",
                " 清风子",
                " 抢杠胡",
                " 十三不搭抢杠",
                " 抢飘杠",
                " 八对子飘财",
                " 清一色飘财",
                " 清八对飘财",
                " 暴头"
            };

            String kTxt = "";
            CChiHuRight kChiHuRight = new CChiHuRight();
            uint[] ValueRight = { kValue };
            kChiHuRight.SetRightData(ValueRight, HNMJLogic_Defines.MAX_RIGHT_COUNT);

            return kTxt;
        }

        private tagGangCardResult m_curGangResult;

        public void setCurrentPlayer(int iCurrentPlayer, int iUserAction, byte cbActionCard = 0, bool bForceAction = false)
        {
            if (m_pLocal.GetChairID() == iCurrentPlayer && iUserAction != HNMJLogic_Defines.WIK_NULL)
            {
                byte cbForceAction = (byte)(bForceAction ? 1 : 0);
                m_pLocal.AddCommand(new HNMJPlayer.PlayerUICommand(HNMJPlayer.PlayerAnimType.SetCurrentPlayer, new[] { (byte)iCurrentPlayer, (byte)iUserAction, cbActionCard, cbForceAction }));
            }
            else
            {
                handle_setCurrentPlayer(iCurrentPlayer, iUserAction, cbActionCard, bForceAction);
            }
        }

        public void handle_setCurrentPlayer(int iCurrentPlayer, int iUserAction, byte cbActionCard = 0, bool bForceAction = false)
        {
#if true
            if (iCurrentPlayer < 0 || iCurrentPlayer > MAX_PLAYER)
            {
                Debug.Assert(false, "setCurrentPlayer assert failed");
                return;
            }
            defaultPlayerActionState();

            m_iCurrentUser = iCurrentPlayer;

            //cocos2d::Node* pRootNode = WidgetFun::getChildWidget(this, "TimeNode");

            HNMJPlayer pPlyer = getPlayerByChairID(m_iCurrentUser);
            if (pPlyer == null)
            {
                return;
            }

            if (iUserAction == HNMJLogic_Defines.WIK_NULL)
            {
                //WidgetFun::setVisible(pRootNode, utility::toString("TimePoint", pPlyer.getIdex()), true);
            }

            Loom.QueueOnMainThread(() =>
            {
                hnManager.setCurrentPlayer(m_iCurrentUser);
            });
#endif
        }

        public void HideWeiXinInvite()
        {

        }


        public void showFinalJieSuanInfo(CMD_GF_Private_End_Info pNetInfo)
        {
            //以下都移到了上层调用函数CServerItem::OnSocketSubPrivateEnd中，因为CServerItem::OnGFGameClose后showFinalJieSuanInfo就不会被调用了

            //hnManager.m_cbGameEndReason = pNetInfo.cbEndReason;

            ////mChen add, for HideSeek
            //GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            //tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            //pGlobalUserData.cbMapIndexRand = pNetInfo.cbMapIndex;
            //pGlobalUserData.wRandseed = pNetInfo.wRandseed;
            //pGlobalUserData.wRandseedForRandomGameObject = pNetInfo.wRandseedForRandomGameObject;
            //pGlobalUserData.wRandseedForInventory = pNetInfo.wRandseedForInventory;

            ////mChen add, for HideSeek
            //UserInfo.getInstance().reqAccountInfo();
            //Loom.QueueOnMainThread(() =>
            //{
            //    Loom.QueueOnMainThread(() =>
            //    {
            //        if (hnManager != null)
            //        {
            //            GameObjectsManager.GetInstance().ClearPlayers();
            //            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
            //            if (kernel != null)
            //            {
            //                kernel.clearInfo();
            //            }

            //            if (pNetInfo.cbEndReason == HNMJ_Defines.GER_NORMAL)
            //            {
            //                hnManager.PlayAgain();
            //            }
            //        }
            //    }, 3.0f);//delay to call PlayAgain,保证所有客户端都结束后才调用PlayAgain和ClearPlayers，防止误RemovePlayers
            //});

            return;
        }

        public void showJieSuanInfo(CMD_S_GameEnd pGameEnd, bool bNormalEnd)
        {
        }


        public void PlayHuInfoSound(int wHuUser, uint dwChiHuRigh, bool isZimo)
        {
        }

        public void initButton()
        {
        }

        public void HNMJButtonAction_ShowCard()
        {
            setCurrentPlayer(m_iCurrentUser, HNMJLogic_Defines.WIK_NULL);
        }

        public void HNMJButton_GuoAction()
        {
            CMD_C_OperateCard OperateCard = new CMD_C_OperateCard();
            OperateCard.cbOperateCode = HNMJLogic_Defines.WIK_NULL;
            OperateCard.cbOperateCard = 0;
            var buf = StructConverterByteArray.StructToBytes(OperateCard);
            SendSocketData(HNMJ_Defines.SUB_C_OPERATE_CARD, buf, (ushort)buf.Length);
        }

        public void OnHNMJButton_GangYaoShaiZi_GuoAction()
        {
        }

        public void OnHNMJButton_GangYaoShaiZi_YaoAction()
        {
        }

        public void HNMJButton_GangAction(byte cbActionCard)
        {
            CMD_C_OperateCard OperateCard = new CMD_C_OperateCard();
            OperateCard.cbOperateCode = HNMJLogic_Defines.WIK_GANG;
            OperateCard.cbOperateCard = cbActionCard;
            var buf = StructConverterByteArray.StructToBytes(OperateCard);
            SendSocketData(HNMJ_Defines.SUB_C_OPERATE_CARD, buf, (ushort)buf.Length);
        }

        public void HNMJButton_PengAction()
        {
            CMD_C_OperateCard OperateCard = new CMD_C_OperateCard();
            OperateCard.cbOperateCode = HNMJLogic_Defines.WIK_PENG;
            OperateCard.cbOperateCard = 0;
            var buf = StructConverterByteArray.StructToBytes(OperateCard);
            SendSocketData(HNMJ_Defines.SUB_C_OPERATE_CARD, buf, (ushort)buf.Length);
        }

        public void HNMJButton_ChiAction_Left()
        {
            byte nChiAction = (byte)HNMJLogic_Defines.WIK_LEFT;
            ChiAction(nChiAction);
        }

        public void HNMJButton_ChiAction_Center()
        {
            byte nChiAction = (byte)HNMJLogic_Defines.WIK_CENTER;
            ChiAction(nChiAction);
        }

        public void HNMJButton_ChiAction_Right()
        {
            byte nChiAction = (byte)HNMJLogic_Defines.WIK_RIGHT;
            ChiAction(nChiAction);
        }

        public void ChiAction(byte nChiAction)
        {
            CMD_C_OperateCard OperateCard = new CMD_C_OperateCard();
            OperateCard.cbOperateCode = nChiAction;

            OperateCard.cbOperateCard = 0;
            var buf = StructConverterByteArray.StructToBytes(OperateCard);
            SendSocketData(HNMJ_Defines.SUB_C_OPERATE_CARD, buf, (ushort)buf.Length);
        }

        public void HNMJButton_HuAction()
        {
            Debug.Log("-------------------onClick HuAction");
            handle_HNMJButton_HuAction();
        }

        public void handle_HNMJButton_HuAction()
        {
        }

        public void HNMJButton_XiaoHuAction()
        {
        }

        public void HNMJButton_Ready()
        {
            sendReady();
            defaultState();
        }

        //add Language and Emoji

        public void initLanguageAndEmoji()
        {
        }

        public void hideLanguageAndEmoji()
        {
        }

        public void showLanguageAndEmoji()
        {
        }

        public void showPlane(int type, int iIdex, int chairID)
        {
            if (type == TYPE_LANS)
                showLans(iIdex, chairID);
            else
                showEmos(iIdex, chairID);
        }

        public void Button_LanAndEmo()
        {
            showLanguageAndEmoji();
        }
        public void showLans(int lIdex, int chairID)
        {
            Loom.QueueOnMainThread(() =>
            {
                hnManager.ShowLans(chairID, lIdex);
            });
        }

        public void Button_LansShow(int lansIndex)
        {
            showPlane(TYPE_LANS, lansIndex, m_pLocal.GetChairID());

            //mChen
            CMD_GF_C_UserChatIdx UserChat = new CMD_GF_C_UserChatIdx();
            UserChat.wItemIndex = (ushort)lansIndex;
            UserChat.wSendUserID = m_pLocal.GetChairID();
            var buf = StructConverterByteArray.StructToBytes(UserChat);
            CServerItem.get()
                .SendSocketData(GameServerDefines.MDM_GF_FRAME, GameServerDefines.SUB_GF_USER_CHAT_INDEX, buf,
                    (ushort)buf.Length);
        }

        public override bool OnSubUserChatIndex(byte[] data, int dataSize)
        {
            var typeValue = typeof(CMD_GF_C_UserChatIdx);
            if (dataSize != Marshal.SizeOf(typeValue)) return false;

            //消息处理
            CMD_GF_C_UserChatIdx pUserChatIdx =
                (CMD_GF_C_UserChatIdx)StructConverterByteArray.BytesToStruct(data, typeValue);
            ushort localChairId = m_pLocal.GetChairID();
            if (pUserChatIdx.wSendUserID != localChairId)
            {
                showPlane(TYPE_LANS, pUserChatIdx.wItemIndex, pUserChatIdx.wSendUserID);
            }

            return true;
        }

        public override bool OnSubUserExpressionIndex(byte[] data, int dataSize)
        {
            //效验参数
            var typeValue = typeof(CMD_GF_C_UserExpressionIdx);
            if (dataSize != Marshal.SizeOf(typeValue)) return false;

            //消息处理
            CMD_GF_C_UserExpressionIdx pUserExpressionIdx =
                (CMD_GF_C_UserExpressionIdx)StructConverterByteArray.BytesToStruct(data, typeValue);
            ushort localChairId = m_pLocal.GetChairID();
            if (pUserExpressionIdx.wSendUserID != localChairId)
            {
                showPlane(TYPE_EMOS, pUserExpressionIdx.wItemIndex, pUserExpressionIdx.wSendUserID);
            }

            return true;
        } //mChen

        public void showEmos(int eIdex, int chairID)
        {
            Loom.QueueOnMainThread(() =>
            {
                hnManager.ShowExpression(chairID, eIdex);
            });
        }

        public void Button_EmosShow(int emoIdex)
        {
            showPlane(TYPE_EMOS, emoIdex, m_pLocal.GetChairID());

            //mChen
            CMD_GF_C_UserExpressionIdx UserExpression = new CMD_GF_C_UserExpressionIdx();

            UserExpression.wItemIndex = (ushort)emoIdex;
            UserExpression.wSendUserID = m_pLocal.GetChairID();
            var buf = StructConverterByteArray.StructToBytes(UserExpression);
            CServerItem.get().SendSocketData(GameServerDefines.MDM_GF_FRAME, GameServerDefines.SUB_GF_USER_EXPRESSION_INDEX, buf, (ushort)buf.Length);
        }
        // public void Button_SetPlane(cocos2d::Ref*, WidgetUserInfo*);
        public void setPlane(int pIdex)
        {
        }

        //public static int KIND_LAN_EMO; //语言和表情ID 

        public override bool OnEventSceneMessage(byte cbGameStatus, bool bLookonUser, byte[] data, int dataSize)
        {
            if (hnManager != null)
            {
                hnManager.m_bIsToHallByDisconnect = false;
                hnManager.HideOfflineWaitingUI();
            }

            //fix服务端在处理SUB_GF_GAME_OPTION时先发送GAME_SCENE，后发送GAME_STATUS，导致这里的cbGameStatus是老的值
            cbGameStatus = data[0];

            Debug.Log("flow.CClientKernel::OnSocketSubGameScene4: cbGameStatus=" + cbGameStatus);

            if (cbGameStatus == SocketDefines.GAME_STATUS_FREE || cbGameStatus == SocketDefines.GAME_STATUS_WAIT)
            {
                OnFreeScence(data, dataSize);
                CServerItem.get().SetGameStatus(cbGameStatus);
                setGameState(MJState.HNMJ_STATE_NULL);
            }
            else if (cbGameStatus == SocketDefines.GAME_STATUS_PLAY || cbGameStatus == SocketDefines.GAME_STATUS_HIDE)
            {
                OnPlayScence(data, dataSize);
                CServerItem.get().SetGameStatus(cbGameStatus);
                setGameState(MJState.HNMJ_STATE_PLAYING);
            }
            else
            {
                Debug.LogError("OnEventSceneMessage: cbGameStatus error!!! cbGameStatus=" + cbGameStatus);
                Debug.Assert(false, "OnEventSceneMessage: cbGameStatus error!");
            }

            return true;
        }

        void OnFreeScence(byte[] data, int dataSize)
        {
            var typeValue = typeof(CMD_S_StatusFree);
            int localDataSize = Marshal.SizeOf(typeValue);
            if (dataSize != localDataSize)
            {
                Debug.LogError("OnFreeScence：localDataSize=" + localDataSize + ", serverDataSize=" + dataSize);
                return;
            }

            CMD_S_StatusFree pNetInfo = (CMD_S_StatusFree)StructConverterByteArray.BytesToStruct(data, typeValue);

            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            pGlobalUserData.cbMapIndexRand = pNetInfo.cbMapIndex;
            pGlobalUserData.wRandseed = pNetInfo.wRandseed;
            pGlobalUserData.wRandseedForRandomGameObject = pNetInfo.wRandseedForRandomGameObject;
            pGlobalUserData.wRandseedForInventory = pNetInfo.wRandseedForInventory;
            //道具同步
            Array.Copy(pNetInfo.sInventoryList, pGlobalUserData.sInventoryList, pNetInfo.sInventoryList.Length);
            //Buffer.BlockCopy(pNetInfo.sInventoryList, 0, pGlobalUserData.sInventoryList, 0, pNetInfo.sInventoryList.Length);

            Loom.QueueOnMainThread(() =>
            {
                hnManager.LoadHideSeekSceneOfWangHu();
            });
        }

        void OnPlayScence(byte[] data, int dataSize)
        {
            //旁观者或者重连

            var typeValue = typeof(CMD_S_StatusPlay);
            int localDataSize = Marshal.SizeOf(typeValue);
            if (dataSize != localDataSize)
            {
                Debug.LogError("OnPlayScence：localDataSize=" + localDataSize + ", serverDataSize=" + dataSize);
                return;
            }

            CMD_S_StatusPlay pNetInfo = (CMD_S_StatusPlay)StructConverterByteArray.BytesToStruct(data, typeValue);

            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            pGlobalUserData.cbMapIndexRand = pNetInfo.cbMapIndex;
            pGlobalUserData.wRandseed = pNetInfo.wRandseed;
            pGlobalUserData.wRandseedForRandomGameObject = pNetInfo.wRandseedForRandomGameObject;
            pGlobalUserData.wRandseedForInventory = pNetInfo.wRandseedForInventory;
            //道具同步
            Array.Copy(pNetInfo.sInventoryList, pGlobalUserData.sInventoryList, pNetInfo.sInventoryList.Length);
            //Buffer.BlockCopy(pNetInfo.sInventoryList, 0, pGlobalUserData.sInventoryList, 0, pNetInfo.sInventoryList.Length);

            //旁观者
            int nStatus = SocketDefines.US_NULL;
            IClientUserItem pMeItem = CServerItem.get().GetMeUserItem();
            if (pMeItem != null)
            {
                nStatus = pMeItem.GetUserStatus();
            }

            if (nStatus == SocketDefines.US_LOOKON) 
            {
                Loom.QueueOnMainThread(() =>
                {
                    hnManager.LoadHideSeekSceneOfWangHu();
                });
            }
            else
            {
                //mChen add, temp
                Loom.QueueOnMainThread(() =>
                {
                    hnManager.LoadHideSeekSceneOfWangHu();
                });

                if(false)//if (!hnManager.bEnteredGameScene)
                {
                    //没进入游戏场景

                    //强杀进程后进来

                    Debug.LogError("OnPlayScence : 在大厅重连?强杀进程后回来?  cbGameStatus=" + pNetInfo.cbGameStatus + " UserStatus=" + nStatus + " bEnteredGameScene=" + hnManager.bEnteredGameScene);

                    //在大厅重连?
                    Loom.QueueOnMainThread(() =>
                    {
                        //hnManager.LoadHideSeekSceneOfWangHu();
                        //hnManager.SetLoading(false);

                        hnManager.StartOrStopGameSceneHeartBeat(false);
                        hnManager.LeaveRoom();
                        hnManager.LeaveGameToHall();
                        //CServerItem.get().IntermitConnect(true);
                    });
                }
            }

            //重连

            byte cbGameStatus = pNetInfo.cbGameStatus;
            Debug.Log("OnPlayScence : cbGameStatus=" + cbGameStatus + " UserStatus=" + nStatus + " bEnteredGameScene=" + hnManager.bEnteredGameScene);

            //mChen add, for HideSeek:
            //if (nStatus != SocketDefines.US_LOOKON && !hnManager.bEnteredGameScene)
            //{
            //    //在大厅重连?
            //    StartOrStopGameSceneHeartBeat(false);
            //    CServerItem.get().IntermitConnect(true);
            //    hnManager.LeaveRoom();
            //}

            defaultState();
        }

        //mChen add, for HideSeek WangHu
        public void OnSocketGFSubHeartBeat(byte[] data, ushort wDataSize)
        {
            Loom.QueueOnMainThread(() =>
            {
                GameManager gameMgr = GameManager.GetInstance();
                if (gameMgr != null)
                {
                    gameMgr.OnSubHeartBeat_WangHu(data, wDataSize);
                }
            });

            return;
        }
        public void OnSocketGFSubAICreate(byte[] data, ushort wDataSize)
        {
            if (hnManager != null)
            {
                hnManager.OnSocketGFSubAICreate(data, wDataSize);
            }
            //Loom.QueueOnMainThread(() =>
            //{
            //    GameManager gameMgr = GameManager.GetInstance();
            //    if (gameMgr != null)
            //    {
            //        gameMgr.OnSubAICreateInfo_WangHu(data, wDataSize);
            //    }
            //});

            return;
        }

        public HNMJPlayer getPlayerByChairID(int iChairID)
        {
            if (iChairID < 0 || iChairID > MAX_PLAYER) iChairID = 0;
            if (m_pLocal.getUserItem(false) == null)
            {
                return m_pLocal;
            }
            int iIdex = (m_pLocal.GetChairID() - iChairID + MAX_PLAYER) % MAX_PLAYER;
            return m_pPlayer[iIdex];
        }

        public HNMJPlayer getPlayerByIndex(int iIdex)
        {
            if (iIdex >= 0 && iIdex < MAX_PLAYER)
            {
                return m_pPlayer[iIdex];
            }
            else
            {
                return null;
            }
        }

        protected override GamePlayer CreatePlayer(IClientUserItem pIClientUserItem)
        {
            lock (HNGameManager.LockObjOfLoadScene)
            {
                int nChairID = pIClientUserItem.GetChairID();

                if (pIClientUserItem.GetUserID() == UserInfo.getInstance().getUserID())
                {
                    m_pLocal.setUserItem(pIClientUserItem);

                    Debug.Log("---------------------CreatePlayer m_pLocal");

                    //mChen add, for HideSeek
                    Loom.QueueOnMainThread(() =>
                    {
                        GameObjectsManager.s_LocalHumanTeamType = PlayerTeam.PlayerTeamType.TaggerTeam;
                        //GameObjectsManager.s_LocalHumanTeamType = PlayerTeam.PlayerTeamType.HideTeam; //测试
                        HNGameManager.m_iLocalChairID = nChairID;

                        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                        {
                            PlayerTeam.PlayerTeamType teamType = pIClientUserItem.GetTeamType();// PlayerTeam.PlayerTeamType.TaggerTeam;
                            //PlayerTeam.PlayerTeamType teamType = PlayerTeam.PlayerTeamType.HideTeam; //测试
                            GameObjectsManager.s_LocalHumanTeamType = teamType;

                            byte cbModelIndex = pIClientUserItem.GetModelIndex();

                            PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(teamType);
                            team.AddAPlayer(false, nChairID, cbModelIndex);
                        }
                    });

                    return m_pLocal;
                }
                else
                {
                    GamePlayer pPlayer = getPlayerByChairID(pIClientUserItem.GetChairID());
                    pPlayer.setUserItem(pIClientUserItem);

                    //mChen add, for HideSeek
                    Loom.QueueOnMainThread(() =>
                    {
                        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
                        {
                            PlayerTeam.PlayerTeamType teamType = pIClientUserItem.GetTeamType();//PlayerTeam.PlayerTeamType.TaggerTeam;
                            //PlayerTeam.PlayerTeamType teamType = PlayerTeam.PlayerTeamType.HideTeam; //测试
                            byte cbModelIndex = pIClientUserItem.GetModelIndex();

                            PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(teamType);
                            team.AddAPlayer(false, nChairID, cbModelIndex);
                        }
                    });

                    return pPlayer;
                }
            }
        }

        protected override void DeletePlayer(GamePlayer pPlayer)
        {
            //null
        }

        public override void upSelfPlayerInfo()
        {
            //null
        }

        public void OnGFGameClose(int iExitCode)
        {
            ExitGame();
        }

        public void OnEventUserStatus(GamePlayer pPlayer)
        {
        }

        //私人场
        public void defaultPrivateState()
        {
        }

        public override void OnSocketSubPrivateRoomInfo(CMD_GF_Private_Room_Info pNetInfo)
        {
            Loom.QueueOnMainThread((() =>
            {
                hnManager.SetRoomInfo(ref pNetInfo);
            }));
            setPlayCount(pNetInfo.dwPlayCout);
        }

        public override void OnSocketSubPrivateEnd(CMD_GF_Private_End_Info pNetInfo)
        {
            showFinalJieSuanInfo(pNetInfo);
        }

        public override void OnSocketSubPrivateDismissInfo(CMD_GF_Private_Dismiss_Info pNetInfo)
        {
            Loom.QueueOnMainThread(() =>
            {
                hnManager.OnSocketSubPrivateDismissInfo(pNetInfo);
            });
        }

        //void XZDDButton_WeiXinFriend(cocos2d::Ref*, WidgetUserInfo*);
        // void XZDDButton_WeiXinImagic(cocos2d::Ref*, WidgetUserInfo*);
        //void ButtonPlayerHeadClick(cocos2d::Ref*, WidgetUserInfo* pUserInfo);
        public void initNet()
        {
            Debug.Log("initNet");

            addNetCB(HNMJ_Defines.SUB_S_CHAT_PLAY, this, OnSubChatData, "OnSubChatData");
            addNetCB(HNMJ_Defines.SUB_S_GAME_START, this, OnSubGameStart, "OnSubGameStart");
            addNetCB(HNMJ_Defines.SUB_S_OUT_CARD, this, OnSubOutCard, "OnSubOutCard");
            addNetCB(HNMJ_Defines.SUB_S_SEND_CARD, this, OnSubSendCard, "OnSubSendCard");
            addNetCB(HNMJ_Defines.SUB_S_OPERATE_NOTIFY, this, OnSubOperateNotify, "OnSubOperateNotify");
            addNetCB(HNMJ_Defines.SUB_S_OPERATE_RESULT, this, OnSubOperateResult, "OnSubOperateResult");
            addNetCB(HNMJ_Defines.SUB_S_GAME_END, this, OnSubGameEnd, "OnSubGameEnd");
            addNetCB(HNMJ_Defines.SUB_S_TRUSTEE, this, OnSubTrustee, "OnSubTrustee");
            addNetCB(HNMJ_Defines.SUB_S_CHI_HU, this, OnSubUserChiHu, "OnSubUserChiHu");
            addNetCB(HNMJ_Defines.SUB_S_GANG_SCORE, this, OnSubGangScore, "OnSubGangScore");
            addNetCB(HNMJ_Defines.SUB_S_REPLACE_CARD, this, OnSubReplaceCard, "OnSubReplaceCard");

            //mChen add, for HideSeek
            addNetCB(HNMJ_Defines.SUB_S_HideSeek_HeartBeat, this, OnSocketGFSubHeartBeat, "OnSocketGFSubHeartBeat");
            addNetCB(HNMJ_Defines.SUB_S_HideSeek_AICreateInfo, this, OnSocketGFSubAICreate, "OnSocketGFSubAICreate");

            //WQ 游戏中奖励
            addNetCB(HNMJ_Defines.SUB_S_AWARD_RESULT, this, OnSubAwardData, "OnSubAwardData");
            addNetCB(HNMJ_Defines.SUB_S_CONSUMPTION_INVENTORY_RESULT, this, OnSubInventoryConsumption, "OnSubInventoryConsumption");
            addNetCB(HNMJ_Defines.SUB_S_CONSUMPTION_INVENTORY_EVENT, this, OnSubInventoryConsumptionEvent, "OnSubInventoryConsumptionEvent");
        }
        #region  游戏中使用消费道具
        public void SendInventoryConsumption(byte ItemID, ushort wAwardGlod = 10, byte cbCostType = 0)
        {
            Debug.LogWarning("------------SendInventoryConsumption");
            CMD_C_InventoryConsumptionInfo kNetInfo = new CMD_C_InventoryConsumptionInfo();

            kNetInfo.dwUserID = GlobalUserInfo.getUserID();
            kNetInfo.cbItemID = ItemID;
            kNetInfo.wAmount = wAwardGlod;
            kNetInfo.cbCostType = cbCostType;
            //发送数据
            var buf = StructConverterByteArray.StructToBytes(kNetInfo);
            SendSocketData(HNMJ_Defines.SUB_C_CONSUMPTION_INVENTORY, buf, (ushort)buf.Length);
        }
        public void OnSubInventoryConsumption(byte[] pBuffer, ushort wDataSize)
        {
            Debug.LogWarning("------------OnSubInventoryConsumption");
            var typeValue = typeof(CMD_S_InventoryConsumptionInfoResult);
            if (wDataSize != Marshal.SizeOf(typeValue))
            {
                Debug.LogError("数据长度不符合");
                return;
            }
            CMD_S_InventoryConsumptionInfoResult InfoResult = (CMD_S_InventoryConsumptionInfoResult)StructConverterByteArray.BytesToStruct(pBuffer, typeValue);
            string str = GlobalUserInfo.GBToUtf8(InfoResult.szDescribeString);
            Debug.Log("onSubInventoryConsumptionResult: " + str);
            if (InfoResult.cbSuccess == 0)  //成功
            {
                long ScoreNow = 0;
                if (InfoResult.cbCostType == 0) //金币
                {
                    ScoreNow = GlobalUserInfo.getUserScore();
                    GlobalUserInfo.setUserScore(InfoResult.dwFinalScore);
                }
                else                            //钻石
                {
                    ScoreNow = GlobalUserInfo.getUserInsure();
                    GlobalUserInfo.setUserInsure(InfoResult.dwFinalScore);
                }

                Loom.QueueOnMainThread(() =>
                {
                    if (InfoResult.cbItemID == (byte)ControlManager.InventoryItemID.Stealth)  //开启隐身冷却时间
                    {
                        if (UIManager.GetInstance() != null)
                            UIManager.GetInstance().StartColdTime(ControlManager.GetInstance().StealthButton, 60);
                    }
                    if (UIManager.GetInstance() != null)
                    {
                        UIManager.GetInstance().UpdateUIInfo();   //更新钻石金币
                        if (InfoResult.cbCostType == 0) //金币
                        {
                            if (ScoreNow != GlobalUserInfo.getUserScore())
                                UIManager.GetInstance().ShowMiddleTips(str);
                        }
                        else                            //钻石
                        {
                            if (ScoreNow != GlobalUserInfo.getUserInsure())
                                UIManager.GetInstance().ShowMiddleTips(str);
                        }
                    }
                });
            }
            else
            {
                GameSceneUIHandler.ShowLog(str);
            }
        }
        public void OnSubInventoryConsumptionEvent(byte[] pBuffer, ushort wDataSize)
        {
            Debug.LogWarning("------------OnSubInventoryConsumptionEvent");
            var typeValue = typeof(CMD_S_InventoryConsumptionEvent);
            if (wDataSize != Marshal.SizeOf(typeValue))
            {
                Debug.LogError("数据长度不符合");
                return;
            }
            CMD_S_InventoryConsumptionEvent InfoResult = (CMD_S_InventoryConsumptionEvent)StructConverterByteArray.BytesToStruct(pBuffer, typeValue);
            if (InfoResult.cbChairID != HNGameManager.m_iLocalChairID)    //接收其他人的道具使用信息
            {
                PlayerBase player = GameObjectsManager.GetInstance().GetPlayerByChairID(InfoResult.cbChairID);
                if (player != null)
                {
                    switch ((ControlManager.InventoryItemID)InfoResult.cbItemID)
                    {
                        case ControlManager.InventoryItemID.ChangeModel: //变身
                            Loom.QueueOnMainThread(() => { ControlManager.GetInstance().ChangeModel(player.gameObject, InfoResult.dwModelIndex); });
                            break;
                        case ControlManager.InventoryItemID.Stealth: //隐身
                            {
                                if (InfoResult.cbStealthStatus == 0)
                                    Loom.QueueOnMainThread(() => { ControlManager.GetInstance().Stealth(player.gameObject, true); });
                                else if (InfoResult.cbStealthStatus == 1)
                                {
                                    Loom.QueueOnMainThread(() => { ControlManager.GetInstance().Stealth(player.gameObject, false); });
                                }
                            }
                            break;
                        case ControlManager.InventoryItemID.Resurrection: //复活
                            Loom.QueueOnMainThread(() => { ControlManager.GetInstance().Resurrection(player.gameObject); });
                            break;
                    }
                }
            }
            else    //接收自己的道具使用信息
            {
                //使用道具
                switch ((ControlManager.InventoryItemID)InfoResult.cbItemID)
                {
                    case ControlManager.InventoryItemID.ChangeModel: //变身
                        Loom.QueueOnMainThread(() => { ControlManager.GetInstance().ChangeModel(InfoResult.dwModelIndex); });
                        break;
                    case ControlManager.InventoryItemID.Stealth: //隐身
                        {
                            if (InfoResult.cbStealthStatus == 0)
                            {
                                Loom.QueueOnMainThread(() =>
                                {
                                    ControlManager.GetInstance().Stealth(true);
                                    ControlManager.GetInstance().StealthTime(255);
                                });
                            }
                            else if (InfoResult.cbStealthStatus == 1)
                            {
                                Loom.QueueOnMainThread(() =>
                                {
                                    ControlManager.GetInstance().Stealth(false);
                                    ControlManager.GetInstance().StealthTime(InfoResult.cbStealthTimeLeft);
                                });
                            }
                        }
                        break;
                    case ControlManager.InventoryItemID.Resurrection: //复活
                        Loom.QueueOnMainThread(() => { ControlManager.GetInstance().Resurrection(); });
                        break;
                }
            }
        }
        #endregion
        #region  游戏中奖励发送接收
        public void SendAwardData(uint dwAwardGlod = 10, byte cbCostType = 0)
        {
            Debug.LogWarning("------------SendAwardData");
            //变量定义
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

            CMD_C_AwardDone kNetInfo = new CMD_C_AwardDone();
            kNetInfo.Init();
            kNetInfo.dwUserID = pGlobalUserData.dwUserID;
            kNetInfo.dwAwardGold = dwAwardGlod;
            kNetInfo.cbCostType = cbCostType;
            Buffer.BlockCopy(pGlobalUserData.szPassword, 0, kNetInfo.szPassword, 0, pGlobalUserData.szPassword.Length);
            var buf = StructConverterByteArray.StructToBytes(kNetInfo);
            SendSocketData(HNMJ_Defines.SUB_C_AWARD_DONE, buf, (ushort)buf.Length);
        }
        public void OnSubAwardData(byte[] pBuffer, ushort wDataSize)
        {
            Debug.LogWarning("------------OnSubAwardData");
            var typeValue = typeof(CMD_S_AwardResult);
            if (wDataSize != Marshal.SizeOf(typeValue))
            {
                Debug.LogError("数据长度不符合");
                return;
            }
            CMD_S_AwardResult pNetInfo = (CMD_S_AwardResult)StructConverterByteArray.BytesToStruct(pBuffer, typeValue);
            string strLog = GlobalUserInfo.GBToUtf8(pNetInfo.szNotifyContent);
            Debug.Log("OnSubAwardData: " + strLog);
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            if (pNetInfo.bSuccessed == 0)  //成功
            {
                //Success
                if (pNetInfo.cbCostType == 0)  //奖励金币
                {
                    GlobalUserInfo.setUserScore(pNetInfo.lScore);
                    GameSceneUIHandler.ShowLog("奖励10枚金币");
                }
                else  //奖励钻石
                {
                    GlobalUserInfo.setUserInsure(pNetInfo.lScore);
                    GameSceneUIHandler.ShowLog("奖励10颗钻石");
                }
                Loom.QueueOnMainThread(() =>
                {
                    if (UIManager.GetInstance() != null)
                        UIManager.GetInstance().UpdateUIInfo();   //更新钻石金币
                });
            }
        }
        #endregion
        #region 游戏中聊天信息接发
        //发送聊天数据给服务器
        public void SendChatData(byte[] chatdata)
        {
            CMD_C_CHAT chat = new CMD_C_CHAT();
            chat.UserStatus = CServerItem.get().GetMeUserItem().GetUserStatus();
            chat.ChatData = chatdata;
            if (chat.ChatData.Length <= 100 && chat.ChatData.Length > 0)
            {
                var buf = StructConverterByteArray.StructToBytes(chat);
                SendSocketData(HNMJ_Defines.SUB_C_CHAT_PLAY, buf, (ushort)buf.Length);
            }
            else
                Debug.Log("文字数据不存在或过长");
        }
        //接收聊天数据
        public void OnSubChatData(byte[] pBuffer, ushort wDataSize)
        {
            var typeValue = typeof(CMD_C_CHAT);
            if (wDataSize != Marshal.SizeOf(typeValue))
            {
                Debug.Log("数据长度不符合");
                return;
            }
            CMD_C_CHAT ChatData = (CMD_C_CHAT)StructConverterByteArray.BytesToStruct(pBuffer, typeValue);
            string data = Encoding.GetEncoding(936).GetString(ChatData.ChatData);
            //Debug.Log("接收到的数据：" + data);
            int chairid = ChatData.ChairId;
            byte userstatus = ChatData.UserStatus;
            hnManager.ShowChatPanel(chairid, userstatus, data);
        }
        #endregion

        public void OnSubGameStart(byte[] pBuffer, ushort wDataSize)
        {

            var typeValue = typeof(CMD_S_GameStart);
            Debug.Log("建德游戏开始" + Marshal.SizeOf(typeValue));
            if (wDataSize != Marshal.SizeOf(typeValue))
                return;

            //变量定义
            CMD_S_GameStart pGameStart = (CMD_S_GameStart)StructConverterByteArray.BytesToStruct(pBuffer, typeValue);

            defaultState();

            m_iBankerUser = pGameStart.wBankerUser;
            m_iCurrentUser = pGameStart.wCurrentUser;
            m_iUserAction = pGameStart.cbUserAction;
            //WidgetFun::setText(this, "LastCardCout", (int)pGameStart.cbLeftCardCount);
            //WidgetFun::setVisible(this, "XZDDButton_WeiXinFriend", false);//mChen hack

            for (int i = 0; i < MAX_PLAYER; i++)
            {
                var player = getPlayerByChairID(i);
                player.setHandCard(pGameStart.cbCardData, i * HNMJ_Defines.MAX_COUNT, HNMJ_Defines.MAX_COUNT - 1);
                player.startGame();
            }

            Loom.QueueOnMainThread(() =>
            {
                //Lin: pGameStart.lSiceCount 服务端未设置，暂时rand一个
                hnManager.SetLeftCard(pGameStart.cbLeftCardCount);
                Random.state = new Random.State();
                var buf = new byte[(HNMJ_Defines.MAX_COUNT) * HNMJ_Defines.GAME_PLAYER];
                Buffer.BlockCopy(pGameStart.cbCardData, 0, buf, 0, buf.Length);
                hnManager.GameStartProcedure(Random.Range(2, 13), m_iBankerUser, buf);
                //hnManager.GameStartProcedure(pGameStart.lSiceCount, m_iBankerUser);
            });

            //HNMJButtonAction_ShowCard();
            getPlayerByChairID(m_iBankerUser).setZhuang();
            if (pGameStart.cbXiaoHuTag == 1)
            {
                setGameState(MJState.HNMJ_STATE_XIAO_HU);
            }
            else
            {
                setGameState(MJState.HNMJ_STATE_PLAYING);
            }
        }

        //用户出牌
        public void OnSubOutCard(byte[] pBuffer, ushort wDataSize)
        {
        }

        //发牌消息
        public void OnSubSendCard(byte[] pBuffer, ushort wDataSize)
        {
        }

        public void OnSubOperateNotify(byte[] pBuffer, ushort wDataSize)
        {
            var typeValue = typeof(CMD_S_OperateNotify);
            if (wDataSize != Marshal.SizeOf(typeValue))
                return;

            CMD_S_OperateNotify pOperateNotify = (CMD_S_OperateNotify)StructConverterByteArray.BytesToStruct(pBuffer, typeValue);

            handle_OnSubOperateNotify(pOperateNotify);
            ///m_pLocal.AddCommand( new HNMJPlayer.PlayerUICommand(HNMJPlayer.PlayerAnimType.OnSubOperateNotify, new[] { (byte)pOperateNotify.wResumeUser, pOperateNotify.cbActionMask, pOperateNotify.cbActionCard }) );
        }

        //操作提示
        public void handle_OnSubOperateNotify(CMD_S_OperateNotify pOperateNotify)
        {

        }

        private bool bSelfActioning = false;
        //操作结果
        public void OnSubOperateResult(byte[] pBuffer, ushort wDataSize)
        {
        }

        //补牌
        public void OnSubReplaceCard(byte[] pBuffer, ushort wDataSize)
        {
        }

        //游戏结束
        public void OnSubGameEnd(byte[] pBuffer, ushort wDataSize)
        {
            var typeValue = typeof(CMD_S_GameEnd);
            //if (wDataSize != Marshal.SizeOf(typeValue))
            //    return;

            //消息处理
            CMD_S_GameEnd pGameEnd = (CMD_S_GameEnd)StructConverterByteArray.BytesToStruct(pBuffer, typeValue);

            ////mChen add, for HideSeek WangHu
            //Loom.QueueOnMainThread(() =>
            //{
            //    hnManager.ReturnFromBigFinalToHallScene();
            //});
        }

        //用户托管
        public void OnSubTrustee(byte[] pBuffer, ushort wDataSize)
        {
            var typeValue = typeof(CMD_S_Trustee);
            if (wDataSize != Marshal.SizeOf(typeValue))
                return;

            //消息处理
            CMD_S_Trustee pTrustee = (CMD_S_Trustee)StructConverterByteArray.BytesToStruct(pBuffer, typeValue);

            // UI Log
            HNMJPlayer pPlyer = getPlayerByChairID(pTrustee.wChairID);
            if (pPlyer == null)
            {
                return;
            }
            String strNickName = pPlyer.GetNickName();
            String[] strName = strNickName.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            if (strName.Length > 0)
            {
                Loom.QueueOnMainThread(() =>
                {
                    ChatSystem.GetInstance.ShowChatText("通知", strName[0] + " 离开了房间！");
                });
            }
            Loom.QueueOnMainThread(() =>
            {
                PlayerBase player = GameObjectsManager.GetInstance().GetPlayerByChairID(pTrustee.wChairID);
                if (player != null)
                {
                    Debug.LogWarning("------------- " + player.ChairID);
                    GameObjectsManager.GetInstance().RemovePlayerByChairID(player.TeamType, (byte)player.ChairID);    //离开房间移除玩家
                }
            });
        }

        //public void ResetHandcardBeforeGameEnd()
        //{
        //    {
        //        byte[] cards = new byte[m_pLocal.m_kHandCardCout];
        //        Buffer.BlockCopy(m_pLocal.m_kHandCardData, 0, cards, 0, m_pLocal.m_kHandCardCout);
        //        hnManager.ResetLocalHandCardStuff(cards);
        //    }
        //}

        //吃胡消息
        public void OnSubUserChiHu(byte[] pBuffer, ushort wDataSize)
        {
        }

        //杠得分
        public void OnSubGangScore(byte[] pBuffer, ushort wDataSize)
        {
        }

        public void Command_PlaceBet(int iArea, int iBetScore)
        {
        }


        public void SendOutCard(byte Cardvalue)
        {
            if (m_nGameState != MJState.HNMJ_STATE_PLAYING) return;

            CMD_C_OutCard OutCard = new CMD_C_OutCard();
            OutCard.cbCardData = Cardvalue;//m_pLocal.getTouchCardVlaue(pCard);//temp set the card value!!Lin
            var buf = StructConverterByteArray.StructToBytes(OutCard);
            SendSocketData(HNMJ_Defines.SUB_C_OUT_CARD, buf, (ushort)buf.Length);
        }
        public void initTouch()
        {
        }

        private int preRemovedIndex = -1;
        public bool SendCard(int cardValue, int dataIndex)
        {
            if (m_pLocal.GetChairID() == m_iCurrentUser && getGameState() == MJState.HNMJ_STATE_PLAYING && (m_pLocal.m_bCanDirectSendCard))
            {
                preRemovedIndex = dataIndex;
                //if (m_kTouchSrcPos.y == HNMJPlayer::CARD_UP_POSY)
                {
                    SendOutCard(HNGameManager.GetCardValueByte(cardValue));
                }
                return true;
            }
            return false;
        }
        // bool ccTouchBegan(cocos2d::Vec2 kPos);
        //  void ccTouchMoved(cocos2d::Vec2 kPos);
        //  void ccTouchEnded(cocos2d::Vec2 kPos);

        public void setGameState(MJState nState)
        {
            m_nGameState = nState;
        }

        public MJState getGameState()
        {
            return m_nGameState;
        }

        public void setPlayCount(uint nCount)
        {
            m_nPlayCount = nCount;
        }

        public uint getPlayCount()
        {
            return m_nPlayCount;
        }

        public HNMJPlayer getLocalPlayer()
        {
            //mChen edit, for HideSeek 旁观者
            if (m_pLocal.GetUserID() == UserInfo.getInstance().getUserID())// if (m_pLocal.getUserItem(false) != null)
            {
                return m_pLocal;
            }
            else
            {
                //旁观者返回null
                return null;
            }
            ///return m_pLocal;
        }

        public int GetCurrentPlayerID()
        {
            return m_iCurrentUser;
        }

        protected int m_iBankerUser; //庄家用户
        protected int m_iCurrentUser; //当前用户
        protected int m_iUserAction; //玩家动作

        protected HNMJPlayer m_pLocal;
        protected HNMJPlayer[] m_pPlayer = new HNMJPlayer[MAX_PLAYER];

        //cocos2d::Node* m_pTouchCardNode;
        //cocos2d::Vec2 m_kTouchSrcPos;

        protected MJState m_nGameState;
        protected uint m_nPlayCount;
    };
}
