using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using GameAnalyticsSDK;
public class ProgressManager : MonoBehaviour, ISaveable
{

    private Upgrade[] upgrades;

    [SaveThis]
    private int maxSpawnedCarType = 0;

    [SaveThis]
    private int[] executionCountPerUpgrade;

    private void Awake()
    {
        Global.Register<ProgressManager>(this);

        LoadUpgrades();
    }

    private void Start()
    {
        SaveDataManager.LoadAll();

        GameAnalytics.Initialize();
        Facebook.Unity.FB.Init();
    }

    public void OnBeforeDataSaved()
    {
        executionCountPerUpgrade = new int[upgrades.Length];

        for (int i = 0; i < upgrades.Length; i++)
        {
            executionCountPerUpgrade[i] = upgrades[i].executionsCount;
        }
    }

    public void OnAfterDataLoaded()
    {
        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i].executionsCount = executionCountPerUpgrade[i];
        }

        foreach (var upgradeButton in FindObjectsOfType<UpgradeButtonUI>())
        {
            upgradeButton.UpdatePrice();
        }
    }

    private void LoadUpgrades()
    {
        Resources.LoadAll<Upgrade>("");

        upgrades = GetAllUpgrades();

        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i].SetUnlockCondition();
            upgrades[i].SetMaxCondition();
        }
    }

    public void NewCarTypeSpawned(int index)
    {
        if (index > maxSpawnedCarType)
        {
            Global.Get<UIManager>().OpenNewCarPopUp(index - 1);
            maxSpawnedCarType = index;
        }
    }

    public bool IsUpgradeMaxedOut(int index)
    {
        return upgrades[index].MaxCondition.Invoke();
    }

    public bool IsUpgradeUnlocked(int index)
    {
        return upgrades[index].UnlockCondition.Invoke();
    }

    public float GetUpgradeDefaultPrice(int index)
    {
        return upgrades[index].defaultPrice;
    }

    public float GetUpgradePrice(int index)
    {
        return upgrades[index].GetCurrentPrice();
    }

    public void ExecuteUpgrade(int upgradeIndex)
    {
        if (Global.Get<MoneyManager>().CanAffordUpgrade(upgrades[upgradeIndex].defaultPrice))
        {
            upgrades[upgradeIndex].Execute();
            SaveDataManager.SaveAll();
        }
    }

    public static Upgrade[] GetAllUpgrades()
    {
        return Resources.FindObjectsOfTypeAll<Upgrade>();
    }

    private void OnApplicationQuit()
    {
        SaveDataManager.SaveAll();
    }


}
