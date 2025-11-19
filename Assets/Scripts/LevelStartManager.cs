using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelStartManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject startPanel;        // Panel "Press Space"
    public Text objectiveText;           // Text objective

    [Header("Objective Settings")]
    [TextArea(2, 4)]
    public string levelObjective = "Kalahkan semua musuh AI untuk menang!";

    [Header("Audio")]
    public AudioSource bgmSource;        // Drag AudioSource di sini
    public AudioClip bgmClip;            // File musik BGM
    [Range(0f, 1f)] public float targetVolume = 1f; // volume target
    public float fadeDuration = 2f;      // durasi fade-in dalam detik

    private bool gameStarted = false;

    private void Start()
    {
        // ✅ Game pause saat masuk level
        Time.timeScale = 0f;

        if (startPanel != null)
            startPanel.SetActive(true);

        if (objectiveText != null)
            objectiveText.text = $"🎯 Objective:\n{levelObjective}\n\nTekan [SPACE] untuk mulai!";

        // Pastikan BGM belum jalan
        if (bgmSource != null)
        {
            bgmSource.Stop();
            bgmSource.playOnAwake = false;
            bgmSource.volume = 0f; // mulai dari 0 volume
        }
    }

    private void Update()
    {
        if (!gameStarted && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (gameStarted) return;

        gameStarted = true;
        if (startPanel != null)
            startPanel.SetActive(false);

        // ✅ Jalankan game
        Time.timeScale = 1f;

        // ✅ Mulai BGM dengan fade-in
        if (bgmSource != null)
        {
            if (bgmClip != null)
                bgmSource.clip = bgmClip;

            bgmSource.loop = true;
            StartCoroutine(FadeInMusic(fadeDuration));
        }
    }

    // ✅ Coroutine Fade-In Musik
    IEnumerator FadeInMusic(float duration)
    {
        bgmSource.volume = 0f;
        bgmSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled biar gak kena Time.timeScale
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }
}
