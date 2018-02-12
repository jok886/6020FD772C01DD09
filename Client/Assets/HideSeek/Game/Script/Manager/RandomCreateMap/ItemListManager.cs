using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding.Serialization.JsonFx;
using System.IO;

public class ItemListManager
{
    //string path = Application.persistentDataPath + @"/ItemsList.json";
    string path = Application.streamingAssetsPath + @"/ItemsList.json";
    public Items items;
    public static ItemListManager _instance = null;
    public static ItemListManager GetInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ItemListManager();
            }
            return _instance;
        }
    }
    public ItemListManager()
    {
        items = new Items();
    }
    //序列化+写
    public void SerializeAndSave(Items items)
    {
        if (items.ItemList.Count == 0)
            return;
        if (File.Exists(path))
            File.Delete(path);
        string data = JsonWriter.Serialize(items);
        var streamWriter = new StreamWriter(path);
        streamWriter.Write(data);
        streamWriter.Close();
    }
    //反序列化+读
    public void LoadAndDeserialize()
    {
        //判断是否为空文件
        //if (new FileInfo(path).Length == 0)
        //{
        //    return;
        //}
        //var streamReader = new StreamReader(path);
        //string data = streamReader.ReadToEnd();
        //streamReader.Close();
        string data = HNGameManager.ItemsListText;
        items = JsonReader.Deserialize<Items>(data);
    }
    public string GetMapName(int mapIndex)
    {
        CreateObjectManager.MapType mapType = (CreateObjectManager.MapType)(mapIndex % (int)CreateObjectManager.MapType.MapNum);

        string name = "";
        switch (mapType)
        {
            case CreateObjectManager.MapType.Military:
                name = "Military";
                break;
            case CreateObjectManager.MapType.Office:
                name = "Office";
                break;
            case CreateObjectManager.MapType.Port:
                name = "Port";
                break;
            case CreateObjectManager.MapType.ClassRoom:
                name = "ClassRoom";
                break;
            case CreateObjectManager.MapType.Town:
                name = "Town";
                break;
            default:
                name = null;
                Debug.LogError("该地图在配置列表中不存在！");
                break;
        }
        return name;
    }
}