using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIController : MonoBehaviour
{
    [Header("HUD References")]
    [SerializeField] private TMP_Text clockText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text quotaText;
    [SerializeField] private Image[] strikeIcons; // 3 red X icons
    [SerializeField] private GameObject rulebookPanel;
    [SerializeField] private TMP_Text rulebookContent;

    [Header("Day Banner")]
    [SerializeField] private GameObject dayBanner; // full-screen overlay
    [SerializeField] private TMP_Text dayBannerText;
    [SerializeField] private float bannerDuration = 2f;

    [Header("Strike Flash")]
    [SerializeField] private GameObject strikeFlash; // red vignette overlay
    [SerializeField] private float flashDuration = 0.6f;

    private DeskManager _desk;

    private void Start()
    {
        _desk = FindAnyObjectByType<DeskManager>();

        GameManager gm = GameManager.Instance;
        gm.OnDayStart.AddListener(HandleDayStart);
        gm.OnQuotaUpdated.AddListener(UpdateQuotaDisplay);
        gm.OnStrikeGiven.AddListener(HandleStrike);
        gm.OnDayEnd.AddListener(HandleDayEnd);
        gm.OnGameOver.AddListener(HandleGameOver);
        gm.OnGameWin.AddListener(HandleGameWin);

        rulebookPanel.SetActive(false);
        BuildRulebookText();
    }

    private void Update()
    {
        if (GameManager.Instance.IsDayActive)
        {
            clockText.text = GameManager.Instance.GetClockString();
            dateText.text = $"{GameManager.Instance.CurrentDay}일차";
        }
    }

    private void HandleDayStart()
    {
        StartCoroutine(ShowBanner($"— {GameManager.Instance.CurrentDay}일차 시작 —", () =>
        {
            _desk.SpawnNextCase();
        }));
        UpdateQuotaDisplay();
        ResetStrikes();
    }

    private void HandleDayEnd()
    {
        _desk.ClearDesk();
        StartCoroutine(ShowBanner("오늘 업무 종료", () =>
        {
            if (GameManager.Instance.CurrentDay <= 5) // still more days
                GameManager.Instance.StartDay();
        }));
    }

    private void HandleStrike()
    {
        // Light up the next strike icon
        int idx = GameManager.Instance.StrikesToday - 1;
        if (idx >= 0 && idx < strikeIcons.Length)
            strikeIcons[idx].gameObject.SetActive(true);

        // Red flash
        if (strikeFlash != null)
        {
            strikeFlash.SetActive(true);
            StartCoroutine(FlashRoutine());
        }

        AudioManager.Instance?.Play("BuzzerError");
    }

    private void ResetStrikes()
    {
        foreach (var icon in strikeIcons)
            icon.gameObject.SetActive(false);
    }

    private void UpdateQuotaDisplay()
    {
        int processed = GameManager.Instance.ProcessedToday;
        quotaText.text = $"처리 건수: {processed}/15";
    }

    private void HandleGameOver()
    {
        _desk.ClearDesk();
        ScreenManager.Instance?.ShowGameOver();
    }

    private void HandleGameWin()
    {
        _desk.ClearDesk();
        ScreenManager.Instance?.ShowWin();
    }

    public void ToggleRulebook()
    {
        rulebookPanel.SetActive(!rulebookPanel.activeSelf);
        AudioManager.Instance?.Play("PaperShuffle");
    }

    private void BuildRulebookText()
    {
        rulebookContent.text =
            "<b>═══ 사내 업무 규정집 ═══</b>\n\n" +
            "<b>§1  블랙리스트 거래처</b>\n" +
            "  • 그림자 중개소\n" +
            "  • 황금껍데기 무역\n" +
            "  • 유령공작소\n\n" +
            "<b>§2  승인 권한 등급</b>\n" +
            "  기안자의 보안 등급이 기안서의\n" +
            "  필요 보안등급 이상이어야 함.\n" +
            "  (미달 시 → 반려)\n\n" +
            "<b>§3  재무 정산 지침</b>\n" +
            "  기안서의 청구 금액과 영수증의\n" +
            "  총 금액이 일치해야 함.\n" +
            "  금액 불일치 → 반려.\n\n" +
            "<b>§4  데이터 보안 규정</b>\n" +
            "  임직원은 소속 부서의\n" +
            "  서버만 접근할 수 있음.\n" +
            "  비정상 데이터 전송 → 반려.\n\n" +
            "<b>§5  QA 검수 지침</b>\n" +
            "  QA 보고서는 변조 흔적이 없어야 함.\n" +
            "  수기 조작 흔적 → 반려.\n" +
            "  유출 IP와 사원 단말기 IP 대조.\n\n" +
            "<b>§6  VIP 추천서 규정</b>\n" +
            "  서명자(회장)의 성명 철자 확인.\n" +
            "  발행일이 1년 이상 지난 추천서는\n" +
            "  만료된 것으로 간주 → 반려.";
    }

    private IEnumerator ShowBanner(string msg, System.Action after)
    {
        GameManager.Instance.PauseGame(true);
        dayBannerText.text = msg;
        dayBanner.SetActive(true);
        yield return new WaitForSeconds(bannerDuration);
        dayBanner.SetActive(false);
        GameManager.Instance.PauseGame(false);
        after?.Invoke();
    }

    private IEnumerator FlashRoutine()
    {
        yield return new WaitForSeconds(flashDuration);
        strikeFlash.SetActive(false);
    }
}