using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GameNet;
using UnityEngine;

namespace GameNet
{
    //修改资料
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagModifyIndividual1
    {
        public byte cbGender; //用户性别

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_USER_NOTE)]
        public byte[] szHeadHttp; //头像HTTP
    };

    ///*
    //////////////////////////////////////////////////////////////////////////
    // 用户资料修改结构
    //////////////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct tagModifyIndividual
    {
        //public tagModifyIndividual()
        //{
        //    szNickName = new byte[SocketDefines.LEN_UNDER_WRITE];
        //    szHeadHttp = new byte[SocketDefines.LEN_USER_NOTE];
        //}

        public void reset()
        {
            Array.Clear(szNickName, 0, szNickName.Length);
            Array.Clear(szHeadHttp, 0, szHeadHttp.Length);
        }

        // 性别
        public byte cbGender;

        //用户昵称
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_USER_NOTE)]
        public byte[] szNickName;
        ///char szNickName[LEN_NICKNAME];

        ////个性签名
        //char szUnderWrite[LEN_UNDER_WRITE];
        ////用户备注
        //char szUserNote[LEN_USER_NOTE];
        ////真实名字
        //char szCompellation[LEN_COMPELLATION];
        ////固定号码
        //char szSeatPhone[LEN_SEAT_PHONE];
        ////手机号码
        //char szMobilePhone[LEN_MOBILE_PHONE];
        ////Q Q 号码
        //char szQQ[LEN_QQ];
        ////电子邮件
        //char szEMail[LEN_EMAIL];
        ////详细地址
        //char szDwellingPlace[LEN_DWELLING_PLACE];

        //头像HTTP
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SocketDefines.LEN_USER_NOTE)]
        public byte[] szHeadHttp;
        ///char szHeadHttp[LEN_USER_NOTE];

        ////渠道号
        //char szUserChannel[LEN_NICKNAME];
    };

    //*/
    //////////////////////////////////////////////////////////////////////////
    // 用户资料修改提示
    //////////////////////////////////////////////////////////////////////////
    public interface IGPIndividualMissionSink
    {
        void onGPIndividualInfo(int type);
        void onGPAccountInfo(CMD_GP_UserAccountInfo pAccountInfo);
        void onGPAccountInfoHttpIP(uint dwUserID, string strIP, string strHttp);
        void onGPIndividualSuccess(int type, byte[] szDescription);
        void onGPIndividualFailure(int type, byte[] szDescription);
    };

//////////////////////////////////////////////////////////////////////////
// 用户资料修改任务
//////////////////////////////////////////////////////////////////////////
    public class CGPIndividualMission : CSocketMission
    {
        public enum Type
        {
            MISSION_INDIVIDUAL_NULL,
            MISSION_INDIVIDUAL_QUERY,
            MISSION_INDIVIDUAL_Account,
            MISSION_INDIVIDUAL_MODIFY,
            MISSION_INDIVIDUAL_SPREADER,

            //mChen add
            //代理
            MISSION_INDIVIDUAL_ADDDEL_SPREADER,             //增加/删除推荐人身份
            MISSION_INDIVIDUAL_QUERY_SPREADERS_INFO,		//查询代理人列表
            MISSION_INDIVIDUAL_QUERY_TOP_PLAYERS_INFO,       //查询比赛积分排行
            MISSION_INDIVIDUAL_QUERY_NICKNAME_BY_ID,        //根据userid查询用户名（昵称），用于转房卡确认UI的显示
            MISSION_INDIVIDUAL_TRANSFER_DIAMOND,            //转房卡
            //内购
            MISSION_INDIVIDUAL_ADD_PAYMENT,
            MISSION_INDIVIDUAL_QUERY_CHILDREN_PAYMENT_INFO,
            MISSION_INDIVIDUAL_QUERY_PREPAYID,               //微信支付PrePayID
            MISSION_INDIVIDUAL_UPLOAD_CLIENT_PAY,            //上传客户端微信支付消息
            //企业提现
            MISSION_INDIVIDUAL_ADD_ENTERPRISE_PAYMENT,

            //mChen add, for HideSeek
            MISSION_INDIVIDUAL_BOUGHT_TAGGER_MODEL,
            //商品购买 WQ
            MISSION_INDIVIDUAL_ADD_SHOPITEM,
            //钻石金币兑换
            MISSION_INDIVIDUAL_EXCHANGESCORE
        };

        public CGPIndividualMission(byte[] url, int port) : base(url, port)
        {
            m_bRevStop = (true);
            mMissionType = (byte) Type.MISSION_INDIVIDUAL_NULL;
            mIGPIndividualMissionSink = null;

            mModifyIndividual = new tagModifyIndividual();
            mModifyIndividual.szNickName = new byte[SocketDefines.LEN_UNDER_WRITE];
            mModifyIndividual.szHeadHttp = new byte[SocketDefines.LEN_USER_NOTE];

            m_cSurrogate = null;
            m_tip = null;
        }

        // 设置回调接口
        public void setMissionSink(IGPIndividualMissionSink pIGPIndividualMissionSink)
        {
            mIGPIndividualMissionSink = pIGPIndividualMissionSink;
        }

// 查询个人资料
        public void query(int iAccountID, bool bRecStop = true)
        {
            //mChen add, for headPic
            return;

            m_bRevStop = bRecStop;

            mAccountInfoID = (uint)iAccountID;

            mMissionType = Type.MISSION_INDIVIDUAL_QUERY;

            start();
        }

// 查询个人资料
        public void queryAccountInfo(int iAccountID = -1)
        {
            if (iAccountID >= 0)
            {
                mAccountInfoID = (uint)iAccountID;
            }
            else
            {
                GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
                tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

                mAccountInfoID = pGlobalUserData.dwUserID;
            }

            mMissionType = Type.MISSION_INDIVIDUAL_Account;
            start();
        }

        public void modifyGender(byte gender)
        {
            /*
            memset(&mModifyIndividual, 0, sizeof(mModifyIndividual));
            mModifyIndividual.cbGender = gender;
            mMissionType = MISSION_INDIVIDUAL_MODIFY;
            start();
            */
        }

        public void modify(string kNickName, byte gender)
        {
            /*
            memset(&mModifyIndividual, 0, sizeof(mModifyIndividual));
            mModifyIndividual.cbGender = gender;
            strncpy(mModifyIndividual.szNickName, kNickName.c_str(), kNickName.size());
            mMissionType = MISSION_INDIVIDUAL_MODIFY;
            start();
            */
        }
        /*
        void modify(tagModifyIndividual individual)
        {
            
        } 
        */

        // 修改推广人
        public void modifySpreader(uint dwSpreaderID)
        {
            m_dwSpreaderID = dwSpreaderID;

            mMissionType = Type.MISSION_INDIVIDUAL_SPREADER;
            start();
        }
        //void modifySpreader(string kSpreaderID)
        //{
        //}

        //mChen add
        //增加/删除推荐人身份
        public void addDelSpreader(uint dwSpreaderID, string szSpreaderRealName, string szSpreaderTelNum, string szSpreaderWeiXinAccount, uint dwParentSpreaderID, ushort wSpreaderLevel, bool bIsAddSpreader)
        {
            m_dwSpreaderID = dwSpreaderID;
            m_szSpreaderRealName = szSpreaderRealName;
            m_dwParentSpreaderID = dwParentSpreaderID;
            m_szSpreaderTelNum = szSpreaderTelNum;
            m_szSpreaderWeiXinAccount = szSpreaderWeiXinAccount;
            ///m_szSpreaderIDCardNo = szSpreaderIDCardNo;
            m_wSpreaderLevel = wSpreaderLevel;
            m_bIsAddSpreader = bIsAddSpreader;

            mMissionType = Type.MISSION_INDIVIDUAL_ADDDEL_SPREADER;
            start();
        }
        //查询代理人列表
        public void querySpreadersInfo(Surrogate cSurrogate)
        {
            m_cSurrogate = cSurrogate;

            mMissionType = Type .MISSION_INDIVIDUAL_QUERY_SPREADERS_INFO;
            start();
        }

        //查询代理人列表
        public void queryTopPlayersInfo(MatchScore cMatch)
        {
            m_CMatchScore = cMatch;

            mMissionType = Type.MISSION_INDIVIDUAL_QUERY_TOP_PLAYERS_INFO;
            start();
        }

        public void queryUserNickNameByUserID(uint userID, TransferDiamond cTransferDiamond)
        {
            m_userIdToQueryNickName = userID;
            m_transferDiamond = cTransferDiamond;
            mMissionType = Type.MISSION_INDIVIDUAL_QUERY_NICKNAME_BY_ID;
            start();
        }

        public void queryPrePayIDByShopID(uint shopItemID)
        {
            m_shopItemID = shopItemID;
            mMissionType = Type.MISSION_INDIVIDUAL_QUERY_PREPAYID;
            start();
        }
        //商品购买 WQ
        public void queryAddShopItem(CMD_GP_ShopItemInfo kNetInfo)
        {
            cmdShopItem.Init();

            Buffer.BlockCopy(kNetInfo.szUID, 0, cmdShopItem.szUID, 0, kNetInfo.szUID.Length);
            Buffer.BlockCopy(kNetInfo.szOrderID, 0, cmdShopItem.szOrderID, 0, kNetInfo.szOrderID.Length);
            cmdShopItem.wItemID = kNetInfo.wItemID;
            cmdShopItem.wAmount = kNetInfo.wAmount;
            cmdShopItem.wCount = kNetInfo.wCount;
            mMissionType = Type.MISSION_INDIVIDUAL_ADD_SHOPITEM;
            start();
        }
        //WQ 钻石金币兑换
        public void requestExchangeScore(byte itemId, byte exchangeType, int amount)
        {
            mMissionType = Type.MISSION_INDIVIDUAL_EXCHANGESCORE;
            cmdExchangeScore.cbItemID = itemId;
            cmdExchangeScore.cbExchangeType = exchangeType;
            cmdExchangeScore.wAmount = (ushort)amount;
            start();
        }
        public void uploadPayInfo()
        {
            if (m_lastTradeNoStr == null || m_lastTradeNoStr != m_TradeNoStr)
            {
                m_lastTradeNoStr = m_TradeNoStr;
                mMissionType = Type.MISSION_INDIVIDUAL_UPLOAD_CLIENT_PAY;
                start();
            }
        }

        public void transferDiamond(uint diamondNum)
        {
            m_diamondNumber = diamondNum;
            mMissionType = Type.MISSION_INDIVIDUAL_TRANSFER_DIAMOND;
            start();
        }
        // 修改昵称
        public void modifyName(string kName)
        {

            //CGlobalUserInfo pGlobalUserInfo = CGlobalUserInfo::GetInstance();
            //tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            //memset(&mModifyIndividual, 0, sizeof(mModifyIndividual));
            //mModifyIndividual.cbGender = pGlobalUserData.cbGender;
            //strncpy(mModifyIndividual.szNickName, kName.c_str(), kName.size());
            byte[] NickName = System.Text.Encoding.UTF8.GetBytes(kName);
            Array.Clear(mModifyIndividual.szNickName, 0, mModifyIndividual.szNickName.Length);
            Buffer.BlockCopy(NickName, 0, mModifyIndividual.szNickName, 0, NickName.Length);
            mMissionType = Type.MISSION_INDIVIDUAL_MODIFY;
            start();

        }
        // 修改头像http
        public void modifyHeadHttp(byte[] kHttp)
        {
            //string strHeadHttp = Encoding.Default.GetString(kHttp);
            //Debug.Log("mChen modifyHeadHttp:HeadHttp =" + strHeadHttp);

            //mModifyIndividual.reset();
            ///memset(&mModifyIndividual, 0, sizeof(mModifyIndividual));

            //var tempBuf = Encoding.Default.GetBytes(kHttp);
            Array.Clear(mModifyIndividual.szHeadHttp,0, mModifyIndividual.szHeadHttp.Length);
            Buffer.BlockCopy(kHttp, 0, mModifyIndividual.szHeadHttp, 0, kHttp.Length);
            ///strncpy(mModifyIndividual.szHeadHttp, kHttp.c_str(), kHttp.size());

            mMissionType = Type.MISSION_INDIVIDUAL_MODIFY;
            start();
        }
        //修改玩家信息
        public void modUserInfo(CMD_GP_ModUserInfo kNetInfo)
        {
            Array.Clear(mModifyIndividual.szNickName, 0, mModifyIndividual.szNickName.Length);
            Buffer.BlockCopy(kNetInfo.szNickName, 0, mModifyIndividual.szNickName, 0, kNetInfo.szNickName.Length);
            Array.Clear(mModifyIndividual.szHeadHttp, 0, mModifyIndividual.szHeadHttp.Length);
            Buffer.BlockCopy(kNetInfo.szHeadHttp, 0, mModifyIndividual.szHeadHttp, 0, kNetInfo.szHeadHttp.Length);
            mMissionType = Type.MISSION_INDIVIDUAL_MODIFY;
            start();
        }

// 渠道号
        public void modifyUserChannel(string szUserChannel)
        {
            
        }

        public void modifyPhoneNumber(string kPhoneNumber)
        {
            
        }

        //mChen add
        //游戏内购
        public void AddPayment(uint dwPayment, uint dwBoughtDiamond)
        {
            m_dwPayment = dwPayment;
            m_dwBoughtDiamond = dwBoughtDiamond;

            mMissionType = Type.MISSION_INDIVIDUAL_ADD_PAYMENT;
            start();
        }
        //名下用户交易信息
        public void queryChildrenPaymentInfo(Surrogate cSurrogate)
        {
            m_cSurrogate = cSurrogate;

            mMissionType = Type.MISSION_INDIVIDUAL_QUERY_CHILDREN_PAYMENT_INFO;
            start();
        }
        //企业提现
        public void AddEnterprisePay(uint dwPayment)
        {
            m_dwEnterprisePayment = dwPayment;

            mMissionType = Type.MISSION_INDIVIDUAL_ADD_ENTERPRISE_PAYMENT;
            start();
        }

        //mChen add, for HideSeek
        public void BoughtTaggerModel(uint dwPayment, byte cbPaymentType, ushort wBoughtModelIndex)
        {
            m_dwPaymentOfBoughtTaggerModel = dwPayment;
            m_cbPaymentTypeOfBoughtTaggerModel = cbPaymentType;
            m_wBoughtModelIndex = wBoughtModelIndex;

            mMissionType = Type.MISSION_INDIVIDUAL_BOUGHT_TAGGER_MODEL;
            start();
        }


        //////////////////////////////////////////////////////////////////////////
        // ISocketEngineSink
        public override void onEventTCPSocketLink()
        {
            //变量定义
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            switch (mMissionType)
            {
                // 查询个人资料
                case Type.MISSION_INDIVIDUAL_Account:
                    {
                        //变量定义
                        CMD_GP_QueryAccountInfo QueryIndividual  = new CMD_GP_QueryAccountInfo();

                        QueryIndividual.dwUserID = mAccountInfoID;
                        var buf = StructConverterByteArray.StructToBytes(QueryIndividual);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_QUERY_ACCOUNTINFO, buf, buf.Length);
                        break;
                    }
                // 查询个人资料
                case Type.MISSION_INDIVIDUAL_QUERY:
                    {
                        //变量定义
                        CMD_GP_QueryIndividual QueryIndividual = new CMD_GP_QueryIndividual();
                        var buf = StructConverterByteArray.StructToBytes(QueryIndividual);

                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_QUERY_INDIVIDUAL, buf, buf.Length);
                        break;
                    }
                case Type.MISSION_INDIVIDUAL_SPREADER:
                    {
                        //变量定义
                        CMD_GP_ModifySpreader kNetInfo = new CMD_GP_ModifySpreader();
                        kNetInfo.Init();

                        //设置变量
                        kNetInfo.dwUserID = pGlobalUserData.dwUserID;
                        Buffer.BlockCopy(pGlobalUserData.szPassword,0,kNetInfo.szPassword,0,pGlobalUserData.szPassword.Length);
                        //strncpy(kNetInfo.szPassword, pGlobalUserData.szPassword, countarray(kNetInfo.szPassword));
                        kNetInfo.dwSpreaderID = m_dwSpreaderID;
                        //var buf = Encoding.Default.GetBytes(m_kSpreaderID);
                        //Buffer.BlockCopy(buf, 0, kNetInfo.szSpreader, 0, buf.Length);
                        ///strncpy(kNetInfo.szSpreader, m_kSpreaderID.c_str(), countarray(kNetInfo.szSpreader));

                        //发送数据
                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_MODIFY_SPREADER, buf, buf.Length);
                        break;
                    }

                    //mChen add
                case Type.MISSION_INDIVIDUAL_ADDDEL_SPREADER:
                    {
                        //变量定义
                        CMD_GP_AddDelSpreader kNetInfo = new CMD_GP_AddDelSpreader();
                        kNetInfo.Init();

                        //设置变量
                        kNetInfo.dwUserID = pGlobalUserData.dwUserID;
                        Buffer.BlockCopy(pGlobalUserData.szPassword, 0, kNetInfo.szPassword, 0, pGlobalUserData.szPassword.Length);
                        ///strncpy(kNetInfo.szPassword, pGlobalUserData->szPassword, countarray(kNetInfo.szPassword));

                        kNetInfo.dwSpreaderID = m_dwSpreaderID;
                        kNetInfo.dwParentSpreaderID = m_dwParentSpreaderID;
                        kNetInfo.bIsAdd = (byte)(m_bIsAddSpreader ? 1 : 0);

                        var szSpreaderRealName = Encoding.UTF8.GetBytes(m_szSpreaderRealName);
                        int nLen = szSpreaderRealName.Length;
                        if(nLen > kNetInfo.szSpreaderRealName.Length)
                        {
                            //to fix bug
                            nLen = kNetInfo.szSpreaderRealName.Length;
                        }
                        Buffer.BlockCopy(szSpreaderRealName, 0, kNetInfo.szSpreaderRealName, 0, nLen);
                        ///strncpy(kNetInfo.szSpreaderRealName, m_szSpreaderRealName.c_str(), countarray(kNetInfo.szSpreaderRealName));

                        //var szSpreaderIDCardNo = Encoding.Default.GetBytes(m_szSpreaderIDCardNo);
                        //Buffer.BlockCopy(szSpreaderIDCardNo, 0, kNetInfo.szSpreaderIDCardNo, 0, szSpreaderIDCardNo.Length);
                        /////strncpy(kNetInfo.szSpreaderIDCardNo, m_szSpreaderIDCardNo.c_str(), countarray(kNetInfo.szSpreaderIDCardNo));

                        var szSpreaderTelNum = Encoding.Default.GetBytes(m_szSpreaderTelNum);
                        Buffer.BlockCopy(szSpreaderTelNum, 0, kNetInfo.szSpreaderTelNum, 0, szSpreaderTelNum.Length);

                        var szSpreaderWeiXinAccount = Encoding.Default.GetBytes(m_szSpreaderWeiXinAccount);
                        Buffer.BlockCopy(szSpreaderWeiXinAccount, 0, kNetInfo.szSpreaderWeiXinAccount, 0, szSpreaderWeiXinAccount.Length);

                        kNetInfo.wSpreaderLevel = (ushort)m_wSpreaderLevel;

                        //发送数据
                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_ADDDEL_SPREADER, buf, buf.Length);

                        break;
                    }
                    //查询代理人列表
                case Type.MISSION_INDIVIDUAL_QUERY_SPREADERS_INFO:
                    {
                        CMD_GP_QuerySpreadersInfo kNetInfo = new CMD_GP_QuerySpreadersInfo();
                        kNetInfo.Init();
                        ///zeromemory(&kNetInfo, sizeof(kNetInfo));

                        //设置变量
                        kNetInfo.dwUserID = pGlobalUserData.dwUserID;

                        Buffer.BlockCopy(pGlobalUserData.szPassword, 0, kNetInfo.szPassword, 0, pGlobalUserData.szPassword.Length);
                        ///strncpy(kNetInfo.szPassword, pGlobalUserData->szPassword, countarray(kNetInfo.szPassword));

                        //发送数据
                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_QUERY_SPREADERS_INFO, buf, buf.Length);
                        ///send(MDM_GP_USER_SERVICE, SUB_GP_QUERY_SPREADERS_INFO, &kNetInfo, sizeof(kNetInfo));

                        break;
                    }
                case Type.MISSION_INDIVIDUAL_QUERY_TOP_PLAYERS_INFO:
                    {
                        CMD_GR_QueryTopNum kNetInfo = new CMD_GR_QueryTopNum();
                        kNetInfo.dwTopCount = 50;    //排行榜人数
                        //发送数据
                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_MATCH_TOP_LIST, buf, buf.Length);
                        break;
                    }
                case Type.MISSION_INDIVIDUAL_QUERY_NICKNAME_BY_ID:
                {
                    CMD_GR_QueryNickName kNetInfo = new CMD_GR_QueryNickName();
                    kNetInfo.dwUserID = m_userIdToQueryNickName;
                    //发送数据
                    var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                    send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_QUERY_NICKNAME, buf, buf.Length);
                    break;
                }
                case Type.MISSION_INDIVIDUAL_TRANSFER_DIAMOND:
                {
                    CMD_GR_TransferDiamond kNetInfo = new CMD_GR_TransferDiamond();
                    kNetInfo.dwLocalID = GlobalUserInfo.getUserID();
                    kNetInfo.dwUserID = m_userIdToQueryNickName;
                    kNetInfo.dwDiamondNum = m_diamondNumber;
                    //发送数据
                    var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                    send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_TRANSFER_DIAMOND, buf, buf.Length);
                    break;
                }
                // 修改个人资料
                case Type.MISSION_INDIVIDUAL_MODIFY:
                    {

                        //变量定义
                        var cbBuffer = new byte[HeaderStruct.SIZE_PACK_DATA];
                        ///byte cbBuffer[SIZE_PACK_DATA];
                        ///zeromemory(cbBuffer, sizeof(cbBuffer));

                        CMD_GP_ModifyIndividual pModifyIndividual = (CMD_GP_ModifyIndividual)StructConverterByteArray.BytesToStruct(cbBuffer, typeof(CMD_GP_ModifyIndividual));
                        ///CMD_GP_ModifyIndividual * pModifyIndividual = (CMD_GP_ModifyIndividual*)cbBuffer;
                 
                        var sizeOfModifyIndividual = Marshal.SizeOf(typeof(CMD_GP_ModifyIndividual));
                        int wMaxBytes = cbBuffer.Length - sizeOfModifyIndividual;
                        byte[] pcbBuffer = new byte[wMaxBytes];
                        CSendPacketHelper SendPacket = new CSendPacketHelper(pcbBuffer, (ushort)wMaxBytes);
                        ///CSendPacketHelper SendPacket(cbBuffer + sizeof(CMD_GP_ModifyIndividual), sizeof(cbBuffer) - sizeof(CMD_GP_ModifyIndividual));

                        //设置变量
                        pModifyIndividual.cbGender = pGlobalUserData.cbGender;
                        pModifyIndividual.dwUserID = pGlobalUserData.dwUserID;
                        pModifyIndividual.wModCost = 0;
                        Buffer.BlockCopy(pGlobalUserData.szPassword, 0, pModifyIndividual.szPassword, 0, pGlobalUserData.szPassword.Length);

                        //用户昵称
                        if (mModifyIndividual.szNickName[0] != 0 && mModifyIndividual.szNickName.Length >= 6)
                        {
                            //PLAZZ_PRINTF(("szNickName:%s \n"), mModifyIndividual.szNickName);
                            if (mModifyIndividual.szNickName != pGlobalUserData.szNickName)
                            {
                                pModifyIndividual.wModCost += 1;        //修改昵称费用
                                pModifyIndividual.cbModCosttType = 1;   //钻石
                            }
                            SendPacket.AddPacket(mModifyIndividual.szNickName, MsgDefine.DTP_GP_UI_NICKNAME);
                        }

                        //string strHeadHttp1 = Encoding.Default.GetString(mModifyIndividual.szHeadHttp);
                        //Debug.Log("mChen MISSION_INDIVIDUAL_MODIFY:strHeadHttp1=" + strHeadHttp1);

                        //头像数据
                        if (mModifyIndividual.szHeadHttp[0] != 0)
                        {
                            //string strHeadHttp = Encoding.Default.GetString(mModifyIndividual.szHeadHttp);
                            //Debug.Log("mChen send MISSION_INDIVIDUAL_MODIFY:HeadHttp =" + strHeadHttp);

                            SendPacket.AddPacket(mModifyIndividual.szHeadHttp, MsgDefine.DTP_GP_UI_HEAD_HTTP);
                        }

                        //拷贝数据
                        var buf = StructConverterByteArray.StructToBytes(pModifyIndividual);
                        Buffer.BlockCopy(buf, 0, cbBuffer, 0, sizeOfModifyIndividual);
                        Buffer.BlockCopy(pcbBuffer, 0, cbBuffer, sizeOfModifyIndividual, pcbBuffer.Length);

                        //发送数据
                        int wSendSize = sizeOfModifyIndividual + SendPacket.GetDataSize();
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_MODIFY_INDIVIDUAL, cbBuffer, wSendSize);

                        /*
                        //变量定义
                        CMD_GP_ModifyIndividual * pModifyIndividual = (CMD_GP_ModifyIndividual*)cbBuffer;
                        CSendPacketHelper SendPacket(cbBuffer + sizeof(CMD_GP_ModifyIndividual), sizeof(cbBuffer) - sizeof(CMD_GP_ModifyIndividual));

                        //设置变量
                        pModifyIndividual.cbGender = mModifyIndividual.cbGender;
                        pModifyIndividual.dwUserID = pGlobalUserData.dwUserID;
                        strncpy(pModifyIndividual.szPassword, pGlobalUserData.szPassword, countarray(pModifyIndividual.szPassword));

                        //用户昵称
                        if (mModifyIndividual.szNickName[0] != 0 && countarray(mModifyIndividual.szNickName) >= 6)
                        {
                            PLAZZ_PRINTF(("szNickName:%s \n"), mModifyIndividual.szNickName);
                            SendPacket.AddPacket(mModifyIndividual.szNickName, DTP_GP_UI_NICKNAME);
                        }

                        //个性签名
                        if (mModifyIndividual.szUnderWrite[0] != 0)
                        {
                            PLAZZ_PRINTF(("szUnderWrite:%s \n"), mModifyIndividual.szUnderWrite);
                            SendPacket.AddPacket(mModifyIndividual.szUnderWrite, DTP_GP_UI_UNDER_WRITE);
                        }

                        //用户备注
                        if (mModifyIndividual.szUserNote[0] != 0)
                        {
                            SendPacket.AddPacket(mModifyIndividual.szUserNote, DTP_GP_UI_USER_NOTE);
                        }

                        //真实名字
                        if (mModifyIndividual.szCompellation[0] != 0)
                        {
                            SendPacket.AddPacket(mModifyIndividual.szCompellation, DTP_GP_UI_COMPELLATION);
                        }

                        //固定号码
                        if (mModifyIndividual.szSeatPhone[0] != 0)
                        {
                            SendPacket.AddPacket(mModifyIndividual.szSeatPhone, DTP_GP_UI_SEAT_PHONE);
                        }

                        //手机号码
                        if (mModifyIndividual.szMobilePhone[0] != 0)
                        {
                            SendPacket.AddPacket(mModifyIndividual.szMobilePhone, DTP_GP_UI_MOBILE_PHONE);
                        }

                        //Q Q 号码
                        if (mModifyIndividual.szQQ[0] != 0)
                        {
                            SendPacket.AddPacket(mModifyIndividual.szQQ, DTP_GP_UI_QQ);
                        }

                        //电子邮件
                        if (mModifyIndividual.szEMail[0] != 0)
                        {
                            SendPacket.AddPacket(mModifyIndividual.szEMail, DTP_GP_UI_EMAIL);
                        }

                        //详细地址
                        if (mModifyIndividual.szDwellingPlace[0] != 0)
                        {
                            SendPacket.AddPacket(mModifyIndividual.szDwellingPlace, DTP_GP_UI_DWELLING_PLACE);
                        }

                        //详细地址
                        if (mModifyIndividual.szHeadHttp[0] != 0)
                        {
                            SendPacket.AddPacket(mModifyIndividual.szHeadHttp, DTP_GP_UI_HEAD_HTTP);
                        }

                        //详细地址
                        if (mModifyIndividual.szUserChannel[0] != 0)
                        {
                            SendPacket.AddPacket(mModifyIndividual.szUserChannel, DTP_GP_UI_CHANNEL);
                        }

                        //发送数据
                        word wSendSize = sizeof(CMD_GP_ModifyIndividual) + SendPacket.GetDataSize();
                        send(MDM_GP_USER_SERVICE, SUB_GP_MODIFY_INDIVIDUAL, cbBuffer, wSendSize);
                        */
                        break;
                    }
                case Type.MISSION_INDIVIDUAL_ADD_PAYMENT:
                    {
                        CMD_GP_AddPayment kNetInfo = new CMD_GP_AddPayment();
                        kNetInfo.Init();

                        //设置变量
                        kNetInfo.dwUserID = pGlobalUserData.dwUserID;
                        kNetInfo.dwPayment = m_dwPayment;
                        kNetInfo.dwBoughtDiamond = m_dwBoughtDiamond;

                        Buffer.BlockCopy(pGlobalUserData.szPassword, 0, kNetInfo.szPassword, 0, pGlobalUserData.szPassword.Length);

                        //发送数据
                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_ADD_PAYMENT, buf, buf.Length);

                        break;
                    }
                case Type.MISSION_INDIVIDUAL_QUERY_CHILDREN_PAYMENT_INFO:
                    {

                        CMD_GP_QueryChildrenPaymentInfo kNetInfo = new CMD_GP_QueryChildrenPaymentInfo();
                        kNetInfo.Init();
                        ///zeromemory(&kNetInfo, sizeof(kNetInfo));

                        //设置变量
                        kNetInfo.dwUserID = pGlobalUserData.dwUserID;

                        Buffer.BlockCopy(pGlobalUserData.szPassword, 0, kNetInfo.szPassword, 0, pGlobalUserData.szPassword.Length);
                        ///strncpy(kNetInfo.szPassword, pGlobalUserData->szPassword, countarray(kNetInfo.szPassword));

                        //发送数据
                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_QUERY_CHILDREN_PAYMENT_INFO, buf, buf.Length);

                        break;
                    }
                case Type.MISSION_INDIVIDUAL_QUERY_PREPAYID:
                    {
                        CMD_GP_QueryPrePayID kNetInfo = new CMD_GP_QueryPrePayID();
                        
                        //设置变量
                        kNetInfo.dwShopItemID = m_shopItemID;
                        kNetInfo.dwUserID = GlobalUserInfo.getUserID();
                        //发送数据
                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_QUERY_PREPAYID, buf, buf.Length);
                        break;
                    }
                case Type.MISSION_INDIVIDUAL_UPLOAD_CLIENT_PAY:
                {
                    CMD_GP_ClientPayInfo cPay = new CMD_GP_ClientPayInfo();
                    cPay.dwUserID = GlobalUserInfo.getUserID();
                    cPay.cbSuccessState = (byte)(1);
                    cPay.szTradeNo = new byte[32];
                    Buffer.BlockCopy(m_TradeNoStr, 0, cPay.szTradeNo, 0, 32);
                    var buf = StructConverterByteArray.StructToBytes(cPay);
                    send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_UPLOAD_PAY_INFO, buf, buf.Length);
                    break;
                }
                case Type.MISSION_INDIVIDUAL_ADD_ENTERPRISE_PAYMENT:
                    {
                        CMD_GP_AddEnterprisePayment kNetInfo = new CMD_GP_AddEnterprisePayment();
                        kNetInfo.Init();

                        //设置变量
                        kNetInfo.dwUserID = pGlobalUserData.dwUserID;
                        kNetInfo.dwEnterprisePayment = m_dwEnterprisePayment;

                        Buffer.BlockCopy(pGlobalUserData.szPassword, 0, kNetInfo.szPassword, 0, pGlobalUserData.szPassword.Length);

                        //发送数据
                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_ADD_ENTERPRISE_PAYMENT, buf, buf.Length);

                        break;
                    }

                    //mChen add, for HideSeek
                case Type.MISSION_INDIVIDUAL_BOUGHT_TAGGER_MODEL:
                    {
                        CMD_GP_BoughtTaggerModel kNetInfo = new CMD_GP_BoughtTaggerModel();
                        kNetInfo.Init();

                        //设置变量
                        kNetInfo.dwUserID = pGlobalUserData.dwUserID;
                        kNetInfo.dwPayment = m_dwPaymentOfBoughtTaggerModel;
                        kNetInfo.cbPaymentType = m_cbPaymentTypeOfBoughtTaggerModel;
                        kNetInfo.wBoughtModelIndex = m_wBoughtModelIndex;

                        Buffer.BlockCopy(pGlobalUserData.szPassword, 0, kNetInfo.szPassword, 0, pGlobalUserData.szPassword.Length);

                        //发送数据
                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_BOUGHT_TAGGER_MODEL, buf, buf.Length);

                        break;
                    }
                case Type.MISSION_INDIVIDUAL_ADD_SHOPITEM:  //商品购买 WQ
                    {
                        CMD_GP_ShopItemInfo kNetInfo = new CMD_GP_ShopItemInfo();
                        kNetInfo.Init();

                        var typeValue = typeof(CMD_GP_ShopItemInfo);
                        int ss = Marshal.SizeOf(typeValue);

                        kNetInfo.dwUserID = GlobalUserInfo.getUserID();
                        Buffer.BlockCopy(cmdShopItem.szUID, 0, kNetInfo.szUID, 0, cmdShopItem.szUID.Length);
                        Buffer.BlockCopy(cmdShopItem.szOrderID, 0, kNetInfo.szOrderID, 0, cmdShopItem.szOrderID.Length);
                        kNetInfo.wItemID = cmdShopItem.wItemID;
                        kNetInfo.wAmount = cmdShopItem.wAmount;
                        kNetInfo.wCount = cmdShopItem.wCount;
                        //发送数据
                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_ADDSHOPITEM, buf, buf.Length);

                        break;
                    }
                case Type.MISSION_INDIVIDUAL_EXCHANGESCORE:
                    {
                        CMD_GP_ExchangScoreInfo kNetInfo = new CMD_GP_ExchangScoreInfo();

                        kNetInfo.dwUserID = GlobalUserInfo.getUserID();
                        kNetInfo.cbItemID = cmdExchangeScore.cbItemID;
                        kNetInfo.wAmount = cmdExchangeScore.wAmount;
                        kNetInfo.cbExchangeType = cmdExchangeScore.cbExchangeType;
                        //发送数据
                        var buf = StructConverterByteArray.StructToBytes(kNetInfo);
                        send(MsgDefine.MDM_GP_USER_SERVICE, MsgDefine.SUB_GP_EXCHANGESCORE, buf, buf.Length);
                        break;
                    }
            }
        }

        public override void onEventTCPSocketShut()
        {
            
        }

        public override void onEventTCPSocketError(Exception errorCode)
        {
            Debug.Log("IndividualMission exception: " + errorCode.Message);
        }

        public override bool onEventTCPSocketRead(int main, int sub, byte[] data, int dataSize)
        {
            if (main != MsgDefine.MDM_GP_USER_SERVICE)
            {
                return false;
            }

            Debug.Log("IndividualMission Read Sub " + sub + " data size " + dataSize);

            switch (sub)
            {
                //个人信息
                case MsgDefine.SUB_GP_QUERY_ACCOUNTINFO: return onSubUserAccountInfo(data, dataSize);
                //个人信息
                case MsgDefine.SUB_GP_USER_INDIVIDUAL: return onSubUserIndividual(data, dataSize);
                //设置推荐人结果
                case MsgDefine.SUB_GP_SPREADER_RESOULT: return onSubSpreaderResoult(data, dataSize);

                //mChen add:查询代理人列表结果
                case MsgDefine.SUB_GP_SPREADERS_INFO_RESOULT: return onSubSpreadersInfoResoult(data, dataSize);

                case MsgDefine.SUB_GP_TOP_PLAYERS_INFO_RESOULT: return onSubTopPlayersInfoResoult(data, dataSize);
                case MsgDefine.SUB_GP_NICKNAME_INFO: return onSubNickNameInfoResoult(data, dataSize);
                case MsgDefine.SUB_GP_TRANSFER_DIAMOND_RESULT: return onSubTransferDiamondResult(data, dataSize);
 				//内购
                case MsgDefine.SUB_GP_ADD_PAYMENT_RESULT: return onSubAddPaymentResult(data, dataSize);
                //名下用户交易信息
                case MsgDefine.SUB_GP_CHILDREN_PAYMENT_INFO_RESULT: return onSubChildrenPaymentInfoResult(data, dataSize);
                case MsgDefine.SUB_GP_QUERY_PREPAYID: return onPrePayIDResoult(data, dataSize);
                case MsgDefine.SUB_GP_UPLOAD_PAY_INFO: return onPayInfoResoult(data, dataSize);
                //企业提现
                case MsgDefine.SUB_GP_ADD_ENTERPRISE_PAYMENT_RESULT: return onSubAddEnterprisePaymentResult(data, dataSize);

                //操作成功
                case MsgDefine.SUB_GP_OPERATE_SUCCESS: return onSubOperateSuccess(data, dataSize);
                //操作失败
                case MsgDefine.SUB_GP_OPERATE_FAILURE: return onSubOperateFailure(data, dataSize);

                //mChen add, for HideSeek
                case MsgDefine.SUB_GP_BOUGHT_TAGGER_MODEL_RESULT: return onSubBoughtTaggerModelResoult(data, dataSize);

                //商品购买反馈  WQ
                case MsgDefine.SUB_GP_ADDSHOPITEM_RESULT:  return onSubAddShopItemResult(data, dataSize);
                //钻石金币兑换
                case MsgDefine.SUB_GP_EXCHANGESCORE_RESULT: return onSubExchangeScoreResult(data, dataSize);
            }

            return false;
        }

//////////////////////////////////////////////////////////////////////////
// 子消息处理

        // 个人信息
        bool onSubUserAccountInfo(byte[] data, int size)
        {
            //变量定义
            CMD_GP_UserAccountInfo pAccountInfo = (CMD_GP_UserAccountInfo)StructConverterByteArray.BytesToStruct(data,typeof(CMD_GP_UserAccountInfo));
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            if (pAccountInfo.dwUserID == GlobalUserInfo.getUserID())
            {
                pGlobalUserData.lUserScore = pAccountInfo.lUserScore;
                pGlobalUserData.lUserInsure = pAccountInfo.lUserInsure;
                //保存信息
                pGlobalUserData.wFaceID = pAccountInfo.wFaceID;
                pGlobalUserData.cbGender = pAccountInfo.cbGender;
                pGlobalUserData.dwUserID = pAccountInfo.dwUserID;
                pGlobalUserData.dwGameID = pAccountInfo.dwGameID;
                pGlobalUserData.dwSpreaderID = pAccountInfo.dwSpreaderID;
                pGlobalUserData.dwExperience = pAccountInfo.dwExperience;
                Debug.Log("Server user data :" + GlobalUserInfo.GBToUtf8(pAccountInfo.szNickName));
                Buffer.BlockCopy(pAccountInfo.szNickName,0,pGlobalUserData.szNickName,0,pAccountInfo.szNickName.Length);
                //strcpy(pGlobalUserData.szNickName, utility::a_u8((char)pAccountInfo.szNickName).c_str());
                Buffer.BlockCopy(pAccountInfo.szAccounts, 0, pGlobalUserData.szAccounts, 0, pAccountInfo.szAccounts.Length);
                //strncpy(pGlobalUserData.szAccounts, ((char)pAccountInfo.szAccounts), countarray(pGlobalUserData.szAccounts));
                Buffer.BlockCopy(pAccountInfo.szLogonIp, 0, pGlobalUserData.szLogonIP, 0, pAccountInfo.szLogonIp.Length);
                //strncpy(pGlobalUserData.szLogonIP, ((char)pAccountInfo.szLogonIp), countarray(pGlobalUserData.szLogonIP));
                //金币信息
                pGlobalUserInfo.upPlayerInfo();

                Loom.QueueOnMainThread(() =>
                {
                    if (HNGameManager.GetInstance != null)
                        HNGameManager.GetInstance.ExperienceSystem();
                    if (CreateOrJoinRoom.GetInstance != null)
                        CreateOrJoinRoom.GetInstance.UpdateInfo();
                });

            }
            //if (mIGPIndividualMissionSink!=null)
            //{
            //    mIGPIndividualMissionSink.onGPAccountInfo(pAccountInfo);
            //}

            if (m_bRevStop)
            {
                stop();
            }

            return true;
        }
// 个人信息
        bool onSubUserIndividual(byte[] data, int size)
        {
/*
            CMD_GP_UserIndividual pModifyIndividual = (CMD_GP_UserIndividual)StructConverterByteArray.BytesToStruct(data,typeof(CMD_GP_UserIndividual));


            void pDataBuffer = NULL;
            tagDataDescribe DataDescribe;
            CRecvPacketHelper RecvPacket(pModifyIndividual + 1, size - sizeof(CMD_GP_UserIndividual));

            bool bUpdate = false;
            string kIP, kHttp, kChannel;
            //扩展信息
            while (true)
            {
                pDataBuffer = RecvPacket.GetData(DataDescribe);
                if (DataDescribe.wDataDescribe == DTP_NULL) break;
                switch (DataDescribe.wDataDescribe)
                {
                    case DTP_GP_UI_HEAD_HTTP:   //联系地址
                        {
                            if (DataDescribe.wDataSize <= LEN_USER_NOTE)
                            {
                                bUpdate = true;
                                kHttp.assign((char)pDataBuffer, DataDescribe.wDataSize);

                            }
                            break;
                        }
                    case DTP_GP_UI_IP:  //联系地址
                        {
                            if (DataDescribe.wDataSize <= LEN_NICKNAME)
                            {
                                bUpdate = true;
                                kIP.assign((char)pDataBuffer, DataDescribe.wDataSize);

                            }
                            break;
                        }
                    case DTP_GP_UI_CHANNEL: //联系地址
                        {
                            if (DataDescribe.wDataSize <= LEN_NICKNAME)
                            {
                                bUpdate = true;
                                kChannel.assign((char)pDataBuffer, DataDescribe.wDataSize);

                            }
                            break;
                        }
                }
            }
            CGlobalUserInfo pGlobalUserInfo = CGlobalUserInfo::GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            if (pModifyIndividual.dwUserID == pGlobalUserInfo.getUserID())
            {
                if (kIP != "")
                {
                    strncpy(pGlobalUserData.szLogonIP, (kIP.c_str()), kIP.size());
                }
                if (kHttp != "")
                {
                    strncpy(pGlobalUserData.szHeadHttp, (kHttp.c_str()), kHttp.size());
                }
                if (kChannel != "")
                {
                    strncpy(pGlobalUserData.szUserChannel, (kChannel.c_str()), kChannel.size());
                }
                //金币信息
                pGlobalUserInfo.upPlayerInfo();
            }

            if (bUpdate && mIGPIndividualMissionSink!=null)
                mIGPIndividualMissionSink.onGPAccountInfoHttpIP(pModifyIndividual.dwUserID, kIP, kHttp);

            if (m_bRevStop)
            {
                stop();
            }
            */
            //通知
            if (mIGPIndividualMissionSink != null)
                mIGPIndividualMissionSink.onGPIndividualInfo((int) mMissionType);
            return true;
        }

        // 设置推荐人，增加/删除推荐人结果
        bool onSubSpreaderResoult(byte[] data, int size)
        {
            //变量定义
            CMD_GP_SpreaderResoult pOperateSuccess = (CMD_GP_SpreaderResoult)StructConverterByteArray.BytesToStruct(data,typeof(CMD_GP_SpreaderResoult));

            //效验数据
            if (size < (Marshal.SizeOf(typeof(CMD_GP_SpreaderResoult)) - pOperateSuccess.szDescribeString.Length)) return false;

            //log
            string strDescribe = GlobalUserInfo.GBToUtf8(pOperateSuccess.szDescribeString);// Encoding.UTF8.GetString(pOperateSuccess.szDescribeString);
            Debug.Log("GPIndividualMission:onSubSpreaderResoult: strDescribe =" + strDescribe);
            GameSceneUIHandler.ShowLog(strDescribe);

            //变量定义
            bool bNeedQuerySpreadersInfo = false;
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            if (pOperateSuccess.lResultCode == 0)
            {
                //Success

                pGlobalUserData.lUserInsure = pOperateSuccess.lScore;

                pGlobalUserInfo.upPlayerInfo();

                if(pOperateSuccess.cbOperateType == 0)//操作类型：0-绑定代理，1-增加代理，2-删除代理
                {
                    //绑定代理

                    if (pGlobalUserData.dwSpreaderID == 0)
                    {
                        pGlobalUserData.dwSpreaderID = pOperateSuccess.dwBindSpreaderId;
                    }
                }
                else
                {
                    //增加,删除代理

                    if (pOperateSuccess.cbOperateType == 2)
                    {
                        //删除代理
                        bNeedQuerySpreadersInfo = true;

                        Loom.QueueOnMainThread(() =>
                        {
                            if (m_cSurrogate != null)
                            {
                                querySpreadersInfo(m_cSurrogate);
                                Debug.Log("GPIndividualMission:onSubSpreaderResoult: start querySpreadersInfo");
                            }
                        });
                    }
   
                }
            }

            //关闭连接
            if (m_bRevStop && !bNeedQuerySpreadersInfo)
            {
                stop();
            }

            //log
            Loom.QueueOnMainThread(() =>
            {
                if (pOperateSuccess.cbOperateType == 0)//操作类型：0-绑定代理，1-增加代理，2-删除代理
                {
                    if (m_tip == null)
                    {
                        var inviteWindow = GameObject.Find("InviteWindow");
                        if (inviteWindow != null)
                        {
                            m_tip = inviteWindow.GetComponent<Tip>();
                        }
                    }
                    if (m_tip != null)
                    {
                        bool bIsSuccess = (pOperateSuccess.lResultCode == 0);
                        m_tip.HandleBindResult(bIsSuccess, strDescribe);
                    }
                }
                else
                {
                    if (m_cSurrogate == null)
                    {
                        var surrogateWindow = GameObject.Find("SurrogateWindow");
                        if (surrogateWindow != null)
                        {
                            m_cSurrogate = surrogateWindow.GetComponent<Surrogate>();
                        }
                    }
                    if (m_cSurrogate != null)
                    {
                        m_cSurrogate.ShowLog(strDescribe);
                    }
                }
                    
            });

            //显示消息
            if (mIGPIndividualMissionSink!=null)
                mIGPIndividualMissionSink.onGPIndividualSuccess((int)mMissionType, pOperateSuccess.szDescribeString);
            return true;
        }


        //mChen add
        //查询代理人列表结果
        bool onSubSpreadersInfoResoult(byte[] data, int size)
        {
            //变量定义
            CMD_GP_SpreadersInfoResoult pSpreaderInfoResoult = new CMD_GP_SpreadersInfoResoult();
            pSpreaderInfoResoult.StreamValue(data, size);

            //CMD_GP_SpreadersInfoResoult pSpreaderInfoResoult = (CMD_GP_SpreadersInfoResoult)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GP_SpreadersInfoResoult));
            ///CMD_GP_SpreadersInfoResoult pSpreaderInfoResoult = (CMD_GP_SpreadersInfoResoult*)data;

            //效验数据
            ushort wDataSize = (ushort)(Marshal.SizeOf(typeof(SpreaderInfoItem)) * pSpreaderInfoResoult.SpreaderInfoItems.Length);//sizeof(pSpreaderInfoResoult->SpreaderInfoItems);
            ushort wEffectDataSize = (ushort)(Marshal.SizeOf(typeof(SpreaderInfoItem)) * pSpreaderInfoResoult.wItemCount);// sizeof(SpreaderInfoItem) * pSpreaderInfoResoult->wItemCount;
            ushort wHeadSize = (ushort)(Marshal.SizeOf(typeof(CMD_GP_SpreadersInfoResoult)) - wDataSize);// sizeof(CMD_GP_SpreadersInfoResoult) - wDataSize;
            ///ASSERT(size >= wHeadSize);
            if (size < wHeadSize) return false;

            //log
            string strDescribe = GlobalUserInfo.GBToUtf8(pSpreaderInfoResoult.szDescribeString);// Encoding.UTF8.GetString(pSpreaderInfoResoult.szDescribeString);
            Debug.Log("strDescribe =" + strDescribe);
            //GameSceneUIHandler.ShowLog(strDescribe);

            //关闭连接
            if (m_bRevStop)
            {
                if (pSpreaderInfoResoult.wPacketIdx >= pSpreaderInfoResoult.wPacketNum - 1)
                {
                    stop();
                } 
            }

            //显示消息
            ///NoticeMsg::Instance().ShowTopMsg(utility::a_u8(pSpreaderInfoResoult->szDescribeString));

            ////更新UI列表
            //if (mIGPIndividualMissionSink)
            //{
            //	///mIGPIndividualMissionSink ---> GameBase
            //	mIGPIndividualMissionSink->onGPSpreadersInfo(pSpreaderInfoResoult->wItemCount, pSpreaderInfoResoult->SpreaderInfoItems);
            //}

            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagSpreadersInfo pGlobalSpreadersInfo = pGlobalUserInfo.GetSpreadersInfo();
            if (pSpreaderInfoResoult.wPacketIdx==0)
            {
                pGlobalSpreadersInfo.wItemCount = pSpreaderInfoResoult.wItemCount;
            }
            else
            {
                pGlobalSpreadersInfo.wItemCount += pSpreaderInfoResoult.wItemCount;
            }

            Array.Copy(pSpreaderInfoResoult.SpreaderInfoItems, 0, pGlobalSpreadersInfo.SpreaderInfoItems, pSpreaderInfoResoult.wPacketIdx*GlobalUserInfo.Max_Spreaders_Num_PerSend, pSpreaderInfoResoult.wItemCount);
            ///memcpy(pGlobalSpreadersInfo->SpreaderInfoItems, pSpreaderInfoResoult->SpreaderInfoItems, wEffectDataSize);

            if (pSpreaderInfoResoult.wPacketIdx >= pSpreaderInfoResoult.wPacketNum - 1)
            {
                //pGlobalUserInfo.onGPSpreadersInfo();
                //HNHomeScence::Instance().CreateSpreadersListView(count, spreaderInfoItems);
                Loom.QueueOnMainThread(() =>
                {
                    if (m_cSurrogate != null)
                    {
                        ///m_cSurrogate.ShowLog(strDescribe);

                        m_cSurrogate.UpdateSpreadersListView();
                    }
                });
            }

            return true;
        }


        //mChen add, for HideSeek
        bool onSubBoughtTaggerModelResoult(byte[] data, int size)
        {
            //变量定义
            CMD_GP_BoughtTaggerModelResult pOperateSuccess = (CMD_GP_BoughtTaggerModelResult)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GP_BoughtTaggerModelResult));

            //效验数据
            if (size < (Marshal.SizeOf(typeof(CMD_GP_BoughtTaggerModelResult)) - pOperateSuccess.szDescribeString.Length)) return false;

            //log
            string strDescribe = GlobalUserInfo.GBToUtf8(pOperateSuccess.szDescribeString);// Encoding.UTF8.GetString(pOperateSuccess.szDescribeString);
            Debug.Log("GPIndividualMission:onSubBoughtTaggerModelResoult: strDescribe =" + strDescribe);
            GameSceneUIHandler.ShowLog(strDescribe);

            //变量定义
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            if (pOperateSuccess.lResultCode == 0)
            {
                //Success

                pGlobalUserData.lUserScore = pOperateSuccess.lUserScore;
                pGlobalUserData.lUserInsure = pOperateSuccess.lUserInsure;
                pGlobalUserData.lModelIndex0 |= (1 << pOperateSuccess.wBoughtModelIndex);

                Loom.QueueOnMainThread(() =>
                {
                    GameObject shopItem = GameObject.Find("RechargeView/Viewport/Content/" + pOperateSuccess.wBoughtModelIndex);
                    if (shopItem != null)
                    {
                        ShopItemStatus shopItemStatus = shopItem.GetComponent<ShopItemStatus>();
                        if (shopItemStatus != null)
                        {
                            shopItemStatus.OnBought();
                        }
                    }
                });
            }

            //关闭连接
            if (m_bRevStop)
            {
                stop();
            }

            return true;
        }


        //mChen add
        //企业提现
        bool onSubAddEnterprisePaymentResult(byte[] data, int size)
        {
            //变量定义
            CMD_GP_AddEnterprisePaymentResult pOperateSuccess = (CMD_GP_AddEnterprisePaymentResult)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GP_AddEnterprisePaymentResult));

            //效验数据
            if (size < (Marshal.SizeOf(typeof(CMD_GP_AddEnterprisePaymentResult)) - pOperateSuccess.szDescribeString.Length)) return false;

            //log
            string strDescribe = GlobalUserInfo.GBToUtf8(pOperateSuccess.szDescribeString);// Encoding.UTF8.GetString(pOperateSuccess.szDescribeString);
            Debug.Log("GPIndividualMission:onSubAddEnterprisePaymentResult: strDescribe =" + strDescribe);
            GameSceneUIHandler.ShowLog(strDescribe);

            //变量定义
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            if (pOperateSuccess.lResultCode == 0)
            {
                //Success
            }

            //log
            Loom.QueueOnMainThread(() =>
            {
                if (m_cSurrogate != null)
                {
                    m_cSurrogate.EnableCashOut();
                }
            });

            //关闭连接
            if (m_bRevStop)
            {
                stop();
            }

            return true;
        }
        //内购
        bool onSubAddPaymentResult(byte[] data, int size)
        {
            //变量定义
            CMD_GP_AddPaymentResult pOperateSuccess = (CMD_GP_AddPaymentResult)StructConverterByteArray.BytesToStruct(data, typeof(CMD_GP_AddPaymentResult));

            //效验数据
            if (size < (Marshal.SizeOf(typeof(CMD_GP_AddPaymentResult)) - pOperateSuccess.szDescribeString.Length)) return false;

            //log
            string strDescribe = GlobalUserInfo.GBToUtf8(pOperateSuccess.szDescribeString);// Encoding.UTF8.GetString(pOperateSuccess.szDescribeString);
            Debug.Log("GPIndividualMission:onSubSpreaderResoult: strDescribe =" + strDescribe);
            GameSceneUIHandler.ShowLog(strDescribe);

            //变量定义
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
            if (pOperateSuccess.lResultCode == 0)
            {
                //Success
                pGlobalUserData.lUserInsure = pOperateSuccess.dwFinalInsureScore;

                pGlobalUserInfo.upPlayerInfo();

            }

            //关闭连接
            if (m_bRevStop)
            {
                stop();
            }

            return true;
        }
        //名下用户交易信息
        bool onSubChildrenPaymentInfoResult(byte[] data, int size)
        {
            //变量定义
            CMD_GP_ChildrenPaymentInfoResult pChildrenPaymentInfoResult = new CMD_GP_ChildrenPaymentInfoResult();
            pChildrenPaymentInfoResult.StreamValue(data, size);

            //效验数据
            ushort wDataSize = (ushort)(Marshal.SizeOf(typeof(PaymentInfoItem)) * pChildrenPaymentInfoResult.PaymentInfoItems.Length);
            ushort wEffectDataSize = (ushort)(Marshal.SizeOf(typeof(PaymentInfoItem)) * pChildrenPaymentInfoResult.wItemCount);
            ushort wHeadSize = (ushort)(Marshal.SizeOf(typeof(CMD_GP_ChildrenPaymentInfoResult)) - wDataSize);
            ///ASSERT(size >= wHeadSize);
            if (size < wHeadSize) return false;

            //log
            string strDescribe = GlobalUserInfo.GBToUtf8(pChildrenPaymentInfoResult.szDescribeString);
            Debug.Log("strDescribe =" + strDescribe);
            GameSceneUIHandler.ShowLog(strDescribe);

            //关闭连接
            if (m_bRevStop)
            {
                if (pChildrenPaymentInfoResult.wPacketIdx >= pChildrenPaymentInfoResult.wPacketNum - 1)
                {
                    stop();
                }
            }

            //显示消息
            ///NoticeMsg::Instance().ShowTopMsg(utility::a_u8(pChildrenPaymentInfoResult->szDescribeString));

            ////更新UI列表
            //if (mIGPIndividualMissionSink)
            //{
            //	///mIGPIndividualMissionSink ---> GameBase
            //	mIGPIndividualMissionSink->onGPSpreadersInfo(pChildrenPaymentInfoResult->wItemCount, pChildrenPaymentInfoResult->PaymentInfoItems);
            //}

            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagChildrenPaymentInfo pGlobalChildrenPaymentInfo = pGlobalUserInfo.GetChildrenPaymentInfo();
            pGlobalChildrenPaymentInfo.dTotalGrantOfChildrenBuy = pChildrenPaymentInfoResult.dTotalGrantOfChildrenBuy;
            pGlobalChildrenPaymentInfo.dExtraCash = pChildrenPaymentInfoResult.dExtraCash;
            pGlobalChildrenPaymentInfo.dCashedOut = pChildrenPaymentInfoResult.dCashedOut;
            pGlobalChildrenPaymentInfo.dTotalLeftCash = pChildrenPaymentInfoResult.dTotalLeftCash;
            if (pChildrenPaymentInfoResult.wPacketIdx == 0)
            {
                pGlobalChildrenPaymentInfo.wItemCount = pChildrenPaymentInfoResult.wItemCount;
            }
            else
            {
                pGlobalChildrenPaymentInfo.wItemCount += pChildrenPaymentInfoResult.wItemCount;
            }
            Array.Copy(pChildrenPaymentInfoResult.PaymentInfoItems, 0, pGlobalChildrenPaymentInfo.PaymentInfoItems, pChildrenPaymentInfoResult.wPacketIdx * GlobalUserInfo.Max_PaymentInfo_Num_PerSend, pChildrenPaymentInfoResult.wItemCount);

            if (pChildrenPaymentInfoResult.wPacketIdx >= pChildrenPaymentInfoResult.wPacketNum - 1)
            {
                //pGlobalUserInfo.onGPSpreadersInfo();
                //HNHomeScence::Instance().CreateSpreadersListView(count, spreaderInfoItems);
                Loom.QueueOnMainThread(() =>
                {
                    if (m_cSurrogate != null)
                    {
                        ///m_cSurrogate.ShowLog(strDescribe);

                        m_cSurrogate.UpdateChildrenPaymentListView();
                    }
                });
            }

            return true;
        }
        
        //得到PrePayId，开始下单
        bool onPrePayIDResoult(byte[] data, int size)
        {
            var typeVal = typeof(CMD_GP_PrePayIDInfo);
            if (size != Marshal.SizeOf(typeVal)) return false;
            CMD_GP_PrePayIDInfo prePayInfo =
                (CMD_GP_PrePayIDInfo)StructConverterByteArray.BytesToStruct(data, typeVal);
           
            //关闭连接
            if (m_bRevStop)
            {
                stop();
            }
            m_TradeNoStr = new byte[32];
            Buffer.BlockCopy(prePayInfo.szTradeNo, 0, m_TradeNoStr, 0, 32);
            HNGameManager.WechatPay(ref prePayInfo);
            return true;
        }

        //得到返回值，更新钻石数目
        bool onPayInfoResoult(byte[] data, int size)
        {
            var typeVal = typeof(CMD_GP_ClientPayInfoResoult);
            if (size != Marshal.SizeOf(typeVal)) return false;
            CMD_GP_ClientPayInfoResoult prePayInfo =
                (CMD_GP_ClientPayInfoResoult)StructConverterByteArray.BytesToStruct(data, typeVal);

            //关闭连接
            if (m_bRevStop)
            {
                stop();
            }

            GameSceneUIHandler.ShowLog(GlobalUserInfo.GBToUtf8(prePayInfo.szMsg));
            GlobalUserInfo.setUserInsure(prePayInfo.dwInsureGold);
            return true;
        }

        bool onSubAddShopItemResult(byte[] data, int size)
        {
            var typeVal = typeof(CMD_GP_ShopItemInfoResult);
            if (size != Marshal.SizeOf(typeVal)) return false;
            CMD_GP_ShopItemInfoResult InfoResult = (CMD_GP_ShopItemInfoResult)StructConverterByteArray.BytesToStruct(data, typeVal);

            //关闭连接
            if (m_bRevStop)
            {
                stop();
            }
            if (InfoResult.cbSuccess == 0)  //成功
            {
                //DOTO 更新钻石
                //queryAccountInfo();
                GlobalUserInfo.setUserInsure(InfoResult.dwFinalInsureScore);
            }
            Loom.QueueOnMainThread(()=>
            {
                if (CreateOrJoinRoom.GetInstance != null)
                    CreateOrJoinRoom.GetInstance.UpdateInfo();
            });

            //string str = System.Text.Encoding.UTF8.GetString(InfoResult.szDescribeString);
            string str = GlobalUserInfo.GBToUtf8(InfoResult.szDescribeString);
            GameSceneUIHandler.ShowLog(str);
            return true;
        }
        //WQ 钻石金币兑换
        bool onSubExchangeScoreResult(byte[] data, int size)
        {
            var typeVal = typeof(CMD_GP_ExchangScoreInfoResult);
            if (size != Marshal.SizeOf(typeVal)) return false;
            CMD_GP_ExchangScoreInfoResult InfoResult = (CMD_GP_ExchangScoreInfoResult)StructConverterByteArray.BytesToStruct(data, typeVal);

            //关闭连接
            if (m_bRevStop)
            {
                stop();
            }
            string str = GlobalUserInfo.GBToUtf8(InfoResult.szDescribeString);
            Debug.Log("onSubInventoryConsumptionResult: " + str);
            if (InfoResult.cbSuccess == 0)  //成功
            {
                GlobalUserInfo.setUserScore(InfoResult.dwFinalScore);
                GlobalUserInfo.setUserInsure(InfoResult.dwFinalInsure);

                Loom.QueueOnMainThread(() =>
                {
                    if (UIManager.GetInstance() != null)
                        UIManager.GetInstance().UpdateUIInfo();   //更新钻石金币
                    if (CreateOrJoinRoom.GetInstance != null)
                        CreateOrJoinRoom.GetInstance.UpdateInfo();
                });
            }
            GameSceneUIHandler.ShowLog(str);

            return true;
        }
        //lin add
        //查询比赛积分列表结果
        bool onSubTopPlayersInfoResoult(byte[] data, int size)
        {
            //变量定义
            CMD_GP_TopPlayersInfoResoult pTopPlayersInfoResoult = new CMD_GP_TopPlayersInfoResoult();
            pTopPlayersInfoResoult.StreamValue(data, size);

            //关闭连接
            if (m_bRevStop)
            {
                stop();
            }

            //显示消息
            ///NoticeMsg::Instance().ShowTopMsg(utility::a_u8(pSpreaderInfoResoult->szDescribeString));

            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            /*TopPlayersInfoItem[] pGlobalMatchTopPlayersInfo = pGlobalUserInfo.GetTopPlayersInfo();
            pGlobalMatchTopPlayersInfo = new TopPlayersInfoItem[pTopPlayersInfoResoult.wItemCount];

            Array.Copy(pTopPlayersInfoResoult.TopPlayersInfoItems, 0, pGlobalMatchTopPlayersInfo, 0, pTopPlayersInfoResoult.wItemCount);*/
            ///memcpy(pGlobalSpreadersInfo->SpreaderInfoItems, pSpreaderInfoResoult->SpreaderInfoItems, wEffectDataSize);

            Loom.QueueOnMainThread(() =>
            {
                if (m_CMatchScore != null)
                {
                    m_CMatchScore.CreateTopPlayersListView(ref pTopPlayersInfoResoult.TopPlayersInfoItems);
                }
            });

            return true;
        }


        //lin add
        //查询昵称结果
        bool onSubNickNameInfoResoult(byte[] data, int size)
        {
            var typeVal = typeof (CMD_GP_NickNameInfo_Resoult);
            if (size != Marshal.SizeOf(typeVal)) return false;
            CMD_GP_NickNameInfo_Resoult nickNameInfo =
                (CMD_GP_NickNameInfo_Resoult) StructConverterByteArray.BytesToStruct(data, typeVal);

            //关闭连接
            if (m_bRevStop)
            {
                stop();
            }
            Loom.QueueOnMainThread(() =>
            {
                if (m_transferDiamond != null)
                {
                    if (nickNameInfo.cbSuccess == 0)
                    {
                        m_transferDiamond.ShowNickNameInfo(GlobalUserInfo.GBToUtf8(nickNameInfo.szNickName));
                    }
                    else
                    {
                        m_transferDiamond.ShowErrorTips(GlobalUserInfo.GBToUtf8(nickNameInfo.szDescribeString));
                    }
                }
                else
                {
                    GameSceneUIHandler.ShowLog(GlobalUserInfo.GBToUtf8(nickNameInfo.szDescribeString));
                }
            });
            return true;
        }   
        //lin add
        //转房卡结果
        bool onSubTransferDiamondResult(byte[] data, int size)
        {
            var typeVal = typeof(CMD_GP_Transfer_Diamonds_Resoutl);
            if (size != Marshal.SizeOf(typeVal)) return false;
            CMD_GP_Transfer_Diamonds_Resoutl transferInfo =
                (CMD_GP_Transfer_Diamonds_Resoutl)StructConverterByteArray.BytesToStruct(data, typeVal);

            //关闭连接
            if (m_bRevStop)
            {
                stop();
            }
            Loom.QueueOnMainThread(() =>
            {
                if (m_transferDiamond != null)
                {
                    m_transferDiamond.ShowErrorTips(GlobalUserInfo.GBToUtf8(transferInfo.szDescribeString));
                }
                else
                {
                    GameSceneUIHandler.ShowLog(GlobalUserInfo.GBToUtf8(transferInfo.szDescribeString));
                }
            });

            return true;
        }
        // 操作成功
        bool onSubOperateSuccess(byte[] data, int size)
        {
            //变量定义
            CMD_GP_OperateSuccess pOperateSuccess = (CMD_GP_OperateSuccess)StructConverterByteArray.BytesToStruct(data,typeof(CMD_GP_OperateSuccess));

            //效验数据
           
            if (size < (Marshal.SizeOf(typeof(CMD_GP_OperateSuccess)) - pOperateSuccess.szDescribeString.Length)) return false;

            //log
            string strDescribe = GlobalUserInfo.GBToUtf8(pOperateSuccess.szDescribeString);
            Debug.Log("GPIndividualMission:onSubOperateSuccess: strDescribe =" + strDescribe);

            //变量定义
            GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
            tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

            switch (mMissionType)
            {
                // 查询个人资料
                case Type.MISSION_INDIVIDUAL_SPREADER:
                    {
                        pGlobalUserData.dwSpreaderID = 1;
                        pGlobalUserInfo.upPlayerInfo();
                        break;
                    }
                // 查询个人资料
                case Type.MISSION_INDIVIDUAL_QUERY:
                    {
                        break;
                    }
                // 修改个人资料
                case Type.MISSION_INDIVIDUAL_MODIFY:
                    {
                        if(mModifyIndividual.szNickName[0]!=0)
                        {
                            if (GlobalUserInfo.GBToUtf8(mModifyIndividual.szNickName) != GlobalUserInfo.GBToUtf8(pGlobalUserData.szNickName))
                            {
                                //更新金币钻石
                                Loom.QueueOnMainThread(() =>    //fix因为后面的stop()导致的没有执行
                                {
                                    queryAccountInfo();
                                });
                                //GlobalUserInfo.setUserInsure(pGlobalUserData.lUserInsure - 1);
                                ///GlobalUserInfo.setUserScore(pGlobalUserData.lUserScore - 100);
                            }
                            Array.Clear(pGlobalUserData.szNickName, 0, pGlobalUserData.szNickName.Length);
                            Array.Clear(pGlobalUserData.szHeadHttp, 0, pGlobalUserData.szHeadHttp.Length);
                            Buffer.BlockCopy(mModifyIndividual.szNickName, 0, pGlobalUserData.szNickName, 0, mModifyIndividual.szNickName.Length);
                            Buffer.BlockCopy(mModifyIndividual.szHeadHttp, 0, pGlobalUserData.szHeadHttp, 0, mModifyIndividual.szHeadHttp.Length);
                            pGlobalUserInfo.upPlayerInfo();
                            Loom.QueueOnMainThread(() =>    
                            {
                                if(HNGameManager.bWeChatLogonIn==false)  //PC账号
                                {
                                    PlayerPrefs.SetString("NickName", Encoding.UTF8.GetString(pGlobalUserData.szNickName));
                                    //PlayerPrefs.SetString("HeadURL", Encoding.UTF8.GetString(pGlobalUserData.szHeadHttp));
                                    PlayerPrefs.Save();
                                }
                                else  //移动账号
                                {
                                    PlayerPrefs.SetString("NickName_WX", GlobalUserInfo.GBToUtf8(pGlobalUserData.szNickName));
                                    //PlayerPrefs.SetString("HeadURL_WX", Encoding.UTF8.GetString(pGlobalUserData.szHeadHttp));
                                    PlayerPrefs.Save();
                                }
                                if (CreateOrJoinRoom.GetInstance != null)
                                {
                                    CreateOrJoinRoom.GetInstance.UpdateInfo();
                                    CreateOrJoinRoom.GetInstance.HideUserWin();
                                }
                                if (UserInfoWin.GetInstance != null)
                                    UserInfoWin.GetInstance.UpdateInfo();
                            });

                            GameSceneUIHandler.ShowLog(strDescribe);
                        }
                        /*tagIndividualUserData* pIndividualUserData = pGlobalUserInfo.GetIndividualUserData();

                        //帐号资料
                        pGlobalUserData.cbGender = mModifyIndividual.cbGender;

                        //用户昵称
                        if (mModifyIndividual.szNickName[0] != 0)
                            strcpy(pGlobalUserData.szNickName, utility::a_u8(mModifyIndividual.szNickName).c_str());

                        //个性签名
                        if (mModifyIndividual.szUnderWrite[0] != 0)
                            strncpy(pGlobalUserData.szUnderWrite, mModifyIndividual.szUnderWrite, countarray(pGlobalUserData.szUnderWrite));

                        //详细资料

                        //用户备注
                        if (mModifyIndividual.szUserNote[0] != 0)
                            strncpy(pIndividualUserData.szUserNote, mModifyIndividual.szUserNote, countarray(pIndividualUserData.szUserNote));

                        //真实名字
                        if (mModifyIndividual.szCompellation[0] != 0)
                            strncpy(pIndividualUserData.szCompellation, mModifyIndividual.szCompellation, countarray(pIndividualUserData.szCompellation));

                        //固定号码
                        if (mModifyIndividual.szSeatPhone[0] != 0)
                            strncpy(pIndividualUserData.szSeatPhone, mModifyIndividual.szSeatPhone, countarray(pIndividualUserData.szSeatPhone));

                        //手机号码
                        if (mModifyIndividual.szMobilePhone[0] != 0)
                            strncpy(pIndividualUserData.szMobilePhone, mModifyIndividual.szMobilePhone, countarray(pIndividualUserData.szMobilePhone));

                        //Q Q 号码
                        if (mModifyIndividual.szQQ[0] != 0)
                            strncpy(pIndividualUserData.szQQ, mModifyIndividual.szQQ, countarray(pIndividualUserData.szQQ));

                        //电子邮件
                        if (mModifyIndividual.szEMail[0] != 0)
                            strncpy(pIndividualUserData.szEMail, mModifyIndividual.szEMail, countarray(pIndividualUserData.szEMail));

                        //详细地址
                        if (mModifyIndividual.szHeadHttp[0] != 0)
                            strncpy(pGlobalUserData.szHeadHttp, mModifyIndividual.szHeadHttp, countarray(pGlobalUserData.szHeadHttp));

                        //详细地址
                        if (mModifyIndividual.szUserChannel[0] != 0)
                            strncpy(pGlobalUserData.szUserChannel, mModifyIndividual.szUserChannel, countarray(pGlobalUserData.szUserChannel));

                        pGlobalUserInfo.upPlayerInfo();
                        */
                        break;
                    }
            }

            //关闭连接
            if (m_bRevStop)
            {
                stop();
            }

            //显示消息
            if (mIGPIndividualMissionSink!=null)
                mIGPIndividualMissionSink.onGPIndividualSuccess((int)mMissionType, pOperateSuccess.szDescribeString);
            return true;
        }

        // 操作失败
        bool onSubOperateFailure(byte[] data, int size)
        {
            //效验参数
            CMD_GP_OperateFailure pOperateFailure = (CMD_GP_OperateFailure)StructConverterByteArray.BytesToStruct(data,typeof(CMD_GP_OperateFailure));
           
            if (size < (Marshal.SizeOf(typeof(CMD_GP_OperateFailure)) - (pOperateFailure.szDescribeString.Length))) return false;

            //log
            string strDescribe = GlobalUserInfo.GBToUtf8(pOperateFailure.szDescribeString);//Encoding.UTF8.GetString(pOperateFailure.szDescribeString);
            Debug.Log("GPIndividualMission:onSubOperateFailure: strDescribe =" + strDescribe);

            GameSceneUIHandler.ShowLog(strDescribe);

            //log
            Loom.QueueOnMainThread(() =>
            {
                if (m_cSurrogate == null)
                {
                    var surrogateWindow = GameObject.Find("SurrogateWindow");
                    if (surrogateWindow != null)
                    {
                        m_cSurrogate = surrogateWindow.GetComponent<Surrogate>();
                    }
                }
                if (m_cSurrogate != null)
                {
                    m_cSurrogate.ShowLog(strDescribe);
                }
            });

            //关闭连接
            if (m_bRevStop)
            {
                stop();
            }

            //显示消息
            if (mIGPIndividualMissionSink!=null)
                mIGPIndividualMissionSink.onGPIndividualFailure((int)mMissionType, pOperateFailure.szDescribeString);

            return true;
        }

        private bool m_bRevStop;
        // 任务类型
        Type mMissionType;

        uint mAccountInfoID;

        //修改代理人
        uint m_dwSpreaderID;
        ///string m_kSpreaderID;
        Tip m_tip;

        //mChen add
        //增加/删除推荐人身份
        string m_szSpreaderRealName;
        string m_szSpreaderIDCardNo;
        string m_szSpreaderTelNum;
        string m_szSpreaderWeiXinAccount;
        uint m_dwParentSpreaderID;
        uint m_wSpreaderLevel;
        bool m_bIsAddSpreader;
        //查询代理人列表
        Surrogate m_cSurrogate;
        private MatchScore m_CMatchScore;
        string m_kHeadHttp;
        private uint m_userIdToQueryNickName;
        private TransferDiamond m_transferDiamond;
        private uint m_diamondNumber;
        //商品ID
        private uint m_shopItemID;
        //private string m_Transaction_ID;
        private byte[] m_TradeNoStr;//支付成功后用来查询
        private byte[] m_lastTradeNoStr = null;//上次用来查询的Tradestr数组
        // 回调
        IGPIndividualMissionSink mIGPIndividualMissionSink;

//////////////////////////////////////////////////////////////////////////
// 修改个人资料
//////////////////////////////////////////////////////////////////////////
        tagModifyIndividual mModifyIndividual;

        //mChen add
        //内购
        uint m_dwPayment;         //购买金额（元）
        uint m_dwBoughtDiamond;     //购买到的钻石数
        //企业提现
        uint m_dwEnterprisePayment;

        //mChen add, for HideSeek
        uint m_dwPaymentOfBoughtTaggerModel;
        byte m_cbPaymentTypeOfBoughtTaggerModel;
        ushort m_wBoughtModelIndex;

        //byte[] m_UID = new byte[SocketDefines.LEN_ADDRANK];
        //byte[] m_OderID = new byte[SocketDefines.LEN_USERNOTE];
        //ushort m_ItemID;
        //ushort m_Amount;
        //ushort m_Count;
        CMD_GP_ShopItemInfo cmdShopItem = new CMD_GP_ShopItemInfo();
        CMD_GP_ExchangScoreInfo cmdExchangeScore = new CMD_GP_ExchangScoreInfo();
    }; // CGPIndividualMission
}
