using UnityEngine;

// Maneja la moneda: la hace girar y detecta cuando el jugador la toca.
public class Coin : MonoBehaviour
{
    public float rotationSpeed = 100f; // velocidad a la que gira la moneda
    public int scorePoints = 50;       // puntos que da al recogerla

    private void Update()
    {
        // hacemos girar la moneda en el eje Y para que se vea bonita
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // revisamos si lo que toco la moneda es el jugador
        if (other.gameObject.CompareTag("Player"))
        {
            // sumamos la moneda y los puntos
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoin();
                GameManager.Instance.AddScore(scorePoints);
            }

            // sonido de moneda
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoinSFX();
            }

            // la moneda se borra porque ya la recogimos
            Destroy(gameObject);
        }
    }
}
