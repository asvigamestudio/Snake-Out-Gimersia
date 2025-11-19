using UnityEngine;
using System.Collections.Generic;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance;

    [Header("Progress Data")]
    public int totalScore = 0;
    public int unlockedLevel = 1;
    public int selectedLevel = 1;
    public int selectedSkinIndex = 0;

    private float autoSaveTimer = 0f;

    [Header("Skin Unlock Thresholds")]
    [Tooltip("Total skor akumulatif untuk membuka skin berikutnya")]
    public int[] scoreThresholds = { 0, 10000, 15000, 25000 }; 

    [Header("Skins")]
    public List<SnakeSkinData> allSkins = new List<SnakeSkinData>();
    public List<bool> unlockedSkins = new List<bool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitSkins();
        LoadProgress();
    }

    private void Update()
    {
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= 10f) // setiap 10 detik
        {
            SaveProgress();
            autoSaveTimer = 0f;
        }
    }

    private void InitSkins()
    {
        unlockedSkins.Clear();
        for (int i = 0; i < allSkins.Count; i++)
            unlockedSkins.Add(i == 0); // skin pertama selalu kebuka
    }

    public void AddScore(int value)
    {
        totalScore += value;
        CheckSkinUnlock();
        SaveProgress();
    }

    public void UnlockNextLevel()
    {
        // Ambil level yang baru saja dimainkan
        int currentLevel = selectedLevel;

        // Pastikan hanya membuka level berikutnya
        if (currentLevel >= unlockedLevel && currentLevel < 4)
        {
            unlockedLevel = currentLevel + 1;
            Debug.Log($"🔓 Level {unlockedLevel} berhasil terbuka!");
            SaveProgress();
        }
        else
        {
            Debug.Log($"✅ Tidak ada level baru untuk dibuka (current: {currentLevel}, unlocked: {unlockedLevel})");
        }
    }


    public void CheckSkinUnlock()
    {
        for (int i = 0; i < scoreThresholds.Length && i < unlockedSkins.Count; i++)
        {
            if (totalScore >= scoreThresholds[i])
                unlockedSkins[i] = true;
        }
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt("TotalScore", totalScore);
        PlayerPrefs.SetInt("UnlockedLevel", unlockedLevel);
        PlayerPrefs.SetInt("SelectedLevel", selectedLevel);
        PlayerPrefs.SetInt("SelectedSkin", selectedSkinIndex);

        for (int i = 0; i < unlockedSkins.Count; i++)
            PlayerPrefs.SetInt($"SkinUnlocked_{i}", unlockedSkins[i] ? 1 : 0);

        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
        selectedSkinIndex = PlayerPrefs.GetInt("SelectedSkin", 0);

        unlockedSkins.Clear();
        for (int i = 0; i < allSkins.Count; i++)
        {
            bool unlocked = PlayerPrefs.GetInt($"SkinUnlocked_{i}", i == 0 ? 1 : 0) == 1;
            unlockedSkins.Add(unlocked);
        }

        CheckSkinUnlock(); // pastikan sinkronisasi
    }
}
