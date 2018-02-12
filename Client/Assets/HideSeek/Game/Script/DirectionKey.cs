using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionKey : MonoBehaviour
{
    //public GameObject sss;
    private Vector3 V1 = Vector3.zero;
    private Vector3 V2 = Vector3.zero;
    private GameObject arrow;
    private GameObject playerObject;
    private GameObject targetObject;
    private float deg = 0;
    private bool isDeal = false;
    float height = 0;
    private static DirectionKey _instance = null;
    public static DirectionKey GetInstance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<DirectionKey>();
            return _instance;
        }
    }
    void Update()
    {
        if (isDeal)
        {
            GetAngle();
            DirectionHint();
        }
    }
    public void Deal(GameObject player, GameObject target)
    {
        GameObject temp = Resources.Load("Player/Prefabs/Invenrtory/Inventory_Arrow") as GameObject;
        if (temp != null)
        {
            arrow = Instantiate(temp);
            arrow.transform.SetParent(player.transform, false);
            if (ControlManager.isPerson_1st)
            {
                height = player.GetComponent<CharacterController>().height / 3f;
                arrow.transform.localPosition = new Vector3(0, height, 3);
            }
            else
            {
                height = player.GetComponent<CharacterController>().height / 2f * 3f;
                arrow.transform.localPosition = new Vector3(0, height, 0);
            }
            arrow.transform.eulerAngles = Vector3.zero;
        }
        //arrow.transform.localScale = new Vector3(5, 5, 5);
        playerObject = player;
        targetObject = target;
        isDeal = true;
        StartCoroutine(DeadTime());
    }
    public void GetAngle()
    {
        if (targetObject == null || playerObject == null)
            return;
        // 为了方便理解便于计算，将向量在 Y 轴上的偏移量设置为 0
        Vector3 s = playerObject.transform.forward;
        V1 = new Vector3(s.x, 0, s.z);
        Vector3 temp = targetObject.transform.position - playerObject.transform.position;
        V2 = new Vector3(temp.x, 0, temp.z);

        // 分别取 V1，V2 方向上的 单位向量（只是为了方便下面计算）
        V1 = V1.normalized;
        V2 = V2.normalized;

        // 计算向量 V1，V2 点乘结果
        // 即获取 V1,V2夹角余弦    cos(夹角)
        float direction = Vector3.Dot(V1, V2);
        //Debug.LogError("direction : " + direction);

        // 夹角方向一般取（0 - 180 度）
        // 如果取(0 - 360 度)
        // direction >= 0 则夹角在 （0 - 90] 和 [270 - 360] 度之间
        // direction < 0 则夹角在 （90 - 270) 度之间
        // direction 无法确定具体角度

        // 反余弦求V1，V2 夹角的弧度
        float rad = Mathf.Acos(direction);
        // 再将弧度转换为角度
        deg = rad * Mathf.Rad2Deg;
        // 得到的 deg 为 V1，V2 在（0 - 180 度的夹角）还无法确定V1，V2 的相对夹角
        // deg 还是无法确定具体角度

        // 计算向量 V1， V2 的叉乘结果 
        // 得到垂直于 V1， V2 的向量， Vector3(0, sin(V1,V2夹角), 0)
        // 即 u.y = sin(V1,V2夹角)
        Vector3 u = Vector3.Cross(V1, V2);
        //Debug.LogError("u.y  : " + u.y);

        // u.y >= 0 则夹角在 ( 0 - 180] 度之间
        // u.y < 0 则夹角在 (180 - 360) 度之间
        // u.y 依然无法确定具体角度

        // 结合 direction >0 、 u.y > 0 和 deg 的值
        // 即可确定 V2 相对于 V1 的夹角
        if (u.y >= 0) // (0 - 180]
        {
            if (direction >= 0)
            {
                // (0 - 90] 度
            }
            else
            {
                // (90 - 180] 度
            }
        }
        else    // (180 - 360]
        {
            if (direction >= 0)
            {
                // [270 - 360]
                // 360 + (-1)deg
                deg = 360 - deg;
            }
            else
            {
                // (180 - 270)
                deg = 360 - deg;
            }
        }
        //Debug.LogError(deg);
    }
    public void DirectionHint()
    {
        if(arrow!=null)
        {
            arrow.transform.localEulerAngles = new Vector3(0, deg, 0);
            arrow.transform.localPosition = new Vector3(arrow.transform.localPosition.x, height, arrow.transform.localPosition.z);
        }

    }
    IEnumerator DeadTime()
    {
        yield return new WaitForSeconds(5);
        isDeal = false;
        //Destroy(arrow); 
    }
}
