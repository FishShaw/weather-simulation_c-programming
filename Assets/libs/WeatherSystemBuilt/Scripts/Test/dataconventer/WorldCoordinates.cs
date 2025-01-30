using UnityEngine;
using Wander;
using WeatherSystem.SingleGrid;
using System.IO;
using Newtonsoft.Json;

public class WorldCoordinates : MonoBehaviour
{
    private TerrainBuilder terrainBuilder;
    private WeatherCoordinates weatherCoordinates;
    private const string SAVE_DIR = "Assets/libs/WeatherSystemBuilt/Resources/WeatherData_json_textures/weather_data_20250127_125930";

    [System.Serializable]
    private class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3Data(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
    }

    [System.Serializable]
    private class TerrainCornerData
    {
        public Vector3Data leftBottom;
        public Vector3Data rightBottom;
        public Vector3Data leftTop;
        public Vector3Data rightTop;
        public double originRDX;
        public double originRDY;
        public double boundsSize;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 查找所有TerrainBuilder实例
        TerrainBuilder[] builders = FindObjectsByType<TerrainBuilder>(FindObjectsSortMode.None);
        
        foreach (var builder in builders)
        {
            Debug.Log($"Found TerrainBuilder on: {builder.gameObject.name}");
            if (builder.gameObject.name.Contains("TerrainGenerator_11"))
            {
                terrainBuilder = builder;
                Debug.Log($"Selected TerrainBuilder from: {builder.gameObject.name}");
                break;
            }
        }

        if (terrainBuilder == null)
        {
            Debug.LogError("No TerrainGenerator_11 instance found in scene!");
            return;
        }

        // 尝试获取或创建WeatherCoordinates
        weatherCoordinates = FindFirstObjectByType<WeatherCoordinates>();
        if (weatherCoordinates == null)
        {
            GameObject weatherObj = new GameObject("WeatherCoordinates");
            weatherCoordinates = weatherObj.AddComponent<WeatherCoordinates>();
            Debug.Log("Created new WeatherCoordinates object");
        }

        // 确保WeatherCoordinates已初始化
        weatherCoordinates.Initialize();

        // 检查组件状态并生成数据
        if (terrainBuilder != null && weatherCoordinates != null && weatherCoordinates.IsInitialized)
        {
            Debug.Log("=== Terrain Data Generation Started ===");
            Debug.Log($"TerrainBuilder found on: {terrainBuilder.gameObject.name}");
            Debug.Log($"WeatherCoordinates status: {weatherCoordinates.IsInitialized}");
            Debug.Log($"Origin RD: ({terrainBuilder.originRDX}, {terrainBuilder.originRDY})");
            Debug.Log($"Bounds Size: {terrainBuilder.boundsSize}");
            
            SaveTerrainCornersToJson();
        }
        else
        {
            if (terrainBuilder == null) Debug.LogError("TerrainBuilder component not found in any TerrainGenerator_11 instance");
            if (weatherCoordinates == null) Debug.LogError("Failed to create WeatherCoordinates");
            if (weatherCoordinates != null && !weatherCoordinates.IsInitialized) Debug.LogError("WeatherCoordinates failed to initialize");
        }
    }

    public void SaveTerrainCornersToJson()
    {
        Vector3[] corners = GetTerrainCorners();
        if (corners == null) return;

        var cornerData = new TerrainCornerData
        {
            leftBottom = new Vector3Data(corners[0]),
            rightBottom = new Vector3Data(corners[1]),
            leftTop = new Vector3Data(corners[2]),
            rightTop = new Vector3Data(corners[3]),
            originRDX = terrainBuilder.originRDX,
            originRDY = terrainBuilder.originRDY,
            boundsSize = terrainBuilder.boundsSize
        };

        try
        {
            if (!Directory.Exists(SAVE_DIR))
            {
                Directory.CreateDirectory(SAVE_DIR);
            }

            string filePath = Path.Combine(SAVE_DIR, "terrain_corners.json");
            string json = JsonConvert.SerializeObject(cornerData, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });

            File.WriteAllText(filePath, json);
            
            // 打印详细的日志信息
            Debug.Log("=== Terrain Corners Data ===");
            Debug.Log($"Left Bottom: {corners[0]}");
            Debug.Log($"Right Bottom: {corners[1]}");
            Debug.Log($"Left Top: {corners[2]}");
            Debug.Log($"Right Top: {corners[3]}");
            Debug.Log($"Saved to: {filePath}");
            Debug.Log("=== End of Terrain Data ===");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving terrain corners: {e.Message}");
        }
    }

    public Vector3[] GetTerrainCorners()
    {
        if (terrainBuilder == null || weatherCoordinates == null)
        {
            Debug.LogError("Required components not found!");
            return null;
        }

        Vector3[] corners = new Vector3[4];
        float halfSize = (float)terrainBuilder.boundsSize / 2.0f;

        // 计算四个角点的RD坐标
        double[] cornerRDX = new double[] 
        {
            terrainBuilder.originRDX - halfSize,  // 左下
            terrainBuilder.originRDX + halfSize,  // 右下
            terrainBuilder.originRDX - halfSize,  // 左上
            terrainBuilder.originRDX + halfSize   // 右上
        };

        double[] cornerRDY = new double[] 
        {
            terrainBuilder.originRDY - halfSize,  // 左下
            terrainBuilder.originRDY - halfSize,  // 右下
            terrainBuilder.originRDY + halfSize,  // 左上
            terrainBuilder.originRDY + halfSize   // 右上
        };

        // 使用WeatherCoordinates中的RDToWorld方法转换为世界坐标
        for (int i = 0; i < 4; i++)
        {
            corners[i] = weatherCoordinates.RDToWorld(cornerRDX[i], cornerRDY[i]);
        }

        Debug.Log("Terrain Corners:");
        Debug.Log($"Left Bottom: {corners[0]}");
        Debug.Log($"Right Bottom: {corners[1]}");
        Debug.Log($"Left Top: {corners[2]}");
        Debug.Log($"Right Top: {corners[3]}");

        return corners;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
