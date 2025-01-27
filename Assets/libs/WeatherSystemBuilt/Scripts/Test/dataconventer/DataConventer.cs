using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System;
using System.Collections.Generic;

public class DataConverter : MonoBehaviour
{
    private const string SOURCE_DIR = "Assets/libs/WeatherSystemBuilt/Resources/WeatherData_json";
    private const string CONVERTED_DIR = "Assets/libs/WeatherSystemBuilt/Resources/WeatherData_json_converted";
    private const string TEXTURES_DIR = "Assets/libs/WeatherSystemBuilt/Resources/WeatherData_json_textures";
    private const int GRID_SIZE = 64;
    private const int TIME_COUNT = 16;

    [Serializable]
    private class WeatherDataJson
    {
        public string timestamp;
        public Dictionary<string, WeatherTimeSlice> weatherData;
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

    [Serializable]
    private class NormalizationMetadata
    {
        public float windU_min;
        public float windU_max;
        public float windV_min;
        public float windV_max;
        public float rain_min;
        public float rain_max;
    }

    private void Start()
    {
        ProcessAllFiles();
    }

    private void ProcessAllFiles()
    {
        string[] files = Directory.GetFiles(SOURCE_DIR, "weather_data_*.json");
        foreach (string file in files)
        {
            string baseName = Path.GetFileNameWithoutExtension(file);
            ConvertFormat(file, baseName);
        }
    }

    public void ConvertFormat(string inputPath, string baseName)
    {
        try
        {
            if (!File.Exists(inputPath)) return;

            // Read and parse input JSON
            string jsonContent = File.ReadAllText(inputPath);
            var originalData = JsonConvert.DeserializeObject<WeatherDataJson>(jsonContent);
            
            if (originalData?.weatherData == null) return;

            // Create converted data structure
            var convertedData = new WeatherGridFormat
            {
                timeCount = TIME_COUNT,
                width = GRID_SIZE,
                height = GRID_SIZE,
                data = new TimeSliceData[TIME_COUNT]
            };

            // Process all 16 time points
            for (int hour = 0; hour < TIME_COUNT; hour++)
            {
                string timeKey = hour.ToString();
                if (!originalData.weatherData.TryGetValue(timeKey, out WeatherTimeSlice timeSlice))
                {
                    Debug.LogError($"Missing data for hour {hour}");
                    continue;
                }

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

            // Save converted data
            if (!Directory.Exists(CONVERTED_DIR))
            {
                Directory.CreateDirectory(CONVERTED_DIR);
            }

            string convertedPath = Path.Combine(CONVERTED_DIR, $"{baseName}.json");
            string json = JsonConvert.SerializeObject(convertedData, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
            File.WriteAllText(convertedPath, json, Encoding.UTF8);

            // Calculate normalization parameters
            var metadata = CalculateNormalizationParams(convertedData);

            // Save metadata
            SaveMetadata(metadata, baseName);

            // Normalize data
            var normalizedData = NormalizeData(convertedData, metadata);

            // Generate and save textures
            GenerateTextureFromData(normalizedData, baseName);

            Debug.Log($"Successfully processed {baseName}");
        }
        catch (System.Exception e) 
        {
            Debug.LogError($"Error converting file {inputPath}: {e.Message}");
        }
    }

    private NormalizationMetadata CalculateNormalizationParams(WeatherGridFormat data)
    {
        // Initialize metadata with extreme values
        var metadata = new NormalizationMetadata
        {
            windU_min = float.MaxValue,
            windU_max = float.MinValue,
            windV_min = float.MaxValue,
            windV_max = float.MinValue,
            rain_min = float.MaxValue,
            rain_max = float.MinValue
        };

        // Iterate through all time points and grid points to find global min/max values
        for (int t = 0; t < data.timeCount; t++)
        {
            var timeSlice = data.data[t];
            for (int y = 0; y < data.height; y++)
            {
                for (int x = 0; x < data.width; x++)
                {
                    var point = timeSlice.grid[y][x];
                    
                    // Update wind_u min/max
                    metadata.windU_min = Mathf.Min(metadata.windU_min, point.wind_u);
                    metadata.windU_max = Mathf.Max(metadata.windU_max, point.wind_u);
                    
                    // Update wind_v min/max
                    metadata.windV_min = Mathf.Min(metadata.windV_min, point.wind_v);
                    metadata.windV_max = Mathf.Max(metadata.windV_max, point.wind_v);
                    
                    // Update rain min/max
                    metadata.rain_min = Mathf.Min(metadata.rain_min, point.rain);
                    metadata.rain_max = Mathf.Max(metadata.rain_max, point.rain);
                }
            }
        }

        Debug.Log($"Normalization Parameters:");
        Debug.Log($"Wind U: {metadata.windU_min:F2} to {metadata.windU_max:F2}");
        Debug.Log($"Wind V: {metadata.windV_min:F2} to {metadata.windV_max:F2}");
        Debug.Log($"Rain: {metadata.rain_min:F2} to {metadata.rain_max:F2}");

        return metadata;
    }

    private WeatherGridFormat NormalizeData(WeatherGridFormat data, NormalizationMetadata metadata)
    {
        // Create a new WeatherGridFormat object with the same dimensions
        var normalizedData = new WeatherGridFormat
        {
            timeCount = data.timeCount,
            width = data.width,
            height = data.height,
            data = new TimeSliceData[data.timeCount]
        };

        // Normalize each time slice
        for (int t = 0; t < data.timeCount; t++)
        {
            var timeSlice = data.data[t];
            var normalizedGrid = new WeatherGridPoint[data.height][];

            for (int y = 0; y < data.height; y++)
            {
                normalizedGrid[y] = new WeatherGridPoint[data.width];
                for (int x = 0; x < data.width; x++)
                {
                    var point = timeSlice.grid[y][x];
                    normalizedGrid[y][x] = new WeatherGridPoint
                    {
                        // Normalize wind_u: (value - min) / (max - min)
                        wind_u = (point.wind_u - metadata.windU_min) / (metadata.windU_max - metadata.windU_min),
                        
                        // Normalize wind_v
                        wind_v = (point.wind_v - metadata.windV_min) / (metadata.windV_max - metadata.windV_min),
                        
                        // Normalize rain
                        rain = (point.rain - metadata.rain_min) / (metadata.rain_max - metadata.rain_min)
                    };
                }
            }

            normalizedData.data[t] = new TimeSliceData
            {
                timeIndex = t,
                grid = normalizedGrid
            };
        }

        return normalizedData;
    }

    private void GenerateTextureFromData(WeatherGridFormat normalizedData, string baseName)
    {
        // Create directory for textures if it doesn't exist
        string textureDir = Path.Combine(TEXTURES_DIR, baseName);
        if (!Directory.Exists(textureDir))
        {
            Directory.CreateDirectory(textureDir);
        }

        // Generate texture for each time slice
        for (int t = 0; t < normalizedData.timeCount; t++)
        {
            // Create new texture with RGBA32 format
            var texture = new Texture2D(GRID_SIZE, GRID_SIZE, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var timeSlice = normalizedData.data[t];
            
            // Set pixel colors for the texture
            for (int y = 0; y < GRID_SIZE; y++)
            {
                for (int x = 0; x < GRID_SIZE; x++)
                {
                    var point = timeSlice.grid[y][x];
                    
                    // Create color with normalized values:
                    // R: wind_u (normalized)
                    // G: wind_v (normalized)
                    // B: rain (normalized)
                    // A: 1.0 (fully opaque)
                    Color pixelColor = new Color(
                        point.wind_u,  // R channel
                        point.wind_v,  // G channel
                        point.rain,    // B channel
                        1.0f          // A channel
                    );
                    
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            
            // Apply all pixel changes
            texture.Apply();

            // Save texture as PNG
            byte[] bytes = texture.EncodeToPNG();
            string texturePath = Path.Combine(textureDir, $"hour_{t:D2}.png");
            File.WriteAllBytes(texturePath, bytes);

            // Clean up texture
            Destroy(texture);
        }
    }

    private void SaveMetadata(NormalizationMetadata metadata, string baseName)
    {
        try
        {
            // Create textures directory if it doesn't exist
            string textureDir = Path.Combine(TEXTURES_DIR, baseName);
            if (!Directory.Exists(textureDir))
            {
                Directory.CreateDirectory(textureDir);
            }

            // Create metadata file path
            string metadataPath = Path.Combine(textureDir, "metadata.json");

            // Serialize metadata with indented formatting
            string json = JsonConvert.SerializeObject(metadata, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });

            // Save metadata file
            File.WriteAllText(metadataPath, json, Encoding.UTF8);
            Debug.Log($"Saved metadata to {metadataPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving metadata: {e.Message}");
        }
    }
}
