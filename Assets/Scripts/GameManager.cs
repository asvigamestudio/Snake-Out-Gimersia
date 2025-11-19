using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Music Settings")]
    public AudioSource musicSource;
    public AudioClip playerDeathMusic;
    public AudioClip enemyDeathMusic;
    public AudioClip victoryMusic;

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject pausePanel;

    private SnakeController player;
    private SnakeAIController[] enemies;
    private bool gameEnded = false;
    private bool isPaused = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        player = FindFirstObjectByType<SnakeController>();
        enemies = FindObjectsByType<SnakeAIController>(FindObjectsSortMode.None);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }


    void Update()
    {
        // 🔹 Toggle Pause Menu
        if (Input.GetKeyDown(KeyCode.Escape) && !gameEnded)
        {
            if (!isPaused)
                PauseGame();
            else
                ResumeGame();
        }
    }

    // ==================== GAME STATES ====================
    public void OnPlayerDeath()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("💀 Player Died!");
        PlayMusic(playerDeathMusic);

        // ✅ Tambahkan proteksi biar aman
        if (GameProgressManager.Instance != null && ScoreManager.Instance != null)
        {
            GameProgressManager.Instance.AddScore(ScoreManager.Instance.matchScore);
            ScoreManager.Instance.ResetMatchScore();
        }
        else
        {
            Debug.LogWarning("⚠️ GameProgressManager or ScoreManager not found in scene!");
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }


    public void OnEnemyDeath()
    {
        if (gameEnded) return;

        bool allDead = true;
        enemies = FindObjectsByType<SnakeAIController>(FindObjectsSortMode.None);

        foreach (var e in enemies)
        {
            if (e != null && !e.IsDead())
            {
                allDead = false;
                break;
            }
        }

        PlayMusic(enemyDeathMusic);

        if (allDead)
            OnPlayerWin();
    }


    public void OnPlayerWin()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("🏆 Player Won!");
        PlayMusic(victoryMusic);

        // ✅ Tambahkan skor match ke total progres global
        GameProgressManager.Instance.AddScore(ScoreManager.Instance.matchScore);
        ScoreManager.Instance.ResetMatchScore();

        // ✅ Naikkan level
        GameProgressManager.Instance.UnlockNextLevel();

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // ==================== MUSIC ====================
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = false;
        musicSource.Play();
    }

    // ==================== PAUSE SYSTEM ====================
    public void PauseGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        isPaused = true;
        Time.timeScale = 0f;

        if (musicSource != null)
            musicSource.Pause();

        Debug.Log("⏸ Game Paused");
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        isPaused = false;
        Time.timeScale = 1f;

        if (musicSource != null)
            musicSource.UnPause();

        Debug.Log("▶ Game Resumed");
    }

    // ==================== SCENE CONTROLS ====================
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
