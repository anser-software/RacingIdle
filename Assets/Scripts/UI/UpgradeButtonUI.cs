using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
[RequireComponent(typeof(Button))]
public class UpgradeButtonUI : MonoBehaviour
{

    [SerializeField]
    private bool hideTheEntireButtonIfLocked;

    [HideInInspector]
    public int upgradeIndex;

    [SerializeField]
    private TextMeshProUGUI priceText;

    [SerializeField]
    private int modBaseForPrice;

    private int price;

    private Button button;

    private Image image;

    private GameObject childElementsParent;

    private bool canAfford => Global.Get<MoneyManager>().CanAffordUpgrade(price);

    private bool isUnlocked => Global.Get<ProgressManager>().IsUpgradeUnlocked(upgradeIndex);

    private bool isMaxedOut => Global.Get<ProgressManager>().IsUpgradeMaxedOut(upgradeIndex);

    private void Start()
    {
        UpdatePrice();

        button = GetComponent<Button>();

        image = GetComponent<Image>();

        childElementsParent = transform.GetChild(0).gameObject;
    }

    public void ExecuteUpgrade()
    {
        Global.Get<UIManager>().ExecuteUpgradeButton(upgradeIndex);
        UpdatePrice();
    }

    public void UpdatePrice()
    {
        price = Mathf.RoundToInt(Global.Get<ProgressManager>().GetUpgradePrice(upgradeIndex));

        price -= Utility.Modulo(price, modBaseForPrice);

        priceText.text = "$" + price.ToString();
    }

    private void OnGUI()
    {
        button.interactable = true;

        if (isUnlocked)
        {
            if (hideTheEntireButtonIfLocked && !image.enabled)
            {
                childElementsParent.SetActive(true);
                image.enabled = true;
            }

            if (isMaxedOut)
            {
                button.interactable = false;
                priceText.text = "MAX";
            }
            else
            {
                button.interactable = canAfford;

                priceText.text = "$" + price.ToString();
            }
        }
        else
        {
            if (hideTheEntireButtonIfLocked)
            {
                childElementsParent.SetActive(false);
                image.enabled = false;
            } else
            {
                button.interactable = false;
            }
        }
    }




}
