using UnityEngine;
using UnityEngine.UI;

public class TotalScoreUI : MonoBehaviour
{
    [Header("UI Reference")]
    public Text totalScoreText;

    [Header("Optional")]
    public bool autoUpdate = true; // kalau mau teks update realtime

    private void Start()
    {
        UpdateScoreText();
    }

    private void Update()
    {
        if (autoUpdate)
            UpdateScoreText();
    }

    public void UpdateScoreText()
    {
        if (totalScoreText == null)
        {
            Debug.LogWarning("⚠️ totalScoreText belum diassign di Inspector.");
            return;
        }

        if (GameProgressManager.Instance == null)
        {
            totalScoreText.text = "Score: 0";
            return;
        }

        int score = GameProgressManager.Instance.totalScore;
        totalScoreText.text = $"Total Score: {score:N0}";
        // :N0 = format angka dengan pemisah ribuan (contoh: 12,540)
    }
}
