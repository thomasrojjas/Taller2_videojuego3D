using UnityEngine;
using UnityEngine.SceneManagement;

// Lleva el control general del juego: el puntaje, las monedas y si perdimos o no.
public class GameManager : MonoBehaviour
{
    // instancia global para usar el GameManager desde otros scripts
    public static GameManager Instance { get; private set; }

    [Header("Jugador")]
    public Transform playerTransform; // referencia al jugador para medir la distancia

    [Header("Estado del juego")]
    public int score = 0;
    public int coins = 0;
    public bool IsGameOver { get; private set; } = false;

    private float playerStartX = 0f;
    private float playerStartZ = 0f;
    private int bonusScore = 0; // puntos extra por matar zombies o juntar monedas

    private void Awake()
    {
        // armamos el singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // reiniciamos todo al empezar
        IsGameOver = false;
        score = 0;
        coins = 0;
        bonusScore = 0;

        // si no asignaron el jugador lo buscamos por el tag
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        // guardamos donde empezo el jugador para calcular cuanto avanzo
        if (playerTransform != null)
        {
            playerStartX = playerTransform.position.x;
            playerStartZ = playerTransform.position.z;
        }

        // arrancamos la musica de fondo
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM();
        }
    }

    private void Update()
    {
        if (IsGameOver) return;

        // el puntaje sube segun lo que el jugador avanza en Z
        if (playerTransform != null)
        {
            float distanceTraveled = playerTransform.position.z - playerStartZ;
            // 10 puntos por cada unidad avanzada, mas los puntos extra
            score = Mathf.FloorToInt(distanceTraveled * 10f) + bonusScore;
        }
    }

    // suma una moneda
    public void AddCoin()
    {
        if (IsGameOver) return;
        coins++;
    }

    // suma puntos extra (por matar zombies o juntar monedas)
    public void AddScore(int points)
    {
        if (IsGameOver) return;
        bonusScore += points;
    }

    // se llama cuando el jugador pierde
    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        // sonido de derrota y paramos la musica
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSFX();
        }

        // mostramos el menu de derrota
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDefeatPanel();
        }

        Debug.Log("Game Over! Puntaje final: " + score + ", Monedas: " + coins);
    }

    // reinicia la partida volviendo a cargar la escena (todo vuelve a cero)
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
