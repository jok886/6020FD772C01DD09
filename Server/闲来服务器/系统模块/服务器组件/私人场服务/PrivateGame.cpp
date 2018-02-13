#include "StdAfx.h"
#include "PrivateGame.h"
#include "FangKaHttpUnits.h"
#include "..\游戏服务器\DataBasePacket.h"

////////////////////////////////////////////////////////////////////////////////////////////////////////////
#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//构造函数
PriaveteGame::PriaveteGame()
{
	m_pGameServiceOption=NULL;
	m_pGameServiceAttrib=NULL;

	//内核接口
	m_pTableInfo=NULL;
	m_pITimerEngine=NULL;
	m_pIDataBaseEngine=NULL;
	m_pITCPNetworkEngineEvent=NULL;

	//服务接口
	m_pIGameServiceFrame=NULL;
	m_pIServerUserManager=NULL;
	m_pAndroidUserManager=NULL;
}

PriaveteGame::~PriaveteGame(void)
{
	//释放资源
	SafeDeleteArray(m_pTableInfo);

	//关闭定时器
	m_pITimerEngine->KillTimer(IDI_DISMISS_WAITE_END);

	m_pITimerEngine->KillTimer(IDI_PLAY_AGAIN_WAIT_END);
}

//接口查询
VOID* PriaveteGame::QueryInterface(REFGUID Guid, DWORD dwQueryVer)
{	
	QUERYINTERFACE(IGamePrivateItem,Guid,dwQueryVer);
	QUERYINTERFACE(IPrivateEventSink,Guid,dwQueryVer);
	QUERYINTERFACE(IServerUserItemSink,Guid,dwQueryVer);	
	QUERYINTERFACE_IUNKNOWNEX(IGamePrivateItem,Guid,dwQueryVer);
	return NULL;
}

//绑定桌子
bool PriaveteGame::BindTableFrame(ITableFrame * pTableFrame,WORD wTableID)
{
	if(pTableFrame==NULL || wTableID>m_pGameServiceOption->wTableCount)
	{
		ASSERT(false);
		return false;
	}

	//创建钩子
	CTableFramePrivate * pTableFrameHook=new CTableFramePrivate();
	pTableFrameHook->InitTableFramePrivate(QUERY_OBJECT_PTR_INTERFACE(pTableFrame,IUnknownEx));
	pTableFrameHook->SetPrivateEventSink(QUERY_OBJECT_PTR_INTERFACE(this,IUnknownEx));

	//设置接口
	pTableFrame->SetTableFramePrivate(QUERY_OBJECT_PTR_INTERFACE(pTableFrameHook,IUnknownEx));
	m_pTableInfo[wTableID].pITableFrame=pTableFrame;
	m_pTableInfo[wTableID].restValue();

	return true;
}

//初始化接口
bool PriaveteGame::InitPrivateInterface(tagPrivateManagerParameter & MatchManagerParameter)
{
	m_pGameServiceOption=MatchManagerParameter.pGameServiceOption;
	m_pGameServiceAttrib=MatchManagerParameter.pGameServiceAttrib;

	//内核组件
	m_pITimerEngine=MatchManagerParameter.pITimerEngine;
	m_pIDataBaseEngine=MatchManagerParameter.pICorrespondManager;
	m_pITCPNetworkEngineEvent=MatchManagerParameter.pTCPNetworkEngine;

	//服务组件		
	m_pIGameServiceFrame=MatchManagerParameter.pIMainServiceFrame;		
	m_pIServerUserManager=MatchManagerParameter.pIServerUserManager;
	m_pAndroidUserManager=MatchManagerParameter.pIAndroidUserManager;
	m_pIServerUserItemSink=MatchManagerParameter.pIServerUserItemSink;

	//创建桌子
	if (m_pTableInfo==NULL)
	{
		m_pTableInfo = new PrivateTableInfo[m_pGameServiceOption->wTableCount];
	}

	return true;
}
void PriaveteGame::OnStartService()
{
	//变量定义
	DBR_GR_Private_Info kPrivateInfo;
	ZeroMemory(&kPrivateInfo,sizeof(kPrivateInfo));
	kPrivateInfo.wKindID=m_pGameServiceOption->wKindID;
	m_pIDataBaseEngine->PostDataBaseRequest(0L,DBR_GR_PRIVATE_INFO,0L,&kPrivateInfo,sizeof(kPrivateInfo));

	m_pITimerEngine->SetTimer(IDI_DISMISS_WAITE_END,5000L,TIMES_INFINITY,0);

	//mChen add, for HideSeek
	//游戏服务器重启，删除锁表记录
	ClearGameScoreLocker();
}

//mChen add, for HideSeek
void PriaveteGame::OnStopService()
{
	//游戏服务器停止，删除锁表记录
	ClearGameScoreLocker();
}

//mChen add, for HideSeek
//删除锁表记录
void PriaveteGame::ClearGameScoreLocker()
{
	//Log
	CTraceService::TraceString(TEXT("PriaveteGame::ClearGameScoreLocker"), TraceLevel_Normal);

	m_pIDataBaseEngine->PostDataBaseRequest(0L, DBR_GR_CLEAR_GAME_SCORE_LOCKER, 0L, NULL, 0);
}

//时间事件
bool PriaveteGame::OnEventTimer(DWORD dwTimerID, WPARAM dwBindParameter)
{	
	switch(dwTimerID)
	{
	//	//mChen add, for HideSeek
	//case IDI_PLAY_AGAIN_WAIT_END:
	//{
	//	//蓄房等待时间到

	//	WORD wTableId = (WORD)dwBindParameter;
	//	if (wTableId < m_pGameServiceOption->wTableCount)
	//	{
	//		PrivateTableInfo* pTableInfo = &m_pTableInfo[wTableId];

	//		if (pTableInfo->pITableFrame->GetGameStatus() != GAME_STATUS_FREE)
	//		{
	//			return true;
	//		}

	//		WORD wSitUserCount = pTableInfo->pITableFrame->GetSitUserCount();
	//		WORD wPlayingUserCount = pTableInfo->pITableFrame->GetPlayingUserCount();
	//		//if (wSitUserCount == 0)
	//		//{
	//		//	return true;
	//		//}

	//		if (wPlayingUserCount > 0)
	//		{
	//			//Log
	//			TCHAR szString[128] = TEXT("");
	//			_sntprintf(szString, CountArray(szString), TEXT("IDI_PLAY_AGAIN_WAIT_END: StartWaitGame,TableId=%d"), pTableInfo->pITableFrame->GetTableID());
	//			CTraceService::TraceString(szString, TraceLevel_Warning);

	//			pTableInfo->pITableFrame->StartWaitGame();
	//		}
	//		else
	//		{
	//			//Log
	//			TCHAR szString[128] = TEXT("");
	//			_sntprintf(szString, CountArray(szString), TEXT("IDI_PLAY_AGAIN_WAIT_END: 蓄房时间结束，房间内没有人或者都断线了，强制解散房间,TableId=%d"), pTableInfo->pITableFrame->GetTableID());
	//			CTraceService::TraceString(szString, TraceLevel_Warning);

	//			//蓄房时间结束，房间内没有人或者都断线了，强制解散房间
	//			DismissRoom(pTableInfo, GER_DISMISS);
	//			ClearRoom(pTableInfo);
	//		}
	//	}
	//	return true;
	//}

	case IDI_DISMISS_WAITE_END:				//解散等待时间 10s
		{
			for(int i = 0;i<m_pGameServiceOption->wTableCount;i++)
			{
				PrivateTableInfo* pTableInfo = &m_pTableInfo[i];

#if false
				if (pTableInfo->bInEnd)
				{
					pTableInfo->fAgainPastTime += 5.0f;
					if (pTableInfo->fAgainPastTime >= AGAIN_WAITE_TIME)
					{
						//Log
						CTraceService::TraceString(TEXT("PriaveteGame::IDI_DISMISS_WAITE_END: pTableInfo->fAgainPastTime >= AGAIN_WAITE_TIME cause ClearRoom"), TraceLevel_Normal);

						ClearRoom(pTableInfo);
					}
				}
#endif
				if (pTableInfo->kDismissChairID.size())
				{
					pTableInfo->fDismissPastTime += 5.0f;
					if (pTableInfo->fDismissPastTime >= DISMISS_WAITE_TIME)
					{
						if (pTableInfo->kNotAgreeChairID.size() <= 1)
						{
							// Log
							CTraceService::TraceString(TEXT("PriaveteGame::OnEventTimer:IDI_DISMISS_WAITE_END:解散等待时间到,DismissRoom"), TraceLevel_Warning);

							DismissRoom(pTableInfo, GER_DISMISS);
						}
					}
				}
			}
			return true;
		}
	}
	
	return true;
}

//发送数据
bool PriaveteGame::SendData(IServerUserItem * pIServerUserItem, WORD wMainCmdID, WORD wSubCmdID, VOID * pData, DWORD wDataSize)
{
	if(pIServerUserItem!=NULL)
		return m_pIGameServiceFrame->SendData(pIServerUserItem,wMainCmdID,wSubCmdID,pData,wDataSize);
	else
		return m_pIGameServiceFrame->SendData(BG_ALL_CLIENT, wMainCmdID,wSubCmdID, pData, wDataSize);

	return true;
}
bool PriaveteGame::SendTableData(ITableFrame*	pITableFrame, WORD wMainCmdID, WORD wSubCmdID, VOID * pData, DWORD wDataSize)
{
	return pITableFrame->SendTableData(INVALID_CHAIR,wSubCmdID,pData,wDataSize,wMainCmdID);
}

//mChen add, for 比赛场:每场结束第一名获得一个钻石
void PriaveteGame::CreatePrivateMatchAward(DWORD dwUserID, DWORD dwAwardScore)
{
	DBR_GR_Create_Private_Cost kNetInfo;
	kNetInfo.dwUserID = dwUserID;
	kNetInfo.dwCost = -dwAwardScore;
	kNetInfo.dwCostType = Type_Private;
	m_pIDataBaseEngine->PostDataBaseRequest(0L, DBR_GR_CREATE_PRIVAT_COST, 0L, &kNetInfo, sizeof(kNetInfo));
}

//mChen add, for HideSeek
void PriaveteGame::CreateUserCost(DWORD dwUserID, DWORD dwCostType, DWORD dwCost)
{
	DBR_GR_Create_Private_Cost kNetInfo;
	kNetInfo.dwUserID = dwUserID;
	kNetInfo.dwCostType = dwCostType;
	kNetInfo.dwCost = dwCost;

	m_pIDataBaseEngine->PostDataBaseRequest(0L, DBR_GR_CREATE_PRIVAT_COST, 0L, &kNetInfo, sizeof(kNetInfo));
}

void PriaveteGame::CreatePrivateCost(PrivateTableInfo* pTableInfo)
{
	if (pTableInfo->cbRoomType == Type_Private)
	{
		//mChen edit
		if (pTableInfo->cbPlayCostTypeIdex == 0)
		{
			//房主支付

			DBR_GR_Create_Private_Cost kNetInfo;
			kNetInfo.dwUserID = pTableInfo->dwCreaterUserID;
			kNetInfo.dwCost = pTableInfo->dwPlayCost;
			kNetInfo.dwCostType = CostType_InsureScore;// pTableInfo->cbRoomType;强制钻石消费
			m_pIDataBaseEngine->PostDataBaseRequest(0L, DBR_GR_CREATE_PRIVAT_COST, 0L, &kNetInfo, sizeof(kNetInfo));
		}
		else
		{
			//平均支付

			for (int i = 0; i < pTableInfo->pITableFrame->GetSitUserCount(); i++)
			{
				if (!pTableInfo->pITableFrame->GetTableUserItem(i))
				{
					continue;
				}
				DBR_GR_Create_Private_Cost kNetInfo;
				kNetInfo.dwUserID = pTableInfo->pITableFrame->GetTableUserItem(i)->GetUserID();
				//kNetInfo.dwCost = pTableInfo->dwPlayCost/4;
				kNetInfo.dwCost = pTableInfo->dwPlayCost;
				kNetInfo.dwCostType = CostType_InsureScore;// pTableInfo->cbRoomType;强制钻石消费
				m_pIDataBaseEngine->PostDataBaseRequest(0L, DBR_GR_CREATE_PRIVAT_COST, 0L, &kNetInfo, sizeof(kNetInfo));
			}
		}
		//DBR_GR_Create_Private_Cost kNetInfo;
		//kNetInfo.dwUserID = pTableInfo->dwCreaterUserID;
		//kNetInfo.dwCost = pTableInfo->dwPlayCost;
		//kNetInfo.dwCostType = pTableInfo->cbRoomType;
		//m_pIDataBaseEngine->PostDataBaseRequest(0L, DBR_GR_CREATE_PRIVAT_COST, 0L, &kNetInfo, sizeof(kNetInfo));

	}
	if (pTableInfo->cbRoomType == Type_Public)
	{
		//mChen note:金币场模拟比赛场

		for (int i = 0;i<pTableInfo->pITableFrame->GetSitUserCount();i++)
		{
			if (!pTableInfo->pITableFrame->GetTableUserItem(i))
			{
				continue;
			}
			DBR_GR_Create_Private_Cost kNetInfo;
			kNetInfo.dwUserID = pTableInfo->pITableFrame->GetTableUserItem(i)->GetUserID();
			kNetInfo.dwCost = pTableInfo->dwPlayCost;
			kNetInfo.dwCostType = CostType_InsureScore;// pTableInfo->cbRoomType;强制钻石消费
			m_pIDataBaseEngine->PostDataBaseRequest(0L,DBR_GR_CREATE_PRIVAT_COST,0L,&kNetInfo,sizeof(kNetInfo));
		}
	}

}

PrivateTableInfo* PriaveteGame::getTableInfoByRoomID(DWORD dwRoomID)
{
	for (int i = 0;i<m_pGameServiceOption->wTableCount;i++)
	{
		if (m_pTableInfo[i].dwRoomNum == dwRoomID)
		{
			return &m_pTableInfo[i];
		}
	}
	return NULL;
}

//mChen add
PrivateTableInfo* PriaveteGame::getTableInfoByRoomIDAndTypeIdex(DWORD dwRoomID,WORD cbGameTypeIdex)
{
	for (int i = 0; i < m_pGameServiceOption->wTableCount; i++)
	{
		if (m_pTableInfo[i].dwRoomNum == dwRoomID && m_pTableInfo[i].bGameTypeIdex == cbGameTypeIdex)
		{
			return &m_pTableInfo[i];
		}
	}
	return NULL;
}

PrivateTableInfo* PriaveteGame::getTableInfoByCreaterID(DWORD dwUserID)
{
	for (int i = 0;i<m_pGameServiceOption->wTableCount;i++)
	{
		if (m_pTableInfo[i].dwCreaterUserID == dwUserID)
		{
			return &m_pTableInfo[i];
		}
	}
	return NULL;
}
PrivateTableInfo* PriaveteGame::getTableInfoByTableID(DWORD dwRoomID)
{

	for (int i = 0;i<m_pGameServiceOption->wTableCount;i++)
	{
		if (m_pTableInfo[i].pITableFrame && m_pTableInfo[i].pITableFrame->GetTableID() == dwRoomID)
		{
			return &m_pTableInfo[i];
		}
	}
	return NULL;
}
PrivateTableInfo* PriaveteGame::getTableInfoByTableFrame(ITableFrame* pTableFrame)
{
	for (int i = 0;i<m_pGameServiceOption->wTableCount;i++)
	{
		if (m_pTableInfo[i].pITableFrame == pTableFrame)
		{
			return &m_pTableInfo[i];
		}
	}
	return NULL;
}

//数据库事件
bool PriaveteGame::OnEventDataBase(WORD wRequestID, IServerUserItem * pIServerUserItem, VOID * pData, WORD wDataSize)
{
	switch (wRequestID)
	{
	case DBO_GR_PRIVATE_INFO:		//私人场信息
		{
			//参数效验
			if(wDataSize>sizeof(DBO_GR_Private_Info)) return false;

			//提取数据
			DBO_GR_Private_Info * pPrivate = (DBO_GR_Private_Info*)pData;
			m_kPrivateInfo.wKindID = pPrivate->wKindID;
			m_kPrivateInfo.lCostGold = pPrivate->lCostGold;
			memcpy(&m_kPrivateInfo.bPlayCout,pPrivate->bPlayCout,sizeof(m_kPrivateInfo.bPlayCout));
			memcpy(&m_kPrivateInfo.lPlayCost,pPrivate->lPlayCost,sizeof(m_kPrivateInfo.lPlayCost));
			memcpy(&m_kPrivateInfo.lPlayAvgCost, pPrivate->lPlayAvgCost, sizeof(m_kPrivateInfo.lPlayAvgCost));

			//mChen add,金币场模拟比赛场
			m_kPrivateInfo.cbMatchPlayCout = pPrivate->cbMatchPlayCout;
			m_kPrivateInfo.MatchStartTime = pPrivate->MatchStartTime;
			m_kPrivateInfo.MatchEndTime = pPrivate->MatchEndTime;

			break;
		}
	case DBO_GR_CREATE_PRIVATE:		//私人场信息
		{
			if (pIServerUserItem == NULL) return false;

			bool bCreteSuccess = OnEventCreatePrivate(wRequestID,pIServerUserItem,pData,wDataSize,"");
			if(bCreteSuccess == false)
			{
				DBR_GR_Jion_Failed joinFalied;
				joinFalied.wServerID = m_pGameServiceOption->wServerID;
				joinFalied.dwUserID = pIServerUserItem->GetUserID();
				m_pIDataBaseEngine->PostDataBaseRequest(pIServerUserItem->GetUserID(), DBR_GR_JOIN_FAILED, wRequestID, &joinFalied, sizeof(DBR_GR_Jion_Failed));
			}
			break;
		}
	case DBO_GR_CHECK_JOIN_PRIVATE:		//私人场AA房间加入check返回信息
	{
		//参数效验
		if (pIServerUserItem == NULL) return false;
		if (wDataSize>sizeof(DBO_GR_CreatePrivateResoult)) return false;

		//提取数据
		DBO_GR_CreatePrivateResoult* pPrivate = (DBO_GR_CreatePrivateResoult*)pData;
		DWORD dwAgaginTable = pPrivate->dwAgaginTable;//存储想要加入房间的TableId
		
		//寻找位置
		ITableFrame * pICurrTableFrame = NULL;
		PrivateTableInfo* pCurrTableInfo = NULL;
		if (dwAgaginTable != INVALID_DWORD)
		{
			pCurrTableInfo = getTableInfoByTableID(dwAgaginTable);
			if (!pCurrTableInfo)
			{
				return false;
			}
			pICurrTableFrame = pCurrTableInfo->pITableFrame;
		}
		
		//桌子判断
		if (pICurrTableFrame == NULL)
		{
			DBR_GR_Jion_Failed joinFalied;
			joinFalied.wServerID = m_pGameServiceOption->wServerID;
			joinFalied.dwUserID = pIServerUserItem->GetUserID();
			m_pIDataBaseEngine->PostDataBaseRequest(pIServerUserItem->GetUserID(), DBR_GR_JOIN_FAILED, wRequestID, &joinFalied, sizeof(DBR_GR_Jion_Failed));
			m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem, TEXT("寻找桌子失败。"), SMT_EJECT);
			return false;
		}

		if (!joinPrivateRoom(pIServerUserItem, pICurrTableFrame))
		{
			DBR_GR_Jion_Failed joinFalied;
			joinFalied.wServerID = m_pGameServiceOption->wServerID;
			joinFalied.dwUserID = pIServerUserItem->GetUserID();
			m_pIDataBaseEngine->PostDataBaseRequest(pIServerUserItem->GetUserID(), DBR_GR_JOIN_FAILED, wRequestID, &joinFalied, sizeof(DBR_GR_Jion_Failed));
			m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem, TEXT("加入房间失败1。"), SMT_EJECT);
			return false;
		}

		break;
	}
	}
	return true;
}

bool PriaveteGame::joinPrivateRoom(IServerUserItem * pIServerUserItem,ITableFrame * pICurrTableFrame)
{
	WORD wGaveInChairID = INVALID_CHAIR;
	for (WORD i=0;i<pICurrTableFrame->GetChairCount();i++)
	{
		if (pICurrTableFrame->GetTableUserItem(i)==pIServerUserItem)
		{
			wGaveInChairID = i;
			break;
		}
	}

	PrivateTableInfo* pTableInfo = getTableInfoByTableFrame(pICurrTableFrame);
	BYTE cbRoomType = Type_Private;
	if (pTableInfo != NULL)
	{
		cbRoomType = pTableInfo->cbRoomType;
	}

	if (wGaveInChairID!=INVALID_CHAIR)
	{
		//mChen note:蓄房进这里

		pIServerUserItem->SetUserStatus(US_READY,pICurrTableFrame->GetTableID(),wGaveInChairID);

		//mChen add, for HideSeek
		///RandomUserTeamTypeAndModelIndex(pIServerUserItem, pICurrTableFrame->GetMapIndexRand(), pICurrTableFrame->GetChoosedMapIndex(), true);//移到了HideSeek_FreeGameAndTimers，因为所有用户都必须提前全设置完
		pICurrTableFrame->SendUsersInfoPacket(pIServerUserItem);
		return true;
	}

	WORD wChairID = INVALID_CHAIR;
	//椅子搜索
	for (WORD i=0;i<pICurrTableFrame->GetChairCount();i++)
	{
		if (pICurrTableFrame->GetTableUserItem(i)==NULL)
		{
			wChairID = i;
			break;
		}
	}

	//分配用户
	if (wChairID!=INVALID_CHAIR)
	{
		//用户起立
		if (pIServerUserItem->GetTableID()!=INVALID_TABLE)
		{
			//旁观者蓄房会进这里

			//mChen edit, for HideSeek：fix旁观者无法蓄房的bug
			if (pIServerUserItem->GetUserStatus() != US_LOOKON)
			{
				// Log
				TCHAR szString[256] = TEXT("");
				_sntprintf(szString, CountArray(szString), TEXT("joinPrivateRoom:pIServerUserItem->wTableID!=INVALID_TABLE: userId=%d, tableId=%d"), pIServerUserItem->GetUserInfo()->dwUserID, pIServerUserItem->GetTableID());
				CTraceService::TraceString(szString, TraceLevel_Normal);

				return pIServerUserItem->GetTableID() == pICurrTableFrame->GetTableID();
			}
			//else
			//{
			//	pICurrTableFrame->PerformStandUpAction(pIServerUserItem);
			//}
			///return pIServerUserItem->GetTableID() == pICurrTableFrame->GetTableID();
		}

		//用户坐下
		if(pICurrTableFrame->PerformSitDownAction(wChairID,pIServerUserItem)==false)
		{
			return false;
		}

		//mChen edit, for HideSeek: 警察无敌时间之前加入的玩家，都分配阵营，只有在这之后加入的才是旁观者: fix不同步bug
		if (pICurrTableFrame->IsGameStarted())
		{
			pIServerUserItem->SetUserStatus(US_PLAYING, pICurrTableFrame->GetTableID(), wChairID);
		}
		else
		{
#if ADD_READY_STEP
			//mChen edit, add Press Ready step
			pIServerUserItem->SetUserStatus(US_SIT, pICurrTableFrame->GetTableID(), wChairID);
			//pIServerUserItem->SetUserStatus(US_READY,pICurrTableFrame->GetTableID(),wChairID);
#else
			pIServerUserItem->SetUserStatus(US_READY, pICurrTableFrame->GetTableID(), wChairID);
#endif
		}

		//mChen add, for HideSeek
		RandomUserTeamTypeAndModelIndex(pIServerUserItem, pICurrTableFrame->GetMapIndexRand(), pICurrTableFrame->GetChoosedMapIndex(), false);

		return true;
	}	
	return false;
}

//mChen add, for HideSeek
void PriaveteGame::RandomUserTeamTypeAndModelIndex(IServerUserItem * pIServerUserItem, BYTE cbMapIndexRand, BYTE cbChoosedMapIndex, bool bIsXuFang)
{
	if (pIServerUserItem == NULL) return;

	////MapIndexRand
	//if (cbRoomType == Type_Private)
	//{
	//	pIServerUserItem->SetMapIndexRand(cbChoosedMapIndex);
	//}
	//else
	//{
	//	pIServerUserItem->SetMapIndexRand(cbMapIndexRand);
	//}

	// TeamTyp
	PlayerTeamType teamType;
	int iRand = rand() % 100;
	if (iRand < 35) 
	{
		teamType = PlayerTeamType::TaggerTeam;
	}
	else
	{
		teamType = PlayerTeamType::HideTeam;
	}
	//if (!bIsXuFang)
	//{
	//	//第一个进去的人是警察：会导致房主退出后再进一直是警察
	//	//DWORD dwSitUserCount = pTableInfo->pITableFrame->GetSitUserCount();
	//	//if(dwSitUserCount==0)
	//	WORD wChairID = pIServerUserItem->GetChairID();
	//	if (wChairID == 0)
	//	{
	//		teamType = PlayerTeamType::TaggerTeam;
	//	}
	//}
	//teamType = PlayerTeamType::HideTeam;
	pIServerUserItem->SetPlayerTeamType(teamType);

	// ModelIndex
	tagUserInfo * pUserInfo = pIServerUserItem->GetUserInfo();
	if (pUserInfo->cbTeamType == PlayerTeamType::HideTeam)
	{
		BYTE cbModelIndex = rand() % 0xFF;
		pIServerUserItem->SetModelIndex(cbModelIndex);

		// Log
		TCHAR szString[128] = TEXT("");
		_sntprintf(szString, CountArray(szString), TEXT("CTableFrame::PriaveteGame::RandomUserTeamTypeAndModelIndex: HideTeam cbModelIndex=%d, dwUserID=%d"), cbModelIndex, pUserInfo->dwUserID);
		CTraceService::TraceString(szString, TraceLevel_Normal);
	}
	else
	{
		//警察角色的ModelIndex由客户端上传
		BYTE cbModelIndex = pUserInfo->cbChoosedModelIndexOfTagger;
		pIServerUserItem->SetModelIndex(cbModelIndex);
	}

}
void PriaveteGame::ForceDismissAndClearRoom(WORD wTableId)
{
	if (wTableId >= m_pGameServiceOption->wTableCount)
	{
		//Log
		TCHAR szString[128] = TEXT("");
		_sntprintf(szString, CountArray(szString), TEXT("ForceDismissAndClearRoom: incorrect wTableId=%d"), wTableId);
		CTraceService::TraceString(szString, TraceLevel_Warning);

		return;
	}

	PrivateTableInfo* pTableInfo = &m_pTableInfo[wTableId];
	if (pTableInfo)
	{
		DismissRoom(pTableInfo, GER_DISMISS);
		ClearRoom(pTableInfo);
	}
}

//创建房间
bool PriaveteGame::OnEventCreatePrivate(WORD wRequestID, IServerUserItem * pIServerUserItem, VOID * pData, WORD wDataSize,std::string kChannel)
{
	//参数效验
	if(pIServerUserItem==NULL) return false;
	if(wDataSize>sizeof(DBO_GR_CreatePrivateResoult)) return false;

	//提取数据
	DBO_GR_CreatePrivateResoult* pPrivate = (DBO_GR_CreatePrivateResoult*)pData;	
	DWORD dwAgaginTable = pPrivate->dwAgaginTable;
	//报名失败
	if(pPrivate->bSucess==false)
	{
		//mChen edit for HideSeek: for蓄房失败（钻石不足）:fix继续游戏报钻石不够创建房间:强制用户离开游戏
		if (dwAgaginTable != INVALID_DWORD)
		{
			m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem, pPrivate->szDescribeString, SMT_EJECT | SMT_CLOSE_GAME);

			PrivateTableInfo* pCurrTableInfo = getTableInfoByTableID(dwAgaginTable);
			if (pCurrTableInfo != NULL)
			{
				ITableFrame * pICurrTableFrame = pCurrTableInfo->pITableFrame;
				if (pICurrTableFrame != NULL && pIServerUserItem->GetTableID() != INVALID_TABLE)
				{
					//自己退出房间，否则会离线在房间中
					pICurrTableFrame->PerformStandUpActionEx(pIServerUserItem);
				}
			}
		}
		else
		{
			m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem, pPrivate->szDescribeString, SMT_EJECT);
		}
		///m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem,pPrivate->szDescribeString,SMT_EJECT);

		return false;
	}
	if (pPrivate->bPlayCoutIdex < 0 || pPrivate->bPlayCoutIdex >= 4)
	{
		return false;
	}

	//寻找位置
	ITableFrame * pICurrTableFrame=NULL;
	PrivateTableInfo* pCurrTableInfo=NULL;
	if (dwAgaginTable != INVALID_DWORD)
	{
		pCurrTableInfo = getTableInfoByTableID(dwAgaginTable);
		if (!pCurrTableInfo)
		{
			return false;
		}
		pICurrTableFrame = pCurrTableInfo->pITableFrame;
		if (pCurrTableInfo->bInEnd == false)
		{
			return joinPrivateRoom(pIServerUserItem, pICurrTableFrame);
		}
		else
		{
			pCurrTableInfo->restAgainValue();
			sendPrivateRoomInfo(NULL,pCurrTableInfo);
		}
	}
	else
	{
		WORD wTableID = 0;
		for (wTableID=0;wTableID<m_pGameServiceOption->wTableCount;wTableID++)
		{
			//获取对象
			ASSERT(m_pTableInfo[wTableID].pITableFrame!=NULL);
			ITableFrame * pITableFrame=m_pTableInfo[wTableID].pITableFrame;
			if(m_pTableInfo[wTableID].bInEnd)
			{
				continue;
			}
			//状态判断
			WORD wNullChairCount = pITableFrame->GetNullChairCount();
			WORD wChairCount = pITableFrame->GetChairCount();
			if (pITableFrame->GetNullChairCount()==pITableFrame->GetChairCount())
			{
				pCurrTableInfo = &m_pTableInfo[wTableID];

				//mChen add, for代开房：fix每次代开房会使用上次的代开房间，导致每个人只有最后一次代开的的那个房间是有效的
				if (pCurrTableInfo->dwCreaterUserID != 0)
				{
					//该房间有代开房房主

					continue;
				}

				pICurrTableFrame = pITableFrame;
				pCurrTableInfo->restValue();
				break;
			}
		}
		if (getTableInfoByCreaterID(pIServerUserItem->GetUserID()))
		{
			//自己已经是某个房间的房主

			//mChen comment, for代开房：fix有人加入自己的代开房后，自己无法再创建房间
			///return false;
		}
	}
	//桌子判断
	if(pICurrTableFrame==NULL)
	{
		m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem,TEXT("寻找桌子失败。"),SMT_EJECT);
		return false;
	}

	//mChen add, for HideSeek
	//SetMapIndexRand must be called before joinPrivateRoom
	if (pPrivate->cbRoomType == Type_Private)
	{
		//创建房间

		pICurrTableFrame->SetChoosedMapIndex(pPrivate->cbChoosedMapIndex);
		pICurrTableFrame->SetMapIndexRand(pPrivate->cbChoosedMapIndex);
	}

	if(!joinPrivateRoom(pIServerUserItem,pICurrTableFrame))
	{
		m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem,TEXT("加入房间失败2。"),SMT_EJECT);
		return false;
	}
	int iRandNum = 1000+rand()%8900;
	while (getTableInfoByRoomID(iRandNum))
	{
		iRandNum = 1000+rand()%8900;
	}
	if (m_pGameServiceOption->wServerID >= 90)
	{
		ASSERT(false);
		return false;
	}
	iRandNum = (m_pGameServiceOption->wServerID+10)*10000+iRandNum;
	pCurrTableInfo->dwPlayCout = (DWORD)m_kPrivateInfo.bPlayCout[pPrivate->bPlayCoutIdex];
	if (pPrivate->cbRoomType == Type_Private)
	{
		//创建房间

		if(pPrivate->cbPlayCostTypeIdex == 1)//AA支付
		{
			pCurrTableInfo->dwPlayCost = (DWORD)m_kPrivateInfo.lPlayAvgCost[pPrivate->bPlayCoutIdex];			
		}
		else if(pPrivate->cbPlayCostTypeIdex == 0)//房主支付
		{
			pCurrTableInfo->dwPlayCost = (DWORD)m_kPrivateInfo.lPlayCost[pPrivate->bPlayCoutIdex];
		}

		//mChen add, for HideSeek
		//在收到房主的SUB_GF_CREATER_PRESS_START消息时才调用StartWaitGame
		///pICurrTableFrame->StartWaitGame();
	}
	else
	{
		//自由匹配的创建房间

		//mChen add:金币场模拟比赛场，局数设置
		pCurrTableInfo->dwPlayCout = (DWORD)m_kPrivateInfo.cbMatchPlayCout;

		pCurrTableInfo->dwPlayCost = (DWORD)m_kPrivateInfo.lCostGold;

		//mChen add, for HideSeek
		if (dwAgaginTable == INVALID_DWORD)
		{
			//非蓄房

			pICurrTableFrame->StartWaitGame();
		}
	}
	
	if (pPrivate->cbRoomType == Type_Private && dwAgaginTable != INVALID_DWORD)
	{
		//创建房间的蓄房，不更改房间号
	}
	else
	{
		pCurrTableInfo->setRoomNum(iRandNum);
	}
	pCurrTableInfo->dwCreaterUserID = pIServerUserItem->GetUserID();
	pCurrTableInfo->kHttpChannel = kChannel;
	pCurrTableInfo->cbRoomType = pPrivate->cbRoomType;

	pCurrTableInfo->bGameRuleIdex = pPrivate->bGameRuleIdex;
	pCurrTableInfo->bGameTypeIdex = pPrivate->bGameTypeIdex;
	pCurrTableInfo->bPlayCoutIdex = pPrivate->bPlayCoutIdex;
	GetLocalTime(&pCurrTableInfo->kTotalRecord.kPlayTime);

	//mChen add
	pCurrTableInfo->cbPlayCostTypeIdex = pPrivate->cbPlayCostTypeIdex;
	pCurrTableInfo->lBaseScore = pPrivate->lBaseScore;
	//ZY add
	pCurrTableInfo->PlayerCount = pPrivate->PlayerCount;

	pICurrTableFrame->SetPrivateInfo(pCurrTableInfo->bGameTypeIdex,pCurrTableInfo->bGameRuleIdex, pCurrTableInfo->lBaseScore,pCurrTableInfo->dwPlayCout,pCurrTableInfo->PlayerCount);

	CMD_GF_Create_Private_Sucess kSucessInfo;
	kSucessInfo.lCurSocre = pPrivate->bSucess;
	kSucessInfo.dwRoomNum = pCurrTableInfo->dwRoomNum;
	SendData(pIServerUserItem,MDM_GR_PRIVATE,SUB_GR_CREATE_PRIVATE_SUCESS,&kSucessInfo,sizeof(kSucessInfo));


	sendPrivateRoomInfo(NULL,pCurrTableInfo);
	return true;
}

	//创建私人场
bool PriaveteGame::OnTCPNetworkSubCreatePrivate(VOID * pData, WORD wDataSize, IServerUserItem * pIServerUserItem, DWORD dwSocketID)
{
	//参数效验
	ASSERT(wDataSize==sizeof(CMD_GR_Create_Private));
	if(wDataSize!=sizeof(CMD_GR_Create_Private)) return false;

	CMD_GR_Create_Private* pCMDInfo = (CMD_GR_Create_Private*)pData;
	pCMDInfo->stHttpChannel[CountArray(pCMDInfo->stHttpChannel)-1]=0;

	if (pIServerUserItem->GetTableID() != INVALID_TABLE)
	{
		//房卡模式，房主未点开始就杀进程或者断线，回来再创建房间会进这里？

		// Log
		TCHAR szString[128] = TEXT("");
		_sntprintf(szString, CountArray(szString), TEXT("OnTCPNetworkSubCreatePrivate: pIServerUserItem->GetTableID() != INVALID_TABLE"));
		CTraceService::TraceString(szString, TraceLevel_Exception);

		//mChen add, for HideSeek,fix无法创建房间
		OnEventReqStandUP(pIServerUserItem, true);

		return true;
	}

	// mChen add, for HideSeek
	pIServerUserItem->SetChoosedModelIndexOfTagger(pCMDInfo->cbChoosedModelIndex);
	pIServerUserItem->SetModelIndex(pCMDInfo->cbChoosedModelIndex);

	if(pCMDInfo->cbGameType == Type_Private)
	{
		//mChen note: Client Button_CreateRoom

		DBR_GR_Create_Private kDBRInfo;
		kDBRInfo.dwUserID = pIServerUserItem->GetUserID();
		kDBRInfo.wKindID = m_pGameServiceAttrib->wKindID;
		kDBRInfo.cbRoomType = Type_Private;
		kDBRInfo.dwCostType = Type_Private;
		kDBRInfo.dwAgaginTable = INVALID_DWORD;
		if(pCMDInfo->cbPlayCostTypeIdex == 1)//AA支付
		{
			kDBRInfo.dwCost = (DWORD)m_kPrivateInfo.lPlayAvgCost[pCMDInfo->bPlayCoutIdex];
		}
		else if(pCMDInfo->cbPlayCostTypeIdex == 0)//房主支付
		{
			kDBRInfo.dwCost = (DWORD)m_kPrivateInfo.lPlayCost[pCMDInfo->bPlayCoutIdex];
		}
		kDBRInfo.bPlayCoutIdex = pCMDInfo->bPlayCoutIdex;
		kDBRInfo.bGameRuleIdex = pCMDInfo->bGameRuleIdex;
		kDBRInfo.bGameTypeIdex = pCMDInfo->bGameTypeIdex;

		//mChen add
		kDBRInfo.cbPlayCostTypeIdex = pCMDInfo->cbPlayCostTypeIdex;
		kDBRInfo.lBaseScore = pCMDInfo->lBaseScore;
		//ZY add
		kDBRInfo.PlayerCount = pCMDInfo->PlayerCount;

		//mChen add, for HideSeek
		kDBRInfo.cbChoosedMapIndex = pCMDInfo->cbChoosedMapIndex;

		DBR_CreatePrivate(&kDBRInfo,dwSocketID,pIServerUserItem,pCMDInfo->stHttpChannel);
	}
	else
	{
		//mChen note: Client Button_JoinPublic

		////mChen add, 金币场模拟比赛场
		////Verify Time
		//CTime CurTime = CTime::GetCurrentTime();
		//CTime MatchStartTime(m_kPrivateInfo.MatchStartTime);
		//CTime MatchEndTime(m_kPrivateInfo.MatchEndTime);
		//DWORD dwCurrStamp = CurTime.GetHour() * 3600 + CurTime.GetMinute() * 60 + CurTime.GetSecond();
		//DWORD dwStartStamp = MatchStartTime.GetHour() * 3600 + MatchStartTime.GetMinute() * 60 + MatchStartTime.GetSecond();
		//DWORD dwEndStamp = MatchEndTime.GetHour() * 3600 + MatchEndTime.GetMinute() * 60 + MatchEndTime.GetSecond();
		//if (dwCurrStamp < dwStartStamp)
		//{
		//	//比赛未开始
		//	TCHAR szMessage[128] = TEXT("");
		//	_sntprintf(szMessage, CountArray(szMessage), TEXT("今天的比赛尚未开始,请您于%d时%d分%d秒前来参加比赛！"), MatchStartTime.GetHour(), MatchStartTime.GetMinute(), MatchStartTime.GetSecond());
		//	m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem, szMessage, SMT_EJECT | SMT_CHAT);
		//	return true;
		//}
		//if (dwCurrStamp > dwEndStamp)
		//{
		//	//比赛已结束
		//	m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem, TEXT("今天的比赛已经结束，请您明天再来参加比赛。"), SMT_EJECT | SMT_CHAT);
		//	return true;
		//}

		ITableFrame * pICurrTableFrame=NULL;
		PrivateTableInfo* pCurrTableInfo=NULL;
		WORD wTableID = 0;
		for (wTableID=0;wTableID<m_pGameServiceOption->wTableCount;wTableID++)
		{
			//获取对象
			PrivateTableInfo& pTableInfo = m_pTableInfo[wTableID];
			ASSERT(pTableInfo.pITableFrame!=NULL);
			ITableFrame * pITableFrame=pTableInfo.pITableFrame;
			if(pTableInfo.bInEnd)
			{
				continue;
			}
			if(pTableInfo.cbRoomType != Type_Public)
			{
				continue;
			}
			if (m_pTableInfo[wTableID].bGameRuleIdex != pCMDInfo->bGameRuleIdex 
				|| m_pTableInfo[wTableID].bGameTypeIdex != pCMDInfo->bGameTypeIdex )
			{
				continue;
			}
			//状态判断
			if (pITableFrame->GetNullChairCount() >= 0)
			{
				pICurrTableFrame = pITableFrame;
				pCurrTableInfo = &m_pTableInfo[wTableID];
				break;
			}
		}
		if (pICurrTableFrame == NULL)
		{
			DBR_GR_Create_Private kDBRInfo;
			kDBRInfo.dwUserID = pIServerUserItem->GetUserID();
			kDBRInfo.wKindID = m_pGameServiceAttrib->wKindID;
			kDBRInfo.cbRoomType = Type_Public;
			kDBRInfo.dwCostType = Type_Public;
			kDBRInfo.dwAgaginTable = INVALID_DWORD;
			//kDBRInfo.dwCost = (DWORD)m_kPrivateInfo.lPlayCost[pCMDInfo->bPlayCoutIdex];
			kDBRInfo.dwCost = 0;   //匹配模式无费用
			kDBRInfo.bPlayCoutIdex = pCMDInfo->bPlayCoutIdex;
			kDBRInfo.bGameRuleIdex = pCMDInfo->bGameRuleIdex;
			kDBRInfo.bGameTypeIdex = pCMDInfo->bGameTypeIdex;

			//mChen add
			kDBRInfo.cbPlayCostTypeIdex = pCMDInfo->cbPlayCostTypeIdex;
			kDBRInfo.lBaseScore = pCMDInfo->lBaseScore;
			//ZY add
			kDBRInfo.PlayerCount = pCMDInfo->PlayerCount;

			DBR_CreatePrivate(&kDBRInfo,dwSocketID,pIServerUserItem,"");
		}
		else
		{
			joinPrivateRoom(pIServerUserItem,pICurrTableFrame);
		}
	}
	return true;
}
//重新加入私人场
bool PriaveteGame::OnTCPNetworkSubAgainEnter(VOID * pData, WORD wDataSize, IServerUserItem * pIServerUserItem, DWORD dwSocketID)
{
	//参数效验
	ASSERT(wDataSize==sizeof(CMD_GR_Again_Private));
	if (wDataSize != sizeof(CMD_GR_Again_Private))
	{
		// Log
		CTraceService::TraceString(TEXT("OnTCPNetworkSubAgainEnter: incorrect wDataSize"), TraceLevel_Exception);

		return false;
	}

	CMD_GR_Again_Private* pCMDInfo = (CMD_GR_Again_Private*)pData;
	pCMDInfo->stHttpChannel[CountArray(pCMDInfo->stHttpChannel)-1] = 0;

	PrivateTableInfo* pTableInfo = getTableInfoByTableID(pIServerUserItem->GetTableID());
	if (!pTableInfo)
	{
		// Log
		CTraceService::TraceString(TEXT("OnTCPNetworkSubAgainEnter: pTableInfo==null"), TraceLevel_Exception);

		return true;
	}


	//mChen add, for HideSeek:所有在线的人都蓄房后才开始游戏
	WORD wChairID = pIServerUserItem->GetChairID();
	IServerUserItem * pITableUserItem = pTableInfo->pITableFrame->GetTableUserItem(wChairID);
	BYTE cbUserStatus = pIServerUserItem->GetUserStatus();
	//if (pIServerUserItem == pITableUserItem)
	if (cbUserStatus == US_LOOKON)
	{
		pTableInfo->m_cbAgainLookonNum++;
	}
	else
	{
		//pIServerUserItem不是旁观者
		pTableInfo->m_cbAgainHumanNum++;
	}
	WORD wPlayingUserCount = pTableInfo->pITableFrame->GetPlayingUserCount();
	// Log
	TCHAR szString[128] = TEXT("");
	_sntprintf(szString, CountArray(szString), TEXT("OnTCPNetworkSubAgainEnter: userId=%d, AgainHumanNum=%d, AgainLookonNum=%d, wPlayingUserCount=%d, tableId=%d, roomNum=%d"), pIServerUserItem->GetUserInfo()->dwUserID, pTableInfo->m_cbAgainHumanNum, pTableInfo->m_cbAgainLookonNum, wPlayingUserCount, pTableInfo->pITableFrame->GetTableID(), pTableInfo->dwRoomNum);
	CTraceService::TraceString(szString, TraceLevel_Normal);
	if (cbUserStatus != US_LOOKON)
	{
		if (pTableInfo->m_cbAgainHumanNum + pTableInfo->m_cbAgainLookonNum == wPlayingUserCount)
		{
			pTableInfo->pITableFrame->StartWaitGame();
		}
	}

	if (!pTableInfo->bInEnd && pTableInfo->dwRoomNum != 0)
	{
		//第1个以后的蓄房者都进这里

		joinPrivateRoom(pIServerUserItem,pTableInfo->pITableFrame);
		return true;
	}

	DBR_GR_Create_Private kDBRInfo;
	ZeroMemory(&kDBRInfo,sizeof(kDBRInfo));
	kDBRInfo.cbRoomType = pTableInfo->cbRoomType;
	kDBRInfo.dwUserID = pIServerUserItem->GetUserID();
	kDBRInfo.wKindID = m_pGameServiceAttrib->wKindID;
	kDBRInfo.dwCost = (DWORD)m_kPrivateInfo.lPlayCost[pTableInfo->bPlayCoutIdex];
	kDBRInfo.dwAgaginTable = pIServerUserItem->GetTableID();

	//mChen add
	kDBRInfo.cbPlayCostTypeIdex = pTableInfo->cbPlayCostTypeIdex;

	//mChen add, for蓄房
	kDBRInfo.bPlayCoutIdex = pTableInfo->bPlayCoutIdex;
	kDBRInfo.bGameTypeIdex = pTableInfo->bGameTypeIdex;
	kDBRInfo.bGameRuleIdex = pTableInfo->bGameRuleIdex;
	kDBRInfo.lBaseScore = pTableInfo->lBaseScore;
	//ZY add
	kDBRInfo.PlayerCount = pTableInfo->PlayerCount;

	////mChen add, for HideSeek：旁观者蓄房
	//if (pIServerUserItem->GetUserStatus() == US_LOOKON)
	//{
	//	pTableInfo->pITableFrame->PerformStandUpAction(pIServerUserItem);
	//}

	//mChen add, for HideSeek：fix房卡房蓄房后mapindex不对的bug
	if (pTableInfo->cbRoomType == Type_Private)
	{
		kDBRInfo.cbChoosedMapIndex = pTableInfo->pITableFrame->GetChoosedMapIndex();
	}

	DBR_CreatePrivate(&kDBRInfo,dwSocketID,pIServerUserItem,pCMDInfo->stHttpChannel);

	return true;
}
//加入私人场
bool PriaveteGame::OnTCPNetworkSubJoinPrivate(VOID * pData, WORD wDataSize, IServerUserItem * pIServerUserItem, DWORD dwSocketID)
{
	//mChen note: Client Button_Join_Choose(Button_Join_Private)

	//参数效验
	ASSERT(wDataSize==sizeof(CMD_GR_Join_Private));
	if(wDataSize!=sizeof(CMD_GR_Join_Private)) return false;

	CMD_GR_Join_Private* pCMDInfo = (CMD_GR_Join_Private*)pData;

	PrivateTableInfo* pTableInfo = getTableInfoByRoomID(pCMDInfo->dwRoomNum);
	///PrivateTableInfo* pTableInfo = getTableInfoByRoomIDAndTypeIdex(pCMDInfo->dwRoomNum, pCMDInfo->cbGameTypeIdex);

	if (!pTableInfo)
	{
		m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem,TEXT("对不起，没有找到该房间，可能房主已经退出。"),SMT_EJECT|SMT_CHAT);
		DBR_GR_Jion_Failed joinFalied;
		joinFalied.wServerID = m_pGameServiceOption->wServerID;
		joinFalied.dwUserID = pIServerUserItem->GetUserID();
		m_pIDataBaseEngine->PostDataBaseRequest(pIServerUserItem->GetUserID(), DBR_GR_JOIN_FAILED, dwSocketID, &joinFalied, sizeof(DBR_GR_Jion_Failed));
		return true;
	}

	//mChen add
	if (pTableInfo->bGameTypeIdex != pCMDInfo->cbGameTypeIdex)
	{
		m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem, TEXT("对不起，您输入的房间号不对。"), SMT_EJECT | SMT_CHAT);//您输入的房间号不是私有房间号
		DBR_GR_Jion_Failed joinFalied;
		joinFalied.wServerID = m_pGameServiceOption->wServerID;
		joinFalied.dwUserID = pIServerUserItem->GetUserID();
		m_pIDataBaseEngine->PostDataBaseRequest(pIServerUserItem->GetUserID(), DBR_GR_JOIN_FAILED, dwSocketID, &joinFalied, sizeof(DBR_GR_Jion_Failed));
		return true;
	}

	//mChen add
	if (pTableInfo->cbRoomType != Type_Private)
	{
		DBR_GR_Jion_Failed joinFalied;
		joinFalied.wServerID = m_pGameServiceOption->wServerID;
		joinFalied.dwUserID = pIServerUserItem->GetUserID();
		m_pIDataBaseEngine->PostDataBaseRequest(pIServerUserItem->GetUserID(), DBR_GR_JOIN_FAILED, dwSocketID, &joinFalied, sizeof(DBR_GR_Jion_Failed));
		m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem, TEXT("对不起，您输入的房间号不对。"), SMT_EJECT | SMT_CHAT);//您输入的房间号不是私有房间号
		return true;
	}

	if (pTableInfo->pITableFrame->GetNullChairCount() <= 0)
	{
		DBR_GR_Jion_Failed joinFalied;
		joinFalied.wServerID = m_pGameServiceOption->wServerID;
		joinFalied.dwUserID = pIServerUserItem->GetUserID();
		m_pIDataBaseEngine->PostDataBaseRequest(pIServerUserItem->GetUserID(), DBR_GR_JOIN_FAILED, dwSocketID, &joinFalied, sizeof(DBR_GR_Jion_Failed));
		m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem,TEXT("对不起，游戏人数已满，无法加入。"),SMT_EJECT|SMT_CHAT);
		return true;
	}
	//if (pTableInfo->bStart || pTableInfo->bInEnd)
	//{
	//	DBR_GR_Jion_Failed joinFalied;
	//	joinFalied.wServerID = m_pGameServiceOption->wServerID;
	//	joinFalied.dwUserID = pIServerUserItem->GetUserID();
	//	m_pIDataBaseEngine->PostDataBaseRequest(pIServerUserItem->GetUserID(), DBR_GR_JOIN_FAILED, dwSocketID, &joinFalied, sizeof(DBR_GR_Jion_Failed));
	//	m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem,TEXT("对不起，游戏已经开始，无法加入。"),SMT_EJECT|SMT_CHAT);
	//	return true;
	//}

	// mChen add, for HideSeek
	pIServerUserItem->SetChoosedModelIndexOfTagger(pCMDInfo->cbChoosedModelIndex);
	pIServerUserItem->SetModelIndex(pCMDInfo->cbChoosedModelIndex);

//林：加入AA支付的房间需要检测是否有足够的钻石
	if(pTableInfo->cbPlayCostTypeIdex == 0)
	{
		if (!joinPrivateRoom(pIServerUserItem, pTableInfo->pITableFrame))
		{
			if (pIServerUserItem->GetUserStatus() != US_LOOKON)
			{
				DBR_GR_Jion_Failed joinFalied;
				joinFalied.wServerID = m_pGameServiceOption->wServerID;
				joinFalied.dwUserID = pIServerUserItem->GetUserID();
				m_pIDataBaseEngine->PostDataBaseRequest(pIServerUserItem->GetUserID(), DBR_GR_JOIN_FAILED, dwSocketID, &joinFalied, sizeof(DBR_GR_Jion_Failed));
				m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem, TEXT("加入房间失败3。"), SMT_EJECT);
			}
		}
	}
	else if(pTableInfo->cbPlayCostTypeIdex == 1)//AA支付（使用创建房间的检测）
	{
		DBR_GR_Create_Private kDBRInfo;
		ZeroMemory(&kDBRInfo, sizeof(kDBRInfo));
		kDBRInfo.cbRoomType = pTableInfo->cbRoomType;
		kDBRInfo.dwUserID = pIServerUserItem->GetUserID();
		kDBRInfo.wKindID = m_pGameServiceAttrib->wKindID;
		kDBRInfo.dwCost = pTableInfo->dwPlayCost;
		kDBRInfo.dwAgaginTable = pTableInfo->pITableFrame->GetTableID();

		//mChen add
		kDBRInfo.cbPlayCostTypeIdex = pTableInfo->cbPlayCostTypeIdex;

		//mChen add, for蓄房
		kDBRInfo.bPlayCoutIdex = pTableInfo->bPlayCoutIdex;
		kDBRInfo.bGameTypeIdex = pTableInfo->bGameTypeIdex;
		kDBRInfo.bGameRuleIdex = pTableInfo->bGameRuleIdex;
		kDBRInfo.lBaseScore = pTableInfo->lBaseScore;
		//ZY add
		kDBRInfo.PlayerCount = pTableInfo->PlayerCount;

		m_pIDataBaseEngine->PostDataBaseRequest(pIServerUserItem->GetUserID(), DBR_GR_JOIN_CHECK, dwSocketID, &kDBRInfo, sizeof(DBR_GR_Create_Private));
	}

	return true;
}
bool PriaveteGame::OnTCPNetworkSubDismissPrivate(VOID * pData, WORD wDataSize, IServerUserItem * pIServerUserItem, DWORD dwSocketID)
{
	CTraceService::TraceString(TEXT("OnTCPNetworkSubDismissPrivate"),TraceLevel_Normal);

	//参数效验
	ASSERT(wDataSize==sizeof(CMD_GR_Dismiss_Private));
	if(wDataSize!=sizeof(CMD_GR_Dismiss_Private)) return false;

	CMD_GR_Dismiss_Private* pCMDInfo = (CMD_GR_Dismiss_Private*)pData;

	PrivateTableInfo* pTableInfo = getTableInfoByTableID(pIServerUserItem->GetTableID());
	if (!pTableInfo)
	{
		return false;
	}

	/*
	if (!pTableInfo->bStart && pTableInfo->dwCreaterUserID != pIServerUserItem->GetUserID())
	{
		//游戏未开始，非房主申请解散

		if (pTableInfo->cbRoomType == Type_Private)
		{
			//房卡模式

			//只是自己退出房间
			ITableFrame * pITableFrame = pTableInfo->pITableFrame;
			if (pITableFrame != NULL)
			{
				pITableFrame->PerformStandUpActionEx(pIServerUserItem);
			}

			return true;
		}
		else
		{
			//比赛模式

			//比赛场没开始前任何人都能解散房间
		}
	}
	//*/
	///*
	if (!pTableInfo->bStart && pTableInfo->dwCreaterUserID != pIServerUserItem->GetUserID())
	{
		//游戏未开始，非房主申请解散

		//mChen edit
		if (pTableInfo->cbRoomType == Type_Private)
		{
			//房卡模式，非房主不都能解散房
			return true;
		}
		else
		{
			//比赛场没开始前,任何人都能解散房间
		}
		///return true;
	}
	//*/

	if (pTableInfo->bInEnd)
	{
		return true;
	}
	if (!pTableInfo)
	{
		return true;
	}
	if (pTableInfo->kDismissChairID.size() == 0 && !pCMDInfo->bDismiss)
	{
		return true;
	}
	if(pCMDInfo->bDismiss)
	{
		for (int i = 0;i<(int)pTableInfo->kDismissChairID.size();i++)
		{
			if (pTableInfo->kDismissChairID[i] == pIServerUserItem->GetChairID())
			{
				return true;
			}
		}
		pTableInfo->kDismissChairID.push_back(pIServerUserItem->GetChairID());
	}
	else
	{
		for (int i = 0;i<(int)pTableInfo->kNotAgreeChairID.size();i++)
		{
			if (pTableInfo->kNotAgreeChairID[i] == pIServerUserItem->GetChairID())
			{
				return true;
			}
		}
		pTableInfo->kNotAgreeChairID.push_back(pIServerUserItem->GetChairID());
	}
	CMD_GF_Private_Dismiss_Info kNetInfo;

	/*
	//mChen edit, for offline user auto agree dismiss
	ITableFrame * pITableFrame = pTableInfo->pITableFrame;
	DWORD dwOfflineUserCout = 0;
	if (pITableFrame != NULL)
	{
		for (int i = 0; i < pITableFrame->GetChairCount(); i++)
		{
			IServerUserItem* pUserItem = pITableFrame->GetTableUserItem(i);
			if (pUserItem&&pUserItem->GetTableID() != INVALID_TABLE)
			{
				BYTE cbUserStatus = pUserItem->GetUserStatus();
				if (cbUserStatus == US_OFFLINE)
				{
					dwOfflineUserCout++;
				}
			}
		}
	}
	DWORD dwSitUserCount = pTableInfo->pITableFrame->GetSitUserCount();
	kNetInfo.dwDissUserCout = pTableInfo->kDismissChairID.size() + dwOfflineUserCout;
	//*/
	kNetInfo.dwDissUserCout = pTableInfo->kDismissChairID.size();

	kNetInfo.dwNotAgreeUserCout = pTableInfo->kNotAgreeChairID.size();
	for (int i = 0;i<(int)pTableInfo->kDismissChairID.size();i++)
	{
		kNetInfo.dwDissChairID[i] = pTableInfo->kDismissChairID[i];
	}
	for (int i = 0;i<(int)pTableInfo->kNotAgreeChairID.size();i++)
	{
		kNetInfo.dwNotAgreeChairID[i] = pTableInfo->kNotAgreeChairID[i];
	}
	SendTableData(pTableInfo->pITableFrame,MDM_GR_PRIVATE,SUB_GR_PRIVATE_DISMISS,&kNetInfo,sizeof(kNetInfo));

	bool bClearDismissInfo = false;
	if (pTableInfo->kNotAgreeChairID.size() >= 1)//mChen edit, >= 2
	{
		bClearDismissInfo = true;
	}

	if (!pTableInfo->bStart || (int)kNetInfo.dwDissUserCout >= (int)pTableInfo->pITableFrame->GetSitUserCount())//-1
	{
		bClearDismissInfo = true;

		pTableInfo->pITableFrame->SendGameMessage(TEXT("房间已被解散！"),SMT_EJECT);

		DismissRoom(pTableInfo, GER_DISMISS);

		//Log
		CTraceService::TraceString(TEXT("PriaveteGame::OnTCPNetworkSubDismissPrivate cause ClearRoom"), TraceLevel_Normal);

		ClearRoom(pTableInfo);

	}
	if (bClearDismissInfo)
	{
		pTableInfo->kNotAgreeChairID.clear();
		pTableInfo->kDismissChairID.clear();
		kNetInfo.dwDissUserCout = pTableInfo->kDismissChairID.size();
		kNetInfo.dwNotAgreeUserCout = pTableInfo->kNotAgreeChairID.size();
		SendTableData(pTableInfo->pITableFrame,MDM_GR_PRIVATE,SUB_GR_PRIVATE_DISMISS,&kNetInfo,sizeof(kNetInfo));
	}
	return true;
}
//比赛事件
bool PriaveteGame::OnEventSocketPrivate(WORD wSubCmdID, VOID * pData, WORD wDataSize, IServerUserItem * pIServerUserItem, DWORD dwSocketID)
{
	switch (wSubCmdID)
	{
	case SUB_GR_PRIVATE_INFO:	//私人场信息
		{
			SendData(pIServerUserItem,MDM_GR_PRIVATE,SUB_GR_PRIVATE_INFO,&m_kPrivateInfo,sizeof(m_kPrivateInfo));
			return true;
		}
	case SUB_GR_CREATE_PRIVATE:	//创建私人场
		{
			return OnTCPNetworkSubCreatePrivate(pData,wDataSize,pIServerUserItem,dwSocketID);
		}
	case SUB_GR_RIVATE_AGAIN:	//重新加入私人场
		{
			return OnTCPNetworkSubAgainEnter(pData,wDataSize,pIServerUserItem,dwSocketID);
		}
	case SUB_GR_JOIN_PRIVATE:	//加入私人场
		{
			return OnTCPNetworkSubJoinPrivate(pData,wDataSize,pIServerUserItem,dwSocketID);	
		}
	case SUB_GR_PRIVATE_DISMISS:	//解散
		{
			return OnTCPNetworkSubDismissPrivate(pData,wDataSize,pIServerUserItem,dwSocketID);	
		}
	}
	return true;
}

//重置桌位
void PriaveteGame::DismissRoom(PrivateTableInfo* pTableInfo, BYTE cbReason)
{
	ASSERT(pTableInfo!=NULL);
	if (pTableInfo==NULL) return;

	ITableFrame* pTableFrame=pTableInfo->pITableFrame;
	ASSERT(pTableFrame!=NULL);
	if (pTableFrame==NULL) return;

	//mChen add, for HideSeek:要在Send SUB_GF_PRIVATE_END之前，因为要提前设置m_wMapIndexRand及m_wRandseed等
	//BYTE cbEndReason = (pTableInfo->dwFinishPlayCout >= pTableInfo->dwPlayCout ? GER_NORMAL : GER_DISMISS);
	pTableFrame->HideSeek_FreeGameAndTimers(cbReason);

	//mChen edit:游戏没开始也能解散房间，从而使客户端调用showFinalJieSuanInfo，保证hnManager.m_cbGameEndReason值是正确的
	if(true)//if (pTableInfo->bStart)
	{
		WORD wSitUserCount = pTableFrame->GetSitUserCount();//pTableFrame->GetChairCount()

		CMD_GF_Private_End_Info kNetInfo;
		kNetInfo.cbEndReason = cbReason;// (pTableInfo->dwFinishPlayCout >= pTableInfo->dwPlayCout ? GER_NORMAL : GER_DISMISS);

		//mChen add, for HideSeek
		if (pTableInfo->cbRoomType == Type_Private)
		{
			kNetInfo.cbMapIndex = pTableFrame->GetChoosedMapIndex();
		}
		else
		{
			kNetInfo.cbMapIndex = pTableFrame->GetMapIndexRand();
		}
		kNetInfo.wRandseed = pTableFrame->GetRandSeed();
		kNetInfo.wRandseedForRandomGameObject = pTableFrame->GetRandSeedForRandomGameObjec();
		kNetInfo.wRandseedForInventory = pTableFrame->GetRandSeedForInventory();
		//道具同步
		InventoryItem* pInventoryList = pTableFrame->GetInventoryList();
		CopyMemory(kNetInfo.sInventoryList, pInventoryList, sizeof(kNetInfo.sInventoryList));

		//mChen comment, for HideSeek
		/*
		kNetInfo.lPlayerWinLose.resize(wSitUserCount);
		kNetInfo.lPlayerAction.resize(wSitUserCount*MAX_PRIVATE_ACTION);
		for (int i = 0;i<wSitUserCount;i++)
		{
			kNetInfo.lPlayerWinLose[i] = pTableInfo->lPlayerWinLose[i];
			for (int m = 0;m<MAX_PRIVATE_ACTION;m++)
			{
				kNetInfo.lPlayerAction[i*MAX_PRIVATE_ACTION+m] = pTableInfo->lPlayerAction[i][m];
			}

			//IServerUserItem* pUserItem = pTableFrame->GetTableUserItem(i);
			//if (pUserItem && Type_Public == pTableInfo->cbRoomType) // 比赛场结束时将积分信息写入数据库
			//{
			//	DBR_GR_MatchRecordScore recordScore;
			//	recordScore.dwUserID = pUserItem->GetUserID();
			//	recordScore.lScore = kNetInfo.lPlayerWinLose[i];
			//	lstrcpyn(recordScore.strNickName, pUserItem->GetNickName(), CountArray(recordScore.strNickName));
			//	m_pIDataBaseEngine->PostDataBaseRequest(recordScore.dwUserID, DBR_GR_MATCH_RECORD_SCORE, 0, &recordScore, sizeof(DBR_GR_MatchRecordScore));
			//}
		}

		datastream kDataStream;
		kNetInfo.StreamValue(kDataStream,true);
		SendTableData(pTableFrame,MDM_GR_PRIVATE,SUB_GF_PRIVATE_END,&kDataStream[0],kDataStream.size());

		//mChen add, for HideSeek
		pTableFrame->SendLookonData(INVALID_CHAIR, SUB_GF_PRIVATE_END, &kDataStream[0], kDataStream.size(), MDM_GR_PRIVATE);
		*/

		SendTableData(pTableFrame, MDM_GR_PRIVATE, SUB_GF_PRIVATE_END, &kNetInfo, sizeof(kNetInfo));
		pTableFrame->SendLookonData(INVALID_CHAIR, SUB_GF_PRIVATE_END, &kNetInfo, sizeof(kNetInfo), MDM_GR_PRIVATE);
	}

	//强制解散游戏
	if (pTableFrame->IsGameStarted()==true)
	{
		bool bSuccess=pTableFrame->DismissGame();
		if (bSuccess==false)
		{
			CTraceService::TraceString(TEXT("PriaveteGame 解散游戏出现错误"),TraceLevel_Exception);
			return;
		}
	}

	if (pTableInfo->bStart)
	{
		pTableInfo->bInEnd = true;
		pTableInfo->dwCreaterUserID = 0;

		datastream kDataStream;
		pTableInfo->kTotalRecord.StreamValue(kDataStream,true);

		if (pTableInfo->dwFinishPlayCout >= pTableInfo->dwPlayCout)		//mChen add, for已打场次
		{
			m_pIDataBaseEngine->PostDataBaseRequest(INVALID_DWORD, DBR_GR_PRIVATE_GAME_RECORD, 0, &kDataStream[0], kDataStream.size());
		}

		pTableInfo->bStart = false;
		sendPrivateRoomInfo(NULL,pTableInfo);

		////mChen add, for HideSeek
		//ITableFrame* pTableFrame = pTableInfo->pITableFrame;
		//if (pTableFrame != NULL)
		//{
		//	WORD wSitUserCount = pTableInfo->pITableFrame->GetSitUserCount();
		//	if (wSitUserCount == 0)
		//	{
		//		//Log
		//		CTraceService::TraceString(TEXT("DismissRoom:所有人都离开了，强制解散房间!"), TraceLevel_Normal);

		//		//游戏结束或者解散房间时(DismissRoom)，所有人都离开了,强制解散房间
		//		ClearRoom(pTableInfo);
		//	}
		//}
	}
	else
	{
		//Log
		CTraceService::TraceString(TEXT("DismissRoom: !pTableInfo->bStart cause ClearRoom"), TraceLevel_Normal);

		ClearRoom(pTableInfo);
	}
	pTableFrame->ResetPlayerTotalScore();

	return;
}
void PriaveteGame::ClearRoom(PrivateTableInfo* pTableInfo)
{
	ASSERT(pTableInfo!=NULL);
	if (pTableInfo==NULL) return;

	ITableFrame* pTableFrame=pTableInfo->pITableFrame;
	ASSERT(pTableFrame!=NULL);
	if (pTableFrame==NULL) return;

	//该桌用户全部离开
	for (int i=0;i<pTableFrame->GetChairCount();i++)
	{
		IServerUserItem* pUserItem=pTableFrame->GetTableUserItem(i);
		if(pUserItem && pUserItem->GetTableID()!=INVALID_TABLE)
		{
			pTableFrame->PerformStandUpActionEx(pUserItem);
		}
	}
	pTableInfo->restValue();

	//mChen add, for HideSeek
	pTableFrame->SendGameMessage(TEXT("房间已解散！"), SMT_EJECT);
	//Log
	TCHAR szString[128] = TEXT("");
	_sntprintf(szString, CountArray(szString), TEXT("ClearRoom: 房间已经解散,TableId=%d"), pTableFrame->GetTableID());
	CTraceService::TraceString(szString, TraceLevel_Normal);
}
void PriaveteGame::DBR_CreatePrivate(DBR_GR_Create_Private* kInfo,DWORD dwSocketID,IServerUserItem* pIServerUserItem,std::string kHttpChannel)
{
	if (kHttpChannel != "")
	{
		 int iCout = FangKaHttpUnits::UseCard(pIServerUserItem->GetUserID(),0,kHttpChannel);
		 DBO_GR_CreatePrivateResoult kNetInfo;
		 ZeroMemory(&kNetInfo,sizeof(kNetInfo));
		 kNetInfo.dwAgaginTable = kInfo->dwAgaginTable;
		 kNetInfo.bGameRuleIdex = kInfo->bGameRuleIdex;
		 kNetInfo.bGameTypeIdex = kInfo->bGameTypeIdex;
		 kNetInfo.bPlayCoutIdex = kInfo->bPlayCoutIdex;
		 kNetInfo.cbRoomType = Type_Private;

		 //mChen add
		 kNetInfo.cbPlayCostTypeIdex = kInfo->cbPlayCostTypeIdex;
		 kNetInfo.lBaseScore = kInfo->lBaseScore;
		 //ZY add
		 kNetInfo.PlayerCount = kInfo->PlayerCount;

		 if (iCout < (int)kInfo->dwCost)
		 {
			 kNetInfo.bSucess = false;
			 lstrcpyn(kNetInfo.szDescribeString,TEXT("房卡不够"),CountArray(kNetInfo.szDescribeString));
		 }
		 else
		 {
			 kNetInfo.bSucess = true;
			 kNetInfo.lCurSocre = iCout;
		 }
		 OnEventCreatePrivate(DBO_GR_CREATE_PRIVATE,pIServerUserItem,&kNetInfo,sizeof(kNetInfo),kHttpChannel);
	}
	else
	{
		m_pIDataBaseEngine->PostDataBaseRequest(pIServerUserItem->GetUserID(),DBR_GR_CREATE_PRIVAT,dwSocketID,kInfo,sizeof(DBR_GR_Create_Private));
	}
}
//用户积分
bool PriaveteGame::OnEventUserItemScore(IServerUserItem * pIServerUserItem, BYTE cbReason)
{
	//效验参数
	ASSERT(pIServerUserItem!=NULL);
	if (pIServerUserItem==NULL) return false;

	m_pIServerUserItemSink->OnEventUserItemScore(pIServerUserItem,cbReason);
	return true;
}

//用户状态
bool PriaveteGame::OnEventUserItemStatus(IServerUserItem * pIServerUserItem, WORD wOldTableID, WORD wOldChairID)
{
	if(m_pIServerUserItemSink!=NULL)
	{
		return m_pIServerUserItemSink->OnEventUserItemStatus(pIServerUserItem,wOldTableID,wOldChairID);
	}

	return true;
}

//用户权限
bool PriaveteGame::OnEventUserItemRight(IServerUserItem *pIServerUserItem, DWORD dwAddRight, DWORD dwRemoveRight,bool bGameRight)
{
	if(m_pIServerUserItemSink!=NULL)
	{
		return m_pIServerUserItemSink->OnEventUserItemRight(pIServerUserItem,dwAddRight,dwRemoveRight,bGameRight);
	}

	return true;
}

//用户登录
bool PriaveteGame::OnEventUserLogon(IServerUserItem * pIServerUserItem)
{
	return true;
}

//用户登出
bool PriaveteGame::OnEventUserLogout(IServerUserItem * pIServerUserItem)
{
	return true;
}

//进入事件
bool PriaveteGame::OnEventEnterPrivate(DWORD dwSocketID ,VOID* pData,DWORD dwUserIP, bool bIsMobile)
{
	//手机用户
	if(bIsMobile == true)
	{
		//处理消息
		CMD_GR_LogonMobile * pLogonMobile=(CMD_GR_LogonMobile *)pData;
		pLogonMobile->szPassword[CountArray(pLogonMobile->szPassword)-1]=0;
		pLogonMobile->szMachineID[CountArray(pLogonMobile->szMachineID)-1]=0;

		//变量定义
		DBR_GR_LogonMobile LogonMobile;
		ZeroMemory(&LogonMobile,sizeof(LogonMobile));

		//构造数据
		LogonMobile.dwUserID=pLogonMobile->dwUserID;
		LogonMobile.dwClientAddr=dwUserIP;		
		LogonMobile.cbDeviceType=pLogonMobile->cbDeviceType;
		LogonMobile.wBehaviorFlags=pLogonMobile->wBehaviorFlags;
		LogonMobile.wPageTableCount=pLogonMobile->wPageTableCount;		
		lstrcpyn(LogonMobile.szPassword,pLogonMobile->szPassword,CountArray(LogonMobile.szPassword));
		lstrcpyn(LogonMobile.szMachineID,pLogonMobile->szMachineID,CountArray(LogonMobile.szMachineID));

		//投递请求
		m_pIDataBaseEngine->PostDataBaseRequest(LogonMobile.dwUserID,DBR_GR_LOGON_MOBILE,dwSocketID,&LogonMobile,sizeof(LogonMobile));		
	}
	else
	{
		//处理消息
		CMD_GR_LogonUserID * pLogonUserID=(CMD_GR_LogonUserID *)pData;
		pLogonUserID->szPassword[CountArray(pLogonUserID->szPassword)-1]=0;
		pLogonUserID->szMachineID[CountArray(pLogonUserID->szMachineID)-1]=0;

		//变量定义
		DBR_GR_LogonUserID LogonUserID;
		ZeroMemory(&LogonUserID,sizeof(LogonUserID));

		//构造数据
		LogonUserID.dwClientAddr=dwUserIP;
		LogonUserID.dwUserID=pLogonUserID->dwUserID;
		lstrcpyn(LogonUserID.szPassword,pLogonUserID->szPassword,CountArray(LogonUserID.szPassword));
		lstrcpyn(LogonUserID.szMachineID,pLogonUserID->szMachineID,CountArray(LogonUserID.szMachineID));

		//投递请求
		m_pIDataBaseEngine->PostDataBaseRequest(LogonUserID.dwUserID,DBR_GR_LOGON_USERID,dwSocketID,&LogonUserID,sizeof(LogonUserID));
	}
	return true;
}
bool PriaveteGame::AddPrivateAction(ITableFrame* pTbableFrame,DWORD dwChairID, BYTE	bActionIdex)
{
	PrivateTableInfo* pTableInfo = getTableInfoByTableFrame(pTbableFrame);
	ASSERT(pTableInfo);
	if (!pTableInfo)
	{
		return true;
	}
	if (dwChairID >= 100 || bActionIdex >= MAX_PRIVATE_ACTION)
	{
		return true;
	}
	pTableInfo->lPlayerAction[dwChairID][bActionIdex] ++;
	return true;
}

//用户
bool PriaveteGame::OnEventUserJoinPrivate(IServerUserItem * pIServerUserItem, BYTE cbReason,DWORD dwSocketID)
{
	//参数校验
	ASSERT(pIServerUserItem!=NULL);
	if (pIServerUserItem == NULL) return false;

	SendData(pIServerUserItem,MDM_GR_PRIVATE,SUB_GR_PRIVATE_INFO,&m_kPrivateInfo,sizeof(m_kPrivateInfo));

	PrivateTableInfo* pTableInfo = getTableInfoByTableID(pIServerUserItem->GetTableID());
	//判断状态
	if(pIServerUserItem->GetTableID()!=INVALID_TABLE)
	{
		sendPrivateRoomInfo(pIServerUserItem,getTableInfoByTableID(pIServerUserItem->GetTableID()));
	}
	return true;
}

//用户退赛
bool PriaveteGame::OnEventUserQuitPrivate(IServerUserItem * pIServerUserItem, BYTE cbReason, WORD *pBestRank, DWORD dwContextID)
{
	ASSERT(pIServerUserItem!=NULL);
	if (pIServerUserItem==NULL) return false;

	return true;
}

//游戏开始
bool PriaveteGame::OnEventGameStart(ITableFrame *pITableFrame, WORD wChairCount)
{
	PrivateTableInfo* pTableInfo = getTableInfoByTableFrame(pITableFrame);
	ASSERT(pTableInfo);
	if (!pTableInfo)
	{
		return true;
	}
	pTableInfo->dwStartPlayCout ++;
	pTableInfo->bStart = true;
	pTableInfo->newRandChild();

	sendPrivateRoomInfo(NULL,pTableInfo);
	return true;
}

//游戏结束
bool PriaveteGame::OnEventGameEnd(ITableFrame *pITableFrame,WORD wChairID, IServerUserItem * pIServerUserItem, BYTE cbReason)
{
	ASSERT(pITableFrame!=NULL);
	if (pITableFrame==NULL) return false;

	PrivateTableInfo* pTableInfo = getTableInfoByTableFrame(pITableFrame);
	if (!pTableInfo)
	{
		return true;
	}

	//mChen add, for HideSeek
	pTableInfo->m_cbAgainHumanNum = 0;
	pTableInfo->m_cbAgainLookonNum = 0;
	//if (cbReason == GER_NORMAL)
	//{
	//	m_pITimerEngine->SetTimer(IDI_PLAY_AGAIN_WAIT_END, 6000L, 1, pITableFrame->GetTableID());//6s
	//}

	if(true)//if (pTableInfo->cbRoomType == Type_Private)
	{
		if (pTableInfo->dwFinishPlayCout == 0)
		{
			if (pTableInfo->kHttpChannel != "")
			{
				FangKaHttpUnits::UseCard(pTableInfo->dwCreaterUserID,(int)m_kPrivateInfo.lPlayCost[pTableInfo->bPlayCoutIdex],pTableInfo->kHttpChannel);
			}
			else
			{
				CreatePrivateCost(pTableInfo);
			}
		}

		if (cbReason == GER_NORMAL)		//mChen add,fix解散房间客户端收到的EndReason也为GER_NORMAL
		{
			pTableInfo->dwFinishPlayCout++;
		}

		sendPrivateRoomInfo(NULL,pTableInfo);

		//mChen edit, for HideSeek: 非正常结束直接ClearRoom，ClearRoom后客户端会直接返回大厅，无法蓄房
		if (cbReason != GER_NORMAL)
		{
			DismissRoom(pTableInfo, cbReason);

			//Log
			CTraceService::TraceString(TEXT("PriaveteGame::OnEventGameEnd: cbReason!=GER_NORMA cause ClearRoom"), TraceLevel_Normal);

			ClearRoom(pTableInfo);
		}
		else if (pTableInfo->dwFinishPlayCout >= pTableInfo->dwPlayCout)
		{
			DismissRoom(pTableInfo, cbReason);
		}
		//if (pTableInfo->dwFinishPlayCout >= pTableInfo->dwPlayCout)
		//{
		//	DismissRoom(pTableInfo, cbReason);
		//}
	}
	else if (pTableInfo->cbRoomType == Type_Public)
	{
		/*
		//mChen add:金币场模拟比赛场:每场结束第一名获得一个钻石,参加比赛场不扣费用
		pTableInfo->dwFinishPlayCout++;
		sendPrivateRoomInfo(NULL, pTableInfo);
		if (pTableInfo->dwFinishPlayCout >= pTableInfo->dwPlayCout)
		{
			SCORE lMaxScore = -1;
			WORD dwWinUserID = 0;
			for (int i = 0; i < pITableFrame->GetChairCount(); i++)
			{
				if (pTableInfo->lPlayerWinLose[i] > lMaxScore)
				{
					IServerUserItem* pUserItem = pTableInfo->pITableFrame->GetTableUserItem(i);
					if (pUserItem != NULL)
					{
						dwWinUserID = pUserItem->GetUserID();
						lMaxScore = pTableInfo->lPlayerWinLose[i];
					}
				}
			}

			DWORD dwAwardScore = 1;
			CreatePrivateMatchAward(dwWinUserID, dwAwardScore);

			DismissRoom(pTableInfo, cbReason);
			///ClearRoom(pTableInfo);//comment to fix 客户端没有结算数据，返回大厅无效
		}
		*/

		//mChen comment
		CreatePrivateCost(pTableInfo);
		DismissRoom(pTableInfo, cbReason);
		ClearRoom(pTableInfo);
	}
	return true;
}
bool PriaveteGame::WriteTableScore(ITableFrame* pITableFrame,tagScoreInfo ScoreInfoArray[], WORD wScoreCount,datastream& kData)
{
	PrivateTableInfo* pTableInfo = getTableInfoByTableFrame(pITableFrame);
	if (!pTableInfo)
	{
		return true;
	}
	pTableInfo->writeSocre( ScoreInfoArray,wScoreCount,kData);

	return true;
}
void PriaveteGame::sendPrivateRoomInfo(IServerUserItem * pIServerUserItem,PrivateTableInfo* pTableInfo)
{
	ASSERT(pTableInfo);
	if (!pTableInfo)
	{
		return;
	}

	CMD_GF_Private_Room_Info kNetInfo;
	kNetInfo.cbRoomType = pTableInfo->cbRoomType;
	kNetInfo.bStartGame = pTableInfo->bStart;
	kNetInfo.dwRoomNum = pTableInfo->dwRoomNum;
	kNetInfo.dwPlayCout = pTableInfo->dwStartPlayCout;
	kNetInfo.dwCreateUserID = pTableInfo->dwCreaterUserID;
	kNetInfo.bGameRuleIdex = pTableInfo->bGameRuleIdex;
	kNetInfo.bGameTypeIdex = pTableInfo->bGameTypeIdex;
	kNetInfo.bPlayCoutIdex = pTableInfo->bPlayCoutIdex;
	kNetInfo.dwPlayTotal = pTableInfo->dwPlayCout;

	//mChen add
	kNetInfo.lBaseScore = pTableInfo->lBaseScore;
	//ZY add
	kNetInfo.PlayerCount = pTableInfo->PlayerCount;

	kNetInfo.kWinLoseScore.resize(pTableInfo->pITableFrame->GetChairCount());
	for (WORD i = 0;i<pTableInfo->pITableFrame->GetChairCount();i++)
	{
		kNetInfo.kWinLoseScore[i] =  (int)pTableInfo->lPlayerWinLose[i];
	}

	datastream kDataStream;
	kNetInfo.StreamValue(kDataStream,true);
	
	if (pIServerUserItem)
	{
		SendData(pIServerUserItem,MDM_GR_PRIVATE,SUB_GF_PRIVATE_ROOM_INFO,&kDataStream[0],kDataStream.size());
	}
	else
	{
		SendTableData(pTableInfo->pITableFrame,MDM_GR_PRIVATE,SUB_GF_PRIVATE_ROOM_INFO,&kDataStream[0],kDataStream.size());
	}
}
//断线
bool PriaveteGame::OnActionUserOffLine(WORD wChairID, IServerUserItem * pIServerUserItem, char*pFunc, char *pFile, int nLine)
{
	ASSERT(pIServerUserItem);
	if (!pIServerUserItem)
	{
		return false;
	}
	PrivateTableInfo* pTableInfo = getTableInfoByTableID(pIServerUserItem->GetTableID());
	if (!pTableInfo)
	{
		return false;
	}

	//Log
	TCHAR szString[256] = TEXT("");
	_sntprintf(szString, CountArray(szString), TEXT("OnActionUserOffLine:pFunc=%s, userName=%s, userId=%d, Line=%d"), pFunc, pIServerUserItem->GetNickName(), pIServerUserItem->GetUserInfo()->dwUserID, nLine);//pFile
	CTraceService::TraceString(szString, TraceLevel_Normal);

	//mChen add, for HideSeek
	if (pTableInfo->cbRoomType == Type_Private && pTableInfo->dwCreaterUserID == pIServerUserItem->GetUserID() && !pTableInfo->bStart)
	{
		//是房卡房房主，且游戏未开始

		ITableFrame* pTableFrame = pTableInfo->pITableFrame;
		if (pTableFrame != NULL)
		{
			pTableFrame->SendGameMessage(TEXT("房主离开，房间自动解散！"), SMT_EJECT);
		}

		//OnActionUserOffLine：游戏未开始，房卡房房主断线,强制解散房间，fix因为没人点开始导致游戏永远开始不了
		DismissRoom(pTableInfo, GER_DISMISS);
		_sntprintf(szString, CountArray(szString), TEXT("OnActionUserOffLine:游戏未开始，房卡房房主断线，强制解散房间"));
		CTraceService::TraceString(szString, TraceLevel_Normal);
		ClearRoom(pTableInfo);

		return true;
	}

	if (pTableInfo->dwCreaterUserID == pIServerUserItem->GetUserID() && !pTableInfo->bInEnd)
	{
		return true;
	}
	if (pTableInfo->bInEnd)
	{
		//已经结束

		pTableInfo->pITableFrame->PerformStandUpActionEx(pIServerUserItem);

		//mChen add, for HideSeek
		//所有人离开后
		WORD wTableUserCount = pTableInfo->pITableFrame->GetSitUserCount();
		if (wTableUserCount == 0)
		{
			//OnActionUserOffLine：游戏已经结束，所有人都离开或者断线了,强制解散房间
			DismissRoom(pTableInfo, GER_DISMISS);
			CTraceService::TraceString(TEXT("OnActionUserOffLine:游戏结束后，所有人都离开或者断线了，强制解散房间!"), TraceLevel_Normal);
			ClearRoom(pTableInfo);
		}

		//// LJ-修复房间无法创建的bug
		//if (pTableInfo->pITableFrame->GetNullChairCount() == pTableInfo->pITableFrame->GetChairCount())
		//{
		//	//所有人离开后重置bInEnd值
		//	pTableInfo->bInEnd = false;
		//	pTableInfo->restValue();
		//}
	}
	return true;
}
//用户坐下
bool PriaveteGame::OnActionUserSitDown(WORD wTableID, WORD wChairID, IServerUserItem * pIServerUserItem, bool bLookonUser)
{ 
	return true; 
}

//用户起立
bool PriaveteGame::OnActionUserStandUp(WORD wTableID, WORD wChairID, IServerUserItem * pIServerUserItem, bool bLookonUser)
{
	//移除分组
	return true;
}

 //用户同意
bool PriaveteGame::OnActionUserOnReady(WORD wTableID, WORD wChairID, IServerUserItem * pIServerUserItem, VOID * pData, WORD wDataSize)
{ 
	ASSERT(pIServerUserItem);
	if (!pIServerUserItem)
	{
		return true;
	}
	PrivateTableInfo* pTableInfo = getTableInfoByTableID(pIServerUserItem->GetTableID());
	if (!pTableInfo)
	{
		return true;
	}
	if (pTableInfo->bInEnd)
	{
		return false;
	}
	return true; 
}

bool PriaveteGame::OnEventReqStandUP(IServerUserItem * pIServerUserItem, BYTE cbForceStandUP)
{
	// Log
	TCHAR szString[128] = TEXT("");
	_sntprintf(szString, CountArray(szString), TEXT("OnEventReqStandUP: userName=%s, userId=%d"), pIServerUserItem->GetNickName(), pIServerUserItem->GetUserInfo()->dwUserID);
	CTraceService::TraceString(szString, TraceLevel_Normal);

	ASSERT(pIServerUserItem);
	if (!pIServerUserItem)
	{
		return true;
	}
	PrivateTableInfo* pTableInfo = getTableInfoByTableID(pIServerUserItem->GetTableID());
	ASSERT(pTableInfo);
	if (!pTableInfo)
	{
		return true;
	}

	if (pTableInfo->bStart && !pTableInfo->bInEnd && (cbForceStandUP==FALSE))
	{
		BYTE cbUserStatus = pIServerUserItem->GetUserStatus();
		if (cbUserStatus != US_LOOKON)//mChen add, for HideSeek
		{
			m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem, TEXT("游戏已经开始，无法退出。"), SMT_EJECT);
			return true;
		}
	}

	//mChen add, for HideSeek
	if (pTableInfo->bStart && !pTableInfo->bInEnd)
	{
		BYTE cbUserStatus = pIServerUserItem->GetUserStatus();
		DWORD dwUserID = pIServerUserItem->GetUserID();
		BYTE cbHP = pTableInfo->pITableFrame->GetTheHumanHP(pIServerUserItem->GetChairID());
		if (cbUserStatus != US_LOOKON && pTableInfo->cbRoomType == Type_Private && pTableInfo->dwCreaterUserID != dwUserID && cbHP!=0)
		{
			//房卡房，非房主退出房间时，如果没死亡，退出扣除50金币
			CreateUserCost(dwUserID, CostType_Score, 50);
		}
	}
	if (pTableInfo->cbRoomType == Type_Private && pTableInfo->dwCreaterUserID == pIServerUserItem->GetUserID() && !pTableInfo->bStart)
	{
		//是房卡房房主，且游戏未开始

		ITableFrame* pTableFrame = pTableInfo->pITableFrame;
		if (pTableFrame != NULL)
		{
			pTableFrame->SendGameMessage(TEXT("房主离开，房间自动解散！"), SMT_EJECT);
		}

		//OnEventReqStandUP：游戏未开始，房卡房房主离开,强制解散房间
		DismissRoom(pTableInfo, GER_DISMISS);
		_sntprintf(szString, CountArray(szString), TEXT("OnEventReqStandUP:游戏未开始，房卡房房主离开，强制解散房间"));
		CTraceService::TraceString(szString, TraceLevel_Normal);
		ClearRoom(pTableInfo);

		return true;
	}

	/*
	if (getTableInfoByCreaterID(pIServerUserItem->GetUserID()) && !pTableInfo->bStart)
	{
		//自己是某个房间房主，且游戏未开始

		////mChen add，insert代开房记录
		//DBR_GR_Insert_Private_Room privateRoom;
		//privateRoom.dwUserID = pIServerUserItem->GetUserID();
		//privateRoom.dwRoomNum = pTableInfo->dwRoomNum;
		//privateRoom.dwBaseScore = pTableInfo->lBaseScore;
		//privateRoom.dwPlayCout = pTableInfo->dwPlayCout;
		//privateRoom.dwChairCount = pTableInfo->getChairCout();
		//m_pIDataBaseEngine->PostDataBaseRequest(0L, DBR_GR_INSERT_PRIVSTE_ROOM, 0L, &privateRoom, sizeof(DBR_GR_Insert_Private_Room));

		//mChen comment，for代开房
		m_pIGameServiceFrame->SendRoomMessage(pIServerUserItem,TEXT("您已返回大厅,房间将会继续保留。"),SMT_CLOSE_GAME);
		//return true;
	}
	//*/

	pTableInfo->pITableFrame->PerformStandUpActionEx(pIServerUserItem);

	if (pTableInfo->bInEnd)
	{
		////mChen add, for HideSeek
		//WORD wTableUserCount = pTableInfo->pITableFrame->GetSitUserCount();
		//if (wTableUserCount == 0)
		//{
		//	//Log
		//	CTraceService::TraceString(TEXT("OnEventReqStandUP:游戏结束后，所有人都离开了，强制解散房间!"), TraceLevel_Normal);

		//	//游戏结束后，所有人都离开了,强制结束游戏，解散房间
		//	DismissRoom(pTableInfo, GER_DISMISS);
		//	ClearRoom(pTableInfo);
		//}

		////LJ-修复房间无法创建的bug:结束后房间，当所有人退出后，重置房间结束标志，使该房间可被重新使用
		//if (pTableInfo->pITableFrame->GetNullChairCount() == pTableInfo->pITableFrame->GetChairCount())
		//{
		//	pTableInfo->bInEnd = false;
		//	pTableInfo->restValue();
		//}
	}

	//mChen add, for HideSeek
	//当所有人退出
	WORD wNullChairCount = pTableInfo->pITableFrame->GetNullChairCount();
	WORD wChairCount = pTableInfo->pITableFrame->GetChairCount();
	if (wNullChairCount == wChairCount)
	{
		//OnEventReqStandUP：所有人都退出了,强制解散房间
		DismissRoom(pTableInfo, GER_DISMISS);
		CTraceService::TraceString(TEXT("OnEventReqStandUP:所有人都退出了，强制解散房间"), TraceLevel_Normal);
		ClearRoom(pTableInfo);

		return true;
	}

	return true;
}
bool PriaveteGame::OnEventClientReady(WORD wChairID,IServerUserItem * pIServerUserItem)
{
	ASSERT(pIServerUserItem);
	if (!pIServerUserItem)
	{
		return true;
	}
	PrivateTableInfo* pTableInfo = getTableInfoByTableID(pIServerUserItem->GetTableID());
	if (!pTableInfo)
	{
		return true;
	}
	sendPrivateRoomInfo(pIServerUserItem,pTableInfo);

	if (pTableInfo->pITableFrame->GetGameStatus() == GAME_STATUS_FREE)
	{
#if ADD_READY_STEP
		//mChen edit, add Press Ready step
		BYTE cbUserStatus = pIServerUserItem->GetUserStatus();
		BYTE cbLastUserStatus = pIServerUserItem->GetLastUserStatus();
		if (cbLastUserStatus == US_OFFLINE)
		{
			///pIServerUserItem->SetUserStatus(cbUserStatus, pTableInfo->pITableFrame->GetTableID(), wChairID);
		}
		else
		{
			pIServerUserItem->SetUserStatus(US_SIT, pTableInfo->pITableFrame->GetTableID(), wChairID);
		}
		//pIServerUserItem->SetUserStatus(US_READY,pTableInfo->pITableFrame->GetTableID(), wChairID);
#else
		pIServerUserItem->SetUserStatus(US_READY, pTableInfo->pITableFrame->GetTableID(), wChairID);
#endif
	}

	if (pTableInfo->kDismissChairID.size())
	{
		CMD_GF_Private_Dismiss_Info kNetInfo;
		kNetInfo.dwDissUserCout = pTableInfo->kDismissChairID.size();
		kNetInfo.dwNotAgreeUserCout = pTableInfo->kNotAgreeChairID.size();
		for (int i = 0;i<(int)pTableInfo->kDismissChairID.size();i++)
		{
			kNetInfo.dwDissChairID[i] = pTableInfo->kDismissChairID[i];
		}
		for (int i = 0;i<(int)pTableInfo->kNotAgreeChairID.size();i++)
		{
			kNetInfo.dwNotAgreeChairID[i] = pTableInfo->kNotAgreeChairID[i];
		}
		SendTableData(pTableInfo->pITableFrame,MDM_GR_PRIVATE,SUB_GR_PRIVATE_DISMISS,&kNetInfo,sizeof(kNetInfo));
	}

	return true;
}

////mChen add, for HideSeek
//bool PriaveteGame::CanStartWaitGame(ITableFrame *pITableFrame)
//{
//	PrivateTableInfo* pTableInfo = getTableInfoByTableFrame(pITableFrame);
//	if (!pTableInfo)
//	{
//		return false;
//	}
//
//	if (pTableInfo->cbRoomType == Type_Public)
//	{
//		return true;
//	}
//	else
//	{
//		bool bResult = pTableInfo->pITableFrame->IsCreaterUserPressedStart();
//		return bResult;
//	}
//}