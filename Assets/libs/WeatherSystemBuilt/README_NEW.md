# Weather System

A real-time weather system based on geographic information, supporting 15-hour weather data visualization and control.

## Project Structure & Functionality

WeatherSystemBuilt/
├── Scripts/
│   ├── Core/                 
│   │   ├── WeatherManager.cs        # System initialization and coordination
│   │   │                            # - Manages component lifecycle
│   │   │                            # - Coordinates data flow between modules
│   │   │                            # - Handles system initialization
│   │   │
│   │   └── WeatherSettings.cs       # Global configuration
│   │                                # - System parameters
│   │                                # - Performance settings
│   │
│   ├── DataHandling/         
│   │   ├── WeatherData.cs           # Weather data structures
│   │   │                            # - Holds weather textures
│   │   │                            # - Provides data conversion methods
│   │   │
│   │   └── WeatherDataLoader.cs     # Data loading and interpolation
│   │                                # - Loads PNG weather data
│   │                                # - Performs 5-minute interpolation
│   │                                # - Manages data caching
│   │
│   ├── Mapping/             
│   │   ├── WeatherGridMapper.cs     # Coordinate system conversion
│   │   │                            # - RD to WGS84 conversion
│   │   │                            # - Grid coordinate mapping
│   │   │                            # - UV coordinate calculation
│   │   │
│   │   └── TerrainTileMapper.cs     # Terrain integration
│   │                                # - Maps weather data to terrain
│   │                                # - Handles material instances
│   │                                # - Controls weather visualization
│   │
│   ├── TimeManagement/      
│   │   ├── TimeController.cs        # Time sequence control
│   │   │                            # - Manages 15-hour playback
│   │   │                            # - Controls data updates
│   │   │
│   │   └── TimelineUI.cs            # User interface
│   │                                # - Timeline visualization
│   │                                # - Playback controls
│   │
│   └── VFX/                 
│       ├── RainController.cs        # Rain visualization
│       │                            # - Particle system control
│       │                            # - Rain intensity mapping
│       │
│       └── WindController.cs        # Wind visualization
│                                    # - Wind vector visualization
│                                    # - Speed and direction control

## System Workflow

1. Data Flow:
   ```mermaid
   graph TD
      A[WeatherDataLoader] -->|Load PNG| B[WeatherData]
      B -->|Update| C[TerrainTileMapper]
      B -->|Control| D[VFX Controllers]
      E[TimeController] -->|Trigger| A
      E -->|Sync| D
   ```

2. Component Dependencies:
   - WeatherManager
     → Initializes all components
     → Coordinates data flow
   
   - WeatherDataLoader
     → Depends on: WeatherData
     → Used by: TimeController, VFX Controllers
   
   - TerrainTileMapper
     → Depends on: WeatherGridMapper, WeatherData
     → Used by: WeatherManager
   
   - VFX Controllers
     → Depend on: WeatherData, TimeController
     → Used by: WeatherManager

3. Update Cycle:
   a. TimeController triggers data updates
   b. WeatherDataLoader interpolates data
   c. TerrainTileMapper updates terrain visualization
   d. VFX Controllers update particle systems

## Performance Considerations

1. Data Management:
   - Cached weather data
   - 5-minute interpolation intervals
   - Optimized texture handling

2. Visualization:
   - Material instancing
   - Efficient UV mapping
   - VFX optimization

3. Memory Usage:
   - Texture compression
   - Data caching strategies
   - Resource cleanup