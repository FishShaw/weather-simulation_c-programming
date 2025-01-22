using UnityEngine;
using UnityEngine.UI;
using WeatherSystem.SingleGrid;
using System.Threading.Tasks;

namespace WeatherSystem.Testing
{
    public class WeatherDataDisplayTester : MonoBehaviour
    {
        [Header("Debug Display")]
        [SerializeField] private RawImage rainfallDisplay;
        [SerializeField] private RawImage windDisplay;
        [SerializeField] private int currentHour = 0;
        
        private const float MAX_WIND_SPEED = 30f;
        private Texture2D rainfallTexture;
        private Texture2D windTexture;
        private WeatherData_new currentData;

        private async void Start()
        {
            InitializeTextures();
            await LoadAndDisplayWeatherData();
        }

        private void InitializeTextures()
        {
            // Initialize textures
            rainfallTexture = new Texture2D(64, 64);
            windTexture = new Texture2D(64, 64);
            
            rainfallTexture.filterMode = FilterMode.Bilinear;
            windTexture.filterMode = FilterMode.Bilinear;
            
            if (rainfallDisplay != null) rainfallDisplay.texture = rainfallTexture;
            if (windDisplay != null) windDisplay.texture = windTexture;
        }

        private async Task LoadAndDisplayWeatherData()
        {
            if (!WeatherDataStorage.Instance.IsDataLoaded)
            {
                Debug.Log("Loading weather data from Resources folder...");
                await WeatherDataStorage.Instance.LoadWeatherData();
            }

            currentData = WeatherDataStorage.Instance.GetHourData(currentHour);
            if (currentData != null)
            {
                UpdateTextureDisplay(currentData);
                Debug.Log($"Weather data visualization updated for hour {currentHour}");
            }
            else
            {
                Debug.LogError($"Failed to get weather data for hour {currentHour}");
            }
        }

        private void UpdateTextureDisplay(WeatherData_new data)
        {
            // Update rainfall display
            if (rainfallDisplay != null && rainfallTexture != null)
            {
                for (int x = 0; x < rainfallTexture.width; x++)
                for (int y = 0; y < rainfallTexture.height; y++)
                {
                    // 将纹理坐标映射到网格坐标
                    Vector2Int currentGridPos = new Vector2Int(
                        x * 64 / rainfallTexture.width,
                        y * 64 / rainfallTexture.height
                    );
                    
                    float rainfall = data.Rain[currentGridPos.x, currentGridPos.y];
                    Color rainfallColor = Color.Lerp(
                        Color.clear,      // 无雨
                        Color.blue,       // 小雨
                        rainfall / 100f    // 降雨量缩放因子
                    );
                    
                    rainfallTexture.SetPixel(x, y, rainfallColor);
                }
                rainfallTexture.Apply();
            }

            // Update wind display
            if (windDisplay != null && windTexture != null)
            {
                for (int x = 0; x < windTexture.width; x++)
                for (int y = 0; y < windTexture.height; y++)
                {
                    // 将纹理坐标映射到网格坐标
                    Vector2Int currentGridPos = new Vector2Int(
                        x * 64 / windTexture.width,
                        y * 64 / windTexture.height
                    );
                    
                    Vector2 wind = new Vector2(
                        data.WindU[currentGridPos.x, currentGridPos.y],
                        data.WindV[currentGridPos.x, currentGridPos.y]
                    );
                    float windSpeed = wind.magnitude;
                    float windAngle = Mathf.Atan2(wind.y, wind.x);
                    
                    // HSV颜色映射：
                    // H(色相) - 风向 (0-360度)
                    // S(饱和度) - 风速
                    // V(明度) - 保持最大
                    Color windColor = Color.HSVToRGB(
                        (windAngle + Mathf.PI) / (2f * Mathf.PI),  // 映射到0-1
                        Mathf.Clamp01(windSpeed / MAX_WIND_SPEED),
                        1f
                    );
                    
                    windTexture.SetPixel(x, y, windColor);
                }
                windTexture.Apply();
            }
        }

        private void OnDestroy()
        {
            // Clean up textures
            if (rainfallTexture != null) Destroy(rainfallTexture);
            if (windTexture != null) Destroy(windTexture);
        }
    }
}
