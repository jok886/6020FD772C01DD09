
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace GameNet
{
    class CSocketEngine : ISocketEngine, ISocketSink
    {
        // 接收处理模块
        private ISocketEngineSink mISocketEngineSink;//修改了类型
        // 连接SOCKET
        private CSocket mSocket;
        // 发送校验
        private bool mIsSendTCPValidate;

        //////////////////////////////////////////////////////////////////////////
        // 数据缓冲
        // 临时缓冲
        private byte[] mBufPack;
        private byte[] mBufUnPack;
        // 接收缓冲
        private byte[] mBufRecieve;
        // 接收长度
        private int mBufRevLength;

        //  add by  lesten
        private byte[] mTempBuf;
        private uint temp_size_;

        private object BufLockObj { get; set; }

        //加密数据
        protected Byte m_cbSendRound;                        //字节映射
        protected Byte m_cbRecvRound;                        //字节映射
        protected uint m_dwSendXorKey;                        //发送密钥
        protected uint m_dwRecvXorKey;                        //接收密钥

        //计数变量
        protected uint m_dwSendPacketCount;               //发送计数
        protected uint m_dwRecvPacketCount;               //接受计数

        //mChen add, for HideSeek
        private byte[] m_cbRecDataBuffer;

        /** 销毁 */

        public void destory(ISocketEngine pISocketEngine) //修改了参数类型去掉了指针
        {
            pISocketEngine = null;
        }

        public CSocketEngine()
        {
            if (BufLockObj == null)
            {
                BufLockObj = new object();
            }

            mSocket = new CSocket();
            mISocketEngineSink = null;
            mIsSendTCPValidate = false;
            mSocket.setSocketSink(this);

            mBufPack = new byte[HeaderStruct.SIZE_TCP_BUFFER];
            mBufUnPack = new byte[HeaderStruct.SIZE_TCP_BUFFER];
            // 接收缓冲
            mBufRecieve = new byte[HeaderStruct.SIZE_TCP_BUFFER];
            mTempBuf = new byte[HeaderStruct.SIZE_TCP_BUFFER];
            //加密数据
            m_cbSendRound = 0;
            m_cbRecvRound = 0;
            m_dwSendXorKey = 0;
            m_dwRecvXorKey = 0;

            m_dwSendPacketCount = 0;
            m_dwRecvPacketCount = 0;

            // add by lesten
            temp_size_ = 0;

            //mChen add, for HideSeek
            m_cbRecDataBuffer = new byte[Packet.SOCKET_TCP_BUFFER];
        }
        ~CSocketEngine()
        {
            mISocketEngineSink = null;
            mSocket.setSocketSink(null);
            disconnect();
        }

        //////////////////////////////////////////////////////////////////////////
        // 接口ISocketEngine

        /** 设置Socket接收器 */
        public void setSocketEngineSink(ISocketEngineSink pISocketEngineSink)//修改了参数类型
        {
            mISocketEngineSink = pISocketEngineSink;
        }
        /** 链接网络 **/
        public bool connect(string url, int port)
        {
            initValue();

            //cocos2d::log("Connect %s", url);

            return mSocket.connect(url, port) == 0;
        }
        /** 关闭网络 **/
        public bool disconnect()
        {
            initValue();

            mSocket.disconnect();
            return true;
        }
        /** 发送数据 **/
        //有问题
        public bool send(int main, int sub, byte[] data,int size)
        {
            if (!isAlive())
                return false;
            //构造数据
            byte[] cbDataBuffer = new byte[Packet.SOCKET_TCP_BUFFER];

            Packet.TCP_Head pHead = new Packet.TCP_Head();

            pHead.CommandInfo.wMainCmdID = (ushort)main;
            pHead.CommandInfo.wSubCmdID = (ushort)sub;

            byte[] structData = StructConverterByteArray.StructToBytes(pHead);
            Buffer.BlockCopy(structData, 0, cbDataBuffer, 0, structData.Length);
            if (size > 0)
            {
                Buffer.BlockCopy(data, 0, cbDataBuffer, structData.Length, size);
                //memcpy(pHead + 1, data, dataSize);
            }
            //加密数据
            uint wSendSize = EncryptBuffer(ref cbDataBuffer, (uint)(Packet.SizeOfTCP_Head + size), (uint)cbDataBuffer.Length);
           
            mSocket.send(cbDataBuffer, (int)wSendSize);
            return true;
        }
        /** 状态判断 **/
        public bool isAlive()
        {
            return mSocket.isAlive();
        }
        /** 发送校验 **/
        public void setTCPValidate(bool send)
        {
            mIsSendTCPValidate = send;
        }
        //////////////////////////////////////////////////////////////////////////
        // 接口ISocketSink
        public virtual void onSocketLink()
        {
            sendTCPValidate();
            if (mISocketEngineSink != null)
                mISocketEngineSink.onEventTCPSocketLink();
        }
        public virtual void onSocketShut()
        {
            if (mISocketEngineSink != null)
                mISocketEngineSink.onEventTCPSocketShut();
        }
   
        public virtual void onSocketError(Exception errorCode)
        {
            if (mISocketEngineSink != null)
                mISocketEngineSink.onEventTCPSocketError(errorCode);
        }
        
        public virtual void onSocketData(byte[] data, int dataSize)
        {
            if (BufLockObj == null)
            {
                BufLockObj = new object();
            }

            //lock (BufLockObj)
            {
                //var dataSize = data.Length;
                int nRecvSize = 0;

                // 1
                //  add by lesten
                if (temp_size_ != 0)
                {
                    nRecvSize += (int)temp_size_;
                    Buffer.BlockCopy(mTempBuf, 0, mBufRecieve, 0, (int)temp_size_);
                    //memcpy(&mBufRecieve[0], mTempBuf, temp_size_);

                    temp_size_ = 0;
                    Array.Clear(mTempBuf, 0, HeaderStruct.SIZE_TCP_BUFFER);
                    //memset(mTempBuf, SIZE_TCP_BUFFER, 0);
                }
                if (nRecvSize + dataSize >= HeaderStruct.SIZE_TCP_BUFFER)
                {
                    //cocos2d::log("nRecvSize + dataSize >= SIZE_TCP_BUFFER");

                    Debug.LogError("disconnect1: nRecvSize + dataSize >= HeaderStruct.SIZE_TCP_BUFFER cause disconnect!!! nRecvSize=" + nRecvSize + ", dataSize=" + dataSize);

                    disconnect();
                    return;
                }

                // 2
                Buffer.BlockCopy(data, 0, mBufRecieve, nRecvSize, dataSize);
                //memcpy(&mBufRecieve[nRecvSize], data, dataSize);

                nRecvSize += dataSize;

                //变量定义
                ushort wPacketSize = 0;
                //byte[] m_cbRecDataBuffer = new byte[Packet.SOCKET_TCP_BUFFER];

                // Log
                if (dataSize > 5000)
                {
                    Debug.LogError("onSocketData: dataSize>5000 dataSize=" + dataSize + ", nRecvSize = " + nRecvSize);
                }

                while (nRecvSize >= Packet.SizeOfTCP_Head)
                {
                    Packet.TCP_Info pHeadTCPInfo =
                        (Packet.TCP_Info)StructConverterByteArray.BytesToStruct(mBufRecieve, typeof(Packet.TCP_Info));
                    wPacketSize = pHeadTCPInfo.wPacketSize;

                   
                    //if(wPacketSize==316 || dataSize==432)
                    //{
                    //    //wPacketSize==316 : SUB_S_HideSeek_HeartBeat
                    //    Debug.Log("which may cause nRecvSize!=0");
                    //}

                    if (wPacketSize > (Packet.SOCKET_TCP_PACKET + Packet.SizeOfTCP_Head))
                    {
                        Debug.LogError("disconnect3: wPacketSize  > (Packet.SOCKET_TCP_PACKET + Packet.SizeOfTCP_Head cause disconnect!!! wPacketSize=" + wPacketSize);

                        disconnect();
                        return;
                    }

                    if (nRecvSize < wPacketSize)
                    {
                        // 3
                        temp_size_ = (uint)nRecvSize;
                        Buffer.BlockCopy(mBufRecieve, 0, mTempBuf, 0, (int)temp_size_);
                        //memcpy(mTempBuf, mBufRecieve, temp_size_);

                        return;
                    }

                    if (pHeadTCPInfo.cbDataKind != Packet.DK_ENCRYPT)
                    {
                        //pHeadTCPInfo.cbDataKind==190 178 125 144 116
                        Debug.LogError("disconnect2: cbDataKind != Packet.DK_ENCRYP cause disconnect!!! cbDataKind=" + pHeadTCPInfo.cbDataKind + ", dataSize=" + dataSize+ ", wPacketSize="+ wPacketSize + ", nRecvSize="+ nRecvSize);

                        //mChen edit,disconnect改为丢弃该包
                        nRecvSize -= (int)wPacketSize;
                        var bufReceiveClone = (byte[])mBufRecieve.Clone();
                        Buffer.BlockCopy(bufReceiveClone, (int)wPacketSize, mBufRecieve, 0, nRecvSize);
                        continue;
                        //disconnect();
                        //return;
                    }

                    //拷贝数据
                    m_dwRecvPacketCount++; 

                    Buffer.BlockCopy(mBufRecieve, 0, m_cbRecDataBuffer, 0, (int)wPacketSize);
                    //memcpy(m_cbRecDataBuffer, mBufRecieve, wPacketSize);
                    nRecvSize -= (int)wPacketSize;
                    var tempBuf = (byte[])mBufRecieve.Clone();
                    Buffer.BlockCopy(tempBuf, (int)wPacketSize, mBufRecieve, 0, nRecvSize);
                    //memmove(mBufRecieve, mBufRecieve + wPacketSize, nRecvSize);

                    //解密数据
                    uint wRealySize = CrevasseBuffer(m_cbRecDataBuffer, wPacketSize);
                    if (wRealySize < Packet.SizeOfTCP_Head)
                    {
                        Debug.LogError("onSocketData: wRealySize < Packet.SizeOfTCP_Head, wRealySize="+ wRealySize + ", wPacketSize="+ wPacketSize);

                        return;
                    }

                    //解释数据
                    //unsigned short wDataSize = wRealySize - sizeof(TCP_Head);
                    //void* pDataBuffer = m_cbRecDataBuffer + sizeof(TCP_Head);
                    //TCP_Command Command = ((TCP_Head*)m_cbRecDataBuffer)->CommandInfo;
                    uint wDataSize = (uint)(wRealySize - Packet.SizeOfTCP_Head);
                    byte[] pDataBuffer = new byte[Packet.SOCKET_TCP_PACKET];
                    Buffer.BlockCopy(m_cbRecDataBuffer, Packet.SizeOfTCP_Head, pDataBuffer, 0, pDataBuffer.Length);//Packet.SOCKET_TCP_PACKET

                    Packet.TCP_Head h =
                        (Packet.TCP_Head)StructConverterByteArray.BytesToStruct(m_cbRecDataBuffer, typeof(Packet.TCP_Head));
                    Packet.TCP_Command Command = h.CommandInfo;

                    //mChen log
                    if (wRealySize != wPacketSize)
                    {
                        Debug.LogError("onSocketData:wRealySize!=wPacketSize, wRealySize=" + wRealySize + ", wPacketSize=" + wPacketSize + ", wMainCmdID=" + Command.wMainCmdID + ", wSubCmdID=" + Command.wSubCmdID);
                    }

                    if (Command.wMainCmdID == Packet.MDM_KN_COMMAND && Command.wSubCmdID == Packet.SUB_KN_DETECT_SOCKET)
                    {
                        //Debug.Log("Send Heart Beat");
                        if (CServerItem.get() != null && CServerItem.get().GetServerItemSocketEngine() == this)
                        {
                            //是CServerItem连接

                            HNGameManager.BReceivedHeartBeatMsg = true;
                        }
                        else
                        {
                            //不是CServerItem连接
                            //Debug.LogError("onSocketData: SUB_KN_DETECT_SOCKET GetServerItemSocketEngine!=this");
                        }

                        //byte[] dataTmp = new byte[wDataSize];
                        //Buffer.BlockCopy(m_cbRecDataBuffer, Packet.SizeOfTCP_Head, dataTmp, 0, (int)wDataSize);
                        //send(Packet.MDM_KN_COMMAND, Packet.SUB_KN_DETECT_SOCKET, dataTmp, (int)wDataSize);
                        send(Packet.MDM_KN_COMMAND, Packet.SUB_KN_DETECT_SOCKET, pDataBuffer, (int)wDataSize);

                        if (mISocketEngineSink != null)
                        {
                            mISocketEngineSink.onEventTCPHeartTick();
                        }
                        continue;
                    }
                    else
                    {
                        if( Command.wMainCmdID == GameServerDefines.MDM_GF_GAME && Command.wSubCmdID == HNMJ_Defines.SUB_S_HideSeek_HeartBeat)
                        {

                        }
                        else if(Command.wMainCmdID == GameServerDefines.MDM_GF_FRAME && Command.wSubCmdID == GameServerDefines.SUB_GF_SYSTEM_MESSAGE)
                        {

                        }
                        else
                        {
                            Debug.Log("REV- --main command---- " + Command.wMainCmdID + " -- Sub Command -------" + Command.wSubCmdID);
                        }

                        //mChen add, for HideSeek: fix客户端7s断线检测经常误报
                        if (CServerItem.get()!=null && CServerItem.get().GetServerItemSocketEngine() == this)
                        {
                            //是CServerItem连接

                            HNGameManager.BReceivedHeartBeatMsg = true;
                        }
                    }

                    if (mISocketEngineSink != null)
                    {
                        //int wDataSizeTmp = (int)(wPacketSize - 8);
                        //byte[] dataTmp2 = new byte[wDataSizeTmp];
                        //Buffer.BlockCopy(m_cbRecDataBuffer, Packet.SizeOfTCP_Head, dataTmp2, 0, wDataSizeTmp);
                        //bool bHandle = mISocketEngineSink.onEventTCPSocketRead(Command.wMainCmdID, Command.wSubCmdID,
                        //        dataTmp2, wDataSizeTmp);
                        bool bHandle = mISocketEngineSink.onEventTCPSocketRead(Command.wMainCmdID, Command.wSubCmdID,
                            pDataBuffer, (int)(wPacketSize - 8));

                        if (!bHandle)
                        {
                            Debug.Log("REV- --main command---- " + Command.wMainCmdID + " -- Sub Command -------" + Command.wSubCmdID);
                            //CCASSERT(false, "");//不知道函数定义或者函数返回false

                            Debug.LogWarning("disconnect3.1: !bHandle cause disconnect!!!: "+ "main command " + Command.wMainCmdID + " Sub Command:" + Command.wSubCmdID);

                            //if(Command.wMainCmdID == GameServerDefines.MDM_GF_GAME && Command.wSubCmdID==HNMJ_Defines.SUB_S_HideSeek_HeartBeat)
                            //{
                            //}
                            //else
                            //{
                            //}
                            //mChen edit
                            continue;
                            //disconnect();
                            //return;
                        }
                    }
                }

                //mChen add, for HideSeek
                //Log
                if(nRecvSize!=0)
                {
                    Debug.LogError("onSocketData: nRecvSize!=0, nRecvSize=" + nRecvSize + ", wPacketSize="+ wPacketSize + ", dataSize="+ dataSize);

                    //temp_size_ = 0;
                    //Array.Clear(mTempBuf, 0, HeaderStruct.SIZE_TCP_BUFFER);
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // 辅助函数

        private void initValue()
        {
            if (BufLockObj == null)
            {
                BufLockObj = new object();
            }

            //lock (BufLockObj)
            {
                m_cbSendRound = 0;
                m_cbRecvRound = 0;
                m_dwSendXorKey = 0;
                m_dwRecvXorKey = 0;
                m_dwSendPacketCount = 0;
                m_dwRecvPacketCount = 0;
                mBufRevLength = 0;
                temp_size_ = 0;

                Array.Clear(mTempBuf, 0, HeaderStruct.SIZE_TCP_BUFFER);
                //memset(mTempBuf, SIZE_TCP_BUFFER, 0);
            }
        }

        private void sendTCPValidate()
        {
            if (mIsSendTCPValidate == false)
                return;
            // 获取验证信息
            QPCipher.tcpValidate(mBufPack, 0);
            // 发送验证
            string ss = "";
            for (int i = 0; i < QPCipher.SIZE_VALIDATE; i++)
            {
                ss += mBufPack[i];
                ss += ",";
            }
            //mSocket.send(ss, (int)ss.Length);
        }
        private byte[] pack(int main, int sub, byte[] data, int size)
        {
            mBufPack[0] = 0;
            int packsize = HeaderStruct.SIZE_PACK_HEAD + size;
            QPCipher.setPackInfo(mBufPack, packsize, main, sub);

            // 赋值
            if (size > 0)
            {
                for (int i = 0; i < size; ++i)
                {
                    mBufPack[i+ HeaderStruct.SIZE_PACK_HEAD] = data[i];
                }
                //memcpy(&mBufPack[SIZE_PACK_HEAD], data, size);
            }
            // 加密数据
            QPCipher.encryptBuffer(mBufPack, packsize);
            return mBufPack;
        }

        private bool unpack(byte[] data, int start, int length)
        {
            // 解密
            if ((data[start] & QPCipher.getCipherMode()) > 0)
            {
                QPCipher.decryptBuffer(data, start, length);
            }
            // 主命令码
            int main = QPCipher.getMainCommand(data, start);
            // 次命令码
            int sub = QPCipher.getSubConmmand(data, start);

            // 附加数据
            if (length > 8)
            {
                for (int i = 0; i < length - 8; ++i)
                {
                    mBufPack[i] = data[start + 8 + i];
                }
                //memcpy(mBufUnPack, &data[start + 8], length - 8);
            }

            length -= 8;



            //if (SOCKET_CHECK)
            //	PLAZZ_PRINTF("Main:%d Sub:%d Size:%d\n", main, sub, length);



            if (main == 0 && sub == 1)
            {
                //PLAZZ_PRINTF("REV-HEART\n");
            }
            else
            {
                if (mISocketEngineSink != null)
                {
                    bool bHandle = mISocketEngineSink.onEventTCPSocketRead(main, sub, mBufUnPack, length);

                    //if (!bHandle)
                    //	PLAZZ_PRINTF("no match: main:%d sub:%d size:%d\n", main, sub, length);
                    return bHandle;
                }
            }

            return true;
        }
        
        //加密数据
        public uint EncryptBuffer(ref byte[] pcbDataBuffer, uint wDataSize, uint wBufferSize)
        {
            uint wEncryptSize = (uint)(wDataSize - Packet.SizeOfTCP_Commend);
            uint wSnapCount = 0;
            if ((wEncryptSize % sizeof(uint)) != 0)
            {
                wSnapCount = (uint)(sizeof(uint) - wEncryptSize % sizeof(uint));
           
                Array.Clear(pcbDataBuffer, (int)(Packet.SizeOfTCP_Info + wEncryptSize), (int)wSnapCount);     
                // memset(pcbDataBuffer + Packet.SizeOfTCP_Info + wEncryptSize, 0, wSnapCount);
            }

            //效验码与字节映射
            Byte cbCheckCode = 0;
            uint i = 0;
            for (i = Packet.SizeOfTCP_Info; i < wDataSize; i++)
            {
                cbCheckCode += pcbDataBuffer[i];
                //cocos2d::log("CSocketEngine::EncryptBuffer MapSendByte -- 1 -- %d  ", pcbDataBuffer[i]);

                pcbDataBuffer[i] = MapSendByte(pcbDataBuffer[i]);

                //cocos2d::log("CSocketEngine::EncryptBuffer MapSendByte -- 2 -- %d  ", pcbDataBuffer[i]);
            }

            //填写信息头
#if true
            Packet.TCP_Info pHeadTCPInfo;// = (Packet.TCP_Info)StructConverterByteArray.BytesToStruct(pcbDataBuffer, typeof(Packet.TCP_Info));
            pHeadTCPInfo.cbCheckCode = (byte)(~cbCheckCode + 1);
            pHeadTCPInfo.wPacketSize = (ushort)wDataSize;
            pHeadTCPInfo.cbDataKind = Packet.DK_ENCRYPT;
            var headTCPInfoBuf = StructConverterByteArray.StructToBytes(pHeadTCPInfo);
            Buffer.BlockCopy(headTCPInfoBuf, 0, pcbDataBuffer, 0, headTCPInfoBuf.Length);
#else
            Packet.TCP_Head pHead = new Packet.TCP_Head();
            pHead.TCPInfo.cbCheckCode = (byte)(~cbCheckCode + 1);
            pHead.TCPInfo.wPacketSize = wDataSize;
            pHead.TCPInfo.cbDataKind = Packet.DK_ENCRYPT;
#endif
            //创建密钥
            uint dwXorKey = m_dwSendXorKey;
            if (m_dwSendPacketCount == 0)
            {
                //随机映射种子
                //dwXorKey = (uint)DateTime.Now.Ticks;//不知道函数定义
                dwXorKey = 1000;
                dwXorKey = SeedRandMap((ushort)dwXorKey);
                dwXorKey |= ((uint)SeedRandMap((ushort)(dwXorKey >> 16))) << 16;
                dwXorKey ^= Packet.g_dwPacketKey;
                m_dwSendXorKey = dwXorKey;
                m_dwRecvXorKey = dwXorKey;
                //Debug.Log("1new recv key: "+ m_dwRecvXorKey);
            }

            //加密数据

            ushort pwSeed;
            uint pdwXor;

            uint wEncrypCount = (ushort)((wEncryptSize + wSnapCount) / sizeof(uint));

            for (uint j = 0; j < wEncrypCount; j++)
            {
                pdwXor = BitConverter.ToUInt32(pcbDataBuffer, (int)(Packet.SizeOfTCP_Info + 4 * j));
                pdwXor ^= dwXorKey;
                var newValue = BitConverter.GetBytes(pdwXor);
                Buffer.BlockCopy(newValue, 0, pcbDataBuffer, (int)(Packet.SizeOfTCP_Info + 4*j), 4);
                pwSeed = BitConverter.ToUInt16(pcbDataBuffer, (int)(Packet.SizeOfTCP_Info + 4 * j));
                dwXorKey = SeedRandMap(pwSeed);
                pwSeed = BitConverter.ToUInt16(pcbDataBuffer, (int)(Packet.SizeOfTCP_Info + 4*j + 2));
                dwXorKey |= ((uint)SeedRandMap(pwSeed)) << 16;
 
                dwXorKey ^= Packet.g_dwPacketKey;
            }
            
            //插入密钥
            if (m_dwSendPacketCount == 0)
            {
                byte[] tempBuf = (byte[])pcbDataBuffer.Clone();
                Buffer.BlockCopy(tempBuf, Packet.SizeOfIPC_Head, pcbDataBuffer,Packet.SizeOfIPC_Head+sizeof(uint), (int)wDataSize);
                //memmove(pcbDataBuffer + Packet.SizeOfTCP_Head+Packet.SizeOf(uint), pcbDataBuffer + Packet.SizeOfTCP_Head, wDataSize);

                var sendKeyBuf = BitConverter.GetBytes(m_dwSendXorKey);
                Buffer.BlockCopy(sendKeyBuf, 0, pcbDataBuffer, Packet.SizeOfIPC_Head, sizeof(uint));
                //*((uint*)(pcbDataBuffer + Packet.SizeOfTCP_Head)) = m_dwSendXorKey;//
                //byte[] temp = System.BitConverter.GetBytes(m_dwSendXorKey);

                //pcbDataBuffer[Packet.SizeOfTCP_Head] = (byte)(m_dwSendXorKey >> 24);
                // pcbDataBuffer[Packet.SizeOfTCP_Head + 1] = (byte)(m_dwSendXorKey >> 16);
                //pcbDataBuffer[Packet.SizeOfTCP_Head + 2] = (byte)(m_dwSendXorKey >> 8);
                // pcbDataBuffer[Packet.SizeOfTCP_Head + 3] = (byte)(m_dwSendXorKey);

                //pHead.TCPInfo.wPacketSize += sizeof(uint);
                Packet.TCP_Info pHeadTCPInfoTmp = (Packet.TCP_Info)StructConverterByteArray.BytesToStruct(pcbDataBuffer, typeof(Packet.TCP_Info));
                pHeadTCPInfoTmp.wPacketSize += sizeof(uint);
                var headTCPInfoBufTmp = StructConverterByteArray.StructToBytes(pHeadTCPInfoTmp);
                Buffer.BlockCopy(headTCPInfoBufTmp, 0, pcbDataBuffer, 0, headTCPInfoBufTmp.Length);
                //var pHeadData = StructConverterByteArray.StructToBytes(pHead);
                //Buffer.BlockCopy(pHeadData, 0, pcbDataBuffer, 0, pHeadData.Length);

                wDataSize += sizeof(uint);
            }

            //cocos2d::log("CSocketEngine::EncryptBuffer cbCheckCode -- %d   m_dwSendXorKey --- %x ----   wDataSize ---- %d", pHead->TCPInfo.cbCheckCode, m_dwSendXorKey, wDataSize);

            //设置变量
            m_dwSendPacketCount++;
            m_dwSendXorKey = dwXorKey;

            return wDataSize;
        }
        //解密数据
        public uint CrevasseBuffer(byte[] pcbDataBuffer, uint wDataSize)//修改了参数类型
        {
            //if (wDataSize == 16 )
            //{
            //    Debug.Log("CrevasseBuffer wDataSize==16");
            //}

            //ASSERT(m_dwSendPacketCount > 0);
            System.Diagnostics.Debug.Assert(wDataSize >= Packet.SizeOfTCP_Head);
            //调整长度
            uint wSnapCount = 0;
            if ((wDataSize % sizeof(uint)) != 0)
            {
                wSnapCount = (uint)(sizeof(uint) - wDataSize % sizeof(uint));
                
                Array.Clear(pcbDataBuffer, (int)wDataSize, (int)wSnapCount);
                //memset(pcbDataBuffer + wDataSize, 0, wSnapCount);
            }

        
            if (m_dwRecvPacketCount == 0)
            {
                //数据包长度错误
                if (wDataSize < (Packet.SizeOfTCP_Head + sizeof(uint)))
                    return 0;

                //m_dwRecvXorKey = *(uint*)(pcbDataBuffer + Packet.SizeOfTCP_Head);
                m_dwRecvXorKey = BitConverter.ToUInt32(pcbDataBuffer, Packet.SizeOfTCP_Head);
                //Debug.Log("2new recv key: " + m_dwRecvXorKey);
                m_dwSendXorKey = m_dwRecvXorKey;
                //memmove(pcbDataBuffer + sizeof(TCP_Head), pcbDataBuffer + sizeof(TCP_Head) + sizeof(unsigned int),wDataSize - sizeof(TCP_Head) - sizeof(unsigned int));
                byte[] tempBuf = (byte[])pcbDataBuffer.Clone();
                Buffer.BlockCopy(tempBuf,Packet.SizeOfTCP_Head + sizeof(uint),pcbDataBuffer,Packet.SizeOfIPC_Head, (int)(wDataSize - Packet.SizeOfTCP_Head - sizeof(uint)) );
                
                wDataSize -= sizeof(uint);
                //((TCP_Head*)pcbDataBuffer)->TCPInfo.wPacketSize -= sizeof(unsigned int);
                var packetSize = BitConverter.ToUInt32(pcbDataBuffer, sizeof(byte) * 2) - sizeof(uint);
                var sizeBuf = BitConverter.GetBytes(packetSize);
                Buffer.BlockCopy(sizeBuf,0,pcbDataBuffer, sizeof(byte) * 2, sizeBuf.Length);
            }

            //解密数据
            uint dwXorKey = m_dwRecvXorKey;
            //uint* pdwXor = (uint*)(pcbDataBuffer + sizeof(TCP_Info));
            //ushort* pwSeed = (ushort*)(pcbDataBuffer + sizeof(TCP_Info));
            ushort pwSeed;
            uint pdwXor;
            uint wEncrypCount = (uint)((wDataSize + wSnapCount - Packet.SizeOfTCP_Info) / 4);
    
            for (uint i = 0; i < wEncrypCount; i++)
            {
                if ((i == (wEncrypCount - 1)) && (wSnapCount > 0))
                {
                    //byte[] pcbKeys = new byte[wSnapCount];
                    var pcbKeys = BitConverter.GetBytes(m_dwRecvXorKey);
                    
                    Buffer.BlockCopy(pcbKeys, (int)(sizeof(uint) - wSnapCount), pcbDataBuffer, (int)wDataSize, (int)wSnapCount);
                    //unsigned char* pcbKey = ((unsigned char*)&m_dwRecvXorKey) +sizeof(unsigned int)-wSnapCount;
                    //memcpy(pcbDataBuffer + wDataSize, pcbKey, wSnapCount);
                }

                pwSeed = BitConverter.ToUInt16(pcbDataBuffer, (int)(Packet.SizeOfTCP_Info + i * 4));
                

                dwXorKey = SeedRandMap(pwSeed);
                pwSeed = BitConverter.ToUInt16(pcbDataBuffer, (int)(Packet.SizeOfTCP_Info + i * 4 + 2));
                dwXorKey |= ((uint)SeedRandMap(pwSeed)) << 16;
                dwXorKey ^= Packet.g_dwPacketKey;

                pdwXor = BitConverter.ToUInt32(pcbDataBuffer, (int)(Packet.SizeOfTCP_Info + 4 * i));
                pdwXor ^= m_dwRecvXorKey;

                var newValue = BitConverter.GetBytes(pdwXor);
                Buffer.BlockCopy(newValue, 0, pcbDataBuffer, (int)(Packet.SizeOfTCP_Info + 4 * i), 4);

                m_dwRecvXorKey = dwXorKey;
                //Debug.Log("3new recv key: " + m_dwRecvXorKey);
            }
           // Debug.Log("----------!!!------------");
            //效验码与字节映射
            Packet.TCP_Info pHeadTCPInfo = (Packet.TCP_Info)StructConverterByteArray.BytesToStruct(pcbDataBuffer,typeof(Packet.TCP_Info));
           
            byte cbCheckCode = pHeadTCPInfo.cbCheckCode;
            for (int i = Packet.SizeOfTCP_Info; i < wDataSize; i++)
            {
                pcbDataBuffer[i] = MapRecvByte(pcbDataBuffer[i]);
                cbCheckCode += pcbDataBuffer[i];
            }
            if (cbCheckCode != 0)
            {
                return 0;
            }

            return wDataSize;
        }
       //随机映射
        public ushort SeedRandMap(ushort wSeed)
        {
               uint dwHold = wSeed;
               return (ushort)((dwHold =(uint)( dwHold * 241103L + 2533101L) >> 16));
        }
        //映射发送数据
        public Byte MapSendByte(Byte cbData)
        {
            Byte cbMap = Packet.g_SendByteMap[(Byte)(cbData + m_cbSendRound)];

            // cocos2d::log("CSocketEngine::EncryptBuffer MapSendByte -- g_SendByteMap->cbMap -- %d-----%d---%d  ", cbMap, m_cbSendRound, cbData);

            m_cbSendRound += 3;
            return cbMap;
        }
         
        //映射接收数据
        public Byte MapRecvByte(Byte cbData)
        {
            Byte cbMap = (Byte)(Packet.g_RecvByteMap[cbData] - m_cbRecvRound);
            m_cbRecvRound += 3;
            return cbMap;
        }
    }
}