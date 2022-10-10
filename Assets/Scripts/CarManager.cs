using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class CarManager : MonoBehaviour
{

    public float TotalIncomePerSecond { get; private set; }

    public int CurrentCarCount => carsActive.Values.Count;

    public int MaxPossibleCarCount => Global.Get<SpawnerSystem>().CarTypesCount * 3 - 1;

    public bool MaxCarsSpawned => !CanCreateNewCar(0);

    //TEMPORARY
    public int MaxCars;

    public float spawnedCarMoveDuration;

    [SerializeField]
    private float speedUpDuration, speedUpSpeed, carMergeDuration, carTrailTime;

    private Dictionary<int, List<Car>> carsActive = new Dictionary<int, List<Car>>();

    private bool isSpeedUp;

    private int highestLevel = 0;

    private void Awake()
    {
        Global.Register<CarManager>(this);
    }

    private void Start()
    {
        Global.Get<InputManager>().OnTap += SpeedUp;
    }

    private bool CanCreateNewCar(int index)
    {
        if (!carsActive.ContainsKey(index))
            return true;

        var thisLvlCars = carsActive[index];

        if (!carsActive.ContainsKey(index + 1))
        {
            return thisLvlCars.Count < MaxCars;
        }

        var nextLvlCars = carsActive[index + 1];

        if(thisLvlCars.Count < (3 - Mathf.FloorToInt(nextLvlCars.Count / (float)3F)))
        {
            return CanCreateNewCar(index + 1);
        } else
        {
            return false;
        }
    }

    public bool CanMergeCars()
    {
        foreach (var carLevel in carsActive.Keys)
        {
            if (carsActive[carLevel].Count >= 3)
                return true;
        }

        return false;
    }

    private void Update()
    {
        var incomeThisSecond = 0F;
        foreach (var carLevel in carsActive.Keys)
        {       
            incomeThisSecond = carsActive[carLevel].Select(c => c.incomePerSecond).Sum();
        }

        TotalIncomePerSecond = (incomeThisSecond
                * Global.Get<SpawnerSystem>().CurrentCheckpointCount) / (Global.Get<PathManager>().PathLength * 1.085F);
    }

    public void RegisterCar(Car car, int level)
    {
        if(!carsActive.ContainsKey(level))
        {
            carsActive[level] = new List<Car>();
        }

        carsActive[level].Add(car);

        highestLevel = Mathf.Max(level, highestLevel);

        if (isSpeedUp)
        {
            car.SetExtraSpeed(speedUpSpeed);
            car.SetTrailTime(0.5F, speedUpDuration * 0.2F);
        }
    }

    public void MergeCars()
    {
        var centralPos = Global.Get<PathManager>().GetPathAverageWorldPos();

        var mergeSeq = DOTween.Sequence();

        var mergeLevel = carsActive.Keys.First(l => carsActive[l].Count >= 3);

        var mergeableLevelCars = carsActive[mergeLevel];

        for (int i = 0; i < 3; i++)
        {
            mergeableLevelCars[i].Disable();

            mergeSeq.Join(mergeableLevelCars[i].transform.DOMove(centralPos, carMergeDuration).SetEase(Ease.Linear));
            mergeSeq.Join(mergeableLevelCars[i].transform.DOBlendableRotateBy(Vector3.up * 540F, carMergeDuration).SetEase(Ease.InCirc));
            mergeSeq.Join(mergeableLevelCars[i].transform.DOBlendableRotateBy(Vector3.up * 540F, carMergeDuration).SetEase(Ease.InCirc));
            mergeSeq.Join(mergeableLevelCars[i].transform.DOBlendableRotateBy(Vector3.up * 540F, carMergeDuration).SetEase(Ease.InCirc));
            mergeSeq.Join(mergeableLevelCars[i].transform.DOBlendableRotateBy(Vector3.up * 540F, carMergeDuration).SetEase(Ease.InCirc));
            mergeSeq.Join(mergeableLevelCars[i].transform.DOBlendableRotateBy(Vector3.up * 540F, carMergeDuration).SetEase(Ease.InCirc));
        }

        mergeSeq.OnComplete(() => 
        {
            for (int i = 0; i < 3; i++)
            {
                var car = carsActive[mergeLevel][0];

                carsActive[mergeLevel].RemoveAt(0);

                Destroy(car.gameObject);
            }        

            Global.Get<SpawnerSystem>().SpawnCar(mergeLevel + 1, centralPos); 
        });
    }

    private void SpeedUp()
    {
        if (isSpeedUp)
            return;

        isSpeedUp = true;


        foreach (var carLevel in carsActive.Keys)
        {
            foreach (var car in carsActive[carLevel])
            {
                car.SetExtraSpeed(speedUpSpeed);
                car.SetTrailTime(carTrailTime, speedUpDuration * 0.2F);
            }
        }

        DOTween.Sequence().SetDelay(speedUpDuration).OnComplete(() => CancelSpeedUp());
    }

    private void CancelSpeedUp()
    {
        isSpeedUp = false;

        foreach (var carLevel in carsActive.Keys)
        {
            foreach (var car in carsActive[carLevel])
            {
                car.SetExtraSpeed(0F);
                car.SetTrailTime(0F, speedUpDuration * 2F);
            }
        }
    }
}
