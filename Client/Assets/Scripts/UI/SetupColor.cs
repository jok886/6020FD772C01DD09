using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupColor : MonoBehaviour {
    public GameObject[] tableObjs;
    public HNGameManager HnManager;
    //Table Colors
    //Color(10, 85, 80, 255);
    private Color tableCol1 = new Color(0.0392f, 0.3333f, 0.3137f, 1f);
    //Color(125, 100, 40, 255);
    private Color tableCol2 = new Color(0.4902f, 0.3922f, 0.1569f, 1f);
    //Color(20, 90, 120, 255);
    private Color tableCol3 = new Color(0.0784f, 0.3529f, 0.4706f, 1f);
    //Color(65, 50, 110, 255);
    private Color tableCol4 = new Color(0.2549f, 0.1961f, 0.4314f, 1f);

    //Table Toggles
    public Toggle tableToggle1 = null;
    public Toggle tableToggle2 = null;
    public Toggle tableToggle3 = null;
    public Toggle tableToggle4 = null;

    //Tiles Colors
    //Color(245, 135, 15, 255);
    private Color tilesCol1 = new Color(0.9608f, 0.5294f, 0.0588f, 1f);
    //Color(25, 135, 15, 255);
    private Color tilesCol2 = new Color(0.0980f, 0.5294f, 0.0588f, 1f);
    //Color(45, 180, 235, 255);
    private Color tilesCol3 = new Color(0.1765f, 0.7059f, 0.9216f, 1f);
    //Color(200, 60, 255, 255);
    private Color tilesCol4 = new Color(0.7843f, 0.2353f, 1f, 1f);

    //Tiles Toggles
    public Toggle tilesToggle1 = null;
    public Toggle tilesToggle2 = null;
    public Toggle tilesToggle3 = null;
    public Toggle tilesToggle4 = null;

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            if (!tableObjs[i])
            {
                Debug.LogWarning("请在设置窗口C#中的桌面部件组内添加对象。");
            }
        }
        //初始化桌子颜色
            tableObjs[0].GetComponent<Renderer>().material.SetColor("_Color", tableCol1);
            tableObjs[1].GetComponent<Renderer>().material.SetColor("_Color", tableCol1);
            tableObjs[2].GetComponent<Renderer>().material.SetColor("_Color", tableCol1);
            tableObjs[3].GetComponent<Renderer>().material.SetColor("_Color", tableCol1);
            tableObjs[4].GetComponent<Renderer>().material.SetColor("_Color", tableCol1);

        //sound
        if (HnManager != null)
        {
            Slider musicSlider = transform.Find("Music/Slider").GetComponent<Slider>();
            if (musicSlider != null)
            {
                musicSlider.value = HnManager.m_musicVolume;
            }

            Slider soundSlider = transform.Find("Sound/Slider").GetComponent<Slider>();
            if (soundSlider != null)
            {
                soundSlider.value = HnManager.m_soundEffectVolume;
            }
        }
    }


    #region "Set Table Color"
    //麻将颜色设置
    public void SetCardsColor(int colorIndex)
    {
        Color color2Set = tilesCol1;
        switch (colorIndex)
        {
            case 1:
                color2Set = tilesCol1;
                break;
            case 2:
                color2Set = tilesCol2;
                break;
            case 3:
                color2Set = tilesCol3;
                break;
            case 4:
                color2Set = tilesCol4;
                break;
        }
        HnManager.SetAllCardsColor(color2Set);
    }
    #endregion

    #region "Set Table Color"

    //桌子颜色设置
    public void SetTableColor(int colorIndex)
    {
        Color color2Set = tableCol1;
        switch (colorIndex)
        {
            case 1:
                color2Set = tableCol1;
                break;
            case 2:
                color2Set = tableCol2;
                break;
            case 3:
                color2Set = tableCol3;
                break;
            case 4:
                color2Set = tableCol4;
                break;
        }

        foreach (var tableObj in tableObjs)
        {
            tableObj.GetComponent<Renderer>().material.SetColor("_Color", color2Set);
        }

    }
    #endregion

}