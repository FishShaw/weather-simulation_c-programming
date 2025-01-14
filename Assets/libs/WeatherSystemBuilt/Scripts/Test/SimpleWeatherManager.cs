using UnityEngine;
using WeatherSystem.DataHandling;
using WeatherSystem.Mapping;

namespace WeatherSystem.Testing
{
    public class SimpleWeatherManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private WeatherDataLoader dataLoader;
        [SerializeField] private WeatherGridMapper gridMapper;
        [SerializeField] private SimpleTimeController timeController;
        
        [Header("Material")]
        [SerializeField] private Material weatherDataMaterial;

        private void Start()
        {
            // Auto-find components
            if (dataLoader == null) dataLoader = FindFirstObjectByType<WeatherDataLoader>();
            if (gridMapper == null) gridMapper = FindFirstObjectByType<WeatherGridMapper>();
            if (timeController == null) timeController = FindFirstObjectByType<SimpleTimeController>();

            // Initialize components
            gridMapper?.Initialize();
            dataLoader?.Initialize();

            // Setup material
            if (weatherDataMaterial != null)
            {
                weatherDataMaterial.SetFloat("_RainfallScale", WeatherData.RAINFALL_SCALE);
                weatherDataMaterial.SetFloat("_WindScale", WeatherData.WIND_SCALE);
                weatherDataMaterial.SetFloat("_WindOffset", WeatherData.WIND_OFFSET);
            }
        }

        private void Update()
        {
            if (!timeController.IsPlaying) return;
            UpdateWeatherData();
        }

        private void UpdateWeatherData()
        {
            WeatherData data = dataLoader.GetInterpolatedWeatherData(timeController.GetCurrentTime());
            if (data != null && weatherDataMaterial != null)
            {
                UpdateMaterialData(data);
            }
        }

        private void UpdateMaterialData(WeatherData data)
        {
            weatherDataMaterial.SetTexture("_RainfallTex", data.rainfallTexture);
            weatherDataMaterial.SetTexture("_WindUTex", data.windUTexture);
            weatherDataMaterial.SetTexture("_WindVTex", data.windVTexture);
        }
    }
}
