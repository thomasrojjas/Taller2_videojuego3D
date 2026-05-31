using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton manager for the overall game state, score tracking, and level loading.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Settings")]
    [Tooltip("Reference to the player Transform to track distance score.")]
    public Transform playerTransform;

    [Header("Game State")]
    public int score = 0;
    public int coins = 0;
    public bool IsGameOver { get; private set; } = false;

    private float playerStartX = 0f;
    private float playerStartZ = 0f;
    private int bonusScore = 0;

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
        IsGameOver = false;
        score = 0;
        coins = 0;
        bonusScore = 0;

        // Automatically find player if not assigned
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        if (playerTransform != null)
        {
            playerStartX = playerTransform.position.x;
            playerStartZ = playerTransform.position.z;
        }

        // Start playing background music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM();
        }
    }

    private void Update()
    {
        if (IsGameOver) return;

        // Calculate score based on player distance traveled
        if (playerTransform != null)
        {
            float distanceTraveled = playerTransform.position.z - playerStartZ;
            // 10 points per unit of distance, plus any kill/collect bonuses
            score = Mathf.FloorToInt(distanceTraveled * 10f) + bonusScore;
        }
    }

    /// <summary>
    /// Adds to the coin count and triggers UI update.
    /// </summary>
    public void AddCoin()
    {
        if (IsGameOver) return;
        coins++;
    }

    /// <summary>
    /// Adds bonus points to the player's score.
    /// </summary>
    public void AddScore(int points)
    {
        if (IsGameOver) return;
        bonusScore += points;
    }

    /// <summary>
    /// Ends the current game session, displaying the defeat menu.
    /// </summary>
    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        // Play defeat sound & stop/change background music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSFX();
        }

        // Show the defeat menu via UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDefeatPanel();
        }

        Debug.Log("Game Over! Final Score: " + score + ", Coins: " + coins);
    }

    /// <summary>
    /// Reloads the active scene to restart the game.
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
