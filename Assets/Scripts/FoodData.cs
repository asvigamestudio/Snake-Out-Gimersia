using UnityEngine;

[CreateAssetMenu(fileName = "NewFood", menuName = "Snake/Food Data", order = 1)]
public class FoodData : ScriptableObject
{
    public enum FoodType { Normal, PowerUp }

    [Header("Type")]
    public FoodType foodType = FoodType.Normal;

    [Header("Visuals")]
    public GameObject prefab;
    public Color glowColor = Color.white;

    [Header("Stats")]
    public int scoreValue = 10;
    public float staminaBonus = 10f;
    public int growCount = 1;

    [Header("Spawn Settings")]
    [Range(0f, 1f)] public float spawnChance = 0.5f;

    [Header("Power-Up (optional)")]
    public PowerUpData powerUpData; // Kalau ini null, berarti bukan power-up
}
