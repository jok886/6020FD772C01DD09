using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using Sproto;
using SprotoType;
using Debug = UnityEngine.Debug;

public delegate void SocketConnected();

public class NetCore
{
    private static Socket socket;

	public static bool PreConnected;
    public static bool Enabled;

    private static int CONNECT_TIMEOUT = 3000;
    private static ManualResetEvent TimeoutObject;

    private static Queue<byte[]> recvQueue = new Queue<byte[]>();

    private static SprotoPack sendPack = new SprotoPack();
    private static SprotoPack recvPack = new SprotoPack();

    private static SprotoStream sendStream = new SprotoStream();
    private static SprotoStream recvStream = new SprotoStream();

    private static ProtocolFunctionDictionary protocol = Protocol.Instance.Protocol;
    private static Dictionary<long, ProtocolFunctionDictionary.typeFunc> sessionDict;

    private static AsyncCallback connectCallback = new AsyncCallback(Connected);
    private static AsyncCallback receiveCallback = new AsyncCallback(Receive);

    public static event EventHandler SocketDisconnectHandler;

    private static AddressFamily _addressFamily;

    //lin: only clear Data, do not remove callbacks
//    public static void Reset()
//    {
//        sessionDict.Clear();
//        recvQueue.Clear();
//        //sendStream.Reset();
//        //lin: sendstream need not to reset as it will overwrite the data when send data
//
//        recvStream.Reset();
//
//        //lin: reset this static pos
//        receivePosition = 0;
//
//        LogIned = false;
//        Enabled = false;
//        socket = null;
//        TimeoutObject = null;
//
//        _reConnectCount = 105;
//    }

    public static void Init()
    {
        byte[] receiveBuffer = new byte[1 << 16];
        recvStream.Write(receiveBuffer, 0, receiveBuffer.Length);
        recvStream.Seek(0, SeekOrigin.Begin);

        sessionDict = new Dictionary<long, ProtocolFunctionDictionary.typeFunc>();
        //_addressFamily = AddressFamily.InterNetworkV6;
		IPAddress[] hostIP = Dns.GetHostAddresses(NetWorkClient.HostStr);
		
        //_addressFamily = (Socket.OSSupportsIPv6 && !Socket.SupportsIPv4) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
		_addressFamily = hostIP[0].AddressFamily;
		//Debug.Log ("family " + _addressFamily);
    }

    private static int _reConnectCount = 115;
	private static Socket bakedSocket;

    public static void Connect(string host, int port, SocketConnected socketConnected)
    {
        lock (GameManager.LockObj)
        {
            _bReconnecting = true;
        }
        Disconnect();

		if(socket == null)
		{
			socket = new Socket (_addressFamily, SocketType.Stream, ProtocolType.Tcp);
			socket.NoDelay = true;
			socket.LingerState = new LingerOption (true, 0);
			bakedSocket = new Socket (_addressFamily, SocketType.Stream, ProtocolType.Tcp);
			bakedSocket.NoDelay = true;
			bakedSocket.LingerState = new LingerOption (true, 0);

			Debug.Log("create socket handle " + socket.Handle + "next socket " + bakedSocket.Handle);
			//socket.UseOnlyOverlappedIO = true; // not implemented in mono
		}
		Debug.Log ("Connect at thread " + Thread.CurrentThread.ManagedThreadId + "with socket " + socket.Handle);
	
        socket.BeginConnect(host, port, connectCallback, socket);
       
        _reConnectCount--;

        TimeoutObject = new ManualResetEvent(false);
        TimeoutObject.Reset();

		if (TimeoutObject.WaitOne (CONNECT_TIMEOUT,false) && socket.Connected) {
			Receive ();
			socketConnected ();
		}
    }

    //BeginConnect callback
    private static void Connected(IAsyncResult ar)
    {
		try{
			Debug.Log("Connected callback");
        	socket.EndConnect(ar);
			PreConnected = true;
			///GameManager.NetWorkClient.CheckConnectVar = true;//mChen
			Debug.Log("socket connected now");
		}catch(Exception e) {
			
			if (e is SocketException) {
				Debug.Log (((SocketException)e).ErrorCode);
				Debug.Log ("beginconnected failed beacuse of socket error: " + e.ToString ());

				if (socket.Connected) {
					try {
						socket.Shutdown (SocketShutdown.Both);
					} catch (Exception ee) {
						Debug.Log ("connected Shutdown error code " + ((SocketException)ee).ErrorCode);
						Debug.Log ("Shutdown socket failed beacuse of socket error:  " + ee.ToString ());
					}
					Debug.Log ("shutdown ended in connected");
					try {
						socket.Close ();
						//socket = null;
					} catch (Exception ex) {
						Debug.Log (((SocketException)ex).ErrorCode);
						Debug.Log ("Close socket failed beacuse of socket error:  " + ex.ToString ());
					}
				}

				#if UNITY_IOS && !UNITY_EDITOR
				if(((SocketException)e).ErrorCode == 10045 || ((SocketException)e).ErrorCode == 10022)
				{
					PreConnected = true;//set this value to re create socket on iphone
				}
				#endif
			}
			else {
				Debug.Log ("begin connected failed beacuse of non-socket error: " + e.ToString ());
			}
		}
		TimeoutObject.Set();
		Debug.Log("timeout obj is seted");
		lock (GameManager.LockObj) {
			_bReconnecting = false;
		}
    }

    public static void Disconnect()
    {
        if (bConnected)
        {
            socket.Close();
            //socket = null;
        }
    }

	public static bool checkNetWork()
	{
		if (socket.Poll (100, SelectMode.SelectRead)) {
			if (socket.Available == 0)
				return false;
		}
		return true;
	}

	private static Socket _preSocket;
	private static SocketInformation temp;
	private static bool _bReconnecting = false;
    public static void ReConnect()
    {
		lock (GameManager.LockObj) {
			if (_bReconnecting)
				return;

			_bReconnecting = true;
		}
		Debug.Log("Connect Timeout, reConnectCount is " + _reConnectCount);
		//Debug.Log("Close socket handle " + socket.Handle);
        recvQueue.Clear();

        recvStream.Reset();

        //lin: reset this static pos
        receivePosition = 0;
        wantDataSize = 0;

        if (PreConnected) {
			if (socket.Connected) {
				try{
					socket.Shutdown (SocketShutdown.Both);
				}catch(Exception ee){
					Debug.Log ("shut down error code " + ((SocketException)ee).ErrorCode);
					Debug.Log ("shutdown failed beacuse of " + ee.ToString ());
				}
				Debug.Log ("shutdown ended in reconnect");
			}

			try {
				Debug.Log("start close socket in reconnect");
				socket.Close ();
				Debug.Log("socket close in reconnect");
			} catch (Exception ex) {
				Debug.Log (((SocketException)ex).ErrorCode);
				Debug.Log ("Close socket failed beacuse of socket error:  " + ex.ToString ());
			}
#if UNITY_IOS && !UNITY_EDITOR
			socket = null;
			socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.NoDelay = true;
			socket.LingerState = new LingerOption (true, 0);
			Debug.Log ("create new socket "+ socket.Handle);
#else
            //on PC, new socket will get the same socket, so we need to new another one first, then close the current socket
			if (socket == bakedSocket) {
				bakedSocket = new Socket (_addressFamily, SocketType.Stream, ProtocolType.Tcp);
				bakedSocket.NoDelay = true;
				bakedSocket.LingerState = new LingerOption (true, 0);
				Debug.Log ("create new baked socket "+ bakedSocket.Handle);
			}

			socket = null;

			socket = bakedSocket;
			Debug.Log ("change socket now");
#endif
			PreConnected = false;
			//bakedSocket = null;
		}


		//if (_reConnectCount > 0)
        SocketDisconnectHandler(null, EventArgs.Empty);

    }

    public static bool bConnected
    {
        get
        {
            return socket != null && socket.Connected;
        }
    }

    public static void Send<T>(SprotoTypeBase rpc = null, long? session = null)
    {
        Send(rpc, session, protocol[typeof(T)]);
    }

    private static int MAX_PACK_LEN = (1 << 16) - 1;
    private static void Send(SprotoTypeBase rpc, long? session, int tag)
    {
        if (!bConnected || !Enabled)
        {
            return;
        }

        package pkg = new package();
        pkg.type = tag;

        if (session != null)
        {
            pkg.session = (long)session;
            sessionDict.Add((long)session, protocol[tag].Response.Value);
        }

        sendStream.Seek(0, SeekOrigin.Begin);
        int len = pkg.encode(sendStream);
        if (rpc != null)
        {
            len += rpc.encode(sendStream);
        }

        byte[] data = sendPack.pack(sendStream.Buffer, len);
        if (data.Length > MAX_PACK_LEN)
        {
            Debug.Log("data.Length > " + MAX_PACK_LEN + " => " + data.Length);
            return;
        }

        sendStream.Seek(0, SeekOrigin.Begin);
        sendStream.WriteByte((byte)(data.Length >> 8));
        sendStream.WriteByte((byte)data.Length);
        sendStream.Write(data, 0, data.Length);

        try {
            socket.Send(sendStream.Buffer, sendStream.Position, SocketFlags.None);
        }
        catch (Exception e) {
			Debug.Log("send Lost network" + ((SocketException)e).ErrorCode);
			Debug.Log("send Lost network" + ((SocketException)e).ToString());
            Debug.LogWarning(e.ToString());
        }
    }

    private static int receivePosition;
    private static byte[] receivedData;
    private static int wantDataSize = 0;
    public static void Receive(IAsyncResult ar = null)
    {
        if (!bConnected)
        {
            return;
        }

        if (ar != null)
        {
            try {
                receivePosition += socket.EndReceive(ar);
            }
            catch (Exception e) {
				
                Debug.Log("Lost network " + ((SocketException)e).ErrorCode);
				Debug.Log("Lost network " + ((SocketException)e).ToString());
				return;//only PC can catch this exception when network closed, use heartbeat to detect lost connection
                //出现10053的原因是因为在你执行这次send的时候对端已经执行过closesocket了，而发送的数据还是被成功的推入了发送缓冲区中，因此返回了0，
                //此时你可能还没得到FIN消息，而紧接着recv这边就得到了对端关闭socket的FIN消息，因此此时需要放弃发送缓冲中的数据，异常终止连接，
                //所以得到了10053错误：您的主机中的软件中止了一个已建立的连接。 

                //而为什么又能得到10054的错误号，原因应该在于你设置了SO_LINGER了，一但设置了它，则有一个等待奔洌?
                //在该等待时间内可以处理发送缓冲区的数据，一但超时或者发送缓冲都被发送完并被确认，则服务端有可能发送RST消息而不是FIN,
                //此时就应该得到重置错误，也就是10054。

                recvQueue.Clear();

                recvStream.Reset();

                //lin: reset this static pos
                receivePosition = 0;
               
                if (((SocketException)e).ErrorCode == 10054)//WSAECONNRESET   断线 10054: An existing connection was forcibly closed by the remote host.
                {
                    SocketDisconnectHandler(null, EventArgs.Empty);
                }
                else if (((SocketException)e).ErrorCode == 10053) // 10053: An established connection was aborted by the software in your host machine. 
                {
                    SocketDisconnectHandler(null, EventArgs.Empty);
                }
                
                Debug.LogWarning(e.ToString());
            }
        }

        int i = recvStream.Position;
        while (receivePosition >= i + 2)
        {
            int length = 0;
            int curLen = (receivePosition - i);
            if (wantDataSize == 0)
            {
                length = (recvStream[i] << 8) | recvStream[i + 1];
                //Debug.Log("received data : " + length);
                Debug.Assert(length > 0, "received data size <= 0");
                int sz = length + 2;

                recvStream.Seek(2, SeekOrigin.Current);
                curLen -= 2;
                receivedData = new byte[length];              

                if (curLen >= length)
                {
                    recvStream.Read(receivedData, 0, length);
                    recvQueue.Enqueue(receivedData);
                    i += sz;
                }
                else
                {
                    wantDataSize = length - curLen;//待接收字符
                   // Debug.Log("want received data 1: " + wantDataSize);
                    recvStream.Read(receivedData, 0, curLen);
                    i += (curLen + 2);
                }                    
            }
            else
            {
                if(curLen >= wantDataSize)
                {
                    recvStream.Read(receivedData, receivedData.Length - wantDataSize, wantDataSize);
                    recvQueue.Enqueue(receivedData);
                    i += wantDataSize;
                    //Debug.Log("received data 2: " + wantDataSize + curLen);
                    wantDataSize = 0;
                    
                }
                else
                {
                    recvStream.Read(receivedData, receivedData.Length - wantDataSize, curLen);
                    i += curLen;
                    wantDataSize -= curLen;
                    //Debug.Log("want received data 2: " + wantDataSize);
                }
                
            }
        }

        if (receivePosition == recvStream.Buffer.Length)
        {
            recvStream.Seek(0, SeekOrigin.End);
            recvStream.MoveUp(i, i);
            receivePosition = recvStream.Position;
            recvStream.Seek(0, SeekOrigin.Begin);
        }

        try {
            socket.BeginReceive(recvStream.Buffer, receivePosition,
                recvStream.Buffer.Length - receivePosition,
                SocketFlags.None, receiveCallback, socket);
        }
        catch (Exception e) {
            Debug.LogWarning(e.ToString());
        }
    }

    public static void Dispatch()
    {
        //Debug.Log("dis");
        try
        {
            package pkg = new package();

            if (recvQueue.Count > 20)
            {
                Debug.Log("recvQueue.Count: " + recvQueue.Count);
            }

            while (recvQueue.Count > 0)
            {
                //Debug.Log("dis");
                byte[] data = recvPack.unpack(recvQueue.Dequeue());
                int offset = pkg.init(data);

                int tag = (int)pkg.type;
                long session = (long)pkg.session;

                if (pkg.HasType)
                {
                    //Debug.Log("type");
                    try
                    {
                        RpcReqHandler rpcReqHandler = NetReceiver.GetHandler(tag);
                        if (rpcReqHandler != null)
                        {
                            SprotoTypeBase rpcRsp = rpcReqHandler(protocol.GenRequest(tag, data, offset));
                            if (pkg.HasSession)
                            {
                                Send(rpcRsp, session, tag);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("NetReceiver exception: " + e.Message + tag + session + offset);
                        throw;
                    }
                }
                else
                {
                    //Debug.Log("no type");
                    try
                    {
                        RpcRspHandler rpcRspHandler = NetSender.GetHandler(session);
                        if (rpcRspHandler != null)
                        {
                            ProtocolFunctionDictionary.typeFunc GenResponse;
                            sessionDict.TryGetValue(session, out GenResponse);
                            rpcRspHandler(GenResponse(data, offset));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("NetReceiver exception: " + e.Message + tag + session + offset);
                        throw;
                    }
                }
            }
        
        }
        catch (Exception e)
        {
            Debug.Log("Dispatch exception: " + e.Message);
            throw;
        }
    }

}
