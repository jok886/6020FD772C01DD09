#include "StdAfx.h"
#include "TableFrameSink.h"
#include "FvMask.h"
#include "..\..\..\公共组件\服务核心\WHDataLocker.h"

//////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////

#define IDI_TIMER_XIAO_HU			1 //小胡

#define TIME_XIAO_HU  8

//构造函数
CTableFrameSink::CTableFrameSink()
{
	//游戏变量
	m_wBankerUser = INVALID_CHAIR;
	ZeroMemory(m_cbCardIndex, sizeof(m_cbCardIndex));
	ZeroMemory(m_bTrustee, sizeof(m_bTrustee));
	ZeroMemory(m_lGameScore, sizeof(m_lGameScore));
	ZeroMemory(m_tGangResult, sizeof(m_tGangResult));
	//出牌信息
	m_cbOutCardData = 0;
	m_cbOutCardCount = 0;
	m_wOutCardUser = INVALID_CHAIR;
	ZeroMemory(m_cbDiscardCard, sizeof(m_cbDiscardCard));
	ZeroMemory(m_cbDiscardCount, sizeof(m_cbDiscardCount));

	//发牌信息
	m_cbSendCardData = 0;
	m_cbSendCardCount = 0;
	m_cbLeftCardCount = 0;
	ZeroMemory(m_cbRepertoryCard, sizeof(m_cbRepertoryCard));
	ZeroMemory(m_cbRepertoryCard_HZ, sizeof(m_cbRepertoryCard_HZ));

	//运行变量
	m_cbProvideCard = 0;
	m_wResumeUser = INVALID_CHAIR;
	m_wCurrentUser = INVALID_CHAIR;
	m_wProvideUser = INVALID_CHAIR;

	//状态变量
	m_bSendStatus = false;
	m_bGangStatus = false;
	m_bGangOutStatus = false;


	//用户状态
	ZeroMemory(m_bResponse, sizeof(m_bResponse));
	ZeroMemory(m_cbUserAction, sizeof(m_cbUserAction));
	ZeroMemory(m_cbOperateCard, sizeof(m_cbOperateCard));
	ZeroMemory(m_cbPerformAction, sizeof(m_cbPerformAction));

	//组合扑克
	ZeroMemory(m_WeaveItemArray, sizeof(m_WeaveItemArray));
	ZeroMemory(m_cbWeaveItemCount, sizeof(m_cbWeaveItemCount));

	//结束信息
	m_cbChiHuCard = 0;
	ZeroMemory(m_dwChiHuKind, sizeof(m_dwChiHuKind));
	for (WORD i = 0; i < GAME_PLAYER; i++)
	{
		m_ChiHuRight[i].SetEmpty();
	}
	memset(m_wProvider, INVALID_CHAIR, sizeof(m_wProvider));

	//组件变量
	m_pITableFrame = NULL;
	m_pGameServiceOption = NULL;

	//ZY add
	ZeroMemory(TotalScore_MJ, sizeof(TotalScore_MJ));
	m_bPlayCoutIdex = 0;
	playercount = 0;
	nowcount = 0;

	//重置数据
	ZeroMemory(m_bCanPiaoStatus, sizeof(m_bCanPiaoStatus));
	ZeroMemory(m_bPiaoStatus, sizeof(m_bPiaoStatus));
	ZeroMemory(m_cbPiaoCount, sizeof(m_cbPiaoCount));
	ZeroMemory(m_bGangPiao, sizeof(m_bGangPiao));
	ZeroMemory(m_bPiaoGang, sizeof(m_bPiaoGang));

	m_bOperateStatus = false;
	m_wPiaoChengBaoUser = INVALID_CHAIR;
	ZeroMemory(m_lPiaoScore, sizeof(m_lPiaoScore));
	ZeroMemory(m_lBasePiaoScore, sizeof(m_lBasePiaoScore));

	m_bBuGang = false;

	ZeroMemory(m_cbChiPengCount, sizeof(m_cbChiPengCount));
	for (WORD i = 0; i < GAME_PLAYER; i++)
	{
		for (BYTE j = 0; j < MAX_WEAVE; j++)
			m_wChiPengUser[i][j] = INVALID_CHAIR;
	}
	ZeroMemory(m_bQingYiSeChengBao, sizeof(m_bQingYiSeChengBao));

	m_cbChiCard = 0;

	ZeroMemory(m_bLouPengStatus, sizeof(m_bLouPengStatus));
	ZeroMemory(m_cbLouPengCard, sizeof(m_cbLouPengCard));

	m_wChiHuUser = INVALID_CHAIR;

	ZeroMemory(m_cbGangCount, sizeof(m_cbGangCount));


	//mChen add, test
	m_cbGameTypeIdex = GAME_TYPE_ZZ;
	FvMask::Add(m_dwGameRuleIdex, _MASK_((DWORD)GAME_TYPE_ZZ_HONGZHONG));
	m_lBaseScore = 1;

	//mChen add, for HideSeek
	//for (WORD wChairID = 0; wChairID < GAME_PLAYER; wChairID++)
	//{
	//	m_sClientsPlayersInfo[wChairID].Reset();
	//}
	m_sClientsPlayersInfos.Reset();
	//ZeroMemory(m_bIsHumanDead, sizeof(m_bIsHumanDead));
	//ZeroMemory(m_bIsAIDead, sizeof(m_bIsAIDead));
	ZeroMemory(m_cbAINum, sizeof(m_cbAINum));
	ZeroMemory(m_cbAICreateInfoItems, sizeof(m_cbAICreateInfoItems));
	ResetInventoryList();

	return;
}

//析构函数
CTableFrameSink::~CTableFrameSink(void)
{
}

//接口查询
void *CTableFrameSink::QueryInterface(const IID & Guid, DWORD dwQueryVer)
{
	QUERYINTERFACE(ITableFrameSink, Guid, dwQueryVer);
	QUERYINTERFACE(ITableUserAction, Guid, dwQueryVer);
	QUERYINTERFACE_IUNKNOWNEX(ITableFrameSink, Guid, dwQueryVer);
	return NULL;
}

//初始化
bool CTableFrameSink::Initialization(IUnknownEx *pIUnknownEx)
{
	//查询接口
	ASSERT(pIUnknownEx != NULL);
	m_pITableFrame = QUERY_OBJECT_PTR_INTERFACE(pIUnknownEx, ITableFrame);
	if (m_pITableFrame == NULL)
		return false;

	//获取参数
	m_pGameServiceOption = m_pITableFrame->GetGameServiceOption();
	ASSERT(m_pGameServiceOption != NULL);

	//mChen edit, for HideSeek:: 警察无敌时间之前加入的玩家，都分配阵营，只有在这之后加入的才是旁观者:fix不同步bug
	//开始模式
	m_pITableFrame->SetStartMode(START_MODE_TIME_CONTROL); 
	//m_pITableFrame->SetStartMode(START_MODE_FULL_READY); 

	return true;
}

//复位桌子
VOID CTableFrameSink::RepositionSink()
{
	//游戏变量
	ZeroMemory(m_cbCardIndex, sizeof(m_cbCardIndex));
	ZeroMemory(m_bTrustee, sizeof(m_bTrustee));

	ZeroMemory(m_lGameScore, sizeof(m_lGameScore));
	ZeroMemory(m_tGangResult, sizeof(m_tGangResult));
	//ZY add
	//ZeroMemory(TotalScore_MJ, sizeof(TotalScore_MJ));

	//出牌信息
	m_cbOutCardData = 0;
	m_cbOutCardCount = 0;
	m_wOutCardUser = INVALID_CHAIR;
	ZeroMemory(m_cbDiscardCard, sizeof(m_cbDiscardCard));
	ZeroMemory(m_cbDiscardCount, sizeof(m_cbDiscardCount));

	//发牌信息
	m_cbSendCardData = 0;
	m_cbSendCardCount = 0;
	m_cbLeftCardCount = 0;
	ZeroMemory(m_cbRepertoryCard, sizeof(m_cbRepertoryCard));
	ZeroMemory(m_cbRepertoryCard_HZ, sizeof(m_cbRepertoryCard_HZ));

	//运行变量
	m_cbProvideCard = 0;
	m_wResumeUser = INVALID_CHAIR;
	m_wCurrentUser = INVALID_CHAIR;
	m_wProvideUser = INVALID_CHAIR;

	//状态变量
	m_bSendStatus = false;
	m_bGangStatus = false;
	m_bGangOutStatus = false;


	//用户状态
	ZeroMemory(m_bResponse, sizeof(m_bResponse));
	ZeroMemory(m_cbUserAction, sizeof(m_cbUserAction));
	ZeroMemory(m_cbOperateCard, sizeof(m_cbOperateCard));
	ZeroMemory(m_cbPerformAction, sizeof(m_cbPerformAction));

	//组合扑克
	ZeroMemory(m_WeaveItemArray, sizeof(m_WeaveItemArray));
	ZeroMemory(m_cbWeaveItemCount, sizeof(m_cbWeaveItemCount));

	//结束信息
	m_cbChiHuCard = 0;
	ZeroMemory(m_dwChiHuKind, sizeof(m_dwChiHuKind));
	for (WORD i = 0; i < GAME_PLAYER; i++)
	{
		m_ChiHuRight[i].SetEmpty();
	}
	memset(m_wProvider, INVALID_CHAIR, sizeof(m_wProvider));



	//重置数据
	ZeroMemory(m_bCanPiaoStatus, sizeof(m_bCanPiaoStatus));
	ZeroMemory(m_bPiaoStatus, sizeof(m_bPiaoStatus));
	ZeroMemory(m_cbPiaoCount, sizeof(m_cbPiaoCount));
	ZeroMemory(m_bGangPiao, sizeof(m_bGangPiao));
	ZeroMemory(m_bPiaoGang, sizeof(m_bPiaoGang));

	m_bOperateStatus = false;
	m_wPiaoChengBaoUser = INVALID_CHAIR;
	ZeroMemory(m_lPiaoScore, sizeof(m_lPiaoScore));
	ZeroMemory(m_lBasePiaoScore, sizeof(m_lBasePiaoScore));

	m_bBuGang = false;

	ZeroMemory(m_cbChiPengCount, sizeof(m_cbChiPengCount));
	for (WORD i = 0; i < GAME_PLAYER; i++)
	{
		for (BYTE j = 0; j < MAX_WEAVE; j++)
			m_wChiPengUser[i][j] = INVALID_CHAIR;
	}
	ZeroMemory(m_bQingYiSeChengBao, sizeof(m_bQingYiSeChengBao));

	m_cbChiCard = 0;

	ZeroMemory(m_bLouPengStatus, sizeof(m_bLouPengStatus));
	ZeroMemory(m_cbLouPengCard, sizeof(m_cbLouPengCard));

	m_wChiHuUser = INVALID_CHAIR;

	ZeroMemory(m_cbGangCount, sizeof(m_cbGangCount));

	//mChen add, for HideSeek
	//for (WORD wChairID = 0; wChairID < GAME_PLAYER; wChairID++)
	//{
	//	m_sClientsPlayersInfo[wChairID].Reset();
	//}
	m_sClientsPlayersInfos.Reset();
	//ZeroMemory(m_bIsHumanDead, sizeof(m_bIsHumanDead));
	//ZeroMemory(m_bIsAIDead, sizeof(m_bIsAIDead));
	ZeroMemory(m_cbAINum, sizeof(m_cbAINum));
	ZeroMemory(m_cbAICreateInfoItems, sizeof(m_cbAICreateInfoItems));
	ZeroMemory(m_sStealth, sizeof(m_sStealth));

	return;
}

//游戏开始
bool CTableFrameSink::OnEventGameStart()
{
	return true;

	CTraceService::TraceString(TEXT("==========OnEventGameStart========="), TraceLevel_Normal);
	if (hasRule(GAME_TYPE_ZZ_HONGZHONG))
	{
		Shuffle(m_cbRepertoryCard_HZ, MAX_REPERTORY_HZ);
	}
	else
	{
		Shuffle(m_cbRepertoryCard, MAX_REPERTORY);
	}

	if (m_cbGameTypeIdex == GAME_TYPE_ZZ)
	{
		GameStart_ZZ();
	}
	//m_wBankerUser = 0;

	ZeroMemory(m_sStealth, sizeof(m_sStealth));
	return true;
}

void CTableFrameSink::Shuffle(BYTE *pRepertoryCard, int nCardCount)
{
	m_cbLeftCardCount = nCardCount;
	m_GameLogic.RandCardData(pRepertoryCard, nCardCount);


	//test
	/*BYTE byTest[] = {
	0x01,0x05,0x06,
	0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,
	0x01,0x02,0x03,0x04,0x05,0x08,


	0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x19,
	0x11,0x13,0x14,0x15,0x16,0x17,0x18,0x19,
	0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x19,
	0x11,0x12,0x15,0x16,0x17,0x18,0x19,0x34,
	0x18,0x34,0x33,0x33,
	
	0x23,0x23,0x23,0x32,
	0x21,0x24,0x05,0x18,

	0x12,0x31,0x14,0x25,0x22,0x24,0x26,0x07,0x08,0x09,0x28,



	0x01,0x21,0x22,0x26,0x27,0x28,0x29,0x29,0x24,0x25,0x26,0x27,0x29,0x21,0x25,0x37,

	0x27,0x31,0x32,0x13,0x33,0x25,0x35,0x36,0x21,0x22,0x23,0x24,0x26,0x27,0x33,0x32,

	0x07,0x06,0x36,0x31,0x32,0x34,0x35,0x36,0x31,0x34,0x35,0x36,0x35,0x37,0x22,0x29,

	0x01,0x01,0x01,0x02,0x03,0x04,0x37,0x37,0x06,0x07,0x08,0x09,0x09,0x09,0x28,0x28,
	};
	CopyMemory(pRepertoryCard, byTest, sizeof(byTest));*/
	//分发扑克
	for (WORD i = 0; i < GAME_PLAYER; i++)
	{
		if (m_pITableFrame->GetTableUserItem(i) != NULL)
		{
			m_cbLeftCardCount -= (MAX_COUNT - 1);
			m_GameLogic.SwitchToCardIndex(&pRepertoryCard[m_cbLeftCardCount], MAX_COUNT - 1, m_cbCardIndex[i]);
		}
	}
}

void CTableFrameSink::GameStart_ZZ()
{
	CTraceService::TraceString(TEXT("GameStart_ZZ"), TraceLevel_Normal);
	//mChen edit
	LONG lSiceCount = 0;
	if (m_wBankerUser == INVALID_CHAIR)
	{
		m_wBankerUser = 0;
	}
	else
	{
		////混乱扑克
		//LONG lSiceCount = MAKELONG(MAKEWORD(rand() % 6 + 1, rand() % 6 + 1), MAKEWORD(rand() % 6 + 1, rand() % 6 + 1));
		//m_wBankerUser = ((BYTE)(lSiceCount >> 24) + (BYTE)(lSiceCount >> 16) - 1) % GAME_PLAYER;
	}

	//设置变量
	m_cbProvideCard = 0;
	m_wProvideUser = INVALID_CHAIR;
	m_wCurrentUser = m_wBankerUser;

	//构造数据
	CMD_S_GameStart GameStart;
	ZeroMemory(&GameStart, sizeof(GameStart));
	GameStart.lSiceCount = lSiceCount;
	GameStart.wBankerUser = m_wBankerUser;
	GameStart.wCurrentUser = m_wCurrentUser;
	GameStart.cbLeftCardCount = m_cbLeftCardCount;

	m_pITableFrame->SetGameStatus(GAME_STATUS_PLAY);
	GameStart.cbXiaoHuTag = 0;
	ZeroMemory(GameStart.cbCardData, sizeof(GameStart.cbCardData));
	//发送数据
	for (WORD i = 0; i < GAME_PLAYER; i++)
	{
		//设置变量
		GameStart.cbUserAction = WIK_NULL;//m_cbUserAction[i];		
		m_GameLogic.SwitchToCardData(m_cbCardIndex[i], &GameStart.cbCardData[i*MAX_COUNT]);

		if (m_pITableFrame->GetTableUserItem(i)->IsAndroidUser())
		{
			BYTE bIndex = 1;
			for (WORD j = 0; j < GAME_PLAYER; j++)
			{
				if (j == i) continue;
				m_GameLogic.SwitchToCardData(m_cbCardIndex[j], &GameStart.cbCardData[MAX_COUNT * bIndex++]);
			}
		}		
	}
	//发送数据
	m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_GAME_START, &GameStart, sizeof(GameStart));
	//m_pITableFrame->SendLookonData(i, SUB_S_GAME_START, &GameStart, sizeof(GameStart));
	m_bSendStatus = true;
	DispatchCardData(m_wCurrentUser);
}


//游戏结束
bool CTableFrameSink::OnEventGameConclude(WORD wChairID, IServerUserItem *pIServerUserItem, BYTE cbReason)
{
	switch (cbReason)
	{
	case GER_NORMAL:		//常规结束
	{
		//变量定义
		CMD_S_GameEnd GameEnd;
		ZeroMemory(&GameEnd, sizeof(GameEnd));
		//GameEnd.wLeftUser = INVALID_CHAIR;

		//mChen add
		GameEnd.cbEndReason = cbReason;

		tagScoreInfo ScoreInfoArray[GAME_PLAYER];
		ZeroMemory(&ScoreInfoArray, sizeof(ScoreInfoArray));

		int	lGameTaxs[GAME_PLAYER];				//
		ZeroMemory(&lGameTaxs, sizeof(lGameTaxs));
		//统计积分
		for (WORD i = 0; i < GAME_PLAYER; i++)
		{
			if (NULL == m_pITableFrame->GetTableUserItem(i)) continue;

			//设置积分
			if (m_lGameScore[i] > 0L)
			{
				lGameTaxs[i] = m_pITableFrame->CalculateRevenue(i, m_lGameScore[i]);
				m_lGameScore[i] -= lGameTaxs[i];
			}

			BYTE ScoreKind;
			if (m_lGameScore[i] > 0L) ScoreKind = SCORE_TYPE_WIN;
			else if (m_lGameScore[i] < 0L) ScoreKind = SCORE_TYPE_LOSE;
			else ScoreKind = SCORE_TYPE_DRAW;

			ScoreInfoArray[i].lScore = m_lGameScore[i];
			ScoreInfoArray[i].lRevenue = lGameTaxs[i];
			ScoreInfoArray[i].cbType = ScoreKind;

			//ZY add
			TotalScore_MJ[i] += m_lGameScore[i];
		//	GameEnd.lTotalScore[i] = TotalScore_MJ[i];

		}

		//写入积分
		m_pITableFrame->WriteTableScore(ScoreInfoArray, CountArray(ScoreInfoArray));

		//发送结束信息
		m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_GAME_END, &GameEnd, sizeof(GameEnd));
		m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_GAME_END, &GameEnd, sizeof(GameEnd));

		//清空数据
		ZeroMemory(m_bCanPiaoStatus, sizeof(m_bCanPiaoStatus));
		ZeroMemory(m_bPiaoStatus, sizeof(m_bPiaoStatus));
		ZeroMemory(m_cbPiaoCount, sizeof(m_cbPiaoCount));
		ZeroMemory(m_bGangPiao, sizeof(m_bGangPiao));
		ZeroMemory(m_bPiaoGang, sizeof(m_bPiaoGang));

		m_bOperateStatus = false;
		m_wPiaoChengBaoUser = INVALID_CHAIR;
		ZeroMemory(m_lPiaoScore, sizeof(m_lPiaoScore));
		ZeroMemory(m_lBasePiaoScore, sizeof(m_lBasePiaoScore));

		m_bBuGang = false;

		ZeroMemory(m_cbChiPengCount, sizeof(m_cbChiPengCount));
		for (WORD i = 0; i < GAME_PLAYER; i++)
		{
			for (BYTE j = 0; j < MAX_WEAVE; j++)
				m_wChiPengUser[i][j] = INVALID_CHAIR;
		}
		ZeroMemory(m_bQingYiSeChengBao, sizeof(m_bQingYiSeChengBao));

		m_cbChiCard = 0;

		ZeroMemory(m_bLouPengStatus, sizeof(m_bLouPengStatus));
		ZeroMemory(m_cbLouPengCard, sizeof(m_cbLouPengCard));

		m_wChiHuUser = INVALID_CHAIR;

		ZeroMemory(m_cbGangCount, sizeof(m_cbGangCount));

		//ZY add
		nowcount++;
		if (nowcount == playercount) {
			nowcount = 0;
			playercount = 0;
			ZeroMemory(TotalScore_MJ, sizeof(TotalScore_MJ));
		}
		//结束游戏
		m_pITableFrame->ConcludeGame(GAME_STATUS_FREE,cbReason);

		return true;
	}
	case GER_DISMISS:		//游戏解散
	{
		//变量定义
		CMD_S_GameEnd GameEnd;
		ZeroMemory(&GameEnd, sizeof(GameEnd));
		//GameEnd.wLeftUser = INVALID_CHAIR;

		//mChen add
		GameEnd.cbEndReason = cbReason;
		//ZY add
		ZeroMemory(TotalScore_MJ, sizeof(TotalScore_MJ));
		nowcount = 0;
		playercount = 0;

		////设置变量
		//memset(GameEnd.wProvideUser, INVALID_CHAIR, sizeof(GameEnd.wProvideUser));

		////拷贝扑克
		//for (WORD i = 0; i < GAME_PLAYER; i++)
		//{
		//	GameEnd.cbCardCount[i] = m_GameLogic.SwitchToCardData(m_cbCardIndex[i], GameEnd.cbCardData[i]);
		//}

		//发送信息
		m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_GAME_END, &GameEnd, sizeof(GameEnd));
		m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_GAME_END, &GameEnd, sizeof(GameEnd));

		//结束游戏
		m_pITableFrame->ConcludeGame(GAME_STATUS_FREE,cbReason);

		return true;
	}
	case GER_NETWORK_ERROR:		//网络错误
	case GER_USER_LEAVE:		//用户强退
	{
		m_bTrustee[wChairID] = true;

		return true;
	}
	}

	//错误断言
	ASSERT(FALSE);
	return false;
}
void CTableFrameSink::ResetPlayerTotalScore() {
	CTraceService::TraceString(TEXT("ResetPlayerTotalScore"), TraceLevel_Normal);
	ZeroMemory(TotalScore_MJ, sizeof(TotalScore_MJ));
	nowcount = 0;
	playercount = 0;
}
//发送场景
bool CTableFrameSink::OnEventSendGameScene(WORD wChiarID, IServerUserItem *pIServerUserItem, BYTE cbGameStatus, bool bSendSecret)
{
	switch (cbGameStatus)
	{
	case GAME_STATUS_FREE:	//空闲状态
	case GAME_STATUS_WAIT:
	{
		//变量定义
		CMD_S_StatusFree StatusFree;
		memset(&StatusFree, 0, sizeof(StatusFree));
		StatusFree.cbGameStatus = cbGameStatus;
		StatusFree.cbMapIndex = m_pITableFrame->GetMapIndexRand();
		StatusFree.wRandseed = m_pITableFrame->GetRandSeed();
		StatusFree.wRandseedForRandomGameObject = m_pITableFrame->GetRandSeedForRandomGameObjec();
		StatusFree.wRandseedForInventory = m_pITableFrame->GetRandSeedForInventory();
		//道具同步
		CopyMemory(StatusFree.sInventoryList, m_sInventoryList, sizeof(m_sInventoryList));

		//构造数据
		//StatusFree.wBankerUser = m_wBankerUser;
		//StatusFree.lCellScore = m_lBaseScore;//m_pGameServiceOption->lCellScore
		//CopyMemory(StatusFree.bTrustee, m_bTrustee, sizeof(m_bTrustee));

		//发送场景
		return m_pITableFrame->SendGameScene(pIServerUserItem, &StatusFree, sizeof(StatusFree));
	}
	case GAME_STATUS_HIDE:
	case GAME_STATUS_PLAY:	//游戏状态
	{
		//旁观者或者重连

		//变量定义
		CMD_S_StatusPlay StatusPlay;
		memset(&StatusPlay, 0, sizeof(StatusPlay));
		StatusPlay.cbGameStatus = cbGameStatus;
		StatusPlay.cbMapIndex = m_pITableFrame->GetMapIndexRand();
		StatusPlay.wRandseed = m_pITableFrame->GetRandSeed();
		StatusPlay.wRandseedForRandomGameObject = m_pITableFrame->GetRandSeedForRandomGameObjec();
		StatusPlay.wRandseedForInventory = m_pITableFrame->GetRandSeedForInventory();
		//道具同步
		CopyMemory(StatusPlay.sInventoryList, m_sInventoryList, sizeof(m_sInventoryList));

		////Log:提示消息
		//TCHAR szString[512] = TEXT("");
		//_sntprintf(szString, CountArray(szString), TEXT("OnEventSendGameScene：未使用道具列表："));
		//CTraceService::TraceString(szString, TraceLevel_Normal);
		//for (int i = 0; i < MAX_INVENTORY_NUM; i++)
		//{
		//	if (m_sInventoryList[i].cbUsed == 0)
		//	{
		//		_sntprintf(szString, CountArray(szString), TEXT("道具：i=%d"), i);
		//		CTraceService::TraceString(szString, TraceLevel_Normal);
		//	}
		//}

		////游戏变量
		//StatusPlay.wBankerUser = m_wBankerUser;
		//StatusPlay.wCurrentUser = m_wCurrentUser;
		//StatusPlay.lCellScore = m_lBaseScore;//m_pGameServiceOption->lCellScore
		//CopyMemory(StatusPlay.bTrustee, m_bTrustee, sizeof(m_bTrustee));

		////状态变量
		//StatusPlay.cbActionCard = m_cbProvideCard;
		//StatusPlay.cbLeftCardCount = m_cbLeftCardCount;
		//StatusPlay.cbActionMask = (m_bResponse[wChiarID] == false) ? m_cbUserAction[wChiarID] : WIK_NULL;

		////历史记录
		//StatusPlay.wOutCardUser = m_wOutCardUser;
		//StatusPlay.cbOutCardData = m_cbOutCardData;
		//for (int i = 0; i < GAME_PLAYER; i++)
		//{
		//	CopyMemory(StatusPlay.cbDiscardCard[i], m_cbDiscardCard[i], sizeof(BYTE) * 60);
		//}
		//CopyMemory(StatusPlay.cbDiscardCount, m_cbDiscardCount, sizeof(StatusPlay.cbDiscardCount));

		////组合扑克
		//CopyMemory(StatusPlay.WeaveItemArray, m_WeaveItemArray, sizeof(m_WeaveItemArray));
		//CopyMemory(StatusPlay.cbWeaveCount, m_cbWeaveItemCount, sizeof(m_cbWeaveItemCount));
		//
		////扑克数据
		//for(int i = 0; i<GAME_PLAYER;i++)
		//{
		//	if (i == wChiarID)
		//		StatusPlay.cbCardCount = m_GameLogic.SwitchToCardData(m_cbCardIndex[i], &StatusPlay.cbCardData[i * MAX_COUNT]);
		//	else
		//		m_GameLogic.SwitchToCardData(m_cbCardIndex[i], &StatusPlay.cbCardData[i*MAX_COUNT]);
		//}
		////StatusPlay.cbCardCount = m_GameLogic.SwitchToCardData(m_cbCardIndex[wChiarID], &StatusPlay.cbCardData[wChiarID]);
		//StatusPlay.cbSendCardData = ((m_cbSendCardData != 0) && (m_wProvideUser == wChiarID)) ? m_cbSendCardData : 0x00;
		////ZY add
		//for (int i = 0; i < GAME_PLAYER; i++) {
		//	StatusPlay.TotalScore_MJ[i] = TotalScore_MJ[i];
		//}
		//
		//CopyMemory(&StatusPlay.tGangResult, &m_tGangResult[wChiarID], sizeof(StatusPlay.tGangResult));
	
		//发送场景
		bool bResult = m_pITableFrame->SendGameScene(pIServerUserItem, &StatusPlay, sizeof(StatusPlay));

		//mChen add, for HideSeek
		BYTE cbUserStatus = US_NULL;
		if (pIServerUserItem != NULL)
		{
			cbUserStatus = pIServerUserItem->GetUserStatus();
		}
		if (cbUserStatus == US_LOOKON) 
		{
			if (m_cbAINum[PlayerTeamType::TaggerTeam] + m_cbAINum[PlayerTeamType::HideTeam] > 0)
			{
				//GenerateAICreateInfo已经调用过

				HideSeek_SendAICreateInfo(pIServerUserItem);
			}

			//道具生成同步
			m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_GF_INVENTORY_CREATE, NULL, 0, MDM_GF_FRAME);
			m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_GF_INVENTORY_CREATE, NULL, 0, MDM_GF_FRAME);
		}
		else
		{
			//fix玩家在WAIT断线，错过IDI_HIDESEEK_END_WAIT_GAME时发送的HideSeek_SendAICreateInfo消息，导致没有AI

			if (m_cbAINum[PlayerTeamType::TaggerTeam] + m_cbAINum[PlayerTeamType::HideTeam] > 0)
			{
				//GenerateAICreateInfo已经调用过

				HideSeek_SendAICreateInfo(pIServerUserItem);
			}
		}

		return bResult;
	}
	}

	return false;
}

//定时器事件
bool CTableFrameSink::OnTimerMessage(DWORD wTimerID, WPARAM wBindParam)
{
	switch (wTimerID)
	{
		////mChen add, for HideSeek
		//case IDI_TIMER_TABLE_SINK_HIDESEEK_USERS_TICK:
		//{
		//	HideSeek_SendHeartBeat();
		//	return true;
		//}

		case IDI_TIMER_XIAO_HU:  //换牌结束
		{
			ZeroMemory(m_cbUserAction, sizeof(m_cbUserAction));
			m_pITableFrame->SetGameStatus(GAME_STATUS_PLAY);

			m_bSendStatus = true;
			DispatchCardData(m_wCurrentUser);
			return true;
		}
	}
	return false;
}

//游戏消息处理
bool CTableFrameSink::OnGameMessage(WORD wSubCmdID, VOID *pDataBuffer, WORD wDataSize, IServerUserItem *pIServerUserItem/*, IMainServiceFrame * pIMainServiceFrame*/)
{
	switch (wSubCmdID)
	{
		case SUB_C_CHAT_PLAY:
		{
			//验证消息
			ASSERT(wDataSize == sizeof(CMD_C_CHAT));
			if (wDataSize != sizeof(CMD_C_CHAT)) return false;
			CMD_C_CHAT *pData = (CMD_C_CHAT*)pDataBuffer;
			pData->ChairId = pIServerUserItem->GetChairID();

			//CMD_C_CHAT Chat;
			//ZeroMemory(&Chat, sizeof(Chat));
			//pIMainServiceFrame->SensitiveWordFilter(pData->ChatData, Chat.ChatData, CountArray(Chat.ChatData));
			//Chat.ChairId = pIServerUserItem->GetChairID();
			//Chat.UserStatus = Data->UserStatus;


			TCHAR szString1[512] = TEXT("");
			_sntprintf(szString1, CountArray(szString1), TEXT("文字数据ID=% d"), pData->ChairId);
			CTraceService::TraceString(szString1, TraceLevel_Normal);
			//转发给所有玩家
			m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_CHAT_PLAY, pData, sizeof(CMD_C_CHAT));
			m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_CHAT_PLAY, pData, sizeof(CMD_C_CHAT));
			return true;
		}
	//mChen add, for HideSeek
	case SUB_C_HIDESEEK_PLAYERS_INFO:
	{
		return HideSeek_OnClientPlayersInfo(pDataBuffer, wDataSize, pIServerUserItem);
	}

	case SUB_C_OUT_CARD:		//出牌消息
	{
		//效验消息
		ASSERT(wDataSize == sizeof(CMD_C_OutCard));
		if (wDataSize != sizeof(CMD_C_OutCard)) return false;

		//用户效验
		if (pIServerUserItem->GetUserStatus() != US_PLAYING) return true;

		//消息处理
		CMD_C_OutCard *pOutCard = (CMD_C_OutCard *)pDataBuffer;
		return OnUserOutCard(pIServerUserItem->GetChairID(), pOutCard->cbCardData);
	}
	case SUB_C_OPERATE_CARD:	//操作消息
	{
		//效验消息
		ASSERT(wDataSize == sizeof(CMD_C_OperateCard));
		if (wDataSize != sizeof(CMD_C_OperateCard)) return false;

		//用户效验
		if (pIServerUserItem->GetUserStatus() != US_PLAYING) return true;

		//消息处理
		CMD_C_OperateCard *pOperateCard = (CMD_C_OperateCard *)pDataBuffer;
		return OnUserOperateCard(pIServerUserItem->GetChairID(), pOperateCard->cbOperateCode, pOperateCard->cbOperateCard);
	}
	case SUB_C_TRUSTEE:
	{
		CMD_C_Trustee *pTrustee = (CMD_C_Trustee *)pDataBuffer;
		if (wDataSize != sizeof(CMD_C_Trustee)) return false;


		m_bTrustee[pIServerUserItem->GetChairID()] = pTrustee->bTrustee;
		CMD_S_Trustee Trustee;
		ZeroMemory(&Trustee, sizeof(Trustee));
		Trustee.bTrustee = pTrustee->bTrustee;
		Trustee.wChairID = pIServerUserItem->GetChairID();
		m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_TRUSTEE, &Trustee, sizeof(Trustee));
		m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_TRUSTEE, &Trustee, sizeof(Trustee));

		return true;
	}

	}

	return false;
}

//框架消息处理
bool CTableFrameSink::OnFrameMessage(WORD wSubCmdID, VOID *pDataBuffer, WORD wDataSize, IServerUserItem *pIServerUserItem)
{
	return false;
}

//用户坐下
bool CTableFrameSink::OnActionUserSitDown(WORD wChairID, IServerUserItem *pIServerUserItem, bool bLookonUser)
{
	return true;
}

//用户起来
bool CTableFrameSink::OnActionUserStandUp(WORD wChairID, IServerUserItem *pIServerUserItem, bool bLookonUser)
{
	//庄家设置
	if (bLookonUser == false)
	{
		m_bTrustee[wChairID] = false;
		CMD_S_Trustee Trustee;
		ZeroMemory(&Trustee, sizeof(Trustee));
		Trustee.bTrustee = false;
		Trustee.wChairID = wChairID;
		m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_TRUSTEE, &Trustee, sizeof(Trustee));
		m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_TRUSTEE, &Trustee, sizeof(Trustee));
	}

	return true;
}

//设置游戏规则
void CTableFrameSink::SetPrivateInfo(BYTE bGameTypeIdex, DWORD	bGameRuleIdex, SCORE lBaseScore, BYTE PlayCout, BYTE PlayerCount)
{
	ResetPlayerTotalScore();
	m_cbGameTypeIdex = bGameTypeIdex;
	m_dwGameRuleIdex = bGameRuleIdex;
	m_lBaseScore = lBaseScore;
	m_bPlayCoutIdex = PlayCout;
	/*TCHAR szString[512] = TEXT("");
	_sntprintf(szString, CountArray(szString), TEXT("总局数=%d"), PlayCout);
	CTraceService::TraceString(szString, TraceLevel_Normal);*/
	playercount = PlayCout;
}

//mChen add, for HideSeek
InventoryItem* CTableFrameSink::GetInventoryList()
{
	return m_sInventoryList;
}
void CTableFrameSink::ResetInventoryList()
{
	for (int i = 0; i < MAX_INVENTORY_NUM; i++)
	{
		m_sInventoryList[i].cbId = i;
		m_sInventoryList[i].cbType = (InventoryType)(rand() % (int)InventoryType::Inventory_Type_Num);
		m_sInventoryList[i].cbUsed = 0;
	}
}
void CTableFrameSink::SetResurrection(WORD wChairID)
{
	if (wChairID < GAME_PLAYER)
	{
		m_sClientsPlayersInfos.HumanInfoItems[wChairID].cbHP = 1;
	}
}

void CTableFrameSink::SetStealth(DWORD dwTime, WORD wChairID)
{
	if (wChairID < GAME_PLAYER)
	{
		m_sStealth[wChairID].cbStealTimeLeft = dwTime;
		m_sStealth[wChairID].cbStealStatus = 1;

		//CMD_S_InventoryConsumptionEvent InventoryConsumptionEvent;
		//ZeroMemory(&InventoryConsumptionEvent, sizeof(InventoryConsumptionEvent));
		//InventoryConsumptionEvent.cbChairID = wChairID;
		//InventoryConsumptionEvent.cbItemID = 1;  //隐身道具ID
		//InventoryConsumptionEvent.cbStealStatus = 1;
		//InventoryConsumptionEvent.cbStealTimeLeft = dwTime;
		//m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_CONSUMPTION_INVENTORY_EVENT, &InventoryConsumptionEvent, sizeof(InventoryConsumptionEvent));
		//m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_CONSUMPTION_INVENTORY_EVENT, &InventoryConsumptionEvent, sizeof(InventoryConsumptionEvent));
	}
}

void CTableFrameSink::StealthUpate()
{
	for (int i = 0; i < GAME_PLAYER; i++)
	{
		if (m_sStealth[i].cbStealStatus == 1 && m_sStealth[i].cbStealTimeLeft != 0)
		{
			m_sStealth[i].cbStealTimeLeft--;
			CMD_S_InventoryConsumptionEvent InventoryConsumptionEvent;
			ZeroMemory(&InventoryConsumptionEvent, sizeof(InventoryConsumptionEvent));
			InventoryConsumptionEvent.cbChairID = i;
			InventoryConsumptionEvent.cbItemID = 1;  //隐身道具ID
			InventoryConsumptionEvent.cbStealStatus = 1;
			InventoryConsumptionEvent.cbStealTimeLeft = m_sStealth[i].cbStealTimeLeft;
			if (m_sStealth[i].cbStealTimeLeft == 0)
			{
				InventoryConsumptionEvent.cbStealStatus = 0;
				m_sStealth[i].cbStealStatus == 0;
			}
			m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_CONSUMPTION_INVENTORY_EVENT, &InventoryConsumptionEvent, sizeof(InventoryConsumptionEvent));
			m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_CONSUMPTION_INVENTORY_EVENT, &InventoryConsumptionEvent, sizeof(InventoryConsumptionEvent));
		}
	}
}

BYTE CTableFrameSink::GetHumanHP(WORD wChairID)
{
	BYTE cbHP = 1;

	if (wChairID < GAME_PLAYER)
	{
		IServerUserItem * pIServerUserItem = m_pITableFrame->GetTableUserItem(wChairID);
		const PlayerInfoItem *pHumanInfoItem = &m_sClientsPlayersInfos.HumanInfoItems[wChairID];
		if (pIServerUserItem != NULL && pHumanInfoItem->cbIsValid)
		{
			cbHP = pHumanInfoItem->cbHP;
		}
	}

	return cbHP;
}
WORD CTableFrameSink::GetDeadHumanNumOfTeam(PlayerTeamType teamType)
{
	if (teamType >= PlayerTeamType::MaxTeamNum)
	{
		return 0;
	}

	WORD wDeadHumanNum = 0;
	for (WORD wChairID = 0; wChairID < GAME_PLAYER; wChairID++)
	{
		if (m_sClientsPlayersInfos.HumanInfoItems[wChairID].cbHP==0 && m_sClientsPlayersInfos.HumanInfoItems[wChairID].cbTeamType==teamType)
		{
			wDeadHumanNum++;
		}
	}

	return wDeadHumanNum;
}
WORD CTableFrameSink::GetDeadAINumOfTeam(PlayerTeamType teamType)
{
	if (teamType >= PlayerTeamType::MaxTeamNum)
	{
		return 0;
	}

	WORD wDeadNum = 0;
	for (BYTE cbAIId = 0; cbAIId < GAME_PLAYER; cbAIId++)
	{
		if(m_sClientsPlayersInfos.AIInfoItems[cbAIId].cbHP==0 && m_sClientsPlayersInfos.AIInfoItems[cbAIId].cbTeamType==teamType)
		{
			wDeadNum++;
		}
	}

	return wDeadNum;
}
bool CTableFrameSink::AreAllPlayersOfTeamDead(PlayerTeamType teamType)
{
	WORD wTableUserCountOfTeam = m_pITableFrame->GetSitUserCountOfTeam(teamType);
	WORD wDeadHumanNumOfTeam = GetDeadHumanNumOfTeam(teamType);
	bool bAreAllHumanOfTeamDead = (wDeadHumanNumOfTeam == wTableUserCountOfTeam);

	WORD wDeadAINumOfTeam = GetDeadAINumOfTeam(teamType);
	bool bAreAllAIsOfTeamDead = (wDeadAINumOfTeam == m_cbAINum[teamType]);
	
	bool bAreAllPlayersOfTeamDead = (bAreAllHumanOfTeamDead && bAreAllAIsOfTeamDead && (wDeadHumanNumOfTeam + wDeadAINumOfTeam) > 0);
	return bAreAllPlayersOfTeamDead;
}
bool CTableFrameSink::HideSeek_OnClientPlayersInfo(VOID *pDataBuffer, WORD wDataSize, IServerUserItem *pIServerUserItem)
{
	WORD wChairID = pIServerUserItem->GetChairID();
	if (wChairID >= GAME_PLAYER)
	{
		//Log
		TCHAR szString[256] = TEXT("");
		_sntprintf(szString, CountArray(szString), TEXT("HideSeek_OnClientPlayersInfo：incorrect wChairID=%d"), wChairID);
		CTraceService::TraceString(szString, TraceLevel_Warning);

		return true;
	}

	if (m_pITableFrame->GetGameStatus() != GAME_STATUS_HIDE && m_pITableFrame->GetGameStatus() != GAME_STATUS_PLAY)
	{
		return true;
	}

	CWHDataLocker DataLocker(m_CriticalSection);

	//m_sClientsPlayersInfo[wChairID].StreamValue((BYTE *)pDataBuffer, wDataSize);
	m_sClientsPlayersInfos.StreamValue((BYTE *)pDataBuffer, wDataSize);

	return true;
}
void CTableFrameSink::HideSeek_SendHeartBeat()
{
	if (m_pITableFrame->GetGameStatus() != GAME_STATUS_HIDE && m_pITableFrame->GetGameStatus() != GAME_STATUS_PLAY)  //GAME_STATUS_WAIT
	{
		return;
	}

	CMD_S_HideSeek_HeartBeat HeartBeatMsg;
	//ZeroMemory(&HeartBeatMsg, sizeof(HeartBeatMsg));

	CWHDataLocker DataLocker(m_CriticalSection);

	// Human Items
	const PlayerInfoItem *pHumanInfoItem = NULL;
	for (WORD wChairID = 0; wChairID < GAME_PLAYER; wChairID++)
	{
		pHumanInfoItem = &m_sClientsPlayersInfos.HumanInfoItems[wChairID];

		if (!pHumanInfoItem->cbIsValid)
		{
			continue;
		}

		HeartBeatMsg.PlayerInfoItems.push_back(*pHumanInfoItem);
	}

	// AI Items
	const PlayerInfoItem *pAIInfoItem = NULL;
	for (BYTE cbAIId = 0; cbAIId < GAME_PLAYER; cbAIId++)
	{
		pAIInfoItem = &m_sClientsPlayersInfos.AIInfoItems[cbAIId];

		if (!pAIInfoItem->cbIsValid)
		{
			continue;
		}

		HeartBeatMsg.PlayerInfoItems.push_back(*pAIInfoItem);
	}

	// Event Items
	int nEventSize = m_sClientsPlayersInfos.PlayerEventItems.size();
	if (nEventSize > 0)
	{
		const PlayerEventItem *pEventItem = NULL;
		for (int i = 0; i < nEventSize; i++)
		{
			pEventItem = &m_sClientsPlayersInfos.PlayerEventItems[i];
			HeartBeatMsg.PlayerEventItems.push_back(*pEventItem);

			// 解析事件
			BYTE cbTeamType = pEventItem->cbTeamType;
			WORD wChairId = pEventItem->wChairId;
			BYTE cbAIId = pEventItem->cbAIId;
			bool bIsHuman = (cbAIId == INVALID_AI_ID);
			if (wChairId >= GAME_PLAYER)
			{
				//Log
				TCHAR szString[128] = TEXT("");
				_sntprintf(szString, CountArray(szString), TEXT("HideSeek_SendHeartBeat: incorrect pEventItem->wChairId = %d"), wChairId);
				CTraceService::TraceString(szString, TraceLevel_Warning);

				continue;
			}

			PlayerInfoItem *pPlayerOfEvent = NULL;
			if (bIsHuman)
			{
				pPlayerOfEvent = &m_sClientsPlayersInfos.HumanInfoItems[wChairId];
			}
			else
			{
				if (cbAIId >= GAME_PLAYER)
				{
					//Log
					TCHAR szString[128] = TEXT("");
					_sntprintf(szString, CountArray(szString), TEXT("HideSeek_SendHeartBeat: incorrect pEventItem->cbAIId = %d"), cbAIId);
					CTraceService::TraceString(szString, TraceLevel_Warning);

					continue;
				}
				pPlayerOfEvent = &m_sClientsPlayersInfos.AIInfoItems[cbAIId];
			}

			switch (pEventItem->cbEventKind)
			{
				//死亡事件
				//fix AI断线期间,B将A击杀,A断线重连回来,发现自己还活着
			case PlayerEventKind::DeadByDecHp:
			{
				pPlayerOfEvent->cbHP = 0;
			}
			case PlayerEventKind::DeadByPicked:
			case PlayerEventKind::DeadByBoom:
			{
				pPlayerOfEvent->cbHP = 0;

				//Heal Killer,HP++

				// killer
				BYTE cbKillerTeamType = pEventItem->nCustomData0;
				WORD wKillerChairId = pEventItem->nCustomData1;
				BYTE cbKillerAIId = pEventItem->nCustomData2;
				bool bKillerIsHuman = (cbKillerAIId == INVALID_AI_ID);
				if (wKillerChairId >= GAME_PLAYER)
				{
					//Log
					TCHAR szString[128] = TEXT("");
					_sntprintf(szString, CountArray(szString), TEXT("HideSeek_SendHeartBeat: incorrect wKillerChairId = %d"), wKillerChairId);
					CTraceService::TraceString(szString, TraceLevel_Warning);

					break;
				}

				PlayerInfoItem *pKillerOfEvent = NULL;
				if (bKillerIsHuman)
				{
					pKillerOfEvent = &m_sClientsPlayersInfos.HumanInfoItems[wKillerChairId];
				}
				else
				{
					if (cbKillerAIId >= GAME_PLAYER)
					{
						//Log
						TCHAR szString[128] = TEXT("");
						_sntprintf(szString, CountArray(szString), TEXT("HideSeek_SendHeartBeat: incorrect cbKillerAIId = %d"), cbKillerAIId);
						CTraceService::TraceString(szString, TraceLevel_Warning);

						break;
					}
					pKillerOfEvent = &m_sClientsPlayersInfos.AIInfoItems[cbKillerAIId];
				}

				pKillerOfEvent->cbHP++;
				if (pKillerOfEvent->cbHP > MAX_PLAYER_HP)
				{
					pKillerOfEvent->cbHP = MAX_PLAYER_HP;
				}
			}
			break;

			//HP++
			//加血道具使用同步
			case PlayerEventKind::AddHp:
			{
				pPlayerOfEvent->cbHP++;
				if (pPlayerOfEvent->cbHP > MAX_PLAYER_HP)
				{
					pPlayerOfEvent->cbHP = MAX_PLAYER_HP;
				}
			}
			break;

			//HP--
			case PlayerEventKind::DecHp:
			{
				if (pPlayerOfEvent->cbHP > 0)
				{
					pPlayerOfEvent->cbHP--;
				}
			}
			break;

			//设置m_sInventoryList[i].cbUsed
			case PlayerEventKind::GetInventory:
			{
				int nInventoryId = (int)pEventItem->nCustomData0;
				InventoryType eInventoryType = (InventoryType)pEventItem->nCustomData1;
				for (int i = 0; i < MAX_INVENTORY_NUM; i++)
				{
					if (m_sInventoryList[i].cbId == nInventoryId)
					{
						m_sInventoryList[i].cbUsed = 1;

						//Log:提示消息
						TCHAR szString[128] = TEXT("");
						_sntprintf(szString, CountArray(szString), TEXT("User %d 使用了道具：Type=%d, Index=%d, Id=%d"), wChairId, eInventoryType, i, nInventoryId);
						CTraceService::TraceString(szString, TraceLevel_Normal);
					}
				}
			}
			break;
			}
		}

		//释放m_sClientsPlayersInfos.PlayerEventItems占用的空间
		std::vector<PlayerEventItem> tmpPlayerEventItems;
		tmpPlayerEventItems.swap(m_sClientsPlayersInfos.PlayerEventItems);
		//m_sClientsPlayersInfos.PlayerEventItems.clear();
	}

	datastream kDataStream;
	HeartBeatMsg.StreamValue(kDataStream, true);

	m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_HideSeek_HeartBeat, &kDataStream[0], kDataStream.size());
	m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_HideSeek_HeartBeat, &kDataStream[0], kDataStream.size());
}

/*
void CTableFrameSink::HideSeek_SendHeartBeat()
{
	if (m_pITableFrame->GetGameStatus() != GAME_STATUS_HIDE && m_pITableFrame->GetGameStatus() != GAME_STATUS_PLAY)  //GAME_STATUS_WAIT
	{
		return;
	}

	CMD_S_HideSeek_HeartBeat HeartBeatMsg;
	//ZeroMemory(&HeartBeatMsg, sizeof(HeartBeatMsg));

	CWHDataLocker DataLocker(m_CriticalSection);

	for (WORD wChairID = 0; wChairID < GAME_PLAYER; wChairID++)
	{
		CMD_C_HideSeek_ClientPlayersInfo *pClientsPlayersInfo = &m_sClientsPlayersInfo[wChairID];

		if (!pClientsPlayersInfo->bIsValid)
		{
			continue;
		}

		//pClientsPlayersInfo->bIsValid = false;

		// Event Items
		int nEventSize = pClientsPlayersInfo->PlayerEventItems.size();
		if (nEventSize > 0)
		{
			//解析事件
			for (int i = 0; i < nEventSize; i++)
			{
				//解析事件,设置m_bIsHumanDead,m_bIsAIDead
				//fix AI断线期间,B将A击杀,A断线重连回来,发现自己还活着
				const PlayerEventItem *pEventItem = &pClientsPlayersInfo->PlayerEventItems[i];
				if (pEventItem->cbEventKind == PlayerEventKind::DeadByDecHp || pEventItem->cbEventKind == PlayerEventKind::DeadByPicked || pEventItem->cbEventKind == PlayerEventKind::DeadByBoom)
				{
					if (pEventItem->wChairId < GAME_PLAYER && pEventItem->cbTeamType < PlayerTeamType::MaxTeamNum)
					{
						if (pEventItem->cbAIId == INVALID_AI_ID)
						{
							m_bIsHumanDead[pEventItem->cbTeamType][pEventItem->wChairId] = true;
						}
						else
						{
							m_bIsAIDead[pEventItem->cbTeamType][pEventItem->cbAIId] = true;
						}
					}

					//解析事件,HP++
					//Heal Killer
					if (pEventItem->cbEventKind == PlayerEventKind::DeadByPicked || pEventItem->cbEventKind == PlayerEventKind::DeadByBoom)
					{
						// killer
						BYTE cbTeamType = pEventItem->nCustomData0;
						WORD wChairId = pEventItem->nCustomData1;
						BYTE cbAIId = pEventItem->nCustomData2;

						if (wChairId < GAME_PLAYER && cbTeamType < PlayerTeamType::MaxTeamNum)
						{
							if (cbAIId == INVALID_AI_ID)
							{
								m_sClientsPlayersInfo[wChairId].HumanInfoItem.cbHP++;
								if (m_sClientsPlayersInfo[wChairId].HumanInfoItem.cbHP > 4)
								{
									m_sClientsPlayersInfo[wChairId].HumanInfoItem.cbHP = 4;
								}
							}
							else
							{
								// AI
								for (int j = 0; j < m_sClientsPlayersInfo[wChairId].wAIItemCount; j++)
								{
									PlayerInfoItem *pAIInfoItem = &m_sClientsPlayersInfo[wChairId].AIInfoItems[j];
									if (pAIInfoItem->cbAIId == cbAIId)
									{
										pAIInfoItem->cbHP++;
										if (pAIInfoItem->cbHP > 4)
										{
											pAIInfoItem->cbHP = 4;
										}
									}
								}
							}
						}
					}
				}

				//解析事件,HP++ 
				//加血道具使用同步
				if (pEventItem->cbEventKind == PlayerEventKind::AddHp)
				{
					BYTE cbTeamType = pEventItem->cbTeamType;
					WORD wChairId = pEventItem->wChairId;
					BYTE cbAIId = pEventItem->cbAIId;

					if (wChairId < GAME_PLAYER && cbTeamType < PlayerTeamType::MaxTeamNum)
					{
						if (cbAIId == INVALID_AI_ID)
						{
							m_sClientsPlayersInfo[wChairId].HumanInfoItem.cbHP++;
							if (m_sClientsPlayersInfo[wChairId].HumanInfoItem.cbHP > 4)
							{
								m_sClientsPlayersInfo[wChairId].HumanInfoItem.cbHP = 4;
							}
						}
						else
						{
							// AI
							for (int j = 0; j < m_sClientsPlayersInfo[wChairId].wAIItemCount; j++)
							{
								PlayerInfoItem *pAIInfoItem = &m_sClientsPlayersInfo[wChairId].AIInfoItems[j];
								if (pAIInfoItem->cbAIId == cbAIId)
								{
									pAIInfoItem->cbHP++;
									if (pAIInfoItem->cbHP > 4)
									{
										pAIInfoItem->cbHP = 4;;
									}
								}
							}
						}
					}
				}

				//解析事件,HP--
				if (pEventItem->cbEventKind == PlayerEventKind::DecHp)
				{
					BYTE cbTeamType = pEventItem->cbTeamType;
					WORD wChairId = pEventItem->wChairId;
					BYTE cbAIId = pEventItem->cbAIId;

					if (wChairId < GAME_PLAYER && cbTeamType < PlayerTeamType::MaxTeamNum)
					{
						if (cbAIId == INVALID_AI_ID)
						{
							if (m_sClientsPlayersInfo[wChairId].HumanInfoItem.cbHP > 0)
							{
								m_sClientsPlayersInfo[wChairId].HumanInfoItem.cbHP--;
							}
						}
						else
						{
							// AI
							for (int j = 0; j < m_sClientsPlayersInfo[wChairId].wAIItemCount; j++)
							{
								PlayerInfoItem *pAIInfoItem = &m_sClientsPlayersInfo[wChairId].AIInfoItems[j];
								if (pAIInfoItem->cbAIId == cbAIId)
								{
									if (pAIInfoItem->cbHP > 0) 
									{
										pAIInfoItem->cbHP--;
									}
								}
							}
						}
					}
				}

				//解析事件,设置m_sInventoryList
				if (pEventItem->cbEventKind == PlayerEventKind::GetInventory)
				{
					int nInventoryId = (int)pEventItem->nCustomData0;
					for (int i = 0; i < MAX_INVENTORY_NUM; i++)
					{
						if (m_sInventoryList[i].cbId == nInventoryId)
						{
							m_sInventoryList[i].cbUsed = 1;

							//Log:提示消息
							TCHAR szString[512] = TEXT("");
							_sntprintf(szString, CountArray(szString), TEXT("使用了道具：InventoryIndex=%d, Id=%d"), i, nInventoryId);
							CTraceService::TraceString(szString, TraceLevel_Normal);
						}
					}
				}

				HeartBeatMsg.PlayerEventItems.push_back(*pEventItem);
			}

			//释放pClientsPlayersInfo->PlayerEventItems占用的空间
			std::vector<PlayerEventItem> tmpPlayerEventItems;
			tmpPlayerEventItems.swap(pClientsPlayersInfo->PlayerEventItems);
			//pClientsPlayersInfo->PlayerEventItems.clear();
		}

		// Human Item
		{
			//设置m_bIsHumanDead
			PlayerInfoItem *pHumanInfoItem = &pClientsPlayersInfo->HumanInfoItem;
			if (pHumanInfoItem->cbHP == 0 && pHumanInfoItem->wChairId < GAME_PLAYER)
			{
				m_bIsHumanDead[pHumanInfoItem->cbTeamType][pHumanInfoItem->wChairId] = true;
			}

			// fix 自己死了后断线，或者自己断线后死了，重连后又复活的bug
			if (m_bIsHumanDead[pHumanInfoItem->cbTeamType][pHumanInfoItem->wChairId] && pHumanInfoItem->cbHP > 0)
			{
				// 已经在m_bIsHumanDead中标记为死亡，但收到的数据cbHP>0

				pHumanInfoItem->cbHP = 0;
			}

			HeartBeatMsg.PlayerInfoItems.push_back(pClientsPlayersInfo->HumanInfoItem);
		}

		// AI Items
		for (WORD i = 0; i < pClientsPlayersInfo->wAIItemCount; i++)
		{
			//设置m_bIsAIDead
			PlayerInfoItem *pAIInfoItem = &pClientsPlayersInfo->AIInfoItems[i];
			if (pAIInfoItem->cbHP == 0 && pAIInfoItem->cbAIId < GAME_PLAYER)
			{
				m_bIsAIDead[pAIInfoItem->cbTeamType][pAIInfoItem->cbAIId] = true;
			}

			// fix 自己AI死了后断线，或者自己断线后自己的AI被别人点死了，重连后又复活的bug
			if (pAIInfoItem->cbAIId < GAME_PLAYER)
			{
				if (m_bIsAIDead[pAIInfoItem->cbTeamType][pAIInfoItem->cbAIId] && pAIInfoItem->cbHP > 0)
				{
					// 已经在m_bIsAIDead中标记为死亡，但收到的数据cbHP>0

					pAIInfoItem->cbHP = 0;
				}
			}

			HeartBeatMsg.PlayerInfoItems.push_back(pClientsPlayersInfo->AIInfoItems[i]);
		}
		pClientsPlayersInfo->wAIItemCount = 0;
	}

	datastream kDataStream;
	HeartBeatMsg.StreamValue(kDataStream, true);

	m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_HideSeek_HeartBeat, &kDataStream[0], kDataStream.size());
	m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_HideSeek_HeartBeat, &kDataStream[0], kDataStream.size());
}
*/
void CTableFrameSink::GenerateAICreateInfo()
{
	//ZeroMemory(m_cbAICreateInfoItems, sizeof(m_cbAICreateInfoItems));

	WORD wTotalAICount = 0;
	for (PlayerTeamType teamType = PlayerTeamType::TaggerTeam; teamType < PlayerTeamType::MaxTeamNum; teamType = (PlayerTeamType)(teamType + 1))
	{
		//计算AI人数
		WORD wMinUserNumOfTeam = 0;
		if (teamType == PlayerTeamType::TaggerTeam)
		{
			wMinUserNumOfTeam = 3;
		}
		else
		{
			wMinUserNumOfTeam = 7;
		}
		//wMinUserNumOfTeam = 0;

		WORD wTableUserCountOfTeam = m_pITableFrame->GetSitUserCountOfTeam(teamType);
		WORD wAINumOfTeam = 0;
		if (wTableUserCountOfTeam < wMinUserNumOfTeam)
		{
			//人数不足
			wAINumOfTeam = wMinUserNumOfTeam - wTableUserCountOfTeam;
		}
		m_cbAINum[teamType] = wAINumOfTeam;

		//分配AI
		WORD wTableUserCount = m_pITableFrame->GetSitUserCount();
		WORD wCreatedAINumOfTeam = 0;
		if (wAINumOfTeam > 0)
		{
			for (WORD wChairID = 0; wChairID < GAME_PLAYER; wChairID++)
			{
				IServerUserItem * pIServerUserItem = m_pITableFrame->GetTableUserItem(wChairID);
				if (pIServerUserItem != NULL)
				{
					WORD wAINumOfUser = wAINumOfTeam / wTableUserCount;
					WORD wAINumLeft = wAINumOfTeam % wTableUserCount;

					if (wChairID < wAINumLeft)
					{
						wAINumOfUser++;
					}

					for (WORD wAIIdx = 0; wAIIdx < wAINumOfUser; wAIIdx++)
					{
						tagUserInfo * pCueUserInfo = pIServerUserItem->GetUserInfo();

						BYTE cbAIIdxOfUser = m_cbAICreateInfoItems[wChairID].cbAINum;
						AICreateInfoItem* pInfoItem = &m_cbAICreateInfoItems[wChairID].cbAICreateInfoItem[cbAIIdxOfUser];
						pInfoItem->cbTeamType = teamType;
						pInfoItem->wChairId = pIServerUserItem->GetChairID();
						pInfoItem->cbModelIdx = rand() % 255;
						pInfoItem->cbAIId = wTotalAICount;

						m_cbAICreateInfoItems[wChairID].cbAINum++;
						wTotalAICount++;
						wCreatedAINumOfTeam++;
					}

					if (wCreatedAINumOfTeam >= wAINumOfTeam)
					{
						break;
					}
				}
			}
		}
	}
}
void CTableFrameSink::HideSeek_SendAICreateInfo(IServerUserItem *pIServerUserItem, bool bOnlySendToLookonUser)
{
	CMD_GF_S_AICreateInfoItems AICreateInfoItems;
	ZeroMemory(&AICreateInfoItems, sizeof(AICreateInfoItems));

	for (WORD wChairID = 0; wChairID < GAME_PLAYER; wChairID++)
	{
		for (WORD wAIIdxOfUser = 0; wAIIdxOfUser < m_cbAICreateInfoItems[wChairID].cbAINum; wAIIdxOfUser++)
		{
			AICreateInfoItems.InfoItems[AICreateInfoItems.wItemCount] = m_cbAICreateInfoItems[wChairID].cbAICreateInfoItem[wAIIdxOfUser];
			AICreateInfoItems.wItemCount++;
		}
	}

	//发送消息
	DWORD wHeadSize = sizeof(AICreateInfoItems) - sizeof(AICreateInfoItems.InfoItems);
	DWORD wItemDataSize = sizeof(AICreateInfoItem)*AICreateInfoItems.wItemCount;
	DWORD wDataSize = wHeadSize + wItemDataSize;

	//Log:提示消息
	TCHAR szString[512] = TEXT("");
	_sntprintf(szString, CountArray(szString), TEXT("HideSeek_SendAICreateInfo:wItemCount=%d, wDataSize=%d"), AICreateInfoItems.wItemCount, wDataSize);
	CTraceService::TraceString(szString, TraceLevel_Normal);

	if (pIServerUserItem != NULL)
	{
		m_pITableFrame->SendUserItemData(pIServerUserItem, SUB_S_HideSeek_AICreateInfo, &AICreateInfoItems, wDataSize);
	}
	else
	{
		if (bOnlySendToLookonUser)
		{
			m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_HideSeek_AICreateInfo, &AICreateInfoItems, wDataSize);
		}
		else
		{
			m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_HideSeek_AICreateInfo, &AICreateInfoItems, wDataSize);
			m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_HideSeek_AICreateInfo, &AICreateInfoItems, wDataSize);
		}
	}
}

//用户出牌
bool CTableFrameSink::OnUserOutCard(WORD wChairID, BYTE cbCardData)
{
	//效验状态
	ASSERT(m_pITableFrame->GetGameStatus() == GAME_STATUS_PLAY);
	if (m_pITableFrame->GetGameStatus() != GAME_STATUS_PLAY) return true;

	//错误断言
	ASSERT(wChairID == m_wCurrentUser);
	ASSERT(m_GameLogic.IsValidCard(cbCardData) == true);

	//效验参数
	if (wChairID != m_wCurrentUser) return true;
	if (m_GameLogic.IsValidCard(cbCardData) == false) return true;

	if (cbCardData == m_cbChiCard)
	{

	}
	else
	{
		//删除扑克
		if (m_GameLogic.RemoveCard(m_cbCardIndex[wChairID], cbCardData) == false)
		{
			ASSERT(FALSE);
			return true;
		}

		//设置变量
		m_bSendStatus = true;
		
		m_cbUserAction[wChairID] = WIK_NULL;
		m_cbPerformAction[wChairID] = WIK_NULL;

		//出牌记录
		m_cbOutCardCount++;
		m_wOutCardUser = wChairID;
		m_cbOutCardData = cbCardData;



		//检测是否可以飘财
		bool bBaoTou = false;
		bool bCanPiao = false;
		m_GameLogic.IsBaDui(m_cbCardIndex[wChairID], m_WeaveItemArray[wChairID], m_cbWeaveItemCount[wChairID], m_cbOutCardData, bBaoTou, bCanPiao);

		if (m_GameLogic.CanPiaoCai(m_cbCardIndex[wChairID], m_cbOutCardData))
			m_bCanPiaoStatus[wChairID] = true;
		else if (bCanPiao)
			m_bCanPiaoStatus[wChairID] = true;
		else
			m_bCanPiaoStatus[wChairID] = false;


		//如果打出万能牌，检测是否是飘财
		if (m_GameLogic.IsMagicCard(cbCardData))
		{
			if (!m_bPiaoStatus[wChairID] && m_bGangStatus)
				m_bGangPiao[wChairID] = true;


			m_bPiaoStatus[wChairID] = true;

			//m_bCanPiaoStatus[wChairID]为true才算真飘，飘财只算m_cbPiaoCount[wChairID]的数目
			if (m_bCanPiaoStatus[wChairID])
				m_cbPiaoCount[wChairID]++;
			else
				m_cbPiaoCount[wChairID] = 0;
			

			//这里开始计算是否飘财承包
			if (m_cbPiaoCount[wChairID] == 1)
				m_lBasePiaoScore[wChairID] = pow((LONG)2, (LONG)(m_cbPiaoCount[wChairID] + 1 + m_cbGangCount[wChairID]));

			if (m_cbPiaoCount[wChairID] >= 2 && m_bOperateStatus)
			{
				//因为吃碰杠（被动）而二次飘财
				SCORE lPiaoScore = pow((LONG)2, (LONG)(m_cbPiaoCount[wChairID] + 1 + m_cbGangCount[wChairID]));
				if (lPiaoScore > 20)
					lPiaoScore = 20;
				m_lPiaoScore[wChairID][m_wPiaoChengBaoUser] += lPiaoScore * 3 / 2;
			}
		}
		else
		{
			m_bPiaoStatus[wChairID] = false;
			m_cbPiaoCount[wChairID] = 0;
			ZeroMemory(m_lPiaoScore[wChairID], sizeof(m_lPiaoScore[wChairID]));
			m_lBasePiaoScore[wChairID] = 0;

			m_bGangPiao[wChairID] = false;						//如果不飘财，则释放这个状态，不算杠飘
			m_bPiaoGang[wChairID] = false;						//如果补飘财，则释放这个状态，不算飘杠

			m_cbGangCount[wChairID] = 0;						//如果不是打出财神，就清空杠数统计
		}

		m_bOperateStatus = false;								//操作状态，出牌后重置
		m_wPiaoChengBaoUser = INVALID_CHAIR;

		m_bBuGang = false;

		m_cbChiCard = 0;										//重置吃牌

		if (m_bGangStatus)
		{
			m_bGangStatus = false;
			m_bGangOutStatus = true;
		}

		//过了自己就清空漏碰记录
		m_bLouPengStatus[wChairID] = false;
		m_cbLouPengCard[wChairID] = 0;



		//构造数据
		CMD_S_OutCard OutCard;
		ZeroMemory(&OutCard, sizeof(OutCard));
		OutCard.wOutCardUser = wChairID;
		OutCard.cbOutCardData = cbCardData;
		OutCard.bIsPiao = m_bPiaoStatus[wChairID];

		//发送消息
		m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_OUT_CARD, &OutCard, sizeof(OutCard));
		m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_OUT_CARD, &OutCard, sizeof(OutCard));

		m_wProvideUser = wChairID;
		m_cbProvideCard = cbCardData;

		//用户切换
		m_wCurrentUser = (wChairID + GAME_PLAYER - 1) % GAME_PLAYER;

		//响应判断
		bool bAroseAction = EstimateUserRespond(wChairID, cbCardData, EstimatKind_OutCard);

		//丢弃扑克(提前加入丢弃牌堆，后面操作时要重新拿出来)
		if ((m_wOutCardUser != INVALID_CHAIR) && (m_cbOutCardData != 0))
		{
			m_cbDiscardCount[m_wOutCardUser]++;
			m_cbDiscardCard[m_wOutCardUser][m_cbDiscardCount[m_wOutCardUser] - 1] = m_cbOutCardData;
		}

		//派发扑克
		if (bAroseAction == false) DispatchCardData(m_wCurrentUser);

		return true;
	}
}

//用户操作
bool CTableFrameSink::OnUserOperateCard(WORD wChairID, BYTE cbOperateCode, BYTE cbOperateCard)
{
	//效验状态
	ASSERT(m_pITableFrame->GetGameStatus() != GAME_STATUS_FREE);
	if (m_pITableFrame->GetGameStatus() == GAME_STATUS_FREE)
		return true;

	//效验用户	注意：机器人有可能发生此断言
	//ASSERT((wChairID==m_wCurrentUser)||(m_wCurrentUser==INVALID_CHAIR));
	if ((wChairID != m_wCurrentUser) && (m_wCurrentUser != INVALID_CHAIR))
		return true;

	//被动动作
	if (m_wCurrentUser == INVALID_CHAIR)
	{
		//效验状态
		if (m_bResponse[wChairID] == true)
			return true;
		if ((cbOperateCode != WIK_NULL) && ((m_cbUserAction[wChairID] & cbOperateCode) == 0))
			return true;

		//变量定义
		WORD wTargetUser = wChairID;
		BYTE cbTargetAction = cbOperateCode;

		//设置变量
		m_bResponse[wChairID] = true;
		m_cbPerformAction[wChairID] = cbOperateCode;
		m_cbOperateCard[wChairID] = m_cbProvideCard;

		//漏碰记录
		if (cbOperateCode == WIK_NULL && (m_cbUserAction[wChairID] & WIK_PENG) != 0)
		{
			//客户端对于碰选项，选择了过
			m_bLouPengStatus[wChairID] = true;
			m_cbLouPengCard[wChairID] = m_cbProvideCard;
		}

		//执行判断
		for (WORD i = 0; i < GAME_PLAYER; i++)
		{
			//获取动作
			BYTE cbUserAction = (m_bResponse[i] == false) ? m_cbUserAction[i] : m_cbPerformAction[i];

			//优先级别
			BYTE cbUserActionRank = m_GameLogic.GetUserActionRank(cbUserAction);
			BYTE cbTargetActionRank = m_GameLogic.GetUserActionRank(cbTargetAction);

			//动作判断
			if (cbUserActionRank > cbTargetActionRank)
			{
				wTargetUser = i;
				cbTargetAction = cbUserAction;
			}
		}
		if (m_bResponse[wTargetUser] == false)
			return true;

		//吃胡等待
		if (cbTargetAction == WIK_CHI_HU)
		{
			for (WORD i = 0; i < GAME_PLAYER; i++)
			{
				if ((m_bResponse[i] == false) && (m_cbUserAction[i] & WIK_CHI_HU))
					return true;
			}
		}

		//放弃操作
		if (cbTargetAction == WIK_NULL)
		{
			//用户状态
			ZeroMemory(m_bResponse, sizeof(m_bResponse));
			ZeroMemory(m_cbUserAction, sizeof(m_cbUserAction));
			ZeroMemory(m_cbOperateCard, sizeof(m_cbOperateCard));
			ZeroMemory(m_cbPerformAction, sizeof(m_cbPerformAction));

			//发送扑克
			DispatchCardData(m_wResumeUser);

			return true;
		}

		//丢弃扑克(操作时要重新拿出来)
		m_cbDiscardCount[m_wProvideUser]--;
		m_cbDiscardCard[m_wProvideUser][m_cbDiscardCount[m_wProvideUser]] = 0;

		//变量定义
		BYTE cbTargetCard = m_cbOperateCard[wTargetUser];

		//出牌变量
		m_cbOutCardData = 0;
		m_bSendStatus = true;
		m_wOutCardUser = INVALID_CHAIR;

		//胡牌操作
		if (cbTargetAction == WIK_CHI_HU)
		{
			//结束信息
			m_cbChiHuCard = cbTargetCard;

			for (WORD i = (m_wProvideUser + GAME_PLAYER - 1) % GAME_PLAYER; i != m_wProvideUser; i = (i + GAME_PLAYER - 1) % GAME_PLAYER)
			{
				//过虑判断
				if ((m_cbPerformAction[i] & WIK_CHI_HU) == 0)
					continue;

				//胡牌判断,赋值权位
				BYTE cbWeaveItemCount = m_cbWeaveItemCount[i];
				tagWeaveItem *pWeaveItem = m_WeaveItemArray[i];
				m_dwChiHuKind[i] = AnalyseChiHuCard(m_cbCardIndex[i], pWeaveItem, cbWeaveItemCount, m_cbChiHuCard, m_ChiHuRight[i]);

				//插入扑克
				if (m_dwChiHuKind[i] != WIK_NULL)
				{
					m_cbCardIndex[i][m_GameLogic.SwitchToCardIndex(m_cbChiHuCard)]++;
					ProcessChiHuUser(i, false);

					//if ((m_ChiHuRight[i] & CHR_QIANG_GANG).IsEmpty())
					//{
					m_wBankerUser = i;	//胡牌方为下一局庄家
					//}
					//else
					//{
					//	m_wBankerUser = m_wProvideUser; //抢杠胡，被抢杠者为下一局庄家
					//}
				}
			}

			OnEventGameConclude(INVALID_CHAIR, NULL, GER_NORMAL);

			return true;
		}

		//用户状态
		ZeroMemory(m_bResponse, sizeof(m_bResponse));
		ZeroMemory(m_cbUserAction, sizeof(m_cbUserAction));
		ZeroMemory(m_cbOperateCard, sizeof(m_cbOperateCard));
		ZeroMemory(m_cbPerformAction, sizeof(m_cbPerformAction));

		//组合扑克
		ASSERT(m_cbWeaveItemCount[wTargetUser] < MAX_WEAVE);
		WORD wIndex = m_cbWeaveItemCount[wTargetUser]++;
		m_WeaveItemArray[wTargetUser][wIndex].cbPublicCard = TRUE;
		m_WeaveItemArray[wTargetUser][wIndex].cbCenterCard = cbTargetCard;
		m_WeaveItemArray[wTargetUser][wIndex].cbWeaveKind = cbTargetAction;
		m_WeaveItemArray[wTargetUser][wIndex].wProvideUser = (m_wProvideUser == INVALID_CHAIR) ? wTargetUser : m_wProvideUser;

		//删除扑克，吃、碰、杠
		switch (cbTargetAction)
		{
		case WIK_LEFT:		//上牌操作
		{
			//删除扑克
			BYTE cbRemoveCard[3];
			m_GameLogic.GetWeaveCard(WIK_LEFT, cbTargetCard, cbRemoveCard);
			VERIFY(m_GameLogic.RemoveCard(cbRemoveCard, 3, &cbTargetCard, 1));
			VERIFY(m_GameLogic.RemoveCard(m_cbCardIndex[wTargetUser], cbRemoveCard, 2));

			m_cbChiCard = cbTargetCard;
			break;
		}
		case WIK_RIGHT:		//上牌操作
		{
			//删除扑克
			BYTE cbRemoveCard[3];
			m_GameLogic.GetWeaveCard(WIK_RIGHT, cbTargetCard, cbRemoveCard);
			VERIFY(m_GameLogic.RemoveCard(cbRemoveCard, 3, &cbTargetCard, 1));
			VERIFY(m_GameLogic.RemoveCard(m_cbCardIndex[wTargetUser], cbRemoveCard, 2));

			m_cbChiCard = cbTargetCard;
			break;
		}
		case WIK_CENTER:	//上牌操作
		{
			//删除扑克
			BYTE cbRemoveCard[3];
			m_GameLogic.GetWeaveCard(WIK_CENTER, cbTargetCard, cbRemoveCard);
			VERIFY(m_GameLogic.RemoveCard(cbRemoveCard, 3, &cbTargetCard, 1));
			VERIFY(m_GameLogic.RemoveCard(m_cbCardIndex[wTargetUser], cbRemoveCard, 2));

			m_cbChiCard = cbTargetCard;
			break;
		}
		case WIK_PENG:		//碰牌操作
		{
			//删除扑克
			BYTE cbRemoveCard[] = { cbTargetCard,cbTargetCard };
			VERIFY(m_GameLogic.RemoveCard(m_cbCardIndex[wTargetUser], cbRemoveCard, 2));
			break;
		}
		case WIK_GANG:		//放杠操作
		{
			BYTE cbRemoveCard[] = { cbTargetCard,cbTargetCard,cbTargetCard };
			VERIFY(m_GameLogic.RemoveCard(m_cbCardIndex[wTargetUser], cbRemoveCard, CountArray(cbRemoveCard)));
			break;
		}
		default:
			ASSERT(FALSE);
			return false;
		}


		//飘财承包
		m_bOperateStatus = true;
		m_wPiaoChengBaoUser = m_wProvideUser;
		//清一色承包
		m_wChiPengUser[wTargetUser][m_cbChiPengCount[wTargetUser]++] = m_wProvideUser;



		WORD wClearUser = (wTargetUser + GAME_PLAYER - 1) % GAME_PLAYER;
		//清除漏碰记录
		if ((cbTargetAction & WIK_PENG) != 0 || (cbTargetAction & WIK_GANG) != 0)
		{
			//有碰杠
			if (m_wProvideUser != INVALID_CHAIR)
			{
				//重置供牌用户和操作用户中间那些人的漏碰记录
				for (WORD wNextUser = (m_wProvideUser + GAME_PLAYER - 1) % GAME_PLAYER; wNextUser != wClearUser; wNextUser = (wNextUser + GAME_PLAYER - 1) % GAME_PLAYER)
				{
					m_bLouPengStatus[wNextUser] = false;
					m_cbLouPengCard[wNextUser] = 0;
				}
			}
		}



		WORD first = INVALID_CHAIR;
		WORD second = INVALID_CHAIR;
		WORD third = INVALID_CHAIR;
		BYTE count_1 = 0;
		BYTE count_2 = 0;
		BYTE count_3 = 0;
		//BYTE index = 0;
		bool setOnce = false;

		for (BYTE i = 0; i < m_cbChiPengCount[wTargetUser]; i++)
		{
			if (first == INVALID_CHAIR)
			{
				first = m_wChiPengUser[wTargetUser][i];
				count_1++;
			}
			else if (first == m_wChiPengUser[wTargetUser][i])
				count_1++;
			else if (first != INVALID_CHAIR && first != m_wChiPengUser[wTargetUser][i] && second == INVALID_CHAIR)
			{
				second = m_wChiPengUser[wTargetUser][i];
				count_2++;
			}
			else if (second == m_wChiPengUser[wTargetUser][i])
				count_2++;
			else if (first != INVALID_CHAIR && first != m_wChiPengUser[wTargetUser][i] && second != INVALID_CHAIR && second != m_wChiPengUser[wTargetUser][i] && third == INVALID_CHAIR)
			{
				third = m_wChiPengUser[wTargetUser][i];
				count_3++;
			}
			else if (third == m_wChiPengUser[wTargetUser][i])
				count_3++;

			//if (count_1 == 3 || count_2 == 3 || count_3 == 3)
			//{
			//	if (setOnce == false)
			//	{
			//		index = i;
			//		setOnce = true;
			//	}
			//}
		}

		if (count_1 >= 3)
			m_bQingYiSeChengBao[wTargetUser][first] = true;
		else if (count_2 >= 3)
			m_bQingYiSeChengBao[wTargetUser][second] = true;
		else if (count_3 >= 3)
			m_bQingYiSeChengBao[wTargetUser][third] = true;

		/*if (index == 2 && m_cbChiPengCount[wTargetUser] > 3)
		{
			ZeroMemory(m_bQingYiSeChengBao[wTargetUser], sizeof(m_bQingYiSeChengBao[wTargetUser]));
			m_bQingYiSeChengBao[wTargetUser][m_wChiPengUser[wTargetUser][3]] = true;
		}
		else if (index == 3 && m_cbChiPengCount[wTargetUser] > 4)
		{
			ZeroMemory(m_bQingYiSeChengBao[wTargetUser], sizeof(m_bQingYiSeChengBao[wTargetUser]));
			m_bQingYiSeChengBao[wTargetUser][m_wChiPengUser[wTargetUser][4]] = true;
		}*/

		if (m_cbWeaveItemCount[wTargetUser] > 3)
		{
			ZeroMemory(m_bQingYiSeChengBao[wTargetUser], sizeof(m_bQingYiSeChengBao[wTargetUser]));
			m_bQingYiSeChengBao[wTargetUser][m_wChiPengUser[wTargetUser][3]] = true;
		}

		if (m_cbWeaveItemCount[wTargetUser] > 2)
		{
			BYTE cbColor = m_WeaveItemArray[wTargetUser][0].cbCenterCard & MASK_COLOR;
			for (BYTE i = 0; i < m_cbWeaveItemCount[wTargetUser]; i++)
			{
				BYTE cbWeaveColor = m_WeaveItemArray[wTargetUser][i].cbCenterCard & MASK_COLOR;
				if (cbColor != cbWeaveColor)
					ZeroMemory(m_bQingYiSeChengBao[wTargetUser], sizeof(m_bQingYiSeChengBao[wTargetUser]));
			}
		}



		//构造结果
		CMD_S_OperateResult OperateResult;
		ZeroMemory(&OperateResult, sizeof(OperateResult));
		OperateResult.wOperateUser = wTargetUser;
		OperateResult.cbOperateCard = cbTargetCard;
		OperateResult.cbOperateCode = cbTargetAction;
		OperateResult.wProvideUser = (m_wProvideUser == INVALID_CHAIR) ? wTargetUser : m_wProvideUser;

		//发送消息
		m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_OPERATE_RESULT, &OperateResult, sizeof(OperateResult));
		m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_OPERATE_RESULT, &OperateResult, sizeof(OperateResult));

		//设置用户
		m_wCurrentUser = wTargetUser;

		//吃碰后检测能否杠牌
		if (cbTargetAction == WIK_LEFT || cbTargetAction == WIK_CENTER || cbTargetAction == WIK_RIGHT || cbTargetAction == WIK_PENG)
		{
			//杠牌判断
			if (m_cbLeftCardCount > 18 && (m_cbUserAction[wTargetUser] & WIK_GANG) == 0)
			{
				tagGangCardResult GangCardResult;
				m_cbUserAction[wTargetUser] |= m_GameLogic.AnalyseGangCard(m_cbCardIndex[wTargetUser], m_WeaveItemArray[wTargetUser], m_cbWeaveItemCount[wTargetUser], GangCardResult, false);

				if ((m_cbUserAction[wTargetUser] & WIK_GANG) != 0)
				{
					CMD_S_OperateNotify OperateNotify;
					ZeroMemory(&OperateNotify, sizeof(OperateNotify));
					OperateNotify.wResumeUser = wTargetUser;
					OperateNotify.cbActionCard = GangCardResult.cbCardData[0];
					OperateNotify.cbActionMask = m_cbUserAction[wTargetUser];

					//发送数据
					m_pITableFrame->SendTableData(wTargetUser, SUB_S_OPERATE_NOTIFY, &OperateNotify, sizeof(OperateNotify));
					m_pITableFrame->SendLookonData(wTargetUser, SUB_S_OPERATE_NOTIFY, &OperateNotify, sizeof(OperateNotify));
				}
			}
		}

		//杠牌处理，杠分计算
		if (cbTargetAction == WIK_GANG)
		{
			//计算连续杠牌
			if (m_cbGangCount[wTargetUser] == 0 && m_bGangStatus == false)
				m_cbGangCount[wTargetUser] = 1;
			else
				m_cbGangCount[wTargetUser]++;

			m_bGangStatus = true;

			DispatchCardData(wTargetUser, true);
		}

		return true;
	}

	//主动动作
	if (m_wCurrentUser == wChairID)
	{
		//效验操作
		if ((cbOperateCode == WIK_NULL) || ((m_cbUserAction[wChairID] & cbOperateCode) == 0))
			return true;

		//扑克效验
		ASSERT((cbOperateCode == WIK_NULL)
			|| (cbOperateCode == WIK_CHI_HU)
			|| (m_GameLogic.IsValidCard(cbOperateCard) == true));
		if ((cbOperateCode != WIK_NULL)
			&& (cbOperateCode != WIK_CHI_HU)
			&& (m_GameLogic.IsValidCard(cbOperateCard) == false))
			return true;

		//设置变量
		m_bSendStatus = true;
		m_cbUserAction[m_wCurrentUser] = WIK_NULL;
		m_cbPerformAction[m_wCurrentUser] = WIK_NULL;

		//执行动作
		switch (cbOperateCode)
		{
		case WIK_GANG:			//杠牌操作
		{
			//变量定义
			BYTE cbWeaveIndex = 0xFF;
			BYTE cbCardIndex = m_GameLogic.SwitchToCardIndex(cbOperateCard);

			//补杠
			if (m_cbCardIndex[wChairID][cbCardIndex] == 1)
			{
				//寻找组合
				for (BYTE i = 0; i < m_cbWeaveItemCount[wChairID]; i++)
				{
					BYTE cbWeaveKind = m_WeaveItemArray[wChairID][i].cbWeaveKind;
					BYTE cbCenterCard = m_WeaveItemArray[wChairID][i].cbCenterCard;
					if ((cbCenterCard == cbOperateCard) && (cbWeaveKind == WIK_PENG))
					{
						m_bBuGang = true;
						cbWeaveIndex = i;
						break;
					}
				}

				//效验动作
				ASSERT(cbWeaveIndex != 0xFF);
				if (cbWeaveIndex == 0xFF) return false;

				//组合扑克
				m_WeaveItemArray[wChairID][cbWeaveIndex].cbPublicCard = TRUE;
				m_WeaveItemArray[wChairID][cbWeaveIndex].wProvideUser = wChairID;
				m_WeaveItemArray[wChairID][cbWeaveIndex].cbWeaveKind = cbOperateCode;
				m_WeaveItemArray[wChairID][cbWeaveIndex].cbCenterCard = cbOperateCard;
			}
			else
			{
				//暗杠
				ASSERT(m_cbCardIndex[wChairID][cbCardIndex] == 4);
				if (m_cbCardIndex[wChairID][cbCardIndex] != 4)
					return false;

				m_bBuGang = false;
				//设置变量
				cbWeaveIndex = m_cbWeaveItemCount[wChairID]++;
				m_WeaveItemArray[wChairID][cbWeaveIndex].cbPublicCard = FALSE;
				m_WeaveItemArray[wChairID][cbWeaveIndex].wProvideUser = wChairID;
				m_WeaveItemArray[wChairID][cbWeaveIndex].cbWeaveKind = cbOperateCode;
				m_WeaveItemArray[wChairID][cbWeaveIndex].cbCenterCard = cbOperateCard;
			}

			//删除扑克
			m_cbCardIndex[wChairID][cbCardIndex] = 0;

			//计算连续杠牌
			if (m_cbGangCount[wChairID] == 0 && m_bGangStatus == false)
				m_cbGangCount[wChairID] = 1;
			else
				m_cbGangCount[wChairID]++;

			m_bGangStatus = true;



			//构造结果
			CMD_S_OperateResult OperateResult;
			ZeroMemory(&OperateResult, sizeof(OperateResult));
			OperateResult.wOperateUser = wChairID;
			OperateResult.wProvideUser = wChairID;
			OperateResult.cbOperateCode = cbOperateCode;
			OperateResult.cbOperateCard = cbOperateCard;

			//发送消息
			m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_OPERATE_RESULT, &OperateResult, sizeof(OperateResult));
			m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_OPERATE_RESULT, &OperateResult, sizeof(OperateResult));


			//设置变量
			m_cbProvideCard = cbOperateCard;
			m_wProvideUser = wChairID;
			m_wCurrentUser = wChairID;


			//效验动作
			bool bAroseAction = false;

			if (m_bBuGang == true)
				bAroseAction = EstimateUserRespond(wChairID, cbOperateCard, EstimatKind_GangCard);

			//发送扑克
			if (bAroseAction == false)
			{
				DispatchCardData(wChairID, true);
			}

			return true;
		}
		case WIK_CHI_HU:		//吃胡操作
		{
			//吃牌权位
			if (m_cbOutCardCount == 0)
			{
				m_wProvideUser = m_wCurrentUser;
				m_cbProvideCard = m_cbSendCardData;
			}

			//胡牌判断,赋值权位
			BYTE cbWeaveItemCount = m_cbWeaveItemCount[wChairID];
			tagWeaveItem *pWeaveItem = m_WeaveItemArray[wChairID];
			m_GameLogic.RemoveCard(m_cbCardIndex[wChairID], &m_cbProvideCard, 1);
			m_dwChiHuKind[wChairID] = AnalyseChiHuCard(m_cbCardIndex[wChairID], pWeaveItem, cbWeaveItemCount, m_cbProvideCard, m_ChiHuRight[wChairID]);


			m_cbCardIndex[wChairID][m_GameLogic.SwitchToCardIndex(m_cbProvideCard)]++;
			ProcessChiHuUser(wChairID, false);


			//结束信息
			m_cbChiHuCard = m_cbProvideCard;


			m_wBankerUser = wChairID;	//胡牌方为下一局庄家

			OnEventGameConclude(INVALID_CHAIR, NULL, GER_NORMAL);

			return true;
		}
		}

		return true;
	}

	return false;
}

//发送操作
bool CTableFrameSink::SendOperateNotify()
{
	//发送提示
	for (WORD i = 0; i < GAME_PLAYER; i++)
	{
		if (m_cbUserAction[i] != WIK_NULL)
		{
			//构造数据
			CMD_S_OperateNotify OperateNotify;
			ZeroMemory(&OperateNotify, sizeof(OperateNotify));
			OperateNotify.wResumeUser = m_wResumeUser;
			OperateNotify.cbActionCard = m_cbProvideCard;
			OperateNotify.cbActionMask = m_cbUserAction[i];

			//发送数据
			m_pITableFrame->SendTableData(i, SUB_S_OPERATE_NOTIFY, &OperateNotify, sizeof(OperateNotify));
			m_pITableFrame->SendLookonData(i, SUB_S_OPERATE_NOTIFY, &OperateNotify, sizeof(OperateNotify));
		}
	}

	return true;
}

//派发扑克
bool CTableFrameSink::DispatchCardData(WORD wCurrentUser, bool bTail)
{
	//状态效验
	ASSERT(wCurrentUser != INVALID_CHAIR);
	if (wCurrentUser == INVALID_CHAIR)
		return false;

	//荒庄结束
	if (m_cbLeftCardCount < 19)
	{
		//流局情况下，抓最后一张牌的为下局庄家
		m_wBankerUser = m_wProvideUser;
		m_wResumeUser = INVALID_CHAIR;
		m_wCurrentUser = INVALID_CHAIR;
		m_wProvideUser = INVALID_CHAIR;
		m_cbProvideCard = 0;

		//结束信息
		m_cbChiHuCard = 0;
		ZeroMemory(m_dwChiHuKind, sizeof(m_dwChiHuKind));
		for (WORD i = 0; i < GAME_PLAYER; i++)
		{
			m_ChiHuRight[i].SetEmpty();
		}
		memset(m_wProvider, INVALID_CHAIR, sizeof(m_wProvider));

		OnEventGameConclude(INVALID_CHAIR, NULL, GER_NORMAL);

		return true;
	}

	//设置变量
	m_cbOutCardData = 0;
	m_wOutCardUser = INVALID_CHAIR;

	//杠后炮
	if (m_bGangOutStatus)
	{
		m_bGangOutStatus = false;
	}



	//飘杠
	if (m_bPiaoStatus[wCurrentUser] == true && m_bGangStatus)
		m_bPiaoGang[wCurrentUser] = true;

	//因为别人而杠，飘财承包
	if (m_bPiaoGang[wCurrentUser] && m_bOperateStatus)
	{
		SCORE lPiaoScore = pow((LONG)2, (LONG)(m_cbPiaoCount[wCurrentUser] + 1 + m_cbGangCount[wCurrentUser]));
		if (lPiaoScore > 20)
			lPiaoScore = 20;
		m_lPiaoScore[wCurrentUser][m_wPiaoChengBaoUser] += lPiaoScore * 3 / 2;
	}

	//构造数据
	CMD_S_SendCard SendCard;
	ZeroMemory(&SendCard, sizeof(SendCard));

	//发牌处理
	if (m_bSendStatus == true)
	{
		//发送扑克
		m_cbSendCardCount++;
		if (hasRule(GAME_TYPE_ZZ_HONGZHONG))
		{
			m_cbSendCardData = m_cbRepertoryCard_HZ[--m_cbLeftCardCount];
		}
		else
		{
			m_cbSendCardData = m_cbRepertoryCard[--m_cbLeftCardCount];
		}

		//设置变量
		m_cbProvideCard = m_cbSendCardData;
		m_wProvideUser = wCurrentUser;
		m_wCurrentUser = wCurrentUser;

		//胡牌判断,赋值权位
		BYTE cbWeaveItemCount = m_cbWeaveItemCount[wCurrentUser];
		tagWeaveItem *pWeaveItem = m_WeaveItemArray[wCurrentUser];
		m_cbUserAction[wCurrentUser] |= AnalyseChiHuCard(m_cbCardIndex[wCurrentUser], pWeaveItem, cbWeaveItemCount, m_cbSendCardData, m_ChiHuRight[wCurrentUser]);


		

		ZeroMemory(&SendCard.tGangCard, sizeof(tagGangCardResult));
		/*//杠牌判断
		if (m_cbLeftCardCount > 18)
		{
			auto actionMask = m_GameLogic.EstimateGangCard(m_cbCardIndex[wCurrentUser], m_cbSendCardData);//暗杠
			if (actionMask == WIK_GANG)
			{
				SendCard.tGangCard.cbCardData[SendCard.tGangCard.cbCardCount++] = m_cbSendCardData;
			}
			else
			{
				actionMask = m_GameLogic.EstimateGangCard(m_WeaveItemArray[wCurrentUser], m_cbWeaveItemCount[wCurrentUser], m_cbSendCardData);//补杠
				if (actionMask == WIK_GANG)
				{
					SendCard.tGangCard.cbCardData[SendCard.tGangCard.cbCardCount++] = m_cbSendCardData;
				}
			}
		}*/

		//加牌
		m_cbCardIndex[wCurrentUser][m_GameLogic.SwitchToCardIndex(m_cbSendCardData)]++;



		//杠牌判断
		if (m_cbLeftCardCount > 18)
		{
			m_cbUserAction[wCurrentUser] |= m_GameLogic.AnalyseGangCard(m_cbCardIndex[wCurrentUser], m_WeaveItemArray[wCurrentUser], m_cbWeaveItemCount[wCurrentUser], SendCard.tGangCard, false);
			if (SendCard.tGangCard.cbCardCount > 0)
			{
				CopyMemory(&m_tGangResult[wCurrentUser], &(SendCard.tGangCard), sizeof(tagGangCardResult));
				
				SendCard.cbActionMask |= WIK_GANG;
			}
		}
	}

	SendCard.wCurrentUser = wCurrentUser;
	SendCard.bTail = bTail;
	SendCard.cbActionMask = m_cbUserAction[wCurrentUser];
	SendCard.cbSendCardData = (m_bSendStatus == true) ? m_cbSendCardData : 0x00;
	SendCard.bKaiGangYaoShaiZi = false;

	//发送数据
	m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_SEND_CARD, &SendCard, sizeof(SendCard));
	m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_SEND_CARD, &SendCard, sizeof(SendCard));

	return true;
}

//响应判断
bool CTableFrameSink::EstimateUserRespond(WORD wCenterUser, BYTE cbCenterCard, enEstimatKind EstimatKind)
{
	//变量定义
	bool bAroseAction = false;

	//用户状态
	ZeroMemory(m_bResponse, sizeof(m_bResponse));
	ZeroMemory(m_cbUserAction, sizeof(m_cbUserAction));
	ZeroMemory(m_cbPerformAction, sizeof(m_cbPerformAction));


	//如果有人飘财，只有飘财的玩家可以吃、碰
	bool bHasPiao = false;
	for (WORD i = 0; i < GAME_PLAYER; i++)
	{
		if (m_bPiaoStatus[i] == true)
			bHasPiao = true;
	}


	//动作判断，碰，放杠
	//出牌类型
	if (EstimatKind == EstimatKind_OutCard)
	{
		for (WORD i = 0; i < GAME_PLAYER; i++)
		{
			//用户过滤,出牌用户
			if (wCenterUser == i) continue;

			//没人飘财
			if (!bHasPiao)
			{
				//没有漏碰或者不与漏碰的牌相同
				if (m_bLouPengStatus[i] == false || m_cbLouPengCard[i] != cbCenterCard)
					m_cbUserAction[i] |= m_GameLogic.EstimatePengCard(m_cbCardIndex[i], cbCenterCard);

				//放杠判断
				if (m_cbLeftCardCount > 18)//为了杠牌后，有牌可以补
				{
					m_cbUserAction[i] |= m_GameLogic.EstimateGangCard(m_cbCardIndex[i], cbCenterCard);
				}
			}
			//至少一个人飘财
			else
			{
				if (m_bPiaoStatus[i] == true)
				{
					//没有漏碰或者不与漏碰的牌相同
					if (m_bLouPengStatus[i] == false || m_cbLouPengCard[i] != cbCenterCard)
						m_cbUserAction[i] |= m_GameLogic.EstimatePengCard(m_cbCardIndex[i], cbCenterCard);

					//放杠判断
					if (m_cbLeftCardCount > 18)//为了杠牌后，有牌可以补
					{
						m_cbUserAction[i] |= m_GameLogic.EstimateGangCard(m_cbCardIndex[i], cbCenterCard);
					}
				}
			}

			//结果判断,有一个不为null可以
			if (m_cbUserAction[i] != WIK_NULL)
				bAroseAction = true;
		}
	}


	//抢杠胡,补杠
	//暴头情况下不能抢杠
	bool bHasBaoTou = false;
	bool bHasOne = false;
	if (EstimatKind == EstimatKind_GangCard)
	{
		for (WORD i = 0; i < GAME_PLAYER; i++)
		{
			//用户过滤,出牌用户
			if (wCenterUser == i) continue;

			//吃胡判断
			BYTE cbWeaveItemCount = m_cbWeaveItemCount[i];
			tagWeaveItem *pWeaveItem = m_WeaveItemArray[i];
			m_cbUserAction[i] |= AnalyseChiHuCard(m_cbCardIndex[i], pWeaveItem, cbWeaveItemCount, cbCenterCard, m_ChiHuRight[i]);

			//结果判断,有一个不为null可以
			if (m_cbUserAction[i] != WIK_NULL)
				bHasOne = true;

			if (!(m_ChiHuRight[i] & CHR_BAO_TOU).IsEmpty())
				bHasBaoTou = true;
		}

		if (bHasOne && !bHasBaoTou)
			bAroseAction = true;
	}
	else
	{
		//统计吃数
		BYTE bChiCount = 0;
		for (BYTE j = 0; j < m_cbWeaveItemCount[m_wCurrentUser]; j++)
		{
			tagWeaveItem m_WeaveItem = m_WeaveItemArray[m_wCurrentUser][j];
			if (m_WeaveItem.cbWeaveKind == WIK_GANG || m_WeaveItem.cbWeaveKind == WIK_PENG) continue;
			bChiCount++;
		}

		//最多只能吃两次
		if (bChiCount >= 2)
		{

		}
		else
		{
			if (!bHasPiao)
			{
				m_cbUserAction[m_wCurrentUser] |= m_GameLogic.EstimateEatCard(m_cbCardIndex[m_wCurrentUser], cbCenterCard);
			}
			else
			{
				if (m_bPiaoStatus[m_wCurrentUser] == true)
				{
					m_cbUserAction[m_wCurrentUser] |= m_GameLogic.EstimateEatCard(m_cbCardIndex[m_wCurrentUser], cbCenterCard);
				}
			}
		}

		//结果判断
		if (m_cbUserAction[m_wCurrentUser] != WIK_NULL)
			bAroseAction = true;
	}

	//结果处理
	if (bAroseAction == true)
	{
		//设置变量
		m_wProvideUser = wCenterUser;
		m_cbProvideCard = cbCenterCard;
		m_wResumeUser = m_wCurrentUser;
		m_wCurrentUser = INVALID_CHAIR;

		//发送提示
		SendOperateNotify();
		return true;
	}

	return false;
}

//计算积分
void CTableFrameSink::ProcessChiHuUser(WORD wChairId, bool bGiveUp)
{
	if (!bGiveUp)
	{
		//权位附加
		FiltrateRight(wChairId, m_ChiHuRight[wChairId]);

		WORD wFanShu = 0;
		if (m_cbGameTypeIdex == GAME_TYPE_ZZ)
		{
			wFanShu = m_GameLogic.GetChiHuActionRank_ZZ(m_ChiHuRight[wChairId]);
		}

		//连续多次杠牌加倍
		if (m_cbGangCount[wChairId] > 1)
			wFanShu = wFanShu * (m_cbGangCount[wChairId] - 1) * 2;

		if (!(m_ChiHuRight[wChairId] & CHR_ZI_MO).IsEmpty())
		{
			if (wFanShu > 20)
				wFanShu = 20;
		}
		else
		{
			if (wFanShu > 60)
				wFanShu = 60;
		}

		//mChen edit
		LONGLONG lChiHuScore = wFanShu * m_lBaseScore;
		///LONGLONG lChiHuScore = wFanShu * m_pGameServiceOption->lCellScore;


		if (m_wProvideUser == wChairId)
		{
			for (WORD i = 0; i < GAME_PLAYER; i++)
			{
				if (i == wChairId) continue;

				//胡牌分
				m_lGameScore[i] -= lChiHuScore;
				m_lGameScore[wChairId] += lChiHuScore;

			}

			//飘财承包
			bool bHasPiaoScore = false;
			for (WORD i = 0; i < GAME_PLAYER; i++)
			{
				if (i == wChairId) continue;

				if (m_lPiaoScore[wChairId][i] == 0) continue;

				ZeroMemory(m_lGameScore, sizeof(m_lGameScore));
				bHasPiaoScore = true;
			}
			if (bHasPiaoScore)
			{
				for (WORD i = 0; i < GAME_PLAYER; i++)
				{
					if (i == wChairId) continue;

					m_lGameScore[i] -= (m_lBasePiaoScore[wChairId] + m_lPiaoScore[wChairId][i]) * m_lBaseScore;//m_pGameServiceOption->lCellScore
					m_lGameScore[wChairId] += (m_lBasePiaoScore[wChairId] + m_lPiaoScore[wChairId][i]) * m_lBaseScore;;
				}
			}


			//清一色承包
			bool isQing = false;
			bool isBaoTou = false;
			for (WORD i = 0; i < GAME_PLAYER; i++)
			{
				if (i == wChairId) continue;

				if (m_bQingYiSeChengBao[wChairId][i] != true && m_bQingYiSeChengBao[i][wChairId] != true) continue;


				//反承包
				isQing = m_GameLogic.IsQingYiSe(m_cbCardIndex[i], m_WeaveItemArray[i], m_cbWeaveItemCount[i], m_GameLogic.SwitchToCardData(33));
				isBaoTou = m_GameLogic.IsBaoTou(m_cbCardIndex[i], m_WeaveItemArray[i], m_cbWeaveItemCount[i], m_GameLogic.SwitchToCardData(33));

				LONGLONG lLoserScore = 0L;
				WORD wLoser = 0;
				if (isQing)
				{
					wLoser = 10;
					if (isBaoTou)
						wLoser = 20;
					lLoserScore = wLoser * m_lBaseScore;//m_pGameServiceOption->lCellScore
				}

				//防止反承包bug,胡牌家是清一色但m_bQingYiSeChengBao == false;另一家与胡牌方的m_bQingYiSeChengBao == true,但不是清一色，这时候也算承包了
				//if (m_bQingYiSeChengBao[wChairId][i] == false && m_bQingYiSeChengBao[i][wChairId] == true && !isQing) continue;


				if (!(m_ChiHuRight[wChairId] & CHR_QING_YI_SE).IsEmpty() || isQing)
				{
					if (lLoserScore > lChiHuScore)
						lChiHuScore = lLoserScore;

					ZeroMemory(m_lGameScore, sizeof(m_lGameScore));
					m_lGameScore[i] -= lChiHuScore * 3;
					m_lGameScore[wChairId] += lChiHuScore * 3;
				}
			}

		}
		//抢杠
		else
		{
			bool isQing = false;
			bool isBaoTou = false;
			isQing = m_GameLogic.IsQingYiSe(m_cbCardIndex[m_wProvideUser], m_WeaveItemArray[m_wProvideUser], m_cbWeaveItemCount[m_wProvideUser], m_GameLogic.SwitchToCardData(33));
			isBaoTou = m_GameLogic.IsBaoTou(m_cbCardIndex[m_wProvideUser], m_WeaveItemArray[m_wProvideUser], m_cbWeaveItemCount[m_wProvideUser], m_GameLogic.SwitchToCardData(33));

			LONGLONG lLoserScore = 0L;
			WORD wLoser = 0;
			if (isQing)
			{
				wLoser = 30;
				if (isBaoTou)
					wLoser = 60;
			}

			if (m_cbGangCount[m_wProvideUser] > 1)
				wLoser = 12 * (m_cbGangCount[m_wProvideUser] - 1) * 2;

			if (wLoser > 60)
				wLoser = 60;

			lLoserScore = wLoser * m_lBaseScore;//m_pGameServiceOption->lCellScore


			if (lLoserScore > lChiHuScore)
				lChiHuScore = lLoserScore;


			m_lGameScore[m_wProvideUser] -= lChiHuScore;
			m_lGameScore[wChairId] += lChiHuScore;
		}

		//设置变量
		m_wProvider[wChairId] = m_wProvideUser;
		m_bGangStatus = false;
		m_bGangOutStatus = false;
		m_wChiHuUser = wChairId;


		//发送消息
		CMD_S_ChiHu ChiHu;
		ZeroMemory(&ChiHu, sizeof(ChiHu));
		ChiHu.wChiHuUser = wChairId;
		ChiHu.wProviderUser = m_wProvideUser;
		ChiHu.lGameScore = m_lGameScore[wChairId];
		ChiHu.cbCardCount = m_GameLogic.GetCardCount(m_cbCardIndex[wChairId]);
		ChiHu.cbChiHuCard = m_cbProvideCard;
		m_pITableFrame->SendTableData(INVALID_CHAIR, SUB_S_CHI_HU, &ChiHu, sizeof(ChiHu));
		m_pITableFrame->SendLookonData(INVALID_CHAIR, SUB_S_CHI_HU, &ChiHu, sizeof(ChiHu));

	}

	return;
}





//////////////////////////////////////////////////////////////////////////
