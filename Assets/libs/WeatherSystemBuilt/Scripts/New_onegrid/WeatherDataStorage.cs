using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherSystem.SingleGrid;
using Wander;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

public class WeatherDataStorage : MonoBehaviour
{
    // 使用相对于Assets文件夹的路径
    private const string WEATHER_DATA_PATH = "libs/WeatherSystemBuilt/Resources/WeatherData_json/weather_data_{0}.json";
    private const int MAX_BACKUPS = 5; // 保留最近5个备份
    
    [System.Serializable]
    private class WeatherDataCache
    {
        public string timestamp;  // 添加时间戳
        public Dictionary<int, WeatherData_new> weatherData;
        public List<Vector2> coordinates;
    }

    // 单例实现
    private static WeatherDataStorage instance;
    public static WeatherDataStorage Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("WeatherDataStorage");
                instance = go.AddComponent<WeatherDataStorage>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private Dictionary<int, WeatherData_new> weatherData;
    private bool isDataLoaded = false;
    private PythonWeatherPipe weatherPipe;
    private WeatherCoordinates gridMapper;
    private List<Vector2> coordinates;  // 添加网格坐标缓存
    private TerrainBuilder terrainBuilder;

    public bool IsDataLoaded => isDataLoaded;
    public Dictionary<int, WeatherData_new> WeatherData => weatherData;
    public List<Vector2> Coordinates => coordinates;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 获取TerrainBuilder引用
        terrainBuilder = FindFirstObjectByType<TerrainBuilder>();
    }

    public async Task LoadWeatherData()
    {
        if (LoadFromCache()) return;

        try
        {
            if (weatherPipe == null)
                weatherPipe = gameObject.AddComponent<PythonWeatherPipe>();
            if (gridMapper == null)
            {
                gridMapper = gameObject.AddComponent<WeatherCoordinates>();
                gridMapper.Initialize();
            }

            coordinates = gridMapper.Generate64x64Grid();
            weatherData = await weatherPipe.RequestWeatherData(Vector3.zero);
            
            if (weatherData != null)
            {
                SaveToCache();
                isDataLoaded = true;
                
                if (weatherPipe != null) Destroy(weatherPipe);
                if (gridMapper != null) Destroy(gridMapper);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading weather data: {e.Message}");
        }
    }

    // 获取天气统计信息
    public WeatherStatistics GetWeatherStatistics(int hour)
    {
        if (!isDataLoaded || weatherData == null || 
            !weatherData.TryGetValue(hour, out WeatherData_new data))
            return null;

        var stats = new WeatherStatistics();
        stats.AnalyzeRainfall(data, coordinates);
        return stats;
    }

    // 获取特定小时的天气数据
    public WeatherData_new GetHourData(int hour)
    {
        if (!isDataLoaded || weatherData == null) return null;
        return weatherData.TryGetValue(hour, out var data) ? data : null;
    }

    // 强制刷新数据
    public async Task RefreshData()
    {
        Debug.Log("Refreshing weather data...");
        // 在加载新数据前备份当前数据
        if (isDataLoaded && weatherData != null)
        {
            BackupCurrentCache();
        }
        
        isDataLoaded = false;
        await LoadWeatherData();
    }

    private void BackupCurrentCache()
    {
        try
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(
                Application.persistentDataPath, 
                string.Format(WEATHER_DATA_PATH, timestamp)
            );
            
            var cache = new WeatherDataCache
            {
                timestamp = timestamp,
                weatherData = weatherData,
                coordinates = coordinates
            };

            string json = JsonConvert.SerializeObject(cache);
            File.WriteAllText(backupPath, json);
            Debug.Log($"Created backup at: {backupPath}");

            // 清理旧备份
            CleanOldBackups();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create backup: {e.Message}");
        }
    }

    private void CleanOldBackups()
    {
        try
        {
            string cacheDir = Path.Combine(Application.persistentDataPath, "WeatherCache");
            if (!Directory.Exists(cacheDir)) return;

            var files = Directory.GetFiles(cacheDir, "weather_data_*.json")
                               .OrderByDescending(f => f)
                               .Skip(MAX_BACKUPS);

            foreach (var file in files)
            {
                File.Delete(file);
                Debug.Log($"Deleted old backup: {file}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to clean old backups: {e.Message}");
        }
    }

    private string GetFullPath(string filename)
    {
        // 获取Assets文件夹的绝对路径
        string assetsPath = Application.dataPath;
        return Path.Combine(assetsPath, WEATHER_DATA_PATH.Replace("{0}", filename));
    }

    private void SaveToCache()
    {
        try
        {
            string fullPath = GetFullPath("latest");
            string directoryPath = Path.GetDirectoryName(fullPath);
            
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var cache = new WeatherDataCache
            {
                timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                coordinates = coordinates,
                weatherData = weatherData
            };

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(cache, settings);
            File.WriteAllText(fullPath, json);
            Debug.Log($"Weather data cached to: {fullPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save cache: {e.Message}\n{e.StackTrace}");
        }
    }

    private bool ValidateWeatherData(WeatherDataCache cache)
    {
        if (cache == null) return false;
        if (cache.coordinates == null || cache.coordinates.Count != 4096) return false;
        if (cache.weatherData == null || cache.weatherData.Count != 16) return false;

        var firstHourData = cache.weatherData[0];
        if (firstHourData == null) return false;

        if (firstHourData.WindU == null || firstHourData.WindU.GetLength(0) != 64 || firstHourData.WindU.GetLength(1) != 64) return false;
        if (firstHourData.WindV == null || firstHourData.WindV.GetLength(0) != 64 || firstHourData.WindV.GetLength(1) != 64) return false;
        if (firstHourData.Rain == null || firstHourData.Rain.GetLength(0) != 64 || firstHourData.Rain.GetLength(1) != 64) return false;

        return true;
    }

    private bool LoadFromCache()
    {
        string fullPath = GetFullPath("latest");
        
        if (!File.Exists(fullPath)) return false;

        try
        {
            string json = File.ReadAllText(fullPath);
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            
            var cache = JsonConvert.DeserializeObject<WeatherDataCache>(json, settings);
            
            if (!ValidateWeatherData(cache)) return false;

            weatherData = cache.weatherData;
            coordinates = cache.coordinates;
            isDataLoaded = true;
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load cache: {e.Message}");
            return false;
        }
    }

    // 清理数据
    public void ClearData()
    {
        weatherData = null;
        isDataLoaded = false;
    }

    private void OnDestroy()
    {
        // 只在对象被销毁时清理组件
        if (weatherPipe != null) Destroy(weatherPipe);
        if (gridMapper != null) Destroy(gridMapper);
    }
}
