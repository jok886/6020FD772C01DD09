
using System;
using System.Collections.Generic;

namespace GameNet
{
    struct NetCall
    {
        public int iSub;
        //std::function<void(void*, int)> pFun;
        public delegate void DelegateMethod(byte[] b,int i);
        public DelegateMethod pFun;
    }

    abstract class CallBase
    {
        public abstract void CallFun();
    }

    //template<class T>
    class CallStruct<T> : CallBase
    {
        public override void CallFun()
        {
            pFun(kInfo);
        }
        public T kInfo;
        public delegate void DelegateMethod(T t);  //声明了一个Delegate Type

        public DelegateMethod pFun;   //声明了一个Delegate对象
       // std::function<void(T)> pFun;
    }

    class CallVoid : CallBase
    {
        public override void CallFun()
        {
            pFun();
        }
        //std::function<void()> pFun;
        public delegate void DelegateMethod();
        public DelegateMethod pFun;
    }

    class CCallMission : CSocketMission//模板和可调用对象没办法处理
    {
       
	    public CCallMission(string kName,byte[] url = null, int port = 0 ):base(url,port)
        {
            m_kClassName = kName;
            m_kLinkCallFun = new List<CallBase>();
            m_kNetCallFun = new List<NetCall>();
        }
        ~CCallMission()
        {
            clearCall();
        }
        public void addLinkCallFun(CallVoid.DelegateMethod pFun)
        {
            CallVoid pCallInfo = new CallVoid();
            pCallInfo.pFun = pFun;
            m_kLinkCallFun.Add(pCallInfo);
        }

        //template<class T>
	    public void addLinkCallStruct<T>(CallStruct<T>.DelegateMethod pFun, T kInfo)
        {
            CallStruct<T> pCallInfo = new CallStruct<T> ();
            pCallInfo.kInfo = kInfo;
            pCallInfo.pFun = pFun;
            m_kLinkCallFun.Add(pCallInfo);
        }

        public void addNetCall(NetCall.DelegateMethod pFun, int iSub)
        {
            NetCall kCallInfo = new NetCall();
            kCallInfo.pFun = pFun;
            kCallInfo.iSub = iSub;
            m_kNetCallFun.Add(kCallInfo);
        }

        public void clearCall()
        {
            for (int i = 0; i < (int)m_kLinkCallFun.Count; i++)
            {
                //delete m_kLinkCallFun[i];//
            }
            m_kLinkCallFun.Clear();
        }
        
	    public override void onEventTCPSocketLink()
        {
            //utility::log(utility::toString(m_kClassName, ":onEventTCPSocketLink").c_str());//

            for (int i = 0; i < (int)m_kLinkCallFun.Count; i++)
            {
                m_kLinkCallFun[i].CallFun();
            }
            clearCall();
        }
        public override void onEventTCPSocketShut()
        {
            //utility::log(utility::toString(m_kClassName, ":onEventTCPSocketShut").c_str());//

            clearCall();
        }
        public override void onEventTCPSocketError(Exception errorCode)
        {
            //utility::log(utility::toString(m_kClassName, ":onEventTCPSocketShut").c_str());//
            base.onEventTCPSocketError(errorCode);
            clearCall();
        }
        public override bool onEventTCPSocketRead(int main, int sub, byte[] data, int dataSize)
        {
            //utility::log(utility::toString(m_kClassName, ":onEventTCPSocketRead").c_str());//

            for (int i = 0; i < (int)m_kNetCallFun.Count; i++)
            {
                //NetCall kNetInfo = m_kNetCallFun[i];
                if (m_kNetCallFun[i].iSub == sub)
                {
                    m_kNetCallFun[i].pFun(data, dataSize);
                    return true;
                }
            }
            //CCASSERT(false, "");//
            return false;
        }
	    private string m_kClassName;
        //std::vector<CallBase*> m_kLinkCallFun;
        List<CallBase> m_kLinkCallFun;
        //std::vector<NetCall> m_kNetCallFun;
        List<NetCall> m_kNetCallFun;


    }
}