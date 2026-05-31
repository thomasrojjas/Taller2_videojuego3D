using UnityEngine;

// Script principal del jugador.
// El jugador avanza solo hacia adelante sin parar, se puede mover entre 3 carriles
// con A y D, saltar con espacio y disparar con la tecla F.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento hacia adelante")]
    [SerializeField] private float forwardSpeed = 12f; // velocidad fija hacia adelante (Z)

    [Header("Carriles")]
    [SerializeField] private float laneDistance = 2.5f;   // separacion entre carriles
    [SerializeField] private float laneChangeSpeed = 15f; // que tan rapido se cambia de carril

    // carril actual: -1 izquierda, 0 centro, 1 derecha
    private int currentLane = 0;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 9f;       // fuerza del salto
    [SerializeField] private float customGravity = -25f; // gravedad propia (cae mas rapido, se siente arcade)

    private bool isGrounded = true; // si esta tocando el piso

    [Header("Disparo")]
    public GameObject projectilePrefab;  // la bala que dispara
    public Transform firePoint;          // de donde sale la bala
    [SerializeField] private float fireCooldown = 0.5f; // tiempo minimo entre disparos

    private float nextFireTime = 0f; // cuando va a poder volver a disparar

    [Header("Estado")]
    [SerializeField] private bool isAlive = true; // si esta vivo

    private int coinCount = 0; // monedas que lleva
    private Rigidbody rb;
    private float targetX = 0f; // X a la que se quiere mover segun el carril

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // que no se de vuelta al chocar
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false; // usamos nuestra propia gravedad
    }

    private void Update()
    {
        if (!isAlive) return;

        // revisamos las teclas cada frame
        HandleLaneInput();
        HandleJumpInput();
        HandleShootInput();
    }

    private void FixedUpdate()
    {
        if (!isAlive) return;

        // 1) lo empujamos siempre hacia adelante
        Vector3 velocity = rb.velocity;
        velocity.z = forwardSpeed;

        // 2) le aplicamos la gravedad
        velocity.y += customGravity * Time.fixedDeltaTime;

        rb.velocity = velocity;

        // 3) lo movemos de a poco hacia el carril que toca (eje X)
        Vector3 pos = rb.position;
        pos.x = Mathf.Lerp(pos.x, targetX, laneChangeSpeed * Time.fixedDeltaTime);
        rb.MovePosition(new Vector3(pos.x, rb.position.y, rb.position.z));
    }

    // A = izquierda, D = derecha
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

    // espacio para saltar (solo si esta en el piso)
    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Vector3 v = rb.velocity;
            v.y = jumpForce;
            rb.velocity = v;
            isGrounded = false;

            // sonido de salto
            if (AudioManager.Instance != null) AudioManager.Instance.PlayJumpSFX();
        }
    }

    // F para disparar, respetando el tiempo de espera
    private void HandleShootInput()
    {
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    // crea la bala
    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[PlayerController] falta poner el prefab de la bala.");
            return;
        }

        // si hay un firePoint usamos ese, si no, disparamos desde el jugador
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
            spawnRot = Quaternion.LookRotation(Vector3.forward); // dispara hacia adelante
        }

        Instantiate(projectilePrefab, spawnPos, spawnRot);

        // sonido del disparo
        if (AudioManager.Instance != null) AudioManager.Instance.PlayShootSFX();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // si chocamos algo desde arriba, quiere decir que tocamos el piso
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }

        // si chocamos un zombie o un obstaculo, perdemos
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // si pasamos por una moneda la juntamos
        if (other.CompareTag("Coin"))
        {
            AddCoin(1);
            Destroy(other.gameObject);
        }
    }

    // suma monedas al jugador
    public void AddCoin(int amount)
    {
        if (amount <= 0) return;
        coinCount += amount;
    }

    // devuelve cuantas monedas lleva
    public int GetCoinCount()
    {
        return coinCount;
    }

    // se llama cuando el jugador muere
    private void Die()
    {
        if (!isAlive) return;
        isAlive = false;

        // avisamos al GameManager para que muestre el menu de derrota
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            // por las dudas, si no hay GameManager reiniciamos directo
            RestartGame();
        }
    }

    // reinicia la partida volviendo a cargar la escena
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    // dice si el jugador sigue vivo
    public bool IsAlive()
    {
        return isAlive;
    }
}
