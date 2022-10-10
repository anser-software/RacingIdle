using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AddRoadUpgrade", menuName = "Upgrades/Add Road")]
public class AddRoadUpgrade : Upgrade
{

    public override void Execute()
    {
        Global.Get<PathManager>().LoadRoadAsSegment(6);

        base.Execute();
    }

}
