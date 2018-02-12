using UnityEngine;
using System.Collections;

public class AI : PlayerBase
{
    private void Awake()
    {
        base.Awake();

        IsAI = true;
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