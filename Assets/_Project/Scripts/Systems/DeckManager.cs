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

    public GameObject rerollButton; // Drag the button here

    public TMPro.TextMeshProUGUI deckCountText; // NEW
    private void Awake()
    {
        _scoreManager = FindFirstObjectByType<ScoreManager>();

        // --- NEW: LOAD FROM GAME MANAGER ---
        if (GameManager.Instance != null && GameManager.Instance.RunDeck.Count > 0)
        {
            // If we have a run going, use that deck instead of the Inspector default
            masterDeck.Clear();
            masterDeck.AddRange(GameManager.Instance.RunDeck);
        }
        // ------------------------------------

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
        UpdateDeckCountUI();
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
        UpdateDeckCountUI();
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
            UpdateDeckCountUI();
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
        if (rerollButton != null) rerollButton.SetActive(false);
        if (selectedCards.Count == 0) return;

        if (_scoreManager != null && !_scoreManager.TryConsumeHand()) return;

        // 1. Calculate Score & Volatility
        int handScore = 0;
        int handVol = 0;

        foreach (var card in selectedCards)
        {
            handScore += card.baseYield;
            handVol += card.volatility; // Add up the heat
        }

        // 2. Apply Effects
        if (_scoreManager != null)
        {
            _scoreManager.AddVolatility(handVol); // First add heat
            _scoreManager.AddScore(handScore);    // Then try to score (will fail if overheated)
        }

        MoveSelectedToDiscard();
        RefillHand();
        UpdateDeckCountUI();
    }

    // 2. DISCARD HAND (Consumes "Shred", saves "Market Hour")
    public void DiscardSelectedHand()
    {
        if (rerollButton != null) rerollButton.SetActive(false);
        if (selectedCards.Count == 0) return;

        if (_scoreManager != null && !_scoreManager.TryConsumeDiscard()) return;

        // GDD Rule: Discarding generates 2% Heat per card
        int discardHeatCost = selectedCards.Count * 2;

        Debug.Log($"Shredding assets. Heat +{discardHeatCost}%");

        if (_scoreManager != null)
        {
            _scoreManager.AddVolatility(discardHeatCost);
        }

        MoveSelectedToDiscard();
        RefillHand();
        UpdateDeckCountUI();
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
        UpdateDeckCountUI();
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
    public void RerollOpeningHand()
    {
        // 1. Return hand to Draw Pile
        drawPile.AddRange(hand);
        hand.Clear();

        // 2. Clear Visuals
        foreach (Transform child in handContainer) Destroy(child.gameObject);

        // 3. Shuffle & Redraw
        ShuffleDrawPile();
        RefillHand();

        // 4. Disable Button (One use only)
        if (rerollButton != null) rerollButton.SetActive(false);

        Debug.Log("Pre-Market Reroll used.");
    }
    private void UpdateDeckCountUI()
    {
        if (deckCountText != null)
        {
            // Total cards = Draw Pile + Hand + Discard Pile
            int total = drawPile.Count + hand.Count + discardPile.Count;
            deckCountText.text = $"DECK: {total}";
        }
    }
}