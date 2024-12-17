using UnityEngine;
using System;
using WeatherSystem.Mapping;
using WeatherSystem.DataHandling;
using WeatherSystem.TimeManagement;
using WeatherSystem.VFX;
using System.Collections.Generic;

namespace WeatherSystem.Core
{
    public class WeatherManager : MonoBehaviour
    {
        public static WeatherManager Instance { get; private set; }

        [Header("Components")]
        [SerializeField] private WeatherSettings settings;
        [SerializeField] private WeatherGridMapper gridMapper;
        [SerializeField] private TimeController timeController;
        [SerializeField] private WeatherDataLoader dataLoader;

        [Header("Update Settings")]
        [SerializeField] private float updateInterval = 0.5f;
        private float timeSinceLastUpdate = 0f;

        private List<RainController> rainControllers = new List<RainController>();
        private List<WindController> windControllers = new List<WindController>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (settings == null)
            {
                Debug.LogError("Weather Settings not assigned!");
                return;
            }

            if (gridMapper == null)
                gridMapper = GetComponent<WeatherGridMapper>();

            if (timeController == null)
                timeController = FindObjectOfType<TimeController>();

            if (dataLoader == null)
                dataLoader = FindObjectOfType<WeatherDataLoader>();

            Initialize();
        }

        private void Initialize()
        {
            gridMapper?.Initialize();
            dataLoader?.Initialize();
            
            rainControllers.AddRange(FindObjectsOfType<RainController>());
            windControllers.AddRange(FindObjectsOfType<WindController>());
        }

        private void Update()
        {
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate >= updateInterval)
            {
                timeSinceLastUpdate = 0f;
                UpdateWeatherData();
            }
        }

        public void UpdateWeatherData()
        {
            if (timeController == null || dataLoader == null) return;

            DateTime currentTime = timeController.GetCurrentTime();
            WeatherData interpolatedData = dataLoader.GetInterpolatedWeatherData(currentTime);
            
            if (interpolatedData != null)
            {
                foreach (var controller in rainControllers)
                {
                    controller.UpdateRainEffect(currentTime);
                }
                foreach (var controller in windControllers)
                {
                    controller.UpdateWindEffect(currentTime);
                }
            }
        }

        private void OnValidate()
        {
            if (settings == null)
            {
                Debug.LogWarning("Please assign Weather Settings!");
            }
        }
    }
} 