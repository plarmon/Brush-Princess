using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintDoor : MonoBehaviour
{
    [SerializeField] private Transform toPoint;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera doorCam;
    [SerializeField] private GameObject door;
    [SerializeField] private float paintLimit;
    [SerializeField] private float doorSpeed;
    public bool opened = false;
    private GameManager gm;

    private void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void OpenDoor()
    {
        doorCam.Priority = 100;
        opened = true;
        StartCoroutine(DoorTransition());
    }

    private IEnumerator DoorTransition()
    {
        Vector3 startPosition = door.transform.position;
        float lerpInc = 0.0f;
        while(lerpInc <= 1)
        {
            lerpInc += doorSpeed * Time.deltaTime;
            door.transform.position = Vector3.Lerp(startPosition, toPoint.position, lerpInc);
            yield return null;
        }
        doorCam.Priority = 1;
        gm.player.state = PlayerController.States.IDLE;
        gm.player.Pause(false);
    }

    public float GetLimit()
    {
        return paintLimit;
    }
}
