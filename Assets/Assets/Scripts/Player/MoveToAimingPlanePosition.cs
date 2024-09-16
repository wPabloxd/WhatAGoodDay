using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveToAimingPlanePosition : MonoBehaviour
{
    [SerializeField] LayerMask layerMask = Physics.DefaultRaycastLayers;
    [SerializeField] Camera mainCamera;
    void Update()
    {
        Vector2 cursorPosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(cursorPosition);
        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point;
        }
    }
}
