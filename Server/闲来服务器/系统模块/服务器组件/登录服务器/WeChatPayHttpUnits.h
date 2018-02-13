#pragma once
#include <functional>

//服务单元
namespace WeChatPayHttpUnits
{
	template<typename T>
	inline std::wstring toString(T p)
	{
		std::wostringstream stream;
		stream << p;
		return stream.str();
	}
	template<typename T>
	inline T parseValue(const std::wstring& _value)
	{
		std::wistringstream stream(_value);
		T result;
		stream >> result;
		if (stream.fail())
			return T();
		else
		{
			int item = stream.get();
			while (item != -1)
			{
				if (item != ' ' && item != '\t')
					return T();
				item = stream.get();
			}
		}
		return result;
	}
	inline int parseInt(const std::wstring& _value)
	{
		return parseValue<int>(_value);
	}
	template<typename T>
	inline T parseValue(const std::string& _value)
	{
		std::istringstream stream(_value);
		T result;
		stream >> result;
		if (stream.fail())
			return T();
		else
		{
			int item = stream.get();
			while (item != -1)
			{
				if (item != ' ' && item != '\t')
					return T();
				item = stream.get();
			}
		}
		return result;
	}
	inline int parseInt(const std::string& _value)
	{
		return parseValue<int>(_value);
	}

	template<typename T>
	static std::string getDataValueStr(T* kValue, std::string kName)
	{
		if ((*kValue).HasMember(kName.c_str()) && ((*kValue)[kName.c_str()].IsString()))
		{
			return (*kValue)[kName.c_str()].GetString();
		}
		return "";
	}
	template<typename T>
	static int getDataValueInt(T* kValue, std::string kName)
	{
		if ((*kValue).HasMember(kName.c_str()) && ((*kValue)[kName.c_str()].IsInt()))
		{
			return (*kValue)[kName.c_str()].GetInt();
		}
		return 0;
	}
	
	void enterprisePay(int dwEnterprisePayment, int userID, std::wstring wstrRealName, std::wstring wstrOpenid, void* pNetEngine, void* pDataBaseEngine, DWORD tradeNum, std::function<void(int userID, void* netEngine, void* dataBaseEngine, DWORD dwSocketID, bool bSuccess, std::string& resultMsg, std::string&, std::string&, std::string&, std::string&, std::string& tradeNo)> sendCallBack);

	//tradeNum自定义订单号，统一下单时通过时间加dwSocketID拼接成的唯一的订单号（tradeNum == dwSocketID）
	void getPrePayStr(int itemId, int userID, void* pNetEngine, DWORD tradeNum,std::function<void(void*netEngine, DWORD dwSocketID, bool bSuccess, std::string&, std::string&, std::string&, std::string&, std::string&)> sendCallBack);
	bool getPrePayStr(int itemId, std::string& trade_no, std::string& prePayID, std::string& nonceStr, std::string& timeStamp, std::string& sign);

	void checkPayResult(int userId, std::string& transNoStr, int payStatus, void* pNetEngine, DWORD dwSocket, std::function<void(void*netEngine, DWORD dwSocketID, int retStatus,int,std::string)> CallBack);
};
