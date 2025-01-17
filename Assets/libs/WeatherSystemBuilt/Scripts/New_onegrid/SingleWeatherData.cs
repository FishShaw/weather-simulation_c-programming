using UnityEngine;

namespace WeatherSystem.SingleGrid
{
    public struct SingleWeatherData
    {
        public float WindU;
        public float WindV;
        public float WindSpeed;
        public float Rain;

        public Vector2 GetWindVector()
        {
            return new Vector2(WindU, WindV);
        }
    }
}