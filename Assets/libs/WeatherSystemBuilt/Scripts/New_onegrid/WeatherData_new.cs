using UnityEngine;
using System;

namespace WeatherSystem.SingleGrid
{
    [Serializable]
    public class WeatherData_new
    {
        public float WindU { get; set; }      // 风速U分量
        public float WindV { get; set; }      // 风速V分量
        public float WindSpeed { get; set; }  // 风速大小
        public float WindDirection { get; set; } // 风向角度（弧度）
        public float Rain { get; set; }       // 降雨量

        public Vector2 GetWindVector()
        {
            return new Vector2(WindU, WindV);
        }

        public Vector2 GetWindDirectionVector()
        {
            return new Vector2(
                Mathf.Cos(WindDirection) * WindSpeed,
                Mathf.Sin(WindDirection) * WindSpeed
            );
        }
    }
}