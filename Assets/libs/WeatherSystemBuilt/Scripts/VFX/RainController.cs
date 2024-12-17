using UnityEngine;
using UnityEngine.VFX;
using System;
using WeatherSystem.DataHandling;
using WeatherSystem.TimeManagement;
using WeatherSystem.Mapping;

namespace WeatherSystem.VFX
{
    public class RainController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private VisualEffect rainVFX;
        [SerializeField] private TimeController timeController;
        [SerializeField] private WeatherDataLoader dataLoader;
        [SerializeField] private WeatherGridMapper weatherGridMapper;

        [Header("VFX Parameters")]
        [SerializeField] private string spawnRateParameter = "SpawnRate";
        [SerializeField] private string rainIntensityParameter = "RainIntensity";
        
        [Header("Rain Settings")]
        [SerializeField] private float minSpawnRate = 100f;
        [SerializeField] private float maxSpawnRate = 2000f;
        [SerializeField] private float minRainIntensity = 0.1f;
        [SerializeField] private float maxRainIntensity = 1.0f;
        
        [Header("Performance")]
        [SerializeField] private float updateInterval = 0.5f;
        private float timeSinceLastUpdate = 0f;

        [Header("Wind Influence")]
        [SerializeField] private float windInfluence = 1.0f;
        [SerializeField] private string rainDirectionParameter = "RainDirection";

        private void Start()
        {
            if (timeController == null)
                timeController = FindObjectOfType<TimeController>();
                
            if (dataLoader == null)
                dataLoader = FindObjectOfType<WeatherDataLoader>();

            timeController.OnTimeChanged += UpdateRainEffect;
        }

        private void Update()
        {
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate >= updateInterval)
            {
                timeSinceLastUpdate = 0f;
                UpdateRainEffect(timeController.GetCurrentTime());
            }
        }

        public void UpdateRainEffect(DateTime time)
        {
            WeatherData weatherData = dataLoader.GetInterpolatedWeatherData(time);
            if (weatherData == null) return;

            // get the grid position from the camera position
            Vector3 cameraPosition = Camera.main.transform.position;
            Vector2Int gridPos = GetGridPositionFromWorldPosition(cameraPosition);
            
            // get the rainfall value from the weather data
            float rainfallValue = weatherData.GetRainfallValue(gridPos);
            Vector2 windVector = weatherData.GetWindValue(gridPos);
            
            if (rainVFX != null)
            {
                // update the rainfall parameters
                float normalizedRainfall = Mathf.InverseLerp(0f, 100f, rainfallValue);
                float spawnRate = Mathf.Lerp(minSpawnRate, maxSpawnRate, normalizedRainfall);
                float intensity = Mathf.Lerp(minRainIntensity, maxRainIntensity, normalizedRainfall);
                
                // calculate the rain direction (considering wind influence)
                Vector3 baseDirection = Vector3.down;
                Vector3 windDirection = new Vector3(windVector.x, 0, windVector.y) * windInfluence;
                Vector3 finalDirection = (baseDirection + windDirection).normalized;
                
                // set the parameters
                rainVFX.SetFloat(spawnRateParameter, spawnRate);
                rainVFX.SetFloat(rainIntensityParameter, intensity);
                rainVFX.SetVector3(rainDirectionParameter, finalDirection);
                
                // enable/disable the effect
                rainVFX.enabled = rainfallValue >= 0.1f;
            }
        }

        private Vector2Int GetGridPositionFromWorldPosition(Vector3 worldPos)
        {
            if (weatherGridMapper == null)
            {
                weatherGridMapper = FindObjectOfType<WeatherGridMapper>();
            }

            // convert the world position to RD coordinates
            double rdX = worldPos.x;
            double rdY = worldPos.z;
            
            return weatherGridMapper.RDToGridCoordinate(rdX, rdY);
        }

        private void OnDestroy()
        {
            if (timeController != null)
            {
                timeController.OnTimeChanged -= UpdateRainEffect;
            }
        }
    }
} 