using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Added Drag handlers
[RequireComponent(typeof(CanvasGroup))]
public class CardInteraction : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Settings")]
    public float selectOffset = 40f;
    public float hoverScale = 1.1f;

    private bool _isSelected = false;
    private RectTransform _rectTransform;
    private CardDisplay _display;
    private DeckManager _deckManager;
    private CanvasGroup _canvasGroup;
    private Transform _startParent;
    private Canvas _mainCanvas;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        // --- FIX: Search Children ---
        _display = GetComponentInChildren<CardDisplay>();

        _canvasGroup = GetComponent<CanvasGroup>();
        _deckManager = FindFirstObjectByType<DeckManager>();
        _mainCanvas = GetComponentInParent<Canvas>();
    }

    // --- Helper to expose Data to other scripts ---
    public CardData GetData()
    {
        return _display.GetData();
    }

    // --- HOVER LOGIC ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.dragging) return; // Don't hover scale if we are dragging something
        if (_isSelected) return;
        transform.localScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isSelected) return;
        transform.localScale = Vector3.one;
    }

    // --- CLICK LOGIC ---
    public void OnPointerClick(PointerEventData eventData)
    {
        // If we were dragging, don't register this as a click
        if (eventData.dragging) return;
        ToggleSelection();
    }

    private void ToggleSelection()
    {
        if (!_isSelected)
        {
            // NEW: Check Limit
            if (_deckManager != null && !_deckManager.CanSelectMore())
            {
                // Optional: Add a "Buzz" sound or red flash here
                Debug.Log("Bandwidth Full! Deselect a card first.");
                return; // Stop here, do not select
            }

            _isSelected = true;
            _rectTransform.anchoredPosition += new Vector2(0, selectOffset);
            if (_deckManager != null) _deckManager.SelectCard(_display.GetData());
        }
        else
        {
            _isSelected = false;
            _rectTransform.anchoredPosition -= new Vector2(0, selectOffset);
            transform.localScale = Vector3.one;
            if (_deckManager != null) _deckManager.DeselectCard(_display.GetData());
        }
    }

    // --- DRAG LOGIC ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isSelected) ToggleSelection(); // Deselect if we start dragging

        _startParent = transform.parent;

        // Reparent to the root Canvas so it draws ON TOP of everything else
        if (_mainCanvas != null) transform.SetParent(_mainCanvas.transform, true);

        // Turn off Raycasts so the mouse sees what is BEHIND the card (the drop target)
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Follow the mouse
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;

        // If the object was destroyed by fusion, this script instance might be invalid
        if (this == null || gameObject == null) return;

        // If we didn't fuse, return to the hand layout
        transform.SetParent(_startParent, true);

        // The LayoutGroup will fix the position automatically next frame
    }

    // --- DROP LOGIC (The Receiver) ---
    public void OnDrop(PointerEventData eventData)
    {
        // Check if the thing dropped on us is a Card
        CardInteraction droppedCard = eventData.pointerDrag.GetComponent<CardInteraction>();

        if (droppedCard != null && droppedCard != this)
        {
            if (_deckManager != null)
            {
                // Try to fuse. If successful, DeckManager handles the destruction.
                _deckManager.AttemptFusion(droppedCard, this);
            }
        }
    }
}