#include "StdAfx.h"
#include "TableFrame.h"
#include "StockManager.h"
#include "AttemperEngineSink.h"
#include "DataBasePacket.h"

//////////////////////////////////////////////////////////////////////////////////

//断线定义
#define IDI_OFF_LINE				(TIME_TABLE_SINK_RANGE+1)			//断线标识		

//mChen add, for HideSeek
#define IDI_HIDESEEK_WAIT_GAME			(TIME_TABLE_SINK_RANGE+2)
#define IDI_HIDESEEK_END_WAIT_GAME		(TIME_TABLE_SINK_RANGE+3)
#define IDI_HIDESEEK_USERS_TICK			(TIME_TABLE_SINK_RANGE+4)
#define IDI_HIDESEEK_HIDDING_TIME		(TIME_TABLE_SINK_RANGE+5)
#define IDI_HIDESEEK_END_HIDDING_TIME	(TIME_TABLE_SINK_RANGE+6)
#define IDI_HIDESEEK_GAME_PLAY_TIME		(TIME_TABLE_SINK_RANGE+7)	
#define IDI_HIDESEEK_END_GAME			(TIME_TABLE_SINK_RANGE+8)	
#define IDI_HIDESEEK_PLAY_AGAIN_WAIT_END (TIME_TABLE_SINK_RANGE+9)	


//mChen add, for HideSeek
#define HIDESEEK_WAIT_GAME_TIME		8L		//8s
#define HIDESEEK_HIDING_TIME		45L		//45s	
#define HIDESEEK_GAME_PLAY_TIME		200L	//200s

#define MAX_OFF_LINE				3									//断线次数
#define TIME_OFF_LINE				60000L								//断线时间

//////////////////////////////////////////////////////////////////////////////////

//组件变量
CStockManager						g_StockManager;						//库存管理

//游戏记录
CGameScoreRecordArray				CTableFrame::m_GameScoreRecordBuffer;

//////////////////////////////////////////////////////////////////////////////////

//构造函数
CTableFrame::CTableFrame()
{
	//固有属性
	m_wUserCount = 0;
	m_wTableID=0;
	m_wChairCount=0;
	m_cbStartMode=START_MODE_ALL_READY;

	//标志变量
	m_bGameStarted=false;
	m_bDrawStarted=false;
	m_bTableStarted=false;
	ZeroMemory(m_bAllowLookon,sizeof(m_bAllowLookon));
	ZeroMemory(m_lFrozenedScore,sizeof(m_lFrozenedScore));

	//游戏变量
	m_lCellScore=0L;
	m_cbGameStatus=GAME_STATUS_FREE;

	//mChen add, for HideSeek
	m_wWaitGameTime = HIDESEEK_WAIT_GAME_TIME;
	m_wHiddingTime = HIDESEEK_HIDING_TIME;
	m_wGamePlayTime = HIDESEEK_GAME_PLAY_TIME;
	m_wMapIndexRand = rand() % 0xFF;
	//for随机种子同步
	m_wRandseed = (WORD)(rand() % 0xFFFF);
	m_wRandseedForRandomGameObject = (WORD)(rand() % 0xFFFF);
	m_wRandseedForInventory = (WORD)(rand() % 0xFFFF);
	m_cbGameEndReason = GER_NORMAL;

	//时间变量
	m_dwDrawStartTime=0L;
	ZeroMemory(&m_SystemTimeStart,sizeof(m_SystemTimeStart));

	//动态属性
	m_dwTableOwnerID=0L;
	ZeroMemory(m_szEnterPassword,sizeof(m_szEnterPassword));

	//断线变量
	ZeroMemory(m_wOffLineCount,sizeof(m_wOffLineCount));
	ZeroMemory(m_dwOffLineTime,sizeof(m_dwOffLineTime));

	//配置信息
	m_pGameParameter=NULL;
	m_pGameServiceAttrib=NULL;
	m_pGameServiceOption=NULL;

	//组件接口
	m_pITimerEngine=NULL;
	m_pITableFrameSink=NULL;
	m_pIMainServiceFrame=NULL;
	m_pIAndroidUserManager=NULL;

	//扩张接口
	m_pITableUserAction=NULL;
	m_pITableUserRequest=NULL;
	m_pIMatchTableAction=NULL;

	m_pITableFramePrivate=NULL;
	m_pIPrivateTableAction=NULL;

	//数据接口
	m_pIKernelDataBaseEngine=NULL;
	m_pIRecordDataBaseEngine=NULL;

	//比赛接口
	m_pITableFrameHook=NULL;

	//用户数组
	ZeroMemory(m_TableUserItemArray,sizeof(m_TableUserItemArray));

	return;
}

//析构函数
CTableFrame::~CTableFrame()
{
	//释放对象
	SafeRelease(m_pITableFrameSink);
	SafeRelease(m_pITableFrameHook);
	SafeRelease(m_pITableFramePrivate);
	
	return;
}

//接口查询
VOID * CTableFrame::QueryInterface(REFGUID Guid, DWORD dwQueryVer)
{
	QUERYINTERFACE(ITableFrame,Guid,dwQueryVer);
	QUERYINTERFACE_IUNKNOWNEX(ITableFrame,Guid,dwQueryVer);
	return NULL;
}

//开始游戏
bool CTableFrame::StartGame()
{
	//mChen log
	// Log
	TCHAR szString[128] = TEXT("");
	_sntprintf(szString, CountArray(szString), TEXT("StartGame: tableId=%d"), m_wTableID);
	CTraceService::TraceString(szString, TraceLevel_Normal);

	//游戏状态
	ASSERT(m_bDrawStarted==false);
	if (m_bDrawStarted==true) return false;

	//保存变量
	bool bGameStarted=m_bGameStarted;
	bool bTableStarted=m_bTableStarted;

	//设置状态
	m_bGameStarted=true;
	m_bDrawStarted=true;
	m_bTableStarted=true;

	//开始时间
	GetLocalTime(&m_SystemTimeStart);
	m_dwDrawStartTime=(DWORD)time(NULL);

	//开始设置
	if (bGameStarted==false)
	{
		//状态变量
		ZeroMemory(m_wOffLineCount,sizeof(m_wOffLineCount));
		ZeroMemory(m_dwOffLineTime,sizeof(m_dwOffLineTime));

		//设置用户
		for (WORD i=0;i<m_wChairCount;i++)
		{
			//获取用户
			IServerUserItem * pIServerUserItem=GetTableUserItem(i);

			//设置用户
			if (pIServerUserItem!=NULL)
			{
				//锁定游戏币
				if (m_pGameServiceOption->lServiceScore>0L)
				{
					m_lFrozenedScore[i]=m_pGameServiceOption->lServiceScore;
					pIServerUserItem->FrozenedUserScore(m_pGameServiceOption->lServiceScore);
				}

				//设置状态
				BYTE cbUserStatus=pIServerUserItem->GetUserStatus();
				if ((cbUserStatus!=US_OFFLINE)&&(cbUserStatus!=US_PLAYING)) pIServerUserItem->SetUserStatus(US_PLAYING,m_wTableID,i);
			}
		}

		//发送状态
		if (bTableStarted!=m_bTableStarted) SendTableStatus();
	}

	////通知事件
	//ASSERT(m_pITableFrameSink!=NULL);
	//if (m_pITableFrameSink!=NULL) m_pITableFrameSink->OnEventGameStart();

	//比赛通知
	bool bStart=true;
	if(m_pITableFrameHook!=NULL) bStart=m_pITableFrameHook->OnEventGameStart(this, m_wChairCount);

	//私人场通知
	if(m_pITableFramePrivate!=NULL) bStart=m_pITableFramePrivate->OnEventGameStart(this, m_wChairCount);

	//通知事件
	ASSERT(m_pITableFrameSink!=NULL);
	if (m_pITableFrameSink!=NULL&&bStart) m_pITableFrameSink->OnEventGameStart();

	//mChen add, for HideSeek
	///SetGameTimer(IDI_TIMER_TABLE_SINK_HIDESEEK_USERS_TICK, 10L, TIMES_INFINITY, 0);//0.03s 30ms

	return true;
}

//解散游戏
bool CTableFrame::DismissGame()
{
	//状态判断
	ASSERT(m_bTableStarted==true);
	if (m_bTableStarted==false) return false;

	//结束游戏
	if ((m_bGameStarted==true)&&(m_pITableFrameSink->OnEventGameConclude(INVALID_CHAIR,NULL,GER_DISMISS)==false))
	{
		ASSERT(FALSE);
		return false;
	}

	//设置状态
	if ((m_bGameStarted==false)&&(m_bTableStarted==true))
	{
		//设置变量
		m_bTableStarted=false;

		//发送状态
		SendTableStatus();
	}

	return true;
}

//结束游戏
bool CTableFrame::ConcludeGame(BYTE cbGameStatus, BYTE cbEndReason)
{
	//效验状态
	ASSERT(m_bGameStarted==true);
	if (m_bGameStarted==false) return false;

	//保存变量
	bool bDrawStarted=m_bDrawStarted;

	//设置状态
	m_bDrawStarted=false;
	m_cbGameStatus=cbGameStatus;
	m_bGameStarted=(cbGameStatus>=GAME_STATUS_PLAY)?true:false;

	//游戏记录
	if (bDrawStarted==true)
	{
		//写入记录
		if (CServerRule::IsRecordGameScore(m_pGameServiceOption->dwServerRule)==true)
		{
			//变量定义
			DBR_GR_GameScoreRecord GameScoreRecord;
			//设置变量
			GameScoreRecord.wTableID=m_wTableID;
			GameScoreRecord.dwPlayTimeCount=(bDrawStarted==true)?(DWORD)time(NULL)-m_dwDrawStartTime:0;

			//游戏时间
			GameScoreRecord.SystemTimeStart=m_SystemTimeStart;
			GetLocalTime(&GameScoreRecord.SystemTimeConclude);

			//用户积分
			for (INT_PTR i=0;i<m_GameScoreRecordActive.GetCount();i++)
			{
				//获取对象
				ASSERT(m_GameScoreRecordActive[i]!=NULL);
				tagGameScoreRecord * pGameScoreRecord=m_GameScoreRecordActive[i];

				//用户数目
				if (pGameScoreRecord->cbAndroid==FALSE)
				{
					GameScoreRecord.wUserCount++;
				}
				else
				{
					GameScoreRecord.wAndroidCount++;
				}

				//奖牌统计
				GameScoreRecord.dwUserMemal+=pGameScoreRecord->dwUserMemal;

				//统计信息
				if (pGameScoreRecord->cbAndroid==FALSE)
				{
					GameScoreRecord.lWasteCount-=(pGameScoreRecord->lScore+pGameScoreRecord->lRevenue);
					GameScoreRecord.lRevenueCount+=pGameScoreRecord->lRevenue;
				}

				WORD wIndex=GameScoreRecord.wRecordCount++;
				GameScoreRecord.GameScoreRecord.push_back(*pGameScoreRecord);
			}

			//投递数据
			if(GameScoreRecord.wUserCount > 0)
			{
				GameScoreRecord.dataGameDefine = m_UserDefine;

				datastream kDataStream;
				GameScoreRecord.StreamValue(kDataStream,true);
				m_pIRecordDataBaseEngine->PostDataBaseRequest(DBR_GR_GAME_SCORE_RECORD,0,&kDataStream[0],kDataStream.size());
			}
		}

		//清理记录
		if (m_GameScoreRecordActive.GetCount()>0L)
		{
			m_GameScoreRecordBuffer.Append(m_GameScoreRecordActive);
			m_GameScoreRecordActive.RemoveAll();
		}
	}

	//结束设置
	if (m_bGameStarted==false)
	{
		//变量定义
		bool bOffLineWait=false;

		//设置用户
		for (WORD i=0;i<m_wChairCount;i++)
		{
			//获取用户
			IServerUserItem * pIServerUserItem=GetTableUserItem(i);

			//用户处理
			if (pIServerUserItem!=NULL)
			{
				tagTimeInfo* TimeInfo=pIServerUserItem->GetTimeInfo();
				//游戏时间
				DWORD dwCurrentTime=(DWORD)time(NULL);
				TimeInfo->dwEndGameTimer=dwCurrentTime;

				//解锁游戏币
				if (m_lFrozenedScore[i]!=0L)
				{
					pIServerUserItem->UnFrozenedUserScore(m_lFrozenedScore[i]);
					m_lFrozenedScore[i]=0L;
				}

				//设置状态
				if (pIServerUserItem->GetUserStatus()==US_OFFLINE)
				{
					//断线处理
					bOffLineWait=true;
					if(m_pGameServiceOption->wServerType!=GAME_GENRE_MATCH)
					{
						// Log
						TCHAR szString[128] = TEXT("");
						_sntprintf(szString, CountArray(szString), TEXT("ConcludeGame::游戏结束时离线玩家强制离开: userName=%s, userId=%d, chairId=%d"), pIServerUserItem->GetNickName(), pIServerUserItem->GetUserInfo()->dwUserID, pIServerUserItem->GetChairID());
						CTraceService::TraceString(szString, TraceLevel_Normal);

						//mChen edit, for HideSeek:游戏结束时离线玩家处理：强制离线玩家离开
						PerformStandUpActionEx(pIServerUserItem);
						///PerformStandUpAction(pIServerUserItem);
					}
					else
					{
						pIServerUserItem->SetClientReady(true);
						pIServerUserItem->SetUserStatus(US_SIT,m_wTableID,i);
					}
				}
				else
				{
					//设置状态
					pIServerUserItem->SetUserStatus(US_SIT,m_wTableID,i);

				}
			}
		}

		//删除时间
		if (bOffLineWait==true) KillGameTimer(IDI_OFF_LINE);
	}

	//通知比赛
	if(m_pITableFrameHook!=NULL) m_pITableFrameHook->OnEventGameEnd(this,0, NULL, cbEndReason);

	//通知比赛
	if(m_pITableFramePrivate!=NULL) m_pITableFramePrivate->OnEventGameEnd(this,0, NULL, cbEndReason);
	
	//重置桌子
	ASSERT(m_pITableFrameSink!=NULL);
	if (m_pITableFrameSink!=NULL) m_pITableFrameSink->RepositionSink();

	//踢出检测
	if (m_bGameStarted==false)
	{
		for (WORD i=0;i<m_wChairCount;i++)
		{
			//获取用户
			if (m_TableUserItemArray[i]==NULL) continue;
			IServerUserItem * pIServerUserItem=m_TableUserItemArray[i];

			//积分限制
			if ((m_pGameServiceOption->lMinTableScore!=0L)&&(pIServerUserItem->GetUserScore()<m_pGameServiceOption->lMinTableScore))
			{
				//构造提示
				TCHAR szDescribe[128]=TEXT("");
				if (m_pGameServiceOption->wServerType&GAME_GENRE_GOLD)
				{
					_sntprintf(szDescribe,CountArray(szDescribe),TEXT("您的游戏币少于 ") SCORE_STRING TEXT("，不能继续游戏！"),m_pGameServiceOption->lMinTableScore);
				}
				else
				{
					_sntprintf(szDescribe,CountArray(szDescribe),TEXT("您的游戏积分少于 ") SCORE_STRING TEXT("，不能继续游戏！"),m_pGameServiceOption->lMinTableScore);
				}

				//发送消息
				if (pIServerUserItem->IsAndroidUser()==true)
					SendGameMessage(pIServerUserItem,szDescribe,SMT_CHAT|SMT_CLOSE_GAME|SMT_CLOSE_ROOM|SMT_EJECT);
				else
					SendGameMessage(pIServerUserItem,szDescribe,SMT_CHAT|SMT_CLOSE_GAME|SMT_EJECT);

				//用户起立
				PerformStandUpAction(pIServerUserItem);

				continue;
			}

			//关闭判断
			if ((CServerRule::IsForfendGameEnter(m_pGameServiceOption->dwServerRule)==true)&&(pIServerUserItem->GetMasterOrder()==0))
			{
				//发送消息
				LPCTSTR pszMessage=TEXT("由于系统维护，当前游戏桌子禁止用户继续游戏！");
				SendGameMessage(pIServerUserItem,pszMessage,SMT_EJECT|SMT_CHAT|SMT_CLOSE_GAME);

				//用户起立
				PerformStandUpAction(pIServerUserItem);

				continue;
			}
		}
	}

	//结束桌子
	ConcludeTable();

	//发送状态
	SendTableStatus();

	return true;
}

//结束桌子
bool CTableFrame::ConcludeTable()
{
	//结束桌子
	if ((m_bGameStarted==false)&&(m_bTableStarted==true))
	{
		//人数判断
		WORD wTableUserCount=GetSitUserCount();
		if (wTableUserCount==0) m_bTableStarted=false;
		if (m_pGameServiceAttrib->wChairCount==MAX_CHAIR) m_bTableStarted=false;

		//模式判断
		if (m_cbStartMode==START_MODE_FULL_READY) m_bTableStarted=false;
		if (m_cbStartMode==START_MODE_PAIR_READY) m_bTableStarted=false;
		if (m_cbStartMode==START_MODE_ALL_READY) m_bTableStarted=false;
	}

	return true;
}

//写入积分
bool CTableFrame::WriteUserScore(WORD wChairID, tagScoreInfo & ScoreInfo)
{
	//效验参数
	ASSERT((wChairID<m_wChairCount)&&(ScoreInfo.cbType!=SCORE_TYPE_NULL));
	if ((wChairID>=m_wChairCount)&&(ScoreInfo.cbType==SCORE_TYPE_NULL)) return false;

	//获取用户
	ASSERT(GetTableUserItem(wChairID)!=NULL);
	IServerUserItem * pIServerUserItem=GetTableUserItem(wChairID);
	TCHAR szMessage[128]=TEXT("");

	//写入积分
	if (pIServerUserItem!=NULL)
	{
		//变量定义
		DWORD dwUserMemal=0L;
		SCORE lRevenueScore=__min(m_lFrozenedScore[wChairID],m_pGameServiceOption->lServiceScore);

		//扣服务费
		if (m_pGameServiceOption->lServiceScore>0L 
			&& m_pGameServiceOption->wServerType == GAME_GENRE_GOLD
			&& m_pITableFrameSink->QueryBuckleServiceCharge(wChairID))
		{
			//扣服务费
			ScoreInfo.lScore-=lRevenueScore;
			ScoreInfo.lRevenue+=lRevenueScore;

			//解锁游戏币
			pIServerUserItem->UnFrozenedUserScore(m_lFrozenedScore[wChairID]);
			m_lFrozenedScore[wChairID]=0L;
		}

		//奖牌计算
		if (ScoreInfo.lRevenue>0L)
		{
			WORD wMedalRate=m_pGameParameter->wMedalRate;
			dwUserMemal=(DWORD)(ScoreInfo.lRevenue*wMedalRate/1000L);
		}

		//游戏时间
		DWORD dwCurrentTime=(DWORD)time(NULL);
		DWORD dwPlayTimeCount=(m_bDrawStarted==true)?dwCurrentTime-m_dwDrawStartTime:0L;

		//变量定义
		tagUserProperty * pUserProperty=pIServerUserItem->GetUserProperty();

		//道具判断
		if(m_pGameServiceOption->wServerType == GAME_GENRE_SCORE)
		{
			if (ScoreInfo.lScore>0L)
			{
				//四倍积分
				if ((pUserProperty->wPropertyUseMark&PT_USE_MARK_FOURE_SCORE)!=0)
				{
					//变量定义
					DWORD dwValidTime=pUserProperty->PropertyInfo[1].wPropertyCount*pUserProperty->PropertyInfo[1].dwValidNum;
					if(pUserProperty->PropertyInfo[1].dwEffectTime+dwValidTime>dwCurrentTime)
					{
						//积分翻倍
						ScoreInfo.lScore *= 4;
						_sntprintf(szMessage,CountArray(szMessage),TEXT("[ %s ] 使用了[ 四倍积分卡 ]，得分翻四倍！)"),pIServerUserItem->GetNickName());
					}
					else
					{
						pUserProperty->wPropertyUseMark&=~PT_USE_MARK_FOURE_SCORE;
					}
				} //双倍积分
				else if ((pUserProperty->wPropertyUseMark&PT_USE_MARK_DOUBLE_SCORE)!=0)
				{
					//变量定义
					DWORD dwValidTime=pUserProperty->PropertyInfo[0].wPropertyCount*pUserProperty->PropertyInfo[0].dwValidNum;
					if (pUserProperty->PropertyInfo[0].dwEffectTime+dwValidTime>dwCurrentTime)
					{
						//积分翻倍
						ScoreInfo.lScore*=2L;
						_sntprintf(szMessage,CountArray(szMessage),TEXT("[ %s ] 使用了[ 双倍积分卡 ]，得分翻倍！"), pIServerUserItem->GetNickName());
					}
					else
					{
						pUserProperty->wPropertyUseMark&=~PT_USE_MARK_DOUBLE_SCORE;
					}
				}
			}
			else
			{
				//附身符
				if ((pUserProperty->wPropertyUseMark&PT_USE_MARK_POSSESS)!=0)
				{
					//变量定义
					DWORD dwValidTime=pUserProperty->PropertyInfo[3].wPropertyCount*pUserProperty->PropertyInfo[3].dwValidNum;
					if(pUserProperty->PropertyInfo[3].dwEffectTime+dwValidTime>dwCurrentTime)
					{
						//积分翻倍
						ScoreInfo.lScore = 0;
						_sntprintf(szMessage,CountArray(szMessage),TEXT("[ %s ] 使用了[ 护身符卡 ]，积分不变！"),pIServerUserItem->GetNickName());
					}
					else
					{
						pUserProperty->wPropertyUseMark &= ~PT_USE_MARK_POSSESS;
					}
				}
			}
		}

		SCORE lScore = ScoreInfo.lScore;
		if (lScore + pIServerUserItem->GetUserScore() < 0)
		{
			lScore = -pIServerUserItem->GetUserScore();
		}
			//写入积分
		pIServerUserItem->WriteUserScore(lScore,ScoreInfo.lGrade,ScoreInfo.lRevenue,dwUserMemal,ScoreInfo.cbType,dwPlayTimeCount);
	

		//游戏记录
		if (pIServerUserItem->IsAndroidUser()==false && CServerRule::IsRecordGameScore(m_pGameServiceOption->dwServerRule)==true)
		{
			//变量定义
			tagGameScoreRecord * pGameScoreRecord=NULL;

			//查询库存
			if (m_GameScoreRecordBuffer.GetCount()>0L)
			{
				//获取对象
				INT_PTR nCount=m_GameScoreRecordBuffer.GetCount();
				pGameScoreRecord=m_GameScoreRecordBuffer[nCount-1];

				//删除对象
				m_GameScoreRecordBuffer.RemoveAt(nCount-1);
			}

			//创建对象
			if (pGameScoreRecord==NULL)
			{
				try
				{
					//创建对象
					pGameScoreRecord=new tagGameScoreRecord;
					if (pGameScoreRecord==NULL) throw TEXT("游戏记录对象创建失败");
				}
				catch (...)
				{
					ASSERT(FALSE);
				}
			}

			//记录数据
			if (pGameScoreRecord!=NULL)
			{
				//用户信息
				pGameScoreRecord->wChairID=wChairID;
				pGameScoreRecord->dwUserID=pIServerUserItem->GetUserID();
				pGameScoreRecord->cbAndroid=(pIServerUserItem->IsAndroidUser()?TRUE:FALSE);

				//用户信息
				pGameScoreRecord->dwDBQuestID=pIServerUserItem->GetDBQuestID();
				pGameScoreRecord->dwInoutIndex=pIServerUserItem->GetInoutIndex();

				//成绩信息
				pGameScoreRecord->lScore=ScoreInfo.lScore;
				pGameScoreRecord->lGrade=ScoreInfo.lGrade;
				pGameScoreRecord->lRevenue=ScoreInfo.lRevenue;

				//附加信息
				pGameScoreRecord->dwUserMemal=dwUserMemal;
				pGameScoreRecord->dwPlayTimeCount=dwPlayTimeCount;

				//机器人免税
				if(pIServerUserItem->IsAndroidUser())
				{
					pGameScoreRecord->lScore += pGameScoreRecord->lRevenue;
					pGameScoreRecord->lRevenue = 0L;
				}

				//插入数据
				m_GameScoreRecordActive.Add(pGameScoreRecord);
			}
		}
	}
	else
	{
		//离开用户
		CTraceService::TraceString(TEXT("系统暂时未支持离开用户的补分操作处理！"),TraceLevel_Exception);

		return false;
	}

	//广播消息
	if (szMessage[0]!=0)
	{
		//变量定义
		IServerUserItem * pISendUserItem = NULL;
		WORD wEnumIndex=0;

		//游戏玩家
		for (WORD i=0;i<m_wChairCount;i++)
		{
			//获取用户
			pISendUserItem=GetTableUserItem(i);
			if(pISendUserItem==NULL) continue;

			//发送消息
			SendGameMessage(pISendUserItem, szMessage, SMT_CHAT);
		}

		//旁观用户
		do
		{
			pISendUserItem=EnumLookonUserItem(wEnumIndex++);
			if(pISendUserItem!=NULL) 
			{
				//发送消息
				SendGameMessage(pISendUserItem, szMessage, SMT_CHAT);
			}
		} while (pISendUserItem!=NULL);
	}

	return true;
}

//写入积分
bool CTableFrame::WriteTableScore(tagScoreInfo ScoreInfoArray[], WORD wScoreCount,datastream kDataStream)
{
	//效验参数
	ASSERT(wScoreCount==m_wChairCount);
	if (wScoreCount!=m_wChairCount) return false;

	//写入分数
	for (WORD i=0;i<m_wChairCount;i++)
	{
		if (ScoreInfoArray[i].cbType!=SCORE_TYPE_NULL)
		{
			WriteUserScore(i,ScoreInfoArray[i]);
		}
	}

	if (m_pITableFramePrivate)
	{
		m_pITableFramePrivate->WriteTableScore(this,ScoreInfoArray,wScoreCount,kDataStream);
	}
	return true;
}

//计算税收
SCORE CTableFrame::CalculateRevenue(WORD wChairID, SCORE lScore)
{
	//效验参数
	ASSERT(wChairID<m_wChairCount);
	if (wChairID>=m_wChairCount) return 0L;

	//计算税收
	if ((m_pGameServiceOption->wRevenueRatio>0)&&(lScore>=REVENUE_BENCHMARK))
	{
		//获取用户
		ASSERT(GetTableUserItem(wChairID)!=NULL);
		IServerUserItem * pIServerUserItem=GetTableUserItem(wChairID);

		//计算税收
		SCORE lRevenue=lScore*m_pGameServiceOption->wRevenueRatio/REVENUE_DENOMINATOR;

		return lRevenue;
	}

	return 0L;
}

//消费限额
SCORE CTableFrame::QueryConsumeQuota(IServerUserItem * pIServerUserItem)
{
	//用户效验
	ASSERT(pIServerUserItem->GetTableID()==m_wTableID);
	if (pIServerUserItem->GetTableID()!=m_wTableID) return 0L;

	//查询额度
	SCORE lTrusteeScore=pIServerUserItem->GetTrusteeScore();
	SCORE lMinEnterScore=m_pGameServiceOption->lMinTableScore;
	SCORE lUserConsumeQuota=m_pITableFrameSink->QueryConsumeQuota(pIServerUserItem);

	//效验额度
	ASSERT((lUserConsumeQuota>=0L)&&(lUserConsumeQuota<=pIServerUserItem->GetUserScore()-lMinEnterScore));
	if ((lUserConsumeQuota<0L)||(lUserConsumeQuota>pIServerUserItem->GetUserScore()-lMinEnterScore)) return 0L;

	return lUserConsumeQuota+lTrusteeScore;
}

//寻找用户
IServerUserItem * CTableFrame::SearchUserItem(DWORD dwUserID)
{
	//变量定义
	WORD wEnumIndex=0;
	IServerUserItem * pIServerUserItem=NULL;

	//桌子用户
	for (WORD i=0;i<m_wChairCount;i++)
	{
		pIServerUserItem=GetTableUserItem(i);
		if ((pIServerUserItem!=NULL)&&(pIServerUserItem->GetUserID()==dwUserID)) return pIServerUserItem;
	}

	//旁观用户
	do
	{
		pIServerUserItem=EnumLookonUserItem(wEnumIndex++);
		if ((pIServerUserItem!=NULL)&&(pIServerUserItem->GetUserID()==dwUserID)) return pIServerUserItem;
	} while (pIServerUserItem!=NULL);

	return NULL;
}

//游戏用户
IServerUserItem * CTableFrame::GetTableUserItem(WORD wChairID)
{
	//效验参数
	ASSERT(wChairID<m_wChairCount);
	if (wChairID>=m_wChairCount) return NULL;

	//获取用户
	return m_TableUserItemArray[wChairID];
}

//旁观用户
IServerUserItem * CTableFrame::EnumLookonUserItem(WORD wEnumIndex)
{
	if (wEnumIndex>=m_LookonUserItemArray.GetCount()) return NULL;
	return m_LookonUserItemArray[wEnumIndex];
}

//设置时间
bool CTableFrame::SetGameTimer(DWORD dwTimerID, DWORD dwElapse, DWORD dwRepeat, WPARAM dwBindParameter)
{
	//效验参数
	ASSERT((dwTimerID>0)&&(dwTimerID<TIME_TABLE_MODULE_RANGE));
	if ((dwTimerID<=0)||(dwTimerID>=TIME_TABLE_MODULE_RANGE)) return false;

	//设置时间
	DWORD dwEngineTimerID=IDI_TABLE_MODULE_START+m_wTableID*TIME_TABLE_MODULE_RANGE;
	if (m_pITimerEngine!=NULL) m_pITimerEngine->SetTimer(dwEngineTimerID+dwTimerID,dwElapse,dwRepeat,dwBindParameter);
	return true;
}

//删除时间
bool CTableFrame::KillGameTimer(DWORD dwTimerID)
{
	//效验参数
	ASSERT((dwTimerID>0)&&(dwTimerID<=TIME_TABLE_MODULE_RANGE));
	if ((dwTimerID<=0)||(dwTimerID>TIME_TABLE_MODULE_RANGE)) return false;

	//删除时间
	DWORD dwEngineTimerID=IDI_TABLE_MODULE_START+m_wTableID*TIME_TABLE_MODULE_RANGE;
	if (m_pITimerEngine!=NULL) m_pITimerEngine->KillTimer(dwEngineTimerID+dwTimerID);

	return true;
}

//发送数据
bool CTableFrame::SendUserItemData(IServerUserItem * pIServerUserItem, WORD wSubCmdID)
{
	//状态效验
	ASSERT((pIServerUserItem!=NULL)&&(pIServerUserItem->IsClientReady()==true));
	if ((pIServerUserItem==NULL)&&(pIServerUserItem->IsClientReady()==false)) return false;

	//发送数据
	m_pIMainServiceFrame->SendData(pIServerUserItem,MDM_GF_GAME,wSubCmdID,NULL,0);

	return true;
}

//发送数据
bool CTableFrame::SendUserItemData(IServerUserItem * pIServerUserItem, WORD wSubCmdID, VOID * pData, DWORD wDataSize)
{
	//状态效验
	ASSERT((pIServerUserItem!=NULL)&&(pIServerUserItem->IsClientReady()==true));
	if ((pIServerUserItem==NULL)&&(pIServerUserItem->IsClientReady()==false)) return false;

	//发送数据
	m_pIMainServiceFrame->SendData(pIServerUserItem,MDM_GF_GAME,wSubCmdID,pData,wDataSize);

	return true;
}

//发送数据
bool CTableFrame::SendTableData(WORD wChairID, WORD wSubCmdID)
{
	//用户群发
	if (wChairID==INVALID_CHAIR)
	{
		for (WORD i=0;i<m_wChairCount;i++)
		{
			//获取用户
			IServerUserItem * pIServerUserItem=GetTableUserItem(i);
			if (pIServerUserItem==NULL) continue;

			//效验状态
			ASSERT(pIServerUserItem->IsClientReady()==true);
			if (pIServerUserItem->IsClientReady()==false) continue;

			//发送数据
			m_pIMainServiceFrame->SendData(pIServerUserItem,MDM_GF_GAME,wSubCmdID,NULL,0);
		}

		return true;
	}
	else
	{
		//获取用户
		IServerUserItem * pIServerUserItem=GetTableUserItem(wChairID);
		if (pIServerUserItem==NULL) return false;

		//效验状态
		ASSERT(pIServerUserItem->IsClientReady()==true);
		if (pIServerUserItem->IsClientReady()==false) return false;

		//发送数据
		m_pIMainServiceFrame->SendData(pIServerUserItem,MDM_GF_GAME,wSubCmdID,NULL,0);

		return true;
	}

	return false;
}

//发送数据
bool CTableFrame::SendTableData(WORD wChairID, WORD wSubCmdID, VOID * pData, DWORD wDataSize,WORD wMainCmdID)
{
	//用户群发
	if (wChairID==INVALID_CHAIR)
	{
		for (WORD i=0;i<m_wChairCount;i++)
		{
			//获取用户
			IServerUserItem * pIServerUserItem=GetTableUserItem(i);
			if ((pIServerUserItem==NULL)||(pIServerUserItem->IsClientReady()==false)) continue;
			if ((pIServerUserItem==NULL)) continue;

			//发送数据
			m_pIMainServiceFrame->SendData(pIServerUserItem,wMainCmdID,wSubCmdID,pData,wDataSize);
		}

		return true;
	}
	else
	{
		//获取用户
		IServerUserItem * pIServerUserItem=GetTableUserItem(wChairID);
		if ((pIServerUserItem==NULL)||(pIServerUserItem->IsClientReady()==false)) return false;

		//发送数据
		m_pIMainServiceFrame->SendData(pIServerUserItem,wMainCmdID,wSubCmdID,pData,wDataSize);

		return true;
	}

	return false;
}

//发送数据
bool CTableFrame::SendLookonData(WORD wChairID, WORD wSubCmdID)
{
	//变量定义
	WORD wEnumIndex=0;
	IServerUserItem * pIServerUserItem=NULL;

	//枚举用户
	do
	{
		//获取用户
		pIServerUserItem=EnumLookonUserItem(wEnumIndex++);
		if (pIServerUserItem==NULL) break;

		//效验状态
		ASSERT(pIServerUserItem->IsClientReady()==true);
		if (pIServerUserItem->IsClientReady()==false) return false;

		//发送数据
		if ((wChairID==INVALID_CHAIR)||(pIServerUserItem->GetChairID()==wChairID))
		{
			m_pIMainServiceFrame->SendData(pIServerUserItem,MDM_GF_GAME,wSubCmdID,NULL,0);
		}

	} while (true);

	return true;
}

//发送数据
bool CTableFrame::SendLookonData(WORD wChairID, WORD wSubCmdID, VOID * pData, DWORD wDataSize, WORD wMainCmdID)//, WORD wMainCmdID
{
	//变量定义
	WORD wEnumIndex=0;
	IServerUserItem * pIServerUserItem=NULL;

	//枚举用户
	do
	{
		//获取用户
		pIServerUserItem=EnumLookonUserItem(wEnumIndex++);
		if (pIServerUserItem==NULL) break;

		//效验状态
		//ASSERT(pIServerUserItem->IsClientReady()==true);
		if (pIServerUserItem->IsClientReady()==false) return false;

		//发送数据
		if ((wChairID==INVALID_CHAIR)||(pIServerUserItem->GetChairID()==wChairID))
		{
			m_pIMainServiceFrame->SendData(pIServerUserItem,wMainCmdID,wSubCmdID,pData,wDataSize);
		}

	} while (true);

	return true;
}

//发送消息
bool CTableFrame::SendGameMessage(LPCTSTR lpszMessage, WORD wType)
{
	//变量定义
	WORD wEnumIndex=0;

	//发送消息
	for (WORD i=0;i<m_wChairCount;i++)
	{
		//获取用户
		IServerUserItem * pIServerUserItem=GetTableUserItem(i);

		if ( (pIServerUserItem!=NULL) && (pIServerUserItem->IsClientReady()==false) && (pIServerUserItem->GetUserStatus()!=US_OFFLINE) )
		{
			//mChen log
			TCHAR szString[256] = TEXT("");
			_sntprintf( szString, CountArray(szString), TEXT("CTableFrame::SendGameMessage:IsClientReady false: userName=%s, userId=%d, chairId=%d, UserStatus= %d "), pIServerUserItem->GetNickName(), pIServerUserItem->GetUserInfo()->dwUserID, pIServerUserItem->GetChairID(), pIServerUserItem->GetUserStatus());
			CTraceService::TraceString(szString, TraceLevel_Warning);

			//mChen hack, temp
			//pIServerUserItem->SetClientReady(true);
		}

		if ((pIServerUserItem==NULL)||(pIServerUserItem->IsClientReady()==false)) continue;

		//发送消息
		m_pIMainServiceFrame->SendGameMessage(pIServerUserItem,lpszMessage,wType);
	}

	//枚举用户
	do
	{
		//获取用户
		IServerUserItem * pIServerUserItem=EnumLookonUserItem(wEnumIndex++);
		if (pIServerUserItem==NULL) break;

		if ((pIServerUserItem != NULL) && (pIServerUserItem->IsClientReady() == false) && (pIServerUserItem->GetUserStatus() != US_OFFLINE))
		{
			//mChen log
			TCHAR szString[256] = TEXT("");
			_sntprintf(szString, CountArray(szString), TEXT("CTableFrame::SendGameMessage:Lookon user %d IsClientReady false , UserStatus= %d "), pIServerUserItem->GetChairID(), pIServerUserItem->GetUserStatus());
			CTraceService::TraceString(szString, TraceLevel_Warning);

			//mChen hack, temp
			pIServerUserItem->SetClientReady(true);
		}

		//效验状态
		ASSERT(pIServerUserItem->IsClientReady()==true);
		if (pIServerUserItem->IsClientReady()==false) return false;

		//发送消息
		m_pIMainServiceFrame->SendGameMessage(pIServerUserItem,lpszMessage,wType);

	} while (true);

	return true;
}

//房间消息
bool CTableFrame::SendRoomMessage(IServerUserItem * pIServerUserItem, LPCTSTR lpszMessage, WORD wType)
{
	//用户效验
	ASSERT(pIServerUserItem!=NULL);
	if (pIServerUserItem==NULL) return false;

	//发送消息
	m_pIMainServiceFrame->SendRoomMessage(pIServerUserItem,lpszMessage,wType);

	return true;
}

//游戏消息
bool CTableFrame::SendGameMessage(IServerUserItem * pIServerUserItem, LPCTSTR lpszMessage, WORD wType)
{
	//用户效验
	ASSERT(pIServerUserItem!=NULL);
	if (pIServerUserItem==NULL) return false;

	//发送消息
	return m_pIMainServiceFrame->SendGameMessage(pIServerUserItem,lpszMessage,wType);
}

//发送场景
bool CTableFrame::SendGameScene(IServerUserItem * pIServerUserItem, VOID * pData, WORD wDataSize)
{
	//用户效验
	ASSERT((pIServerUserItem!=NULL)&&(pIServerUserItem->IsClientReady()==true));
	if ((pIServerUserItem==NULL)||(pIServerUserItem->IsClientReady()==false)) return false;

	//发送场景
	ASSERT(m_pIMainServiceFrame!=NULL);
	m_pIMainServiceFrame->SendData(pIServerUserItem,MDM_GF_FRAME,SUB_GF_GAME_SCENE,pData,wDataSize);

	return true;
}

//设置接口
bool CTableFrame::SetTableFrameHook(IUnknownEx * pIUnknownEx)
{
	ASSERT(pIUnknownEx!=NULL);
	if(pIUnknownEx==NULL) return false;

	//类型判断
	if((m_pGameServiceOption->wServerType&GAME_GENRE_MATCH)==0) return false;

	//查询接口
	m_pITableFrameHook=QUERY_OBJECT_PTR_INTERFACE(pIUnknownEx,ITableFrameHook);
	m_pIMatchTableAction=QUERY_OBJECT_PTR_INTERFACE(pIUnknownEx,ITableUserAction);

	return true;
}
bool CTableFrame::SetTableFramePrivate(IUnknownEx * pIUnknownEx)
{
	ASSERT(pIUnknownEx!=NULL);
	if(pIUnknownEx==NULL) return false;

	//类型判断
	if((m_pGameServiceOption->wServerType&GAME_GENRE_EDUCATE)==0) return false;

	//查询接口
	m_pITableFramePrivate=QUERY_OBJECT_PTR_INTERFACE(pIUnknownEx,ITableFramePrivate);
	m_pIPrivateTableAction=QUERY_OBJECT_PTR_INTERFACE(pIUnknownEx,ITableUserAction);

	return true;
}
//添加事件
void CTableFrame::AddPrivateAction(DWORD dwChairID,BYTE	bActionIdex)
{
	if (m_pITableFramePrivate)
	{
		m_pITableFramePrivate->AddPrivateAction(this,dwChairID,bActionIdex);
	}
}
//设置私人场信息
void CTableFrame::SetPrivateInfo(BYTE bGameTypeIdex,DWORD bGameRuleIdex,SCORE lBaseScore, BYTE PlayCout, BYTE PlayerCount)
{
	if (m_pITableFrameSink)
	{
		m_wChairCount = PlayerCount;//ZY add

		m_pITableFrameSink->SetPrivateInfo(bGameTypeIdex,bGameRuleIdex,lBaseScore, PlayCout, PlayerCount);
	}
}
void CTableFrame::ResetPlayerTotalScore() {
	m_pITableFrameSink->ResetPlayerTotalScore();
}
//断线事件
bool CTableFrame::OnEventUserOffLine(IServerUserItem * pIServerUserItem)
{
	//参数效验
	ASSERT(pIServerUserItem!=NULL);
	if (pIServerUserItem==NULL) return false;

	//用户变量
	tagUserInfo * pUserInfo=pIServerUserItem->GetUserInfo();
	IServerUserItem * pITableUserItem=m_TableUserItemArray[pUserInfo->wChairID];

	//用户属性
	WORD wChairID=pIServerUserItem->GetChairID();
	BYTE cbUserStatus=pIServerUserItem->GetUserStatus();

	//游戏用户
	if (cbUserStatus!=US_LOOKON)
	{
		//效验用户
		ASSERT(pIServerUserItem==GetTableUserItem(wChairID));
		if (pIServerUserItem!=GetTableUserItem(wChairID)) return false;


		//私人类型
		if(m_pGameServiceOption->wServerType==GAME_GENRE_EDUCATE)
		{
			//mChen comment：因为游戏结束时会让离线玩家强制离开
			////mChen add, for HideSeek:玩家在游戏快结束时离线，强制离开（踢出）：fix游戏快结束的时候断线重连回来已经第二轮的bug
			//if ( (cbUserStatus == US_PLAYING && m_cbGameStatus == GAME_STATUS_PLAY && m_wGamePlayTime <= 20) || (m_cbGameStatus == GAME_STATUS_END) )
			//{
			//	//Log
			//	TCHAR szString[128] = TEXT("");
			//	_sntprintf(szString, CountArray(szString), TEXT("OnEventUserOffLine:userName=%s,userId=%d,玩家在游戏快结束时离线，强制踢出"), pIServerUserItem->GetNickName(), pIServerUserItem->GetUserInfo()->dwUserID);
			//	CTraceService::TraceString(szString, TraceLevel_Normal);

			//	PerformStandUpActionEx(pIServerUserItem);
			//	pIServerUserItem->SetUserStatus(US_NULL, INVALID_TABLE, INVALID_CHAIR);
			//	return true;
			//}

			pIServerUserItem->SetUserStatus(US_OFFLINE, m_wTableID, wChairID);

			//掉线通知
			if(m_pIPrivateTableAction!=NULL) m_pIPrivateTableAction->OnActionUserOffLine(wChairID,pIServerUserItem, __FUNCTION__);

			return true;
		}

		//断线处理
		if ((cbUserStatus==US_PLAYING)&&(m_wOffLineCount[wChairID]<MAX_OFF_LINE))
		{
			//用户设置
			pIServerUserItem->SetClientReady(false);
			pIServerUserItem->SetUserStatus(US_OFFLINE,m_wTableID,wChairID);

			//比赛类型
			if(m_pGameServiceOption->wServerType==GAME_GENRE_MATCH)
			{
				//if(pIServerUserItem->IsTrusteeUser()==false)
				{
					//设置托管
					//pIServerUserItem->SetTrusteeUser(true);

					//掉线通知
					if(m_pITableUserAction!=NULL) m_pITableUserAction->OnActionUserOffLine(wChairID,pIServerUserItem, __FUNCTION__);
				}

				return true;
			}

			//掉线通知
			if(m_pITableUserAction!=NULL) m_pITableUserAction->OnActionUserOffLine(wChairID,pIServerUserItem, __FUNCTION__);

			//断线处理
			if (m_dwOffLineTime[wChairID]==0L)
			{
				//设置变量
				m_wOffLineCount[wChairID]++;
				m_dwOffLineTime[wChairID]=(DWORD)time(NULL);

				//时间设置
				WORD wOffLineCount=GetOffLineUserCount();
				if (wOffLineCount==1) SetGameTimer(IDI_OFF_LINE,TIME_OFF_LINE,1,wChairID);
			}

			return true;
		}
	}
	//用户起立
	PerformStandUpAction(pIServerUserItem);

	//删除用户
	ASSERT(pIServerUserItem->GetUserStatus()==US_FREE);
	pIServerUserItem->SetUserStatus(US_NULL,INVALID_TABLE,INVALID_CHAIR);

	return true;
}

//积分事件
bool CTableFrame::OnUserScroeNotify(WORD wChairID, IServerUserItem * pIServerUserItem, BYTE cbReason)
{
	//通知游戏
	return m_pITableFrameSink->OnUserScroeNotify(wChairID,pIServerUserItem,cbReason);
}


//mChen add, for HideSeek
void CTableFrame::StartWaitGame()
{
	// Log
	CTraceService::TraceString(TEXT("CTableFrame::StartWaitGame try"), TraceLevel_Normal);

	//第一个人
	if (m_cbGameStatus != GAME_STATUS_FREE)
	{
		return;
	}

	// Log
	CTraceService::TraceString(TEXT("CTableFrame::StartWaitGame trigger"), TraceLevel_Normal);

	m_cbGameStatus = GAME_STATUS_WAIT;
	m_wWaitGameTime = HIDESEEK_WAIT_GAME_TIME;
	m_wHiddingTime = HIDESEEK_HIDING_TIME;
	m_wGamePlayTime = HIDESEEK_GAME_PLAY_TIME;
	m_cbGameEndReason = GER_NORMAL;

	SetGameTimer(IDI_HIDESEEK_WAIT_GAME, 1L * 1000L, TIMES_INFINITY, 0);//1s
	SetGameTimer(IDI_HIDESEEK_END_WAIT_GAME, (HIDESEEK_WAIT_GAME_TIME+1)*1000L, 1, 0);//8s

	//发送状态
	CMD_GF_GameStatus GameStatus;
	GameStatus.cbGameStatus = m_cbGameStatus;
	GameStatus.cbAllowLookon = TRUE;
	SendTableData(INVALID_CHAIR, SUB_GF_GAME_STATUS, &GameStatus, sizeof(GameStatus), MDM_GF_FRAME);
	SendLookonData(INVALID_CHAIR, SUB_GF_GAME_STATUS, &GameStatus, sizeof(GameStatus), MDM_GF_FRAME);
}
void CTableFrame::HideSeek_FreeGameAndTimers(BYTE cbEndReason)
{
	KillGameTimer(IDI_HIDESEEK_WAIT_GAME);
	KillGameTimer(IDI_HIDESEEK_END_WAIT_GAME);
	KillGameTimer(IDI_HIDESEEK_USERS_TICK);
	KillGameTimer(IDI_HIDESEEK_HIDDING_TIME);
	KillGameTimer(IDI_HIDESEEK_END_HIDDING_TIME);
	KillGameTimer(IDI_HIDESEEK_GAME_PLAY_TIME);
	KillGameTimer(IDI_HIDESEEK_END_GAME);

	KillGameTimer(IDI_HIDESEEK_USERS_TICK);
	KillGameTimer(IDI_HIDESEEK_GAME_PLAY_TIME);
	KillGameTimer(IDI_HIDESEEK_END_GAME);

	m_cbGameStatus = GAME_STATUS_FREE;
	///ConcludeGame(GAME_STATUS_FREE,cbEndReason);

	//重置m_wMapIndexRand
	m_wMapIndexRand = rand() % 0xFF;
	//for随机种子同步
	m_wRandseed = (WORD)(rand() % 0xFFFF);
	m_wRandseedForRandomGameObject = (WORD)(rand() % 0xFFFF);
	m_wRandseedForInventory = (WORD)(rand() % 0xFFFF);
	//道具同步
	if (m_pITableFrameSink)
	{
		m_pITableFrameSink->ResetInventoryList();
	}

	// Log
	TCHAR szString[128] = TEXT("");
	_sntprintf(szString, CountArray(szString), TEXT("CTableFrame::HideSeek_FreeGameAndTimers: m_wMapIndexRand=%d, tableId=%d"), m_wMapIndexRand, m_wTableID);
	CTraceService::TraceString(szString, TraceLevel_Normal);

	//蓄房提前重置所有人的TeamType和ModelIndex，保证所有用户在收到蓄房消息前重置完毕
	if (m_pITableFramePrivate != NULL)
	{
		for (WORD i = 0; i < m_wChairCount; i++)
		{
			if (m_TableUserItemArray[i] == NULL) continue;

			IServerUserItem * pIServerUserItem = m_TableUserItemArray[i];
			m_pITableFramePrivate->RandomUserTeamTypeAndModelIndex(pIServerUserItem, m_wMapIndexRand, m_cbChoosedMapIndex, true);
		}
	}

	//发送状态
	CMD_GF_GameStatus GameStatus;
	GameStatus.cbGameStatus = m_cbGameStatus;
	GameStatus.cbAllowLookon = TRUE;
	SendTableData(INVALID_CHAIR, SUB_GF_GAME_STATUS, &GameStatus, sizeof(GameStatus), MDM_GF_FRAME);
	SendLookonData(INVALID_CHAIR, SUB_GF_GAME_STATUS, &GameStatus, sizeof(GameStatus), MDM_GF_FRAME);
}
void CTableFrame::SendUsersInfoPacket(IServerUserItem * pIServerUserItem)
{
	for (WORD i = 0; i < m_wChairCount; i++)
	{
		//获取用户
		if (m_TableUserItemArray[i] == NULL) continue;
		if (pIServerUserItem == NULL) continue;

		IServerUserItem * pSendIServerUserItem = m_TableUserItemArray[i];
		m_pIMainServiceFrame->SendUserInfoPacket(pSendIServerUserItem, pIServerUserItem);
		m_pIMainServiceFrame->SendUserInfoPacket(pIServerUserItem, pSendIServerUserItem);
	}
}

//时间事件
bool CTableFrame::OnEventTimer(DWORD dwTimerID, WPARAM dwBindParameter)
{
	//回调事件
	if ((dwTimerID>=0)&&(dwTimerID<TIME_TABLE_SINK_RANGE))
	{
		ASSERT(m_pITableFrameSink!=NULL);
		return m_pITableFrameSink->OnTimerMessage(dwTimerID,dwBindParameter);
	}

	//事件处理
	switch (dwTimerID)
	{
		//mChen add, for HideSeek
		case IDI_HIDESEEK_WAIT_GAME:
		{
			// Send message
			TCHAR szMessage[128] = TEXT("");
			_sntprintf(szMessage, CountArray(szMessage), TEXT("Waiting for game to start [%d]"), m_wWaitGameTime);
			SendGameMessage(szMessage, SMT_CHAT);

			m_wWaitGameTime--;

			return true;
		}
		case IDI_HIDESEEK_END_WAIT_GAME:
		{
			KillGameTimer(IDI_HIDESEEK_WAIT_GAME);
			KillGameTimer(IDI_HIDESEEK_END_WAIT_GAME);

			SetGameTimer(IDI_HIDESEEK_HIDDING_TIME, 1L*1000L, TIMES_INFINITY, 0);//1s
			SetGameTimer(IDI_HIDESEEK_USERS_TICK, 30L, TIMES_INFINITY, 0);//0.03s 30ms
			SetGameTimer(IDI_HIDESEEK_END_HIDDING_TIME, (HIDESEEK_HIDING_TIME+1)*1000L, 1, 0);//10s

			m_cbGameStatus = GAME_STATUS_HIDE;

			StartGame();

			//发送状态
			CMD_GF_GameStatus GameStatus;
			GameStatus.cbGameStatus = m_cbGameStatus;
			GameStatus.cbAllowLookon = TRUE;
			SendTableData(INVALID_CHAIR, SUB_GF_GAME_STATUS, &GameStatus, sizeof(GameStatus), MDM_GF_FRAME);
			SendLookonData(INVALID_CHAIR, SUB_GF_GAME_STATUS, &GameStatus, sizeof(GameStatus), MDM_GF_FRAME);

			//发送AI分配信息
			m_pITableFrameSink->GenerateAICreateInfo();
			m_pITableFrameSink->HideSeek_SendAICreateInfo();

			return true;
		}
		case IDI_HIDESEEK_HIDDING_TIME:
		{
			// Send message
			TCHAR szMessage[128] = TEXT("");
			_sntprintf(szMessage, CountArray(szMessage), TEXT("Hidding time [%d]"), m_wHiddingTime);
			SendGameMessage(szMessage, SMT_CHAT);

			m_wHiddingTime--;

			return true;
		}
		case IDI_HIDESEEK_USERS_TICK:
		{
			m_pITableFrameSink->HideSeek_SendHeartBeat();
			return true;
		}
		case IDI_HIDESEEK_END_HIDDING_TIME:
		{
			KillGameTimer(IDI_HIDESEEK_HIDDING_TIME);
			KillGameTimer(IDI_HIDESEEK_END_HIDDING_TIME);

			SetGameTimer(IDI_HIDESEEK_GAME_PLAY_TIME, 1L * 1000L, TIMES_INFINITY, 0);//1s
			SetGameTimer(IDI_HIDESEEK_END_GAME, (HIDESEEK_GAME_PLAY_TIME+1)*1000L, 1, 0);//30s

			//m_cbGameEndReason = GER_NORMAL;

			m_cbGameStatus = GAME_STATUS_PLAY;

			//发送状态
			CMD_GF_GameStatus GameStatus;
			GameStatus.cbGameStatus = m_cbGameStatus;
			GameStatus.cbAllowLookon = TRUE;
			SendTableData(INVALID_CHAIR, SUB_GF_GAME_STATUS, &GameStatus, sizeof(GameStatus), MDM_GF_FRAME);
			SendLookonData(INVALID_CHAIR, SUB_GF_GAME_STATUS, &GameStatus, sizeof(GameStatus), MDM_GF_FRAME);

			return true;
		}
		case IDI_HIDESEEK_GAME_PLAY_TIME:
		{
			// Send message
			TCHAR szMessage[128] = TEXT("");
			_sntprintf(szMessage, CountArray(szMessage), TEXT("Gameplay time [%d]"), m_wGamePlayTime);
			SendGameMessage(szMessage, SMT_CHAT);

			//道具生成同步
			if (m_wGamePlayTime == (HIDESEEK_GAME_PLAY_TIME - 10) || m_wGamePlayTime == 100)  //Play阶段开始和最后100s各刷一次
			{
				SendTableData(INVALID_CHAIR, SUB_GF_INVENTORY_CREATE, NULL, 0, MDM_GF_FRAME);
				SendLookonData(INVALID_CHAIR, SUB_GF_INVENTORY_CREATE, NULL, 0, MDM_GF_FRAME);
			}

			//道具隐身效果
			m_pITableFrameSink->StealthUpate();

			m_wGamePlayTime--;

			//一方死光
			for (PlayerTeamType teamType = PlayerTeamType::TaggerTeam; teamType < PlayerTeamType::MaxTeamNum; teamType=(PlayerTeamType)(teamType + 1))
			{
				if (m_pITableFrameSink->AreAllPlayersOfTeamDead(teamType))
				{
					// Log
					TCHAR szString[128] = TEXT("");
					_sntprintf(szString, CountArray(szString), TEXT("一方死光，强制结束游戏： teamType=%d"), teamType);
					CTraceService::TraceString(szString, TraceLevel_Normal);

					//一方死光,强制结束游戏
					m_cbGameEndReason = GER_NORMAL;
					KillGameTimer(IDI_HIDESEEK_END_GAME);
					SetGameTimer(IDI_HIDESEEK_END_GAME, 0, 1, 0);
					return true;
				}
			}

			//移到PriaveteGame::OnEventReqStandUP中执行
			////当所有人退出
			//WORD wNullChairCount = GetNullChairCount();
			//WORD wChairCount = GetChairCount();
			//if (wNullChairCount == wChairCount)
			//{
			//	//Log
			//	CTraceService::TraceString(TEXT("所有人都已经退出"), TraceLevel_Normal);

			//	//强制结束游戏
			//	m_cbGameEndReason = GER_DISMISS;
			//	KillGameTimer(IDI_HIDESEEK_END_GAME);
			//	SetGameTimer(IDI_HIDESEEK_END_GAME, 0, 1, 0);
			//	//DismissRoom(pTableInfo, GER_DISMISS);
			//	//ClearRoom(pTableInfo);
			//	return true;
			//}

			////没有在玩的玩家（已经离开或者断线）
			//WORD wPlayingUserCount = GetPlayingUserCount();
			//if (wPlayingUserCount == 0)
			//{
			//	//Log
			//	CTraceService::TraceString(TEXT("没有在玩的玩家"), TraceLevel_Normal);

			//	//强制结束游戏
			//	m_cbGameEndReason = GER_USER_LEAVE;
			//	KillGameTimer(IDI_HIDESEEK_END_GAME);
			//	SetGameTimer(IDI_HIDESEEK_END_GAME, 0, 1, 0);
			//	return true;
			//}

			return true;
		}
		case IDI_HIDESEEK_END_GAME:
		{
			KillGameTimer(IDI_HIDESEEK_USERS_TICK);
			KillGameTimer(IDI_HIDESEEK_GAME_PLAY_TIME);
			KillGameTimer(IDI_HIDESEEK_END_GAME);

			//发送状态
			m_cbGameStatus = GAME_STATUS_END;
			CMD_GF_GameStatus GameStatus;
			GameStatus.cbGameStatus = GAME_STATUS_END;
			GameStatus.cbAllowLookon = TRUE;
			SendTableData(INVALID_CHAIR, SUB_GF_GAME_STATUS, &GameStatus, sizeof(GameStatus), MDM_GF_FRAME);
			SendLookonData(INVALID_CHAIR, SUB_GF_GAME_STATUS, &GameStatus, sizeof(GameStatus), MDM_GF_FRAME);

			WORD wSitUserCount = GetSitUserCount();
			//if (wSitUserCount > 0)
			{
				//强制结束游戏
				m_pITableFrameSink->OnEventGameConclude(INVALID_CHAIR, NULL, m_cbGameEndReason);

				wSitUserCount = GetSitUserCount();

				if (wSitUserCount == 0 && m_pITableFramePrivate != NULL)
				{
					//所有人都离开了

					//IDI_HIDESEEK_END_GAME：游戏时间结束，房间内没有人了，强制解散房间
					//Log
					TCHAR szString[128] = TEXT("");
					_sntprintf(szString, CountArray(szString), TEXT("IDI_HIDESEEK_END_GAME：游戏时间结束，房间内没有人了，强制解散房间,TableId=%d"), m_wTableID);
					CTraceService::TraceString(szString, TraceLevel_Warning);
					m_pITableFramePrivate->ForceDismissAndClearRoom(m_wTableID);
				}
				else
				{
					SetGameTimer(IDI_HIDESEEK_PLAY_AGAIN_WAIT_END, 6000L, 1, 0);//6s
				}
			}

			return true;
		}
		case IDI_HIDESEEK_PLAY_AGAIN_WAIT_END:
		{
			//蓄房等待时间到

			if (m_cbGameStatus != GAME_STATUS_FREE)
			{
				return true;
			}

			WORD wSitUserCount = GetSitUserCount();
			WORD wPlayingUserCount = GetPlayingUserCount();
			//if (wSitUserCount == 0)
			//{
			//	return true;
			//}

			if (wPlayingUserCount > 0)
			{
				for (WORD i = 0; i < m_wChairCount; i++)
				{
					IServerUserItem * pITableUserItem = GetTableUserItem(i);
					if (pITableUserItem != NULL)
					{
						//Log
						//TCHAR szString[128] = TEXT("");
						//_sntprintf(szString, CountArray(szString), TEXT("IDI_HIDESEEK_PLAY_AGAIN_WAIT_END: userState=%d, userName=%s, userId=%d, chairId=%d"), pITableUserItem->GetUserStatus(), pITableUserItem->GetNickName(), pITableUserItem->GetUserInfo()->dwUserID, pITableUserItem->GetChairID());
						//CTraceService::TraceString(szString, TraceLevel_Warning);

						//mChen add, for HideSeek
						//离线玩家和还未蓄房的玩家强制离开，fix有人在团灭前断线，重连回来（已经下一局）还在原来的房间：已经蓄房的人状态为US_READY
						if (pITableUserItem->GetUserStatus() == US_SIT || pITableUserItem->GetUserStatus() == US_OFFLINE)
						{
							//Log
							TCHAR szString[128] = TEXT("");
							_sntprintf(szString, CountArray(szString), TEXT("IDI_HIDESEEK_PLAY_AGAIN_WAIT_END: PerformStandUpActionEx: userState=%d, userName=%s, userId=%d, chairId=%d"), pITableUserItem->GetUserStatus(), pITableUserItem->GetNickName(), pITableUserItem->GetUserInfo()->dwUserID, pITableUserItem->GetChairID());
							CTraceService::TraceString(szString, TraceLevel_Warning);

							PerformStandUpActionEx(pITableUserItem);
						}
					}
				}

				//Log
				TCHAR szString[128] = TEXT("");
				_sntprintf(szString, CountArray(szString), TEXT("IDI_HIDESEEK_PLAY_AGAIN_WAIT_END: StartWaitGame,TableId=%d"), m_wTableID);
				CTraceService::TraceString(szString, TraceLevel_Warning);

				StartWaitGame();
			}
			else
			{
				//IDI_HIDESEEK_PLAY_AGAIN_WAIT_END：蓄房时间结束，房间内没有人或者都断线了，强制解散房间
				if (m_pITableFramePrivate != NULL)
				{
					//Log
					TCHAR szString[128] = TEXT("");
					_sntprintf(szString, CountArray(szString), TEXT("IDI_HIDESEEK_PLAY_AGAIN_WAIT_END: 蓄房时间结束，房间内没有人或者都断线了，强制解散房间,TableId=%d"), m_wTableID);
					CTraceService::TraceString(szString, TraceLevel_Warning);

					m_pITableFramePrivate->ForceDismissAndClearRoom(m_wTableID);
				}
			}

			return true;
		}

	case IDI_OFF_LINE:	//断线事件
		{
			//效验状态
			ASSERT(m_bGameStarted==true);
			if (m_bGameStarted==false) return false;

			//变量定义
			DWORD dwOffLineTime=0L;
			WORD wOffLineChair=INVALID_CHAIR;

			//寻找用户
			for (WORD i=0;i<m_wChairCount;i++)
			{
				if ((m_dwOffLineTime[i]!=0L)&&((m_dwOffLineTime[i]<dwOffLineTime)||(wOffLineChair==INVALID_CHAIR)))
				{
					wOffLineChair=i;
					dwOffLineTime=m_dwOffLineTime[i];
				}
			}

			//位置判断
			ASSERT(wOffLineChair!=INVALID_CHAIR);
			if (wOffLineChair==INVALID_CHAIR) return false;

			//用户判断
			ASSERT(dwBindParameter<m_wChairCount);
			if (wOffLineChair!=(WORD)dwBindParameter)
			{
				//时间计算
				DWORD dwCurrentTime=(DWORD)time(NULL);
				DWORD dwLapseTime=dwCurrentTime-m_dwOffLineTime[wOffLineChair];

				//设置时间
				dwLapseTime=__min(dwLapseTime,TIME_OFF_LINE-2000L);
				SetGameTimer(IDI_OFF_LINE,TIME_OFF_LINE-dwLapseTime,1,wOffLineChair);

				return true;
			}

			//获取用户
			ASSERT(GetTableUserItem(wOffLineChair)!=NULL);
			IServerUserItem * pIServerUserItem=GetTableUserItem(wOffLineChair);

			//结束游戏
			if (pIServerUserItem!=NULL)
			{
				//设置变量
				m_dwOffLineTime[wOffLineChair]=0L;

				//用户起立
				PerformStandUpAction(pIServerUserItem);
			}

			//继续时间
			if (m_bGameStarted==true)
			{
				//变量定义
				DWORD dwOffLineTime=0L;
				WORD wOffLineChair=INVALID_CHAIR;

				//寻找用户
				for (WORD i=0;i<m_wChairCount;i++)
				{
					if ((m_dwOffLineTime[i]!=0L)&&((m_dwOffLineTime[i]<dwOffLineTime)||(wOffLineChair==INVALID_CHAIR)))
					{
						wOffLineChair=i;
						dwOffLineTime=m_dwOffLineTime[i];
					}
				}

				//设置时间
				if (wOffLineChair!=INVALID_CHAIR)
				{
					//时间计算
					DWORD dwCurrentTime=(DWORD)time(NULL);
					DWORD dwLapseTime=dwCurrentTime-m_dwOffLineTime[wOffLineChair];

					//设置时间
					dwLapseTime=__min(dwLapseTime,TIME_OFF_LINE-2000L);
					SetGameTimer(IDI_OFF_LINE,TIME_OFF_LINE-dwLapseTime,1,wOffLineChair);
				}
			}

			return true;
		}
	}

	//错误断言
	ASSERT(FALSE);

	return false;
}

//游戏事件
bool CTableFrame::OnEventSocketGame(WORD wSubCmdID, VOID * pData, WORD wDataSize, IServerUserItem * pIServerUserItem)
{
	//效验参数
	ASSERT(pIServerUserItem!=NULL);
	ASSERT(m_pITableFrameSink!=NULL);
	//消息处理
	return m_pITableFrameSink->OnGameMessage(wSubCmdID,pData,wDataSize,pIServerUserItem/*, m_pIMainServiceFrame*/);
}

//框架事件
bool CTableFrame::OnEventSocketFrame(WORD wSubCmdID, VOID * pData, WORD wDataSize, IServerUserItem * pIServerUserItem)
{
	TCHAR szString[512] = TEXT("");
	//_sntprintf(szString, CountArray(szString), TEXT("框架事件"));
	////提示消息
	//CTraceService::TraceString(szString, TraceLevel_Normal);

	//游戏处理
	if (m_pITableFrameSink->OnFrameMessage(wSubCmdID,pData,wDataSize,pIServerUserItem)==true) return true;

	////mChen log
	//TCHAR szString[512] = TEXT("");
	//_sntprintf(szString, CountArray(szString), TEXT("CTableFrame::OnEventSocketFrame:wSubCmdID=%d m_cbGameStatus=%d", wSubCmdID, m_cbGameStatus));
	////提示消息
	//CTraceService::TraceString(szString, TraceLevel_Normal);


	//默认处理
	switch (wSubCmdID)
	{
	case SUB_GF_GAME_OPTION:	//游戏配置
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GF_GameOption));
			if (wDataSize!=sizeof(CMD_GF_GameOption)) return false;

			//变量定义
			CMD_GF_GameOption * pGameOption=(CMD_GF_GameOption *)pData;

			//获取属性
			WORD wChairID=pIServerUserItem->GetChairID();
			BYTE cbUserStatus=pIServerUserItem->GetUserStatus();

			//断线清理
			if ((cbUserStatus!=US_LOOKON)&&((m_dwOffLineTime[wChairID]!=0L)))
			{
				//设置变量
				m_dwOffLineTime[wChairID]=0L;

				//删除时间
				WORD wOffLineCount=GetOffLineUserCount();
				if (wOffLineCount==0) KillGameTimer(IDI_OFF_LINE);
			}

			//设置状态
			pIServerUserItem->SetClientReady(true);
			if (cbUserStatus!=US_LOOKON) m_bAllowLookon[wChairID]=pGameOption->cbAllowLookon?true:false;

			// mChen note：将OnEventSendGameScene移到SendUsersInfoPacket之前，保证客户端先调用LoadScene("mainScene")再调用OnSocketSubUserEnter 
			//发送场景
			bool bSendSecret = ((cbUserStatus != US_LOOKON) || (m_bAllowLookon[wChairID] == true));
			m_pITableFrameSink->OnEventSendGameScene(wChairID, pIServerUserItem, m_cbGameStatus, bSendSecret);


			//mChen note：将SendUserInfoPacket移到send SUB_GF_GAME_STATUS之前，
			//				保证客户端先调用GameScene::CreatePlayer->team.AddAPlayer，再调用OnSocketSubGameStatus->case SocketDefines.GAME_STATUS_PLAY:
			//				fix断线重连后因为localHuman为null导致的无法设置镜头的问题
			SendUsersInfoPacket(pIServerUserItem);
			//for (WORD i = 0; i < m_wChairCount; i++)
			//{
			//	//获取用户
			//	if (m_TableUserItemArray[i] == NULL) continue;
			//	if (pIServerUserItem == NULL) continue;
			//	IServerUserItem * pSendIServerUserItem = m_TableUserItemArray[i];
			//	m_pIMainServiceFrame->SendUserInfoPacket(pSendIServerUserItem, pIServerUserItem);
			//	m_pIMainServiceFrame->SendUserInfoPacket(pIServerUserItem, pSendIServerUserItem);
			//}
			
			//发送状态
			CMD_GF_GameStatus GameStatus;
			GameStatus.cbGameStatus = m_cbGameStatus;
			GameStatus.cbAllowLookon = m_bAllowLookon[wChairID] ? TRUE : FALSE;
			m_pIMainServiceFrame->SendData(pIServerUserItem, MDM_GF_FRAME, SUB_GF_GAME_STATUS, &GameStatus, sizeof(GameStatus));


			////发送消息
			//TCHAR szMessage[128]=TEXT("");
			//_sntprintf(szMessage,CountArray(szMessage),TEXT("欢迎您进入“%s”游戏，祝您游戏愉快！"),m_pGameServiceAttrib->szGameName);
			//m_pIMainServiceFrame->SendGameMessage(pIServerUserItem,szMessage,SMT_CHAT);

			if(m_pITableFramePrivate!=NULL) m_pITableFramePrivate->OnEventClientReady(wChairID,pIServerUserItem);


			//开始判断
			if (EfficacyStartGame(wChairID)==true)
			{
				////mChen
				//TCHAR szString[512] = TEXT("");
				//_sntprintf(szString, CountArray(szString), TEXT("CTableFrame::OnEventSocketFrame:wSubCmdID=SUB_GF_GAME_OPTION, StartGame"));
				////提示消息
				//CTraceService::TraceString(szString, TraceLevel_Normal);

				StartGame(); 
			}
			else
			{
				////mChen
				//TCHAR szString[512] = TEXT("");
				//_sntprintf(szString, CountArray(szString), TEXT("CTableFrame::OnEventSocketFrame:wSubCmdID=SUB_GF_GAME_OPTION"));
				////提示消息
				//CTraceService::TraceString(szString, TraceLevel_Normal);
			}

			return true;
		}
	case SUB_GF_USER_READY:		//用户准备
		{
			//log
			CTraceService::TraceString(TEXT("用户准备了"), TraceLevel_Normal);

			//获取属性
			WORD wChairID=pIServerUserItem->GetChairID();
			BYTE cbUserStatus=pIServerUserItem->GetUserStatus();

			//效验状态
			ASSERT(GetTableUserItem(wChairID)==pIServerUserItem);
			if (GetTableUserItem(wChairID)!=pIServerUserItem) return false;

			//log
			TCHAR szString[512] = TEXT("");
			_sntprintf(szString, CountArray(szString), TEXT("效验状态1:%d"), cbUserStatus);
			CTraceService::TraceString(szString, TraceLevel_Normal);

			//效验状态
			//ASSERT(cbUserStatus==US_SIT);
			if (cbUserStatus!=US_SIT) return true;

			//分组判断
// 			if ((m_pGameServiceOption->cbDistributeRule&DISTRIBUTE_ALLOW)!=0)
// 			{
// 				m_pIMainServiceFrame->InsertDistribute(pIServerUserItem);
// 				return true;
// 			}

			//事件通知
			if (m_pITableUserAction!=NULL)
			{
				m_pITableUserAction->OnActionUserOnReady(wChairID,pIServerUserItem,pData,wDataSize);
			}

			//事件通知
			if(m_pIMatchTableAction!=NULL && !m_pIMatchTableAction->OnActionUserOnReady(wChairID,pIServerUserItem, pData,wDataSize))
			{
				//log
				CTraceService::TraceString(TEXT("tz2"), TraceLevel_Normal);

				return true;
			}

			//事件通知
			if(m_pIPrivateTableAction!=NULL && !m_pIPrivateTableAction->OnActionUserOnReady(wChairID,pIServerUserItem, pData,wDataSize))
			{
				//log
				CTraceService::TraceString(TEXT("tz3"), TraceLevel_Normal);

				return true;
			}
			
			//开始判断
			if (EfficacyStartGame(wChairID)==false)
			{
				pIServerUserItem->SetUserStatus(US_READY,m_wTableID,wChairID);

				//log
				CTraceService::TraceString(TEXT("EfficacyStartGame==false, SetUserStatus US_READY"), TraceLevel_Normal);

				///SendTableData(INVALID_CHAIR, SUB_GF_USER_READY, &wChairID, sizeof(wChairID), MDM_GF_FRAME);
			}
			else
			{
				StartGame(); 
			}

			return true;
		}

		//mChen add, for HideSeek
	case SUB_GF_CREATER_PRESS_START:
	{
		if (m_cbGameStatus == GAME_STATUS_FREE)
		{
			StartWaitGame();
		}

		return true;
	}

		//mChen add:用户聊天
		//////////////////////////////////////////////////////////////////////////
	case SUB_GF_USER_CHAT_INDEX:
	{
		//变量定义
		//CMD_GF_C_UserChatIdx * pUserChat = (CMD_GF_C_UserChatIdx *)pData;

		//状态判断
		if ((CServerRule::IsForfendGameChat(m_pGameServiceOption->dwServerRule) == true) && (pIServerUserItem->GetMasterOrder() == 0L))
		{
			SendGameMessage(pIServerUserItem, TEXT("抱歉，当前游戏房间禁止游戏聊天！"), SMT_CHAT);
			return true;
		}

		//权限判断
		if (CUserRight::CanRoomChat(pIServerUserItem->GetUserRight()) == false)
		{
			SendGameMessage(pIServerUserItem, TEXT("抱歉，您没有游戏聊天的权限，若需要帮助，请联系游戏客服咨询！"), SMT_EJECT | SMT_CHAT);
			return true;
		}

		SendTableData(INVALID_CHAIR, SUB_GF_USER_CHAT_INDEX, pData, wDataSize, MDM_GF_FRAME);
		return true;
	}
	case SUB_GF_USER_EXPRESSION_INDEX:
	{
		//变量定义
		//CMD_GF_C_UserExpressionIdx * pUserChat = (CMD_GF_C_UserExpressionIdx *)pData;

		//状态判断
		if ((CServerRule::IsForfendGameChat(m_pGameServiceOption->dwServerRule) == true) && (pIServerUserItem->GetMasterOrder() == 0L))
		{
			SendGameMessage(pIServerUserItem, TEXT("抱歉，当前游戏房间禁止游戏聊天！"), SMT_CHAT);
			return true;
		}

		//权限判断
		if (CUserRight::CanRoomChat(pIServerUserItem->GetUserRight()) == false)
		{
			SendGameMessage(pIServerUserItem, TEXT("抱歉，您没有游戏聊天的权限，若需要帮助，请联系游戏客服咨询！"), SMT_EJECT | SMT_CHAT);
			return true;
		}

		SendTableData(INVALID_CHAIR, SUB_GF_USER_EXPRESSION_INDEX, pData, wDataSize, MDM_GF_FRAME);
		return true;
	}
	//////////////////////////////////////////////////////////////////////////

	case SUB_GF_USER_CHAT:		//用户聊天
		{
			//变量定义
			CMD_GF_C_UserChat * pUserChat=(CMD_GF_C_UserChat *)pData;

			//效验参数
			ASSERT(wDataSize<=sizeof(CMD_GF_C_UserChat));
			ASSERT(wDataSize>=(sizeof(CMD_GF_C_UserChat)-sizeof(pUserChat->szChatString)));
			ASSERT(wDataSize==(sizeof(CMD_GF_C_UserChat)-sizeof(pUserChat->szChatString)+pUserChat->wChatLength*sizeof(pUserChat->szChatString[0])));

			//效验参数
			if (wDataSize>sizeof(CMD_GF_C_UserChat)) return false;
			if (wDataSize<(sizeof(CMD_GF_C_UserChat)-sizeof(pUserChat->szChatString))) return false;
			if (wDataSize!=(sizeof(CMD_GF_C_UserChat)-sizeof(pUserChat->szChatString)+pUserChat->wChatLength*sizeof(pUserChat->szChatString[0]))) return false;

			//目标用户
			if ((pUserChat->dwTargetUserID!=0)&&(SearchUserItem(pUserChat->dwTargetUserID)==NULL))
			{
				ASSERT(FALSE);
				return true;
			}

			//状态判断
			if ((CServerRule::IsForfendGameChat(m_pGameServiceOption->dwServerRule)==true)&&(pIServerUserItem->GetMasterOrder()==0L))
			{
				SendGameMessage(pIServerUserItem,TEXT("抱歉，当前游戏房间禁止游戏聊天！"),SMT_CHAT);
				return true;
			}

			//权限判断
			if (CUserRight::CanRoomChat(pIServerUserItem->GetUserRight())==false)
			{
				SendGameMessage(pIServerUserItem,TEXT("抱歉，您没有游戏聊天的权限，若需要帮助，请联系游戏客服咨询！"),SMT_EJECT|SMT_CHAT);
				return true;
			}

			//构造消息
			CMD_GF_S_UserChat UserChat;
			ZeroMemory(&UserChat,sizeof(UserChat));

			//字符过滤
			m_pIMainServiceFrame->SensitiveWordFilter(pUserChat->szChatString,UserChat.szChatString,CountArray(UserChat.szChatString));

			//构造数据
			UserChat.dwChatColor=pUserChat->dwChatColor;
			UserChat.wChatLength=pUserChat->wChatLength;
			UserChat.dwTargetUserID=pUserChat->dwTargetUserID;
			UserChat.dwSendUserID=pIServerUserItem->GetUserID();
			UserChat.wChatLength=CountStringBuffer(UserChat.szChatString);

			//发送数据
			WORD wHeadSize=sizeof(UserChat)-sizeof(UserChat.szChatString);
			DWORD wSendSize=wHeadSize+UserChat.wChatLength*sizeof(UserChat.szChatString[0]);

			//游戏用户
			for (WORD i=0;i<m_wChairCount;i++)
			{
				//获取用户
				IServerUserItem * pIServerUserItem=GetTableUserItem(i);
				if ((pIServerUserItem==NULL)||(pIServerUserItem->IsClientReady()==false)) continue;

				m_pIMainServiceFrame->SendData(pIServerUserItem,MDM_GF_FRAME,SUB_GF_USER_CHAT,&UserChat,wSendSize);
			}

			//旁观用户
			WORD wEnumIndex=0;
			IServerUserItem * pIServerUserItem=NULL;

			//枚举用户
			do
			{
				//获取用户
				pIServerUserItem=EnumLookonUserItem(wEnumIndex++);
				if (pIServerUserItem==NULL) break;

				//发送数据
				if (pIServerUserItem->IsClientReady()==true)
				{
					m_pIMainServiceFrame->SendData(pIServerUserItem,MDM_GF_FRAME,SUB_GF_USER_CHAT,&UserChat,wSendSize);
				}
			} while (true);

			return true;
		}
	case SUB_GF_USER_EXPRESSION:	//用户表情
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GF_C_UserExpression));
			if (wDataSize!=sizeof(CMD_GF_C_UserExpression)) return false;

			//变量定义
			CMD_GF_C_UserExpression * pUserExpression=(CMD_GF_C_UserExpression *)pData;

			//目标用户
			if ((pUserExpression->dwTargetUserID!=0)&&(SearchUserItem(pUserExpression->dwTargetUserID)==NULL))
			{
				ASSERT(FALSE);
				return true;
			}

			//状态判断
			if ((CServerRule::IsForfendGameChat(m_pGameServiceOption->dwServerRule)==true)&&(pIServerUserItem->GetMasterOrder()==0L))
			{
				SendGameMessage(pIServerUserItem,TEXT("抱歉，当前游戏房间禁止游戏聊天！"),SMT_CHAT);
				return true;
			}

			//权限判断
			if (CUserRight::CanRoomChat(pIServerUserItem->GetUserRight())==false)
			{
				SendGameMessage(pIServerUserItem,TEXT("抱歉，您没有游戏聊天的权限，若需要帮助，请联系游戏客服咨询！"),SMT_EJECT|SMT_CHAT);
				return true;
			}

			//构造消息
			CMD_GR_S_UserExpression UserExpression;
			ZeroMemory(&UserExpression,sizeof(UserExpression));

			//构造数据
			UserExpression.wItemIndex=pUserExpression->wItemIndex;
			UserExpression.dwSendUserID=pIServerUserItem->GetUserID();
			UserExpression.dwTargetUserID=pUserExpression->dwTargetUserID;

			//游戏用户
			for (WORD i=0;i<m_wChairCount;i++)
			{
				//获取用户
				IServerUserItem * pIServerUserItem=GetTableUserItem(i);
				if ((pIServerUserItem==NULL)||(pIServerUserItem->IsClientReady()==false)) continue;

				//发送数据
				m_pIMainServiceFrame->SendData(pIServerUserItem,MDM_GF_FRAME,SUB_GF_USER_EXPRESSION,&UserExpression,sizeof(UserExpression));
			}

			//旁观用户
			WORD wEnumIndex=0;
			IServerUserItem * pIServerUserItem=NULL;

			//枚举用户
			do
			{
				//获取用户
				pIServerUserItem=EnumLookonUserItem(wEnumIndex++);
				if (pIServerUserItem==NULL) break;

				//发送数据
				if (pIServerUserItem->IsClientReady()==true)
				{
					m_pIMainServiceFrame->SendData(pIServerUserItem,MDM_GF_FRAME,SUB_GF_USER_EXPRESSION,&UserExpression,sizeof(UserExpression));
				}
			} while (true);

			return true;
		}
	case SUB_GR_TABLE_TALK:		//游戏聊天
		{
			SendTableData(INVALID_CHAIR,SUB_GR_TABLE_TALK,pData,wDataSize,MDM_GF_FRAME);
			return true;
		}
	case SUB_GF_LOOKON_CONFIG:		//旁观配置
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GF_LookonConfig));
			if (wDataSize<sizeof(CMD_GF_LookonConfig)) return false;

			//变量定义
			CMD_GF_LookonConfig * pLookonConfig=(CMD_GF_LookonConfig *)pData;

			//目标用户
			if ((pLookonConfig->dwUserID!=0)&&(SearchUserItem(pLookonConfig->dwUserID)==NULL))
			{
				ASSERT(FALSE);
				return true;
			}

			//用户效验
			ASSERT(pIServerUserItem->GetUserStatus()!=US_LOOKON);
			if (pIServerUserItem->GetUserStatus()==US_LOOKON) return false;

			//旁观处理
			if (pLookonConfig->dwUserID!=0L)
			{
				for (INT_PTR i=0;i<m_LookonUserItemArray.GetCount();i++)
				{
					//获取用户
					IServerUserItem * pILookonUserItem=m_LookonUserItemArray[i];
					if (pILookonUserItem->GetUserID()!=pLookonConfig->dwUserID) continue;
					if (pILookonUserItem->GetChairID()!=pIServerUserItem->GetChairID()) continue;

					//构造消息
					CMD_GF_LookonStatus LookonStatus;
					LookonStatus.cbAllowLookon=pLookonConfig->cbAllowLookon;

					//发送消息
					ASSERT(m_pIMainServiceFrame!=NULL);
					m_pIMainServiceFrame->SendData(pILookonUserItem,MDM_GF_FRAME,SUB_GF_LOOKON_STATUS,&LookonStatus,sizeof(LookonStatus));

					break;
				}
			}
			else
			{
				//设置判断
				bool bAllowLookon=(pLookonConfig->cbAllowLookon==TRUE)?true:false;
				if (bAllowLookon==m_bAllowLookon[pIServerUserItem->GetChairID()]) return true;

				//设置变量
				m_bAllowLookon[pIServerUserItem->GetChairID()]=bAllowLookon;

				//构造消息
				CMD_GF_LookonStatus LookonStatus;
				LookonStatus.cbAllowLookon=pLookonConfig->cbAllowLookon;

				//发送消息
				for (INT_PTR i=0;i<m_LookonUserItemArray.GetCount();i++)
				{
					//获取用户
					IServerUserItem * pILookonUserItem=m_LookonUserItemArray[i];
					if (pILookonUserItem->GetChairID()!=pIServerUserItem->GetChairID()) continue;

					//发送消息
					ASSERT(m_pIMainServiceFrame!=NULL);
					m_pIMainServiceFrame->SendData(pILookonUserItem,MDM_GF_FRAME,SUB_GF_LOOKON_STATUS,&LookonStatus,sizeof(LookonStatus));
				}
			}

			return true;
		}
	}

	return false;
}

//获取空位
WORD CTableFrame::GetNullChairID()
{
	//椅子搜索
	for (WORD i=0;i<m_wChairCount;i++)
	{
		if (m_TableUserItemArray[i]==NULL)
		{
			return i;
		}
	}

	return INVALID_CHAIR;
}

//随机空位
WORD CTableFrame::GetRandNullChairID()
{
	//椅子搜索
	WORD wIndex = rand()%m_wChairCount;
	for (WORD i=wIndex;i<m_wChairCount+wIndex;i++)
	{
		if (m_TableUserItemArray[i%m_wChairCount]==NULL)
		{
			return i%m_wChairCount;
		}
	}

	return INVALID_CHAIR;
}

//用户数目
WORD CTableFrame::GetSitUserCount()
{
	//变量定义
	WORD wUserCount=0;

	//数目统计
	for (WORD i=0;i<m_wChairCount;i++)
	{
		if (GetTableUserItem(i)!=NULL)
		{
			wUserCount++;
		}
	}

	return wUserCount;
}

//mChen add, for HideSeek
//玩家血量
BYTE CTableFrame::GetTheHumanHP(WORD wChairID)
{
	BYTE cbHP = 1;

	if (m_pITableFrameSink)
	{
		cbHP = m_pITableFrameSink->GetHumanHP(wChairID);
	}

	return cbHP;
}
void CTableFrame::ResurrectionEvent(WORD wChairID)
{
	if (m_pITableFrameSink)
		m_pITableFrameSink->SetResurrection(wChairID);
}
void CTableFrame::StealthEvent(DWORD dwTime, WORD wChairID)
{
	if (m_pITableFrameSink)
		m_pITableFrameSink->SetStealth(dwTime, wChairID);
}
//用户数目
WORD CTableFrame::GetSitUserCountOfTeam(PlayerTeamType teamType)
{
	//变量定义
	WORD wUserCount = 0;

	//数目统计
	for (WORD i = 0; i < m_wChairCount; i++)
	{
		IServerUserItem * pIServerUserItem = GetTableUserItem(i);
		if (pIServerUserItem != NULL)
		{
			tagUserInfo * pUserInfo = pIServerUserItem->GetUserInfo();
			if (pUserInfo->cbTeamType == teamType)
			{
				wUserCount++;
			}
		}
	}

	return wUserCount;
}
InventoryItem* CTableFrame::GetInventoryList()
{
	InventoryItem* list = NULL;

	if (m_pITableFrameSink)
	{
		list = m_pITableFrameSink->GetInventoryList();
	}

	return list;
}
/*
//道具同步
LONGLONG CTableFrame::GetEncodedInventoryList()
{
	LONGLONG lEncodedInventoryList = 0;
	LONGLONG lInventoryType;
	int nMoveStep;
	for (int i = 0; i < MAX_INVENTORY_NUM; i++)
	{
		//if(!m_sInventoryList[i].bUsed)

		lInventoryType = (LONGLONG)(m_sInventoryList[i].type & 0x0F);
		nMoveStep = 4 * i;
		lEncodedInventoryList |= (lInventoryType << nMoveStep);
	}

	InventoryType inventoryType = (InventoryType)(lEncodedInventoryList & 0xF);

	// Log
	TCHAR szString[128] = TEXT("");
	_sntprintf(szString, CountArray(szString), TEXT("CTableFrame::GetEncodedInventoryList: lEncodedInventoryList= %I64d"), lEncodedInventoryList);
	CTraceService::TraceString(szString, TraceLevel_Normal);

	return lEncodedInventoryList;

	//LONGLONG result = 0;
	//for (int i = MAX_INVENTORY_NUM-1; i >= 0; i--)
	//{
	//	//if(!m_sInventoryList[i].bUsed)

	//	if (i != MAX_INVENTORY_NUM - 1)
	//	{
	//		result = (result << 4);
	//	}

	//	result |= (m_sInventoryList[i].type & 0xF);
	//}
	//return result;
}
*/
WORD CTableFrame::GetPlayingUserCount()
{
	//变量定义
	WORD wPlayingUserCount = 0;

	//数目统计
	for (WORD i = 0; i < m_wChairCount; i++)
	{
		IServerUserItem * pITableUserItem = GetTableUserItem(i);
		if (pITableUserItem != NULL && 
			(pITableUserItem->GetUserStatus() == US_SIT || pITableUserItem->GetUserStatus() == US_READY || pITableUserItem->GetUserStatus() == US_PLAYING)
			)
		{
			wPlayingUserCount++;
		}
	}

	return wPlayingUserCount;
}

void CTableFrame::SetCellScore(LONG lCellScore) 
{
	m_lCellScore=lCellScore; 
	if (m_pITableFrameSink)
	{
		m_pITableFrameSink->SetGameBaseScore(lCellScore);
	}
}

//旁观数目
WORD CTableFrame::GetLookonUserCount()
{
	//获取数目
	INT_PTR nLookonCount=m_LookonUserItemArray.GetCount();

	return (WORD)(nLookonCount);
}

//断线数目
WORD CTableFrame::GetOffLineUserCount()
{
	//变量定义
	WORD wOffLineCount=0;

	//断线人数
	for (WORD i=0;i<m_wChairCount;i++)
	{
		if (m_dwOffLineTime[i]!=0L)
		{
			wOffLineCount++;
		}
	}

	return wOffLineCount;
}

//桌子状况
WORD CTableFrame::GetTableUserInfo(tagTableUserInfo & TableUserInfo)
{
	//设置变量
	ZeroMemory(&TableUserInfo,sizeof(TableUserInfo));

	//用户分析
	for (WORD i=0;i<m_pGameServiceAttrib->wChairCount;i++)
	{
		//获取用户
		IServerUserItem * pIServerUserItem=GetTableUserItem(i);
		if (pIServerUserItem==NULL) continue;

		//用户类型
		if (pIServerUserItem->IsAndroidUser()==false)
		{
			TableUserInfo.wTableUserCount++;
		}
		else
		{
			TableUserInfo.wTableAndroidCount++;
		}

		//准备判断
		if (pIServerUserItem->GetUserStatus()==US_READY)
		{
			TableUserInfo.wTableReadyCount++;
		}
	}

	//最少数目
	switch (m_cbStartMode)
	{
	case START_MODE_ALL_READY:		//所有准备
		{
			TableUserInfo.wMinUserCount=2;
			break;
		}
	case START_MODE_PAIR_READY:		//配对开始
		{
			TableUserInfo.wMinUserCount=2;
			break;
		}
	case START_MODE_TIME_CONTROL:	//时间控制
		{
			TableUserInfo.wMinUserCount=1;
			break;
		}
	default:						//默认模式
		{
			TableUserInfo.wMinUserCount=m_pGameServiceAttrib->wChairCount;
			break;
		}
	}

	return TableUserInfo.wTableAndroidCount+TableUserInfo.wTableUserCount;
}

//配置桌子
bool CTableFrame::InitializationFrame(WORD wTableID, tagTableFrameParameter & TableFrameParameter)
{
	//设置变量
	m_wTableID=wTableID;
	m_wChairCount=TableFrameParameter.pGameServiceAttrib->wChairCount;

	//配置参数
	m_pGameParameter=TableFrameParameter.pGameParameter;
	m_pGameServiceAttrib=TableFrameParameter.pGameServiceAttrib;
	m_pGameServiceOption=TableFrameParameter.pGameServiceOption;

	//组件接口
	m_pITimerEngine=TableFrameParameter.pITimerEngine;
	m_pIMainServiceFrame=TableFrameParameter.pIMainServiceFrame;
	m_pIAndroidUserManager=TableFrameParameter.pIAndroidUserManager;
	m_pIKernelDataBaseEngine=TableFrameParameter.pIKernelDataBaseEngine;
	m_pIRecordDataBaseEngine=TableFrameParameter.pIRecordDataBaseEngine;

	//创建桌子
	IGameServiceManager * pIGameServiceManager=TableFrameParameter.pIGameServiceManager;
	m_pITableFrameSink=(ITableFrameSink *)pIGameServiceManager->CreateTableFrameSink(IID_ITableFrameSink,VER_ITableFrameSink);

	//错误判断
	if (m_pITableFrameSink==NULL)
	{
		ASSERT(FALSE);
		return false;
	}

	//设置桌子
	IUnknownEx * pITableFrame=QUERY_ME_INTERFACE(IUnknownEx);
	if (m_pITableFrameSink->Initialization(pITableFrame)==false) return false;

	//设置变量
	m_lCellScore=m_pGameServiceOption->lCellScore;

	//扩展接口
	m_pITableUserAction=QUERY_OBJECT_PTR_INTERFACE(m_pITableFrameSink,ITableUserAction);
	m_pITableUserRequest=QUERY_OBJECT_PTR_INTERFACE(m_pITableFrameSink,ITableUserRequest);

	return true;
}

//起立动作
bool CTableFrame::PerformStandUpAction(IServerUserItem * pIServerUserItem)
{
	WORD wChairID=pIServerUserItem->GetChairID();
	//私人场类型
	if(m_pGameServiceOption->wServerType==GAME_GENRE_EDUCATE)
	{
		//掉线通知
		if(m_pIPrivateTableAction!=NULL && m_pIPrivateTableAction->OnActionUserOffLine(wChairID,pIServerUserItem, __FUNCTION__))
		{
			return true;
		}
	}
	return PerformStandUpActionEx(pIServerUserItem);
}
//起立动作
bool CTableFrame::PerformStandUpActionEx(IServerUserItem * pIServerUserItem)
{
	//效验参数
	ASSERT(pIServerUserItem!=NULL);
	ASSERT(pIServerUserItem->GetTableID()==m_wTableID);
	ASSERT(pIServerUserItem->GetChairID()<=m_wChairCount);

	//用户属性
	WORD wChairID=pIServerUserItem->GetChairID();
	BYTE cbUserStatus=pIServerUserItem->GetUserStatus();
	IServerUserItem * pITableUserItem=GetTableUserItem(wChairID);

	//游戏用户
	if ((m_bGameStarted==true)&&((cbUserStatus==US_PLAYING)||(cbUserStatus==US_OFFLINE)))
	{
		//比赛类型
		if(m_pGameServiceOption->wServerType==GAME_GENRE_MATCH)
		{
			//掉线通知
			if(m_pITableUserAction!=NULL) m_pITableUserAction->OnActionUserOffLine(wChairID,pIServerUserItem, __FUNCTION__);

			return true;
		}

		//结束游戏
		BYTE cbConcludeReason=(cbUserStatus==US_OFFLINE)?GER_NETWORK_ERROR:GER_USER_LEAVE;
		m_pITableFrameSink->OnEventGameConclude(wChairID,pIServerUserItem,cbConcludeReason);

		//离开判断
		if (m_TableUserItemArray[wChairID]!=pIServerUserItem) return true;
	}
	//设置变量
	if (pIServerUserItem==pITableUserItem)
	{
		//wChairID座位上的人是pIServerUserItem自己，说明pIServerUserItem不是旁观者

		//解锁游戏币
		if (m_lFrozenedScore[wChairID]!=0L)
		{
			pIServerUserItem->UnFrozenedUserScore(m_lFrozenedScore[wChairID]);
			m_lFrozenedScore[wChairID]=0L;
		}

		//事件通知
		if (m_pITableUserAction!=NULL)
		{
			m_pITableUserAction->OnActionUserStandUp(wChairID,pIServerUserItem,false);
		}

		//事件通知
		if(m_pIMatchTableAction!=NULL) m_pIMatchTableAction->OnActionUserStandUp(wChairID,pIServerUserItem,false);
		//事件通知
		if(m_pIPrivateTableAction!=NULL) m_pIPrivateTableAction->OnActionUserStandUp(wChairID,pIServerUserItem,false);

		//设置变量
		m_TableUserItemArray[wChairID]=NULL;

		//用户状态
		pIServerUserItem->SetClientReady(false);
		pIServerUserItem->SetUserStatus((cbUserStatus==US_OFFLINE)?US_NULL:US_FREE,INVALID_TABLE,INVALID_CHAIR);

		//变量定义
		bool bTableLocked=IsTableLocked();
		bool bTableStarted=IsTableStarted();
		WORD wTableUserCount=GetSitUserCount();

		//设置变量
		m_wUserCount=wTableUserCount;

		//桌子信息
		if (wTableUserCount==0)
		{
			m_dwTableOwnerID=0L;
			m_szEnterPassword[0]=0;
		}

		//踢走旁观
		if (wTableUserCount==0)
		{
			for (INT_PTR i=0;i<m_LookonUserItemArray.GetCount();i++)
			{
				SendGameMessage(m_LookonUserItemArray[i],TEXT("此游戏桌的所有玩家已经离开了！"),SMT_CLOSE_GAME|SMT_EJECT);

				//mChen add, for HideSeek， fix客户端会2次收到SUB_S_HideSeek_AICreateInfo消息，导致创建2倍数量的AI
				IServerUserItem * pILookonUserItem = m_LookonUserItemArray[i];
				if (pILookonUserItem != NULL)
				{
					//删除子项
					m_LookonUserItemArray.RemoveAt(i);

					//事件通知
					if (m_pITableUserAction != NULL)
					{
						m_pITableUserAction->OnActionUserStandUp(wChairID, pILookonUserItem, true);
					}

					//事件通知
					if (m_pIMatchTableAction != NULL) m_pIMatchTableAction->OnActionUserStandUp(wChairID, pILookonUserItem, true);

					//事件通知
					if (m_pIPrivateTableAction != NULL) m_pIPrivateTableAction->OnActionUserStandUp(wChairID, pILookonUserItem, true);

					//用户状态
					pILookonUserItem->SetClientReady(false);
					pILookonUserItem->SetUserStatus(US_FREE, INVALID_TABLE, INVALID_CHAIR);

					///return true;
				}
			}
		}

		//结束桌子
		ConcludeTable();

		//开始判断
		if (EfficacyStartGame(INVALID_CHAIR)==true)
		{
			StartGame();
		}

		//发送状态
		if ((bTableLocked!=IsTableLocked())||(bTableStarted!=IsTableStarted()))
		{
			SendTableStatus();
		}

		return true;
	}
	else
	{
		//wChairID座位上的人不是pIServerUserItem，说明pIServerUserItem是旁观者

		//起立处理
		for (INT_PTR i=0;i<m_LookonUserItemArray.GetCount();i++)
		{
			if (pIServerUserItem==m_LookonUserItemArray[i])
			{
				//删除子项
				m_LookonUserItemArray.RemoveAt(i);

				//事件通知
				if (m_pITableUserAction!=NULL)
				{
					m_pITableUserAction->OnActionUserStandUp(wChairID,pIServerUserItem,true);
				}

				//事件通知
				if(m_pIMatchTableAction!=NULL) m_pIMatchTableAction->OnActionUserStandUp(wChairID,pIServerUserItem,true);

				//事件通知
				if(m_pIPrivateTableAction!=NULL) m_pIPrivateTableAction->OnActionUserStandUp(wChairID,pIServerUserItem,true);

				//用户状态
				pIServerUserItem->SetClientReady(false);
				pIServerUserItem->SetUserStatus(US_FREE,INVALID_TABLE,INVALID_CHAIR);

				return true;
			}
		}

		//错误断言
		ASSERT(FALSE);
	}

	return true;
}

//旁观动作
bool CTableFrame::PerformLookonAction(WORD wChairID, IServerUserItem * pIServerUserItem)
{
	//效验参数
	ASSERT((pIServerUserItem!=NULL)&&(wChairID<m_wChairCount));
	ASSERT((pIServerUserItem->GetTableID()==INVALID_TABLE)&&(pIServerUserItem->GetChairID()==INVALID_CHAIR));

	//变量定义
	tagUserInfo * pUserInfo=pIServerUserItem->GetUserInfo();
	tagUserRule * pUserRule=pIServerUserItem->GetUserRule();
	IServerUserItem * pITableUserItem=GetTableUserItem(wChairID);

	//游戏状态
	if ((m_bGameStarted==false)&&(pIServerUserItem->GetMasterOrder()==0L))
	{
		SendRequestFailure(pIServerUserItem,TEXT("游戏还没有开始，不能旁观此游戏桌！"),REQUEST_FAILURE_NORMAL);
		return false;
	}

	//模拟处理
	if (m_pGameServiceAttrib->wChairCount < MAX_CHAIR && pIServerUserItem->IsAndroidUser()==false)
	{
		//定义变量
		CAttemperEngineSink * pAttemperEngineSink=(CAttemperEngineSink *)m_pIMainServiceFrame;

		//查找机器
		for (WORD i=0; i<m_pGameServiceAttrib->wChairCount; i++)
		{
			//获取用户
			IServerUserItem *pIUserItem=m_TableUserItemArray[i];
			if(pIUserItem==NULL) continue;
			if(pIUserItem->IsAndroidUser()==false)break;

			//获取参数
			tagBindParameter * pBindParameter=pAttemperEngineSink->GetBindParameter(pIUserItem->GetBindIndex());
			IAndroidUserItem * pIAndroidUserItem=m_pIAndroidUserManager->SearchAndroidUserItem(pIUserItem->GetUserID(),pBindParameter->dwSocketID);
			tagAndroidParameter * pAndroidParameter=pIAndroidUserItem->GetAndroidParameter();

			//模拟判断
			if((pAndroidParameter->dwServiceGender&ANDROID_SIMULATE)!=0
				&& (pAndroidParameter->dwServiceGender&ANDROID_PASSIVITY)==0
				&& (pAndroidParameter->dwServiceGender&ANDROID_INITIATIVE)==0)
			{
				SendRequestFailure(pIServerUserItem,TEXT("抱歉，当前游戏桌子禁止用户旁观！"),REQUEST_FAILURE_NORMAL);
				return false;
			}

			break;
		}
	}


	//旁观判断
	if (CServerRule::IsAllowAndroidSimulate(m_pGameServiceOption->dwServerRule)==true
		&& (CServerRule::IsAllowAndroidAttend(m_pGameServiceOption->dwServerRule)==false))
	{
		if ((pITableUserItem!=NULL)&&(pITableUserItem->IsAndroidUser()==true))
		{
			SendRequestFailure(pIServerUserItem,TEXT("抱歉，当前游戏房间禁止用户旁观！"),REQUEST_FAILURE_NORMAL);
			return false;
		}
	}

	//状态判断
	if ((CServerRule::IsForfendGameLookon(m_pGameServiceOption->dwServerRule)==true)&&(pIServerUserItem->GetMasterOrder()==0))
	{
		SendRequestFailure(pIServerUserItem,TEXT("抱歉，当前游戏房间禁止用户旁观！"),REQUEST_FAILURE_NORMAL);
		return false;
	}

	//椅子判断
	if ((pITableUserItem==NULL)&&(pIServerUserItem->GetMasterOrder()==0L))
	{
		SendRequestFailure(pIServerUserItem,TEXT("您所请求的位置没有游戏玩家，无法旁观此游戏桌"),REQUEST_FAILURE_NORMAL);
		return false;
	}

	//密码效验
	if ((IsTableLocked()==true)&&(pIServerUserItem->GetMasterOrder()==0L)&&(lstrcmp(pUserRule->szPassword,m_szEnterPassword)!=0))
	{
		SendRequestFailure(pIServerUserItem,TEXT("游戏桌进入密码不正确，不能旁观游戏！"),REQUEST_FAILURE_PASSWORD);
		return false;
	}

	//扩展效验
	if (m_pITableUserRequest!=NULL)
	{
		//变量定义
		tagRequestResult RequestResult;
		ZeroMemory(&RequestResult,sizeof(RequestResult));

		//坐下效验
		if (m_pITableUserRequest->OnUserRequestLookon(wChairID,pIServerUserItem,RequestResult)==false)
		{
			//发送信息
			SendRequestFailure(pIServerUserItem,RequestResult.szFailureReason,RequestResult.cbFailureCode);

			return false;
		}
	}

	//设置用户
	m_LookonUserItemArray.Add(pIServerUserItem);

	//用户状态
	pIServerUserItem->SetClientReady(false); //pIServerUserItem->SetClientReady(false);
	pIServerUserItem->SetUserStatus(US_LOOKON,m_wTableID,wChairID);

	//事件通知
	if (m_pITableUserAction!=NULL)
	{
		m_pITableUserAction->OnActionUserSitDown(wChairID,pIServerUserItem,true);
	}

	//事件通知
	if(m_pIMatchTableAction!=NULL) m_pIMatchTableAction->OnActionUserSitDown(wChairID,pIServerUserItem,true);
	//事件通知
	if(m_pIPrivateTableAction!=NULL) m_pIPrivateTableAction->OnActionUserSitDown(wChairID,pIServerUserItem,true);
	return true;
}

//坐下动作
bool CTableFrame::PerformSitDownAction(WORD wChairID, IServerUserItem * pIServerUserItem, LPCTSTR lpszPassword)
{
	//mChen log
	TCHAR szString[512] = TEXT("");
	_sntprintf(szString, CountArray(szString), TEXT("CTableFrame::PerformSitDownAction:wChairID=%d, tableId=%d, userName=%s, userId=%d"), wChairID, m_wTableID, pIServerUserItem->GetNickName(), pIServerUserItem->GetUserInfo()->dwUserID);
	//提示消息
	CTraceService::TraceString(szString, TraceLevel_Normal);

	//效验参数
	ASSERT((pIServerUserItem!=NULL)&&(wChairID<m_wChairCount));
	ASSERT((pIServerUserItem->GetTableID()==INVALID_TABLE)&&(pIServerUserItem->GetChairID()==INVALID_CHAIR));

	//变量定义
	tagUserInfo * pUserInfo=pIServerUserItem->GetUserInfo();
	tagUserRule * pUserRule=pIServerUserItem->GetUserRule();
	IServerUserItem * pITableUserItem=GetTableUserItem(wChairID);

	//状态判断
	if ((CServerRule::IsForfendGameEnter(m_pGameServiceOption->dwServerRule)==true)&&(pIServerUserItem->GetMasterOrder()==0))
	{
		SendRequestFailure(pIServerUserItem,TEXT("抱歉，当前游戏桌子禁止用户进入！"),REQUEST_FAILURE_NORMAL);
		return false;
	}

	//模拟处理
	if (m_pGameServiceAttrib->wChairCount < MAX_CHAIR && pIServerUserItem->IsAndroidUser()==false)
	{
		//定义变量
		CAttemperEngineSink * pAttemperEngineSink=(CAttemperEngineSink *)m_pIMainServiceFrame;

		//查找机器
		for (WORD i=0; i<m_pGameServiceAttrib->wChairCount; i++)
		{
			//获取用户
			IServerUserItem *pIUserItem=m_TableUserItemArray[i];
			if(pIUserItem==NULL) continue;
			if(pIUserItem->IsAndroidUser()==false)break;

			//获取参数
			tagBindParameter * pBindParameter=pAttemperEngineSink->GetBindParameter(pIUserItem->GetBindIndex());
			IAndroidUserItem * pIAndroidUserItem=m_pIAndroidUserManager->SearchAndroidUserItem(pIUserItem->GetUserID(),pBindParameter->dwSocketID);
			tagAndroidParameter * pAndroidParameter=pIAndroidUserItem->GetAndroidParameter();

			//模拟判断
			if((pAndroidParameter->dwServiceGender&ANDROID_SIMULATE)!=0
				&& (pAndroidParameter->dwServiceGender&ANDROID_PASSIVITY)==0
				&& (pAndroidParameter->dwServiceGender&ANDROID_INITIATIVE)==0)
			{
				SendRequestFailure(pIServerUserItem,TEXT("抱歉，当前游戏桌子禁止用户进入！"),REQUEST_FAILURE_NORMAL);
				return false;
			}

			break;
		}
	}

	//动态加入
	bool bDynamicJoin=true;
	if (m_pGameServiceAttrib->cbDynamicJoin==FALSE) bDynamicJoin=false;
	if (CServerRule::IsAllowDynamicJoin(m_pGameServiceOption->dwServerRule)==false) bDynamicJoin=false;

	//游戏状态
	//mChen edit, for HideSeek: 警察无敌模式之前，任何时间进入的人都分配阵营
	if( (m_bGameStarted==true)&&(bDynamicJoin==false) && (m_cbGameStatus == GAME_STATUS_PLAY && m_wGamePlayTime < 45) )
	///if((m_bGameStarted == true) && (bDynamicJoin == false))
	{
		SendRequestFailure(pIServerUserItem,TEXT("游戏已经开始，请等待游戏结束！"),REQUEST_FAILURE_NORMAL);//XT("游戏已经开始了，现在不能进入游戏桌！")

		//mChen add, for HideSeek
		for (INT_PTR i = 0; i < m_LookonUserItemArray.GetCount(); i++)
		{
			if (pIServerUserItem == m_LookonUserItemArray[i])
			{
				//该旁观者已经存在

				//删除子项
				m_LookonUserItemArray.RemoveAt(i);

				//return false;
			}
		}
		///if (m_cbGameStatus == GAME_STATUS_HIDE || m_cbGameStatus == GAME_STATUS_PLAY)
		{
			//// Send message
			//TCHAR szMessage[128] = TEXT("");
			//_sntprintf(szMessage, CountArray(szMessage), TEXT("游戏已经开始了，请等待游戏结束"));
			//SendGameMessage(pIServerUserItem, TEXT("游戏已经开始，请等待游戏结束！"), SMT_CHAT);

			//旁观者
			WORD wLookonChairID = 0;
			//椅子搜索,防止PerformLookonAction时报"您所请求的位置没有游戏玩家，无法旁观此游戏桌"
			for (WORD i = 0; i < GetChairCount(); i++)
			{
				if (GetTableUserItem(i) != NULL)
				{
					wLookonChairID = i;
					break;
				}
			}
			PerformLookonAction(wLookonChairID, pIServerUserItem);
			return false;//return true会导致UserStatus从US_LOOKON变为US_READY，因为return true后上层调用在后面还会调用SetUserStatus(US_READY
		}

		return false;
	}

	//椅子判断
	if (pITableUserItem!=NULL)
	{
		for (int i = 0;i<m_wChairCount;i++)
		{
			if (!GetTableUserItem(i))
			{
				wChairID = i;
				pITableUserItem = NULL;
				break;
			}
		}
		if (pITableUserItem)
		{
			//构造信息
			TCHAR szDescribe[128]=TEXT("");
			_sntprintf(szDescribe,CountArray(szDescribe),TEXT("椅子已经被 [ %s ] 捷足先登了，下次动作要快点了！"),pITableUserItem->GetNickName());

			//发送信息
			SendRequestFailure(pIServerUserItem,szDescribe,REQUEST_FAILURE_NORMAL);

			return false;
		}
	}

	//积分变量
	SCORE lUserScore=pIServerUserItem->GetUserScore();
	SCORE lMinTableScore=m_pGameServiceOption->lMinTableScore;
	SCORE lLessEnterScore=m_pITableFrameSink->QueryLessEnterScore(wChairID,pIServerUserItem);

	//密码效验
	if(((IsTableLocked()==true)&&(pIServerUserItem->GetMasterOrder()==0L))
		&&((lpszPassword==NULL)||(lstrcmp(lpszPassword,m_szEnterPassword)!=0)))
	{
		SendRequestFailure(pIServerUserItem,TEXT("游戏桌进入密码不正确，不能加入游戏！"),REQUEST_FAILURE_PASSWORD);
		return false;
	}

	//积分限制
	if (((lMinTableScore!=0L)&&(lUserScore<lMinTableScore))||((lLessEnterScore!=0L)&&(lUserScore<lLessEnterScore)))
	{
		//构造信息
		TCHAR szDescribe[128]=TEXT("");
		if(m_pGameServiceOption->wServerType==GAME_GENRE_GOLD)
			_sntprintf(szDescribe,CountArray(szDescribe),TEXT("加入游戏至少需要 ") SCORE_STRING TEXT(" 的游戏币，您的游戏币不够，不能加入！"),__max(lMinTableScore,lLessEnterScore));
		else if(m_pGameServiceOption->wServerType==GAME_GENRE_MATCH)
			_sntprintf(szDescribe,CountArray(szDescribe),TEXT("加入游戏至少需要 ") SCORE_STRING TEXT(" 的比赛币，您的比赛币不够，不能加入！"),__max(lMinTableScore,lLessEnterScore));
		else
			_sntprintf(szDescribe,CountArray(szDescribe),TEXT("加入游戏至少需要 ") SCORE_STRING TEXT(" 的游戏积分，您的积分不够，不能加入！"),__max(lMinTableScore,lLessEnterScore));

		//发送信息
		SendRequestFailure(pIServerUserItem,szDescribe,REQUEST_FAILURE_NOSCORE);

		return false;
	}

	//规则效验
	if (EfficacyIPAddress(pIServerUserItem)==false) return false;
	if (EfficacyScoreRule(pIServerUserItem)==false) return false;

	//扩展效验
	if (m_pITableUserRequest!=NULL)
	{
		//变量定义
		tagRequestResult RequestResult;
		ZeroMemory(&RequestResult,sizeof(RequestResult));

		//坐下效验
		if (m_pITableUserRequest->OnUserRequestSitDown(wChairID,pIServerUserItem,RequestResult)==false)
		{
			//发送信息
			SendRequestFailure(pIServerUserItem,RequestResult.szFailureReason,RequestResult.cbFailureCode);

			return false;
		}
	}


	//设置变量
	m_TableUserItemArray[wChairID]=pIServerUserItem;

	//用户状态
	if ((IsGameStarted()==false)||(m_cbStartMode!=START_MODE_TIME_CONTROL))
	{
		if (CServerRule::IsAllowAvertCheatMode(m_pGameServiceOption->dwServerRule)==false && (m_pGameServiceOption->wServerType&GAME_GENRE_MATCH)==0)
		{
			pIServerUserItem->SetClientReady(false); //QY 2016 05 10 如果已经在游戏中再次调用会永远无法开始游戏
			pIServerUserItem->SetUserStatus(US_SIT,m_wTableID,wChairID);
		}
		else
		{
			pIServerUserItem->SetClientReady(false);
			pIServerUserItem->SetUserStatus(US_READY,m_wTableID,wChairID);
		}
	}
	else
	{
		//设置变量
		m_wOffLineCount[wChairID]=0L;
		m_dwOffLineTime[wChairID]=0L;

		//锁定游戏币
		if (m_pGameServiceOption->lServiceScore>0L)
		{
			m_lFrozenedScore[wChairID]=m_pGameServiceOption->lServiceScore;
			pIServerUserItem->FrozenedUserScore(m_pGameServiceOption->lServiceScore);
		}

		//设置状态
		pIServerUserItem->SetClientReady(false);
		pIServerUserItem->SetUserStatus(US_PLAYING,m_wTableID,wChairID);
	}

	m_wUserCount=GetSitUserCount();

	//桌子信息
	if (GetSitUserCount()==1)
	{
		//状态变量
		bool bTableLocked=IsTableLocked();

		//设置变量
		m_dwTableOwnerID=pIServerUserItem->GetUserID();
		lstrcpyn(m_szEnterPassword,pUserRule->szPassword,CountArray(m_szEnterPassword));

		//发送状态
		if (bTableLocked!=IsTableLocked()) SendTableStatus();
	}

	//事件通知
	if (m_pITableUserAction!=NULL)
	{
		m_pITableUserAction->OnActionUserSitDown(wChairID,pIServerUserItem,false);
	}

	//事件通知
	if(m_pIMatchTableAction!=NULL) m_pIMatchTableAction->OnActionUserSitDown(wChairID,pIServerUserItem,false);

	//事件通知
	if(m_pIPrivateTableAction!=NULL) m_pIPrivateTableAction->OnActionUserSitDown(wChairID,pIServerUserItem,false);

	////mChen add, for HideSeek
	//if (m_cbGameStatus == GAME_STATUS_FREE)///if (m_wUserCount == 1)
	//{
	//	//第一个人

	//	if (m_pITableFramePrivate != NULL && m_pITableFramePrivate->CanStartWaitGame(this))
	//	{
	//		StartWaitGame();
	//	}
	//}

	return true;
}

//桌子状态
bool CTableFrame::SendTableStatus()
{
	//变量定义
	CMD_GR_TableStatus TableStatus;
	ZeroMemory(&TableStatus,sizeof(TableStatus));

	//构造数据
	TableStatus.wTableID=m_wTableID;
	TableStatus.TableStatus.cbTableLock=IsTableLocked()?TRUE:FALSE;
	TableStatus.TableStatus.cbPlayStatus=IsTableStarted()?TRUE:FALSE;

	//电脑数据
	m_pIMainServiceFrame->SendData(BG_COMPUTER,MDM_GR_STATUS,SUB_GR_TABLE_STATUS,&TableStatus,sizeof(TableStatus));

	//手机数据

	return true;
}

//请求失败
bool CTableFrame::SendRequestFailure(IServerUserItem * pIServerUserItem, LPCTSTR pszDescribe, LONG lErrorCode)
{
	//变量定义
	CMD_GR_RequestFailure RequestFailure;
	ZeroMemory(&RequestFailure,sizeof(RequestFailure));

	//构造数据
	RequestFailure.lErrorCode=lErrorCode;
	lstrcpyn(RequestFailure.szDescribeString,pszDescribe,CountArray(RequestFailure.szDescribeString));

	//发送数据
	WORD wDataSize=CountStringBuffer(RequestFailure.szDescribeString);
	WORD wHeadSize=sizeof(RequestFailure)-sizeof(RequestFailure.szDescribeString);
	m_pIMainServiceFrame->SendData(pIServerUserItem,MDM_GR_USER,SUB_GR_REQUEST_FAILURE,&RequestFailure,wHeadSize+wDataSize);

	return true;
}

//开始效验
bool CTableFrame::EfficacyStartGame(WORD wReadyChairID)
{
	////mChen log
	//TCHAR szString[512] = TEXT("");
	//_sntprintf(szString, CountArray(szString), TEXT("CTableFrame::EfficacyStartGame:1"));
	////提示消息
	//CTraceService::TraceString(szString, TraceLevel_Normal);

	//状态判断
	if (m_bGameStarted==true) return false;

	//模式过滤
	if (m_cbStartMode==START_MODE_TIME_CONTROL) return false;
	if (m_cbStartMode==START_MODE_MASTER_CONTROL) return false;

	//准备人数
	WORD wReadyUserCount=0;
	for (WORD i=0;i<m_wChairCount;i++)
	{
		//获取用户
		IServerUserItem * pITableUserItem=GetTableUserItem(i);
		if (pITableUserItem==NULL) continue;

		//用户统计
		if (pITableUserItem!=NULL)
		{
			//状态判断
			if (pITableUserItem->IsClientReady() == false)
			{
				////mChen log
				//_sntprintf(szString, CountArray(szString), TEXT("CTableFrame::IsClientReady=false"));
				////提示消息
				//CTraceService::TraceString(szString, TraceLevel_Normal);

				return false;
			}
			if ((wReadyChairID != i) && (pITableUserItem->GetUserStatus() != US_READY))
			{
				////mChen log
				//_sntprintf(szString, CountArray(szString), TEXT("CTableFrame::GetUserStatus= x:%x c:%c d:%d"), pITableUserItem->GetUserStatus(), pITableUserItem->GetUserStatus(), pITableUserItem->GetUserStatus());
				////提示消息
				//CTraceService::TraceString(szString, TraceLevel_Normal);

				//mChen hack
				//pITableUserItem->SetUserStatus(US_READY, m_wTableID, wReadyChairID);

				return false;
			}

			//用户计数
			wReadyUserCount++;
		}
	}

	////mChen log
	//_sntprintf(szString, CountArray(szString), TEXT("CTableFrame::EfficacyStartGame:wReadyUserCount=%f m_cbStartMode=%x", wSubCmdID, m_cbStartMode));
	////提示消息
	//CTraceService::TraceString(szString, TraceLevel_Normal);


	//开始处理
	switch (m_cbStartMode)
	{
	case START_MODE_ALL_READY:			//所有准备
		{
			//数目判断
			if (wReadyUserCount>=2L) return true;

			return false;
		}
	case START_MODE_FULL_READY:			//满人开始
		{
			//人数判断
			if (wReadyUserCount==m_wChairCount) return true;

			return false;
		}
	case START_MODE_PAIR_READY:			//配对开始
		{
			//数目判断
			if (wReadyUserCount==m_wChairCount) return true;
			if ((wReadyUserCount<2L)||(wReadyUserCount%2)!=0) return false;

			//位置判断
			for (WORD i=0;i<m_wChairCount/2;i++)
			{
				//获取用户
				IServerUserItem * pICurrentUserItem=GetTableUserItem(i);
				IServerUserItem * pITowardsUserItem=GetTableUserItem(i+m_wChairCount/2);

				//位置过滤
				if ((pICurrentUserItem==NULL)&&(pITowardsUserItem!=NULL)) return false;
				if ((pICurrentUserItem!=NULL)&&(pITowardsUserItem==NULL)) return false;
			}

			return true;
		}
	default:
		{
			ASSERT(FALSE);
			return false;
		}
	}

	return false;
}

//地址效验
bool CTableFrame::EfficacyIPAddress(IServerUserItem * pIServerUserItem)
{
	//管理员不受限制
	if(pIServerUserItem->GetMasterOrder()!=0) return true;

	//规则判断
	if (CServerRule::IsForfendGameRule(m_pGameServiceOption->dwServerRule)==true) return true;

	//地址效验
	const tagUserRule * pUserRule=pIServerUserItem->GetUserRule(),*pTableUserRule=NULL;
	bool bCheckSameIP=pUserRule->bLimitSameIP;
	for (WORD i=0;i<m_wChairCount;i++)
	{
		//获取用户
		IServerUserItem * pITableUserItem=GetTableUserItem(i);
		if (pITableUserItem!=NULL && (!pITableUserItem->IsAndroidUser()) && (pITableUserItem->GetMasterOrder()==0))
		{
			pTableUserRule=pITableUserItem->GetUserRule();
			if (pTableUserRule->bLimitSameIP==true) 
			{
				bCheckSameIP=true;
				break;
			}
		}
	}

	//地址效验
	if (bCheckSameIP==true)
	{
		DWORD dwUserIP=pIServerUserItem->GetClientAddr();
		for (WORD i=0;i<m_wChairCount;i++)
		{
			//获取用户
			IServerUserItem * pITableUserItem=GetTableUserItem(i);
			if ((pITableUserItem!=NULL)&&(pITableUserItem != pIServerUserItem)&&(!pITableUserItem->IsAndroidUser())&&(pITableUserItem->GetMasterOrder()==0)&&(pITableUserItem->GetClientAddr()==dwUserIP))
			{
				if (!pUserRule->bLimitSameIP)
				{
					//发送信息
					LPCTSTR pszDescribe=TEXT("此游戏桌玩家设置了不跟相同 IP 地址的玩家游戏，您 IP 地址与此玩家的 IP 地址相同，不能加入游戏！");
					SendRequestFailure(pIServerUserItem,pszDescribe,REQUEST_FAILURE_NORMAL);
					return false;
				}
				else
				{
					//发送信息
					LPCTSTR pszDescribe=TEXT("您设置了不跟相同 IP 地址的玩家游戏，此游戏桌存在与您 IP 地址相同的玩家，不能加入游戏！");
					SendRequestFailure(pIServerUserItem,pszDescribe,REQUEST_FAILURE_NORMAL);
					return false;
				}
			}
		}
		for (WORD i=0;i<m_wChairCount-1;i++)
		{
			//获取用户
			IServerUserItem * pITableUserItem=GetTableUserItem(i);
			if (pITableUserItem!=NULL && (!pITableUserItem->IsAndroidUser()) && (pITableUserItem->GetMasterOrder()==0))
			{
				for (WORD j=i+1;j<m_wChairCount;j++)
				{
					//获取用户
					IServerUserItem * pITableNextUserItem=GetTableUserItem(j);
					if ((pITableNextUserItem!=NULL) && (!pITableNextUserItem->IsAndroidUser()) && (pITableNextUserItem->GetMasterOrder()==0)&&(pITableUserItem->GetClientAddr()==pITableNextUserItem->GetClientAddr()))
					{
						LPCTSTR pszDescribe=TEXT("您设置了不跟相同 IP 地址的玩家游戏，此游戏桌存在 IP 地址相同的玩家，不能加入游戏！");
						SendRequestFailure(pIServerUserItem,pszDescribe,REQUEST_FAILURE_NORMAL);
						return false;
					}
				}
			}
		}
	}
	return true;
}

//积分效验
bool CTableFrame::EfficacyScoreRule(IServerUserItem * pIServerUserItem)
{
	//管理员不受限制
	if(pIServerUserItem->GetMasterOrder()!=0) return true;

	//规则判断
	if (CServerRule::IsForfendGameRule(m_pGameServiceOption->dwServerRule)==true) return true;

	//变量定义
	WORD wWinRate=pIServerUserItem->GetUserWinRate();
	WORD wFleeRate=pIServerUserItem->GetUserFleeRate();

	//积分范围
	for (WORD i=0;i<m_wChairCount;i++)
	{
		//获取用户
		IServerUserItem * pITableUserItem=GetTableUserItem(i);

		//规则效验
		if (pITableUserItem!=NULL)
		{
			//获取规则
			tagUserRule * pTableUserRule=pITableUserItem->GetUserRule();

			//逃率效验
			if ((pTableUserRule->bLimitFleeRate)&&(wFleeRate>pTableUserRule->wMaxFleeRate))
			{
				//构造信息
				TCHAR szDescribe[128]=TEXT("");
				_sntprintf(szDescribe,CountArray(szDescribe),TEXT("您的逃跑率太高，与 %s 设置的设置不符，不能加入游戏！"),pITableUserItem->GetNickName());

				//发送信息
				SendRequestFailure(pIServerUserItem,szDescribe,REQUEST_FAILURE_NORMAL);

				return false;
			}

			//胜率效验
			if ((pTableUserRule->bLimitWinRate)&&(wWinRate<pTableUserRule->wMinWinRate))
			{
				//构造信息
				TCHAR szDescribe[128]=TEXT("");
				_sntprintf(szDescribe,CountArray(szDescribe),TEXT("您的胜率太低，与 %s 设置的设置不符，不能加入游戏！"),pITableUserItem->GetNickName());

				//发送信息
				SendRequestFailure(pIServerUserItem,szDescribe,REQUEST_FAILURE_NORMAL);

				return false;
			}

			//积分效验
			if (pTableUserRule->bLimitGameScore==true)
			{
				//最高积分
				if (pIServerUserItem->GetUserScore()>pTableUserRule->lMaxGameScore)
				{
					//构造信息
					TCHAR szDescribe[128]=TEXT("");
					_sntprintf(szDescribe,CountArray(szDescribe),TEXT("您的积分太高，与 %s 设置的设置不符，不能加入游戏！"),pITableUserItem->GetNickName());

					//发送信息
					SendRequestFailure(pIServerUserItem,szDescribe,REQUEST_FAILURE_NORMAL);

					return false;
				}

				//最低积分
				if (pIServerUserItem->GetUserScore()<pTableUserRule->lMinGameScore)
				{
					//构造信息
					TCHAR szDescribe[128]=TEXT("");
					_sntprintf(szDescribe,CountArray(szDescribe),TEXT("您的积分太低，与 %s 设置的设置不符，不能加入游戏！"),pITableUserItem->GetNickName());

					//发送信息
					SendRequestFailure(pIServerUserItem,szDescribe,REQUEST_FAILURE_NORMAL);

					return false;
				}
			}
		}
	}

	return true;
}


//////////////////////////////////////////////////////////////////////////////////
