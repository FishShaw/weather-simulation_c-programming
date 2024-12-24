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

        private List<WeatherVFXController> vfxControllers = new List<WeatherVFXController>();

        [Header("Visualization")]
        [SerializeField] private Material weatherDataMaterial;
        [SerializeField] private bool showWeatherTexture = false;

        private List<TerrainTileMapper> terrainMappers = new List<TerrainTileMapper>();

        public WeatherSettings Settings => settings;

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

            Initialize();
        }

        private void Initialize()
        {
            if (gridMapper == null)
                gridMapper = FindFirstObjectByType<WeatherGridMapper>();

            if (timeController == null)
                timeController = FindFirstObjectByType<TimeController>();

            if (dataLoader == null)
                dataLoader = FindFirstObjectByType<WeatherDataLoader>();

            gridMapper?.Initialize();
            dataLoader?.Initialize();

            // 获取所有VFX控制器
            vfxControllers.AddRange(FindObjectsByType<WeatherVFXController>(FindObjectsSortMode.None));

            // 获取所有地形映射器
            terrainMappers.AddRange(FindObjectsByType<TerrainTileMapper>(FindObjectsSortMode.None));

            // 初始化材质
            if (weatherDataMaterial != null)
            {
                weatherDataMaterial.SetFloat("_RainfallScale", settings.rainfallScale);
                weatherDataMaterial.SetFloat("_WindScale", settings.windScale);
                weatherDataMaterial.SetFloat("_WindOffset", settings.windOffset);
                weatherDataMaterial.SetFloat("_Opacity", showWeatherTexture ? 1 : 0);
            }
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
                UpdateMaterialData(interpolatedData);
                
                UpdateTerrainMapping();
                
                foreach (var controller in vfxControllers)
                {
                    controller.UpdateWeatherEffects(currentTime);
                }
            }
        }

        private void UpdateMaterialData(WeatherData data)
        {
            if (weatherDataMaterial == null) return;
            
            weatherDataMaterial.SetTexture("_RainfallTex", data.rainfallTexture);
            weatherDataMaterial.SetTexture("_WindUTex", data.windUTexture);
            weatherDataMaterial.SetTexture("_WindVTex", data.windVTexture);
        }

        private void UpdateTerrainMapping()
        {
            foreach (var mapper in terrainMappers)
            {
                Vector2[] uvs = mapper.GetWeatherUVs();
                if (uvs != null)
                {
                    Material instanceMaterial = new Material(weatherDataMaterial);
                    instanceMaterial.SetVector("_WeatherUV_BL", uvs[0]);
                    instanceMaterial.SetVector("_WeatherUV_BR", uvs[1]);
                    instanceMaterial.SetVector("_WeatherUV_TL", uvs[2]);
                    instanceMaterial.SetVector("_WeatherUV_TR", uvs[3]);
                    
                    MeshRenderer renderer = mapper.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = instanceMaterial;
                    }
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