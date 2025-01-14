using UnityEngine;
using UnityEngine.VFX;

namespace WeatherSystem.SingleGrid
{
    public class SingleWeatherVFXController : MonoBehaviour
    {
        [Header("VFX References")]
        [SerializeField] private VisualEffect rainVFX;
        [SerializeField] private VisualEffect windVFX;
        
        [Header("VFX Parameters")]
        [SerializeField] private string rainIntensityParameter = "RainIntensity";
        [SerializeField] private string windDirectionParameter = "WindDirection";
        [SerializeField] private string windSpeedParameter = "WindSpeed";

        [Header("Transition Settings")]
        [SerializeField] private float transitionSpeed = 5f;
        private float currentRainfall;
        private Vector2 currentWind;

        private void Start()
        {
            // 初始化VFX状态
            if (rainVFX != null)
                rainVFX.SetFloat(rainIntensityParameter, 0f);
            
            if (windVFX != null)
            {
                windVFX.SetVector3(windDirectionParameter, Vector3.zero);
                windVFX.SetFloat(windSpeedParameter, 0f);
            }
        }

        public void UpdateWeatherEffects(SingleWeatherData data, Vector2Int gridPos)
        {
            if (data == null) return;

            float targetRainfall = data.GetRainfallValue(gridPos);
            Vector2 targetWind = data.GetWindValue(gridPos);

            // 平滑过渡到新的天气状态
            currentRainfall = Mathf.Lerp(currentRainfall, targetRainfall, Time.deltaTime * transitionSpeed);
            currentWind = Vector2.Lerp(currentWind, targetWind, Time.deltaTime * transitionSpeed);

            UpdateRainEffect(currentRainfall);
            UpdateWindEffect(currentWind);
        }

        private void UpdateRainEffect(float rainfall)
        {
            if (rainVFX == null) return;
            
            float normalizedRainfall = Mathf.InverseLerp(0, SingleWeatherData.RAINFALL_SCALE, rainfall);
            rainVFX.SetFloat(rainIntensityParameter, normalizedRainfall);
        }

        private void UpdateWindEffect(Vector2 wind)
        {
            if (windVFX == null) return;
            
            Vector3 windDirection = new Vector3(wind.x, 0f, wind.y).normalized;
            float windSpeed = wind.magnitude;
            
            windVFX.SetVector3(windDirectionParameter, windDirection);
            windVFX.SetFloat(windSpeedParameter, windSpeed);

            // 可选：调整粒子系统的方向
            transform.forward = windDirection;
        }

        private void OnDestroy()
        {
            // 清理VFX状态
            if (rainVFX != null)
                rainVFX.SetFloat(rainIntensityParameter, 0f);
            
            if (windVFX != null)
            {
                windVFX.SetVector3(windDirectionParameter, Vector3.zero);
                windVFX.SetFloat(windSpeedParameter, 0f);
            }
        }
    }
} 