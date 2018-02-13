#ifndef TABLE_FRAME_SINK_HEAD_FILE
#define TABLE_FRAME_SINK_HEAD_FILE

#pragma once

#include "Stdafx.h"
#include "GameLogic.h"

//////////////////////////////////////////////////////////////////////////

//枚举定义

//效验类型
enum enEstimatKind
{
	EstimatKind_OutCard,			//出牌效验
	EstimatKind_GangCard,			//杠牌效验
};

//mChen add, for HideSeek
//AI同步
struct AIInfoItem
{
	BYTE							cbAINum;
	AICreateInfoItem				cbAICreateInfoItem[GAME_PLAYER]; //AI们
};

//////////////////////////////////////////////////////////////////////////

//游戏桌子类
class CTableFrameSink : public ITableFrameSink, public ITableUserAction
{
	//游戏变量
protected:
	WORD							m_wBankerUser;							//庄家用户
	LONGLONG						m_lGameScore[GAME_PLAYER];				//游戏得分
	BYTE							m_cbCardIndex[GAME_PLAYER][MAX_INDEX];	//用户扑克
	bool							m_bTrustee[GAME_PLAYER];				//是否托管



	//出牌信息
protected:
	WORD							m_wOutCardUser;							//出牌用户
	BYTE							m_cbOutCardData;						//出牌扑克
	BYTE							m_cbOutCardCount;						//出牌数目
	BYTE							m_cbDiscardCount[GAME_PLAYER];			//丢弃数目
	BYTE							m_cbDiscardCard[GAME_PLAYER][55];		//丢弃记录

	//发牌信息
protected:
	BYTE							m_cbSendCardData;						//发牌扑克
	BYTE							m_cbSendCardCount;						//发牌数目
	BYTE							m_cbLeftCardCount;						//剩余数目
	BYTE							m_cbRepertoryCard[MAX_REPERTORY];		//库存扑克
	BYTE							m_cbRepertoryCard_HZ[MAX_REPERTORY_HZ];	//库存扑克


	//运行变量
protected:
	WORD							m_wResumeUser;							//还原用户
	WORD							m_wCurrentUser;							//当前用户
	WORD							m_wProvideUser;							//供应用户
	BYTE							m_cbProvideCard;						//供应扑克

	//状态变量
protected:
	bool							m_bSendStatus;							//发牌状态
	bool							m_bGangStatus;							//杠牌状态，这种状态下会发生抢杠胡、杠开
	bool							m_bGangOutStatus;						//杠后状态，这种状态下会发生杠上炮

	//用户状态
public:
	bool							m_bResponse[GAME_PLAYER];				//响应标志
	BYTE							m_cbUserAction[GAME_PLAYER];			//用户动作
	BYTE							m_cbOperateCard[GAME_PLAYER];			//操作扑克
	BYTE							m_cbPerformAction[GAME_PLAYER];			//执行动作

	//mChen add, for HideSeek
	CMD_S_HideSeek_ClientPlayersInfo m_sClientsPlayersInfos;
	//CMD_C_HideSeek_ClientPlayersInfo m_sClientsPlayersInfo[GAME_PLAYER];
	CCriticalSection				m_CriticalSection;					//同步对象
	//bool							m_bIsHumanDead[PlayerTeamType::MaxTeamNum][GAME_PLAYER];
	//bool							m_bIsAIDead[PlayerTeamType::MaxTeamNum][GAME_PLAYER];
	//AI同步
	BYTE							m_cbAINum[PlayerTeamType::MaxTeamNum];
	AIInfoItem						m_cbAICreateInfoItems[GAME_PLAYER];		//下标：wChairID
	//道具同步
	InventoryItem					m_sInventoryList[MAX_INVENTORY_NUM];
	//隐身状态
	StealthEffect                   m_sStealth[GAME_PLAYER];

	//Add begin
	bool							m_bCanPiaoStatus[GAME_PLAYER];					//可以飘财状态
	bool                            m_bPiaoStatus[GAME_PLAYER];						//飘财状态
	BYTE							m_cbPiaoCount[GAME_PLAYER];						//飘财次数
	bool							m_bGangPiao[GAME_PLAYER];						//杠飘
	bool							m_bPiaoGang[GAME_PLAYER];						//飘杠

	bool							m_bOperateStatus;								//操作状态
	WORD							m_wPiaoChengBaoUser;							//飘财承包用户
	SCORE							m_lPiaoScore[GAME_PLAYER][GAME_PLAYER];			//飘财承包所要承包的分数
	SCORE							m_lBasePiaoScore[GAME_PLAYER];					//承包时的基础分（不被承包的分数）

	bool							m_bBuGang;										//补杠

	BYTE							m_cbChiPengCount[GAME_PLAYER];
	WORD							m_wChiPengUser[GAME_PLAYER][MAX_WEAVE];			//记录各玩家的吃碰次数
	bool							m_bQingYiSeChengBao[GAME_PLAYER][GAME_PLAYER];	//清一色承包状态

	BYTE							m_cbChiCard;									//吃牌

	bool							m_bLouPengStatus[GAME_PLAYER];					//漏碰状态
	BYTE							m_cbLouPengCard[GAME_PLAYER];					//漏碰牌

	WORD							m_wChiHuUser;

	BYTE							m_cbGangCount[GAME_PLAYER];						//连续杠牌次数

	tagGangCardResult				m_tGangResult[GAME_PLAYER];						//杠牌分析结果（林：）
	//Add end

	//组合扑克
protected:
	BYTE							m_cbWeaveItemCount[GAME_PLAYER];			//组合数目
	tagWeaveItem					m_WeaveItemArray[GAME_PLAYER][MAX_WEAVE];	//组合扑克

	//结束信息
protected:
	BYTE							m_cbChiHuCard;							//吃胡扑克
	DWORD							m_dwChiHuKind[GAME_PLAYER];				//吃胡结果
	CChiHuRight						m_ChiHuRight[GAME_PLAYER];				//
	WORD							m_wProvider[GAME_PLAYER];				//

	//组件变量
protected:
	CGameLogic						m_GameLogic;							//游戏逻辑
	ITableFrame						* m_pITableFrame;						//框架接口
	const tagGameServiceOption		* m_pGameServiceOption;					//配置参数
	BYTE							m_cbGameTypeIdex;						//游戏类型
	DWORD							m_dwGameRuleIdex;						//游戏规则

	//mChen add
	SCORE							m_lBaseScore;

	//ZY add
	int								TotalScore_MJ[GAME_PLAYER];							//麻将总分
	int								m_bPlayCoutIdex;
	BYTE							playercount;
	BYTE							nowcount;
	//函数定义
public:
	//构造函数
	CTableFrameSink();
	//析构函数
	virtual ~CTableFrameSink();

	//基础接口
public:
	//释放对象
	virtual VOID Release() { }
	//接口查询
	virtual void * QueryInterface(const IID & Guid, DWORD dwQueryVer);

	//管理接口
public:
	//初始化
	virtual bool Initialization(IUnknownEx * pIUnknownEx);
	//复位桌子
	virtual VOID RepositionSink();

	//查询接口
public:
	//查询限额
	virtual SCORE QueryConsumeQuota(IServerUserItem * pIServerUserItem){  return 0; };
	//最少积分
	virtual SCORE QueryLessEnterScore(WORD wChairID, IServerUserItem * pIServerUserItem){ return 0; };
	//查询是否扣服务费
	virtual bool QueryBuckleServiceCharge(WORD wChairID){return true;}

	//比赛接口
public:
	//设置基数
	virtual void SetGameBaseScore(LONG lBaseScore){};

	//游戏事件
public:
	//游戏开始
	virtual bool OnEventGameStart();
	//游戏结束
	virtual bool OnEventGameConclude(WORD wChairID, IServerUserItem * pIServerUserItem, BYTE cbReason);
	//发送场景
	virtual bool OnEventSendGameScene(WORD wChiarID, IServerUserItem * pIServerUserItem, BYTE cbGameStatus, bool bSendSecret);
	//游戏重置 ZY add
	virtual void ResetPlayerTotalScore();
	void Shuffle(BYTE* RepertoryCard,int nCardCount); //洗牌

	void GameStart_ZZ();


	//事件接口
public:
	//定时器事件
	virtual bool OnTimerMessage(DWORD wTimerID, WPARAM wBindParam);
	//数据事件
	virtual bool OnDataBaseMessage(WORD wRequestID, VOID * pData, WORD wDataSize) { return false; }
	//积分事件
	virtual bool OnUserScroeNotify(WORD wChairID, IServerUserItem * pIServerUserItem, BYTE cbReason) { return false; }

	//mChen add, for HideSeek
	virtual void HideSeek_SendHeartBeat();
	virtual void GenerateAICreateInfo();
	virtual void HideSeek_SendAICreateInfo(IServerUserItem *pIServerUserItem = NULL, bool bOnlySendToLookonUser=false);
	virtual WORD GetDeadHumanNumOfTeam(PlayerTeamType teamType);
	virtual WORD GetDeadAINumOfTeam(PlayerTeamType teamType);
	virtual bool AreAllPlayersOfTeamDead(PlayerTeamType teamType);
	virtual BYTE GetHumanHP(WORD wChairID);
	//道具同步
	virtual InventoryItem* GetInventoryList();
	virtual void ResetInventoryList();
	virtual void SetResurrection(WORD wChairID);
	virtual void SetStealth(DWORD dwTime, WORD wChairID);
	virtual void StealthUpate();

	//网络接口
public:
	//游戏消息处理
	virtual bool OnGameMessage(WORD wSubCmdID, VOID * pDataBuffer, WORD wDataSize, IServerUserItem * pIServerUserItem/*, IMainServiceFrame * m_pIMainServiceFram*/);
	//框架消息处理
	virtual bool OnFrameMessage(WORD wSubCmdID, VOID * pDataBuffer, WORD wDataSize, IServerUserItem * pIServerUserItem);

	//用户事件
public:
	//用户断线
	virtual bool OnActionUserOffLine(WORD wChairID,IServerUserItem * pIServerUserItem, char *pFunc = NULL, char *pFile = __FILE__, int nLine = __LINE__) { return true; }
	//用户重入
	virtual bool OnActionUserConnect(WORD wChairID,IServerUserItem * pIServerUserItem) { return true; }
	//用户坐下
	virtual bool OnActionUserSitDown(WORD wChairID,IServerUserItem * pIServerUserItem, bool bLookonUser);
	//用户起立
	virtual bool OnActionUserStandUp(WORD wChairID,IServerUserItem * pIServerUserItem, bool bLookonUser);
	//用户同意
	virtual bool OnActionUserOnReady(WORD wChairID,IServerUserItem * pIServerUserItem, void * pData, WORD wDataSize) { return true; }

	virtual void SetPrivateInfo(BYTE bGameTypeIdex,DWORD bGameRuleIdex,SCORE lBaseScore, BYTE bPlayCoutIdex, BYTE PlayerCount);

	//游戏事件
protected:
	//mChen add, for HideSeek
	bool HideSeek_OnClientPlayersInfo(VOID *pDataBuffer, WORD wDataSize, IServerUserItem *pIServerUserItem);

	//用户出牌
	bool OnUserOutCard(WORD wChairID, BYTE cbCardData);
	//用户操作
	bool OnUserOperateCard(WORD wChairID, BYTE cbOperateCode, BYTE cbOperateCard);

public:
	bool hasRule(BYTE cbRule);
	BYTE AnalyseChiHuCard(const BYTE cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], BYTE cbWeaveCount, BYTE cbCurrentCard, CChiHuRight &ChiHuRight);
	BYTE AnalyseChiHuCardZZ(const BYTE cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], BYTE cbWeaveCount, BYTE cbCurrentCard, CChiHuRight &ChiHuRight,bool bSelfSendCard);



	//辅助函数
protected:
	//发送操作
	bool SendOperateNotify();
	//派发扑克
	bool DispatchCardData(WORD wCurrentUser,bool bTail=false);
	//响应判断
	bool EstimateUserRespond(WORD wCenterUser, BYTE cbCenterCard, enEstimatKind EstimatKind);

	//
	void ProcessChiHuUser( WORD wChairId, bool bGiveUp );
	//
	void FiltrateRight( WORD wChairId,CChiHuRight &chr );

};

//////////////////////////////////////////////////////////////////////////

#endif
