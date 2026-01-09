using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText; // Shows "Current Yield" (e.g., $50)
    public TextMeshProUGUI quotaText; // Shows "Target" (e.g., / $100)
    public TextMeshProUGUI handTypeText; // NEW: Shows "Sector Rally" etc.
    public TextMeshProUGUI previewScoreText; // NEW: Shows "(+$20)" next to score

    public Slider volatilitySlider;
    public TextMeshProUGUI volText;

    [Header("Resources UI")]
    public TextMeshProUGUI handsText;
    public TextMeshProUGUI discardsText;

    [Header("Game State")]
    public int currentYield = 0;
    public int currentQuota = 100;

    [Header("Volatility")]
    public int currentVolatility = 0;
    public int maxVolatility = 100;

    [Header("Resources")]
    public int handsRemaining = 4;
    public int discardsRemaining = 3;
    public int maxHands = 4;
    public int maxDiscards = 3;

    // Config
    private const float SYNERGY_MULTIPLIER = 1.5f;

    private GameFlowManager _flowManager;

    private void Start()
    {
        _flowManager = FindFirstObjectByType<GameFlowManager>();

        // Initialize UI with empty/default state
        if (handTypeText) handTypeText.text = "Select Assets";
        if (previewScoreText) previewScoreText.text = "";

        // Force an update to remove weird "/100" defaults
        UpdateUI();
    }

    // --- NEW: HAND EVALUATION (Visuals Only) ---
    public void UpdateHandPreview(List<CardData> selectedCards)
    {
        if (selectedCards.Count == 0)
        {
            if (handTypeText) handTypeText.text = "Select Assets";
            if (previewScoreText) previewScoreText.text = "";
            return;
        }

        // Calculate hypothetical score
        int projectedScore = CalculateScoreInternal(selectedCards, out string handName, out float mult);

        // Update UI
        if (handTypeText) handTypeText.text = handName;
        if (previewScoreText) previewScoreText.text = $"(+${projectedScore})";
    }

    // --- CORE LOGIC (Shared by Preview & Commit) ---
    private int CalculateScoreInternal(List<CardData> cards, out string handName, out float multiplier)
    {
        int totalYield = 0;
        foreach (var card in cards) totalYield += card.baseYield;

        // Synergy Check
        var sectorGroups = cards.GroupBy(c => c.sector).OrderByDescending(g => g.Count());
        var primaryGroup = sectorGroups.FirstOrDefault();
        int count = primaryGroup != null ? primaryGroup.Count() : 0;

        multiplier = 1.0f;
        handName = "Diversified"; // Default High Card

        if (count >= 3)
        {
            handName = $"{primaryGroup.Key} Rally"; // e.g., "Tech Rally"
            multiplier = SYNERGY_MULTIPLIER;
        }
        else if (count == 2)
        {
            handName = "Merger"; // Pair
        }

        if (cards.Count == 1) handName = "Single Asset";

        return Mathf.RoundToInt(totalYield * multiplier);
    }

    // --- COMMIT SCORE (Playing the hand) ---
    public int CalculateAndCommitScore(List<CardData> cardsPlayed)
    {
        int totalVol = 0;
        foreach (var card in cardsPlayed) totalVol += card.volatility;

        // Use the shared logic
        int finalScore = CalculateScoreInternal(cardsPlayed, out string handName, out float mult);

        Debug.Log($"📈 PLAYED: {handName} for ${finalScore}");

        AddVolatility(totalVol);
        AddScore(finalScore);

        // Clear preview
        if (handTypeText) handTypeText.text = "";
        if (previewScoreText) previewScoreText.text = "";

        return finalScore;
    }

    // --- EXISTING STATE LOGIC ---
    public void AddVolatility(int amount)
    {
        currentVolatility = Mathf.Clamp(currentVolatility + amount, 0, maxVolatility);
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        if (currentVolatility >= maxVolatility) return; // Circuit Breaker
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
                if (_flowManager != null) _flowManager.GameOver(false, currentYield);
            }
            return true;
        }
        return false;
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
            // TurnManager handles the actual win flow now
            Debug.Log("Quota Met within ScoreManager");
        }
    }

    public void UpdateUI()
    {
        // Safety checks + Formatting
        if (scoreText) scoreText.text = $"${currentYield}";

        // Get Quota from GameManager if possible, else use local fallback
        int target = GameManager.Instance ? GameManager.Instance.currentQuota : currentQuota;
        if (quotaText) quotaText.text = $"/ ${target}";

        if (handsText) handsText.text = $"{handsRemaining}/{maxHands}";
        if (discardsText) discardsText.text = $"{discardsRemaining}/{maxDiscards}";

        if (volatilitySlider) volatilitySlider.value = currentVolatility;
        if (volText) volText.text = $"{currentVolatility}%";
    }
}