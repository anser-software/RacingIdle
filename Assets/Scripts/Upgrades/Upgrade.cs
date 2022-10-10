using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using org.matheval;
public abstract class Upgrade : ScriptableObject
{

    public float defaultPrice;

    [Header("p = Default price")]
    [Header("n = Number of clicks")]
    [SerializeField]
    protected string priceAsFuncOfClicks = "0.1 * p * n^1.5 + p";

    [NonSerialized]
    public int executionsCount = 0;
     
    public virtual Func<bool> UnlockCondition { get; protected set; }

    public virtual Func<bool> MaxCondition { get; protected set; }

    public virtual void SetUnlockCondition()
    {
        UnlockCondition = () => true;
    }

    public virtual void SetMaxCondition()
    {
        MaxCondition = () => false;
    }

    public virtual void Execute()
    {
        Global.Get<MoneyManager>().TakeMoney(GetCurrentPrice());

        executionsCount++;
    }

    public virtual float GetCurrentPrice() =>
        (float)new Expression(priceAsFuncOfClicks).SetScale(0).Bind("p", defaultPrice).Bind("n", executionsCount).Eval<decimal>();
    

}
