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
        [SerializeField] private Color gridColor = new Color(1f, 0f, 0f, 0.3f);

        private Vector2Int localGridStart;
        private float gridSpacing;
        private bool isInitialized = false;
        private Vector3 gridOrigin;

        private void Start()
        {
            InitializeGridSettings();
        }

        private void InitializeGridSettings()
        {
            if (settings == null || terrainBuilder == null) return;

            double halfSize = terrainBuilder.boundsSize / 2;
            double terrainWest = terrainBuilder.originRDX - halfSize;
            double terrainSouth = terrainBuilder.originRDY - halfSize;

            var (swLat, swLon) = gridMapper.RDToGeographic(terrainWest, terrainSouth);
            localGridStart.x = Mathf.FloorToInt((float)((swLon - settings.defaultWest) / WeatherSettings.LON_STEP));
            localGridStart.y = Mathf.FloorToInt((float)((swLat - settings.defaultSouth) / WeatherSettings.LAT_STEP));

            gridSpacing = WeatherSettings.GRID_SIZE_KM * 1000;
            gridOrigin = new Vector3((float)terrainWest, 0, (float)terrainSouth);

            isInitialized = true;
        }

        private void OnDrawGizmos()
        {
            if (!showGrid) return;
            
            // 确保组件已初始化
            if (!isInitialized)
            {
                gridMapper ??= FindFirstObjectByType<SingleWeatherGridMapper>();
                terrainBuilder ??= FindFirstObjectByType<TerrainBuilder>();
                settings ??= FindFirstObjectByType<SingleGridWeatherManager>()?.Settings;
                
                if (gridMapper != null && terrainBuilder != null && settings != null)
                {
                    InitializeGridSettings();
                }
            }
            
            if (isInitialized)
            {
                DrawCurrentCell();
            }
        }

        private void DrawCurrentCell()
        {
            if (Camera.main == null) return;

            Vector3 cameraPos = Camera.main.transform.position;
            Vector2Int gridPos = gridMapper.WorldToGrid(cameraPos);

            float gridWorldX = Mathf.Floor((cameraPos.x - gridOrigin.x) / gridSpacing) * gridSpacing + gridOrigin.x;
            float gridWorldZ = Mathf.Floor((cameraPos.z - gridOrigin.z) / gridSpacing) * gridSpacing + gridOrigin.z;

            Gizmos.color = gridColor;
            Vector3 gridCenter = new Vector3(
                gridWorldX + gridSpacing/2, 
                0,
                gridWorldZ + gridSpacing/2
            );
            Gizmos.DrawCube(gridCenter, new Vector3(gridSpacing, 0.1f, gridSpacing));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!isInitialized)
            {
                gridMapper ??= FindFirstObjectByType<SingleWeatherGridMapper>();
                terrainBuilder ??= FindFirstObjectByType<TerrainBuilder>();
                settings ??= FindFirstObjectByType<SingleGridWeatherManager>()?.Settings;
                
                if (gridMapper != null && terrainBuilder != null && settings != null)
                {
                    InitializeGridSettings();
                }
            }
        }
#endif
    }
}