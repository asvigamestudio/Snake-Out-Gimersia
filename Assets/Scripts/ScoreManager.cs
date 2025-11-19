using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Session Score")]
    public int matchScore = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddScore(int amount)
    {
        matchScore += amount;
    }

    public void ResetMatchScore()
    {
        matchScore = 0;
    }
}
