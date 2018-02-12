using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShowWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject win;

    public void OnPointerEnter(PointerEventData eventData)
    {
        win.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        win.SetActive(false);
    }
}
