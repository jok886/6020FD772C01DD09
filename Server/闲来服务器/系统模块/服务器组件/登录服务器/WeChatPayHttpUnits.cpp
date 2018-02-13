#include "StdAfx.h"
#include "WeChatPayHttpUnits.h"
/*#include "restsdk/json/writer.h"
#include "restsdk/json/stringbuffer.h"
#include "restsdk/json/document.h"*/

#include "cpprest/http_client.h"
#include "cpprest/json.h"
#include <iostream>
#include <cpprest/asyncrt_utils.h>
#include <cpprest/http_msg.h>

using namespace web;
using namespace web::http;
using namespace web::http::client;

#ifndef _DEBUG
#pragma comment (lib,"cpprest140_2_9.lib")
#else
#pragma comment (lib,"cpprest140_2_9.lib")
#endif

////服务单元
namespace WeChatPayHttpUnits
{
	static utility::string_t s_kUrl = L"http://127.0.0.1:9111/";

	std::string WChar2Ansi(LPCWSTR pwszSrc)
	{
		int nLen = WideCharToMultiByte(CP_ACP, 0, pwszSrc, -1, NULL, 0, NULL, NULL);

		if (nLen <= 0) return std::string("");

		char* pszDst = new char[nLen];
		if (NULL == pszDst) return std::string("");

		WideCharToMultiByte(CP_ACP, 0, pwszSrc, -1, pszDst, nLen, NULL, NULL);
		pszDst[nLen - 1] = 0;

		std::string strTemp(pszDst);
		delete[] pszDst;

		return strTemp;
	}

	//mChen add, 企业提现
	void enterprisePay(int dwEnterprisePayment, int userID, std::wstring wstrRealName, std::wstring wstrOpenid, void* pNetEngine, void* pDataBaseEngine, DWORD dwContextID,
		std::function<void(int userID, void* netEngine, void* dataBaseEngine, DWORD dwSocketID, bool bSuccess, std::string& resultMsg, std::string&, std::string&, std::string&, std::string&, std::string& tradeNo)> sendCallBack)
	{
		//utility::string_t kUrl =
		//	s_kUrl + std::wstring(L"reqEnterprisePay?") +
		//	std::wstring(L"openid=") + wstrOpenid +	//std::wstring(L"ogzta0uNjL4HWJCTUZX8hCBMs-xM") +//	ogzta0vMIm5Vo2sc7zhdIBS6cLyU
		//	std::wstring(L"&amount=") + toString(dwEnterprisePayment) +
		//	std::wstring(L"&userID=") + toString(userID) +
		//	std::wstring(L"&realName=") + wstrRealName;// std::wstring(L"realname1");//wstrRealName
		//http_client client(kUrl);

		http_client client(s_kUrl);
		uri_builder builder(L"/reqEnterprisePay");
		builder.append_query(L"openid", wstrOpenid);
		builder.append_query(L"amount", toString(dwEnterprisePayment));
		builder.append_query(L"userID", toString(userID));
		builder.append_query(L"realName", wstrRealName);

		CTraceService::TraceString(TEXT("Start enterprisePay"), TraceLevel_Warning);
		client.request(methods::GET, builder.to_string()).then([userID, pNetEngine, pDataBaseEngine, dwContextID, sendCallBack](http_response response) -> pplx::task<json::value>
		{
			if (response.status_code() == status_codes::OK)
			{
				///CTraceService::TraceString(TEXT("企业提现成功！"), TraceLevel_Warning);
				return response.extract_json();
			}
			else
			{
				///CTraceService::TraceString(TEXT("企业提现失败！"), TraceLevel_Warning);
				return response.extract_json();
				//// Handle error cases, for now return empty json value...
				//return pplx::task_from_result(json::value());
			}
		})
			.then([userID, pNetEngine, pDataBaseEngine, dwContextID, sendCallBack](pplx::task<json::value> previousTask)
			{
				try
				{
					const json::value& jv = previousTask.get();
					if (jv.is_null() == false)
					{
						// Perform actions here to process the JSON value...
						auto success = WChar2Ansi(jv.at(utility::string_t(L"success")).as_string().c_str());
						bool isFailed = (success == "false");
						bool isSuccess = (success == "true");
						if (isSuccess)
						{
							//提现成功

							auto amount = WChar2Ansi(jv.at(utility::string_t(L"amount")).as_string().c_str());
							auto nonceStr = WChar2Ansi(jv.at(utility::string_t(L"nonceStr")).as_string().c_str());
							auto partnerTradeNo = WChar2Ansi(jv.at(L"partnerTradeNo").as_string().c_str());			//商户订单号
							auto paymentNo = WChar2Ansi(jv.at(L"paymentNo").as_string().c_str());					//微信订单号
							auto paymentTime = WChar2Ansi(jv.at(L"paymentTime").as_string().c_str());

							//log
							CString strLog;
							strLog.Format(TEXT("用户%d提现成功"), userID);
							CTraceService::TraceString(strLog, TraceLevel_Warning);

							std::string resultMsg = "企业提现成功！";
							//CString str;
							//str.Format(TEXT("获取企业提现result info： %s : %s : %s : %s"), amount.c_str(), nonceStr.c_str(), partnerTradeNo.c_str(), paymentNo.c_str());
							//CTraceService::TraceString(str, TraceLevel_Normal);
							sendCallBack(userID, pNetEngine, pDataBaseEngine, dwContextID, isSuccess, resultMsg, amount, nonceStr, partnerTradeNo, paymentNo, paymentTime);
						}
						else
						{
							//提现失败，有errMsg

							auto resultMsg = WChar2Ansi(jv.at(utility::string_t(L"errMsg")).as_string().c_str());

							//log
							CString strLog;
							strLog.Format(TEXT("用户%d提现失败: %s"), userID, resultMsg.c_str());
							CTraceService::TraceString(strLog, TraceLevel_Warning);

							std::string nullString;
							sendCallBack(userID, pNetEngine, pDataBaseEngine, dwContextID, isSuccess, resultMsg, nullString, nullString, nullString, nullString, nullString);
						}
					}
					else
					{
						//提现失败，无errMsg

						//log
						CString strLog;
						strLog.Format(TEXT("用户%d提现失败"), userID);
						CTraceService::TraceString(strLog, TraceLevel_Warning);

						std::string resultMsg = "企业提现失败！";
						std::string nullString;
						sendCallBack(userID, pNetEngine, pDataBaseEngine, dwContextID, false, resultMsg, nullString, nullString, nullString, nullString, nullString);
					}

				}
				catch (const http_exception& e)
				{
					// Print error.
					CString str;
					str.Format(TEXT("用户%d企业提现 异常： %s"), userID, e.what());
					CTraceService::TraceString(str, TraceLevel_Normal);
				}
			});
	}

	void getPrePayStr(int itemId, int userID,void* pNetEngine, DWORD tradeNum, std::function<void(void*netEngine, DWORD dwSocketID,bool bSuccess, std::string&, std::string&, std::string&, std::string&,std::string& tradeNo)> sendCallBack)
	{
	
		utility::string_t kUrl =
			s_kUrl + std::wstring(L"reqPrePayID?") +
			std::wstring(L"saleID=") + toString(itemId) +
			std::wstring(L"&tradeNo=") + toString(tradeNum % 1000000)+
			std::wstring(L"&userID=") + toString(userID);

		http_client client(kUrl);
#if true
		CTraceService::TraceString(TEXT("获取prePayId号成功！！"), TraceLevel_Warning);
		client.request(methods::GET).then([pNetEngine,tradeNum, sendCallBack](http_response response) -> pplx::task<json::value>
		{
			CTraceService::TraceString(TEXT("获取prePayId号成功！！"), TraceLevel_Warning);
			if (response.status_code() == status_codes::OK)
			{
				CTraceService::TraceString(TEXT("获取prePayId号成功！！"), TraceLevel_Warning);
				return response.extract_json();
			}
			CTraceService::TraceString(TEXT("获取prePayId号失败！！"), TraceLevel_Normal);
			// Handle error cases, for now return empty json value...
			return pplx::task_from_result(json::value());
		})
			.then([pNetEngine, tradeNum, sendCallBack](pplx::task<json::value> previousTask)
		{
			try
			{
				const json::value& jv = previousTask.get();
				if(jv.is_null() == false)
				{
					// Perform actions here to process the JSON value...
					auto prePayID = WChar2Ansi(jv.at(utility::string_t(L"prepayid")).as_string().c_str());
					auto nonceStr = WChar2Ansi(jv.at(utility::string_t(L"noncestr")).as_string().c_str());
					auto timeStamp = WChar2Ansi(jv.at(L"timestamp").as_string().c_str());
					auto sign = WChar2Ansi(jv.at(L"sign").as_string().c_str());
					auto transNo = WChar2Ansi(jv.at(L"trade_no").as_string().c_str());

					CString str;
					str.Format(TEXT("获取prepayID info： %s : %s : %s : %s"), prePayID.c_str(), nonceStr.c_str(), timeStamp.c_str(), sign.c_str());
					CTraceService::TraceString(str, TraceLevel_Normal);
					sendCallBack(pNetEngine, tradeNum, true, prePayID, nonceStr, timeStamp, sign, transNo);
				}
				else
				{
					std::string nullString;
					sendCallBack(pNetEngine, tradeNum, false, nullString, nullString, nullString, nullString, nullString);
				}
				
			}
			catch (const http_exception& e)
			{
				// Print error.
				CString str;
				str.Format(TEXT("获取prepayID 异常： %s"), e.what());
				CTraceService::TraceString(str, TraceLevel_Normal);
			}
		});
#else
		CTraceService::TraceString(TEXT("获取prePayId号开始！！"), TraceLevel_Normal);
		http_response response = client.request(methods::GET,pplx::cancellation_token::none()).get();
		CTraceService::TraceString(TEXT("获取prePayId号结束！！"), TraceLevel_Normal);
		if (response.status_code() == status_codes::OK)
		{
			const json::value& jv = response.extract_json().get();

			{
				CTraceService::TraceString(TEXT("获取prePayId号成功！！"), TraceLevel_Warning);
				auto prePayID = WChar2Ansi(jv.at(utility::string_t(L"prepayid")).as_string().c_str());
				auto nonceStr = WChar2Ansi(jv.at(utility::string_t(L"noncestr")).as_string().c_str());
				auto timeStamp = WChar2Ansi(jv.at(L"timestamp").as_string().c_str());
				auto sign = WChar2Ansi(jv.at(L"sign").as_string().c_str());

				CString str;
				str.Format(TEXT("获取prepayID info： %s : %s : %s : %s"), prePayID.c_str(), nonceStr.c_str(), timeStamp.c_str(), sign.c_str());
				CTraceService::TraceString(str, TraceLevel_Normal);
				//return atoi((const char*)v.c_str());
			}
		}
		else
		{
			CTraceService::TraceString(TEXT("获取prePayId号失败！！"), TraceLevel_Warning);
		}
#endif
		
	}

	bool getPrePayStr(int itemId, std::string& trade_no, std::string& prePayID, std::string& nonceStr, std::string& timeStamp, std::string& sign)
	{
		utility::string_t kUrl =
			s_kUrl + std::wstring(L"reqPrePayID?") +
			std::wstring(L"saleID=") + toString(itemId) +
			std::wstring(L"&tradeNo=") + toString(trade_no.c_str());
		
		http_client client(kUrl);

		http_response response = client.request(methods::GET, pplx::cancellation_token::none()).get();
		CTraceService::TraceString(TEXT("获取prePayId号结束！！"), TraceLevel_Normal);
		if (response.status_code() == status_codes::OK)
		{
			const json::value& jv = response.extract_json().get();

			{
				CTraceService::TraceString(TEXT("获取prePayId号成功！！"), TraceLevel_Warning);
				prePayID = WChar2Ansi(jv.at(utility::string_t(L"prepayid")).as_string().c_str());
				nonceStr = WChar2Ansi(jv.at(utility::string_t(L"noncestr")).as_string().c_str());
				timeStamp = WChar2Ansi(jv.at(L"timestamp").as_string().c_str());
				sign = WChar2Ansi(jv.at(L"sign").as_string().c_str());

				CString str;
				str.Format(TEXT("获取prepayID info： %s : %s : %s : %s"), prePayID.c_str(), nonceStr.c_str(), timeStamp.c_str(), sign.c_str());
				CTraceService::TraceString(str, TraceLevel_Normal);
				return true;
				//return atoi((const char*)v.c_str());
			}
		}
		else
		{
			CTraceService::TraceString(TEXT("获取prePayId号失败！！"), TraceLevel_Warning);
			return false;
		}
	}

	void checkPayResult(int userId, std::string& transNoStr, int payStatus, void* pNetEngine, DWORD dwSocket, std::function<void(void*netEngine, DWORD dwSocketID,int retStatus,int,std::string)> CallBack)
	{
		utility::string_t kUrl =
			s_kUrl + std::wstring(L"uploadClientPayInfo?") +
			std::wstring(L"userID=") + toString(userId) +
			std::wstring(L"&payStatus=") + toString(payStatus) +
			std::wstring(L"&transNo=") + toString(transNoStr.c_str());

		http_client client(kUrl);
		client.request(methods::GET).then([pNetEngine, dwSocket, CallBack](http_response response) -> pplx::task<json::value>
		{
			CTraceService::TraceString(TEXT("交易信息返回！！"), TraceLevel_Warning);
			if (response.status_code() == status_codes::OK)
			{
				CTraceService::TraceString(TEXT("获取交易信息成功！！"), TraceLevel_Warning);
				return response.extract_json();
			}
			CTraceService::TraceString(TEXT("获取交易信息失败！！"), TraceLevel_Normal);
			// Handle error cases, for now return empty json value...
			return pplx::task_from_result(json::value());
		})
			.then([pNetEngine, dwSocket, CallBack](pplx::task<json::value> previousTask)
		{
			CTraceService::TraceString(TEXT("获取交易信息成功2！！"), TraceLevel_Warning);
			try
			{
				CTraceService::TraceString(TEXT("获取交易信息成功3！！"), TraceLevel_Warning);
				const json::value& jv = previousTask.get();
				CTraceService::TraceString(TEXT("获取交易信息成功4！！"), TraceLevel_Warning);
				if (jv.is_null() == false)
				{
					CTraceService::TraceString(TEXT("获取交易信息成功5！！"), TraceLevel_Warning);
					// Perform actions here to process the JSON value...
					/*
					auto statuscode = jv.at(utility::string_t(L"retStatus")).as_integer();
					auto curGold = jv.at(utility::string_t(L"curGold")).as_integer();*/
					auto statuscode = WChar2Ansi(jv.at(utility::string_t(L"retStatus")).as_string().c_str());
					auto curGold = WChar2Ansi(jv.at(utility::string_t(L"curGold")).as_string().c_str());
					auto msg = WChar2Ansi(jv.at(utility::string_t(L"msg")).as_string().c_str());
					CString str;
					str.Format(TEXT("订单查询返回： %s --- %s ---%s"), statuscode.c_str(), curGold.c_str(), msg);
					CTraceService::TraceString(str, TraceLevel_Normal);
					CallBack(pNetEngine, dwSocket, atoi(statuscode.c_str()), atoi(curGold.c_str()),msg);
				}
				else
				{
					CTraceService::TraceString(TEXT("获取交易信息Json字符串为null！！"), TraceLevel_Warning);
					std::string nullString;
					CallBack(pNetEngine, dwSocket, 0,0,"");
				}

			}
			catch (const http_exception& e)
			{
				// Print error.
				CString str;
				str.Format(TEXT("获取交易信息异常： %s"), e.what());
				CTraceService::TraceString(str, TraceLevel_Normal);
			}
		});
	}
};
