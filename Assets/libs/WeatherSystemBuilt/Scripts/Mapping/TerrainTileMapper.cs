using UnityEngine;
using Wander;

namespace WeatherSystem.Mapping
{
    public class TerrainTileMapper : MonoBehaviour
    {
        [SerializeField] private WeatherGridMapper weatherGridMapper;
        [SerializeField] private TerrainTile terrainTile;

        private void Awake()
        {
            if (terrainTile == null)
            {
                terrainTile = GetComponent<TerrainTile>();
            }

            if (weatherGridMapper == null)
            {
                weatherGridMapper = FindObjectOfType<WeatherGridMapper>();
            }
        }

        public void UpdateWeatherMapping()
        {
            if (terrainTile == null || weatherGridMapper == null) return;

            // get the weather data UV mapping for the terrain tile
            Vector2[] weatherUVs = weatherGridMapper.GetTileWeatherUVs(terrainTile);
            
            // TODO: use these UV coordinates to map weather data to the terrain tile
        }
    }
} 