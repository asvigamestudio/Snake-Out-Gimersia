using UnityEngine;

[CreateAssetMenu(fileName = "NewSnakeSkin", menuName = "Snake/Skin Data", order = 1)]
public class SnakeSkinData : ScriptableObject
{
    [Header("Identity")]
    public string skinName = "Default Snake";
    public Sprite previewImage; // ✅ Tambahkan ini untuk tampilan UI

    [Header("Prefabs")]
    [Tooltip("Prefab kepala ular (boleh null kalau pakai yang default di scene)")]
    public GameObject headPrefab;
    [Tooltip("Prefab badan ular (untuk bagian tubuh yang akan tumbuh)")]
    public GameObject bodyPrefab;

    [Header("Stats")]
    public float baseSpeed = 5f;
    public float boostMultiplier = 3f;
    public float maxStamina = 100f;
    public float staminaRegenRate = 20f;
    public float staminaDrainRate = 40f;

    [Header("Abilities")]
    [Tooltip("Ular bisa bersembunyi lebih lama di semak")]
    public bool canHideLonger = false;
}
