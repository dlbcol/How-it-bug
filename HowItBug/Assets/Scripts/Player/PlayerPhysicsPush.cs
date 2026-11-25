using UnityEngine;

public class PlayerPhysicsPush : MonoBehaviour
{
    [SerializeField] private float pushForce = 1.5f;
    [SerializeField] private LayerMask pushableLayers;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if ((pushableLayers.value & (1 << hit.gameObject.layer)) == 0)
            return;

        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic)
            return;

        Vector3 pushDirection = new Vector3(
            hit.moveDirection.x,
            0f,
            hit.moveDirection.z
        );

        body.AddForce(pushDirection * pushForce, ForceMode.Impulse);
    }
}