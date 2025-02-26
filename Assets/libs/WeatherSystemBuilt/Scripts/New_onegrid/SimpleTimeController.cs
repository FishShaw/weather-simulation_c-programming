using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using Newtonsoft.Json;

namespace WeatherSystem.SingleGrid
{
    public class SimpleTimeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WeatherVFXController_new vfxController;
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Button playButton;
        [SerializeField] private Button fastButton;
        [SerializeField] private TMP_Text weatherInfoText;

        [Header("Time Control")]
        private bool isPlaying = false;
        private float playSpeed = 1f;
        private const float FRAME_DURATION = 3f; // 每帧持续3秒
        private const float NORMAL_SPEED = 1f / FRAME_DURATION; // 正常速度：3秒一帧
        private const float FAST_SPEED = 5f / FRAME_DURATION;   // 快速速度：0.6秒一帧

        // 气象数据范围
        private float windU_min, windU_max;
        private float windV_min, windV_max;
        private float rain_min, rain_max;
        private const string METADATA_PATH = "Assets/libs/WeatherSystemBuilt/Resources/WeatherData_json_textures/weather_data_20250127_125930/metadata.json";

        private void Start()
        {
            LoadMetadata();
            SetupUI();
            UpdateWeatherDisplay(0); // 显示初始帧
        }

        private void LoadMetadata()
        {
            try
            {
                string jsonText = File.ReadAllText(METADATA_PATH);
                var metadata = JsonConvert.DeserializeObject<WeatherMetadata>(jsonText);
                
                windU_min = metadata.windU_min;
                windU_max = metadata.windU_max;
                windV_min = metadata.windV_min;
                windV_max = metadata.windV_max;
                rain_min = metadata.rain_min;
                rain_max = metadata.rain_max;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading metadata: {e.Message}");
            }
        }

        private void SetupUI()
        {
            if (timeSlider != null)
            {
                timeSlider.minValue = 0;
                timeSlider.maxValue = 15;
                timeSlider.onValueChanged.AddListener(OnSliderValueChanged);
            }

            if (playButton != null)
                playButton.onClick.AddListener(TogglePlay);

            if (fastButton != null)
                fastButton.onClick.AddListener(ToggleFastPlay);
        }

        private void Update()
        {
            if (isPlaying && vfxController != null)
            {
                float newValue = timeSlider.value + Time.deltaTime * playSpeed;
                if (newValue >= 15f) newValue = 0f;
                timeSlider.value = newValue;
            }
        }

        public void OnSliderValueChanged(float value)
        {
            int current = Mathf.FloorToInt(value);
            int next = (current + 1) % vfxController.weatherMaps.Length;

            // 更新VFX纹理
            vfxController.vfx.SetTexture("_WeatherA", vfxController.weatherMaps[current]);
            vfxController.vfx.SetTexture("_WeatherB", vfxController.weatherMaps[next]);

            // 更新天气信息显示
            UpdateWeatherDisplay(current);
        }

        private void UpdateWeatherDisplay(int frameIndex)
        {
            if (vfxController == null || vfxController.weatherMaps == null) return;

            Texture2D currentTexture = vfxController.weatherMaps[frameIndex];
            if (!currentTexture.isReadable)
            {
                Debug.LogWarning("Texture is not readable. Enable Read/Write in import settings.");
                return;
            }

            // 读取中心点的颜色值
            // Color centerPixel = currentTexture.GetPixel(0, 0);
            Color centerPixel = currentTexture.GetPixel(32, 32);

            // 反归一化
            float windU = DenormalizeValue(centerPixel.r, windU_min, windU_max);
            float windV = DenormalizeValue(centerPixel.g, windV_min, windV_max);
            float rain = DenormalizeValue(centerPixel.b, rain_min, rain_max);

            if (weatherInfoText != null)
            {
                weatherInfoText.text = $"Frame: {frameIndex}\n" +
                                     $"Wind U: {windU:F2} m/s\n" +
                                     $"Wind V: {windV:F2} m/s\n" +
                                     $"Rain: {rain:F2} mm/h";
            }
        }

        private float DenormalizeValue(float normalizedValue, float min, float max)
        {
            return normalizedValue * (max - min) + min;
        }

        public void TogglePlay()
        {
            
            isPlaying = true;
            // isPlaying = !isPlaying;
            playSpeed = NORMAL_SPEED;
        }

        public void ToggleFastPlay()
        {
            isPlaying = true;
            playSpeed = FAST_SPEED;
        }

        private class WeatherMetadata
        {
            public float windU_min { get; set; }
            public float windU_max { get; set; }
            public float windV_min { get; set; }
            public float windV_max { get; set; }
            public float rain_min { get; set; }
            public float rain_max { get; set; }
        }
    }
} 