using UnityEngine;

namespace WeatherSystem.SingleGrid
{
    [CreateAssetMenu(fileName = "WeatherSettings_new", menuName = "Weather System/WeatherSettings_new")]
    public class WeatherSettings_new : ScriptableObject
    {
        [Header("Geographic Bounds")]
        public double defaultNorth = 56.002;  // lat_max
        public double defaultSouth = 49.0;    // lat_min
        public double defaultEast = 11.281;   // lon_max
        public double defaultWest = 0.0;      // lon_min

        // 使用常量定义固定步长
        public const float LAT_STEP = 0.018f; // (56.002 - 49.0) / (390 - 1)
        public const float LON_STEP = 0.029f; // (11.281 - 0.0) / (390 - 1)

        [Header("VFX Settings")]
        public float maxRainRate = 40f;           // 最大降雨率 mm/h
        public float maxWindSpeed = 35f;          // 最大风速 m/s
        public float transitionSpeed = 2f;        // 天气效果过渡速度
    }
}