# Weather System

A real-time weather system based on geographic information, supporting 16-hour weather data visualization and control.

## Project Structure

WeatherSystemBuilt/
├── Scripts/
│   ├── New_onegrid/
│   │   ├── WeatherManager_new.cs         # System management
│   │   ├── WeatherCoordinates.cs         # Coordinate conversion
│   │   ├── WeatherVFXController_new.cs   # Visual effects
│   │   ├── PythonWeatherPipe.cs          # Python communication
│   │   ├── WeatherData_new.cs            # Data structures
│   │   └── SimpleTimeController.cs        # Time control
│   └── ...

## Core Components

### 1. WeatherCoordinates
- **Purpose**: Handles coordinate system conversions
- **Features**:
  - World → RD coordinate conversion
  - RD → WGS84 (Geographic) conversion
  - Integration with TerrainBuilder

### 2. WeatherManager_new
- **Purpose**: Manages weather system and updates
- **Features**:
  - Terrain-based weather updates
  - Time-based interpolation
  - VFX control
  - Weather texture generation

### 3. PythonWeatherPipe
- **Purpose**: Communicates with Python backend
- **Features**:
  - Batch coordinate data sending (64×64 grid)
  - 16-hour weather data handling
  - Data format: Wind (U,V,Speed,Direction) and Rain

### 4. Weather Textures
- **Format**:
  - Wind Texture (RGBA): U,V,Speed,Direction
  - Rain Texture (R): Rainfall intensity
- **Resolution**: 64×64
- **Storage**: In-memory, 16 sets (one per hour)

## Data Flow

1. TerrainBuilder generates 3x3 terrain
2. System calculates 64x64 grid coordinates
3. Coordinates sent to Python backend
4. Python returns 16-hour weather data
5. System generates weather textures
6. VFX system updates based on time interpolation

## Time System
- Fixed period: 2024/7/9 12:00 - 2024/7/10 3:00
- Features:
  - Pause/Resume
  - Fast forward
  - Time interpolation in VFX

## Implementation Notes

### Weather Data
- Data range: 16 hours
- Update trigger: New terrain generation
- Memory usage: ~320KB (16 hours × 20KB per hour)

### VFX System
- Rain effect based on rainfall intensity
- Wind effect using direction and speed
- Smooth transitions between weather states

## Future Improvements
- Terrain height adaptation
- Cross-day weather handling
- Advanced interpolation methods
- Performance optimization