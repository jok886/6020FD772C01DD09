using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameNet
{
    public class GameBase : IClientKernelSink, IGPIndividualMissionSink
    {
        protected HNGameManager hnManager;

        public void onGPIndividualInfo(int type)
        {
        }

        public void onGPIndividualSuccess(int type, byte[] szDescription)
        {
        }

        public void onGPIndividualFailure(int type, byte[] szDescription)
        {
        }

        public struct NET_CBInfo
        {
            public int iIdex;
            public object pSrc1;
            public string kCallFunName;

            public delegate void Net_Callback(byte[] data, ushort dataSize);

            public Net_Callback pCallback;
        };

        public GameBase(HNGameManager ma)
        {
            m_kIndividualMission = new CGPIndividualMission(System.Text.Encoding.Default.GetBytes(LoginScene.m_strServerIP),
                LoginScene.m_nLogonServerPort);
            m_kIndividualMission.setMissionSink(this);
            hnManager = ma;
        }

        public void ExitGameBase()
        {
            if (CServerItem.get() != null)
            {
                CServerItem.get().PerformStandUpAction();
            }
            else
            {
                ExitGame();
            }
        }

        public void ExitGame()
        {
            clearInfo();
            m_kIndividualMission.stop();
            //ScenceManagerBase::InstanceBase().GameBackScence();
            Loom.QueueOnMainThread(() =>
            {
#if ApplyAutoReConnect
                ///if (GameScene.getGameState() == GameScene.MJState.HNMJ_STATE_PLAYING)
                if (hnManager.m_cbGameEndReason == HNMJ_Defines.GER_NOT_END)//if (hnManager.m_cbGameEndReason = HNMJ_Defines.GER_NOT_END; && hnManager.m_bRoomStartGame)
                {
                    hnManager.ShowOfflineUI(HNGameManager.m_iLocalChairID, true);
                    hnManager.ResetGameAfterDisconnect();
                }
                else
                {
                    HNGameManager.GameType = HNPrivateScenceBase.GAME_TYPE_Null;    //用于单人解散改gametype
                    hnManager.LeaveGameToHall();
                }
#else
                //enServiceStatus serviceStatus = CServerItem.get().GetServiceStatus();
                //var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                //if (kernel != null && kernel.getGameState() == GameScene.MJState.HNMJ_STATE_PLAYING && serviceStatus == enServiceStatus.ServiceStatus_NetworkDown)//UIState != GameUIState.UI_Starting)
                //{
                //}

                HNGameManager.GameType = HNPrivateScenceBase.GAME_TYPE_Null;    //用于单人解散改gametype
                hnManager.LeaveGameToHall();
#endif

            });
            //UserInfo.getInstance().reqAccountInfo();
            GameManagerBaseNet.InstanceBase().disconnectServer();
        }

        //控制接口
        //清理
        public void clearInfo()
        {
            while (m_kPlayers.Count > 0)
            {
                GamePlayer pTempPlayer = m_kPlayers[0];
                removeGamePlayerToList(pTempPlayer);
                DeletePlayer(pTempPlayer);
            }
        }

        //启动游戏
        public bool SetupGameClient()
        {
            return true;
        }

        //重置游戏
        public void ResetGameClient()
        {

        }

        //关闭游戏
        public void CloseGameClient()
        {
            ExitGame();
        }

        //框架事件

        //系统滚动消息
        public virtual bool OnGFTableMessage(byte[] szMessage)
        {
            return true;
        }

        //比赛信息
        public virtual bool OnGFMatchInfo(tagMatchInfo pMatchInfo)
        {
            return true;
        }

        //比赛等待提示
        public virtual bool OnGFMatchWaitTips(tagMatchWaitTip pMatchWaitTip)
        {
            return true;
        }

        //比赛结果
        public virtual bool OnGFMatchResult(tagMatchResult pMatchResult)
        {
            return true;
        }

        public virtual bool OnGFPlayerReady(int nChairID)
        {
            hnManager.ShowPlayerReadyUI(nChairID, true);

            return true;
        }

        //游戏事件
        //旁观消息
        public virtual bool OnEventLookonMode(byte[] data, int dataSize)
        {
            return true;
        }

        //场景消息
        public virtual bool OnEventSceneMessage(byte cbGameStatus, bool bLookonUser, byte[] data, int dataSize)
        {
            return true;
        }
        
        //场景消息
        public virtual bool OnEventGameMessage(int sub, byte[] data, int dataSize)
        {
            List<NET_CBInfo> kCallFun = new List<NET_CBInfo>();

            foreach (var netCbInfo in m_kCBInfoList)
            {
                if (sub == netCbInfo.iIdex)
                {
                    kCallFun.Add(netCbInfo);
                }
            }

            foreach (var netCbInfo in kCallFun)
            {
                netCbInfo.pCallback(data, (ushort) dataSize);
            }

            //if (HNGameManager.bFakeServer == false && HNGameManager.GameType == HNPrivateScenceBase.GAME_TYPE_JianDe)
            //{
            //    Loom.QueueOnMainThread(() =>
            //    {
            //        if (sub == HNMJ_Defines.SUB_S_GAME_START)
            //            PlayBackStorage.GetInstance().StartRecord(hnManager);
            //        else if (sub == HNMJ_Defines.SUB_S_GAME_END)
            //        {
            //            PlayBackStorage.GetInstance().RecordGameMsg(200, sub, data, dataSize);
            //            PlayBackStorage.GetInstance().StopRecord(data, dataSize);
            //            Debug.Log("Normal End");
            //            return;
            //        }
            //        PlayBackStorage.GetInstance().RecordGameMsg(200, sub, data, dataSize);
            //    });
            //}
            return true;
        }

        public void sendReady()
        {
            GameNet.CServerItem.get().SendUserReady(null, 0);
        }

        //发送函数
        public bool SendSocketData(ushort wSubCmdID)
        {
            return CServerItem.get().SendSocketData(GameServerDefines.MDM_GF_GAME, wSubCmdID);
        }

        //发送函数
        public bool SendSocketData(ushort wSubCmdID, byte[] data, ushort dataSize)
        {
            return CServerItem.get().SendSocketData(GameServerDefines.MDM_GF_GAME, wSubCmdID, data, dataSize);
        }

        public void addNetCB(int iIdex, object pScence, NET_CBInfo.Net_Callback pCallBack, string kCallName)
        {
            //mChen add temp
            foreach (var netCbInfo in m_kCBInfoList)
            {
                if (iIdex == netCbInfo.iIdex)
                {
                    Debug.LogError("--------------------GameBase::addNetCB, already had" + iIdex);
                }
            }

            NET_CBInfo kInfo = new NET_CBInfo();
            kInfo.iIdex = iIdex;
            kInfo.pCallback = pCallBack;
            kInfo.pSrc1 = pScence;
            kInfo.kCallFunName = kCallName;
            m_kCBInfoList.Add(kInfo);
        }

        //用户事件
        //用户进入
        public virtual void OnEventUserEnter(IClientUserItem pIClientUserItem, bool bLookonUser)
        {
            if (HNGameManager.LockObjOfLoadScene == null)
            {
                Debug.LogError("HNGameManager.LockObjOfLoadScene == null");
                HNGameManager.LockObjOfLoadScene = new object();
            }

            lock (HNGameManager.LockObjOfLoadScene)
            {
                GamePlayer pPlayer = getGamePlayerByUserItem(pIClientUserItem);
                if (pPlayer != null)
                {
                    pPlayer.upPlayerInfo();
                    return;
                }
            }

            if(CServerItem.get() == null)
            {
                Debug.LogError("OnEventUserEnter:ServerItem.get() == null");
                return;
            }

            IClientUserItem pMeItem = CServerItem.get().GetMeUserItem();
            if (pMeItem == null)
            {
                return;
            }
            if (pMeItem.GetTableID() != pIClientUserItem.GetTableID())
            {
                return;
            }

            if (pIClientUserItem.GetUserStatus() == SocketDefines.US_LOOKON)
            {
                return;
            }

            //UI Log
            string strNickName = GlobalUserInfo.GBToUtf8(pIClientUserItem.GetNickName());
            Debug.Log("---------------------OnEventUserEnter:" + strNickName);
            String[] strName = strNickName.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            if (strName.Length > 0)
            {
                Loom.QueueOnMainThread(() =>
                {
                    if(ChatSystem.GetInstance!=null)
                        ChatSystem.GetInstance.ShowChatText("通知", strName[0] + " 加入了房间！");
                });
            }

            if (CServerItem.get().GetMeUserItem() == pIClientUserItem)
            {
                Debug.Log("OnEventUserEnter:Local Player Enter");

                if (m_pSelfPlayer == null)
                {
                    Debug.Log("---------------------Disconnect back 2 ?: Try to Create Local Player");

                    //GameObject LocalTaggerObj = GameObject.Find("Player/TaggerTeam/LocalTagger");
                    //GameObject.Destroy(LocalTaggerObj);
                    //删除Human
                    //GameObjectsManager.GetInstance().ClearPlayers(false);

                    m_pSelfPlayer = CreatePlayer(pIClientUserItem);

                    Loom.QueueOnMainThread(() =>
                    {
                        if (hnManager==null)
                        {
                            hnManager = GameObject.FindObjectOfType<HNGameManager>();
                        }
                        if(hnManager != null)
                        {
                            if (m_pSelfPlayer != null)
                            {
                                int nChairId = m_pSelfPlayer.GetChairID();//m_pSelfPlayer
                                hnManager.LocalUserEnter(nChairId);
                            }
                        }
                    });

                    addGamePlayerToList(m_pSelfPlayer);
                }

                upSelfPlayerInfo();

                int iIdex = 0;
                while (true)
                {
                    IClientUserItem pTempUserItem = CServerItem.get().GetTableUserItem((ushort) iIdex);
                    iIdex++;
                    if (pTempUserItem == null)
                    {
                        break;
                    }
                    if (pTempUserItem.GetTableID() != pMeItem.GetTableID())
                    {
                        continue;
                    }
                    if (pTempUserItem.GetUserStatus() == SocketDefines.US_LOOKON)
                    {
                        continue;
                    }

                    if (m_bEnterGetUserInfo)
                    {
                        m_kIndividualMission.query((int) pIClientUserItem.GetUserID(), false);
                    }
                    if (pTempUserItem == pIClientUserItem)
                    {
                        continue;
                    }
                    GamePlayer pTempPlayer = CreatePlayer(pTempUserItem);
                    addGamePlayerToList(pTempPlayer);
                }
            }
            else
            {
                if (m_pSelfPlayer != null || pMeItem.GetUserStatus()==SocketDefines.US_LOOKON)
                {
                    GamePlayer pTempPlayer = CreatePlayer(pIClientUserItem);
                    addGamePlayerToList(pTempPlayer);
                }
            }
            if (m_bEnterGetUserInfo)
            {
                m_kIndividualMission.query((int) pIClientUserItem.GetUserID(), false);
            }
        }

        //用户离开
        public virtual void OnEventUserLeave(IClientUserItem pIClientUserItem, bool bLookonUser)
        {
            GamePlayer pPlayer = getGamePlayerByUserItem(pIClientUserItem);
            if (pPlayer != null)
            {
                //fix连续断线导致的多个LocalTagger和LocalHide
                if (pPlayer == m_pSelfPlayer)
                {
                    //断线时删除Human
                    GameObjectsManager.GetInstance().ClearPlayers(false);
                }

                removeGamePlayerToList(pPlayer);
                DeletePlayer(pPlayer);
            }
        }

        //用户积分
        public virtual void OnEventUserScore(IClientUserItem pIClientUserItem, bool bLookonUser)
        {
            GamePlayer pPlayer = getGamePlayerByUserItem(pIClientUserItem);
            if (pPlayer != null)
            {
                pPlayer.upPlayerInfo();
            }
            if (pPlayer == m_pSelfPlayer)
            {
                upSelfPlayerInfo();
            }
        }

        //用户状态
        public virtual void OnEventUserStatus(IClientUserItem pIClientUserItem, bool bLookonUser)
        {
            GamePlayer pPlayer = getGamePlayerByUserItem(pIClientUserItem);
            if (pPlayer != null)
            {
                pPlayer.upPlayerState();
                OnEventUserStatus(pPlayer);

                //设置离线状态UI
                Loom.QueueOnMainThread(() =>
                {
                    int nStatus = pPlayer.GetUserStatus();
                    bool bIsOffline = (nStatus == SocketDefines.US_OFFLINE || nStatus == SocketDefines.US_NULL);
                    hnManager.ShowOfflineUI(pPlayer.GetChairID(), bIsOffline);
                });
            }
        }

        //用户状态
        public virtual void OnEventUserStatus(GamePlayer pPlayer)
        {
        }

        //用户属性
        public virtual void OnEventUserAttrib(IClientUserItem pIClientUserItem, bool bLookonUser)
        {
            GamePlayer pPlayer = getGamePlayerByUserItem(pIClientUserItem);
            if (pPlayer != null)
            {
                pPlayer.upPlayerInfo();
            }
            if (pPlayer == m_pSelfPlayer)
            {
                upSelfPlayerInfo();
            }
        }

        //用户头像
        public virtual void OnEventCustomFace(IClientUserItem pIClientUserItem, bool bLookonUser)
        {
            GamePlayer pPlayer = getGamePlayerByUserItem(pIClientUserItem);
            if (pPlayer != null)
            {
                pPlayer.upPlayerInfo();
            }
        }

        public virtual void onGPAccountInfo(CMD_GP_UserAccountInfo pNetInfo)
        {

        }

        public virtual void onGPAccountInfoHttpIP(uint dwUserID, string strIP, string strHttp)
        {
            GamePlayer pPlayer = getPlayerByUserID(dwUserID);
            if (pPlayer == null)
            {
                return;
            }
            IClientUserItem pIClientUserItem = pPlayer.getUserItem(false);
            if (pIClientUserItem == null)
            {
                return;
            }

            //获取用户
            tagUserInfo pUserInfo = pIClientUserItem.GetUserInfo();
            tagCustomFaceInfo pCustomFaceInfo = pIClientUserItem.GetCustomFaceInfo();
            var cbLogonIP = Encoding.Default.GetBytes(strIP);
            pUserInfo.dwClientAddr = (uint)(cbLogonIP[0] | cbLogonIP[1] << 8 | cbLogonIP[2] << 16 | cbLogonIP[3] << 24);
            ///pUserInfo.szLogonIP = Encoding.Default.GetBytes(strIP);
            pUserInfo.szHeadHttp = Encoding.Default.GetBytes(strHttp);
            //  strncpy(pUserInfo.szLogonIP, strIP.c_str(), countarray(pUserInfo.szLogonIP));
            //  strncpy(pUserInfo.szHeadHttp, strHttp.c_str(), countarray(pUserInfo.szHeadHttp));

            pPlayer.upPlayerInfo();
        }

        //私人房

        public virtual void OnSocketSubPrivateRoomInfo(CMD_GF_Private_Room_Info pNetInfo)
        {

        }

        public virtual void OnSocketSubPrivateEnd(CMD_GF_Private_End_Info pNetInfo)
        {

        }


        public virtual bool RevTalkFile(byte[] data, int dataSize)
        {
            var typeValue = typeof (CMD_GR_C_TableTalk);
            Debug.Log("talk data size: " + dataSize);
            CMD_GR_C_TableTalk kTalkNet = (CMD_GR_C_TableTalk)StructConverterByteArray.BytesToStruct(data, typeValue);
            if (kTalkNet.cbChairID == m_pSelfPlayer.GetChairID())
            {
                return true;
            }
           
            int structSize = Marshal.SizeOf(typeValue);
            var lenbuf = new byte[4];
            Buffer.BlockCopy(data, structSize, lenbuf, 0, 4);
            int soundLen = dataSize - structSize - 4;
            Debug.Log("received sound data " + soundLen + "in " + BitConverter.ToInt32(lenbuf, 0));
            Debug.Assert(BitConverter.ToInt32(lenbuf, 0) == soundLen, "RevTalkFile failed");
            var soundData = new byte[soundLen];
            Buffer.BlockCopy(data, structSize + 4, soundData, 0, soundLen);
            Debug.Log("talk data size: " + dataSize + "structSize " + structSize);
            Loom.QueueOnMainThread(() =>
            {
                hnManager.UserTalk(kTalkNet.cbChairID, soundData);
            });
#if false
            datastream kDataStream(data, dataSize);
            CMD_GR_C_TableTalk kTalkNet;
            kTalkNet.StreamValue(kDataStream, false);
            if (kTalkNet.kDataStream.size() == 0)
            {
                return true;
            }
            static int iIdex = 0;
            iIdex++;
            std::string kFile = utility::toString(cocos2d::CCFileUtils::sharedFileUtils().getWritablePath(), "TableTalk", iIdex, ".arm");
            FILE* fp = fopen(kFile.c_str(), "wb");

            fseek(fp, 0, SEEK_END);
            fseek(fp, 0, SEEK_SET);
            fwrite(&kTalkNet.kDataStream[0], sizeof(unsigned char), kTalkNet.kDataStream.size(),fp);
            fclose(fp);

            std::string kDestFile = kFile;
            utility::StringReplace(kDestFile, "arm", "wav");
            ArmFun::ArmToWav(kFile.c_str(), kDestFile.c_str());

            //临时设置音量最大
            float effectSound = SoundFun::GetSoundEffect();

            if (SoundFun::GetSoundEffect() != 1)
                SoundFun::SetSoundEffect(1);

            SoundFun::playEffectDirect(kDestFile);
#endif
            return true;
        }

        public virtual void OnSocketSubPrivateDismissInfo(CMD_GF_Private_Dismiss_Info pNetInfo)
        {

        }

        public void sendTalkFile(byte[] soundData)
        {
            CMD_GR_C_TableTalk kNetInfo = new CMD_GR_C_TableTalk();
            kNetInfo.cbChairID = (byte) m_pSelfPlayer.GetChairID();
            var buf = StructConverterByteArray.StructToBytes(kNetInfo);
            var sendData = new byte[soundData.Length + buf.Length + 4];//lin: 4 是 uint的大小，是语音字节数目
            /*
             * datastream& pushValue(datastream& value)
	{
		push(int(value.size()));
		if (!value.size())
		{
			return *this;
		}
		memcpy(inc_size(value.size()), (void*)&value[0], value.size());
		return *this;
	}  
	datastream& popValue(datastream& value)
	{
		int nSize = 0;
		pop(nSize);
		if (nSize == 0)
		{
			return *this;
		}
		if (nSize > (int)size())
		{
			return *this;
		}
		std::vector<char>::iterator first=begin(), last=first+nSize;
		value.assign(first, last);
		erase(first, last);
		return *this;
	} 
             * */
            Buffer.BlockCopy(buf, 0, sendData, 0, buf.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(soundData.Length), 0, sendData, buf.Length, 4);
            Buffer.BlockCopy(soundData, 0, sendData, buf.Length + 4, soundData.Length);
            CServerItem.get().SendSocketData(GameServerDefines.MDM_GF_FRAME, GameServerDefines.SUB_GR_TABLE_TALK, sendData, (ushort)sendData.Length);
#if false
            ssize_t iSize = 0;
            std::string kDestFile = cocos2d::CCFileUtils::getInstance().getWritablePath() + "talk.arm";
            ArmFun::WavToArm(kFile.c_str(), kDestFile.c_str());
            CMD_GR_C_TableTalk kNetInfo;
            unsigned char* pData = cocos2d::CCFileUtils::sharedFileUtils().getFileData(kDestFile, "rb", &iSize);
            if (!pData)
            {
                return;
            }
            kNetInfo.cbChairID = iChair;
            kNetInfo.kDataStream.pushValue((char*)pData, iSize);
            free(pData);
            datastream kDataStream;
            kNetInfo.StreamValue(kDataStream, true);
            CServerItem.get().SendSocketData(MDM_GF_FRAME, SUB_GR_TABLE_TALK, &kDataStream[0], kDataStream.size());
#endif
        }

        //mChen
        public virtual bool OnSubUserChatIndex(byte[] data, int dataSize)
        {
            return true;
        }

        public virtual bool OnSubUserExpressionIndex(byte[] data, int dataSize)
        {
            return true;
        }


        protected GamePlayer getPoolPlayer(IClientUserItem pIClientUserItem)
        {
            GamePlayer pPlayer = null;
            if (m_kPoolPlayer.Count > 0)
            {
                pPlayer = m_kPoolPlayer.Last();
                m_kPoolPlayer.RemoveAt(m_kPoolPlayer.Count - 1);
                pPlayer.setUserItem(pIClientUserItem);
            }
            return pPlayer;
        }

        protected virtual GamePlayer CreatePlayer(IClientUserItem pIClientUserItem)
        {
            GamePlayer pPlayer = getPoolPlayer(pIClientUserItem);
            if (pPlayer != null)
            {
                return pPlayer;
            }
            return new GamePlayer(pIClientUserItem);
        }

        protected virtual void DeletePlayer(GamePlayer pPlayer)
        {
            if (m_kPoolPlayer.Contains(pPlayer))
            {
                return;
            }
            m_kPoolPlayer.Add(pPlayer);
        }

        public virtual void upSelfPlayerInfo()
        {
        }

        public GamePlayer getGamePlayerByUserItem(IClientUserItem pIClientUserItem)
        {
            foreach (var gamePlayer in m_kPlayers)
            {
                if (gamePlayer.getUserItem() == pIClientUserItem)
                {
                    return gamePlayer;
                }
            }

            return null;
        }

        public GamePlayer getPlayerByChairID(ushort wChairID)
        {
            foreach (var gamePlayer in m_kPlayers)
            {
                if (gamePlayer.GetChairID() == wChairID)
                {
                    return gamePlayer;
                }
            }
            return null;
        }

        public GamePlayer getPlayerByUserID(uint wUserID)
        {

            foreach (var gamePlayer in m_kPlayers)
            {
                if (gamePlayer.GetUserID() == wUserID)
                {
                    return gamePlayer;
                }
            }
            return null;
        }

        public void addGamePlayerToList(GamePlayer pPlayer)
        {
            if (m_kPlayers.Contains(pPlayer) == false)
            {
                m_kPlayers.Add(pPlayer);
            }
            pPlayer.PlayerEnter();
            pPlayer.upPlayerInfo();
            pPlayer.upPlayerState();
        }

        public void removeGamePlayerToList(GamePlayer pPlayer)
        {
            if (m_pSelfPlayer == pPlayer)
            {
                m_pSelfPlayer = null;
            }
            m_kPlayers.Remove(pPlayer);

            pPlayer.PlayerLeave();
            pPlayer.setUserItem(null);
        }

        public GamePlayer getSelfGamePlayer()
        {
            return m_pSelfPlayer;
        }

        public ushort getSelfChairID()
        {
            if (m_pSelfPlayer == null)
            {
                return 0;
            }
            return m_pSelfPlayer.GetChairID();
        }

        public bool IsPrivateGame()
        {
            return CServerItem.get().GetServerAttribute().wServerType == SocketDefines.GAME_GENRE_EDUCATE;
        }

        public bool IsMatchGame() //mChen
        {
            return CServerItem.get().GetServerAttribute().wServerType == SocketDefines.GAME_GENRE_MATCH;
        }

        protected GamePlayer m_pSelfPlayer;
        protected List<GamePlayer> m_kPlayers = new List<GamePlayer>();
        protected List<NET_CBInfo> m_kCBInfoList = new List<NET_CBInfo>();

        protected bool m_bEnterGetUserInfo;
        protected CGPIndividualMission m_kIndividualMission;
        protected List<GamePlayer> m_kPoolPlayer = new List<GamePlayer>();
    };
}
