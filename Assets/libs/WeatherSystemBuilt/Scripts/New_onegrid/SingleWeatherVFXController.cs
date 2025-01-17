using UnityEngine;
using UnityEngine.VFX;

namespace WeatherSystem.SingleGrid
{
    public class SingleWeatherVFXController : MonoBehaviour
    {
        [SerializeField] private VisualEffect rainVFX;
        [SerializeField] private VisualEffect windVFX;
        [SerializeField] private float transitionSpeed = 2f;
        
        private float currentRainfall;
        private Vector2 currentWind;

        public void UpdateWeatherEffects(float rainfall, Vector2 wind)
        {
            // 平滑过渡到新的天气状态
            currentRainfall = Mathf.Lerp(currentRainfall, rainfall, Time.deltaTime * transitionSpeed);
            currentWind = Vector2.Lerp(currentWind, wind, Time.deltaTime * transitionSpeed);

            UpdateRainEffect(currentRainfall);
            UpdateWindEffect(currentWind);
        }

        private void UpdateRainEffect(float rainfall)
        {
            if (rainVFX == null) return;
            rainVFX.SetFloat("RainIntensity", rainfall);
        }

        private void UpdateWindEffect(Vector2 wind)
        {
            if (windVFX == null) return;
            
            Vector3 windDirection = new Vector3(wind.x, 0f, wind.y).normalized;
            float windSpeed = wind.magnitude;
            
            windVFX.SetVector3("WindDirection", windDirection);
            windVFX.SetFloat("WindSpeed", windSpeed);
            transform.forward = windDirection;
        }
    }
} 