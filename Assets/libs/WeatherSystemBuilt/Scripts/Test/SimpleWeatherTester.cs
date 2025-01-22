using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using WeatherSystem.SingleGrid; 

public class SimpleWeatherTester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        string jsonPath = Path.Combine(Application.dataPath, "libs/WeatherSystemBuilt/Resources/WeatherData_json/weather_data_latest.json");
        Debug.Log($"Attempting to read JSON from: {jsonPath}");

        try
        {
            string jsonContent = File.ReadAllText(jsonPath);
            Debug.Log($"=== Raw JSON Content ===");
            Debug.Log($"First 1000 chars:\n{jsonContent.Substring(0, Mathf.Min(1000, jsonContent.Length))}");

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            
            var weatherData = JsonConvert.DeserializeObject<WeatherDataCache>(jsonContent, settings);

            if (weatherData != null)
            {
                Debug.Log("\n=== Weather Data Analysis ===");
                Debug.Log($"Timestamp: {weatherData.timestamp}");
                
                // 分析坐标数据
                if (weatherData.coordinates != null)
                {
                    Debug.Log($"\n=== Coordinates Analysis ===");
                    Debug.Log($"Total coordinates: {weatherData.coordinates.Count}");
                    Debug.Log($"First coordinate: ({weatherData.coordinates[0].x:F6}°N, {weatherData.coordinates[0].y:F6}°E)");
                    Debug.Log($"Last coordinate: ({weatherData.coordinates[^1].x:F6}°N, {weatherData.coordinates[^1].y:F6}°E)");
                }

                // 分析天气数据
                if (weatherData.weatherData != null)
                {
                    Debug.Log("\n=== Weather Data Check ===");
                    Debug.Log($"Total hours: {weatherData.weatherData.Count}");
                    
                    // 检查第一个小时的数据
                    if (weatherData.weatherData.TryGetValue(0, out var firstHourData))
                    {
                        Debug.Log("\n=== First Hour Data Analysis ===");
                        Debug.Log($"WindU dimensions: {firstHourData.WindU.GetLength(0)}x{firstHourData.WindU.GetLength(1)}");
                        Debug.Log($"WindV dimensions: {firstHourData.WindV.GetLength(0)}x{firstHourData.WindV.GetLength(1)}");
                        Debug.Log($"Rain dimensions: {firstHourData.Rain.GetLength(0)}x{firstHourData.Rain.GetLength(1)}");
                        
                        // 采样一些数据点
                        Debug.Log("\n=== Data Samples ===");
                        Debug.Log($"Wind at [0,0]: ({firstHourData.WindU[0,0]:F2}, {firstHourData.WindV[0,0]:F2})");
                        Debug.Log($"Rain at [0,0]: {firstHourData.Rain[0,0]:F2} mm/h");
                        
                        // 检查四个角点
                        Debug.Log("\n=== Corner Points ===");
                        int max = 63;
                        Debug.Log($"Bottom-Left  [0,0]:   Wind=({firstHourData.WindU[0,0]:F2}, {firstHourData.WindV[0,0]:F2}), Rain={firstHourData.Rain[0,0]:F2}");
                        Debug.Log($"Bottom-Right [63,0]:  Wind=({firstHourData.WindU[max,0]:F2}, {firstHourData.WindV[max,0]:F2}), Rain={firstHourData.Rain[max,0]:F2}");
                        Debug.Log($"Top-Left     [0,63]:  Wind=({firstHourData.WindU[0,max]:F2}, {firstHourData.WindV[0,max]:F2}), Rain={firstHourData.Rain[0,max]:F2}");
                        Debug.Log($"Top-Right    [63,63]: Wind=({firstHourData.WindU[max,max]:F2}, {firstHourData.WindV[max,max]:F2}), Rain={firstHourData.Rain[max,max]:F2}");
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to parse JSON");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading/parsing JSON: {e.Message}\n{e.StackTrace}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class WeatherDataCache
{
    public string timestamp;
    public List<Vector2> coordinates;
    public Dictionary<int, WeatherData_new> weatherData;
}

[System.Serializable]
public class WeatherDataJson
{
    public string timestamp;
    public Coordinate[] coordinates;
    
    // 尝试不同的变量名称
    public float[] wind_u;
    public float[] wind_v;
    public float[] wind_speed;
    public float[] wind_direction;
    public float[] rain;
}

[System.Serializable]
public class Coordinate
{
    public float x;
    public float y;
}

[System.Serializable]
public class HourlyWeatherData
{
    public int hour;
    public float[] wind_u;
    public float[] wind_v;
    public float[] rain;
    public float[] temperature;  // 可选
    public float[] humidity;     // 可选
}
