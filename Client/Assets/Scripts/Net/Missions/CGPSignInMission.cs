using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

namespace GameNet
{
    interface IGPSignInMissionSink
    {
        void onSignInQueryInfoResult(CMD_GP_CheckInInfo pNetInfo);
        void onSignInDoneResult(CMD_GP_CheckInResult pNetInfo);
    };


    //////////////////////////////////////////////////////////////////////////
    // 签到任务
    //////////////////////////////////////////////////////////////////////////
    class CGPSignInMission : CSocketMission
    {
        public enum Type
        {
            BEGINNER_MISSION_NULL,
            BEGINNER_MISSION_QUERY,
            BEGINNER_MISSION_DONE,
            SIGNIN_MISSION_AWARD,
        };

        private IGPSignInMissionSink mIGPSignInMissionSink;
        Type mMissionType;        // 任务类型

        public CGPSignInMission(byte[] url, int port) : base(url, port)
        {
            mIGPSignInMissionSink = null;
            mMissionType = Type.BEGINNER_MISSION_NULL;
        }

        // 设置回调接口
        public void setMissionSink(IGPSignInMissionSink pIGPSignInMissionSink)
        {
            mIGPSignInMissionSink = pIGPSignInMissionSink;
        }

        //查询签到
        public void query()
        {
            mMissionType = Type.BEGINNER_MISSION_QUERY;
            start();
        }

        //签到
        public void done()
        {
            mMissionType = Type.BEGINNER_MISSION_DONE;
            start();
        }

        //领取奖励
        public void award()
        {
            mMissionType = Type.SIGNIN_MISSION_AWARD;
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
                case Type.BEGINNER_MISSION_QUERY:
                    {
                        CMD_GP_CheckInQueryInfo kNetInfo = new CMD_GP_CheckInQueryInfo();
                        kNetInfo.Init();
                        ///CMD_GP_CheckInQueryInfo kNetInfo;

                        kNetInfo.dwUserID = pGlobalUserData.dwUserID;
                        Buffer.BlockCopy(pGlobalUserData.szPassword, 0, kNetInfo.szPassword, 0, pGlobalUserData.szPassword.Length);
                        ///strncpy(kNetInfo.szPassword, pGlobalUserData.szPassword, countarray(kNetInfo.szPassword));

                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_CHECKIN_QUERY, buf, buf.Length);
                        ///send(MDM_GP_USER_SERVICE, SUB_GP_CHECKIN_QUERY, &kNetInfo, sizeof(CMD_GP_CheckInQueryInfo));

                        break;
                    }

                //执行签到
                case Type.BEGINNER_MISSION_DONE:
                    {
                        CMD_GP_CheckInDone kNetInfo = new CMD_GP_CheckInDone();
                        kNetInfo.Init();

                        kNetInfo.dwUserID = pGlobalUserData.dwUserID;

                        Buffer.BlockCopy(pGlobalUserData.szPassword, 0, kNetInfo.szPassword, 0, pGlobalUserData.szPassword.Length);
                        ///strncpy(kNetInfo.szPassword, pGlobalUserData->szPassword, countarray(kNetInfo.szPassword));

                        var MachineID = DF.shared().GetMachineID();
                        if(MachineID != null)
                        {
                            Buffer.BlockCopy(MachineID, 0, kNetInfo.szMachineID, 0, MachineID.Length);
                        }
                        ///strncpy(kNetInfo.szMachineID, DF::shared()->GetMachineID(), countarray(kNetInfo.szMachineID));

                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_CHECKIN_DONE, buf, buf.Length);
                        ///send(MDM_GP_USER_SERVICE, SUB_GP_CHECKIN_DONE, &kNetInfo, sizeof(CMD_GP_CheckInDone));

                        break;
                    }

                case Type.SIGNIN_MISSION_AWARD:
                    {
                        CMD_GP_CheckInDone kNetInfo = new CMD_GP_CheckInDone();
                        kNetInfo.Init();

                        kNetInfo.dwUserID = pGlobalUserData.dwUserID;

                        Buffer.BlockCopy(pGlobalUserData.szPassword, 0, kNetInfo.szPassword, 0, pGlobalUserData.szPassword.Length);
                        ///strncpy(kNetInfo.szPassword, pGlobalUserData->szPassword, countarray(kNetInfo.szPassword));

                        var MachineID = DF.shared().GetMachineID();
                        if (MachineID != null)
                        {
                            Buffer.BlockCopy(MachineID, 0, kNetInfo.szMachineID, 0, MachineID.Length);
                        }
                        ///strncpy(kNetInfo.szMachineID, DF::shared()->GetMachineID(), countarray(kNetInfo.szMachineID));

                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_CHECKIN_AWARD, buf, buf.Length);
                        ///send(MDM_GP_USER_SERVICE, SUB_GP_CHECKIN_AWARD, &kNetInfo, sizeof(CMD_GP_CheckInDone));

                        break;
                    }

                default:
                    stop();
                    break;
            }
        }

        public override void onEventTCPSocketShut()
        {
            Debug.Log("CGPSignInMission onEventTCPSocketShut");
        }

        public override void onEventTCPSocketError(Exception errorCode)
        ///public override void onEventTCPSocketError(int errorCode)
        {
            Debug.Log("CGPSignInMission exception: " + errorCode.Message);
        }

        public override bool onEventTCPSocketRead(int main, int sub, byte[] data, int dataSize)
        {
            if (main != MsgDefine.MDM_GP_USER_SERVICE)
            {
                return false;
            }
            switch (sub)
            {
                //签到信息
                case MsgDefine.SUB_GP_CHECKIN_INFO:
                    {

                        return onSubQueryInfoResult(data, dataSize);
                    }

                //签到结果
                case MsgDefine.SUB_GP_CHECKIN_RESULT:
                    {
                        return onSubDoneResult(data, dataSize);
                    }
            }

            return false;
        }

	    private bool onSubQueryInfoResult(byte[] data, int size)
        {
            if( size != Marshal.SizeOf(typeof(CMD_GP_CheckInInfo)) )
            ///if (size != sizeof(CMD_GP_CheckInInfo))
            {
                ///ASSERT(false);
                return false;
            }

            CMD_GP_CheckInInfo pNetInfo = (CMD_GP_CheckInInfo)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GP_CheckInInfo));
            ///CMD_GP_CheckInInfo* pNetInfo = (CMD_GP_CheckInInfo*)data;

            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            pGlobalUserData.wSeriesDate = pNetInfo.wSeriesDate;
            pGlobalUserData.bTodayChecked = (pNetInfo.bTodayChecked != 0);
            //pGlobalUserData.wSeriesDate++;//hack

            if (mIGPSignInMissionSink != null)
            {
                mIGPSignInMissionSink.onSignInQueryInfoResult(pNetInfo);
            }

            stop();


            return true;
        }

        private bool onSubDoneResult(byte[] data, int size)
        {
            CMD_GP_CheckInResult pNetInfo = (CMD_GP_CheckInResult)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GP_CheckInResult));
            ///CMD_GP_CheckInResult* pNetInfo = (CMD_GP_CheckInResult*)data;

            string strLog = GlobalUserInfo.GBToUtf8(pNetInfo.szNotifyContent); 
            Debug.Log("CGPSignInMission::onSubDoneResult:" + strLog);
            GameSceneUIHandler.ShowLog(strLog);

            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            if (pNetInfo.bSuccessed != 0)
            {
                //Success

                if (pNetInfo.bType == 0)
                {
                    //签到结果返回

                    pGlobalUserData.wSeriesDate = pNetInfo.wSeriesDate;
                    award();
                }
                else
                {
                    Loom.QueueOnMainThread(()=> { HNGameManager.GetInstance.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_SIGNGIN_AWARED); });
                }

                pGlobalUserData.lUserInsure = pNetInfo.lScore;
                pGlobalUserInfo.upPlayerInfo();
                //刷新UI
                Loom.QueueOnMainThread(()=> 
                {
                    if (CreateOrJoinRoom.GetInstance != null)
                        CreateOrJoinRoom.GetInstance.UpdateInfo();
                });

            }
            else
            {
                stop();
            }

            if(pNetInfo.bType == 0)
            {
                //签到结果返回
            }
            else
            {
                //签到奖励返回

                stop();
            }

            if (mIGPSignInMissionSink != null)
            {
                mIGPSignInMissionSink.onSignInDoneResult(pNetInfo);
            }

            return true;
        }
        private bool onSubRewardResult(byte[] data, int size)
        {

            return true;
        }
    }; // CGPMessageMission

}