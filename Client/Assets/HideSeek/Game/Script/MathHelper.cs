using UnityEngine;
using System.Collections;

public class MathHelper
{
    //扇形区域判断
    public static bool CheckIsTargetInSector(Vector3 sectorPos, Vector3 sectorDir, /*GameObject center,*/ GameObject target, float checkRadius, float checkAngle)
    {
        Vector3 centerPos = sectorPos/*center.transform.position*/;
        Vector3 forward = sectorDir/*center.transform.forward*/;
        Vector3 targetPos = target.transform.position;

        centerPos.y = 0;
        targetPos.y = 0;

        Vector3 deltaVector3 = targetPos - centerPos;

        if (checkRadius < 0f || deltaVector3.magnitude <= checkRadius)//在距离范围内
        {
            var aimAngle = Vector3.Angle(deltaVector3, forward);
            if (aimAngle < (checkAngle / 2))
            {
                return true;
            }
        }
        return false;
    }
}
