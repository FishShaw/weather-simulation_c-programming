using UnityEngine;
using System;
using System.Collections.Generic;
using WeatherSystem.Core;
using System.IO;

namespace WeatherSystem.DataHandling
{
    public class WeatherDataLoader : MonoBehaviour
    {
        [SerializeField] private WeatherSettings settings;
        
        private Dictionary<DateTime, WeatherData> weatherDataCache = new Dictionary<DateTime, WeatherData>();
        private DateTime startTime = new DateTime(2024, 7, 9, 12, 0, 0);
        private DateTime endTime = new DateTime(2024, 7, 10, 4, 0, 0);

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
            string timeString = time.ToString("MMdd_HHmm_HHmm");
            
            // Load rainfall texture
            string rainfallPath = Path.Combine(settings.dataPath, settings.rainfallPath, $"rainfall_{timeString}");
            Texture2D rainfallTexture = Resources.Load<Texture2D>(rainfallPath);

            // Load wind textures
            string windUPath = Path.Combine(settings.dataPath, settings.windPath, $"wind_u_10m_{timeString}");
            string windVPath = Path.Combine(settings.dataPath, settings.windPath, $"wind_v_10m_{timeString}");
            Texture2D windUTexture = Resources.Load<Texture2D>(windUPath);
            Texture2D windVTexture = Resources.Load<Texture2D>(windVPath);

            if (rainfallTexture == null || windUTexture == null || windVTexture == null)
            {
                Debug.LogError($"Failed to load weather data for time: {timeString}");
                return;
            }

            WeatherData weatherData = new WeatherData
            {
                rainfallTexture = rainfallTexture,
                windUTexture = windUTexture,
                windVTexture = windVTexture,
                timestamp = time
            };

            weatherDataCache[time] = weatherData;
        }

        public WeatherData GetWeatherData(DateTime time)
        {
            if (weatherDataCache.ContainsKey(time))
            {
                return weatherDataCache[time];
            }
            
            Debug.LogWarning($"Weather data not found for time: {time}");
            return null;
        }

        public WeatherData GetInterpolatedWeatherData(DateTime time)
        {
            // TODO: Implement interpolation between two nearest time points
            return null;
        }
    }
} 