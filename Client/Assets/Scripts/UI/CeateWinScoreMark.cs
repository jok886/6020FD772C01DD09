using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CeateWinScoreMark : MonoBehaviour {

    private int[] baseScores =
    {
        1, 2, 5, 10, 20, 30, 50, 100
    };
    private int scoreIdx = 0;

    public Text scoreText;
    private int scoreMark;
    private HNGameManager hnGameManager;


    // Use this for initialization
    void Start()
    {
        //scoreText默认值
        scoreIdx = 0;
        scoreMark = baseScores[scoreIdx];
        scoreText.text = scoreMark.ToString();

        hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
    }

    // Update is called once per frame
    //void Update () {

    //}
    public void AddscoreText()
    {
        scoreIdx = (scoreIdx + 1) % baseScores.Length;
        scoreMark = baseScores[scoreIdx];
        //scoreMark += 1;
        //if (scoreMark > 10) scoreMark = 10;

        scoreText.text = scoreMark.ToString();

        if(hnGameManager != null)
        {
            hnGameManager.m_baseScore = (long)scoreMark;
        }
    }

    public void CutscoreText()
    {
        scoreIdx = (scoreIdx - 1) % baseScores.Length;
        scoreMark = baseScores[scoreIdx];
        //scoreMark -= 1;
        //if (scoreMark < 1) scoreMark = 1;

        scoreText.text = scoreMark.ToString();

        if (hnGameManager != null)
        {
            hnGameManager.m_baseScore = (long)scoreMark;
        }
    }
}
