using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Share : MonoBehaviour {

    public void ShareGame(int platform)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (platform == 0)
        {
            StartCoroutine(DirectShareIcon(Platform.WEIXIN));
        }
        else
        {
            StartCoroutine(DirectShareIcon(Platform.WEIXIN_CIRCLE));
        }
#endif
    }

    IEnumerator DirectShareIcon(Platform p)
    {
        yield return new WaitForEndOfFrame();
        HNGameManager.Share(p);
    }

    IEnumerator CaptureScreenAndShare()
    {
        yield return new WaitForEndOfFrame();
        //HNGameManager.ShareMatchResult();
    }

    public void ShareMatchResult()
    {
        StartCoroutine(CaptureScreenAndShare());
    }
}
