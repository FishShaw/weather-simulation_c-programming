# Weather System

A real-time weather system based on geographic information, supporting 15-hour weather data visualization and control.

## Project Structure

WeatherSystemBuilt/
├── Scripts/
│   ├── New_onegrid/
│   │   ├── SingleWeatherGridMapper.cs    # Coordinate conversion
│   │   ├── SingleGridWeatherManager.cs    # System management
│   │   ├── SingleWeatherVFXController.cs  # Visual effects
│   │   ├── PythonWeatherPipe.cs          # Python communication
│   │   └── WeatherHourData.cs            # Data structures
│   └── ...

## Core Components

### 1. SingleWeatherGridMapper
- **Purpose**: Handles coordinate system conversions
- **Features**:
  - World → RD coordinate conversion
  - RD → WGS84 (Geographic) conversion

### 2. SingleGridWeatherManager
- **Purpose**: Manages weather system and updates
- **Features**:
  - Position-based weather updates
  - Time-based interpolation
  - VFX control

### 3. PythonWeatherPipe
- **Purpose**: Communicates with Python backend
- **Features**:
  - Real-time weather data requests
  - 15-hour forecast data handling