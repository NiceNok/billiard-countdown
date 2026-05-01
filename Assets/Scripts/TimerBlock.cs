using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerBlock : MonoBehaviour
{
    [Serializable]
    private class TimerState
    {
        public double accumulatedSeconds;
        public bool isRunning;
        public long runStartUtcTicks;
    }

    [SerializeField] private string timerId;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button startTimer;
    [SerializeField] private Button stopTimer;
    [SerializeField] private Button resumeTimer;
    [SerializeField] private Button resetTimer;

    private TimerState _state = new TimerState();
    private string _saveKey;

    private void Awake()
    {
        _saveKey = $"TimerBlock.State.{ResolveTimerId()}";
        LoadState();
        BindButtons();
        UpdateDisplay();
        UpdateButtonState();
    }

    private void Update()
    {
        if (_state.isRunning)
        {
            UpdateDisplay();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveState();
        }
    }

    private void OnApplicationQuit()
    {
        SaveState();
    }

    private void OnDestroy()
    {
        SaveState();
    }

    private void BindButtons()
    {
        if (startTimer != null) startTimer.onClick.AddListener(StartOrResume);
        if (resumeTimer != null) resumeTimer.onClick.AddListener(StartOrResume);
        if (stopTimer != null) stopTimer.onClick.AddListener(Stop);
        if (resetTimer != null) resetTimer.onClick.AddListener(ResetTimer);
    }

    private void StartOrResume()
    {
        if (_state.isRunning)
        {
            return;
        }

        _state.runStartUtcTicks = DateTime.UtcNow.Ticks;
        _state.isRunning = true;
        SaveState();
        UpdateButtonState();
    }

    private void Stop()
    {
        if (!_state.isRunning)
        {
            return;
        }

        _state.accumulatedSeconds = GetCurrentElapsedSeconds();
        _state.isRunning = false;
        _state.runStartUtcTicks = 0;

        SaveState();
        UpdateDisplay();
        UpdateButtonState();
    }

    private void ResetTimer()
    {
        var elapsedBeforeReset = GetCurrentElapsedSeconds();
        BilliardTimersMain.Instance?.RegisterReset(ResolveTimerId(), elapsedBeforeReset);

        _state.accumulatedSeconds = 0;
        _state.isRunning = false;
        _state.runStartUtcTicks = 0;

        SaveState();
        UpdateDisplay();
        UpdateButtonState();
    }

    private void UpdateDisplay()
    {
        if (timerText == null)
        {
            return;
        }

        var elapsed = TimeSpan.FromSeconds(GetCurrentElapsedSeconds());
        timerText.text = $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }

    private void UpdateButtonState()
    {
        if (startTimer != null) startTimer.gameObject.SetActive(!_state.isRunning && _state.accumulatedSeconds <= 0);
        if (resumeTimer != null) resumeTimer.gameObject.SetActive(!_state.isRunning && _state.accumulatedSeconds > 0);
        if (stopTimer != null) stopTimer.gameObject.SetActive(_state.isRunning);
    }

    private double GetCurrentElapsedSeconds()
    {
        if (!_state.isRunning)
        {
            return _state.accumulatedSeconds;
        }

        if (_state.runStartUtcTicks <= 0)
        {
            return _state.accumulatedSeconds;
        }

        var runStartUtc = new DateTime(_state.runStartUtcTicks, DateTimeKind.Utc);
        var sessionElapsed = (DateTime.UtcNow - runStartUtc).TotalSeconds;
        return Math.Max(0, _state.accumulatedSeconds + sessionElapsed);
    }

    private void SaveState()
    {
        var payload = JsonUtility.ToJson(_state);
        PlayerPrefs.SetString(_saveKey, payload);
        PlayerPrefs.Save();
    }

    private void LoadState()
    {
        if (!PlayerPrefs.HasKey(_saveKey))
        {
            _state = new TimerState();
            return;
        }

        var payload = PlayerPrefs.GetString(_saveKey, string.Empty);
        if (string.IsNullOrWhiteSpace(payload))
        {
            _state = new TimerState();
            return;
        }

        _state = JsonUtility.FromJson<TimerState>(payload) ?? new TimerState();

        if (_state.isRunning)
        {
            _state.accumulatedSeconds = GetCurrentElapsedSeconds();
            _state.runStartUtcTicks = DateTime.UtcNow.Ticks;
            SaveState();
        }
    }

    private string ResolveTimerId()
    {
        if (!string.IsNullOrWhiteSpace(timerId))
        {
            return timerId.Trim();
        }

        return BuildHierarchyPath(transform);
    }

    private static string BuildHierarchyPath(Transform current)
    {
        if (current == null)
        {
            return "UnknownTimer";
        }

        var path = current.name;
        while (current.parent != null)
        {
            current = current.parent;
            path = $"{current.name}/{path}";
        }

        return path;
    }
}
