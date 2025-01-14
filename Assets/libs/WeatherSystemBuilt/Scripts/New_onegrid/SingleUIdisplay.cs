using UnityEngine;
using TMPro;
using WeatherSystem.SingleGrid;

public class WeatherInfoDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infoText;
    private SingleGridWeatherManager weatherManager;
    private SingleWeatherGridMapper gridMapper;

    private void Start()
    {
        weatherManager = SingleGridWeatherManager.Instance;
        gridMapper = FindFirstObjectByType<SingleWeatherGridMapper>();

        if (weatherManager != null)
        {
            weatherManager.OnWeatherValuesUpdated += UpdateWeatherInfo;
        }
    }

    private void OnDestroy()
    {
        if (weatherManager != null)
        {
            weatherManager.OnWeatherValuesUpdated -= UpdateWeatherInfo;
        }
    }

    private void UpdateWeatherInfo(float rainfall, Vector2 wind)
    {
        if (infoText == null) return;

        Vector3 cameraPos = Camera.main.transform.position;
        Vector2Int gridPos = gridMapper.WorldToGrid(cameraPos);
        
        // 获取RD坐标
        var (rdX, rdY) = gridMapper.WorldToRD(cameraPos);
        
        // 获取地理坐标（WGS84）
        var (lat, lon) = gridMapper.RDToGeographic(rdX, rdY);

        float windSpeed = wind.magnitude;
        float windDirection = Mathf.Atan2(wind.y, wind.x) * Mathf.Rad2Deg;

        string info = 
            $"<b>Location Information</b>\n" +
            $"RD Coordinates: ({rdX:F1}, {rdY:F1})\n" +
            $"WGS84: ({lon:F6}°, {lat:F6}°)\n" +
            $"Weather Grid: ({gridPos.x}, {gridPos.y})\n\n" +
            
            $"<b>Weather Data</b>\n" +
            $"Rainfall: {rainfall:F2} mm/h\n" +
            $"Wind Speed: {windSpeed:F2} m/s ({windSpeed * 3.6f:F1} km/h)\n" +
            $"Wind Direction: {GetWindDirection(windDirection)} ({windDirection:F1}°)\n" +
            $"Wind Components: U={wind.x:F2} m/s, V={wind.y:F2} m/s";

        infoText.text = info;
    }

    private string GetWindDirection(float angle)
    {
        string[] directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        float normalizedAngle = (angle + 360f) % 360f;
        int index = Mathf.RoundToInt(normalizedAngle / 45f) % 8;
        return directions[index];
    }
}
