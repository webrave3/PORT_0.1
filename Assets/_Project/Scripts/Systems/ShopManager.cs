using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Configuration")]
    public int cardBaseCost = 10;
    public int cardVariance = 3; // Cost is 10 +/- 3
    public int rerollCost = 5;
    public int removeCardCost = 15;

    [Header("UI References")]
    public Transform shopContainer; // Where items for sale go
    public Transform deckContainer; // Where your current deck is shown
    public GameObject shopSlotPrefab; // The prefab with ShopSlot.cs

    public TextMeshProUGUI cashText;
    public Button rerollButton;
    public Button removeServiceButton;
    public TextMeshProUGUI rerollCostText;
    public TextMeshProUGUI removeCostText;
    public TextMeshProUGUI feedbackText; // "Welcome to the closing bell..."

    private bool _isRemoveMode = false;

    private void Start()
    {
        UpdateCashUI();
        GenerateShopInventory();
        LoadPlayerDeck();

        // Setup Buttons
        if (rerollCostText) rerollCostText.text = $"Reroll (${rerollCost})";
        if (removeCostText) removeCostText.text = $"Fire Asset (${removeCardCost})";
    }

    // --- 1. GENERATE SHOP ---
    public void GenerateShopInventory()
    {
        // Clear existing
        foreach (Transform child in shopContainer) Destroy(child.gameObject);

        // Get 3 Random Cards (Tier 1 for now)
        List<CardData> pool = CardLibrary.GetDraftPool();

        for (int i = 0; i < 3; i++)
        {
            if (pool.Count == 0) break;
            CardData randomCard = pool[Random.Range(0, pool.Count)];

            // Calculate randomized price
            int price = Mathf.Max(5, cardBaseCost + Random.Range(-cardVariance, cardVariance + 1));

            // Spawn
            GameObject obj = Instantiate(shopSlotPrefab, shopContainer);
            ShopSlot slot = obj.GetComponent<ShopSlot>();
            slot.SetupForSale(randomCard, price, OnBuyCardClicked);
        }
    }

    public void OnRerollClicked()
    {
        if (TrySpendCash(rerollCost))
        {
            GenerateShopInventory();
            SetFeedback("Market Refreshed.");
        }
        else
        {
            SetFeedback("Insufficient Funds for Reroll.");
        }
    }

    // --- 2. BUYING LOGIC ---
    private void OnBuyCardClicked(ShopSlot slot)
    {
        int price = slot.GetPrice();

        if (TrySpendCash(price))
        {
            // Add to Persistent Deck
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RunDeck.Add(slot.GetData());
            }

            // Visuals
            slot.MarkAsSold();
            LoadPlayerDeck(); // Refresh deck view to show new card
            SetFeedback($"Acquired {slot.GetData().cardName}!");
        }
        else
        {
            SetFeedback("Insufficient Funds.");
        }
    }

    // --- 3. REMOVAL LOGIC ---
    public void OnRemoveServiceClicked()
    {
        // Toggle Mode
        _isRemoveMode = !_isRemoveMode;

        if (_isRemoveMode)
        {
            SetFeedback($"<color=red>SELECT AN ASSET TO FIRE (Cost: ${removeCardCost})</color>");
            // Optional: Highlight deck UI
        }
        else
        {
            SetFeedback("Restructuring Cancelled.");
        }
    }

    private void OnDeckCardClicked(ShopSlot slot)
    {
        if (_isRemoveMode)
        {
            // Try to remove
            if (TrySpendCash(removeCardCost))
            {
                // Remove from Data
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RunDeck.Remove(slot.GetData());
                }

                // Visuals
                Destroy(slot.gameObject); // Poof
                _isRemoveMode = false; // Turn off mode after one use
                SetFeedback($"Asset Liquidated.");
                UpdateCashUI();
            }
            else
            {
                SetFeedback("Cannot afford severance package.");
                _isRemoveMode = false;
            }
        }
        else
        {
            // Just viewing
            SetFeedback($"{slot.GetData().cardName}: {slot.GetData().description}");
        }
    }

    // --- HELPERS ---
    private void LoadPlayerDeck()
    {
        if (GameManager.Instance == null) return;

        foreach (Transform child in deckContainer) Destroy(child.gameObject);

        // Sort by Sector just to be nice
        List<CardData> myDeck = new List<CardData>(GameManager.Instance.RunDeck);
        myDeck.Sort((a, b) => a.sector.CompareTo(b.sector));

        foreach (var card in myDeck)
        {
            GameObject obj = Instantiate(shopSlotPrefab, deckContainer);
            ShopSlot slot = obj.GetComponent<ShopSlot>();
            slot.SetupForDeck(card, OnDeckCardClicked);
        }
    }

    private bool TrySpendCash(int amount)
    {
        if (GameManager.Instance != null && GameManager.Instance.RunCash >= amount)
        {
            GameManager.Instance.RunCash -= amount;
            UpdateCashUI();
            return true;
        }
        return false;
    }

    private void UpdateCashUI()
    {
        if (GameManager.Instance != null && cashText != null)
        {
            cashText.text = $"CASH: ${GameManager.Instance.RunCash}";
        }
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText) feedbackText.text = msg;
    }

    // --- NAVIGATION ---
    public void OnNextRoundClicked()
    {
        // Go back to Game Loop
        SceneManager.LoadScene("02_GameLoop");
    }
}