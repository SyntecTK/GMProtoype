using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<float, float> OnResourcesChanged;
    public event Action<bool> OnGameEnded;

    public bool PlayerCanMove = true;

    [Header("Actors")]
    [SerializeField] private GameObject player;

    [SerializeField] private float flowValue = 50f;
    [SerializeField] private float energyValue = 0f;

    [SerializeField] private float maxFlow = 100f;
    [SerializeField] private float maxEnergy = 100f;

    private bool canparry = false;
    public bool CanParry => canparry;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    public void DamagePlayer(float damage)
    {
        //flowValue -= damage;
        flowValue -= damage;
        OnResourcesChanged?.Invoke(flowValue, energyValue);
        if (flowValue <= 0f)
        {
            Debug.Log("Player Died!");
            OnGameEnded?.Invoke(true);
        }
    }
    public bool UseFlow(float amount)
    {
        if (flowValue >= amount)
        {
            flowValue -= amount;
            OnResourcesChanged?.Invoke(flowValue, energyValue);
            Debug.Log("Used Flow");
            return true;
        }
        return false;
    }
    public bool UseEnergy(float amount)
    {
        if (energyValue >= amount)
        {
            energyValue -= amount;
            OnResourcesChanged?.Invoke(flowValue, energyValue);
            Debug.Log("Used Energy!");
            return true;
        }
        return false;
    }
    public void GainFlow(float amount)
    {
        flowValue = Mathf.Min(flowValue + amount, maxFlow);
        OnResourcesChanged?.Invoke(flowValue, energyValue);
        Debug.Log("Gained Health!");
    }
    public void GainEnergy(float amount)
    {
        energyValue = Mathf.Min(energyValue + amount, maxEnergy);
        OnResourcesChanged?.Invoke(flowValue, energyValue);
        Debug.Log("Gained Energy!");
    }
    public float GetFlow() => flowValue;
    public float GetEnergy() => energyValue;
    public Vector3 GetPlayerPosition()
    {
        return player.transform.position;
    }
    public void StartParryWindow()
    {
        canparry = true;
    }
    public void EndParryWindow()
    {
        canparry = false;
    }
    public void LevelClearedInvoke()
    {
        OnGameEnded?.Invoke(false);
    }

    public void BlockMovement()
    {
        PlayerCanMove = false;
    }

    public void ReleaseMovementBlock()
    {
        PlayerCanMove = true;
    }

}
