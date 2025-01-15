using UnityEngine;
using WeatherSystem.Core;
using Wander;

namespace WeatherSystem.Mapping
{
    public class WeatherGridMapper : MonoBehaviour
    {
        [SerializeField] private WeatherSettings settings;
        [SerializeField] private TerrainBuilder terrainBuilder;
        private bool isInitialized = false;

        public void Initialize()
        {
            if (!ValidateComponents()) return;
            isInitialized = true;
        }

        private bool ValidateComponents()
        {
            if (settings == null)
            {
                var manager = FindAnyObjectByType<WeatherManager>();
                if (manager != null) settings = manager.Settings;
                else
                {
                    Debug.LogError("Weather Settings not assigned!");
                    return false;
                }
            }

            if (terrainBuilder == null)
            {
                terrainBuilder = FindAnyObjectByType<TerrainBuilder>();
                if (terrainBuilder == null)
                {
                    Debug.LogError("TerrainBuilder not found!");
                    return false;
                }
            }
            return true;
        }

        // 1. World -> RD
        public (double rdX, double rdY) WorldToRD(Vector3 worldPosition)
        {
            if (!isInitialized) return (0, 0);
            return (
                terrainBuilder.originRDX + worldPosition.x,
                terrainBuilder.originRDY + worldPosition.z
            );
        }

        // 2. RD -> WGS84
        public (double lat, double lon) RDToGeographic(double rdX, double rdY)
        {
            RDUtils.RD2GPS(rdX, rdY, out double lat, out double lon);
            return (lat, lon);
        }

        // 3. WGS84 -> Grid
        public Vector2Int GeographicToGrid(double latitude, double longitude)
        {
            if (!isInitialized) return Vector2Int.zero;
            
            double normalizedLon = longitude - settings.defaultWest;
            double normalizedLat = latitude - settings.defaultSouth;
            
            double xDouble = normalizedLon / WeatherSettings.LON_STEP;
            double yDouble = normalizedLat / WeatherSettings.LAT_STEP;
            
            return new Vector2Int(
                Mathf.Clamp(Mathf.FloorToInt((float)xDouble), 0, settings.gridWidth - 1),
                Mathf.Clamp(Mathf.FloorToInt((float)yDouble), 0, settings.gridHeight - 1)
            );
        }

        // 4. Grid -> UV
        public Vector2 GridToUV(Vector2Int gridPos)
        {
            const int GRID_SIZE = 390;
            return new Vector2((float)gridPos.x / GRID_SIZE, (float)gridPos.y / GRID_SIZE);
        }

        // combination: World -> Grid
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            var (rdX, rdY) = WorldToRD(worldPosition);
            var (lat, lon) = RDToGeographic(rdX, rdY);
            return GeographicToGrid(lat, lon);
        }

        public TerrainBuilder GetTerrainBuilder()
        {
            return terrainBuilder;
        }
    }
} 