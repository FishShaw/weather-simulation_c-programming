using UnityEngine;
using System;
using System.Collections.Generic;
using WeatherSystem.Core;

namespace WeatherSystem.SingleGrid
{
    public class SingleWeatherDataLoader : MonoBehaviour
    {
        [SerializeField] private WeatherSettings settings;
        
        private Dictionary<DateTime, SingleWeatherData> weatherDataCache = new Dictionary<DateTime, SingleWeatherData>();
        private Vector2Int currentGridPosition;
        private DateTime startTime = new DateTime(2024, 7, 9, 12, 0, 0);
        private DateTime endTime = new DateTime(2024, 7, 10, 3, 0, 0);

        public void Initialize()
        {
            LoadTimeSeriesData();
        }

        private void LoadTimeSeriesData()
        {
            weatherDataCache.Clear();
            DateTime currentTime = startTime;

            while (currentTime <= endTime)
            {
                LoadHourData(currentTime);
                currentTime = currentTime.AddHours(1);
            }
        }

        private void LoadHourData(DateTime time)
        {
            string timeString = time.ToString("MMdd");
            string hourString = $"{time.Hour:D2}00_{(time.Hour + 1):D2}00";
            string windHourString = $"{time.Hour:D2}00";
            
            string rainfallPath = $"Weatherdata/Texture_new_raw/rainfall/PNG/rainfall_{timeString}_{hourString}";
            string windUPath = $"Weatherdata/Texture_new_raw/wind/PNG/wind_u_10m_{timeString}_{windHourString}";
            string windVPath = $"Weatherdata/Texture_new_raw/wind/PNG/wind_v_10m_{timeString}_{windHourString}";

            var rainfallTexture = Resources.Load<Texture2D>(rainfallPath);
            var windUTexture = Resources.Load<Texture2D>(windUPath);
            var windVTexture = Resources.Load<Texture2D>(windVPath);

            if (rainfallTexture == null || windUTexture == null || windVTexture == null)
            {
                Debug.LogError($"Failed to load weather data for time: {time}");
                return;
            }

            var weatherData = new SingleWeatherData
            {
                timestamp = time,
                rainfallTexture = rainfallTexture,
                windUTexture = windUTexture,
                windVTexture = windVTexture
            };
            
            weatherData.ProcessTextureData(currentGridPosition);
            weatherDataCache[time] = weatherData;
        }

        public SingleWeatherData GetInterpolatedWeatherData(DateTime time)
        {
            DateTime prevHour = time.Date.AddHours(time.Hour);
            DateTime nextHour = prevHour.AddHours(1);

            SingleWeatherData prevData = GetWeatherData(prevHour);
            SingleWeatherData nextData = GetWeatherData(nextHour);

            if (prevData == null || nextData == null)
            {
                return prevData ?? nextData;
            }

            float t = (float)(time - prevHour).TotalMinutes / 60f;
            
            SingleWeatherData interpolatedData = new SingleWeatherData
            {
                timestamp = time
            };
            interpolatedData.SetupInterpolation(prevData, nextData, t);
            
            interpolatedData.GetRainfallValue(currentGridPosition);
            interpolatedData.GetWindValue(currentGridPosition);
            
            return interpolatedData;
        }

        private SingleWeatherData GetWeatherData(DateTime time)
        {
            return weatherDataCache.ContainsKey(time) ? weatherDataCache[time] : null;
        }

        public void UpdateGridPosition(Vector2Int newGridPos)
        {
            currentGridPosition = newGridPos;
        }

        private void OnDestroy()
        {
            foreach (var data in weatherDataCache.Values)
            {
                if (data.rainfallTexture != null)
                    Resources.UnloadAsset(data.rainfallTexture);
                if (data.windUTexture != null)
                    Resources.UnloadAsset(data.windUTexture);
                if (data.windVTexture != null)
                    Resources.UnloadAsset(data.windVTexture);
            }
            weatherDataCache.Clear();
        }
    }
}
