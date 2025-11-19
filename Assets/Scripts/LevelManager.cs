using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public int levelIndex = 1;
    public int rewardScore = 5000;

    public void OnLevelComplete()
    {
        var gm = GameProgressManager.Instance;
        if (gm == null) return;

        gm.AddScore(rewardScore);
        if (gm.unlockedLevel < levelIndex + 1)
            gm.UnlockNextLevel();

        gm.SaveProgress();

        // Kembali ke main menu
        SceneManager.LoadScene("MainMenuScene");
    }
}
