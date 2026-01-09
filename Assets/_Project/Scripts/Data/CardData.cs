using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "QuantitativeEasing/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string cardName;

    [Header("Stats")]
    public string sector;
    public int tier;
    public int cashCost;

    [Header("Combat Values")]
    public int baseYield;
    public int volatility;
    public bool isIllegal;

    [Header("Fusion")]
    public CardData nextTierCard; // DRAG HERE: The card this becomes (e.g., Tech_01 -> Tech_02)

    [Header("Visuals")]
    [TextArea] public string description;
    public Sprite icon;
}