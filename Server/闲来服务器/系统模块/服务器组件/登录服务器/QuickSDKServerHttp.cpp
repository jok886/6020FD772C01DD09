#include "Stdafx.h"
#include "QuickSDKServerHttp.h"
#include "md5.h"

#pragma region 辅助函数
unsigned char FromHex(unsigned char x)
{
	unsigned char y;
	if (x >= 'A' && x <= 'Z') y = x - 'A' + 10;
	else if (x >= 'a' && x <= 'z') y = x - 'a' + 10;
	else if (x >= '0' && x <= '9') y = x - '0';
	else assert(0);
	return y;
}

string UrlDecode(const std::string& str)
{
	std::string strTemp = "";
	size_t length = str.length();
	for (size_t i = 0; i < length; i++)
	{
		if (str[i] == '+') strTemp += ' ';
		else if (str[i] == '%')
		{
			assert(i + 2 < length);
			unsigned char high = FromHex((unsigned char)str[++i]);
			unsigned char low = FromHex((unsigned char)str[++i]);
			strTemp += high * 16 + low;
		}
		else strTemp += str[i];
	}
	return strTemp;
}
string decode(string src, string key)  //SDK解密
{
	if (src.length() <= 0)
	{
		return src;
	}

	const regex pattern("(\\d+)");
	smatch matchResult;
	vector<int> list;

	const sregex_token_iterator end;
	for (sregex_token_iterator i(src.begin(), src.end(), pattern); i != end; ++i)
	{
		int n = atoi(i->str().c_str());
		list.push_back(n);
	}

	std::string data;
	for (int i = 0; i < list.size(); i++)
	{
		char c = list[i] - (0xff & key[i % key.size()]);
		data.append(1, c);
	}

	return data;
}
string encode(string src, string key)  //SDK加密
{
	string sb;
	for (int i = 0; i < src.size(); i++) {
		int n = (0xff & src[i]) + (0xff & key[i % key.length()]);
		stringstream thisGroup;
		thisGroup << "@" << n;
		string thisGroupString;
		thisGroup >> thisGroupString;
		sb.append(thisGroupString);
	}
	return sb;
}
ShopItemInfoXML ParseXML(string str)
{
	ShopItemInfoXML ShopItemInfoXml;
	ZeroMemory(&ShopItemInfoXml, sizeof(ShopItemInfoXML));

	tinyxml2::XMLDocument *doc = new tinyxml2::XMLDocument();
	doc->Parse(str.c_str());
	XMLElement* root = doc->RootElement();
	XMLElement* userNode = root->FirstChildElement("message");
	while (userNode == NULL)
	{
		userNode = userNode->NextSiblingElement();//下一个兄弟节点
	}
	if (userNode != NULL)  //searched successfully
	{
		XMLElement* testNode = userNode->FirstChildElement("is_test");
		ShopItemInfoXml.cbTest = atoi(testNode->GetText());
		//str.push_back(*testNode->GetText());
		XMLElement* channelNode = userNode->FirstChildElement("channel");
		ShopItemInfoXml.wChannel = atoi(channelNode->GetText());
		//str.push_back(*channelNode->GetText());
		XMLElement* channel_uidNode = userNode->FirstChildElement("channel_uid");
		lstrcpyn(ShopItemInfoXml.szUID, channel_uidNode->GetText(), CountArray(ShopItemInfoXml.szUID));
		//str.push_back(*channel_uidNode->GetText());
		XMLElement* game_orderNode = userNode->FirstChildElement("game_order");
		lstrcpyn(ShopItemInfoXml.szOrderID, game_orderNode->GetText(), CountArray(ShopItemInfoXml.szOrderID));
		//str.push_back(*game_orderNode->GetText());
		XMLElement* order_noNode = userNode->FirstChildElement("order_no");
		lstrcpyn(ShopItemInfoXml.szSDKOrderID, order_noNode->GetText(), CountArray(ShopItemInfoXml.szSDKOrderID));
		//str.push_back(*order_noNode->GetText());
		XMLElement* pay_timeNode = userNode->FirstChildElement("pay_time");
		lstrcpyn(ShopItemInfoXml.szPayTime, pay_timeNode->GetText(), CountArray(ShopItemInfoXml.szPayTime));
		//str.push_back(*pay_timeNode->GetText());
		XMLElement* amountNode = userNode->FirstChildElement("amount");
		ShopItemInfoXml.wAmount = atof(amountNode->GetText());
		//str.push_back(*amountNode->GetText());
		XMLElement* statusNode = userNode->FirstChildElement("status");
		ShopItemInfoXml.wPayStatus = atoi(statusNode->GetText());
		//str.push_back(*statusNode->GetText());
		XMLElement* extras_paramsNode = userNode->FirstChildElement("extras_params");
		//str.push_back(*extras_paramsNode->GetText());
		string st = extras_paramsNode->GetText();
		std::vector<std::string> v;
		string s = "";
		for (auto iter = st.begin(); iter <= st.end(); iter++)
		{
			if (*iter == '&' || iter == st.end())
			{
				v.push_back(s);
				s = "";
			}
			else
				s += *iter;
		}

		ShopItemInfoXml.wItemID = atoi(v[0].c_str());
		ShopItemInfoXml.wCount = atoi(v[1].c_str());
		ShopItemInfoXml.dwUserID = atoi(v[2].c_str());
	}
	return ShopItemInfoXml;
}
string WChar2Ansi(LPCWSTR pwszSrc)   //wstring转string
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
#pragma endregion 辅助函数

//////////////////////////////////////////////////

void CommandHandler::ListenerStart(IDataBaseEngine * m_pIDataBaseEngine)
{
	try
	{
		utility::string_t address = U("http://*:8090");
		uri_builder uri(address);
		auto addr = uri.to_uri().to_string();
		CommandHandler handler(addr);
		CTraceService::TraceString("订单监听开始。。。。。", TraceLevel_Warning);
		handler.open().wait();
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
CommandHandler::CommandHandler(utility::string_t url) : m_listener(url)
{
	//m_listener.support(methods::GET, std::bind(&CommandHandler::handle_get_or_post_server, this, std::placeholders::_1));
	m_listener.support(methods::POST, std::bind(&CommandHandler::handle_get_or_post_server, this, std::placeholders::_1));
}

void CommandHandler::handle_get_or_post_server(http_request message)
{
	//ucout << "Method: " << message.method() << std::endl;
	//ucout << "URI: " << http::uri::decode(message.relative_uri().path()) << std::endl;
	//ucout << "Query: " << http::uri::decode(message.relative_uri().query()) << std::endl << std::endl;

	auto bodyStream = message.body();
	concurrency::streams::stringstreambuf sbuffer;
	auto& target = sbuffer.collection();
	bodyStream.read_to_end(sbuffer).get();

	//ofstream ofs/*("TEXT.txt", ios::in | ios::out | ios::binary)*/;
	//ofs.open("TEXT.txt");
	//for (auto iter = target.begin(); iter != target.end(); iter++)
	//{
	//	ofs << *iter;
	//}
	//ofs.close();

	//string str = "nt_data=%40113%40113%40171%40158%40160%4082%40167%40157%40169%40165%40155%40161%40167%40115%4091%40103%40103%4098%4084%4082%40153%40165%40149%40159%40150%40162%40166%40160%40114%4087%40136%40133%40123%4095%40107%4083%4084%40165%40165%40153%40165%40150%40147%40158%40168%40164%40158%40115%4091%40160%40161%4084%40115%40117%40110%40161%40167%40162%40155%40164%40168%40153%40158%40144%40162%40151%40166%40164%40149%40153%40150%40118%40115%40159%40151%40165%40172%40151%40160%40155%40119%40110%40155%40165%40147%40171%40151%40163%40166%40119%40104%40117%40100%40158%40166%40144%40169%40151%40166%40165%40114%40110%40148%40160%40152%40160%40160%40151%40165%40116%40106%40105%40109%40110%4097%40149%40156%40152%40160%40158%40151%40165%40118%40117%40152%40157%40148%40159%40163%40151%40159%40144%40169%40155%40149%40118%40109%4099%4098%40107%40113%40103%40106%40108%40117%4097%40149%40154%40149%40165%40160%40149%40158%40152%40173%40162%40153%40115%40111%40152%40150%40159%40152%40144%40163%40164%40149%40157%40169%40112%40104%40101%40111%40107%40107%40107%40105%40105%40100%40103%40107%40111%4099%4098%40102%40108%40108%40105%40101%40107%40111%4096%40156%40147%40160%40150%40147%40161%40163%40156%40156%40164%40112%40110%40168%40168%40157%40155%40171%40145%40160%40161%40114%40104%40101%40100%40100%40105%40105%40113%40101%40102%40101%40102%40103%4098%4099%4097%40105%40105%40106%40106%40107%40103%40104%40103%40110%40107%40108%40114%40104%40161%40164%40150%40153%40169%40145%40158%40161%40119%40116%40169%40150%40174%40146%40165%40158%40159%40152%40111%40102%4098%4098%40112%40100%4098%4099%4095%40107%40107%4089%40104%40105%40108%4098%4099%40110%40107%40101%40108%4097%40169%40153%40178%40148%40169%40156%40158%40154%40112%40111%40146%40161%40161%40166%40166%40171%40112%4098%4096%40106%40102%40117%40101%40154%40159%40161%40167%40162%40171%40112%40108%40165%40173%40153%40173%40170%40168%40113%4097%40113%4097%40166%40165%40149%40166%40166%40171%40117%40110%40151%40170%40173%40168%40154%40169%40152%40162%40147%40164%40149%40164%40165%40110%4099%4095%40105%40105%4091%40102%4099%4097%40101%4098%40103%40109%4099%40151%40169%40172%40169%40147%40165%40145%40169%40151%40171%40151%40166%40165%40112%40110%4099%40164%40151%40163%40165%40154%40159%40158%40115%40113%4098%40162%40170%40155%40150%40156%40167%40150%40156%40151%40164%40151%40165%40165%40154%40157%40158%40116&sign=%40150%4098%40106%40106%40151%40104%40148%40155%40103%40105%40107%40152%40113%40102%40159%40105%40107%40148%40151%40152%40153%40109%4099%40150%4099%40154%40113%40158%40108%40152%40105%40106&md5Sign=b60f179507470104cd4718a40aa48a50";
	string nt_data = "";
	string sign = "";
	string md5Sign = "";
	int flag = 0;
	for (auto iter = target.begin(); iter <= target.end();)
	{
		if (*iter == '=')
		{
			iter++;
			decltype(iter) iterr;
			for (iterr = iter; *iterr != '&'; iterr++)
			{
				if (iterr == target.end())
					break;
				if (flag == 0)
					nt_data += *iterr;
				else if (flag == 1)
					sign += *iterr;
				else if (flag == 2)
					md5Sign += *iterr;
			}
			flag++;
			iter = iterr;
		}
		else
			iter++;
	}

	string md5key = "jle81o1jbjyaxscv6qca8uaqsqkzwh6f";
	string callbackkey = "52314218722296969222472029895531";
	

	//对数据进行urldecode
	nt_data = UrlDecode(nt_data.c_str());
	sign = UrlDecode(sign.c_str());

	if (MD5(nt_data + sign + md5key).toStr() != md5Sign)
	{
		message.reply(status_codes::OK, "FAILED");
		CTraceService::TraceString("信息校验错误！！！！", TraceLevel_Warning);
		return;
	}

	//对数据进行QuickSDK decode
	string result = decode(nt_data.c_str(), callbackkey);

	//ofs.open("TEXT1.txt");
	//for (auto iter = result.begin(); iter != result.end(); iter++)
	//{
	//	ofs << *iter;
	//}
	//ofs.close();

	auto ShopItemInfo = ParseXML(result);
	if (ShopItemInfo.wPayStatus == 0)
	{
		//变量定义
		DBR_GP_ShopItemInfo DBShopItemInfo;
		ZeroMemory(&DBShopItemInfo, sizeof(DBShopItemInfo));

		//构造数据
		DBShopItemInfo.dwUserID = ShopItemInfo.dwUserID;
		lstrcpyn(DBShopItemInfo.szUID, ShopItemInfo.szUID, CountArray(ShopItemInfo.szUID));
		lstrcpyn(DBShopItemInfo.szOrderID, ShopItemInfo.szOrderID, CountArray(ShopItemInfo.szOrderID));
		DBShopItemInfo.wItemID = ShopItemInfo.wItemID;
		DBShopItemInfo.wAmount = ShopItemInfo.wAmount;
		DBShopItemInfo.wCount = ShopItemInfo.wCount;

		//投递请求
		bool resultCode = m_pIDataBaseEngine->PostDataBaseRequest(DBR_GP_ADD_SHOPITEM, NULL, &DBShopItemInfo, sizeof(DBShopItemInfo));

		if (resultCode)
		{
			message.reply(status_codes::OK, "SUCCESS");
			CTraceService::TraceString("订单操作执行完毕！！！！", TraceLevel_Warning);
		}
		else
		{
			message.reply(status_codes::OK, "FAILED");
			CTraceService::TraceString("数据库操作失败！！！！", TraceLevel_Warning);
		}
	}
	else
	{
		message.reply(status_codes::OK, "FAILED");
		CTraceService::TraceString("支付状态显示失败！！！！", TraceLevel_Warning);
	}
}

void CommandHandler::handle_get_or_post_client()
{
}




