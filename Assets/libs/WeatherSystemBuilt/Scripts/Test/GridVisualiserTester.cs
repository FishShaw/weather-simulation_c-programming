using UnityEngine;
using WeatherSystem.SingleGrid;

public class GridVisualiserTester : MonoBehaviour
{
    [SerializeField] private WeatherGridVisualiser gridVisualiser;
    [SerializeField] private WeatherCoordinates gridMapper;
    [SerializeField] private Transform testCube;    // 替换相机为测试用 Cube
    [SerializeField] private KeyCode updateKey = KeyCode.Space;
    [SerializeField] private KeyCode moveKey = KeyCode.M;  // 新增移动按键
    [SerializeField] private float moveDistance = 100f;    // 移动距离（米）

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (gridMapper == null)
        {
            gridMapper = FindFirstObjectByType<WeatherCoordinates>();
            Debug.Log($"[GridTester] Found gridMapper: {gridMapper != null}");
        }
            
        if (gridVisualiser == null)
        {
            gridVisualiser = FindFirstObjectByType<WeatherGridVisualiser>();
            Debug.Log($"[GridTester] Found gridVisualiser: {gridVisualiser != null}");
        }
            
        if (testCube == null)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "TestCube";
            cube.transform.position = Vector3.zero;
            testCube = cube.transform;
            Debug.Log("[GridTester] Created test cube at origin");
        }
            
        // 初始化组件
        gridMapper?.Initialize();
        Debug.Log($"[GridTester] GridMapper initialized: {gridMapper?.IsInitialized}");
        
        // 确保 WeatherGridVisualiser 已经初始化
        if (gridVisualiser != null)
        {
            gridVisualiser.InitializeComponents();
            Debug.Log("[GridTester] GridVisualiser components initialized");
            
            // 立即进行一次网格可视化测试
            TestGridVisualization();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(updateKey))
        {
            TestGridVisualization();
        }

        // 添加测试用的移动控制
        if (Input.GetKeyDown(moveKey))
        {
            MoveCube();
        }
    }

    private void TestGridVisualization()
    {
        if (gridVisualiser == null || testCube == null || gridMapper == null) return;

        // 获取 Cube 位置
        Vector3 cubePosition = testCube.position;
        
        // 转换为地理坐标
        var (lat, lon) = gridMapper.WorldToGeographic(cubePosition);
        
        // 计算网格位置
        int gridX = Mathf.FloorToInt((float)((lon - gridVisualiser.Settings.defaultWest) / WeatherSettings_new.LON_STEP));
        int gridY = Mathf.FloorToInt((float)((lat - gridVisualiser.Settings.defaultSouth) / WeatherSettings_new.LAT_STEP));
        
        // 更新网格位置
        gridVisualiser.UpdateGridPosition(new Vector2Int(gridX, gridY));
        
        // 输出调试信息
        Debug.Log($"[GridTester] Test Results:\n" +
                 $"Cube Position: {cubePosition}\n" +
                 $"Geographic: Lat={lat:F4}, Lon={lon:F4}\n" +
                 $"Grid Position: X={gridX}, Y={gridY}");
    }

    private void MoveCube()
    {
        if (testCube == null) return;
        
        // 在x-z平面上随机移动
        float x = Random.Range(-moveDistance, moveDistance);
        float z = Random.Range(-moveDistance, moveDistance);
        testCube.position += new Vector3(x, 0, z);
        
        // 立即更新网格显示
        TestGridVisualization();
    }
}
