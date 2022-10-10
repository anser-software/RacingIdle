using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class UIManager : MonoBehaviour
{

    [SerializeField]
    private GameObject newCarParent;

    [SerializeField]
    private GameObject[] cars3DUI;

    private bool newCarPopUp;

    private int currentNewCarIndex;

    private void Awake()
    {
        Global.Register<UIManager>(this);
    }

    public void ExecuteUpgradeButton(int upgradeIndex)
    {
        Global.Get<ProgressManager>().ExecuteUpgrade(upgradeIndex);
    }

    private void Update()
    {
        if(newCarPopUp && Input.GetMouseButtonDown(0))
        {
            CloseNewCarPopUp();
        }
    }

    public void OpenNewCarPopUp(int carIndex)
    {
        Debug.Log("OPEN CAR POP UP");

        currentNewCarIndex = carIndex;

        cars3DUI[carIndex].SetActive(true);

        newCarParent.SetActive(true);

        newCarPopUp = true;
    }

    private void CloseNewCarPopUp()
    {
        cars3DUI[currentNewCarIndex].SetActive(false);

        newCarParent.SetActive(false);

        newCarPopUp = false;
    }

}
