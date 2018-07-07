using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameNet
{
    public class HomeScene : IServerListDataSink
    {
        /*----------------IServerListDataSink------------------------*/
        //完成通知
        public void OnGameItemFinish()
        {

        }
        //完成通知
        public void OnGameKindFinish(ushort wKindID)
        {

        }
        //更新通知
        public void OnGameItemUpdateFinish() { }

        //更新通知

        //插入通知
        public void OnGameItemInsert(CGameListItem pGameListItem) { }
        //更新通知
        public void OnGameItemUpdate(CGameListItem pGameListItem) { }
        //删除通知
        public void OnGameItemDelete(CGameListItem pGameListItem) { }

        public void onGPNoticeResult(tagGameMatch pGameMatchInfo, bool bSucess, byte[] pStr)
        {
            Debug.Log(pStr);
        }
        /*----------------IServerListDataSink------------------------*/

        public HomeScene()
        {
            CServerListData.shared().SetServerListDataSink(this);
            UserInfo.getInstance().addUpPlayerInfoCB(this, upPlayerInfo);
            UserInfo.getInstance().addLoginSucessCB(this, LogonSucess);
        }

        /*----------------UserInfo------------------------*/

        void upPlayerInfo()
        {
            //
        }

        void LogonSucess()
        {
            UserInfo.getInstance().reqAccountInfo();

            // to store HeadHttp
            //delay 3s to avoid cover mMissionType to MISSION_INDIVIDUAL_MODIFY when reqAccountInfo(MISSION_INDIVIDUAL_Account) has not sent yet
            //UserInfo.getInstance().modifyIndivHeadHttp(2.0f);
        }
        /*----------------UserInfo------------------------*/
    }
}
