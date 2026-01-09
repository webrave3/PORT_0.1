using UnityEngine;

public enum TurnPhase { Start, PlayerAction, Resolution, MarketEvent, End }

public class TurnManager : MonoBehaviour
{
    public TurnPhase CurrentPhase;
    private DeckManager _deckManager;

    private void Start()
    {
        _deckManager = FindFirstObjectByType<DeckManager>();
        StartTurn();
    }

    public void StartTurn()
    {
        CurrentPhase = TurnPhase.Start;
        Debug.Log("--- NEW TURN START ---");

        CurrentPhase = TurnPhase.PlayerAction;
        Debug.Log("Phase: Player Action. Waiting for input...");
    }

    public void EndTurn()
    {
        // Logic to discard hand, trigger enemy, etc.
        Debug.Log("Ending Turn...");
        CurrentPhase = TurnPhase.End;
        // In future: Loop back to StartTurn
    }
}