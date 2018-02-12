using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using GameNet;
public class UserInfoWeChat : MonoBehaviour
{

    public Image HeaderImage;
    public Image Distinction;
    public Text PlayerName;
    public Text PlayerID;

    public Text MatchTimeLabel;
    public GameObject UserInfoWindow;

    // Use this for initialization
    void Start()
    {
        GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
        tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

        var hnManger = GameObject.Find("HNGameManager").GetComponent<HNGameManager>();
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
        //StartCoroutine(HeaderImageLoadAndShow());
#endif

#if UNITY_IPHONE
        if (pGlobalUserData.bGPIsForAppleReview)
        {
            GameObject canvesTmp = GameObject.Find("Canvas");
            if (canvesTmp != null)
            {
                var inviteObj = canvesTmp.transform.Find("Panel/FunctionButtons/Invite");
                if (inviteObj != null)
                {
                    inviteObj.gameObject.SetActive(false);
                }
            }
        }
#endif

        //PlayerName.text = LoginScene.m_kNickName;
        //PlayerID.text = GlobalUserInfo.getUserID().ToString();
        //Distinction.sprite = hnManger.GenderSprites[LoginScene.m_bMale ? 1 : 0];

        if (hnManger != null)
        {
            string strMatchTime = string.Format("比赛时间:  {0:D2}:{1:D2}", hnManger.m_matchStartTime.wHour, hnManger.m_matchStartTime.wMinute);
            strMatchTime = strMatchTime + string.Format("~{0:D2}:{1:D2}", hnManger.m_matchEndTime.wHour, hnManger.m_matchEndTime.wMinute);
            //MatchTimeLabel.text = strMatchTime;
        }

        if(pGlobalUserData.iSpreaderLevel >= 0)
        {
            GameObject canves = GameObject.Find("Canvas");
            if(canves != null)
            {
                var surrogateObj = canves.transform.Find("Panel/FunctionButtons/Surrogate");
                if(surrogateObj != null)
                {
                    surrogateObj.gameObject.SetActive(true);
                }
            }
        }

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(delegate()
        {
            UserInfoWindow.SetActive(true);
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
            UserInfoWindow.transform.Find("UserImage").GetComponent<Image>().sprite = HeaderImage.sprite;
#endif
            UserInfoWindow.transform.Find("UserInfo/NameText").GetComponent<Text>().text = PlayerName.text;
            UserInfoWindow.transform.Find("UserInfo/IDText").GetComponent<Text>().text = PlayerID.text;
            UserInfoWindow.transform.Find("UserInfo/IPText").GetComponent<Text>().text =Encoding.Default.GetString(pGlobalUserData.szLogonIP);
            UserInfoWindow.transform.Find("UserInfo/DateText").GetComponent<Text>().text =
                DateTime.Today.ToShortDateString();
        });
    }

    IEnumerator HeaderImageLoadAndShow()
    {
        //Debug.Log("mChen HeaderImageLoadAndShow2:headURL=" + LoginScene.m_headURL);

        WWW www = new WWW("");
        yield return www;

        //Texture2D tex = www.texture;
        //HeaderImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }


    public void OnMusicVolumeChange(Slider slider)
    {
        var hnManger = GameObject.Find("HNGameManager").GetComponent<HNGameManager>();
        hnManger.OnMusicVolumeChange(slider);
    }
    public void OnSoundEffectVolumeChange(Slider slider)
    {
        var hnManger = GameObject.Find("HNGameManager").GetComponent<HNGameManager>();
        hnManger.OnSoundEffectVolumeChange(slider);
    }
}
