using UnityEngine;

/// <summary>
/// Controls the bullet projectile fired by the player.
/// Moves straight forward and handles impact with enemies and obstacles.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Tooltip("Speed of the projectile.")]
    public float speed = 25f;
    [Tooltip("Maximum lifetime of the projectile in seconds before automatic destruction.")]
    public float lifetime = 3f;

    private void Start()
    {
        // Auto-destruct after lifetime expires to clean up memory
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move straight forward relative to its orientation
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private Transform FindTaggedParent(GameObject obj, string tagToFind)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.CompareTag(tagToFind))
            {
                return current;
            }
            current = current.parent;
        }
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform enemyTransform = FindTaggedParent(other.gameObject, "Enemy");
        if (enemyTransform != null)
        {
            // Destroy the zombie root object
            Destroy(enemyTransform.gameObject);

            // Play enemy death SFX
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayZombieDeathSFX();
            }

            // Award points for killing an enemy
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(150); // 150 bonus points
            }

            // Destroy the bullet itself
            Destroy(gameObject);
            return;
        }

        Transform obstacleTransform = FindTaggedParent(other.gameObject, "Obstacle");
        if (obstacleTransform != null)
        {
            // Bullets are absorbed by obstacles
            Destroy(gameObject);
        }
    }
}
