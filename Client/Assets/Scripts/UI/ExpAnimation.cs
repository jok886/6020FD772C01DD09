using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//脚本附着在播放表情的exp对象上（UI Image）
public class ExpAnimation : MonoBehaviour {
    public Texture[] exp;
    public float expPlayTime = 15f;
    public float expPlaySpeed = 0.1f;
    private float currenTime = 0f;
    //test tex number
    public int texnum = 0;
    private bool palyBool = true;

    IEnumerator ExpAnim(int texNum, Action endCallbackAction)
    {
        //初始化
        this.GetComponent<Image>().material.SetTexture("_MainTex", exp[texNum]);

        while (currenTime <= expPlayTime)
        {
            this.GetComponent<Image>().material.SetFloat("_PlayTime", currenTime);
            //currenTime += expPlaySpeed;
            currenTime += Time.deltaTime * 10;
            yield return null;
        }

        endCallbackAction();
        GetComponent<Image>().sprite = null;
        GetComponent<Image>().material.SetTexture("_MainTex", null);
        currenTime = 0f;
    }

    //texNum 表情号   
    //播放时间预先在表情对应的material属性上设置完成
    public void ExpAnimationPlay(int texNum, Action endCallbackAction)
    {
        StartCoroutine(ExpAnim(texNum, endCallbackAction));
    }

}
