using UnityEngine;
using System;
using WeatherSystem.Core;
using WeatherSystem.DataHandling;

namespace WeatherSystem.TimeManagement
{
    public class TimeController : MonoBehaviour
    {
        [SerializeField] private WeatherSettings settings;
        [SerializeField] private WeatherDataLoader dataLoader;
        
        private DateTime currentTime;
        private DateTime startTime;
        private DateTime endTime;
        private bool isPlaying = false;
        private float playbackSpeed = 1.0f;
        private float accumulatedTime = 0f;

        public event Action<DateTime> OnTimeChanged;
        public event Action<bool> OnPlaybackStateChanged;

        private void Start()
        {
            if (dataLoader == null)
            {
                dataLoader = FindObjectOfType<WeatherDataLoader>();
            }

            // Initialize with the data loader's time range
            startTime = new DateTime(2024, 7, 9, 12, 0, 0);
            endTime = new DateTime(2024, 7, 10, 4, 0, 0);
            currentTime = startTime;
        }

        private void Update()
        {
            if (isPlaying)
            {
                accumulatedTime += Time.deltaTime * playbackSpeed;
                if (accumulatedTime >= settings.updateInterval)
                {
                    accumulatedTime = 0f;
                    AdvanceTime();
                    WeatherManager.Instance.UpdateWeatherData();
                }
            }
        }

        private void AdvanceTime()
        {
            currentTime = currentTime.AddMinutes(1);
            if (currentTime > endTime)
            {
                currentTime = startTime;
            }
            OnTimeChanged?.Invoke(currentTime);
        }

        public void Play()
        {
            isPlaying = true;
            OnPlaybackStateChanged?.Invoke(isPlaying);
        }

        public void Pause()
        {
            isPlaying = false;
            OnPlaybackStateChanged?.Invoke(isPlaying);
        }

        public void SetTime(DateTime newTime)
        {
            if (newTime >= startTime && newTime <= endTime)
            {
                currentTime = newTime;
                OnTimeChanged?.Invoke(currentTime);
            }
        }

        public enum PlaybackSpeed
        {
            Normal = 1,
            Double = 2,
            Fast = 5,
            VeryFast = 10
        }

        private PlaybackSpeed currentSpeed = PlaybackSpeed.Normal;
        
        public void SetPlaybackSpeed(PlaybackSpeed speed)
        {
            currentSpeed = speed;
            playbackSpeed = (float)speed;
            OnPlaybackSpeedChanged?.Invoke((int)currentSpeed);
        }

        public PlaybackSpeed GetCurrentSpeed()
        {
            return currentSpeed;
        }

        public event Action<int> OnPlaybackSpeedChanged;

        public DateTime GetCurrentTime()
        {
            return currentTime;
        }

        public float GetNormalizedTime()
        {
            TimeSpan totalDuration = endTime - startTime;
            TimeSpan currentDuration = currentTime - startTime;
            return (float)(currentDuration.TotalMinutes / totalDuration.TotalMinutes);
        }
    }
} 