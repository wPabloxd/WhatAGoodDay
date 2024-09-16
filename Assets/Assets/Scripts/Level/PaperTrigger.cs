using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaperTrigger : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Sprite thisPaper;
    [SerializeField] Color paperColor;
    AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            audioSource.Play();
            image.gameObject.SetActive(true);
            image.sprite = thisPaper;
            image.color = paperColor;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            image.gameObject.SetActive(false);
        }
    }
}
