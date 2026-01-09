using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCard", menuName = "QuantitativeEasing/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Identity")]
    public string id;           // e.g., "TECH_001"
    public string cardName;     // e.g., "Tech Startup"

    [Header("Stats")]
    public string sector;       // Tech, Energy, Finance, etc.
    public int tier;            // 1, 2, or 3
    public int cashCost;        // For shop purchasing

    [Header("Combat Values")]
    public int baseYield;       // Score generated
    public int volatility;      // Heat generated
    public bool isIllegal;      // Triggers Suspicion?

    [Header("Visuals")]
    [TextArea] public string description;
    public Sprite icon;         // We will auto-link this later by name
}