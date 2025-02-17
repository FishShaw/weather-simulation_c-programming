using UnityEngine;
using WeatherSystem.SingleGrid;
using System.Threading.Tasks;
using Wander;

public class AmsterdamWeatherTester : MonoBehaviour
{
    [SerializeField] private TerrainBuilder terrainBuilder;
    
    private void Start()
    {
        // 使用非异步方法调用异步方法
        _ = RunTest();
    }

    private async Task RunTest()
    {
        if (terrainBuilder == null)
            terrainBuilder = FindFirstObjectByType<TerrainBuilder>();
            
        if (terrainBuilder != null)
        {
            // 不再硬编码坐标，而是使用TerrainBuilder当前的坐标
            Debug.Log($"=== Testing Weather Data ===");
            Debug.Log($"Center Position: RD({terrainBuilder.originRDX}, {terrainBuilder.originRDY})");
            
            // 刷新天气数据并等待完成
            await WeatherDataStorage.Instance.RefreshData();
            
            if (!WeatherDataStorage.Instance.IsDataLoaded)
            {
                await WeatherDataStorage.Instance.LoadWeatherData();
            }
            
            TestWeatherData();
        }
    }

    private void TestWeatherData()
    {
        var weatherData = WeatherDataStorage.Instance.WeatherData;
        var coordinates = WeatherDataStorage.Instance.Coordinates;
        
        if (weatherData != null)
        {
            // 获取中心点坐标并显示
            var weatherCoords = FindFirstObjectByType<WeatherCoordinates>();
            // 使用RDUtils.RD2GPS进行坐标转换
            RDUtils.RD2GPS(terrainBuilder.originRDX, terrainBuilder.originRDY, out double lat, out double lon);
            
            Debug.Log($"\n=== Center Point Coordinates ===");
            Debug.Log($"RD Coordinates: ({terrainBuilder.originRDX:F2}, {terrainBuilder.originRDY:F2})");
            Debug.Log($"LatLon Coordinates: ({lat:F6}°N, {lon:F6}°E)");
            
            // 参考WeatherSystemTester的测试点
            int[] testPoints = new[] { 0, 63, 4032, 4095 };
            string[] cornerNames = new[] { "Southwest", "Southeast", "Northwest", "Northeast" };
            
            // 测试16小时的数据
            for (int hour = 0; hour < 16; hour++)
            {
                if (weatherData.TryGetValue(hour, out WeatherData_new data))
                {
                    Debug.Log($"\n=== Hour {hour:D2}:00 ===");
                    
                    // 获取统计信息
                    var stats = WeatherDataStorage.Instance.GetWeatherStatistics(hour);
                    
                    // 打印降雨统计
                    Debug.Log($"Rainfall Statistics:");
                    Debug.Log($"  Average: {stats.AverageRainfall:F2} mm/h");
                    Debug.Log($"  Min: {stats.MinRainfall:F2} mm/h at ({stats.MinRainfallCoordinate.x:F4}°N, {stats.MinRainfallCoordinate.y:F4}°E)");
                    Debug.Log($"  Max: {stats.MaxRainfall:F2} mm/h at ({stats.MaxRainfallCoordinate.x:F4}°N, {stats.MaxRainfallCoordinate.y:F4}°E)");
                    
                    // 打印四个角点数据
                    for (int i = 0; i < testPoints.Length; i++)
                    {
                        int index = testPoints[i];
                        Vector2 coord = coordinates[index];
                        Vector2Int gridPos = new Vector2Int(index % 64, index / 64);
                        
                        Debug.Log($"\n{cornerNames[i]} Corner ({coord.x:F4}°N, {coord.y:F4}°E):");
                        Vector2 wind = data.GetWindVector(gridPos);
                        float windSpeed = wind.magnitude;
                        float windDirection = Mathf.Atan2(wind.y, wind.x) * Mathf.Rad2Deg;
                        if (windDirection < 0) windDirection += 360;
                        
                        Debug.Log($"Wind: {windSpeed:F1} m/s at {windDirection:F1}°");
                        Debug.Log($"Wind Vector: ({wind.x:F1}, {wind.y:F1})");
                        Debug.Log($"Rainfall: {data.GetRainfallValue(gridPos):F1} mm/h");
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
