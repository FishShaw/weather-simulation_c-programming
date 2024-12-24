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
                weatherGridMapper = FindAnyObjectByType<WeatherGridMapper>();
            }
        }

        // 只返回UV坐标映射数据
        public Vector2[] GetWeatherUVs()
        {
            if (terrainTile == null || weatherGridMapper == null) return null;
            return weatherGridMapper.GetTileWeatherUVs(terrainTile);
        }

        // 获取地形tile的位置信息
        public Vector3 GetTilePosition()
        {
            return terrainTile != null ? terrainTile.transform.position : Vector3.zero;
        }
    }
} 