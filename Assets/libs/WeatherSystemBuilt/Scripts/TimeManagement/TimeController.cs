using UnityEngine;
using System;
using WeatherSystem.Core;
using WeatherSystem.DataHandling;

namespace WeatherSystem.TimeManagement
{
    public class TimeController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private WeatherDataLoader dataLoader;
        [SerializeField] private WeatherSettings settings;

        [Header("Time Settings")]
        [SerializeField] private float timeScale = 1f;
        [SerializeField] private DateTime startTime;
        private DateTime currentTime;

        public event Action<DateTime> OnTimeChanged;

        private void Start()
        {
            if (dataLoader == null)
                dataLoader = FindFirstObjectByType<WeatherDataLoader>();

            if (settings == null)
                settings = FindFirstObjectByType<WeatherManager>()?.Settings;

            currentTime = startTime;
            OnTimeChanged?.Invoke(currentTime);
        }

        private void Update()
        {
            UpdateTime();
        }

        private void UpdateTime()
        {
            currentTime = currentTime.AddSeconds(Time.deltaTime * timeScale);
            OnTimeChanged?.Invoke(currentTime);
        }

        public DateTime GetCurrentTime()
        {
            return currentTime;
        }

        public void SetTime(DateTime time)
        {
            currentTime = time;
            OnTimeChanged?.Invoke(currentTime);
        }
    }
} 