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
        private DateTime lastUpdateTime;

        [Header("Time Control")]
        [SerializeField] private float timeScale = 1f;
        [SerializeField] private float normalSpeed = 300f;  // 每秒前进5分钟
        [SerializeField] private float fastSpeed = 1800f;   // 每秒前进30分钟
        private bool isPlaying = false;
        private float updateInterval => settings != null ? settings.updateInterval : 1.0f;
        private float timeSinceLastUpdate = 0f;

        public event Action<DateTime> OnTimeChanged;
        public bool IsPlaying => isPlaying;

        private void Start()
        {
            if (dataLoader == null)
                dataLoader = FindFirstObjectByType<WeatherDataLoader>();

            if (settings == null)
                settings = FindFirstObjectByType<WeatherManager>()?.Settings;

            currentTime = startTime;
            lastUpdateTime = currentTime;
            OnTimeChanged?.Invoke(currentTime);
        }

        private void Update()
        {
            if (!isPlaying || currentTime >= endTime) return;

            timeSinceLastUpdate += Time.deltaTime;
            
            if (timeSinceLastUpdate >= updateInterval)
            {
                float deltaTime = timeSinceLastUpdate * timeScale;
                DateTime newTime = currentTime.AddSeconds(deltaTime);
                
                if (newTime > endTime)
                    newTime = endTime;
                    
                if (newTime != currentTime)
                {
                    currentTime = newTime;
                    lastUpdateTime = currentTime;
                    OnTimeChanged?.Invoke(currentTime);
                    Debug.Log($"[TimeController] Time updated: {currentTime}, Scale: {timeScale}");
                }
                
                timeSinceLastUpdate = 0f;
            }
        }

        public DateTime GetCurrentTime() => currentTime;

        public void SetTime(DateTime time)
        {
            if (time >= startTime && time <= endTime)
            {
                currentTime = time;
                OnTimeChanged?.Invoke(currentTime);
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
    }
} 