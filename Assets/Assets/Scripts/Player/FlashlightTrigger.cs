using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WeatherManager;

public class FlashlightTrigger : PhaseTrigger
{
    [SerializeField] GameObject blocker;
    [SerializeField] GameObject crawlerManager;
    [SerializeField] bool activateFlashlight;

    private void OnEnable()
    {
        blocker.SetActive(activateFlashlight);
        alreadyTriggered = false;
    }
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !alreadyTriggered)
        {
            alreadyTriggered = true;
            weatherManager.UpdateWeatherState(weatherState, weatherManager.transitionTime);
            StartCoroutine(DelayLightOn());
        }
    }
    IEnumerator DelayLightOn()
    {
        Debug.Log("1");
        yield return new WaitForEndOfFrame();
        Debug.Log("2");
        crawlerManager.GetComponent<CrawlerInstantiator>().bloodStormActive = activateFlashlight;
        crawlerManager.GetComponent<CrawlerInstantiator>().StartAttack();
        yield return new WaitForEndOfFrame();
        Debug.Log("3");
        playerController.ToggleFlashlight(activateFlashlight);
        yield return new WaitForEndOfFrame();
        Debug.Log("4");
        blocker.SetActive(!activateFlashlight);
        yield return new WaitForEndOfFrame();
        playerController.UpdateMovementPhase(movementPhase);
        gameObject.SetActive(false);
        Debug.Log("LLEGA");
    }
}
