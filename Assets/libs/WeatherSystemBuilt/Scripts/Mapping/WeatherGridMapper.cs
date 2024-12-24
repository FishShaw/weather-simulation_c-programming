using UnityEngine;
using WeatherSystem.Core;
using Wander;

namespace WeatherSystem.Mapping
{
    public class WeatherGridMapper : MonoBehaviour
    {
        [SerializeField] private WeatherSettings settings;
        [SerializeField] private TerrainBuilder terrainBuilder;

        private double north, south, east, west;
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

            // using the settings to initialize the geographic bounds
            this.north = settings.defaultNorth;
            this.south = settings.defaultSouth;
            this.east = settings.defaultEast;
            this.west = settings.defaultWest;
            
            isInitialized = true;
        }

        // convert geographic coordinates to weather grid coordinates
        public Vector2Int GeographicToGridCoordinate(double latitude, double longitude)
        {
            if (!isInitialized)
            {
                Debug.LogError("WeatherGridMapper not initialized!");
                return Vector2Int.zero;
            }

            int x = Mathf.FloorToInt((float)((longitude - west) / (east - west) * settings.gridWidth));
            int y = Mathf.FloorToInt((float)((latitude - south) / (north - south) * settings.gridHeight));
            
            return new Vector2Int(
                Mathf.Clamp(x, 0, settings.gridWidth - 1),
                Mathf.Clamp(y, 0, settings.gridHeight - 1)
            );
        }

        // convert weather grid coordinates to UV coordinates
        public Vector2 GridToUVCoordinate(Vector2Int gridPos)
        {
            return new Vector2(
                (float)gridPos.x / settings.gridWidth,
                (float)gridPos.y / settings.gridHeight
            );
        }

        // convert RD coordinates to weather grid coordinates
        public Vector2Int RDToGridCoordinate(double rdX, double rdY)
        {
            // first convert RD coordinates to WGS84
            double lat, lon;
            RDUtils.RD2GPS(rdX, rdY, out lat, out lon);
            
            // then convert to grid coordinates
            return GeographicToGridCoordinate(lat, lon);
        }

        // get the weather data UV mapping for a specified terrain tile
        public Vector2[] GetTileWeatherUVs(TerrainTile tile)
        {
            Vector2[] uvs = new Vector2[4]; // 瓦片的四个角
            
            // get the RD coordinates of the tile
            double tileSize = RDUtils.CalcTileSizeRD(terrainBuilder.zoom);
            double tileX = tile.originRDX;
            double tileY = tile.originRDY;

            // calculate the UV coordinates of the four corners
            Vector2Int gridPos;
            
            // bottom left corner
            gridPos = RDToGridCoordinate(tileX, tileY);
            uvs[0] = GridToUVCoordinate(gridPos);
            
            // bottom right corner
            gridPos = RDToGridCoordinate(tileX + tileSize, tileY);
            uvs[1] = GridToUVCoordinate(gridPos);
            
            // top left corner
            gridPos = RDToGridCoordinate(tileX, tileY + tileSize);
            uvs[2] = GridToUVCoordinate(gridPos);
            
            // top right corner
            gridPos = RDToGridCoordinate(tileX + tileSize, tileY + tileSize);
            uvs[3] = GridToUVCoordinate(gridPos);

            return uvs;
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            // Unity世界坐标系中，x对应RD的x，z对应RD的y
            double rdX = worldPosition.x;
            double rdY = worldPosition.z;
            
            // 使用现有的RD到网格坐标的转换方法
            return RDToGridCoordinate(rdX, rdY);
        }
    }
} 