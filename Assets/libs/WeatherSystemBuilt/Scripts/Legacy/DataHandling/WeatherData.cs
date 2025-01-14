using UnityEngine;
using System;
using WeatherSystem.Core;

namespace WeatherSystem.DataHandling
{
    [Serializable]
    public class WeatherData
    {
        // 添加常量定义并设为公开
        public const float RAINFALL_SCALE = 100.0f;
        public const float WIND_SCALE = 327.67f;
        public const float WIND_OFFSET = 100.0f;

        public Texture2D rainfallTexture;
        public Texture2D windUTexture;
        public Texture2D windVTexture;
        public DateTime timestamp;

        // 处理后的数值数组
        private float[,] rainfallValues;
        private Vector2[,] windValues;
        private bool isProcessed = false;

        // 插值相关
        private WeatherData prevData;
        private WeatherData nextData;
        private float interpolationT;
        private bool useInterpolation;

        // 处理纹理数据
        public void ProcessTextureData()
        {
            if (isProcessed) return;
            
            int width = rainfallTexture.width;
            int height = rainfallTexture.height;
            
            rainfallValues = new float[width, height];
            windValues = new Vector2[width, height];
            
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                rainfallValues[x,y] = rainfallTexture.GetPixel(x, y).r / RAINFALL_SCALE;
                float u = (windUTexture.GetPixel(x, y).r / WIND_SCALE) - WIND_OFFSET;
                float v = (windVTexture.GetPixel(x, y).r / WIND_SCALE) - WIND_OFFSET;
                windValues[x,y] = new Vector2(u, v);
            }
            
            isProcessed = true;
        }

        // 设置插值参数
        public void SetupInterpolation(WeatherData prev, WeatherData next, float t)
        {
            prevData = prev;
            nextData = next;
            interpolationT = t;
            useInterpolation = true;
        }

        // 获取降雨值
        public float GetRainfallValue(Vector2Int gridPos)
        {
            if (!useInterpolation)
            {
                if (!isProcessed) ProcessTextureData();
                return rainfallValues[gridPos.x, gridPos.y];
            }

            float prevValue = prevData.GetRainfallValue(gridPos);
            float nextValue = nextData.GetRainfallValue(gridPos);
            return Mathf.Lerp(prevValue, nextValue, interpolationT);
        }

        // 获取风向风速
        public Vector2 GetWindValue(Vector2Int gridPos)
        {
            if (!useInterpolation)
            {
                if (!isProcessed) ProcessTextureData();
                return windValues[gridPos.x, gridPos.y];
            }

            Vector2 prevWind = prevData.GetWindValue(gridPos);
            Vector2 nextWind = nextData.GetWindValue(gridPos);
            return Vector2.Lerp(prevWind, nextWind, interpolationT);
        }
    }
} 