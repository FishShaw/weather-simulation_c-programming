# Weather System

A real-time weather system based on geographic information, supporting 15-hour weather data visualization and control.

## Project Structure

WeatherSystemBuilt/
├── Scripts/
│   ├── Core/                 # Core system components
│   │   ├── WeatherManager.cs
│   │   └── WeatherSettings.cs
│   ├── DataHandling/         # Data processing related
│   │   ├── WeatherData.cs
│   │   └── WeatherDataLoader.cs
│   ├── Mapping/             # Geographic mapping related
│   │   ├── WeatherGridMapper.cs
│   │   └── TerrainTileMapper.cs
│   ├── TimeManagement/      # Time sequence management
│   │   ├── TimeController.cs
│   │   └── TimelineUI.cs
│   └── VFX/                 # Visual effects related
│       ├── RainController.cs
│       └── WindController.cs
├── Shaders/                 # Custom shader files
├── Materials/               # Material files
├── Prefabs/                 # Prefab assets
└── Resources/               # Resource files

## Development Plan

### 1. Geographic Mapping System
- TerrainTile system integration
- 390×390 weather grid mapping
- UV coordinate system alignment

### 2. Time Sequence Control
- 15-hour data playback system
- Data interpolation for smooth transitions
- Timeline UI control system

### 3. Particle System
- rain.vfx integration
- Wind data-driven particle movement
- Performance optimization

### 4. Data Management
- PNG data preloading and caching
- Real-time data updates
- Grid interpolation processing