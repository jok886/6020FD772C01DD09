using System;

namespace GameNet
{
    public interface ISocketEngineSink
    {
        void onEventTCPSocketLink();
	    void onEventTCPSocketShut();
	    void onEventTCPSocketError(Exception errorCode);
	    bool onEventTCPSocketRead(int main, int sub, byte[] data, int dataSize);
        bool onEventTCPHeartTick();//接口没有默认实现 { return true; }
    }
}