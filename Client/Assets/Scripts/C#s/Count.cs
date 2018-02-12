using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Count : MonoBehaviour {
    public InputField num;
    int n = 1;
    void OnEnable()
    {
        num.text = n+"";
    }

    public void add() {
        n += 1;
        num.text = n + "";
    }

    public void Reduce()
    {
        n -= 1;
        if (n < 1)
            n = 1;
        num.text = n + "";
    }
}
