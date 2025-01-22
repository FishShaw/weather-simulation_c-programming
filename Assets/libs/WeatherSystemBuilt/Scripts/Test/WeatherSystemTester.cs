using UnityEngine;
using WeatherSystem.SingleGrid;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Wander;

public class WeatherStatistics
{
    private const int GRID_SIZE = 64;  // 添加常量定义
    
    public float MinRainfall { get; private set; } = float.MaxValue;
    public float MaxRainfall { get; private set; } = float.MinValue;
    public float AverageRainfall { get; private set; } = 0;
    public Vector2Int MaxRainfallLocation { get; private set; }
    public Vector2Int MinRainfallLocation { get; private set; }
    public Vector2 MaxRainfallCoordinate { get; private set; }
    public Vector2 MinRainfallCoordinate { get; private set; }

    public void AnalyzeRainfall(WeatherData_new data, List<Vector2> coordinates)
    {
        float sum = 0;
        for (int i = 0; i < coordinates.Count; i++)
        {
            int x = i % GRID_SIZE;
            int y = i / GRID_SIZE;
            float rainfall = data.Rain[x, y];
            
            sum += rainfall;
            if (rainfall < MinRainfall)
            {
                MinRainfall = rainfall;
                MinRainfallLocation = new Vector2Int(x, y);
                MinRainfallCoordinate = coordinates[i];
            }
            if (rainfall > MaxRainfall)
            {
                MaxRainfall = rainfall;
                MaxRainfallLocation = new Vector2Int(x, y);
                MaxRainfallCoordinate = coordinates[i];
            }
        }
        AverageRainfall = sum / coordinates.Count;
    }
}

public class WeatherSystemTester : MonoBehaviour
{
    private async void Start()
    {
        await TestWeatherData();
    }

    private async Task TestWeatherData()
    {
        // 确保数据已加载
        if (!WeatherDataStorage.Instance.IsDataLoaded)
        {
            await WeatherDataStorage.Instance.LoadWeatherData();
        }

        var weatherData = WeatherDataStorage.Instance.WeatherData;
        var coordinates = WeatherDataStorage.Instance.Coordinates;
        
        if (weatherData != null)
        {
            Debug.Log("=== Weather Forecast ===");
            
            // 打印四个角点的数据作为示例
            int[] testPoints = new[] { 0, 63, 4032, 4095 }; // 左下、右下、左上、右上
            string[] cornerNames = new[] { "Southwest", "Southeast", "Northwest", "Northeast" };
            
            for (int hour = 0; hour < 16; hour++)
            {
                if (weatherData.TryGetValue(hour, out WeatherData_new data))
                {
                    Debug.Log($"\nTime: {hour:D2}:00");
                    
                    // 获取统计信息
                    var stats = WeatherDataStorage.Instance.GetWeatherStatistics(hour);
                    
                    Debug.Log($"Rainfall Statistics:");
                    Debug.Log($"  Average: {stats.AverageRainfall:F2} mm/h");
                    Debug.Log($"  Min: {stats.MinRainfall:F2} mm/h at grid ({stats.MinRainfallLocation.x}, {stats.MinRainfallLocation.y})");
                    Debug.Log($"  Max: {stats.MaxRainfall:F2} mm/h at grid ({stats.MaxRainfallLocation.x}, {stats.MaxRainfallLocation.y})");
                    
                    // 打印四个角点数据
                    for (int i = 0; i < testPoints.Length; i++)
                    {
                        int index = testPoints[i];
                        Vector2 coord = coordinates[index];
                        Vector2Int gridPos = new Vector2Int(index % 64, index / 64);
                        
                        Debug.Log($"\n{cornerNames[i]} Corner ({coord.x:F4}°N, {coord.y:F4}°E):");
                        Vector2 wind = data.GetWindVector(gridPos);
                        float windSpeed = wind.magnitude;
                        float windDirection = Mathf.Atan2(wind.y, wind.x) * Mathf.Rad2Deg;
                        if (windDirection < 0) windDirection += 360;
                        Debug.Log($"Wind: {windSpeed:F1} m/s at {windDirection:F1}°");
                        Debug.Log($"Wind Vector: ({wind.x:F1}, {wind.y:F1})");
                        Debug.Log($"Rainfall: {data.GetRainfallValue(gridPos):F1} mm/h");
                    }
                    
                    Debug.Log("-------------------");
                }
            }
        }
    }
}
