using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Baracuda.Monitoring;
using System;
using System.Linq;
public class MoneyManager : MonoBehaviour, ISaveable
{

    public event Action OnCheckpointPassed;

    public float Money { get; private set; }

    public float MoneyPerSecond => Global.Get<CarManager>().TotalIncomePerSecond;

    List<MoneyCheckpoint> checkpoints = new List<MoneyCheckpoint>();

    [SerializeField]
    private float debugMoneyMultiplier;

    [SaveThis]
    private float totalMoney;

    private void Awake()
    {
        Global.Register<MoneyManager>(this);
    }


    public void CheckCarPassCheckpoint(float lastProgress, float currentProgress, float moneyToAddIfTrue)
    {
        foreach (var checkpoint in checkpoints)
        {
            if (lastProgress <= checkpoint.progress && currentProgress >= checkpoint.progress)
            {
                AddMoney(moneyToAddIfTrue);

                checkpoint.OnPassed(moneyToAddIfTrue);

                OnCheckpointPassed?.Invoke();
            }
        }
    }

    public void AddMoney(float amount)
    {
        Money += amount * debugMoneyMultiplier;
    }

    public void TakeMoney(float amount)
    {
        if (Money - amount < 0F)
            return;

        Money -= amount;
    }

    public bool CanAffordUpgrade(float price)
    {
        return Money - price >= 0F;
    }

    public void RegisterCheckpoint(MoneyCheckpoint checkpoint)
    {
        checkpoints.Add(checkpoint);
    }

    public void UnregisterCheckpoint(MoneyCheckpoint checkpoint)
    {
        if(checkpoints.Contains(checkpoint))
        {
            checkpoints.Remove(checkpoint);
        }
    }

    public void OnBeforeDataSaved()
    {
        totalMoney = Money;
    }

    public void OnAfterDataLoaded()
    {
        Money = totalMoney;
    }
}
