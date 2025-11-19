using UnityEngine;

public class SkinApplier : MonoBehaviour
{
    private void Start()
    {
        // Pastikan ada GameProgressManager
        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("⚠️ GameProgressManager belum ada, skin tidak bisa diaplikasikan.");
            return;
        }

        // Ambil skin terpilih
        int selectedIndex = GameProgressManager.Instance.selectedSkinIndex;

        // Cek kalau skin tersedia
        if (GameProgressManager.Instance.allSkins == null || 
            GameProgressManager.Instance.allSkins.Count == 0)
        {
            Debug.LogWarning("⚠️ Daftar skin kosong, pastikan allSkins diisi di GameProgressManager.");
            return;
        }

        // Pastikan index valid
        selectedIndex = Mathf.Clamp(selectedIndex, 0, GameProgressManager.Instance.allSkins.Count - 1);
        SnakeSkinData chosenSkin = GameProgressManager.Instance.allSkins[selectedIndex];

        // Cari SnakeController di scene
        SnakeController snake = FindObjectOfType<SnakeController>();
        if (snake != null)
        {
            snake.ApplySkin(chosenSkin);
            Debug.Log($"🐍 Skin diterapkan: {chosenSkin.skinName}");
        }
        else
        {
            Debug.LogWarning("⚠️ Tidak menemukan SnakeController di scene!");
        }
    }
}
