using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GameNet
{
    //////////////////////////////////////////////////////////////////////////

    interface ICGUserInGameServerInfoSink
    {
        void onUserInGameServerID(CMD_GP_InGameSeverID pNetInfo);
    };

    //////////////////////////////////////////////////////////////////////////

    class CGUserInGameServerInfo : CCallMission
{
        //函数定义
        //构造函数
        public CGUserInGameServerInfo(byte[] url, int port):base("CGUserInGameServerInfo",url,port)
        {
            m_pInGameServerInfoSink = null;
            addNetCall(Net_InGameServerID, MsgDefine.SUB_GP_QUERY_INGAME_SEVERID);
        }

      public  void setMissionSink(ICGUserInGameServerInfoSink pSink)
        {
            m_pInGameServerInfoSink = pSink;
        }

    //网络事件
        public void PerformInGameServerID(int iUserID)
        {
            addLinkCallStruct(CB_InGameServerID, iUserID);
            start();
        }

        public void CB_InGameServerID(int iUserID)
        {
            CMD_GP_UserInGameServerID kNetInfo = new CMD_GP_UserInGameServerID();
            kNetInfo.dwUserID = (uint)iUserID;
            var buf = StructConverterByteArray.StructToBytes(kNetInfo);
            send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_QUERY_INGAME_SEVERID, buf, buf.Length);
        }

        public void Net_InGameServerID(byte[] data, int dataSize)
        {
            if (dataSize != Marshal.SizeOf(typeof(CMD_GP_InGameSeverID))) return;

            CMD_GP_InGameSeverID pNetInfo = (CMD_GP_InGameSeverID)StructConverterByteArray.BytesToStruct(data,typeof(CMD_GP_InGameSeverID));

            if (m_pInGameServerInfoSink!=null)
            {
                m_pInGameServerInfoSink.onUserInGameServerID(pNetInfo);
            }
            stop();
        }

	// 回调
	private ICGUserInGameServerInfoSink m_pInGameServerInfoSink;
}; 
}
