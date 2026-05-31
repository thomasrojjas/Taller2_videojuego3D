using UnityEngine;

// Es la bala que dispara el jugador. Avanza derecho hacia adelante sin caerse
// (no le afecta la gravedad) y se borra cuando choca con un zombie o un obstaculo.
public class Projectile : MonoBehaviour
{
    public float speed = 25f;    // velocidad de la bala
    public float lifetime = 3f;  // segundos antes de que se borre sola

    // para que la bala solo le pegue a UNA cosa y no cuente el golpe dos veces
    private bool hasHit = false;

    private void Start()
    {
        // si no le pega a nada, igual se borra despues de un rato
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // movemos la bala hacia adelante todo el tiempo
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // busca si el objeto (o su "papa") tiene cierto tag
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
        // si ya le pego a algo no hacemos nada mas
        if (hasHit) return;

        // si choco con un enemigo (zombie)
        Transform enemyTransform = FindTaggedParent(other.gameObject, "Enemy");
        if (enemyTransform != null)
        {
            hasHit = true;

            // sonido de muerte del zombie
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayZombieDeathSFX();
            }

            // damos puntos por matarlo
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(150);
            }

            // borramos al zombie y a la bala
            Destroy(enemyTransform.gameObject);
            Destroy(gameObject);
            return;
        }

        // si choco con un obstaculo, la bala simplemente se destruye
        Transform obstacleTransform = FindTaggedParent(other.gameObject, "Obstacle");
        if (obstacleTransform != null)
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }
}
