using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Economy : MonoBehaviour
{
    public GameObject balanceGO;
    public int balance = 0;
    int prevBalance = 0;
    TMP_Text buttonText;

    public GameObject consumptionGO;
    string str;
    string prevstr = "";
    TMP_Text buttonText2;
    public EnergyConsumption energyConsumption;
    Color initColor;

    void Awake()
    {
        //energyConsumption = GetComponent<EnergyConsumption>();
        
        buttonText = balanceGO.GetComponent<TMP_Text>();
        buttonText2 = consumptionGO.GetComponent<TMP_Text>();
        initColor = buttonText2.color;
        
    }
    public void SetBalance(int value)
    {
        balance = value * 250;
        Debug.Log(balance);
        buttonText.text = "$" + balance;
    }
    void Update()
    {
        if (prevBalance != balance)
            buttonText.text = "$" + balance;
        prevBalance = balance;

        str = energyConsumption.totalMW + "MW/" + energyConsumption.houseMW + "MW";
        if (prevstr != str)
            buttonText2.text = str;
        prevstr = str;
        if (energyConsumption.totalMW >= energyConsumption.houseMW)
            buttonText2.color = Color.green;
        else buttonText2.color = initColor;
    }
}
