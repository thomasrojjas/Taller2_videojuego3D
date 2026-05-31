using UnityEngine;
using UnityEngine.UI;

// Maneja los textos de la pantalla (puntaje y monedas) y el menu de derrota.
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Textos de arriba (HUD)")]
    public Text scoreText;  // texto del puntaje
    public Text coinsText;  // texto de las monedas

    [Header("Menu de derrota")]
    public GameObject defeatPanel;   // el panel que aparece al perder
    public Text finalScoreText;      // puntaje final
    public Text finalCoinsText;      // monedas finales
    public Button restartButton;     // boton para reiniciar

    private void Awake()
    {
        // singleton
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
        // escondemos el menu de derrota al empezar
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }

        // conectamos el boton de reiniciar
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // actualizamos el puntaje (ej: PUNTUACIÓN: 00000345)
        if (scoreText != null)
        {
            scoreText.text = "PUNTUACIÓN: " + GameManager.Instance.score.ToString("D8");
        }

        // actualizamos las monedas (ej: MONEDAS: 09)
        if (coinsText != null)
        {
            coinsText.text = "MONEDAS: " + GameManager.Instance.coins.ToString("D2");
        }
    }

    // muestra la pantalla de derrota con el puntaje final
    public void ShowDefeatPanel()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        if (finalScoreText != null && GameManager.Instance != null)
        {
            finalScoreText.text = "Puntaje Final: " + GameManager.Instance.score.ToString();
        }

        if (finalCoinsText != null && GameManager.Instance != null)
        {
            finalCoinsText.text = "Monedas Recolectadas: " + GameManager.Instance.coins.ToString();
        }
    }

    // cuando aprietan el boton de reiniciar
    private void OnRestartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}
