using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento hacia adelante")]
    [SerializeField] private float forwardSpeed = 12f;

    [Header("Carriles")]
    [SerializeField] private float laneDistance = 2.5f;
    [SerializeField] private float laneChangeSpeed = 15f;

    private int currentLane = 0;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 9f;
    [SerializeField] private float customGravity = -25f;

    private bool isGrounded = true;

    [Header("Disparo")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    [SerializeField] private float fireCooldown = 0.5f;

    private float nextFireTime = 0f;

    [Header("Estado")]
    [SerializeField] private bool isAlive = true;

    private int coinCount = 0;
    private Rigidbody rb;
    private float targetX = 0f;

  
    private Animator animator;
   

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;

       
        animator = GetComponent<Animator>();
        animator.SetBool("isWalking", true);
       
    }

    private void Update()
    {
        if (!isAlive) return;

        HandleLaneInput();
        HandleJumpInput();
        HandleShootInput();
    }

    private void FixedUpdate()
    {
        if (!isAlive) return;

        Vector3 velocity = rb.velocity;
        velocity.z = forwardSpeed;
        velocity.y += customGravity * Time.fixedDeltaTime;
        rb.velocity = velocity;

        Vector3 pos = rb.position;
        pos.x = Mathf.Lerp(pos.x, targetX, laneChangeSpeed * Time.fixedDeltaTime);
        rb.MovePosition(new Vector3(pos.x, rb.position.y, rb.position.z));
    }

    private void HandleLaneInput()
    {
        if (Input.GetKeyDown(KeyCode.A) && currentLane > -1)
        {
            currentLane--;
            targetX = currentLane * laneDistance;
        }
        else if (Input.GetKeyDown(KeyCode.D) && currentLane < 1)
        {
            currentLane++;
            targetX = currentLane * laneDistance;
        }
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Vector3 v = rb.velocity;
            v.y = jumpForce;
            rb.velocity = v;
            isGrounded = false;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayJumpSFX();
        }
    }

    private void HandleShootInput()
    {
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[PlayerController] falta poner el prefab de la bala.");
            return;
        }

        Vector3 spawnPos;
        Quaternion spawnRot;
        if (firePoint != null)
        {
            spawnPos = firePoint.position;
            spawnRot = firePoint.rotation;
        }
        else
        {
            spawnPos = transform.position + transform.forward * 0.8f + Vector3.up * 1.2f;
            spawnRot = Quaternion.LookRotation(Vector3.forward);
        }

        Instantiate(projectilePrefab, spawnPos, spawnRot);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSFX();
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }

        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            AddCoin(1);
            Destroy(other.gameObject);
        }
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0) return;
        coinCount += amount;
    }

    public int GetCoinCount()
    {
        return coinCount;
    }

    private void Die()
    {
        if (!isAlive) return;
        isAlive = false;

       
        animator.SetBool("isWalking", false);
        

        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
        else
            RestartGame();
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public bool IsAlive()
    {
        return isAlive;
    }
}