using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIMoney : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI totalMoneyCounter;

    [SerializeField]
    private TextMeshProUGUI moneyPerSecondCounter;


    private MoneyManager moneyManager => Global.Get<MoneyManager>();

    private void Update()
    {
        totalMoneyCounter.text = "$" + moneyManager.Money.ToString("0.0");

        moneyPerSecondCounter.text = "$" + moneyManager.MoneyPerSecond.ToString("0.0") + "/s";
    }

}
