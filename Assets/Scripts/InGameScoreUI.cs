using UnityEngine;
using UnityEngine.UI;

public class InGameScoreUI : MonoBehaviour
{
    public Text scoreText;

    private void Update()
    {
        if (ScoreManager.Instance != null)
            scoreText.text = $"Score: {ScoreManager.Instance.matchScore}";
    }
}
