using UnityEngine;
using UnityEngine.VFX;
using System;
using WeatherSystem.DataHandling;
using WeatherSystem.TimeManagement;
using WeatherSystem.Mapping;

namespace WeatherSystem.VFX
{
    public class WindController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private VisualEffect windVFX;
        [SerializeField] private TimeController timeController;
        [SerializeField] private WeatherDataLoader dataLoader;
        [SerializeField] private WeatherGridMapper weatherGridMapper;

        [Header("VFX Parameters")]
        [SerializeField] private string windDirectionParameter = "WindDirection";
        [SerializeField] private string windSpeedParameter = "WindSpeed";
        
        [Header("Wind Settings")]
        [SerializeField] private float maxWindSpeed = 30f;  // 最大风速 m/s

        [Header("Performance")]
        [SerializeField] private float updateInterval = 0.5f;
        private float timeSinceLastUpdate = 0f;

        private void Start()
        {
            if (timeController == null)
                timeController = FindObjectOfType<TimeController>();
                
            if (dataLoader == null)
                dataLoader = FindObjectOfType<WeatherDataLoader>();

            timeController.OnTimeChanged += UpdateWindEffect;
        }

        private void Update()
        {
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate >= updateInterval)
            {
                timeSinceLastUpdate = 0f;
                UpdateWindEffect(timeController.GetCurrentTime());
            }
        }

        public void UpdateWindEffect(DateTime time)
        {
            WeatherData weatherData = dataLoader.GetInterpolatedWeatherData(time);
            if (weatherData == null) return;

            Vector3 cameraPosition = Camera.main.transform.position;
            Vector2Int gridPos = GetGridPositionFromWorldPosition(cameraPosition);
            
            // get the wind direction and speed
            Vector2 windVector = weatherData.GetWindValue(gridPos);
            
            // update the VFX parameters
            if (windVFX != null)
            {
                // turn the 2D wind vector to a 3D vector (x,z plane)
                Vector3 windDirection = new Vector3(windVector.x, 0, windVector.y).normalized;
                windVFX.SetVector3(windDirectionParameter, windDirection);
                
                // normalize the wind speed
                float normalizedSpeed = Mathf.Clamp01(windVector.magnitude / maxWindSpeed);
                windVFX.SetFloat(windSpeedParameter, normalizedSpeed);
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
                timeController.OnTimeChanged -= UpdateWindEffect;
            }
        }
    }
} 