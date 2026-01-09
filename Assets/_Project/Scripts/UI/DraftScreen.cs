using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DraftScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject draftPanelUI;
    public GameObject cardPrefab;
    public Transform container;

    public void ShowDraft()
    {
        if (draftPanelUI != null) draftPanelUI.SetActive(true);
        foreach (Transform child in container) Destroy(child.gameObject);

        // --- FIX: Only Get Tier 1 Cards ---
        List<CardData> pool = CardLibrary.GetDraftPool();
        // ----------------------------------

        for (int i = 0; i < 3; i++)
        {
            if (pool.Count == 0) break;
            CardData randomCard = pool[Random.Range(0, pool.Count)];

            GameObject cardObj = Instantiate(cardPrefab, container);
            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            if (display != null) display.Setup(randomCard);

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

            // --- REMOVED: Manual Quota Increase ---
            // GameManager.Instance.currentQuota += 50; 
            // The Calendar system in GameManager now handles this automatically.
        }

        // Restart Scene (In the future, this will go to the next battle)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}