using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement; // Required to reload the scene

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    [Header("End Game UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    [Header("Settings")]
    public float gameDuration = 120f;
    [Tooltip("How many seconds the player must wait before they can reset the game.")]
    public float resetCooldown = 3f;

    private float timer;
    private int score = 0;
    private bool isGameOver = false;
    private bool canReset = false;

    void Start()
    {
        timer = gameDuration;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (!isGameOver)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                StartCoroutine(EndGameRoutine());
            }

            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
        else if (canReset)
        {
            // NEW: Check if ANY action was performed on the keyboard OR any Gamepad/VR controller
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame ||
                Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
            {
                ReloadGame();
            }
        }
    }

    // Coroutine to handle the delayed activation of the reset functionality
    private System.Collections.IEnumerator EndGameRoutine()
    {
        isGameOver = true;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = $"Targets Destroyed: {score}";

        Time.timeScale = 0f; // Pause the simulation

        // Wait for the cooldown duration
        float elapsed = 0f;
        while (elapsed < resetCooldown)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled because Time.timeScale is 0!
            yield return null;
        }

        canReset = true;
    }

    public void AddScore()
    {
        if (isGameOver) return;
        score++;
        scoreText.text = $"Targets: {score}";
    }

    void ReloadGame()
    {
        Time.timeScale = 1f; // Unpause before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}