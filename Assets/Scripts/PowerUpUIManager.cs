using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpUIManager : MonoBehaviour
{
    public static PowerUpUIManager Instance;

    [Header("Player UI")]
    public Image playerPowerUpIcon;
    public Image playerGlowEffect;

    private Coroutine playerTimerRoutine;

    private void Awake()
    {
        Instance = this;

        if (playerPowerUpIcon) playerPowerUpIcon.enabled = false;
        if (playerGlowEffect) playerGlowEffect.enabled = false;
    }

    // ================= PLAYER ONLY =================
    public void ShowPlayerPowerUp(PowerUpData data)
    {
        if (playerPowerUpIcon == null) return;
        if (playerTimerRoutine != null) StopCoroutine(playerTimerRoutine);

        playerPowerUpIcon.sprite = data.icon;
        playerPowerUpIcon.color = Color.white;
        playerPowerUpIcon.enabled = true;

        if (playerGlowEffect)
        {
            playerGlowEffect.enabled = true;
            playerGlowEffect.color = data.glowColor;
        }

        playerTimerRoutine = StartCoroutine(HideAfterTime(data.duration, playerPowerUpIcon, playerGlowEffect));
    }

    private IEnumerator HideAfterTime(float time, Image icon, Image glow)
    {
        yield return new WaitForSeconds(time);

        if (icon) icon.enabled = false;
        if (glow) glow.enabled = false;
    }
}
