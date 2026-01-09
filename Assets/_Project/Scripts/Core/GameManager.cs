using UnityEngine;
using System.Collections.Generic;

public enum GameState { Boot, MainMenu, Gameplay, Paused }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Run State")]
    public GameState CurrentState;
    public List<CardData> RunDeck = new List<CardData>();
    public int RunCash = 0;

    [Header("The Calendar (Progression)")]
    public int currentQuarter = 1; // "Ante"
    public int currentMonth = 1;   // "Blind" (1, 2, or 3)
    public int currentQuota = 25; // Target Score

    // Difficulty Scaling Config
    private const int BASE_QUOTA = 25;
    private const float QUARTER_SCALING = 2.0f; // Quota doubles every quarter

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- HELPER METHODS ---

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
    }

    // --- PROGRESSION LOGIC ---

    public void StartNewRun(List<CardData> starterDeck)
    {
        RunDeck.Clear();
        RunDeck.AddRange(starterDeck);
        RunCash = 0;

        currentQuarter = 1;
        currentMonth = 1;

        CalculateQuota();
    }

    public void EndRun()
    {
        Debug.Log("Run Ended. Resetting state...");

        // Reset everything
        currentQuarter = 1;
        currentMonth = 1;
        RunCash = 0;
        RunDeck.Clear();
        currentQuota = 100; // Fixed: lowercase 'c' to match variable name

        ChangeState(GameState.MainMenu); // Fixed: Added the missing method above
    }

    public void AdvanceCalendar()
    {
        // 1. Move to next Month (Blind)
        currentMonth++;

        // 2. If Month > 3, we finished the Quarter -> Move to next Quarter (Ante)
        if (currentMonth > 3)
        {
            currentMonth = 1;
            currentQuarter++;
        }

        // 3. Recalculate Difficulty
        CalculateQuota();
    }

    private void CalculateQuota()
    {
        // Formula: Base * (Scaling ^ (Quarter-1))
        // Month 1: 100% | Month 2: 150% | Month 3: 200% (Boss)

        float baseForQuarter = BASE_QUOTA * Mathf.Pow(QUARTER_SCALING, currentQuarter - 1);
        float monthMultiplier = 1.0f;

        if (currentMonth == 2) monthMultiplier = 1.5f;
        if (currentMonth == 3) monthMultiplier = 2.0f; // Boss

        currentQuota = Mathf.RoundToInt(baseForQuarter * monthMultiplier);

        Debug.Log($"📅 NEW DATE: Q{currentQuarter} - Month {currentMonth} | 🎯 QUOTA: ${currentQuota}");
    }
}