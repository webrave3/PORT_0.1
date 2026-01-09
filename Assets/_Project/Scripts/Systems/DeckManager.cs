using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DeckManager : MonoBehaviour
{
    [Header("Configuration")]
    public List<CardData> masterDeck = new List<CardData>();
    public int maxHandSize = 8;        // RAM (How many you hold)
    public int maxSelectionSize = 5;   // Bandwidth (How many you play)

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

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.RunDeck.Count > 0)
            {
                masterDeck.Clear();
                masterDeck.AddRange(GameManager.Instance.RunDeck);
            }
            else
            {
                GameManager.Instance.RunDeck.Clear();
                GameManager.Instance.RunDeck.AddRange(masterDeck);
            }
        }
        InitializeDeck();
    }

    public void InitializeDeck()
    {
        // Re-sync if needed (redundant safety)
        if (GameManager.Instance != null && GameManager.Instance.RunDeck.Count > 0)
        {
            masterDeck.Clear();
            masterDeck.AddRange(GameManager.Instance.RunDeck);
        }

        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();
        selectedCards.Clear();

        foreach (Transform child in handContainer) Destroy(child.gameObject);

        drawPile.AddRange(masterDeck);
        ShuffleDrawPile();
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

    // --- SELECTION LOGIC (UPDATED) ---
    public bool CanSelectMore()
    {
        return selectedCards.Count < maxSelectionSize;
    }

    // ... inside DeckManager class ...

    public void SelectCard(CardData card)
    {
        if (!selectedCards.Contains(card) && CanSelectMore())
        {
            selectedCards.Add(card);
            // NEW: Update UI Preview
            if (_scoreManager != null) _scoreManager.UpdateHandPreview(selectedCards);
        }
    }

    public void DeselectCard(CardData card)
    {
        if (selectedCards.Contains(card))
        {
            selectedCards.Remove(card);
            // NEW: Update UI Preview
            if (_scoreManager != null) _scoreManager.UpdateHandPreview(selectedCards);
        }
    }

    // Ensure PlaySelectedHand clears the preview
    public void PlaySelectedHand()
    {
        if (selectedCards.Count == 0) return;
        if (_scoreManager != null && !_scoreManager.TryConsumeHand()) return;

        // Calculate and Commit
        if (_scoreManager != null)
        {
            int score = _scoreManager.CalculateAndCommitScore(selectedCards);
            if (TurnManager.Instance != null) TurnManager.Instance.OnHandPlayed(score);
        }

        MoveSelectedToDiscard();
        RefillHand();

        // Clear Preview
        if (_scoreManager != null) _scoreManager.UpdateHandPreview(selectedCards);
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

        // Refresh visuals to remove the gaps
        foreach (Transform child in handContainer) Destroy(child.gameObject);
        foreach (CardData data in hand) SpawnCardVisual(data);

        UpdateDeckCountUI();
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

    // --- FUSION ---
    public bool AttemptFusion(CardInteraction source, CardInteraction target)
    {
        CardData sourceData = source.GetData();
        CardData targetData = target.GetData();

        if (sourceData != targetData) return false;
        if (sourceData.nextTierCard == null) return false;

        hand.Remove(sourceData);
        hand.Remove(targetData);
        hand.Add(sourceData.nextTierCard);

        Destroy(source.gameObject);
        Destroy(target.gameObject);
        SpawnCardVisual(sourceData.nextTierCard);

        UpdateDeckCountUI();
        return true;
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