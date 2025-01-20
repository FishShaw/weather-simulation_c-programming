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
    private WeatherCoordinates gridMapper;
    private PythonWeatherPipe weatherPipe;
    private Dictionary<int, WeatherData_new> weatherData;

    private async void Start()
    {
        gridMapper = gameObject.AddComponent<WeatherCoordinates>();
        weatherPipe = gameObject.AddComponent<PythonWeatherPipe>();
        
        gridMapper.Initialize();
        await Task.Delay(500); // 给予足够的时间让 PythonWeatherPipe 完成连接
        await TestWeatherData();
    }

    private async Task TestWeatherData()
    {
        // 1. 获取地形网格坐标
        List<Vector2> coordinates = gridMapper.Generate64x64Grid();
        
        // 打印地形范围信息
        var terrainBuilder = FindFirstObjectByType<TerrainBuilder>();
        Debug.Log($"Terrain Bounds: Origin({terrainBuilder.originRDX}, {terrainBuilder.originRDY}), " +
                 $"Size({terrainBuilder.boundsSize})");
        
        // 打印网格信息
        Debug.Log($"Generated {coordinates.Count} grid coordinates");
        Debug.Log($"First coordinate: ({coordinates[0].x:F4}°N, {coordinates[0].y:F4}°E)");
        Debug.Log($"Last coordinate: ({coordinates[4095].x:F4}°N, {coordinates[4095].y:F4}°E)");
        
        // 2. 获取天气数据
        weatherData = await weatherPipe.RequestWeatherData(Vector3.zero);
        
        // 3. 打印结果
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
                    
                    // 添加统计分析
                    WeatherStatistics stats = new WeatherStatistics();
                    stats.AnalyzeRainfall(data, coordinates);
                    
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
        else
        {
            Debug.LogError("Failed to get weather data");
        }
    }
}
