using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimEndScript : MonoBehaviour
{
    private HNGameManager hnManager = null;

    void Start()
    {
        if (hnManager == null)
        {
            hnManager = GameObject.FindObjectOfType<HNGameManager>();
        }
    }

    void EndAnimHandler()
    {
        switch (transform.parent.name.Last())
        {
            case 'E':
                hnManager.AnimEnd(0);
                break;
            case 'S':
                hnManager.AnimEnd(1);
                break;
            case 'W':
                hnManager.AnimEnd(2);
                break;
            case 'N':
                hnManager.AnimEnd(3);
                break;
        }
    }

    public void BuPaiAnimEnd()
    {
        EndAnimHandler();
       /* switch (transform.parent.name)
        {
            case "TilesAnimationGrandpa_BP_E":
                hnManager.AnimEnd(0);
                break;
            case "TilesAnimationGrandpa_BP_S":
                hnManager.AnimEnd(1);
                break;
            case "TilesAnimationGrandpa_BP_W":
                hnManager.AnimEnd(2);
                break;
            case "TilesAnimationGrandpa_BP_N":
                hnManager.AnimEnd(3);
                break;
        }*/
    }

    public void TableCardsEnd()
    {
        hnManager.StartKaiPai();
        Debug.Log("TableCardsEnd");
    }

    public void QiPaiAnimEnd()
    {
        //Debug.Log("Anim end " + transform.parent.name);
        /* switch (transform.parent.name)
         {
             case "TilesAnimationGrandpa_Pos_QP_E":
                 hnManager.AnimEnd(0);
                 break;
             case "TilesAnimationGrandpa_Pos_QP_S":
                 hnManager.AnimEnd(1);
                 break;
             case "TilesAnimationGrandpa_Pos_QP_W":
                 hnManager.AnimEnd(2);
                 break;
             case "TilesAnimationGrandpa_Pos_QP_N":
                 hnManager.AnimEnd(3);
                 break;
         }*/
        EndAnimHandler();
    }

    public void ZhuanPaiAnimEnd()
    {
        /* switch (transform.parent.name)
         {
             case "TilesAnimationGrandpa_4ZP_E":
                 hnManager.AnimEnd(0);
                 break;
             case "TilesAnimationGrandpa_4ZP_S":
                 hnManager.AnimEnd(1);
                 break;
             case "TilesAnimationGrandpa_4ZP_W":
                 hnManager.AnimEnd(2);
                 break;
             case "TilesAnimationGrandpa_4ZP_N":
                 hnManager.AnimEnd(3);
                 break;
         }*/
        EndAnimHandler();
    }

    public void NewPaiAnimEnd()
    {
        /*switch (transform.parent.name)
        {
            case "TilesAnimationGrandpa_NewP_E":
                hnManager.AnimEnd(0);
                break;
            case "TilesAnimationGrandpa_NewP_S":
                hnManager.AnimEnd(1);
                break;
            case "TilesAnimationGrandpa_NewP_W":
                hnManager.AnimEnd(2);
                break;
            case "TilesAnimationGrandpa_NewP_N":
                hnManager.AnimEnd(3);
                break;
        }*/
        EndAnimHandler();
    }
    public void ChaPaiAnimEnd()
    {
       /* switch (transform.parent.name)
        {
            case "TilesAnimationGrandpa_CP_E":
                hnManager.AnimEnd(0);
                break;
            case "TilesAnimationGrandpa_CP_S":
                hnManager.AnimEnd(1);
                break;
            case "TilesAnimationGrandpa_CP_W":
                hnManager.AnimEnd(2);
                break;
            case "TilesAnimationGrandpa_CP_N":
                hnManager.AnimEnd(3);
                break;
        }*/
        EndAnimHandler();
    }
}
