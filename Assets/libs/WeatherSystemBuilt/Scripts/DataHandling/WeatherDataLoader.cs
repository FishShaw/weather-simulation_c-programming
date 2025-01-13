using UnityEngine;
using System;
using System.Collections.Generic;
using WeatherSystem.Core;

namespace WeatherSystem.DataHandling
{
    public class WeatherDataLoader : MonoBehaviour
    {
        [SerializeField] private WeatherSettings settings;
        
        private Dictionary<DateTime, WeatherData> weatherDataCache = new Dictionary<DateTime, WeatherData>();
        private DateTime startTime = new DateTime(2024, 7, 9, 12, 0, 0);
        private DateTime endTime = new DateTime(2024, 7, 10, 3, 0, 0);

        public void Initialize()
        {
            if (settings == null)
            {
                Debug.LogError("Weather Settings not assigned!");
                return;
            }
            PreloadWeatherData();
        }

        private void PreloadWeatherData()
        {
            DateTime currentTime = startTime;
            while (currentTime <= endTime)
            {
                LoadWeatherDataForTime(currentTime);
                currentTime = currentTime.AddHours(1);
            }
        }

        private void LoadWeatherDataForTime(DateTime time)
        {
            if (time > endTime) return;

            string timeString = time.ToString("MMdd");
            string hourString = $"{time.Hour:D2}00_{(time.Hour + 1):D2}00";
            string windHourString = $"{time.Hour:D2}00";
            
            string rainfallPath = $"Weatherdata/Texture_new_raw/rainfall/PNG/rainfall_{timeString}_{hourString}";
            string windUPath = $"Weatherdata/Texture_new_raw/wind/PNG/wind_u_10m_{timeString}_{windHourString}";
            string windVPath = $"Weatherdata/Texture_new_raw/wind/PNG/wind_v_10m_{timeString}_{windHourString}";
            
            Texture2D rainfallTexture = Resources.Load<Texture2D>(rainfallPath);
            Texture2D windUTexture = Resources.Load<Texture2D>(windUPath);
            Texture2D windVTexture = Resources.Load<Texture2D>(windVPath);

            if (rainfallTexture == null || windUTexture == null || windVTexture == null) return;

            WeatherData weatherData = new WeatherData
            {
                rainfallTexture = rainfallTexture,
                windUTexture = windUTexture,
                windVTexture = windVTexture,
                timestamp = time
            };
            
            weatherData.ProcessTextureData();
            weatherDataCache[time] = weatherData;
        }

        public WeatherData GetInterpolatedWeatherData(DateTime time)
        {
            DateTime prevHour = time.Date.AddHours(time.Hour);
            DateTime nextHour = prevHour.AddHours(1);

            WeatherData prevData = GetWeatherData(prevHour);
            WeatherData nextData = GetWeatherData(nextHour);

            if (prevData == null || nextData == null)
            {
                return prevData ?? nextData;
            }

            float t = (float)(time - prevHour).TotalMinutes / 60f;
            
            WeatherData interpolatedData = new WeatherData
            {
                timestamp = time,
                rainfallTexture = prevData.rainfallTexture,  // 使用前一时刻的纹理用于显示
                windUTexture = prevData.windUTexture,
                windVTexture = prevData.windVTexture
            };

            // 设置插值参数
            interpolatedData.SetupInterpolation(prevData, nextData, t);
            return interpolatedData;
        }

        private WeatherData GetWeatherData(DateTime time)
        {
            return weatherDataCache.ContainsKey(time) ? weatherDataCache[time] : null;
        }
    }
} 