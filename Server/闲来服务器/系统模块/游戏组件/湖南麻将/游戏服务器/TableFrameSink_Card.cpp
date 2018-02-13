#include "StdAfx.h"
#include "TableFrameSink.h"
#include "FvMask.h"

bool CTableFrameSink::hasRule(BYTE cbRule)
{
	return FvMask::HasAny(m_dwGameRuleIdex, _MASK_(cbRule));
}
BYTE CTableFrameSink::AnalyseChiHuCardZZ(const BYTE cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], BYTE cbWeaveCount, BYTE cbCurrentCard, CChiHuRight &ChiHuRight, bool bSelfSendCard)
{
	//变量定义
	BYTE cbChiHuKind = WIK_NULL;
	CAnalyseItemArray AnalyseItemArray;

	//设置变量
	AnalyseItemArray.RemoveAll();
	ChiHuRight.SetEmpty();

	//构造扑克
	BYTE cbCardIndexTemp[MAX_INDEX];
	CopyMemory(cbCardIndexTemp, cbCardIndex, sizeof(cbCardIndexTemp));

	//cbCurrentCard一定不为0			!!!!!!!!!
	ASSERT(cbCurrentCard != 0);
	if (cbCurrentCard == 0) return WIK_NULL;

	//插入扑克
	if (cbCurrentCard != 0)
		cbCardIndexTemp[m_GameLogic.SwitchToCardIndex(cbCurrentCard)]++;

	//分析扑克，是否满足基本胡牌型
	bool bValue = m_GameLogic.AnalyseCard(cbCardIndexTemp, WeaveItem, cbWeaveCount, AnalyseItemArray);
	if (!bValue)
	{
		cbChiHuKind = WIK_NULL;
	}
	else
	{
		cbChiHuKind = WIK_CHI_HU;
	}

	//八对
	bool bBaoTou = false;
	bool bCanPiao = false;
	if (m_GameLogic.IsBaDui(cbCardIndex, WeaveItem, cbWeaveCount, cbCurrentCard, bBaoTou, bCanPiao))
	{
		if (bBaoTou == true)
			ChiHuRight |= CHR_BAO_TOU;

		ChiHuRight |= CHR_BA_DUI;
	}

	//清风子
	if (m_GameLogic.IsQingFengZi(cbCardIndex, WeaveItem, cbWeaveCount, cbCurrentCard))
		ChiHuRight |= CHR_QING_FENG_ZI;

	//十三不搭
	if (m_GameLogic.IsShiSanBuDa(cbCardIndex, WeaveItem, cbWeaveCount, cbCurrentCard))
		ChiHuRight |= CHR_SHI_SAN_BU_DA;

	if (!ChiHuRight.IsEmpty())
		cbChiHuKind = WIK_CHI_HU;

	//能与其他胡牌型叠加
	if (cbChiHuKind == WIK_CHI_HU)
	{
		//清一色
		if (m_GameLogic.IsQingYiSe(cbCardIndex, WeaveItem, cbWeaveCount, cbCurrentCard))
			ChiHuRight |= CHR_QING_YI_SE;

		//暴头
		for (INT_PTR i = 0; i < AnalyseItemArray.GetCount(); i++)
		{
			//变量定义
			tagAnalyseItem *pAnalyseItem = &AnalyseItemArray[i];

			if (m_GameLogic.IsBaoTou(pAnalyseItem, cbCurrentCard))
				ChiHuRight |= CHR_BAO_TOU;
		}
	}

	//自摸
	if (bSelfSendCard && cbChiHuKind == WIK_CHI_HU)
	{
		ChiHuRight |= CHR_ZI_MO;

		//杠开
		if (m_bGangStatus)
		{
			ChiHuRight |= CHR_GANG_KAI;
		}
	}
	else if (!bSelfSendCard && cbChiHuKind == WIK_CHI_HU && m_bGangStatus && m_bBuGang && m_wCurrentUser == INVALID_CHAIR)
	{
		ChiHuRight |= CHR_QIANG_GANG;
	}
	else
	{
		ChiHuRight.SetEmpty();
		cbChiHuKind = WIK_NULL;
	}

	return cbChiHuKind;
}

BYTE CTableFrameSink::AnalyseChiHuCard(const BYTE cbCardIndex[MAX_INDEX], const tagWeaveItem WeaveItem[], BYTE cbWeaveCount, BYTE cbCurrentCard, CChiHuRight &ChiHuRight)
{
	if (GAME_TYPE_ZZ == m_cbGameTypeIdex)
	{
		return AnalyseChiHuCardZZ(cbCardIndex, WeaveItem, cbWeaveCount, cbCurrentCard, ChiHuRight, m_wProvideUser == m_wCurrentUser);
	}

	ASSERT(false);
	return 0;
}


void CTableFrameSink::FiltrateRight(WORD wChairId, CChiHuRight &chr)
{
	//权位增加
	if (m_cbPiaoCount[wChairId] == 1)
	{
		chr |= CHR_PIAO_CAI_YI;
		if ((chr&CHR_BAO_TOU).IsEmpty())
			chr |= CHR_BAO_TOU;
	}
	else if (m_cbPiaoCount[wChairId] == 2)
	{
		chr |= CHR_PIAO_CAI_ER;
		if ((chr&CHR_BAO_TOU).IsEmpty())
			chr |= CHR_BAO_TOU;
	}
	else if (m_cbPiaoCount[wChairId] == 3)
	{
		chr |= CHR_PIAO_CAI_SAN;
		if ((chr&CHR_BAO_TOU).IsEmpty())
			chr |= CHR_BAO_TOU;
	}

	if (!(chr&CHR_GANG_KAI).IsEmpty())
	{
		if (!(chr&CHR_BAO_TOU).IsEmpty())
			chr |= CHR_GANG_BAO;
	}

	//杠飘
	if (m_bGangPiao[wChairId] == true && m_cbPiaoCount[wChairId] != 0)
		chr |= CHR_GANG_PIAO;
	//飘杠
	else if (m_cbPiaoCount[wChairId] != 0 && m_bPiaoGang[wChairId] == true)
		chr |= CHR_PIAO_GANG;
	
	

	
	if (!(chr&CHR_BA_DUI).IsEmpty() && (chr&CHR_QING_YI_SE).IsEmpty())
	{
		if (m_cbPiaoCount[wChairId] != 0)
		{
			chr |= CHR_BA_DUI_ZI_PIAO_CAI;
		}
	}
	

	if (!(chr&CHR_QIANG_GANG).IsEmpty() && !(chr&CHR_SHI_SAN_BU_DA).IsEmpty())
	{
		chr |= CHR_SHI_SAN_BU_DA_QIANG_GANG;
	}
	if (!(chr&CHR_QING_YI_SE).IsEmpty() && !(chr&CHR_QIANG_GANG).IsEmpty())
	{
		chr |= CHR_QING_YI_SE_QIANGGANG;
	}
}
