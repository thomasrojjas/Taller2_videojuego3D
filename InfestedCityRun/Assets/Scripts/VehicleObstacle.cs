using UnityEngine;

/// <summary>
/// Identifies static obstacles like vehicles that the player can jump over.
/// Automatically adjusts collider properties to ensure they are jumpable.
/// </summary>
public class VehicleObstacle : MonoBehaviour
{
    [Tooltip("Is this obstacle a vehicle? (Used for custom physics or height adjustments)")]
    public bool isVehicle = true;

    private void Start()
    {
        // Configure collider to ensure it is jumpable
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            // Standard jump height allows jumping over colliders of height <= 1.5 units
            boxCollider.size = new Vector3(boxCollider.size.x, Mathf.Min(boxCollider.size.y, 1.4f), boxCollider.size.z);
            boxCollider.center = new Vector3(boxCollider.center.x, boxCollider.size.y / 2f, boxCollider.center.z);
        }
        else
        {
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                capsuleCollider.height = Mathf.Min(capsuleCollider.height, 1.4f);
                capsuleCollider.center = new Vector3(capsuleCollider.center.x, capsuleCollider.height / 2f, capsuleCollider.center.z);
            }
        }
    }
}
