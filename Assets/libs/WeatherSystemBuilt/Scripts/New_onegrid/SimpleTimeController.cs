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

        [Header("Camera Control")]
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float cameraHeight = 500f;
        [SerializeField] private float cameraDistance = 50f;
        [SerializeField] private bool invertYAxis = false;
        
        private float rotationX = 0f;
        private float rotationY = 0f;
        private bool isCameraControlEnabled = true;

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

        [Header("Data Paths")]
        [Tooltip("元数据文件的路径，包含天气数据范围")]
        [SerializeField] private string metadataPath = "Assets/libs/WeatherSystemBuilt/Resources/WeatherData_json_textures/weather_data_Amsterdam_10km/metadata.json";

        private void Start()
        {
            LoadMetadata();
            SetupUI();
            UpdateWeatherDisplay(0); // show initial frame
            
            // move camera to initial position
            MoveToPosition(currentX, currentY);
            
            // Initial camera rotation
            rotationX = 0f;
            rotationY = 20f; // 给摄像机一个初始俯角
        }

        private void LoadMetadata()
        {
            try
            {
                string jsonText = File.ReadAllText(metadataPath);
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
                Debug.LogError($"Error loading metadata from path: {metadataPath}\nError: {e.Message}");
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
            // 天气时间流逝控制
            if (isPlaying && vfxController != null && timeSlider != null)
            {
                float newValue = timeSlider.value + Time.deltaTime * playSpeed;
                if (newValue >= 15f) newValue = 0f;
                timeSlider.value = newValue;
            }
            
            // 鼠标摄像机控制
            if (isCameraControlEnabled)
            {
                // 按住鼠标右键时才能旋转
                if (Input.GetMouseButton(1))
                {
                    rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
                    
                    float yAxis = Input.GetAxis("Mouse Y") * mouseSensitivity;
                    if (invertYAxis) yAxis = -yAxis;
                    rotationY -= yAxis;
                    
                    // 限制垂直角度，防止翻转
                    rotationY = Mathf.Clamp(rotationY, -80f, 80f);
                }
                
                // 更新摄像机位置和旋转
                UpdateCameraPosition();
            }
            
            // 启用/禁用摄像机控制（可选，按ESC键切换）
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isCameraControlEnabled = !isCameraControlEnabled;
            }
        }

        private void UpdateCameraPosition()
        {
            if (mainCamera == null || positionMarker == null) return;
            
            // 计算摄像机位置
            Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
            Vector3 targetPosition = positionMarker.transform.position;
            
            // 根据旋转计算摄像机偏移
            Vector3 cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);
            Vector3 cameraPosition = targetPosition + rotation * cameraOffset;
            
            // 设置摄像机位置和旋转
            mainCamera.transform.position = cameraPosition;
            mainCamera.transform.rotation = rotation;
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
                weatherInfoText.text = $"Position: ({x},{y})\n" +
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
            if (positionMarker == null) return;
            
            // 计算世界坐标位置
            Vector3 targetPosition = CalculateWorldPosition(x, y);
            
            // 将标记移动到目标位置
            positionMarker.transform.position = targetPosition;
            
            // 更新摄像机位置
            UpdateCameraPosition();
            
            // 更新当前坐标值
            currentX = x;
            currentY = y;
            
            // 更新天气信息显示
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