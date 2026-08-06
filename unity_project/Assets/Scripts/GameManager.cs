using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Day Settings")]
    [SerializeField] private int totalDays = 5;
    [SerializeField] private int dailyQuota = 15;
    [SerializeField] private int maxStrikes = 3;
    [SerializeField] private float dayLengthSeconds = 300f; // 5 min per day

    public int CurrentDay { get; private set; } = 1;
    public int ProcessedToday { get; private set; } = 0;
    public int StrikesToday { get; private set; } = 0;
    public float DayTimeRemaining { get; private set; }
    public bool IsDayActive { get; private set; } = false;
    public bool IsGamePaused { get; private set; } = false;

    public UnityEvent OnDayStart;
    public UnityEvent OnDayEnd;
    public UnityEvent OnStrikeGiven;
    public UnityEvent OnQuotaUpdated;
    public UnityEvent OnGameOver;
    public UnityEvent OnGameWin;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartNewGame()
    {
        CurrentDay = 1;
        StartDay();
    }

    public void StartDay()
    {
        ProcessedToday = 0;
        StrikesToday = 0;
        DayTimeRemaining = dayLengthSeconds;
        IsDayActive = true;
        IsGamePaused = false;
        OnDayStart?.Invoke();
        OnQuotaUpdated?.Invoke();
    }

    private void Update()
    {
        if (!IsDayActive || IsGamePaused) return;

        DayTimeRemaining -= Time.deltaTime;

        if (DayTimeRemaining <= 0f)
        {
            EndDay();
        }
    }

    /// <summary>
    /// Called by DeskManager after a stamp decision is evaluated.
    /// </summary>
    public void SubmitDecision(bool wasCorrect)
    {
        if (!IsDayActive) return;

        ProcessedToday++;

        if (!wasCorrect)
        {
            StrikesToday++;
            OnStrikeGiven?.Invoke();

            if (StrikesToday >= maxStrikes)
            {
                IsDayActive = false;
                OnGameOver?.Invoke();
                return;
            }
        }

        OnQuotaUpdated?.Invoke();

        // Check if quota met early
        if (ProcessedToday >= dailyQuota)
        {
            EndDay();
        }
    }

    private void EndDay()
    {
        IsDayActive = false;
        OnDayEnd?.Invoke();

        if (CurrentDay >= totalDays)
        {
            OnGameWin?.Invoke();
        }
        else
        {
            CurrentDay++;
            // UIController will show "Day X Complete" then call StartDay()
        }
    }

    public void PauseGame(bool pause) => IsGamePaused = pause;

    public string GetClockString()
    {
        // Map remaining seconds to 09:00 - 18:00 range
        float t = 1f - (DayTimeRemaining / dayLengthSeconds);
        int totalMinutes = Mathf.FloorToInt(t * 540); // 9 hours = 540 min
        int hours = 9 + totalMinutes / 60;
        int mins = totalMinutes % 60;
        return $"{hours:D2}:{mins:D2}";
    }
}