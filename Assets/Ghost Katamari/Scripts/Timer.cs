using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI timerText; // Assign a Text UI element in the Inspector

    private float elapsedTime = 0f;
    private bool isRunning = false;

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay(elapsedTime);
        }
    }

    private void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int hundredths = Mathf.FloorToInt((time * 100f) % 100f);

        // Format: 0'00''00
        timerText.text = string.Format("{0}'{1:00}''{2:00}", minutes, seconds, hundredths);
    }

    // Optional control methods
    public void StartTimer() => isRunning = true;
    public void StopTimer() => isRunning = false;
    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerDisplay(elapsedTime);
    }
}