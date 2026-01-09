using UnityEngine;
using System.Collections.Generic;

public enum GameState { Boot, MainMenu, Gameplay, Paused }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    // --- NEW: RUN PERSISTENCE ---
    [Header("Current Run Data")]
    public List<CardData> RunDeck = new List<CardData>(); // The player's growing deck
    public int RunCash = 0;
    public int CurrentQuota = 100; // Track difficulty scaling
    // -----------------------------

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

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
    }

    // Call this when clicking "Start Game" from Main Menu
    public void StartNewRun(List<CardData> starterDeck)
    {
        RunDeck.Clear();
        RunDeck.AddRange(starterDeck);
        CurrentQuota = 100; // Reset difficulty
        RunCash = 0;
    }
}