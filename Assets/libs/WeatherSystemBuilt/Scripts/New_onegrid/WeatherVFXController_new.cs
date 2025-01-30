using UnityEngine;
using UnityEngine.VFX;

namespace WeatherSystem.SingleGrid
{
    public class WeatherVFXController_new : MonoBehaviour
    {
        public VisualEffect vfx;
        public Texture2D[] weatherMaps;
        float timer = 0f;
        int current = 0;
        public float interval = 2f; // 2s each frame

        void Start()
        {
            if (vfx && weatherMaps.Length > 1)
            {
                vfx.SetTexture("_WeatherA", weatherMaps[0]);
                vfx.SetTexture("_WeatherB", weatherMaps[1]);
            }
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                timer = 0f;
                current++;
                if (current >= weatherMaps.Length) current = 0;
                int next = (current + 1) % weatherMaps.Length;
                vfx.SetTexture("_WeatherA", weatherMaps[current]);
                vfx.SetTexture("_WeatherB", weatherMaps[next]);
            }
        }
    }
} 