using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance { get; private set; }

    [Header("Screens (full-screen panels)")]
    [SerializeField] private GameObject introScreen;
    [SerializeField] private GameObject tutorialScreen;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject pauseScreen;

    [Header("Buttons")]
    [SerializeField] private Button introStartBtn;
    [SerializeField] private Button tutorialNextBtn;
    [SerializeField] private Button tutorialBackBtn;
    [SerializeField] private Button gameOverRetryBtn;
    [SerializeField] private Button winRestartBtn;
    [SerializeField] private Button pauseResumeBtn;

    [Header("Tutorial Pages")]
    [SerializeField] private TMP_Text tutorialText;

    private int _tutorialPage = 0;

    private readonly string[] _tutorialPages = new[]
    {
        "<b>결재 승인관님, 환영합니다!</b>\n\n" +
        "당신은 '메리디안 코퍼레이션'의 결재 데스크를 맡게 되었습니다.\n" +
        "매일 직원들이 수많은 결재 안건을 제출합니다.\n" +
        "당신의 임무: <b>정당한 안건은 승인</b>하고,\n" +
        "<b>수상한 사기/비리 안건은 반려</b>하는 것입니다.\n\n" +
        "사건마다 총 3장의 서류가 지급됩니다.\n" +
        "서류들을 꼼꼼하게 교차 검증하십시오.",

        "<b>지급 문서 안내</b>\n\n" +
        "① <b>사원 프로필</b> – 신원 정보.\n" +
        "   성명, 부서, 사번, 보안 등급이 기재되어 있습니다.\n\n" +
        "② <b>결재 기안서</b> – 신청 서류.\n" +
        "   무엇을 청구하는지, 누구의 기안인지 명시됩니다.\n" +
        "   <color=yellow>이 문서 위에 도장을 찍어야 합니다.</color>\n\n" +
        "③ <b>첨부 증빙 서류</b> – 증거 자료.\n" +
        "   영수증, 서버 접근 기록, 보고서, 추천서 등.",

        "<b>도장 찍는 방법</b>\n\n" +
        "<color=green>초록색 [승인]</color> 도장 또는\n" +
        "<color=red>빨간색 [반려]</color> 도장을 드래그하여\n" +
        "<b>결재 기안서</b> 문서 위에 내려놓으세요.\n\n" +
        "도장을 찍으면 판정이 완료되고 서류가 처리됩니다.\n" +
        "곧바로 다음 사건의 서류가 들어옵니다.",

        "<b>생존 수칙</b>\n\n" +
        "• 잘못된 판정 = <color=red>경고 (STRIKE)</color> 부여.\n" +
        "• 하루 경고 3회 누적 = <color=red>즉시 해고!</color>\n" +
        "• 하루 목표 처리량 15건을 완수하세요.\n" +
        "• 5일 동안 살아남으면 승리합니다.\n\n" +
        "좌측 하단의 <b>업무 규정집 (RULEBOOK)</b>을 열어\n" +
        "블랙리스트와 상세 검수 지침을 확인하세요.\n\n" +
        "<i>아무도 믿지 마세요. 오직 서류로만 말합니다.</i>"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        HideAll();
        introScreen.SetActive(true);

        introStartBtn.onClick.AddListener(OnIntroStart);
        tutorialNextBtn.onClick.AddListener(OnTutorialNext);
        tutorialBackBtn.onClick.AddListener(OnTutorialBack);
        tutorialBackBtn.interactable = false; // disabled on page 0
        gameOverRetryBtn.onClick.AddListener(OnRetry);
        winRestartBtn.onClick.AddListener(OnRetry);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Only allow pause during active gameplay, not on menus
            if (GameManager.Instance.IsDayActive || pauseScreen.activeSelf)
            {
                TogglePause();
            }
        }
    }

    private void OnIntroStart()
    {
        HideAll();
        tutorialScreen.SetActive(true);
        _tutorialPage = 0;
        tutorialText.text = _tutorialPages[0];
        tutorialBackBtn.interactable = false; // page 0, can't go back
    }

    private void OnTutorialNext()
    {
        _tutorialPage++;
        if (_tutorialPage < _tutorialPages.Length)
        {
            tutorialText.text = _tutorialPages[_tutorialPage];
            tutorialBackBtn.interactable = true;
        }
        else
        {
            HideAll();
            GameManager.Instance.StartNewGame();
        }
    }

    private void OnTutorialBack()
    {
        if (_tutorialPage <= 0) return;
        _tutorialPage--;
        tutorialText.text = _tutorialPages[_tutorialPage];
        tutorialBackBtn.interactable = (_tutorialPage > 0);
    }

    public void ShowGameOver()
    {
        HideAll();
        gameOverScreen.SetActive(true);
        AudioManager.Instance?.Play("BuzzerError");
    }

    public void ShowWin()
    {
        HideAll();
        winScreen.SetActive(true);
    }

    private void OnRetry()
    {
        HideAll();
        GameManager.Instance.StartNewGame();
    }

    private void HideAll()
    {
        introScreen.SetActive(false);
        tutorialScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        winScreen.SetActive(false);
        if (pauseScreen) pauseScreen.SetActive(false);
    }

    public void TogglePause()
    {
        bool isPaused = pauseScreen.activeSelf;

        if (isPaused)
        {
            pauseScreen.SetActive(false);
            GameManager.Instance.PauseGame(false);
        }
        else
        {
            pauseScreen.SetActive(true);
            GameManager.Instance.PauseGame(true);
        }
    }
}