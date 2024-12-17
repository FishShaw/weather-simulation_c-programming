using UnityEngine;
using Wander;

namespace WeatherSystem.Mapping
{
    public class TerrainTileMapper : MonoBehaviour
    {
        [SerializeField] private WeatherGridMapper weatherGridMapper;
        [SerializeField] private TerrainTile terrainTile;
        [SerializeField] private Material weatherMaterial;

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
            
            // update the material's UV coordinates
            Material instanceMaterial = new Material(weatherMaterial);
            instanceMaterial.SetVector("_WeatherUV_BL", weatherUVs[0]);
            instanceMaterial.SetVector("_WeatherUV_BR", weatherUVs[1]);
            instanceMaterial.SetVector("_WeatherUV_TL", weatherUVs[2]);
            instanceMaterial.SetVector("_WeatherUV_TR", weatherUVs[3]);
            
            // apply the material to the terrain tile
            MeshRenderer renderer = terrainTile.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = instanceMaterial;
            }
        }
    }
} 