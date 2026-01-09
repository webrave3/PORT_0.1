using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI quotaText;
    public TextMeshProUGUI handsText;
    public TextMeshProUGUI discardsText; // NEW

    [Header("Game State")]
    public int currentQuota = 100; // Enemy HP
    public int currentYield = 0;   // Player Score

    [Header("Resources")]
    public int handsRemaining = 4;
    public int maxHands = 4;
    public int discardsRemaining = 3; // NEW
    public int maxDiscards = 3;

    private void Start()
    {
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        currentYield += amount;
        UpdateUI();
        CheckWinCondition();
    }

    public bool TryConsumeHand()
    {
        if (handsRemaining > 0)
        {
            handsRemaining--;
            UpdateUI();

            if (handsRemaining <= 0 && currentYield < currentQuota)
            {
                Debug.Log("MARKET CLOSED: Insolvency! (Game Over)");
            }
            return true; // Success
        }
        return false; // Cannot play
    }

    public bool TryConsumeDiscard()
    {
        if (discardsRemaining > 0)
        {
            discardsRemaining--;
            UpdateUI();
            return true;
        }
        return false;
    }

    private void CheckWinCondition()
    {
        if (currentYield >= currentQuota)
        {
            Debug.Log("QUOTA MET! Market Closed. (Win)");
        }
    }

    private void UpdateUI()
    {
        if (scoreText) scoreText.text = $"YIELD: ${currentYield}";
        if (quotaText) quotaText.text = $"QUOTA: ${currentQuota}";
        if (handsText) handsText.text = $"HOURS: {handsRemaining}/{maxHands}";
        if (discardsText) discardsText.text = $"SHREDS: {discardsRemaining}/{maxDiscards}";
    }
}