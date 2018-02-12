using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameNet
{

    //桌子视图
    interface ITableView
    {
        //功能接口
        //空椅子数
        ushort GetNullChairCount(ref ushort wNullChairID, uint wUserID);
        //配置函数
        void InitTableView(ushort wTableID, ushort wChairCount, ITableViewFrame pITableFrameView);

        //用户接口
        //获取用户
        IClientUserItem GetClientUserItem(ushort wChairID);
        //设置信息
        bool SetClientUserItem(ushort wChairID, IClientUserItem pIClientUserItem);

        //设置接口
        //焦点框架
        void SetFocusFrame(bool bFocusFrame);
        //桌子状态 
        void SetTableStatus(bool bPlaying, bool bLocker);

        //查询接口
        //游戏标志
        bool GetPlayFlag();
        //密码标志
        bool GetLockerFlag();
    };

    //桌子框架
    interface ITableViewFrame
    {

        //配置接口
        //配置桌子
        bool ConfigTableFrame(ushort wTableCount, ushort wChairCount, ushort wServerID);

        //信息接口
        //桌子数目
        ushort GetTableCount();
        //椅子数目
        ushort GetChairCount();

        //用户接口
        //获取用户
        IClientUserItem GetClientUserItem(ushort wTableID, ushort wChairID);
        //设置信息
        bool SetClientUserItem(ushort wTableID, ushort wChairID, IClientUserItem pIClientUserItem);

        //状态接口
        //游戏标志
        bool GetPlayFlag(ushort wTableID);
        //密码标志
        bool GetLockerFlag(ushort wTableID);
        //焦点框架
        void SetFocusFrame(ushort wTableID, bool bFocusFrame);
        //桌子状态 
        void SetTableStatus(ushort wTableID, bool bPlaying, bool bLocker);

        //视图控制
        //闪动桌子
        bool FlashGameTable(ushort wTableID);
        //闪动椅子
        bool FlashGameChair(ushort wTableID, ushort wChairID);

        //功能接口
        //更新桌子
        bool UpdateTableView(ushort wTableID);
        //获取桌子
        ITableView GetTableViewItem(ushort wTableID);
        //空椅子数
        ushort GetNullChairCount(ushort wTableID, ref ushort wNullChairID, uint wUserID);
    };

    //////////////////////////////////////////////////////////////////////////////////
    //桌子视图
    class CTableView : ITableView
    { 
	//桌子属性
	//桌子标志
	bool mIsLocker;                         //密码标志
    bool mIsPlaying;                            //游戏标志
    bool mIsFocusFrame;                     //框架标志

    //桌子状态
    ushort mWatchCount;                       //旁观数目
    uint mTableOwnerID;                        //桌主索引

    //属性变量
    ushort mTableID;                          //桌子号码
    ushort mChairCount;                       //椅子数目
    IClientUserItem[] mIClientUserItem = new IClientUserItem[SocketDefines.MAX_CHAIR];       //用户信息

    //组件接口
	ITableViewFrame mITableViewFrame;                  //桌子接口

    //函数定义
    //构造函数
        public CTableView()
        {
            //组件接口
            mITableViewFrame = null;

            //桌子标志
            mIsLocker = false;
            mIsPlaying = false;
            mIsFocusFrame = false;

            //桌子状态
            mWatchCount = 0;
            mTableOwnerID = 0;

            //属性变量
            mTableID = 0;
            mChairCount = 0;

        }

    //功能接口
	//空椅子数
        public ushort GetNullChairCount(ref ushort wNullChairID, uint wUserID)
        {
            //设置变量
            wNullChairID = SocketDefines.INVALID_CHAIR;

            //寻找位置
            ushort wNullCount = 0;
            for (ushort i = 0; i < mChairCount; i++)
            {
                if (mIClientUserItem[i] == null)
                {
                    //设置数目
                    wNullCount++;

                    //设置结果
                    if (wNullChairID == SocketDefines.INVALID_CHAIR) wNullChairID = i;
                }
            }
            for (ushort i = 0; i < mChairCount; i++)
            {
                if (mIClientUserItem[i]!=null && mIClientUserItem[i].GetUserID() == wUserID)
                {
                    //设置结果
                    if (wNullChairID == SocketDefines.INVALID_CHAIR) wNullChairID = i;
                }
            }

            return wNullCount;
        }
    //配置函数
        public void InitTableView(ushort wTableID, ushort wChairCount, ITableViewFrame pITableViewFrame)
        {
            //设置属性
            mTableID = wTableID;
            mChairCount = wChairCount;

            //设置接口
            mITableViewFrame = pITableViewFrame;
        }

    //用户接口
	//获取用户
        public IClientUserItem GetClientUserItem(ushort wChairID)
        {
            //效验参数
            Debug.Assert(wChairID < mChairCount);
            if (wChairID >= mChairCount) return null;

            //获取用户
            return mIClientUserItem[wChairID];
        }
    //设置信息
        public bool SetClientUserItem(ushort wChairID, IClientUserItem pIClientUserItem)
        {
            //效验参数
            //ASSERT(wChairID < mChairCount);
            if (wChairID >= mChairCount) return false;

            //设置用户
            mIClientUserItem[wChairID] = pIClientUserItem;

            //更新界面
            mITableViewFrame.UpdateTableView(mTableID);

            return true;
        }

    //查询接口
	//游戏标志
	public bool GetPlayFlag() { return mIsPlaying; }
    //密码标志
    public bool GetLockerFlag() { return mIsLocker; }

    //状态接口
	//焦点框架
        public void SetFocusFrame(bool bFocusFrame)
        {
            //设置标志
            if (mIsFocusFrame != bFocusFrame)
            {
                //设置变量
                mIsFocusFrame = bFocusFrame;

                //更新界面
                mITableViewFrame.UpdateTableView(mTableID);
            }
        }
    //桌子状态 
        public void SetTableStatus(bool bPlaying, bool bLocker)
        {
            //设置标志
            if ((mIsLocker != bLocker) || (mIsPlaying != bPlaying))
            {
                //设置变量
                mIsLocker = bLocker;
                mIsPlaying = bPlaying;

                //更新界面
                mITableViewFrame.UpdateTableView(mTableID);
            }
        }
};

//////////////////////////////////////////////////////////////////////////////////

//桌子框架
    class CTableViewFrame : ITableViewFrame
    {

//属性变量
        ushort mTableCount; //游戏桌数
        ushort mChairCount; //椅子数目

//控件变量
        public List<CTableView> mTableViewArray = new List<CTableView>(); //桌子数组

//比赛变量
        public uint mMatchWaittingCount; //等待人数
        public uint mMatchTotalUser; //参赛人数
        public uint mMatchBestRank; //最好成绩
        public uint mMatchJoinCount; //参数次数
        public byte mMatchStatus; //比赛状态
        public tagMatchDesc mMatchDesc; //信息描述

//函数定义
        //构造函数
        public CTableViewFrame()
        {
            //属性变量
            mTableCount = 0;
            mChairCount = 0;

            //比赛变量
            mMatchTotalUser = 0;
            mMatchWaittingCount = 0;
            mMatchStatus = SocketDefines.MS_NULL;
        }

        //配置接口
        //配置桌子
        public bool ConfigTableFrame(ushort wTableCount, ushort wChairCount, ushort wServerID)
        {
            //设置变量
            mTableCount = wTableCount;
            mChairCount = wChairCount;
            mTableViewArray.Clear();
            mTableViewArray.Capacity = (mTableCount);

            //创建桌子
            for (ushort i = 0; i < mTableCount; i++)
            {
                mTableViewArray.Add(new CTableView());
                mTableViewArray[i].InitTableView(i, wChairCount, this);
            }

            return true;
        }

//信息接口
        //桌子数目
        public ushort GetTableCount()
        {
            return mTableCount;
        }

        //椅子数目
        public ushort GetChairCount()
        {
            return mChairCount;
        }

//用户接口
        //获取用户
        public IClientUserItem GetClientUserItem(ushort wTableID, ushort wChairID)
        {
            ITableView pITableView = GetTableViewItem(wTableID);

            //获取用户
            if (pITableView != null)
            {
                return pITableView.GetClientUserItem(wChairID);
            }

            return null;
        }

        //设置信息
        public bool SetClientUserItem(ushort wTableID, ushort wChairID, IClientUserItem pIClientUserItem)
        {
            ITableView pITableView = GetTableViewItem(wTableID);
            if (pITableView != null) pITableView.SetClientUserItem(wChairID, pIClientUserItem);
            return true;
        }

//状态接口
        //游戏标志
        public bool GetPlayFlag(ushort wTableID)
        {
            ITableView pITableView = GetTableViewItem(wTableID);

            //获取标志
            if (pITableView != null)
            {
                return pITableView.GetPlayFlag();
            }

            return false;
        }

        //密码标志
        public bool GetLockerFlag(ushort wTableID)
        {
            ITableView pITableView = GetTableViewItem(wTableID);

            //获取标志
            if (pITableView != null)
            {
                return pITableView.GetLockerFlag();
            }

            return false;
        }

        //焦点框架
        public void SetFocusFrame(ushort wTableID, bool bFocusFrame)
        {
            ITableView pITableView = GetTableViewItem(wTableID);

            //设置标志
            if (pITableView != null) pITableView.SetFocusFrame(bFocusFrame);
        }

        //桌子状态 
        public void SetTableStatus(ushort wTableID, bool bPlaying, bool bLocker)
        {
            ITableView pITableView = GetTableViewItem(wTableID);

            //设置标志
            if (pITableView != null) pITableView.SetTableStatus(bPlaying, bLocker);
        }

        //桌子状态 
        public void SetTableStatus(bool bWaitDistribute)
        {

        }

//视图控制
        //桌子可视
        public bool VisibleTable(ushort wTableID)
        {
            if (wTableID >= mTableCount) return false;

            return true;
        }

        //闪动桌子
        public bool FlashGameTable(ushort wTableID)
        {
            //获取桌子
            ITableView pITableView = GetTableViewItem(wTableID);

            //错误判断
            if (pITableView == null)
            {
                Debug.Assert(false, "FlashGameTable");
                return false;
            }


            return true;
        }

        //闪动椅子
        public bool FlashGameChair(ushort wTableID, ushort wChairID)
        {
            //获取桌子
            ITableView pITableView = GetTableViewItem(wTableID);

            //错误判断
            if (pITableView == null)
            {
                Debug.Assert(false, "FlashGameChair");
                return false;
            }

            return true;
        }

//功能接口
        //更新桌子
        public bool UpdateTableView(ushort wTableID)
        {
            //获取桌子
            ITableView pITableView = GetTableViewItem(wTableID);
            if (pITableView == null) return false;


            return true;
        }

        //获取桌子
        public ITableView GetTableViewItem(ushort wTableID)
        {
            //获取桌子
            if (wTableID != SocketDefines.INVALID_TABLE)
            {
                //效验参数
                Debug.Assert(wTableID < (int) mTableViewArray.Count);
                if (wTableID >= (int) mTableViewArray.Count) return null;

                //获取桌子
                ITableView pITableView = mTableViewArray[wTableID];

                return pITableView;
            }

            return null;
        }

        //空椅子数
        public ushort GetNullChairCount(ushort wTableID, ref ushort wNullChairID, uint wUserID)
        {
            //获取桌子

            ITableView pITableView = GetTableViewItem(wTableID);

            //获取状态
            if (pITableView != null)
            {
                return pITableView.GetNullChairCount(ref wNullChairID, wUserID);
            }

            return 0;
        }

        //比赛函数
        //比赛状态
        public void SetMatchStatus(byte cbMatchStatus)
        {
            mMatchStatus = cbMatchStatus;
        }
    };

}
