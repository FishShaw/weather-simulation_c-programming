using UnityEngine;
using System;
using WeatherSystem.TimeManagement;
using WeatherSystem.Core;

namespace WeatherSystem.SingleGrid
{
    public class SingleGridWeatherManager : MonoBehaviour
    {
        public static SingleGridWeatherManager Instance { get; private set; }

        [Header("Components")]
        [SerializeField] private WeatherSettings settings;
        public WeatherSettings Settings => settings;
        [SerializeField] private TimeController timeController;
        [SerializeField] private Transform targetTransform;

        private SingleWeatherDataLoader dataLoader;
        private SingleWeatherGridMapper gridMapper;
        private Vector2Int currentGridPos;

        // 当前气象值
        private float currentRainfall;
        private Vector2 currentWind;
        
        // 提供给其他组件访问当前气象值的属性
        public float CurrentRainfall => currentRainfall;
        public Vector2 CurrentWind => currentWind;
        public Vector2Int CurrentGridPosition => currentGridPos;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (targetTransform == null)
                targetTransform = Camera.main.transform;

            if (timeController == null)
                timeController = FindFirstObjectByType<TimeController>();

            gridMapper = FindFirstObjectByType<SingleWeatherGridMapper>();
            dataLoader = FindFirstObjectByType<SingleWeatherDataLoader>();

            gridMapper?.Initialize();
            
            // 初始化当前网格位置
            currentGridPos = gridMapper.WorldToGrid(targetTransform.position);
            dataLoader?.Initialize();
        }

        private void Update()
        {
            if (!gridMapper.IsPositionInBounds(targetTransform.position))
            {
                Debug.LogWarning("Position out of weather data bounds!");
                return;
            }
            
            Vector2Int newGridPos = gridMapper.WorldToGrid(targetTransform.position);
            if (newGridPos != currentGridPos)
            {
                currentGridPos = newGridPos;
                dataLoader.UpdateGridPosition(currentGridPos);
            }
            
            UpdateWeatherValues();
        }

        private void UpdateWeatherValues()
        {
            var weatherData = dataLoader.GetCurrentWeatherData();
            if (weatherData != null)
            {
                currentRainfall = weatherData.GetRainfallValue(currentGridPos);
                currentWind = weatherData.GetWindValue(currentGridPos);
                OnWeatherValuesUpdated?.Invoke(currentRainfall, currentWind);
            }
        }

        // 天气值更新事件
        public delegate void WeatherValuesUpdatedHandler(float rainfall, Vector2 wind);
        public event WeatherValuesUpdatedHandler OnWeatherValuesUpdated;
    }
}
