using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    [Header("UI References (Loss Screen)")]
    public GameObject resultsPanel;      // The black panel for Game Over
    public TextMeshProUGUI titleText;    // "INSOLVENT"
    public TextMeshProUGUI subText;      // "Yield: $0"
    public GameObject continueButton;    // The button to restart

    [Header("UI References (Win Screen)")]
    public DraftScreen draftScreen;      // Reference to the script we just made

    public void GameOver(bool isWin, int finalYield)
    {
        if (isWin)
        {
            // --- WIN LOGIC ---
            // Hide Game Over panel just in case
            if (resultsPanel) resultsPanel.SetActive(false);

            // Show the Draft Selection Screen
            if (draftScreen != null)
            {
                draftScreen.ShowDraft();
            }
            else
            {
                Debug.LogError("DraftScreen is not assigned in the Inspector!");
            }
        }
        else
        {
            // --- LOSS LOGIC ---
            if (resultsPanel != null)
            {
                resultsPanel.SetActive(true);

                // Style for Bankruptcy
                if (titleText)
                {
                    titleText.text = "INSOLVENT";
                    titleText.color = Color.red;
                }

                if (subText)
                {
                    subText.text = $"Chapter 11 Bankruptcy Declared.\nFinal Yield: ${finalYield}";
                }

                // Update button text to indicate a full restart
                if (continueButton)
                {
                    TextMeshProUGUI btnText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText) btnText.text = "RESTRUCTURE (RESTART)";
                }
            }
        }
    }

    // Linked to the Button on the Results (Loss) Panel
    public void OnContinuePressed()
    {
        // Simply reload the scene to try again
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}