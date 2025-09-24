using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<float, float> OnResourcesChanged;

    private float flowValue = 50f;
    private float energyValue = 0f;

    [SerializeField] private float maxFlow = 100f;
    [SerializeField] private float maxEnergy = 100f;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public bool UseFlow(float amount)
    {
        if(flowValue >= amount)
        {
            Debug.Log("Using Flow. Flow now: " + flowValue);
            flowValue -= amount;
            OnResourcesChanged?.Invoke(flowValue, energyValue);
            Debug.Log("Used Flow. Flow now: " + flowValue);
            return true;
        }
        return false;
    }

    public bool UseEnergy(float amount)
    {
        if(energyValue >= amount)
        {
            energyValue -= amount;
            OnResourcesChanged?.Invoke(flowValue, energyValue);
            return true;
        }
        return false;
    }

    public void GainFlow(float amount)
    {
        flowValue = Mathf.Min(flowValue + amount, maxFlow);
        OnResourcesChanged?.Invoke(flowValue, energyValue);
    }

    public void GainEnergy(float amount)
    {
        energyValue = Mathf.Min(energyValue + amount, maxEnergy);
        OnResourcesChanged?.Invoke(flowValue, energyValue);
    }

    public float GetFlow() => flowValue;
    public float GetEnergy() => energyValue;
}
