using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using WeatherSystem.SingleGrid;

namespace WeatherSystem.Test
{
    public class ShowEffectAtPixel : MonoBehaviour
    {
        [Header("VFX References")]
        [SerializeField] private WeatherVFXController_new vfxController;
        
        [Header("UI Controls")]
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Button playButton;
        [SerializeField] private Button fastButton;
        [SerializeField] private TMP_Text pixelInfoText;
        
        [Header("Test Settings")]
        [SerializeField] private int pixelX = 0;
        [SerializeField] private int pixelY = 0;
        [SerializeField] private float testAreaSize = 100f;
        [SerializeField] private GameObject testAreaVisualizer;
        
        [Header("Time Control")]
        private bool isPlaying = false;
        private float playSpeed = 1f;
        private const float NORMAL_SPEED = 0.33f; // Normal speed: 3 seconds per frame
        private const float FAST_SPEED = 1.67f;   // Fast speed: 0.6 seconds per frame
        
        // Weather data ranges
        private float windU_min, windU_max;
        private float windV_min, windV_max;
        private float rain_min, rain_max;
        private const string METADATA_PATH = "Assets/libs/WeatherSystemBuilt/Resources/WeatherData_json_textures/weather_data_20250127_125930/metadata.json";
        
        // Terrain parameters
        private float terrainSize = 10321.92f;
        private Vector3 testPosition;
        
        private void Start()
        {
            LoadMetadata();
            SetupUI();
            
            // Calculate test position
            testPosition = CalculateWorldPosition(pixelX, pixelY);
            
            // Visualize test area
            CreateTestAreaVisualizer();
            
            // Update initial information
            if (vfxController != null && vfxController.weatherMaps != null && vfxController.weatherMaps.Length > 0)
            {
                UpdatePixelInfo(0);
            }
            else
            {
                Debug.LogError("VFX Controller or weather maps array not set or empty!");
            }
        }

        private Vector3 CalculateWorldPosition(int pixelX, int pixelY)
        {
            return new Vector3(
                (pixelX / 63.0f * terrainSize) - terrainSize / 2,
                0,
                (pixelY / 63.0f * terrainSize) - terrainSize / 2
            );
        }
        
        private void CreateTestAreaVisualizer()
        {
            if (testAreaVisualizer != null)
            {
                testAreaVisualizer.transform.position = testPosition;
                testAreaVisualizer.transform.localScale = new Vector3(testAreaSize, 1, testAreaSize);
            }
            
            Debug.DrawRay(testPosition, Vector3.up * 1000, Color.red, Mathf.Infinity);
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
            if (isPlaying && timeSlider != null)
            {
                float newValue = timeSlider.value + Time.deltaTime * playSpeed;
                if (newValue >= 15f) newValue = 0f;
                
                // Temporarily remove listener to avoid callback conflicts
                timeSlider.onValueChanged.RemoveAllListeners();
                timeSlider.value = newValue;
                timeSlider.onValueChanged.AddListener(OnSliderValueChanged);
                
                // Update our display with current frame
                UpdatePixelInfo(Mathf.FloorToInt(newValue));
            }
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

        public void OnSliderValueChanged(float value)
        {
            // Only update our display, don't set VFX parameters
            UpdatePixelInfo(Mathf.FloorToInt(value));
        }

        private void UpdatePixelInfo(int frameIndex)
        {
            if (vfxController == null || vfxController.weatherMaps == null) return;
            
            // Ensure frameIndex is in valid range
            if (frameIndex < 0 || frameIndex >= vfxController.weatherMaps.Length)
            {
                Debug.LogError($"Frame index {frameIndex} out of range (0-{vfxController.weatherMaps.Length-1})");
                return;
            }

            Texture2D currentTexture = vfxController.weatherMaps[frameIndex];
            if (currentTexture == null)
            {
                Debug.LogError($"Texture for frame {frameIndex} is null");
                return;
            }
            
            if (!currentTexture.isReadable)
            {
                Debug.LogWarning("Texture is not readable. Enable Read/Write in import settings.");
                return;
            }

            // Ensure pixel coordinates are within texture bounds
            if (pixelX < 0 || pixelX >= currentTexture.width || pixelY < 0 || pixelY >= currentTexture.height)
            {
                Debug.LogError($"Pixel coordinates ({pixelX},{pixelY}) out of texture dimensions ({currentTexture.width}x{currentTexture.height})");
                return;
            }

            // Read color value at specified position
            Color pixelData = currentTexture.GetPixel(pixelX, pixelY);

            // Denormalize values
            float windU = DenormalizeValue(pixelData.r, windU_min, windU_max);
            float windV = DenormalizeValue(pixelData.g, windV_min, windV_max);
            float rain = DenormalizeValue(pixelData.b, rain_min, rain_max);

            // Calculate wind magnitude
            Vector2 windVector = new Vector2(windU, windV);
            float windMagnitude = windVector.magnitude;

            // Update UI display
            if (pixelInfoText != null)
            {
                pixelInfoText.text = $"Test Position: ({pixelX},{pixelY})\n" +
                                     $"World Coords: ({testPosition.x:F1}, {testPosition.z:F1})\n" +
                                     $"Hour: {frameIndex}\n" +
                                     $"Wind: {windMagnitude:F2} m/s ({windU:F2}, {windV:F2})\n" +
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
