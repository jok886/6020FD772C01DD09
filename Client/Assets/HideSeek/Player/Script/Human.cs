using UnityEngine;
using System.Collections;
using System;

public class Human : PlayerBase
{
    private void Awake()
    {
        base.Awake();

        IsAI = false;
    }

    // Use this for initialization
    private void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    private void Update()
    {
        base.Update();
    }

    public override void CustomUpdate()
    {
        base.CustomUpdate();
    }
}