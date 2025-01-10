using UnityEngine;
using System;
using System.Linq;
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
                DateTime nextTime = currentTime.AddHours(1);
                if (nextTime > endTime)
                    break;
                currentTime = nextTime;
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

            weatherDataCache[time] = weatherData;
        }

        public WeatherData GetWeatherData(DateTime time)
        {
            if (weatherDataCache.ContainsKey(time))
            {
                return weatherDataCache[time];
            }
            return null;
        }

        public WeatherData GetInterpolatedWeatherData(DateTime time)
        {
            // 将时间规整到最近的5分钟
            int minutes = time.Minute;
            int roundedMinutes = (minutes / 5) * 5;
            DateTime roundedTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, roundedMinutes, 0);

            // 检查缓存中是否已有插值数据
            if (interpolatedDataCache.ContainsKey(roundedTime))
            {
                return interpolatedDataCache[roundedTime];
            }

            // 找到小时数据点
            DateTime prevHour = time.Date.AddHours(time.Hour);
            DateTime nextHour = prevHour.AddHours(1);

            WeatherData prevData = GetWeatherData(prevHour);
            WeatherData nextData = GetWeatherData(nextHour);

            if (prevData == null || nextData == null)
            {
                return prevData ?? nextData;
            }

            // 计算5分钟间隔的插值
            float t = (float)(roundedTime - prevHour).TotalMinutes / 60f;
            
            WeatherData interpolatedData = new WeatherData
            {
                timestamp = roundedTime,
                rainfallTexture = InterpolateTexture(prevData.rainfallTexture, nextData.rainfallTexture, t),
                windUTexture = InterpolateTexture(prevData.windUTexture, nextData.windUTexture, t),
                windVTexture = InterpolateTexture(prevData.windVTexture, nextData.windVTexture, t)
            };

            // 缓存插值结果
            interpolatedDataCache[roundedTime] = interpolatedData;

            return interpolatedData;
        }

        private Texture2D InterpolateTexture(Texture2D tex1, Texture2D tex2, float t)
        {
            if (tex1 == null || tex2 == null) return null;
            
            int width = tex1.width;
            int height = tex1.height;
            
            Texture2D result = new Texture2D(width, height);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color color1 = tex1.GetPixel(x, y);
                    Color color2 = tex2.GetPixel(x, y);
                    result.SetPixel(x, y, Color.Lerp(color1, color2, t));
                }
            }
            
            result.Apply();
            return result;
        }

        // 添加插值数据缓存
        private Dictionary<DateTime, WeatherData> interpolatedDataCache = new Dictionary<DateTime, WeatherData>();
    }
} 