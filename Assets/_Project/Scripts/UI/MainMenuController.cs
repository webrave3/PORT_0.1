using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    

    public void OnStartGameClicked()
    {
        // 1. Generate Deck using the Library
        List<CardData> starterDeck = CardLibrary.GetStarterDeck();

        // 2. Pass to GameManager (The one spawned by Bootstrapper)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewRun(starterDeck);
        }
        else
        {
            Debug.LogError("GameManager is missing! Did you start from Bootstrapper?");
        }

        // 3. Load GameLoop
        SceneManager.LoadScene("02_GameLoop");
    }
}