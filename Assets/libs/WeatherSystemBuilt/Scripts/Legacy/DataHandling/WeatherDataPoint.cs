using UnityEngine;
using System;

namespace WeatherSystem.DataHandling 
{
    public struct WeatherDataPoint
    {
        public DateTime timestamp;
        public float rainfall;    // 实际降雨量 (mm/h)
        public Vector2 wind;      // 实际风速 (m/s)
        public bool isValid;      // 数据有效性标志

        public static WeatherDataPoint FromRawPixels(DateTime time, Color rainfallPixel, Color windUPixel, Color windVPixel)
        {
            return new WeatherDataPoint
            {
                timestamp = time,
                rainfall = rainfallPixel.r / WeatherData.RAINFALL_SCALE,
                wind = new Vector2(
                    (windUPixel.r / WeatherData.WIND_SCALE) - WeatherData.WIND_OFFSET,
                    (windVPixel.r / WeatherData.WIND_SCALE) - WeatherData.WIND_OFFSET
                ),
                isValid = true
            };
        }

        public static WeatherDataPoint Lerp(WeatherDataPoint a, WeatherDataPoint b, float t)
        {
            return new WeatherDataPoint
            {
                timestamp = a.timestamp.AddSeconds((b.timestamp - a.timestamp).TotalSeconds * t),
                rainfall = Mathf.Lerp(a.rainfall, b.rainfall, t),
                wind = Vector2.Lerp(a.wind, b.wind, t),
                isValid = a.isValid && b.isValid
            };
        }
    }
}
