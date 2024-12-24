using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using WeatherSystem.Core;

namespace WeatherSystem.TimeManagement
{
    public class TimelineUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TimeController timeController;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private WeatherSettings settings;

        [Header("Time Display")]
        [SerializeField] private string timeFormat = "HH:mm";

        private void Start()
        {
            if (timeController == null)
                timeController = FindAnyObjectByType<TimeController>();

            if (settings == null)
            {
                var manager = FindAnyObjectByType<WeatherManager>();
                if (manager != null)
                {
                    settings = manager.Settings;
                }
            }

            if (timeSlider != null)
            {
                timeSlider.onValueChanged.AddListener(OnSliderValueChanged);
            }

            timeController.OnTimeChanged += UpdateTimeDisplay;
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
            if (timeText != null)
            {
                timeText.text = time.ToString(timeFormat);
            }
        }

        private void OnDestroy()
        {
            if (timeController != null)
            {
                timeController.OnTimeChanged -= UpdateTimeDisplay;
            }

            if (timeSlider != null)
            {
                timeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }
    }
} 