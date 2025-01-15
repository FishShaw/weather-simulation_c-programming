using UnityEngine;

namespace WeatherSystem.Core
{
    [CreateAssetMenu(fileName = "WeatherSettings", menuName = "Weather System/Weather Settings")]
    public class WeatherSettings : ScriptableObject
    {
        [Header("Grid Settings")]
        public int gridWidth = 390;
        public int gridHeight = 390;
        
        [Header("Geographic Bounds")]
        public double defaultNorth = 56.002;  // lat_max
        public double defaultSouth = 49.0;    // lat_min
        public double defaultEast = 11.281;   // lon_max
        public double defaultWest = 0.0;      // lon_min

        // 使用常量定义固定步长
        public const float LAT_STEP = 0.018f; // (56.002 - 49.0) / (390 - 1)
        public const float LON_STEP = 0.029f; // (11.281 - 0.0) / (390 - 1)

        // 实际物理距离（仅用于参考）
        public const float GRID_SIZE_KM = 2.0f; // 约2km

        [Header("Data Settings")]
        public string dataPath = "WeatherData";
        public float updateInterval = 1.0f;

        [Header("KNMI HARMONIE Data")]
        public string rainfallPath = "Texture_new_raw/rainfall/PNG";
        public string windPath = "Texture_new_raw/wind/PNG";
        public string metadataPath = "Metadata_new_raw";

        [Header("Shader Parameters")]
        public float rainfallScale = 100.0f;      // 降水数据缩放
        public float windScale = 327.67f;         // 风力数据缩放
        public float windOffset = 100.0f;         // 风力数据偏移
        public bool showWeatherTexture = false;   // 是否显示2D天气纹理
        
        [Header("Material Settings")]
        public Material weatherDataMaterial;       // 天气数据材质模板
        
        [Header("VFX Settings")]
        public float maxRainRate = 1000f;         // 最大降雨率
        public float maxWindSpeed = 30f;          // 最大风速
        public float particleLifetime = 2f;       // 粒子生命周期
        public float windInfluence = 1.0f;        // 风对雨滴的影响程度
    }
} 