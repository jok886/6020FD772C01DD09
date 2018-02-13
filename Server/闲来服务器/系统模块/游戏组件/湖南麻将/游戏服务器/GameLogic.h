#ifndef GAME_LOGIC_HEAD_FILE
#define GAME_LOGIC_HEAD_FILE

#pragma once

#include "Stdafx.h"
#include "../../../全局定义/Array.h"

#pragma pack(1)
//////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////

//逻辑掩码

#define	MASK_COLOR					0xF0								//花色掩码
#define	MASK_VALUE					0x0F								//数值掩码

//////////////////////////////////////////////////////////////////////////
//动作定义

//动作标志
#define WIK_NULL					0x00								//没有类型
#define WIK_LEFT					0x01								//左吃类型
#define WIK_CENTER					0x02								//中吃类型
#define WIK_RIGHT					0x04								//右吃类型
#define WIK_PENG					0x08								//碰牌类型
#define WIK_GANG					0x10								//杠牌类型
#define WIK_XIAO_HU					0x20								//小胡
#define WIK_CHI_HU					0x40								//吃胡类型
#define WIK_ZI_MO					0x80								//自摸

//////////////////////////////////////////////////////////////////////////
//胡牌定义

//胡牌
#define CHK_NULL						0x00										//非胡类型
#define CHK_CHI_HU						0x01										//胡类型

//胡牌型
#define CHR_BA_DUI						0x00000001									//八对X2
#define CHR_BAO_TOU						0x00000002									//暴头X2
#define CHR_PIAO_CAI_YI					0x00000004									//飘财X4
#define CHR_PIAO_CAI_ER					0x00000008									//二次飘财X8

#define CHR_PIAO_CAI_SAN				0x00000010									//三次飘财X16
#define CHR_GANG_KAI					0x00000020									//杠开X2
#define CHR_GANG_BAO					0x00000040									//杠暴
#define CHR_PIAO_GANG					0x00000080									//飘杠

#define CHR_GANG_PIAO					0x00000100									//杠飘
#define CHR_SHI_SAN_BU_DA				0x00000200									//十三不搭X4
#define CHR_QING_YI_SE					0x00000400									//清一色X10
#define CHR_QING_FENG_ZI				0x00000800                                  //清风子X20

#define CHR_QIANG_GANG					0x00001000									//抢杠
#define CHR_SHI_SAN_BU_DA_QIANG_GANG	0x00002000									//十三不搭抢杠
#define CHR_QIANG_PIAO_GANG				0x00004000									//抢飘杠
#define CHR_BA_DUI_ZI_PIAO_CAI			0x00008000									//八对子飘财

#define CHR_QING_YI_SE_PIAO_CAI			0x00010000									//清一色飘财
#define CHR_QING_BA_DUI_PIAO_CAI		0x00020000									//清八对飘财
#define CHR_QING_YI_SE_QIANGGANG		0x00040000									//清一色抢杠

#define CHR_ZI_MO		0x01000000									//自摸
#define CHR_SHU_FAN		0x02000000									//素翻


//////////////////////////////////////////////////////////////////////////

#define ZI_PAI_COUNT	7

//类型子项
struct tagKindItem
{
	BYTE							cbWeaveKind;						//组合类型
	BYTE							cbCenterCard;						//中心扑克
	BYTE							cbCardIndex[3];						//扑克索引
	BYTE							cbValidIndex[3];					//实际扑克索引
};

//组合子项
struct tagWeaveItem
{
	BYTE							cbWeaveKind;						//组合类型
	BYTE							cbCenterCard;						//中心扑克
	BYTE							cbPublicCard;						//公开标志
	WORD							wProvideUser;						//供应用户
};

//杠牌结果
struct tagGangCardResult
{
	BYTE							cbCardCount;						//扑克数目
	BYTE							cbCardData[MAX_WEAVE];				//扑克数据
};

//分析子项
struct tagAnalyseItem
{
	BYTE							cbCardEye;							//牌眼扑克
	bool                            bMagicEye;                          //牌眼是否是王霸
	BYTE							cbWeaveKind[MAX_WEAVE];				//组合类型
	BYTE							cbCenterCard[MAX_WEAVE];			//中心扑克
	BYTE                            cbCardData[MAX_WEAVE][MAX_WEAVE];   //实际扑克
};

//////////////////////////////////////////////////////////////////////////

#define MASK_CHI_HU_RIGHT			0x0fffffff

/*
//	权位类。
//  注意，在操作仅位时最好只操作单个权位.例如
//  CChiHuRight chr;
//  chr |= (chr_zi_mo|chr_peng_peng)，这样结果是无定义的。
//  只能单个操作:
//  chr |= chr_zi_mo;
//  chr |= chr_peng_peng;
*/
class CChiHuRight
{
	//静态变量
private:
	static bool						m_bInit;
	static DWORD					m_dwRightMask[MAX_RIGHT_COUNT];

	//权位变量
private:
	DWORD							m_dwRight[MAX_RIGHT_COUNT];

public:
	//构造函数
	CChiHuRight();

	//运算符重载
public:
	//赋值符
	CChiHuRight & operator = (DWORD dwRight);

	//与等于
	CChiHuRight & operator &= (DWORD dwRight);
	//或等于
	CChiHuRight & operator |= (DWORD dwRight);

	//与
	CChiHuRight operator & (DWORD dwRight);
	CChiHuRight operator & (DWORD dwRight) const;

	//或
	CChiHuRight operator | (DWORD dwRight);
	CChiHuRight operator | (DWORD dwRight) const;

	//功能函数
public:
	//是否权位为空
	bool IsEmpty();

	//设置权位为空
	void SetEmpty();

	//获取权位数值
	BYTE GetRightData(DWORD dwRight[], BYTE cbMaxCount);

	//设置权位数值
	bool SetRightData(const DWORD dwRight[], BYTE cbRightCount);

private:
	//检查权位是否正确
	bool IsValidRight(DWORD dwRight);
};


//////////////////////////////////////////////////////////////////////////

//数组说明
typedef CWHArray<tagAnalyseItem, tagAnalyseItem &> CAnalyseItemArray;

//游戏逻辑类
class CGameLogic
{
	//变量定义
protected:
	static const BYTE				m_cbCardDataArray[MAX_REPERTORY];		//扑克数据
	static const BYTE				m_cbCardDataArray_HZ[MAX_REPERTORY_HZ];	//扑克数据
	BYTE							m_cbMagicIndex;							//钻牌索引
	//函数定义
public:
	//构造函数
	CGameLogic();
	//析构函数
	virtual ~CGameLogic();

	//控制函数
public:
	//混乱扑克
	void RandCardData(BYTE cbCardData[], BYTE cbMaxCount);
	//删除扑克
	bool RemoveCard(BYTE cbCardIndex[MAX_INDEX], BYTE cbRemoveCard);
	//删除扑克
	bool RemoveCard(BYTE cbCardIndex[MAX_INDEX], const BYTE cbRemoveCard[], BYTE cbRemoveCount);
	//删除扑克
	bool RemoveCard(BYTE cbCardData[], BYTE cbCardCount, const BYTE cbRemoveCard[], BYTE cbRemoveCount);
	//设置钻牌
	void SetMagicIndex(BYTE cbMagicIndex) { m_cbMagicIndex = cbMagicIndex; }
	//钻牌
	bool IsMagicCard(BYTE cbCardData);

	//获取万能牌数目
	BYTE GetMagicCount(const BYTE cbCardIndex[]);

	//辅助函数
public:
	//有效判断
	bool IsValidCard(BYTE cbCardData);
	//扑克数目
	BYTE GetCardCount(const BYTE cbCardIndex[]);
	//组合扑克
	BYTE GetWeaveCard(BYTE cbWeaveKind, BYTE cbCenterCard, BYTE cbCardBuffer[4]);

	//等级函数
public:
	//动作等级
	BYTE GetUserActionRank(BYTE cbUserAction);
	//胡牌等级

	WORD GetChiHuActionRank_ZZ(const CChiHuRight & ChiHuRight);


	//动作判断
public:
	//吃牌判断
	BYTE EstimateEatCard(const BYTE cbCardIndex[MAX_INDEX], BYTE cbCurrentCard);
	//碰牌判断
	BYTE EstimatePengCard(const BYTE cbCardIndex[MAX_INDEX], BYTE cbCurrentCard);
	//杠牌判断,手上杠牌，放杠、暗杠
	BYTE EstimateGangCard(const BYTE cbCardIndex[MAX_INDEX], BYTE cbCurrentCard);
	//杠牌判断，补杠
	BYTE EstimateGangCard(const tagWeaveItem WeaveItem[], BYTE cbWeaveCount, BYTE cbCurrentCard);

	//动作判断
public:
	//杠牌分析
	BYTE AnalyseGangCard(const BYTE cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], BYTE cbWeaveCount, tagGangCardResult & GangCardResult,bool bOnlyAnalyseShouPai);
	

	//转换函数
public:
	//扑克转换
	BYTE SwitchToCardData(BYTE cbCardIndex);
	//扑克转换
	BYTE SwitchToCardIndex(BYTE cbCardData);
	//扑克转换
	BYTE SwitchToCardData(const BYTE cbCardIndex[MAX_INDEX], BYTE cbCardData[MAX_COUNT]);
	//扑克转换
	BYTE SwitchToCardIndex(const BYTE cbCardData[], BYTE cbCardCount, BYTE cbCardIndex[MAX_INDEX]);

	//胡法分析
public:
	

	//分析扑克
	bool AnalyseCard(const BYTE cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], BYTE cbItemCount, CAnalyseItemArray & AnalyseItemArray);
	//排序,根据牌值排序
	bool SortCardList(BYTE cbCardData[MAX_COUNT], BYTE cbCardCount);


	//建德麻将
	//八对
	bool IsBaDui(const BYTE cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], const BYTE cbWeaveCount, const BYTE cbCurrentCard, bool &bBaoTou, bool &bCanPiao);
	//清一色牌
	bool IsQingYiSe(const BYTE cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], const BYTE cbWeaveCount, const BYTE cbCurrentCard);
	//清风子
	bool IsQingFengZi(const BYTE cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], const BYTE cbWeaveCount, const BYTE cbCurrentCard);
	//爆头
	bool IsBaoTou(const tagAnalyseItem *pAnalyseItem, const BYTE cbCurrentCard);
	bool IsBaoTou(const BYTE cbCardIndex[], const tagWeaveItem WeaveItem[], const BYTE cbWeaveCount, const BYTE cbCurrentCard);
	//飘财
	bool CanPiaoCai(const BYTE cbCardIndex[], const BYTE cbCurrentCard);
	//十三不搭
	bool IsShiSanBuDa(const BYTE cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], const BYTE cbWeaveCount, const BYTE cbCurrentCard);
};
//////////////////////////////////////////////////////////////////////////
#pragma pack()
#endif
