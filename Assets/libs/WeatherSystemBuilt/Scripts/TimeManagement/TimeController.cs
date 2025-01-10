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
        [SerializeField] private DateTime startTime = new DateTime(2024, 7, 9, 12, 0, 0);
        private DateTime endTime = new DateTime(2024, 7, 10, 3, 0, 0);
        private DateTime currentTime;

        [Header("Time Control")]
        [SerializeField] private float timeScale = 1f;
        [SerializeField] private float normalSpeed = 60f;  // 每秒前进60秒（1分钟）
        [SerializeField] private float fastSpeed = 300f;   // 每秒前进300秒（5分钟）
        private bool isPlaying = false;

        public event Action<DateTime> OnTimeChanged;

        private void Start()
        {
            if (dataLoader == null)
                dataLoader = FindFirstObjectByType<WeatherDataLoader>();

            if (settings == null)
                settings = FindFirstObjectByType<WeatherManager>()?.Settings;

            currentTime = startTime;
            OnTimeChanged?.Invoke(currentTime);
            Debug.Log($"[TimeController] Initialized with time: {currentTime}");
        }

        private void Update()
        {
            if (isPlaying && currentTime < endTime)
            {
                currentTime = currentTime.AddSeconds(Time.deltaTime * timeScale);
                if (currentTime > endTime)
                    currentTime = endTime;
                
                OnTimeChanged?.Invoke(currentTime);
                Debug.Log($"[TimeController] Current time: {currentTime}, TimeScale: {timeScale}");
            }
        }

        public DateTime GetCurrentTime() => currentTime;

        public void SetTime(DateTime time)
        {
            if (time >= startTime && time <= endTime)
            {
                currentTime = time;
                OnTimeChanged?.Invoke(currentTime);
                Debug.Log($"[TimeController] Time set to: {currentTime}");
            }
        }

        public void Play()
        {
            isPlaying = true;
            timeScale = normalSpeed;
            Debug.Log("[TimeController] Started playing");
        }

        public void Stop()
        {
            isPlaying = false;
            timeScale = 0f;
            Debug.Log("[TimeController] Stopped");
        }

        public void SpeedUp()
        {
            if (isPlaying)
            {
                timeScale = fastSpeed;
                Debug.Log("[TimeController] Speed up");
            }
        }

        public void SetNormalSpeed()
        {
            if (isPlaying)
            {
                timeScale = normalSpeed;
                Debug.Log("[TimeController] Normal speed");
            }
        }
    }
} 