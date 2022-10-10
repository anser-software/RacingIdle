using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AddCarUpgrade", menuName = "Upgrades/Add Car")]
public class AddCarUpgrade : Upgrade
{

    public override void Execute()
    {
        Global.Get<SpawnerSystem>().SpawnCar(0);

        base.Execute();
    }

    public override void SetMaxCondition()
    {
        MaxCondition = () => Global.Get<CarManager>().MaxCarsSpawned; 
    }
}
