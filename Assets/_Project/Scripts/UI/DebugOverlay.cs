using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.InputSystem; // --- NEW: Required for New Input System ---

public class DebugOverlay : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI debugText;
    public GameObject debugContent;

    private void Update()
    {
        // Safety check: Ensure keyboard exists
        if (Keyboard.current == null) return;

        // Toggle Key: TAB or Tilde (`)
        if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            if (debugContent != null) debugContent.SetActive(!debugContent.activeSelf);
        }

        // Only update logic if visible
        if (debugContent != null && debugContent.activeSelf)
        {
            UpdateVisuals();
            HandleCheats();
        }
    }

    private void UpdateVisuals()
    {
        // 1. Gather Managers
        var gm = GameManager.Instance;
        var sm = FindFirstObjectByType<ScoreManager>();
        var dm = FindFirstObjectByType<DeckManager>();

        if (gm == null || sm == null)
        {
            debugText.text = "Waiting for Managers...";
            return;
        }

        // 2. Format Data
        string bossName = gm.currentQuarterBoss != null ?
            $"<color=red>{gm.currentQuarterBoss.eventName} ({gm.currentQuarterBoss.eventID})</color>" :
            "<color=grey>None</color>";

        string calendar = $"<b>Q{gm.currentQuarter}-M{gm.currentMonth}</b> | Quota: <color=yellow>${gm.currentQuota}</color>";
        string combat = $"Yield: ${sm.currentYield} | Heat: {sm.currentVolatility}/{sm.maxVolatility}%";
        string resources = $"Hands: {sm.handsRemaining} | Shreds: {sm.discardsRemaining}";

        string deck = "Deck: N/A";
        if (dm != null)
        {
            int total = dm.drawPile.Count + dm.hand.Count + dm.discardPile.Count;
            deck = $"Total: {total} (Drw:{dm.drawPile.Count} | Hnd:{dm.hand.Count} | Dsc:{dm.discardPile.Count})";
        }

        // 3. Render
        debugText.text = $"<size=120%><color=green>[DEBUG DASHBOARD]</color></size>\n" +
                         $"--------------------------------\n" +
                         $"CALENDAR: {calendar}\n" +
                         $"FORECAST: {bossName}\n" +
                         $"STATUS:   {combat}\n" +
                         $"ACTION:   {resources}\n" +
                         $"CARDS:    {deck}\n" +
                         $"--------------------------------\n" +
                         $"<size=80%><color=orange>CHEATS:</color>\n" +
                         $"[1] Force BEAR BOSS  [2] Force AUDIT BOSS\n" +
                         $"[W] Force WIN Round  [L] Force LOSE Round\n" +
                         $"[R] Reload Scene     [M] Main Menu</size>";
    }

    private void HandleCheats()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // --- BOSS CHEATS ---
        // Press 1 or NumPad 1
        if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) ForceBoss("BEAR");
        // Press 2 or NumPad 2
        if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) ForceBoss("AUDIT");

        // --- GAMEPLAY CHEATS ---
        var sm = FindFirstObjectByType<ScoreManager>();

        // Force Win [W]
        if (kb.wKey.wasPressedThisFrame && sm != null)
        {
            sm.AddScore(sm.currentQuota - sm.currentYield + 1);
            Debug.Log("👨‍💻 CHEAT: Instant Win");
        }

        // Force Lose [L]
        if (kb.lKey.wasPressedThisFrame && sm != null)
        {
            sm.handsRemaining = 0;
            sm.TryConsumeHand();
            Debug.Log("👨‍💻 CHEAT: Instant Loss");
        }

        // Reload [R]
        if (kb.rKey.wasPressedThisFrame) SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Menu [M]
        if (kb.mKey.wasPressedThisFrame) SceneManager.LoadScene("01_MainMenu");
    }

    private void ForceBoss(string id)
    {
        var allEvents = Resources.LoadAll<MarketEventData>("Events");
        var target = allEvents.FirstOrDefault(e => e.eventID == id);
        if (target != null && GameManager.Instance != null)
        {
            GameManager.Instance.currentQuarterBoss = target;
        }
    }
}