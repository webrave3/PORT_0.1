using UnityEngine;

[CreateAssetMenu(fileName = "NewMarketEvent", menuName = "QuantitativeEasing/Market Event")]
public class MarketEventData : ScriptableObject
{
    [Header("Identity")]
    public string eventName;        // e.g., "Bear Market"
    public string eventID;          // e.g., "BEAR", "AUDIT"
    [TextArea] public string description; // e.g., "All Yields -50%."
    public Sprite icon;             // Optional: For UI later
}