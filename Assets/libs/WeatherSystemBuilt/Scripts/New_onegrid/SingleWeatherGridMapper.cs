using UnityEngine;
using Wander;
using WeatherSystem.Core;

namespace WeatherSystem.SingleGrid
{
    // 单网格天气系统的坐标映射器
    public class SingleWeatherGridMapper : MonoBehaviour
    {
        [SerializeField] private WeatherSettings settings;
        private TerrainBuilder terrainBuilder;
        private bool isInitialized = false;

        // 初始化组件
        public void Initialize()
        {
            terrainBuilder = FindFirstObjectByType<TerrainBuilder>();
            if (settings == null)
            {
                settings = FindFirstObjectByType<SingleGridWeatherManager>()?.Settings;
            }
            isInitialized = terrainBuilder != null && settings != null;
        }

        // 世界坐标转RD坐标
        public (double rdX, double rdY) WorldToRD(Vector3 worldPosition)
        {
            if (!isInitialized) return (0, 0);
            return (
                terrainBuilder.originRDX + (double)worldPosition.x,
                terrainBuilder.originRDY + (double)worldPosition.z
            );
        }

        // RD坐标转地理坐标
        public (double lat, double lon) RDToGeographic(double rdX, double rdY)
        {
            double lat, lon;
            RDUtils.RD2GPS(rdX, rdY, out lat, out lon);
            return (lat, lon);
        }

        // 世界坐标转网格坐标
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            var (rdX, rdY) = WorldToRD(worldPosition);
            var (lat, lon) = RDToGeographic(rdX, rdY);
            
            double normalizedLon = lon - settings.defaultWest;
            double normalizedLat = lat - settings.defaultSouth;
            
            int gridX = Mathf.Clamp(
                Mathf.FloorToInt((float)(normalizedLon / settings.lonStep)), 
                0, 
                settings.gridWidth - 1
            );
            
            int gridY = Mathf.Clamp(
                Mathf.FloorToInt((float)(normalizedLat / settings.latStep)), 
                0, 
                settings.gridHeight - 1
            );
            
            return new Vector2Int(gridX, gridY);
        }

        // 检查位置是否在有效范围内
        public bool IsPositionInBounds(Vector3 worldPosition)
        {
            var (rdX, rdY) = WorldToRD(worldPosition);
            var (lat, lon) = RDToGeographic(rdX, rdY);
            
            return lon >= settings.defaultWest && 
                   lon <= settings.defaultEast && 
                   lat >= settings.defaultSouth && 
                   lat <= settings.defaultNorth;
        }
    }
} 