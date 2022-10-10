using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseIncomeUpgrade", menuName = "Upgrades/Increase Income")]
public class IncreaseIncomeUpgrade : Upgrade
{

    public override void Execute()
    {
        Global.Get<SpawnerSystem>().SpawnMoneyCheckpoint();

        base.Execute();
    }

    public override void SetMaxCondition()
    {
        MaxCondition = () => Global.Get<SpawnerSystem>().MaxCheckpointsSpawned;
    }
}