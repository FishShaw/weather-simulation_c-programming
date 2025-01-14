using UnityEngine;
using Wander;
using WeatherSystem.Core;

namespace WeatherSystem.SingleGrid
{
    public class SingleGridVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SingleWeatherGridMapper gridMapper;
        [SerializeField] private TerrainBuilder terrainBuilder;
        [SerializeField] private WeatherSettings settings;

        [Header("Grid Settings")]
        [SerializeField] private bool showGrid = true;
        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.2f);
        [SerializeField] private float gridHeight = 1f;

        private Vector2Int localGridStart;  // 当前地形在整个天气网格中的起始位置
        private Vector2Int localGridSize;   // 当前地形覆盖的网格数量
        private float gridSpacing;          // 每个网格的实际大小（米）
        private bool isInitialized = false;

        private void Start()
        {
            InitializeGridSettings();
        }

        private void InitializeGridSettings()
        {
            if (settings == null)
            {
                var weatherManager = FindFirstObjectByType<SingleGridWeatherManager>();
                if (weatherManager != null)
                {
                    settings = weatherManager.Settings;
                }
            }

            if (terrainBuilder != null && settings != null)
            {
                // 获取3x3地形的完整范围
                double terrainWest = terrainBuilder.originRDX;    // 3x3地形的最西边
                double terrainSouth = terrainBuilder.originRDY;   // 3x3地形的最南边
                double terrainSize = terrainBuilder.boundsSize;   // 3x3地形的总大小

                // 计算天气网格的实际大小（RD坐标系中每个网格的大小）
                double weatherGridCellSizeRD = (settings.defaultEast - settings.defaultWest) / settings.gridWidth;

                // 计算当前3x3地形区域在整个天气网格中的起始位置
                localGridStart.x = Mathf.FloorToInt((float)((terrainWest - settings.defaultWest) / weatherGridCellSizeRD));
                localGridStart.y = Mathf.FloorToInt((float)((terrainSouth - settings.defaultSouth) / weatherGridCellSizeRD));

                // 计算覆盖整个3x3地形的网格数量（向上取整并加1以确保完全覆盖）
                localGridSize.x = Mathf.CeilToInt((float)(terrainSize / weatherGridCellSizeRD)) + 1;
                localGridSize.y = localGridSize.x; // 保持正方形

                // 使用天气数据的原始网格大小作为网格间距
                gridSpacing = (float)weatherGridCellSizeRD;

                Debug.Log($"Terrain bounds: {terrainSize}m x {terrainSize}m");
                Debug.Log($"Weather grid cell size: {weatherGridCellSizeRD}m");
                Debug.Log($"Local grid: start={localGridStart}, size={localGridSize}, spacing={gridSpacing}m");

                isInitialized = true;
            }
            else
            {
                Debug.LogWarning("Failed to initialize grid settings: Missing required components");
            }
        }

        private void OnDrawGizmos()
        {
            if (!showGrid || terrainBuilder == null || !isInitialized) return;

            Gizmos.color = gridColor;

            // 使用terrainBuilder的实际边界
            float mapSize = (float)terrainBuilder.boundsSize;
            Vector3 origin = new Vector3(
                (float)terrainBuilder.originRDX, 
                gridHeight, 
                (float)terrainBuilder.originRDY
            );

            // 绘制网格线
            for (int i = 0; i <= localGridSize.x; i++)
            {
                float x = i * gridSpacing;
                if (x > mapSize) break;
                
                Vector3 start = origin + new Vector3(x, 0, 0);
                Vector3 end = start + new Vector3(0, 0, mapSize);
                Gizmos.DrawLine(start, end);
            }

            for (int i = 0; i <= localGridSize.y; i++)
            {
                float z = i * gridSpacing;
                if (z > mapSize) break;
                
                Vector3 start = origin + new Vector3(0, 0, z);
                Vector3 end = start + new Vector3(mapSize, 0, 0);
                Gizmos.DrawLine(start, end);
            }

            // 绘制当前相机位置对应的网格单元
            if (Camera.main != null)
            {
                Vector3 cameraPos = Camera.main.transform.position;
                Vector2Int globalGridPos = gridMapper.WorldToGrid(cameraPos);
                Vector2Int localGridPos = globalGridPos - localGridStart; // 转换为本地网格坐标
                
                // 计算网格世界坐标
                float gridWorldX = Mathf.Floor((cameraPos.x - origin.x) / gridSpacing) * gridSpacing + origin.x;
                float gridWorldZ = Mathf.Floor((cameraPos.z - origin.z) / gridSpacing) * gridSpacing + origin.z;

                // 高亮显示当前网格
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Vector3 gridCenter = new Vector3(
                    gridWorldX + gridSpacing/2, 
                    gridHeight, 
                    gridWorldZ + gridSpacing/2
                );
                Gizmos.DrawCube(gridCenter, new Vector3(gridSpacing, 0.1f, gridSpacing));

                // 显示坐标信息
                var (rdX, rdY) = gridMapper.WorldToRD(cameraPos);
                var (lat, lon) = gridMapper.RDToGeographic(rdX, rdY);
                
                UnityEditor.Handles.Label(gridCenter + Vector3.up * 2, 
                    $"Global Grid: ({globalGridPos.x}, {globalGridPos.y})\n" +
                    $"Local Grid: ({localGridPos.x}, {localGridPos.y})\n" +
                    $"RD: ({rdX:F0}, {rdY:F0})\n" +
                    $"WGS84: ({lat:F6}, {lon:F6})");
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gridMapper == null)
                gridMapper = FindFirstObjectByType<SingleWeatherGridMapper>();
            
            if (terrainBuilder == null)
                terrainBuilder = FindFirstObjectByType<TerrainBuilder>();

            if (settings == null)
            {
                var weatherManager = FindFirstObjectByType<SingleGridWeatherManager>();
                if (weatherManager != null)
                {
                    settings = weatherManager.Settings;
                }
            }

            if (Application.isPlaying && !isInitialized)
            {
                InitializeGridSettings();
            }
        }
#endif
    }
} 