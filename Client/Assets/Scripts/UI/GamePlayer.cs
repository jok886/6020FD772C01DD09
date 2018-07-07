using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GameNet
{
    public class GamePlayer
    {
        public enum PlayerAnimType
        {
            NewCardIn = 0,
            BuHua,
            ChaPai,
            ShowLeftOutCard,
            SetOperateResoult,
            ResetHandcardBeforeGameEnd,
            GameEndShowCard,
            ///HuAction,
            ///OnSubOperateNotify,
            SetCurrentPlayer,
        }

        public GamePlayer(IClientUserItem pUserItem)
        {
            m_pUserItem = (pUserItem);
        }

        public void setUserItem(IClientUserItem pItem)
        {
            m_pUserItem = pItem;
            upPlayerInfo();
        }

        public IClientUserItem getUserItem(bool bAssert = true)
        {
            return m_pUserItem;
        }

        public ushort GetTableID()
        {
            if (m_pUserItem == null)
            {
                return 0;
            }
            return m_pUserItem.GetTableID();
        }

        public ushort GetChairID()
        {
            if (m_pUserItem == null)
            {
                return 0;
            }
            return m_pUserItem.GetChairID();
        }

        public byte GetUserStatus()
        {
            if (m_pUserItem == null)
            {
                return 0;
            }
            return m_pUserItem.GetUserStatus();
        }

        public long GetUserScore()
        {
            if (m_pUserItem == null)
            {
                return 0;
            }
            return m_pUserItem.GetUserScore();
        }

        public ushort GetFaceID()
        {
            if (m_pUserItem == null)
            {
                return 0;
            }
            return m_pUserItem.GetFaceID();
        }

        public byte GetGender()
        {
            if (m_pUserItem == null)
            {
                return 0;
            }
            return m_pUserItem.GetGender();
        }

        public uint GetUserID()
        {
            if (m_pUserItem == null)
            {
                return 0;
            }
            return m_pUserItem.GetUserID();
        }

        public uint GetGameID()
        {
            if (m_pUserItem == null)
            {
                return 0;
            }
            return m_pUserItem.GetGameID();
        }

        public string GetHeadHttp()
        {
            if (m_pUserItem == null)
            {
                return "";
            }
            return Encoding.Default.GetString(m_pUserItem.GetUserInfo().szHeadHttp);
        }

        //用户昵称
        public string GetNickName()
        {
            if (m_pUserItem == null)
            {
                return "";
            }
            string result = GlobalUserInfo.GBToUtf8(m_pUserItem.GetNickName());
            return result;
            ///return Encoding.Default.GetString(m_pUserItem.GetNickName());
        }

        //用户信息
        public tagUserInfo? GetUserInfo()
        {
            if (m_pUserItem == null)
            {
                return null;
            }
            return m_pUserItem.GetUserInfo();
        }

        public virtual void PlayerEnter()
        {
        }

        public virtual void PlayerLeave()
        {
        }

        public virtual void upPlayerInfo()
        {
        }

        public virtual void upPlayerState()
        {
        }

        protected IClientUserItem m_pUserItem;
    };


    struct OutCardInfo
    {
        public OutCardInfo( byte nData)
        {
            //pCardNode = (pCard);
            nCardData = (nData);
        }
        //public GameObject pCardNode;
        public byte nCardData;
    };

    public class HNMJ_Defines
    {
        //组件属性
        public const int GAME_PLAYER = 20;                          //游戏人数

        // for HideSeek WangHu
        public const byte INVALID_AI_ID = 255;

        public const int GAME_GENRE = (SocketDefines.GAME_GENRE_SCORE | SocketDefines.GAME_GENRE_MATCH | SocketDefines.GAME_GENRE_GOLD);	//游戏类型

        //游戏状态
        //public const int GS_MJ_FREE = SocketDefines.GAME_STATUS_FREE;				//空闲状态
        //public const int GS_MJ_PLAY = (SocketDefines.GAME_STATUS_PLAY+1);			//游戏状态

        public const int TIME_START_GAME = 30;					//开始定时器
        public const int TIME_OPERATE_CARD = 15;                                //操作定时器

        //////////////////////////////////////////////////////////////////////////



        //////////////////////////////////////////////////////////////////////////
        //服务器命令结构
        public const int SUB_S_GAME_START = 100;					//游戏开始
        public const int SUB_S_OUT_CARD = 101;								//出牌命令
        public const int SUB_S_SEND_CARD = 102;							//发送扑克
        public const int SUB_S_OPERATE_NOTIFY = 104;							//操作提示
        public const int SUB_S_OPERATE_RESULT = 105;								//操作命令
        public const int SUB_S_GAME_END = 106;								//游戏结束
        public const int SUB_S_TRUSTEE = 107;								//用户托管
        public const int SUB_S_CHI_HU = 108;								//
        public const int SUB_S_GANG_SCORE = 110;                                //
        public const int SUB_S_REPLACE_CARD = 111;						    //用户补牌

        public const int SUB_S_CHAT_PLAY = 1000;                           //获取游戏中聊天

        // for HideSeek
        public const int SUB_S_HideSeek_HeartBeat = 115;
        public const int SUB_S_HideSeek_AICreateInfo = 116;


        public const int SUB_C_AWARD_DONE = 1002;                          //接收游戏中奖励
        public const int SUB_S_AWARD_RESULT = 1003;                       //游戏中奖励
        //WQ 道具消费
        public const int SUB_C_CONSUMPTION_INVENTORY = 1004;            //道具消耗
        public const int SUB_S_CONSUMPTION_INVENTORY_RESULT = 1005;     //道具消耗返回命令 
        public const int SUB_S_CONSUMPTION_INVENTORY_EVENT = 1006;     //道具消耗返回事件 

        public const int ZI_PAI_COUNT = 7;									//堆立全牌

        public const uint MASK_CHI_HU_RIGHT = 0x0fffffff;					//最大权位DWORD个数			

        //常量定义
        public const int MAX_WEAVE = 5;							//最大组合
        public const int MAX_INDEX = 34;							//最大索引
        public const int MAX_COUNT = 17;							//最大数目
        public const int MAX_REPERTORY = 136;                                   //最大库存
        public const int MAX_HUA_CARD = 8;                              //花牌个数

        public const int MAX_NIAO_CARD = 6;								//最大中鸟数


        //////////////////////////////////////////////////////////////////////////
        //客户端命令结构

        public const int SUB_C_OUT_CARD = 1;								//出牌命令
        public const int SUB_C_OPERATE_CARD = 3;								//操作扑克
        public const int SUB_C_TRUSTEE = 4;								//用户托管
        public const int SUB_C_XIAOHU = 5;                                  //小胡

        // for HideSeek
        public const int SUB_C_HIDESEEK_PLAYER_INFO = 6;
        public const int SUB_C_HIDESEEK_PLAYERS_INFO = 7;

        public const int SUB_C_CHAT_PLAY = 1001;                        //发送游戏中聊天    

        //WQ add
        //结束原因
        public const byte GER_NORMAL = 0x00;                                //常规结束
        public const byte GER_DISMISS = 0x01;                               //游戏解散
        public const byte GER_USER_LEAVE = 0x02;                                //用户离开
        public const byte GER_NETWORK_ERROR = 0x03;                             //网络错误
        public const byte GER_NOT_END = 0x05;
    }


    //组合子项
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_WeaveItem
    {
        public byte cbWeaveKind; //组合类型
        public byte cbCenterCard; //中心扑克
        public byte cbPublicCard; //公开标志
        public ushort wProvideUser; //供应用户
    };

    // for HideSeek
    //道具同步
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct InventoryItem
    {
        public byte cbId;
        public byte cbType;
        //public byte bCreated;
        public byte cbUsed;
    };

    //游戏状态
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_StatusFree
    {
        // for HideSeek
        public byte cbGameStatus;
        public byte cbMapIndex;
        //for随机种子同步
        public ushort wRandseed;
        //地图随机物品生成
        public ushort wRandseedForRandomGameObject;
        //道具同步
        public ushort wRandseedForInventory;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = InventoryManager.MAX_INVENTORY_NUM)]
        public InventoryItem[] sInventoryList;
    };

    //游戏状态
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_StatusPlay
    {
        public byte cbGameStatus;
        public byte cbMapIndex;

        //for随机种子同步
        public ushort wRandseed;
        //地图随机物品生成
        public ushort wRandseedForRandomGameObject;
        //道具同步
        public ushort wRandseedForInventory;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = InventoryManager.MAX_INVENTORY_NUM)]
        public InventoryItem[] sInventoryList;
    };
    //商品信息 WQ
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_C_InventoryConsumptionInfo
    {
        public uint dwUserID;                   //用户标识
        public byte cbItemID;                    //商品ID
        public ushort wAmount;                  //商品金额
        public byte cbCostType;                 //消费类型
    };

    //消费道具信息反馈
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_S_InventoryConsumptionInfoResult
    {
        public uint dwUserID;                           //用户标识
        public byte cbSuccess;                          //购买成功
        public byte cbItemID;                           //商品ID
        public uint dwFinalScore;                       //当前钻石/金币
        public byte cbCostType;                         //道具金额类型  
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] szDescribeString;                 //描述

        public void Init()
        {
            szDescribeString = new byte[256];
        }
    };
    //消费道具信息事件
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_S_InventoryConsumptionEvent
    {
        public byte cbChairID;                          //使用道具者座位ID
        public byte cbItemID;                           //商品ID
        public Int32 dwModelIndex;                      //使用变身道具后的modelindex
        public byte cbStealthStatus;                    //隐身状态
        public byte cbStealthTimeLeft;                  //隐身剩余时间 
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        //public byte[] szDescribeString;                 //描述

        //public void Init()
        //{
        //    szDescribeString = new byte[256];
        //}
    };
    //执行奖励
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_C_AwardDone
    {
        public uint dwUserID; //用户标识
        public uint dwAwardGold;//奖励的金额
        public byte cbCostType; //花费类型：0-金币，1-钻石

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_PASSWORD)]
        public byte[] szPassword; //登录密码

        public void Init()
        {
            szPassword = new byte[SocketDefines.LEN_PASSWORD];
        }
    };
    //执行奖励结果
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct CMD_S_AwardResult
    {
        public uint dwUserID; //用户标识
        public byte bSuccessed; //成功标识
        public byte cbCostType; //花费类型：0-金币，1-钻石
        public long lScore; //当前金额

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] szNotifyContent; //提示内容
    };

    //文字聊天数据
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_C_CHAT
    {
        public ushort ChairId;
        public byte UserStatus;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public byte[] ChatData;
    }

    //游戏开始
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_GameStart
    {
        public int lSiceCount;                                  //骰子点数
        public ushort wBankerUser;                               //庄家用户
        public ushort wCurrentUser;                              //当前用户
        public byte cbUserAction;                              //用户动作
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.MAX_COUNT * HNMJ_Defines.GAME_PLAYER)]
        public byte[] cbCardData;           //扑克列表
        public byte cbLeftCardCount;                           //
        public byte cbXiaoHuTag;                             //小胡标记 0 没小胡 1 有小胡；
    };


    //出牌命令
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_OutCard
    {
        public ushort wOutCardUser;                      //出牌用户
        public byte cbOutCardData;                     //出牌扑克

        public byte bIsPiao;//spHe add
    };

    //发送扑克
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_SendCard
    {
        public byte cbSendCardData;                        //扑克数据
        public byte cbActionMask;                      //动作掩码
        public ushort wCurrentUser;                      //当前用户
        public byte bTail;                             //末尾发牌

        //WQ add
        //public byte cbActionCard;                      //动作扑克
        public tagGangCardResult tGangCard;				//杠数据（最多有5个杠）
        public byte bKaiGangYaoShaiZi;                 //是否开杠摇骰子
        ///public ushort wReplaceUser;						//补牌用户
    };


    //操作提示
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_OperateNotify
    {
        public ushort wResumeUser;                       //还原用户
        public byte cbActionMask;                      //动作掩码
        public byte cbActionCard;                      //动作扑克
        ///public bool bForceAction;						//for海南麻将倒数第二张牌打出，能胡必须胡
    };

    //操作命令
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_OperateResult
    {
        public ushort wOperateUser;                      //操作用户
        public ushort wProvideUser;                      //供应用户
        public byte cbOperateCode;                     //操作代码
        public byte cbOperateCard;                     //操作扑克
    };

    //游戏结束
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_GameEnd
    {
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER)]
        //public byte[] cbCardCount;          //
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER * HNMJ_Defines.MAX_COUNT)]
        //public byte[] cbCardData;   // [GAME_PLAYER][MAX_COUNT]
        //                            //结束信息
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER, ArraySubType = UnmanagedType.U2)]
        //public ushort[] wProvideUser;         //供应用户
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER, ArraySubType = UnmanagedType.U4)]
        //public uint[] dwChiHuRight;            //胡牌类型
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER, ArraySubType = UnmanagedType.U4)]
        //public uint[] dwStartHuRight;          //起手胡牌类型
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER,ArraySubType = UnmanagedType.I8)]
        //public long[] lStartHuScore;            //起手胡牌分数

        ////积分信息
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER,ArraySubType = UnmanagedType.I8)]
        //public long[] lGameScore;           //游戏积分
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER, ArraySubType = UnmanagedType.I8)]
        //public long[] lTotalScore;           //游戏总分
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER,ArraySubType = UnmanagedType.I4)]
        //public int[] lGameTax;              //

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER,ArraySubType = UnmanagedType.U2)]
        //public ushort[] wWinOrder;                //胡牌排名
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER, ArraySubType = UnmanagedType.I8)]
        //public long[] lGangScoreInfo;           //详细得分
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER)]
        //public byte[] cbGenCount;           //
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER * HNMJ_Defines.GAME_PLAYER, ArraySubType = UnmanagedType.U2)]
        //public ushort[] wLostFanShu; //[GAME_PLAYER][GAME_PLAYER]
        //public ushort wLeftUser;                         //

        ////组合扑克
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER)]
        //public byte[] cbWeaveCount;                 //组合数目
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER * HNMJ_Defines.MAX_WEAVE)]
        //public CMD_WeaveItem[] WeaveItemArray;		//组合扑克 [GAME_PLAYER][MAX_WEAVE]
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.MAX_NIAO_CARD)]
        //public byte[] cbCardDataNiao; // 鸟牌
        //public byte cbNiaoCount;   //鸟牌个数
        //public byte cbNiaoPick;    //中鸟个数

        //WQ add
        public byte cbEndReason;

        ////WQ add：剩余库存扑克
        //public byte cbLeftCardCount;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.MAX_REPERTORY)]
        //public byte[] cbRepertoryLeftCard;
    };

    //用户托管
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_Trustee
    {
        public byte bTrustee;                          //是否托管
        public ushort wChairID;                          //托管用户
    };

    //
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_ChiHu
    {
        public ushort wChiHuUser;                            //
        public ushort wProviderUser;                     //
        public byte cbChiHuCard;                       //
        public byte cbCardCount;                       //
        public long lGameScore;                            //
        public byte cbWinOrder;                            //
    };

    //
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_GangScore
    {
        public ushort wChairId;                          //
        public byte cbXiaYu;                           //
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER,ArraySubType = UnmanagedType.I8)]
        public long[] lGangScore;           //
    };

    //补牌命令
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_ReplaceCard
    {
        public ushort wReplaceUser;                      //补牌用户
        public byte cbReplaceCard;                     //补牌扑克
    };

    //出牌命令
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_C_OutCard
    {
        public byte cbCardData;                            //扑克数据
    };

    // for HideSeek
    //客户端玩家事件信息
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlayerEventItem
    {
        public byte cbTeamType;
        public ushort wChairId;
        public byte cbAIId;

        public byte cbEventKind;

        public Int32 nCustomData0;
        public Int32 nCustomData1;
        public Int32 nCustomData2;

        public void StreamValue(byte[] kData, int offset, int dataSize)
        {
            cbTeamType = kData[offset];
            offset += sizeof(byte);

            wChairId = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            cbAIId = kData[offset];
            offset += sizeof(byte);

            cbEventKind = kData[offset];
            offset += sizeof(byte);

            nCustomData0 = System.BitConverter.ToInt32(kData, offset);
            offset += sizeof(Int32);

            nCustomData1 = System.BitConverter.ToInt32(kData, offset);
            offset += sizeof(Int32);

            nCustomData2 = System.BitConverter.ToInt32(kData, offset);
            offset += sizeof(Int32);
        }
    };
    //客户端玩家位置等信息
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlayerInfoItem
    {
        public byte cbTeamType;
        public ushort wChairId;
        public byte cbAIId;

        public Int32 posX;
        public Int32 posY;
        public Int32 posZ;

        public Int32 angleX;
        public Int32 angleY;
        public Int32 angleZ;

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_NICKNAME)]
        //public byte[] objNamePicked;

        //必须放在最后
        public byte cbHP;
        public byte cbIsValid;

        ////IsPicked:0x80, HasKilledPlayer:0x40, KilledPlayerChairID:0x3f
        //public byte cbIsPickedAndKilled;
        ////HasKilledPlayer:0x80, KilledPlayerIsAI:0x40, KilledPlayerTeamType:0x20, KilledPlayerChairID:0x1f, 
        //public byte cbKilledPlayer;
        //public byte cbKilledAIIdx;

        public void StreamValue(byte[] kData, int offset, int dataSize)
        {
            cbTeamType = kData[offset];// System.BitConverter.ToChar(kData, offset);
            offset += sizeof(byte);

            wChairId = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            cbAIId = kData[offset];// System.BitConverter.ToChar(kData, offset);
            offset += sizeof(byte);


            posX = System.BitConverter.ToInt32(kData, offset);
            offset += sizeof(Int32);

            posY = System.BitConverter.ToInt32(kData, offset);
            offset += sizeof(Int32);

            posZ = System.BitConverter.ToInt32(kData, offset);
            offset += sizeof(Int32);


            angleX = System.BitConverter.ToInt32(kData, offset);
            offset += sizeof(Int32);

            angleY = System.BitConverter.ToInt32(kData, offset);
            offset += sizeof(Int32);

            angleZ = System.BitConverter.ToInt32(kData, offset);
            offset += sizeof(Int32);

            //objNamePicked = new byte[SocketDefines.LEN_NICKNAME];
            //Buffer.BlockCopy(kData, offset, objNamePicked, 0, objNamePicked.Length);
            //offset += objNamePicked.Length;

            cbHP = kData[offset];
            offset++;

            cbIsValid = kData[offset];
            offset++;

            //cbIsPickedAndKilled = kData[offset];
            //offset++;

            //cbKilledPlayer = kData[offset];
            //offset++;

            //cbKilledAIIdx = kData[offset];
            //offset++;
        }
    };
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_C_HideSeek_ClientPlayersInfo
    {
        public ushort wAIItemCount;
        public ushort wEventItemCount;

        public PlayerInfoItem HumanInfoItem;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER)]
        public PlayerInfoItem[] AIInfoItems;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER)]
        public PlayerEventItem[] PlayerEventItems;

        //public void Init()
        //{
        //    PlayerInfoItems = new PlayerInfoItem[HNMJ_Defines.GAME_PLAYER];
        //}

        public byte[] ToBytes()
        {
            ushort dataSizeBeforeEvent = (ushort)(Marshal.SizeOf(wAIItemCount) + Marshal.SizeOf(wEventItemCount) + Marshal.SizeOf(HumanInfoItem) + wAIItemCount*Marshal.SizeOf(typeof(PlayerInfoItem)) );
            ushort dataSizeOfEventInfo = (ushort)(wEventItemCount * Marshal.SizeOf(typeof(PlayerEventItem)));
            ushort dataSize = (ushort)(dataSizeBeforeEvent + dataSizeOfEventInfo);

            byte[] resultBytes = new byte[dataSize];

            int offset = 0;

            var thisBuf = StructConverterByteArray.StructToBytes(this);
            Buffer.BlockCopy(thisBuf, 0, resultBytes, offset, dataSizeBeforeEvent);
            offset += dataSizeBeforeEvent;

            //var tmpBuf = BitConverter.GetBytes(wPlayerItemCount);
            //offset += sizeof(ushort);

            int eventOffsetOfThisBuf = (ushort)( Marshal.SizeOf(wAIItemCount) + Marshal.SizeOf(wEventItemCount) + Marshal.SizeOf(HumanInfoItem) + HNMJ_Defines.GAME_PLAYER * Marshal.SizeOf(typeof(PlayerInfoItem)) );
            Buffer.BlockCopy(thisBuf, eventOffsetOfThisBuf, resultBytes, offset, dataSizeOfEventInfo);
            offset += dataSizeOfEventInfo;

            return resultBytes;
        }
    }
    //从服务器收到的玩家信息
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_S_HideSeek_HeartBeat
    {
        public Int32 nPlayerItemCount;
        public Int32 nEventItemCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER)]
        public PlayerInfoItem[] PlayerInfoItems;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER)]
        public PlayerEventItem[] PlayerEventItems;

        public void StreamValue(byte[] kData, int dataSize)
        {
            int offset = 0;

            // Player Items

            nPlayerItemCount = System.BitConverter.ToInt32(kData, offset);
            offset += sizeof(Int32);

            if(nPlayerItemCount> HNMJ_Defines.GAME_PLAYER)
            {
                Debug.LogError("CMD_S_HideSeek_HeartBeat:StreamValue: incorrect nPlayerItemCount=" + nPlayerItemCount);
            }

            PlayerInfoItems = new PlayerInfoItem[nPlayerItemCount];
            int nItemSize = Marshal.SizeOf(typeof(PlayerInfoItem));
            for(int i=0; i< nPlayerItemCount; i++)
            {
                PlayerInfoItems[i].StreamValue(kData, offset, nItemSize);
                offset += nItemSize;
            }
            //while (offset <= dataSize - nItemSize)
            //{
            //    PlayerInfoItems[i].StreamValue(kData, offset, nItemSize);
            //    offset += nItemSize;
            //    i++;
            //}


            // Event Items

            nEventItemCount = System.BitConverter.ToInt32(kData, offset);
            offset += sizeof(Int32);

            if (nEventItemCount > HNMJ_Defines.GAME_PLAYER)
            {
                Debug.LogError("CMD_S_HideSeek_HeartBeat:StreamValue: incorrect nEventItemCount=" + nEventItemCount);
            }

            PlayerEventItems = new PlayerEventItem[nEventItemCount];
            nItemSize = Marshal.SizeOf(typeof(PlayerEventItem));
            for (int i = 0; i < nEventItemCount; i++)
            {
                PlayerEventItems[i].StreamValue(kData, offset, nItemSize);
                offset += nItemSize;
            }
            //while (offset <= dataSize - nItemSize)
            //{
            //    PlayerEventItems[i].StreamValue(kData, offset, nItemSize);
            //    offset += nItemSize;
            //    i++;
            //}
        }
    };

    // for HideSeek
    //AI分配信息
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AICreateInfoItem
    {
        public byte cbTeamType;
        public ushort wChairId;
        public byte cbModelIdx;

        public byte cbAIId;
    };
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_GF_S_AICreateInfoItems
    {
        public ushort wItemCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.GAME_PLAYER)]
        public AICreateInfoItem[] AICreateInfoItems;

        public void StreamValue(byte[] kData, int dataSize)
        {
            int offset = 0;

            wItemCount = System.BitConverter.ToUInt16(kData, offset);
            offset += sizeof(ushort);

            AICreateInfoItems = new AICreateInfoItem[wItemCount];
            int i = 0;
            int nItemSize = Marshal.SizeOf(typeof(AICreateInfoItem));
            while (offset <= dataSize - nItemSize)
            {
                byte[] tmpItem = new byte[nItemSize];
                Buffer.BlockCopy(kData, offset, tmpItem, 0, nItemSize);
                if(i>= wItemCount)
                {
                    Debug.LogError("incorrect i="+i+ ", wItemCount="+ wItemCount);
                }
                AICreateInfoItems[i] = (AICreateInfoItem)StructConverterByteArray.BytesToStruct(tmpItem, typeof(AICreateInfoItem));
                offset += nItemSize;
                i++;
            }
        }
    };

    //操作命令
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_C_OperateCard
    {
        public byte cbOperateCode;                     //操作代码
        public byte cbOperateCard;                     //操作扑克
    };

    //用户托管
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_C_Trustee
    {
        public byte bTrustee;                          //是否托管	
    };

    //起手小胡
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CMD_C_XiaoHu
    {
        public byte cbOperateCode;                     //操作代码
        public byte cbOperateCard;                     //操作扑克
    };


    /*----------------------HNMJLogic.h--------------------------*/
    //////////////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////////////
    //逻辑掩码
    public class HNMJLogic_Defines
    {

        public const int MASK_COLOR = 0xF0;							//花色掩码
        public const int MASK_VALUE = 0x0F;							//数值掩码

        //////////////////////////////////////////////////////////////////////////
        //动作定义

        //动作标志
        public const int WIK_NULL = 0x00;							//没有类型
        public const int WIK_LEFT = 0x01;							//左吃类型
        public const int WIK_CENTER = 0x02;							//中吃类型
        public const int WIK_RIGHT = 0x04;						//右吃类型
        public const int WIK_PENG = 0x08;								//碰牌类型
        public const int WIK_GANG = 0x10;							//杠牌类型
        public const int WIK_XIAO_HU = 0x20;								//小胡
        public const int WIK_CHI_HU = 0x40;							//吃胡类型
        public const int WIK_ZI_MO = 0x80;								//自摸

        public const int WIK_GANG_YAOSHAIZI = 0x20;							//开杠摇骰子 WQ add

        //////////////////////////////////////////////////////////////////////////
        //胡牌定义

        //胡牌
        public const int CHK_NULL = 0x00;									//非胡类型
        public const int CHK_CHI_HU = 0x01;                                 //胡类型

        public const int CHR_BA_DUI = 0x00000001;									//八对X2
        public const int CHR_BAO_TOU = 0x00000002;									//暴头X2
        public const int CHR_PIAO_CAI_YI = 0x00000004;									//飘财X4
        public const int CHR_PIAO_CAI_ER = 0x00000008;									//二次飘财X8
        public const int CHR_PIAO_CAI_SAN = 0x00000010;									//三次飘财X16
        public const int CHR_GANG_KAI = 0x00000020;									//杠开X2
        public const int CHR_GANG_BAO = 0x00000040;									//杠暴
        public const int CHR_PIAO_GANG = 0x00000080;								//飘杠
        public const int CHR_GANG_PIAO = 0x00000100;									//杠飘
        public const int CHR_SHI_SAN_BU_DA = 0x00000200;									//十三不搭X4
        public const int CHR_QING_YI_SE = 0x00000400;									//清一色X10
        public const int CHR_QING_FENG_ZI = 0x00000800;                                  //清风子X20
        public const int CHR_QIANG_GANG_HU = 0x00001000;									//抢杠胡X6
        public const int CHR_SHI_SAN_BU_DA_QIANG_GANG = 0x00002000;									//十三不搭抢杠
        public const int CHR_QIANG_PIAO_GANG = 0x00004000;									//抢飘杠
        public const int CHR_BA_DUI_ZI_PIAO_CAI = 0x00008000;									//八对子飘财
        public const int CHR_QING_YI_SE_PIAO_CAI = 0x00010000;									//清一色飘财
        public const int CHR_QING_BA_DUI_PIAO_CAI = 0x00020000;                                 //清八对飘财
        public const int CHR_QING_YI_SE_QIANGGANG = 0x00040000;                                 //清一色抢杠

        public const int CHR_ZI_MO = 0x01000000;							//自摸
        public const int CHR_SHU_FAN = 0x02000000;							//素翻

        //扑克定义
        public const int HEAP_FULL_COUNT = 26;						//堆立全牌

        public const int MAX_RIGHT_COUNT = 1;
        //////////////////////////////////////////////////////////////////////////
    }

    
       
        /*
            //类型子项
            struct tagKindItem
            {
                byte cbWeaveKind;                       //组合类型
                byte cbCenterCard;                      //中心扑克
                byte cbCardIndex[3];                        //扑克索引
                byte cbValidIndex[3];                   //实际扑克索引
            };
            */
            //组合子项
    public struct tagWeaveItem
    {
        byte cbWeaveKind; //组合类型
        byte cbCenterCard; //中心扑克
        byte cbPublicCard; //公开标志
        ushort wProvideUser; //供应用户
    };

    //杠牌结果
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct tagGangCardResult
    {
        public byte cbCardCount; //扑克数目
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HNMJ_Defines.MAX_WEAVE)]
        public byte[] cbCardData; // = new byte[4];                     //扑克数据
    };

            //分析子项
    public struct tagAnalyseItem
    {
        byte cbCardEye; //牌眼扑克
        bool bMagicEye; //牌眼是否是王霸
        private byte[] cbWeaveKind; // = new byte[4];                        //组合类型
        private byte[] cbCenterCard; // = new byte[4];                   //中心扑克
        private byte[] cbCardData; // = new byte[16];//[4][4];                   //实际扑克
    };

            //////////////////////////////////////////////////////////////////////////

            //
            //	权位类。
            //  注意，在操作仅位时最好只操作单个权位.例如
            //  CChiHuRight chr;
            //  chr |= (chr_zi_mo|chr_peng_peng)，这样结果是无定义的。
            //  只能单个操作:
            //  chr |= chr_zi_mo;
            //  chr |= chr_peng_peng;
            //
    class CChiHuRight
    {
        //静态变量
        private static bool m_bInit = false;
        private static uint[] m_dwRightMask = new uint[HNMJLogic_Defines.MAX_RIGHT_COUNT];

        //权位变量
        private uint[] m_dwRight = new uint[HNMJLogic_Defines.MAX_RIGHT_COUNT];


        //构造函数
        public CChiHuRight()
        {
            if (!m_bInit)
            {
                m_bInit = true;
                for (byte i = 0; i < m_dwRightMask.Length; i++)
                {
                    if (0 == i)
                        m_dwRightMask[i] = 0;
                    else
                        m_dwRightMask[i] = ((uint) ((Math.Pow(2, (i - 1))))) << 28;
                }
            }
        }

        public void CloneFrom(CChiHuRight other)
        {
            Array.Copy(other.m_dwRight, 0, m_dwRight, 0, other.m_dwRight.Length);
        }

        //运算符重载
        //赋值符 //c#不允许重载赋值符，改为构造
        public CChiHuRight(uint dwRight)
        {
            uint dwOtherRight = 0;
            //验证权位
            if (!IsValidRight(dwRight))
            {
                //验证取反权位
                //ASSERT(IsValidRight(~dwRight));
                if (!IsValidRight(~dwRight)) return;
                dwRight = ~dwRight;
                dwOtherRight = HNMJ_Defines.MASK_CHI_HU_RIGHT;
            }

            for (byte i = 0; i < (m_dwRightMask.Length); i++)
            {
                if ((dwRight & m_dwRightMask[i]) != 0 || (i == 0 && dwRight < 0x10000000))
                    m_dwRight[i] = dwRight & HNMJ_Defines.MASK_CHI_HU_RIGHT;
                else m_dwRight[i] = dwOtherRight;
            }
        }

        //与
        public static CChiHuRight operator &(CChiHuRight chr, uint dwRight)
        {
            CChiHuRight ls = new CChiHuRight();
            ls.CloneFrom(chr);

            bool bNavigate = false;
            //验证权位
            if (!ls.IsValidRight(dwRight))
            {
                //验证取反权位
                //ASSERT(IsValidRight(~dwRight));
                if (!ls.IsValidRight(~dwRight)) return ls;
                //调整权位
                uint dwHeadRight = (~dwRight) & 0xF0000000;
                uint dwTailRight = dwRight & HNMJ_Defines.MASK_CHI_HU_RIGHT;
                dwRight = dwHeadRight | dwTailRight;
                bNavigate = true;
            }

            for (byte i = 0; i < (m_dwRightMask.Length); i++)
            {
                if ((dwRight & m_dwRightMask[i]) != 0 || (i == 0 && dwRight < 0x10000000))
                {
                    ls.m_dwRight[i] &= (dwRight & HNMJ_Defines.MASK_CHI_HU_RIGHT);
                }
                else if (!bNavigate)
                    ls.m_dwRight[i] = 0;
            }

            return ls;
        }

        //或
        public static CChiHuRight operator |(CChiHuRight chr, uint dwRight)
        {
            CChiHuRight ls = new CChiHuRight();
            ls.CloneFrom(chr);

            //验证权位
            if (!ls.IsValidRight(dwRight)) return ls;

            for (byte i = 0; i < (m_dwRightMask.Length); i++)
            {
                if ((dwRight & m_dwRightMask[i]) != 0 || (i == 0 && dwRight < 0x10000000))
                    ls.m_dwRight[i] |= (dwRight & HNMJ_Defines.MASK_CHI_HU_RIGHT);
            }

            return ls;
        }

        //功能函数

        //是否权位为空
        public bool IsEmpty()
        {
            for (byte i = 0; i < (m_dwRight.Length); i++)
                if (m_dwRight[i] != 0) return false;
            return true;
        }

        //设置权位为空
        public void SetEmpty()
        {
            Array.Clear(m_dwRight, 0, m_dwRight.Length);
        }

        //获取权位数值
        public byte GetRightData(ref uint[] dwRight, byte cbMaxCount)
        {
            //ASSERT(cbMaxCount >= CountArray(m_dwRight));
            if (cbMaxCount < (m_dwRight.Length)) return 0;
            Array.Copy(m_dwRight, 0, dwRight, 0, m_dwRight.Length);
            // memcpy(dwRight, m_dwRight, sizeof(dword) * CountArray(m_dwRight));
            return (byte) (m_dwRight.Length);
        }

        //设置权位数值
        public bool SetRightData(uint[] dwRight, byte cbRightCount)
        {
            if (cbRightCount > (m_dwRight.Length)) return false;

            //zeromemory(m_dwRight, sizeof (m_dwRight));
            Array.Clear(m_dwRight, 0, m_dwRight.Length);
            //memcpy(m_dwRight, dwRight, sizeof (dword)*cbRightCount);
            Array.Copy(dwRight, 0, m_dwRight, 0, cbRightCount);
            return true;
        }


        //检查权位是否正确
        private bool IsValidRight(uint dwRight)
        {
            uint dwRightHead = dwRight & 0xF0000000;
            for (byte i = 0; i < (m_dwRightMask.Length); i++)
                if (m_dwRightMask[i] == dwRightHead) return true;
            return false;
        }
    };


            //////////////////////////////////////////////////////////////////////////

            //游戏逻辑类
    class CGameLogic
    {
        private static CGameLogic ms_pkInstance;


        public static CGameLogic Instance()
        {
            if (ms_pkInstance == null)
            {
                ms_pkInstance = new CGameLogic();
            }
            return ms_pkInstance;
        }

        //变量定义
        protected static byte[] m_cbCardDataArray = new byte[HNMJ_Defines.MAX_REPERTORY]; //扑克数据
        protected byte m_cbMagicIndex; //钻牌索引

        //函数定义
        //构造函数
        private CGameLogic()
        {
            m_cbMagicIndex = HNMJ_Defines.MAX_INDEX;
        }


        //控制函数
#if false
    //混乱扑克
        public void RandCardData(byte cbCardData[], byte cbMaxCount);
        //删除扑克

        public bool RemoveCard(byte cbCardIndex[MAX_INDEX], byte cbRemoveCard);
        //删除扑克

        public bool RemoveCard(byte cbCardIndex[MAX_INDEX], const byte cbRemoveCard[], byte cbRemoveCount);
        //删除扑克
#endif
        public bool RemoveValueCard(ref byte[] cbCardData ,byte cbCardCount, byte[] cbRemoveCard,byte cbRemoveCount)
        {
            //检验数据
            //ASSERT(cbCardCount <= MAX_COUNT);
            //ASSERT(cbRemoveCount <= cbCardCount);

            //定义变量
            byte cbDeleteCount = 0;
            byte[] cbTempCardData = new byte[HNMJ_Defines.MAX_COUNT];
            if (cbCardCount > (cbTempCardData.Length))
                return false;
            Buffer.BlockCopy(cbCardData, 0, cbTempCardData, 0, cbCardCount);
            //memcpy(cbTempCardData, cbCardData, cbCardCount * sizeof(cbCardData[0]));

            //置零扑克
            for (byte i = 0; i < cbRemoveCount; i++)
            {
                for (byte j = 0; j < cbCardCount; j++)
                {
                    if (cbRemoveCard[i] == cbTempCardData[j])
                    {
                        cbDeleteCount++;
                        cbTempCardData[j] = 0;
                        break;
                    }
                }
            }

            //成功判断
            if (cbDeleteCount != cbRemoveCount)
            {
                Debug.Assert(false, "RemoveValueCard assert failed because of cbDeleteCount != cbRemoveCount" + cbDeleteCount + " remove count " + cbRemoveCount);
                return false;
            }

            //清理扑克
            byte cbCardPos = 0;
            for (byte i = 0; i < cbCardCount; i++)
            {
                if (cbTempCardData[i] != 0)
                    cbCardData[cbCardPos++] = cbTempCardData[i];
            }
            while (cbCardPos < HNMJ_Defines.MAX_COUNT)
            {
                cbCardData[cbCardPos++] = 0;
            }

            return true;

        }
        //删除扑克

        public byte RemoveValueCardAll(ref byte[] cbCardData,byte cbCardCount,byte cbRemoveCard )
        {
            byte[] cbCardIndex = new byte[HNMJ_Defines.MAX_INDEX];            //手中扑克
            SwitchToCardIndex(cbCardData, cbCardCount,ref cbCardIndex);
            byte []cbRemoveCardArray= new byte[HNMJ_Defines.MAX_INDEX];
            byte cbRemoveCout = cbCardIndex[SwitchToCardIndex(cbRemoveCard)];
            for (int i = 0; i < cbRemoveCout; i++)
            {
                cbRemoveCardArray[i] = cbRemoveCard;
            }
            RemoveValueCard(ref cbCardData, cbCardCount, cbRemoveCardArray, cbRemoveCout);
            return (byte)(cbCardCount - cbRemoveCout);
        }

        //删除扑克
        public bool RemoveValueCardOne(ref byte[] cbCardData, byte cbCardCount, byte cbRemoveCard)
        {
            byte[] cbRemoveCardArray = new byte[HNMJ_Defines.MAX_INDEX];
            cbRemoveCardArray[0] = cbRemoveCard;
            return RemoveValueCard(ref cbCardData, cbCardCount, cbRemoveCardArray, 1);
        }

        //设置钻牌
#if false
    public void SetMagicIndex(byte cbMagicIndex) { m_cbMagicIndex = cbMagicIndex; }
        //钻牌

        public bool IsMagicCard(byte cbCardData);

            //辅助函数
            public:
            //有效判断
       
            public bool IsValidCard(byte cbCardData);
        //扑克数目

        public byte GetCardCount(const byte cbCardIndex[MAX_INDEX]);
        //组合扑克

        public byte GetWeaveCard(byte cbWeaveKind, byte cbCenterCard, byte cbCardBuffer[4]);

            //等级函数
            public:
            //动作等级
      
            public byte GetUserActionRank(byte cbUserAction);
        //胡牌等级

        public WORD GetChiHuActionRank(const CChiHuRight & ChiHuRight);

            //动作判断
            public:
            //吃牌判断
       
            public byte EstimateEatCard(const byte cbCardIndex[MAX_INDEX], byte cbCurrentCard);
        //碰牌判断

        public byte EstimatePengCard(const byte cbCardIndex[MAX_INDEX], byte cbCurrentCard);
        //杠牌判断

        public byte EstimateGangCard(const byte cbCardIndex[MAX_INDEX], byte cbCurrentCard);

            //动作判断
            public:
            //杠牌分析
       
            public byte AnalyseGangCard(const byte cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], byte cbWeaveCount, tagGangCardResult & GangCardResult);
            //吃胡分析
       
            public byte AnalyseChiHuCard(const byte cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], byte cbWeaveCount, byte cbCurrentCard, CChiHuRight &ChiHuRight);
            //听牌分析
        
            public byte AnalyseTingCard( const byte cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], byte cbWeaveCount);
        //是否听牌

        public bool IsTingCard( const byte cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], byte cbWeaveCount);
        //是否花猪

        public bool IsHuaZhu( const byte cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], byte cbWeaveCount);

        //杠牌分析

        public byte AnalyseGangCard(const byte cbCardIndex[MAX_INDEX], const CMD_WeaveItem WeaveItem[], byte cbWeaveCount, tagGangCardResult & GangCardResult);
    
#endif
            //转换函数
            //扑克转换
        
            //public byte SwitchToCardData(byte cbCardIndex);
        //扑克转换

        public byte SwitchToCardIndex(byte cbCardData)
        {
            return (byte)(((cbCardData & HNMJLogic_Defines.MASK_COLOR) >> 4) * 9 + (cbCardData & HNMJLogic_Defines.MASK_VALUE) - 1);
        }
        //扑克转换

        //public byte SwitchToCardData(const byte cbCardIndex[MAX_INDEX], byte cbCardData[MAX_COUNT]);
        //扑克转换

        public byte SwitchToCardIndex(byte[] cbCardData, byte cbCardCount,ref byte[] cbCardIndex)
        {
            //设置变量
            //zeromemory(cbCardIndex, sizeof(BYTE) * MAX_INDEX);

            //转换扑克
            for (byte i = 0; i < cbCardCount; i++)
            {
                //ASSERT(IsValidCard(cbCardData[i]));
                cbCardIndex[SwitchToCardIndex(cbCardData[i])]++;
            }

            return cbCardCount;
        }

        //排序,根据牌值排序

        public bool SortCardList(ref byte[] cbCardData, byte cbCardCount)
        {
            //数目过虑
            if (cbCardCount == 0 || cbCardCount > HNMJ_Defines.MAX_COUNT) return false;

            //找出白板個數
            int baiNum = 0;
            for (int i = 0; i < cbCardCount; i++)
            {
                if (cbCardData[i] == 55)
                {
                    baiNum++;
                    cbCardData[i] = 0;
                }
            }
            //排序操作
            bool bSorted = true;
            byte cbSwitchData = 0, cbLast = (byte)(cbCardCount - 1);
            do
            {
                bSorted = true;
                for (byte i = 0; i < cbLast; i++)
                {
                    if (cbCardData[i] == 55)
                    {
                        continue;
                    }
                    else if (cbCardData[i] > cbCardData[i + 1])
                    {
                        //设置标志
                        bSorted = false;

                        //扑克数据
                        cbSwitchData = cbCardData[i];
                        cbCardData[i] = cbCardData[i + 1];
                        cbCardData[i + 1] = cbSwitchData;
                    }
                }
                cbLast--;
            } while (bSorted == false);

            for (int i = 0; i < baiNum; i++)
            {
                cbCardData[i] = 55;
            }
            Debug.Log( "Card Sorted now " + cbCardCount);
            return true;
        }

#if false
    //胡法分析

    //大对子
    protected bool IsPengPeng( const tagAnalyseItem* pAnalyseItem);
        //清一色牌
        protected bool IsQingYiSe(const byte cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], const byte cbItemCount,const byte cbCurrentCard);
        //七小对牌
        protected bool IsQiXiaoDui(const byte cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[],const  byte cbWeaveCount,const byte cbCurrentCard);
        //带幺
        protected bool IsDaiYao( const tagAnalyseItem* pAnalyseItem);
        //将对
        protected bool IsJiangDui( const tagAnalyseItem* pAnalyseItem);

         public byte GetCardColor(byte cbCardDat);
        public byte GetCardValue(byte cbCardDat);
#endif
    };
        /*----------------------HNMJLogic.h--------------------------*/
    //游戏逻辑
    class HNMJPlayer : GamePlayer
    {
        private HNGameManager hnManager;

        public int getWeaveCount()
        {
            return m_kWeaveCount;
        }
        public HNMJPlayer(int iIdex, HNGameManager ma) : base(null)
        {
            m_iIdex = (iIdex);
            //m_pSeatNode = (pSeatNode);
            m_kHandCardCout = (0);
            m_kWeaveCount = (0);
            //m_pHandOutCard = (null);
            m_bWasKaiGangYaoShaiZi = (false); //mChen
            hnManager = ma;
        }

        public void init()
        {
            PlayerLeave();
        }

        public int getIdex()
        {
            return m_iIdex;
        }

        public void defaultState()
        {
            cbChiCard = 0;
            m_bCanDirectSendCard = false;
            m_kHandCardCout = 0;
            Array.Clear(m_kHandCardData, 0, m_kHandCardData.Length);
            Array.Clear(m_kWeaveItemArray, 0, m_kWeaveItemArray.Length);
            m_kWeaveCount = 0;
            //m_pHandOutCard = null;
            m_iNewWaveIndex = -1;
            setActOutCard(-1);
            m_kOutCardList.Clear();
            setChiHuCard(0);

            //selfUICommandList.Clear();
            //WQ add,补花
            m_cbHuaCardCount = 0;
            Array.Clear(m_cbHuaCard, 0, m_cbHuaCard.Length);
            ///memset(m_cbHuaCard, 0, sizeof(m_cbHuaCard));

#if false // UI stuff
            WidgetFun::setVisible(m_pSeatNode, "Zhuang", false);
            WidgetFun::setVisible(m_pSeatNode, "BigOutNode", false);
            WidgetFun::setVisible(m_pSeatNode, "ReadyState", false);
            WidgetFun::getChildWidget(m_pSeatNode, "CardNode1").removeAllChildren();
            WidgetFun::getChildWidget(m_pSeatNode, "CardNode2").removeAllChildren();
            WidgetFun::setVisible(m_pSeatNode, "AnimateNode", false);
#endif
            upPlayerState();
        }

        public void startGame()
        {
        }

        public void EndGame()
        {
            if (m_pUserItem == null)
            {
                return;
            }
        }

        public void setZhuang()
        {
            Loom.QueueOnMainThread(() =>
            {
                hnManager.SetZhuang(GetChairID());
            });
#if false // UI stuff
            WidgetFun::setVisible(m_pSeatNode, "Zhuang", true);
#endif
        }

        public void showAddGold(int iGold)
        {
#if false // UI stuff
            WidgetFun::setText(m_pSeatNode, "AddGoldTxt", iGold);
            WidgetFun::runWidgetAction(m_pSeatNode, "AddGoldTxt", "Start");
#endif
        }

        public void showEffect(string kKey)
        {
#if false // UI stuff

            cocos2d::Node* pNode = WidgetFun::getChildWidget(m_pSeatNode, "EffectImagic");
            WidgetFun::setPlaceImagicKey(pNode, kKey);
            pNode.stopAllActions();
            WidgetFun::runWidgetAction(pNode, "Start");
#endif
        }

        public void showStatusImagic(string kKey)
        {
#if false // UI stuff
            WidgetFun::setPlaceImagicKey(m_pSeatNode, "ReadyState", kKey);
            WidgetFun::setVisible(m_pSeatNode, "ReadyState", true);
#endif
        }

        public override void PlayerEnter()
        {
#if false // UI stuff
            WidgetFun::setVisible(m_pSeatNode, "Name", true);
            WidgetFun::setVisible(m_pSeatNode, "GoldImagic", true);
            WidgetFun::setVisible(m_pSeatNode, "HeadImagicEmpty", false);
            WidgetFun::setVisible(m_pSeatNode, "HeadImagic", true);

            if (WidgetFun::getChildWidget(m_pSeatNode, "ButtonPlayerHeadClick"))
            {
                WidgetFun::setWidgetUserInfo(m_pSeatNode, "ButtonPlayerHeadClick", "UserID", utility::toString((int)GetUserID()));
            }
#else
#endif
        }

        public override void PlayerLeave()
        {
            defaultState();
#if false // UI stuff
            WidgetFun::setVisible(m_pSeatNode, "HeadImagicEmpty", true);
            WidgetFun::setVisible(m_pSeatNode, "HeadImagic", false);
            WidgetFun::setVisible(m_pSeatNode, "Name", false);
            WidgetFun::setVisible(m_pSeatNode, "GoldImagic", false);
            WidgetFun::setText(m_pSeatNode, "Name", "");
            WidgetFun::setText(m_pSeatNode, "GoldTxt", "");
#endif
        }

        public override void upPlayerInfo()
        {
#if false // UI stuff
            WidgetFun::setText(m_pSeatNode, "Name", GetNickName());
            WidgetFun::setText(m_pSeatNode, "GoldTxt", (int)GetUserScore());
            if (GetHeadHttp() != "")
            {
                ImagicDownManager::Instance().addDown(WidgetFun::getChildWidget(m_pSeatNode, "HeadImagic"),
                    GetHeadHttp(), GetUserID());
            }
#else
            Loom.QueueOnMainThread(() =>
            {
                hnManager.UpdateUserInfo(m_iIdex, GetNickName(), String.Format("{0}", GetUserID()), GetGender(), GetHeadHttp());
            });
#endif
        }

        public override void upPlayerState()
        {
            if (GetUserStatus() == SocketDefines.US_READY)
            {
                showStatusImagic("Ready");
            }
#if false // UI stuff
            WidgetFun::setVisible(m_pSeatNode, "Offline", GetUserStatus() == SocketDefines.US_OFFLINE);
#endif
        }
       
        public void setHandCard(byte[] pCardData,int offset, int iCout)
        {
            //memcpy(m_kHandCardData, pCardData, sizeof(m_kHandCardData));
            Buffer.BlockCopy(pCardData, offset, m_kHandCardData, 0, m_kHandCardData.Length);
            //m_kHandCardData = pCardData;
            m_kHandCardCout = (byte) iCout;
        }

        public void setWeaveItem(CMD_WeaveItem[] pWeave, int iCout)
        {
            if (iCout > 0)
            {
                Loom.QueueOnMainThread(() =>
                {
                    hnManager.SetWeaveCards(pWeave, GetChairID());
                });
            }
            m_kWeaveItemArray = pWeave;
            //memcpy(m_kWeaveItemArray, pWeave, sizeof(m_kWeaveItemArray));
            m_kWeaveCount = (byte) iCout;
        }

        public int getHuaData(ref byte[] cards)
        {
            cards = m_cbHuaCard;
            return m_cbHuaCardCount;
        }

        //WQ add,补花
        public void setHuaCard(byte[] pHuaCardData, int iCout)
        {
            m_cbHuaCard = pHuaCardData;
            ///memcpy(m_cbHuaCard, pHuaCardData, sizeof(m_cbHuaCard));
            m_cbHuaCardCount = (byte)iCout;
            Loom.QueueOnMainThread(() =>
            {
                for (int i = 0; i < iCout; i++)
                {
                    hnManager.SetHuaCardObj(GetChairID(), pHuaCardData[i], i);
                }
            });
        }

        public int ReplaceLogic(byte cbReplaceCard)
        {
            m_cbHuaCardCount++;
            m_cbHuaCard[m_cbHuaCardCount - 1] = cbReplaceCard;

            //if (m_iIdex == 0)
            if(true)
            {
                CGameLogic.Instance().RemoveValueCardOne(ref m_kHandCardData, m_kHandCardCout, cbReplaceCard);
                ///HNMJ::CGameLogic::Instance().RemoveValueCardOne(m_kHandCardData, m_kHandCardCout, cbReplaceCard);
                m_kHandCardCout--;
                CGameLogic.Instance().SortCardList(ref m_kHandCardData, m_kHandCardCout);
            }
            else
            {
                m_kHandCardCout--;
            }

            ///HNMJ::CGameLogic::Instance().SortCardList(m_kHandCardData, m_kHandCardCout);
            return m_cbHuaCardCount;
        }

        public void setReplaceCard(CMD_S_ReplaceCard pNetInfo)
        {
            ushort wReplaceUser = pNetInfo.wReplaceUser;
            byte cbReplaceCard = pNetInfo.cbReplaceCard;

            selfUICommandList.Add(new PlayerUICommand(PlayerAnimType.BuHua, new[] { cbReplaceCard }));
        }

        public byte cbChiCard = 0;

        public void setOperateResoult(CMD_S_OperateResult pNetInfo)
        {
            Debug.Log("--------------------------------setOperateResoult: selfUICommandList.Add");

            selfUICommandList.Add(new PlayerUICommand(PlayerAnimType.SetOperateResoult, new[] { (byte)pNetInfo.wOperateUser, (byte)pNetInfo.wProvideUser, pNetInfo.cbOperateCode, pNetInfo.cbOperateCard }));
            ///handleOperateResoult(pNetInfo);

            return;
        }

        public void handleOperateResoult(CMD_S_OperateResult pNetInfo)
        {
            Debug.Log("--------------------------------handleOperateResoult");

            bool cbPublicCard = false;
            ushort wOperateUser = pNetInfo.wOperateUser;
            byte cbOperateCard = pNetInfo.cbOperateCard;

            if (pNetInfo.cbOperateCode == HNMJLogic_Defines.WIK_PENG)
            {
                //HNMJGameScence::Instance().hideFocus();
                runAniPeng();
            }
            if (pNetInfo.cbOperateCode == HNMJLogic_Defines.WIK_LEFT ||
                pNetInfo.cbOperateCode == HNMJLogic_Defines.WIK_CENTER ||
                pNetInfo.cbOperateCode == HNMJLogic_Defines.WIK_RIGHT)
            {
                //HNMJGameScence::Instance().hideFocus();
                runAniChi();
                if (m_iIdex == 0)
                {
                    cbChiCard = cbOperateCard;//本人吃牌时记录被吃的牌，出牌不可以出同一张牌，客户端来判断
                }
            }
            else if (pNetInfo.cbOperateCode == HNMJLogic_Defines.WIK_GANG && pNetInfo.wProvideUser == wOperateUser)
            {
                runAniAnGang();
            }
            else if (pNetInfo.cbOperateCode == HNMJLogic_Defines.WIK_GANG && pNetInfo.wProvideUser != wOperateUser)
            {
                //HNMJGameScence::Instance().hideFocus();
                runAniMingGang();
            }

            if (pNetInfo.cbOperateCode != HNMJLogic_Defines.WIK_NULL)
            {
                hnManager.ResetMovingCard();
            }

            if (pNetInfo.cbOperateCode != HNMJLogic_Defines.WIK_NULL /*&& m_iIdex == 0*/)
            {
                byte[] cards = new byte[m_kHandCardCout+1];
                Debug.Log("--------------------------------------------------------------setOperateResoult:ResetLocalHandCardStuff1 cards=" + cards.Length + ", m_kHandCardCout=" + m_kHandCardCout);
                Buffer.BlockCopy(m_kHandCardData, 0, cards, 0, m_kHandCardCout);
                cards[m_kHandCardCout] = (byte)GetChairID();
                //Loom.QueueOnMainThread(() =>
                //{
                    hnManager.ResetLocalHandCardStuff(cards);
                    Debug.Log("--------------------------------------------------------------setOperateResoult:ResetLocalHandCardStuff2 cards=" + cards.Length + ", m_kHandCardCout="+ m_kHandCardCout);
                //});
            }

            if ((pNetInfo.cbOperateCode & HNMJLogic_Defines.WIK_GANG) != 0)
            {
                //组合扑克
                byte cbWeaveIndex = 0xFF;
                for (byte i = 0; i < m_kWeaveCount; i++)
                {
                    byte cbWeaveKind = m_kWeaveItemArray[i].cbWeaveKind;
                    byte cbCenterCard = m_kWeaveItemArray[i].cbCenterCard;
                    if ((cbCenterCard == pNetInfo.cbOperateCard) && (cbWeaveKind == HNMJLogic_Defines.WIK_PENG))
                    {
                        ///bNewWaveIn = true;//WQ add,fix补杠点杠后Weave牌组UI没更新

                        cbWeaveIndex = i;
                        m_iNewWaveIndex = cbWeaveIndex;
                        m_kWeaveItemArray[cbWeaveIndex].cbPublicCard = 1;
                        m_kWeaveItemArray[cbWeaveIndex].cbWeaveKind = pNetInfo.cbOperateCode;
                        m_kWeaveItemArray[cbWeaveIndex].wProvideUser = pNetInfo.wProvideUser;
                        break;
                    }
                }

                //组合扑克
                if (cbWeaveIndex == 0xFF)
                {
                    //暗杠判断
                    cbPublicCard = (pNetInfo.wProvideUser != wOperateUser);

                    //设置扑克
                    cbWeaveIndex = m_kWeaveCount++;
                    m_iNewWaveIndex = cbWeaveIndex;
                    m_kWeaveItemArray[cbWeaveIndex].cbPublicCard = (byte) (cbPublicCard ? 1 : 0);
                    m_kWeaveItemArray[cbWeaveIndex].cbCenterCard = cbOperateCard;
                    m_kWeaveItemArray[cbWeaveIndex].cbWeaveKind = pNetInfo.cbOperateCode;
                    m_kWeaveItemArray[cbWeaveIndex].wProvideUser = pNetInfo.wProvideUser;
                }

                //扑克设置
                //if (m_iIdex == 0)
                if(true)
                {
                    //自己补杠时判断若摸的牌不是补杠的那张牌，那么需要对牌进行排序，因为补杠后会摸一张牌，若不先排序，牌就不对了
                    bool bNeedSort = pNetInfo.cbOperateCard != m_kHandCardData[m_kHandCardCout - 1];
                    CGameLogic.Instance().RemoveValueCardAll(ref m_kHandCardData, m_kHandCardCout, pNetInfo.cbOperateCard);
                    if (bNeedSort)
                    {
                        CGameLogic.Instance()
                            .SortCardList(ref m_kHandCardData, (byte)(HNMJ_Defines.MAX_COUNT - m_kWeaveCount*3 - 1));
                    }
                    Debug.Log("-------------------------------------------------------------setOperateResoult:m_kHandCardCout" + (HNMJ_Defines.MAX_COUNT - m_kWeaveCount * 3 - 1));
                }
                m_kHandCardCout = (byte) (HNMJ_Defines.MAX_COUNT - m_kWeaveCount*3 - 1);
                
            }
            else if (pNetInfo.cbOperateCode != HNMJLogic_Defines.WIK_NULL)
            {
                //设置组合
                byte cbWeaveIndex = m_kWeaveCount++;
                m_iNewWaveIndex = cbWeaveIndex;
                m_kWeaveItemArray[cbWeaveIndex].cbPublicCard = 1;
                m_kWeaveItemArray[cbWeaveIndex].cbCenterCard = cbOperateCard;
                m_kWeaveItemArray[cbWeaveIndex].cbWeaveKind = pNetInfo.cbOperateCode;
                m_kWeaveItemArray[cbWeaveIndex].wProvideUser = pNetInfo.wProvideUser;

                //组合界面
                //删除扑克
                //if (m_iIdex == 0)
                if (true)
                {
                    byte[] cbWeaveCard = new byte[] {cbOperateCard, cbOperateCard, cbOperateCard, cbOperateCard};
                    if (pNetInfo.cbOperateCode == HNMJLogic_Defines.WIK_LEFT)
                    {
                        cbWeaveCard[0] = (byte) (cbOperateCard + 1);
                        cbWeaveCard[1] = (byte) (cbOperateCard + 2);
                    }
                    if (pNetInfo.cbOperateCode == HNMJLogic_Defines.WIK_CENTER)
                    {
                        cbWeaveCard[0] = (byte) (cbOperateCard - 1);
                        cbWeaveCard[1] = (byte) (cbOperateCard + 1);
                    }
                    if (pNetInfo.cbOperateCode == HNMJLogic_Defines.WIK_RIGHT)
                    {
                        cbWeaveCard[0] = (byte) (cbOperateCard - 1);
                        cbWeaveCard[1] = (byte) (cbOperateCard - 2);
                    }
                    CGameLogic.Instance().RemoveValueCard(ref m_kHandCardData, m_kHandCardCout, cbWeaveCard, 2);
                    m_kHandCardCout -= 2;
                }
                else
                {
                    m_kHandCardCout = (byte) (HNMJ_Defines.MAX_COUNT - m_kWeaveCount*3);
                }
            }
            ///selfUICommandList.Add(new PlayerUICommand(4,null));
            showHandCard();
        }

        public void NewCardLogic(int iCard)
        {
            m_kHandCardCout++;
            Debug.Log("------------------------------------------------------------NewCardLogic:addNewInCard=" + iCard + ", m_kHandCardCout=" + m_kHandCardCout);
            //if (m_iIdex == 0)
            {
                m_kHandCardData[HNMJ_Defines.MAX_COUNT - 1 - m_kWeaveCount * 3] = (byte)iCard;
            }

            //showHandCard();//mChen temp add for test
        }

        public void addNewInCard(byte iCard, bool bHu = false)
        {
            Debug.Log("New Card In" + HNGameManager.GetCardValueInt((byte)iCard));
            if (bHu == false)
            {
                selfUICommandList.Add(new PlayerUICommand(PlayerAnimType.NewCardIn, new[] { iCard }));
                Loom.QueueOnMainThread(() =>
                {
                    hnManager.SetLeftCard(-1);
                    //hnManager.NewCardIn(GetChairID(), iCard);
                });
            }
        }

        public void AddLeftCard(byte iCard)
        {
            int nLength = m_kOutCardList.Count;
            m_kOutCardList.Add(new OutCardInfo(iCard));
            ushort chairId = GetChairID();
            selfUICommandList.Add(new PlayerUICommand(PlayerAnimType.ShowLeftOutCard, new[] { (byte)chairId, iCard, (byte)nLength }));
        }

        public void sendOutCard(int iCard, int removedIndex)
        {
            if (m_iIdex == 0)
            {
                cbChiCard = 0;//出完牌后重置
            }
            addHandOutCard(iCard);
            int outNum = m_kOutCardList.Count;

            setActOutCard(iCard, removedIndex);
        }

        public void ChaPaiLogic(int removedIndex, int insertIndex, byte cardByte)
        {
            if (removedIndex == insertIndex)
            {
                m_kHandCardData[insertIndex] = cardByte;
            }
            else if (removedIndex > insertIndex)
            {
                for (int i = removedIndex; i > insertIndex + 1 && i > 1; i--)
                {
                    m_kHandCardData[i] = m_kHandCardData[i - 1];
                }
                m_kHandCardData[insertIndex] = cardByte;
            }
            else
            {
                for (int i = removedIndex; i < insertIndex; i++)
                {
                    m_kHandCardData[i] = m_kHandCardData[i + 1];
                }
                m_kHandCardData[insertIndex] = cardByte;
            }
        }

        public void setActOutCard(int iCard, int removedIndex = -1, bool bDel = true)
        {

            // cocos2d::Node* pNode = WidgetFun::getChildWidget(m_pSeatNode, "BigOutNode");
            // pNode.setVisible(iCard >= 0);

            if (iCard < 0)
            {
                return;
            }
           // std::string kImagic = WidgetFun::getWidgetUserInfo(pNode, "Imagic");
           // setCardImagic(pNode, iCard, kImagic);

            //if (m_pHandOutCard != NULL)
            {
                //出牌动画
           //     cocos2d::Vec2 kEndPos = m_pHandOutCard.getPosition();
          //      pNode.setAnchorPoint(m_pHandOutCard.getAnchorPoint());
          //      WidgetFun::setWidgetUserInfo(pNode, "OutHandPos", utility::toString(kEndPos));
          //      WidgetFun::runWidgetAction(pNode, "Start");
            }
            
            //  HNMJSoundFun::playCardByGender(GetGender(), iCard);
            if (bDel)
            {
                //lin:重连时bDel为false，不需要播放声音
                Loom.QueueOnMainThread(() =>
                {
                    hnManager.PlayCardClipByGender(GetChairID(), GetGender(), iCard);
                });

                if (m_iIdex == 0 && HNGameManager.bFakeServer == false)
                //if (true)
                {
                   Debug.Log("out index is " + removedIndex + " out card " +
                                  HNGameManager.GetCardValueInt((byte) iCard));
                        Debug.Assert(iCard == m_kHandCardData[removedIndex],
                            "Removed Card Error!! " + iCard + " != " + m_kHandCardData[removedIndex] + " with " +
                            removedIndex);
                   
                    if (removedIndex == m_kHandCardCout - 1)
                    {
#if true
                        removedIndex = 255;
                        /*Loom.QueueOnMainThread(() =>
                        {
                            hnManager.SendLastCard(GetChairID(), m_kOutCardList.Count - 1, removedIndex, (byte)iCard);
                        });
                        m_kHandCardCout--;*/
                        m_kHandCardData[m_kHandCardCout - 1] = 0;
                        m_kHandCardCout--;

                        selfUICommandList.Add(new PlayerUICommand(PlayerAnimType.ChaPai, new[] { (byte)(m_kOutCardList.Count - 1), (byte)removedIndex, (byte)(m_kHandCardCout), (byte)iCard }));

#else
                        Loom.QueueOnMainThread(() =>
                        {
                            hnManager.SendLastCard(GetChairID(), m_kOutCardList.Count - 1, removedIndex, (byte)iCard);
                        });
                        m_kHandCardCout--;
#endif
                        return;
                    }
                    int iInsertIndex = -1;

                    if (m_kHandCardData[m_kHandCardCout - 1] == 55)
                    {
                        iInsertIndex = 0;
                    }
                    else
                    {
                        for (int i = 0; i < m_kHandCardCout - 1; i++)
                        {
                            Debug.Log("card data " + m_kHandCardData[i]);
                            if (m_kHandCardData[i] == 55)
                                continue;

                            if (i == removedIndex)
                            {
                                if (i == m_kHandCardCout - 2)
                                {
                                    Debug.Log("Insert at index i == m_kHandCardCout - 2");
                                    iInsertIndex = removedIndex;
                                    break;
                                }
                                else if (m_kHandCardData[i + 1] >= m_kHandCardData[m_kHandCardCout - 1])
                                {
                                    Debug.Log("Insert at index m_kHandCardData[i + 1] > m_kHandCardData[m_kHandCardCout - 1] " + m_kHandCardData[i + 1]);
                                    iInsertIndex = removedIndex;
                                    break;
                                }
                            }
                            else if (i < removedIndex)
                            {
                                if (m_kHandCardData[i] > m_kHandCardData[m_kHandCardCout - 1])
                                {
                                    iInsertIndex = i;
                                    Debug.Log("Insert at index " + i + " m_kHandCardData[i] " + m_kHandCardData[i] + " " + m_kHandCardData[m_kHandCardCout - 1]);
                                    break;
                                }
                            }
                            else
                            {
                                if (m_kHandCardData[i] >= m_kHandCardData[m_kHandCardCout - 1])
                                {
                                    iInsertIndex = i - 1;
                                    Debug.Log("Insert at index " + i + " m_kHandCardData[i] " + m_kHandCardData[i] + " " + m_kHandCardData[m_kHandCardCout - 1]);
                                    break;
                                }
                            }
                        }
                    }

                    CGameLogic.Instance().RemoveValueCardOne(ref m_kHandCardData, m_kHandCardCout, (byte)iCard);
                    m_kHandCardCout--;
                    CGameLogic.Instance().SortCardList(ref m_kHandCardData, m_kHandCardCout);
                    
                    if (iInsertIndex == -1)
                    {
                        iInsertIndex = m_kHandCardCout - 1;
                    }
#if true
                    selfUICommandList.Add(new PlayerUICommand(PlayerAnimType.ChaPai, new[] { (byte)removedIndex, (byte)iInsertIndex, (byte)(3 * m_kWeaveCount), (byte)iCard }));
#else
                    Loom.QueueOnMainThread(() =>
                    {
                        hnManager.ChaPai(GetChairID(), removedIndex, iInsertIndex, 3 * m_kWeaveCount);
                    });
#endif
                }
                else
                {
                    if (iCard == m_kHandCardData[m_kHandCardCout-1])//刚摸的牌直接打出，无需插牌动画
                    {
                        removedIndex = 255;
                        /*Loom.QueueOnMainThread(() =>
                        {
                            hnManager.SendLastCard(GetChairID(), m_kOutCardList.Count - 1, removedIndex, (byte)iCard);
                        });
                        m_kHandCardCout--;*/
                        m_kHandCardData[m_kHandCardCout - 1] = 0;
                        m_kHandCardCout--;

                        selfUICommandList.Add(new PlayerUICommand(PlayerAnimType.ChaPai, new[] { (byte)(m_kOutCardList.Count - 1), (byte)removedIndex, (byte)(m_kHandCardCout), (byte)iCard }));
                        return;
                    }

                    for (int i = 0; i < m_kHandCardCout; i++)
                    {
                        if (iCard == m_kHandCardData[i])
                        {
                            removedIndex = i;
                        }
                    }
                    int iInsertIndex = -1;

                    if (m_kHandCardData[m_kHandCardCout - 1] == 55)
                    {
                        iInsertIndex = 0;
                    }
                    else
                    {
                        for (int i = 0; i < m_kHandCardCout - 1; i++)
                        {
                            Debug.Log("card data " + m_kHandCardData[i]);
                            if (m_kHandCardData[i] == 55)
                                continue;

                            if (i == removedIndex)
                            {
                                if (i == m_kHandCardCout - 2)
                                {
                                    Debug.Log("Insert at index i == m_kHandCardCout - 2");
                                    iInsertIndex = removedIndex;
                                    break;
                                }
                                else if (m_kHandCardData[i + 1] >= m_kHandCardData[m_kHandCardCout - 1])
                                {
                                    Debug.Log("Insert at index m_kHandCardData[i + 1] > m_kHandCardData[m_kHandCardCout - 1] " + m_kHandCardData[i + 1]);
                                    iInsertIndex = removedIndex;
                                    break;
                                }
                            }
                            else if (i < removedIndex)
                            {
                                if (m_kHandCardData[i] > m_kHandCardData[m_kHandCardCout - 1])
                                {
                                    iInsertIndex = i;
                                    Debug.Log("Insert at index " + i + " m_kHandCardData[i] " + m_kHandCardData[i] + " " + m_kHandCardData[m_kHandCardCout - 1]);
                                    break;
                                }
                            }
                            else
                            {
                                if (m_kHandCardData[i] >= m_kHandCardData[m_kHandCardCout - 1])
                                {
                                    iInsertIndex = i - 1;
                                    Debug.Log("Insert at index " + i + " m_kHandCardData[i] " + m_kHandCardData[i] + " " + m_kHandCardData[m_kHandCardCout - 1]);
                                    break;
                                }
                            }
                        }
                    }

                    CGameLogic.Instance().RemoveValueCardOne(ref m_kHandCardData, m_kHandCardCout, (byte)iCard);
                    m_kHandCardCout--;
                    CGameLogic.Instance().SortCardList(ref m_kHandCardData, m_kHandCardCout);

                    if (iInsertIndex == -1)
                    {
                        iInsertIndex = m_kHandCardCout - 1;
                    }
                    selfUICommandList.Add(new PlayerUICommand(PlayerAnimType.ChaPai, new[] { (byte)removedIndex, (byte)iInsertIndex, (byte)(3 * m_kWeaveCount), (byte)iCard }));
                }
            }
            else
            {
                //lin: 重连时牌已经在onplayscene函数内调用显示了，此处不需要重复调用
                //重连游戏，没有动画，直接显示麻将
                //showHandCard();
            }
        }

        public int getOutCardNum()
        {
            return m_kOutCardList.Count;
        }
        public void addHandOutCard(int iCard)
        {
            m_kOutCardList.Add(new OutCardInfo((byte)iCard));
            //return obj;
        }

        public void ShowOutCard()
        {
            int chairID = GetChairID();
            for (int i = 0; i < m_kOutCardList.Count; i++)
            {
                hnManager.SetOutCardObj(chairID, i,
                    hnManager.getOutCard(HNGameManager.GetCardValueInt(m_kOutCardList[i].nCardData), -2, chairID));
            }
        }

        public void showHandCard()
        {
            showHandCard(m_kWeaveItemArray, m_kWeaveCount, m_kHandCardData, m_kHandCardCout, m_cbHuaCard, m_cbHuaCardCount);
        }

        public void showHandCard(CMD_WeaveItem[] pWeave, int iWeaveCout, byte[] pHandCard, int iHandCout,
          byte[] pHuaCard, int iHuaCardCount)
        {
            bool bHaveNewIn = (iWeaveCout*3 + iHandCout) == HNMJ_Defines.MAX_COUNT;
            int iIdex = m_iIdex;
            Debug.Log("-------------------------------------------------showHandCard:iHandCout" + iHandCout);
#if true
            /*
            cocos2d::Node* pCardNode = WidgetFun::getChildWidget(m_pSeatNode, "CardNode1");
            pCardNode.removeAllChildren();
            cocos2d::Vec2 kStartPos = WidgetFun::getChildWidget(m_pSeatNode, "HandPosNode").getPosition();
            int iAddOder = utility::parseInt(WidgetFun::getWidgetUserInfo(
                m_pSeatNode, "HandPosNode", "AddOder"));
            cocos2d::Vec2 kHandAddPos = utility::parsePoint(WidgetFun::getWidgetUserInfo(
                m_pSeatNode, "HandPosNode", "HandAddPos"));
            cocos2d::Vec2 kNewInAddPos = utility::parsePoint(WidgetFun::getWidgetUserInfo(
                m_pSeatNode, "HandPosNode", "NewInAddPos"));
            std::string kPengSkin = utility::toString("HNMJ_PENG_", iIdex);
            std::string kGangSkin = utility::toString("HNMJ_GANG_", iIdex);
            std::string kHandSkin = utility::toString("HNMJ_HAND_", iIdex);
            */
            int iOder = 0;

            /*
            //补花
            std::string kHuaSkin = utility::toString("HNMJ_HUA_", iIdex);
            for (int i = 0; i < iHuaCardCount; i++)
            {
                BYTE* pTemp = pHuaCard + i;
                int iCardValue = *pTemp;

                cocos2d::Node* pNode = WidgetManager::Instance().createWidget(kHuaSkin, pCardNode);
                pNode->setZOrder(iOder);
                pNode->setPosition(kStartPos);
                kStartPos += kHandAddPos * 1;
                std::string kImagic = WidgetFun::getWidgetUserInfo(pNode, "Imagic");
                setCardImagic(WidgetFun::getChildWidget(pNode, "Card0"), iCardValue, kImagic);
            }
            */
            if (m_iNewWaveIndex != -1)
            {
                CMD_WeaveItem pTemp = pWeave[m_iNewWaveIndex];
                m_iNewWaveIndex = -1;
                int iCardValue = HNGameManager.GetCardValueInt(pTemp.cbCenterCard);
#if false //暗杠没做
                if (pTemp.cbPublicCard == 0)
                {
                    iCardValue = -1;
                }
#endif
                if (pTemp.cbWeaveKind == HNMJLogic_Defines.WIK_PENG)
                {
                    //Loom.QueueOnMainThread(() =>
                    //{
                        int[] valueArray = {iCardValue, iCardValue, iCardValue};
                        hnManager.SetChiPengGangObj(GetChairID(), valueArray, 1,
                            HNGameManager.CalArrowDir(GetChairID(), pTemp.wProvideUser));
                    //});
                 
                }
                else if (pTemp.cbWeaveKind == HNMJLogic_Defines.WIK_GANG)
                {
                    //Loom.QueueOnMainThread(() =>
                    //{
                        int[] valueArray = {iCardValue, iCardValue, iCardValue, iCardValue};
                        hnManager.SetChiPengGangObj(GetChairID(), valueArray, 2,
                            HNGameManager.CalArrowDir(GetChairID(), pTemp.wProvideUser));
                    //});
                }
                else if (pTemp.cbWeaveKind == HNMJLogic_Defines.WIK_LEFT)
                {
                    //Loom.QueueOnMainThread(() =>
                    //{
                        int[] valueArray = {iCardValue, iCardValue + 1, iCardValue + 2};
                        hnManager.SetChiPengGangObj(GetChairID(), valueArray, 3, 0);
                    //});
                }
                else if (pTemp.cbWeaveKind == HNMJLogic_Defines.WIK_CENTER)
                {
                    //Loom.QueueOnMainThread(() =>
                    //{
                        int[] valueArray = {iCardValue - 1, iCardValue, iCardValue + 1};
                        hnManager.SetChiPengGangObj(GetChairID(), valueArray, 4, 0);
                    //});
                }
                else if (pTemp.cbWeaveKind == HNMJLogic_Defines.WIK_RIGHT)
                {
                   // Loom.QueueOnMainThread(() =>
                    //{
                        int[] valueArray = {iCardValue - 2, iCardValue - 1, iCardValue};
                        hnManager.SetChiPengGangObj(GetChairID(), valueArray, 5, 0);
                   // });
                }
            }
            //if (m_iIdex == 0)
            if(true)
            {

                //Loom.QueueOnMainThread(() =>
                //{
                    int hancCount = iHandCout;
#if false
                    if (bHaveNewIn)
                    {
                        hancCount -= 1;
                    }
#endif
                    for (int i = 0; i < hancCount; i++)
                    {
                        byte pTemp = pHandCard[i];
                        Debug.Log(m_iIdex + " player : card value: " + pTemp + " at index: " + i);
                        if (pTemp > 0)
                        {
                            pTemp = (byte) HNGameManager.GetCardValueInt(pTemp);
                            hnManager.setPlayerCardObj(GetChairID(), i, i + iWeaveCout * 3, pTemp);
                            // WidgetFun::setImagic(pNode, utility::toString(kImagicFront, (int)cbColor, (int)cbValue, ".png"), false);
                        }
                    }
            }
            else
            {
                //Loom.QueueOnMainThread(() =>
                //{
                    hnManager.SetOtherPlayersCard(GetChairID(), iWeaveCout * 3, m_kHandCardCout, bHaveNewIn);
                //});
            }
#endif
        }

        public void setCardImagic(object pNode, int kValue, string kImagicFront)
        {

        }

#if false
        public object getTouchCardNode(cocos2d::Vec2 kTouchPos)
        {
            
        }
        public byte getTouchCardVlaue(GameObject pNode)
        {
            
        }
#endif

        public void removeHandOutCard(byte cbCardData)
        {
            int nSize = m_kOutCardList.Count;
            if (nSize <= 0)
            {
                return;
            }
            int i = nSize - 1;
            if (m_kOutCardList[i].nCardData == cbCardData)
            {
                Debug.Log("Remove out card " + cbCardData + " success len : " + m_kOutCardList.Count);
                m_kOutCardList.RemoveAt(m_kOutCardList.Count - 1);
            }
            else
            {
                Debug.Log("Remove out card failed " + cbCardData + " len : " + m_kOutCardList.Count);
            }
        }

        public void showCard()
        {
            Debug.Log("---------------------------Show card,m_kHandCardCout="+ m_kHandCardCout);
            showCard(m_kHandCardData, m_kHandCardCout);
        }

        public void showCard(byte[] cbCardData, byte cbCardCount)
        {
            ushort chairId = GetChairID();
            byte[] parameters = new byte[1+cbCardData.Length];
            parameters[0] = (byte)chairId;
            Buffer.BlockCopy(cbCardData, 0, parameters, 1, cbCardData.Length);
            selfUICommandList.Add( new PlayerUICommand(PlayerAnimType.GameEndShowCard, parameters) );
#if false
#if true
            Loom.QueueOnMainThread(() =>
            {
                for (int i = 0; i < cbCardCount; i++)
                {
                    byte pTemp = cbCardData[i];
                    Debug.Log(m_iIdex + "showCard player : card value: " + pTemp + " at index: " + i);
                    if (pTemp > 0)
                    {
                        pTemp = (byte)HNGameManager.GetCardValueInt(pTemp);
                        hnManager.setPlayerCardObj(GetChairID(), i, i + m_kWeaveCount * 3, pTemp, true);
                        // WidgetFun::setImagic(pNode, utility::toString(kImagicFront, (int)cbColor, (int)cbValue, ".png"), false);
                    }
                }
            });
#else
            BYTE cbIdex = 0;
            cocos2d::Node* pRootNode = WidgetFun::getChildWidget(m_pSeatNode, "CardNode1");
            for (int i = 0; i < pRootNode->getChildrenCount(); i++)
            {
                cocos2d::Sprite* pSprite = dynamic_cast<cocos2d::Sprite*>(pRootNode->getChildren().at(i));
                if (pSprite == NULL || pSprite->getTag() != 1)
                {
                    continue;
                }
                float fHuScale = utility::parseFloat(WidgetFun::getWidgetUserInfo(pSprite, "HuScale"));
                pSprite->setScale(fHuScale);
                std::string kImagic = WidgetFun::getWidgetUserInfo(pSprite, "MingImagic");
                if (cbIdex < cbCardCount)
                {
                    setCardImagic(pSprite, cbCardData[cbIdex], kImagic);
                }
                else
                {
                    setCardImagic(pSprite, 0, kImagic);
                }
                cbIdex++;
            }
#endif
#endif
        }

        public void showJieSuanCard(object pCardNode, CMD_WeaveItem[] pWeave, int iWeaveCout, byte[] pHandCard,int iHandCout)
        {
           //lin: do nothing in hainan majiang
        }

        public void setChiHuCard(byte cbCard)
        {
            m_cbChiHuCard = cbCard;
        }

        public byte getChiHuCard()
        {
            return m_cbChiHuCard;
        }

        //Lin: not used
        public byte getGangCard(byte currentCard)
        {
            return 0;
        }

        public void seatDownCard()
        {
        }

        public void ClickCard(object pCard)
        {
        }

        public void runAniDianPao()
        {
        }

        public void runAniZiMo()
        {
        }

        public void runAniPeng()
        {
            Loom.QueueOnMainThread(() =>
            {
                hnManager.HideFocus();
                //hnManager.PlaySoundEffect(GetChairID(), (int)AudioManager.Sound_Effect_Defines.SOUND_EFFECT_ChiPengGang);
                hnManager.PlaySoundClipByGender(GetChairID(), GetGender(), (int)AudioManager.Sound_Defines.SOUND_PENG);
            });
        }

        public void runAniMingGang()
        {
            Loom.QueueOnMainThread(() =>
            {
                hnManager.HideFocus();
                //hnManager.PlaySoundEffect(GetChairID(), (int)AudioManager.Sound_Effect_Defines.SOUND_EFFECT_ChiPengGang);
                hnManager.PlaySoundClipByGender(GetChairID(), GetGender(), (int)AudioManager.Sound_Defines.SOUND_GANG);
            });
        }

        public void runAniAnGang()
        {
            Loom.QueueOnMainThread(() =>
            {
                //hnManager.PlaySoundEffect(GetChairID(), (int)AudioManager.Sound_Effect_Defines.SOUND_EFFECT_ChiPengGang);
                hnManager.PlaySoundClipByGender(GetChairID(), GetGender(), (int)AudioManager.Sound_Defines.SOUND_GANG);
            });
        }

        public void runAniChi()
        {
            Loom.QueueOnMainThread(() =>
            {
                hnManager.HideFocus();
                //hnManager.PlaySoundEffect(GetChairID(), (int)AudioManager.Sound_Effect_Defines.SOUND_EFFECT_ChiPengGang);
                hnManager.PlaySoundClipByGender(GetChairID(), GetGender(), (int)AudioManager.Sound_Defines.SOUND_CHI);
            });
        }

        //WQ add
        public bool m_bWasKaiGangYaoShaiZi; //是否开杠摇骰子
        public byte m_kCardDataOfZiMo; //发到的牌
        public bool m_bCanDirectSendCard;//暗杠，补刚或者吃胡时，可以直接出牌，无需点“过”按钮

        int m_iIdex;
        //GameObject m_pSeatNode;

        public byte[] m_kHandCardData = new byte[HNMJ_Defines.MAX_COUNT];
        public byte m_kHandCardCout;

        CMD_WeaveItem[] m_kWeaveItemArray = new CMD_WeaveItem[HNMJ_Defines.MAX_WEAVE]; //组合扑克
        byte m_kWeaveCount; //组合数目
        private int m_iNewWaveIndex = -1;

        //补花
        byte m_cbHuaCardCount;                 //花牌数目 m_cbHuaCardCount;
        byte[] m_cbHuaCard = new byte[8];      //花牌记录 m_cbHuaCard[8];

        //	int						m_iOutCardCout;

        List<OutCardInfo> m_kOutCardList = new List<OutCardInfo>();

        byte m_cbChiHuCard; //所胡的牌
        //GameObject m_pHandOutCard;


        public class PlayerUICommand
        {
            public PlayerAnimType type; // 0 : 摸牌动画, 1 : 补花动画 2: 出牌后的插牌动画 3 : 结束后剩余牌显示动画 4： 吃碰杠动画 5:胡牌时显示手牌 6:胡牌显示手牌前重置手牌
            public byte[] cardData;//data for Command
            public bool bRunning;

            public PlayerUICommand(PlayerAnimType t, byte[] v)
            {
                type = t;
                cardData = v;
                bRunning = false;
            }
        }

        List<PlayerUICommand> selfUICommandList = new List<PlayerUICommand>();

        public void AnimDone()
        {
            selfUICommandList.RemoveAt(0);
        }

        public int AnimCount()
        {
            return selfUICommandList.Count;
        }

        public void AddCommand(PlayerUICommand comd)
        {
            selfUICommandList.Add(comd);
        }

        public PlayerUICommand GetFirstCommand()
        {
            PlayerUICommand comd = null;
            if (selfUICommandList.Count > 0)
            {
                comd = selfUICommandList[0];
            }

            return comd;
        }

        public void UpdateAnim()
        {
            if (selfUICommandList.Count > 0)
            {
                if (selfUICommandList.First().bRunning == false)
                {
                    //todo
                    var comd = selfUICommandList[0];
                    comd.bRunning = true;
                    Debug.Log("--------------------------------Start player " + GetChairID() + " Anim : " + comd.type + " total num " + selfUICommandList.Count);
                    switch (comd.type)
                    {
                        case PlayerAnimType.NewCardIn:
                            /*Loom.QueueOnMainThread(() =>
                            {*/
                            hnManager.NewCardIn(GetChairID(), comd.cardData[0]);
                            //});
                            break;
                        case PlayerAnimType.BuHua:
                            //Loom.QueueOnMainThread(() =>
                            //{
                            hnManager.BuHua(GetChairID(), comd.cardData[0]);
                            //});
                            break;
                        case PlayerAnimType.ChaPai:
                            if (comd.cardData[1] == 255)
                            {
                                hnManager.SendLastCard(GetChairID(), comd.cardData[0], comd.cardData[2], comd.cardData[3]);
                                AnimDone();
                            }
                            else
                            {
                                hnManager.ChaPai(GetChairID(), comd.cardData[0], comd.cardData[1], comd.cardData[2],
                                    comd.cardData[3]);
                            }
                            break;
                        case PlayerAnimType.ShowLeftOutCard:
                            hnManager.ShowLeftOutCard((int)comd.cardData[0], comd.cardData[1], comd.cardData[2]);
                            break;
                        case PlayerAnimType.SetOperateResoult:
                            {
                                CMD_S_OperateResult pNetInfo = new CMD_S_OperateResult();
                                pNetInfo.wOperateUser = (ushort)comd.cardData[0];
                                pNetInfo.wProvideUser = (ushort)comd.cardData[1];
                                pNetInfo.cbOperateCode = comd.cardData[2];
                                pNetInfo.cbOperateCard = comd.cardData[3];
                                ///hnManager.LocalChiPengGang(GetChairID(), pNetInfo);
                                handleOperateResoult(pNetInfo);
                                AnimDone();
                            }
                            break;
                        case PlayerAnimType.ResetHandcardBeforeGameEnd:
                            {
                                //ResetHandcardBeforeGameEnd();//显示所有玩家手牌前重置local玩家的手牌

                                hnManager.ResetCardCountMap(comd.cardData[comd.cardData.Length-1]);
                                hnManager.HideOtherPlayersCard();
                                hnManager.ResetLocalHandCardStuff(comd.cardData);

                                hnManager.bResetLoaclCardsBeforeGameEnd = true;
                                AnimDone();
                            }
                            break;
                        case PlayerAnimType.GameEndShowCard:
                            {
                                int chairId = (int)comd.cardData[0];
                                int cardDataLen = comd.cardData.Length - 1;
                                byte[] cardData = new byte[cardDataLen];
                                Buffer.BlockCopy(comd.cardData, 1, cardData, 0, cardDataLen);

                                hnManager.GameEndShowCard(chairId, cardData);
                            }
                            break;
                        //case PlayerAnimType.HuAction:
                        //    {
                        //        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                        //        if(kernel != null)
                        //        {
                        //            kernel.handle_HNMJButton_HuAction();
                        //            AnimDone();
                        //        }
                        //    }
                        //    break;
                        //case PlayerAnimType.OnSubOperateNotify:
                        //    {
                        //        var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                        //        if (kernel != null)
                        //        {
                        //            CMD_S_OperateNotify pNetInfo = new CMD_S_OperateNotify();
                        //            pNetInfo.wResumeUser = (ushort)comd.cardData[0];
                        //            pNetInfo.cbActionMask = comd.cardData[1];
                        //            pNetInfo.cbActionCard = comd.cardData[2];
                        //            kernel.handle_OnSubOperateNotify(pNetInfo);
                        //            AnimDone();
                        //        }
                        //    }
                        //    break;
                        case PlayerAnimType.SetCurrentPlayer:
                            {
                                var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                                if (kernel != null)
                                {
                                    bool bForceAction = (comd.cardData[3] == 1 ? true : false);
                                    kernel.handle_setCurrentPlayer((int)comd.cardData[0], (int)comd.cardData[1], comd.cardData[2], bForceAction);
                                    AnimDone();
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    };
}
