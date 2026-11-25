
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Movement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sprint = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Jumping & Gravity")]
    [SerializeField] private float jump = 2f;
    [SerializeField] private float gravity = 20f;

    [Header("Mouse Look")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private Transform camera;
    [SerializeField] private float maxRotation = 90f;

    [Header("Crouching")]
    [SerializeField] private float crouchingHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 8f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float spawnGroundCheckDistance = 1.5f;

    private CharacterController controller;

    private float verticalVelocity;
    private float rotationX;

    private float standingHeight;
    private Vector3 standingCenter;

    private float standingCameraY;
    private float crouchingCameraY;

    private bool isCrouching;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Use the CharacterController's actual original dimensions.
        standingHeight = controller.height;
        standingCenter = controller.center;

        if (camera != null)
        {
            standingCameraY = camera.localPosition.y;

            crouchingCameraY =
                standingCameraY - (standingHeight - crouchingHeight);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (camera != null)
            camera.gameObject.SetActive(IsOwner);

        if (!IsOwner)
            return;

        FixSpawnPosition();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner)
            return;

        if (HowItBugSessionManager.PauseMenuOpen)
            return;

        Look();
        Crouch();
        Move();
    }

    /// <summary>
    /// Corrects the player position if the CharacterController
    /// initially spawns partially inside the floor.
    /// </summary>
    private void FixSpawnPosition()
    {
        float bottomOffset =
            standingCenter.y - (standingHeight / 2f);

        float capsuleBottomY =
            transform.TransformPoint(
                new Vector3(0f, bottomOffset, 0f)
            ).y;

        Vector3 rayOrigin = transform.position +
            Vector3.up * standingHeight;

        float rayDistance =
            standingHeight + spawnGroundCheckDistance;

        RaycastHit[] hits = Physics.RaycastAll(
            rayOrigin,
            Vector3.down,
            rayDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        float closestFloorY = float.NegativeInfinity;

        foreach (RaycastHit hit in hits)
        {
            // Ignore colliders belonging to this player.
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            // Only consider upward-facing surfaces.
            if (hit.normal.y < 0.5f)
                continue;

            if (hit.point.y > closestFloorY)
                closestFloorY = hit.point.y;
        }

        if (closestFloorY == float.NegativeInfinity)
            return;

        float penetration = closestFloorY - capsuleBottomY;

        // Only correct an actual initial floor penetration.
        if (penetration > 0.01f)
        {
            controller.enabled = false;

            transform.position +=
                Vector3.up * penetration;

            controller.enabled = true;
        }
    }

    /// <summary>
    /// Handles walking, sprinting, jumping and gravity.
    /// </summary>
    private void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move =
            transform.right * x +
            transform.forward * z;

        // Prevent faster diagonal movement.
        move = Vector3.ClampMagnitude(move, 1f);

        float currentSpeed = speed;

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprint;
        }

        // Ground detection.
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        // Jump only while grounded and standing.
        if (controller.isGrounded &&
            !isCrouching &&
            Input.GetButtonDown("Jump"))
        {
            verticalVelocity =
                Mathf.Sqrt(jump * 2f * gravity);
        }

        // Apply gravity.
        verticalVelocity -= gravity * Time.deltaTime;

        Vector3 velocity = move * currentSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        // Stop upward velocity if we hit a ceiling.
        if ((controller.collisionFlags & CollisionFlags.Above) != 0 &&
            verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }
    }

    /// <summary>
    /// Handles horizontal player rotation and vertical camera rotation.
    /// </summary>
    private void Look()
    {
        if (camera == null)
            return;

        float x = Input.GetAxis("Mouse X") * sensitivity;
        float y = Input.GetAxis("Mouse Y") * sensitivity;

        // Rotate the player horizontally.
        transform.Rotate(Vector3.up * x);

        // Rotate the camera vertically.
        rotationX -= y;

        rotationX = Mathf.Clamp(
            rotationX,
            -maxRotation,
            maxRotation
        );

        camera.localRotation =
            Quaternion.Euler(rotationX, 0f, 0f);
    }

    /// <summary>
    /// Handles crouching differently depending on whether
    /// the player is grounded or airborne.
    ///
    /// Grounded: collider shrinks downward.
    /// Airborne: collider shrinks upward.
    /// </summary>
    private void Crouch()
    {
        bool wantsToCrouch =
            Input.GetKey(KeyCode.LeftControl);

        // Prevent standing up underneath obstacles.
        if (!wantsToCrouch && !CanStand())
        {
            wantsToCrouch = true;
        }

        isCrouching = wantsToCrouch;

        float targetHeight = isCrouching
            ? crouchingHeight
            : standingHeight;

        // Smoothly change collider height.
        float newHeight = Mathf.MoveTowards(
            controller.height,
            targetHeight,
            crouchTransitionSpeed * Time.deltaTime
        );

        // Difference between standing and current height.
        float heightDifference =
            standingHeight - newHeight;

        Vector3 newCenter = standingCenter;

        if (controller.isGrounded)
        {
            // GROUNDED:
            // Keep the bottom of the collider fixed.
            // The top moves downward.
            newCenter.y -= heightDifference / 2f;
        }
        else
        {
            // AIRBORNE:
            // Keep the top of the collider fixed.
            // The bottom moves upward.
            newCenter.y += heightDifference / 2f;
        }

        controller.height = newHeight;
        controller.center = newCenter;

        // Smoothly adjust camera height.
        if (camera != null)
        {
            Vector3 cameraPosition =
                camera.localPosition;

            float targetCameraY = isCrouching
                ? crouchingCameraY
                : standingCameraY;

            cameraPosition.y = Mathf.MoveTowards(
                cameraPosition.y,
                targetCameraY,
                crouchTransitionSpeed * Time.deltaTime
            );

            camera.localPosition = cameraPosition;
        }
    }

    /// <summary>
    /// Checks whether there is enough space to stand up.
    /// </summary>
    private bool CanStand()
    {
        if (controller.height >= standingHeight - 0.01f)
            return true;

        float radius = controller.radius * 0.95f;

        // Calculate the intended standing capsule.
        Vector3 worldCenter =
            transform.TransformPoint(standingCenter);

        float halfSegment =
            Mathf.Max(0f, standingHeight / 2f - radius);

        Vector3 bottomSphere =
            worldCenter - transform.up * halfSegment;

        Vector3 topSphere =
            worldCenter + transform.up * halfSegment;

        Collider[] overlaps = Physics.OverlapCapsule(
            bottomSphere,
            topSphere,
            radius,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in overlaps)
        {
            // Ignore our own colliders.
            if (hit.transform.IsChildOf(transform))
                continue;

            // Ignore surfaces below our standing capsule's feet.
            if (hit.bounds.max.y <=
                bottomSphere.y - radius + 0.02f)
                continue;

            return false;
        }

        return true;
    }
}
