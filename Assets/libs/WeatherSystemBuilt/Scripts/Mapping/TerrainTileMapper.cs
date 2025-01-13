using UnityEngine;
using Wander;
using WeatherSystem.Core;
using WeatherSystem.DataHandling;

namespace WeatherSystem.Mapping
{
    public class TerrainTileMapper : MonoBehaviour
    {
        [SerializeField] private WeatherGridMapper weatherGridMapper;
        [SerializeField] private TerrainTile terrainTile;
        private Material weatherMaterialInstance;

        private void Awake()
        {
            if (terrainTile == null) terrainTile = GetComponent<TerrainTile>();
            if (weatherGridMapper == null) weatherGridMapper = FindAnyObjectByType<WeatherGridMapper>();
        }

        public void UpdateWeatherMaterial(Material baseMaterial, WeatherData weatherData)
        {
            if (!ValidateComponents(baseMaterial)) return;

            Vector2[] uvs = CalculateTileUVs();
            if (uvs == null) return;

            UpdateMaterialInstance(baseMaterial, uvs);
            ApplyWeatherTextures(weatherData);
        }

        private Vector2[] CalculateTileUVs()
        {
            if (terrainTile == null || weatherGridMapper == null) return null;

            var terrainBuilder = weatherGridMapper.GetTerrainBuilder();
            if (terrainBuilder == null) return null;

            double tileSize = RDUtils.CalcTileSizeRD(terrainBuilder.zoom);
            var uvs = new Vector2[4];
            var corners = new (double x, double y)[]
            {
                (terrainTile.originRDX, terrainTile.originRDY),                           // BL
                (terrainTile.originRDX + tileSize, terrainTile.originRDY),               // BR
                (terrainTile.originRDX, terrainTile.originRDY + tileSize),               // TL
                (terrainTile.originRDX + tileSize, terrainTile.originRDY + tileSize)     // TR
            };

            for (int i = 0; i < 4; i++)
            {
                var (lat, lon) = weatherGridMapper.RDToGeographic(corners[i].x, corners[i].y);
                var gridPos = weatherGridMapper.GeographicToGrid(lat, lon);
                uvs[i] = weatherGridMapper.GridToUV(gridPos);
            }

            return uvs;
        }

        private void UpdateMaterialInstance(Material baseMaterial, Vector2[] uvs)
        {
            if (weatherMaterialInstance == null)
                weatherMaterialInstance = new Material(baseMaterial);

            weatherMaterialInstance.SetVector("_WeatherUV_BL", uvs[0]);
            weatherMaterialInstance.SetVector("_WeatherUV_BR", uvs[1]);
            weatherMaterialInstance.SetVector("_WeatherUV_TL", uvs[2]);
            weatherMaterialInstance.SetVector("_WeatherUV_TR", uvs[3]);

            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.material = weatherMaterialInstance;
        }

        private void ApplyWeatherTextures(WeatherData data)
        {
            if (weatherMaterialInstance == null) return;
            weatherMaterialInstance.SetTexture("_RainfallTex", data.rainfallTexture);
            weatherMaterialInstance.SetTexture("_WindUTex", data.windUTexture);
            weatherMaterialInstance.SetTexture("_WindVTex", data.windVTexture);
        }

        private bool ValidateComponents(Material baseMaterial)
        {
            return terrainTile != null && weatherGridMapper != null && baseMaterial != null;
        }
    }
} 