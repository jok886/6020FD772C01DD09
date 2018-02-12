using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PlayBackUI : MonoBehaviour
{
    private HNGameManager hnGameManager;
    public GameObject AllRecordsWin;
    public GameObject DetailRecordWin;

    private List<GameObject> recordsList;
    private List<GameObject> detailList; 

    void Start()
    {
        hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
        AllRecordsWin.SetActive(true);
        DetailRecordWin.SetActive(false);
        recordsList = new List<GameObject>();
        detailList = new List<GameObject>();
        UpdateAllRecords();
    }

    public void ShowPlayBackInfos()
    {
        PlayBackStorage.GetInstance().ShowRecordUI();
        AllRecordsWin.SetActive(true);
        DetailRecordWin.SetActive(false);
    }

    public void StartReply(int iRecordIdx, int iMatchIdx)
    {
        hnGameManager.StartFakeReply(iRecordIdx, iMatchIdx);
    }

    public void UpdateAllRecords()
    {
        var viewportContent = AllRecordsWin.transform.Find("Scroll View/Viewport/Content");
        if (viewportContent == null)
        {
            return;
        }

        //设置Content的Bottom
        Vector2 offsetMin = viewportContent.GetComponent<RectTransform>().offsetMin;
        offsetMin.y = -400.0f;
        viewportContent.GetComponent<RectTransform>().offsetMin = offsetMin;

        var RecordTemplete = viewportContent.Find("PlayInfoBack");
        if (RecordTemplete == null)
        {
            return;
        }
        RecordTemplete.gameObject.SetActive(false);

        var allRecords = PlayBackStorage.GetInstance().GetAllRecords();

        if (recordsList.Count < allRecords.Length)
        {
            for (int j = recordsList.Count; j < allRecords.Length; j++)
            {
                var infoItemObj = Instantiate(RecordTemplete.gameObject);
                infoItemObj.transform.SetParent(viewportContent);
                infoItemObj.transform.localScale = RecordTemplete.localScale;
                recordsList.Add(infoItemObj);
            }
        }

        for (int i = 0; i < recordsList.Count; i++)
        {
            recordsList[i].SetActive(false);
        }

        //填充数据
        float curAnchoredPosY = RecordTemplete.GetComponent<RectTransform>().anchoredPosition.y;
        float posStep = 88;

        for (int i = 0; i < allRecords.Length; i++)
        {
            GameObject infoItemObj = recordsList[i];
            if (infoItemObj == null)
            {
                continue;
            }
            infoItemObj.SetActive(true);

            //序号
            var numObj = infoItemObj.transform.Find("Num");
            if (numObj != null)
            {
                numObj.GetComponent<Text>().text = (i + 1).ToString();
            }

            //房间号
            var roomObj = infoItemObj.transform.Find("RoomNums");
            if (roomObj != null)
            {
                roomObj.GetComponent<Text>().text = allRecords[i].roomID.ToString();
            }

            DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();

            dtFormat.ShortDatePattern = "yyyyMMddhhmmss";
            DateTime dt = Convert.ToDateTime(allRecords[i].matchStartTime[0], dtFormat);//第一局时间当做本场开始时间

            //日期
            var dayObj = infoItemObj.transform.Find("Day");
            if (dayObj != null)
            {
                dayObj.GetComponent<Text>().text = dt.ToString("yyyy-MM-dd");
            }

            //时间
            var timeObj = infoItemObj.transform.Find("Time");
            if (timeObj != null)
            {
                timeObj.GetComponent<Text>().text = dt.ToString("hh:mm:ss");
            }

            for (int j = 0; j < 4; j++)
            {
                var user = infoItemObj.transform.Find(string.Format("Users/User{0}",j+1));
//#if UNITY_ANDROID || UNITY_IOS//头像
//                if (allRecords[i].headSprites[j] != null)
//                {
//                    user.GetComponent<Image>().sprite = allRecords[i].headSprites[j];
//                }
//#endif
                var nameObj = user.Find("Name").GetComponent<Text>();
                nameObj.text = Encoding.UTF8.GetString(allRecords[i].userInfo[j].NickName);
                var idObj = user.Find("ID").GetComponent<Text>();
                idObj.text = allRecords[i].userInfo[j].IUserId.ToString();
                var scoreObj = user.Find("Mark").GetComponent<Text>();
                scoreObj.text = allRecords[i].ScoreTotal[j].ToString();
            }

            var DetailsButton = infoItemObj.transform.Find("DetailsButton").GetComponent<Button>();
            DetailsButton.onClick.RemoveAllListeners();
            var RecordIdx = i;
            DetailsButton.onClick.AddListener(() =>
            {
                AllRecordsWin.SetActive(false);
                DetailRecordWin.SetActive(true);
                UpdateDetailInfo(RecordIdx);
            });
            //Set Pos
            Vector3 anchoredPos = RecordTemplete.GetComponent<RectTransform>().anchoredPosition;
             anchoredPos.y = curAnchoredPosY;
             infoItemObj.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
             curAnchoredPosY -= posStep;

             //设置Content的Bottom
             Vector2 offsetMinTmp = viewportContent.GetComponent<RectTransform>().offsetMin;
             offsetMinTmp.y -= posStep;
             viewportContent.GetComponent<RectTransform>().offsetMin = offsetMinTmp;
        }

    }

    void UpdateDetailInfo(int iRecordIdx)
    {
        var viewportContent = DetailRecordWin.transform.Find("Scroll View/Viewport/Content");
        if (viewportContent == null)
        {
            return;
        }

        //设置Content的Bottom
        Vector2 offsetMin = viewportContent.GetComponent<RectTransform>().offsetMin;
        offsetMin.y = -400.0f;
        viewportContent.GetComponent<RectTransform>().offsetMin = offsetMin;

        var RecordTemplete = viewportContent.Find("PlayInfoBack");
        if (RecordTemplete == null)
        {
            return;
        }

        RecordTemplete.gameObject.SetActive(false);

        var oneRecordDetail = PlayBackStorage.GetInstance().GetRecord(iRecordIdx);
        var replyCount = oneRecordDetail.ScorePerMatch.GetLength(0);
        if (detailList.Count < replyCount)
        {
            for (int j = detailList.Count; j < replyCount; j++)
            {
                var infoItemObj = Instantiate(RecordTemplete.gameObject);
                infoItemObj.transform.SetParent(viewportContent);
                infoItemObj.transform.localScale = RecordTemplete.localScale;
                detailList.Add(infoItemObj);
            }
        }

        for (int i = 0; i < detailList.Count; i++)
        {
            detailList[i].SetActive(false);
        }

        //填充上方玩家头像等数据
        var MainInfo = DetailRecordWin.transform.Find("MainInfo");
        for (int j = 0; j < 4; j++)
        {
            var user = MainInfo.transform.Find(string.Format("User{0}", j + 1));
//#if UNITY_ANDROID || UNITY_IOS//头像
//                if (oneRecordDetail.headSprites[j] != null)
//                {
//                    user.GetComponent<Image>().sprite = oneRecordDetail.headSprites[j];
//                }
//#endif
            var nameObj = user.Find("Name").GetComponent<Text>();
            nameObj.text = Encoding.UTF8.GetString(oneRecordDetail.userInfo[j].NickName);
            var idObj = user.Find("ID").GetComponent<Text>();
            idObj.text = oneRecordDetail.userInfo[j].IUserId.ToString();
        }

        //填充数据
        float curAnchoredPosY = RecordTemplete.GetComponent<RectTransform>().anchoredPosition.y;
        float posStep = 88;

        for (int i = 0; i < replyCount; i++)
        {
            GameObject infoItemObj = detailList[i];
            if (infoItemObj == null)
            {
                continue;
            }
            infoItemObj.SetActive(true);

            //序号
            var numObj = infoItemObj.transform.Find("Num");
            if (numObj != null)
            {
                numObj.GetComponent<Text>().text = (i + 1).ToString();
            }

            DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();

            dtFormat.ShortDatePattern = "yyyyMMddhhmmss";
            DateTime dt = Convert.ToDateTime(oneRecordDetail.matchStartTime[i], dtFormat);//第一局时间当做本场开始时间

            //日期
            var dayObj = infoItemObj.transform.Find("Day");
            if (dayObj != null)
            {
                dayObj.GetComponent<Text>().text = dt.ToString("yyyy-MM-dd");
            }

            //时间
            var timeObj = infoItemObj.transform.Find("Time");
            if (timeObj != null)
            {
                timeObj.GetComponent<Text>().text = dt.ToString("hh:mm:ss");
            }

            //每局得分
            for (int j = 0; j < 4; j++)
            {
                var scoreObj = infoItemObj.transform.Find("Mark"+j);
                if (scoreObj != null)
                {
                    scoreObj.GetComponent<Text>().text = oneRecordDetail.ScorePerMatch[i,j].ToString();
                }
            }

            //回放按钮
            var playBackBtn = infoItemObj.transform.Find("PlayBack").GetComponent<Button>();
            playBackBtn.onClick.RemoveAllListeners();
            var iMatchIdx = i;
            playBackBtn.onClick.AddListener(() =>
            {
                StartReply(iRecordIdx, iMatchIdx);
            });

            //Set Pos
            Vector3 anchoredPos = RecordTemplete.GetComponent<RectTransform>().anchoredPosition;
            anchoredPos.y = curAnchoredPosY;
            infoItemObj.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
            curAnchoredPosY -= posStep;

            //设置Content的Bottom
            Vector2 offsetMinTmp = viewportContent.GetComponent<RectTransform>().offsetMin;
            offsetMinTmp.y -= posStep;
            viewportContent.GetComponent<RectTransform>().offsetMin = offsetMinTmp;
        }
    }
}
