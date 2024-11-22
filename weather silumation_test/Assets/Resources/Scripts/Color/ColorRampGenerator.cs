using UnityEngine;

public class ColorRampGenerator
{
    public static Texture2D CreateRainfallColorRamp(int width = 256)
    {
        Texture2D colorRamp = new Texture2D(width, 1, TextureFormat.RGBA32, false);
        colorRamp.wrapMode = TextureWrapMode.Clamp;

        Color[] colors = new Color[width];
        for (int i = 0; i < width; i++)
        {
            float t = i / (float)(width - 1);
            
            if (t < 0.1f) // 毫毛雨
            {
                colors[i] = Color.Lerp(
                    new Color(1, 1, 1, 0), 
                    new Color(0.95f, 0.95f, 0.95f, 0.2f), 
                    t * 10
                );
            }
            else if (t < 0.3f) // 小雨
            {
                colors[i] = Color.Lerp(
                    new Color(0.95f, 0.95f, 0.95f, 0.2f),
                    new Color(0.85f, 0.85f, 0.85f, 0.4f), 
                    (t - 0.1f) * 5
                );
            }
            else if (t < 0.5f) // 中雨
            {
                colors[i] = Color.Lerp(
                    new Color(0.85f, 0.85f, 0.85f, 0.4f),
                    new Color(0.7f, 0.7f, 0.7f, 0.6f), 
                    (t - 0.3f) * 5
                );
            }
            else if (t < 0.7f) // 大雨
            {
                colors[i] = Color.Lerp(
                    new Color(0.7f, 0.7f, 0.7f, 0.6f),
                    new Color(0.5f, 0.5f, 0.5f, 0.8f),
                    (t - 0.5f) * 5
                );
            }
            else // 暴雨/特大暴雨
            {
                colors[i] = Color.Lerp(
                    new Color(0.5f, 0.5f, 0.5f, 0.8f),
                    new Color(0.3f, 0.3f, 0.3f, 1.0f), 
                    (t - 0.7f) * (10/3f)
                );
            }
        }

        colorRamp.SetPixels(colors);
        colorRamp.Apply();
        return colorRamp;
    }
}
