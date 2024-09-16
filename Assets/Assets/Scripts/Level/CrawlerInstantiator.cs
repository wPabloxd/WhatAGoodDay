using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

public class CrawlerInstantiator : MonoBehaviour
{
    PlayerController playerController;
    [SerializeField] GameObject playerControllerObject;
    [SerializeField] GameObject crawlerPrefab;
    [SerializeField] float maxDistance = 20f;
    [SerializeField] float minDistance = 10f;
    [SerializeField] GameObject weatherManagerObject;
    public bool bloodStormActive;
    public int difficulty = 1;
    void Start()
    {
        playerController = playerControllerObject.GetComponent<PlayerController>();
    }
    void Update()
    {
       
    }
    public void StartAttack()
    {
        StartCoroutine(SpawnerByTime());
    }
    IEnumerator SpawnerByTime()
    {
        while (true)
        {
            float randomDelay = Random.Range(2f, 4f);
            yield return new WaitForSeconds(randomDelay);
            if (!bloodStormActive)
                break;
            StartCoroutine(SpawnCrawlers(difficulty));
        }
    }
    IEnumerator SpawnCrawlers(int amount)
    {
        StartCoroutine(weatherManagerObject.GetComponent<WeatherManager>().Lightning());
        Vector3[] spawnPoints = new Vector3[amount];
        for (int i = 0; i < amount; i++)
        {
            spawnPoints[i] = GetRandomSpawnPoint();
            Instantiate(crawlerPrefab, spawnPoints[i], Quaternion.identity);
            yield return new WaitForEndOfFrame();
        }
    }
    Vector3 GetRandomSpawnPoint()
    {
        float randomAngle = Random.Range(-240 / 2, 240 / 2) * Mathf.Deg2Rad;
        //float randomAngle = Random.Range(0, 360) * Mathf.Deg2Rad;
        float randomDistance = Random.Range(minDistance, maxDistance);

        Vector3 randomDirection = Quaternion.Euler(0, randomAngle * Mathf.Rad2Deg, 0) * playerController.direction;
        Vector3 randomPoint = playerControllerObject.transform.position + randomDirection * randomDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 100f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return randomPoint;
    }
}
