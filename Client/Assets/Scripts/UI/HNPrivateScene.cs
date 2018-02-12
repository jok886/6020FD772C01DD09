using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameNet
{
    class HNPrivateScenceBase: IServerPrivateSink
    {
        private HNGameManager hnManager;
        public enum Type_LinkAction
        {
            Type_Link_NULL,
            Type_Link_Create,
            Type_Link_Join,
        };

        public HNPrivateScenceBase(HNGameManager ma)
        {
            m_iJoinCout = (0);
            m_eLinkAction = (Type_LinkAction.Type_Link_NULL);
            m_kCreatePrivateNet = new CMD_GR_Create_Private();
            m_kPrivateInfo = new CMD_GR_Private_Info();
            m_kPrivateRoomInfo = new CMD_GF_Private_Room_Info();

            CServerItem.get().SetServerPrivateSink(this);
            hnManager = ma;

        }

        public void upJoinNumTxt() { }

        public virtual void OnSocketSubPrivateInfo(CMD_GR_Private_Info pNetInfo)
        {
            Loom.QueueOnMainThread(()=>
            {
                m_kPrivateInfo = pNetInfo;

                if (m_eLinkAction == Type_LinkAction.Type_Link_Create)
                {
                    var buf = StructConverterByteArray.StructToBytes(m_kCreatePrivateNet);
                    CServerItem.get().SendSocketData(GameServerDefines.MDM_GR_PRIVATE, GameServerDefines.SUB_GR_CREATE_PRIVATE, buf, (ushort)buf.Length);
                    //zeromemory(&m_kCreatePrivateNet, sizeof(m_kCreatePrivateNet));
                    m_kCreatePrivateNet = new CMD_GR_Create_Private();
                }
                if (m_eLinkAction == Type_LinkAction.Type_Link_Join)
                {
                    CMD_GR_Join_Private kSendNet;
                    kSendNet.dwRoomNum = (uint)m_iJoinRoomId;//utility::parseInt(m_kJoinNumTxt); // lin Temp do this
                    kSendNet.cbGameTypeIdex = m_cbGameTypeIdex;//mChen add
                    kSendNet.cbChoosedModelIndex = (byte)PlayerPrefs.GetInt("ChoosedModelIndex");

                    Debug.Log("OnSocketSubPrivateInfo Type_Link_Join: m_cbGameTypeIdex=" + m_cbGameTypeIdex);

                    var buf = StructConverterByteArray.StructToBytes(kSendNet);
                    CServerItem.get().SendSocketData(GameServerDefines.MDM_GR_PRIVATE, GameServerDefines.SUB_GR_JOIN_PRIVATE, buf, (ushort)buf.Length);
                }
                m_eLinkAction = Type_LinkAction.Type_Link_NULL;
            });
           
        }
        public virtual void OnSocketSubPrivateCreateSuceess(CMD_GR_Create_Private_Sucess pNetInfo) { }
        public virtual void OnSocketSubPrivateRoomInfo(CMD_GF_Private_Room_Info pNetInfo)
        {
            m_kPrivateRoomInfo = pNetInfo;
            Debug.Log("Room Id: " + m_kPrivateRoomInfo.dwRoomNum);
        }
        public virtual void OnSocketSubPrivateEnd(CMD_GF_Private_End_Info pNetInfo) { }
        public virtual void OnSocketSubPrivateDismissInfo(CMD_GF_Private_Dismiss_Info pNetInfo)
        {

        }


        //加入房间
        public void Button_JoinRoom(int roomId)
        {
            PlayerPrefs.SetInt("PubOrPrivate", (int)RoomType.Type_Private);
            PlayerPrefs.Save();

            m_iJoinRoomId = roomId;
            var iServerId = roomId / 10000 - 10;
            CGameServerItem pGameServer = GameManagerBaseNet.InstanceBase().SearchGameServer(iServerId);

            if (pGameServer!=null && pGameServer.IsPrivateRoom())
            {
                GameManagerBaseNet.InstanceBase().connectGameServerByServerID(iServerId);
                m_eLinkAction = Type_LinkAction.Type_Link_Join;

                m_cbGameTypeIdex = HNGameManager.GameType;//mChen add

                Debug.Log("Button_JoinRoom: m_cbGameTypeIdex=" + m_cbGameTypeIdex);
            }
            else
            {
                //NoticeMsg::Instance().ShowTopMsgByScript("JoinRoomNumError");
                Debug.Log("Join Room error!!!");
            }

        }

        //创建房间
        public void Button_CreateRoom()
        {
            CMD_GR_Create_Private kSendNet = new CMD_GR_Create_Private();
            kSendNet.stHttpChannel = new byte[SocketDefines.LEN_NICKNAME];
            kSendNet.cbGameType = (byte)RoomType.Type_Private;
            kSendNet.bGameTypeIdex = GAME_TYPE_JianDe;//mChen kSendNet.bGameTypeIdex = GAME_TYPE_ZZ;
            kSendNet.cbPlayCostTypeIdex = 0;// hnManager.m_cbPlayCostTypeIdex;// 1000 * 0.05;//for金币房三种倍率
            kSendNet.lBaseScore = hnManager.m_baseScore;
            kSendNet.PlayerCount = HNMJ_Defines.GAME_PLAYER;        //游戏人数

            kSendNet.bGameRuleIdex = 130;
            kSendNet.bPlayCoutIdex = hnManager.m_cbPlayCoutIdex;
            kSendNet.cbChoosedMapIndex = (byte)MapChoose.Mapindex;//(byte)(MersenneTwister.MT19937.Int63() % 2);
            kSendNet.cbChoosedModelIndex = (byte)PlayerPrefs.GetInt("ChoosedModelIndex");

            PlayerPrefs.SetInt("PubOrPrivate", (int)RoomType.Type_Private);
            PlayerPrefs.Save();

            //mChen
            ConnectAndCreatePrivateByKindIDAndServerType(GameScene.KIND_ID_JianDe, SocketDefines.GAME_GENRE_EDUCATE, kSendNet);
        }

        //自由匹配
        public void Button_JoinRace()
        {
            CMD_GR_Create_Private kSendNet = new CMD_GR_Create_Private();
            kSendNet.stHttpChannel = new byte[SocketDefines.LEN_NICKNAME];
            kSendNet.cbGameType = (byte)RoomType.Type_Public;
            kSendNet.bGameTypeIdex = GAME_TYPE_JianDe;//mChen kSendNet.bGameTypeIdex = GAME_TYPE_ZZ;
            kSendNet.cbPlayCostTypeIdex = 0;// hnManager.m_cbPlayCostTypeIdex; ;// 1000 * 0.05;//for金币房三种倍率
            kSendNet.lBaseScore = 1;

            kSendNet.PlayerCount = HNMJ_Defines.GAME_PLAYER;

            kSendNet.bGameRuleIdex = 130;
            kSendNet.bPlayCoutIdex = 0;// hnManager.m_cbPlayCoutIdex;
            kSendNet.cbChoosedModelIndex = (byte)PlayerPrefs.GetInt("ChoosedModelIndex");

            PlayerPrefs.SetInt("PubOrPrivate", (int)RoomType.Type_Public);
            PlayerPrefs.Save();

            //mChen
            ConnectAndCreatePrivateByKindIDAndServerType(GameScene.KIND_ID_JianDe, SocketDefines.GAME_GENRE_EDUCATE, kSendNet);
        }
#if false // UI stuff
        
    void Button_JoinNumDel(cocos2d::Ref*, WidgetUserInfo*);
    void Button_JoinNumReset(cocos2d::Ref*, WidgetUserInfo*);

#endif
        public uint GetRoomPlayCout()
        {
            return m_kPrivateRoomInfo.dwPlayCout;
        }

        public void Button_DismissPrivate()
        {
            Loom.QueueOnMainThread((() =>
            {
                hnManager.PopDismissWindow(m_kPrivateRoomInfo.dwPlayCout);
            }));

            //if (m_kPrivateRoomInfo.dwPlayCout == 0)
            //{
            //    Loom.QueueOnMainThread((() =>
            //    {
            //        hnManager.PopDismissWindow(m_kPrivateRoomInfo.dwPlayCout);
            //    }));
            //    //PopScence::Instance().showAccessPlane(utility::getScriptString("PrivateTxt0"), this,
            //    //    button_selector(HNPrivateScenceBase::Button_SureeDismissPrivate), NULL);
            //    return;
            //}
            //else
            //{
            //    Button_SureeDismissPrivate();
            //}
        }

        public void Button_SureeDismissPrivate()
        {
            CMD_GR_Dismiss_Private kNetInfo = new CMD_GR_Dismiss_Private();
            kNetInfo.bDismiss = 1;
            var buf = StructConverterByteArray.StructToBytes(kNetInfo);
            CServerItem.get().SendSocketData(GameServerDefines.MDM_GR_PRIVATE, GameServerDefines.SUB_GR_PRIVATE_DISMISS, buf, (ushort)buf.Length);
        }

        public void Button_DismissPrivateNot()
        {
            CMD_GR_Dismiss_Private kNetInfo = new CMD_GR_Dismiss_Private();
            kNetInfo.bDismiss = 0;
            var buf = StructConverterByteArray.StructToBytes(kNetInfo);
            CServerItem.get().SendSocketData(GameServerDefines.MDM_GR_PRIVATE, GameServerDefines.SUB_GR_PRIVATE_DISMISS, buf, (ushort)buf.Length);
        }

        void ConnectAndCreatePrivateByKindID(int iKindID, CMD_GR_Create_Private kNet)
        {
            GameManagerBaseNet.InstanceBase().connectGameServerByKindID((ushort) iKindID);
            m_kCreatePrivateNet = kNet;
            // memcpy(&m_kCreatePrivateNet,&kNet,sizeof(kNet));
            m_eLinkAction = Type_LinkAction.Type_Link_Create;
        }

        public void ConnectAndCreatePrivateByKindIDAndServerType(int iKindID, ushort wServerType,  CMD_GR_Create_Private kNet)//mChen add
        {
            GameManagerBaseNet.InstanceBase().connectGameServerByKindIDAndServerType((ushort)iKindID, wServerType);
            //memcpy(&m_kCreatePrivateNet, &kNet, sizeof(kNet));
            m_kCreatePrivateNet = kNet;
            m_eLinkAction = Type_LinkAction.Type_Link_Create;
        }

        void ConnectAndCreatePrivateByServerID(int iServerID, CMD_GR_Create_Private kNet)
        {
            GameManagerBaseNet.InstanceBase().connectGameServerByServerID(iServerID);
            m_kCreatePrivateNet = kNet;
            //memcpy(&m_kCreatePrivateNet,&kNet,sizeof(kNet));
            m_eLinkAction = Type_LinkAction.Type_Link_Create;
        }


        public const int GAME_TYPE_JianDe = 0;
        public const int GAME_TYPE_13Shui = 13;
        public const int GAME_TYPE_Null = 101;

        protected int m_iJoinCout;
        //protected byte[] m_kJoinNumTxt = new byte[7];
        protected int m_iJoinRoomId;
        protected int m_kActJoinNum;

        protected CMD_GR_Private_Info m_kPrivateInfo;
        protected CMD_GF_Private_Room_Info m_kPrivateRoomInfo;

        protected CMD_GR_Create_Private m_kCreatePrivateNet;
        protected Type_LinkAction m_eLinkAction;

        //mChen add
        protected byte m_cbGameTypeIdex;
    };
}
