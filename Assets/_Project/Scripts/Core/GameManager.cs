using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum GameState { Boot, MainMenu, Gameplay, Paused }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Run State")]
    public GameState CurrentState;
    public List<CardData> RunDeck = new List<CardData>();
    public int RunCash = 0;

    [Header("The Calendar (Progression)")]
    public int currentQuarter = 1;
    public int currentMonth = 1;
    public int currentQuota = 25;

    [Header("Market Forecast")]
    public MarketEventData currentQuarterBoss; // The Event waiting at Month 3
    private List<MarketEventData> _allEvents;

    // Difficulty Scaling Config
    private const int BASE_QUOTA = 25;
    private const float QUARTER_SCALING = 2.0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Preload all events from "Assets/_Project/Resources/Events"
        _allEvents = Resources.LoadAll<MarketEventData>("Events").ToList();
        if (_allEvents.Count == 0) Debug.LogWarning("⚠️ No Market Events found in Resources/Events!");
    }

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
        PickNewBoss(); // Forecast the first boss
    }

    public void EndRun()
    {
        Debug.Log("Run Ended. Resetting state...");

        currentQuarter = 1;
        currentMonth = 1;
        RunCash = 0;
        RunDeck.Clear();
        currentQuota = 25;
        currentQuarterBoss = null;

        ChangeState(GameState.MainMenu);
    }

    public void AdvanceCalendar()
    {
        currentMonth++;

        // New Quarter Logic
        if (currentMonth > 3)
        {
            currentMonth = 1;
            currentQuarter++;
            PickNewBoss(); // Forecast the new boss for this quarter
        }

        CalculateQuota();
    }

    private void CalculateQuota()
    {
        float baseForQuarter = BASE_QUOTA * Mathf.Pow(QUARTER_SCALING, currentQuarter - 1);
        float monthMultiplier = 1.0f;

        if (currentMonth == 2) monthMultiplier = 1.5f;
        if (currentMonth == 3) monthMultiplier = 2.0f; // Boss Month is hardest

        currentQuota = Mathf.RoundToInt(baseForQuarter * monthMultiplier);

        Debug.Log($"📅 NEW DATE: Q{currentQuarter}-M{currentMonth} | 🎯 QUOTA: ${currentQuota}");
    }

    private void PickNewBoss()
    {
        if (_allEvents != null && _allEvents.Count > 0)
        {
            currentQuarterBoss = _allEvents[Random.Range(0, _allEvents.Count)];
            Debug.Log($"⚠️ FORECAST: The Boss for Q{currentQuarter} is {currentQuarterBoss.eventName}");
        }
        else
        {
            currentQuarterBoss = null;
        }
    }
}