using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelSelectorUI : MonoBehaviour
{
    [Header("Level Buttons (drag ke sini 4 button dari UI)")]
    public Button[] levelButtons;

    [Header("Text Label Tiap Button")]
    public Text[] levelTexts;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip lockedClickSound;

    [Header("Popup (kalau skin belum terbuka)")]
    public GameObject lockedSkinPopup; // drag panel popup di sini

    private string[] sceneNames = {
        "Level_1_SnowDay",
        "Level_2_SnowNight",
        "Level_3_GreenDay",
        "Level_4_GreenNight"
    };

    private string[] levelNames = {
        "Snow Day",
        "Snow Night",
        "Green Day",
        "Green Night"
    };

    private const int MAX_LEVEL_ALLOWED = 2; // 🔒 Level 3 & 4 disable

    private void Start()
    {
        UpdateButtons();
        if (lockedSkinPopup != null) lockedSkinPopup.SetActive(false);
    }

    public void UpdateButtons()
    {
        int unlocked = GameProgressManager.Instance.unlockedLevel;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool isUnlocked = (i + 1) <= unlocked;

            // 🔒 LIMIT: level > 2 selalu dianggap locked
            if ((i + 1) > MAX_LEVEL_ALLOWED)
                isUnlocked = false;

            // update text (dengan icon lock)
            if (levelTexts != null && i < levelTexts.Length)
                levelTexts[i].text = isUnlocked
                    ? $"{levelNames[i]}"
                    : $"{levelNames[i]} 🔒";

            int index = i;
            levelButtons[i].onClick.RemoveAllListeners();

            if (isUnlocked)
            {
                SetButtonColor(levelButtons[i], Color.white);
                levelButtons[i].interactable = true;
                levelButtons[i].onClick.AddListener(() => TryStartLevel(index));
            }
            else
            {
                SetButtonColor(levelButtons[i], new Color(0.6f, 0.6f, 0.6f));
                levelButtons[i].interactable = true;
                levelButtons[i].onClick.AddListener(PlayLockedSound);
            }
        }
    }

    void TryStartLevel(int index)
    {
        var gm = GameProgressManager.Instance;
        bool skinUnlocked = gm.unlockedSkins[gm.selectedSkinIndex];

        if (!skinUnlocked)
        {
            Debug.Log("⚠️ Skin belum terbuka!");
            PlayLockedSound();
            ShowPopup();
            return;
        }

        // 🔒 Prevent start forbidden level
        if ((index + 1) > MAX_LEVEL_ALLOWED)
        {
            Debug.Log("🚫 Level ini dikunci sementara!");
            PlayLockedSound();
            return;
        }

        // Lanjut jika valid
        gm.selectedLevel = index + 1;
        gm.SaveProgress();
        SceneManager.LoadScene(sceneNames[index]);
    }

    void PlayLockedSound()
    {
        if (audioSource != null && lockedClickSound != null)
            audioSource.PlayOneShot(lockedClickSound);
    }

    void SetButtonColor(Button button, Color color)
    {
        if (button == null) return;

        Image img = button.GetComponent<Image>();
        if (img != null)
            img.color = color;
    }

    void ShowPopup()
    {
        if (lockedSkinPopup == null) return;
        StopAllCoroutines();
        StartCoroutine(AutoHidePopup());
    }

    IEnumerator AutoHidePopup()
    {
        lockedSkinPopup.SetActive(true);
        yield return new WaitForSeconds(1f);
        lockedSkinPopup.SetActive(false);
    }
}
