using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    public List<CardData> starterDeck; // Drag your 20 starting cards here in Inspector

    public void OnStartGamePressed()
    {
        // 1. Initialize the Run Data
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewRun(starterDeck);
        }

        // 2. Load the Game
        SceneManager.LoadScene(2);
    }
}