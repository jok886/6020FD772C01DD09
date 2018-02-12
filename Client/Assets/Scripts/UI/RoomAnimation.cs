using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomAnimation : MonoBehaviour {
    [Range(0.01f,0.1f)]public float scaleChangeSpeed = 0.01f;
    private float scaleChange = 0f;

    public GameObject MJRoom;
    public GameObject SSSRoom;

    private bool MJRoomBool = false;
    private bool SSSRoomBool = false;
    //// Use this for initialization
    //void Start () {

    //}
    void Update () {
        if (MJRoomBool)
        {
            MJRoom.transform.localScale = new Vector3(1f, scaleChange, 1f);
            if (MJRoom.activeSelf)
            {
                scaleChange += scaleChangeSpeed;
                MJRoom.transform.localScale = new Vector3(1f, scaleChange, 1f);
                if (scaleChange >= 1f)
                {
                    MJRoom.transform.localScale = new Vector3(1f, 1f, 1f);
                    MJRoomBool = false;
                }
            }
        }

        if (SSSRoomBool)
        {
            SSSRoom.transform.localScale = new Vector3(1f, scaleChange, 1f);
            if (SSSRoom.activeSelf)
            {
                scaleChange += scaleChangeSpeed;
                SSSRoom.transform.localScale = new Vector3(1f, scaleChange, 1f);
                if (scaleChange >= 1f)
                {
                    SSSRoom.transform.localScale = new Vector3(1f, 1f, 1f);
                    SSSRoomBool = false;
                }
            }
        }
    }

    public void MJRoomOpen()
    {
        MJRoomBool = true;
    }
    public void MJRoomClose()
    {
        scaleChange = 0f;
        MJRoom.transform.localScale = new Vector3(1f, 0f, 1f);
    }

    public void SSSRoomOpen()
    {
        SSSRoomBool = true;
    }
    public void SSSRoomClose()
    {
        scaleChange = 0f;
        SSSRoom.transform.localScale = new Vector3(1f, 0f, 1f);
    }
}
