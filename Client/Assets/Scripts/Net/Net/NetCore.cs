using System;
using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace GameNet
{
    internal class StateObject
    {
        public TcpClient client = null;
        //public int totalBytesRead = 0;
        public const int BufferSize = 20480;    //1024 * 1024 * 2;  
        //public string readType = null;
        public byte[] buffer = new byte[BufferSize];
    }

    public delegate void SocketCallbacks();

    public delegate void SocketDataCallbacks(byte[] data, int iLength);

    public delegate void SocketExceptionCallback(Exception e);

    public class CustomSocket
    {
        //reset对象
        //public ManualResetEvent connectDone = new ManualResetEvent(false);
        //tcp客户端,即是与服务端通讯的组件
       
        TcpClient tcpclient = null;
        //网络流
        NetworkStream stream;

        public bool isConnected = false;

        public SocketCallbacks SuccessCallback { set; get; }
        public SocketExceptionCallback ExceptionCallback { set; get; }
        public SocketDataCallbacks DataRecivedCallback { set; get; }
        public SocketCallbacks DisconnectCallback { set; get; }

        public CustomSocket()
        {
            tcpclient = new TcpClient();
            isConnected = false;
            stream = null;
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="ip">服务器IP</param>
        /// <returns></returns>
        public void Connect(string url, int port)
        {
           
            AddressFamily family = AddressFamily.InterNetwork;
            //ipv4转ipv6 - 苹果ios
#if !UNITY_EDITOR && UNITY_IPHONE
			//url = IOSIpv6.GetIPv6Str(url, port, out family);

            //mChen add, for ipv6 support
            IPAddress[] address = Dns.GetHostAddresses("www.baidu.com");
            //IPAddress[] address = Dns.GetHostAddresses("127.0.0.1");  
            //foreach (var info in address)
            //{
            //    Debug.Log(info);
            //}

            if (address[0].AddressFamily == AddressFamily.InterNetworkV6)
            {
                family = AddressFamily.InterNetworkV6;
                Debug.Log("Connect InterNetworkV6");
            }
            else
            {
                family = AddressFamily.InterNetwork;
                Debug.Log("Connect InterNetwork");
            }

#endif

            try
            {
                tcpclient = new TcpClient(family);
                //防止延迟,即时发送!
                tcpclient.NoDelay = true;
                tcpclient.ReceiveBufferSize = HeaderStruct.SIZE_TCP_BUFFER;
                tcpclient.SendBufferSize = HeaderStruct.SIZE_TCP_BUFFER;
                tcpclient.BeginConnect(url, port, new AsyncCallback(ConnectCallback), tcpclient);
                //tcpclient.SendTimeout=
                //tcpclient.ReceiveTimeout= //默认为0,无超时
                Debug.Log(" connect to url with port: port=" + port + ", url=" + url);
            }
            catch (Exception ex)
            {
                //设置标志,连接服务端失败!
                Debug.Log("服务器断开连接，请重新运行程序或稍后再试");
                Debug.Log("----------------Connect------------------------Exception----------------");
                //	ReConnectScript.getInstance().ReConnectToServer(); 
                Debug.Log(ex.ToString());
                isConnected = false;
            }
        }

        /// <summary>
        /// 关闭网络流
        /// </summary>
        private void DisConnect()
        {
            Debug.Log("Closing tcp client : " + tcpclient.Client.Handle);
            if (tcpclient != null)
            {
                tcpclient.Close();
                tcpclient = null;
            }
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            isConnected = false;

            //mChen add, temp
            ///DisconnectCallback();
        }
        /*
        public void sendMsg(ClientRequest client)
        {
            Debug.Log("---------------sendMsg----------------messageContent--" + client.messageContent);
            SendData(client.ToBytes());
        }
        */
        /// <summary>
        /// 发送数据
        /// </summary>
        public void send(byte[] data, int size)
        {
            //Debug.Log("----------------SendData-----------------");
            //Debug.Log ("send data"+data.ToString ());
            try
            {
                if (stream != null)
                {
                    //Debug.Log("----------------SendData----2-------------" + size);
#if true
                    stream.BeginWrite(data, 0, size, new AsyncCallback(TCPWriteCallBack), stream);
#else
                    if (stream.CanWrite)
                    {
                        stream.Write(data, 0, size);
                        stream.Flush();
                    }
                    else
                    {
                        Debug.Log("socket can not write now!!!");
                    }
                    
#endif
                }
                else
                {
                    Debug.LogError("disconnect4: send stream cause disconnect!!! ");

                    //	showMessageTip("服务器断开连接，请重新运行程序或稍后再试");
                    Debug.Log("22222222222222222222222222222");
                    isConnected = false;
                    DisconnectCallback();
                    // SocketEventHandle.getInstance().noticeDisConect();
                    //ReConnectScript.getInstance().ReConnectToServer(); 
                    //Connect();
                }
            }
            ////mChen add
            //catch (ObjectDisposedException e)
            //{
            //    Debug.LogError("send catch ObjectDisposedException:" + e);
            //    throw;
            //}
            catch (IOException e)
            {
                Debug.LogError("disconnect5: send() catch cause disconnect!   IOException = " + e);

                isConnected = false;
                DisconnectCallback();
            }
            catch (Exception ex)
            {
                Debug.LogError("disconnect5: send() catch cause disconnect!   Exception = " + ex);
                //Debug.Log(ex.ToString());
                //Debug.Log("----------------SendData------------------------Exception----------------");

                //	showMessageTip("服务器断开连接，请重新运行程序或稍后再试");
                isConnected = false;
                DisconnectCallback();
                //SocketEventHandle.getInstance().noticeDisConect();
                //ReConnectScript.getInstance().ReConnectToServer(); 
                //Connect();
            }
        }

        /// <summary>
        /// TCP写数据的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void TCPWriteCallBack(IAsyncResult ar)
        {
            var steam = (NetworkStream) ar.AsyncState;

            try
            {
                steam.EndWrite(ar);
                //Debug.Log("写入socket数据完成 ");
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log("写入socket数据失败 ");
            }
        }

        /*
        /// <summary>
        /// 发送心跳包
        /// </summary>
        /// *

        public bool sendHeadData()
        {
            Debug.Log("send head data");
            try
            {
                if (stream != null && tcpclient.Connected)
                {
                    stream.Write(headBytes, 0, headBytes.Length);
                    GlobalDataScript.getInstance().xinTiaoTime = DateTime.UtcNow;
                    return true;
                }
                else
                {
                    isConnected = false;
                    SocketEventHandle.getInstance().noticeDisConect();
                    return false;

                }

            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
                isConnected = false;
                //showMessageTip ("服务器已断开连接，请重新登录");
                isConnected = false;
                SocketEventHandle.getInstance().noticeDisConect();
                return false;
            }
        }

        public bool sendHeadData2()
        {
            Debug.Log("send head data");
            try
            {
                if (stream != null && tcpclient.Connected)
                {
                    //stream.Write(headBytes, 0, headBytes.Length);

                    return true;
                }
                else
                {
                    isConnected = false;
                    if (!GlobalDataScript.isonLoginPage)
                    {
                        PrefabManage.loadPerfab("Prefab/Panel_Start");
                    }
                    SocketEventHandle.getInstance().noticeDisConect();
                    return false;
                }

            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
                isConnected = false;
                //showMessageTip ("服务器已断开连接，请重新登录");
                isConnected = false;
                SocketEventHandle.getInstance().noticeDisConect();
                return false;
            }
        }
        */
        /// <summary>
        /// 异步连接的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            //connectDone.Set();
            if ((tcpclient != null) && (tcpclient.Connected))
            {
                stream = tcpclient.GetStream();
                isConnected = true;
                asyncread(tcpclient);
                Debug.Log("connected tcp client : " + tcpclient.Client.Handle);
                Debug.Log("服务器已经连接!");
                Debug.Log("---------------ConnectCallback----------------ConnectCallback--");
            }
            else
            {
                //tcpclient.BeginConnect(IPAddress.Parse(APIS.socketUrl), APIS.socketPort, new AsyncCallback(ConnectCallback), tcpclient);
                
            }

            TcpClient t = (TcpClient)ar.AsyncState;
            try
            {
                t.EndConnect(ar);
                SuccessCallback();
            }
            catch (Exception ex)
            {
                ExceptionCallback(ex);
                Debug.Log("---------------ConnectCallback----------------Exception--");
                //设置标志,连接服务端失败!
                Debug.Log(ex.ToString());
                //tcpclient.BeginConnect(IPAddress.Parse(APIS.socketUrl), APIS.socketPort, new AsyncCallback(ConnectCallback), tcpclient);
            }
        }
        /// <summary>
        /// 异步读TCP数据
        /// </summary>
        /// <param name="sock"></param>
        private void asyncread(TcpClient sock)
        {
            StateObject state = new StateObject();
            state.client = sock;
            NetworkStream streamRead;
            try
            {
                streamRead = sock.GetStream();
                if (streamRead.CanRead)
                {
                    try
                    {
                        IAsyncResult ar = streamRead.BeginRead(state.buffer, 0, StateObject.BufferSize,
                                new AsyncCallback(TCPReadCallBack), state);

                    }
                    catch (Exception ex)
                    {
                        ExceptionCallback(ex);
                        //设置标志,连接服务端失败!
                        Debug.Log("---------------连接异常-------------1-----");
                        Debug.Log(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                //设置标志,连接服务端失败!
                // NetManaged.isConnectServer = false;
                // NetManaged.surcessstate = 0;
                ExceptionCallback(ex);
                Debug.Log(ex.ToString());
                Debug.Log("---------------连接异常-------------2-----");
            }

        }

        /// <summary>
        /// TCP读数据的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void TCPReadCallBack(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            //主动断开时
            if ((state.client == null) || (!state.client.Connected))
            {
                Debug.Log("---------------TCPReadCallBack:closeSocket ");
                ///Debug.LogError("disconnect6: TCPReadCallBack:closeSocket: client cause disconnect!!! ");

                closeSocket();

                //mChen add, for HideSeek 断线重连：设置serviceStatus使CheckInGameServerAfterOffline中的自动重连生效，因为客户端的7s断线检测关了（StartOrStopGameSceneHeartBeat直接return）
                ///DisconnectCallback();

                return;
            }
            int numberOfBytesRead;
            NetworkStream mas = state.client.GetStream();
            try
            {
                //EndRead 方法将一直处于阻止状态，直到有数据可用为止。
                //EndRead 方法将读取尽可能多的可用数据，直到达到在 BeginRead 方法的 size 参数中指定的字节数为止。
                //如果远程主机关闭了 Socket 连接并且已接收到所有可用数据，EndRead 方法将立即完成并返回零字节。
                numberOfBytesRead = mas.EndRead(ar);
                //state.totalBytesRead += numberOfBytesRead;
                //Debug.Log("read byte size : " + numberOfBytesRead);

                if (numberOfBytesRead > 0)
                {
                    ///byte[] dd = new byte[numberOfBytesRead];
                    ///Array.Copy(state.buffer, 0, dd, 0, numberOfBytesRead);
                    ReceiveCallBack(state.buffer, numberOfBytesRead);
                    Array.Clear(state.buffer,0,StateObject.BufferSize);
                    if (isConnected)
                    {
                        mas.BeginRead(state.buffer, 0, StateObject.BufferSize,
                            new AsyncCallback(TCPReadCallBack), state);
                    }
                }
                else
                {

                    Debug.LogError("disconnect7: TCPReadCallBack: numberOfBytesRead<=0 cause disconnect! 连接异常,客户端被动断开! numberOfBytesRead=" + numberOfBytesRead);

                    //设置标志,连接服务端失败!
                    //Debug.Log("---------------连接异常,客户端被动断开-------------3-----");

#if true
                    //被动断开时 
                    mas.Close();
                    state.client.Close();
                    mas = null;
                    state = null;

                    DisconnectCallback();
#else
                    //mChen add, for HideSeek
                    Array.Clear(state.buffer, 0, StateObject.BufferSize);
                    if (isConnected)
                    {
                        mas.BeginRead(state.buffer, 0, StateObject.BufferSize,
                            new AsyncCallback(TCPReadCallBack), state);
                    }
#endif

                }
            }
            catch (SocketException e)
            {
                Debug.LogError("disconnect8: TCPReadCallBack: SocketException = " + e + ", message=" + e.Message);
            }
            catch (IOException e)
            {
                Debug.LogError("disconnect8: TCPReadCallBack: catch cause disconnect,读取socket数据失败!    IOException = " + e);
                //Debug.LogWarning("IOException: IOException = " + e );
                //Debug.LogWarning("InnerException: InnerException=" + e.InnerException);
                //Debug.Log("读取socket数据失败 tcp client: " + state.client.Client.Handle);

                DisconnectCallback();
                throw;
            }
#if false
            catch (ObjectDisposedException e)
            {
                Debug.Log(e.Message);
                throw;
            }
#endif
        }

        private void ReceiveCallBack(byte[] receiveBuffer, int iLength)
        {
#if true
          DataRecivedCallback(receiveBuffer, iLength);
#else
    //通知调用端接收完毕
            try
            {
                //	Debug.Log("receiveBuffer======"+receiveBuffer.Length);
                MemoryStream ms = new MemoryStream(receiveBuffer);
                BinaryReader buffers = new BinaryReader(ms, UTF8Encoding.Default);
                readBuffer(buffers);
            }
            catch (Exception ex)
            {
                Debug.Log("---------------连接异常-------------4-----");
                Debug.Log("socket exception:" + ex.Message);
                throw new Exception(ex.Message);
            }
#endif
        }
        /*
        private void readBuffer(BinaryReader buffers)
        {
            byte flag = buffers.ReadByte();
            int lens = ReadInt(buffers.ReadBytes(4));
            disConnectCount = 0;
            if (!hasStartTimer && lens == 16)
            {
                startTimer();
                hasStartTimer = true;
            }
            //Debug.Log ("lengs ====>>  "+lens);

            if (lens > buffers.BaseStream.Length)
            {
                waitLen = lens;
                isWait = true;
                buffers.BaseStream.Position = 0;
                byte[] dd = new byte[buffers.BaseStream.Length];
                byte[] temp = buffers.ReadBytes((int)buffers.BaseStream.Length);
                Array.Copy(temp, 0, dd, 0, (int)buffers.BaseStream.Length);
                if (sources == null)
                {
                    sources = dd;
                }
                return;
            }
            int headcode = ReadInt(buffers.ReadBytes(4));
            int status = ReadInt(buffers.ReadBytes(4));
            short messageLen = ReadShort(buffers.ReadBytes(2));
            if (flag == 1)
            {
                string message = Encoding.UTF8.GetString(buffers.ReadBytes(messageLen));
                ClientResponse response = new ClientResponse();
                response.status = status;
                response.message = message;
                response.headCode = headcode;
                Debug.Log("response.headCode = " + response.headCode + "  response.message =   " + message);
                SocketEventHandle.getInstance().addResponse(response);
            }
            if (buffers.BaseStream.Position < buffers.BaseStream.Length)
            {
                readBuffer(buffers);
            }
        }
        */
        public void closeSocket()
        {
            DisConnect();
        }

    }

}

