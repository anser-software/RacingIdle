using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyCheckpoint : MonoBehaviour, ISpawnable
{

    public float progress { get; private set; }

    [SerializeField]
    private UICheckpointPassedMoney onPassedFX;

    [SerializeField]
    private Vector3 fxLocalOffset;


    public void OnAfterSpawn()
    {
        progress = Global.Get<PathManager>().Path.GetClosestTimeOnPath(transform.position);

        Global.Get<MoneyManager>().RegisterCheckpoint(this);

        Global.Get<PathManager>().OnPathChanged += UpdatePlacement;
    }

    private void UpdatePlacement()
    {
        progress = Global.Get<PathManager>().Path.GetClosestTimeOnPath(transform.position);

        transform.position = Global.Get<PathManager>().GetPathPosition(progress);

        //transform.position += transform.TransformVector(moneyCheckpoint.LocalOffset);

        var worldForward = Global.Get<PathManager>().GetPathDirection(progress);

        transform.forward = worldForward;

        var worldTargetForward = transform.TransformDirection(Vector3.left);

        transform.rotation *= Quaternion.AngleAxis(Vector3.Angle(worldForward, worldTargetForward), Vector3.Cross(worldTargetForward, worldForward).normalized);
    }

    public void OnPassed(float moneyGained)
    {
        var fxRotation = transform.rotation * Quaternion.AngleAxis(90F, Vector3.up);

        if (Vector3.Dot(transform.right, Camera.main.transform.forward) < 0F)
            fxRotation *= Quaternion.AngleAxis(180F, Vector3.up);

        var onPassedInstance = Instantiate(onPassedFX, transform.position + transform.TransformVector(fxLocalOffset), fxRotation);

        onPassedInstance.Init(moneyGained);
    }

}
