using UnityEngine;
using Wander;

namespace WeatherSystem.SingleGrid
{
    public class WeatherCoordinates : MonoBehaviour
    {
        private TerrainBuilder terrainBuilder;
        private bool isInitialized = false;
        
        // 添加公共属性
        public bool IsInitialized => isInitialized;

        public void Initialize()
        {
            if (terrainBuilder == null)
            {
                // 首先尝试查找TerrainGenerator_11
                GameObject generator = GameObject.Find("TerrainGenerator_11");
                if (generator != null)
                {
                    terrainBuilder = generator.GetComponent<TerrainBuilder>();
                }
                
                // 如果找不到特定名称的对象，尝试查找任何TerrainBuilder
                if (terrainBuilder == null)
                {
                    terrainBuilder = FindFirstObjectByType<TerrainBuilder>();
                }
                
                isInitialized = terrainBuilder != null;
                
                if (isInitialized)
                {
                    Debug.Log($"Found TerrainBuilder with RD origin: ({terrainBuilder.originRDX}, {terrainBuilder.originRDY})");
                }
                else
                {
                    Debug.LogError("No TerrainBuilder found in scene!");
                }
            }
        }

        // 世界坐标转地理坐标
        public (double lat, double lon) WorldToGeographic(Vector3 worldPosition)
        {
            if (!isInitialized) return (0, 0);
            
            // 世界坐标转RD坐标
            double rdX = terrainBuilder.originRDX + worldPosition.x;
            double rdY = terrainBuilder.originRDY + worldPosition.z;
            
            // RD坐标转地理坐标
            RDUtils.RD2GPS(rdX, rdY, out double lat, out double lon);
            return (lat, lon);
        }

        // 地理坐标转世界坐标
        public Vector3 GeographicToWorld(double lat, double lon)
        {
            // 地理坐标转RD坐标
            RDUtils.GPS2RD(lat, lon, out double rdX, out double rdY);
            
            // RD坐标转世界坐标
            return new Vector3(
                (float)(rdX - terrainBuilder.originRDX),
                0,
                (float)(rdY - terrainBuilder.originRDY)
            );
        }

        // RD坐标转世界坐标
        public Vector3 RDToWorld(double rdX, double rdY)
        {
            return new Vector3(
                (float)(rdX - terrainBuilder.originRDX),
                0,
                (float)(rdY - terrainBuilder.originRDY)
            );
        }
    }
} 