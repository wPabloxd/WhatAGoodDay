using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{

    public enum MovementPhases
    {
        Static,
        Walk,
        FastWalk,
        Run,
        Sprint,
    }
    [Header("Mesh")]
    [SerializeField] GameObject mesh;
    [SerializeField] GameObject flashlight;

    [Header("Movement Settings")]
    [SerializeField] float planeSpeed = 3f;
    [SerializeField] float gravity = -9.8f;
    [SerializeField] bool paused;

    [Header("Orientation Settings")]
    [SerializeField] float angularSpeed = 10f;
    [SerializeField] Transform orientationTarget;

    [Header("Movement Inputs")]
    [SerializeField] InputActionReference move;

    [Header("Animation")]
    [SerializeField] float transitionVelocity = 10f;

    [Header("Other")]
    public bool alive = true;
    [SerializeField] GameObject respawnPoint;

    AudioSource audioSource;
    Animator animator;
    CharacterController characterController;
    Vector3 velocityToApply = Vector3.zero;
    float verticalVelocity = 0f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();
    }
    private void Start()
    {
        UpdateMovementPhase(MovementPhases.Walk); 
    }
    private void OnEnable()
    {
        move.action.Enable();
    }
    void Update()
    {
        if (!GameManager.isPaused)
        {
            velocityToApply = Vector3.zero;
            UpdateMovementOnPlane();
            UpdateVerticalMovement();
            UpdateAnimation();
            if(characterController.enabled)
                characterController.Move(velocityToApply * Time.deltaTime);
        }
    }
    public Vector3 lastVelocity = Vector3.zero;
    Vector3 lastPosition = Vector3.zero;
    public Vector3 direction = Vector3.zero;
    private void UpdateMovementOnPlane()
    {
        Vector3 rawMoveValue = new Vector3(move.action.ReadValue<Vector2>().x, 0, move.action.ReadValue<Vector2>().y);

        animator.SetBool("isMoving", rawMoveValue == Vector3.zero ? false : true);

        Vector3 velocity = rawMoveValue * planeSpeed;
        velocityToApply += velocity;

        Vector3 desiredDirection = orientationTarget.transform.position - transform.position;
        desiredDirection.y = 0;
        desiredDirection = desiredDirection.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);
        mesh.transform.rotation = Quaternion.Lerp(mesh.transform.rotation, targetRotation, Time.deltaTime * angularSpeed);

        if(lastPosition != transform.position)
        {
            direction = lastPosition - transform.position;
            direction = direction.normalized;
        }

        lastVelocity = velocity;
        lastPosition = transform.position;
    }
    Vector3 smoothedAnimationVelocity = Vector3.zero;
    private void UpdateAnimation()
    {
        Vector3 velocityDistance = lastVelocity - smoothedAnimationVelocity;
        float transitionVelocityToApply = transitionVelocity * Time.deltaTime;

        transitionVelocityToApply = Mathf.Min(transitionVelocityToApply, velocityDistance.magnitude);
        smoothedAnimationVelocity += velocityDistance.normalized * transitionVelocityToApply;

        Vector3 localSmoothedAnimationVelocity = mesh.transform.InverseTransformDirection(smoothedAnimationVelocity);
        localSmoothedAnimationVelocity = localSmoothedAnimationVelocity.normalized;

        animator.SetFloat("MovX", localSmoothedAnimationVelocity.x);
        animator.SetFloat("MovY", localSmoothedAnimationVelocity.z);
    }
    private void UpdateVerticalMovement()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = 0f;
        }
        verticalVelocity += gravity * Time.deltaTime;
        velocityToApply += verticalVelocity * Vector3.up;
    }
    public void UpdateMovementPhase(MovementPhases newPhase)
    {
        switch (newPhase)
        {
            case MovementPhases.Static:
                planeSpeed = 0f;
                angularSpeed = 0f;
                animator.SetBool("isRunning", false);
                animator.speed = 1f;
                break;
            case MovementPhases.Walk: 
                planeSpeed = 1.6f;
                angularSpeed = 10f;
                animator.SetBool("isRunning", false);
                animator.speed = 1f;
                break;
            case MovementPhases.FastWalk:
                planeSpeed = 2.2f;
                angularSpeed = 10f;
                animator.SetBool("isRunning", false);
                animator.speed = 1.5f;
                break;
            case MovementPhases.Run:
                planeSpeed = 3f;
                angularSpeed = 10f;
                animator.SetBool("isRunning", true);
                animator.speed = 1f;
                break;
            case MovementPhases.Sprint: 
                planeSpeed = 4f;
                angularSpeed = 10f;
                animator.SetBool("isRunning", true);
                animator.speed = 1.33f;
                break;
        }
    }
    public void ToggleFlashlight(bool newStatus)
    {
        flashlight.SetActive(newStatus);
        animator.SetLayerWeight(1, newStatus == true ? 1f : 0f);
    }
    public void GoToRespawnPoint()
    {
        audioSource.Play();
        characterController.enabled = false;
        transform.position = respawnPoint.transform.position;
    }
    public void EnableCharacterController() 
    {
        characterController.enabled = true;
    }
    public void GameFinished()
    {
        animator.SetBool("end", true);
        UpdateMovementPhase(MovementPhases.Static);
        StartCoroutine(GoToMenuDelay());
    }
    IEnumerator GoToMenuDelay()
    {
        yield return new WaitForSeconds(4f);
        GameManager.LoadScene(0);
    }
    private void OnDisable()
    {
        move.action.Disable();
    }
}