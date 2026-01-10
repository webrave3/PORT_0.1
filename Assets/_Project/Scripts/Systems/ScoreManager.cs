using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI quotaText;
    public TextMeshProUGUI handTypeText;
    public TextMeshProUGUI previewScoreText;
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

        if (handTypeText) handTypeText.text = "Select Assets";
        if (previewScoreText) previewScoreText.text = "";

        UpdateUI();
    }

    // --- HAND PREVIEW (Visuals) ---
    public void UpdateHandPreview(List<CardData> selectedCards)
    {
        if (selectedCards.Count == 0)
        {
            if (handTypeText) handTypeText.text = "Select Assets";
            if (previewScoreText) previewScoreText.text = "";
            return;
        }

        int projectedScore = CalculateScoreInternal(selectedCards, out string handName, out float mult);

        if (handTypeText) handTypeText.text = handName;
        if (previewScoreText) previewScoreText.text = $"(+${projectedScore})";
    }

    // --- COMMIT SCORE (Playing) ---
    public int CalculateAndCommitScore(List<CardData> cardsPlayed)
    {
        int totalVol = 0;
        foreach (var card in cardsPlayed) totalVol += card.volatility;

        int finalScore = CalculateScoreInternal(cardsPlayed, out string handName, out float mult);

        Debug.Log($"📈 PLAYED: {handName} for ${finalScore}");

        AddVolatility(totalVol);
        AddScore(finalScore);

        if (handTypeText) handTypeText.text = "";
        if (previewScoreText) previewScoreText.text = "";

        return finalScore;
    }

    // --- CORE CALCULATION LOGIC (Includes Event Modifiers) ---
    private int CalculateScoreInternal(List<CardData> cards, out string handName, out float multiplier)
    {
        // 1. Identify Active Event (Only active in Month 3)
        string activeEventID = "";
        if (GameManager.Instance != null && GameManager.Instance.currentMonth == 3)
        {
            if (GameManager.Instance.currentQuarterBoss != null)
            {
                activeEventID = GameManager.Instance.currentQuarterBoss.eventID;
            }
        }

        // 2. Sum Base Yields with Event Logic
        float totalYield = 0;
        foreach (var card in cards)
        {
            float cardYield = card.baseYield;

            // --- EVENT: BEAR MARKET ---
            if (activeEventID == "BEAR")
            {
                cardYield *= 0.5f; // Halve Yield
            }

            // --- EVENT: SEC AUDIT ---
            if (activeEventID == "AUDIT" && card.isIllegal)
            {
                cardYield = 0; // Illegal cards worth nothing
            }

            totalYield += cardYield;
        }

        // 3. Synergy Check
        var sectorGroups = cards.GroupBy(c => c.sector).OrderByDescending(g => g.Count());
        var primaryGroup = sectorGroups.FirstOrDefault();
        int count = primaryGroup != null ? primaryGroup.Count() : 0;

        multiplier = 1.0f;
        handName = "Diversified";

        if (count >= 3)
        {
            handName = $"{primaryGroup.Key} Rally";
            multiplier = SYNERGY_MULTIPLIER;
        }
        else if (count == 2)
        {
            handName = "Merger";
        }

        if (cards.Count == 1) handName = "Single Asset";

        return Mathf.RoundToInt(totalYield * multiplier);
    }

    // --- EXISTING STATE LOGIC ---
    public void AddVolatility(int amount)
    {
        currentVolatility = Mathf.Clamp(currentVolatility + amount, 0, maxVolatility);
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        if (currentVolatility >= maxVolatility) return;
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
            Debug.Log("Quota Met within ScoreManager");
        }
    }

    public void UpdateUI()
    {
        if (scoreText) scoreText.text = $"${currentYield}";

        int target = GameManager.Instance ? GameManager.Instance.currentQuota : currentQuota;
        if (quotaText) quotaText.text = $"/ ${target}";

        if (handsText) handsText.text = $"{handsRemaining}/{maxHands}";
        if (discardsText) discardsText.text = $"{discardsRemaining}/{maxDiscards}";

        if (volatilitySlider) volatilitySlider.value = currentVolatility;
        if (volText) volText.text = $"{currentVolatility}%";
    }
}