using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BilliardTimersMain : MonoBehaviour
{
    [SerializeField] private Button historyButton;
    [SerializeField] private GameObject historyWindow;
    [SerializeField] private TextMeshProUGUI historyText;

    private const int MaxHistoryEntries = 50;
    private const double MinResetSecondsForHistory = 10;
    private static BilliardTimersMain _instance;

    private readonly List<string> _historyEntries = new List<string>();
    private string _historyPath;

    public static BilliardTimersMain Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        _historyPath = Path.Combine(Application.persistentDataPath, "timers_history.log");

        LoadHistory();
        BindHistoryButton();
        RefreshHistoryText();

        if (historyWindow != null)
        {
            historyWindow.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public void RegisterReset(string timerId, double elapsedBeforeResetSeconds)
    {
        if (elapsedBeforeResetSeconds < MinResetSecondsForHistory)
        {
            return;
        }

        var endTime = DateTime.Now;
        var startTime = endTime - TimeSpan.FromSeconds(elapsedBeforeResetSeconds);
        var duration = TimeSpan.FromSeconds(elapsedBeforeResetSeconds);

        var line =
            $"{endTime:dd.MM.yyyy HH:mm} стол №{ResolveTableTitle(timerId)} {startTime:HH:mm}-{endTime:HH:mm} ({duration.Hours}ч. {duration.Minutes}м.)";

        _historyEntries.Add(line);
        KeepLastEntries();
        SaveHistory();
        RefreshHistoryText();
    }

    private void BindHistoryButton()
    {
        if (historyButton != null)
        {
            historyButton.onClick.AddListener(ToggleHistoryWindow);
        }
    }

    private void ToggleHistoryWindow()
    {
        if (historyWindow == null)
        {
            return;
        }

        var nextState = !historyWindow.activeSelf;
        historyWindow.SetActive(nextState);

        if (nextState)
        {
            RefreshHistoryText();
        }
    }

    private void LoadHistory()
    {
        if (!File.Exists(_historyPath))
        {
            return;
        }

        var lines = File.ReadAllLines(_historyPath)
            .Where(line => !string.IsNullOrWhiteSpace(line));

        _historyEntries.Clear();
        _historyEntries.AddRange(lines);
        KeepLastEntries();
    }

    private void SaveHistory()
    {
        KeepLastEntries();
        File.WriteAllLines(_historyPath, _historyEntries);
    }

    private void KeepLastEntries()
    {
        var overflow = _historyEntries.Count - MaxHistoryEntries;
        if (overflow <= 0)
        {
            return;
        }

        _historyEntries.RemoveRange(0, overflow);
    }

    private void RefreshHistoryText()
    {
        if (historyText == null)
        {
            return;
        }

        if (_historyEntries.Count == 0)
        {
            historyText.text = "История пока пуста";
            return;
        }

        historyText.text = string.Join("\n", _historyEntries.AsEnumerable().Reverse());
    }

    private static string ResolveTableTitle(string timerId)
    {
        if (string.IsNullOrWhiteSpace(timerId))
        {
            return "?";
        }

        return timerId.Trim();
    }
}
