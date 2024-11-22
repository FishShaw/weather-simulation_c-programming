// WeatherData/Scripts/WeatherDataTypes.cs
using UnityEngine;

namespace WeatherSystem
{
    public enum WeatherDataType
    {
        Rainfall,
        WindU,
        WindV
    }

    [System.Serializable]
    public class Coordinates
    {
        public float lat_min;
        public float lat_max;
        public float lon_min;
        public float lon_max;
    }

    [System.Serializable]
    public class WeatherMetadata
    {
        public float original_min;
        public float original_max;
        public string parameter;
        public string units;
        public Coordinates coordinates;
        public string data_type;

        public float ConvertNormalizedToRealValue(float normalizedValue, WeatherDataType dataType)
        {
            switch (dataType)
            {
                case WeatherDataType.Rainfall:
                    return original_min + (original_max - original_min) * normalizedValue;
                
                case WeatherDataType.WindU:
                case WeatherDataType.WindV:
                    float absMax = Mathf.Max(Mathf.Abs(original_min), Mathf.Abs(original_max));
                    return (normalizedValue * 2 - 1) * absMax;
                
                default:
                    return normalizedValue;
            }
        }

        public float ConvertRealToNormalizedValue(float realValue, WeatherDataType dataType)
        {
            switch (dataType)
            {
                case WeatherDataType.Rainfall:
                    if (original_max == original_min) return 0;
                    return (realValue - original_min) / (original_max - original_min);
                
                case WeatherDataType.WindU:
                case WeatherDataType.WindV:
                    float absMax = Mathf.Max(Mathf.Abs(original_min), Mathf.Abs(original_max));
                    if (absMax == 0) return 0.5f;
                    return (realValue / absMax + 1) * 0.5f;
                
                default:
                    return realValue;
            }
        }
    }

    [System.Serializable]
    public class TimeStepData
    {
        public Texture2D texture;
        public TextAsset metadata;
        public WeatherMetadata parsedMetadata;
    }

}