
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace GameNet
{
    interface ISocketSink
    {
        void onSocketLink();
	    void onSocketShut();
	    void onSocketError(Exception e);
	    void onSocketData(byte[] data, int dataSize);

    };
    class CSocket
    {
        private CustomSocket mSocket;
        ISocketSink mSocketSink;

        public CSocket()
        {
            mSocketSink = null;
            mSocket = new CustomSocket
            {
                SuccessCallback = onConnected,
                DisconnectCallback = onDisconnected,
                DataRecivedCallback = onMessageReceived,
                ExceptionCallback = onExceptionCaught
            };
        }
        ~CSocket()
        {

        }
        public void setSocketSink(ISocketSink pISocketSink)
        {
            mSocketSink = pISocketSink;
        }
        public bool isAlive()
        {
            return mSocket.isConnected;  
            //return base.isConnected();
        }
        public int connect(string url, int port)
        {
            mSocket.Connect(url, port);
            return 0;
        }
        public new void disconnect()
        {
            if (!isAlive())
                return;
            mSocket.closeSocket();
        }
        
        public new int send(byte[] data, int size)
        {
            if (!isAlive())
                return -1;

            mSocket.send(data, size);
            return 1;
        }
        
        //////////////////////////////////////////////////////////////////////////
        public void onConnected()
        {
            if (mSocketSink != null)
                mSocketSink.onSocketLink();
        }

        public void onDisconnected()
        {
            if (mSocketSink!=null)
                mSocketSink.onSocketShut();
        }
        public void onExceptionCaught(Exception eStatus)
        {
            if (mSocketSink!=null)
                mSocketSink.onSocketError(eStatus);
        }
        public void onMessageReceived(byte[] pData, int iLength)
        {
            if (mSocketSink != null)
            {
                mSocketSink.onSocketData(pData, iLength);
            }
        }

    }
}