// using UnityEngine;
// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;

// namespace WeatherSystem.SingleGrid
// {
//     public class WeatherManager_new : MonoBehaviour
//     {
//         [Header("Components")]
//         [SerializeField] private SimpleTimeController timeController;
//         [SerializeField] private WeatherCoordinates gridMapper;
//         [SerializeField] private WeatherVFXController_new vfxController;

//         [Header("Textures")]
//         [SerializeField] private int textureSize = 64;
//         private Texture2D[] windTextures;    // RGBA: U,V,Speed,Direction
//         private Texture2D[] rainTextures;     // R: Rainfall

//         private PythonWeatherPipe weatherPipe;
//         private Dictionary<int, WeatherData_new> weatherData;
//         private bool isInitialized = false;

//         private void Start()
//         {
//             Initialize();
//         }

//         private async void Initialize()
//         {
//             // 初始化组件
//             weatherPipe = gameObject.AddComponent<PythonWeatherPipe>();
//             gridMapper.Initialize();

//             // 创建贴图数组
//             windTextures = new Texture2D[16];
//             rainTextures = new Texture2D[16];
//             for (int i = 0; i < 16; i++)
//             {
//                 windTextures[i] = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
//                 rainTextures[i] = new Texture2D(textureSize, textureSize, TextureFormat.R8, false);
//             }

//             // 获取初始天气数据
//             await RequestWeatherData();
            
//             isInitialized = true;
//         }

//         private void Update()
//         {
//             if (!isInitialized || timeController == null) return;

//             UpdateWeatherEffects();
//         }

//         private async Task RequestWeatherData()
//         {
//             // 获取64x64网格的坐标
//             List<Vector2> coordinates = gridMapper.Generate64x64Grid();
//             weatherData = await weatherPipe.RequestWeatherData(Vector3.zero);
            
//             if (weatherData != null)
//             {
//                 GenerateWeatherTextures();
//                 Debug.Log("Weather data updated successfully");
//             }
//             else
//             {
//                 Debug.LogError("Failed to update weather data");
//             }
//         }

//         private void GenerateWeatherTextures()
//         {
//             for (int hour = 0; hour < 16; hour++)
//             {
//                 if (weatherData.TryGetValue(hour, out WeatherData_new data))
//                 {
//                     // 更新风贴图
//                     Color[] windPixels = new Color[textureSize * textureSize];
//                     for (int y = 0; y < textureSize; y++)
//                     for (int x = 0; x < textureSize; x++)
//                     {
//                         int i = y * textureSize + x;
//                         windPixels[i] = new Color(
//                             data.WindU[x, y],           // R: U分量
//                             data.WindV[x, y],           // G: V分量
//                             data.WindSpeed[x, y],       // B: 风速
//                             data.WindDirection[x, y]    // A: 风向
//                         );
//                     }
//                     windTextures[hour].SetPixels(windPixels);
//                     windTextures[hour].Apply();

//                     // 更新雨贴图
//                     Color[] rainPixels = new Color[textureSize * textureSize];
//                     for (int y = 0; y < textureSize; y++)
//                     for (int x = 0; x < textureSize; x++)
//                     {
//                         int i = y * textureSize + x;
//                         rainPixels[i] = new Color(data.Rain[x, y], 0, 0, 1);
//                     }
//                     rainTextures[hour].SetPixels(rainPixels);
//                     rainTextures[hour].Apply();
//                 }
//             }
//         }

//         private void UpdateWeatherEffects()
//         {
//             DateTime currentTime = timeController.GetCurrentTime();
//             int currentHour = currentTime.Hour;
//             float interpolationFactor = timeController.GetHourInterpolationFactor();
//             Vector2Int gridPos = new Vector2Int(0, 0);  // 或者根据需要计算具体的网格位置

//             if (weatherData.TryGetValue(currentHour, out WeatherData_new currentHourData) &&
//                 weatherData.TryGetValue((currentHour + 1) % 24, out WeatherData_new nextHourData))
//             {
//                 float rainfall = Mathf.Lerp(
//                     currentHourData.Rain[gridPos.x, gridPos.y], 
//                     nextHourData.Rain[gridPos.x, gridPos.y], 
//                     interpolationFactor
//                 );
//                 Vector2 wind = Vector2.Lerp(
//                     currentHourData.GetWindVector(gridPos),
//                     nextHourData.GetWindVector(gridPos),
//                     interpolationFactor
//                 );

//                 vfxController.UpdateWeatherEffects(rainfall, wind);
//             }
//         }

//         // 获取当前时刻的贴图
//         public (Texture2D wind, Texture2D rain) GetCurrentTextures()
//         {
//             if (!isInitialized) return (null, null);
            
//             int currentHour = timeController.GetCurrentTime().Hour;
//             return (windTextures[currentHour], rainTextures[currentHour]);
//         }
//     }
// }
