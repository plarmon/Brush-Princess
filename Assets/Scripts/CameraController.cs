using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class CameraController : MonoBehaviour
{
    // [SerializeField] private Transform cameraPoint;
    [SerializeField] private GameObject playerObj;
    // [SerializeField] private float lookOffset;
    [SerializeField] private Player player;
    [SerializeField] private float lookSensitivity;

    [SerializeField] private int playerID;

    private void Awake()
    {
        player = ReInput.players.GetPlayer(playerID);
    }

    private void FixedUpdate()
    {
        transform.rotation *= Quaternion.AngleAxis(player.GetAxis("LookVertical") * lookSensitivity, Vector3.right);

        var angles = transform.localEulerAngles;
        angles.z = 0;

        var angle = transform.localEulerAngles.x;

        // Clamp the Up/Down rotation
        if (angle > 180 && angle < 340)
        {
            angles.x = 340;
        }
        else if (angle < 180 && angle > 40)
        {
            angles.x = 40;
        }

        transform.localEulerAngles = angles;
    }

}
