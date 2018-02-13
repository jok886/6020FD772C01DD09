#ifndef PRIVATE_TABLE_INFO_FILE
#define PRIVATE_TABLE_INFO_FILE

#pragma once

//引入文件
#include "CTableFramePrivate.h"
#include "PrivateServiceHead.h"

///////////////////////////////////////////////////////////////////////////////////////////

enum RoomType
{
	Type_Private=0,
	Type_Public,
};

enum CostType
{
	CostType_InsureScore = 0,
	CostType_Score,
};

class PrivateTableInfo 
{
public:
	PrivateTableInfo();
	~PrivateTableInfo();

	void restValue();
	void restAgainValue();
	void newRandChild();
	WORD getChairCout();
	WORD getSitUserCount();
	void setRoomNum(DWORD RoomNum);
	void writeSocre(tagScoreInfo ScoreInfoArray[], WORD wScoreCount,datastream& daUserDefine);

	ITableFrame*	pITableFrame;
	DWORD			dwCreaterUserID;
	DWORD			dwRoomNum;
	DWORD			dwPlayCout;
	DWORD			dwPlayCost;
	bool			bStart;
	bool			bInEnd;
	float			fAgainPastTime;
	std::string		kHttpChannel;

	BYTE			cbRoomType;

	DWORD			dwStartPlayCout;
	DWORD			dwFinishPlayCout;

	BYTE			bPlayCoutIdex;		//玩家局数
	BYTE			bGameTypeIdex;		//游戏类型
	DWORD			bGameRuleIdex;		//游戏规则

	//mChen add
	BYTE			cbPlayCostTypeIdex;	//支付类型：房主支付、平局支付
	SCORE			lBaseScore;
	//ZY add
	BYTE			PlayerCount;

	//mChen add, for HideSeek
	BYTE			m_cbAgainHumanNum;
	BYTE			m_cbAgainLookonNum;

	SCORE			lPlayerWinLose[MAX_CHAIR];
	BYTE			lPlayerAction[MAX_CHAIR][MAX_PRIVATE_ACTION];

	float			fDismissPastTime;
	std::vector<DWORD> kDismissChairID;
	std::vector<DWORD> kNotAgreeChairID;

	tagPrivateRandTotalRecord kTotalRecord;
};

#endif