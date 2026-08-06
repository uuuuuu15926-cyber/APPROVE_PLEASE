using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class DeskManager : MonoBehaviour
{
    [Header("Spawn Area")]
    [SerializeField] private RectTransform spawnArea; // center of desk

    [Header("Document Prefab")]
    [SerializeField] private GameObject documentPrefab; // a Panel with Image, TextMeshProUGUI children

    [Header("Layout Offsets")]
    [SerializeField] private Vector2 profileOffset   = new(-250f, 50f);
    [SerializeField] private Vector2 proposalOffset  = new(0f, 0f);
    [SerializeField] private Vector2 evidenceOffset  = new(250f, -30f);

    [Header("Sweep Animation")]

    private CaseData _currentCase;
    private GameObject[] _activeDocuments = new GameObject[3];
    private bool _isProcessing = false;

    public void SpawnNextCase()
    {
        if (_isProcessing) return;

        _currentCase = CaseGenerator.Generate(GameManager.Instance.CurrentDay);

        SpawnDocument(_currentCase.profile, "<b>사원 프로필</b>", profileOffset, 0);
        SpawnDocument(_currentCase.proposal, "<b>결재 기안서</b>", proposalOffset, 1);
        SpawnDocument(_currentCase.evidence, "<b>증빙 서류</b>", evidenceOffset, 2);
    }

    private void SpawnDocument<T>(T data, string header, Vector2 offset, int index)
    {
        GameObject doc = Instantiate(documentPrefab, spawnArea);
        RectTransform rt = doc.GetComponent<RectTransform>();
        rt.anchoredPosition = offset;

        // Build the text content
        string content = header + "\n" + new string('─', 16) + "\n";

        if (data is HRProfileData p)
        {
            content += $"성명: {p.fullName}\n" +
                       $"부서: {p.department}\n" +
                       $"사번: {p.employeeID}\n" +
                       $"보안 등급: {p.securityLevel}등급\n" +
                       $"단말기 IP: {p.workstationIP}";
        }
        else if (data is ActionProposalData a)
        {
            content += $"일자: {a.date}\n" +
                       $"건명: {a.requestTitle}\n" +
                       $"금액: {a.requestedAmount}\n" +
                       $"거래처/대상: {a.vendorOrTarget}\n" +
                       $"필요 보안등급: {a.requiredAuthLevel}등급\n" +
                       $"기안자 서명: {a.signatureName}";
        }
        else if (data is EvidenceData e)
        {
            content += $"종류: {e.evidenceType}\n\n";
            foreach (string line in e.lines)
                content += line + "\n";
        }

        // Find the TMP child and set text
        TMP_Text txt = doc.GetComponentInChildren<TMP_Text>();
        if (txt != null) txt.text = content;

        // Tag it so the stamp knows this is the proposal (index 1)
        doc.name = $"Doc_{header}_{index}";
        doc.tag = index == 1 ? "ProposalDoc" : "Untagged";

        _activeDocuments[index] = doc;
    }

    /// <summary>
    /// Called by StampTool when a stamp lands.
    /// </summary>
    public void EvaluateAndSweep(bool approved)
    {
        if (_isProcessing || _currentCase == null) return;
        _isProcessing = true;

        bool correct = approved == !_currentCase.isInnocent;
        // correct: approved innocent OR rejected villain

        GameManager.Instance.SubmitDecision(correct);

        StartCoroutine(SweepDocuments(() =>
        {
            _currentCase = null;
            _isProcessing = false;

            // If game is still active, spawn next
            if (GameManager.Instance.IsDayActive)
                SpawnNextCase();
        }));
    }

    private IEnumerator SweepDocuments(System.Action onComplete)
    {
        Vector3 target = new Vector3(Screen.width + 400f, 0f, 0f);
        float elapsed = 0f;
        Vector3[] startPositions = new Vector3[3];

        for (int i = 0; i < 3; i++)
        {
            if (_activeDocuments[i] != null)
                startPositions[i] = _activeDocuments[i].transform.position;
        }

        while (elapsed < 0.35f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.35f;
            t = t * t; // ease in

            for (int i = 0; i < 3; i++)
            {
                if (_activeDocuments[i] != null)
                    _activeDocuments[i].transform.position = Vector3.Lerp(startPositions[i], target, t);
            }
            yield return null;
        }

        for (int i = 0; i < 3; i++)
        {
            if (_activeDocuments[i] != null)
            {
                Destroy(_activeDocuments[i]);
                _activeDocuments[i] = null;
            }
        }

        onComplete?.Invoke();
    }

    public void ClearDesk()
    {
        for (int i = 0; i < 3; i++)
        {
            if (_activeDocuments[i] != null)
            {
                Destroy(_activeDocuments[i]);
                _activeDocuments[i] = null;
            }
        }
        _currentCase = null;
        _isProcessing = false;
    }
}