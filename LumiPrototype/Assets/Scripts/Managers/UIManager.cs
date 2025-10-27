using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Resource Sliders")]
    [SerializeField] private Slider flowSlider;
    [SerializeField] private Slider energySlider;

    [Header("Screens")]
    [SerializeField] private GameObject gameEndScreen;
    [SerializeField] private GameObject pauseScreen;

    [Header("Text")]
    [SerializeField] private TMP_Text gameOverText;

    private void Start()
    {
        GameManager.Instance.OnResourcesChanged += UpdateResourcesUI;
        GameManager.Instance.OnGameEnded += ShowGameOverScreen;
    }


    private void OnDisable()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnResourcesChanged -= UpdateResourcesUI;
            GameManager.Instance.OnGameEnded -= ShowGameOverScreen;
        }
    }

    #region UI Changes
    private void ShowGameOverScreen(bool playerDied)
    {
        gameEndScreen.SetActive(true);
        Time.timeScale = 0f;    
        if(playerDied)
        {
            gameOverText.text = "Game Over! :(";
        }
        else
        {
            gameOverText.text = "Level Cleared! :)";
        }
    }
    private void UpdateResourcesUI(float flowValue, float energyValue)
    {
        flowSlider.value = flowValue;
        energySlider.value = energyValue; 
    }
    private void ShowPauseScreen()
    {
        gameEndScreen.SetActive(true);
        Time.timeScale = 0f;
        gameOverText.text = "Pause";

    }
    private void ClosePauseScreen()
    {
        gameEndScreen.SetActive(false);
        Time.timeScale = 1f;
    }
    #endregion

    #region Menu Buttons
    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
    #endregion

}
