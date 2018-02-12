using System;
using System.Collections;
using System.Collections.Generic;
using GameNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreateOrJoinRoom : MonoBehaviour
{
    private static CreateOrJoinRoom _instance = null;
    public static CreateOrJoinRoom GetInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<CreateOrJoinRoom>();
            }
            return _instance;
        }
    }

    public Text RoomId;
    public Text RoomId_13;
    private int RoomIdValue;
    private HNGameManager hnGameManager;
    private GameObject Canvas;
    private Button ControlCase;
    //ZY add 钻石
    private Text DiamondNum;
    private Text GlodNum;
    private Text StoreDiamonNum;
    private Text StoreGlodNum;
    private Image ExpNum;
    private Text ExpNumTxt;
    private Text Name;
    private Text ID;
    private Image HeadImage;
    public List<Toggle> ControlType=new List<Toggle>();
    //public static int Diamondnum = 0;
    // Use this for initialization

    private float m_fDeltaTimeAfterLastClick = 0f;

    void Start()
    {
        //mChen add, try to fix返回大厅后，有小概率HideTeam会剩余一个没清除
        GameObjectsManager.GetInstance().ClearPlayers();

        m_fDeltaTimeAfterLastClick = 0f;

        Debug.Log("startcj");
        Canvas = GameObject.Find("Canvas");
        DiamondNum = Canvas.transform.Find("Diamond/DiamondCount").GetComponent<Text>();
        GlodNum = Canvas.transform.Find("Glod/GlodCount").GetComponent<Text>();
        StoreDiamonNum= Canvas.transform.Find("Window/StoreWindow/Diamond/DiamondCount").GetComponent<Text>();
        StoreGlodNum = Canvas.transform.Find("Window/StoreWindow/Glod/GlodCount").GetComponent<Text>();

        ExpNum = Canvas.transform.Find("Experience/ExperienceFill").GetComponent<Image>();
        ExpNumTxt = Canvas.transform.Find("Experience/Level").GetComponent<Text>();
        Name = Canvas.transform.Find("User/Name").GetComponent<Text>();
        Name.text = GlobalUserInfo.getNickName();
        ID = Canvas.transform.Find("User/ID").GetComponent<Text>();
        ID.text = GlobalUserInfo.getUserID().ToString();

        RoomIdValue = 0;
        hnGameManager = GameObject.FindObjectOfType<HNGameManager>();

        //GameHall场景隐藏GameLogoUI
        hnGameManager.gameplayUIETC.SetActive(false);
        hnGameManager.gamePlayUI_HT.SetActive(false);

        HeadImage = Canvas.transform.Find("User/UserMask/UserImage").GetComponent<Image>();
        if (hnGameManager != null)
        {
            //Sound
            Slider musicSlider = transform.Find("SetupWindow/Music/Slider").GetComponent<Slider>();
            if (musicSlider != null)
            {
                musicSlider.value = hnGameManager.BackgroundAudioSource.volume;
            }

            Slider soundSlider = transform.Find("SetupWindow/Sound/Slider").GetComponent<Slider>();
            if (soundSlider != null)
            {
                soundSlider.value = hnGameManager.GlobalEffectAudioSource.volume;
            }

            //Rule
            hnGameManager.m_baseScore = 1;
            hnGameManager.m_cbPlayCoutIdex = 0;
            hnGameManager.m_cbPlayCostTypeIdex = 1;

            //mChen add, for HideSeek
            hnGameManager.m_cbGameEndReason = HNMJ_Defines.GER_NOT_END;
        }

        //控制方案切换
        ControlCase= Canvas.transform.Find("Window/SetupWindow/ControlCase").GetComponent<Button>();
        ControlCase.gameObject.GetComponent<Image>().sprite = Resources.Load("UI/EasyTouchCase/" + HNGameManager.ControlCase, typeof(Sprite)) as Sprite;
        ControlCase.onClick.RemoveAllListeners();
        ControlCase.onClick.AddListener(() => 
        {
            HNGameManager.ControlCase++; 
            HNGameManager.ControlCase %= 4; 
            ControlCase.gameObject.GetComponent<Image>().sprite = Resources.Load("UI/EasyTouchCase/" + HNGameManager.ControlCase, typeof(Sprite)) as Sprite;
        });
        //ControlType.Add(Canvas.transform.Find("Window/SetupWindow/Joystick/ChangeWindow/BackImage/Joystick1").gameObject.GetComponent<Toggle>());
        //ControlType.Add(Canvas.transform.Find("Window/SetupWindow/Joystick/ChangeWindow/BackImage/Joystick2").gameObject.GetComponent<Toggle>());
        UpdateControlType();
        UserInfo.getInstance().reqAccountInfo();

    }

    private void Update()
    {
        if (hnGameManager != null)
        {
            hnGameManager.CheckInGameServerWhenToHallByDisconnect();//断线后自动尝试重连
        }

        m_fDeltaTimeAfterLastClick += 0.3f;
    }

    //控制音效
    public void ChangeBackgroundMusic(Single single)
    {
        hnGameManager.BackgroundAudioSource.volume = single;
        hnGameManager.m_musicVolume = single;
    }
    //控制音乐
    public void ChangeSoundEffect(Single single)
    {
        hnGameManager.GlobalEffectAudioSource.volume = single;
        hnGameManager.m_soundEffectVolume = single;
    }
    public void OnJoinRace_MJ()
    {
        if (m_fDeltaTimeAfterLastClick > 5f)
        {
            m_fDeltaTimeAfterLastClick = 0f;

            Loom.QueueOnMainThread(() =>
            {
                ShowGoldCost(true);
                hnGameManager.JoinRace_MJ();
            });

            //mChen add, for HideSeek
            ///hnGameManager.LoadHideSeekSceneOfWangHu();
        }
    }


    public void OnJoinRace_SSS()
    {

    }

    public void CreateSingleGame()
    {
        Loom.QueueOnMainThread(() =>
        {
            ShowGoldCost(false);
            hnGameManager.LoadSingGame();
        });
    }

    public void CreateRoom()
    {
        if (m_fDeltaTimeAfterLastClick > 5f)
        {
            m_fDeltaTimeAfterLastClick = 0f;

            Loom.QueueOnMainThread(() =>
            {
                ShowGoldCost(true);
                hnGameManager.CreateRoom();
            });//,2.0f);
               //hnGameManager.UpDatePlayerScore(); //头像旁边分数更新

            //mChen add, for HideSeek
            ///hnGameManager.LoadHideSeekSceneOfWangHu();
        }
    }

    public void ButtonNum(GameObject obj)
    {
        if (RoomIdValue >= 100000)
        {
            return;
        }
        int value = 0;
        switch (obj.name)
        {
            case "Button0":
                if (RoomIdValue == 0)
                {
                    return;
                }
                value = 0;
                break;
            case "Button1":
                value = 1;
                break;
            case "Button2":
                value = 2;
                break;
            case "Button3":
                value = 3;
                break;
            case "Button4":
                value = 4;
                break;
            case "Button5":
                value = 5;
                break;
            case "Button6":
                value = 6;
                break;
            case "Button7":
                value = 7;
                break;
            case "Button8":
                value = 8;
                break;
            case "Button9":
                value = 9;
                break;
            default:
                Debug.Assert(false, "Error Num In Join Room!!");
                return;
        }
        RoomIdValue *= 10;
        RoomIdValue += value;

        RoomId.text = RoomIdValue.ToString();

        //Sound effect
        //hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_EFFECT_JoinRoomNumKey);

        if (RoomIdValue >= 100000)
        {
            ButtonJoin();
        }
    }

    public void ButtonDel()
    {
        if (RoomIdValue == 0)
        {
            return;
        }
        RoomIdValue /= 10;
        RoomId.text = RoomIdValue.ToString();
    }

    public void ButtonJoin()
    {
        if (RoomIdValue > 1000000 || RoomIdValue < 100000)
        {
            return;
        }
        //int iServerID = RoomIdValue / 10000 - 10;

        Loom.QueueOnMainThread(() =>
        {
            ShowGoldCost(true);
            hnGameManager.JoinRoom(RoomIdValue);
        });//,2.0f);

        //RoomIdValue = 0;
        //RoomId.text = "";

        //mChen add, for HideSeek
        ///hnGameManager.LoadHideSeekSceneOfWangHu();
    }

    public void ButtonClearInput()
    {
        RoomIdValue = 0;
        RoomId.text = "";
    }
    //人数选择
    public void ChoosePlayerCount_4(Toggle toggle)
    {
        if (toggle.isOn)
        {
            hnGameManager.m_PlayerCount = 4;
        }
    }
    public void ChoosePlayerCount_3(Toggle toggle)
    {
        if (toggle.isOn)
        {
            hnGameManager.m_PlayerCount = 3;
        }
    }

    public void OnPlayCoutIdexChange0(Toggle toggle)
    {
        if(toggle.isOn)
        {
            hnGameManager.m_cbPlayCoutIdex = 0;
        }
    }

    public void OnPlayCoutIdexChange1(Toggle toggle)
    {
        if (toggle.isOn)
        {
            hnGameManager.m_cbPlayCoutIdex = 1;
        }
    }
    public void OnPlayCoutIdexChange2(Toggle toggle)
    {
        if (toggle.isOn)
        {
            hnGameManager.m_cbPlayCoutIdex = 2;
        }
    }

    public void OnPlayCostTypeIdexChange0(Toggle toggle)
    {
        if (toggle.isOn)
        {
            hnGameManager.m_cbPlayCostTypeIdex = 0;
        }
    }

    public void OnPlayCostTypeIdexChange1(Toggle toggle)
    {
        if (toggle.isOn)
        {
            hnGameManager.m_cbPlayCostTypeIdex = 1;
        }
    }
    //摇杆类型选择
    public void ControlTypeChange0(Toggle toggle)
    {
        if (toggle.isOn)
            HNGameManager.ControlCase = 0;
    }
    public void ControlTypeChange1(Toggle toggle)
    {
        if (toggle.isOn)
            HNGameManager.ControlCase = 1;
    }
    public void ControlTypeChange2(Toggle toggle)
    {
        if (toggle.isOn)
            HNGameManager.ControlCase = 2;
    }
    public void ControlTypeChange3(Toggle toggle)
    {
        if (toggle.isOn)
            HNGameManager.ControlCase = 3;
    }
    public void UpdateControlType()
    {
        if (HNGameManager.ControlCase == 0)
        {
            ControlType[0].isOn = true;
            ControlType[1].isOn = false;
            ControlType[2].isOn = false;
            ControlType[3].isOn = false;
        }
        else if (HNGameManager.ControlCase == 1)
        {
            ControlType[0].isOn = false;
            ControlType[1].isOn = true;
            ControlType[2].isOn = false;
            ControlType[3].isOn = false;
        }
        else if (HNGameManager.ControlCase == 2)
        {
            ControlType[0].isOn = false;
            ControlType[1].isOn = false;
            ControlType[2].isOn = true;
            ControlType[3].isOn = false;
        }
        else if (HNGameManager.ControlCase == 3)
        {
            ControlType[0].isOn = false;
            ControlType[1].isOn = false;
            ControlType[2].isOn = false;
            ControlType[3].isOn = true;
        }
    }
    public void UpdateInfo()
    {

        if (DiamondNum != null)
            DiamondNum.text = GlobalUserInfo.getUserInsure().ToString();
        if (GlodNum != null)
            GlodNum.text = GlobalUserInfo.getUserScore().ToString();
        if (StoreDiamonNum != null)
            StoreDiamonNum.text = GlobalUserInfo.getUserInsure().ToString();

        if (StoreGlodNum != null)
            StoreGlodNum.text = GlobalUserInfo.getUserScore().ToString();
        if (ExpNum != null)
            ExpNum.fillAmount = (GlobalUserInfo.getUserExp() % 100) / 100.0f;
        if (ExpNumTxt != null)
            ExpNumTxt.text = String.Format("等级: {0}", GlobalUserInfo.getUserExp()/100);
        if (Name != null)
            Name.text = GlobalUserInfo.getNickName();
        if (HeadImage != null && UserInfoWin.GetInstance != null)
            HeadImage.overrideSprite = UserInfoWin.GetInstance.GetHeadImage(int.Parse(GlobalUserInfo.GBToUtf8(GlobalUserInfo.getHeadHttp())));
    }
    public void HideUserWin()
    {
        GameObject UserWindow = Canvas.transform.Find("Window/UserWindow").gameObject;
        GameObject UserEditorWindow = Canvas.transform.Find("Window/UserEditorWindow").gameObject;
        GameObject TipsWindow = Canvas.transform.Find("Window/TipsWindow").gameObject;
        if (UserWindow != null)
            UserWindow.SetActive(false);
        if (UserEditorWindow != null)
            UserEditorWindow.SetActive(false);
        if (TipsWindow != null)
            TipsWindow.SetActive(false);
    }
    public void ShowGoldCost(bool bVisible)
    {
        Text GoldText = hnGameManager.gamePlayUI_HT.transform.FindChild("Btn/StealthButton/Gold/Text").GetComponent<Text>();
        if(bVisible)
        {
            if (GoldText != null)
            {
                GoldText.text = "X10";
                GoldText = null;
            }
            GoldText = hnGameManager.gamePlayUI_HT.transform.FindChild("Btn/ObjectSwitch/Gold/Text").GetComponent<Text>();
            if (GoldText != null)
            {
                GoldText.text = "X10";
                GoldText = null;
            }
            GoldText = hnGameManager.gamePlayUI_HT.transform.FindChild("Btn/ResurrectionButton/Gold/Text").GetComponent<Text>();
            if (GoldText != null)
            {
                GoldText.text = "X10";
                GoldText = null;
            }
        }
        else
        {
            if (GoldText != null)
            {
                GoldText.text = "";
                GoldText = null;
            }
            GoldText = hnGameManager.gamePlayUI_HT.transform.FindChild("Btn/ObjectSwitch/Gold/Text").GetComponent<Text>();
            if (GoldText != null)
            {
                GoldText.text = "";
                GoldText = null;
            }
            GoldText = hnGameManager.gamePlayUI_HT.transform.FindChild("Btn/ResurrectionButton/Gold/Text").GetComponent<Text>();
            if (GoldText != null)
            {
                GoldText.text = "";
                GoldText = null;
            }
        }

    }
}