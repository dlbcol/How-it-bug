using Unity.Netcode;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    // Floats for the player's movement.
    [SerializeField] private float speed = 5f;       // The speed at which the player walks.
    [SerializeField] private float sprint = 8f;      // The speed at which the player sprints.
    [SerializeField] private float jump = 2f;        // The height of the player's jump.
    [SerializeField] private float gravity = 20f;    // The gravity applied to the player.

    // Floats for the player's mouse movement.
    [SerializeField] private float sensitivity = 2f;         // The sensitivity of the player's mouse movement.
    [SerializeField] private Transform camera;                // The player's camera.
    [SerializeField] private float maxRotation = 90f;         // The maximum rotation of the player's camera.

    [SerializeField] private CharacterController controller;  // The player's character controller.
    [SerializeField] private float verticalVelocity;          // The player's vertical velocity.
    [SerializeField] private float rotationX;                 // The player's rotation on the X axis.

    /// <summary>
    /// Awake is called when the object is created.
    /// </summary>
    void Awake()
    {
        controller = GetComponent<CharacterController>();     // Get the CharacterController component attached to the player.
    }

    /// <summary>
    /// OnNetworkSpawn is called when the player is spawned by the NetworkManager.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (IsOwner)                                          // Checks if this player object belongs to this client.
        {
            camera.gameObject.SetActive(true);                // Enables the camera for the local player.

            Cursor.lockState = CursorLockMode.Locked;         // Lock the cursor to the center of the screen.
            Cursor.visible = false;                           // Hide the cursor.
        }
        else
        {
            camera.gameObject.SetActive(false);               // Disables cameras belonging to other players.
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        if (!IsOwner)                                         // Stops this client from controlling player objects it does not own.
        {
            return;
        }

        if (HowItBugSessionManager.PauseMenuOpen)            // Stops movement and camera controls while the pause menu is open.
        {
            return;
        }

        Move();
        Look();
    }

    /// <summary>
    /// Move calculates movement based on player input and applies it to the CharacterController.
    /// </summary>
    private void Move()
    {
        // Gets input from the player.
        float x = Input.GetAxis("Horizontal");                // Gets horizontal input.
        float z = Input.GetAxis("Vertical");                  // Gets vertical input.

        Vector3 move = transform.right * x + transform.forward * z; // Calculate the movement vector based on input.

        // Sprinting.
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprint : speed; // Ads sprinting by using the left shift key.

        // Keeps the player on the ground.
        if (controller.isGrounded)                            // Checks if the player is on the ground.
        {
            if (verticalVelocity < 0)                         // Checks if the player is falling.
            {
                verticalVelocity = -2f;                       // Sets the vertical velocity to a -2 to keep the player on the ground.
            }

            if (Input.GetButtonDown("Jump"))                  // Checks if the player jumping.
            {
                verticalVelocity = Mathf.Sqrt(jump * 2f * gravity); // Sets the vertical velocity to the jump height.
            }
        }

        // Gravity.
        verticalVelocity -= gravity * Time.deltaTime;         // Applies gravity to the vertical velocity.

        // Move.
        Vector3 velocity = move * currentSpeed;               // Calculates the velocity.
        velocity.y = verticalVelocity;                        // Sets the vertical velocity to the velocity.
        controller.Move(velocity * Time.deltaTime);           // Moves the player based on the velocity.
    }

    /// <summary>
    /// Look handles the player's camera rotation based on mouse input.
    /// </summary>
    private void Look()
    {
        // Takes mouse input from the player.
        float x = Input.GetAxis("Mouse X") * sensitivity;     // Gets the mouse X input and multiplies it by the sensitivity of the mouse.
        float y = Input.GetAxis("Mouse Y") * sensitivity;     // Gets the mouse Y input and multiplies it by the sensitivity of the mouse.

        // Rotate player left to right when turning the mouse left to right.
        transform.Rotate(Vector3.up * x);                     // Rotates the player on the Y axis based on the mouse X input.

        // Rotate camera up/down.
        rotationX -= y;                                       // Subtracts the mouse Y input from the rotationX.
        rotationX = Mathf.Clamp(rotationX, -maxRotation, maxRotation); // Makes sure doesn't rotate past the max rotation.
        camera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);    // Sets the camera's rotation based on the rotationX.
    }
}