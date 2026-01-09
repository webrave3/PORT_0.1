using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI yieldText;
    public TextMeshProUGUI descText;

    [Header("Visual Components")]
    public Image cardBackground; // Link the main panel/image here
    public Image illegalBorder;  // Optional: A red outline for illegal cards

    // The data this object represents
    private CardData _data;

    public void Setup(CardData data)
    {
        _data = data;

        // 1. Text Setup
        nameText.text = _data.cardName;
        costText.text = "$" + _data.cashCost;
        yieldText.text = _data.baseYield.ToString();
        descText.text = _data.description;

        // 2. Color Coding (Procedural Art)
        switch (_data.sector)
        {
            case "Tech":
                cardBackground.color = new Color32(0, 150, 255, 255); // Neon Blue
                break;
            case "Finance":
                cardBackground.color = new Color32(50, 200, 100, 255); // Green
                break;
            case "Energy":
                cardBackground.color = new Color32(255, 200, 0, 255); // Yellow/Orange
                break;
            case "Crypto":
                cardBackground.color = new Color32(150, 0, 255, 255); // Purple
                break;
            case "Bonds":
                cardBackground.color = new Color32(200, 200, 200, 255); // White/Grey
                break;
            case "Illegal":
                cardBackground.color = new Color32(50, 50, 50, 255); // Dark Grey
                break;
            default:
                cardBackground.color = Color.white;
                break;
        }

        // 3. Illegal Flag (Optional visual cue)
        if (illegalBorder != null)
        {
            illegalBorder.enabled = _data.isIllegal;
            if (_data.isIllegal)
            {
                nameText.color = Color.red; // Make text scary red
            }
        }
    }
    public CardData GetData()
    {
        return _data;
    }
}