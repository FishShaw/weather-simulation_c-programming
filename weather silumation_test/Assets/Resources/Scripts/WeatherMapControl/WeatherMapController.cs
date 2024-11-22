using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WeatherSystem;

public class WeatherMapController : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] private WeatherDataLoader dataLoader;
    [SerializeField] private Material weatherMaterial;
    [SerializeField] private MeshRenderer mapRenderer;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI rainfallValueText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private RawImage rainfallLegend;
    [SerializeField] private TextMeshProUGUI legendMinValueText;
    [SerializeField] private TextMeshProUGUI legendMaxValueText;
    [SerializeField] private TextMeshProUGUI legendUnitText;

    [Header("Visualization Settings")]
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] [Range(0f, 1f)] private float defaultAlpha = 0.8f;
    [SerializeField] private bool showDebugInfo = false;

    private TimeStepData currentRainfallData;
    private float updateTimer;
    private Camera mainCamera;
    private bool isInitialized;
    private Texture2D colorRampTexture;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // 获取必要组件
        mainCamera = Camera.main;
        if (mapRenderer == null) mapRenderer = GetComponent<MeshRenderer>();

        // 初始化材质
        if (weatherMaterial == null)
        {
            weatherMaterial = new Material(Shader.Find("Custom/WeatherVisualization"));
        }

        if (weatherMaterial != null)
        {
            mapRenderer.material = weatherMaterial;
            weatherMaterial.SetFloat("_Alpha", defaultAlpha);

            // 创建颜色渐变图
            colorRampTexture = ColorRampGenerator.CreateRainfallColorRamp();
            weatherMaterial.SetTexture("_ColorRamp", colorRampTexture);
        }
        else
        {
            Debug.LogError("Failed to initialize weather material!");
            return;
        }

        // 初始化UI
        if (legendUnitText != null)
        {
            legendUnitText.text = "mm/h";
        }

        isInitialized = true;
        if (showDebugInfo) Debug.Log("WeatherMapController initialized successfully");
    }

    private void Update()
    {
        if (!isInitialized) return;

        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            UpdateRainfallValueAtMousePosition();
            updateTimer = 0f;
        }
    }

    public void UpdateWeatherDisplay(int hour)
    {
        if (!isInitialized) return;

        currentRainfallData = dataLoader.GetRainfallData(hour);
        if (currentRainfallData?.texture != null)
        {
            // 更新主纹理
            weatherMaterial.SetTexture("_MainTex", currentRainfallData.texture);

            // 更新时间显示
            if (timeText != null)
            {
                timeText.text = $"{hour:D2}:00";
            }

            // 更新图例
            UpdateLegend();

            if (showDebugInfo)
            {
                Debug.Log($"Updated weather display for hour {hour}");
            }
        }
    }

    private void UpdateRainfallValueAtMousePosition()
    {
        if (currentRainfallData?.texture == null || currentRainfallData.parsedMetadata == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
        {
            Vector2 uv = hit.textureCoord;
            Color pixelColor = currentRainfallData.texture.GetPixelBilinear(uv.x, uv.y);
            float normalizedValue = pixelColor.r;

            float actualRainfall = currentRainfallData.parsedMetadata.ConvertNormalizedToRealValue(
                normalizedValue, 
                WeatherDataType.Rainfall
            );

            if (rainfallValueText != null)
            {
                if (actualRainfall < 0.01f)
                {
                    rainfallValueText.text = "no rainfall";
                }
                else
                {
                    rainfallValueText.text = $"rainfall: {actualRainfall:F2} mm/h";
                }
            }
        }
    }

    private void UpdateLegend()
    {
        if (rainfallLegend == null || currentRainfallData?.parsedMetadata == null) return;

        // 创建图例纹理
        Texture2D legendTexture = new Texture2D(256, 30);
        for (int x = 0; x < 256; x++)
        {
            float normalizedValue = x / 255f;
            Color legendColor = colorRampTexture.GetPixelBilinear(normalizedValue, 0.5f);

            for (int y = 0; y < 30; y++)
            {
                legendTexture.SetPixel(x, y, legendColor);
            }
        }
        legendTexture.Apply();

        // 更新图例
        rainfallLegend.texture = legendTexture;

        // 更新最大最小值
        if (legendMinValueText != null)
        {
            legendMinValueText.text = $"{currentRainfallData.parsedMetadata.original_min:F1}";
        }
        if (legendMaxValueText != null)
        {
            legendMaxValueText.text = $"{currentRainfallData.parsedMetadata.original_max:F1}";
        }
    }

    public void SetAlpha(float alpha)
    {
        if (weatherMaterial != null)
        {
            weatherMaterial.SetFloat("_Alpha", alpha);
        }
    }

    private void OnDestroy()
    {
        // 清理资源
        if (colorRampTexture != null)
        {
            Destroy(colorRampTexture);
        }
        if (rainfallLegend != null && rainfallLegend.texture != null)
        {
            Destroy(rainfallLegend.texture);
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && isInitialized)
        {
            SetAlpha(defaultAlpha);
        }
    }
    #endif
}
