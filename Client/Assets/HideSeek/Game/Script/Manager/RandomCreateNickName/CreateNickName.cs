using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNickName : MonoBehaviour
{

    private static CreateNickName _instance = null;
    public static CreateNickName GetInstance
    {
        get
        {
            if (_instance == null)
                _instance = new CreateNickName();
            return _instance;
        }
    }
    public CreateNickName() { }

    public string RandomFName()
    {
        string str = "";
        MersenneTwister.MT19937.Seed((ulong)DateTime.Now.Ticks);
        int index = (int)(MersenneTwister.MT19937.Int63() % NickNameListManager.GetInstance.nicknames.FNameList.Count);
        str = NickNameListManager.GetInstance.nicknames.FNameList[index];
        return str;
    }
    public string RandomLName()
    {
        string str = "";
        MersenneTwister.MT19937.Seed((ulong)DateTime.Now.Ticks);
        int index = (int)(MersenneTwister.MT19937.Int63() % NickNameListManager.GetInstance.nicknames.LNameList.Count);
        str = NickNameListManager.GetInstance.nicknames.LNameList[index];
        return str;
    }
    public string RandomName()
    {
        return RandomFName() + RandomLName();
    }
}
