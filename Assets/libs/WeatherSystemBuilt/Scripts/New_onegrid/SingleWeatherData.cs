using UnityEngine;
using System;

namespace WeatherSystem.SingleGrid
{
    public class SingleWeatherData
    {
        public const float RAINFALL_SCALE = 100.0f;
        public const float WIND_SCALE = 327.67f;
        public const float WIND_OFFSET = 100.0f;

        public DateTime timestamp;
        public Texture2D rainfallTexture;
        public Texture2D windUTexture;
        public Texture2D windVTexture;

        private float[,] rainfallValues;
        private Vector2[,] windValues;
        private bool isProcessed = false;
        private Vector2Int gridPosition;

        private SingleWeatherData prevData;
        private SingleWeatherData nextData;
        private float interpolationT;
        private bool useInterpolation;

        public void ProcessTextureData(Vector2Int gridPos)
        {
            if (isProcessed) return;
            
            gridPosition = gridPos;
            int width = rainfallTexture.width;
            int height = rainfallTexture.height;
            
            rainfallValues = new float[width, height];
            windValues = new Vector2[width, height];
            
            Vector2 uv = GridToUV(gridPosition);
            
            Color rainfallColor = rainfallTexture.GetPixelBilinear(uv.x, uv.y);
            Color windUColor = windUTexture.GetPixelBilinear(uv.x, uv.y);
            Color windVColor = windVTexture.GetPixelBilinear(uv.x, uv.y);
            
            rainfallValues[gridPosition.x, gridPosition.y] = rainfallColor.r / RAINFALL_SCALE;
            float u = (windUColor.r / WIND_SCALE) - WIND_OFFSET;
            float v = (windVColor.r / WIND_SCALE) - WIND_OFFSET;
            windValues[gridPosition.x, gridPosition.y] = new Vector2(u, v);
            
            isProcessed = true;
        }

        private Vector2 GridToUV(Vector2Int gridPos)
        {
            const int GRID_SIZE = 390;
            return new Vector2(
                (float)gridPos.x / GRID_SIZE,
                (float)gridPos.y / GRID_SIZE
            );
        }

        public void SetupInterpolation(SingleWeatherData prev, SingleWeatherData next, float t)
        {
            prevData = prev;
            nextData = next;
            interpolationT = t;
            useInterpolation = true;
        }

        public float GetRainfallValue(Vector2Int gridPos)
        {
            if (!useInterpolation)
            {
                if (!isProcessed) ProcessTextureData(gridPos);
                return rainfallValues[gridPos.x, gridPos.y];
            }

            float prevValue = prevData.GetRainfallValue(gridPos);
            float nextValue = nextData.GetRainfallValue(gridPos);
            return Mathf.Lerp(prevValue, nextValue, interpolationT);
        }

        public Vector2 GetWindValue(Vector2Int gridPos)
        {
            if (!useInterpolation)
            {
                if (!isProcessed) ProcessTextureData(gridPos);
                return windValues[gridPos.x, gridPos.y];
            }

            Vector2 prevValue = prevData.GetWindValue(gridPos);
            Vector2 nextValue = nextData.GetWindValue(gridPos);
            return Vector2.Lerp(prevValue, nextValue, interpolationT);
        }
    }
}
