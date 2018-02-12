//////////////////////////////////////////////////////////////////////////////////

//数组说明

using System;
using System.Runtime.InteropServices;
using CGameListItemArray = System.Collections.Generic.List<GameNet.CGameListItem>;
//typedef std::vector<CGameListItem *> CGameListItemArray;

//索引说明
using CGameTypeItemMap = System.Collections.Generic.Dictionary<ushort,GameNet.CGameTypeItem>;
using CGameKindItemMap = System.Collections.Generic.Dictionary<ushort, GameNet.CGameKindItem>;
using CGameServerItemMap = System.Collections.Generic.Dictionary<ushort, GameNet.CGameServerItem>;
using CGameLobbyItemMap = System.Collections.Generic.Dictionary<ushort, GameNet.tagGameLobby>;//mChen add. for HideSeek
//typedef std::map<ushort, CGameTypeItem* > CGameTypeItemMap;
//typedef std::map<ushort, CGameKindItem* > CGameKindItemMap;
//typedef std::map<ushort, CGameServerItem* > CGameServerItemMap;

//////////////////////////////////////////////////////////////////////////////////

namespace GameNet
{
    //////////////////////////////////////////////////////////////////////////////////
    //枚举定义

    //子项类型
    public enum enItemGenre
    {
        ItemGenre_Type, //游戏种类
        ItemGenre_Kind, //游戏类型
        ItemGenre_Node, //游戏节点
        ItemGenre_Page, //游戏页面
        ItemGenre_Server, //游戏房间
        ItemGenre_Inside, //游戏内部
    };

    //房间状态
    public enum enServerStatus
    {
        ServerStatus_Normal, //正常状态
        ServerStatus_Entrance, //正在使用
        ServerStatus_EverEntrance, //曾经进入
    };

    //////////////////////////////////////////////////////////////////////////////////
    /*Do THIS in the header 
    //数组说明
    typedef std::vector<CGameListItem *> CGameListItemArray;

    //索引说明


    typedef std::map<ushort, CGameTypeItem* > CGameTypeItemMap;
    typedef std::map<ushort, CGameKindItem* > CGameKindItemMap;
    typedef std::map<ushort, CGameServerItem* > CGameServerItemMap;
    */
    //////////////////////////////////////////////////////////////////////////////////

    //列表接口
    interface IServerListDataSink
    {
        //状态通知

        //完成通知
        void OnGameItemFinish();
        //完成通知
        void OnGameKindFinish(ushort wKindID);
        //更新通知
        void OnGameItemUpdateFinish();

        //更新通知

        //插入通知
        void OnGameItemInsert(CGameListItem pGameListItem);
        //更新通知
        void OnGameItemUpdate(CGameListItem pGameListItem);
        //删除通知
        void OnGameItemDelete(CGameListItem pGameListItem);
    };

    //////////////////////////////////////////////////////////////////////////////////

    //列表子项
    public class CGameListItem
    {

        //属性数据
        protected enItemGenre m_ItemGenre; //子项类型
        protected CGameListItem m_pParentListItem; //父项子项

        //函数定义
        protected CGameListItem(enItemGenre ItemGenre)
        {
            //属性数据
            m_ItemGenre = ItemGenre;
            m_pParentListItem = null;
        } //构造函数

        //功能函数

        //获取类型
        public enItemGenre GetItemGenre()
        {
            return m_ItemGenre;
        }

        //获取父项
        public CGameListItem GetParentListItem()
        {
            return m_pParentListItem;
        }

        //重载函数
        public virtual ushort GetSortID()
        {
            return 0;
        } //排列索引

    };

    //////////////////////////////////////////////////////////////////////////////////

    //种类结构
    class CGameTypeItem : CGameListItem
    {
        //属性数据
        public tagGameType m_GameType; //种类信息

        //函数定义
        //构造函数
        public CGameTypeItem() : base(enItemGenre.ItemGenre_Type)
        {
        }


        //重载函数

        //排列索引
        public override ushort GetSortID()
        {
            return m_GameType.wSortID;
        }
    };

//////////////////////////////////////////////////////////////////////////////////

    //类型结构
    public class CGameKindItem : CGameListItem
    {
        //属性数据
        public tagGameKind m_GameKind; //类型信息

        //更新变量
        public bool m_bUpdateItem; //更新标志
        public uint m_dwUpdateTime; //更新时间

        //扩展数据
        public uint m_dwProcessVersion; //游戏版本

        //函数定义
        //构造函数
        public CGameKindItem() : base(enItemGenre.ItemGenre_Kind)
        {
            //更新变量
            m_dwUpdateTime = 0;
            m_bUpdateItem = false;

            //扩展数据
            m_dwProcessVersion = 0;

        }


        //重载函数
        //排列索引
        public override ushort GetSortID()
        {
            return m_GameKind.wSortID;
        }
    };


//////////////////////////////////////////////////////////////////////////////////

    //房间结构
    public class CGameServerItem : CGameListItem
    {
        //属性数据
        public tagGameServer m_GameServer; //房间信息
        public tagGameMatch m_GameMatch; //比赛信息

        //用户数据
        public bool m_bSignuped; //报名标识

        //扩展数据
        public enServerStatus m_ServerStatus; //房间状态

        //辅助变量
        public CGameKindItem m_pGameKindItem; //游戏类型

        //函数定义
        public CGameServerItem() : base(enItemGenre.ItemGenre_Server)
        {
            m_bSignuped = false;
            //辅助变量
            m_pGameKindItem = null;

            //扩展数据
            m_ServerStatus = enServerStatus.ServerStatus_Normal;
        }

        //构造函数


        //重载函数

        //排列索引
        public override ushort GetSortID()
        {
            return m_GameServer.wSortID;
        }

        //比赛房间
        public bool IsMatchRoom()
        {
            return (m_GameServer.wServerType & SocketDefines.GAME_GENRE_MATCH) != null;
        }

        //私人房间
        public bool IsPrivateRoom()
        {
            return (m_GameServer.wServerType & SocketDefines.GAME_GENRE_EDUCATE) != null;
        }
    };

//////////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////////////

    //内部结构
    public class CGameInsideItem : CGameListItem
    {
        //属性数据
        public uint m_dwInsideID; //内部 ID

        //函数定义
        //构造函数
        public CGameInsideItem() : base(enItemGenre.ItemGenre_Inside)
        {

        }

        //重载函数
        //排列索引
        public override ushort GetSortID()
        {
            return 0;
        }
    };

//////////////////////////////////////////////////////////////////////////////////

//列表服务
    class CServerListData
    {
        //列表数据
        private static CServerListData __gServerListData = null;

        //静态函数
        //获取对象
        public static CServerListData shared()
        {
            if (__gServerListData == null)
                __gServerListData = new CServerListData();
            return __gServerListData;
        }

        public static void purge()
        {
            if (__gServerListData != null)
                __gServerListData = null;
        }

        //索引变量
        protected CGameTypeItemMap m_GameTypeItemMap; //种类索引
        protected CGameKindItemMap m_GameKindItemMap; //类型索引
        protected CGameServerItemMap m_GameServerItemMap; //房间索引

        //mChen add, for HideSeek
        protected CGameLobbyItemMap m_GameLobbyItemMap; //大厅索引

        //内核变量
        protected CGameListItemArray m_GameListItemWait; //等待子项
        protected IServerListDataSink m_pIServerListDataSink; //回调接口

        public uint m_dwAllOnLineCount; //总在线人数

        //函数定义
        private CServerListData()
        {
            m_GameTypeItemMap = new CGameTypeItemMap();
            m_GameKindItemMap = new CGameKindItemMap();
            m_GameServerItemMap = new CGameServerItemMap();
            m_GameListItemWait = new CGameListItemArray();

            //mChen add. for HideSeek
            m_GameLobbyItemMap = new CGameLobbyItemMap();

            //接口变量
            m_pIServerListDataSink = null;

            m_dwAllOnLineCount = 0;
        } //构造函数


        public static CGameServerItem getGameServerByKind(ushort wKindID)
        {
            CGameServerItem pMinPlayerCoutServer = null;
            CServerListData pListData = GameNet.CServerListData.shared();
            foreach (var cGameServerItem in pListData.m_GameServerItemMap)
            {
                if (cGameServerItem.Value.m_GameServer.wKindID != wKindID)
                {
                    continue;
                }
                if (pMinPlayerCoutServer == null)
                {
                    pMinPlayerCoutServer = cGameServerItem.Value;
                    continue;
                }
                if (pMinPlayerCoutServer.m_GameServer.dwOnLineCount >
                    cGameServerItem.Value.m_GameServer.dwOnLineCount)
                {
                    pMinPlayerCoutServer = cGameServerItem.Value;
                }
            }
            return pMinPlayerCoutServer;
        }

        public static CGameServerItem getGameServerByKindAndServerType(ushort wKindID, ushort wServerType) //mChen add
        {
            CGameServerItem pMinPlayerCoutServer = null;
            CServerListData pListData = GameNet.CServerListData.shared();
            foreach (var cGameServerItem in pListData.m_GameServerItemMap)
            {

                CGameServerItem pActListItem = cGameServerItem.Value;

                if (pActListItem.m_GameServer.wKindID != wKindID || pActListItem.m_GameServer.wServerType != wServerType)
                {
                    continue;
                }
                if (pMinPlayerCoutServer == null)
                {
                    pMinPlayerCoutServer = pActListItem;
                    continue;
                }
                if (pMinPlayerCoutServer.m_GameServer.dwOnLineCount >
                    pActListItem.m_GameServer.dwOnLineCount)
                {
                    pMinPlayerCoutServer = pActListItem;
                }
            }
            return pMinPlayerCoutServer;
        }

        //配置函数
        //设置接口
        public void SetServerListDataSink(IServerListDataSink pIServerListDataSink)
        {
            //设置变量
            m_pIServerListDataSink = pIServerListDataSink;
        }

        //状态通知

        //完成通知
        public void OnEventListFinish()
        {
            if (m_pIServerListDataSink != null) m_pIServerListDataSink.OnGameItemFinish();
        }

        //完成通知
        public void OnEventKindFinish(ushort wKindID)
        {
            if (m_pIServerListDataSink != null) m_pIServerListDataSink.OnGameKindFinish(wKindID);
        }

        //下载通知
        public void OnEventDownLoadFinish(ushort wKindID)
        {
            if (m_GameKindItemMap.ContainsKey(wKindID))
            {
                var pGameKindItem = m_GameKindItemMap[wKindID];
                if (pGameKindItem != null)
                {
                    if (m_pIServerListDataSink != null) m_pIServerListDataSink.OnGameItemUpdate(pGameKindItem);
                }
            }

        }

        //人数函数

        //设置人数
        public void SetKindOnLineCount(ushort wKindID, uint dwOnLineCount)
        {
            //搜索类型
            CGameKindItem pGameKindItem = SearchGameKind(wKindID);

            //设置人数
            if (pGameKindItem != null)
            {
                //设置变量
                pGameKindItem.m_GameKind.dwOnLineCount = dwOnLineCount;

                //事件通知
                //ASSERT(m_pIServerListDataSink!=0);
                if (m_pIServerListDataSink != null) m_pIServerListDataSink.OnGameItemUpdate(pGameKindItem);
            }
        }

        //设置人数
        public void SetServerOnLineCount(ushort wServerID, uint dwOnLineCount)
        {
            //搜索房间
            CGameServerItem pGameServerItem = SearchGameServer(wServerID);

            //设置人数
            if (pGameServerItem != null)
            {
                //设置变量
                m_dwAllOnLineCount -= pGameServerItem.m_GameServer.dwOnLineCount;
                m_dwAllOnLineCount += dwOnLineCount;

                //设置变量
                pGameServerItem.m_GameServer.dwOnLineCount = dwOnLineCount;

                //事件通知
                //ASSERT(m_pIServerListDataSink!=0);
                if (m_pIServerListDataSink != null) m_pIServerListDataSink.OnGameItemUpdate(pGameServerItem);

                //查找类型
                CGameKindItem pGameKindItem = SearchGameKind(pGameServerItem.m_GameServer.wKindID);
                if (pGameKindItem != null)
                {
                    //变量定义
                    uint dwGameKindOnline = 0;
                    foreach (var cGameServerItem in m_GameServerItemMap)
                    {
                        CGameServerItem pGameServerItem2 = cGameServerItem.Value;
                        //设置房间
                        if ((pGameServerItem2 != null) &&
                            (pGameServerItem2.m_GameServer.wKindID == pGameServerItem.m_GameServer.wKindID))
                        {
                            dwGameKindOnline += pGameServerItem2.m_GameServer.dwOnLineCount;
                        }
                    }
                    //设置变量
                    pGameKindItem.m_GameKind.dwOnLineCount = dwGameKindOnline;

                    //事件通知
                    //ASSERT(m_pIServerListDataSink!=0);
                    if (m_pIServerListDataSink != null) m_pIServerListDataSink.OnGameItemUpdate(pGameKindItem);
                }
            }
        }

        //设置人数
        public void SetServerOnLineFinish()
        {
            if (m_pIServerListDataSink != null) m_pIServerListDataSink.OnGameItemUpdateFinish();
        }

        //插入函数
        //插入种类
        public bool InsertGameType(tagGameType pGameType)
        {
            //if (pGameType == null) return false;


            //变量定义
            CGameTypeItem pGameTypeItem = null;
            bool bInsert = false;

            if (m_GameTypeItemMap.ContainsKey(pGameType.wTypeID))
            {
                pGameTypeItem = m_GameTypeItemMap[pGameType.wTypeID];
            }
            else
            {
                pGameTypeItem = new CGameTypeItem();
                bInsert = true;
            }

            if (pGameTypeItem == null) return false;

            //设置数据
            var buf = StructConverterByteArray.StructToBytes(pGameType);
            pGameTypeItem.m_GameType = (tagGameType) StructConverterByteArray.BytesToStruct(buf, typeof (tagGameType));
            //memcpy(&pGameTypeItem.m_GameType, pGameType, sizeof(tagGameType));


            //插入处理
            if (bInsert == true)
            {
                //设置索引
                m_GameTypeItemMap[pGameType.wTypeID] = pGameTypeItem;

                //界面更新
                if (m_pIServerListDataSink != null)
                    m_pIServerListDataSink.OnGameItemInsert(pGameTypeItem);
            }
            else
            {
                //界面更新
                if (m_pIServerListDataSink != null)
                    m_pIServerListDataSink.OnGameItemUpdate(pGameTypeItem);
            }

            return true;
        }

        //插入类型
        public bool InsertGameKind(tagGameKind pGameKind)
        {
            //效验参数
            //ASSERT(pGameKind!=0);
            //if (pGameKind == null) return false;

            //变量定义
            CGameKindItem pGameKindItem = null;
            bool bInsert = false;
            if (m_GameKindItemMap.ContainsKey(pGameKind.wKindID))
            {
                pGameKindItem = m_GameKindItemMap[pGameKind.wKindID];
            }
            else
            {
                pGameKindItem = new CGameKindItem();
                bInsert = true;
            }

            if (pGameKindItem == null) return false;

            //设置数据
            var buf = StructConverterByteArray.StructToBytes(pGameKind);
            pGameKindItem.m_GameKind = (tagGameKind) StructConverterByteArray.BytesToStruct(buf, typeof (tagGameKind));
            //memcpy(&pGameKindItem.m_GameKind, pGameKind, sizeof(tagGameKind));

            //插入处理
            if (bInsert == true)
            {
                //设置索引
                m_GameKindItemMap[pGameKind.wKindID] = pGameKindItem;

                //插入子项
                if (m_pIServerListDataSink != null)
                    m_pIServerListDataSink.OnGameItemInsert(pGameKindItem);
            }
            else
            {
                //更新子项
                if (m_pIServerListDataSink != null)
                    m_pIServerListDataSink.OnGameItemUpdate(pGameKindItem);
            }

            return true;
        }

        //mChen add. for HideSeek
        //插入大厅
        public bool InsertGameLobby(tagGameLobby sGameLobby)
        {
            ////变量定义
            //var buf = StructConverterByteArray.StructToBytes(sGameLobby);
            //tagGameLobby sGameLobbyItem = (tagGameLobby)StructConverterByteArray.BytesToStruct(buf, typeof(tagGameLobby));

            //设置索引
            m_GameLobbyItemMap[sGameLobby.wLobbyID] = sGameLobby;

            return true;
        }
        public tagGameLobby getARandGameLobby()
        {
            tagGameLobby sMinPlayerCoutLobby = new tagGameLobby();
            //CServerListData pListData = GameNet.CServerListData.shared();
            foreach (var cGameLobbyItem in m_GameLobbyItemMap)
            {
                sMinPlayerCoutLobby = cGameLobbyItem.Value;

                int randV = UnityEngine.Random.Range(1, 100);
                //float rand01 = (float)MersenneTwister.MT19937.Real1();
                if(randV > 50)
                {
                    break;
                }
            }

            return sMinPlayerCoutLobby;
        }
        //大厅数目
        public uint GetGameLobbyCount()
        {
            return (uint)m_GameLobbyItemMap.Count;
        }

        //插入房间
        public bool InsertGameServer(tagGameServer pGameServer)
        {
            //效验参数
            //ASSERT(pGameServer!=0);
            //if (pGameServer == null) return false;

            //变量定义
            CGameServerItem pGameServerItem = null;
            bool bInsert = false;
            if (m_GameServerItemMap.ContainsKey(pGameServer.wServerID))
            {
                pGameServerItem = m_GameServerItemMap[pGameServer.wServerID];
            }
            else
            {
                pGameServerItem = new CGameServerItem();
                bInsert = true;
            }

            if (pGameServerItem == null) return false;

            //设置数据
            var buf = StructConverterByteArray.StructToBytes(pGameServer);
            pGameServerItem.m_GameServer =
                (tagGameServer) StructConverterByteArray.BytesToStruct(buf, typeof (tagGameServer));
            //memcpy(&pGameServerItem.m_GameServer, pGameServer, sizeof(tagGameServer));
            m_dwAllOnLineCount += pGameServer.dwOnLineCount;

            pGameServerItem.m_pGameKindItem = SearchGameKind(pGameServer.wKindID);

            //查找类型
            if (bInsert == true && pGameServerItem.m_pGameKindItem != null)
            {
                //变量定义
                uint dwGameKindOnline = 0;
                foreach (var cGameServerItem in m_GameServerItemMap)
                {
                    //获取房间
                    CGameServerItem pGameServerItem2 = cGameServerItem.Value;

                    //设置房间
                    if ((pGameServerItem2 != null) &&
                        (pGameServerItem2.m_GameServer.wKindID == pGameServerItem.m_GameServer.wKindID))
                    {
                        dwGameKindOnline += pGameServerItem2.m_GameServer.dwOnLineCount;
                    }
                }

                //设置变量
                pGameServerItem.m_pGameKindItem.m_GameKind.dwOnLineCount = dwGameKindOnline;

                //事件通知
                //ASSERT(m_pIServerListDataSink!=0);
                if (m_pIServerListDataSink != null)
                    m_pIServerListDataSink.OnGameItemUpdate(pGameServerItem.m_pGameKindItem);
            }


            //插入处理
            if (bInsert == true)
            {
                //设置索引
                m_GameServerItemMap[pGameServer.wServerID] = pGameServerItem;

                //插入子项
                if (m_pIServerListDataSink != null)
                    m_pIServerListDataSink.OnGameItemInsert(pGameServerItem);
            }
            else
            {
                //更新子项
                if (m_pIServerListDataSink != null)
                    m_pIServerListDataSink.OnGameItemUpdate(pGameServerItem);
            }

            return true;
        }

        //删除函数

        //删除种类
        public bool DeleteGameType(ushort wTypeID)
        {
            //查找种类
            if (m_GameTypeItemMap.ContainsKey(wTypeID) == false)
            {
                return false;
            }
            CGameTypeItem pGameTypeItem = m_GameTypeItemMap[wTypeID];

            //删除通知
            //ASSERT(m_pIServerListDataSink!=0);
            if (m_pIServerListDataSink != null) m_pIServerListDataSink.OnGameItemDelete(pGameTypeItem);

            //重置数据
            pGameTypeItem = null;

            //删除数据
            m_GameTypeItemMap.Remove(wTypeID);

            return true;
        }

        //删除类型
        public bool DeleteGameKind(ushort wKindID)
        {
            //查找类型
            if (m_GameKindItemMap.ContainsKey(wKindID) == false)
            {
                return false;
            }
            CGameKindItem pGameKindItem = m_GameKindItemMap[wKindID];

            //删除通知
            //ASSERT(m_pIServerListDataSink!=0);
            if (m_pIServerListDataSink != null) m_pIServerListDataSink.OnGameItemDelete(pGameKindItem);

            //删除数据
            pGameKindItem = null;

            //删除数据
            m_GameKindItemMap.Remove(wKindID);

            return true;
        }

        //删除房间
        public bool DeleteGameServer(ushort wServerID)
        {
            //查找房间
            if (m_GameServerItemMap.ContainsKey(wServerID) == false)
            {
                return false;
            }

            CGameServerItem pGameServerItem = m_GameServerItemMap[wServerID];

            //设置数据
            m_dwAllOnLineCount -= pGameServerItem.m_GameServer.dwOnLineCount;

            //删除通知
            //ASSERT(m_pIServerListDataSink!=0);
            if (m_pIServerListDataSink != null) m_pIServerListDataSink.OnGameItemDelete(pGameServerItem);

            //删除数据
            pGameServerItem = null;

            //删除数据
            m_GameServerItemMap.Remove(wServerID);

            return false;
        }

        //枚举函数
        //枚举种类
        public int getTypeCount()
        {
            return m_GameTypeItemMap.Count;
        }

        public int getKindCount()
        {
            return m_GameKindItemMap.Count;
        }


        //查找函数
        //查找种类
        public CGameTypeItem SearchGameType(ushort wTypeID)
        {
            if (m_GameTypeItemMap.ContainsKey(wTypeID) == false)
            {
                return null;
            }
            return m_GameTypeItemMap[wTypeID];
        }

        //查找类型
        public CGameKindItem SearchGameKind(ushort wKindID)
        {
            if (m_GameKindItemMap.ContainsKey(wKindID) == false)
            {
                return null;
            }
            return m_GameKindItemMap[wKindID];
        }

        //查找房间
        public CGameServerItem SearchGameServer(ushort wServerID)
        {
            if (m_GameServerItemMap.ContainsKey(wServerID) == false)
            {
                return null;
            }
            return m_GameServerItemMap[wServerID];

        }

        //数目函数
        //种类数目
        public uint GetGameTypeCount()
        {
            return (uint) m_GameTypeItemMap.Count;
        }

        //类型数目
        public uint GetGameKindCount()
        {
            return (uint) m_GameKindItemMap.Count;
        }

        //房间数目
        public uint GetGameServerCount()
        {
            return (uint) m_GameServerItemMap.Count;
        }


        //获取总在线人数
        public uint GetAllOnLineCount() //{return m_dwAllOnLineCount;}
        {
            //定义变量
            uint dwAllOnLineCount = 0;

            foreach (var cGameKindItem in m_GameKindItemMap)
            {
                CGameKindItem pGameKindItem = cGameKindItem.Value;
                if (pGameKindItem != null)
                {
                    dwAllOnLineCount += pGameKindItem.m_GameKind.dwOnLineCount;
                }
            }

            return dwAllOnLineCount;
        }
    };

}
