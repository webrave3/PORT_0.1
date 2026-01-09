using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Needed for Button

public class DraftScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject draftPanelUI; // DRAG YOUR DRAFT PANEL HERE
    public GameObject cardPrefab;
    public Transform container;

    [Header("Data")]
    public List<CardData> allPossibleCards;

    public void ShowDraft()
    {
        // 1. Turn on the Visual Panel
        if (draftPanelUI != null) draftPanelUI.SetActive(true);
        else Debug.LogError("Draft Panel UI is not linked in Inspector!");

        // 2. Clear old cards from the container
        foreach (Transform child in container) Destroy(child.gameObject);

        // 3. Pick 3 Random Cards
        for (int i = 0; i < 3; i++)
        {
            if (allPossibleCards.Count == 0) break;

            CardData randomCard = allPossibleCards[Random.Range(0, allPossibleCards.Count)];

            GameObject cardObj = Instantiate(cardPrefab, container);

            // Setup Visuals
            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            if (display != null) display.Setup(randomCard);

            // Add Click Listener
            Button btn = cardObj.AddComponent<Button>();
            btn.onClick.AddListener(() => SelectCard(randomCard));
        }
    }

    void SelectCard(CardData card)
    {
        Debug.Log($"Drafted: {card.cardName}");

        // Add to Persistent Deck
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RunDeck.Add(card);
            GameManager.Instance.CurrentQuota += 50;
        }

        // Restart Scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}