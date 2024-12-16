using UnityEngine;

namespace WeatherSystem.Core
{
    [CreateAssetMenu(fileName = "WeatherSettings", menuName = "Weather System/Weather Settings")]
    public class WeatherSettings : ScriptableObject
    {
        [Header("Grid Settings")]
        public int gridWidth = 390;
        public int gridHeight = 390;
        public float gridResolution = 2.5f; // 2.5km per grid cell
        
        [Header("Geographic Bounds")]
        // bounds of the netherlands, acquired from the KNMI HARMONIE data
        public double defaultNorth = 56.002;  // lat_max
        public double defaultSouth = 49.0;    // lat_min
        public double defaultEast = 11.281;   // lon_max
        public double defaultWest = 0.0;      // lon_min

        [Header("Data Settings")]
        public string dataPath = "WeatherData";
        public float updateInterval = 1.0f;

        [Header("KNMI HARMONIE Data")]
        public string rainfallPath = "Texture_new_raw/rainfall/PNG";
        public string windPath = "Texture_new_raw/wind/PNG";
        public string metadataPath = "Metadata_new_raw";
    }
} 