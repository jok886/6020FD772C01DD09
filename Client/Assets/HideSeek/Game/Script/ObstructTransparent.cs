using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstructTransparent : MonoBehaviour
{
    //观察目标
    private Transform Target;
    //上次碰撞到的物体
    private List<GameObject> lastColliderObject = new List<GameObject>();
    //本次碰撞到的物体
    private List<GameObject> colliderObject = new List<GameObject>();

    float time = 0;
    void Update()
    {
        if (time >= 1)
        {
            time = 0;
            Deal();
        }
        time += Time.deltaTime;
    }
    public void Deal()
    {
        Target = null;
        if (Camera.main.transform.parent != null)
        {
            if (Camera.main.transform.parent.transform.parent != null)
            {
                Target = Camera.main.transform.parent.transform.parent;
            }
        }
        if (Target == null)
            return;
        //这里是计算射线的方向，从主角发射方向是射线机方向
        Vector3 aim = Target.position;
        //得到方向
        Vector3 ve = (Target.position - Camera.main.transform.position).normalized;
        float an = transform.eulerAngles.y;
        aim -= an * ve;

        //在场景视图中可以看到这条射线
        Debug.DrawLine(Target.position, aim, Color.red);

        RaycastHit[] hit;
        hit = Physics.RaycastAll(Target.position, aim, (Target.position - Camera.main.transform.position).magnitude);//起始位置、方向、距离

        //将 colliderObject 中所有的值添加进 lastColliderObject
        for (int i = 0; i < colliderObject.Count; i++)
            lastColliderObject.Add(colliderObject[i]);
        colliderObject.Clear();//清空本次碰撞到的所有物体
        for (int i = 0; i < hit.Length; i++)//获取碰撞到的所有物体
        {
            if (hit[i].collider.gameObject.tag == "MainCamera")
                break;
            //if (hit[i].collider.gameObject.tag != "Tagger"//警察
            //    && hit[i].collider.gameObject.tag != "Hide"//躲藏者
            //    && hit[i].collider.gameObject.tag != "LocalHuman") //本地玩家
            //if(hit[i].collider.gameObject.tag != "LocalHuman")
            {
                //Debug.Log(hit[i].collider.gameObject.name);
                if (hit[i].collider.gameObject.name == "Model")
                {
                    GameObject obj = hit[i].collider.gameObject.transform.parent.gameObject;
                    if (obj != null)
                    {
                        PlayerBase player = obj.GetComponent<PlayerBase>();
                        if (player != null)
                            if (player.IsLocalHuman())
                                continue;
                    }
                }
                colliderObject.Add(hit[i].collider.gameObject);
                Renderer render = hit[i].collider.gameObject.GetComponent<Renderer>();
                if (render != null)
                    SetMaterialsColor(render, 0.2f);//置当前物体材质透明度
            }
        }

        //上次与本次对比，本次还存在的物体则赋值为null
        for (int i = 0; i < lastColliderObject.Count; i++)
        {
            for (int ii = 0; ii < colliderObject.Count; ii++)
            {
                if (colliderObject[ii] != null)
                {
                    if (lastColliderObject[i] == colliderObject[ii])
                    {
                        lastColliderObject[i] = null;
                        break;
                    }
                }
            }
        }

        //当值为null时则可判断当前物体还处于遮挡状态
        //值不为null时则可恢复默认状态(不透明)
        for (int i = 0; i < lastColliderObject.Count; i++)
        {
            if (lastColliderObject[i] != null)
            {
                Renderer render = lastColliderObject[i].GetComponent<Renderer>();
                if(render != null)
                {
                    SetMaterialsColor(render, 1f);//恢复上次物体材质透明度
                    lastColliderObject.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    public void ClearAllMaterialsColor()
    {
        for (int i = 0; i < colliderObject.Count; i++)
        {
            Renderer render = colliderObject[i].GetComponent<Renderer>();
            if (render != null)
            {
                SetMaterialsColor(render, 1f);//恢复上次物体材质透明度
                colliderObject.RemoveAt(i);
                i--;
            }
        }
    }
    /// 置物体所有材质球颜色 <summary>
    /// 置物体所有材质球颜色
    /// </summary>
    /// <param name="_renderer">材质</param>
    /// <param name="Transpa">透明度</param>
    private void SetMaterialsColor(Renderer _renderer, float Transpa)
    {
        //获取当前物体材质球数量
        int materialsNumber = _renderer.materials.Length;
        for (int i = 0; i < materialsNumber; i++)
        {
            if (Transpa == 1)
                _renderer.materials[i].shader = Shader.Find("Legacy Shaders/Diffuse");
            else
                _renderer.materials[i].shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");

            //获取当前材质球颜色
            Color color = _renderer.materials[i].color;

            //设置透明度  取值范围：0~1;  0 = 完全透明
            color.a = Transpa;

            //置当前材质球颜色
            _renderer.materials[i].SetColor("_Color", color);
            Resources.UnloadUnusedAssets();  //卸载未使用资源
        }
    }
}
