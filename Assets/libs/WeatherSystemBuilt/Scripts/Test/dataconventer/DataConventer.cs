using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System;
using System.Collections.Generic;

public class DataConverter : MonoBehaviour
{
    private const string OUTPUT_PATH = "Assets/libs/WeatherSystemBuilt/Resources/WeatherData_json_converted";
    private const int GRID_SIZE = 64;
    
    [SerializeField]
    //每次记得改位置
    private string sourceJsonPath = "Assets/libs/WeatherSystemBuilt/Resources/WeatherData_json/weather_data_20250122_220801.json";
    
    [Serializable]
    private class WeatherDataJson
    {
        public string timestamp;
        public Dictionary<string, WeatherTimeSlice> weatherData;
    }

    [Serializable]
    private class WeatherData
    {
        [JsonProperty("0")]
        public WeatherTimeSlice timeSlice;
    }

    [Serializable]
    private class WeatherTimeSlice
    {
        public float[,] WindU;
        public float[,] WindV;
        public float[,] WindSpeed;
        public float[,] WindDirection;
        public float[,] Rain;
    }

    [Serializable]
    private class WeatherGridPoint
    {
        public float wind_u;
        public float wind_v;
        public float rain;
    }

    [Serializable]
    private class TimeSliceData
    {
        public int timeIndex;
        public WeatherGridPoint[][] grid;
    }

    [Serializable]
    private class WeatherGridFormat
    {
        public int timeCount;
        public int width;
        public int height;
        public TimeSliceData[] data;
    }
    
    private void Start()
    {
        ConvertFormat();
    }
    
    public void ConvertFormat()
    {
        try
        {
            if (!File.Exists(sourceJsonPath)) return;

            string jsonContent = File.ReadAllText(sourceJsonPath);
            var originalData = JsonConvert.DeserializeObject<WeatherDataJson>(jsonContent);
            
            if (originalData?.weatherData == null) return;

            var timeSlice = originalData.weatherData["0"];
            if (timeSlice == null) return;
            
            var convertedData = new WeatherGridFormat
            {
                timeCount = 16,
                width = GRID_SIZE,
                height = GRID_SIZE,
                data = new TimeSliceData[16]
            };

            for (int hour = 0; hour < 16; hour++)
            {
                var grid = new WeatherGridPoint[GRID_SIZE][];
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    grid[y] = new WeatherGridPoint[GRID_SIZE];
                    for (int x = 0; x < GRID_SIZE; x++)
                    {
                        grid[y][x] = new WeatherGridPoint
                        {
                            wind_u = timeSlice.WindU[y, x],
                            wind_v = timeSlice.WindV[y, x],
                            rain = timeSlice.Rain[y, x]
                        };
                    }
                }

                convertedData.data[hour] = new TimeSliceData
                {
                    timeIndex = hour,
                    grid = grid
                };
            }

            if (!Directory.Exists(OUTPUT_PATH))
            {
                Directory.CreateDirectory(OUTPUT_PATH);
            }

            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string outputPath = Path.Combine(OUTPUT_PATH, $"weather_grid_{timestamp}.json");
            
            string json = JsonConvert.SerializeObject(convertedData, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
            
            File.WriteAllText(outputPath, json, Encoding.UTF8);
        }
        catch (System.Exception) { }
    }
}
