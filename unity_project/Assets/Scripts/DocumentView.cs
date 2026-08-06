using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(UnityEngine.UI.Image))]
public class DocumentView : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _rect;
    private Canvas _parentCanvas;
    private Vector2 _dragOffset;
    private bool _dragging;

    private static int _topSortOrder = 10;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Bring to front
        _topSortOrder++;
        GetComponent<RectTransform>().SetAsLastSibling();

        _dragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out _dragOffset);
        _dragOffset = _rect.anchoredPosition - _dragOffset;

        AudioManager.Instance?.Play("PaperShuffle");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_dragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        _rect.anchoredPosition = localPoint + _dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _dragging = false;
    }
}