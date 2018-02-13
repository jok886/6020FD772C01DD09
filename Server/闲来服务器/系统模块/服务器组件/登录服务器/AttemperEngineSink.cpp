#include "StdAfx.h"
#include "ServiceUnits.h"
#include "ControlPacket.h"
#include "AttemperEngineSink.h"
#include "WeChatPayHttpUnits.h"
#include <sstream>
#include <thread>
#include "QuickSDKServerHttp.h"

//////////////////////////////////////////////////////////////////////////////////

//时间标识
#define IDI_LOAD_GAME_LIST			1									//加载列表
#define IDI_CONNECT_CORRESPOND		2									//重连标识
#define IDI_COLLECT_ONLINE_INFO		3									//统计在线

//////////////////////////////////////////////////////////////////////////////////

void ConvertUtf8ToGBK(CString &strUtf8)
{
	int len=MultiByteToWideChar(CP_UTF8, 0, (LPCTSTR)strUtf8, -1, NULL,0);
	wchar_t * wszGBK = new wchar_t[len];
	memset(wszGBK,0,len);
	MultiByteToWideChar(CP_UTF8, 0, (LPCTSTR)strUtf8, -1, wszGBK, len); 

	len = WideCharToMultiByte(CP_ACP, 0, wszGBK, -1, NULL, 0, NULL, NULL);
	char *szGBK=new char[len + 1];
	memset(szGBK, 0, len + 1);
	WideCharToMultiByte (CP_ACP, 0, wszGBK, -1, szGBK, len, NULL,NULL);

	strUtf8 = szGBK;
	delete[] szGBK;
	delete[] wszGBK;
}
void ConvertUtf8ToGBK(char* pChar,int iLen)
{
	CString kString(pChar);
	ConvertUtf8ToGBK(kString);
	strncpy(pChar,kString.GetString(),iLen);
}
//构造函数
CAttemperEngineSink::CAttemperEngineSink()
{
	//状态变量
	m_bNeekCorrespond=true;
	m_bShowServerStatus=false;

	//状态变量
	m_pInitParameter=NULL;
	m_pBindParameter=NULL;

	//组件变量
	m_pITimerEngine=NULL;
	m_pIDataBaseEngine=NULL;
	m_pITCPNetworkEngine=NULL;
	m_pITCPSocketService=NULL;

	//视频配置
	m_wAVServerPort=0;
	m_dwAVServerAddr=0;

	ZeroMemory((void*)&m_kCheckInInfo,sizeof(m_kCheckInInfo));
	ZeroMemory(&m_BaseEnsureParameter,sizeof(m_BaseEnsureParameter));
	return;
}

//析构函数
CAttemperEngineSink::~CAttemperEngineSink()
{
}

//接口查询
VOID * CAttemperEngineSink::QueryInterface(REFGUID Guid, DWORD dwQueryVer)
{
	QUERYINTERFACE(IAttemperEngineSink,Guid,dwQueryVer);
	QUERYINTERFACE_IUNKNOWNEX(IAttemperEngineSink,Guid,dwQueryVer);
	return NULL;
}
void threadhandle(IDataBaseEngine * m_pIDataBaseEngine)
{
	try
	{
		utility::string_t address = U("http://*:8090");
		uri_builder uri(address);
		auto addr = uri.to_uri().to_string();
		CommandHandler handler(addr);
		CTraceService::TraceString("订单监听开始。。。。。", TraceLevel_Warning);
		handler.open().wait();
		handler.m_pIDataBaseEngine = m_pIDataBaseEngine;
		while (true)
		{
			Sleep(2000);
		}
	}
	catch (std::exception& ex)
	{
		CString strLog;
		strLog.Format(TEXT("监听报错----- %s"), ex.what());
		CTraceService::TraceString(strLog, TraceLevel_Warning);
	}
}
//启动事件
bool CAttemperEngineSink::OnAttemperEngineStart(IUnknownEx * pIUnknownEx)
{
	//绑定参数
	m_pBindParameter=new tagBindParameter[m_pInitParameter->m_wMaxConnect];
	ZeroMemory(m_pBindParameter,sizeof(tagBindParameter)*m_pInitParameter->m_wMaxConnect);

	//设置时间
	ASSERT(m_pITimerEngine!=NULL);
	m_pITimerEngine->SetTimer(IDI_COLLECT_ONLINE_INFO,m_pInitParameter->m_wCollectTime*1000L,TIMES_INFINITY,0);

	//获取目录
	TCHAR szPath[MAX_PATH]=TEXT("");
	CString szFileName;
	GetModuleFileName(AfxGetInstanceHandle(),szPath,sizeof(szPath));
	szFileName=szPath;
	int nIndex = szFileName.ReverseFind(TEXT('\\'));
	szFileName = szFileName.Left(nIndex);
	szFileName += TEXT("\\PlazaOptionConfig.ini");

	//读取配置
	m_bShowServerStatus = (GetPrivateProfileInt(TEXT("ServerStatus"),TEXT("ShowServerStatus"),0,szFileName) != 0);

	//获取目录
	TCHAR szServerAddr[LEN_SERVER]=TEXT("");
	GetCurrentDirectory(sizeof(szPath),szPath);

	//读取配置
	TCHAR szVideoFileName[MAX_PATH]=TEXT("");
	_sntprintf(szVideoFileName,CountArray(szVideoFileName),TEXT("%s\\VideoOption.ini"),szPath);
	m_wAVServerPort=GetPrivateProfileInt(TEXT("VideoOption"),TEXT("ServerPort"), 0,szVideoFileName);
	DWORD dwAddrLen=GetPrivateProfileString(TEXT("VideoOption"),TEXT("ServerAddr"), TEXT(""), szServerAddr,LEN_SERVER,szVideoFileName);
	if(dwAddrLen>0)
	{
		CT2CA strServerDomain(szServerAddr);
		m_dwAVServerAddr=inet_addr(strServerDomain);
	}
	else
	{
		m_dwAVServerAddr=0;
	}

	//开启充值监听
	std::thread t1(threadhandle, m_pIDataBaseEngine);//创建一个分支线程，回调到ListenerStart函数里
	t1.detach();
	return true;
}


//停止事件
bool CAttemperEngineSink::OnAttemperEngineConclude(IUnknownEx * pIUnknownEx)
{
	//状态变量
	m_bNeekCorrespond=true;

	//组件变量
	m_pITimerEngine=NULL;
	m_pIDataBaseEngine=NULL;
	m_pITCPNetworkEngine=NULL;
	m_pITCPSocketService=NULL;

	//删除数据
	SafeDeleteArray(m_pBindParameter);

	//列表组件
	m_ServerListManager.ResetServerList();

	return true;
}

//控制事件
bool CAttemperEngineSink::OnEventControl(WORD wIdentifier, VOID * pData, WORD wDataSize)
{
	switch (wIdentifier)
	{
	case CT_LOAD_DB_GAME_LIST:		//加载列表
		{
			//加载列表
			m_ServerListManager.DisuseKernelItem();
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_LOAD_GAME_LIST,0,NULL,0);

			//加载奖励
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_LOAD_CHECKIN_REWARD,0,NULL,0);

			//加载低保
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_LOAD_BASEENSURE,0,NULL,0);

			//加载新手引导
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_LOAD_BEGINNER,0,NULL,0);

			DBR_GP_GetAddBankConfig kGetAddBankConfig;
			kGetAddBankConfig.iIdex = 0;
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_LOAD_ADDRANK_CONFIG,0,&kGetAddBankConfig,sizeof(kGetAddBankConfig));

			kGetAddBankConfig.iIdex = 1;
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_LOAD_ADDRANK_CONFIG,0,&kGetAddBankConfig,sizeof(kGetAddBankConfig));

			kGetAddBankConfig.iIdex = 2;
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_LOAD_ADDRANK_CONFIG,0,&kGetAddBankConfig,sizeof(kGetAddBankConfig));
			return true;
		}
	case CT_CONNECT_CORRESPOND:		//连接协调
		{
			//发起连接
			tagAddressInfo * pCorrespondAddress=&m_pInitParameter->m_CorrespondAddress;
			m_pITCPSocketService->Connect(pCorrespondAddress->szAddress,m_pInitParameter->m_wCorrespondPort);

			//构造提示
			TCHAR szString[512]=TEXT("");
			_sntprintf(szString,CountArray(szString),TEXT("正在连接协调服务器 [ %s:%d ]"),pCorrespondAddress->szAddress,m_pInitParameter->m_wCorrespondPort);

			//提示消息
			CTraceService::TraceString(szString,TraceLevel_Normal);

			return true;
		}
	}

	return false;
}

//调度事件
bool CAttemperEngineSink::OnEventAttemperData(WORD wRequestID, VOID * pData, WORD wDataSize)
{
	return false;
}

//时间事件
bool CAttemperEngineSink::OnEventTimer(DWORD dwTimerID, WPARAM wBindParam)
{
	switch (dwTimerID)
	{
	case IDI_LOAD_GAME_LIST:		//加载列表
		{
			//加载列表
			m_ServerListManager.DisuseKernelItem();
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_LOAD_GAME_LIST,0,NULL,0);

			return true;
		}
	case IDI_CONNECT_CORRESPOND:	//连接协调
		{
			//发起连接
			tagAddressInfo * pCorrespondAddress=&m_pInitParameter->m_CorrespondAddress;
			m_pITCPSocketService->Connect(pCorrespondAddress->szAddress,m_pInitParameter->m_wCorrespondPort);

			//构造提示
			TCHAR szString[512]=TEXT("");
			_sntprintf(szString,CountArray(szString),TEXT("正在连接协调服务器 [ %s:%d ]"),pCorrespondAddress->szAddress,m_pInitParameter->m_wCorrespondPort);

			//提示消息
			CTraceService::TraceString(szString,TraceLevel_Normal);

			return true;
		}
	case IDI_COLLECT_ONLINE_INFO:	//统计在线
		{
			//变量定义
			DBR_GP_OnLineCountInfo OnLineCountInfo;
			ZeroMemory(&OnLineCountInfo,sizeof(OnLineCountInfo));

			//获取总数
			OnLineCountInfo.dwOnLineCountSum=m_ServerListManager.CollectOnlineInfo();

			//获取类型
			POSITION KindPosition=NULL;
			do
			{
				//获取类型
				CGameKindItem * pGameKindItem=m_ServerListManager.EmunGameKindItem(KindPosition);

				//设置变量
				if (pGameKindItem!=NULL)
				{
					WORD wKindIndex=OnLineCountInfo.wKindCount++;
					OnLineCountInfo.OnLineCountKind[wKindIndex].wKindID=pGameKindItem->m_GameKind.wKindID;
					OnLineCountInfo.OnLineCountKind[wKindIndex].dwOnLineCount=pGameKindItem->m_GameKind.dwOnLineCount;
				}

				//溢出判断
				if (OnLineCountInfo.wKindCount>=CountArray(OnLineCountInfo.OnLineCountKind))
				{
					ASSERT(FALSE);
					break;
				}

			} while (KindPosition!=NULL);

			//log
			TCHAR szString[512] = TEXT("");
			_sntprintf(szString, CountArray(szString), TEXT("在线人数：%d"), OnLineCountInfo.dwOnLineCountSum);
			CTraceService::TraceString(szString, TraceLevel_Debug);
			//发送请求
			WORD wHeadSize=sizeof(OnLineCountInfo)-sizeof(OnLineCountInfo.OnLineCountKind);
			DWORD wSendSize=wHeadSize+OnLineCountInfo.wKindCount*sizeof(OnLineCountInfo.OnLineCountKind[0]);
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_ONLINE_COUNT_INFO,0,&OnLineCountInfo,wSendSize);

			return true;
		}
	}

	return false;
}

//应答事件
bool CAttemperEngineSink::OnEventTCPNetworkBind(DWORD dwClientAddr, DWORD dwSocketID)
{
	//获取索引
	ASSERT(LOWORD(dwSocketID)<m_pInitParameter->m_wMaxConnect);
	if (LOWORD(dwSocketID)>=m_pInitParameter->m_wMaxConnect) return false;

	//变量定义
	WORD wBindIndex=LOWORD(dwSocketID);
	tagBindParameter * pBindParameter=(m_pBindParameter+wBindIndex);

	//设置变量
	pBindParameter->dwSocketID=dwSocketID;
	pBindParameter->dwClientAddr=dwClientAddr;
	pBindParameter->dwActiveTime=(DWORD)time(NULL);

	return true;
}

//关闭事件
bool CAttemperEngineSink::OnEventTCPNetworkShut(DWORD dwClientAddr, DWORD dwActiveTime, DWORD dwSocketID)
{
	//清除信息
	WORD wBindIndex=LOWORD(dwSocketID);
	ZeroMemory((m_pBindParameter+wBindIndex),sizeof(tagBindParameter));

	return false;
}

//读取事件
bool CAttemperEngineSink::OnEventTCPNetworkRead(TCP_Command Command, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	switch (Command.wMainCmdID)
	{
	case MDM_GP_LOGON:			//登录命令
		{
			return OnTCPNetworkMainPCLogon(Command.wSubCmdID,pData,wDataSize,dwSocketID);
		}
	case MDM_GP_SERVER_LIST:	//列表命令
		{
			return OnTCPNetworkMainPCServerList(Command.wSubCmdID,pData,wDataSize,dwSocketID);
		}
	case MDM_GP_USER_SERVICE:	//服务命令
		{
			return OnTCPNetworkMainPCUserService(Command.wSubCmdID,pData,wDataSize,dwSocketID);
		}
	case MDM_GP_REMOTE_SERVICE:	//远程服务
		{
			return OnTCPNetworkMainPCRemoteService(Command.wSubCmdID,pData,wDataSize,dwSocketID);
		}
	case MDM_MB_LOGON:			//登录命令
		{
			return OnTCPNetworkMainMBLogon(Command.wSubCmdID,pData,wDataSize,dwSocketID);
		}
	case MDM_MB_SERVER_LIST:	//列表命令
		{
			return OnTCPNetworkMainMBServerList(Command.wSubCmdID,pData,wDataSize,dwSocketID);
		}

	}

	return false;
}

//数据库事件
bool CAttemperEngineSink::OnEventDataBase(WORD wRequestID, DWORD dwContextID, VOID * pData, DWORD wDataSize)
{
	switch (wRequestID)
	{
	case DBO_GP_LOGON_SUCCESS:			//登录成功
		{
			return OnDBPCLogonSuccess(dwContextID,pData,wDataSize);
		}
	case DBO_GP_LOGON_FAILURE:			//登录失败
		{
			return OnDBPCLogonFailure(dwContextID,pData,wDataSize);
		}
	case DBR_GP_VALIDATE_MBCARD:
		{
			return OnDBPCLogonValidateMBCard(dwContextID,pData,wDataSize);
		}
	case DBO_GP_USER_FACE_INFO:			//用户头像
		{
			return OnDBPCUserFaceInfo(dwContextID,pData,wDataSize);
		}
	case DBO_GP_USER_INDIVIDUAL:		//用户信息
		{
			return OnDBPCUserIndividual(dwContextID,pData,wDataSize);
		}
	case DBO_GP_USER_ACCOUNTINFO:		//用户个人信息
		{
			return OnDBPCUserAccountInfo(dwContextID,pData,wDataSize);
		}
	case DBO_GP_USER_INSURE_INFO:		//银行资料
		{
			return OnDBPCUserInsureInfo(dwContextID,pData,wDataSize);
		}
	case DBO_GP_USER_INSURE_SUCCESS:	//银行成功
		{
			return OnDBPCUserInsureSuccess(dwContextID,pData,wDataSize);
		}
	case DBO_GP_USER_INSURE_FAILURE:	//银行失败
		{
			return OnDBPCUserInsureFailure(dwContextID,pData,wDataSize);
		}
	case DBO_GP_USER_INSURE_USER_INFO:  //用户信息
		{
			return OnDBPCUserInsureUserInfo(dwContextID,pData,wDataSize);
		}
	case DBO_GP_PUBLIC_NOTICE:		//自定义数据查询
		{
			return OnDBPCPublicNotice(dwContextID,pData,wDataSize);
		}
	case DBO_GP_USER_INGAME_SERVER_ID:		//自定义数据查询
		{
			return OnDBPCInGameSevrerID(dwContextID,pData,wDataSize);
		}
	case DBO_GP_MATCH_SIGNUP_RESULT:	//报名结果
		{
			return OnDBPCUserMatchSignupResult(dwContextID,pData,wDataSize);
		}
	case DBO_GP_MATCH_AWARD:	//奖励列表
		{
			return OnDBPCMacthAwardList(dwContextID,pData,wDataSize);
		}
	case DBO_GP_OPERATE_SUCCESS:		//操作成功
		{
			return OnDBPCOperateSuccess(dwContextID,pData,wDataSize);
		}
	case DBO_GP_SPREADER_RESOULT:		//操作成功
		{
			return OnDBPCOSpreaderResoult(dwContextID,pData,wDataSize);
		}

		//mChen add
	case DBO_GP_SPREADERS_INFO_RESOULT:
		{
			return OnDBPCOSpreadersInfoResoult(dwContextID, pData, wDataSize);
		}
	case DBO_GP_ADD_PAYMENT_RESULT:
		{
			return OnDBPCOAddPaymentResult(dwContextID, pData, wDataSize);
		}
	case DBO_GP_CHILDREN_PAYMENT_INFO_RESULT:
		{
			return OnDBPCOChildrenPaymentInfoResult(dwContextID, pData, wDataSize);
		}
	case DBO_GP_ADD_ENTERPRISE_PAYMENT_RESULT:
		{
			return OnDBPCOAddEnterprisePaymentResult(dwContextID, pData, wDataSize);
		}

		//mChen add, for HideSeek
	case DBO_GP_BOUGHT_TAGGER_MODEL_RESULT:
	{
		return OnDBPCOBoughtTaggerModelResoult(dwContextID, pData, wDataSize);
	}

	case DBO_GP_OPERATE_FAILURE:		//操作失败
		{
			return OnDBPCOperateFailure(dwContextID,pData,wDataSize);
		}
	case DBO_GP_GAME_TYPE_ITEM:			//类型子项
		{
			return OnDBPCGameTypeItem(dwContextID,pData,wDataSize);
		}
	case DBO_GP_GAME_KIND_ITEM:			//类型子项
		{
			return OnDBPCGameKindItem(dwContextID,pData,wDataSize);
		}
	case DBO_GP_GAME_NODE_ITEM:			//节点子项
		{
			return OnDBPCGameNodeItem(dwContextID,pData,wDataSize);
		}
	case DBO_GP_GAME_PAGE_ITEM:			//定制子项
		{
			return OnDBPCGamePageItem(dwContextID,pData,wDataSize);
		}
	case DBO_GP_GAME_LIST_RESULT:		//加载结果
		{
			return OnDBPCGameListResult(dwContextID,pData,wDataSize);
		}
	case DBO_MB_LOGON_SUCCESS:			//登录成功
		{
			return OnDBMBLogonSuccess(dwContextID,pData,wDataSize);
		}
	case DBO_MB_LOGON_FAILURE:			//登录失败
		{
			return OnDBMBLogonFailure(dwContextID,pData,wDataSize);
		}
	case DBO_GP_CHECKIN_REWARD:			//签到奖励
		{
			return OnDBPCCheckInReward(dwContextID,pData,wDataSize);
		}
	case DBO_GP_CHECKIN_INFO:			//签到信息
		{
			return OnDBPCUserCheckInInfo(dwContextID,pData,wDataSize);
		}
	case DBO_GP_CHECKIN_RESULT:			//签到结果
		{
			return OnDBPCUserCheckInResult(dwContextID,pData,wDataSize);
		}

		//mChen add
	case DBO_GP_RAFFLE_RESULT:			//抽奖结果
		{
			return OnDBPCUserRaffleResult(dwContextID, pData, wDataSize);
		}
	case DBO_GP_BEGINNER_CONFIG:		//新手活动配置
		{
			return OnDBPCBeginnerConfig(dwContextID,pData,wDataSize);
		}
	case DBO_GP_BEGINNER_RESULT:		//新手活动配置
		{
			return OnDBPCUserBeginnerResult(dwContextID,pData,wDataSize);
		}
	case DBO_GP_BEGINNER_INFO:			//新手活动信息
		{
			return OnDBPCUserBeginnerInfo(dwContextID,pData,wDataSize);
		}
	case DBO_GP_BASEENSURE_PARAMETER:	//低保参数
		{
			return OnDBPCBaseEnsureParameter(dwContextID,pData,wDataSize);
		}
	case DBO_GP_BASEENSURE_RESULT:		//低保结果
		{
			return OnDBPCBaseEnsureResult(dwContextID,pData,wDataSize);
		}
	case DBO_GP_ADDRANK_AWARD_CONFIG:		//低保结果
		{
			return OnDBPCAddBankAwardConfig(dwContextID,pData,wDataSize);
		}
	case DBO_GP_ADDRANK_RANK_BACK:		//低保结果
		{
			return OnDBPCAddBankBack(dwContextID,pData,wDataSize);
		}
	case DBO_GP_EXCHAGE_HUAFEI_BACK:	//兑换话费列表
		{
			return OnDBPCExchangeHuaFeiBack(dwContextID,pData,wDataSize);
		}
	case DBO_GP_SHOPINFO_BACK:		//商城列表
		{
			return OnDBPCShopInfoBack(dwContextID,pData,wDataSize);
		}
	case DBO_GP_GAME_RECORD_LIST:		//商城列表
		{
			return OnDBPCGameRecordList(dwContextID,pData,wDataSize);
		}
	case DBO_GP_GAME_RECORD_TOTAL:		//商城列表
		{
			return OnDBPCGameRecordTotal(dwContextID,pData,wDataSize);
		}
	case DBR_GP_QUERY_SPREADER:			//查询推荐人昵称返回
		{
			m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_QUERY_SPREADER,pData,wDataSize);
			return true;
		}
	case DBR_GP_ADD_SPREADER:			//添加推荐人返回
		{
			m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_ADD_SPREADER,pData,wDataSize);
			return true;
		}
	case DBO_GP_MATCH_TOP_PLAYERS:
		{
			return OnDBPCOTopPlayersInfoResoult(dwContextID, pData, wDataSize);
		}
	case DBR_GP_QUERY_NICKNAME:
		{
			return OnDBPCOQueryNickNameInfoResoult(dwContextID, pData, wDataSize);
		}
	case DBR_GP_TRANSFER_DIAMOND:
		{
			return OnDBPCOTransferDiamondsResoult(dwContextID, pData, wDataSize);
		}
	case DBR_GP_ADD_SHOPITEM:
	{
		return OnDBPCAddShopItemResoult(dwContextID, pData, wDataSize);
	}
	case DBR_GP_ADD_EXCHANGSCOREINFO:
	{
		return OnDBPCExchangeScoreInfoResoult(dwContextID, pData, wDataSize);
	}
	}
	
	return false;
}

//关闭事件
bool CAttemperEngineSink::OnEventTCPSocketShut(WORD wServiceID, BYTE cbShutReason)
{
	//协调连接
	if (wServiceID==NETWORK_CORRESPOND)
	{
		//重连判断
		if (m_bNeekCorrespond==true)
		{
			//构造提示
			TCHAR szDescribe[128]=TEXT("");
			_sntprintf(szDescribe,CountArray(szDescribe),TEXT("与协调服务器的连接关闭了，%ld 秒后将重新连接"),m_pInitParameter->m_wConnectTime);

			//提示消息
			CTraceService::TraceString(szDescribe,TraceLevel_Warning);

			//设置时间
			ASSERT(m_pITimerEngine!=NULL);
			m_pITimerEngine->SetTimer(IDI_CONNECT_CORRESPOND,m_pInitParameter->m_wConnectTime*1000L,1,0);

			return true;
		}
	}

	return false;
}

//连接事件
bool CAttemperEngineSink::OnEventTCPSocketLink(WORD wServiceID, INT nErrorCode)
{
	//协调连接
	if (wServiceID==NETWORK_CORRESPOND)
	{
		//错误判断
		if (nErrorCode!=0)
		{
			//构造提示
			TCHAR szDescribe[128]=TEXT("");
			_sntprintf(szDescribe,CountArray(szDescribe),TEXT("协调服务器连接失败 [ %ld ]，%ld 秒后将重新连接"),
				nErrorCode,m_pInitParameter->m_wConnectTime);

			//提示消息
			CTraceService::TraceString(szDescribe,TraceLevel_Warning);

			//设置时间
			ASSERT(m_pITimerEngine!=NULL);
			m_pITimerEngine->SetTimer(IDI_CONNECT_CORRESPOND,m_pInitParameter->m_wConnectTime*1000L,1,0);

			return false;
		}

		//提示消息
		CTraceService::TraceString(TEXT("正在注册游戏登录服务器..."),TraceLevel_Normal);

		//变量定义
		CMD_CS_C_RegisterPlaza RegisterPlaza;
		ZeroMemory(&RegisterPlaza,sizeof(RegisterPlaza));

		//设置变量
		lstrcpyn(RegisterPlaza.szServerName,m_pInitParameter->m_szServerName,CountArray(RegisterPlaza.szServerName));
		lstrcpyn(RegisterPlaza.szServerAddr,m_pInitParameter->m_ServiceAddress.szAddress,CountArray(RegisterPlaza.szServerAddr));

		//发送数据
		m_pITCPSocketService->SendData(MDM_CS_REGISTER,SUB_CS_C_REGISTER_PLAZA,&RegisterPlaza,sizeof(RegisterPlaza));

		return true;
	}

	return true;
}

//读取事件
bool CAttemperEngineSink::OnEventTCPSocketRead(WORD wServiceID, TCP_Command Command, VOID * pData, WORD wDataSize)
{
	//协调连接
	if (wServiceID==NETWORK_CORRESPOND)
	{
		switch (Command.wMainCmdID)
		{
		case MDM_CS_REGISTER:		//注册服务
			{
				return OnTCPSocketMainRegister(Command.wSubCmdID,pData,wDataSize);
			}
		case MDM_CS_SERVICE_INFO:	//服务信息
			{
				return OnTCPSocketMainServiceInfo(Command.wSubCmdID,pData,wDataSize);
			}
		case MDM_CS_REMOTE_SERVICE:	//远程服务
			{
				return OnTCPSocketMainRemoteService(Command.wSubCmdID,pData,wDataSize);
			}
		case MDM_CS_MANAGER_SERVICE: //管理服务
			{
				return true;
			}
		}
	}

	//错误断言
	ASSERT(FALSE);

	return true;
}

//注册事件
bool CAttemperEngineSink::OnTCPSocketMainRegister(WORD wSubCmdID, VOID * pData, WORD wDataSize)
{
	switch (wSubCmdID)
	{
	case SUB_CS_S_REGISTER_FAILURE:		//注册失败
		{
			//变量定义
			CMD_CS_S_RegisterFailure * pRegisterFailure=(CMD_CS_S_RegisterFailure *)pData;

			//效验参数
			ASSERT(wDataSize>=(sizeof(CMD_CS_S_RegisterFailure)-sizeof(pRegisterFailure->szDescribeString)));
			if (wDataSize<(sizeof(CMD_CS_S_RegisterFailure)-sizeof(pRegisterFailure->szDescribeString))) return false;

			//关闭处理
			m_bNeekCorrespond=false;
			m_pITCPSocketService->CloseSocket();

			//显示消息
			LPCTSTR pszDescribeString=pRegisterFailure->szDescribeString;
			if (lstrlen(pszDescribeString)>0) CTraceService::TraceString(pszDescribeString,TraceLevel_Exception);

			//事件通知
			CP_ControlResult ControlResult;
			ControlResult.cbSuccess=ER_FAILURE;
			SendUIControlPacket(UI_CORRESPOND_RESULT,&ControlResult,sizeof(ControlResult));

			return true;
		}
	}

	return true;
}

//列表事件
bool CAttemperEngineSink::OnTCPSocketMainServiceInfo(WORD wSubCmdID, VOID * pData, WORD wDataSize)
{
	switch (wSubCmdID)
	{
	case SUB_CS_S_SERVER_INFO:		//房间信息
		{
			//废弃列表
			m_ServerListManager.DisuseServerItem();

			return true;
		}
	case SUB_CS_S_SERVER_ONLINE:	//房间人数
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_CS_S_ServerOnLine));
			if (wDataSize!=sizeof(CMD_CS_S_ServerOnLine)) return false;

			//变量定义
			CMD_CS_S_ServerOnLine * pServerOnLine=(CMD_CS_S_ServerOnLine *)pData;

			//查找房间
			CGameServerItem * pGameServerItem=m_ServerListManager.SearchGameServer(pServerOnLine->wServerID);
			if(pGameServerItem == NULL) return true;

			//设置人数
			DWORD dwOldOnlineCount=0;
			dwOldOnlineCount = pGameServerItem->m_GameServer.dwOnLineCount;
			pGameServerItem->m_GameServer.dwOnLineCount=pServerOnLine->dwOnLineCount;

			//目录人数
			CGameKindItem * pGameKindItem=m_ServerListManager.SearchGameKind(pGameServerItem->m_GameServer.wKindID);
			if (pGameKindItem!=NULL)
			{
				tagGameServer * pGameServer=&pGameServerItem->m_GameServer;
				pGameKindItem->m_GameKind.dwOnLineCount -= dwOldOnlineCount;
				pGameKindItem->m_GameKind.dwOnLineCount += pGameServer->dwOnLineCount;
			}

			return true;
		}


		//mChen add, for HideSeek
	case SUB_CS_S_LOBBY_INSERT:	//大厅插入
		{
			//效验参数
			ASSERT(wDataSize % sizeof(tagGameLobby) == 0);
			if (wDataSize % sizeof(tagGameLobby) != 0) return false;

			//变量定义
			WORD wItemCount = wDataSize / sizeof(tagGameLobby);
			tagGameLobby * pGameLobby = (tagGameLobby *)pData;

			//更新数据
			for (WORD i = 0; i < wItemCount; i++)
			{
				m_ServerListManager.InsertGameLobby(pGameLobby++);
			}

			return true;
		}
	case SUB_CS_S_LOBBY_REMOVE:	//大厅删除
		{
			//效验参数
			ASSERT(wDataSize == sizeof(CMD_CS_S_LobbyRemove));
			if (wDataSize != sizeof(CMD_CS_S_LobbyRemove)) return false;

			//变量定义
			CMD_CS_S_LobbyRemove * pLobbyRemove = (CMD_CS_S_LobbyRemove *)pData;

			//变量定义
			m_ServerListManager.DeleteGameLobby(pLobbyRemove->wLobbyID);

			return true;
		}

	case SUB_CS_S_SERVER_INSERT:	//房间插入
		{
			//效验参数
			ASSERT(wDataSize%sizeof(tagGameServer)==0);
			if (wDataSize%sizeof(tagGameServer)!=0) return false;

			//变量定义
			WORD wItemCount=wDataSize/sizeof(tagGameServer);
			tagGameServer * pGameServer=(tagGameServer *)pData;

			//更新数据
			for (WORD i=0;i<wItemCount;i++)
			{
				m_ServerListManager.InsertGameServer(pGameServer++);
			}

			return true;
		}
	case SUB_CS_S_SERVER_MODIFY:	//房间修改
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_CS_S_ServerModify));
			if (wDataSize!=sizeof(CMD_CS_S_ServerModify)) return false;

			//变量定义
			CMD_CS_S_ServerModify * pServerModify=(CMD_CS_S_ServerModify *)pData;

			//查找房间
			ASSERT(m_ServerListManager.SearchGameServer(pServerModify->wServerID));
			CGameServerItem * pGameServerItem=m_ServerListManager.SearchGameServer(pServerModify->wServerID);

			//设置房间
			if (pGameServerItem!=NULL)
			{
				//设置人数
				DWORD dwOldOnlineCount=0, dwOldFullCount=0;
				dwOldOnlineCount = pGameServerItem->m_GameServer.dwOnLineCount;
				dwOldFullCount   = pGameServerItem->m_GameServer.dwFullCount;

				//修改房间信息
				pGameServerItem->m_GameServer.wKindID=pServerModify->wKindID;
				pGameServerItem->m_GameServer.wNodeID=pServerModify->wNodeID;
				pGameServerItem->m_GameServer.wSortID=pServerModify->wSortID;
				pGameServerItem->m_GameServer.wServerPort=pServerModify->wServerPort;
				pGameServerItem->m_GameServer.dwOnLineCount=pServerModify->dwOnLineCount;
				pGameServerItem->m_GameServer.dwFullCount=pServerModify->dwFullCount;
				lstrcpyn(pGameServerItem->m_GameServer.szServerName,pServerModify->szServerName,CountArray(pGameServerItem->m_GameServer.szServerName));
				lstrcpyn(pGameServerItem->m_GameServer.szServerAddr,pServerModify->szServerAddr,CountArray(pGameServerItem->m_GameServer.szServerAddr));

				//目录人数
				CGameKindItem * pGameKindItem=m_ServerListManager.SearchGameKind(pGameServerItem->m_GameServer.wKindID);
				if (pGameKindItem!=NULL)
				{
					tagGameServer * pGameServer=&pGameServerItem->m_GameServer;
					pGameKindItem->m_GameKind.dwOnLineCount -= dwOldOnlineCount;
					pGameKindItem->m_GameKind.dwOnLineCount += pGameServer->dwOnLineCount;

					pGameKindItem->m_GameKind.dwFullCount -= dwOldFullCount;
					pGameKindItem->m_GameKind.dwFullCount += pGameServer->dwFullCount;
				}
			}

			return true;
		}
	case SUB_CS_S_SERVER_REMOVE:	//房间删除
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_CS_S_ServerRemove));
			if (wDataSize!=sizeof(CMD_CS_S_ServerRemove)) return false;

			//变量定义
			CMD_CS_S_ServerRemove * pServerRemove=(CMD_CS_S_ServerRemove *)pData;

			//变量定义
			m_ServerListManager.DeleteGameServer(pServerRemove->wServerID);

			return true;
		}
	case SUB_CS_S_SERVER_FINISH:	//房间完成
		{
			//清理列表
			m_ServerListManager.CleanServerItem();

			//事件处理
			CP_ControlResult ControlResult;
			ControlResult.cbSuccess=ER_SUCCESS;
			SendUIControlPacket(UI_CORRESPOND_RESULT,&ControlResult,sizeof(ControlResult));

			return true;
		}
	case SUB_CS_S_MATCH_INSERT:		//比赛插入
		{
			//效验参数
			ASSERT(wDataSize%sizeof(tagGameMatch)==0);
			if (wDataSize%sizeof(tagGameMatch)!=0) return false;

			//变量定义
			WORD wItemCount=wDataSize/sizeof(tagGameMatch);
			tagGameMatch * pGameMatch=(tagGameMatch *)pData;

			//更新数据
			for (WORD i=0;i<wItemCount;i++)
			{
				CGameServerItem * pGameServerItem = m_ServerListManager.SearchGameServer(pGameMatch->wServerID);
				if(pGameServerItem!=NULL)
				{
					CopyMemory(&pGameServerItem->m_GameMatch,pGameMatch++,sizeof(pGameServerItem->m_GameMatch));
				}
			}

			return true;
		}
	}

	return true;
}

//远程服务
bool CAttemperEngineSink::OnTCPSocketMainRemoteService(WORD wSubCmdID, VOID * pData, WORD wDataSize)
{
	switch (wSubCmdID)
	{
	case SUB_CS_S_SEARCH_CORRESPOND:	//协调查找
		{
			//变量定义
			CMD_CS_S_SearchCorrespond * pSearchCorrespond=(CMD_CS_S_SearchCorrespond *)pData;

			//效验参数
			ASSERT(wDataSize<=sizeof(CMD_CS_S_SearchCorrespond));
			ASSERT(wDataSize>=(sizeof(CMD_CS_S_SearchCorrespond)-sizeof(pSearchCorrespond->UserRemoteInfo)));
			ASSERT(wDataSize==(sizeof(CMD_CS_S_SearchCorrespond)-sizeof(pSearchCorrespond->UserRemoteInfo)+pSearchCorrespond->wUserCount*sizeof(pSearchCorrespond->UserRemoteInfo[0])));

			//效验参数
			if (wDataSize>sizeof(CMD_CS_S_SearchCorrespond)) return false;
			if (wDataSize<(sizeof(CMD_CS_S_SearchCorrespond)-sizeof(pSearchCorrespond->UserRemoteInfo))) return false;
			if (wDataSize!=(sizeof(CMD_CS_S_SearchCorrespond)-sizeof(pSearchCorrespond->UserRemoteInfo)+pSearchCorrespond->wUserCount*sizeof(pSearchCorrespond->UserRemoteInfo[0]))) return false;

			//判断在线
			ASSERT(LOWORD(pSearchCorrespond->dwSocketID)<m_pInitParameter->m_wMaxConnect);
			if ((m_pBindParameter+LOWORD(pSearchCorrespond->dwSocketID))->dwSocketID!=pSearchCorrespond->dwSocketID) return true;

			//变量定义
			CMD_GP_S_SearchCorrespond SearchCorrespond;
			ZeroMemory(&SearchCorrespond,sizeof(SearchCorrespond));

			//设置变量
			for (WORD i=0;i<pSearchCorrespond->wUserCount;i++)
			{
				//数据效验
				ASSERT(SearchCorrespond.wUserCount<CountArray(SearchCorrespond.UserRemoteInfo));
				if (SearchCorrespond.wUserCount>=CountArray(SearchCorrespond.UserRemoteInfo)) break;

				//设置变量
				WORD wIndex=SearchCorrespond.wUserCount++;
				CopyMemory(&SearchCorrespond.UserRemoteInfo[wIndex],&pSearchCorrespond->UserRemoteInfo[i],sizeof(SearchCorrespond.UserRemoteInfo[wIndex]));
			}

			//发送数据
			WORD wHeadSize=sizeof(SearchCorrespond)-sizeof(SearchCorrespond.UserRemoteInfo);
			WORD wItemSize=sizeof(SearchCorrespond.UserRemoteInfo[0])*SearchCorrespond.wUserCount;
			m_pITCPNetworkEngine->SendData(pSearchCorrespond->dwSocketID,MDM_GP_REMOTE_SERVICE,SUB_GP_S_SEARCH_CORRESPOND,&SearchCorrespond,wHeadSize+wItemSize);

			return true;
		}
	}

	return true;
}

//登录处理
bool CAttemperEngineSink::OnTCPNetworkMainPCLogon(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	switch (wSubCmdID)
	{
	case SUB_GP_LOGON_GAMEID:		//I D 登录
		{
			return OnTCPNetworkSubPCLogonGameID(pData,wDataSize,dwSocketID);
		}
	case SUB_GP_LOGON_ACCOUNTS:		//帐号登录
		{
			return OnTCPNetworkSubPCLogonAccounts(pData,wDataSize,dwSocketID);
		}
	case SUB_GP_REGISTER_ACCOUNTS:	//帐号注册
		{
			return OnTCPNetworkSubPCRegisterAccounts(pData,wDataSize,dwSocketID);
		}
	case SUB_GP_LOGON_VISITOR:		//游客登录
		{
			return OnTCPNetworkSubPCVisitor(pData,wDataSize,dwSocketID);
		}
	}

	return false;
}

//列表处理
bool CAttemperEngineSink::OnTCPNetworkMainPCServerList(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	switch (wSubCmdID)
	{
	case SUB_GP_GET_LIST:			//获取列表
		{
			//发送列表
			SendGameTypeInfo(dwSocketID);
			SendGameKindInfo(dwSocketID);

			//发送列表
			if (m_pInitParameter->m_cbDelayList==TRUE)
			{
				//发送列表
				SendGamePageInfo(dwSocketID,INVALID_WORD);
				SendGameNodeInfo(dwSocketID,INVALID_WORD);
				SendGameServerInfo(dwSocketID,INVALID_WORD);
			}
			else
			{
				//发送页面
				SendGamePageInfo(dwSocketID,0);
			}

			//发送完成
			m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_FINISH);

			return true;
		}
	case SUB_GP_GET_SERVER:			//获取房间
		{
			//效验数据
			ASSERT(wDataSize%sizeof(WORD)==0);
			if (wDataSize%sizeof(WORD)!=0) return false;

			//发送列表
			UINT nKindCount=wDataSize/sizeof(WORD);
			for (UINT i=0;i<nKindCount;i++)
			{
				SendGameNodeInfo(dwSocketID,((WORD *)pData)[i]);
				SendGamePageInfo(dwSocketID,((WORD *)pData)[i]);
				SendGameServerInfo(dwSocketID,((WORD *)pData)[i]);
			}

			//发送完成
			m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_SERVER_FINISH,pData,wDataSize);

			return true;
		}
	case SUB_GP_GET_ONLINE:			//获取在线
		{
			//变量定义
			CMD_GP_GetOnline * pGetOnline=(CMD_GP_GetOnline *)pData;
			WORD wHeadSize=(sizeof(CMD_GP_GetOnline)-sizeof(pGetOnline->wOnLineServerID));

			//效验数据
			ASSERT((wDataSize>=wHeadSize)&&(wDataSize==(wHeadSize+pGetOnline->wServerCount*sizeof(WORD))));
			if ((wDataSize<wHeadSize)||(wDataSize!=(wHeadSize+pGetOnline->wServerCount*sizeof(WORD)))) return false;

			//变量定义
			CMD_GP_KindOnline KindOnline;
			CMD_GP_ServerOnline ServerOnline;
			ZeroMemory(&KindOnline,sizeof(KindOnline));
			ZeroMemory(&ServerOnline,sizeof(ServerOnline));

			//获取类型
			POSITION KindPosition=NULL;
			do
			{
				//获取类型
				CGameKindItem * pGameKindItem=m_ServerListManager.EmunGameKindItem(KindPosition);

				//设置变量
				if (pGameKindItem!=NULL)
				{
					WORD wKindIndex=KindOnline.wKindCount++;
					KindOnline.OnLineInfoKind[wKindIndex].wKindID=pGameKindItem->m_GameKind.wKindID;
					KindOnline.OnLineInfoKind[wKindIndex].dwOnLineCount=pGameKindItem->m_GameKind.dwOnLineCount;
				}

				//溢出判断
				if (KindOnline.wKindCount>=CountArray(KindOnline.OnLineInfoKind))
				{
					ASSERT(FALSE);
					break;
				}

			} while (KindPosition!=NULL);

			//获取房间
			for (WORD i=0;i<pGetOnline->wServerCount;i++)
			{
				//获取房间
				WORD wServerID=pGetOnline->wOnLineServerID[i];
				CGameServerItem * pGameServerItem=m_ServerListManager.SearchGameServer(wServerID);

				//设置变量
				if (pGameServerItem!=NULL)
				{
					WORD wServerIndex=ServerOnline.wServerCount++;
					ServerOnline.OnLineInfoServer[wServerIndex].wServerID=wServerID;
					ServerOnline.OnLineInfoServer[wServerIndex].dwOnLineCount=pGameServerItem->m_GameServer.dwOnLineCount;
				}

				//溢出判断
				if (ServerOnline.wServerCount>=CountArray(ServerOnline.OnLineInfoServer))
				{
					ASSERT(FALSE);
					break;
				}
			}

			//类型在线
			if (KindOnline.wKindCount>0)
			{
				WORD wHeadSize=sizeof(KindOnline)-sizeof(KindOnline.OnLineInfoKind);
				DWORD wSendSize=wHeadSize+KindOnline.wKindCount*sizeof(KindOnline.OnLineInfoKind[0]);
				m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GR_KINE_ONLINE,&KindOnline,wSendSize);
			}

			//房间在线
			if (ServerOnline.wServerCount>0)
			{
				WORD wHeadSize=sizeof(ServerOnline)-sizeof(ServerOnline.OnLineInfoServer);
				DWORD wSendSize=wHeadSize+ServerOnline.wServerCount*sizeof(ServerOnline.OnLineInfoServer[0]);
				m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GR_SERVER_ONLINE,&ServerOnline,wSendSize);
			}

			return true;
		}
	case SUB_GP_GET_COLLECTION:		//获取收藏
		{
			return true;
		}
	}

	return false;
}

//服务处理
bool CAttemperEngineSink::OnTCPNetworkMainPCUserService(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	switch (wSubCmdID)
	{
	case SUB_GP_GAME_RECORD_LIST://游戏记录
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_GetGameRecord_List));
			if (wDataSize!=sizeof(CMD_GP_GetGameRecord_List)) return false;

			//处理消息
			CMD_GP_GetGameRecord_List * pNetInfo=(CMD_GP_GetGameRecord_List *)pData;
			//变量定义
			DBR_GP_GameRecordList kDBInfo;
			ZeroMemory(&kDBInfo,sizeof(kDBInfo));

			//构造数据
			kDBInfo.dwUserID = pNetInfo->dwUserID;

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_GAME_RECORD_LIST,dwSocketID,&kDBInfo,sizeof(kDBInfo));

			return true;
		}
	case SUB_GP_GAME_RECORD_TOTAL://游戏记录
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_GetGameTotalRecord));
			if (wDataSize!=sizeof(CMD_GP_GetGameTotalRecord)) return false;

			//处理消息
			CMD_GP_GetGameTotalRecord * pNetInfo=(CMD_GP_GetGameTotalRecord *)pData;
			//变量定义
			DBR_GP_GetGameTotalRecord kDBInfo;
			ZeroMemory(&kDBInfo,sizeof(kDBInfo));

			//构造数据
			kDBInfo.dwUserID = pNetInfo->dwUserID;
			kDBInfo.dwRecordID = pNetInfo->dwRecordID;

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_GAME_TOTAL_RECORD,dwSocketID,&kDBInfo,sizeof(kDBInfo));

			return true;
		}
	case SUB_GP_QUERY_PUBLIC_NOTICE://自定义字段查询
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_QueryNotice));
			if (wDataSize!=sizeof(CMD_GP_QueryNotice)) return false;

			//处理消息
			CMD_GP_QueryNotice * pNetInfo=(CMD_GP_QueryNotice *)pData;
			//变量定义
			DBR_GP_PublicNotic kDBInfo;
			ZeroMemory(&kDBInfo,sizeof(kDBInfo));

			//构造数据
			lstrcpyn(kDBInfo.szKeyName,pNetInfo->szKeyName,CountArray(kDBInfo.szKeyName));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_PUBLIC_NOTIC,dwSocketID,&kDBInfo,sizeof(kDBInfo));

			return true;
		}
	case SUB_GP_MODIFY_MACHINE:		//绑定机器
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_ModifyMachine));
			if (wDataSize!=sizeof(CMD_GP_ModifyMachine)) return false;

			//处理消息
			CMD_GP_ModifyMachine * pModifyMachine=(CMD_GP_ModifyMachine *)pData;
			pModifyMachine->szPassword[CountArray(pModifyMachine->szPassword)-1]=0;

			//变量定义
			DBR_GP_ModifyMachine ModifyMachine;
			ZeroMemory(&ModifyMachine,sizeof(ModifyMachine));

			//构造数据
			ModifyMachine.cbBind=pModifyMachine->cbBind;
			ModifyMachine.dwUserID=pModifyMachine->dwUserID;
			ModifyMachine.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(ModifyMachine.szPassword,pModifyMachine->szPassword,CountArray(ModifyMachine.szPassword));
			lstrcpyn(ModifyMachine.szMachineID,pModifyMachine->szMachineID,CountArray(ModifyMachine.szMachineID));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_MODIFY_MACHINE,dwSocketID,&ModifyMachine,sizeof(ModifyMachine));

			return true;
		}
	case SUB_GP_MODIFY_LOGON_PASS:	//修改密码
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_ModifyLogonPass));
			if (wDataSize!=sizeof(CMD_GP_ModifyLogonPass)) return false;

			//处理消息
			CMD_GP_ModifyLogonPass * pModifyLogonPass=(CMD_GP_ModifyLogonPass *)pData;
			pModifyLogonPass->szDesPassword[CountArray(pModifyLogonPass->szDesPassword)-1]=0;
			pModifyLogonPass->szScrPassword[CountArray(pModifyLogonPass->szScrPassword)-1]=0;

			//变量定义
			DBR_GP_ModifyLogonPass ModifyLogonPass;
			ZeroMemory(&ModifyLogonPass,sizeof(ModifyLogonPass));

			//构造数据
			ModifyLogonPass.dwUserID=pModifyLogonPass->dwUserID;
			ModifyLogonPass.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(ModifyLogonPass.szDesPassword,pModifyLogonPass->szDesPassword,CountArray(ModifyLogonPass.szDesPassword));
			lstrcpyn(ModifyLogonPass.szScrPassword,pModifyLogonPass->szScrPassword,CountArray(ModifyLogonPass.szScrPassword));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_MODIFY_LOGON_PASS,dwSocketID,&ModifyLogonPass,sizeof(ModifyLogonPass));

			return true;
		}
	case SUB_GP_MODIFY_INSURE_PASS:	//修改密码
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_ModifyInsurePass));
			if (wDataSize!=sizeof(CMD_GP_ModifyInsurePass)) return false;

			//处理消息
			CMD_GP_ModifyInsurePass * pModifyInsurePass=(CMD_GP_ModifyInsurePass *)pData;
			pModifyInsurePass->szDesPassword[CountArray(pModifyInsurePass->szDesPassword)-1]=0;
			pModifyInsurePass->szScrPassword[CountArray(pModifyInsurePass->szScrPassword)-1]=0;

			//变量定义
			DBR_GP_ModifyInsurePass ModifyInsurePass;
			ZeroMemory(&ModifyInsurePass,sizeof(ModifyInsurePass));

			//构造数据
			ModifyInsurePass.dwUserID=pModifyInsurePass->dwUserID;
			ModifyInsurePass.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(ModifyInsurePass.szDesPassword,pModifyInsurePass->szDesPassword,CountArray(ModifyInsurePass.szDesPassword));
			lstrcpyn(ModifyInsurePass.szScrPassword,pModifyInsurePass->szScrPassword,CountArray(ModifyInsurePass.szScrPassword));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_MODIFY_INSURE_PASS,dwSocketID,&ModifyInsurePass,sizeof(ModifyInsurePass));

			return true;
		}
	case SUB_GP_MODIFY_ACCOUNTS:	//修改帐号
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_ModifyAccounts));
			if (wDataSize!=sizeof(CMD_GP_ModifyAccounts)) return false;

			//处理消息
			CMD_GP_ModifyAccounts * pModifyInsurePass=(CMD_GP_ModifyAccounts *)pData;
			pModifyInsurePass->szDesAccount[CountArray(pModifyInsurePass->szDesAccount)-1]=0;
			pModifyInsurePass->szScrPassword[CountArray(pModifyInsurePass->szScrPassword)-1]=0;

			//变量定义
			DBR_GP_ModifyAccounts ModifyInsurePass;
			ZeroMemory(&ModifyInsurePass,sizeof(ModifyInsurePass));

			//构造数据
			ModifyInsurePass.dwUserID=pModifyInsurePass->dwUserID;
			ModifyInsurePass.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(ModifyInsurePass.szDesAccount,pModifyInsurePass->szDesAccount,CountArray(ModifyInsurePass.szDesAccount));
			lstrcpyn(ModifyInsurePass.szScrPassword,pModifyInsurePass->szScrPassword,CountArray(ModifyInsurePass.szScrPassword));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_MODIFY_ACCOUNTS,dwSocketID,&ModifyInsurePass,sizeof(ModifyInsurePass));

			return true;
		}
	case SUB_GP_MODIFY_SPREADER:	//修改推荐人
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_ModifySpreader));
			if (wDataSize!=sizeof(CMD_GP_ModifySpreader)) return false;

			//处理消息
			CMD_GP_ModifySpreader * pModifyInsurePass=(CMD_GP_ModifySpreader *)pData;
			pModifyInsurePass->szPassword[CountArray(pModifyInsurePass->szPassword)-1]=0;
			///pModifyInsurePass->szSpreader[CountArray(pModifyInsurePass->szSpreader)-1]=0;

			//变量定义
			DBR_GP_ModifySpreader ModifyInsurePass;
			ZeroMemory(&ModifyInsurePass,sizeof(ModifyInsurePass));

			//构造数据
			ModifyInsurePass.dwUserID=pModifyInsurePass->dwUserID;
			ModifyInsurePass.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(ModifyInsurePass.szPassword,pModifyInsurePass->szPassword,CountArray(ModifyInsurePass.szPassword));
			ModifyInsurePass.dwSpreaderID = pModifyInsurePass->dwSpreaderID;//mChen edit,lstrcpyn(ModifyInsurePass.szSpreader,pModifyInsurePass->szSpreader,CountArray(ModifyInsurePass.szSpreader));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_MODIFY_SPREADER,dwSocketID,&ModifyInsurePass,sizeof(ModifyInsurePass));

			return true;
		}

		//mChen add,增加/删除推荐人身份
	case SUB_GP_ADDDEL_SPREADER:
		{
			//效验参数
			ASSERT(wDataSize == sizeof(CMD_GP_AddDelSpreader));
			if (wDataSize != sizeof(CMD_GP_AddDelSpreader)) return false;

			//处理消息
			CMD_GP_AddDelSpreader * pAddDelSpreader = (CMD_GP_AddDelSpreader *)pData;
			ConvertUtf8ToGBK(pAddDelSpreader->szSpreaderRealName, 32);
			pAddDelSpreader->szPassword[CountArray(pAddDelSpreader->szPassword) - 1] = 0;
			pAddDelSpreader->szSpreaderRealName[CountArray(pAddDelSpreader->szSpreaderRealName) - 1] = 0;
			///pAddDelSpreader->szSpreaderIDCardNo[CountArray(pAddDelSpreader->szSpreaderIDCardNo) - 1] = 0;
			pAddDelSpreader->szSpreaderTelNum[CountArray(pAddDelSpreader->szSpreaderTelNum) - 1] = 0;
			pAddDelSpreader->szSpreaderWeiXinAccount[CountArray(pAddDelSpreader->szSpreaderWeiXinAccount) - 1] = 0;

			//变量定义
			DBR_GP_AddDelSpreader AddDelSpreader;
			ZeroMemory(&AddDelSpreader, sizeof(AddDelSpreader));

			//构造数据
			AddDelSpreader.dwUserID = pAddDelSpreader->dwUserID;
			AddDelSpreader.dwClientAddr = (m_pBindParameter + LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(AddDelSpreader.szPassword, pAddDelSpreader->szPassword, CountArray(AddDelSpreader.szPassword));
			AddDelSpreader.dwSpreaderID = pAddDelSpreader->dwSpreaderID;
			AddDelSpreader.dwParentSpreaderID = pAddDelSpreader->dwParentSpreaderID;
			lstrcpyn(AddDelSpreader.szSpreaderRealName, pAddDelSpreader->szSpreaderRealName, CountArray(AddDelSpreader.szSpreaderRealName));
			///lstrcpyn(AddDelSpreader.szSpreaderIDCardNo, pAddDelSpreader->szSpreaderIDCardNo, CountArray(AddDelSpreader.szSpreaderIDCardNo));
			lstrcpyn(AddDelSpreader.szSpreaderTelNum, pAddDelSpreader->szSpreaderTelNum, CountArray(AddDelSpreader.szSpreaderTelNum));
			lstrcpyn(AddDelSpreader.szSpreaderWeiXinAccount, pAddDelSpreader->szSpreaderWeiXinAccount, CountArray(AddDelSpreader.szSpreaderWeiXinAccount));

			AddDelSpreader.wSpreaderLevel = pAddDelSpreader->wSpreaderLevel;
			AddDelSpreader.bIsAddOperate = pAddDelSpreader->bIsAddOperate;

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_ADDDEL_SPREADER, dwSocketID, &AddDelSpreader, sizeof(AddDelSpreader));

			return true;
		}
		//查询代理人列表
	case SUB_GP_QUERY_SPREADERS_INFO:
	{
		//效验参数
		ASSERT(wDataSize == sizeof(CMD_GP_QuerySpreadersInfo));
		if (wDataSize != sizeof(CMD_GP_QuerySpreadersInfo)) return false;

		//处理消息
		CMD_GP_QuerySpreadersInfo * pQuerySpreadersInfo = (CMD_GP_QuerySpreadersInfo *)pData;
		pQuerySpreadersInfo->szPassword[CountArray(pQuerySpreadersInfo->szPassword) - 1] = 0;

		//变量定义
		DBR_GP_QuerySpreadersInfo QuerySpreadersInfo;
		ZeroMemory(&QuerySpreadersInfo, sizeof(QuerySpreadersInfo));

		//构造数据
		QuerySpreadersInfo.dwUserID = pQuerySpreadersInfo->dwUserID;
		lstrcpyn(QuerySpreadersInfo.szPassword, pQuerySpreadersInfo->szPassword, CountArray(QuerySpreadersInfo.szPassword));

		//投递请求
		m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_QUERY_SPREADERS_INFO, dwSocketID, &QuerySpreadersInfo, sizeof(QuerySpreadersInfo));

		return true;
	}

	//mChen add, for HideSeek
	case SUB_GP_BOUGHT_TAGGER_MODEL:
	{
		//效验参数
		ASSERT(wDataSize == sizeof(CMD_GP_BoughtTaggerModel));
		if (wDataSize != sizeof(CMD_GP_BoughtTaggerModel)) return false;

		//处理消息
		CMD_GP_BoughtTaggerModel * pBoughtTaggerModel = (CMD_GP_BoughtTaggerModel *)pData;
		pBoughtTaggerModel->szPassword[CountArray(pBoughtTaggerModel->szPassword) - 1] = 0;

		//投递请求
		m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_BOUGHT_TAGGER_MODEL, dwSocketID, pBoughtTaggerModel, sizeof(CMD_GP_BoughtTaggerModel));

		return true;
	}

	//mChen add
	//企业提现
	case SUB_GP_ADD_ENTERPRISE_PAYMENT:
	{
		//效验参数
		ASSERT(wDataSize == sizeof(CMD_GP_AddEnterprisePayment));
		if (wDataSize != sizeof(CMD_GP_AddEnterprisePayment)) return false;

		//处理消息
		CMD_GP_AddEnterprisePayment * pAddEnterprisePayment = (CMD_GP_AddEnterprisePayment *)pData;
		pAddEnterprisePayment->szPassword[CountArray(pAddEnterprisePayment->szPassword) - 1] = 0;

		//投递请求
		m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_ADD_ENTERPRISE_PAYMENT, dwSocketID, pAddEnterprisePayment, sizeof(CMD_GP_AddEnterprisePayment));

		return true;
	}
	//内购
	case SUB_GP_ADD_PAYMENT:
		{
			//效验参数
			ASSERT(wDataSize == sizeof(CMD_GP_AddPayment));
			if (wDataSize != sizeof(CMD_GP_AddPayment)) return false;

			//处理消息
			CMD_GP_AddPayment * pAddPayment = (CMD_GP_AddPayment *)pData;
			pAddPayment->szPassword[CountArray(pAddPayment->szPassword) - 1] = 0;

			//变量定义
			DBR_GP_AddPayment AddPayment;
			ZeroMemory(&AddPayment, sizeof(AddPayment));

			//构造数据
			AddPayment.dwUserID = pAddPayment->dwUserID;
			AddPayment.dwPayment = pAddPayment->dwPayment;
			AddPayment.dwBoughtDiamond = pAddPayment->dwBoughtDiamond;
			lstrcpyn(AddPayment.szPassword, pAddPayment->szPassword, CountArray(AddPayment.szPassword));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_ADD_PAYMENT, dwSocketID, &AddPayment, sizeof(AddPayment));

			return true;
		}
		//名下用户交易信息
	case SUB_GP_QUERY_CHILDREN_PAYMENT_INFO:
		{
			//效验参数
			ASSERT(wDataSize == sizeof(CMD_GP_QueryChildrenPaymentInfo));
			if (wDataSize != sizeof(CMD_GP_QueryChildrenPaymentInfo)) return false;

			//处理消息
			CMD_GP_QueryChildrenPaymentInfo * pQueryChildrenPaymentInfo = (CMD_GP_QueryChildrenPaymentInfo *)pData;
			pQueryChildrenPaymentInfo->szPassword[CountArray(pQueryChildrenPaymentInfo->szPassword) - 1] = 0;

			//变量定义
			DBR_GP_QueryChildrenPaymentInfo QueryChildrenPaymentInfo;
			ZeroMemory(&QueryChildrenPaymentInfo, sizeof(QueryChildrenPaymentInfo));

			//构造数据
			QueryChildrenPaymentInfo.dwUserID = pQueryChildrenPaymentInfo->dwUserID;
			lstrcpyn(QueryChildrenPaymentInfo.szPassword, pQueryChildrenPaymentInfo->szPassword, CountArray(QueryChildrenPaymentInfo.szPassword));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_QUERY_CHILDREN_PAYMENT_INFO, dwSocketID, &QueryChildrenPaymentInfo, sizeof(QueryChildrenPaymentInfo));

			return true;
		}
	case SUB_GP_QUERY_PREPAYID:
	{
		//效验参数
		ASSERT(wDataSize == sizeof(CMD_GP_QueryPrePayID));
		if (wDataSize != sizeof(CMD_GP_QueryPrePayID)) return false;

		//处理消息
		CMD_GP_QueryPrePayID * pQueryPrePayID = (CMD_GP_QueryPrePayID *)pData;
	
#if true
		
		std::function<void(void*,DWORD,bool,std::string&, std::string&, std::string&, std::string&,std::string&)> pSend = [](void*netEngine, DWORD dwSocketID, bool bSuccess, std::string& prePayStr, std::string& nonceStr, std::string& timeStampStr, std::string& signStr,std::string& tradeNo)
		{
			CMD_GP_PrePayIDInfo prePayInfo;
			ZeroMemory(&prePayInfo, sizeof(CMD_GP_PrePayIDInfo));
			if (bSuccess)
			{
				prePayInfo.cbStatusCode = 1;
				memcpy(prePayInfo.szPrePayID, prePayStr.c_str(), prePayStr.length());
				memcpy(prePayInfo.szNonceStr, nonceStr.c_str(), nonceStr.length());
				memcpy(prePayInfo.szTimeStamp, timeStampStr.c_str(), timeStampStr.length());
				memcpy(prePayInfo.szSign, signStr.c_str(), signStr.length());
				memcpy(prePayInfo.szTradeNo, tradeNo.c_str(), tradeNo.length());
			}
			((ITCPNetworkEngine*)netEngine)->SendData(dwSocketID, MDM_GP_USER_SERVICE, SUB_GP_QUERY_PREPAYID, &prePayInfo, sizeof(CMD_GP_PrePayIDInfo));
		};
		WeChatPayHttpUnits::getPrePayStr(pQueryPrePayID->dwShopItemID, pQueryPrePayID->dwUserID,(void*)m_pITCPNetworkEngine, dwSocketID, pSend);
#else
		std::string prePayID, nonceStr, timeStamp, signStr;
		bool bResult = WeChatPayHttpUnits::getPrePayStr(pQueryPrePayID->dwShopItemID, tradeNo, prePayID, nonceStr, timeStamp, signStr);

		CMD_GP_PrePayIDInfo prePayInfo;
		ZeroMemory(&prePayInfo, sizeof(CMD_GP_PrePayIDInfo));

		if(bResult)
		{
			prePayInfo.cbStatusCode = 1;
#if true
			memcpy(prePayInfo.szPrePayID, prePayID.c_str(), prePayID.length());
			memcpy(prePayInfo.szNonceStr, nonceStr.c_str(), nonceStr.length());
			memcpy(prePayInfo.szTimeStamp, timeStamp.c_str(), timeStamp.length());
			memcpy(prePayInfo.szSign, signStr.c_str(), signStr.length());
#else
			lstrcpyn(prePayInfo.szPrePayID, prePayID.c_str(), prePayID.length() + 1);
			lstrcpyn(prePayInfo.szNonceStr, nonceStr.c_str(), nonceStr.length() + 1);
			lstrcpyn(prePayInfo.szTimeStamp, timeStamp.c_str(), timeStamp.length() + 1);
			lstrcpyn(prePayInfo.szSign, signStr.c_str(), signStr.length() + 1);
#endif
		}
		m_pITCPNetworkEngine->SendData(dwSocketID, MDM_GP_USER_SERVICE, SUB_GP_QUERY_PREPAYID, &prePayInfo, sizeof(CMD_GP_PrePayIDInfo));
#endif
		return true;
	}
	case SUB_GP_UPLOAD_PAY_INFO:
	{
		//效验参数
		ASSERT(wDataSize == sizeof(CMD_GP_ClientPayInfo));
		if (wDataSize != sizeof(CMD_GP_ClientPayInfo)) return false;

		//处理消息
		CMD_GP_ClientPayInfo * pClientPayInfo = (CMD_GP_ClientPayInfo *)pData;

		std::function<void(void*, DWORD, int,int,std::string)> pSend = [](void*netEngine, DWORD dwSocketID, int retStatus,int curDiamond,std::string retMsg)
		{
			CMD_GP_ClientPayInfoResoult payResoult;
			ZeroMemory(&payResoult, sizeof(payResoult));
			payResoult.cbSuccessState = retStatus;
			payResoult.dwInsureGold = curDiamond;
			memcpy(payResoult.szMsg, retMsg.c_str(), retMsg.length());
			((ITCPNetworkEngine*)netEngine)->SendData(dwSocketID, MDM_GP_USER_SERVICE, SUB_GP_UPLOAD_PAY_INFO, &payResoult, sizeof(CMD_GP_ClientPayInfoResoult));
		};
		std::string transNoStr(pClientPayInfo->szTradeNo);
		WeChatPayHttpUnits::checkPayResult(pClientPayInfo->dwUserID, transNoStr,pClientPayInfo->cbSuccessState, (void*)m_pITCPNetworkEngine, dwSocketID, pSend);
		return true;
	}
		//查询比赛积分列表
	case SUB_GP_MATCH_TOP_LIST:
		{
			//效验参数
			ASSERT(wDataSize == sizeof(CMD_GR_QueryTopNum));
			if (wDataSize != sizeof(CMD_GR_QueryTopNum)) return false;
		
			//处理消息
			CMD_GR_QueryTopNum * pQuerySpreadersInfo = (CMD_GR_QueryTopNum *)pData;
			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_QUERY_TOP_PLAYERS, dwSocketID, pData, wDataSize);

			return true;
		}
	case SUB_GP_QUERY_NICKNAME:
	{
		//效验参数
		ASSERT(wDataSize == sizeof(CMD_GR_QueryNickName));
		if (wDataSize != sizeof(CMD_GR_QueryNickName)) return false;

		//处理消息
		CMD_GR_QueryNickName * pQueryNickNameInfo = (CMD_GR_QueryNickName *)pData;
		
		//投递请求
		m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_QUERY_NICKNAME, dwSocketID, pData, wDataSize);
		return true;
	}
	case SUB_GP_TRANSFER_DIAMOND:
	{
		//效验参数
		ASSERT(wDataSize == sizeof(CMD_GR_TransferDiamond));
		if (wDataSize != sizeof(CMD_GR_TransferDiamond)) return false;

		//处理消息
		CMD_GR_TransferDiamond * pTransferDiamondInfo = (CMD_GR_TransferDiamond *)pData;
		//投递请求
		m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_TRANSFER_DIAMOND, dwSocketID, pData, wDataSize);
		return true;
	}
	case SUB_GP_ADDSHOPITEM:    //WQ
	{
		//效验参数
		ASSERT(wDataSize == sizeof(CMD_GP_ShopItemInfo));
		if (wDataSize != sizeof(CMD_GP_ShopItemInfo)) return false;

		//处理消息
		CMD_GP_ShopItemInfo * pShopItemInfo = (CMD_GP_ShopItemInfo *)pData;

		//变量定义
		DBR_GP_ShopItemInfo ShopItemInfo;
		ZeroMemory(&ShopItemInfo, sizeof(ShopItemInfo));

		//构造数据
		ShopItemInfo.dwUserID = pShopItemInfo->dwUserID;
		lstrcpyn(ShopItemInfo.szUID, pShopItemInfo->szUID, CountArray(ShopItemInfo.szUID));
		lstrcpyn(ShopItemInfo.szOrderID, pShopItemInfo->szOrderID, CountArray(ShopItemInfo.szOrderID));
		ShopItemInfo.wItemID = pShopItemInfo->wItemID;
		ShopItemInfo.wAmount = pShopItemInfo->wAmount;
		ShopItemInfo.wCount = pShopItemInfo->wCount;

		//投递请求
		m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_ADD_SHOPITEM, dwSocketID, &ShopItemInfo, sizeof(ShopItemInfo));
		return true;
	}
	case SUB_GP_EXCHANGESCORE:
	{
		//效验参数
		ASSERT(wDataSize == sizeof(CMD_GP_ExchangScoreInfo));
		if (wDataSize != sizeof(CMD_GP_ExchangScoreInfo)) return false;

		//处理消息
		CMD_GP_ExchangScoreInfo * pExchangScoreInfo = (CMD_GP_ExchangScoreInfo *)pData;

		//变量定义
		DBR_GP_ExchangScoreInfo ExchangScoreInfo;
		ZeroMemory(&ExchangScoreInfo, sizeof(ExchangScoreInfo));

		//构造数据
		ExchangScoreInfo.dwUserID = pExchangScoreInfo->dwUserID;
		ExchangScoreInfo.cbItemID = pExchangScoreInfo->cbItemID;
		ExchangScoreInfo.wAmount = pExchangScoreInfo->wAmount;
		ExchangScoreInfo.cbExchangeType = pExchangScoreInfo->cbExchangeType;

		//投递请求
		m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_ADD_EXCHANGSCOREINFO, dwSocketID, &ExchangScoreInfo, sizeof(ExchangScoreInfo));
		return true;
	}
	case SUB_GP_MODIFY_UNDER_WRITE:	//修改签名
		{
			//变量定义
			CMD_GP_ModifyUnderWrite * pModifyUnderWrite=(CMD_GP_ModifyUnderWrite *)pData;

			//效验参数
			ASSERT(wDataSize<=sizeof(CMD_GP_ModifyUnderWrite));
			ASSERT(wDataSize>=(sizeof(CMD_GP_ModifyUnderWrite)-sizeof(pModifyUnderWrite->szUnderWrite)));
			ASSERT(wDataSize==(sizeof(CMD_GP_ModifyUnderWrite)-sizeof(pModifyUnderWrite->szUnderWrite)+CountStringBuffer(pModifyUnderWrite->szUnderWrite)));

			//效验参数
			if (wDataSize>sizeof(CMD_GP_ModifyUnderWrite)) return false;
			if (wDataSize<(sizeof(CMD_GP_ModifyUnderWrite)-sizeof(pModifyUnderWrite->szUnderWrite))) return false;
			if (wDataSize!=(sizeof(CMD_GP_ModifyUnderWrite)-sizeof(pModifyUnderWrite->szUnderWrite)+CountStringBuffer(pModifyUnderWrite->szUnderWrite))) return false;

			//处理消息
			pModifyUnderWrite->szPassword[CountArray(pModifyUnderWrite->szPassword)-1]=0;
			pModifyUnderWrite->szUnderWrite[CountArray(pModifyUnderWrite->szUnderWrite)-1]=0;

			//变量定义
			DBR_GP_ModifyUnderWrite ModifyUnderWrite;
			ZeroMemory(&ModifyUnderWrite,sizeof(ModifyUnderWrite));

			//构造数据
			ModifyUnderWrite.dwUserID=pModifyUnderWrite->dwUserID;
			ModifyUnderWrite.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(ModifyUnderWrite.szPassword,pModifyUnderWrite->szPassword,CountArray(ModifyUnderWrite.szPassword));
			lstrcpyn(ModifyUnderWrite.szUnderWrite,pModifyUnderWrite->szUnderWrite,CountArray(ModifyUnderWrite.szUnderWrite));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_MODIFY_UNDER_WRITE,dwSocketID,&ModifyUnderWrite,sizeof(ModifyUnderWrite));

			return true;
		}
	case SUB_GP_SYSTEM_FACE_INFO:	//修改头像
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_SystemFaceInfo));
			if (wDataSize!=sizeof(CMD_GP_SystemFaceInfo)) return false;

			//处理消息
			CMD_GP_SystemFaceInfo * pSystemFaceInfo=(CMD_GP_SystemFaceInfo *)pData;

			//变量定义
			DBR_GP_ModifySystemFace ModifySystemFace;
			ZeroMemory(&ModifySystemFace,sizeof(ModifySystemFace));

			//构造数据
			ModifySystemFace.wFaceID=pSystemFaceInfo->wFaceID;
			ModifySystemFace.dwUserID=pSystemFaceInfo->dwUserID;
			ModifySystemFace.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(ModifySystemFace.szPassword,pSystemFaceInfo->szPassword,CountArray(ModifySystemFace.szPassword));
			lstrcpyn(ModifySystemFace.szMachineID,pSystemFaceInfo->szMachineID,CountArray(ModifySystemFace.szMachineID));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_MODIFY_SYSTEM_FACE,dwSocketID,&ModifySystemFace,sizeof(ModifySystemFace));

			return true;
		}
	case SUB_GP_CUSTOM_FACE_INFO:	//修改头像
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_CustomFaceInfo));
			if (wDataSize!=sizeof(CMD_GP_CustomFaceInfo)) return false;

			//处理消息
			CMD_GP_CustomFaceInfo * pCustomFaceInfo=(CMD_GP_CustomFaceInfo *)pData;

			//变量定义
			DBR_GP_ModifyCustomFace ModifyCustomFace;
			ZeroMemory(&ModifyCustomFace,sizeof(ModifyCustomFace));

			//构造数据
			ModifyCustomFace.dwUserID=pCustomFaceInfo->dwUserID;
			ModifyCustomFace.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(ModifyCustomFace.szPassword,pCustomFaceInfo->szPassword,CountArray(ModifyCustomFace.szPassword));
			lstrcpyn(ModifyCustomFace.szMachineID,pCustomFaceInfo->szMachineID,CountArray(ModifyCustomFace.szMachineID));
			CopyMemory(ModifyCustomFace.dwCustomFace,pCustomFaceInfo->dwCustomFace,sizeof(ModifyCustomFace.dwCustomFace));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_MODIFY_CUSTOM_FACE,dwSocketID,&ModifyCustomFace,sizeof(ModifyCustomFace));

			return true;
		}
	case SUB_GP_QUERY_INDIVIDUAL:	//查询信息
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_QueryIndividual));
			if (wDataSize!=sizeof(CMD_GP_QueryIndividual)) return false;

			//处理消息
			CMD_GP_QueryIndividual * pQueryIndividual=(CMD_GP_QueryIndividual *)pData;

			//变量定义
			DBR_GP_QueryIndividual QueryIndividual;
			ZeroMemory(&QueryIndividual,sizeof(QueryIndividual));

			//构造数据
			QueryIndividual.dwUserID=pQueryIndividual->dwUserID;
			QueryIndividual.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_QUERY_INDIVIDUAL,dwSocketID,&QueryIndividual,sizeof(QueryIndividual));

			return true;
		}
	case SUB_GP_QUERY_ACCOUNTINFO:	//查询个人信息
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_QueryAccountInfo));
			if (wDataSize!=sizeof(CMD_GP_QueryAccountInfo)) return false;

			//处理消息
			CMD_GP_QueryAccountInfo * pQueryIndividual=(CMD_GP_QueryAccountInfo *)pData;

			//变量定义
			DBR_GP_QueryAccountInfo QueryIndividual;
			ZeroMemory(&QueryIndividual,sizeof(QueryIndividual));

			//构造数据
			QueryIndividual.dwUserID=pQueryIndividual->dwUserID;
			QueryIndividual.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_QUERY_ACCOUNTINFO,dwSocketID,&QueryIndividual,sizeof(QueryIndividual));

			return true;
		}
	case SUB_GP_QUERY_INGAME_SEVERID:	//查询游戏状态
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_UserInGameServerID));
			if (wDataSize!=sizeof(CMD_GP_UserInGameServerID)) return false;

			//处理消息
			CMD_GP_UserInGameServerID * pNetInfo=(CMD_GP_UserInGameServerID *)pData;

			//变量定义
			DBR_GP_QueryUserInGameServerID kDBInfo;
			ZeroMemory(&kDBInfo,sizeof(kDBInfo));
			kDBInfo.dwUserID=pNetInfo->dwUserID;
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_USER_INGAME_SERVERID,dwSocketID,&kDBInfo,sizeof(kDBInfo));

			return true;
		}
	case SUB_GP_MODIFY_INDIVIDUAL:	//修改资料
		{
			//效验参数
			ASSERT(wDataSize>=sizeof(CMD_GP_ModifyIndividual));
			if (wDataSize<sizeof(CMD_GP_ModifyIndividual)) return false;

			//处理消息
			CMD_GP_ModifyIndividual * pModifyIndividual=(CMD_GP_ModifyIndividual *)pData;
			pModifyIndividual->szPassword[CountArray(pModifyIndividual->szPassword)-1]=0;

			//变量定义
			DBR_GP_ModifyIndividual ModifyIndividual;
			ZeroMemory(&ModifyIndividual,sizeof(ModifyIndividual));

			//设置变量
			ModifyIndividual.dwUserID = pModifyIndividual->dwUserID;
			ModifyIndividual.cbGender = pModifyIndividual->cbGender;
			ModifyIndividual.wModCost = pModifyIndividual->wModCost;
			ModifyIndividual.cbModCosttType = pModifyIndividual->cbModCosttType;
			ModifyIndividual.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(ModifyIndividual.szPassword,pModifyIndividual->szPassword,CountArray(ModifyIndividual.szPassword));

			//变量定义
			VOID * pDataBuffer=NULL;
			tagDataDescribe DataDescribe;
			CRecvPacketHelper RecvPacket(pModifyIndividual+1,wDataSize-sizeof(CMD_GP_ModifyIndividual));

			//扩展信息
			while (true)
			{
				pDataBuffer=RecvPacket.GetData(DataDescribe);
				if (DataDescribe.wDataDescribe==DTP_NULL) break;
				switch (DataDescribe.wDataDescribe)
				{
				case DTP_GP_UI_NICKNAME:		//用户昵称
					{
						ASSERT(pDataBuffer!=NULL);
						ASSERT(DataDescribe.wDataSize<=sizeof(ModifyIndividual.szNickName));
						if (DataDescribe.wDataSize<=sizeof(ModifyIndividual.szNickName))
						{
							CopyMemory(&ModifyIndividual.szNickName,pDataBuffer,DataDescribe.wDataSize);
							ModifyIndividual.szNickName[CountArray(ModifyIndividual.szNickName)-1]=0;
						}
						break;
					}
				case DTP_GP_UI_UNDER_WRITE:		//个性签名
					{
						ASSERT(pDataBuffer!=NULL);
						ASSERT(DataDescribe.wDataSize<=sizeof(ModifyIndividual.szUnderWrite));
						if (DataDescribe.wDataSize<=sizeof(ModifyIndividual.szUnderWrite))
						{
							CopyMemory(&ModifyIndividual.szUnderWrite,pDataBuffer,DataDescribe.wDataSize);
							ModifyIndividual.szUnderWrite[CountArray(ModifyIndividual.szUnderWrite)-1]=0;
						}
						break;
					}
				case DTP_GP_UI_USER_NOTE:		//用户备注
					{
						ASSERT(pDataBuffer!=NULL);
						ASSERT(DataDescribe.wDataSize<=sizeof(ModifyIndividual.szUserNote));
						if (DataDescribe.wDataSize<=sizeof(ModifyIndividual.szUserNote))
						{
							CopyMemory(&ModifyIndividual.szUserNote,pDataBuffer,DataDescribe.wDataSize);
							ModifyIndividual.szUserNote[CountArray(ModifyIndividual.szUserNote)-1]=0;
						}
						break;
					}
				case DTP_GP_UI_COMPELLATION:	//真实名字
					{
						ASSERT(pDataBuffer!=NULL);
						ASSERT(DataDescribe.wDataSize<=sizeof(ModifyIndividual.szCompellation));
						if (DataDescribe.wDataSize<=sizeof(ModifyIndividual.szCompellation))
						{
							CopyMemory(&ModifyIndividual.szCompellation,pDataBuffer,DataDescribe.wDataSize);
							ModifyIndividual.szCompellation[CountArray(ModifyIndividual.szCompellation)-1]=0;
						}
						break;
					}
				case DTP_GP_UI_SEAT_PHONE:		//固定电话
					{
						ASSERT(pDataBuffer!=NULL);
						ASSERT(DataDescribe.wDataSize<=sizeof(ModifyIndividual.szSeatPhone));
						if (DataDescribe.wDataSize<=sizeof(ModifyIndividual.szSeatPhone))
						{
							CopyMemory(ModifyIndividual.szSeatPhone,pDataBuffer,DataDescribe.wDataSize);
							ModifyIndividual.szSeatPhone[CountArray(ModifyIndividual.szSeatPhone)-1]=0;
						}
						break;
					}
				case DTP_GP_UI_MOBILE_PHONE:	//移动电话
					{
						ASSERT(pDataBuffer!=NULL);
						ASSERT(DataDescribe.wDataSize<=sizeof(ModifyIndividual.szMobilePhone));
						if (DataDescribe.wDataSize<=sizeof(ModifyIndividual.szMobilePhone))
						{
							CopyMemory(ModifyIndividual.szMobilePhone,pDataBuffer,DataDescribe.wDataSize);
							ModifyIndividual.szMobilePhone[CountArray(ModifyIndividual.szMobilePhone)-1]=0;
						}
						break;
					}
				case DTP_GP_UI_QQ:				//Q Q 号码
					{
						ASSERT(pDataBuffer!=NULL);
						ASSERT(DataDescribe.wDataSize<=sizeof(ModifyIndividual.szQQ));
						if (DataDescribe.wDataSize<=sizeof(ModifyIndividual.szQQ))
						{
							CopyMemory(ModifyIndividual.szQQ,pDataBuffer,DataDescribe.wDataSize);
							ModifyIndividual.szQQ[CountArray(ModifyIndividual.szQQ)-1]=0;
						}
						break;
					}
				case DTP_GP_UI_EMAIL:			//电子邮件
					{
						ASSERT(pDataBuffer!=NULL);
						ASSERT(DataDescribe.wDataSize<=sizeof(ModifyIndividual.szEMail));
						if (DataDescribe.wDataSize<=sizeof(ModifyIndividual.szEMail))
						{
							CopyMemory(ModifyIndividual.szEMail,pDataBuffer,DataDescribe.wDataSize);
							ModifyIndividual.szEMail[CountArray(ModifyIndividual.szEMail)-1]=0;
						}
						break;
					}
				case DTP_GP_UI_DWELLING_PLACE:	//联系地址
					{
						ASSERT(pDataBuffer!=NULL);
						ASSERT(DataDescribe.wDataSize<=sizeof(ModifyIndividual.szDwellingPlace));
						if (DataDescribe.wDataSize<=sizeof(ModifyIndividual.szDwellingPlace))
						{
							CopyMemory(ModifyIndividual.szDwellingPlace,pDataBuffer,DataDescribe.wDataSize);
							ModifyIndividual.szDwellingPlace[CountArray(ModifyIndividual.szDwellingPlace)-1]=0;
						}
						break;
					}
				case DTP_GP_UI_HEAD_HTTP:	//头像数据
					{
						ASSERT(pDataBuffer!=NULL);
						ASSERT(DataDescribe.wDataSize<=sizeof(ModifyIndividual.szHeadHttp));
						if (DataDescribe.wDataSize<=sizeof(ModifyIndividual.szHeadHttp))
						{
							CopyMemory(ModifyIndividual.szHeadHttp,pDataBuffer,DataDescribe.wDataSize);
							ModifyIndividual.szHeadHttp[CountArray(ModifyIndividual.szHeadHttp)-1]=0;

							//mChen log
							//TCHAR szString[512] = TEXT("");
							//_sntprintf(szString, CountArray(szString), TEXT("SUB_GP_MODIFY_INDIVIDUAL::DTP_GP_UI_HEAD_HTTP：szHeadHttp=%s"), ModifyIndividual.szHeadHttp);
							//提示消息
							//CTraceService::TraceString(szString, TraceLevel_Normal);
						}
						break;
					}
				case DTP_GP_UI_CHANNEL:	//联系地址
					{
						ASSERT(pDataBuffer!=NULL);
						ASSERT(DataDescribe.wDataSize<=sizeof(ModifyIndividual.szUserChannel));
						if (DataDescribe.wDataSize<=sizeof(ModifyIndividual.szUserChannel))
						{
							CopyMemory(ModifyIndividual.szUserChannel,pDataBuffer,DataDescribe.wDataSize);
							ModifyIndividual.szUserChannel[CountArray(ModifyIndividual.szUserChannel)-1]=0;
						}
						break;
					}
				}
			}

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_MODIFY_INDIVIDUAL,dwSocketID,&ModifyIndividual,sizeof(ModifyIndividual));

			return true;
		}
	case SUB_GP_USER_SAVE_SCORE:	//存入游戏币
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_UserSaveScore));
			if (wDataSize!=sizeof(CMD_GP_UserSaveScore)) return false;

			//处理消息
			CMD_GP_UserSaveScore * pUserSaveScore=(CMD_GP_UserSaveScore *)pData;
			pUserSaveScore->szMachineID[CountArray(pUserSaveScore->szMachineID)-1]=0;

			//变量定义
			DBR_GP_UserSaveScore UserSaveScore;
			ZeroMemory(&UserSaveScore,sizeof(UserSaveScore));

			//构造数据
			UserSaveScore.dwUserID=pUserSaveScore->dwUserID;
			UserSaveScore.lSaveScore=pUserSaveScore->lSaveScore;
			UserSaveScore.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(UserSaveScore.szMachineID,pUserSaveScore->szMachineID,CountArray(UserSaveScore.szMachineID));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_USER_SAVE_SCORE,dwSocketID,&UserSaveScore,sizeof(UserSaveScore));

			return true;
		}
	case SUB_GP_USER_TAKE_SCORE:	//取出游戏币
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_UserTakeScore));
			if (wDataSize!=sizeof(CMD_GP_UserTakeScore)) return false;

			//处理消息
			CMD_GP_UserTakeScore * pUserTakeScore=(CMD_GP_UserTakeScore *)pData;
			pUserTakeScore->szPassword[CountArray(pUserTakeScore->szPassword)-1]=0;
			pUserTakeScore->szMachineID[CountArray(pUserTakeScore->szMachineID)-1]=0;

			//变量定义
			DBR_GP_UserTakeScore UserTakeScore;
			ZeroMemory(&UserTakeScore,sizeof(UserTakeScore));

			//构造数据
			UserTakeScore.dwUserID=pUserTakeScore->dwUserID;
			UserTakeScore.lTakeScore=pUserTakeScore->lTakeScore;
			UserTakeScore.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(UserTakeScore.szPassword,pUserTakeScore->szPassword,CountArray(UserTakeScore.szPassword));
			lstrcpyn(UserTakeScore.szMachineID,pUserTakeScore->szMachineID,CountArray(UserTakeScore.szMachineID));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_USER_TAKE_SCORE,dwSocketID,&UserTakeScore,sizeof(UserTakeScore));

			return true;
		}
	case SUB_GP_USER_TRANSFER_SCORE://转账游戏币
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_UserTransferScore));
			if (wDataSize!=sizeof(CMD_GP_UserTransferScore)) return false;

			//处理消息
			CMD_GP_UserTransferScore * pUserTransferScore=(CMD_GP_UserTransferScore *)pData;
			pUserTransferScore->szNickName[CountArray(pUserTransferScore->szNickName)-1]=0;
			pUserTransferScore->szPassword[CountArray(pUserTransferScore->szPassword)-1]=0;
			pUserTransferScore->szMachineID[CountArray(pUserTransferScore->szMachineID)-1]=0;

			//变量定义
			DBR_GP_UserTransferScore UserTransferScore;
			ZeroMemory(&UserTransferScore,sizeof(UserTransferScore));

			//构造数据
			UserTransferScore.dwUserID=pUserTransferScore->dwUserID;
			UserTransferScore.cbByNickName=pUserTransferScore->cbByNickName;
			UserTransferScore.lTransferScore=pUserTransferScore->lTransferScore;
			UserTransferScore.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(UserTransferScore.szNickName,pUserTransferScore->szNickName,CountArray(UserTransferScore.szNickName));
			lstrcpyn(UserTransferScore.szPassword,pUserTransferScore->szPassword,CountArray(UserTransferScore.szPassword));
			lstrcpyn(UserTransferScore.szMachineID,pUserTransferScore->szMachineID,CountArray(UserTransferScore.szMachineID));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_USER_TRANSFER_SCORE,dwSocketID,&UserTransferScore,sizeof(UserTransferScore));

			return true;
		}
	case SUB_GP_QUERY_INSURE_INFO:	//查询银行
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_QueryInsureInfo));
			if (wDataSize!=sizeof(CMD_GP_QueryInsureInfo)) return false;

			//处理消息
			CMD_GP_QueryInsureInfo * pQueryInsureInfo=(CMD_GP_QueryInsureInfo *)pData;

			//变量定义
			DBR_GP_QueryInsureInfo QueryInsureInfo;
			ZeroMemory(&QueryInsureInfo,sizeof(QueryInsureInfo));

			//构造数据
			QueryInsureInfo.dwUserID=pQueryInsureInfo->dwUserID;
			QueryInsureInfo.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_QUERY_INSURE_INFO,dwSocketID,&QueryInsureInfo,sizeof(QueryInsureInfo));

			return true;
		}
	case SUB_GP_QUERY_USER_INFO_REQUEST:  //查询用户
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_QueryUserInfoRequest));
			if (wDataSize!=sizeof(CMD_GP_QueryUserInfoRequest)) return false;

			//处理消息
			CMD_GP_QueryUserInfoRequest * pQueryUserInfoRequest=(CMD_GP_QueryUserInfoRequest *)pData;

			//变量定义
			DBR_GP_QueryInsureUserInfo QueryInsureUserInfo;
			ZeroMemory(&QueryInsureUserInfo,sizeof(QueryInsureUserInfo));

			//构造数据
			QueryInsureUserInfo.cbByNickName=pQueryUserInfoRequest->cbByNickName;
			lstrcpyn(QueryInsureUserInfo.szNickName,pQueryUserInfoRequest->szNickName,CountArray(QueryInsureUserInfo.szNickName));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_QUERY_USER_INFO,dwSocketID,&QueryInsureUserInfo,sizeof(QueryInsureUserInfo));

			return true;
		}
	case SUB_GP_CHECKIN_QUERY:		  //查询签到
		{
			OnTCPNetworkSubPCCheckinQuery(wSubCmdID,pData,wDataSize,dwSocketID);
			return true;
		}
	case SUB_GP_BEGINNER_QUERY:		  //新手引导签到
		{
			OnTCPNetworkSubPCBiggerQuery(wSubCmdID,pData,wDataSize,dwSocketID);
			return true;
		}
	case SUB_GP_BEGINNER_DONE:		  //新手引导签到
		{
			OnTCPNetworkSubPCBiggerAward(wSubCmdID,pData,wDataSize,dwSocketID);
			return true;
		}
	case SUB_GP_CHECKIN_DONE:			  //执行签到
		{
			OnTCPNetworkSubPCCheckInDone(wSubCmdID,pData,wDataSize,dwSocketID);
			return true;
		}
	case SUB_GP_CHECKIN_AWARD:			  //执行签到
		{
			OnTCPNetworkSubPCCheckAward(wSubCmdID,pData,wDataSize,dwSocketID);
			return true;
		}

		//mChen add
	case SUB_GP_RAFFLE_DONE:			  //执行抽奖
		{
			OnTCPNetworkSubPCRaffleDone(wSubCmdID, pData, wDataSize, dwSocketID);
			return true;
		}
	case SUB_GP_BASEENSURE_LOAD:		//加载低保
		{
			return OnTCPNetworkSubPCBaseensureLoad(wSubCmdID,pData,wDataSize,dwSocketID);
		}
	case SUB_GP_BASEENSURE_TAKE:		  //领取低保
		{
			return OnTCPNetworkSubPCBaseensureTake(wSubCmdID,pData,wDataSize,dwSocketID);
		}
	case SUB_GP_ADDRANK_GET_AWARD_INFO:		  
		{
			return OnTCPNetworkSubAddBankAwardInfo(wSubCmdID,pData,wDataSize,dwSocketID);
		}
	case SUB_GP_ADDRANK_GET_RANK:		  
		{
			return OnTCPNetworkSubGetAddBank(wSubCmdID,pData,wDataSize,dwSocketID);
		}
	case SUB_GP_MATCH_SIGNUP:			//比赛报名
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_MatchSignup));
			if (wDataSize!=sizeof(CMD_GP_MatchSignup)) return false;

			//处理消息
			CMD_GP_MatchSignup * pMatchSignup=(CMD_GP_MatchSignup *)pData;
			pMatchSignup->szPassword[CountArray(pMatchSignup->szPassword)-1]=0;
			pMatchSignup->szMachineID[CountArray(pMatchSignup->szMachineID)-1]=0;			

			//查找房间
			CGameServerItem * pGameServerItem = m_ServerListManager.SearchGameServer(pMatchSignup->wServerID);
			if(pGameServerItem==NULL || pGameServerItem->IsMatchServer()==false)
			{
				//发送失败
				SendOperateFailure(dwSocketID, 0,TEXT("抱歉，您报名的比赛不存在或者已经结束！"));
				return true;
			}

			//构造结构
			DBR_GP_MatchSignup MatchSignup;

			//比赛信息
			MatchSignup.wServerID = pMatchSignup->wServerID;
			MatchSignup.dwMatchID = pMatchSignup->dwMatchID;
			MatchSignup.dwMatchNO = pMatchSignup->dwMatchNO;

			//用户信息
			MatchSignup.dwUserID = pMatchSignup->dwUserID;
			lstrcpyn(MatchSignup.szPassword,pMatchSignup->szPassword,CountArray(MatchSignup.szPassword));

			//机器信息
			MatchSignup.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(MatchSignup.szMachineID,pMatchSignup->szMachineID,CountArray(MatchSignup.szMachineID));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_MATCH_SIGNUP,dwSocketID,&MatchSignup,sizeof(MatchSignup));

			return true;
		}
	case SUB_GP_MATCH_UNSIGNUP:			//取消报名
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_MatchUnSignup));
			if (wDataSize!=sizeof(CMD_GP_MatchUnSignup)) return false;

			//处理消息
			CMD_GP_MatchUnSignup * pMatchUnSignup=(CMD_GP_MatchUnSignup *)pData;
			pMatchUnSignup->szPassword[CountArray(pMatchUnSignup->szPassword)-1]=0;
			pMatchUnSignup->szMachineID[CountArray(pMatchUnSignup->szMachineID)-1]=0;			

			//构造结构
			DBR_GP_MatchUnSignup MatchUnSignup;

			//比赛信息
			MatchUnSignup.wServerID = pMatchUnSignup->wServerID;
			MatchUnSignup.dwMatchID = pMatchUnSignup->dwMatchID;
			MatchUnSignup.dwMatchNO = pMatchUnSignup->dwMatchNO;

			//用户信息
			MatchUnSignup.dwUserID = pMatchUnSignup->dwUserID;
			lstrcpyn(MatchUnSignup.szPassword,pMatchUnSignup->szPassword,CountArray(MatchUnSignup.szPassword));

			//机器信息
			MatchUnSignup.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
			lstrcpyn(MatchUnSignup.szMachineID,pMatchUnSignup->szMachineID,CountArray(MatchUnSignup.szMachineID));

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_MATCH_UNSIGNUP,dwSocketID,&MatchUnSignup,sizeof(MatchUnSignup));

			return true;
		}
	case SUB_GP_MATCH_AWARD_LIST:
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_MatchGetAward));
			if (wDataSize!=sizeof(CMD_GP_MatchGetAward)) return false;

			//处理消息
			CMD_GP_MatchGetAward * pMacthGetAward = (CMD_GP_MatchGetAward *)pData;		

			//构造结构
			DBR_GR_LoadMatchReward kLoadMatchReward;

			//比赛信息
			kLoadMatchReward.dwUserID = pMacthGetAward->dwUserID;
			kLoadMatchReward.dwMatchID = pMacthGetAward->dwMatchID;
			kLoadMatchReward.dwMatchNO = pMacthGetAward->dwMatchNO;

			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_MATCH_AWARD,dwSocketID,&kLoadMatchReward,sizeof(kLoadMatchReward));

			return true;
		}
	case SUB_GP_EXCHANGEHUAFEI_GET_LIST_INFO:
		{
			return OnTCPNetworkSubGetExchangeHuaFei(wSubCmdID,pData,wDataSize,dwSocketID);
		}
	case SUB_GP_SHOPINFO_GET_LIST_INFO:
		{
			return OnTCPNetworkSubGetShopInfo(wSubCmdID,pData,wDataSize,dwSocketID);
		}
	case SUB_GP_QUERY_SPREADER:	//查询推荐人昵称
		{
			if(sizeof(CMD_GP_QuerySpreader)!= wDataSize) return false;

			CMD_GP_QuerySpreader *pUser =(CMD_GP_QuerySpreader*)pData;
			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_QUERY_SPREADER,dwSocketID,pUser,sizeof(CMD_GP_QuerySpreader));

			return true;
		}
	case SUB_GP_ADD_SPREADER:	//设置推荐人
		{
			if(sizeof(CMD_GP_QuerySpreader)!= wDataSize) return false;

			CMD_GP_QuerySpreader *pUser =(CMD_GP_QuerySpreader*)pData;
			//投递请求
			m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_ADD_SPREADER,dwSocketID,pUser,sizeof(CMD_GP_QuerySpreader));

			return true;
		}
	}
	return false;
}

//操作失败
VOID CAttemperEngineSink::SendOperateFailure(DWORD dwContextID, LONG lResultCode, LPCTSTR pszDescribe)
{
	//效验参数
	ASSERT(pszDescribe != NULL);
	if(pszDescribe == NULL) return;

	//变量定义
	CMD_GP_OperateFailure OperateFailure;
	ZeroMemory(&OperateFailure,sizeof(OperateFailure));

	//构造数据
	OperateFailure.lResultCode=lResultCode;
	lstrcpyn(OperateFailure.szDescribeString,pszDescribe,CountArray(OperateFailure.szDescribeString));

	//发送数据
	WORD wDescribe=CountStringBuffer(OperateFailure.szDescribeString);
	WORD wHeadSize=sizeof(OperateFailure)-sizeof(OperateFailure.szDescribeString);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_OPERATE_FAILURE,&OperateFailure,wHeadSize+wDescribe);

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return;
}

bool CAttemperEngineSink::OnTCPNetworkSubPCBaseensureLoad(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//构造结构
	CMD_GP_BaseEnsureParamter BaseEnsureParameter;
	BaseEnsureParameter.cbTakeTimes=m_BaseEnsureParameter.cbTakeTimes;
	BaseEnsureParameter.lScoreAmount=m_BaseEnsureParameter.lScoreAmount;
	BaseEnsureParameter.lScoreCondition=m_BaseEnsureParameter.lScoreCondition;

	//投递请求
	m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_USER_SERVICE,SUB_GP_BASEENSURE_PARAMETER,&BaseEnsureParameter,sizeof(BaseEnsureParameter));
	return true;
}
bool CAttemperEngineSink::OnTCPNetworkSubPCBaseensureTake(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{

	//参数校验
	ASSERT(wDataSize==sizeof(CMD_GP_BaseEnsureTake));
	if(wDataSize!=sizeof(CMD_GP_BaseEnsureTake)) return false;

	//提取数据
	CMD_GP_BaseEnsureTake * pBaseEnsureTake = (CMD_GP_BaseEnsureTake *)pData;
	pBaseEnsureTake->szPassword[CountArray(pBaseEnsureTake->szPassword)-1]=0;
	pBaseEnsureTake->szMachineID[CountArray(pBaseEnsureTake->szMachineID)-1]=0;

	//构造结构
	DBR_GP_TakeBaseEnsure TakeBaseEnsure;
	TakeBaseEnsure.dwUserID = pBaseEnsureTake->dwUserID;
	TakeBaseEnsure.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
	lstrcpyn(TakeBaseEnsure.szPassword,pBaseEnsureTake->szPassword,CountArray(TakeBaseEnsure.szPassword));
	lstrcpyn(TakeBaseEnsure.szMachineID,pBaseEnsureTake->szMachineID,CountArray(TakeBaseEnsure.szMachineID));

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_BASEENSURE_TAKE,dwSocketID,&TakeBaseEnsure,sizeof(TakeBaseEnsure));
	return true;
}
//远程处理
bool CAttemperEngineSink::OnTCPNetworkMainPCRemoteService(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	switch (wSubCmdID)
	{
	case SUB_GP_C_SEARCH_CORRESPOND:	//协调查找
		{
			//效验参数
			ASSERT(wDataSize==sizeof(CMD_GP_C_SearchCorrespond));
			if (wDataSize!=sizeof(CMD_GP_C_SearchCorrespond)) return false;

			//处理消息
			CMD_GP_C_SearchCorrespond * pSearchCorrespond=(CMD_GP_C_SearchCorrespond *)pData;
			pSearchCorrespond->szNickName[CountArray(pSearchCorrespond->szNickName)-1]=0;

			//变量定义
			CMD_CS_C_SearchCorrespond SearchCorrespond;
			ZeroMemory(&SearchCorrespond,sizeof(SearchCorrespond));

			//连接变量
			SearchCorrespond.dwSocketID=dwSocketID;
			SearchCorrespond.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;

			//查找变量
			SearchCorrespond.dwGameID=pSearchCorrespond->dwGameID;
			lstrcpyn(SearchCorrespond.szNickName,pSearchCorrespond->szNickName,CountArray(SearchCorrespond.szNickName));

			//发送数据
			m_pITCPSocketService->SendData(MDM_CS_REMOTE_SERVICE,SUB_CS_C_SEARCH_CORRESPOND,&SearchCorrespond,sizeof(SearchCorrespond));

			return true;
		}
	}

	return false;
}
//查询签到
bool CAttemperEngineSink::OnTCPNetworkSubPCCheckinQuery(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//参数校验
	ASSERT(wDataSize==sizeof(CMD_GP_CheckInQueryInfo));
	if(wDataSize!=sizeof(CMD_GP_CheckInQueryInfo)) return false;

	//提取数据
	CMD_GP_CheckInQueryInfo *pCheckInQueryInfo = (CMD_GP_CheckInQueryInfo *)pData;
	pCheckInQueryInfo->szPassword[CountArray(pCheckInQueryInfo->szPassword)-1]=0;

	//构造结构
	DBR_GP_CheckInQueryInfo CheckInQueryInfo;
	CheckInQueryInfo.dwUserID = pCheckInQueryInfo->dwUserID;
	lstrcpyn(CheckInQueryInfo.szPassword,pCheckInQueryInfo->szPassword,CountArray(CheckInQueryInfo.szPassword));

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_CHECKIN_QUERY_INFO,dwSocketID,&CheckInQueryInfo,sizeof(CheckInQueryInfo));

	return true;
}

bool CAttemperEngineSink::OnTCPNetworkSubPCBiggerQuery(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//参数校验
	ASSERT(wDataSize==sizeof(CMD_GP_BeginnerQueryInfo));
	if(wDataSize!=sizeof(CMD_GP_BeginnerQueryInfo)) return false;

	//提取数据
	CMD_GP_BeginnerQueryInfo *pNetInfo = (CMD_GP_BeginnerQueryInfo *)pData;
	pNetInfo->szPassword[CountArray(pNetInfo->szPassword)-1]=0;

	//构造结构
	DBR_GP_BeginnerQueryInfo kDBInfo;
	kDBInfo.dwUserID = pNetInfo->dwUserID;
	lstrcpyn(kDBInfo.szPassword,pNetInfo->szPassword,CountArray(kDBInfo.szPassword));

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_BEGINNER_QUERY_INFO,dwSocketID,&kDBInfo,sizeof(kDBInfo));

	return true;
}
bool CAttemperEngineSink::OnTCPNetworkSubPCBiggerAward(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//参数校验
	ASSERT(wDataSize==sizeof(CMD_GP_BeginnerDone));
	if(wDataSize!=sizeof(CMD_GP_BeginnerDone)) return false;

	//提取数据
	CMD_GP_BeginnerDone *pCheckInDone = (CMD_GP_BeginnerDone *)pData;
	pCheckInDone->szPassword[CountArray(pCheckInDone->szPassword)-1]=0;
	pCheckInDone->szMachineID[CountArray(pCheckInDone->szMachineID)-1]=0;

	//构造结构
	DBR_GP_BeginnerDone CheckInDone;
	CheckInDone.dwUserID = pCheckInDone->dwUserID;
	CheckInDone.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
	lstrcpyn(CheckInDone.szPassword,pCheckInDone->szPassword,CountArray(CheckInDone.szPassword));
	lstrcpyn(CheckInDone.szMachineID,pCheckInDone->szMachineID,CountArray(CheckInDone.szMachineID));

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_BEGINNER_DONE,dwSocketID,&CheckInDone,sizeof(CheckInDone));

	return true;
}
bool CAttemperEngineSink::OnTCPNetworkSubGetAddBank(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	ASSERT(wDataSize==sizeof(CMD_GP_GetAddBank));
	if(wDataSize!=sizeof(CMD_GP_GetAddBank)) return false;

	//提取数据
	CMD_GP_GetAddBank *pNetInfo = (CMD_GP_GetAddBank *)pData;
	pNetInfo->szPassword[CountArray(pNetInfo->szPassword)-1]=0;

	DBR_GP_GetAddBank kDBInfo;
	kDBInfo.iIdex = pNetInfo->iRankIdex;
	kDBInfo.dwUserID = pNetInfo->dwUserID;
	lstrcpyn(kDBInfo.szPassword,pNetInfo->szPassword,CountArray(kDBInfo.szPassword));


	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_GET_ADDRANK,dwSocketID,&kDBInfo,sizeof(kDBInfo));
	return true;
}
bool CAttemperEngineSink::OnTCPNetworkSubAddBankAwardInfo(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_USER_SERVICE,SUB_GP_ADDRANK_BACK_AWARD_INFO,&m_BackAddBankAwardInfo,sizeof(m_BackAddBankAwardInfo));

	return true;
}
//执行
bool CAttemperEngineSink::OnTCPNetworkSubPCCheckInDone(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//参数校验
	ASSERT(wDataSize==sizeof(CMD_GP_CheckInDone));
	if(wDataSize!=sizeof(CMD_GP_CheckInDone)) return false;

	//提取数据
	CMD_GP_CheckInDone *pCheckInDone = (CMD_GP_CheckInDone *)pData;
	pCheckInDone->szPassword[CountArray(pCheckInDone->szPassword)-1]=0;
	pCheckInDone->szMachineID[CountArray(pCheckInDone->szMachineID)-1]=0;

	//构造结构
	DBR_GP_CheckInDone CheckInDone;
	CheckInDone.dwUserID = pCheckInDone->dwUserID;
	CheckInDone.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
	lstrcpyn(CheckInDone.szPassword,pCheckInDone->szPassword,CountArray(CheckInDone.szPassword));
	lstrcpyn(CheckInDone.szMachineID,pCheckInDone->szMachineID,CountArray(CheckInDone.szMachineID));

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_CHECKIN_DONE,dwSocketID,&CheckInDone,sizeof(CheckInDone));

	return true;
}

//mChen add
//抽奖执行
bool CAttemperEngineSink::OnTCPNetworkSubPCRaffleDone(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//参数校验
	ASSERT(wDataSize==sizeof(CMD_GP_RaffleDone));
	if(wDataSize!=sizeof(CMD_GP_RaffleDone)) return false;

	//提取数据
	CMD_GP_RaffleDone *pRaffleDone = (CMD_GP_RaffleDone *)pData;
	pRaffleDone->szPassword[CountArray(pRaffleDone->szPassword)-1]=0;
	pRaffleDone->szMachineID[CountArray(pRaffleDone->szMachineID)-1]=0;

	//构造结构
	DBR_GP_RaffleDone RaffleDone;
	RaffleDone.dwUserID = pRaffleDone->dwUserID;
	RaffleDone.dwRaffleGold = pRaffleDone->dwRaffleGold;

	RaffleDone.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
	lstrcpyn(RaffleDone.szPassword, pRaffleDone->szPassword,CountArray(RaffleDone.szPassword));
	lstrcpyn(RaffleDone.szMachineID, pRaffleDone->szMachineID,CountArray(RaffleDone.szMachineID));

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_RAFFLE_DONE,dwSocketID,&RaffleDone,sizeof(RaffleDone));

	return true;
}

//执行
bool CAttemperEngineSink::OnTCPNetworkSubPCCheckAward(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//参数校验
	ASSERT(wDataSize == sizeof(CMD_GP_CheckInDone));
	if (wDataSize != sizeof(CMD_GP_CheckInDone)) return false;

	//提取数据
	CMD_GP_CheckInDone *pCheckInDone = (CMD_GP_CheckInDone *)pData;
	pCheckInDone->szPassword[CountArray(pCheckInDone->szPassword) - 1] = 0;
	pCheckInDone->szMachineID[CountArray(pCheckInDone->szMachineID) - 1] = 0;

	//构造结构
	DBR_GP_CheckInDone CheckInDone;
	CheckInDone.dwUserID = pCheckInDone->dwUserID;
	CheckInDone.dwClientAddr = (m_pBindParameter + LOWORD(dwSocketID))->dwClientAddr;
	lstrcpyn(CheckInDone.szPassword, pCheckInDone->szPassword, CountArray(CheckInDone.szPassword));
	lstrcpyn(CheckInDone.szMachineID, pCheckInDone->szMachineID, CountArray(CheckInDone.szMachineID));

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_CHECKIN_AWARD, dwSocketID, &CheckInDone, sizeof(CheckInDone));

	return true;
}

//登录处理
bool CAttemperEngineSink::OnTCPNetworkMainMBLogon(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	switch (wSubCmdID)
	{
	case SUB_GP_LOGON_GAMEID:		//I D 登录
		{
			return OnTCPNetworkSubMBLogonGameID(pData,wDataSize,dwSocketID);
		}
	case SUB_GP_LOGON_ACCOUNTS:		//帐号登录
		{
			return OnTCPNetworkSubMBLogonAccounts(pData,wDataSize,dwSocketID);
		}
	case SUB_GP_REGISTER_ACCOUNTS:	//帐号注册
		{
			return OnTCPNetworkSubMBRegisterAccounts(pData,wDataSize,dwSocketID);
		}
	}

	return false;
}

//列表处理
bool CAttemperEngineSink::OnTCPNetworkMainMBServerList(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	return false;
}

//I D 登录
bool CAttemperEngineSink::OnTCPNetworkSubPCLogonGameID(VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//效验参数
	ASSERT(wDataSize>=sizeof(CMD_GP_LogonGameID));
	if (wDataSize<sizeof(CMD_GP_LogonGameID))
	{
		if (wDataSize<sizeof(CMD_GP_LogonGameID)-sizeof(BYTE))
			return false;
	}

	//变量定义
	WORD wBindIndex=LOWORD(dwSocketID);
	tagBindParameter * pBindParameter=(m_pBindParameter+wBindIndex);

	//处理消息
	CMD_GP_LogonGameID * pLogonGameID=(CMD_GP_LogonGameID *)pData;
	pLogonGameID->szPassword[CountArray(pLogonGameID->szPassword)-1]=0;
	pLogonGameID->szMachineID[CountArray(pLogonGameID->szMachineID)-1]=0;

	//设置连接
	pBindParameter->cbClientKind=CLIENT_KIND_COMPUTER;
	pBindParameter->dwPlazaVersion=pLogonGameID->dwPlazaVersion;

	//效验版本
	if (CheckPlazaVersion(DEVICE_TYPE_PC,pLogonGameID->dwPlazaVersion,dwSocketID,((pLogonGameID->cbValidateFlags&LOW_VER_VALIDATE_FLAGS)!=0))==false)
	{
		return true;
	}

	//变量定义
	DBR_GP_LogonGameID LogonGameID;
	ZeroMemory(&LogonGameID,sizeof(LogonGameID));

	//附加信息
	LogonGameID.pBindParameter=(m_pBindParameter+LOWORD(dwSocketID));

	//构造数据
	LogonGameID.dwGameID=pLogonGameID->dwGameID;
	LogonGameID.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
	lstrcpyn(LogonGameID.szPassword,pLogonGameID->szPassword,CountArray(LogonGameID.szPassword));
	lstrcpyn(LogonGameID.szMachineID,pLogonGameID->szMachineID,CountArray(LogonGameID.szMachineID));
	LogonGameID.cbNeeValidateMBCard=(pLogonGameID->cbValidateFlags&MB_VALIDATE_FLAGS);

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_LOGON_GAMEID,dwSocketID,&LogonGameID,sizeof(LogonGameID));

	return true;
}

//帐号登录
bool CAttemperEngineSink::OnTCPNetworkSubPCLogonAccounts(VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//效验参数
	ASSERT(wDataSize>=sizeof(CMD_GP_LogonAccounts));
	if (wDataSize<sizeof(CMD_GP_LogonAccounts))
	{
		if (wDataSize<sizeof(CMD_GP_LogonAccounts)-sizeof(BYTE))
			return false;
	}

	//变量定义
	WORD wBindIndex=LOWORD(dwSocketID);
	tagBindParameter * pBindParameter=(m_pBindParameter+wBindIndex);

	//处理消息
	CMD_GP_LogonAccounts * pLogonAccounts=(CMD_GP_LogonAccounts *)pData;
	pLogonAccounts->szAccounts[CountArray(pLogonAccounts->szAccounts)-1]=0;
	pLogonAccounts->szUid[CountArray(pLogonAccounts->szUid) - 1] = 0;
	pLogonAccounts->szOpenid[CountArray(pLogonAccounts->szOpenid) - 1] = 0;
	pLogonAccounts->szPassword[CountArray(pLogonAccounts->szPassword)-1]=0;
	pLogonAccounts->szMachineID[CountArray(pLogonAccounts->szMachineID)-1]=0;

	//设置连接
	pBindParameter->cbClientKind=CLIENT_KIND_COMPUTER;
	pBindParameter->dwPlazaVersion=pLogonAccounts->dwPlazaVersion;

	//版本判断
	if (CheckPlazaVersion(DEVICE_TYPE_PC,pLogonAccounts->dwPlazaVersion,dwSocketID,((pLogonAccounts->cbValidateFlags&LOW_VER_VALIDATE_FLAGS)!=0))==false)
	{
		return true;
	}

	//变量定义
	DBR_GP_LogonAccounts LogonAccounts;
	ZeroMemory(&LogonAccounts,sizeof(LogonAccounts));

	//附加信息
	LogonAccounts.pBindParameter=(m_pBindParameter+LOWORD(dwSocketID));

	//构造数据
	LogonAccounts.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
	lstrcpyn(LogonAccounts.szAccounts, pLogonAccounts->szAccounts, CountArray(LogonAccounts.szAccounts));
	lstrcpyn(LogonAccounts.szUid, pLogonAccounts->szUid, CountArray(LogonAccounts.szUid));
	lstrcpyn(LogonAccounts.szOpenid,pLogonAccounts->szOpenid,CountArray(LogonAccounts.szOpenid));
	lstrcpyn(LogonAccounts.szPassword,pLogonAccounts->szPassword,CountArray(LogonAccounts.szPassword));
	lstrcpyn(LogonAccounts.szMachineID,pLogonAccounts->szMachineID,CountArray(LogonAccounts.szMachineID));
	LogonAccounts.cbNeeValidateMBCard=(pLogonAccounts->cbValidateFlags&MB_VALIDATE_FLAGS);

	//mChen add, for Match Time
	LogonAccounts.wKindID = pLogonAccounts->wKindID;

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_LOGON_ACCOUNTS,dwSocketID,&LogonAccounts,sizeof(LogonAccounts));

	return true;
}

//帐号注册
bool CAttemperEngineSink::OnTCPNetworkSubPCRegisterAccounts(VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//效验参数
	ASSERT(wDataSize>=sizeof(CMD_GP_RegisterAccounts));
	if (wDataSize<sizeof(CMD_GP_RegisterAccounts))
	{
		if (wDataSize<sizeof(CMD_GP_RegisterAccounts)-sizeof(BYTE))
			return false;
	}

	//变量定义
	WORD wBindIndex=LOWORD(dwSocketID);
	tagBindParameter * pBindParameter=(m_pBindParameter+wBindIndex);

	//处理消息
	CMD_GP_RegisterAccounts * pRegisterAccounts=(CMD_GP_RegisterAccounts *)pData;
	ConvertUtf8ToGBK(pRegisterAccounts->szNickName,LEN_NICKNAME);
	pRegisterAccounts->szAccounts[CountArray(pRegisterAccounts->szAccounts)-1]=0;
	pRegisterAccounts->szUid[CountArray(pRegisterAccounts->szUid) - 1] = 0;
	pRegisterAccounts->szOpenid[CountArray(pRegisterAccounts->szOpenid) - 1] = 0;
	pRegisterAccounts->szNickName[CountArray(pRegisterAccounts->szNickName)-1]=0;
	///pRegisterAccounts->szSpreader[CountArray(pRegisterAccounts->szSpreader)-1]=0;
	pRegisterAccounts->szMachineID[CountArray(pRegisterAccounts->szMachineID)-1]=0;
	pRegisterAccounts->szLogonPass[CountArray(pRegisterAccounts->szLogonPass)-1]=0;
	pRegisterAccounts->szInsurePass[CountArray(pRegisterAccounts->szInsurePass)-1]=0;
	pRegisterAccounts->szPassPortID[CountArray(pRegisterAccounts->szPassPortID)-1]=0;
	pRegisterAccounts->szCompellation[CountArray(pRegisterAccounts->szCompellation)-1]=0;

	//设置连接
	pBindParameter->cbClientKind=CLIENT_KIND_COMPUTER;
	pBindParameter->dwPlazaVersion=pRegisterAccounts->dwPlazaVersion;

	//效验版本
	if (CheckPlazaVersion(DEVICE_TYPE_PC,pRegisterAccounts->dwPlazaVersion,dwSocketID,((pRegisterAccounts->cbValidateFlags&LOW_VER_VALIDATE_FLAGS)!=0))==false)
	{
		return true;
	}

	//变量定义
	DBR_GP_RegisterAccounts RegisterAccounts;
	ZeroMemory(&RegisterAccounts,sizeof(RegisterAccounts));

	//附加信息
	RegisterAccounts.pBindParameter=(m_pBindParameter+LOWORD(dwSocketID));

	//构造数据
	RegisterAccounts.wFaceID=pRegisterAccounts->wFaceID;
	RegisterAccounts.wChannleID = pRegisterAccounts->wChannleID;
	RegisterAccounts.cbGender=pRegisterAccounts->cbGender;
	RegisterAccounts.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
	lstrcpyn(RegisterAccounts.szAccounts,pRegisterAccounts->szAccounts,CountArray(RegisterAccounts.szAccounts));
	lstrcpyn(RegisterAccounts.szUid, pRegisterAccounts->szUid, CountArray(RegisterAccounts.szUid));
	lstrcpyn(RegisterAccounts.szOpenid, pRegisterAccounts->szOpenid, CountArray(RegisterAccounts.szOpenid));
	lstrcpyn(RegisterAccounts.szNickName,pRegisterAccounts->szNickName,CountArray(RegisterAccounts.szNickName));
	RegisterAccounts.dwSpreaderID = pRegisterAccounts->dwSpreaderID;	//mChen edit,lstrcpyn(RegisterAccounts.szSpreader,pRegisterAccounts->szSpreader,CountArray(RegisterAccounts.szSpreader));
	lstrcpyn(RegisterAccounts.szMachineID,pRegisterAccounts->szMachineID,CountArray(RegisterAccounts.szMachineID));
	lstrcpyn(RegisterAccounts.szLogonPass,pRegisterAccounts->szLogonPass,CountArray(RegisterAccounts.szLogonPass));
	lstrcpyn(RegisterAccounts.szInsurePass,pRegisterAccounts->szInsurePass,CountArray(RegisterAccounts.szInsurePass));
	lstrcpyn(RegisterAccounts.szPassPortID,pRegisterAccounts->szPassPortID,CountArray(RegisterAccounts.szPassPortID));
	lstrcpyn(RegisterAccounts.szCompellation,pRegisterAccounts->szCompellation,CountArray(RegisterAccounts.szCompellation));

	//mChen add, for Match Time
	RegisterAccounts.wKindID = pRegisterAccounts->wKindID;

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_REGISTER_ACCOUNTS,dwSocketID,&RegisterAccounts,sizeof(RegisterAccounts));

	return true;
}
bool CAttemperEngineSink::OnTCPNetworkSubPCVisitor(VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	ASSERT(wDataSize>=sizeof(CMD_GP_VisitorLogon));
	if (wDataSize<sizeof(CMD_GP_VisitorLogon)) return false;
	CMD_GP_VisitorLogon * pVisitorAccounts=(CMD_GP_VisitorLogon *)pData;
	pVisitorAccounts->szPassWord[CountArray(pVisitorAccounts->szPassWord)-1]=0;
	//效验版本
	if (CheckPlazaVersion(DEVICE_TYPE_PC,pVisitorAccounts->dwPlazaVersion,dwSocketID,((pVisitorAccounts->cbValidateFlags&LOW_VER_VALIDATE_FLAGS)!=0))==false)
	{
		return true;
	}
	DWORD dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;

	ConvertUtf8ToGBK(pVisitorAccounts->szNickName,LEN_NICKNAME);

	DBR_GP_LogonVisitor VisitorAccounts;
	ZeroMemory(&VisitorAccounts,sizeof(VisitorAccounts));
	VisitorAccounts.pBindParameter=(m_pBindParameter+LOWORD(dwSocketID));
	VisitorAccounts.dwClientIP=dwClientAddr;
	VisitorAccounts.wFaceID=pVisitorAccounts->wFaceID;
	VisitorAccounts.cbGender=pVisitorAccounts->cbGender;
	lstrcpyn(VisitorAccounts.szNickName,pVisitorAccounts->szNickName,CountArray(VisitorAccounts.szNickName));
	VisitorAccounts.dwSpreaderID = pVisitorAccounts->dwSpreaderID;	//mChen edit,lstrcpyn(VisitorAccounts.szSpreader,pVisitorAccounts->szSpreader,CountArray(VisitorAccounts.szSpreader));
	lstrcpyn(VisitorAccounts.szPassWord,pVisitorAccounts->szPassWord,CountArray(VisitorAccounts.szPassWord));
	lstrcpyn(VisitorAccounts.szPassWordBank,pVisitorAccounts->szPassWordBank,CountArray(VisitorAccounts.szPassWordBank));

	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_VISITOR_ACCOUNTS,dwSocketID,&VisitorAccounts,sizeof(VisitorAccounts));
	return true;
}

//I D 登录
bool CAttemperEngineSink::OnTCPNetworkSubMBLogonGameID(VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//效验参数
	ASSERT(wDataSize>=sizeof(CMD_MB_LogonGameID));
	if (wDataSize<sizeof(CMD_MB_LogonGameID)) return false;

	//变量定义
	WORD wBindIndex=LOWORD(dwSocketID);
	tagBindParameter * pBindParameter=(m_pBindParameter+wBindIndex);

	//处理消息
	CMD_MB_LogonGameID * pLogonGameID=(CMD_MB_LogonGameID *)pData;
	pLogonGameID->szPassword[CountArray(pLogonGameID->szPassword)-1]=0;
	pLogonGameID->szMachineID[CountArray(pLogonGameID->szMachineID)-1]=0;
	pLogonGameID->szMobilePhone[CountArray(pLogonGameID->szMobilePhone)-1]=0;

	//设置连接
	pBindParameter->cbClientKind=CLIENT_KIND_MOBILE;
	pBindParameter->wModuleID=pLogonGameID->wModuleID;
	pBindParameter->dwPlazaVersion=pLogonGameID->dwPlazaVersion;

	//效验版本
	if (CheckPlazaVersion(pLogonGameID->cbDeviceType,pLogonGameID->dwPlazaVersion,dwSocketID)==false) return true;

	//变量定义
	DBR_MB_LogonGameID LogonGameID;
	ZeroMemory(&LogonGameID,sizeof(LogonGameID));

	//附加信息
	LogonGameID.pBindParameter=(m_pBindParameter+LOWORD(dwSocketID));

	//构造数据
	LogonGameID.dwGameID=pLogonGameID->dwGameID;
	LogonGameID.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
	lstrcpyn(LogonGameID.szPassword,pLogonGameID->szPassword,CountArray(LogonGameID.szPassword));
	lstrcpyn(LogonGameID.szMachineID,pLogonGameID->szMachineID,CountArray(LogonGameID.szMachineID));
	lstrcpyn(LogonGameID.szMobilePhone,pLogonGameID->szMobilePhone,CountArray(LogonGameID.szMobilePhone));

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_MB_LOGON_GAMEID,dwSocketID,&LogonGameID,sizeof(LogonGameID));

	return true;
}

//帐号登录
bool CAttemperEngineSink::OnTCPNetworkSubMBLogonAccounts(VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//效验参数
	ASSERT(wDataSize>=sizeof(CMD_MB_LogonAccounts));
	if (wDataSize<sizeof(CMD_MB_LogonAccounts)) return false;

	//变量定义
	WORD wBindIndex=LOWORD(dwSocketID);
	tagBindParameter * pBindParameter=(m_pBindParameter+wBindIndex);

	//处理消息
	CMD_MB_LogonAccounts * pLogonAccounts=(CMD_MB_LogonAccounts *)pData;
	pLogonAccounts->szAccounts[CountArray(pLogonAccounts->szAccounts)-1]=0;
	pLogonAccounts->szPassword[CountArray(pLogonAccounts->szPassword)-1]=0;
	pLogonAccounts->szMachineID[CountArray(pLogonAccounts->szMachineID)-1]=0;
	pLogonAccounts->szMobilePhone[CountArray(pLogonAccounts->szMobilePhone)-1]=0;

	//设置连接
	pBindParameter->cbClientKind=CLIENT_KIND_MOBILE;
	pBindParameter->wModuleID=pLogonAccounts->wModuleID;
	pBindParameter->dwPlazaVersion=pLogonAccounts->dwPlazaVersion;

	//版本判断
	if (CheckPlazaVersion(pLogonAccounts->cbDeviceType,pLogonAccounts->dwPlazaVersion,dwSocketID)==false) return true;

	//变量定义
	DBR_MB_LogonAccounts LogonAccounts;
	ZeroMemory(&LogonAccounts,sizeof(LogonAccounts));

	//附加信息
	LogonAccounts.pBindParameter=(m_pBindParameter+LOWORD(dwSocketID));

	//构造数据
	LogonAccounts.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
	lstrcpyn(LogonAccounts.szAccounts,pLogonAccounts->szAccounts,CountArray(LogonAccounts.szAccounts));
	lstrcpyn(LogonAccounts.szPassword,pLogonAccounts->szPassword,CountArray(LogonAccounts.szPassword));
	lstrcpyn(LogonAccounts.szMachineID,pLogonAccounts->szMachineID,CountArray(LogonAccounts.szMachineID));
	lstrcpyn(LogonAccounts.szMobilePhone,pLogonAccounts->szMobilePhone,CountArray(LogonAccounts.szMobilePhone));

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_MB_LOGON_ACCOUNTS,dwSocketID,&LogonAccounts,sizeof(LogonAccounts));

	return true;
}

//帐号注册
bool CAttemperEngineSink::OnTCPNetworkSubMBRegisterAccounts(VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	//效验参数
	ASSERT(wDataSize>=sizeof(CMD_MB_RegisterAccounts));
	if (wDataSize<sizeof(CMD_MB_RegisterAccounts)) return false;

	//变量定义
	WORD wBindIndex=LOWORD(dwSocketID);
	tagBindParameter * pBindParameter=(m_pBindParameter+wBindIndex);

	//处理消息
	CMD_MB_RegisterAccounts * pRegisterAccounts=(CMD_MB_RegisterAccounts *)pData;
	pRegisterAccounts->szAccounts[CountArray(pRegisterAccounts->szAccounts)-1]=0;
	pRegisterAccounts->szNickName[CountArray(pRegisterAccounts->szNickName)-1]=0;
	pRegisterAccounts->szMachineID[CountArray(pRegisterAccounts->szMachineID)-1]=0;
	pRegisterAccounts->szLogonPass[CountArray(pRegisterAccounts->szLogonPass)-1]=0;
	pRegisterAccounts->szInsurePass[CountArray(pRegisterAccounts->szInsurePass)-1]=0;
	pRegisterAccounts->szMobilePhone[CountArray(pRegisterAccounts->szMobilePhone)-1]=0;

	//设置连接
	pBindParameter->cbClientKind=CLIENT_KIND_MOBILE;
	pBindParameter->wModuleID=pRegisterAccounts->wModuleID;
	pBindParameter->dwPlazaVersion=pRegisterAccounts->dwPlazaVersion;

	//效验版本
	if (CheckPlazaVersion(pRegisterAccounts->cbDeviceType,pRegisterAccounts->dwPlazaVersion,dwSocketID)==false) return true;

	//变量定义
	DBR_MB_RegisterAccounts RegisterAccounts;
	ZeroMemory(&RegisterAccounts,sizeof(RegisterAccounts));

	//附加信息
	RegisterAccounts.pBindParameter=(m_pBindParameter+LOWORD(dwSocketID));

	//构造数据
	RegisterAccounts.wFaceID=pRegisterAccounts->wFaceID;
	RegisterAccounts.cbGender=pRegisterAccounts->cbGender;
	RegisterAccounts.dwClientAddr=(m_pBindParameter+LOWORD(dwSocketID))->dwClientAddr;
	lstrcpyn(RegisterAccounts.szAccounts,pRegisterAccounts->szAccounts,CountArray(RegisterAccounts.szAccounts));
	lstrcpyn(RegisterAccounts.szNickName,pRegisterAccounts->szNickName,CountArray(RegisterAccounts.szNickName));
	lstrcpyn(RegisterAccounts.szMachineID,pRegisterAccounts->szMachineID,CountArray(RegisterAccounts.szMachineID));
	lstrcpyn(RegisterAccounts.szLogonPass,pRegisterAccounts->szLogonPass,CountArray(RegisterAccounts.szLogonPass));
	lstrcpyn(RegisterAccounts.szInsurePass,pRegisterAccounts->szInsurePass,CountArray(RegisterAccounts.szInsurePass));
	lstrcpyn(RegisterAccounts.szMobilePhone,pRegisterAccounts->szMobilePhone,CountArray(RegisterAccounts.szMobilePhone));

	//投递请求
	m_pIDataBaseEngine->PostDataBaseRequest(DBR_MB_REGISTER_ACCOUNTS,dwSocketID,&RegisterAccounts,sizeof(RegisterAccounts));

	return true;
}

//登录成功
bool CAttemperEngineSink::OnDBPCLogonSuccess(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	BYTE cbDataBuffer[SOCKET_TCP_PACKET];
	DBO_GP_LogonSuccess * pDBOLogonSuccess=(DBO_GP_LogonSuccess *)pData;
	CMD_GP_LogonSuccess * pCMDLogonSuccess=(CMD_GP_LogonSuccess *)cbDataBuffer;

	//发送定义
	WORD wHeadSize=sizeof(CMD_GP_LogonSuccess);
	CSendPacketHelper SendPacket(cbDataBuffer+wHeadSize,sizeof(cbDataBuffer)-wHeadSize);

	//设置变量
	ZeroMemory(pCMDLogonSuccess,sizeof(CMD_GP_LogonSuccess));

	//构造数据
	pCMDLogonSuccess->wFaceID=pDBOLogonSuccess->wFaceID;
	pCMDLogonSuccess->cbGender=pDBOLogonSuccess->cbGender;
	pCMDLogonSuccess->dwGameID=pDBOLogonSuccess->dwGameID;
	pCMDLogonSuccess->dwUserID=pDBOLogonSuccess->dwUserID;
	pCMDLogonSuccess->dwCustomID=pDBOLogonSuccess->dwCustomID;
	pCMDLogonSuccess->dwUserMedal=pDBOLogonSuccess->dwUserMedal;
	pCMDLogonSuccess->dwExperience=pDBOLogonSuccess->dwExperience;
	pCMDLogonSuccess->dwLoveLiness=pDBOLogonSuccess->dwLoveLiness;
	pCMDLogonSuccess->dwSpreaderID=pDBOLogonSuccess->dwSpreaderID;
	pCMDLogonSuccess->cbMoorMachine=pDBOLogonSuccess->cbMoorMachine;
	pCMDLogonSuccess->cbInsureEnabled=pDBOLogonSuccess->cbInsureEnabled;
	lstrcpyn(pCMDLogonSuccess->szAccounts,pDBOLogonSuccess->szAccounts,CountArray(pCMDLogonSuccess->szAccounts));
	lstrcpyn(pCMDLogonSuccess->szNickName,pDBOLogonSuccess->szNickName,CountArray(pCMDLogonSuccess->szNickName));

	//mChen add, for Match Time
	pCMDLogonSuccess->MatchStartTime = pDBOLogonSuccess->MatchStartTime;
	pCMDLogonSuccess->MatchEndTime = pDBOLogonSuccess->MatchEndTime;

	//mChen add, for签到
	pCMDLogonSuccess->wSeriesDate = pDBOLogonSuccess->wSeriesDate;

	//mChen add, 已打场次,for抽奖
	pCMDLogonSuccess->dwPlayCount = pDBOLogonSuccess->dwPlayCount;

	//mChen add,抽奖记录
	pCMDLogonSuccess->dwRaffleCount = pDBOLogonSuccess->dwRaffleCount;
	pCMDLogonSuccess->dwPlayCountPerRaffle = pDBOLogonSuccess->dwPlayCountPerRaffle;

	//mChen add, 代理
	pCMDLogonSuccess->iSpreaderLevel = pDBOLogonSuccess->iSpreaderLevel;	// -1:不是代理人

	//mChen add, for HideSeek:查询警察模型库
	pCMDLogonSuccess->lModelIndex0 = pDBOLogonSuccess->lModelIndex0;

	//苹果内购（1），微信支付切换（0）
	pCMDLogonSuccess->cbGPIsForAppleReview = 1;

	//mChen add,公告信息
	lstrcpyn(pCMDLogonSuccess->szPublicNotice, pDBOLogonSuccess->szPublicNotice, CountArray(pCMDLogonSuccess->szPublicNotice));

	//WQ 头像Http
	lstrcpyn(pCMDLogonSuccess->szHeadHttp, pDBOLogonSuccess->szHeadHttp, CountArray(pCMDLogonSuccess->szHeadHttp));

	//用户成绩
	pCMDLogonSuccess->lUserScore=pDBOLogonSuccess->lUserScore;
	pCMDLogonSuccess->lUserInsure=pDBOLogonSuccess->lUserInsure;

	//配置信息
	pCMDLogonSuccess->cbShowServerStatus=m_bShowServerStatus?1:0;

	//会员信息
	if (pDBOLogonSuccess->cbMemberOrder!=0)
	{
		DTP_GP_MemberInfo MemberInfo;
		ZeroMemory(&MemberInfo,sizeof(MemberInfo));
		MemberInfo.cbMemberOrder=pDBOLogonSuccess->cbMemberOrder;
		MemberInfo.MemberOverDate=pDBOLogonSuccess->MemberOverDate;
		SendPacket.AddPacket(&MemberInfo,sizeof(MemberInfo),DTP_GP_MEMBER_INFO);
	}

	//个性签名
	if (pDBOLogonSuccess->szUnderWrite[0]!=0)
	{
		SendPacket.AddPacket(pDBOLogonSuccess->szUnderWrite,CountStringBuffer(pDBOLogonSuccess->szUnderWrite),DTP_GP_UNDER_WRITE);
	}

	//登录成功
	DWORD wSendSize=SendPacket.GetDataSize()+sizeof(CMD_GP_LogonSuccess);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_LOGON,SUB_GP_LOGON_SUCCESS,cbDataBuffer,wSendSize);

	//发送列表
	if (m_pInitParameter->m_cbDelayList==TRUE)
	{
		//发送列表
		SendGameTypeInfo(dwContextID);
		SendGameKindInfo(dwContextID);
		SendGamePageInfo(dwContextID,INVALID_WORD);
		SendGameNodeInfo(dwContextID,INVALID_WORD);
		SendGameServerInfo(dwContextID,INVALID_WORD);
		m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_SERVER_LIST,SUB_GP_LIST_FINISH);
	}
	else
	{
		SendGameTypeInfo(dwContextID);
		SendGameKindInfo(dwContextID);
		SendGamePageInfo(dwContextID,0);
		SendGameServerInfo(dwContextID,INVALID_WORD);

		m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_SERVER_LIST,SUB_GP_LIST_FINISH);
	}
	//mChen add, for HideSeek
	//发送大厅列表
	SendGameLobbyInfo(dwContextID);

	//报名列表
	SendUserSignupInfo(dwContextID,pDBOLogonSuccess->wSignupCount,pDBOLogonSuccess->SignupMatchInfo);

	//登录完成
	CMD_GP_LogonFinish LogonFinish;
	ZeroMemory(&LogonFinish,sizeof(LogonFinish));
	LogonFinish.wIntermitTime=m_pInitParameter->m_wIntermitTime;
	LogonFinish.wOnLineCountTime=m_pInitParameter->m_wOnLineCountTime;
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_LOGON,SUB_GP_LOGON_FINISH,&LogonFinish,sizeof(LogonFinish));

	return true;
}

//发送报名
VOID CAttemperEngineSink::SendUserSignupInfo(DWORD dwSocketID,WORD wSignupCount,tagSignupMatchInfo * pSignupMatchInfo)
{
	//参数校验
	if(wSignupCount==0 || pSignupMatchInfo==NULL) return;

	//网络数据
	DWORD wSendSize=0;
	BYTE cbDataBuffer[SOCKET_TCP_PACKET];

	//发送数据	
	while((wSignupCount*sizeof(tagSignupMatchInfo))>=sizeof(cbDataBuffer))
	{
		//拷贝数据
		wSendSize = (sizeof(cbDataBuffer)/sizeof(tagSignupMatchInfo))*sizeof(tagSignupMatchInfo);
		CopyMemory(cbDataBuffer,pSignupMatchInfo,wSendSize);

		//发送数据
		m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_LOGON,SUB_GP_MATCH_SIGNUPINFO,cbDataBuffer,wSendSize);

		//调整指针
		pSignupMatchInfo += sizeof(cbDataBuffer)/sizeof(tagSignupMatchInfo);
		wSignupCount -= sizeof(cbDataBuffer)/sizeof(tagSignupMatchInfo);		
	}

	//剩余发送
	if(wSignupCount>0)
	{
		//发送数据
		wSendSize = wSignupCount*sizeof(tagSignupMatchInfo);
		CopyMemory(cbDataBuffer,pSignupMatchInfo,wSendSize);
		m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_LOGON,SUB_GP_MATCH_SIGNUPINFO,cbDataBuffer,wSendSize);
	}

	return;
}
//登录失败
bool CAttemperEngineSink::OnDBPCLogonFailure(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	CMD_GP_LogonFailure LogonFailure;
	ZeroMemory(&LogonFailure,sizeof(LogonFailure));
	DBO_GP_LogonFailure * pLogonFailure=(DBO_GP_LogonFailure *)pData;

	//构造数据
	LogonFailure.lResultCode=pLogonFailure->lResultCode;
	lstrcpyn(LogonFailure.szDescribeString,pLogonFailure->szDescribeString,CountArray(LogonFailure.szDescribeString));

	//发送数据
	WORD wStringSize=CountStringBuffer(LogonFailure.szDescribeString);
	DWORD wSendSize=sizeof(LogonFailure)-sizeof(LogonFailure.szDescribeString)+wStringSize;
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_LOGON,SUB_GP_LOGON_FAILURE,&LogonFailure,wSendSize);

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

//登录失败
bool CAttemperEngineSink::OnDBPCLogonValidateMBCard(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//效验参数
	ASSERT(wDataSize==sizeof(DBR_GP_ValidateMBCard));
	if (wDataSize!=sizeof(DBR_GP_ValidateMBCard)) return false;

	DBR_GP_ValidateMBCard *pValidateMBCard=(DBR_GP_ValidateMBCard *)pData;

	//发送消息
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_LOGON,SUB_GP_VALIDATE_MBCARD,pData,wDataSize);

	return true;
}

//用户头像
bool CAttemperEngineSink::OnDBPCUserFaceInfo(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	CMD_GP_UserFaceInfo UserFaceInfo;
	ZeroMemory(&UserFaceInfo,sizeof(UserFaceInfo));
	DBO_GP_UserFaceInfo * pUserFaceInfo=(DBO_GP_UserFaceInfo *)pData;

	//设置变量
	UserFaceInfo.wFaceID=pUserFaceInfo->wFaceID;
	UserFaceInfo.dwCustomID=pUserFaceInfo->dwCustomID;

	//发送消息
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_USER_FACE_INFO,&UserFaceInfo,sizeof(UserFaceInfo));

	return true;
}

//用户信息
bool CAttemperEngineSink::OnDBPCUserIndividual(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	BYTE cbDataBuffer[SOCKET_TCP_PACKET];
	DBO_GP_UserIndividual * pDBOUserIndividual=(DBO_GP_UserIndividual *)pData;
	CMD_GP_UserIndividual * pCMDUserIndividual=(CMD_GP_UserIndividual *)cbDataBuffer;
	CSendPacketHelper SendPacket(cbDataBuffer+sizeof(CMD_GP_UserIndividual),sizeof(cbDataBuffer)-sizeof(CMD_GP_UserIndividual));

	//设置变量
	ZeroMemory(pCMDUserIndividual,sizeof(CMD_GP_UserIndividual));

	//构造数据
	pCMDUserIndividual->dwUserID=pDBOUserIndividual->dwUserID;

	//用户信息
	if (pDBOUserIndividual->szUserNote[0]!=0)
	{
		WORD wBufferSize=CountStringBuffer(pDBOUserIndividual->szUserNote);
		SendPacket.AddPacket(pDBOUserIndividual->szUserNote,wBufferSize,DTP_GP_UI_USER_NOTE);
	}

	//真实姓名
	if (pDBOUserIndividual->szCompellation[0]!=0)
	{
		WORD wBufferSize=CountStringBuffer(pDBOUserIndividual->szCompellation);
		SendPacket.AddPacket(pDBOUserIndividual->szCompellation,wBufferSize,DTP_GP_UI_COMPELLATION);
	}

	//电话号码
	if (pDBOUserIndividual->szSeatPhone[0]!=0)
	{
		WORD wBufferSize=CountStringBuffer(pDBOUserIndividual->szSeatPhone);
		SendPacket.AddPacket(pDBOUserIndividual->szSeatPhone,wBufferSize,DTP_GP_UI_SEAT_PHONE);
	}

	//移动电话
	if (pDBOUserIndividual->szMobilePhone[0]!=0)
	{
		WORD wBufferSize=CountStringBuffer(pDBOUserIndividual->szMobilePhone);
		SendPacket.AddPacket(pDBOUserIndividual->szMobilePhone,wBufferSize,DTP_GP_UI_MOBILE_PHONE);
	}

	//联系资料
	if (pDBOUserIndividual->szQQ[0]!=0) 
	{
		WORD wBufferSize=CountStringBuffer(pDBOUserIndividual->szQQ);
		SendPacket.AddPacket(pDBOUserIndividual->szQQ,wBufferSize,DTP_GP_UI_QQ);
	}

	//电子邮件
	if (pDBOUserIndividual->szEMail[0]!=0) 
	{
		WORD wBufferSize=CountStringBuffer(pDBOUserIndividual->szEMail);
		SendPacket.AddPacket(pDBOUserIndividual->szEMail,wBufferSize,DTP_GP_UI_EMAIL);
	}

	//联系地址
	if (pDBOUserIndividual->szDwellingPlace[0]!=0)
	{
		WORD wBufferSize=CountStringBuffer(pDBOUserIndividual->szDwellingPlace);
		SendPacket.AddPacket(pDBOUserIndividual->szDwellingPlace,wBufferSize,DTP_GP_UI_DWELLING_PLACE);
	}
	//头像http
	if (pDBOUserIndividual->szHeadHttp[0]!=0)
	{
		WORD wBufferSize=CountStringBuffer(pDBOUserIndividual->szHeadHttp);
		SendPacket.AddPacket(pDBOUserIndividual->szHeadHttp,wBufferSize,DTP_GP_UI_HEAD_HTTP);
	}
	//IP
	if (pDBOUserIndividual->szLogonIP[0]!=0)
	{
		WORD wBufferSize=CountStringBuffer(pDBOUserIndividual->szLogonIP);
		SendPacket.AddPacket(pDBOUserIndividual->szLogonIP,wBufferSize,DTP_GP_UI_IP);
	}
	//渠道号
	if (pDBOUserIndividual->szUserChannel[0]!=0)
	{
		WORD wBufferSize=CountStringBuffer(pDBOUserIndividual->szUserChannel);
		SendPacket.AddPacket(pDBOUserIndividual->szUserChannel,wBufferSize,DTP_GP_UI_CHANNEL);
	}
	
	//发送消息
	DWORD wSendSize=sizeof(CMD_GP_UserIndividual)+SendPacket.GetDataSize();
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_USER_INDIVIDUAL,cbDataBuffer,wSendSize);

	return true;
}
//用户个人信息
bool CAttemperEngineSink::OnDBPCUserAccountInfo(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	DBO_GP_UserAccountInfo * pDBOUserIndividual=(DBO_GP_UserAccountInfo *)pData;
	CMD_GP_UserAccountInfo kCMDUserIndividual;
	CopyMemory(&kCMDUserIndividual,pDBOUserIndividual,sizeof(kCMDUserIndividual));
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_QUERY_ACCOUNTINFO,&kCMDUserIndividual,sizeof(kCMDUserIndividual));
	return true;
}
//银行信息
bool CAttemperEngineSink::OnDBPCUserInsureInfo(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	DBO_GP_UserInsureInfo * pUserInsureInfo=(DBO_GP_UserInsureInfo *)pData;

	//变量定义
	CMD_GP_UserInsureInfo UserInsureInfo;
	ZeroMemory(&UserInsureInfo,sizeof(UserInsureInfo));

	//构造数据
	UserInsureInfo.wRevenueTake=pUserInsureInfo->wRevenueTake;
	UserInsureInfo.wRevenueTransfer=pUserInsureInfo->wRevenueTransfer;
	UserInsureInfo.wServerID=pUserInsureInfo->wServerID;
	UserInsureInfo.lUserScore=pUserInsureInfo->lUserScore;
	UserInsureInfo.lUserInsure=pUserInsureInfo->lUserInsure;
	UserInsureInfo.lTransferPrerequisite=pUserInsureInfo->lTransferPrerequisite;

	//发送数据
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_USER_INSURE_INFO,&UserInsureInfo,sizeof(UserInsureInfo));

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

//银行成功
bool CAttemperEngineSink::OnDBPCUserInsureSuccess(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	DBO_GP_UserInsureSuccess * pUserInsureSuccess=(DBO_GP_UserInsureSuccess *)pData;

	//变量定义
	CMD_GP_UserInsureSuccess UserInsureSuccess;
	ZeroMemory(&UserInsureSuccess,sizeof(UserInsureSuccess));

	//设置变量
	UserInsureSuccess.dwUserID=pUserInsureSuccess->dwUserID;
	UserInsureSuccess.lUserScore=pUserInsureSuccess->lDestScore;
	UserInsureSuccess.lUserInsure=pUserInsureSuccess->lDestInsure;
	lstrcpyn(UserInsureSuccess.szDescribeString,pUserInsureSuccess->szDescribeString,CountArray(UserInsureSuccess.szDescribeString));

	//发送消息
	WORD wDescribe=CountStringBuffer(UserInsureSuccess.szDescribeString);
	WORD wHeadSize=sizeof(UserInsureSuccess)-sizeof(UserInsureSuccess.szDescribeString);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_USER_INSURE_SUCCESS,&UserInsureSuccess,wHeadSize+wDescribe);

	return true;
}

//操作失败
bool CAttemperEngineSink::OnDBPCUserInsureFailure(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	CMD_GP_UserInsureFailure UserInsureFailure;
	ZeroMemory(&UserInsureFailure,sizeof(UserInsureFailure));

	//变量定义
	DBO_GP_UserInsureFailure * pUserInsureFailure=(DBO_GP_UserInsureFailure *)pData;

	//构造数据
	UserInsureFailure.lResultCode=pUserInsureFailure->lResultCode;
	lstrcpyn(UserInsureFailure.szDescribeString,pUserInsureFailure->szDescribeString,CountArray(UserInsureFailure.szDescribeString));

	//发送数据
	WORD wDescribe=CountStringBuffer(UserInsureFailure.szDescribeString);
	WORD wHeadSize=sizeof(UserInsureFailure)-sizeof(UserInsureFailure.szDescribeString);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_USER_INSURE_FAILURE,&UserInsureFailure,wHeadSize+wDescribe);

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

//用户信息
bool CAttemperEngineSink::OnDBPCUserInsureUserInfo(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	DBO_GP_UserTransferUserInfo * pTransferUserInfo=(DBO_GP_UserTransferUserInfo *)pData;

	//变量定义
	CMD_GP_UserTransferUserInfo UserTransferUserInfo;
	ZeroMemory(&UserTransferUserInfo,sizeof(UserTransferUserInfo));

	//构造变量
	UserTransferUserInfo.dwTargetGameID=pTransferUserInfo->dwGameID;
	lstrcpyn(UserTransferUserInfo.szNickName,pTransferUserInfo->szNickName,CountArray(UserTransferUserInfo.szNickName));

	//发送数据
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_QUERY_USER_INFO_RESULT,&UserTransferUserInfo,sizeof(UserTransferUserInfo));

	return true;
}
//自定义数据查询
bool CAttemperEngineSink::OnDBPCPublicNotice(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	CMD_GP_PublicNotice kCMDOutInfo;
	ZeroMemory(&kCMDOutInfo,sizeof(kCMDOutInfo));

	//变量定义
	DBO_GP_PublicNotice * pDBOutInfo=(DBO_GP_PublicNotice *)pData;

	//构造数据
	kCMDOutInfo.lResultCode=pDBOutInfo->lResultCode;
	lstrcpyn(kCMDOutInfo.szDescribeString,pDBOutInfo->szDescribeString,CountArray(kCMDOutInfo.szDescribeString));

	//发送数据
	WORD wDescribe=CountStringBuffer(kCMDOutInfo.szDescribeString);
	WORD wHeadSize=sizeof(kCMDOutInfo)-sizeof(kCMDOutInfo.szDescribeString);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_PUBLIC_NOTICE,&kCMDOutInfo,wHeadSize+wDescribe);

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);
	return true;
}
bool CAttemperEngineSink::OnDBPCInGameSevrerID(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	DBO_GP_UserInGameServerID * pDBInfo=(DBO_GP_UserInGameServerID *)pData;
	//变量定义
	CMD_GP_InGameSeverID kNetInfo;
	ZeroMemory(&kNetInfo,sizeof(kNetInfo));

	kNetInfo.LockKindID=pDBInfo->LockKindID;
	kNetInfo.LockServerID=pDBInfo->LockServerID;

	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_QUERY_INGAME_SEVERID,&kNetInfo,sizeof(kNetInfo));

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

bool CAttemperEngineSink::OnDBPCOSpreaderResoult(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	DBO_GP_SpreaderResoult * pDBInfo=(DBO_GP_SpreaderResoult *)pData;
	//变量定义
	CMD_GP_SpreaderResoult SpreaderResoult;
	ZeroMemory(&SpreaderResoult,sizeof(SpreaderResoult));

	//mChen add, 
	SpreaderResoult.cbOperateType = pDBInfo->cbOperateType;// 操作类型：0-绑定代理，1-增加代理，2-删除代理
	SpreaderResoult.dwBindSpreaderId = pDBInfo->dwBindSpreaderId;

	SpreaderResoult.lResultCode=pDBInfo->lResultCode;
	SpreaderResoult.lScore=pDBInfo->lScore;
	lstrcpyn(SpreaderResoult.szDescribeString,pDBInfo->szDescribeString,CountArray(SpreaderResoult.szDescribeString));

	WORD wDescribe=CountStringBuffer(SpreaderResoult.szDescribeString);
	WORD wHeadSize=sizeof(SpreaderResoult)-sizeof(SpreaderResoult.szDescribeString);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_SPREADER_RESOULT,&SpreaderResoult,wHeadSize+wDescribe);

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}


//mChen add
//查询代理人列表结果
bool CAttemperEngineSink::OnDBPCOSpreadersInfoResoult(DWORD dwContextID, VOID * pData, DWORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID) < m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter + LOWORD(dwContextID))->dwSocketID != dwContextID) return true;

	DBO_GP_SpreadersInfoResoult * pDBInfo = (DBO_GP_SpreadersInfoResoult *)pData;
	//变量定义
	CMD_GP_SpreadersInfoResoult SpreaderInfoResoult;
	ZeroMemory(&SpreaderInfoResoult, sizeof(SpreaderInfoResoult));

	SpreaderInfoResoult.lResultCode = pDBInfo->lResultCode;
	lstrcpyn(SpreaderInfoResoult.szDescribeString, pDBInfo->szDescribeString, CountArray(SpreaderInfoResoult.szDescribeString));

	SpreaderInfoResoult.wPacketIdx = pDBInfo->wPacketIdx;
	SpreaderInfoResoult.wPacketNum = pDBInfo->wPacketNum;

	SpreaderInfoResoult.wItemCount = pDBInfo->wItemCount;
	DWORD wItemDataSize = sizeof(SpreaderInfoItem)*SpreaderInfoResoult.wItemCount;
	CopyMemory(SpreaderInfoResoult.SpreaderInfoItems, pDBInfo->SpreaderInfoItems, wItemDataSize);

	DWORD wHeadSize = sizeof(SpreaderInfoResoult) - sizeof(SpreaderInfoResoult.SpreaderInfoItems);
	m_pITCPNetworkEngine->SendData(dwContextID, MDM_GP_USER_SERVICE, SUB_GP_SPREADERS_INFO_RESOULT, &SpreaderInfoResoult, wHeadSize + wItemDataSize);

	if (SpreaderInfoResoult.wPacketIdx >= SpreaderInfoResoult.wPacketNum - 1)
	{
		//关闭连接
		m_pITCPNetworkEngine->ShutDownSocket(dwContextID);
	}

	return true;
}

std::wstring StringToWString(const std::string& s)
{
	_bstr_t t = s.c_str();
	wchar_t* pwchar = (wchar_t*)t;
	std::wstring result = pwchar;
	return result;
}

//mChen add
//企业提现
bool CAttemperEngineSink::OnDBPCOAddEnterprisePaymentResult(DWORD dwContextID, VOID * pData, DWORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID) < m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter + LOWORD(dwContextID))->dwSocketID != dwContextID) return true;

	DBO_GP_AddEnterprisePaymentResoult * pDBInfo = (DBO_GP_AddEnterprisePaymentResoult *)pData;

	//变量定义
	CMD_GP_AddEnterprisePaymentResult AddEnterprisePaymentResult;
	ZeroMemory(&AddEnterprisePaymentResult, sizeof(AddEnterprisePaymentResult));

	AddEnterprisePaymentResult.dwUserID = pDBInfo->dwUserID;
	lstrcpyn(AddEnterprisePaymentResult.szRealName, pDBInfo->szRealName, CountArray(AddEnterprisePaymentResult.szRealName));
	AddEnterprisePaymentResult.szRealName[CountArray(AddEnterprisePaymentResult.szRealName) - 1] = 0;

	lstrcpyn(AddEnterprisePaymentResult.szOpenid, pDBInfo->szOpenid, CountArray(AddEnterprisePaymentResult.szOpenid));
	AddEnterprisePaymentResult.lResultCode = pDBInfo->lResultCode;
	AddEnterprisePaymentResult.dwEnterprisePayment = pDBInfo->dwEnterprisePayment;
	AddEnterprisePaymentResult.dwTotalLeftCash = pDBInfo->dwTotalLeftCash;
	lstrcpyn(AddEnterprisePaymentResult.szDescribeString, pDBInfo->szDescribeString, CountArray(AddEnterprisePaymentResult.szDescribeString));

	if (AddEnterprisePaymentResult.lResultCode == DB_SUCCESS)
	{
		auto pSend	//std::function<void(int, void*, void*, DWORD, bool, std::string&, std::string&, std::string&, std::string&, std::string&)> pSend
			= [](int userID, void*netEngine, void*dataBaseEngine, DWORD dwSocketID, bool bSuccess, std::string& resultMsg, std::string& amount, std::string& nonceStr, std::string& partnerTradeNo, std::string& paymentNo, std::string& paymentTime)
		{
			CMD_GP_AddEnterprisePaymentResult AddEnterprisePaymentResult;
			ZeroMemory(&AddEnterprisePaymentResult, sizeof(AddEnterprisePaymentResult));
			
			AddEnterprisePaymentResult.dwUserID = userID;
			AddEnterprisePaymentResult.lResultCode = (bSuccess ? DB_SUCCESS : DB_ERROR);

			if (bSuccess)
			{
				AddEnterprisePaymentResult.dwEnterprisePayment = atoi(amount.c_str());
				if (resultMsg.empty())
				{
					resultMsg = TEXT("企业提现成功！");
				}
				lstrcpyn(AddEnterprisePaymentResult.szDescribeString, resultMsg.c_str(), CountArray(AddEnterprisePaymentResult.szDescribeString));

				//prePayInfo.cbStatusCode = 1;
				//memcpy(prePayInfo.szPrePayID, prePayStr.c_str(), prePayStr.length());
				//memcpy(prePayInfo.szNonceStr, nonceStr.c_str(), nonceStr.length());
				//memcpy(prePayInfo.szTimeStamp, timeStampStr.c_str(), timeStampStr.length());
				//memcpy(prePayInfo.szSign, signStr.c_str(), signStr.length());
				//memcpy(prePayInfo.szTradeNo, tradeNo.c_str(), tradeNo.length());

				//投递请求
				((IDataBaseEngine *)dataBaseEngine)->PostDataBaseRequest(DBR_GP_ADD_ENTERPRISE_PAYMENT_RESULT, dwSocketID, &AddEnterprisePaymentResult, sizeof(CMD_GP_AddEnterprisePaymentResult));

			}
			else
			{
				if (resultMsg.empty())
				{
					resultMsg = TEXT("企业提现失败！");
				}
				lstrcpyn(AddEnterprisePaymentResult.szDescribeString, resultMsg.c_str(), CountArray(AddEnterprisePaymentResult.szDescribeString));
			}
			
			WORD wDescribe = CountStringBuffer(AddEnterprisePaymentResult.szDescribeString);
			WORD wHeadSize = sizeof(AddEnterprisePaymentResult) - sizeof(AddEnterprisePaymentResult.szDescribeString);
			((ITCPNetworkEngine*)netEngine)->SendData(dwSocketID, MDM_GP_USER_SERVICE, SUB_GP_ADD_ENTERPRISE_PAYMENT_RESULT, &AddEnterprisePaymentResult, wHeadSize + wDescribe);

		};

		std::string strRealName(AddEnterprisePaymentResult.szRealName);
		std::wstring wstrRealName = StringToWString(strRealName);
		std::string strOpenid(AddEnterprisePaymentResult.szOpenid);
		std::size_t pos = strOpenid.find("Wt");
		if (pos == 0)//!= std::string::npos
		{
			strOpenid.erase(0,2);
		}
		std::wstring wstrOpenid = StringToWString(strOpenid);
		WeChatPayHttpUnits::enterprisePay(AddEnterprisePaymentResult.dwEnterprisePayment, AddEnterprisePaymentResult.dwUserID, wstrRealName, wstrOpenid, (void*)m_pITCPNetworkEngine, (void*)m_pIDataBaseEngine, dwContextID, pSend);
	}
	else
	{
		WORD wDescribe = CountStringBuffer(AddEnterprisePaymentResult.szDescribeString);
		WORD wHeadSize = sizeof(AddEnterprisePaymentResult) - sizeof(AddEnterprisePaymentResult.szDescribeString);
		m_pITCPNetworkEngine->SendData(dwContextID, MDM_GP_USER_SERVICE, SUB_GP_ADD_ENTERPRISE_PAYMENT_RESULT, &AddEnterprisePaymentResult, wHeadSize + wDescribe);

		//关闭连接
		m_pITCPNetworkEngine->ShutDownSocket(dwContextID);
	}

	return true;
}


//mChen add, for HideSeek
bool CAttemperEngineSink::OnDBPCOBoughtTaggerModelResoult(DWORD dwContextID, VOID * pData, DWORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID) < m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter + LOWORD(dwContextID))->dwSocketID != dwContextID) return true;

	DBO_GP_BoughtTaggerModelResoult * pDBInfo = (DBO_GP_BoughtTaggerModelResoult *)pData;

	//变量定义
	CMD_GP_BoughtTaggerModelResult BoughtResult;
	ZeroMemory(&BoughtResult, sizeof(BoughtResult));

	BoughtResult.lResultCode = pDBInfo->lResultCode;
	BoughtResult.lUserScore = pDBInfo->lUserScore;
	BoughtResult.lUserInsure = pDBInfo->lUserInsure;
	BoughtResult.wBoughtModelIndex = pDBInfo->wBoughtModelIndex;
	lstrcpyn(BoughtResult.szDescribeString, pDBInfo->szDescribeString, CountArray(BoughtResult.szDescribeString));

	WORD wDescribe = CountStringBuffer(BoughtResult.szDescribeString);
	WORD wHeadSize = sizeof(BoughtResult) - sizeof(BoughtResult.szDescribeString);
	m_pITCPNetworkEngine->SendData(dwContextID, MDM_GP_USER_SERVICE, SUB_GP_BOUGHT_TAGGER_MODEL_RESULT, &BoughtResult, wHeadSize + wDescribe);

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

//内购
bool CAttemperEngineSink::OnDBPCOAddPaymentResult(DWORD dwContextID, VOID * pData, DWORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID) < m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter + LOWORD(dwContextID))->dwSocketID != dwContextID) return true;

	DBO_GP_AddPaymentResoult * pDBInfo = (DBO_GP_AddPaymentResoult *)pData;

	//变量定义
	CMD_GP_AddPaymentResult AddPaymentResult;
	ZeroMemory(&AddPaymentResult, sizeof(AddPaymentResult));

	AddPaymentResult.lResultCode = pDBInfo->lResultCode;
	AddPaymentResult.dwFinalInsureScore = pDBInfo->dwFinalInsureScore;
	lstrcpyn(AddPaymentResult.szDescribeString, pDBInfo->szDescribeString, CountArray(AddPaymentResult.szDescribeString));


	WORD wDescribe = CountStringBuffer(AddPaymentResult.szDescribeString);
	WORD wHeadSize = sizeof(AddPaymentResult) - sizeof(AddPaymentResult.szDescribeString);
	m_pITCPNetworkEngine->SendData(dwContextID, MDM_GP_USER_SERVICE, SUB_GP_ADD_PAYMENT_RESULT, &AddPaymentResult, wHeadSize + wDescribe);

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}
//名下用户交易信息
bool CAttemperEngineSink::OnDBPCOChildrenPaymentInfoResult(DWORD dwContextID, VOID * pData, DWORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID) < m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter + LOWORD(dwContextID))->dwSocketID != dwContextID) return true;

	DBO_GP_ChildrenPaymentInfoResult * pDBInfo = (DBO_GP_ChildrenPaymentInfoResult *)pData;
	//变量定义
	CMD_GP_ChildrenPaymentInfoResult ChildrenPaymentInfoResult;
	ZeroMemory(&ChildrenPaymentInfoResult, sizeof(ChildrenPaymentInfoResult));

	ChildrenPaymentInfoResult.lResultCode = pDBInfo->lResultCode;
	lstrcpyn(ChildrenPaymentInfoResult.szDescribeString, pDBInfo->szDescribeString, CountArray(ChildrenPaymentInfoResult.szDescribeString));

	ChildrenPaymentInfoResult.dTotalGrantOfChildrenBuy = pDBInfo->dTotalGrantOfChildrenBuy;
	ChildrenPaymentInfoResult.dExtraCash = pDBInfo->dExtraCash;
	ChildrenPaymentInfoResult.dCashedOut = pDBInfo->dCashedOut;
	ChildrenPaymentInfoResult.dTotalLeftCash = pDBInfo->dTotalLeftCash;

	ChildrenPaymentInfoResult.wPacketIdx = pDBInfo->wPacketIdx;
	ChildrenPaymentInfoResult.wPacketNum = pDBInfo->wPacketNum;

	ChildrenPaymentInfoResult.wItemCount = pDBInfo->wItemCount;
	DWORD wItemDataSize = sizeof(SpreaderInfoItem)*ChildrenPaymentInfoResult.wItemCount;
	CopyMemory(ChildrenPaymentInfoResult.PaymentInfoItems, pDBInfo->PaymentInfoItems, wItemDataSize);

	DWORD wHeadSize = sizeof(ChildrenPaymentInfoResult) - sizeof(ChildrenPaymentInfoResult.PaymentInfoItems);
	m_pITCPNetworkEngine->SendData(dwContextID, MDM_GP_USER_SERVICE, SUB_GP_CHILDREN_PAYMENT_INFO_RESULT, &ChildrenPaymentInfoResult, wHeadSize + wItemDataSize);

	if (ChildrenPaymentInfoResult.wPacketIdx >= ChildrenPaymentInfoResult.wPacketNum - 1)
	{
		//关闭连接
		m_pITCPNetworkEngine->ShutDownSocket(dwContextID);
	}

	return true;
}

//操作成功
bool CAttemperEngineSink::OnDBPCOperateSuccess(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	CMD_GP_OperateSuccess OperateSuccess;
	ZeroMemory(&OperateSuccess,sizeof(OperateSuccess));

	//变量定义
	DBO_GP_OperateSuccess * pOperateSuccess=(DBO_GP_OperateSuccess *)pData;

	//构造数据
	OperateSuccess.lResultCode=pOperateSuccess->lResultCode;
	lstrcpyn(OperateSuccess.szDescribeString,pOperateSuccess->szDescribeString,CountArray(OperateSuccess.szDescribeString));

	//发送数据
	WORD wDescribe=CountStringBuffer(OperateSuccess.szDescribeString);
	WORD wHeadSize=sizeof(OperateSuccess)-sizeof(OperateSuccess.szDescribeString);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_OPERATE_SUCCESS,&OperateSuccess,wHeadSize+wDescribe);

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

//操作失败
bool CAttemperEngineSink::OnDBPCOperateFailure(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	CMD_GP_OperateFailure OperateFailure;
	ZeroMemory(&OperateFailure,sizeof(OperateFailure));

	//变量定义
	DBO_GP_OperateFailure * pOperateFailure=(DBO_GP_OperateFailure *)pData;

	//构造数据
	OperateFailure.lResultCode=pOperateFailure->lResultCode;
	lstrcpyn(OperateFailure.szDescribeString,pOperateFailure->szDescribeString,CountArray(OperateFailure.szDescribeString));

	//发送数据
	WORD wDescribe=CountStringBuffer(OperateFailure.szDescribeString);
	WORD wHeadSize=sizeof(OperateFailure)-sizeof(OperateFailure.szDescribeString);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_OPERATE_FAILURE,&OperateFailure,wHeadSize+wDescribe);

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

//报名结果
bool CAttemperEngineSink::OnDBPCUserMatchSignupResult(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	DBO_GP_MatchSignupResult * pMatchSignupResult=(DBO_GP_MatchSignupResult *)pData;

	//变量定义
	CMD_GP_MatchSignupResult MatchSignupResult;
	ZeroMemory(&MatchSignupResult,sizeof(MatchSignupResult));

	//构造结构
	MatchSignupResult.bSignup = pMatchSignupResult->bSignup;
	MatchSignupResult.bSuccessed = pMatchSignupResult->bSuccessed;

	//mChen add
	MatchSignupResult.MatchSignupStartTime = pMatchSignupResult->MatchSignupStartTime;
	MatchSignupResult.MatchSignupEndTime = pMatchSignupResult->MatchSignupEndTime;
	MatchSignupResult.MatchStartTime = pMatchSignupResult->MatchStartTime;
	MatchSignupResult.MatchEndTime = pMatchSignupResult->MatchEndTime;

	lstrcpyn(MatchSignupResult.szDescribeString,pMatchSignupResult->szDescribeString,CountArray(MatchSignupResult.szDescribeString));

	//发送数据
	DWORD wSendSize = sizeof(MatchSignupResult)-sizeof(MatchSignupResult.szDescribeString)+CountStringBuffer(MatchSignupResult.szDescribeString);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_MATCH_SIGNUP_RESULT,&MatchSignupResult,sizeof(MatchSignupResult));

	return true;
}
// 比赛奖励
bool CAttemperEngineSink::OnDBPCMacthAwardList( DWORD dwContextID, VOID * pData, WORD wDataSize )
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//DBO_GR_MatchAwardList
	//DBO_GR_MatchAwardList
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_MATCH_AWARD_LIST,pData,wDataSize);
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

//lin add
//查询比赛积分列表结果
bool CAttemperEngineSink::OnDBPCOTopPlayersInfoResoult(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID) < m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter + LOWORD(dwContextID))->dwSocketID != dwContextID) return true;

	DBO_GP_TopMatchPlayerInfoResult * pDBInfo = (DBO_GP_TopMatchPlayerInfoResult *)pData;
	//变量定义
	CMD_GP_TopMatchPlayerInfoResult TopPlayersInfoResoult;
	ZeroMemory(&TopPlayersInfoResoult, sizeof(TopPlayersInfoResoult));

	TopPlayersInfoResoult.lResultCode = pDBInfo->lResultCode;
	
	TopPlayersInfoResoult.wItemCount = pDBInfo->wItemCount;
	WORD wItemDataSize = sizeof(TopMatchPlayerInfoItem)*TopPlayersInfoResoult.wItemCount;
	CopyMemory(TopPlayersInfoResoult.TopInfoItems, pDBInfo->TopInfoItems, wItemDataSize);

	WORD wHeadSize = sizeof(TopPlayersInfoResoult) - sizeof(TopPlayersInfoResoult.TopInfoItems);
	m_pITCPNetworkEngine->SendData(dwContextID, MDM_GP_USER_SERVICE, SUB_GP_TOP_PLAYERS_INFO_RESOULT, &TopPlayersInfoResoult, wHeadSize + wItemDataSize);

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

// lin add
//查询昵称结果
bool CAttemperEngineSink::OnDBPCOQueryNickNameInfoResoult(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID) < m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter + LOWORD(dwContextID))->dwSocketID != dwContextID) return true;

	DBO_GP_NickNameInfoResult * pDBInfo = (DBO_GP_NickNameInfoResult *)pData;
	

	m_pITCPNetworkEngine->SendData(dwContextID, MDM_GP_USER_SERVICE, SUB_GP_NICKNAME_INFO, pDBInfo, sizeof(DBO_GP_NickNameInfoResult));

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

// lin add
//转房卡结果
bool CAttemperEngineSink::OnDBPCOTransferDiamondsResoult(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID) < m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter + LOWORD(dwContextID))->dwSocketID != dwContextID) return true;

	DBO_GP_TransferDiamondsResult * pDBInfo = (DBO_GP_TransferDiamondsResult *)pData;
	
	m_pITCPNetworkEngine->SendData(dwContextID, MDM_GP_USER_SERVICE, SUB_GP_TRANSFER_DIAMOND_RESULT, pDBInfo, sizeof(DBO_GP_TransferDiamondsResult));

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}
//商品购买结果
bool CAttemperEngineSink::OnDBPCAddShopItemResoult(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID) < m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter + LOWORD(dwContextID))->dwSocketID != dwContextID) return true;

	m_pITCPNetworkEngine->SendData(dwContextID, MDM_GP_USER_SERVICE, SUB_GP_ADDSHOPITEM_RESULT, pData, wDataSize);
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);
	return true;
}
//钻石金币兑换
bool CAttemperEngineSink::OnDBPCExchangeScoreInfoResoult(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID) < m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter + LOWORD(dwContextID))->dwSocketID != dwContextID) return true;

	m_pITCPNetworkEngine->SendData(dwContextID, MDM_GP_USER_SERVICE, SUB_GP_EXCHANGESCORE_RESULT, pData, wDataSize);
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);
	return true;
}
//登录成功
bool CAttemperEngineSink::OnDBMBLogonSuccess(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	DBO_MB_LogonSuccess * pDBOLogonSuccess=(DBO_MB_LogonSuccess *)pData;

	//变量定义
	CMD_MB_LogonSuccess LogonSuccess;
	ZeroMemory(&LogonSuccess,sizeof(LogonSuccess));

	//构造数据
	LogonSuccess.wFaceID=pDBOLogonSuccess->wFaceID;
	LogonSuccess.cbGender=pDBOLogonSuccess->cbGender;
	LogonSuccess.dwGameID=pDBOLogonSuccess->dwGameID;
	LogonSuccess.dwUserID=pDBOLogonSuccess->dwUserID;
	LogonSuccess.dwExperience=pDBOLogonSuccess->dwExperience;
	LogonSuccess.dwLoveLiness=pDBOLogonSuccess->dwLoveLiness;
	lstrcpyn(LogonSuccess.szNickName,pDBOLogonSuccess->szNickName,CountArray(LogonSuccess.szNickName));

	//登录成功
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_MB_LOGON,SUB_MB_LOGON_SUCCESS,&LogonSuccess,sizeof(LogonSuccess));

	//发送房间
	WORD wIndex=LOWORD(dwContextID);
	SendMobileKindInfo(dwContextID,(m_pBindParameter+wIndex)->wModuleID);
	SendMobileServerInfo(dwContextID,(m_pBindParameter+wIndex)->wModuleID);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_MB_SERVER_LIST,SUB_MB_LIST_FINISH);

	return true;
}

//登录失败
bool CAttemperEngineSink::OnDBMBLogonFailure(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	CMD_MB_LogonFailure LogonFailure;
	ZeroMemory(&LogonFailure,sizeof(LogonFailure));
	DBO_MB_LogonFailure * pLogonFailure=(DBO_MB_LogonFailure *)pData;

	//构造数据
	LogonFailure.lResultCode=pLogonFailure->lResultCode;
	lstrcpyn(LogonFailure.szDescribeString,pLogonFailure->szDescribeString,CountArray(LogonFailure.szDescribeString));

	//发送数据
	WORD wStringSize=CountStringBuffer(LogonFailure.szDescribeString);
	DWORD wSendSize=sizeof(LogonFailure)-sizeof(LogonFailure.szDescribeString)+wStringSize;
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_MB_LOGON,SUB_MB_LOGON_FAILURE,&LogonFailure,wSendSize);

	//关闭连接
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

//游戏种类
bool CAttemperEngineSink::OnDBPCGameTypeItem(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//效验参数
	ASSERT(wDataSize%sizeof(DBO_GP_GameType)==0);
	if (wDataSize%sizeof(DBO_GP_GameType)!=0) return false;

	//变量定义
	WORD wItemCount=wDataSize/sizeof(DBO_GP_GameType);
	DBO_GP_GameType * pGameType=(DBO_GP_GameType *)pData;

	//更新数据
	for (WORD i=0;i<wItemCount;i++)
	{
		//变量定义
		tagGameType GameType;
		ZeroMemory(&GameType,sizeof(GameType));

		//构造数据
		GameType.wTypeID=(pGameType+i)->wTypeID;
		GameType.wJoinID=(pGameType+i)->wJoinID;
		GameType.wSortID=(pGameType+i)->wSortID;
		lstrcpyn(GameType.szTypeName,(pGameType+i)->szTypeName,CountArray(GameType.szTypeName));

		//插入列表
		m_ServerListManager.InsertGameType(&GameType);
	}

	return true;
}

//游戏类型
bool CAttemperEngineSink::OnDBPCGameKindItem(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//效验参数
	ASSERT(wDataSize%sizeof(DBO_GP_GameKind)==0);
	if (wDataSize%sizeof(DBO_GP_GameKind)!=0) return false;

	//变量定义
	WORD wItemCount=wDataSize/sizeof(DBO_GP_GameKind);
	DBO_GP_GameKind * pGameKind=(DBO_GP_GameKind *)pData;

	//更新数据
	for (WORD i=0;i<wItemCount;i++)
	{
		//变量定义
		tagGameKind GameKind;
		ZeroMemory(&GameKind,sizeof(GameKind));

		//构造数据
		GameKind.wTypeID=(pGameKind+i)->wTypeID;
		GameKind.wJoinID=(pGameKind+i)->wJoinID;
		GameKind.wSortID=(pGameKind+i)->wSortID;
		GameKind.wKindID=(pGameKind+i)->wKindID;
		GameKind.wGameID=(pGameKind+i)->wGameID;
		GameKind.dwOnLineCount=m_ServerListManager.CollectOnlineInfo((pGameKind+i)->wKindID);
		lstrcpyn(GameKind.szKindName,(pGameKind+i)->szKindName,CountArray(GameKind.szKindName));
		lstrcpyn(GameKind.szProcessName,(pGameKind+i)->szProcessName,CountArray(GameKind.szProcessName));

		//插入列表
		m_ServerListManager.InsertGameKind(&GameKind);
	}

	return true;
}

//游戏节点
bool CAttemperEngineSink::OnDBPCGameNodeItem(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//效验参数
	ASSERT(wDataSize%sizeof(DBO_GP_GameNode)==0);
	if (wDataSize%sizeof(DBO_GP_GameNode)!=0) return false;

	//变量定义
	WORD wItemCount=wDataSize/sizeof(DBO_GP_GameNode);
	DBO_GP_GameNode * pGameNode=(DBO_GP_GameNode *)pData;

	//更新数据
	for (WORD i=0;i<wItemCount;i++)
	{
		//变量定义
		tagGameNode GameNode;
		ZeroMemory(&GameNode,sizeof(GameNode));

		//构造数据
		GameNode.wKindID=(pGameNode+i)->wKindID;
		GameNode.wJoinID=(pGameNode+i)->wJoinID;
		GameNode.wSortID=(pGameNode+i)->wSortID;
		GameNode.wNodeID=(pGameNode+i)->wNodeID;
		lstrcpyn(GameNode.szNodeName,(pGameNode+i)->szNodeName,CountArray(GameNode.szNodeName));

		//插入列表
		m_ServerListManager.InsertGameNode(&GameNode);
	}

	return true;
}

//游戏定制
bool CAttemperEngineSink::OnDBPCGamePageItem(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//效验参数
	ASSERT(wDataSize%sizeof(DBO_GP_GamePage)==0);
	if (wDataSize%sizeof(DBO_GP_GamePage)!=0) return false;

	//变量定义
	WORD wItemCount=wDataSize/sizeof(DBO_GP_GamePage);
	DBO_GP_GamePage * pGamePage=(DBO_GP_GamePage *)pData;

	//更新数据
	for (WORD i=0;i<wItemCount;i++)
	{
		//变量定义
		tagGamePage GamePage;
		ZeroMemory(&GamePage,sizeof(GamePage));

		//构造数据
		GamePage.wKindID=(pGamePage+i)->wKindID;
		GamePage.wNodeID=(pGamePage+i)->wNodeID;
		GamePage.wSortID=(pGamePage+i)->wSortID;
		GamePage.wPageID=(pGamePage+i)->wPageID;
		GamePage.wOperateType=(pGamePage+i)->wOperateType;
		lstrcpyn(GamePage.szDisplayName,(pGamePage+i)->szDisplayName,CountArray(GamePage.szDisplayName));

		//插入列表
		m_ServerListManager.InsertGamePage(&GamePage);
	}

	return true;
}

//游戏列表
bool CAttemperEngineSink::OnDBPCGameListResult(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//效验参数
	ASSERT(wDataSize==sizeof(DBO_GP_GameListResult));
	if (wDataSize!=sizeof(DBO_GP_GameListResult)) return false;

	//变量定义
	DBO_GP_GameListResult * pGameListResult=(DBO_GP_GameListResult *)pData;

	//消息处理
	if (pGameListResult->cbSuccess==TRUE)
	{
		//清理列表
		m_ServerListManager.CleanKernelItem();

		//事件通知
		CP_ControlResult ControlResult;
		ControlResult.cbSuccess=ER_SUCCESS;
		SendUIControlPacket(UI_LOAD_DB_LIST_RESULT,&ControlResult,sizeof(ControlResult));

		//设置时间
		ASSERT(m_pITimerEngine!=NULL);
		m_pITimerEngine->SetTimer(IDI_LOAD_GAME_LIST,m_pInitParameter->m_wLoadListTime*1000L,1,0);
	}
	else
	{
		//构造提示
		TCHAR szDescribe[128]=TEXT("");
		_sntprintf(szDescribe,CountArray(szDescribe),TEXT("服务器列表加载失败，%ld 秒后将重新加载"),m_pInitParameter->m_wReLoadListTime);

		//提示消息
		CTraceService::TraceString(szDescribe,TraceLevel_Warning);

		//设置时间
		ASSERT(m_pITimerEngine!=NULL);
		m_pITimerEngine->SetTimer(IDI_LOAD_GAME_LIST,m_pInitParameter->m_wReLoadListTime*1000L,1,0);
	}

	return true;
}
//签到奖励
bool CAttemperEngineSink::OnDBPCCheckInReward(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//参数校验
	ASSERT(wDataSize==sizeof(DBO_GP_CheckInReward));
	if(wDataSize!=sizeof(DBO_GP_CheckInReward)) return false;

	//提取数据
	DBO_GP_CheckInReward * pCheckInReward=(DBO_GP_CheckInReward *)pData;

	//拷贝数据
	CopyMemory(m_kCheckInInfo.lRewardDay,pCheckInReward->lRewardDay,sizeof(m_kCheckInInfo.lRewardDay));
	CopyMemory(m_kCheckInInfo.lRewardGold,pCheckInReward->lRewardGold,sizeof(m_kCheckInInfo.lRewardGold));
	CopyMemory(m_kCheckInInfo.lRewardType,pCheckInReward->lRewardType,sizeof(m_kCheckInInfo.lRewardType));

	//发送数据
	m_pITCPNetworkEngine->SendData(dwContextID, MDM_GP_USER_SERVICE, SUB_GP_CHECKIN_AWARD, &m_kCheckInInfo, sizeof(m_kCheckInInfo));

	return true;
}

//签到信息
bool CAttemperEngineSink::OnDBPCUserCheckInInfo(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	DBO_GP_CheckInInfo * pCheckInInfo=(DBO_GP_CheckInInfo *)pData;

	//变量定义
	CMD_GP_CheckInInfo CheckInInfo;
	ZeroMemory(&CheckInInfo,sizeof(CheckInInfo));

	//构造变量
	CheckInInfo.bTodayChecked=pCheckInInfo->bTodayChecked;
	CheckInInfo.wAwardDate=pCheckInInfo->wAwardDate;
	CheckInInfo.wSeriesDate=pCheckInInfo->wSeriesDate;
	CopyMemory(CheckInInfo.lRewardDay,m_kCheckInInfo.lRewardDay,sizeof(CheckInInfo.lRewardDay));
	CopyMemory(CheckInInfo.lRewardGold,m_kCheckInInfo.lRewardGold,sizeof(CheckInInfo.lRewardGold));
	CopyMemory(CheckInInfo.lRewardType,m_kCheckInInfo.lRewardType,sizeof(CheckInInfo.lRewardType));

	//发送数据
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_CHECKIN_INFO,&CheckInInfo,sizeof(CheckInInfo));

	return true;
}

//签到结果
bool CAttemperEngineSink::OnDBPCUserCheckInResult(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	DBO_GP_CheckInResult * pCheckInResult=(DBO_GP_CheckInResult *)pData;

	//变量定义
	CMD_GP_CheckInResult CheckInResult;
	ZeroMemory(&CheckInResult,sizeof(CheckInResult));

	//构造变量
	CheckInResult.bType=pCheckInResult->bType;
	CheckInResult.bSuccessed=pCheckInResult->bSuccessed;
	CheckInResult.lScore=pCheckInResult->lScore;
	CheckInResult.wSeriesDate = pCheckInResult->wSeriesDate;
	lstrcpyn(CheckInResult.szNotifyContent,pCheckInResult->szNotifyContent,CountArray(CheckInResult.szNotifyContent));

	//发送数据
	DWORD wSendSize = sizeof(CheckInResult)-sizeof(CheckInResult.szNotifyContent)+CountStringBuffer(CheckInResult.szNotifyContent);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_CHECKIN_RESULT,&CheckInResult,wSendSize);

	return true;
}

//mChen add
//抽奖结果
bool CAttemperEngineSink::OnDBPCUserRaffleResult(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID) < m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter + LOWORD(dwContextID))->dwSocketID != dwContextID) return true;

	//变量定义
	DBO_GP_RaffleResult * pRaffleResult = (DBO_GP_RaffleResult *)pData;

	//变量定义
	CMD_GP_RaffleResult RaffleResult;
	ZeroMemory(&RaffleResult, sizeof(RaffleResult));

	//构造变量
	RaffleResult.bSuccessed = pRaffleResult->bSuccessed;
	RaffleResult.lScore = pRaffleResult->lScore;
	RaffleResult.dwRaffleCount = pRaffleResult->dwRaffleCount;
	RaffleResult.dwPlayCount = pRaffleResult->dwPlayCount;
	lstrcpyn(RaffleResult.szNotifyContent, pRaffleResult->szNotifyContent, CountArray(RaffleResult.szNotifyContent));

	//发送数据
	DWORD wSendSize = sizeof(RaffleResult) - sizeof(RaffleResult.szNotifyContent) + CountStringBuffer(RaffleResult.szNotifyContent);
	m_pITCPNetworkEngine->SendData(dwContextID, MDM_GP_USER_SERVICE, SUB_GP_RAFFLE_RESULT, &RaffleResult, wSendSize);

	return true;
}

//新手引导奖励
bool CAttemperEngineSink::OnDBPCBeginnerConfig(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//参数校验
	ASSERT(wDataSize==sizeof(DBO_GP_BeginnerCofig));
	if(wDataSize!=sizeof(DBO_GP_BeginnerCofig)) return false;

	//提取数据
	DBO_GP_BeginnerCofig * pDBInfo=(DBO_GP_BeginnerCofig *)pData;

	//拷贝数据
	CopyMemory((void*)(&m_kBeginnerInfo.lRewardGold),(void*)&pDBInfo->lRewardGold,sizeof(m_kBeginnerInfo.lRewardGold));
	CopyMemory((void*)(&m_kBeginnerInfo.lRewardType),(void*)&pDBInfo->lRewardType,sizeof(m_kBeginnerInfo.lRewardType));

	return true;
}
//新手引导信息
bool CAttemperEngineSink::OnDBPCUserBeginnerInfo(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	DBO_GP_BeginnerInfo * pDBInfo=(DBO_GP_BeginnerInfo *)pData;

	//变量定义
	CMD_GP_BeginnerInfo kNetInfo;
	ZeroMemory(&kNetInfo,sizeof(kNetInfo));

	//构造变量
	kNetInfo.bTodayChecked=pDBInfo->bTodayChecked;
	kNetInfo.wSeriesDate=pDBInfo->wSeriesDate;
	kNetInfo.bLastCheckIned=pDBInfo->bLastCheckIned;
	CopyMemory((void*)(&kNetInfo.lRewardGold),(void*)&m_kBeginnerInfo.lRewardGold,sizeof(kNetInfo.lRewardGold));
	CopyMemory((void*)(&kNetInfo.lRewardType),(void*)&m_kBeginnerInfo.lRewardType,sizeof(kNetInfo.lRewardType));

	//发送数据
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_BEGINNER_INFO,&kNetInfo,sizeof(kNetInfo));

	return true;
}
//新手引导结果
bool CAttemperEngineSink::OnDBPCUserBeginnerResult(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	DBO_GP_BeginnerResult * pDBInfo = (DBO_GP_BeginnerResult *)pData;

	//变量定义
	CMD_GP_BeginnerResult kNetInfo;
	ZeroMemory(&kNetInfo,sizeof(kNetInfo));

	//构造变量
	kNetInfo.bSuccessed=pDBInfo->bSuccessed;
	kNetInfo.lAwardCout=pDBInfo->lAwardCout;
	kNetInfo.bSuccessed=pDBInfo->bSuccessed;
	kNetInfo.lAwardType=pDBInfo->lAwardType;
	lstrcpyn(kNetInfo.szNotifyContent,pDBInfo->szNotifyContent,CountArray(kNetInfo.szNotifyContent));

	//发送数据
	DWORD wSendSize = sizeof(kNetInfo)-sizeof(kNetInfo.szNotifyContent)+CountStringBuffer(kNetInfo.szNotifyContent);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_BEGINNER_RESULT,&kNetInfo,wSendSize);

	return true;
}
//低保参数
bool CAttemperEngineSink::OnDBPCBaseEnsureParameter(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//变量定义
	DBO_GP_BaseEnsureParameter * pEnsureParameter=(DBO_GP_BaseEnsureParameter *)pData;

	//设置变量
	m_BaseEnsureParameter.cbTakeTimes = pEnsureParameter->cbTakeTimes;
	m_BaseEnsureParameter.lScoreAmount = pEnsureParameter->lScoreAmount;
	m_BaseEnsureParameter.lScoreCondition = pEnsureParameter->lScoreCondition;

	return true;
}
//赚金榜奖励
bool CAttemperEngineSink::OnDBPCAddBankAwardConfig(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//变量定义
	DBO_GP_AddRankAwardConfig * pDBDataInfo=(DBO_GP_AddRankAwardConfig *)pData;
	ASSERT(pDBDataInfo->iIdex >=0 && pDBDataInfo->iIdex < 3);
	if (!(pDBDataInfo->iIdex >=0 && pDBDataInfo->iIdex < 3))
	{
		return false;
	}

	CopyMemory(m_BackAddBankAwardInfo.kRewardGold[pDBDataInfo->iIdex],pDBDataInfo->kRewardGold,sizeof(pDBDataInfo->kRewardGold));
	CopyMemory(m_BackAddBankAwardInfo.kRewardType[pDBDataInfo->iIdex],pDBDataInfo->kRewardType,sizeof(pDBDataInfo->kRewardType));

	return true;
}
//赚金榜排行
bool CAttemperEngineSink::OnDBPCAddBankBack(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//DBO_GP_BackAddBank;//
	//CMD_GP_BackAddBank;//

	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_ADDRANK_BACK_RANK,pData,wDataSize);
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}
bool CAttemperEngineSink::OnDBPCBaseEnsureResult(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//变量定义
	DBO_GP_BaseEnsureResult * pBaseEnsureResult=(DBO_GP_BaseEnsureResult *)pData;

	//构造结构
	CMD_GP_BaseEnsureResult BaseEnsureResult;
	BaseEnsureResult.bSuccessed=pBaseEnsureResult->bSuccessed;
	BaseEnsureResult.lGameScore=pBaseEnsureResult->lGameScore;
	lstrcpyn(BaseEnsureResult.szNotifyContent,pBaseEnsureResult->szNotifyContent,CountArray(BaseEnsureResult.szNotifyContent));

	//发送数据
	WORD wSendDataSize=sizeof(BaseEnsureResult)-sizeof(BaseEnsureResult.szNotifyContent);
	wSendDataSize+=CountStringBuffer(BaseEnsureResult.szNotifyContent);
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_BASEENSURE_RESULT,&BaseEnsureResult,wSendDataSize);

	return true;
}

//版本检测
bool CAttemperEngineSink::CheckPlazaVersion(BYTE cbDeviceType, DWORD dwPlazaVersion, DWORD dwSocketID, bool bCheckLowVer)
{
	//变量定义
	bool bMustUpdate=false;
	bool bAdviceUpdate=false;
	DWORD dwVersion=VERSION_PLAZA;

	//手机版本
	if(cbDeviceType >= DEVICE_TYPE_IPAD) dwVersion=VERSION_MOBILE_IOS;
	else if(cbDeviceType >= DEVICE_TYPE_IPHONE) dwVersion=VERSION_MOBILE_IOS;
	else if(cbDeviceType >= DEVICE_TYPE_ITOUCH) dwVersion=VERSION_MOBILE_IOS;
	else if(cbDeviceType >= DEVICE_TYPE_ANDROID) dwVersion=VERSION_MOBILE_ANDROID;
	else if(cbDeviceType == DEVICE_TYPE_PC) dwVersion=VERSION_PLAZA;

	//版本判断
	if (bCheckLowVer && GetSubVer(dwPlazaVersion) < GetSubVer(dwVersion))
	{
		bAdviceUpdate = true;

		bMustUpdate = true;//mChen add
	}

	if (GetMainVer(dwPlazaVersion)!=GetMainVer(dwVersion)) bMustUpdate=true;
	if (GetProductVer(dwPlazaVersion)!=GetProductVer(dwVersion)) bMustUpdate=true;

	//升级判断
	if ((bMustUpdate==true)||(bAdviceUpdate==true))
	{
		//变量定义
		CMD_GP_UpdateNotify UpdateNotify;
		ZeroMemory(&UpdateNotify,sizeof(UpdateNotify));

		//变量定义
		UpdateNotify.cbMustUpdate=bMustUpdate;
		UpdateNotify.cbAdviceUpdate=bAdviceUpdate;
		UpdateNotify.dwCurrentVersion=dwVersion;

		//发送消息
		m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_LOGON,SUB_GP_UPDATE_NOTIFY,&UpdateNotify,sizeof(UpdateNotify));

		//中断判断
		if (bMustUpdate==true) 
		{
			m_pITCPNetworkEngine->ShutDownSocket(dwSocketID);
			return false;
		}
	}

	return true;
}

//发送请求
bool CAttemperEngineSink::SendUIControlPacket(WORD wRequestID, VOID * pData, WORD wDataSize)
{
	//发送数据
	CServiceUnits * pServiceUnits=CServiceUnits::g_pServiceUnits;
	pServiceUnits->PostControlRequest(wRequestID,pData,wDataSize);

	return true;
}

//发送类型
VOID CAttemperEngineSink::SendGameTypeInfo(DWORD dwSocketID)
{
	//网络数据
	DWORD wSendSize=0;
	BYTE cbDataBuffer[SOCKET_TCP_PACKET];

	//枚举数据
	POSITION Position=NULL;
	CGameTypeItem * pGameTypeItem=NULL;

	//枚举数据
	for (DWORD i=0;i<m_ServerListManager.GetGameTypeCount();i++)
	{
		//发送数据
		if ((wSendSize+sizeof(tagGameType))>sizeof(cbDataBuffer))
		{
			m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_TYPE,cbDataBuffer,wSendSize);
			wSendSize=0;
		}

		//获取数据
		pGameTypeItem=m_ServerListManager.EmunGameTypeItem(Position);
		if (pGameTypeItem==NULL) break;

		//拷贝数据
		CopyMemory(cbDataBuffer+wSendSize,&pGameTypeItem->m_GameType,sizeof(tagGameType));
		wSendSize+=sizeof(tagGameType);
	}

	//发送剩余
	if (wSendSize>0) m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_TYPE,cbDataBuffer,wSendSize);

	return;
}

//发送种类
VOID CAttemperEngineSink::SendGameKindInfo(DWORD dwSocketID)
{
	//网络数据
	DWORD wSendSize=0;
	BYTE cbDataBuffer[SOCKET_TCP_PACKET];

	//枚举数据
	POSITION Position=NULL;
	CGameKindItem * pGameKindItem=NULL;

	//枚举数据
	for (DWORD i=0;i<m_ServerListManager.GetGameKindCount();i++)
	{
		//发送数据
		if ((wSendSize+sizeof(tagGameKind))>sizeof(cbDataBuffer))
		{
			m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_KIND,cbDataBuffer,wSendSize);
			wSendSize=0;
		}

		//获取数据
		pGameKindItem=m_ServerListManager.EmunGameKindItem(Position);
		if (pGameKindItem==NULL) break;

		//拷贝数据
		CopyMemory(cbDataBuffer+wSendSize,&pGameKindItem->m_GameKind,sizeof(tagGameKind));
		wSendSize+=sizeof(tagGameKind);
	}

	//发送剩余
	if (wSendSize>0) m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_KIND,cbDataBuffer,wSendSize);

	return;
}

//发送节点
VOID CAttemperEngineSink::SendGameNodeInfo(DWORD dwSocketID, WORD wKindID)
{
	//网络数据
	DWORD wSendSize=0;
	BYTE cbDataBuffer[SOCKET_TCP_PACKET];

	//枚举数据
	POSITION Position=NULL;
	CGameNodeItem * pGameNodeItem=NULL;

	//枚举数据
	for (DWORD i=0;i<m_ServerListManager.GetGameNodeCount();i++)
	{
		//发送数据
		if ((wSendSize+sizeof(tagGameNode))>sizeof(cbDataBuffer))
		{
			m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_NODE,cbDataBuffer,wSendSize);
			wSendSize=0;
		}

		//获取数据
		pGameNodeItem=m_ServerListManager.EmunGameNodeItem(Position);
		if (pGameNodeItem==NULL) break;

		//拷贝数据
		if ((wKindID==INVALID_WORD)||(pGameNodeItem->m_GameNode.wKindID==wKindID))
		{
			CopyMemory(cbDataBuffer+wSendSize,&pGameNodeItem->m_GameNode,sizeof(tagGameNode));
			wSendSize+=sizeof(tagGameNode);
		}
	}

	//发送剩余
	if (wSendSize>0) m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_NODE,cbDataBuffer,wSendSize);

	return;
}

//发送定制
VOID CAttemperEngineSink::SendGamePageInfo(DWORD dwSocketID, WORD wKindID)
{
	//网络数据
	DWORD wSendSize=0;
	BYTE cbDataBuffer[SOCKET_TCP_PACKET];

	//枚举数据
	POSITION Position=NULL;
	CGamePageItem * pGamePageItem=NULL;

	//枚举数据
	for (DWORD i=0;i<m_ServerListManager.GetGamePageCount();i++)
	{
		//发送数据
		if ((wSendSize+sizeof(tagGamePage))>sizeof(cbDataBuffer))
		{
			m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_PAGE,cbDataBuffer,wSendSize);
			wSendSize=0;
		}

		//获取数据
		pGamePageItem=m_ServerListManager.EmunGamePageItem(Position);
		if (pGamePageItem==NULL) break;

		//拷贝数据
		if ((wKindID==INVALID_WORD)||(pGamePageItem->m_GamePage.wKindID==wKindID))
		{
			CopyMemory(cbDataBuffer+wSendSize,&pGamePageItem->m_GamePage,sizeof(tagGamePage));
			wSendSize+=sizeof(tagGamePage);
		}
	}

	//发送剩余
	if (wSendSize>0) m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_PAGE,cbDataBuffer,wSendSize);

	return;
}

//发送房间
VOID CAttemperEngineSink::SendGameServerInfo(DWORD dwSocketID, WORD wKindID)
{
	//网络数据
	DWORD wSendSize=0;
	BYTE cbDataBuffer[SOCKET_TCP_PACKET];

	//枚举数据
	POSITION Position=NULL;
	CGameServerItem * pGameServerItem=NULL;

	//枚举数据
	for (DWORD i=0;i<m_ServerListManager.GetGameServerCount();i++)
	{
		//发送数据
		if ((wSendSize+sizeof(tagGameServer))>sizeof(cbDataBuffer))
		{
			m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_SERVER,cbDataBuffer,wSendSize);
			wSendSize=0;
		}

		//获取数据
		pGameServerItem=m_ServerListManager.EmunGameServerItem(Position);
		if (pGameServerItem==NULL) break;

		//拷贝数据
		if ((wKindID==INVALID_WORD)||(pGameServerItem->m_GameServer.wKindID==wKindID))
		{
			CopyMemory(cbDataBuffer+wSendSize,&pGameServerItem->m_GameServer,sizeof(tagGameServer));
			wSendSize+=sizeof(tagGameServer);
		}
	}

	//发送剩余
	if (wSendSize>0) m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_SERVER,cbDataBuffer,wSendSize);

	//设置变量
	wSendSize=0;
	ZeroMemory(cbDataBuffer,sizeof(cbDataBuffer));

	//枚举数据
	for (DWORD i=0;i<m_ServerListManager.GetGameServerCount();i++)
	{
		//发送数据
		if ((wSendSize+sizeof(tagGameMatch))>sizeof(cbDataBuffer))
		{
			m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_MATCH,cbDataBuffer,wSendSize);
			wSendSize=0;
		}

		//获取数据
		pGameServerItem=m_ServerListManager.EmunGameServerItem(Position);
		if (pGameServerItem==NULL) break;
		if (pGameServerItem->IsMatchServer()==false) continue;

		//拷贝数据
		if ((wKindID==INVALID_WORD)||(pGameServerItem->m_GameServer.wKindID==wKindID))
		{
			CopyMemory(cbDataBuffer+wSendSize,&pGameServerItem->m_GameMatch,sizeof(tagGameMatch));
			wSendSize+=sizeof(tagGameMatch);
		}
	}

	//发送剩余
	if (wSendSize>0) m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_LIST_MATCH,cbDataBuffer,wSendSize);

	if(m_wAVServerPort!=0 && m_dwAVServerAddr!=0)
	{
		//变量定义
		tagAVServerOption AVServerOption;
		AVServerOption.wAVServerPort=m_wAVServerPort;
		AVServerOption.dwAVServerAddr=m_dwAVServerAddr;

		//发送配置
		m_pITCPNetworkEngine->SendData(dwSocketID,MDM_GP_SERVER_LIST,SUB_GP_VIDEO_OPTION,&AVServerOption,sizeof(AVServerOption));
	};

	return;
}

//mChen add. for HideSeek
//发送大厅列表
VOID CAttemperEngineSink::SendGameLobbyInfo(DWORD dwSocketID)
{
	//网络数据
	DWORD wSendSize = 0;
	BYTE cbDataBuffer[SOCKET_TCP_PACKET];

	//枚举数据
	POSITION Position = NULL;
	tagGameLobby * pGameLobbyItem = NULL;

	//枚举数据
	for (DWORD i = 0; i < m_ServerListManager.GetGameLobbyCount(); i++)
	{
		//发送数据
		if ((wSendSize + sizeof(tagGameLobby)) > sizeof(cbDataBuffer))
		{
			m_pITCPNetworkEngine->SendData(dwSocketID, MDM_GP_SERVER_LIST, SUB_GP_LIST_LOBBY, cbDataBuffer, wSendSize);
			wSendSize = 0;
		}

		//获取数据
		pGameLobbyItem = m_ServerListManager.EmunGameLobbyItem(Position);
		if (pGameLobbyItem == NULL) break;

		//拷贝数据
		CopyMemory(cbDataBuffer + wSendSize, pGameLobbyItem, sizeof(tagGameLobby));
		wSendSize += sizeof(tagGameLobby);
	}

	//发送剩余
	if (wSendSize > 0) m_pITCPNetworkEngine->SendData(dwSocketID, MDM_GP_SERVER_LIST, SUB_GP_LIST_LOBBY, cbDataBuffer, wSendSize);

	return;
}

//发送类型
VOID CAttemperEngineSink::SendMobileKindInfo(DWORD dwSocketID, WORD wModuleID)
{
	return;
}

//发送房间
VOID CAttemperEngineSink::SendMobileServerInfo(DWORD dwSocketID, WORD wModuleID)
{
	//网络数据
	DWORD wSendSize=0;
	BYTE cbDataBuffer[SOCKET_TCP_PACKET];

	//枚举数据
	POSITION Position=NULL;
	CGameServerItem * pGameServerItem=NULL;

	//枚举数据
	for (DWORD i=0;i<m_ServerListManager.GetGameServerCount();i++)
	{
		//发送数据
		if ((wSendSize+sizeof(tagGameServer))>sizeof(cbDataBuffer))
		{
			m_pITCPNetworkEngine->SendData(dwSocketID,MDM_MB_SERVER_LIST,SUB_MB_LIST_SERVER,cbDataBuffer,wSendSize);
			wSendSize=0;
		}

		//获取数据
		pGameServerItem=m_ServerListManager.EmunGameServerItem(Position);
		if (pGameServerItem==NULL) break;

		//拷贝数据
		if (pGameServerItem->m_GameServer.wKindID==wModuleID)
		{
			CopyMemory(cbDataBuffer+wSendSize,&pGameServerItem->m_GameServer,sizeof(tagGameServer));
			wSendSize+=sizeof(tagGameServer);
		}
	}

	//发送剩余
	if (wSendSize>0) m_pITCPNetworkEngine->SendData(dwSocketID,MDM_MB_SERVER_LIST,SUB_MB_LIST_SERVER,cbDataBuffer,wSendSize);

	return;
}



bool CAttemperEngineSink::OnTCPNetworkSubGetExchangeHuaFei( WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID )
{
	ASSERT(wDataSize==sizeof(CMD_GP_GetExchangeHuaFei));
	if(wDataSize!=sizeof(CMD_GP_GetExchangeHuaFei)) return false;

	//提取数据
	CMD_GP_GetExchangeHuaFei *pNetInfo = (CMD_GP_GetExchangeHuaFei *)pData;
	pNetInfo->szPassword[CountArray(pNetInfo->szPassword)-1]=0;

	DBR_GP_GetExchangeHuaFei kDBInfo;
	kDBInfo.dwUserID = pNetInfo->dwUserID;
	lstrcpyn(kDBInfo.szPassword,pNetInfo->szPassword,CountArray(kDBInfo.szPassword));


	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_GET_EXCHANGE_HUAFEI,dwSocketID,&kDBInfo,sizeof(kDBInfo));
	return true;
}

bool CAttemperEngineSink::OnTCPNetworkSubGetShopInfo(WORD wSubCmdID, VOID * pData, WORD wDataSize, DWORD dwSocketID)
{
	ASSERT(wDataSize==sizeof(CMD_GP_GetShopInfo));
	if(wDataSize!=sizeof(CMD_GP_GetShopInfo)) return false;

	//提取数据
	CMD_GP_GetShopInfo *pNetInfo = (CMD_GP_GetShopInfo *)pData;
	pNetInfo->szPassword[CountArray(pNetInfo->szPassword)-1]=0;

	DBR_GP_GetShopInfo kDBInfo;
	kDBInfo.dwUserID = pNetInfo->dwUserID;
	lstrcpyn(kDBInfo.szPassword,pNetInfo->szPassword,CountArray(kDBInfo.szPassword));


	m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_GET_SHOPINFO,dwSocketID,&kDBInfo,sizeof(kDBInfo));
	return true;
}

// 兑换话费列表返回
bool CAttemperEngineSink::OnDBPCExchangeHuaFeiBack( DWORD dwContextID, VOID * pData, WORD wDataSize )
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//DBO_GP_BackShopInfo
	//CMD_GP_BackShopInfo
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_EXCHANGEHUAFEI_BACK,pData,wDataSize);
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

bool CAttemperEngineSink::OnDBPCShopInfoBack(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//DBO_GP_BackExchangeHuaFei
	//CMD_GP_BackExchangeHuaFei
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_SHOPINFO_BACK,pData,wDataSize);
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);

	return true;
}

//游戏记录
bool CAttemperEngineSink::OnDBPCGameRecordList(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//DBO_GP_GameRecord_List
	//CMD_GP_BackGameRecord_List
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_GAME_RECORD_LIST,pData,wDataSize);
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);
	return true;
}

//游戏记录
bool CAttemperEngineSink::OnDBPCGameRecordTotal(DWORD dwContextID, VOID * pData, WORD wDataSize)
{
	//判断在线
	ASSERT(LOWORD(dwContextID)<m_pInitParameter->m_wMaxConnect);
	if ((m_pBindParameter+LOWORD(dwContextID))->dwSocketID!=dwContextID) return true;

	//DBO_GP_GameRecord_Video
	//CMD_GP_BackGameRecord_Video
	m_pITCPNetworkEngine->SendData(dwContextID,MDM_GP_USER_SERVICE,SUB_GP_GAME_RECORD_TOTAL,pData,wDataSize);
	m_pITCPNetworkEngine->ShutDownSocket(dwContextID);
	return true;
}
//////////////////////////////////////////////////////////////////////////////////
