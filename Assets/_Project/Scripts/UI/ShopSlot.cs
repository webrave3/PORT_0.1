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

    [Header("Runtime Data")]
    private CardData _data;
    private int _price;
    private bool _isPurchased = false;
    private System.Action<ShopSlot> _onClickCallback;

    public void SetupForSale(CardData data, int price, System.Action<ShopSlot> onClick)
    {
        _data = data;
        _price = price;
        _onClickCallback = onClick;
        _isPurchased = false;

        // Visuals
        if (cardDisplay != null) cardDisplay.Setup(data);
        if (priceText != null) priceText.text = $"${price}";
        if (priceTagGroup != null) priceTagGroup.SetActive(true);
        if (dimmerOverlay != null) dimmerOverlay.gameObject.SetActive(false);
    }

    public void SetupForDeck(CardData data, System.Action<ShopSlot> onClick)
    {
        _data = data;
        _onClickCallback = onClick;

        // Visuals for owned items (No price tag)
        if (cardDisplay != null) cardDisplay.Setup(data);
        if (priceTagGroup != null) priceTagGroup.SetActive(false);
        if (dimmerOverlay != null) dimmerOverlay.gameObject.SetActive(false);
    }

    public void MarkAsSold()
    {
        _isPurchased = true;
        if (dimmerOverlay != null) dimmerOverlay.gameObject.SetActive(true);
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

    // Reuse your existing hover logic from CardInteraction if you want, 
    // or keep it simple here:
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isPurchased) transform.localScale = Vector3.one * 1.05f;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }

    public CardData GetData() => _data;
    public int GetPrice() => _price;
}