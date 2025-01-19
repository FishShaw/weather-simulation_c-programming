using UnityEngine;
using WeatherSystem.SingleGrid;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class WeatherSystemTester : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
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
                if (weatherData.TryGetValue(hour, out WeatherData_new data))
                {
                    // 获取风向向量用于显示
                    Vector2 windVector = data.GetWindVector();
                    Vector2 windDirVector = data.GetWindDirectionVector();
                    
                    Debug.Log(
                        $"Time: {hour:D2}:00\n" +
                        $"Wind U: {data.WindU:F1} m/s\n" +
                        $"Wind V: {data.WindV:F1} m/s\n" +
                        $"Wind Speed: {data.WindSpeed:F1} m/s\n" +
                        $"Wind Direction: {data.WindDirection * Mathf.Rad2Deg:F1}°\n" +
                        $"Wind Vector: ({windVector.x:F1}, {windVector.y:F1})\n" +
                        $"Wind Dir Vector: ({windDirVector.x:F1}, {windDirVector.y:F1})\n" +
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
