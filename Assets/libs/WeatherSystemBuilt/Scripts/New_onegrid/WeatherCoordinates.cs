using UnityEngine;
using Wander;
using System.Collections.Generic;

namespace WeatherSystem.SingleGrid
{
    public class WeatherCoordinates : MonoBehaviour
    {
        [SerializeField] private WeatherSettings_new settings;
        private TerrainBuilder terrainBuilder;
        private bool isInitialized = false;
        private float centerRDX;
        private float centerRDY;
        
        public bool IsInitialized => isInitialized;

        public void Initialize(double rdX, double rdY)
        {
            centerRDX = (float)rdX;
            centerRDY = (float)rdY;
        }

        public void Initialize()
        {
            if (terrainBuilder == null)
            {
                // find the terrain generator
                GameObject generator = GameObject.Find("TerrainGenerator_11");
                if (generator != null)
                {
                    terrainBuilder = generator.GetComponent<TerrainBuilder>();
                }
                
                // if not found, try to find any terrain builder
                if (terrainBuilder == null)
                {
                    terrainBuilder = FindFirstObjectByType<TerrainBuilder>();
                }
                
                isInitialized = terrainBuilder != null;
                
                if (!isInitialized)
                {
                    Debug.LogError("No TerrainBuilder found in scene!");
                }
            }

            if (settings == null)
            {
                settings = FindFirstObjectByType<WeatherSettings_new>();
            }
        }

        public List<Vector2> Generate64x64Grid()
        {
            List<Vector2> coordinates = new List<Vector2>();
            
            if (terrainBuilder == null)
            {
                Debug.LogError("TerrainBuilder not found!");
                return coordinates;
            }

            // calculate the RD grid step (meter)
            double rdStepX = terrainBuilder.boundsSize / 63.0;
            double rdStepY = terrainBuilder.boundsSize / 63.0;

            // calculate the start RD coordinate (bottom left corner)
            double startRDX = terrainBuilder.originRDX - (terrainBuilder.boundsSize / 2.0);
            double startRDY = terrainBuilder.originRDY - (terrainBuilder.boundsSize / 2.0);
            
            // generate the grid points in the RD coordinate system, then convert to geographic coordinates
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    // calculate the RD coordinate of the grid center
                    double rdX = startRDX + (x * rdStepX) + (rdStepX / 2.0);
                    double rdY = startRDY + (y * rdStepY) + (rdStepY / 2.0);
                    
                    // directly convert the RD coordinate to latitude and longitude
                    RDUtils.RD2GPS(rdX, rdY, out double lat, out double lon);
                    coordinates.Add(new Vector2((float)lat, (float)lon));
                }
            }

            return coordinates;
        }

        // 
        public (double lat, double lon) WorldToGeographic(Vector3 worldPosition)
        {
            if (!isInitialized) return (0, 0);
            
            // world coordinate to RD coordinate
            double rdX = terrainBuilder.originRDX + worldPosition.x;
            double rdY = terrainBuilder.originRDY + worldPosition.z;
            
            // RD coordinate to geographic coordinate
            RDUtils.RD2GPS(rdX, rdY, out double lat, out double lon);
            return (lat, lon);
        }

        // geographic coordinate to world coordinate
        public Vector3 GeographicToWorld(double lat, double lon)
        {
            // geographic coordinate to RD coordinate
            RDUtils.GPS2RD(lat, lon, out double rdX, out double rdY);
            
            // RD coordinate to world coordinate
            return new Vector3(
                (float)(rdX - terrainBuilder.originRDX),
                0,
                (float)(rdY - terrainBuilder.originRDY)
            );
        }

        // RD coordinate to world coordinate
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