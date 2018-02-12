using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class RollText : MonoBehaviour
{
    public Transform start;
    public Transform end;
    public float speed;

    private Text publicNotice;

    private void Start()
    {
        publicNotice = GetComponent<Text>();
        if(publicNotice != null)
        {
            GameNet.GlobalUserInfo pGlobalUserInfo = GameNet.GlobalUserInfo.GetInstance();
            GameNet.tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();

            string szPublicNotice = GameNet.GlobalUserInfo.GBToUtf8(pGlobalUserData.szPublicNotice);
            publicNotice.text = szPublicNotice;
        }
    }

    void Update()
    {
        if (speed != 0)
        {
            float x = transform.position.x + speed * Time.deltaTime;
            if (x < start.position.x)
            {
                x = end.position.x;
                transform.position = new Vector3(end.position.x, 0, 1);
            }
            transform.position = new Vector3(x, start.position.y, 1);
        }
    }
}

