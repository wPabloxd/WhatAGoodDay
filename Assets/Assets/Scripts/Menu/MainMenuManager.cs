using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Slider sliderVolume;
    [SerializeField] GameObject inGameMenu;
    [SerializeField] GameObject pauseButton;
    public void PlayLevel(int levelIndex)
    {
        GameManager.LoadScene(levelIndex);
    }
    private void Start()
    {
        sliderVolume.value = GameManager.Instance.GetVolume();
    }
    public void SetVolume()
    {

        GameManager.Instance.SetVolume(sliderVolume.value);
    }
    public void PauseGame(bool paused)
    {
        inGameMenu.SetActive(paused);
        pauseButton.SetActive(!paused);
        GameManager.Instance.PauseGame(paused);
    }
    public void LoadScene(int levelIndex)
    {
        GameManager.LoadScene(levelIndex);
    }
}