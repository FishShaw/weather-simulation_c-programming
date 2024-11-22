using UnityEngine;
using System.Collections.Generic;
using WeatherSystem;

public class WeatherDataLoader : MonoBehaviour
{
    [Header("Data Paths")]
    [SerializeField] private string rainfallFolder = "Weatherdata/Texture/Rainfall";
    [SerializeField] private string metadataFolder = "Weatherdata/Metadata";
    [SerializeField] private string windFolder = "Weatherdata/Texture/Wind";

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private Dictionary<int, TimeStepData> rainfallData;

    void Awake()
    {
        LoadAllRainfallData();
    }

    private void LoadAllRainfallData()
    {
        rainfallData = new Dictionary<int, TimeStepData>();
        
        for (int hour = 0; hour < 24; hour++)
        {
            string nextHour = ((hour + 1) % 24).ToString("D2") + "00";
            string baseFilename = $"rainfall_{hour:D2}00_{nextHour}";
            
            if (showDebugInfo)
            {
                Debug.Log($"Attempting to load rainfall data for hour {hour:D2}:00");
                Debug.Log($"Looking for file: {rainfallFolder}/{baseFilename}");
            }
            
            TimeStepData data = LoadTimeStepData(baseFilename, rainfallFolder, "Rainfall");
            
            if (data == null)
            {
                Debug.LogWarning($"Failed to load rainfall data for hour {hour:D2}:00");
                continue;
            }
            
            rainfallData[hour] = data;
        }
    }

    public TimeStepData GetRainfallData(int hour)
    {
        if (rainfallData.ContainsKey(hour))
        {
            return rainfallData[hour];
        }
        return null;
    }

    public (TimeStepData, TimeStepData) GetWindData(int hour)
    {
        hour = Mathf.Clamp(hour, 0, 23);
        string timeStr = $"{hour:D3}";
        
        TimeStepData windU = LoadTimeStepData($"wind_u_10m_{timeStr}", windFolder, "Wind");
        TimeStepData windV = LoadTimeStepData($"wind_v_10m_{timeStr}", windFolder, "Wind");

        return (windU, windV);
    }

    private TimeStepData LoadTimeStepData(string baseFilename, string folder, string type)
    {
        TimeStepData data = new TimeStepData();
        
        try
        {
            // 加载纹理
            string texturePath = $"{folder}/{baseFilename}";
            data.texture = Resources.Load<Texture2D>(texturePath);
            
            // 加载元数据
            string metadataPath = $"{metadataFolder}/{type}/{baseFilename}_metadata";
            data.metadata = Resources.Load<TextAsset>(metadataPath);
            
            if (data.metadata != null)
            {
                data.parsedMetadata = JsonUtility.FromJson<WeatherMetadata>(data.metadata.text);
                if (showDebugInfo)
                {
                    Debug.Log($"Successfully loaded metadata from {metadataPath}");
                }
            }
            else
            {
                Debug.LogWarning($"Failed to load metadata from {metadataPath}");
            }

            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading {type} data: {e.Message}");
            return null;
        }
    }
}