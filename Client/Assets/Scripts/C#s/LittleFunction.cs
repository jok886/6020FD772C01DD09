using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleFunction : MonoBehaviour {
    public void ToggleActive()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void openHelp() {
        Application.OpenURL("http://joy.qunl.com/faq/guide.html");
    }
}
