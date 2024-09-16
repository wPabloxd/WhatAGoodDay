using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlerSoundStorm : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            StartCoroutine(DelaySound());
        }
    }
    IEnumerator DelaySound()
    {
        yield return new WaitForSeconds(1.8f);
        audioSource.Play();
    }
}
