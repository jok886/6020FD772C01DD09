using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NickNames
{
    public List<string> FNameList = new List<string>();
    public List<string> LNameList = new List<string>();
    public NickNames() { }
    public NickNames(List<string> fnamelist, List<string> lnamelist)
    {
        for (int i = 0; i < fnamelist.Count; i++)
        {
            FNameList.Add(fnamelist[i]);
        }
        for (int i = 0; i < lnamelist.Count; i++)
        {
            LNameList.Add(lnamelist[i]);
        }
    }
}
