using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class CardLibrary
{
    private static List<CardData> _allCards;

    // Load everything (Internal use)
    private static List<CardData> GetAllCards()
    {
        if (_allCards == null)
        {
            _allCards = Resources.LoadAll<CardData>("Cards").ToList();
        }
        return _allCards;
    }

    // --- 1. STARTER DECK: Basic "Generic" cards only ---
    public static List<CardData> GetStarterDeck()
    {
        // Get all Tier 1 Generic cards (Penny Stocks, Savings Bonds, etc.)
        List<CardData> generics = GetAllCards()
            .Where(c => c.tier == 1 && c.sector == "Generic")
            .ToList();

        // If we have none, fallback to any Tier 1
        if (generics.Count == 0) generics = GetAllCards().Where(c => c.tier == 1).ToList();

        // Create a deck of 8 cards
        List<CardData> deck = new List<CardData>();
        for (int i = 0; i < 8; i++)
        {
            deck.Add(generics[Random.Range(0, generics.Count)]);
        }
        return deck;
    }

    // --- 2. DRAFT POOL: All Tier 1 cards (No Upgraded/Fused cards) ---
    public static List<CardData> GetDraftPool()
    {
        // Exclude "Generic" from draft if you want to force players to specialize, 
        // OR include them. For Balatro style, we usually want "Sectors" now.
        // Rule: Tier 1 ONLY.
        return GetAllCards().Where(c => c.tier == 1).ToList();
    }
}