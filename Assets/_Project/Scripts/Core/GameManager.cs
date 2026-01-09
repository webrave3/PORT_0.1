using UnityEngine;

public enum GameState { Boot, MainMenu, Gameplay, Paused }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    // Global Run Data (Persists between battles)
    public int PlayerCash;
    public int CurrentInsolvency;

    private void Awake()
    {
        // Singleton Logic
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Keeps this object alive across scenes
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"Game State Changed to: {newState}");
    }
}