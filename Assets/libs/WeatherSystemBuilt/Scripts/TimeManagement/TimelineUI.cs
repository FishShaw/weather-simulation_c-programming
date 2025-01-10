using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using WeatherSystem.TimeManagement;
using WeatherSystem.DataHandling;
using WeatherSystem.Core;
using WeatherSystem.Mapping;

[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
public class TimelineUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimeController timeController;
    [SerializeField] private Button playButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button speedUpButton;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("Weather Info UI")]
    [SerializeField] private TextMeshProUGUI rainfallText;  // Display rainfall
    [SerializeField] private TextMeshProUGUI windText;      // Display wind info
    
    [Header("Settings")]
    [SerializeField] private string timeFormat = "MM/dd HH:mm";
    [SerializeField] private float updateInterval = 0.5f;   // Update interval
    
    private WeatherDataLoader dataLoader;
    private WeatherGridMapper gridMapper;
    private float timeSinceLastUpdate = 0f;

    private void Start()
    {
        InitializeComponents();
        SetupTimeController();
        SetupUI();
        
        // Get required components
        dataLoader = FindFirstObjectByType<WeatherDataLoader>();
        gridMapper = FindFirstObjectByType<WeatherGridMapper>();
    }

    private void Update()
    {
        // Periodically update weather info display
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            timeSinceLastUpdate = 0f;
            UpdateWeatherInfoDisplay();
        }
    }

    private void UpdateWeatherInfoDisplay()
    {
        if (dataLoader == null || gridMapper == null) return;

        // Get current camera position
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector2Int gridPos = gridMapper.RDToGridCoordinate(cameraPosition.x, cameraPosition.z);

        // Get weather data for current time
        WeatherData weatherData = dataLoader.GetInterpolatedWeatherData(timeController.GetCurrentTime());
        if (weatherData == null) return;

        // Get rainfall value
        float rainfall = weatherData.GetRainfallValue(gridPos);
        // Get wind direction and speed
        Vector2 wind = weatherData.GetWindValue(gridPos);
        float windSpeed = wind.magnitude;
        float windDirection = Mathf.Atan2(wind.y, wind.x) * Mathf.Rad2Deg;

        // Update UI display
        if (rainfallText != null)
        {
            rainfallText.text = $"Rainfall: {rainfall:F1} mm/h";
        }

        if (windText != null)
        {
            string direction = GetWindDirection(windDirection);
            windText.text = $"Wind Speed: {windSpeed:F1} m/s\nDirection: {direction}";
        }
    }

    private string GetWindDirection(float angle)
    {
        // Convert angle to 8 main wind directions
        angle = (angle + 360) % 360;
        if (angle < 22.5f || angle >= 337.5f) return "North";
        if (angle < 67.5f) return "NorthEast";
        if (angle < 112.5f) return "East";
        if (angle < 157.5f) return "SouthEast";
        if (angle < 202.5f) return "South";
        if (angle < 247.5f) return "SouthWest";
        if (angle < 292.5f) return "West";
        return "NorthWest";
    }

    private void InitializeComponents()
    {
        // Ensure there is EventSystem
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[TimelineUI] Created EventSystem");
        }

        // Set Canvas
        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1;
        }
    }

    private void SetupTimeController()
    {
        if (timeController == null)
        {
            timeController = FindFirstObjectByType<TimeController>();
            if (timeController == null)
            {
                Debug.LogError("[TimelineUI] TimeController not found!");
                return;
            }
        }
        timeController.OnTimeChanged += UpdateTimeDisplay;
        Debug.Log("[TimelineUI] TimeController setup complete");
    }

    private void SetupUI()
    {
        if (!ValidateUIReferences()) return;

        SetupButtons();
        SetupSlider();
        UpdateButtonStates(false);
        Debug.Log("[TimelineUI] UI setup complete");
    }

    private bool ValidateUIReferences()
    {
        if (playButton == null || stopButton == null || speedUpButton == null || timeSlider == null || timeText == null)
        {
            Debug.LogError("[TimelineUI] Missing UI references!");
            return false;
        }
        return true;
    }

    private void SetupButtons()
    {
        Debug.Log("[TimelineUI] Starting button setup");
        
        // Check button references
        if (playButton == null || stopButton == null || speedUpButton == null)
        {
            Debug.LogError("[TimelineUI] One or more buttons are null!");
            return;
        }

        // Remove old listeners
        playButton.onClick.RemoveAllListeners();
        stopButton.onClick.RemoveAllListeners();
        speedUpButton.onClick.RemoveAllListeners();

        // Add new listeners
        playButton.onClick.AddListener(() => {
            Debug.Log("[TimelineUI] Play button clicked - before action");
            OnPlayClick();
            Debug.Log("[TimelineUI] Play button clicked - after action");
        });

        stopButton.onClick.AddListener(() => {
            Debug.Log("[TimelineUI] Stop button clicked - before action");
            OnStopClick();
            Debug.Log("[TimelineUI] Stop button clicked - after action");
        });

        speedUpButton.onClick.AddListener(() => {
            Debug.Log("[TimelineUI] SpeedUp button clicked - before action");
            OnSpeedUpClick();
            Debug.Log("[TimelineUI] SpeedUp button clicked - after action");
        });

        Debug.Log("[TimelineUI] Button setup completed");
    }

    private void SetupSlider()
    {
        timeSlider.onValueChanged.RemoveAllListeners();
        timeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        timeSlider.value = 0;
    }

    private void OnSliderValueChanged(float value)
    {
        if (timeController == null) return;
        
        DateTime startTime = new DateTime(2024, 7, 9, 12, 0, 0);
        DateTime endTime = new DateTime(2024, 7, 10, 3, 0, 0);
        TimeSpan totalDuration = endTime - startTime;
        
        DateTime newTime = startTime.AddMinutes(totalDuration.TotalMinutes * value);
        timeController.SetTime(newTime);
        Debug.Log($"[TimelineUI] Slider changed to: {value}, Time set to: {newTime}");
    }

    private void UpdateTimeDisplay(DateTime time)
    {
        if (timeText != null)
        {
            timeText.text = time.ToString(timeFormat);
            Debug.Log($"[TimelineUI] Time display updated: {time}");
        }
    }

    private void OnPlayClick()
    {
        timeController?.Play();
        UpdateButtonStates(true);
        Debug.Log("[TimelineUI] Play clicked");
    }

    private void OnStopClick()
    {
        timeController?.Stop();
        UpdateButtonStates(false);
        Debug.Log("[TimelineUI] Stop clicked");
    }

    private void OnSpeedUpClick()
    {
        timeController?.SpeedUp();
        Debug.Log("[TimelineUI] Speed up clicked");
    }

    private void UpdateButtonStates(bool isPlaying)
    {
        playButton.interactable = !isPlaying;
        stopButton.interactable = isPlaying;
        speedUpButton.interactable = isPlaying;
        Debug.Log($"[TimelineUI] Button states updated - Play:{!isPlaying}, Stop:{isPlaying}, SpeedUp:{isPlaying}");
    }

    private void OnDestroy()
    {
        if (timeController != null)
        {
            timeController.OnTimeChanged -= UpdateTimeDisplay;
        }
    }
} 