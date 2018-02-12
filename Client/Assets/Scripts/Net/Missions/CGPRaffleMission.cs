using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

namespace GameNet
{
    //////////////////////////////////////////////////////////////////////////
    // 抽奖任务
    //////////////////////////////////////////////////////////////////////////
    class CGPRaffleMission : CSocketMission
    {
        public enum Type
        {
            MISSION_NULL,
            MISSION_RAFFLE
        };

        Type mMissionType;        // 任务类型
        uint m_dwRaffleGold;      // 抽到的钻石
        uint m_dwAwardGlod;       // 奖励金额
        byte m_cbCostType;        // 奖励类型

        public CGPRaffleMission(byte[] url, int port) : base(url, port)
        {
            mMissionType = Type.MISSION_NULL;
        }

        //执行抽奖
        public void raffle(uint dwRaffleGold)
        {
            m_dwRaffleGold = dwRaffleGold;

            mMissionType = Type.MISSION_RAFFLE;
            start();
        }

        //////////////////////////////////////////////////////////////////////////
        // ISocketEngineSink

	    public override void onEventTCPSocketLink()
        {
            //变量定义
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

            switch (mMissionType)
            {       // 查询
                case Type.MISSION_RAFFLE:
                    {
                        CMD_GP_RaffleDone kNetInfo = new CMD_GP_RaffleDone();
                        kNetInfo.Init();

                        kNetInfo.dwUserID = pGlobalUserData.dwUserID;
                        kNetInfo.dwRaffleGold = m_dwRaffleGold;

                        Buffer.BlockCopy(pGlobalUserData.szPassword, 0, kNetInfo.szPassword, 0, pGlobalUserData.szPassword.Length);
                        ///strncpy(kNetInfo.szPassword, pGlobalUserData->szPassword, countarray(kNetInfo.szPassword));

                        var MachineID = DF.shared().GetMachineID();
                        if (MachineID != null)
                        {
                            Buffer.BlockCopy(MachineID, 0, kNetInfo.szMachineID, 0, MachineID.Length);
                        }
                        ///strncpy(kNetInfo.szMachineID, DF::shared()->GetMachineID(), countarray(kNetInfo.szMachineID));

                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_RAFFLE_DONE, buf, buf.Length);
                        ///send(MDM_GP_USER_SERVICE, SUB_GP_RAFFLE_DONE, &kNetInfo, sizeof(CMD_GP_CheckInDone));

                        break;
                    }
                default:
                    stop();
                    break;
            }
        }

        public override void onEventTCPSocketShut()
        {
            Debug.Log("CGPRaffleMission onEventTCPSocketShut");
        }

        public override void onEventTCPSocketError(Exception errorCode)
        ///public override void onEventTCPSocketError(int errorCode)
        {
            Debug.Log("CGPRaffleMission exception: " + errorCode.Message);
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
                case MsgDefine.SUB_GP_RAFFLE_RESULT:
                    {
                        return onSubRaffleResult(data, dataSize);
                    }
            }

            return false;
        }
        
        private bool onSubRaffleResult(byte[] data, int size)
        {
            CMD_GP_RaffleResult pNetInfo = (CMD_GP_RaffleResult)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GP_RaffleResult));
            ///CMD_GP_RaffleResult* pNetInfo = (CMD_GP_RaffleResult*)data;

            string strLog = GlobalUserInfo.GBToUtf8(pNetInfo.szNotifyContent);
            //string strLog2 = Encoding.Default.GetString(pNetInfo.szNotifyContent);
            //string strLog3 = Encoding.GetEncoding(936).GetString(pNetInfo.szNotifyContent);
            //string strLog4 = Encoding.UTF8.GetString(pNetInfo.szNotifyContent);
            Debug.Log("CGPRaffleMission::onSubRaffleResult:" + strLog);
            GameSceneUIHandler.ShowLog(strLog);

            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            if (pNetInfo.bSuccessed != 0)
            {
                //Success

                pGlobalUserData.lUserInsure = pNetInfo.lScore;
                pGlobalUserData.dwPlayCount = pNetInfo.dwPlayCount;
                pGlobalUserData.dwRaffleCount = pNetInfo.dwRaffleCount;

                pGlobalUserInfo.upPlayerInfo();
            }

            stop();

            //if (mIGPSignInMissionSink != null)
            //{
            //    mIGPSignInMissionSink.onSignInDoneResult(pNetInfo);
            //}

            return true;
        }
    }; 

}