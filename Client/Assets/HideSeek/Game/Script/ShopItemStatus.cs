using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameNet;

public class ShopItemStatus : MonoBehaviour
{
    public enum ItemStatus
    {
        Buy,
        Choose,
        Choosed
    }
    public int m_ItemStatus;
    private Button MakeSure;
    void Start()
    {
        MakeSure = transform.Find("MakeSure").GetComponent<Button>();
        MakeSure.onClick.AddListener(() => { SwitchButton(); });
    }
    public void SetStatus(ItemStatus status) 
    {
        m_ItemStatus = (int)status;
    }
    public int GetStatus()
    {
        return m_ItemStatus;
    }
    private void SwitchButton()
    {
        switch ((ItemStatus)m_ItemStatus)
        {
            case ItemStatus.Buy:
                Buy();
                break;
            case ItemStatus.Choose:
                Choose();
                break;
            case ItemStatus.Choosed:
                break;
        }
    }
    private void Buy()
    {
        //花费操作，数据上传
        ushort wBoughtModelIndex = Convert.ToUInt16(gameObject.name);
        UserInfo.getInstance().BoughtTaggerModel(2, 1, wBoughtModelIndex);
        return;
    }
    public void OnBought()
    {
        m_ItemStatus = (int)ItemStatus.Choose;
        gameObject.transform.Find("MakeSure/Select").gameObject.SetActive(true);
        gameObject.transform.Find("MakeSure/True").gameObject.SetActive(false);
        gameObject.transform.Find("MakeSure/Diamond").gameObject.SetActive(false);

        if (CreateOrJoinRoom.GetInstance != null)   //刷新UI
            CreateOrJoinRoom.GetInstance.UpdateInfo();
    }
    private void Choose()
    {
        int id = Convert.ToInt32(gameObject.name);
        int oldid = -1;
        if (PlayerPrefs.HasKey("ChoosedModelIndex"))
        {
            oldid = PlayerPrefs.GetInt("ChoosedModelIndex");
        }
        if (id != oldid)
        {
            m_ItemStatus = (int)ItemStatus.Choosed;
            for (int i = 0; i < gameObject.transform.parent.transform.childCount; i++)
            {
                GameObject parent = gameObject.transform.parent.transform.GetChild(i).gameObject;
                if (Convert.ToInt32(parent.name) != id)
                {
                    if (parent.GetComponent<ShopItemStatus>().m_ItemStatus != 0)
                    {
                        parent.transform.FindChild("MakeSure/Select").gameObject.SetActive(true);
                        parent.transform.FindChild("MakeSure/True").gameObject.SetActive(false);
                        parent.transform.FindChild("MakeSure/Diamond").gameObject.SetActive(false);
                        parent.GetComponent<ShopItemStatus>().m_ItemStatus = (int)ItemStatus.Choose;
                    }
                }
                else
                {
                    if (parent.GetComponent<ShopItemStatus>().m_ItemStatus != 0)
                    {
                        parent.transform.FindChild("MakeSure/Select").gameObject.SetActive(false);
                        parent.transform.FindChild("MakeSure/True").gameObject.SetActive(true);
                        parent.transform.FindChild("MakeSure/Diamond").gameObject.SetActive(false);
                        parent.GetComponent<ShopItemStatus>().m_ItemStatus = (int)ItemStatus.Choosed;
                    }
                }
            }
            PlayerPrefs.SetInt("ChoosedModelIndex", id);
            PlayerPrefs.Save();
        }
    }
}
