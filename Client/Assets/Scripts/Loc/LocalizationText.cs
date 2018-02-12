using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class LocalizationText : MonoBehaviour
{
    public string key = " ";

    void Start()
    {
        GetComponent<Text>().text = LocalizationManager.GetInstance.GetValue(key);
    }
}