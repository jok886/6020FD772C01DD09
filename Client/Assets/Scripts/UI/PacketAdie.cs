using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GameNet;

namespace GameNet
{
    //////////////////////////////////////////////////////////////////////////////////							

    //数据描述
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagDataDescribe
    {
        public ushort wDataSize;                     //数据大小
        public ushort wDataDescribe;                 //数据描述
    };

    //////////////////////////////////////////////////////////////////////////////////

    //发送辅助类
    class CSendPacketHelper
    {
        //变量定义
        protected ushort m_wDataSize;                   //数据大小
        protected ushort m_wMaxBytes;                   //缓冲大小
        protected byte[] m_pcbBuffer;                  //缓冲指针
        protected Type m_ttagDataDescribe;//类型信息
        //函数定义
        public CSendPacketHelper(byte[] pcbBuffer, ushort wMaxBytes) //构造函数
        {
            //设置变量
            m_wDataSize = 0;
            m_wMaxBytes = wMaxBytes;
            m_pcbBuffer = pcbBuffer;
            m_ttagDataDescribe = typeof (tagDataDescribe);
        }

        //功能函数
        //清理数据
        public void CleanData() { m_wDataSize = 0; }
        
        //获取大小
        public ushort GetDataSize() { return m_wDataSize; }
        //获取缓冲
        public byte[] GetDataBuffer() { return m_pcbBuffer; }

        //功能函数
        //插入数据
        public bool AddPacket(byte[] pData, ushort wDataType)
        {
            if ((pData.Length + Marshal.SizeOf(m_ttagDataDescribe) + m_wDataSize) > m_wMaxBytes) return false;

            //插入数据
            tagDataDescribe pDataDescribe = new tagDataDescribe();
            pDataDescribe.wDataSize = (ushort)pData.Length;
            pDataDescribe.wDataDescribe = wDataType;
            var buf = StructConverterByteArray.StructToBytes(pDataDescribe);
            Buffer.BlockCopy(buf,0,m_pcbBuffer,m_wDataSize,buf.Length);
            //tagDataDescribe* pDataDescribe = (tagDataDescribe*)(m_pcbBuffer + m_wDataSize);
            //pDataDescribe->wDataSize = wDataSize;
            //pDataDescribe->wDataDescribe = wDataType;

            //插入数据
            if (pData.Length > 0)
            {
                Buffer.BlockCopy(pData,0,m_pcbBuffer,m_wDataSize+buf.Length,pData.Length);
               //memcpy(pDataDescribe + 1, pData, wDataSize);
            }

            //设置数据
            m_wDataSize += (ushort)(buf.Length + pData.Length);

            return true;
        }
    };

    //////////////////////////////////////////////////////////////////////////////////

    //接收辅助类
    class CRecvPacketHelper
    {
        //变量定义
        protected ushort m_wDataPos;                        //数据点
        ushort m_wDataSize;                   //数据大小
        byte[] m_pcbBuffer;                  //缓冲指针
        protected Type m_ttagDataDescribe;//类型信息
        public const int DTP_NULL = 0;//无效数据
        //函数定义
        public CRecvPacketHelper(byte[] pcbBuffer, ushort wDataSize)
        {
            m_wDataPos = 0;
            m_wDataSize = wDataSize;
            m_pcbBuffer = pcbBuffer;
            m_ttagDataDescribe = typeof(tagDataDescribe);
        }
        //构造函数

        //功能函数
        public byte[] GetData(ref tagDataDescribe DataDescribe)
        {
            //效验数据
            if (m_wDataPos >= m_wDataSize)
            {
                DataDescribe.wDataSize = 0;
                DataDescribe.wDataDescribe = DTP_NULL;//无效数据
                return null;
            }

            //获取数据
            var structSize = Marshal.SizeOf(m_ttagDataDescribe);
            byte[] buf = new byte[structSize];
            Buffer.BlockCopy(m_pcbBuffer,m_wDataPos,buf,0, structSize);
            DataDescribe = (tagDataDescribe)StructConverterByteArray.BytesToStruct(buf, m_ttagDataDescribe);
           
            //memcpy(&DataDescribe, m_pcbBuffer + m_wDataPos, sizeof(tagDataDescribe));
            //ASSERT((m_wDataPos + sizeof(tagDataDescribe) + DataDescribe.wDataSize) <= m_wDataSize);

            //效验数据
            if ((m_wDataPos + structSize + DataDescribe.wDataSize) > m_wDataSize)
            {
                DataDescribe.wDataSize = 0;
                DataDescribe.wDataDescribe = DTP_NULL;
                return null;
            }

            //设置数据
            byte[] pData = null;
            if (DataDescribe.wDataSize > 0)
            {
                pData = new byte[DataDescribe.wDataSize];
                Buffer.BlockCopy(m_pcbBuffer,m_wDataPos + structSize,pData,0,DataDescribe.wDataSize);
                //pData = m_pcbBuffer + m_wDataPos + structSize;
            }
            m_wDataPos += (ushort)(structSize + DataDescribe.wDataSize);

            return pData;
        }
        //获取数据
    };
}
