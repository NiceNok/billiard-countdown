using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerBlock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button startTimer;
    [SerializeField] private Button stopTimer;
    [SerializeField] private Button resumeTimer;
    [SerializeField] private Button resetTimer;
}
