using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Items
{
    public List<Tagger> TaggerList = new List<Tagger>();
    public List<Item> ItemList = new List<Item>();
    public Items() { }
    public Items(List<Tagger> taggerlist, List<Item> itemlist)
    {
        for (int i = 0; i < taggerlist.Count; i++)
        {
            TaggerList.Add(taggerlist[i]);
        }
        for (int i = 0; i < itemlist.Count; i++)
        {
            ItemList.Add(itemlist[i]);
        }
    }
}
[System.Serializable]
public class Item
{
    public string Map;
    //public List<Scheme> SchemeItemList = new List<Scheme>();
    public Scheme SchemeItem;
    public Item() { }
    public Item(string map, Scheme schemeitem)
    {
        this.Map = map;
        this.SchemeItem = schemeitem;
        //for(int i = 0; i < schemeitemlist.Count; i++)
        //{
        //    SchemeItemList.Add(schemeitemlist[i]);
        //}
    }
}
[System.Serializable]
public class Scheme
{
    public List<Model> ModelNameList = new List<Model>();
    public Scheme() { }
    public Scheme(List<Model> modelnamelist)
    {
        for (int i = 0; i < modelnamelist.Count; i++)
        {
            ModelNameList.Add(modelnamelist[i]);
        }
    }
}
[System.Serializable]
public class Model
{
    public string FileName;
    public string Name;
    public string Path;
    public Model() { }
    public Model(string filename, string name, string path)
    {
        this.FileName = filename;
        this.Name = name;
        this.Path = path;
    }
}
[System.Serializable]
public class Tagger
{
    public int Index;
    public string FileName;
    public string Name;
    public string Path;
    public Tagger() { }
    public Tagger(int index, string filename, string name, string path)
    {
        this.Index = index;
        this.FileName = filename;
        this.Name = name;
        this.Path = path;
    }
}
//[System.Serializable]
//public class TaggerModel
//{
//    public int Index;
//    public string Name;
//    public string Path;
//    public TaggerModel() { }
//    public TaggerModel(int index,string name, string path)
//    {
//        this.Index = index;
//        this.Name = name;
//        this.Path = path;
//    }
//}
