using UnityEngine;
using WeatherSystem.SingleGrid;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class WeatherSystemTester : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    private SingleWeatherGridMapper gridMapper;
    private SinglePythonWeatherPipe weatherPipe;
    private Dictionary<int, SingleWeatherData> weatherData;

    private void Start()
    {
        gridMapper = gameObject.AddComponent<SingleWeatherGridMapper>();
        weatherPipe = gameObject.AddComponent<SinglePythonWeatherPipe>();
        
        gridMapper.Initialize();
        TestWeatherData();
    }

    private async void TestWeatherData()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        
        // 1. 转换坐标
        var (lat, lon) = gridMapper.WorldToGeographic(cameraPosition);
        Debug.Log($"Location: ({lat:F4}°N, {lon:F4}°E)\n");

        // 2. 获取天气数据
        weatherData = await weatherPipe.RequestWeatherData(cameraPosition);
        
        // 3. 打印结果
        if (weatherData != null)
        {
            Debug.Log("=== Weather Forecast ===");
            
            for (int hour = 0; hour < 16; hour++)
            {
                if (weatherData.TryGetValue(hour, out SingleWeatherData data))
                {
                    Debug.Log(
                        $"Time: {hour:D2}:00\n" +
                        $"Wind U: {data.WindU:F1} m/s\n" +
                        $"Wind V: {data.WindV:F1} m/s\n" +
                        $"Wind Speed: {data.WindSpeed:F1} m/s\n" +
                        $"Rainfall: {data.Rain:F1} mm/h\n" +
                        "-------------------"
                    );
                }
            }
        }
        else
        {
            Debug.LogError("Failed to get weather data");
        }
    }
}
