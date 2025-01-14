using UnityEngine;
using System;

namespace WeatherSystem.SingleGrid
{
    public class SingleSimpleTimeController : MonoBehaviour
    {
        [SerializeField] private DateTime startTime = new DateTime(2024, 7, 9, 12, 0, 0);
        private DateTime endTime = new DateTime(2024, 7, 10, 3, 0, 0);
        private DateTime currentTime;
        private const float TIME_SPEED = 1800f;  // 30分钟/秒
        private bool isPlaying = false;

        private void Start()
        {
            currentTime = startTime;
            isPlaying = true;  // 自动开始播放
        }

        private void Update()
        {
            if (!isPlaying || currentTime >= endTime) return;

            float deltaTime = Time.deltaTime * TIME_SPEED;
            currentTime = currentTime.AddSeconds(deltaTime);
            
            if (currentTime > endTime)
            {
                currentTime = endTime;
                isPlaying = false;
            }
        }

        public DateTime GetCurrentTime() => currentTime;
        public bool IsPlaying => isPlaying;
    }
} 