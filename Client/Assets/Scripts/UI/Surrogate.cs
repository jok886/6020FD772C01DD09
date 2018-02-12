using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

public class Surrogate : MonoBehaviour {
    public GameObject AddSurrogateWin;
    public GameObject SurrogateInfoWin;
    public GameObject SurrogateMoneyWin;
    //public GameObject ButtonWin;//窗口下按钮底图
    public Text SelfInviteCodeText;

    public Button CashOutButton;

    private uint m_dwSpreaderID;
    private uint m_dwParentSpreaderID;//上级代理ID
    private ushort m_wSpreaderLevel;
    private string m_szSpreaderRealName;
    private string m_szSpreaderIDCardNo;
    private string m_szSpreaderTelNum;
    private string m_szSpreaderWeiXinAccount;

    private int m_infoIdxToRemove;
    private List<GameObject> m_infoItemObjList;

    private List<GameObject> m_paymentInfoItemObjList;
    private double m_dTotalGrantFromChildrenBuy;

    //log
    public Text LogText;
    float m_fLogTimer = 0;

    public static char[] s_codeDict = new char[] {
              'V', '9', 'C', '2', 'Z', 'L',
              'G', '5', 'Y', 'H', 'K', '3', 'I', '8', 'J',  
              'M', '4', 'N', 'T', 'F', '6', 'O', 'R', 'W',
              'S', '0', 'E', '7','U', '1', 'B',  
              'D',
    };
    public static char[] s_scrambleCodeDict = new char[] {
              'X','A','Q','P'
    };
    public static int s_codeLength = 5;

    // Use this for initialization
    void Start()
    {
        m_dwSpreaderID = 0;
        m_dwParentSpreaderID = 0;
        m_wSpreaderLevel = 1;
        m_infoIdxToRemove = -1;

        m_szSpreaderRealName = "";
        m_szSpreaderTelNum = "";
        m_szSpreaderWeiXinAccount = "";

        m_infoItemObjList = new List<GameObject>();
        m_infoItemObjList.Clear();

        m_paymentInfoItemObjList = new List<GameObject>();
        m_paymentInfoItemObjList.Clear();
        m_dTotalGrantFromChildrenBuy = 0;

        if (LogText != null)
        {
            LogText.text = "";
            LogText.gameObject.SetActive(true);
        }

        GameNet.GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
        GameNet.tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
        if (SelfInviteCodeText != null)
        {
            string strCode = IdToInviteCode(pGlobalUserData.dwUserID);
            uint userId = InviteCodeToId(strCode);
            SelfInviteCodeText.text = string.Format("您的邀请码是：{0}", strCode);
            SelfInviteCodeText.gameObject.SetActive(true);
        }

        //GameNet.tagSpreadersInfo pGlobalSpreadersInfo = pGlobalUserInfo.GetSpreadersInfo();
        //if(pGlobalSpreadersInfo.wItemCount==0)
        //{
        //    GameNet.UserInfo.getInstance().querySpreadersInfo(this);
        //}
    }

    //// Update is called once per frame
    //void Update () {

    //}

    public bool IsIDCardValidate(string strIdCard)
    {
        return true;

        int[] weight = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
        char[] validate = { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };
        int sum = 0;
        int mode = 0;
        int nlength = strIdCard.Length;
        if (nlength == 18)
        {
            for (int i = 0; i < nlength - 1; i++)
            {
                sum = sum + (strIdCard[i] - '0') * weight[i];
            }
            mode = sum % 11;
            if (validate[mode] == strIdCard[17])
            {
                return true;
            }
        }

        return false;
    }

    public void AddSurrogate(Toggle toggle)
    {
        if(toggle.isOn)
        {
            //ButtonWin.SetActive(true);
            AddSurrogateWin.SetActive(true);
            SurrogateInfoWin.SetActive(false);
            SurrogateMoneyWin.SetActive(false);
        }
    }

    public void SurrogateInfo(Toggle toggle)
    {
        if (toggle.isOn)
        { 
            //ButtonWin.SetActive(true);
            AddSurrogateWin.SetActive(false);
            SurrogateInfoWin.SetActive(true);
            SurrogateMoneyWin.SetActive(false);

            //GameNet.GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
            //GameNet.tagSpreadersInfo pGlobalSpreadersInfo = pGlobalUserInfo.GetSpreadersInfo();
            //if (pGlobalSpreadersInfo.wItemCount == 0)
            //{
            //    GameNet.UserInfo.getInstance().querySpreadersInfo(this);
            //}
            //else
            //{
            //    UpdateSpreadersListView();
            //}
            GameNet.UserInfo.getInstance().querySpreadersInfo(this);
        }
    }
    
    public void SurrogateMoney(Toggle toggle)
    {
        if (toggle.isOn)
        {
            //ButtonWin.SetActive(false);
            AddSurrogateWin.SetActive(false);
            SurrogateInfoWin.SetActive(false);
            SurrogateMoneyWin.SetActive(true);

            GameNet.UserInfo.getInstance().queryChildrenPaymentInfo(this);
        }
    }

    public void ShowLog(string strLogText)
    {
        if(LogText != null)
        {
            LogText.text = strLogText;
            //LogText.gameObject.SetActive(true);

            m_fLogTimer = 0f;
        }
    }

    private void Update()
    {
        if(LogText != null && LogText.text != "")
        {
            m_fLogTimer += Time.deltaTime;
            if(m_fLogTimer > 3f)
            {
                LogText.text = "";
                m_fLogTimer = 0f;
            }
        }
    }

    public static string IdToInviteCode(uint dwUserID)
    {
        string resultCode = "";

        uint userID = dwUserID;
        uint binLen = (uint)s_codeDict.Length;
        do
        {
            uint mod = userID % binLen;
            resultCode = s_codeDict[mod] + resultCode;

            userID = userID / binLen;
        }while (userID > 0);

        //扰码
        System.Random rand = new System.Random();
        int scrambleCodeIdx = 0;
        while (resultCode.Length < s_codeLength)
        {
            //scrambleCodeIdx = rand.Next(0, s_scrambleCodeDict.Length);

            resultCode = s_scrambleCodeDict[scrambleCodeIdx] + resultCode;
            scrambleCodeIdx++;
        }

        return resultCode;
    }

    public static uint InviteCodeToId(string strCode)
    {
        char[] codes = strCode.ToCharArray();
        uint resultId = 0;

        bool bScramblerIsEnd = false;

        for (int i = 0; i < codes.Length; i++)
        {
            //扰码部分  
            if (!bScramblerIsEnd)
            {
                if (IsScrambler(codes[i]))
                {
                    continue;
                }
                else
                {
                    bScramblerIsEnd = true;
                }
            }

            int idx = 0;
            for (int j = 0; j < s_codeDict.Length; j++)
            {
                if (codes[i] == s_codeDict[j])
                {
                    idx = j;
                    break;
                }
            }

            resultId = resultId * (uint)s_codeDict.Length + (uint)idx;
        }

        return resultId;
    }

    private static bool IsScrambler(char cScrambleCode)
    {
        for(int i=0; i< s_scrambleCodeDict.Length; i++)
        {
            if(cScrambleCode== s_scrambleCodeDict[i])
            {
                return true;
            }
        }

        return false;
    }

    //是否是下级代理
    private bool IsMyChild(uint myUserID, int mySpreaderLevel, uint childUserIDToCheck)
    {
        GameNet.GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
        GameNet.tagSpreadersInfo pGlobalSpreadersInfo = pGlobalUserInfo.GetSpreadersInfo();

        //获取这个childUser的SpreaderInfoItem
        bool bIsChildUserInSpreaderList = false;
        int nItemNum = pGlobalSpreadersInfo.wItemCount;
        GameNet.SpreaderInfoItem childSpreaderInfoItem = new GameNet.SpreaderInfoItem();
        for (int i=0; i< nItemNum; i++)
        {
            if(pGlobalSpreadersInfo.SpreaderInfoItems[i].SpreaderId == childUserIDToCheck)
            {
                bIsChildUserInSpreaderList = true;
                childSpreaderInfoItem = pGlobalSpreadersInfo.SpreaderInfoItems[i];
                break;
            }
        }
        if(!bIsChildUserInSpreaderList)
        {
            //这个childUser不是代理（不在代理人列表中）
            return false;
        }

        bool bIsMyChild = false;
        switch (mySpreaderLevel)
        {
            case 0:
                bIsMyChild = true;
                break;

            case 1:
                if (childSpreaderInfoItem.ParentID == myUserID)
                {
                    //直属代理
                    bIsMyChild = true;
                }
                else
                {
                    for (int j = 0; j < nItemNum; j++)
                    {
                        GameNet.SpreaderInfoItem tmpInfoItem1 = pGlobalSpreadersInfo.SpreaderInfoItems[j];
                        if (childSpreaderInfoItem.ParentID == tmpInfoItem1.SpreaderId && tmpInfoItem1.ParentID == myUserID)
                        {
                            //间接代理:隔1层
                            bIsMyChild = true;
                            break;
                        }
                    }
                }
                break;

            case 2:
                bIsMyChild = (childSpreaderInfoItem.ParentID == myUserID);
                break;

            case 3:
                bIsMyChild = false;
                break;

            default:
                bIsMyChild = false;
                break;
        }

        return bIsMyChild;
    }

    public void UpdateChildrenPaymentListView()
    {
        if (SurrogateMoneyWin == null)
        {
            return;
        }

        var viewportContent = SurrogateMoneyWin.transform.Find("SurrogateInfo/Viewport/Content");
        if (viewportContent == null)
        {
            return;
        }

        //设置Content的Bottom
        Vector2 offsetMin = viewportContent.GetComponent<RectTransform>().offsetMin;
        offsetMin.y = -400.0f;
        viewportContent.GetComponent<RectTransform>().offsetMin = offsetMin;

        var Surrogate_1 = viewportContent.Find("Surrogate_1");
        if (Surrogate_1 == null)
        {
            return;
        }
        Surrogate_1.gameObject.SetActive(false);

        GameNet.GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
        GameNet.tagChildrenPaymentInfo pGlobalChildrenPaymentInfo = pGlobalUserInfo.GetChildrenPaymentInfo();
        Debug.Log("Surrogate:UpdateChildrenPaymentListView:wItemCount=" + pGlobalChildrenPaymentInfo.wItemCount);

        //填充数据
        float curAnchoredPosY = Surrogate_1.GetComponent<RectTransform>().anchoredPosition.y;
        float posStep = 50;
        int nItemNum = pGlobalChildrenPaymentInfo.wItemCount;// *10;
        if (m_paymentInfoItemObjList.Count < nItemNum)
        {
            for (int j = m_paymentInfoItemObjList.Count; j < nItemNum; j++)
            {
                var infoItemObj = Instantiate(Surrogate_1.gameObject);
                infoItemObj.transform.SetParent(viewportContent);
                infoItemObj.transform.localScale = Surrogate_1.localScale;
                //infoItemObj.SetActive(false);

                m_paymentInfoItemObjList.Add(infoItemObj);
            }
        }
        for (int i = 0; i < m_paymentInfoItemObjList.Count; i++)
        {
            m_paymentInfoItemObjList[i].SetActive(false);
        }

        m_dTotalGrantFromChildrenBuy = 0.0;
        for (int i = 0; i < nItemNum; i++)
        {

            GameNet.PaymentInfoItem paymentInfoItem = pGlobalChildrenPaymentInfo.PaymentInfoItems[i];

            GameObject infoItemObj = m_paymentInfoItemObjList[i];
            if (infoItemObj == null)
            {
                continue;
            }
            infoItemObj.SetActive(true);

            //序号
            var numObj = infoItemObj.transform.Find("SurrogateNum");
            if (numObj != null)
            {
                numObj.GetComponent<Text>().text = Convert.ToString(i + 1);
            }

            //姓名
            var nameObj = infoItemObj.transform.Find("SurrogateName");
            if (nameObj != null)
            {
                nameObj.GetComponent<Text>().text = paymentInfoItem.UserId.ToString(); ;
            }

            //操作
            var operateObj = infoItemObj.transform.Find("SurrogateOperation");
            if (operateObj != null)
            {
                operateObj.GetComponent<Text>().text = "充值";
            }

            //当前操作金额
            var totalObj = infoItemObj.transform.Find("SurrogateTotal");
            if (totalObj != null)
            {
                double curTotalGrant = paymentInfoItem.Payment * paymentInfoItem.PaymentGrantRate;
                m_dTotalGrantFromChildrenBuy += curTotalGrant;
                totalObj.GetComponent<Text>().text = curTotalGrant.ToString();
            }

            //Set Pos
            Vector3 anchoredPos = Surrogate_1.GetComponent<RectTransform>().anchoredPosition;
            anchoredPos.y = curAnchoredPosY;
            infoItemObj.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
            curAnchoredPosY -= posStep;

            //设置Content的Bottom
            Vector2 offsetMinTmp = viewportContent.GetComponent<RectTransform>().offsetMin;
            offsetMinTmp.y -= posStep;
            viewportContent.GetComponent<RectTransform>().offsetMin = offsetMinTmp;
        }

        var transTotalMoney = SurrogateMoneyWin.transform.Find("TotalMoney");
        if(transTotalMoney != null)
        {
            Text textTotalMoney = transTotalMoney.GetComponent<Text>();
            if(textTotalMoney != null)
            {
                textTotalMoney.text = string.Format("可提现金额：{0}", pGlobalChildrenPaymentInfo.dTotalLeftCash);
            }
        }

        Surrogate_1.gameObject.SetActive(false);
    }

    public void UpdateSpreadersListView()
    {
        if (SurrogateInfoWin == null)
        {
            return;
        }

        var viewportContent = SurrogateInfoWin.transform.Find("SurrogateInfo/Viewport/Content");
        if (viewportContent == null)
        {
            return;
        }
        //设置Content的Bottom
        Vector2 offsetMin = viewportContent.GetComponent<RectTransform>().offsetMin;
        offsetMin.y = -400.0f;
        viewportContent.GetComponent<RectTransform>().offsetMin = offsetMin;

        var Surrogate_1 = viewportContent.Find("Surrogate_1");
        if (Surrogate_1 == null)
        {
            return;
        }
        Surrogate_1.gameObject.SetActive(false);

        GameNet.GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
        GameNet.tagSpreadersInfo pGlobalSpreadersInfo = pGlobalUserInfo.GetSpreadersInfo();
        Debug.Log("Surrogate:CreateSpreadersListView:wItemCount=" + pGlobalSpreadersInfo.wItemCount);

        //填充数据
        float curAnchoredPosY = Surrogate_1.GetComponent<RectTransform>().anchoredPosition.y;
        float posStep = 50;
        int nItemNum = pGlobalSpreadersInfo.wItemCount;// *10;
        if (m_infoItemObjList.Count < nItemNum)
        {
            for (int j = m_infoItemObjList.Count; j < nItemNum; j++)
            {
                var infoItemObj = Instantiate(Surrogate_1.gameObject);
                infoItemObj.transform.SetParent(viewportContent);
                infoItemObj.transform.localScale = Surrogate_1.localScale;
                //infoItemObj.SetActive(false);

                m_infoItemObjList.Add(infoItemObj);
            }
        }
        for (int i = 0; i < m_infoItemObjList.Count; i++)
        {
            m_infoItemObjList[i].SetActive(false);
        }
        for (int i = 0; i < nItemNum; i++)
        {
            ///int i = k % pGlobalSpreadersInfo.wItemCount;

            GameNet.SpreaderInfoItem spreaderInfoItem = pGlobalSpreadersInfo.SpreaderInfoItems[i];

            ////校验自己名下的代理
            //uint dwSelfUserId = pGlobalUserData.dwUserID;
            //if (pGlobalUserData.iSpreaderLevel == 0)
            //{
            //    //全显示
            //}
            //else if (pGlobalUserData.iSpreaderLevel == 3)
            //{
            //    break;
            //}

            //bool bIsMyChild = IsMyChild(pGlobalUserData.dwUserID, pGlobalUserData.iSpreaderLevel, spreaderInfoItem.SpreaderId);
            //if (!bIsMyChild)
            //{
            //    continue;
            //}
            /*
            else if (pGlobalUserData.iSpreaderLevel == 1)
            {
                if (spreaderInfoItem.ParentID == dwSelfUserId)
                {
                    //直属代理
                    bIsMyChild = true;
                }
                else
                {
                    for (int j = 0; j < nItemNum; j++)
                    {
                        GameNet.SpreaderInfoItem tmpInfoItem1 = pGlobalSpreadersInfo.SpreaderInfoItems[j];
                        if (spreaderInfoItem.ParentID == tmpInfoItem1.SpreaderId && tmpInfoItem1.ParentID == dwSelfUserId)
                        {
                            //间接代理:隔1层
                            bIsMyChild = true;
                        }
                    }
                }

                if (!bIsMyChild)
                {
                    continue;
                }
            }
            else if (pGlobalUserData.iSpreaderLevel == 2)
            {
                bIsMyChild = (spreaderInfoItem.ParentID == dwSelfUserId);
                if (!bIsMyChild)
                {
                    continue;
                }
            }
            //*/

            GameObject infoItemObj = m_infoItemObjList[i];
            if (infoItemObj == null)
            {
                continue;
            }
            infoItemObj.SetActive(true);

            //序号
            var numObj = infoItemObj.transform.Find("SurrogateNum");
            if (numObj != null)
            {
                numObj.GetComponent<Text>().text = Convert.ToString(i + 1);
            }

            //姓名
            var nameObj = infoItemObj.transform.Find("SurrogateName");
            if (nameObj != null)
            {
                string strName = GameNet.GlobalUserInfo.GBToUtf8(spreaderInfoItem.RealName);
                nameObj.GetComponent<Text>().text = strName;
            }

            //代理ID
            var idObj = infoItemObj.transform.Find("SurrogateID");
            if (idObj != null)
            {
                idObj.GetComponent<Text>().text = spreaderInfoItem.SpreaderId.ToString();
            }

            //上级代理ID
            var parentIdObj = infoItemObj.transform.Find("HigherLevelSuttogateID");
            if (parentIdObj != null)
            {
                parentIdObj.GetComponent<Text>().text = spreaderInfoItem.ParentID.ToString();
            }

            //代理等级
            var levelObj = infoItemObj.transform.Find("SurrogateGrading");
            if (levelObj != null)
            {
                levelObj.GetComponent<Text>().text = spreaderInfoItem.SpreaderLevel.ToString();
            }

            //微信号
            var weiXinObj = infoItemObj.transform.Find("WeiXin");
            if (weiXinObj != null)
            {
                string strWeiXin = Encoding.Default.GetString(spreaderInfoItem.WeiXinAccount);
                weiXinObj.GetComponent<Text>().text = strWeiXin;
            }

            //电话
            var telNumObj = infoItemObj.transform.Find("TelNum");
            if (telNumObj != null)
            {
                string strTelNum = Encoding.Default.GetString(spreaderInfoItem.TelNum);
                telNumObj.GetComponent<Text>().text = strTelNum;
            }

            //邀请码
            var inviteObj = infoItemObj.transform.Find("Invite");
            if (inviteObj != null)
            {
                string strCode = IdToInviteCode(spreaderInfoItem.SpreaderId);
                //uint userId = InviteCodeToId(strCode);
                inviteObj.GetComponent<Text>().text = strCode;
            }

            //选项框
            var removeToggle = infoItemObj.transform.Find("Info1").GetComponent<Toggle>();
            ///var toggleGroup = Surrogate_1.GetComponent<ToggleGroup>();
            //removeToggle.group = toggleGroup;
            removeToggle.onValueChanged.RemoveAllListeners();
            int idx = i;
            removeToggle.onValueChanged.AddListener(delegate (bool isOn)
            {
                this.OnRemoveToggleValueChanged(isOn, removeToggle.gameObject, idx);
            });


            //Set Pos
            Vector3 anchoredPos = Surrogate_1.GetComponent<RectTransform>().anchoredPosition;
            anchoredPos.y = curAnchoredPosY;
            infoItemObj.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
            curAnchoredPosY -= posStep;

            //设置Content的Bottom
            Vector2 offsetMinTmp = viewportContent.GetComponent<RectTransform>().offsetMin;
            offsetMinTmp.y -= posStep;
            viewportContent.GetComponent<RectTransform>().offsetMin = offsetMinTmp;
        }

        Surrogate_1.gameObject.SetActive(false);
    }

    public void OnIDInputEnd(InputField inputField)
    {
        uint dwId = 0;
        bool result = uint.TryParse(inputField.text, out dwId);
        if(result)
        {
            m_dwSpreaderID = dwId;//Convert.ToUInt32(inputField.text);
        }
        else
        {
            m_dwSpreaderID = 0;
            ShowLog("代理ID必须是数字");
        }
    }

    public void OnParentIDInputEnd(InputField inputField)
    {
        uint dwParentID = 0;
        bool result = uint.TryParse(inputField.text, out dwParentID);
        if (result)
        {
            m_dwParentSpreaderID = dwParentID;//Convert.ToUInt32(inputField.text);
        }
        else
        {
            m_dwParentSpreaderID = 0;
            ShowLog("上级代理ID必须是数字");
        }
    }

    public void OnNameInputEnd(InputField inputField)
    {
        if (inputField.text.Length > 30)
        {
            m_szSpreaderRealName = "";
            ShowLog("名字超长");
            return;
        }

        m_szSpreaderRealName = inputField.text;
    }

    public void OnTellNumInputEnd(InputField inputField)
    {
        Int64 dwId = 0;
        bool result = Int64.TryParse(inputField.text, out dwId);
        if (result)
        {
            if(inputField.text.Length != 11)
            {
                m_szSpreaderTelNum = "";
                ShowLog("电话号码必须是11位");
            }
            else
            {
                m_szSpreaderTelNum = inputField.text;
            }
        }
        else
        {
            m_szSpreaderTelNum = "";
            ShowLog("电话号码必须是数字");
        }
    }

    public void OnWeiXinAccountInputEnd(InputField inputField)
    {
        m_szSpreaderWeiXinAccount = inputField.text;
    }

    public void OnIDCardInputEnd(InputField inputField)
    {
        m_szSpreaderIDCardNo = inputField.text;
    }

    public void OnButtonAddDelSpreader(bool bIsAdd)
    {
        if(m_dwSpreaderID == 0)
        {
            ShowLog("请输入有效的代理ID");
            return;
        }

        if (m_dwParentSpreaderID == 0)
        {
            ShowLog("请输入有效的上级代理ID");
            return;
        }
        
        if (m_szSpreaderRealName.Length == 0)
        {
            ShowLog("请输入姓名");
            return;
        }

        if (m_szSpreaderTelNum.Length != 11)
        {
            ShowLog("请输入有效的电话号码");
            return;
        }

        if(m_szSpreaderWeiXinAccount.Length == 0)
        {
            ShowLog("请输入微信号");
            return;
        }

        ///bool bIsIDCardValidate = IsIDCardValidate(m_szSpreaderIDCardNo);
        
        GameNet.UserInfo.getInstance().addDelSpreader(m_dwSpreaderID, m_szSpreaderRealName, m_szSpreaderTelNum, m_szSpreaderWeiXinAccount, m_dwParentSpreaderID, m_wSpreaderLevel, bIsAdd);
    }


    public void OnButtonDelSpreader()
    {
        GameNet.GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
        GameNet.tagSpreadersInfo pGlobalSpreadersInfo = pGlobalUserInfo.GetSpreadersInfo();

        if (m_infoIdxToRemove >= 0 && m_infoIdxToRemove < pGlobalSpreadersInfo.wItemCount)
        {
            GameNet.SpreaderInfoItem infoItem = pGlobalSpreadersInfo.SpreaderInfoItems[m_infoIdxToRemove];

            string strRealName = GameNet.GlobalUserInfo.GBToUtf8(infoItem.RealName);
            //int len = strRealName.Length;
            //var szSpreaderRealName = Encoding.UTF8.GetBytes(strRealName);

            ///string strIDCardNo = GameNet.GlobalUserInfo.GBToUtf8(infoItem.IDCardNo);
            string strTelNum = GameNet.GlobalUserInfo.GBToUtf8(infoItem.TelNum);
            string strWeiXinAccount = GameNet.GlobalUserInfo.GBToUtf8(infoItem.WeiXinAccount);
            GameNet.UserInfo.getInstance().addDelSpreader(infoItem.SpreaderId, strRealName, strTelNum, strWeiXinAccount, infoItem.ParentID, infoItem.SpreaderLevel, false);
        }
    }

    public void EnableCashOut()
    {
        if (CashOutButton != null)
        {
            CashOutButton.enabled = true;
        }
    }

    public void OnButtonCashOut()
    {
        if (SurrogateMoneyWin == null)
        {
            return;
        }

        var transTotalMoney = SurrogateMoneyWin.transform.Find("TotalMoney");
        if (transTotalMoney != null)
        {
            Text textTotalMoney = transTotalMoney.GetComponent<Text>();
            if (textTotalMoney != null)
            {
                //string strTotalMoney = textTotalMoney.text;
                //string strPurTotalMoney = strTotalMoney.Replace("可提现金额：","");
                //double dTotalMoney = double.Parse(strPurTotalMoney);

                GameNet.GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
                GameNet.tagChildrenPaymentInfo pGlobalChildrenPaymentInfo = pGlobalUserInfo.GetChildrenPaymentInfo();
                uint dwPayment = (uint)(pGlobalChildrenPaymentInfo.dTotalLeftCash * 100.0);//pGlobalChildrenPaymentInfo.dTotalLeftCash*100.0 单位：分
                if(dwPayment > 0)
                {
                    GameNet.UserInfo.getInstance().AddEnterprisePay(dwPayment);

                    if(CashOutButton != null)
                    {
                        CashOutButton.enabled = false;
                    }
                }
                else
                {
                    ShowLog("提现金额低于最小金额");
                }
            }
        }
    }

    public void OnRemoveToggleValueChanged(bool ison, GameObject sender, int infoIdx)
    {
        //sender.name

        if(ison)
        {
            m_infoIdxToRemove = infoIdx;
        }
    }


    public void OnGradeChanged1(Toggle toggle)
    {
        if(toggle.isOn)
        {
            m_wSpreaderLevel = 1;
        }
    }

    public void OnGradeChanged2(Toggle toggle)
    {
        if (toggle.isOn)
        {
            m_wSpreaderLevel = 2;
        }
    }

    public void OnGradeChanged3(Toggle toggle)
    {
        if (toggle.isOn)
        {
            m_wSpreaderLevel = 3;
        }
    }


    public void OnQuerySpreadersInfo()
    {

    }
}
