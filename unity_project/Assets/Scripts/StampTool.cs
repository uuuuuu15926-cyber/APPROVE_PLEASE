using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StampTool : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private bool isApprove; // true = green APPROVE, false = red REJECT
    [SerializeField] private Sprite stampMarkSprite; // the mark left on paper
    [SerializeField] private Color stampColor;

    private RectTransform _rect;
    private Canvas _canvas;
    private Vector2 _origin;
    private Vector2 _dragOffset;
    private bool _dragging;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _origin = _rect.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _dragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out _dragOffset);
        _dragOffset = _rect.anchoredPosition - _dragOffset;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_dragging) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 lp);
        _rect.anchoredPosition = lp + _dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_dragging) return;
        _dragging = false;

        // Raycast: did we land on the Proposal document?
        PointerEventData pe = new PointerEventData(EventSystem.current);
        pe.position = eventData.position;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pe, results);

        bool hitProposal = false;
        GameObject proposalDoc = null;

        foreach (var r in results)
        {
            if (r.gameObject.CompareTag("ProposalDoc"))
            {
                hitProposal = true;
                proposalDoc = r.gameObject;
                break;
            }
        }

        if (hitProposal && proposalDoc != null)
        {
            // Place stamp mark on the document
            PlaceStampMark(proposalDoc);

            AudioManager.Instance?.Play("StampHeavy");

            // Tell DeskManager to evaluate
            DeskManager desk = FindAnyObjectByType<DeskManager>();
            desk?.EvaluateAndSweep(isApprove);
        }
        else
        {
            // Didn't hit a valid target, snap back
        }

        // Return stamp to origin
        _rect.anchoredPosition = _origin;
    }

    private void PlaceStampMark(GameObject doc)
    {
        GameObject mark = new GameObject("StampMark", typeof(Image));
        mark.transform.SetParent(doc.transform, false);

        Image img = mark.GetComponent<Image>();
        img.sprite = stampMarkSprite;
        img.color = stampColor;

        RectTransform mrt = mark.GetComponent<RectTransform>();
        mrt.anchoredPosition = Vector2.zero;
        mrt.sizeDelta = new Vector2(160f, 60f);
        mrt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-15f, 15f));
    }
}