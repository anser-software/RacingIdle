using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
[CreateAssetMenu(fileName = "MergeCarsUpgrade", menuName = "Upgrades/Merge Cars")]
public class MergeCarsUpgrade : Upgrade
{

    public override void Execute()
    {
        Global.Get<CarManager>().MergeCars();

        base.Execute();
    }

    public override void SetUnlockCondition()
    {
        UnlockCondition = () => Global.Get<CarManager>().CanMergeCars();
    }
}
