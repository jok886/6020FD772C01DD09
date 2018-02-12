using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

namespace GameNet
{
    //////////////////////////////////////////////////////////////////////////
    // 大厅聊天
    //////////////////////////////////////////////////////////////////////////

    class CGPLobbyMission : CSocketMission
    {
        // 单例
        private static CGPLobbyMission _instance;
        public static CGPLobbyMission CreateInstance(byte[] url, int port)
        {
            if (_instance == null)
            {
                _instance = new CGPLobbyMission(url, port);
            }

            return _instance;
        }
        public static CGPLobbyMission GetInstance()
        {
            return _instance;
        }

        public enum Type
        {
            MISSION_NULL,
            MISSION_SEND_CHAT_DATA,
        };

        Type mMissionType;        // 任务类型
        string m_chatText;

        private LobbyChatSystem mLobbyChatSystem;

        public CGPLobbyMission(byte[] url, int port) : base(url, port)
        {
            mMissionType = Type.MISSION_NULL;

            mLobbyChatSystem = null;
        }

        // 设置回调接口
        public void setLobbyChatSystem(LobbyChatSystem lobbyChatSystem)
        {
            mLobbyChatSystem = lobbyChatSystem;
        }
        public bool isLobbyChatSystemSetted()
        {
            return mLobbyChatSystem != null;
        }

        //执行抽奖
        public void SendChatData(string chatText)
        {
            m_chatText = chatText;

            mMissionType = Type.MISSION_SEND_CHAT_DATA;
            start();
        }

        //////////////////////////////////////////////////////////////////////////
        // ISocketEngineSink

	    public override void onEventTCPSocketLink()
        {
            //变量定义


            switch (mMissionType)
            {  
                case Type.MISSION_SEND_CHAT_DATA:
                    {
                        CMD_GP_CHAT kNetInfo = new CMD_GP_CHAT();
                        kNetInfo.Init();

                        GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
                        tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
                        kNetInfo.dwUserID = pGlobalUserData.dwUserID;

                        Buffer.BlockCopy(pGlobalUserData.szNickName, 0, kNetInfo.szNickName, 0, pGlobalUserData.szNickName.Length);

                        var szChatData = Encoding.UTF8.GetBytes(m_chatText);
                        Buffer.BlockCopy(szChatData, 0, kNetInfo.szChatData, 0, szChatData.Length);

                        Buffer.BlockCopy(pGlobalUserData.szHeadHttp, 0, kNetInfo.szHeadHttp, 0, pGlobalUserData.szHeadHttp.Length);

                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_LOBBY_CHAT, buf, buf.Length);
        
                        break;
                    }

                default:
                    stop();
                    break;
            }
        }

        public override void onEventTCPSocketShut()
        {
            Debug.Log("CGPLobbyMission onEventTCPSocketShut");
        }

        public override void onEventTCPSocketError(Exception errorCode)
        {
            Debug.Log("CGPLobbyMission exception: " + errorCode.Message);
        }

        public override bool onEventTCPSocketRead(int main, int sub, byte[] data, int dataSize)
        {
            if (main != MsgDefine.MDM_GP_USER_SERVICE)
            {
                return false;
            }

            switch (sub)
            {
                //签到结果
                case MsgDefine.SUB_GP_LOBBY_CHAT:
                    {
                        return onSubGPLobbyChat(data, dataSize);
                    }
            }

            return false;
        }
        
        private bool onSubGPLobbyChat(byte[] data, int size)
        {
            CMD_GP_CHAT pNetInfo = (CMD_GP_CHAT)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GP_CHAT));

            if (mLobbyChatSystem != null)
            {
                mLobbyChatSystem.onSubGPLobbyChat(pNetInfo);
            }

            ///stop();

            return true;
        }

    }; 

}