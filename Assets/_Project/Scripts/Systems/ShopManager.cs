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

    [Header("Laundering")]
    public int launderCost = 20;
    public int launderAmount = 15; // Reduces heat by 15%

    [Header("UI References")]
    public Transform shopContainer; // Where items for sale go
    public Transform deckContainer; // Where your current deck is shown
    public GameObject shopSlotPrefab; // The prefab with ShopSlot.cs

    public TextMeshProUGUI cashText;
    public Button rerollButton;
    public Button removeServiceButton;
    public TextMeshProUGUI rerollCostText;
    public TextMeshProUGUI removeCostText;
    public TextMeshProUGUI feedbackText;

    // NEW: Laundering UI
    public Button launderButton;
    public TextMeshProUGUI launderCostText;

    private bool _isRemoveMode = false;

    private void Start()
    {
        UpdateCashUI();
        GenerateShopInventory();
        LoadPlayerDeck();

        // Setup Buttons
        if (rerollCostText) rerollCostText.text = $"Reroll (${rerollCost})";
        if (removeCostText) removeCostText.text = $"Fire Asset (${removeCardCost})";
        if (launderCostText) launderCostText.text = $"Launder Heat (${launderCost})";

        CheckLaunderButton();
    }

    // --- 1. GENERATE SHOP ---
    public void GenerateShopInventory()
    {
        // Clear existing
        foreach (Transform child in shopContainer) Destroy(child.gameObject);

        // A. SPAWN 3 CARDS
        List<CardData> cardPool = CardLibrary.GetDraftPool();
        for (int i = 0; i < 3; i++)
        {
            if (cardPool.Count == 0) break;
            CardData randomCard = cardPool[Random.Range(0, cardPool.Count)];

            // Calculate randomized price
            int price = Mathf.Max(5, cardBaseCost + Random.Range(-cardVariance, cardVariance + 1));

            // Spawn
            GameObject obj = Instantiate(shopSlotPrefab, shopContainer);
            ShopSlot slot = obj.GetComponent<ShopSlot>();
            slot.SetupForSale(randomCard, price, OnBuyItemClicked);
        }

        // B. SPAWN 1 ADVISOR
        var loadedAdvisors = Resources.LoadAll<AdvisorData>("Advisors");
        Debug.Log($"🔎 Found {loadedAdvisors.Length} Advisors in Resources/Advisors");
        List<AdvisorData> advisorPool = new List<AdvisorData>(loadedAdvisors);

        // Filter out ones we already own (Advisors are unique)
        if (GameManager.Instance != null)
        {
            advisorPool.RemoveAll(x => GameManager.Instance.HasAdvisor(x.id));
        }

        if (advisorPool.Count > 0)
        {
            AdvisorData randomAdvisor = advisorPool[Random.Range(0, advisorPool.Count)];

            // Spawn
            GameObject obj = Instantiate(shopSlotPrefab, shopContainer);
            ShopSlot slot = obj.GetComponent<ShopSlot>();
            slot.SetupForAdvisor(randomAdvisor, randomAdvisor.basePrice, OnBuyItemClicked);
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

    // --- NEW: LAUNDERING LOGIC ---
    public void OnLaunderClicked()
    {
        if (GameManager.Instance == null) return;

        // Check if there is heat to clean
        if (GameManager.Instance.RunHeat <= 0)
        {
            SetFeedback("Records are already clean.");
            return;
        }

        if (TrySpendCash(launderCost))
        {
            GameManager.Instance.RunHeat = Mathf.Clamp(GameManager.Instance.RunHeat - launderAmount, 0, 100);
            SetFeedback($"Records adjusted. Heat -{launderAmount}%.");
            UpdateCashUI();
            CheckLaunderButton(); // Update interactability
        }
        else
        {
            SetFeedback("Insufficient funds for cleanup.");
        }
    }

    private void CheckLaunderButton()
    {
        if (launderButton != null && GameManager.Instance != null)
        {
            // Optional: Disable button if heat is 0
            launderButton.interactable = GameManager.Instance.RunHeat > 0;
        }
    }

    // --- 2. BUYING LOGIC (Cards & Advisors) ---
    private void OnBuyItemClicked(ShopSlot slot)
    {
        int price = slot.GetPrice();

        if (TrySpendCash(price))
        {
            // CASE A: Buying a CARD
            if (slot.GetCard() != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RunDeck.Add(slot.GetCard());
                }
                SetFeedback($"Acquired {slot.GetCard().cardName}!");
                LoadPlayerDeck(); // Refresh deck view
            }
            // CASE B: Buying an ADVISOR
            else if (slot.GetAdvisor() != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ActiveAdvisors.Add(slot.GetAdvisor());
                }
                SetFeedback($"Hired Advisor: {slot.GetAdvisor().advisorName}");
            }

            // Visuals
            slot.MarkAsSold();
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
                    GameManager.Instance.RunDeck.Remove(slot.GetCard());
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
            if (slot.GetCard() != null)
            {
                SetFeedback($"{slot.GetCard().cardName}: {slot.GetCard().description}");
            }
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