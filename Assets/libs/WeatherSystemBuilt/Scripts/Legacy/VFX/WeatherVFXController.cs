using UnityEngine;
using UnityEngine.VFX;
using System;
using WeatherSystem.Core;
using WeatherSystem.DataHandling;
using WeatherSystem.TimeManagement;
using WeatherSystem.Mapping;

namespace WeatherSystem.VFX
{
    public class WeatherVFXController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private VisualEffect weatherVFX;        // Reference to the Visual Effect Graph
        [SerializeField] private TimeController timeController;   // Reference to the time control system
        [SerializeField] private WeatherDataLoader dataLoader;   // Reference to the weather data loader
        [SerializeField] private WeatherGridMapper weatherGridMapper;  // Reference to the grid mapping system

        [Header("VFX Parameters")]
        // Parameter names in the Visual Effect Graph
        [SerializeField] private string rainIntensityParameter = "RainIntensity";
        [SerializeField] private string rainDirectionParameter = "RainDirection";
        [SerializeField] private string windDirectionParameter = "WindDirection";
        [SerializeField] private string windSpeedParameter = "WindSpeed";

        private WeatherSettings Settings => WeatherManager.Instance.Settings;

        private void Start()
        {
            // Find required components if not assigned
            if (timeController == null)
                timeController = FindFirstObjectByType<TimeController>();
                
            if (dataLoader == null)
                dataLoader = FindFirstObjectByType<WeatherDataLoader>();

            if (weatherGridMapper == null)
                weatherGridMapper = FindFirstObjectByType<WeatherGridMapper>();

            // Subscribe to time change events
            timeController.OnTimeChanged += UpdateWeatherEffects;
        }

        private void Update()
        {
            if (!timeController.IsPlaying) return;
            UpdateWeatherEffects(timeController.GetCurrentTime());
        }

        public void UpdateWeatherEffects(DateTime time)
        {
            // Get interpolated weather data for current time
            WeatherData weatherData = dataLoader.GetInterpolatedWeatherData(time);
            if (weatherData == null) return;

            // Convert camera position to grid coordinates
            Vector3 cameraPosition = Camera.main.transform.position;
            Vector2Int gridPos = weatherGridMapper.WorldToGrid(cameraPosition);
            
            UpdateWeatherVFX(weatherData, gridPos);
        }

        private void UpdateWeatherVFX(WeatherData data, Vector2Int gridPos)
        {
            if (weatherVFX == null) return;

            // Get rainfall value and convert to intensity
            float rainValue = data.GetRainfallValue(gridPos);
            float rainIntensity = Mathf.InverseLerp(0, WeatherData.RAINFALL_SCALE, rainValue);
            
            // Get wind vector for current position
            Vector2 windVector = data.GetWindValue(gridPos);
            
            // Update rain intensity
            weatherVFX.SetFloat(rainIntensityParameter, rainIntensity);
            
            // Calculate and update wind parameters
            Vector3 windDirection = new Vector3(windVector.x, 0f, windVector.y).normalized;
            float windSpeed = windVector.magnitude;
            
            weatherVFX.SetVector3(windDirectionParameter, windDirection);
            weatherVFX.SetFloat(windSpeedParameter, Mathf.Min(windSpeed, Settings.maxWindSpeed));
            
            // Calculate final rain direction influenced by wind
            Vector3 rainDirection = new Vector3(
                windVector.x * Settings.windInfluence,
                -1f,
                windVector.y * Settings.windInfluence
            ).normalized;
            weatherVFX.SetVector3(rainDirectionParameter, rainDirection);
        }

        private void OnDestroy()
        {
            // Unsubscribe from time change events
            if (timeController != null)
            {
                timeController.OnTimeChanged -= UpdateWeatherEffects;
            }
        }
    }
}
