using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameNet;

public class Boom : MonoBehaviour
{
    private HNGameManager hnGameManager;

    void Start()
    {
        hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
    }

    //void Update()
    //{

    //}
    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.collider.tag == "Hide" || collision.collider.tag == "NormalFurniture")
        {
            Debug.Log("---------Boom!!!!!!!!!!!!");
            PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(PlayerTeam.PlayerTeamType.HideTeam);
            Human localHuam = GameObjectsManager.GetInstance().GetLocalHuman();
            if (team != null)
            {
                for (int i = 0; i < team.GetPlayerNum(); i++)
                {
                    PlayerBase playerBase = GameObjectsManager.GetInstance().GetPlayer(PlayerTeam.PlayerTeamType.HideTeam, i);
                    if (playerBase != null)
                    {
                        if (Math.Abs(this.gameObject.transform.position.x - playerBase.gameObject.transform.position.x) < 5 &&
                            Math.Abs(this.gameObject.transform.position.y - playerBase.gameObject.transform.position.y) < 5 &&
                            Math.Abs(this.gameObject.transform.position.z - playerBase.gameObject.transform.position.z) < 5)
                        {
                            if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                            {
                                playerBase.MakeDead();

                                //playerBase.PlayerChairIDOfPickedDead = playerBase.ChairID;
                                //Debug.Log(temp.name + "击杀了: " + playerBase.gameObject.name);
                                var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
                                if (kernel != null)
                                {
                                    String[] str = kernel.getPlayerByChairID(HNGameManager.m_iLocalChairID).GetNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                                    UIManager.GetInstance().ShowMiddleTips(str[0] + "击杀了: " + playerBase.gameObject.name);
                                    localHuam.AddHP();
                                }
                            }
                            else
                            {
                                // Dead event sync
                                PlayerEventItem deadEvent = new PlayerEventItem();
                                deadEvent.cbTeamType = (byte)playerBase.TeamType;
                                deadEvent.wChairId = (ushort)playerBase.ChairID;
                                deadEvent.cbAIId = playerBase.AIId;
                                deadEvent.cbEventKind = (byte)PlayerBase.PlayerEventKind.DeadByBoom;
                                //killer
                                deadEvent.nCustomData0 = (Int32)localHuam.TeamType;
                                deadEvent.nCustomData1 = (Int32)localHuam.ChairID;
                                deadEvent.nCustomData2 = (Int32)localHuam.AIId;
                                GameObjectsManager.GetInstance().PlayerEventList.Add(deadEvent);
                            }
                        }
                    }
                }
            }

            //炸弹特效
            GameObject loadObj = Resources.Load("Player/Prefabs/Invenrtory/FX Comic Explosion 1 Large BOOM") as GameObject;
            GameObject BoomFX = Instantiate(loadObj);
            BoomFX.transform.position = gameObject.transform.position;
            Destroy(this.gameObject);

            if (hnGameManager != null)
            {
                hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_EFFECT_Boom);
            }

            // Boom event sync
            PlayerEventItem boomEvent = new PlayerEventItem();
            boomEvent.cbTeamType = (byte)GameObjectsManager.s_LocalHumanTeamType;
            boomEvent.wChairId = (ushort)HNGameManager.m_iLocalChairID;
            boomEvent.cbAIId = HNMJ_Defines.INVALID_AI_ID;
            boomEvent.cbEventKind = (byte)PlayerBase.PlayerEventKind.Boom;
            boomEvent.nCustomData0 = (Int32)gameObject.transform.position.x;
            boomEvent.nCustomData1 = (Int32)gameObject.transform.position.y;
            boomEvent.nCustomData2 = (Int32)gameObject.transform.position.z;
            GameObjectsManager.GetInstance().PlayerEventList.Add(boomEvent);

        }
    }
}
