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
        [SerializeField] private Camera mainCamera; // add camera

        [Header("Position Control")]
        [SerializeField] private TMP_InputField xCoordInput; // add x coord input
        [SerializeField] private TMP_InputField yCoordInput; // add y coord input
        [SerializeField] private Button setPositionButton; // add set position button
        [SerializeField] private Button centerButton; // add center position button
        [SerializeField] private GameObject positionMarker; // add position marker
        
        private int currentX = 32; // default x position
        private int currentY = 32; // default y position

        [Header("Time Control")]
        private bool isPlaying = false;
        private float playSpeed = 1f;
        private const float FRAME_DURATION = 3f; // every frame lasts 3 seconds
        private const float NORMAL_SPEED = 1f / FRAME_DURATION; // normal speed: 3 seconds per frame
        private const float FAST_SPEED = 5f / FRAME_DURATION;   // fast speed: 0.6 seconds per frame

        // weather data range
        private float windU_min, windU_max;
        private float windV_min, windV_max;
        private float rain_min, rain_max;
        private const string METADATA_PATH = "Assets/libs/WeatherSystemBuilt/Resources/WeatherData_json_textures/weather_data_20250127_125930/metadata.json";

        private void Start()
        {
            LoadMetadata();
            SetupUI();
            UpdateWeatherDisplay(0); // show initial frame
            
            // move camera to initial position
            MoveToPosition(currentX, currentY);
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
                
            // set position control UI
            if (setPositionButton != null)
                setPositionButton.onClick.AddListener(OnSetPositionClicked);
                
            if (centerButton != null)
                centerButton.onClick.AddListener(() => {
                    currentX = 32;
                    currentY = 32;
                    if (xCoordInput != null) xCoordInput.text = "32";
                    if (yCoordInput != null) yCoordInput.text = "32";
                    MoveToPosition(32, 32);
                    UpdateWeatherDisplay(Mathf.FloorToInt(timeSlider.value));
                });
                
            // initialize coordinate input boxes
            if (xCoordInput != null) xCoordInput.text = currentX.ToString();
            if (yCoordInput != null) yCoordInput.text = currentY.ToString();
        }

        private void Update()
        {
            if (isPlaying && vfxController != null && timeSlider != null)
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

            // update VFX texture
            vfxController.vfx.SetTexture("_WeatherA", vfxController.weatherMaps[current]);
            vfxController.vfx.SetTexture("_WeatherB", vfxController.weatherMaps[next]);

            // update weather info display
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

            // ensure the coordinate is within the texture range
            int x = Mathf.Clamp(currentX, 0, currentTexture.width - 1);
            int y = Mathf.Clamp(currentY, 0, currentTexture.height - 1);

            // read the color value of the specified point
            Color pixelData = currentTexture.GetPixel(x, y);

            // denormalize
            float windU = DenormalizeValue(pixelData.r, windU_min, windU_max);
            float windV = DenormalizeValue(pixelData.g, windV_min, windV_max);
            float rain = DenormalizeValue(pixelData.b, rain_min, rain_max);
            
            // calculate the wind speed
            float windMagnitude = Mathf.Sqrt(windU * windU + windV * windV);

            if (weatherInfoText != null)
            {
                Vector3 worldPos = CalculateWorldPosition(x, y);
                weatherInfoText.text = $"Position: ({x},{y})\n" +
                                     $"World: ({worldPos.x:F1}, {worldPos.z:F1})\n" +
                                     $"Hour: {frameIndex}\n" +
                                     $"Wind: {windMagnitude:F2} m/s ({windU:F2}, {windV:F2})\n" +
                                     $"Rain: {rain:F2} mm/h";
            }
        }

        public void OnSetPositionClicked()
        {
            if (xCoordInput != null && yCoordInput != null)
            {
                // parse the coordinate input
                if (int.TryParse(xCoordInput.text, out int x) && 
                    int.TryParse(yCoordInput.text, out int y))
                {
                    if (vfxController != null && vfxController.weatherMaps != null && 
                        vfxController.weatherMaps.Length > 0)
                    {
                        // ensure the coordinate is within the texture range
                        Texture2D tex = vfxController.weatherMaps[0];
                        x = Mathf.Clamp(x, 0, tex.width - 1);
                        y = Mathf.Clamp(y, 0, tex.height - 1);
                        
                        // update the current coordinate
                        currentX = x;
                        currentY = y;
                        
                        // move to the specified position
                        MoveToPosition(x, y);
                        
                        // update the display
                        UpdateWeatherDisplay(Mathf.FloorToInt(timeSlider.value));
                    }
                }
                else
                {
                    Debug.LogWarning("Invalid coordinate input!");
                }
            }
        }
        
        private void MoveToPosition(int x, int y)
        {
            if (mainCamera == null) return;
            
            // calculate the world coordinate position
            Vector3 targetPosition = CalculateWorldPosition(x, y);
            
            // set the camera position (keep the y height unchanged)
            Vector3 cameraPos = mainCamera.transform.position;
            mainCamera.transform.position = new Vector3(
                targetPosition.x, 
                cameraPos.y, 
                targetPosition.z
            );
            
            // keep the camera always facing the terrain center, but keep the horizontal view
            Vector3 terrainCenter = Vector3.zero; // the terrain center should be close to (0,0,0)
            
            // calculate the horizontal direction vector (only consider the x and z components)
            Vector3 lookDirection = terrainCenter - mainCamera.transform.position;
            lookDirection.y = 0; // force the horizontal direction, eliminate the vertical component
            
            if (lookDirection.magnitude > 0.001f) // avoid errors when at the center point
            {
                // only rotate the Y axis (horizontal rotation)
                float targetAngle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
                mainCamera.transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            }
            
            // if there is a position marker, move to the corresponding position
            if (positionMarker != null)
            {
                positionMarker.transform.position = targetPosition;
            }
            
            // update the current coordinate value
            currentX = x;
            currentY = y;
            
            // update the weather info displayhe weather info display
            UpdateWeatherDisplay(Mathf.FloorToInt(timeSlider.value));
        }
        
        private Vector3 CalculateWorldPosition(int pixelX, int pixelY)
        {
            // the four corners of the terrain (from terrain_corners.json)
            Vector3 leftBottom = new Vector3(-5160.96f, 0f, -5160.96f);  // (0,0) pixel point
            Vector3 rightBottom = new Vector3(5160.96f, 0f, -5160.96f);  // (63,0) pixel point
            Vector3 leftTop = new Vector3(-5160.96f, 0f, 5160.96f);      // (0,63) pixel point
            Vector3 rightTop = new Vector3(5160.96f, 0f, 5160.96f);      // (63,63) pixel point
            
            // calculate the total range of the x and z axes
            float xRange = rightBottom.x - leftBottom.x;  // the total width of the x axis
            float zRange = leftTop.z - leftBottom.z;      // the total height of the z axis
            
            // calculate the step size of each pixel
            float xStep = xRange / 63f;
            float zStep = zRange / 63f;
            
            // calculate the world coordinate (starting from the left bottom corner)
            // note: here we use pixelX and pixelY multiplied by the step size, which is consistent with the conversion method in WeatherCoordinates
            float worldX = leftBottom.x + (pixelX * xStep);
            float worldZ = leftBottom.z + (pixelY * zStep);
            
            return new Vector3(worldX, 0f, worldZ);
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