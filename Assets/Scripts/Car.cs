using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Baracuda.Monitoring;
using Baracuda.Monitoring.API;
using DG.Tweening;
public class Car : MonoBehaviour
{

    public int typeID;

    public float Progress => progressOnPath;

    public CarState CarState { get; private set; } = CarState.Initialization;

    public float incomePerSecond { get; private set; }

    [SerializeField]
    private float defaultSpeed, acceleration, inverseDrag;

    [Space, SerializeField]
    private float expectTurnDistance, turnSpeed;

    [Space, SerializeField, Range(0F, 2F)]
    private float cornerSlowingRate;

    [SerializeField]
    private TrailRenderer speedUpTrail;

    [Space, SerializeField]
    private float moneyOnCheckpointPass;

    private float progressOnPath, currentSpeed, targetSpeed, extraSpeed;

    private PathManager PathManager => Global.Get<PathManager>();

    public void InitializeSpawnedCar(Vector3 initialPos)
    {
        CarState = CarState.Initialization;

        currentSpeed = defaultSpeed;

        var moveDuration = Global.Get<CarManager>().spawnedCarMoveDuration;

        progressOnPath = Utility.ModuloOne(PathManager.GetFreeProgressOnPath() + (defaultSpeed * moveDuration) / PathManager.PathLength);

        PathManager.RegisterProgress(this, progressOnPath);

        var targetPos = PathManager.GetPathPosition(progressOnPath);

        var targetRot = PathManager.GetPathRotation(progressOnPath) * Quaternion.AngleAxis(90F, Vector3.forward);

        transform.position = initialPos;

        transform.rotation = Random.rotation;

        var moveTweener = transform.DOMove(targetPos, moveDuration).SetEase(Ease.InSine);


        var rotateTweener = transform.DORotateQuaternion(targetRot, moveDuration).SetEase(Ease.InSine).OnComplete(() => 
        { 
            CarState = CarState.Racing;
        });
    }

    public void InitializeSpawnedCar(float progress)
    {
        currentSpeed = defaultSpeed;

        progressOnPath = progress;

        transform.position = PathManager.GetPathPosition(progressOnPath);

        var rotation = PathManager.GetPathRotation(progressOnPath) * Quaternion.AngleAxis(90F, Vector3.forward);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * turnSpeed);

        CarState = CarState.Racing;
    }

    private void Update()
    {
        if (CarState != CarState.Racing)
        {
            return;
        }

        CalculateSpeed();

        CalculateTransform();

        CalculateProgress();

        CalculateIncomePerSecond();
    }

    private void CalculateIncomePerSecond()
    {
        incomePerSecond = moneyOnCheckpointPass * (defaultSpeed + extraSpeed);
    }

    private void CalculateSpeed()
    {
        targetSpeed = (defaultSpeed + extraSpeed) * (1F - Mathf.Pow(PathManager.GetPathSharpness(progressOnPath + PathManager.DistanceToProgress(expectTurnDistance)), 2F - cornerSlowingRate));

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * acceleration);
    }

    private void CalculateTransform()
    {
        transform.position = Vector3.Lerp(transform.position, PathManager.GetPathPosition(progressOnPath), Time.deltaTime * inverseDrag);

        var rotation = PathManager.GetPathRotation(progressOnPath) * Quaternion.AngleAxis(90F, Vector3.forward);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * turnSpeed);
    }

    private void CalculateProgress()
    {
        var lastProgress = progressOnPath;

        progressOnPath += (currentSpeed * Time.deltaTime) / PathManager.PathLength;

        Global.Get<MoneyManager>().CheckCarPassCheckpoint(lastProgress, progressOnPath, moneyOnCheckpointPass);

        if (progressOnPath > 1F)
        {
            progressOnPath = Utility.ModuloOne(progressOnPath);
        }

        PathManager.RegisterProgress(this, progressOnPath);
    }

    public void SetExtraSpeed(float speed)
    {
        extraSpeed = speed;
    }

    public void SetTrailTime(float time, float duration)
    {
        var setTrailTweenID = "SetTrail_" + gameObject.GetInstanceID().ToString();

        if (DOTween.IsTweening(setTrailTweenID))
        {
            DOTween.Kill(setTrailTweenID);
        }

        DOTween.To(() => speedUpTrail.time, x => speedUpTrail.time = x, time, duration).SetId(setTrailTweenID);
    }

    public void Disable()
    {
        CarState = CarState.Elimination;
    }

    private void OnDestroy()
    {
        PathManager.UnregisterCar(this);
    }

}

public enum CarState
{
    Initialization,
    Racing,
    Elimination
}