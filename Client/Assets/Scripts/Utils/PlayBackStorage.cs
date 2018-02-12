using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GameNet;
using UnityEngine;

public class PlayBackStorage
{
    private static PlayBackStorage _current = null;
    private bool bRecording = false;

    public struct GameMsgEvent
    {
        public int dataSize;//(这里datasize指示存储的该条msg的大小，为data的大小+12，其中12为3个int的大小)
        public int main;
        public int sub;
        public byte[] data;

        public GameMsgEvent(int mainCmd, int subCmd, byte[] cmdData, int cmdDataSize)
        {
            main = mainCmd;
            sub = subCmd;
            data = cmdData;
            dataSize = cmdDataSize + 12;
        }

        public void WriteData(BinaryWriter br)
        {
            br.Write(dataSize);
            br.Write(main);
            br.Write(sub);
            br.Write(data, 0, dataSize - 12);
        }

        public void ReadData(BinaryReader br)
        {
            dataSize = br.ReadInt32() - 12;
            main = br.ReadInt32();
            sub = br.ReadInt32();
            data = br.ReadBytes(dataSize);
        }
    }

    private List<GameMsgEvent> msgQueue;

    public struct UserInfoStorage
    {/*
        public Sprite HeadSprite;*/
        public string NickName;
        public byte BGender;
        public uint IUserId;
        public UserInfoStorage(/*Sprite headImg,*/ string nick, byte bSex, uint id)
        {
            /*HeadSprite = headImg;*/
            NickName = nick;
            BGender = bSex;
            IUserId = id;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct UserInfoStruct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)]
        public byte[] NickName;
        public byte BGender;
        public uint IUserId;
    }

    private UserInfoStorage[] userInfoStorages;
    private UserInfoStruct[] savedUserInfoData;

    public static PlayBackStorage GetInstance()
    {
        if (_current == null)
        {
            _current = new PlayBackStorage();
            _current.lockObj = new object();
            _current.msgQueue = new List<GameMsgEvent>();

        }
        return _current;
    }

    private const int MaxRecordCount = 5;
    private int CurrentRecordCount;
    private int nextRecordNum;

    public void StartRecord(HNGameManager hnGameManager)
    {
        Debug.LogError("StartRecord");
        return;

#if UNITY_STANDALONE
        if(HNGameManager.m_iLocalChairID != 0)
            return;
#endif
        var kernel = (GameScene) CServerItem.get().GetClientKernelSink();
        CurrentRecordCount = PlayerPrefs.GetInt("RecordNum", 0);
        var dirStr = "";
        var trueRecord = CurrentRecordCount;
        if (kernel.getPlayCount() == 1)//新的一场游戏
        {
            CurrentRecordCount++;
            PlayerPrefs.SetInt("RecordNum", CurrentRecordCount);
            DirectoryInfo dInfo =
                   new DirectoryInfo(Application.persistentDataPath + string.Format("/{0}", DateTime.Now.ToString("yyyyMMddhhmmss")));
            dInfo.Create();
            Debug.Log("creating saved Dir :" + dInfo.FullName);
            if (CurrentRecordCount > MaxRecordCount)
            {
                trueRecord = (CurrentRecordCount%(MaxRecordCount + 1)) + 1;
                //删除之前的文件夹
                dirStr = PlayerPrefs.GetString(string.Format("RecordFile{0}", trueRecord));
                Debug.Log("deleting saved dir :" + dirStr);
                var oldDirInfo = new DirectoryInfo(dirStr);
                oldDirInfo.Delete(true);
            }
            else
            {
                trueRecord = CurrentRecordCount;
            }
            Debug.Log("dir full name " + dInfo.FullName);
            PlayerPrefs.SetString(string.Format("RecordFile{0}", trueRecord), dInfo.FullName);
            dirStr = dInfo.FullName;
            FileStream fInfo = new FileStream(string.Format("{0}/recordInfo.save", dInfo.FullName), FileMode.OpenOrCreate);
            bwWriter = new BinaryWriter(fInfo);
            bwWriter.Write(hnGameManager.TempRoomId);
            bwWriter.Write(hnGameManager.CreateUserID);
            bwWriter.Write(hnGameManager.totalcount);
            bwWriter.Write(HNGameManager.m_iLocalChairID);

            userInfoStorages = new UserInfoStorage[4];

            hnGameManager.SaveUserInfo(ref userInfoStorages);
            savedUserInfoData = new UserInfoStruct[4];
            for (int i = 0; i < userInfoStorages.Length; i++)
            {
                savedUserInfoData[i] = new UserInfoStruct
                {
                    BGender = userInfoStorages[i].BGender,
                    IUserId = userInfoStorages[i].IUserId,
                    NickName = new byte[SocketDefines.LEN_NICKNAME]
                };
                var nickBuf = Encoding.UTF8.GetBytes(userInfoStorages[i].NickName);
                Buffer.BlockCopy(nickBuf, 0, savedUserInfoData[i].NickName, 0, SocketDefines.LEN_NICKNAME);
                var infoBuf = StructConverterByteArray.StructToBytes(savedUserInfoData[i]);
                bwWriter.Write(infoBuf, 0, infoBuf.Length);
            }

            bwWriter.Flush();
            bwWriter.Close();
            fInfo.Close();
        }
        else
        {
            if (CurrentRecordCount > MaxRecordCount)
            {
                trueRecord = (CurrentRecordCount % (MaxRecordCount + 1)) + 1;
            }
            Debug.Log("True record is " + trueRecord);
            dirStr = PlayerPrefs.GetString(string.Format("RecordFile{0}", trueRecord));
        }

        fs = new FileStream(string.Format("{0}/recordInfo.save", dirStr), FileMode.Append);
        bwWriter = new BinaryWriter(fs);
        bwWriter.Write(DateTime.Now.ToString("yyyyMMddhhmmss"));
        bwWriter.Flush();
        bwWriter.Close();
        fs.Close();
        Debug.Log("dirStr path: " + dirStr);
        //DirectoryInfo dInfo = new DirectoryInfo(Application.persistentDataPath);
        //var files = dInfo.GetFiles()
        fs = new FileStream(string.Format("{0}/{1}.save", dirStr, hnGameManager.nowcount),
            FileMode.OpenOrCreate);
        Debug.Log("writing saved file :" + fs.Name);
        bwWriter = new BinaryWriter(fs);

        bRecording = true;
        msgQueue.Clear();
    }

    //gameend 消息，存储当局分数等信息到recordInfo.save文件内
    //解散时data为空，当前局所有玩家得分为0
    public void StopRecord(byte[] data = null, int datasize = 0)
    {
        Debug.LogError("StopRecord");
        return;

        if (!bRecording)
            return;

#if UNITY_STANDALONE
        if (HNGameManager.m_iLocalChairID != 0)
            return;
#endif
        for (int i = 0; i < msgQueue.Count; i++)
        {
            msgQueue[i].WriteData(bwWriter);
        }
        bwWriter.Flush();
        bwWriter.Close();
        fs.Close();
        var trueRecord = CurrentRecordCount;
        if (CurrentRecordCount > MaxRecordCount)
        {
            trueRecord = (CurrentRecordCount % (MaxRecordCount + 1)) + 1;
        }
        var dirStr = PlayerPrefs.GetString(string.Format("RecordFile{0}", trueRecord));//存档文件夹
        FileStream fInfo = new FileStream(string.Format("{0}/recordInfo.save", dirStr), FileMode.Append);
        bwWriter = new BinaryWriter(fInfo);

        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        for (int i = 0; i < savedUserInfoData.Length; i++)
        {
//#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
//            string filename = string.Format("{0}/{1}.png", dirStr, savedUserInfoData[i].IUserId);
//            if (File.Exists(filename) == false)
//            {
//                if (HNGameManager.userId2HeadImageDirectory.ContainsKey(savedUserInfoData[i].IUserId))
//                {
//                    var sprite = HNGameManager.userId2HeadImageDirectory[savedUserInfoData[i].IUserId];
//                    Loom.QueueOnMainThread(() =>
//                    {
//                        var buf = sprite.texture.EncodeToPNG();
//                        File.WriteAllBytes(filename, buf);
//                    });
//                }
//            }
//#endif
        }
        
        if (data == null)
        {
            Debug.Log("----------write 0 to file1111");
            for (int i = 0; i < 4; i++)
            {
                bwWriter.Write((long)(0));
            }
        }
        else
        {
            var typeValue = typeof(CMD_S_GameEnd);
            CMD_S_GameEnd pGameEnd = (CMD_S_GameEnd)StructConverterByteArray.BytesToStruct(data, typeValue);
            //bwWriter.Write(pGameEnd.lGameScore[HNGameManager.m_iLocalChairID]);
            //Debug.Log("-------------Write score to file: " + pGameEnd.lGameScore[HNGameManager.m_iLocalChairID]);
            //var curPlayerChairID = HNGameManager.getNextPlayerChairID(HNGameManager.m_iLocalChairID);
            //int iIndex = 1;
            //while (curPlayerChairID != HNGameManager.m_iLocalChairID)
            //{
            //    bwWriter.Write(pGameEnd.lGameScore[curPlayerChairID]);
            //    Debug.Log("-------------Write score to file: " + pGameEnd.lGameScore[curPlayerChairID]);
            //    curPlayerChairID = HNGameManager.getNextPlayerChairID(curPlayerChairID);
            //}
        }
        bwWriter.Flush();
        bwWriter.Close();
        fInfo.Close();

        bRecording = false;
        msgQueue.Clear();
        userInfoStorages = null;
    }
    
    private const int nMsgToSave = 10;
    private FileStream fs;
    private BinaryWriter bwWriter;
    private BinaryReader bwReader;
    private object lockObj;
    private int currentMatch;
    public int GetCurrentRecordIdx()
    {
        return currentMatch;
    }
    public void RecordGameMsg(int main, int sub, byte[] data, int datasize)
    {
        if (bRecording)
        {
            lock (lockObj)
            {
                if (sub == HNMJ_Defines.SUB_S_GAME_END)//长度不对应，读取时会报异常，特殊处理下
                {
                    var typeValue = typeof(CMD_S_GameEnd);
                    var typeSize = Marshal.SizeOf(typeValue);
                    var buf = new byte[typeSize];
                    Buffer.BlockCopy(data, 0, buf, 0, datasize);
                    data = buf;
                    datasize = typeSize;
                }
                msgQueue.Add(new GameMsgEvent(main, sub, data, datasize));
                if (msgQueue.Count >= nMsgToSave)
                {
                    for (int i = 0; i < msgQueue.Count; i++)
                    {
                        msgQueue[i].WriteData(bwWriter);
                    }
                    msgQueue.Clear();
                }
            }
        }
    }

    public struct readRecordStruct
    {
        public string[] matchStartTime;//每局开始游戏时间
        public string dirStr;
        public int iMatchIdx;//当前播放第几局回放
        public int roomID;
        public uint createUserID;
        public int totalCount;
        public int localUserID;

        public UserInfoStruct[] userInfo;
        public long[,] ScorePerMatch;
        public long[] ScoreTotal;
        public void Init()
        {
            userInfo = new UserInfoStruct[4];
            ScoreTotal = new long[4];
        }
    }

    private readRecordStruct[] m_allRecords;
    public void ShowOneMatchRecord(int recordIndex)
    {
        var dirIndex = recordIndex;
        recordIndex--;//从1开始，变成从0开始
        m_allRecords[recordIndex].Init();
        m_allRecords[recordIndex].dirStr = PlayerPrefs.GetString(string.Format("RecordFile{0}", dirIndex));//存档文件夹
        DirectoryInfo dirInfo = new DirectoryInfo(m_allRecords[recordIndex].dirStr);
        var fileInfo = dirInfo.GetFiles();
        int replyCount = fileInfo.Length;
#if UNITY_IOS || UNITY_ANDROID
                replyCount -= 5;
#else
        replyCount -= 1;
#endif
        FileStream fInfo = new FileStream(string.Format("{0}/recordInfo.save", m_allRecords[recordIndex].dirStr), FileMode.Open);
        var bwReader = new BinaryReader(fInfo);

        m_allRecords[recordIndex].Init();
        m_allRecords[recordIndex].roomID = bwReader.ReadInt32();
        m_allRecords[recordIndex].createUserID = bwReader.ReadUInt32();
        m_allRecords[recordIndex].totalCount = bwReader.ReadInt32();
        m_allRecords[recordIndex].localUserID = bwReader.ReadInt32();

        Debug.Log("roomID " + m_allRecords[recordIndex].roomID + "createuserID " + m_allRecords[recordIndex].createUserID + " count " + m_allRecords[recordIndex].totalCount
                   + " local user is " + m_allRecords[recordIndex].localUserID);

        var typeValue = typeof(UserInfoStruct);
        var typeSize = Marshal.SizeOf(typeValue);
        for (int i = 0; i < 4; i++)
        {
            var buf = bwReader.ReadBytes(typeSize);
            m_allRecords[recordIndex].userInfo[i] = (UserInfoStruct)StructConverterByteArray.BytesToStruct(buf, typeValue);
        }
        m_allRecords[recordIndex].ScorePerMatch = new long[replyCount, 4];
        m_allRecords[recordIndex].matchStartTime = new string[replyCount];
        for (int i = 0; i < replyCount; i++)
        {
            m_allRecords[recordIndex].matchStartTime[i] = bwReader.ReadString();
            for (int j = 0; j < 4; j++){
                
                m_allRecords[recordIndex].ScorePerMatch[i,j] = bwReader.ReadInt64();
                m_allRecords[recordIndex].ScoreTotal[j] += m_allRecords[recordIndex].ScorePerMatch[i, j];
            }
        }
        bwReader.Close();
        fInfo.Close();
    }

    public void ShowRecordUI()
    {
        
        CurrentRecordCount = PlayerPrefs.GetInt("RecordNum", 0);
        Debug.Log("ShowRecordUI" + CurrentRecordCount);
        if (CurrentRecordCount <= MaxRecordCount)
        {
            m_allRecords = new readRecordStruct[CurrentRecordCount];
            for (int i = CurrentRecordCount; i >= 1; i--)//显示最近玩的在上面
            {
                ShowOneMatchRecord(i);
            }
        }
        else
        {
            m_allRecords = new readRecordStruct[MaxRecordCount];
            var trueStartIndex = (CurrentRecordCount % (MaxRecordCount + 1)) + 1;
    
            for (int i = 0; i < MaxRecordCount; i++)
            {
                ShowOneMatchRecord(trueStartIndex--);
                if (trueStartIndex == 0)
                {
                    trueStartIndex = MaxRecordCount;
                }
            }
        }
    }

    public void InitUserInfo(int recordIdx, ref tagUserInfo uInfo, int iIndex)
    {
        uInfo.wTableID = uInfo.wLastTableID = 0;
        var trueChairID = m_allRecords[recordIdx].localUserID;
        var cout = iIndex;
        while (--cout >= 0)
        {
            trueChairID = HNGameManager.getNextPlayerChairID(trueChairID);
        }
        uInfo.wChairID = (ushort)trueChairID;
        //userinfo里的内容是先存本人，然后其他人，所以索引和iIndex相同
        uInfo.dwUserID = m_allRecords[recordIdx].userInfo[iIndex].IUserId;
        uInfo.cbGender = m_allRecords[recordIdx].userInfo[iIndex].BGender;
        Buffer.BlockCopy(m_allRecords[recordIdx].userInfo[iIndex].NickName, 0, uInfo.szNickName, 0, m_allRecords[recordIdx].userInfo[iIndex].NickName.Length);
    }

    public void SetMatchIdx(int iRecordIdx, int iMatchIdx)
    {
        currentMatch = iRecordIdx;
        m_allRecords[iRecordIdx].iMatchIdx = iMatchIdx + 1;
        fs = new FileStream(string.Format("{0}/{1}.save", m_allRecords[iRecordIdx].dirStr, m_allRecords[iRecordIdx].iMatchIdx),
           FileMode.Open);
        Debug.Log("Reading saved file :" + fs.Name);
        bwReader = new BinaryReader(fs);
        ReadAllMsg();
        bwReader.Close();
        fs.Close();
    }

    private int curMsgIndex = 0;
    //返回已经播放的百分比
    public float TickGameMsg(out GameMsgEvent msg)
    {
        if (curMsgIndex < msgQueue.Count)
        {
            msg = msgQueue[curMsgIndex];
            return ((float)curMsgIndex ++ )/msgQueue.Count;
        }
        msg = new GameMsgEvent();
        return 1.0f;
    }

    public void ReadAllMsg()
    {
        var bEnd = false;
        while (bEnd == false)
        {
            var msg = new GameMsgEvent();
            try
            {
                msg.ReadData(bwReader);
                msgQueue.Add(msg);
            }
            catch (EndOfStreamException e)
            {
                Debug.Log("读到文件末尾！");
                bEnd = true;
            }
        }
        curMsgIndex = 0;

    }

    public readRecordStruct[] GetAllRecords()
    {
        return m_allRecords;
    }

    public readRecordStruct GetRecord(int iIdx)
    {
        return m_allRecords[iIdx];
    }
}

