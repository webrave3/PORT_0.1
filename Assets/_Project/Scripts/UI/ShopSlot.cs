using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ShopSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public CardDisplay cardDisplay; // Link to your existing CardDisplay component
    public GameObject priceTagGroup;
    public TextMeshProUGUI priceText;
    public Image dimmerOverlay; // Use this to darken "Sold" items

    // NEW: Reference to a simple Image for Advisor Icons
    public Image advisorIconImage;

    [Header("Runtime Data")]
    private CardData _cardData;
    private AdvisorData _advisorData; // NEW
    private int _price;
    private bool _isPurchased = false;
    private System.Action<ShopSlot> _onClickCallback;

    // --- SETUP FOR CARD SALE ---
    public void SetupForSale(CardData data, int price, System.Action<ShopSlot> onClick)
    {
        _cardData = data;
        _advisorData = null;
        _price = price;
        _onClickCallback = onClick;
        _isPurchased = false;

        // Visuals
        if (cardDisplay != null)
        {
            cardDisplay.gameObject.SetActive(true);
            cardDisplay.Setup(data);
        }
        if (advisorIconImage != null) advisorIconImage.gameObject.SetActive(false);

        if (priceText != null) priceText.text = $"${price}";
        if (priceTagGroup != null) priceTagGroup.SetActive(true);
        if (dimmerOverlay != null) dimmerOverlay.gameObject.SetActive(false);
    }

    // --- SETUP FOR ADVISOR SALE (NEW) ---
    public void SetupForAdvisor(AdvisorData data, int price, System.Action<ShopSlot> onClick)
    {
        _advisorData = data;
        _cardData = null;
        _price = price;
        _onClickCallback = onClick;
        _isPurchased = false;

        // Visuals
        if (cardDisplay != null) cardDisplay.gameObject.SetActive(false); // Hide card frame
        if (advisorIconImage != null)
        {
            advisorIconImage.gameObject.SetActive(true);
            advisorIconImage.sprite = data.icon;
        }

        if (priceText != null) priceText.text = $"${price}";
        if (priceTagGroup != null) priceTagGroup.SetActive(true);
        if (dimmerOverlay != null) dimmerOverlay.gameObject.SetActive(false);
    }

    // --- SETUP FOR DECK ---
    public void SetupForDeck(CardData data, System.Action<ShopSlot> onClick)
    {
        _cardData = data;
        _advisorData = null;
        _onClickCallback = onClick;

        // Visuals for owned items (No price tag)
        if (cardDisplay != null)
        {
            cardDisplay.gameObject.SetActive(true);
            cardDisplay.Setup(data);
        }
        if (advisorIconImage != null) advisorIconImage.gameObject.SetActive(false);

        if (priceTagGroup != null) priceTagGroup.SetActive(false);
        if (dimmerOverlay != null) dimmerOverlay.gameObject.SetActive(false);
    }

    public void MarkAsSold()
    {
        _isPurchased = true;
        if (dimmerOverlay != null)
        {
            dimmerOverlay.gameObject.SetActive(true);
            dimmerOverlay.color = new Color(0, 0, 0, 0.8f); // Darken
        }
        if (priceText != null) priceText.text = "SOLD";
    }

    public void MarkAsRemoved()
    {
        // Visual for when a deck card is fired/removed
        if (dimmerOverlay != null)
        {
            dimmerOverlay.gameObject.SetActive(true);
            dimmerOverlay.color = new Color(1, 0, 0, 0.5f); // Red tint
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isPurchased) return;
        _onClickCallback?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isPurchased) transform.localScale = Vector3.one * 1.05f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }

    // Getters
    public CardData GetCard() => _cardData;
    public AdvisorData GetAdvisor() => _advisorData;
    public int GetPrice() => _price;
}