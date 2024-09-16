using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

public class Crawler : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    [SerializeField] GameObject mesh;
    GameObject player;
    GameObject playerMesh;
    bool attacking;

    [SerializeField] float angleThreshold = 10f;
    [SerializeField] float timeToBeDefeated = 0.5f;
    [SerializeField] float timeBeforeAttack = 3f;
    [SerializeField] float distanceBeforeAttack = 2f;

    AudioSource audioSource;
    [SerializeField] AudioClip[] defeatSound;

    private bool defeated;
    float timeHoldingAttack = 0f;
    float timeInLight = 0f;
    void Start()
    {
        player = GameObject.Find("Player");
        playerMesh = GameObject.Find("Woman");
        audioSource = GetComponent<AudioSource>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }
    void Update()
    {
        Movement();
        if (!defeated)
        {
            CheckLight();
        }
        if(GameManager.Instance.weatherState != WeatherManager.WeatherStates.Blood)
        {
            Destroy(gameObject);
        }
    }
    void Movement()
    {
        if (defeated)
        {
            Vector3 directionToPlayer = (transform.position - player.transform.position).normalized;
            Vector3 oppositePosition = transform.position + directionToPlayer * 4;
            Vector3 lookDirection = player.transform.position - transform.position;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                mesh.transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 100);
            }

            navMeshAgent.SetDestination(oppositePosition);
        }
        else if (attacking)
        {
            navMeshAgent.SetDestination(player.transform.position);
        }
        else
        {
            Vector3 directionToPlayer = player.transform.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer > distanceBeforeAttack)
            {
                Vector3 direction = directionToPlayer.normalized;
                Vector3 targetPosition = player.transform.position - direction * distanceBeforeAttack;
                navMeshAgent.SetDestination(targetPosition);
            }

            if (distanceToPlayer <= distanceBeforeAttack + 0.7f)
            {
                HoldAttack();
            }
            else
            {
                timeHoldingAttack = 0f;
            }
        }
    }
    void CheckLight()
    {
        Vector3 directionToCrawler = (transform.position - playerMesh.transform.position).normalized;
        Vector3 playerForward = playerMesh.transform.forward;
        float angle = Vector3.Angle(playerForward, directionToCrawler);

        if (angle <= angleThreshold)
        {
            if(timeInLight >= timeToBeDefeated)
            {
                Defeated();
            }
            timeInLight += Time.deltaTime;
        }
        else
        {
            timeInLight = 0;
        }
    }
    void HoldAttack()
    {
        timeHoldingAttack += Time.deltaTime;
        if(timeHoldingAttack > timeBeforeAttack || player.GetComponent<PlayerController>().lastVelocity == Vector3.zero)
        {
            attacking = true;
        }
    }
    void Defeated()
    {
        audioSource.loop = false;
        audioSource.clip = defeatSound[Random.Range(0, defeatSound.Length)];
        audioSource.Play();
        defeated = true;
        animator.SetBool("defeated", true);
        navMeshAgent.speed = 1.5f;
        StartCoroutine(DestroyDelay());
    }
    IEnumerator DestroyDelay()
    {
        yield return new WaitForSeconds(6);
        Destroy(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(attacking && other.tag == "Player")
        {
            GameManager.Instance.PlayerRespawn();
        }
    }
}