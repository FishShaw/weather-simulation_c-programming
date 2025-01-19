using UnityEngine;
using WeatherSystem.Core;
using WeatherSystem.SingleGrid;

namespace WeatherSystem.SingleGrid
{
    public class WeatherGridVisualiser : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WeatherCoordinates gridMapper;
        [SerializeField] private WeatherSettings_new settings;

        [Header("Grid Settings")]
        [SerializeField] private bool showGrid = true;
        [SerializeField] private Color gridColor = Color.red;
        [SerializeField] private float gridHeight = 100f;

        private bool isInitialized = false;
        private Vector2Int currentGridPosition;

        public void InitializeComponents()
        {
            if (gridMapper == null)
            {
                gridMapper = FindFirstObjectByType<WeatherCoordinates>();
            }
            
            if (settings == null)
            {
                settings = FindFirstObjectByType<WeatherSettings_new>();
            }

            isInitialized = gridMapper != null && settings != null;
        }

        public void UpdateGridPosition(Vector2Int newPosition)
        {
            currentGridPosition = newPosition;
        }

        private void OnDrawGizmos()
        {
            if (!showGrid || !isInitialized || gridMapper == null || !gridMapper.IsInitialized || settings == null)
            {
                return;
            }

            // 绘制3x3网格
            for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                DrawGridCell(new Vector2Int(
                    currentGridPosition.x + x,
                    currentGridPosition.y + y
                ));
            }
        }

        private void DrawGridCell(Vector2Int gridPos)
        {
            try
            {
                double gridLonStart = settings.defaultWest + gridPos.x * WeatherSettings_new.LON_STEP;
                double gridLatStart = settings.defaultSouth + gridPos.y * WeatherSettings_new.LAT_STEP;
                double gridLonEnd = gridLonStart + WeatherSettings_new.LON_STEP;
                double gridLatEnd = gridLatStart + WeatherSettings_new.LAT_STEP;

                Vector3 gridMin = gridMapper.GeographicToWorld(gridLatStart, gridLonStart);
                Vector3 gridMax = gridMapper.GeographicToWorld(gridLatEnd, gridLonEnd);

                Gizmos.color = gridColor;
                Vector3 center = (gridMin + gridMax) * 0.5f;
                Vector3 size = gridMax - gridMin;
                
                Gizmos.DrawWireCube(center + Vector3.up * (gridHeight * 0.5f), 
                                   new Vector3(size.x, gridHeight, size.z));
            }
            catch (System.Exception) { }
        }

        public WeatherSettings_new Settings => settings;

#if UNITY_EDITOR
        private void OnValidate()
        {
            InitializeComponents();
        }
#endif
    }
}
