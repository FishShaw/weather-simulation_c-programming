using UnityEngine;
using Wander;

namespace WeatherSystem.SingleGrid
{
    public class SingleWeatherGridMapper : MonoBehaviour
    {
        private TerrainBuilder terrainBuilder;
        private bool isInitialized = false;

        public void Initialize()
        {
            terrainBuilder = FindFirstObjectByType<TerrainBuilder>();
            isInitialized = terrainBuilder != null;
        }

        // 世界坐标转地理坐标
        public (double lat, double lon) WorldToGeographic(Vector3 worldPosition)
        {
            if (!isInitialized) return (0, 0);
            
            // 世界坐标转RD坐标
            double rdX = terrainBuilder.originRDX + (double)worldPosition.x;
            double rdY = terrainBuilder.originRDY + (double)worldPosition.z;
            
            // RD坐标转地理坐标
            double lat, lon;
            RDUtils.RD2GPS(rdX, rdY, out lat, out lon);
            return (lat, lon);
        }
    }
} 