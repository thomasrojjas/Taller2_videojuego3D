using UnityEngine;

// Controla al zombie. El zombie avanza solo en linea recta hacia el jugador
// y es lo suficientemente alto como para que no se pueda saltar.
public class EnemyZombie : MonoBehaviour
{
    public float speed = 3f; // velocidad con la que se mueve hacia el jugador

    private Animator animator;
    private Rigidbody rb;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        // el zombie no usa fisica de gravedad, lo movemos nosotros a mano
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // le ponemos una velocidad de animacion un poco distinta a cada uno
        // asi no se mueven todos exactamente igual
        if (animator != null)
        {
            animator.SetFloat("SpeedMultiplier", Random.Range(0.8f, 1.2f));
        }

        // agrandamos el collider para que el zombie NO se pueda saltar
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider != null)
        {
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
        // si el juego ya termino el zombie se queda quieto
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        // avanza derecho hacia adelante (hacia el jugador)
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
