

using System;
using UnityEngine;

namespace GameNet
{
    public class CSocketMission: ISocketEngineSink
    {
        public virtual void onEventTCPSocketLink()
        {
        }

        public virtual void onEventTCPSocketShut()
        {
        }

        public virtual void onEventTCPSocketError(Exception errorCode) { Debug.Log("TCP Socket exception: " + errorCode.ToString());}
        public virtual bool onEventTCPSocketRead(int main, int sub, byte[] data, int dataSize) { return false; }

        public virtual bool onEventTCPHeartTick()
        {
            //Debug.Log("sending TCPHeartTick msg");
            return true;
        }

        public CSocketMission(byte[] url = null, int port = 0)
        {
            mSocketEngine = new CSocketEngine();
            mSocketEngine.setTCPValidate(false);
            mSocketEngine.setSocketEngineSink(this);

            setUrl(url, port);
        }
        ~CSocketMission()
        {
            mSocketEngine.setSocketEngineSink(null);
            mSocketEngine.destory(mSocketEngine);
            mSocketEngine = null;
        }

        public void setUrl(byte[] url, int port)
        {
            if (url != null)
                //sprintf(mUrl, "%s", url);
                mUrl = url;
            else
                mUrl[0] = 0;
            mPort = port;
        }
        public bool start()
        {
            if (isAlive())
            {
                onEventTCPSocketLink();
                return true;
            }
            else
            {
                return mSocketEngine.connect(System.Text.Encoding.Default.GetString(mUrl), mPort);
            }
            return false;
        }
        public void stop()
        {
            mSocketEngine.disconnect();
        }
        public bool isAlive()
        {
            return mSocketEngine.isAlive();
        }
        public bool send(int main, int sub)
        {
            return send(main, sub, null, 0);
        }
        public bool send(int main, int sub, byte[] data, int size)
        {
            return mSocketEngine.send(main, sub, data, size);
        }
      
	    private ISocketEngine mSocketEngine;
        private byte[] mUrl = new byte[260];
        private int mPort;
    }
}