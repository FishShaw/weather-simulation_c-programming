# Weather System

A real-time weather system based on geographic information, supporting 15-hour weather data visualization and control.

## Project Structure & Functionality

WeatherSystemBuilt/
├── Scripts/
│   ├── New_one grid ver/           # New implementation
│   │   ├── SingleWeatherGridMapper.cs    # Core coordinate mapping
│   │   ├── SingleGridVisualizer.cs       # Grid visualization
│   │   └── SingleGridWeatherManager.cs    # System management
│   └── ...

## Core Components

### 1. SingleWeatherGridMapper
- **Purpose**: Handles all coordinate system conversions
- **Key Features**:
  - World → RD coordinate conversion
  - RD → WGS84 (Geographic) conversion
  - Geographic → Weather Grid conversion
  - Grid → UV mapping for textures
- **Design Principles**:
  - Single responsibility for coordinate conversion
  - No boundary validation (handled by visualizer)
  - Pure conversion functions

### 2. SingleGridVisualizer
- **Purpose**: Visualizes weather grid overlay on terrain
- **Key Features**:
  - Automatic grid size calculation for 3x3 terrain
  - Dynamic grid cell visualization
  - Real-time coordinate display
  - Boundary validation and handling
- **Grid Calculation**:
  ```
  // Calculate weather grid cell size in RD coordinates
  weatherGridCellSizeRD = (settings.defaultEast - settings.defaultWest) / settings.gridWidth

  // Calculate local grid start position
  localGridStart.x = floor((terrainWest - settings.defaultWest) / weatherGridCellSizeRD)
  localGridStart.y = floor((terrainSouth - settings.defaultSouth) / weatherGridCellSizeRD)

  // Calculate grid size with overlap handling
  localGridSize.x = ceil(terrainSize / weatherGridCellSizeRD) + 1
  localGridSize.y = localGridSize.x  // Keep square grid
  ```

### 3. Coordinate Systems Integration
1. **Terrain System**:
   - Uses TerrainBuilder for 3x3 terrain bounds
   - Handles complete terrain area coverage
   - Ensures grid alignment with terrain tiles

2. **Weather Grid**:
   - 390x390 grid covering Netherlands
   - Each cell represents 2.5km x 2.5km area
   - Aligned with RD coordinate system

3. **Coordinate Conversions**:
   ```
   World Space (Unity) → RD Coordinates → WGS84 → Weather Grid → UV
   ```

## Implementation Details

### 1. Boundary Handling
- **TerrainBuilder**:
  - Provides total bounds size for 3x3 terrain
  - Manages terrain tile organization
  - Supplies RD coordinate reference

- **TileStreamer**:
  - Handles dynamic terrain loading
  - Provides runtime coordinate reference
  - Manages terrain tile streaming

### 2. Grid Visualization
- **Debug Display**:
  - Current grid cell highlight
  - Coordinate system information
  - Grid overlay with configurable height
  - Customizable grid colors

- **Grid Lines**:
  ```
  // Draw vertical lines
  for (int i = 0; i <= localGridSize.x; i++)
  {
      Vector3 start = origin + new Vector3(i * gridSpacing, gridHeight, 0);
      Vector3 end = start + new Vector3(0, 0, mapSize);
      Gizmos.DrawLine(start, end);
  }
  ```

### 3. Performance Considerations
1. **Optimization**:
   - Minimal coordinate conversions
   - Efficient grid calculation
   - Smart boundary checking

2. **Memory Management**:
   - No redundant validations
   - Clear component responsibilities
   - Efficient data structures

## Usage Guidelines

### 1. Setup
```csharp
// Add required components
- SingleWeatherGridMapper
- SingleGridVisualizer
- SingleGridWeatherManager
```

### 2. Configuration
- Set weather grid parameters in WeatherSettings
- Configure visualization options in SingleGridVisualizer
- Adjust grid height and colors as needed

### 3. Runtime Usage
```csharp
// Get weather data for current position
Vector3 worldPos = Camera.main.transform.position;
Vector2Int gridPos = gridMapper.WorldToGrid(worldPos);
var weatherData = dataLoader.GetWeatherData(gridPos);
```

## Future Improvements

1. **Grid System**:
   - Adaptive grid resolution
   - Dynamic cell size based on zoom
   - Optimized boundary calculations

2. **Visualization**:
   - Enhanced debug information
   - Custom grid styling options
   - Performance optimizations

3. **Integration**:
   - Better terrain system coupling
   - Extended weather data support
   - Advanced interpolation methods

## Notes
- Grid calculations ensure complete terrain coverage
- Coordinate conversions maintain precision
- System designed for Netherlands geographic area
- Supports both editor and runtime visualization