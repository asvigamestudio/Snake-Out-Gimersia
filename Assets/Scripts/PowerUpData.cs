using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "Snake/PowerUp", order = 2)]
public class PowerUpData : ScriptableObject
{
    public string powerUpName;
    public Sprite icon;
    public float duration = 5f;
    public Color glowColor = Color.white;
    public enum PowerUpType { Speed, Shield, Magnet, Stamina, TimeSlow,}
    public PowerUpType type;
}
