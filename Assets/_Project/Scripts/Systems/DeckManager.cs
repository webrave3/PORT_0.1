using UnityEngine;
using System.Collections.Generic;
using TMPro; // Needed for UI references

public class DeckManager : MonoBehaviour
{
    [Header("Configuration")]
    public List<CardData> masterDeck = new List<CardData>();
    public int maxHandSize = 8;

    [Header("Visual References")]
    public GameObject cardPrefab;
    public Transform handContainer;
    public GameObject rerollButton;
    public TextMeshProUGUI deckCountText;

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

        if (GameManager.Instance != null && GameManager.Instance.RunDeck.Count > 0)
        {
            masterDeck.Clear();
            masterDeck.AddRange(GameManager.Instance.RunDeck);
        }

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

    public void RefillHand()
    {
        int cardsNeeded = maxHandSize - hand.Count;
        if (cardsNeeded > 0) DrawCard(cardsNeeded);
        UpdateDeckCountUI();
    }

    public void DrawCard(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count > 0) ReshuffleDiscard();
                else return;
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

    // --- FUSION MECHANIC ---
    public bool AttemptFusion(CardInteraction sourceCard, CardInteraction targetCard)
    {
        CardData sourceData = sourceCard.GetData();
        CardData targetData = targetCard.GetData();

        // 1. Validation: Must be same card, must have upgrade
        if (sourceData != targetData) return false;
        if (sourceData.nextTierCard == null)
        {
            Debug.Log("Asset is already a Monopoly (Max Tier).");
            return false;
        }

        // 2. Logic: Remove 2 Old, Add 1 New
        hand.Remove(sourceData); // Removes first instance
        hand.Remove(targetData); // Removes second instance
        hand.Add(sourceData.nextTierCard);

        // 3. Visuals: Destroy old objects, spawn new one
        // We defer destruction slightly to let the drag event finish cleanly
        Destroy(sourceCard.gameObject);
        Destroy(targetCard.gameObject);

        SpawnCardVisual(sourceData.nextTierCard);
        UpdateDeckCountUI();

        Debug.Log($"MERGER EXECUTED: {sourceData.cardName} + {targetData.cardName} -> {sourceData.nextTierCard.cardName}");
        return true;
    }

    // --- SELECTION & SCORING ---
    public void SelectCard(CardData card)
    {
        if (!selectedCards.Contains(card)) selectedCards.Add(card);
    }

    public void DeselectCard(CardData card)
    {
        if (selectedCards.Contains(card)) selectedCards.Remove(card);
    }

    public void PlaySelectedHand()
    {
        if (rerollButton != null) rerollButton.SetActive(false);
        if (selectedCards.Count == 0) return;
        if (_scoreManager != null && !_scoreManager.TryConsumeHand()) return;

        int handScore = 0;
        int handVol = 0;

        foreach (var card in selectedCards)
        {
            handScore += card.baseYield;
            handVol += card.volatility;
        }

        if (_scoreManager != null)
        {
            _scoreManager.AddVolatility(handVol);
            _scoreManager.AddScore(handScore);
        }

        MoveSelectedToDiscard();
        RefillHand();
    }

    public void DiscardSelectedHand()
    {
        if (rerollButton != null) rerollButton.SetActive(false);
        if (selectedCards.Count == 0) return;
        if (_scoreManager != null && !_scoreManager.TryConsumeDiscard()) return;

        int discardHeatCost = selectedCards.Count * 2;
        if (_scoreManager != null) _scoreManager.AddVolatility(discardHeatCost);

        MoveSelectedToDiscard();
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
        drawPile.AddRange(hand);
        hand.Clear();
        foreach (Transform child in handContainer) Destroy(child.gameObject);
        ShuffleDrawPile();
        RefillHand();
        if (rerollButton != null) rerollButton.SetActive(false);
    }

    private void UpdateDeckCountUI()
    {
        if (deckCountText != null)
        {
            int total = drawPile.Count + hand.Count + discardPile.Count;
            deckCountText.text = $"DECK: {total}";
        }
    }
}