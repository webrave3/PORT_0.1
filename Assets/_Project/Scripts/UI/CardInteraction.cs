using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardInteraction : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    public float selectOffset = 40f; // How high it pops up
    public float hoverScale = 1.1f;  // Slight zoom on hover

    private bool _isSelected = false;
    private RectTransform _rectTransform;
    private CardDisplay _display;
    private DeckManager _deckManager;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _display = GetComponent<CardDisplay>();
        // Find the specific manager in the scene
        _deckManager = FindFirstObjectByType<DeckManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isSelected) return; // Don't scale if already selected
        transform.localScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isSelected) return;
        transform.localScale = Vector3.one;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleSelection();
    }

    private void ToggleSelection()
    {
        _isSelected = !_isSelected;

        if (_isSelected)
        {
            // Move UP
            _rectTransform.anchoredPosition += new Vector2(0, selectOffset);

            // Tell Manager we selected this card
            if (_deckManager != null) _deckManager.SelectCard(_display.GetData());
        }
        else
        {
            // Move DOWN
            _rectTransform.anchoredPosition -= new Vector2(0, selectOffset);
            transform.localScale = Vector3.one; // Reset scale

            // Tell Manager we deselected
            if (_deckManager != null) _deckManager.DeselectCard(_display.GetData());
        }
    }
}