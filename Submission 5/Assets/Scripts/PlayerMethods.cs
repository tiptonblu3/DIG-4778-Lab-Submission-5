using UnityEngine;

public class PlayerMethods : MonoBehaviour
{
    public PlayerInput playerInput; // Reference to the PlayerInput component of the player
    public Rigidbody rb;
    public int speed = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.playerInput = GetComponent<PlayerInput>(); // Get the PlayerInput component attached to the player GameObject
        this.rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
    }

    public void MovePlayer()
    {
        Vector3 moveDirection = new Vector3(playerInput.moveInput.x, 0, playerInput.moveInput.y); // Create a movement direction vector based on the player's input
        rb.linearVelocity = moveDirection.normalized * speed; // Set the player's Rigidbody linear velocity to move the player in the desired direction at the specified speed
    
    }
}
