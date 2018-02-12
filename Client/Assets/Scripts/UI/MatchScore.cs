using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchScore : MonoBehaviour {
    private List<GameObject> m_infoItemObjList = new List<GameObject>();
    public void QueryAndShow()
    {
        GameNet.UserInfo.getInstance().queryTopPlayersInfo(this);
        gameObject.SetActive(true);
    }

    public void CreateTopPlayersListView(ref GameNet.TopPlayersInfoItem[] topPlayersInfo)
    {
        Debug.Log("MatchScore:CreateTopPlayersListView:wItemCount=" + topPlayersInfo.Length);

        var viewportContent = gameObject.transform.Find("RankingWindow/RankingNums/Scroll View/Viewport/Content");
        if (viewportContent == null)
        {
            return;
        }

        var InfosItem = viewportContent.Find("Infos");
        if (InfosItem == null)
        {
            return;
        }

        //填充数据
        float curAnchoredPosY = InfosItem.GetComponent<RectTransform>().anchoredPosition.y;
        float posStep = 65;
        int nItemNum = topPlayersInfo.Length;
        viewportContent.GetComponent<RectTransform>().sizeDelta = new Vector3(0, 0, 65 * nItemNum);
        for (int i = 0; i < m_infoItemObjList.Count; i++)
        {
            m_infoItemObjList[i].SetActive(false);
        }
        if (m_infoItemObjList.Count < nItemNum)
        {
            for (int j = m_infoItemObjList.Count; j < nItemNum; j++)
            {
                var infoItemObj = Instantiate(InfosItem.gameObject);
                infoItemObj.transform.SetParent(viewportContent);
                infoItemObj.transform.localPosition = Vector3.zero;
                infoItemObj.transform.localScale = InfosItem.localScale;
                infoItemObj.SetActive(false);

                m_infoItemObjList.Add(infoItemObj);
            }
        }
        for (int i = 0; i < /*nItemNum*/m_infoItemObjList.Count; i++)
        {
            GameObject infoItemObj = m_infoItemObjList[i];
            if (infoItemObj == null)
            {
                continue;
            }
            infoItemObj.SetActive(true);

            var playerInfoItem = topPlayersInfo[i];

            //序号
            var numObj = infoItemObj.transform.Find("NumInfo");
            if (numObj != null)
            {
                numObj.GetComponent<Text>().text = Convert.ToString(i + 1);
            }

            //姓名
            var nameObj = infoItemObj.transform.Find("NameInfo");
            if (nameObj != null)
            {
                string strName = GameNet.GlobalUserInfo.GBToUtf8(playerInfoItem.strNickName);
                nameObj.GetComponent<Text>().text = strName;
            }

            //经验
            var experienceObj = infoItemObj.transform.Find("GradeInfo");
            if (experienceObj != null)
                experienceObj.GetComponent<Text>().text = playerInfoItem.iExperience.ToString();

            ////分数
            //var scoreObj = infoItemObj.transform.Find("MatchScore");
            //if (scoreObj != null)
            //{
            //    scoreObj.GetComponent<Text>().text = playerInfoItem.lTotalScore.ToString();
            //}

            ////胜利场次
            //var winObj = infoItemObj.transform.Find("Wincount");
            //if (winObj != null)
            //{
            //    winObj.GetComponent<Text>().text = playerInfoItem.wWinCount.ToString();
            //}
            ////平局场次
            //var drawObj = infoItemObj.transform.Find("DrawCount");
            //if (drawObj != null)
            //{
            //    drawObj.GetComponent<Text>().text = playerInfoItem.wDrawCount.ToString();
            //}
            ////失败场次
            //var loseObj = infoItemObj.transform.Find("LoseCount");
            //if (loseObj != null)
            //{
            //    loseObj.GetComponent<Text>().text = playerInfoItem.wLoseCount.ToString();
            //}

            //Set Pos
            Vector3 anchoredPos = InfosItem.GetComponent<RectTransform>().anchoredPosition;
            anchoredPos.y = curAnchoredPosY;
            infoItemObj.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
            curAnchoredPosY -= posStep;

            //设置Content的Bottom
            Vector2 offsetMin = viewportContent.GetComponent<RectTransform>().offsetMin;
            offsetMin.y -= posStep;
            viewportContent.GetComponent<RectTransform>().offsetMin = offsetMin;
        }

        InfosItem.gameObject.SetActive(false);
    }
}
