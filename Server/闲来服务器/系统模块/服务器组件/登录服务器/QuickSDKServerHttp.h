#ifndef QUICKSDKSERVERHTTP
#define QUICKSDKSERVERHTTP
#pragma once

#include "Stdafx.h"
#include "DataBasePacketIn.h"

#include <string>
#include <regex>
#include <sstream>
#include "tinyxml2.h"
#include <thread>
#include <fstream>

#include <cpprest/uri.h>
#include <cpprest/http_listener.h>
#include <cpprest/asyncrt_utils.h>


#pragma comment(lib, "bcrypt.lib")
#pragma comment(lib, "crypt32.lib")
#pragma comment(lib, "winhttp.lib")
#pragma comment(lib, "httpapi.lib")
#ifndef _DEBUG
#pragma comment (lib,"cpprest140_2_9.lib")
#else
#pragma comment (lib,"cpprest140_2_9.lib")
#endif

using namespace web;
using namespace http;
using namespace utility;
using namespace http::experimental::listener;
using namespace web::http::client;
using namespace std;
using namespace tinyxml2;
using namespace std;

class CommandHandler
{
public:
	CommandHandler() {}
	CommandHandler(utility::string_t url);
	pplx::task<void> open() {
		return m_listener.open();
	}
	pplx::task<void> close() {
		return m_listener.close();
	}
	void ListenerStart(IDataBaseEngine *	  m_pIDataBaseEngine);
private:
	void handle_get_or_post_server(http_request message);
	void handle_get_or_post_client();
	http_listener m_listener;
public:
	IDataBaseEngine *	  m_pIDataBaseEngine = NULL;	//Êý¾ÝÒýÇæ
};
#endif
