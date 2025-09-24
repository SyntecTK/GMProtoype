using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Resource Sliders")]
    [SerializeField] private Slider flowSlider;
    [SerializeField] private Slider energySlider;

    private void Start()
    {
        GameManager.Instance.OnResourcesChanged += UpdateResourcesUI;
    }

    private void OnDisable()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnResourcesChanged -= UpdateResourcesUI;
        }
    }

    private void UpdateResourcesUI(float flowValue, float energyValue)
    {
        flowSlider.value = flowValue;
        energySlider.value = energyValue; 
    }

}
