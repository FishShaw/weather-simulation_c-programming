using UnityEngine;
using Wander;

namespace WeatherSystem.SingleGrid
{
    public class SingleWeatherGridMapper : MonoBehaviour
    {
        [SerializeField] private WeatherSettings settings;
        private TerrainBuilder terrainBuilder;

        public void Initialize()
        {
            terrainBuilder = FindFirstObjectByType<TerrainBuilder>();
            if (settings == null)
            {
                settings = FindFirstObjectByType<SingleGridWeatherManager>()?.Settings;
            }
        }

        // 直接从世界坐标获取网格位置
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            // 1. 世界坐标转RD
            var (rdX, rdY) = RDUtils.ToRD(worldPosition.x, worldPosition.z);
            
            // 2. RD转WGS84
            RDUtils.RD2GPS(rdX, rdY, out double lat, out double lon);
            
            // 3. WGS84转网格坐标
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
            var (rdX, rdY) = RDUtils.ToRD(worldPosition.x, worldPosition.z);
            RDUtils.RD2GPS(rdX, rdY, out double lat, out double lon);
            
            return lon >= settings.defaultWest && 
                   lon <= settings.defaultEast && 
                   lat >= settings.defaultSouth && 
                   lat <= settings.defaultNorth;
        }
    }
} 