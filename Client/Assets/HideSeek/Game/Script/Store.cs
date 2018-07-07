using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameNet;

public class Store : MonoBehaviour
{
    public const int row = 2;   //列数
    private GameObject m_Canvas;
    public GameObject RechargeView;
    public GameObject DiamondView;
    public GameObject RechargeToggle;
    public GameObject DiamondToggle;
    private List<int> m_ShopItem;
    private Text DiamondNum;
    private Text GlodNum;
    void Start()
    {
        Debug.Log("Stroe Start()!!!");
        m_Canvas = GameObject.Find("Canvas");
        m_ShopItem = new List<int>();

        DiamondNum = m_Canvas.transform.Find("Window/StoreWindow/Diamond/DiamondCount").GetComponent<Text>();
        GlodNum = m_Canvas.transform.Find("Window/StoreWindow/Glod/GlodCount").GetComponent<Text>();
        if (DiamondNum != null)
            DiamondNum.text = GlobalUserInfo.getUserInsure().ToString();
        if (GlodNum != null)
            GlodNum.text = GlobalUserInfo.getUserScore().ToString();

        //TODO  获取服务器商品列表

        GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
        tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
        long lModelIndex0 = pGlobalUserData.lModelIndex0;
        for (int idx = 0; idx < 64; idx++)
        {
            if (((int)lModelIndex0 & 0x01) != 0)
            {
                m_ShopItem.Add(idx);
            }

            lModelIndex0 = lModelIndex0 >> 1;
        }
        //m_ShopItem.Add(1);
        ////m_ShopItem.Add(2);
        //m_ShopItem.Add(3);
        ////m_ShopItem.Add(4);

        ShopItemInit();
    }

    void Update()
    {
        UpdateInfo();
    }
    private void GetShopItemFromServer()
    {

    }
    private string GetCharacterNameById(int id)
    {
        string name = null;
        for (int i = 0; i < HNGameManager.listTagger.Count; i++)
        {
            if (HNGameManager.listTagger[i].Index == id)
            {
                name = HNGameManager.listTagger[i].FileName;
                break;
            }
        }
        return name;
    }
    private string GetCharacterPathById(int id)
    {
        string path = null;
        for (int i = 0; i < HNGameManager.listTagger.Count; i++)
        {
            if (HNGameManager.listTagger[i].Index == id)
            {
                path = HNGameManager.listTagger[i].Path;
                break;
            }
        }
        return path;
    }
    private void ShopItemInit()
    {
        GameObject StoreObj = m_Canvas.transform.Find("Window/StoreWindow").gameObject;
        GameObject Content = StoreObj.transform.Find("RechargeView/Viewport/Content").gameObject;
        Content.GetComponent<RectTransform>().sizeDelta = new Vector2(1000, 300 * (float)Math.Ceiling((double)
            HNGameManager.listTagger.Count / row));
        for (int i = 0; i < HNGameManager.listTagger.Count; i++)
        {
            GameObject loadObj = Resources.Load("UI/Store/Prefabs/Purchase0") as GameObject;
            GameObject shopObj = Instantiate(loadObj);
            shopObj.transform.SetParent(Content.transform, false);
            shopObj.name = HNGameManager.listTagger[i].Index + "";
            shopObj.AddComponent<ShopItemStatus>();
            if (m_ShopItem.Contains(HNGameManager.listTagger[i].Index))
            {
                shopObj.transform.Find("MakeSure/Select").gameObject.SetActive(true);
                shopObj.transform.Find("MakeSure/True").gameObject.SetActive(false);
                shopObj.transform.Find("MakeSure/Diamond").gameObject.SetActive(false);
                shopObj.GetComponent<ShopItemStatus>().m_ItemStatus = (int)ShopItemStatus.ItemStatus.Choose;
            }
            else
            {
                shopObj.transform.Find("MakeSure/Select").gameObject.SetActive(false);
                shopObj.transform.Find("MakeSure/True").gameObject.SetActive(false);
                shopObj.transform.Find("MakeSure/Diamond").gameObject.SetActive(true);
                shopObj.GetComponent<ShopItemStatus>().m_ItemStatus = (int)ShopItemStatus.ItemStatus.Buy;
            }
            if (PlayerPrefs.HasKey("ChoosedModelIndex"))
            {
                if (HNGameManager.listTagger[i].Index == PlayerPrefs.GetInt("ChoosedModelIndex") && m_ShopItem.Contains(PlayerPrefs.GetInt("ChoosedModelIndex")))
                {
                    shopObj.transform.Find("MakeSure/Select").gameObject.SetActive(false);
                    shopObj.transform.Find("MakeSure/True").gameObject.SetActive(true);
                    shopObj.transform.Find("MakeSure/Diamond").gameObject.SetActive(false);
                    shopObj.GetComponent<ShopItemStatus>().m_ItemStatus = (int)ShopItemStatus.ItemStatus.Choosed;
                }
            }
            else
            {
                if (i == 0)
                {
                    shopObj.transform.Find("MakeSure/Select").gameObject.SetActive(false);
                    shopObj.transform.Find("MakeSure/True").gameObject.SetActive(true);
                    shopObj.transform.Find("MakeSure/Diamond").gameObject.SetActive(false);
                    shopObj.GetComponent<ShopItemStatus>().m_ItemStatus = (int)ShopItemStatus.ItemStatus.Choosed;
                }
            }

            //设置价格
            Transform diamondCount = shopObj.transform.Find("MakeSure/Diamond/Count");
            if (diamondCount != null)
            {
                diamondCount.GetComponent<Text>().text = "2";
            }

            shopObj.transform.Find("PurchaseInfo").GetComponent<Image>().overrideSprite = Resources.Load("UI/Store/Sprite/" + HNGameManager.listTagger[i].FileName, typeof(Sprite)) as Sprite;
            switch (i % row)
            {
                case 0://第一列
                    shopObj.transform.localPosition = new Vector3(-350 + 500, i / row * -300 + (-150), 0);
                    break;
                case 1://第二列
                    shopObj.transform.localPosition = new Vector3(0 + 500, i / row * -300 + (-150), 0);
                    break;
                case 2://第三列
                    shopObj.transform.localPosition = new Vector3(350 + 500, i / row * -300 + (-150), 0);
                    break;
            }
        }
    }
    public void UpdateInfo()
    {
        if (DiamondNum != null)
            DiamondNum.text = GlobalUserInfo.getUserInsure().ToString();
        if (GlodNum != null)
            GlodNum.text = GlobalUserInfo.getUserScore().ToString();
    }
    public void ShowWinForModel()
    {
        RechargeView.SetActive(true);
        DiamondView.SetActive(false);
        DiamondToggle.GetComponent<Toggle>().isOn = false;
        RechargeToggle.GetComponent<Toggle>().isOn = true;
    }
    public void ShowWinForDiamond()
    {
        RechargeView.SetActive(false);
        DiamondView.SetActive(true);
        RechargeToggle.GetComponent<Toggle>().isOn = false;
        DiamondToggle.GetComponent<Toggle>().isOn = true;
    }
}
