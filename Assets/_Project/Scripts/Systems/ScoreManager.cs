using UnityEngine;
using UnityEngine.UI; // Needed for Slider
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI quotaText;
    public TextMeshProUGUI handsText;
    public TextMeshProUGUI discardsText;
    public Slider volatilitySlider;
    public TextMeshProUGUI volText;

    [Header("Game State")]
    public int currentQuota = 100;
    public int currentYield = 0;

    [Header("Volatility")]
    public int currentVolatility = 0;
    public int maxVolatility = 100; // Circuit Breaker Limit

    [Header("Resources")]
    public int handsRemaining = 4;
    public int discardsRemaining = 3;
    public int maxHands = 4;
    public int maxDiscards = 3;

    // Dependencies
    private GameFlowManager _flowManager;

    private void Start()
    {
        _flowManager = FindFirstObjectByType<GameFlowManager>();
        UpdateUI();
    }

    // --- VOLATILITY LOGIC ---
    public void AddVolatility(int amount)
    {
        currentVolatility += amount;

        // Clamp between 0 and Max
        currentVolatility = Mathf.Clamp(currentVolatility, 0, maxVolatility);

        UpdateUI();
    }

    public bool IsOverheated()
    {
        return currentVolatility >= maxVolatility;
    }

    // --- SCORING LOGIC ---
    public void AddScore(int amount)
    {
        // 1. Check Circuit Breaker
        if (IsOverheated())
        {
            Debug.Log("CIRCUIT BREAKER! Score Voided.");
            // You could add a sound effect play here
            return; // Add 0 score
        }

        // 2. Add Score
        currentYield += amount;
        UpdateUI();

        // 3. Check Win
        CheckWinCondition();
    }

    public bool TryConsumeHand()
    {
        if (handsRemaining > 0)
        {
            handsRemaining--;
            UpdateUI();

            // Check Loss Condition immediately after consuming the last hand
            // NOTE: We check if yield < quota inside the DeckManager flow usually, 
            // but for safety, we check here if they ran out.
            if (handsRemaining <= 0 && currentYield < currentQuota)
            {
                if (_flowManager != null) _flowManager.GameOver(false, currentYield);
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
            Debug.Log("QUOTA MET! (Win)");
            if (_flowManager != null) _flowManager.GameOver(true, currentYield);
        }
    }

    private void UpdateUI()
    {
        if (scoreText) scoreText.text = $"YIELD: ${currentYield}";
        if (quotaText) quotaText.text = $"QUOTA: ${currentQuota}";
        if (handsText) handsText.text = $"HOURS: {handsRemaining}/{maxHands}";
        if (discardsText) discardsText.text = $"SHREDS: {discardsRemaining}/{maxDiscards}";

        // Update Slider
        if (volatilitySlider) volatilitySlider.value = currentVolatility;
        if (volText) volText.text = $"{currentVolatility}% HEAT";
    }
}