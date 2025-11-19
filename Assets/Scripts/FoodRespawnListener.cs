using UnityEngine;

public class FoodRespawnListener : MonoBehaviour
{
    public FoodSpawner spawner;

    private void OnDestroy()
    {
        // 🚫 Jangan respawn saat shutdown/unload
        if (!Application.isPlaying) return;
        if (spawner == null) return;
        if (spawner.isShuttingDown) return;

        // 🍎 Hanya respawn jika makanan DIMAKAN (destroy normal)
        spawner.NotifyFoodEaten(gameObject);
    }
}
