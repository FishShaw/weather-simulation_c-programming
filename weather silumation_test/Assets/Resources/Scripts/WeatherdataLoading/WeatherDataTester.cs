using UnityEngine;
using WeatherSystem;

public class WeatherDataTester : MonoBehaviour
{
    public WeatherDataLoader dataLoader;
    
    void Start()
    {
        // 只测试降雨数据
        var rainfallData = dataLoader.GetRainfallData(0);
        if (rainfallData != null)
        {
            Debug.Log($"Successfully loaded rainfall data");
        }

        // 注释掉风力数据测试
        /*
        var (windU, windV) = dataLoader.GetWindData(0);
        if (windU != null && windV != null)
        {
            Debug.Log($"Successfully loaded wind data");
        }
        */
    }
}
