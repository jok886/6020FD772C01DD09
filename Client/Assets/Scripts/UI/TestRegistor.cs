                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestRegistor : MonoBehaviour {

    //SingIn
    public int SeriesDate;
    private const int maxSignInDay = 7;
    private GameObject RegisterRaffleWindow;
    private GameObject[] SignInDayUI;
    private GameObject SignInButton;

    //Raffle
    public uint PlayCount;
    public uint RaffleCount = 0;
    private const uint PlayCountPerRaffle = 1;
    private GameObject RaffleButton;
    private GameObject RaffleImageObj;

    //Raffle rotate
    public float m_timer = 0.01f;
    public int m_tcount = 0;
    public int m_endcount = -99;
    public float m_deltav = -999.0f;
    public float m_initV = -999.0f;
    public bool m_brot = false;

    // Use this for initialization
    void Start () {

        //签到查询
        GameNet.UserInfo.getInstance().QuerySignIn();

        RegisterRaffleWindow = GameObject.Find("RegisterRaffleWindow");
        SignInDayUI = new GameObject[maxSignInDay];

        if (RegisterRaffleWindow == null)
        {
            return;
        }

        SignInButton = RegisterRaffleWindow.transform.Find("RegisterButton").gameObject;
        //RaffleButton = RegisterRaffleWindow.transform.Find("RaffleButton").gameObject;
        //RaffleImageObj = RegisterRaffleWindow.transform.Find("RaffleImage").gameObject;

        GameNet.GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
        GameNet.tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
        PlayCount = pGlobalUserData.dwPlayCount;
        SeriesDate = pGlobalUserData.wSeriesDate;
        RaffleCount = pGlobalUserData.dwRaffleCount;
        //System.Random rand = new System.Random();
        //SeriesDate = rand.Next(0,10);
        //SeriesDate = SeriesDate % 5;

        for (int i = 0; i < maxSignInDay; i++)
        {
            SignInDayUI[i] = RegisterRaffleWindow.transform.Find("RegisterImage" + i + "/RegisteredImage").gameObject;
            if(SignInDayUI[i] == null)
            {
                continue;
            }

            if(i < SeriesDate)
            {
                SignInDayUI[i].SetActive(true);
            }
            else
            {
                SignInDayUI[i].SetActive(false);
            }
        }

        // Raffle
        uint minPlayCountToRaffle = RaffleCount * PlayCountPerRaffle;
        if(minPlayCountToRaffle == 0)
        {
            minPlayCountToRaffle = PlayCountPerRaffle;
        }
        if (PlayCount < minPlayCountToRaffle)
        {
            if(RaffleButton!=null)
            {
                RaffleButton.GetComponent<Button>().interactable = false;
            }
        }
    }


 
    // Update is called once per frame
    void Update()
    {
        GameNet.GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
        GameNet.tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
        if(SeriesDate != pGlobalUserData.wSeriesDate)
        {
            SeriesDate = pGlobalUserData.wSeriesDate;

            ///SignInDayUI[SeriesDate].SetActive(true);
            for (int i = 0; i < maxSignInDay; i++)
            {
                if (SignInDayUI[i] == null)
                {
                    continue;
                }

                if (i < SeriesDate)
                {
                    SignInDayUI[i].SetActive(true);
                }
                else
                {
                    SignInDayUI[i].SetActive(false);
                }
            }
        }
        //if(PlayCount != pGlobalUserData.dwPlayCount)
        //{
        //    PlayCount = pGlobalUserData.dwPlayCount;
        //}
        //if (RaffleCount != pGlobalUserData.dwRaffleCount)
        //{
        //    RaffleCount = pGlobalUserData.dwRaffleCount;
        //}

        //uint minPlayCountToRaffle = RaffleCount * PlayCountPerRaffle;
        //if (minPlayCountToRaffle == 0)
        //{
        //    minPlayCountToRaffle = PlayCountPerRaffle;
        //}
        //if (PlayCount >= minPlayCountToRaffle)
        //{
        //    RaffleButton.GetComponent<Button>().interactable = true;
        //}
        //else
        //{
        //    RaffleButton.GetComponent<Button>().interactable = false;
        //}

        if (m_brot)
        {
            m_timer -= Time.deltaTime;
            if (m_timer <= 0.0f)
            {
                m_timer = 0.01f;
                //every 0.1s
                float temv = m_initV - m_tcount * m_deltav;
                RaffleImageObj.transform.Rotate(new Vector3(0, 0, temv));

                m_tcount += 1;
            }
            if (m_tcount >= m_endcount)
            {
                m_brot = false;
            }
        }
    }

    public void OnRegistorButtonClicked()
    {
        ///SignInDayUI[SeriesDate].SetActive(true);

        if(SignInButton != null)
        {
            SignInButton.GetComponent<Button>().interactable = false;
        }

        //签到
        GameNet.UserInfo.getInstance().DoneSignIn();
    }

    public void OnRaffleButtonClicked()
    {
        //RaffleCount++;
        ///RaffleButton.GetComponent<Button>().interactable = false;

        int idToRaffle = -1;
        System.Random rand = new System.Random();
        int tnum = rand.Next(0, 1000);
        if (tnum < 400)
        {
            int tnum2 = rand.Next(0, 100);
            if (tnum2 < 50)
            {
                idToRaffle = 1;
            }
            else
            {
                idToRaffle = 5;
            }
        }
        else if (tnum < 600)
        {
            idToRaffle = 0;

            GameNet.UserInfo.getInstance().DoneRaffle(1);
        }
        else if (tnum < 650)
        {
            idToRaffle = 2;

            GameNet.UserInfo.getInstance().DoneRaffle(8);
        }
        else if(tnum < 750)
        {
            idToRaffle = 3;

            GameNet.UserInfo.getInstance().DoneRaffle(4);
        }
        else if (tnum < 775)
        {
            idToRaffle = 4;

            GameNet.UserInfo.getInstance().DoneRaffle(10);
        }
        else if (tnum < 925)
        {
            idToRaffle = 6;

            GameNet.UserInfo.getInstance().DoneRaffle(2);
        }
        else if (tnum < 1000)
        {
            idToRaffle = 7;

            GameNet.UserInfo.getInstance().DoneRaffle(6);
        }

        float degreeToRot = 45.0f / 2 + idToRaffle * 45.0f;
        int n = 5;
        //float dt = 0.01f, t = 5.0f;
        int k = 500; //t/dt
        //all rot time k = t / dt次(50次)
        //(0 + v) * k / 2 = n * 360.0f + degreeToRot
        //v = (n * 360.0f + degreeToRot) * 2.0 / k
        float v = (n * 360.0f + degreeToRot) * 2.0f / k;
        float dv = v / k;
        //INIT
        m_deltav = dv;
        m_initV = v;
        m_brot = true;
        m_endcount = k;

        if (RaffleImageObj == null)
        {
            Debug.Log("Error");
        }

    }

    public void OnRaffleCheck()
    {
        Debug.Log("OnRaffleCheck");

        //if (nowround >= CanRaffleRound)
        {
            RaffleButton.GetComponent<Button>().interactable = true;
            Debug.Log("BeChecking");
        }
    }
}
