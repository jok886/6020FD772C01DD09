#include "StdAfx.h"
#include "PrivateTableInfo.h"

////////////////////////////////////////////////////////////////////////////////////////////////////////////
#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//构造函数
PrivateTableInfo::PrivateTableInfo()
	:pITableFrame(NULL)
{
	restValue();

	//mChen add, for HideSeek
	m_cbAgainHumanNum = 0;
	m_cbAgainLookonNum = 0;
}

PrivateTableInfo::~PrivateTableInfo(void)
{
	//关闭定时器
}
void PrivateTableInfo::restAgainValue()
{
	bStart = false;
	bInEnd = false;
	kDismissChairID.clear();
	kNotAgreeChairID.clear();
	fDismissPastTime = 0;
	dwFinishPlayCout = 0;
	dwStartPlayCout = 0;
	fAgainPastTime = 0.0f;
	ZeroMemory(lPlayerWinLose,sizeof(lPlayerWinLose));
	ZeroMemory(lPlayerAction,sizeof(lPlayerAction));
	kTotalRecord = tagPrivateRandTotalRecord();

	if (pITableFrame)
	{
		//ZY add
		kTotalRecord.kScore.resize(pITableFrame->GetChairCount());
		kTotalRecord.kUserID.resize(pITableFrame->GetChairCount());
		kTotalRecord.kNickName.resize(pITableFrame->GetChairCount());
		//kTotalRecord.kScore.resize(4);
		//kTotalRecord.kUserID.resize(4);
		//kTotalRecord.kNickName.resize(4);

		kTotalRecord.dwKindID = pITableFrame->GetGameServiceAttrib()->wKindID;
		kTotalRecord.dwVersion = pITableFrame->GetGameServiceAttrib()->dwClientVersion;
		for (int i = 0;i<(int)getChairCout();i++)
		{
			kTotalRecord.kScore[i] = 0;
		}
	}
}
void PrivateTableInfo::restValue()
{
	bStart = false;
	bInEnd = false;
	bPlayCoutIdex = 0;
	bGameTypeIdex = 0;
	bGameRuleIdex = 0;
	cbRoomType = Type_Private;
	dwPlayCout = 0;
	dwRoomNum = 0;
	dwCreaterUserID = 0;
	fDismissPastTime = 0;
	dwFinishPlayCout = 0;
	dwStartPlayCout = 0;
	dwPlayCost = 0;
	fAgainPastTime = 0.0f;
	kHttpChannel = "";
	//ZY add
	PlayerCount = 4;

	restAgainValue();
}
void PrivateTableInfo::newRandChild()
{
	WORD wSitUserCount = getSitUserCount();//getChairCout()
	tagPrivateRandRecordChild kRecordChild;
	kRecordChild.dwKindID = pITableFrame->GetGameServiceAttrib()->wKindID;
	kRecordChild.dwVersion = pITableFrame->GetGameServiceAttrib()->dwClientVersion;
	kRecordChild.kScore.resize(wSitUserCount);
	for (int i = 0;i<(int)wSitUserCount;i++)
	{
		IServerUserItem * pServerItem = pITableFrame->GetTableUserItem(i);
		if (pServerItem != NULL)
		{
			kTotalRecord.kNickName[i] = pServerItem->GetNickName();
			kTotalRecord.kUserID[i] = pServerItem->GetUserID();
		}

		kRecordChild.kScore[i] = 0;
	}
	GetLocalTime(&kRecordChild.kPlayTime);
	kTotalRecord.kRecordChild.push_back(kRecordChild);
}
WORD PrivateTableInfo::getChairCout()
{
	return pITableFrame->GetChairCount();
}

WORD PrivateTableInfo::getSitUserCount()
{
	return pITableFrame->GetSitUserCount();
}

void PrivateTableInfo::setRoomNum(DWORD RoomNum)
{
	kTotalRecord.iRoomNum = (int)RoomNum;
	dwRoomNum = RoomNum;
}
void PrivateTableInfo::writeSocre(tagScoreInfo ScoreInfoArray[], WORD wScoreCount,datastream& daUserDefine)
{
	if (kTotalRecord.kRecordChild.size() == 0)
	{
		ASSERT(false);
		return;
	}
	tagPrivateRandRecordChild& kRecord = kTotalRecord.kRecordChild.back();
	if (kRecord.kScore.size() < wScoreCount)
	{
		ASSERT(false);
		return;
	}
	for(WORD i = 0;i < wScoreCount;i++)
	{

		kRecord.kScore[i] += (int)ScoreInfoArray[i].lScore;
		kTotalRecord.kScore[i] += (int)ScoreInfoArray[i].lScore;
		lPlayerWinLose[i] += ScoreInfoArray[i].lScore;
	}

	GetLocalTime(&kRecord.kPlayTime);
	kRecord.kRecordGame = daUserDefine;
}