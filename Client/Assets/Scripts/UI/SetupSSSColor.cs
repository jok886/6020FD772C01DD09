using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupSSSColor : MonoBehaviour {
    public Image tableObjs;
    public Image BiPai;
    public HNGameManager HnManager;
    //Table Colors
    //Color(10, 85, 80, 255);
    private Color tableCol1 = new Color(0.0392f, 0.3333f, 0.3137f, 1f);
    //Color(158, 122, 122, 255);
    private Color tableCol2 = new Color(0.6196f, 0.4784f, 0.4784f, 1f);
    //Color(20, 90, 120, 255);
    private Color tableCol3 = new Color(0.0784f, 0.3529f, 0.4706f, 1f);
    //Color(65, 50, 110, 255);
    private Color tableCol4 = new Color(0.2549f, 0.1961f, 0.4314f, 1f);

    //Table Toggles
    public Toggle tableToggle1 = null;
    public Toggle tableToggle2 = null;
    public Toggle tableToggle3 = null;
    public Toggle tableToggle4 = null;
    
    // Use this for initialization
    void Start()
    {
        //初始化桌子颜色
            tableObjs.material.SetColor("_Color", tableCol1);
            BiPai.material.SetColor("_Color", tableCol1);
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
        
        tableObjs.material.SetColor("_Color", color2Set);
        BiPai.material.SetColor("_Color", color2Set);

    }
    #endregion

}