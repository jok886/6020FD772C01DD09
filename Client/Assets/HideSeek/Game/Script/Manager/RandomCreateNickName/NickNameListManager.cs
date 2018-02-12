using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;

public class NickNameListManager
{

    public NickNames nicknames;
    public static NickNameListManager _instance = null;
    public static NickNameListManager GetInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new NickNameListManager();
            }
            return _instance;
        }
    }
    public NickNameListManager()
    {
        nicknames = new NickNames();
    }
    //反序列化+读
    public void LoadAndDeserialize()
    {
        string data = HNGameManager.NickNameListText;
        nicknames = JsonReader.Deserialize<NickNames>(data);
    }
}
