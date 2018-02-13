#ifndef CMD_SPARROW_HEAD_FILE
#define CMD_SPARROW_HEAD_FILE

//////////////////////////////////////////////////////////////////////////
//公共宏定义
#pragma pack(1)

#define KIND_ID						311									//游戏 I D mChen 310 311 312
//组件属性
#define GAME_PLAYER					20									//游戏人数

//mChen add, for HideSeek WangHu
#define MAX_AI_PER_CLIENT			10									//每个客户端的最大游戏人数==1+最大AI数
#define INVALID_AI_ID				255

#define VERSION_SERVER				PROCESS_VERSION(6,0,3)				//程序版本
#define VERSION_CLIENT				PROCESS_VERSION(6,0,3)				//程序版本

#define GAME_NAME					TEXT("开心躲猫猫")					//游戏名字 mChen 湖南麻将 湖南麻将-转转 湖南麻将-邵东
#define GAME_GENRE					(GAME_GENRE_SCORE|GAME_GENRE_MATCH|GAME_GENRE_GOLD|GAME_GENRE_EDUCATE)	//游戏类型 mChen (GAME_GENRE_SCORE|GAME_GENRE_MATCH|GAME_GENRE_GOLD)

//游戏状态
//#define GS_MJ_FREE					GAME_STATUS_FREE								//空闲状态
//#define GS_MJ_PLAY					(GAME_STATUS_PLAY+1)							//游戏状态
#define GS_MJ_XIAOHU				(GAME_STATUS_PLAY+2)							//小胡状态

//常量定义

#define MAX_WEAVE					5									//最大组合
#define MAX_INDEX					34									//最大索引
#define MAX_COUNT					17									//最大数目
#define MAX_REPERTORY				108									//最大库存
#define MAX_REPERTORY_HZ			136									//红中麻将最大库存

#define MAX_NIAO_CARD				6									//最大中鸟数


#define MAX_RIGHT_COUNT				1									//最大权位DWORD个数	

#define GAME_TYPE_ZZ				0
#define GAME_TYPE_CS				1
#define GAME_TYPE_SD				2	//mChen
#define GAME_TYPE_13Shui			13

#define GAME_TYPE_ZZ_258			0		//只能258做将
#define GAME_TYPE_ZZ_ZIMOHU		    1		//只能自模胡
#define GAME_TYPE_ZZ_QIDUI			2		//可胡七对
#define GAME_TYPE_ZZ_QIANGGANGHU	3		//可抢杠胡
#define GAME_TYPE_ZZ_ZHANIAO2		4		//扎鸟2个
#define GAME_TYPE_ZZ_ZHANIAO4		5		//扎鸟4个
#define GAME_TYPE_ZZ_ZHANIAO6		6		//扎鸟6个
#define GAME_TYPE_ZZ_HONGZHONG		7		//红中玩法


#define ZZ_ZHANIAO0		0		//扎鸟2个
#define ZZ_ZHANIAO2		2		//扎鸟2个
#define ZZ_ZHANIAO4		4		//扎鸟4个
#define ZZ_ZHANIAO6		6		//扎鸟6个
#pragma pack()
#include "../游戏服务器//GameLogic.h"
#include "../../../全局定义/Platform.h"
#include "../../../服务器组件/内核引擎/TraceService.h"
#pragma pack(1)
//////////////////////////////////////////////////////////////////////////

//组合子项
struct CMD_WeaveItem
{
	BYTE							cbWeaveKind;						//组合类型
	BYTE							cbCenterCard;						//中心扑克
	BYTE							cbPublicCard;						//公开标志
	WORD							wProvideUser;						//供应用户
};

//////////////////////////////////////////////////////////////////////////
//服务器命令结构
#define SUB_S_GAME_START			100									//游戏开始
#define SUB_S_OUT_CARD				101									//出牌命令
#define SUB_S_SEND_CARD				102									//发送扑克
#define SUB_S_OPERATE_NOTIFY		104									//操作提示
#define SUB_S_OPERATE_RESULT		105									//操作命令
#define SUB_S_GAME_END				106									//游戏结束
#define SUB_S_TRUSTEE				107									//用户托管
#define SUB_S_CHI_HU				108									//
#define SUB_S_GANG_SCORE			110									//

#define SUB_S_CHAT_PLAY             1000                                //发送文字聊天数据给客户端

//mChen add, for HideSeek
#define SUB_S_HideSeek_HeartBeat			115						
#define SUB_S_HideSeek_AICreateInfo			116		

//文字聊天
struct CMD_C_CHAT
{
	WORD ChairId;
	TCHAR UserStatus;
	TCHAR ChatData[100];
};

//游戏状态
struct CMD_S_StatusFree
{
	//mChen add, for HideSeek
	BYTE							cbGameStatus;
	BYTE							cbMapIndex;
	//for随机种子同步
	WORD							wRandseed;
	//地图随机物品生成
	WORD							wRandseedForRandomGameObject;
	//道具同步
	WORD							wRandseedForInventory;
	InventoryItem					sInventoryList[MAX_INVENTORY_NUM];

	//LONGLONG						lCellScore;							//基础金币
	//WORD							wBankerUser;						//庄家用户
	//bool							bTrustee[GAME_PLAYER];				//是否托管
};

//游戏状态
struct CMD_S_StatusPlay
{
	//mChen add, for HideSeek
	BYTE							cbGameStatus;
	BYTE							cbMapIndex;

	//for随机种子同步
	WORD							wRandseed;
	//地图随机物品生成
	WORD							wRandseedForRandomGameObject;
	//道具同步
	WORD							wRandseedForInventory;
	InventoryItem					sInventoryList[MAX_INVENTORY_NUM];

	////游戏变量
	//LONGLONG						lCellScore;									//单元积分
	//WORD							wBankerUser;								//庄家用户
	//WORD							wCurrentUser;								//当前用户

	////状态变量
	//BYTE							cbActionCard;								//动作扑克
	//BYTE							cbActionMask;								//动作掩码
	//BYTE							cbLeftCardCount;							//剩余数目
	//bool							bTrustee[GAME_PLAYER];						//是否托管
	//WORD							wWinOrder[GAME_PLAYER];						//

	////出牌信息
	//WORD							wOutCardUser;								//出牌用户
	//BYTE							cbOutCardData;								//出牌扑克
	//BYTE							cbDiscardCount[GAME_PLAYER];				//丢弃数目
	//BYTE							cbDiscardCard[GAME_PLAYER][60];				//丢弃记录

	////扑克数据
	//BYTE							cbCardCount;								//扑克数目
	//BYTE							cbCardData[MAX_COUNT * GAME_PLAYER];		//扑克列表
	//BYTE							cbSendCardData;								//发送扑克

	////组合扑克
	//BYTE							cbWeaveCount[GAME_PLAYER];					//组合数目
	//CMD_WeaveItem					WeaveItemArray[GAME_PLAYER][MAX_WEAVE];		//组合扑克

	////ZY add 分数信息
	//int								TotalScore_MJ[GAME_PLAYER];					//麻将总分
	//tagGangCardResult				tGangResult;								//重连玩家杠牌信息
};

//游戏开始
struct CMD_S_GameStart
{
	LONG							lSiceCount;									//骰子点数
	WORD							wBankerUser;								//庄家用户
	WORD							wCurrentUser;								//当前用户
	BYTE							cbUserAction;								//用户动作
	BYTE							cbCardData[MAX_COUNT*GAME_PLAYER];			//扑克列表
	BYTE							cbLeftCardCount;							//
	BYTE							cbXiaoHuTag;                           //小胡标记 0 没小胡 1 有小胡；

};

//出牌命令
struct CMD_S_OutCard
{
	WORD							wOutCardUser;						//出牌用户
	BYTE							cbOutCardData;						//出牌扑克
	bool							bIsPiao;
};

//发送扑克
struct CMD_S_SendCard
{
	BYTE							cbSendCardData;						//扑克数据
	BYTE							cbActionMask;						//动作掩码
	WORD							wCurrentUser;						//当前用户
	bool							bTail;								//末尾发牌

																		//mChen add
	//BYTE							cbActionCard;						//动作扑克
	tagGangCardResult				tGangCard;							//杠数据（最多有4个杠）
	bool							bKaiGangYaoShaiZi;					//是否开杠摇骰子
};


//操作提示
struct CMD_S_OperateNotify
{
	WORD							wResumeUser;						//还原用户
	BYTE							cbActionMask;						//动作掩码
	BYTE							cbActionCard;						//动作扑克
};

//操作命令
struct CMD_S_OperateResult
{
	WORD							wOperateUser;						//操作用户
	WORD							wProvideUser;						//供应用户
	BYTE							cbOperateCode;						//操作代码
	BYTE							cbOperateCard;						//操作扑克
};

//游戏结束
struct CMD_S_GameEnd
{
	//BYTE							cbCardCount[GAME_PLAYER];			//
	//BYTE							cbCardData[GAME_PLAYER][MAX_COUNT];	//
	////结束信息
	//WORD							wProvideUser[GAME_PLAYER];			//供应用户
	//DWORD							dwChiHuRight[GAME_PLAYER];			//胡牌类型
	//DWORD							dwStartHuRight[GAME_PLAYER];			//起手胡牌类型
	//LONGLONG						lStartHuScore[GAME_PLAYER];			//起手胡牌分数

	////积分信息
	//LONGLONG						lGameScore[GAME_PLAYER];			//游戏积分
	//LONGLONG						lTotalScore[GAME_PLAYER];			//游戏总分
	//int								lGameTax[GAME_PLAYER];				//

	//WORD							wWinOrder[GAME_PLAYER];				//胡牌排名

	//LONGLONG						lGangScore[GAME_PLAYER];//详细得分
	//BYTE							cbGenCount[GAME_PLAYER];			//
	//WORD							wLostFanShu[GAME_PLAYER][GAME_PLAYER];
	//WORD							wLeftUser;	//

	////组合扑克
	//BYTE							cbWeaveCount[GAME_PLAYER];					//组合数目
	//CMD_WeaveItem					WeaveItemArray[GAME_PLAYER][MAX_WEAVE];		//组合扑克


	//BYTE							cbCardDataNiao[MAX_NIAO_CARD];	// 鸟牌
	//BYTE							cbNiaoCount;	//鸟牌个数
	//BYTE							cbNiaoPick;	//中鸟个数

	//mChen add
	BYTE							cbEndReason;

	////mChen add：剩余库存扑克
	//BYTE							cbLeftCardCount;
	//BYTE							cbRepertoryLeftCard[MAX_REPERTORY_HZ];
};

//用户托管
struct CMD_S_Trustee
{
	bool							bTrustee;							//是否托管
	WORD							wChairID;							//托管用户
};

//胡牌信息
struct CMD_S_ChiHu
{
	WORD							wChiHuUser;							//
	WORD							wProviderUser;						//
	BYTE							cbChiHuCard;						//
	BYTE							cbCardCount;						//
	LONGLONG						lGameScore;							//
	BYTE							cbWinOrder;							//
};

//杠分信息
struct CMD_S_GangScore
{
	WORD							wChairId;							//
	BYTE							cbXiaYu;							//
	LONGLONG						lGangScore[GAME_PLAYER];			//
};

//////////////////////////////////////////////////////////////////////////
//客户端命令结构

#define SUB_C_OUT_CARD				1									//出牌命令
#define SUB_C_OPERATE_CARD			3									//操作扑克
#define SUB_C_TRUSTEE				4									//用户托管
#define SUB_C_XIAOHU				5									//小胡

//mChen add, for HideSeek
#define SUB_C_HIDESEEK_PLAYER_INFO			6									//客户端玩家位置等信息
#define SUB_C_HIDESEEK_PLAYERS_INFO			7

#define SUB_C_CHAT_PLAY                     1001                                //从客户端接收文字聊天数据

//mChen add, for HideSeek
#define MAX_PLAYER_HP						4
//客户端玩家事件信息
enum PlayerEventKind
{
	Pick = 0,
	Boom,			//炸弹爆炸

	DeadByDecHp,    //自己扣完血而死
	DeadByPicked,   //被警察点死
	DeadByBoom,     //被炸弹炸死

	GetInventory,   //拾取道具
	DecHp,
	AddHp,

	MaxEventNum
};
struct PlayerEventItem
{
	BYTE cbTeamType;
	WORD wChairId;
	BYTE cbAIId;

	BYTE cbEventKind;

	INT32 nCustomData0;
	INT32 nCustomData1;
	INT32 nCustomData2;

	void StreamValue(datastream& kData, bool bSend)
	{
		Stream_VALUE(cbTeamType);
		Stream_VALUE(wChairId);
		Stream_VALUE(cbAIId);

		Stream_VALUE(cbEventKind);

		Stream_VALUE(nCustomData0);
		Stream_VALUE(nCustomData1);
		Stream_VALUE(nCustomData2);
	}
};
//客户端玩家位置等信息
struct PlayerInfoItem
{
	BYTE cbTeamType;
	WORD wChairId;
	BYTE cbAIId;

	INT32 posX;
	INT32 posY;
	INT32 posZ;

	INT32 angleX;
	INT32 angleY;
	INT32 angleZ;

	///TCHAR objNamePicked[LEN_NICKNAME];

	//必须放在最后
	BYTE cbHP;	
	BYTE cbIsValid;

	////IsPicked:0x80, HasKilledPlayer:0x40, KilledPlayerChairID:0x3f
	//BYTE cbIsPickedAndKilled;

	////HasKilledPlayer:0x80, KilledPlayerIsAI:0x40, KilledPlayerTeamType:0x20, KilledPlayerChairID:0x1f, 
	//BYTE cbKilledPlayer;
	//BYTE cbKilledAIIdx;

	void StreamValue(datastream& kData, bool bSend)
	{
		Stream_VALUE(cbTeamType);
		Stream_VALUE(wChairId);
		Stream_VALUE(cbAIId);

		Stream_VALUE(posX);
		Stream_VALUE(posY);
		Stream_VALUE(posZ);

		Stream_VALUE(angleX);
		Stream_VALUE(angleY);
		Stream_VALUE(angleZ);

		Stream_VALUE(cbHP);
		Stream_VALUE(cbIsValid);
	}
};
/*
struct CMD_C_HideSeek_ClientPlayersInfo
{
	bool bIsValid;
	bool bIsLocked;

	WORD wAIItemCount;
	///WORD wEventItemCount;

	PlayerInfoItem HumanInfoItem;
	PlayerInfoItem AIInfoItems[GAME_PLAYER];
	///PlayerEventItem PlayerEventItems[GAME_PLAYER];
	///std::vector<PlayerInfoItem>	AIInfoItems;
	std::vector<PlayerEventItem> PlayerEventItems;

	void Reset()
	{
		bIsValid = false;
		bIsLocked = true;
		wAIItemCount = 0;
		ZeroMemory(&HumanInfoItem, sizeof(PlayerInfoItem));
		ZeroMemory(AIInfoItems, sizeof(AIInfoItems));
		PlayerEventItems.clear();

		//HP
		HumanInfoItem.cbHP = 4;
		for (int i = 0; i < GAME_PLAYER; i++)
		{
			AIInfoItems[i].cbHP = 4;
		}
	}

	void StreamValue(BYTE *pDataBuffer, WORD wDataSize)
	{
		bIsValid = true;

		wAIItemCount = *((WORD*)pDataBuffer);
		pDataBuffer += sizeof(wAIItemCount);

		WORD wEventItemCount = *((WORD*)pDataBuffer);
		pDataBuffer += sizeof(wEventItemCount);

		if (wAIItemCount > GAME_PLAYER || wEventItemCount > GAME_PLAYER)
		{
			////提示消息
			//TCHAR szString[512] = TEXT("");
			//_sntprintf(szString, CountArray(szString), TEXT("CMD_C_HideSeek_ClientPlayersInfo StreamValue error: wAIItemCount=%d, wEventItemCount=%d ]"), wAIItemCount, wEventItemCount);
			//CTraceService::TraceString(szString, TraceLevel_Warning);

			wAIItemCount = 0;
			wEventItemCount = 0;

			return;
		}

		// Human Item
		BYTE cbHPOld = HumanInfoItem.cbHP;
		CopyMemory(&HumanInfoItem, pDataBuffer, sizeof(HumanInfoItem)-sizeof(HumanInfoItem.cbHP));
		pDataBuffer += sizeof(HumanInfoItem);
		//if (bIsLocked)
		//{
		//	//第一次获取到客户端的数据
		//}
		//else
		//{
		//	//只有在第一次接收客户端的HP数据，以后都只用服务端的HP数据

		//	//恢复成服务端原来的HP数据
		//	HumanInfoItem.cbHP = cbHPOld;
		//}

		// AI Items
		for (int i = 0; i < wAIItemCount; i++)
		{
			cbHPOld = AIInfoItems[i].cbHP;

			CopyMemory(&AIInfoItems[i], pDataBuffer, sizeof(PlayerInfoItem)-sizeof(AIInfoItems[i].cbHP));
			///AIInfoItems[i] = *((PlayerInfoItem*)pDataBuffer);
			pDataBuffer += sizeof(PlayerInfoItem);

			//if (!bIsLocked)
			//{
			//	AIInfoItems[i].cbH = cbHPOld;
			//}
		}

		bIsLocked = false;

		//for (int i = 0; i < wEventItemCount; i++)
		//{
		//	CopyMemory(&PlayerEventItems[i], pDataBuffer, sizeof(PlayerEventItem));
		//	///PlayerEventItems[i] = *((PlayerEventItem*)pDataBuffer);
		//	pDataBuffer += sizeof(PlayerEventItem);
		//}

		//AIInfoItems.clear();
		//for (int i = 0; i < wAIItemCount; i++)
		//{
		//	PlayerInfoItem pItem = *((PlayerInfoItem*)pDataBuffer);
		//	AIInfoItems.push_back(pItem);
		//	pDataBuffer += sizeof(PlayerInfoItem);
		//}

		// Event Items
		//PlayerEventItems.clear();
		for (int i = 0; i < wEventItemCount; i++)
		{
			PlayerEventItem pItem = *((PlayerEventItem*)pDataBuffer);
			PlayerEventItems.push_back(pItem);
			pDataBuffer += sizeof(PlayerEventItem);
		}
	}
};
*/
struct CMD_S_HideSeek_ClientPlayersInfo
{
	PlayerInfoItem HumanInfoItems[GAME_PLAYER];
	PlayerInfoItem AIInfoItems[GAME_PLAYER];
	std::vector<PlayerEventItem> PlayerEventItems;

	void Reset()
	{
		ZeroMemory(HumanInfoItems, sizeof(HumanInfoItems));
		ZeroMemory(AIInfoItems, sizeof(AIInfoItems));

		std::vector<PlayerEventItem> tmpEventItems;
		tmpEventItems.swap(PlayerEventItems);
		//PlayerEventItems.clear();

		//HP
		for (int i = 0; i < GAME_PLAYER; i++)
		{
			HumanInfoItems[i].cbHP = 4;
		}
		for (int i = 0; i < GAME_PLAYER; i++)
		{
			AIInfoItems[i].cbHP = 4;
		}
	}

	void StreamValue(BYTE *pDataBuffer, WORD wDataSize)
	{
		WORD wAIItemCount = *((WORD*)pDataBuffer);
		pDataBuffer += sizeof(wAIItemCount);

		WORD wEventItemCount = *((WORD*)pDataBuffer);
		pDataBuffer += sizeof(wEventItemCount);

		if (wAIItemCount > GAME_PLAYER || wEventItemCount > GAME_PLAYER)
		{
			//Log
			TCHAR szString[128] = TEXT("");
			_sntprintf(szString, CountArray(szString), TEXT("CMD_S_HideSeek_ClientPlayersInfo StreamValue error: wAIItemCount=%d, wEventItemCount=%d"), wAIItemCount, wEventItemCount);
			CTraceService::TraceString(szString, TraceLevel_Warning);

			wAIItemCount = 0;
			wEventItemCount = 0;

			return;
		}

		// Human Item
		PlayerInfoItem *pClientHumanInfoItem = (PlayerInfoItem *)pDataBuffer;
		WORD wHumanChairId = pClientHumanInfoItem->wChairId;
		if (wHumanChairId >= GAME_PLAYER)
		{
			//Log
			TCHAR szString[128] = TEXT("");
			_sntprintf(szString, CountArray(szString), TEXT("CMD_S_HideSeek_ClientPlayersInfo StreamValue error: wHumanChairId=%d"), wHumanChairId);
			CTraceService::TraceString(szString, TraceLevel_Warning);
		}
		PlayerInfoItem *pServerHumanInfoItem = &this->HumanInfoItems[wHumanChairId];
		CopyMemory(pServerHumanInfoItem, pDataBuffer, sizeof(PlayerInfoItem) - sizeof(pClientHumanInfoItem->cbHP) - sizeof(pClientHumanInfoItem->cbIsValid));//去除cbHP和cbIsValid拷贝
		pServerHumanInfoItem->cbIsValid = 1;
		pDataBuffer += sizeof(PlayerInfoItem);
		//if (bIsLocked)
		//{
		//	//第一次获取到客户端的数据
		//}
		//else
		//{
		//	//只有在第一次接收客户端的HP数据，以后都只用服务端的HP数据

		//	//恢复成服务端原来的HP数据
		//	HumanInfoItem.cbHP = cbHPOld;
		//}

		// AI Items
		PlayerInfoItem *pClientAIInfoItem = NULL;
		PlayerInfoItem *pServerAIInfoItem = NULL;
		BYTE cbAIId = 0;
		for (int i = 0; i < wAIItemCount; i++)
		{
			pClientAIInfoItem = (PlayerInfoItem *)pDataBuffer;
			cbAIId = pClientAIInfoItem->cbAIId;

			if (cbAIId >= GAME_PLAYER)
			{
				//Log
				TCHAR szString[128] = TEXT("");
				_sntprintf(szString, CountArray(szString), TEXT("CMD_S_HideSeek_ClientPlayersInfo StreamValue error: cbAIId=%d"), cbAIId);
				CTraceService::TraceString(szString, TraceLevel_Warning);
			}

			PlayerInfoItem *pServerAIInfoItem = &this->AIInfoItems[cbAIId];
			CopyMemory(pServerAIInfoItem, pDataBuffer, sizeof(PlayerInfoItem) - sizeof(pClientAIInfoItem->cbHP) - sizeof(pClientAIInfoItem->cbIsValid));//去除cbHP和cbIsValid拷贝
			///AIInfoItems[i] = *((PlayerInfoItem*)pDataBuffer);
			pServerAIInfoItem->cbIsValid = 1;
			pDataBuffer += sizeof(PlayerInfoItem);
		}

		//for (int i = 0; i < wEventItemCount; i++)
		//{
		//	CopyMemory(&PlayerEventItems[i], pDataBuffer, sizeof(PlayerEventItem));
		//	///PlayerEventItems[i] = *((PlayerEventItem*)pDataBuffer);
		//	pDataBuffer += sizeof(PlayerEventItem);
		//}

		//AIInfoItems.clear();
		//for (int i = 0; i < wAIItemCount; i++)
		//{
		//	PlayerInfoItem pItem = *((PlayerInfoItem*)pDataBuffer);
		//	AIInfoItems.push_back(pItem);
		//	pDataBuffer += sizeof(PlayerInfoItem);
		//}

		// Event Items
		for (int i = 0; i < wEventItemCount; i++)
		{
			PlayerEventItem pItem = *((PlayerEventItem*)pDataBuffer);
			this->PlayerEventItems.push_back(pItem);
			pDataBuffer += sizeof(PlayerEventItem);
		}
	}
};
struct CMD_S_HideSeek_HeartBeat
{
	//WORD wPlayerItemCount;
	//WORD wEventItemCount;

	///PlayerInfoItem PlayerInfoItems[GAME_PLAYER];

	std::vector<PlayerInfoItem>	PlayerInfoItems;
	std::vector<PlayerEventItem> PlayerEventItems;

	void StreamValue(datastream& kData, bool bSend)
	{
		//Stream_VALUE(wPlayerItemCount);
		//Stream_VALUE(wEventItemCount);
		StructVecotrMember(PlayerInfoItem, PlayerInfoItems);
		StructVecotrMember(PlayerEventItem, PlayerEventItems);
	}
};
//AI分配信息
struct AICreateInfoItem
{
	BYTE cbTeamType;
	WORD wChairId;
	BYTE cbModelIdx;

	BYTE cbAIId;
};
struct CMD_GF_S_AICreateInfoItems
{
	WORD wItemCount;

	AICreateInfoItem InfoItems[GAME_PLAYER];
};

//出牌命令
struct CMD_C_OutCard
{
	BYTE							cbCardData;							//扑克数据
};

//操作命令
struct CMD_C_OperateCard
{
	BYTE							cbOperateCode;						//操作代码
	BYTE							cbOperateCard;						//操作扑克
};

//用户托管
struct CMD_C_Trustee
{
	bool							bTrustee;							//是否托管	
};

//起手小胡
struct CMD_C_XiaoHu
{
	BYTE							cbOperateCode;						//操作代码
	BYTE							cbOperateCard;						//操作扑克
};


//////////////////////////////////////////////////////////////////////////
#pragma pack()
#endif
