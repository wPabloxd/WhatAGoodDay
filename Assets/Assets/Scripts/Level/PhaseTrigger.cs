using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseTrigger : MonoBehaviour
{
    [SerializeField] protected WeatherManager.WeatherStates weatherState;
    [SerializeField] protected PlayerController.MovementPhases movementPhase;

    [SerializeField] protected GameObject weatherManagerObject;
    protected WeatherManager weatherManager;
    [SerializeField] protected GameObject PlayerControllerObject;
    protected PlayerController playerController;

    protected bool alreadyTriggered = false;
    void Start()
    {
        weatherManager = weatherManagerObject.GetComponent<WeatherManager>();
        playerController = PlayerControllerObject.GetComponent<PlayerController>();
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && !alreadyTriggered)
        {
            alreadyTriggered = true;
            weatherManager.UpdateWeatherState(weatherState, weatherManager.transitionTime);
            StartCoroutine(DelaySpeedUp(weatherManager.transitionTime));
        }
    }
    protected IEnumerator DelaySpeedUp(float delay) 
    {
        yield return new WaitForSeconds(delay);
        playerController.UpdateMovementPhase(movementPhase);
        gameObject.SetActive(false);
    }
}
