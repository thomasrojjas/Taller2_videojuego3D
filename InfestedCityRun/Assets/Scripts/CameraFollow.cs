using UnityEngine;

/// <summary>
/// Controls the camera behavior to follow the player.
/// Per requirements, it follows the player's forward progress (Z) and height (Y) 
/// but stays static horizontally (X).
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Target transform of the player to follow.")]
    public Transform target;
    [Tooltip("Vertical separation from the player.")]
    public float yOffset = 5f;
    [Tooltip("Backward separation from the player.")]
    public float zOffset = -8f;
    [Tooltip("Fixed horizontal position of the camera.")]
    public float fixedX = 0f;
    [Tooltip("Pitch angle of the camera looking down.")]
    public float pitchAngle = 20f;

    private void Start()
    {
        // Automatically find player if not assigned
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }

        // Align rotation looking down and forward
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Position camera: Z and Y follow player, X is fixed at center
        float targetY = target.position.y + yOffset;
        float targetZ = target.position.z + zOffset;

        transform.position = new Vector3(fixedX, targetY, targetZ);
    }
}
