using UnityEngine;
using System;

namespace WeatherSystem.DataHandling
{
    [Serializable]
    public class WeatherData
    {
        public Texture2D rainfallTexture;
        public Texture2D windUTexture;
        public Texture2D windVTexture;
        
        [Header("Time Information")]
        public DateTime timestamp;
        
        [Header("Conversion Parameters")]
        public float rainfallScaleFactor = 100.0f;
        public float windScaleFactor = 327.67f;
        public float windOffset = 100.0f;

        public float GetRainfallValue(Vector2Int gridPos)
        {
            if (rainfallTexture == null) return 0f;
            Color pixel = rainfallTexture.GetPixel(gridPos.x, gridPos.y);
            return pixel.r / rainfallScaleFactor;
        }

        public Vector2 GetWindValue(Vector2Int gridPos)
        {
            if (windUTexture == null || windVTexture == null) return Vector2.zero;
            
            Color pixelU = windUTexture.GetPixel(gridPos.x, gridPos.y);
            Color pixelV = windVTexture.GetPixel(gridPos.x, gridPos.y);
            
            float u = (pixelU.r / windScaleFactor) - windOffset;
            float v = (pixelV.r / windScaleFactor) - windOffset;
            
            return new Vector2(u, v);
        }
    }
} 