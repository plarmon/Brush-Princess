using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float pushForce;
    [SerializeField] private float turnSpeed;

    [SerializeField] private float dragForce;
    [SerializeField] private float gravity;

    [SerializeField] private int playerID = 0;
    [SerializeField] private Player player;

    private CharacterController controller;

    private float directionY;

    public bool grounded;

    private void Awake()
    {
        player = ReInput.players.GetPlayer(playerID);
        controller = GetComponent<CharacterController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        grounded = false;
    }

    private void FixedUpdate()
    {
        float moveHorizontal = player.GetAxis("Move Horizontal");
        float moveVertical = player.GetAxis("Move Vertical");

        transform.Rotate(0, moveHorizontal * turnSpeed, 0, Space.Self);

        Vector3 movedir = transform.forward * moveVertical;

        if (player.GetButtonDown("Jump") && controller.isGrounded)
        {
            Debug.Log("Jump");
            directionY = jumpForce;
        }

        if(player.GetButtonDown("Push") && controller.isGrounded)
        {
            // controller.Move(transform.forward * pushForce * Time.deltaTime);
            movedir += (transform.forward * pushForce);
            
        }
        
        directionY -= gravity;
        movedir.y = directionY;


        controller.Move(movedir * speed * Time.deltaTime);
    }

    

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Floor"))
        {
            controller.Move(transform.forward * pushForce * Time.deltaTime);
        }
    }
}
