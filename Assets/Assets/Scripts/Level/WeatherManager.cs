using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WeatherManager : MonoBehaviour
{
    public enum WeatherStates
    {
        Sunny,
        Cloudy,
        Rainy,
        Storm,
        Blood
    };

    [Header("Object References")]
    [SerializeField] Light directionalLight;
    [SerializeField] Light lightning;
    [SerializeField] ParticleSystem rainParticles;

    [Header("Directional Light Colors")]
    [SerializeField] UnityEngine.Color sunnyColor;
    [SerializeField] UnityEngine.Color cloudyColor;
    [SerializeField] UnityEngine.Color rainyColor;
    [SerializeField] UnityEngine.Color stormColor;
    [SerializeField] UnityEngine.Color bloodColor;

    [Header("Sky Colors")]
    [SerializeField] UnityEngine.Color sunnySkyColor;
    [SerializeField] UnityEngine.Color cloudySkyColor;
    [SerializeField] UnityEngine.Color stormSkyColor;
    [SerializeField] UnityEngine.Color bloodSkyColor;

    [Header("Light Intensities")]
    [SerializeField] float sunnyIntensity;
    [SerializeField] float cloudyIntensity;
    [SerializeField] float rainyIntensity;
    [SerializeField] float stormIntensity;
    [SerializeField] float bloodIntensity;

    [Header("Shadow Intensities")]
    [SerializeField] float sunnyShadow;
    [SerializeField] float cloudyShadow;
    [SerializeField] float rainyShadow;
    [SerializeField] float stormShadow;
    [SerializeField] float bloodShadow;

    [Header("Rain Intensities")]
    [SerializeField] float rainyRain;
    [SerializeField] float stormRain;
    [SerializeField] float bloodRain;

    [Header("Rain Colors")]
    [SerializeField] UnityEngine.Color normalRainColor;
    [SerializeField] UnityEngine.Color bloodRainColor;

    [Header("Sound")]
    [SerializeField] AudioSource[] audioSources;
    [SerializeField] AudioClip sunnyAmbience;
    [SerializeField] AudioClip cloudyAmbience;
    [SerializeField] AudioClip rainyAmbience;
    [SerializeField] AudioClip stormAmbience;
    [SerializeField] AudioClip[] thunderSounds;
    private bool swapAmbienceSource;
    private bool swapThunderSource;
    private float updateInterval = 0.2f;

    [Header("Other")]
    public float transitionTime;
    public void UpdateWeatherState(WeatherStates newState, float transitionTime)
    {
        GameManager.Instance.weatherState = newState;
        switch(newState) 
        {
            case WeatherStates.Sunny: TransitionToSunny(transitionTime); break;
            case WeatherStates.Cloudy: TransitionToCloudy(transitionTime); break;
            case WeatherStates.Rainy: TransitionToRainning(transitionTime); break;
            case WeatherStates.Storm: TransitionToStorm(transitionTime); break;
            case WeatherStates.Blood: TransitionToBlood(); break;
            default: Debug.LogWarning("Unhandled WeatherState: " + newState); break;
        }
    }

    private void TransitionToSunny(float transitionTime)
    {
        StartCoroutine(SmoothDirectionalLightWeatherChange(directionalLight.color, sunnyColor, directionalLight.intensity, sunnyIntensity, directionalLight.shadowStrength, sunnyShadow, transitionTime));
        StartCoroutine(SmoothUpdateRain(rainParticles.emission.rateOverTime.constant, 0f, rainParticles.main.startColor.color, normalRainColor, transitionTime));
        StartCoroutine(SmoothUpdateSky(UnityEngine.RenderSettings.ambientLight, sunnySkyColor, transitionTime));
        StartCoroutine(CrossfadeRoutine(sunnyAmbience, transitionTime));
    }
    private void TransitionToCloudy(float transitionTime)
    {
        StartCoroutine(SmoothDirectionalLightWeatherChange(directionalLight.color, cloudyColor, directionalLight.intensity, cloudyIntensity, directionalLight.shadowStrength, cloudyShadow, transitionTime));
        StartCoroutine(SmoothUpdateRain(rainParticles.emission.rateOverTime.constant, 0f, rainParticles.main.startColor.color, normalRainColor, transitionTime));
        StartCoroutine(SmoothUpdateSky(UnityEngine.RenderSettings.ambientLight, cloudySkyColor, transitionTime));
        StartCoroutine(CrossfadeRoutine(cloudyAmbience, transitionTime));
    }
    private void TransitionToRainning(float transitionTime)
    {
        StartCoroutine(SmoothDirectionalLightWeatherChange(directionalLight.color, rainyColor, directionalLight.intensity, rainyIntensity, directionalLight.shadowStrength, rainyShadow, transitionTime));
        StartCoroutine(SmoothUpdateRain(rainParticles.emission.rateOverTime.constant, rainyRain, rainParticles.main.startColor.color, normalRainColor, transitionTime));
        StartCoroutine(SmoothUpdateSky(UnityEngine.RenderSettings.ambientLight, cloudySkyColor, transitionTime));
        StartCoroutine(CrossfadeRoutine(rainyAmbience, transitionTime));
    }
    private void TransitionToStorm(float transitionTime)
    {
        StartCoroutine(SmoothDirectionalLightWeatherChange(directionalLight.color, stormColor, directionalLight.intensity, stormIntensity, directionalLight.shadowStrength, stormShadow, transitionTime));
        StartCoroutine(SmoothUpdateRain(rainParticles.emission.rateOverTime.constant, stormRain, rainParticles.main.startColor.color, normalRainColor, transitionTime));
        StartCoroutine(SmoothUpdateSky(UnityEngine.RenderSettings.ambientLight, stormSkyColor, transitionTime));
        StartCoroutine(CrossfadeRoutine(stormAmbience, transitionTime));
    }
    private void TransitionToBlood()
    {
        StartCoroutine(Lightning());
        StartCoroutine(SmoothDirectionalLightWeatherChange(directionalLight.color, bloodColor, directionalLight.intensity, bloodIntensity, directionalLight.shadowStrength, bloodShadow, 0.2f));
        StartCoroutine(SmoothUpdateRain(rainParticles.emission.rateOverTime.constant, bloodRain, rainParticles.main.startColor.color, bloodRainColor, 0.2f));
        StartCoroutine(SmoothUpdateSky(UnityEngine.RenderSettings.ambientLight, bloodSkyColor, 0.2f));
        StartCoroutine(CrossfadeRoutine(stormAmbience, 0.2f));
    }
    private IEnumerator SmoothDirectionalLightWeatherChange(UnityEngine.Color colorFrom, UnityEngine.Color colorTo, float intensityFrom, float intensityTo, float shadowFrom, float shadowTo, float duration)
    {
        float elapsedTime = 0f;
        UnityEngine.Color finalColor = colorFrom;
        float finalIntensity = intensityFrom;
        float finalShadow = shadowFrom;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float exponentialT = Mathf.Pow(t, 2);
            finalColor = UnityEngine.Color.Lerp(colorFrom, colorTo, t);
            finalIntensity = Mathf.Lerp(intensityFrom, intensityTo, t);
            finalShadow = Mathf.Lerp(shadowFrom, shadowTo, exponentialT);
            directionalLight.color = finalColor;
            directionalLight.intensity = finalIntensity;
            directionalLight.shadowStrength = finalShadow;
            yield return new WaitForSeconds(updateInterval / 5);
            elapsedTime += Time.deltaTime + updateInterval / 5;
        }
        finalColor = colorTo;
        finalIntensity = intensityTo;
        finalShadow = shadowTo;
        directionalLight.color = finalColor;
        directionalLight.intensity = finalIntensity;
        directionalLight.shadowStrength = finalShadow;
    }
    private IEnumerator SmoothUpdateRain(float intensityFrom, float intensityTo, UnityEngine.Color colorFrom, UnityEngine.Color colorTo, float duration)
    {
        float elapsedTime = 0f;
        UnityEngine.Color finalColor = colorFrom;
        float finalIntensity = intensityFrom;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            finalColor = UnityEngine.Color.Lerp(colorFrom, colorTo, t);
            finalIntensity = Mathf.Lerp(intensityFrom, intensityTo, t);
            var emission = rainParticles.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(finalIntensity);
            var main = rainParticles.main;
            main.startColor = finalColor;
            yield return new WaitForSeconds(updateInterval);
            elapsedTime += Time.deltaTime + updateInterval;
        }
        finalColor = colorTo;
        finalIntensity = intensityTo;
        var emissionFinal = rainParticles.emission;
        emissionFinal.rateOverTime = new ParticleSystem.MinMaxCurve(finalIntensity);
        var mainFinal = rainParticles.main;
        mainFinal.startColor = finalColor;
    }
    private IEnumerator SmoothUpdateSky(UnityEngine.Color colorFrom, UnityEngine.Color colorTo, float duration)
    {
        float elapsedTime = 0f;
        UnityEngine.Color finalColor = colorFrom;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            finalColor = UnityEngine.Color.Lerp(colorFrom, colorTo, t);
            UnityEngine.RenderSettings.ambientLight = finalColor;
            yield return new WaitForSeconds(updateInterval);
            elapsedTime += Time.deltaTime + updateInterval;
        }
        finalColor = colorTo;
        UnityEngine.RenderSettings.ambientLight = finalColor;
    }
    public IEnumerator Lightning()
    {
        audioSources[swapThunderSource == false ? 2 : 3].clip = thunderSounds[Random.Range(0, thunderSounds.Length)];
        audioSources[swapThunderSource == false ? 2 : 3].pitch = Random.Range(0.85f, 1.15f);
        audioSources[swapThunderSource == false ? 2 : 3].Play();
        lightning.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        lightning.gameObject.SetActive(false);
    }

    private IEnumerator CrossfadeRoutine(AudioClip newClip, float fadeDuration)
    {
        int currentAudio = swapAmbienceSource == false ? 0 : 1;
        int newAudio = swapAmbienceSource == false ? 1 : 0;
        float elapsedTime = 0f;

        audioSources[newAudio].clip = newClip;
        audioSources[newAudio].volume = 0f;
        audioSources[newAudio].Play();

        float startVolumeCurrent = audioSources[currentAudio].volume;
        float startVolumeNew = audioSources[newAudio].volume;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;

            audioSources[currentAudio].volume = Mathf.Lerp(startVolumeCurrent, 0f, t);
            audioSources[newAudio].volume = Mathf.Lerp(startVolumeNew, 1f, t);

            yield return null;
        }
        audioSources[currentAudio].volume = 0f;
        audioSources[newAudio].volume = 1f;

        audioSources[currentAudio].Stop();
        swapAmbienceSource = !swapAmbienceSource;
    }
}