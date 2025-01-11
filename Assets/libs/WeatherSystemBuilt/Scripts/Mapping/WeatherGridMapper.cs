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
            if (settings == null)
            {
                var manager = FindAnyObjectByType<WeatherManager>();
                if (manager != null)
                {
                    settings = manager.Settings;
                }
                else
                {
                    Debug.LogError("Weather Settings not assigned!");
                    return;
                }
            }

            if (terrainBuilder == null)
            {
                terrainBuilder = FindAnyObjectByType<TerrainBuilder>();
                if (terrainBuilder == null)
                {
                    Debug.LogError("TerrainBuilder not found!");
                    return;
                }
            }
            
            isInitialized = true;
        }

        // Convert geographic coordinates (WGS84) to weather grid coordinates
        public Vector2Int GeographicToGridCoordinate(double latitude, double longitude)
        {
            if (!isInitialized)
            {
                Debug.LogError("WeatherGridMapper not initialized!");
                return Vector2Int.zero;
            }

            // Calculate normalized position within bounds
            double normalizedLon = longitude - settings.defaultWest;
            double normalizedLat = latitude - settings.defaultSouth;
            
            // Calculate grid indices
            double xDouble = normalizedLon / settings.lonStep;
            double yDouble = normalizedLat / settings.latStep;
            
            // Convert to integers and clamp to valid range
            int x = Mathf.Clamp(Mathf.FloorToInt((float)xDouble), 0, settings.gridWidth - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt((float)yDouble), 0, settings.gridHeight - 1);
            
            return new Vector2Int(x, y);
        }

        // Convert weather grid coordinates to UV coordinates for texture sampling
        public Vector2 GridToUVCoordinate(Vector2Int gridPos)
        {
            const int GRID_SIZE = 390;
            return new Vector2((float)gridPos.x / GRID_SIZE, (float)gridPos.y / GRID_SIZE);
        }

        // Convert RD coordinates to weather grid coordinates
        public Vector2Int RDToGridCoordinate(double rdX, double rdY)
        {
            // Convert RD to WGS84
            double lat, lon;
            RDUtils.RD2GPS(rdX, rdY, out lat, out lon);
            
            // Convert WGS84 to grid coordinates
            return GeographicToGridCoordinate(lat, lon);
        }

        // Get UV coordinates for all corners of a terrain tile
        public Vector2[] GetTileWeatherUVs(TerrainTile tile)
        {
            Vector2[] uvs = new Vector2[4];
            
            // Get tile properties
            double tileSize = RDUtils.CalcTileSizeRD(terrainBuilder.zoom);
            double tileX = tile.originRDX;
            double tileY = tile.originRDY;

            // Calculate UV coordinates for each corner
            Vector2Int gridPos;
            
            // Bottom left
            gridPos = RDToGridCoordinate(tileX, tileY);
            uvs[0] = GridToUVCoordinate(gridPos);
            
            // Bottom right
            gridPos = RDToGridCoordinate(tileX + tileSize, tileY);
            uvs[1] = GridToUVCoordinate(gridPos);
            
            // Top left
            gridPos = RDToGridCoordinate(tileX, tileY + tileSize);
            uvs[2] = GridToUVCoordinate(gridPos);
            
            // Top right
            gridPos = RDToGridCoordinate(tileX + tileSize, tileY + tileSize);
            uvs[3] = GridToUVCoordinate(gridPos);

            return uvs;
        }

        // Convert Unity world position to weather grid coordinates
        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            if (!isInitialized || terrainBuilder == null)
            {
                Debug.LogError("WeatherGridMapper not initialized or TerrainBuilder not found!");
                return Vector2Int.zero;
            }
            
            // Convert world position to RD coordinates
            double rdX = terrainBuilder.originRDX + worldPosition.x;
            double rdY = terrainBuilder.originRDY + worldPosition.z;
            
            // Convert RD coordinates to weather grid coordinates
            return RDToGridCoordinate(rdX, rdY);
        }
    }
} 