using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyTrigger : MonoBehaviour
{
    [SerializeField] int newDifficulty;
    [SerializeField] GameObject crawlerManager;
    [SerializeField] bool deactivate;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            crawlerManager.GetComponent<CrawlerInstantiator>().difficulty = newDifficulty;
            if(deactivate)
                gameObject.SetActive(false);
        }
    }
}
