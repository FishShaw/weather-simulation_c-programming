using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherSystem.SingleGrid
{
    public class WeatherManager_new : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform;
        [SerializeField] private SimpleTimeController timeController;
        [SerializeField] private WeatherCoordinates gridMapper;
        [SerializeField] private WeatherVFXController_new vfxController;
        
        private PythonWeatherPipe weatherPipe;
        private Dictionary<int, WeatherData_new> currentWeatherData;
        private Vector3 lastCheckedPosition;
        private float updateInterval = 1f;
        private float timer;
        private float positionThreshold = 10f; // 10米的位置更新阈值

        private async void Start()
        {
            weatherPipe = gameObject.AddComponent<PythonWeatherPipe>();
            gridMapper.Initialize();
            timer = updateInterval;
            
            // 初始化时立即获取一次天气数据
            await UpdateWeatherData();
        }

        private async void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = updateInterval;
                
                // 检查位置是否有显著变化
                if (Vector3.Distance(targetTransform.position, lastCheckedPosition) > positionThreshold)
                {
                    await UpdateWeatherData();
                }
                
                if (currentWeatherData != null)
                {
                    UpdateWeatherEffects();
                }
            }
        }

        private async Task UpdateWeatherData()
        {
            lastCheckedPosition = targetTransform.position;
            currentWeatherData = await weatherPipe.RequestWeatherData(lastCheckedPosition);
        }

        private void UpdateWeatherEffects()
        {
            if (timeController == null) return;

            DateTime currentTime = timeController.GetCurrentTime();
            int currentHour = currentTime.Hour;
            float interpolationFactor = timeController.GetHourInterpolationFactor();

            if (currentWeatherData.TryGetValue(currentHour, out WeatherData_new currentHourData) &&
                currentWeatherData.TryGetValue((currentHour + 1) % 24, out WeatherData_new nextHourData))
            {
                // 在两个小时之间进行插值
                float rainfall = Mathf.Lerp(currentHourData.Rain, nextHourData.Rain, interpolationFactor);
                Vector2 wind = Vector2.Lerp(
                    new Vector2(currentHourData.WindU, currentHourData.WindV),
                    new Vector2(nextHourData.WindU, nextHourData.WindV),
                    interpolationFactor
                );

                vfxController.UpdateWeatherEffects(rainfall, wind);
            }
        }
    }
}
