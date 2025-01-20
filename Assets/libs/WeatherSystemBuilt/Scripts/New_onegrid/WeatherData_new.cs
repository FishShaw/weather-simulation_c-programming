using UnityEngine;
using System;

namespace WeatherSystem.SingleGrid
{
    [Serializable]
    public class WeatherData_new
    {
        public float[,] WindU { get; set; }      // 64x64 grid wind speed U component
        public float[,] WindV { get; set; }      // 64x64 grid wind speed V component
        public float[,] WindSpeed { get; set; }  // 64x64 grid wind speed size
        public float[,] WindDirection { get; set; } // 64x64 grid wind direction angle
        public float[,] Rain { get; set; }       // 64x64 grid rainfall

        private const int GRID_SIZE = 64;

        public WeatherData_new()
        {
            WindU = new float[GRID_SIZE, GRID_SIZE];
            WindV = new float[GRID_SIZE, GRID_SIZE];
            WindSpeed = new float[GRID_SIZE, GRID_SIZE];
            WindDirection = new float[GRID_SIZE, GRID_SIZE];
            Rain = new float[GRID_SIZE, GRID_SIZE];
        }

        public Vector2 GetWindVector(Vector2Int gridPos)
        {
            return new Vector2(WindU[gridPos.x, gridPos.y], WindV[gridPos.x, gridPos.y]);
        }

        public Vector2 GetWindDirectionVector(Vector2Int gridPos)
        {
            float angle = WindDirection[gridPos.x, gridPos.y];
            float speed = WindSpeed[gridPos.x, gridPos.y];
            return new Vector2(
                Mathf.Cos(angle) * speed,
                Mathf.Sin(angle) * speed
            );
        }

        // provide a one-dimensional array access method to maintain compatibility
        public float GetValueFromIndex(float[,] array, int index)
        {
            int x = index % GRID_SIZE;
            int y = index / GRID_SIZE;
            return array[x, y];
        }

        public float GetRainfallValue(Vector2Int gridPos)
        {
            return Rain[gridPos.x, gridPos.y];
        }

        public Vector2 GetWindValue(Vector2Int gridPos)
        {
            return GetWindVector(gridPos);
        }
    }
}