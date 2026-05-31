using UnityEngine;

/// <summary>
/// Controls the collectible coin behavior: rotation and trigger collision detection.
/// </summary>
public class Coin : MonoBehaviour
{
    [Tooltip("Rotation speed of the coin in degrees per second.")]
    public float rotationSpeed = 100f;
    [Tooltip("Amount of points awarded when collected.")]
    public int scorePoints = 50;

    private void Update()
    {
        // Rotate around Y-axis for visual appeal
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if collided with the Player
        if (other.gameObject.CompareTag("Player"))
        {
            // Award coin and score
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoin();
                GameManager.Instance.AddScore(scorePoints);
            }

            // Play coin pickup sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoinSFX();
            }

            // Self destruct
            Destroy(gameObject);
        }
    }
}
