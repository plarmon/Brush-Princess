using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    private GameManager gm;
    private PlayerController player;

    private void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // If there is a collision with the player then respawn then fade out and respawn at a given point
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().Pause(true);
            gm.ui.StartCoroutine(gm.ui.FadeOutRespawn(respawnPoint));
        }
    }
}
