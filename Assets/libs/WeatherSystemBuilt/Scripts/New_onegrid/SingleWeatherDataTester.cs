using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using WeatherSystem.SingleGrid;

namespace WeatherSystem.Testing
{
    public class SingleWeatherDataTester : MonoBehaviour
    {
        [Header("Required Components")]
        [SerializeField] private SingleGridWeatherManager weatherManager;
        [SerializeField] private SingleWeatherDataLoader dataLoader;
        [SerializeField] private SingleWeatherGridMapper gridMapper;
        [SerializeField] private SingleSimpleTimeController timeController;
        
        [Header("Debug Display")]
        [SerializeField] private RawImage rainfallDisplay;
        [SerializeField] private RawImage windDisplay;
        [SerializeField] private TextMeshProUGUI debugText;

        private const float MAX_WIND_SPEED = 30f;
        private Texture2D rainfallTexture;
        private Texture2D windTexture;

        private void Start()
        {
            // Auto-find required components
            if (dataLoader == null) dataLoader = FindFirstObjectByType<SingleWeatherDataLoader>();
            if (gridMapper == null) gridMapper = FindFirstObjectByType<SingleWeatherGridMapper>();
            if (timeController == null) timeController = FindFirstObjectByType<SingleSimpleTimeController>();
            
            // Initialize textures
            rainfallTexture = new Texture2D(256, 256);
            windTexture = new Texture2D(256, 256);
            
            if (rainfallDisplay != null) rainfallDisplay.texture = rainfallTexture;
            if (windDisplay != null) windDisplay.texture = windTexture;
            
            if (debugText != null)
                debugText.text = "Waiting for data loading...";
        }

        private void OnDestroy()
        {
            if (rainfallTexture != null) Destroy(rainfallTexture);
            if (windTexture != null) Destroy(windTexture);
        }

        private void Update()
        {
            if (!ValidateComponents()) return;

            Vector3 cameraPos = Camera.main.transform.position;
            Vector2Int gridPos = gridMapper.WorldToGrid(cameraPos);
            DateTime currentTime = timeController.GetCurrentTime();
            SingleWeatherData weatherData = dataLoader.GetInterpolatedWeatherData(currentTime);

            if (weatherData != null)
            {
                UpdateTextureDisplay(weatherData, gridPos);
                UpdateDebugInfo(weatherData, gridPos, currentTime);
            }
        }

        private void UpdateDebugInfo(SingleWeatherData data, Vector2Int gridPos, DateTime time)
        {
            if (debugText == null) return;

            float rainfall = data.GetRainfallValue(gridPos);
            Vector2 wind = data.GetWindValue(gridPos);
            float windSpeed = wind.magnitude;
            float windDirection = Mathf.Atan2(wind.y, wind.x) * Mathf.Rad2Deg;
            
            string info = 
                $"<b>Time Information</b>\n" +
                $"Current Time: {time:yyyy-MM-dd HH:mm:ss}\n" +
                $"Play Status: {(timeController.IsPlaying ? "Playing" : "Paused")}\n\n" +
                
                $"<b>Position Information</b>\n" +
                $"Grid Position: ({gridPos.x}, {gridPos.y})\n" +
                $"Camera Position: ({Camera.main.transform.position.x:F1}, " +
                $"{Camera.main.transform.position.y:F1}, {Camera.main.transform.position.z:F1})\n\n" +
                
                $"<b>Weather Data</b>\n" +
                $"Rainfall Intensity: {rainfall:F2} mm/h\n" +
                $"Rainfall Level: {GetRainfallLevel(rainfall)}\n" +
                $"Wind Speed: {windSpeed:F2} m/s ({windSpeed * 3.6f:F1} km/h)\n" +
                $"Wind Direction: {GetWindDirection(windDirection)} ({windDirection:F1}°)\n" +
                $"Beaufort Scale: {GetBeaufortScale(windSpeed)}\n" +
                $"Wind Components: U={wind.x:F2} m/s, V={wind.y:F2} m/s";

            debugText.text = info;
        }

        private string GetRainfallLevel(float rainfall)
        {
            if (rainfall < 0.1f) return "No Rain";
            if (rainfall < 2.5f) return "Light Rain";
            if (rainfall < 8f) return "Moderate Rain";
            if (rainfall < 15.0f) return "Heavy Rain";
            if (rainfall < 30.0f) return "Very Heavy Rain";
            if (rainfall < 70.0f) return "Extreme Rain";
            return "Torrential Rain";
        }

        private string GetWindDirection(float angle)
        {
            string[] directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            float normalizedAngle = (angle + 360f) % 360f;
            int index = Mathf.RoundToInt(normalizedAngle / 45f) % 8;
            return directions[index];
        }

        private string GetBeaufortScale(float windSpeed)
        {
            if (windSpeed < 0.3f) return "Force 0 - Calm";
            if (windSpeed < 1.6f) return "Force 1 - Light Air";
            if (windSpeed < 3.4f) return "Force 2 - Light Breeze";
            if (windSpeed < 5.5f) return "Force 3 - Gentle Breeze";
            if (windSpeed < 8.0f) return "Force 4 - Moderate Breeze";
            if (windSpeed < 10.8f) return "Force 5 - Fresh Breeze";
            if (windSpeed < 13.9f) return "Force 6 - Strong Breeze";
            if (windSpeed < 17.2f) return "Force 7 - Near Gale";
            if (windSpeed < 20.8f) return "Force 8 - Gale";
            if (windSpeed < 24.5f) return "Force 9 - Strong Gale";
            if (windSpeed < 28.5f) return "Force 10 - Storm";
            if (windSpeed < 32.7f) return "Force 11 - Violent Storm";
            return "Force 12 - Hurricane";
        }

        private bool ValidateComponents()
        {
            if (dataLoader == null || gridMapper == null || timeController == null)
            {
                if (debugText != null)
                    debugText.text = "Missing required components!";
                return false;
            }
            return true;
        }

        private void UpdateTextureDisplay(SingleWeatherData data, Vector2Int gridPos)
        {
            // Update rainfall display
            if (rainfallDisplay != null && rainfallTexture != null)
            {
                float rainfall = data.GetRainfallValue(gridPos);
                Color rainfallColor = Color.Lerp(
                    Color.clear,
                    Color.blue,
                    rainfall / SingleWeatherData.RAINFALL_SCALE * 5f
                );
                
                for (int x = 0; x < rainfallTexture.width; x++)
                for (int y = 0; y < rainfallTexture.height; y++)
                {
                    rainfallTexture.SetPixel(x, y, rainfallColor);
                }
                rainfallTexture.Apply();
            }

            // Update wind display
            if (windDisplay != null && windTexture != null)
            {
                Vector2 wind = data.GetWindValue(gridPos);
                float windSpeed = wind.magnitude;
                float windAngle = Mathf.Atan2(wind.y, wind.x);
                
                Color windColor = Color.HSVToRGB(
                    (windAngle + Mathf.PI) / (2 * Mathf.PI),
                    windSpeed / MAX_WIND_SPEED,
                    1f
                );
                
                for (int x = 0; x < windTexture.width; x++)
                for (int y = 0; y < windTexture.height; y++)
                {
                    windTexture.SetPixel(x, y, windColor);
                }
                windTexture.Apply();
            }
        }
    }
} 