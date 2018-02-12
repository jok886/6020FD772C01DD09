using System;
using System.Collections.Generic;

// for single game control 
public class LocalGameServer
{
    public class HeartbeatMsg
    {
        public Int64 GameFrameNum;
        public bool HasDpadMsg;
        public List<ControlManager.DPadInfo> DPadMsgList;

        public HeartbeatMsg()
        {
            this.GameFrameNum = -1;
            this.HasDpadMsg = false;
            this.DPadMsgList = new List<ControlManager.DPadInfo>();
        }
    }

    // 单例
    private static LocalGameServer _instance;
    public static LocalGameServer GetInstance()
    {
        if (_instance == null)
        {
            _instance = new LocalGameServer();
        }
        return _instance;
    }

    // for single game control: simulate server control(multi control) process, as similar to multi game control
    private List<ControlManager.DPadInfo> _dPadMsgFromClient;
    //public List<ButtonInfo> ButtonMsgOnFakeServer;

    private Int64 _gameFrameNum = 1;

    public LocalGameServer()
    {
        _dPadMsgFromClient = new List<ControlManager.DPadInfo>();
    }

    public void ReceiveControlMsgFromClient(object controlInfoMsg)
    {
        ControlManager.DPadInfo dPadInfo = controlInfoMsg as ControlManager.DPadInfo;
        //ButtonInfo buttonInfo = controlInfoMsg as ButtonInfo;

        if (dPadInfo != null)
        {
            lock (GameManager.LockObj)
                _dPadMsgFromClient.Add(dPadInfo);
        }
        //else if (buttonInfo != null)
        //{
        //    lock (GameManager.LockObj)
        //    {
        //        ButtonMsgOnFakeServer.Add(buttonInfo);

        //        //// Log
        //        //Debug.Log("SendToFakeServer at local frame:" + GameFrameNum);//GameManager.GetInstance()._matchFrame
        //        //PrintButtonInfo(buttonInfo);
        //    }
        //}
        //else
        //{
        //    Int64 frameNum = (Int64)controlInfoMsg;
        //    lock (GameManager.LockObj)
        //        FrameMsgOnFakeServer.Add(frameNum);
        //}
    }

    public void Heartbeat()
    {
        // Package HeartbeatMsg

        HeartbeatMsg heartbeatMsg = new HeartbeatMsg();
        heartbeatMsg.GameFrameNum = _gameFrameNum;

        if (_dPadMsgFromClient.Count > 0)
        {
            ControlManager.DPadInfo dPadMsg = _dPadMsgFromClient[_dPadMsgFromClient.Count - 1];
            if (dPadMsg != null)
            {
                heartbeatMsg.HasDpadMsg = true;

                dPadMsg.FrameNum = _gameFrameNum;
                heartbeatMsg.DPadMsgList.Add(dPadMsg);
            }
            //foreach (ControlManager.DPadInfo dPadMsg in _dPadMsgFromClient)
            //{
            //    dPadMsg.FrameNum = _gameFrameNum;
            //    heartbeatMsg.DPadMsgList.Add(dPadMsg);
            //}

            _dPadMsgFromClient.Clear();
        }

        // Send HeartbeatMsg
        SendHeartbeatMsgToClient(heartbeatMsg);

        _gameFrameNum++;
    }

    private void SendHeartbeatMsgToClient(HeartbeatMsg heartbeatMsg)
    {
        // Send to Client
        // ..

        // Assume Client receive msg immediately
        ControlManager.GetInstance().ReceiveHeartbeatMsgFromServer(heartbeatMsg);
    }
}