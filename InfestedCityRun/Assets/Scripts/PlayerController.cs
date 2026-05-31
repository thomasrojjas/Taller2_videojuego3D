using UnityEngine;

/// <summary>
/// Controls the player character: forward running, jumping, lane-switching, and shooting projectiles.
/// Uses direct velocity control for stable physical movement.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Initial forward speed of the player.")]
    public float forwardSpeed = 8f;
    [Tooltip("Rate at which the forward speed increases per second.")]
    public float speedIncreaseRate = 0.05f;
    [Tooltip("Maximum forward speed the player can reach.")]
    public float maxSpeed = 20f;
    [Tooltip("Force applied when jumping.")]
    public float jumpForce = 8f;
    
    [Header("Lane Settings")]
    [Tooltip("Distance between the centers of adjacent lanes.")]
    public float laneDistance = 3f;
    [Tooltip("How fast the player shifts horizontally between lanes.")]
    public float laneChangeSpeed = 10f;

    [Header("Shooting Settings")]
    [Tooltip("Bullet prefab to spawn when shooting.")]
    public GameObject projectilePrefab;
    [Tooltip("Cooldown between consecutive shots in seconds.")]
    public float shootCooldown = 0.5f;
    [Tooltip("Vertical and forward offset relative to player transform to spawn the bullet.")]
    public Vector3 spawnOffset = new Vector3(0f, 1.2f, 1.2f);

    // Dynamic state variables
    private int currentLane = 0; // -1: Left, 0: Center, 1: Right
    private float nextShootTime = 0f;
    private bool isGrounded = true;

    // Component references
    private Rigidbody rb;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        // Ensure Rigidbody settings are correct for stable infinite running
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        // Don't update player logic if game is over
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        // Increase speed over time
        forwardSpeed = Mathf.Min(forwardSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);

        // Check horizontal input
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentLane > -1)
            {
                currentLane--;
            }
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentLane < 1)
            {
                currentLane++;
            }
        }

        // Check jump input
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded)
        {
            Jump();
        }

        // Check shoot input
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextShootTime)
        {
            Shoot();
        }

        // Update animator parameters
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("SpeedMultiplier", forwardSpeed / 8f);
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        // Calculate target horizontal X position
        float targetX = currentLane * laneDistance;
        
        // Calculate X velocity needed to switch lanes smoothly
        float xDiff = targetX - rb.position.x;
        float xVelocity = xDiff * laneChangeSpeed;

        // Constant forward velocity
        float zVelocity = forwardSpeed;

        // Keep the falling/gravity velocity from physics
        float yVelocity = rb.velocity.y;

        // Apply velocities directly for smooth physical movement
        rb.velocity = new Vector3(xVelocity, yVelocity, zVelocity);

        // Solid ground check based on player absolute Y coordinate
        isGrounded = (rb.position.y <= 0.25f);
    }

    private bool CompareTagInHierarchy(GameObject obj, string tagToCompare)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.CompareTag(tagToCompare))
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        isGrounded = false;
        
        if (animator != null)
        {
            animator.SetTrigger("JumpTrigger");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayJumpSFX();
        }
    }

    private void Shoot()
    {
        nextShootTime = Time.time + shootCooldown;
        
        // Calculate shoot position in front of player
        Vector3 shootPos = transform.position + transform.right * spawnOffset.x + transform.up * spawnOffset.y + transform.forward * spawnOffset.z;
        Instantiate(projectilePrefab, shootPos, transform.rotation);

        if (animator != null)
        {
            animator.SetTrigger("ShootTrigger");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShootSFX();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (CompareTagInHierarchy(collision.gameObject, "Enemy") || CompareTagInHierarchy(collision.gameObject, "Obstacle"))
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CompareTagInHierarchy(other.gameObject, "Enemy") || CompareTagInHierarchy(other.gameObject, "Obstacle"))
        {
            Die();
        }
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        if (animator != null)
        {
            animator.SetTrigger("DieTrigger");
        }
    }
}
