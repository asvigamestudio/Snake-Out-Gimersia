using UnityEngine;
using UnityEngine.UI;

public class SkinSelectorUI : MonoBehaviour
{
    [Header("UI References")]
    public Text skinNameText;
    public Image skinPreviewImage;
    public Button leftButton;
    public Button rightButton;
    public Text priceText; // 🟢 Tambahan baru: Text harga

    private int currentIndex = 0;
    private GameProgressManager gm;

    private void Start()
    {
        gm = GameProgressManager.Instance;

        if (gm == null)
        {
            Debug.LogError("❌ GameProgressManager.Instance belum ada di scene!");
            enabled = false;
            return;
        }

        if (gm.allSkins == null || gm.allSkins.Count == 0)
        {
            Debug.LogError("❌ GameProgressManager.allSkins kosong!");
            enabled = false;
            return;
        }

        if (gm.unlockedSkins == null || gm.unlockedSkins.Count != gm.allSkins.Count)
        {
            Debug.LogWarning("⚠️ unlockedSkins belum diatur, auto isi true untuk debug.");
            gm.unlockedSkins = new System.Collections.Generic.List<bool>();
            for (int i = 0; i < gm.allSkins.Count; i++)
                gm.unlockedSkins.Add(true);
        }

        currentIndex = Mathf.Clamp(gm.selectedSkinIndex, 0, gm.allSkins.Count - 1);
        UpdateUI();

        if (leftButton != null) leftButton.onClick.AddListener(PrevSkin);
        if (rightButton != null) rightButton.onClick.AddListener(NextSkin);
    }

    void PrevSkin()
    {
        currentIndex = Mathf.Max(0, currentIndex - 1);
        UpdateUI();
    }

    void NextSkin()
    {
        currentIndex = Mathf.Min(gm.allSkins.Count - 1, currentIndex + 1);
        UpdateUI();
    }

    void UpdateUI()
    {
        var skin = gm.allSkins[currentIndex];
        bool unlocked = gm.unlockedSkins[currentIndex];

        // 🟢 Nama Skin
        if (skinNameText != null)
        {
            if (unlocked)
                skinNameText.text = skin.skinName;
            else
                skinNameText.text = $"{skin.skinName} 🔒";
        }

        // 🟢 Gambar Preview
        if (skinPreviewImage != null)
        {
            skinPreviewImage.sprite = skin.previewImage;
            skinPreviewImage.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.4f);
        }

        // 🟢 Harga / Requirement Text
        if (priceText != null)
        {
            if (unlocked)
            {
                priceText.text = "Unlocked";
                priceText.color = Color.green;
            }
            else
            {
                int requiredScore = 0;

                // Hindari out-of-range pada threshold
                if (currentIndex < gm.scoreThresholds.Length)
                    requiredScore = gm.scoreThresholds[currentIndex];
                else
                    requiredScore = gm.scoreThresholds[gm.scoreThresholds.Length - 1];

                priceText.text = $"Unlock at {requiredScore} pts";
                priceText.color = Color.yellow;
            }
        }

        gm.selectedSkinIndex = currentIndex;
        gm.SaveProgress();
    }
}
