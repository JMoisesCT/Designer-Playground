using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Sender Events")]
    [SerializeField] private VoidEventChannel _eventMovementScreenFinished;

    [Header("Listener Events")]
    [SerializeField] private VoidEventChannel _eventMovementStarted;

    private void Start()
    {
        
    }
}
