using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [Header("Configuration")]
    public List<CardData> masterDeck = new List<CardData>();
    public int maxHandSize = 8; // "RAM" Limit

    [Header("Visual References")]
    public GameObject cardPrefab;
    public Transform handContainer;

    [Header("Runtime State")]
    public List<CardData> drawPile = new List<CardData>();
    public List<CardData> hand = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();

    [Header("Selection State")]
    public List<CardData> selectedCards = new List<CardData>();

    private ScoreManager _scoreManager;

    private void Awake()
    {
        _scoreManager = FindFirstObjectByType<ScoreManager>();
        InitializeDeck();
    }

    public void InitializeDeck()
    {
        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();
        selectedCards.Clear();

        foreach (Transform child in handContainer) Destroy(child.gameObject);

        drawPile.AddRange(masterDeck);
        ShuffleDrawPile();

        // Initial Draw: Fill to max
        RefillHand();
    }

    public void ShuffleDrawPile()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            CardData temp = drawPile[i];
            int randomIndex = Random.Range(i, drawPile.Count);
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
    }

    // --- REFILL LOGIC (The "Balatro" Loop) ---
    public void RefillHand()
    {
        int cardsNeeded = maxHandSize - hand.Count;
        if (cardsNeeded > 0)
        {
            DrawCard(cardsNeeded);
        }
    }

    public void DrawCard(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count > 0) ReshuffleDiscard();
                else return; // Truly empty
            }

            CardData data = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(data);
            SpawnCardVisual(data);
        }
    }

    private void SpawnCardVisual(CardData data)
    {
        GameObject newCardObj = Instantiate(cardPrefab, handContainer);
        CardDisplay display = newCardObj.GetComponent<CardDisplay>();
        if (display != null) display.Setup(data);
    }

    // --- SELECTION ---
    public void SelectCard(CardData card)
    {
        if (!selectedCards.Contains(card)) selectedCards.Add(card);
    }

    public void DeselectCard(CardData card)
    {
        if (selectedCards.Contains(card)) selectedCards.Remove(card);
    }

    private int CalculateCurrentScore()
    {
        int total = 0;
        foreach (var c in selectedCards) total += c.baseYield;
        return total;
    }

    // --- ACTIONS ---

    // 1. PLAY HAND (Consumes "Market Hour")
    public void PlaySelectedHand()
    {
        if (selectedCards.Count == 0) return;

        // Ask ScoreManager if we have hands left
        if (_scoreManager != null && !_scoreManager.TryConsumeHand())
        {
            Debug.Log("No Market Hours remaining!");
            return;
        }

        // Add Score
        int handScore = CalculateCurrentScore();
        if (_scoreManager != null) _scoreManager.AddScore(handScore);

        // Move to Discard
        MoveSelectedToDiscard();

        // THE LOOP: Draw new cards to replace them
        RefillHand();
    }

    // 2. DISCARD HAND (Consumes "Shred", saves "Market Hour")
    public void DiscardSelectedHand()
    {
        if (selectedCards.Count == 0) return;

        // Ask ScoreManager if we have discards left
        if (_scoreManager != null && !_scoreManager.TryConsumeDiscard())
        {
            Debug.Log("No Shreds remaining!");
            return;
        }

        // Just move to discard (No Score)
        Debug.Log("Shredding assets...");
        MoveSelectedToDiscard();

        // THE LOOP: Draw new cards
        RefillHand();
    }

    private void MoveSelectedToDiscard()
    {
        foreach (var card in selectedCards)
        {
            if (hand.Contains(card))
            {
                hand.Remove(card);
                discardPile.Add(card);
            }
        }
        selectedCards.Clear();
        RefreshHandVisuals();
    }

    private void RefreshHandVisuals()
    {
        foreach (Transform child in handContainer) Destroy(child.gameObject);
        foreach (CardData data in hand) SpawnCardVisual(data);
    }

    private void ReshuffleDiscard()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDrawPile();
    }
}