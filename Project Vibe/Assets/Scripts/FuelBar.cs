using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FuelBar : MonoBehaviour
{
    public Slider slider;

    public void setFuel(int fuelLevel) //moves slider UI according to fuel usage in gameplay
    {
        slider.value = fuelLevel;
    }

    public void setMaxFuel(int fuelLevel)
    {
        slider.maxValue = fuelLevel;
        slider.value = fuelLevel;
    }
}
