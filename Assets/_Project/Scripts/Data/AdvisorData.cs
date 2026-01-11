using UnityEngine;

[CreateAssetMenu(menuName = "QuantitativeEasing/Advisor")]
public class AdvisorData : ScriptableObject
{
    [Header("Identity")]
    public string advisorName;       // e.g., "The Lobbyist"
    public string id;                // e.g., "LOBBYIST"
    [TextArea] public string description; // "Tech cards earn +50% Yield."
    public Sprite icon;              // Face of the advisor

    [Header("Economy")]
    public int basePrice = 50;       // Advisors are expensive!
}