# 🌦️ Weather Digital Twin - Phase 2: Interactive 3D Visualization System

## 📋 Project Structure

This repository contains the **second phase** of my master's thesis project: developing the **interactive 3D visualization and immersive user experience** for a comprehensive weather digital twin system. This phase focuses on:

- 🎮 **Real-time 3D Weather Visualization** (Unity/C# interactive environments)
- 🌪️ **Advanced Particle Effects System** (Wind flow & precipitation rendering)
- 🗺️ **Geospatial Digital Twin Integration** (Coordinate system mapping & terrain streaming)
- 🎯 **Interactive Decision Support Interface** (Climate adaptation research tools)

**➡️ The first phase (Data Processing Engine, Interpolation & Statistical Analysis) can be found here:** [Python Data Processing Pipeline](https://github.com/FishShaw/weather-simulation_unity_AnywhereXRweather-simulation_unity_AnywhereXR_DataAcquirement_Preprocessing)

---

## Interactive Weather Digital Twin for Climate Research | Master's Thesis Project | Wageningen University

Building immersive 3D experiences that transform the advanced data infrastructure from Phase 1 into intuitive, interactive climate visualization tools for research and public understanding.

![Unity Scene Overview](Docs/Images/unity_scene_1.png)

## 🎯 Projct Overview (Phase 2)

**Phase 2** transforms the sophisticated data infrastructure built in Phase 1 into an **immersive 3D interactive experience**. While Phase 1 focused on building the "brain" (data processing, interpolation, validation), Phase 2 creates the "body" - the visual, tangible interface that makes complex climate data accessible to everyone.

Imagine being able to **walk through tomorrow's weather** - feeling the wind, seeing the rain, and understanding how climate patterns affect real landscapes. This phase creates the interactive "digital twin" interface of the Netherlands' weather system, consuming the high-quality data streams from Phase 1 and turning them into engaging 3D experiences.

### 🔗 Integration with Phase 1
- **Data Source**: Consumes validated, interpolated weather data from Phase 1's Python processing engine
- **Communication**: Uses the custom IPC protocol developed in Phase 1 for real-time data streaming  
- **Quality Assurance**: Builds on Phase 1's validation framework to ensure visualization accuracy

## 🚀 What is it doing?

### 🌍 **Seamless Data Integration**
- **Real-time Consumption**: Direct integration with Phase 1's validated KNMI weather predictions
- **Geographic Precision**: Accurate Dutch terrain rendering with coordinate system mapping
- **Live Synchronization**: Real-time updates through custom IPC communication layer

### 🎮 **Immersive 3D Experience**
- **Interactive Navigation**: Walk through weather patterns as if you're really there
- **Dynamic Particle Systems**: See wind as flowing particle effects around buildings and trees
- **Realistic Precipitation**: Experience rainfall with accurate 3D precipitation effects
- **Multi-perspective Views**: Switch between 2D analytical and 3D immersive modes

### ⚡ **Performance-Optimized Rendering**
- **Efficient 3D Engine**: Smooth real-time visualization consuming Phase 1's processed data streams
- **Smart Resource Management**: Optimized terrain streaming and weather effect rendering
- **Responsive Interface**: Sub-second response times for interactive climate exploration

## 🎬 See It In Action

### 🎥 Live Demos

**3D Interactive Experience:**
![3D Weather Demo](Docs/Demo/3D_demo.gif)
*Navigate through 3D terrain while experiencing real-time weather effects*

**2D Data Visualization:**
![2D Weather Demo](Docs/Demo/2D_demo.gif)
*Understand weather patterns through clear 2D visualizations*

### 🎞️ Full Video Demo
[![Watch Full Demo](https://img.youtube.com/vi/n3oZr7uwDeM/0.jpg)](https://youtu.be/n3oZr7uwDeM?si=efUXHCIVCleZBH7V)
*Click to watch the complete demonstration on YouTube*

## 🛠️ Technical Highlights

Building on the robust data foundation from Phase 1, this phase addresses the visualization and interaction challenges:

### 🎮 **3D Visualization Engine**
- **Unity/C# Implementation**: Real-time 3D rendering consuming Phase 1's data streams
- **Geospatial Integration**: Advanced coordinate mapping (World → RD → WGS84 → Weather Grid)
- **Interactive Controls**: Immersive navigation and exploration interfaces

### 🌪️ **Advanced Weather Effects System**
- **Particle Systems**: Real-time wind flow and precipitation visualization
- **Performance Optimization**: Efficient rendering of complex weather patterns
- **Visual Fidelity**: Realistic representation of meteorological phenomena

### 🗺️ **Digital Twin Architecture**
- **Terrain Streaming**: Dynamic 3x3 terrain loading with coordinate precision
- **Real-time Synchronization**: Live integration with Phase 1's data processing pipeline
- **Decision Support Tools**: Interactive interfaces for climate research applications

![Technical Methods Overview](Docs/Images/methods.png)
![Weather VFX Flow Diagram](Docs/Images/weather-vfx-flow-diagram.png)

## 🏗️ Complete System Architecture (Phase 1 + Phase 2)

```text
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   PHASE 1       │    │   INTEGRATION   │    │   PHASE 2       │
│   Data Engine   │    │   Layer         │    │   3D Interface  │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ • KNMI GRIB     │    │ • Custom IPC    │    │ • Unity/C#      │
│ • Interpolation │───▶│ • Named Pipes   │───▶│ • 3D Rendering  │
│ • Validation    │    │ • Data Streams  │    │ • Particle FX   │
│ • Statistics    │    │ • Quality Ctrl  │    │ • User Interface│
└─────────────────┘    └─────────────────┘    └─────────────────┘
     ↑                         ↑                         ↑
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ Weather Data    │    │ Real-time       │    │ Interactive     │
│ Processing      │    │ Communication   │    │ Visualization   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🏛️ Academic Context

### 📋 **Research Poster**
![Academic Poster](Docs/Poster/poster_Xiaoyu%20Yang.pdf)
*[View full poster](Docs/Poster/poster_Xiaoyu%20Yang.pdf) - Presented at Wageningen University*

### 📝 **Research Survey**
Want to contribute to climate research? [Take our survey](https://tinyurl.com/weathersurveymgi) about weather visualization preferences.

## 🌐 Real-World Applications
### 🏢 **For Researchers**
- **Interactive Climate Scenarios**: Immersively explore different weather patterns using Phase 1's validated data
- **3D Data Analysis**: Understand spatial relationships through interactive 3D visualization
- **Stakeholder Communication**: Demonstrate research findings through engaging visual experiences

### 🏫 **For Education**
- **Immersive Learning**: Students can "walk through" weather systems to understand complex concepts
- **Visual Meteorology**: Transform abstract data from Phase 1 into tangible 3D learning experiences
- **Interactive Exploration**: Hands-on understanding of climate science principles

### 🏛️ **For Policy Makers**
- **Scenario Visualization**: Experience potential climate impacts through immersive 3D environments
- **Evidence-Based Planning**: Make informed infrastructure decisions using interactive climate models
- **Public Engagement**: Communicate climate risks through accessible, visual demonstrations

## 🔧 System Views

**Unity Development Environment:**
![Unity Scene 2](Docs/Images/unity_scene_2.png)

**2D Analytical View:**
![2D Still View](Docs/Images/2D_view_still.png)

## 🎓 About the Developer

This Phase 2 project represents the visualization and interaction component of my Master's studies in Geo-Information Science at Wageningen University. Combined with Phase 1's data processing foundation, it demonstrates a complete end-to-end solution for innovative geospatial technology applications in climate research and public understanding.

---

## 📚 Technical Documentation

<details>
<summary>Click to expand technical details</summary>

### Core System Architecture

The system consists of three main components working together:

#### 1. Weather Data Pipeline (Python)
- **Data Source**: KNMI Harmonia prediction datasets (24h forecasts)
- **Processing**: xarray and GDAL for geospatial data manipulation
- **Output**: Standardized grid format for Unity consumption

#### 2. Geospatial Mapping System (Unity/C#)
- **SingleWeatherGridMapper**: Handles coordinate system conversions
  - World → RD coordinate conversion
  - RD → WGS84 (Geographic) conversion  
  - Geographic → Weather Grid conversion
- **Coverage**: 390x390 grid covering Netherlands (2.5km x 2.5km per cell)

#### 3. 3D Visualization Engine (Unity/C#)
- **Real-time particle systems** for wind and precipitation effects
- **Dynamic terrain loading** with 3x3 tile streaming
- **Interactive camera controls** for immersive exploration

### Key Technical Achievements

#### Coordinate System Integration
```
World Space (Unity) → RD Coordinates → WGS84 → Weather Grid → UV Mapping
```

#### Performance Optimization
- Efficient grid calculation algorithms
- Smart boundary checking and validation
- Minimal coordinate conversions for real-time performance

#### Data Accuracy
- Maintains precision across coordinate transformations
- Real-time synchronization with meteorological data
- Automated interpolation for smooth visual transitions

### System Requirements
- Unity 2022.3 LTS or later
- HDRP (High Definition Render Pipeline)
- Python 3.8+ with xarray, GDAL packages
- Windows 10/11 (tested platform)

</details>

---
