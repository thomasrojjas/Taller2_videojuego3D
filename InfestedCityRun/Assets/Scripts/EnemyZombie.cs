using UnityEngine;

/// <summary>
/// Controls the Zombie enemy behaviour.
/// Zombies spawn in a lane and move continuously in the opposite direction of the player.
/// They are tall enough (visually and physically) that they cannot be jumped over.
/// </summary>
public class EnemyZombie : MonoBehaviour
{
    [Tooltip("Movement speed of the zombie towards the screen/player.")]
    public float speed = 3f;

    private Animator animator;
    private Rigidbody rb;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Configure Animator to play the running animation
        if (animator != null)
        {
            // Zombies move with a slightly randomized speed multiplier for visual variety
            animator.SetFloat("SpeedMultiplier", Random.Range(0.8f, 1.2f));
        }

        // Scale up collider to ensure the zombie CANNOT be jumped over
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider != null)
        {
            // Increase height and center to make it tall enough
            collider.height = 4f;
            collider.center = new Vector3(collider.center.x, 2f, collider.center.z);
        }
        else
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.size = new Vector3(boxCollider.size.x, 4f, boxCollider.size.z);
                boxCollider.center = new Vector3(boxCollider.center.x, 2f, boxCollider.center.z);
            }
        }
    }

    private void Update()
    {
        // Stop moving if game is over
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        // Move straight backwards along the Z-axis (towards the player)
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
