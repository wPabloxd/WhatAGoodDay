using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSounds : MonoBehaviour
{
    [SerializeField] AudioSource[] audioSources;
    [SerializeField] AudioClip[] stepDrySounds;
    [SerializeField] AudioClip[] stepWetSounds;
    bool swap;
    public void MakeSound()
    {
        bool isWet = (GameManager.Instance.weatherState == WeatherManager.WeatherStates.Sunny || GameManager.Instance.weatherState == WeatherManager.WeatherStates.Cloudy) ? false : true;
        if (isWet)
        {
            audioSources[swap == true ? 0 : 1].clip = stepWetSounds[Random.Range(0, stepWetSounds.Length)];
            if(GameManager.Instance.weatherState == WeatherManager.WeatherStates.Cloudy)
                audioSources[swap == true ? 0 : 1].volume = 0.25f;
            else if(GameManager.Instance.weatherState == WeatherManager.WeatherStates.Rainy)
                audioSources[swap == true ? 0 : 1].volume = 0.4f;
            else
                audioSources[swap == true ? 0 : 1].volume = 0.5f;
        }
        else
        {
            audioSources[swap == true ? 0 : 1].clip = stepDrySounds[Random.Range(0,stepDrySounds.Length)];
            audioSources[swap == true ? 0 : 1].volume = 0.3f;
        }
        audioSources[swap == true ? 0 : 1].pitch = Random.Range(0.85f, 1.15f);
        audioSources[swap == true ? 0 : 1].Play();
        swap = !swap;
    }
}
