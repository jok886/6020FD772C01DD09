using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class BuyButton : MonoBehaviour
{
    public EventHandle eventHandle;
#if UNITY_IPHONE
	//[DllImport("__Internal")]  
	//private static extern void requestProduct();

	//[DllImport("__Internal")]  
	//private static extern void paymentWithProduct(int iapID);

	//[DllImport("__Internal")]  
	//private static extern bool IsRequestSuccess();

#endif

    private HNGameManager hnManger;
    void Start()
    {
        hnManger = GameObject.Find("HNGameManager").GetComponent<HNGameManager>();
        eventHandle = GameObject.FindObjectOfType<EventHandle>();
    }

    public void buyProduct(int iapID)
    {
        if(false)
        {
            GameObject Obj = GameObject.Find("Canvas/Window/TipsNoBtnWindow").gameObject;
            Obj.SetActive(true);
        }
        else
        {
#if UNITY_IPHONE
        if (eventHandle == null)
            eventHandle = GameObject.FindObjectOfType<EventHandle>();
        eventHandle.onPay(iapID);
#elif UNITY_ANDROID //&& !UNITY_EDITOR
            if (eventHandle == null)
                eventHandle = GameObject.FindObjectOfType<EventHandle>();
            eventHandle.onPay(iapID);

#elif UNITY_STANDALONE || UNITY_EDITOR
        GameNet.CMD_GP_ShopItemInfo kNetInfo = new GameNet.CMD_GP_ShopItemInfo();
        kNetInfo.Init();

        byte[] a = new byte[50];
        byte[] b = new byte[32];
        a = System.Text.Encoding.Default.GetBytes("UID");
        string str = "";
        DateTime dt = DateTime.Now;
        str = dt.Ticks.ToString();
        str += (MersenneTwister.MT19937.Int63() % 100).ToString("D2");
        b = System.Text.Encoding.Default.GetBytes(str);
        Buffer.BlockCopy(a, 0, kNetInfo.szUID, 0, a.Length);
        Buffer.BlockCopy(b, 0, kNetInfo.szOrderID, 0, b.Length);
        kNetInfo.wItemID = (ushort)iapID;
        kNetInfo.wAmount = 1;
        kNetInfo.wCount = 10;
        GameNet.UserInfo.getInstance().queryAddShopItem(kNetInfo);
#endif
        }
    }
    public void ExchangeInsureToScore(int itemID)
    {
        int amount = 0;
        switch (itemID)
        {
            case 0:
                amount = 50;
                break;
            case 1:
                amount = 100;
                break;
            case 2:
                amount = 200;
                break;
        }
        GameNet.UserInfo.getInstance().requestExchangeScore((byte)itemID, 1, amount);
    }
}
