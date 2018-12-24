using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HP : MonoBehaviour
{
    public Slider Health;
    public int HealthMax;
    public Text HealthText;

    private bool isYellow = false;

	void Start () {
        Health.minValue = 0;
        HealthText.text = Health.value.ToString();
        HealthText.color = new Color(14.0f / 255.0f, 121.0f / 255.0f, 4.0f / 255.0f);
    }

    public void Minus(float value)
    {
        Health.value = Health.value - value;
        HealthText.text = Health.value.ToString();
        if (Health.value < (HealthMax * 2 / 3) && !isYellow) { HealthText.color = new Color(0.0f / 255.0f, 0.0f / 0.0f, 0.0f / 255.0f); isYellow = true; }
        if (Health.value < (HealthMax / 3) && isYellow) { HealthText.color = new Color(255.0f / 255.0f, 0.0f / 255.0f, 0.0f / 255.0f); isYellow = false; }
    }

    public float GetHealth()
    {
        return Health.value;
    }

    public void Init(float val)
    {
        Health.maxValue = val;
        Health.value = val;
    }
}
