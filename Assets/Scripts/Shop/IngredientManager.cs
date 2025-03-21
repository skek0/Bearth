using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IngredientManager : MonoBehaviour
{
    float metal =1000;
    int fuel;
    Text metalText;
    Text fuelText;

    bool isPressing = false;
    float timer = 0f;
    bool isConverting = false;

    private void Start()
    {
        transform.parent.Find("MetalText").TryGetComponent(out metalText);
        transform.parent.Find("FuelText").TryGetComponent(out fuelText);
    }
    private void Update()
    {
        metal += Time.deltaTime;
        metalText.text = "Metal : " + metal.ToString("F2");
        fuelText.text = "Fuel : " + fuel.ToString();
        if (isPressing && !isConverting)
        {
            timer += Time.deltaTime;
            if (timer > 1f) StartCoroutine(RepeatConverting());
        }
    }

    public void ConvertToFuel()
    {
        if(metal > 10f)
        {
            metal -= 10f;
            fuel++;
        }
    }

    public void OnClickingDown()
    {
        isPressing = true;
    }

    IEnumerator RepeatConverting()
    {
        isConverting = true;
        while (isPressing)
        {
            ConvertToFuel();
            yield return CoroutineCache.WaitforSeconds(0.1f);
        }
    }

    public void OnClickingUp()
    {
        isPressing = false; 
        ConvertToFuel();
        timer = 0f;
    }
}
