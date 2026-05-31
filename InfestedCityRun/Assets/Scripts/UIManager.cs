using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton that manages HUD displays (Score and Coins) and the Defeat Menu panel.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Outlets")]
    [Tooltip("Text component displaying current score.")]
    public Text scoreText;
    [Tooltip("Text component displaying current coin count.")]
    public Text coinsText;

    [Header("Defeat Panel Outlets")]
    [Tooltip("Panel containing the defeat menu UI.")]
    public GameObject defeatPanel;
    [Tooltip("Text component displaying the final score in the defeat menu.")]
    public Text finalScoreText;
    [Tooltip("Text component displaying final coins in the defeat menu.")]
    public Text finalCoinsText;
    [Tooltip("Button used to restart the game.")]
    public Button restartButton;

    private void Awake()
    {
        // Setup Singleton instance
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
        // Hide defeat panel at game start
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }

        // Bind restart button click listener
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // Update score HUD (Format: PUNTUACIÓN: 00000345)
        if (scoreText != null)
        {
            scoreText.text = "PUNTUACIÓN: " + GameManager.Instance.score.ToString("D8");
        }

        // Update coins HUD (Format: MONEDAS: 09)
        if (coinsText != null)
        {
            coinsText.text = "MONEDAS: " + GameManager.Instance.coins.ToString("D2");
        }
    }

    /// <summary>
    /// Displays the defeat screen and final stats.
    /// </summary>
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

    private void OnRestartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}
