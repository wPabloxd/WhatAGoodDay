using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static PlayerController;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public WeatherManager.WeatherStates weatherState = WeatherManager.WeatherStates.Sunny;
    [SerializeField] AudioMixer audioMixer;
    GameObject weatherManager;
    GameObject player;
    GameObject crawlerManager;
    GameObject grounFlashlight;
    GameObject blackScreen;
    private float volume = 1f;
    public static bool isPaused;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }   
    public void PlayerRespawn()
    {
        weatherManager.GetComponent<WeatherManager>().UpdateWeatherState(WeatherManager.WeatherStates.Storm, 1f);
        player.GetComponent<PlayerController>().GoToRespawnPoint();
        player.GetComponent<PlayerController>().UpdateMovementPhase(MovementPhases.Static);
        player.GetComponent<PlayerController>().ToggleFlashlight(false);
        crawlerManager.GetComponent<CrawlerInstantiator>().bloodStormActive = false;
        grounFlashlight.SetActive(true);
        StartCoroutine(FadeOutBlackScreen());
        StartCoroutine(RespawnDelay());
    }
    IEnumerator FadeOutBlackScreen()
    {
        blackScreen.GetComponent<Image>().enabled = true;
        blackScreen.GetComponent<Image>().color = new Color(0f, 0f, 0f, 255f);
        yield return new WaitForSeconds(1f);
        float duration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            blackScreen.GetComponent<Image>().color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        blackScreen.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        blackScreen.GetComponent<Image>().enabled = false;
    }
    IEnumerator RespawnDelay()
    {
        yield return new WaitForSeconds(1.5f);
        player.GetComponent<PlayerController>().UpdateMovementPhase(MovementPhases.Run);
        player.GetComponent<PlayerController>().EnableCharacterController();
    }
    public static void LoadScene(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }
    public float GetVolume()
    {
        return volume;
    }
    public void SetVolume(float volume)
    {
        this.volume = volume;
        audioMixer.SetFloat("AudioMixer", Mathf.Log10(volume) * 20);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 1)
        {
            Shader.WarmupAllShaders();
            AudioClip[] allAudioClips = Resources.LoadAll<AudioClip>("Assets/Sounds");
            PauseGame(false);
            player = GameObject.Find("Player");
            weatherManager = GameObject.Find("WeatherManager");
            crawlerManager = GameObject.Find("CrawlersManager");
            grounFlashlight = GameObject.Find("FlashlightGround");
            blackScreen = GameObject.Find("BlackScreen");
        }
    }
    public void PauseGame(bool paused)
    {
        isPaused = paused;
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        if (paused)
        {
            Time.timeScale = 0f;
            foreach (AudioSource audioSource in allAudioSources)
            {
                audioSource.Pause();
            }
        }
        else
        {
            Time.timeScale = 1f;
            foreach (AudioSource audioSource in allAudioSources)
            {
                audioSource.UnPause();
            }
        }
    }
}
