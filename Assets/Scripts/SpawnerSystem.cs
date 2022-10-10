using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class SpawnerSystem : MonoBehaviour, ISaveable
{

    public int CarTypesCount => cars.Length;

    public int CurrentCheckpointCount => checkpointCount;

    public bool MaxCheckpointsSpawned => checkpointCount >= maxCheckpointCountPerSegment * (1 + Global.Get<PathManager>().SegmentsAppendedCount);

    [SerializeField]
    private SpawnableObject moneyCheckpoint;

    [SerializeField]
    private int initialCheckpointCount, maxCheckpointCountPerSegment;

    [SerializeField]
    private GameObject[] cars;

    [SaveThis]
    private int checkpointCount;

    [SaveThis]
    private List<int> carTypesSpawned;

    [SaveThis]
    private List<float> progressesPerCarSpawned;

    private void Awake()
    {
        Global.Register<SpawnerSystem>(this);
    }

    public void SpawnCar(int index)
    {
        var car = Instantiate(cars[index]).GetComponent<Car>();

        car.InitializeSpawnedCar(UnityEngine.Random.Range(0F, 1F));

        Global.Get<ProgressManager>().NewCarTypeSpawned(index);

        Global.Get<CarManager>().RegisterCar(car, index);
    }

    public void SpawnCar(int index, Vector3 initialPos)
    {
        var car = Instantiate(cars[index]).GetComponent<Car>();

        car.InitializeSpawnedCar(initialPos);

        Global.Get<ProgressManager>().NewCarTypeSpawned(index);

        Global.Get<CarManager>().RegisterCar(car, index);

        SaveDataManager.SaveAll();
    }

    public void SpawnCar(int index, float progress)
    {
        var car = Instantiate(cars[index]).GetComponent<Car>();

        car.InitializeSpawnedCar(progress);

        Global.Get<ProgressManager>().NewCarTypeSpawned(index);

        Global.Get<CarManager>().RegisterCar(car, index);
    }


    public void SpawnMoneyCheckpoint()
    {
        var objInstance = Instantiate(moneyCheckpoint.Prefab);

        checkpointCount++;

        var allCheckPointProgresses = FindObjectsOfType<MoneyCheckpoint>().Select(c => c.progress).ToList();

        var pathProgress = Global.Get<PathManager>().GetFreeProgressOnPath(allCheckPointProgresses);

        var pos = Global.Get<PathManager>().GetPathPosition(pathProgress);

        objInstance.transform.position = pos;

        objInstance.transform.position += objInstance.transform.TransformVector(moneyCheckpoint.LocalOffset);

        var worldForward = Global.Get<PathManager>().GetPathDirection(pathProgress);

        objInstance.transform.forward = worldForward;

        var worldTargetForward = objInstance.transform.TransformDirection(moneyCheckpoint.Forward);

        objInstance.transform.rotation *= Quaternion.AngleAxis(Vector3.Angle(worldForward, worldTargetForward), Vector3.Cross(worldTargetForward, worldForward).normalized);

        var ispawnable = objInstance.GetComponent<ISpawnable>();

        if (ispawnable != null)
        {
            ispawnable.OnAfterSpawn();
        }
    }

    public void OnBeforeDataSaved()
    {
        Debug.Log("Saving car data...");

        carTypesSpawned = new List<int>();

        progressesPerCarSpawned = new List<float>();

        foreach (var car in FindObjectsOfType<Car>().Where(c => c.CarState != CarState.Elimination))
        {
            carTypesSpawned.Add(car.typeID);
            progressesPerCarSpawned.Add(car.Progress);
        }
    }

    public void OnAfterDataLoaded()
    {
        Debug.Log("Spawning loaded cars...");

        for (int i = 0; i < carTypesSpawned.Count; i++)
        {
            SpawnCar(carTypesSpawned[i], progressesPerCarSpawned[i]);
        }

        var savedCheckpointCount = checkpointCount;

        checkpointCount = 0;

        for (int i = 0; i < savedCheckpointCount; i++)
        {
            SpawnMoneyCheckpoint();
        }
    }

    public void OnNoSaveDataFound()
    {
        Debug.Log("Spawning default car and checkpoints...");

        for (int i = 0; i < initialCheckpointCount; i++)
        {
            SpawnMoneyCheckpoint();
        }

        SpawnCar(0);
    }

}

[Serializable]
public class SpawnableObject
{

    public GameObject Prefab;

    public Vector3 LocalOffset, Forward;

}