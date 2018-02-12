using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateObject
{
    public static CreateObject _instance = null;
    public static CreateObject GetInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CreateObject();
            }
            return _instance;
        }
    }
    private CreateObject()
    {

    }
    public void InstanceObject(string name, string path,Vector3 position, Vector3 eulerangles/*, CreateObjectManager.GameObjectType type*/)
    {
        GameObject loadObj = null;
        loadObj = Resources.Load(path) as GameObject;
        if (loadObj != null)
        {
            GameObject temp = UnityEngine.Object.Instantiate(loadObj);
            //temp.tag = "NormalFurniture";
            temp.transform.SetParent(GameObject.Find("HelpObjs").transform.Find("SceneObjsPoints/GameObjectPrefabs").transform, false);
            temp.transform.position = position;
            temp.transform.localEulerAngles = eulerangles;
            GameObject Model = temp.transform.FindChild("Model").gameObject;
            Model.tag = "NormalFurniture";
        }
        else
        {
            Debug.Log("loadObj为Null");
        }
    }
}
