using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace WeatherSystem.TimeManagement
{
    public class TimelineUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider timelineSlider;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text speedText;
        [SerializeField] private Button[] speedButtons;

        [Header("Components")]
        [SerializeField] private TimeController timeController;

        private int[] speedValues = { 2, 5, 10 };

        private void Start()
        {
            if (timeController == null)
            {
                timeController = FindObjectOfType<TimeController>();
            }

            SetupUIElements();
            SubscribeToEvents();
        }

        private void SetupUIElements()
        {
            timelineSlider.onValueChanged.AddListener(OnSliderValueChanged);
            playButton.onClick.AddListener(OnPlayClicked);
            pauseButton.onClick.AddListener(OnPauseClicked);
            SetupSpeedButtons();

            UpdateTimeDisplay(timeController.GetCurrentTime());
            UpdateSpeedText((int)timeController.GetCurrentSpeed());
        }

        private void SubscribeToEvents()
        {
            timeController.OnTimeChanged += UpdateTimeDisplay;
            timeController.OnPlaybackStateChanged += UpdatePlaybackUI;
            timeController.OnPlaybackSpeedChanged += UpdateSpeedText;
        }

        private void OnSliderValueChanged(float value)
        {
            DateTime startTime = new DateTime(2024, 7, 9, 12, 0, 0);
            DateTime endTime = new DateTime(2024, 7, 10, 4, 0, 0);
            TimeSpan totalDuration = endTime - startTime;
            
            DateTime newTime = startTime.AddMinutes(totalDuration.TotalMinutes * value);
            timeController.SetTime(newTime);
        }

        private void UpdateTimeDisplay(DateTime time)
        {
            timeText.text = time.ToString("MM/dd HH:mm");
            timelineSlider.value = timeController.GetNormalizedTime();
        }

        private void UpdatePlaybackUI(bool isPlaying)
        {
            playButton.gameObject.SetActive(!isPlaying);
            pauseButton.gameObject.SetActive(isPlaying);
        }

        private void OnPlayClicked() => timeController.Play();
        private void OnPauseClicked() => timeController.Pause();

        private void SetupSpeedButtons()
        {
            for (int i = 0; i < speedButtons.Length; i++)
            {
                int speedIndex = i;
                speedButtons[i].onClick.AddListener(() => OnSpeedButtonClicked(speedValues[speedIndex]));
            }
        }

        private void OnSpeedButtonClicked(int speedValue)
        {
            timeController.SetPlaybackSpeed((TimeController.PlaybackSpeed)speedValue);
            UpdateSpeedText(speedValue);
        }

        private void UpdateSpeedText(int speed)
        {
            speedText.text = speed == 1 ? "normal" : $"{speed}x";
        }

        private void OnDestroy()
        {
            if (timeController != null)
            {
                timeController.OnTimeChanged -= UpdateTimeDisplay;
                timeController.OnPlaybackStateChanged -= UpdatePlaybackUI;
            }
        }
    }
} 