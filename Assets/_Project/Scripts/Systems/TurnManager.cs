using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("UI References")]
    // We only track the Date here. Score/Quota/Hands are handled by ScoreManager.
    public TextMeshProUGUI dateText;
    public DraftScreen draftScreen;

    private DeckManager _deckManager;
    private ScoreManager _scoreManager;

    private bool _roundActive = true; // Prevents double-triggering Win/Loss

    private void Awake()
    {
        Instance = this;
        _deckManager = FindFirstObjectByType<DeckManager>();
        _scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    private void Start()
    {
        // 1. Get the correct Quota from the persistent GameManager
        int targetQuota = 100; // Default fallback
        if (GameManager.Instance != null)
        {
            targetQuota = GameManager.Instance.currentQuota;
            UpdateDateUI();
        }

        // 2. Initialize the Round
        StartRound(targetQuota);
    }

    public void StartRound(int quota)
    {
        _roundActive = true;

        // 3. SYNC SCOREMANAGER
        // We push the correct Quota to ScoreManager and reset its state immediately.
        if (_scoreManager != null)
        {
            _scoreManager.currentYield = 0;       // Reset Score
            _scoreManager.currentQuota = quota;   // Set Goal

            // Reset Resources
            _scoreManager.handsRemaining = _scoreManager.maxHands;
            _scoreManager.discardsRemaining = _scoreManager.maxDiscards;

            // REMOVED: _scoreManager.currentVolatility = 0; 
            // FIX: Heat is now persistent in GameManager, so we DO NOT reset it here.

            _scoreManager.UpdateUI(); // Force visual update NOW
        }

        // 4. Initialize Deck
        if (_deckManager != null)
        {
            _deckManager.InitializeDeck();
        }
    }

    // Called by DeckManager when a hand is played
    public void OnHandPlayed(int handScore)
    {
        if (!_roundActive) return;

        // We don't need to track score here manually. 
        // ScoreManager already added it to 'currentYield'.
        // We just check if that was enough to win.
        CheckRoundEnd();
    }

    private void CheckRoundEnd()
    {
        if (!_roundActive) return;
        if (_scoreManager == null) return;

        // --- WIN CONDITION ---
        if (_scoreManager.currentYield >= _scoreManager.currentQuota)
        {
            Debug.Log("✅ QUOTA MET! Advancing...");
            RoundWon();
        }
        // --- LOSS CONDITION ---
        else if (_scoreManager.handsRemaining <= 0)
        {
            Debug.Log("❌ INSOLVENT. Game Over.");
            RoundLost();
        }
    }

    private void RoundWon()
    {
        _roundActive = false;

        if (GameManager.Instance != null && _scoreManager != null)
        {
            // 1. BANK THE CASH
            GameManager.Instance.RunCash += _scoreManager.currentYield;
            Debug.Log($"💰 BANKED: ${_scoreManager.currentYield}. Total: ${GameManager.Instance.RunCash}");

            // 2. Advance Date
            GameManager.Instance.AdvanceCalendar();
        }

        // 3. Load Shop Scene
        SceneManager.LoadScene("03_Shop");
    }

    private void RoundLost()
    {
        _roundActive = false;

        // 1. Reset Run Data
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndRun();
        }

        // 2. Return to Main Menu
        SceneManager.LoadScene("01_MainMenu");
    }

    private void UpdateDateUI()
    {
        if (dateText != null && GameManager.Instance != null)
        {
            dateText.text = $"Q{GameManager.Instance.currentQuarter} - Month {GameManager.Instance.currentMonth}";
        }
    }
}