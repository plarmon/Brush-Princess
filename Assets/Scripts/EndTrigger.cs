using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    [SerializeField] private Cinemachine.CinemachineVirtualCamera endCam;
    [SerializeField] private Transform toPosition;
    [SerializeField] private float launchSpeed;
    [SerializeField] private string sceneName;
    private GameManager gm;

    private void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the player collides with the end trigger then ennd the level
        if(other.gameObject.CompareTag("Player"))
        {
            // End Level
            gm.EndLevel(toPosition, launchSpeed, sceneName);
            // Transitions to the end level cam
            Debug.Log("Hit endCam");
            endCam.Priority = 100;
        }
    }
}
