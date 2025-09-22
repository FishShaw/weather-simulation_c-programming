# 🌦️ Interactive Weather Digital Twin for Climate Research

**A Master's Thesis Project | Wageningen University | Geo-Information Science**

Transform complex weather data into immersive 3D experiences that anyone can understand and explore.

![Unity Scene Overview](Docs/Images/unity_scene_1.png)

## 🎯 What is This Project?

Imagine being able to **walk through tomorrow's weather** - feeling the wind, seeing the rain, and understanding how climate patterns affect real landscapes. This project creates a "digital twin" of the Netherlands' weather system, turning scientific data into an interactive 3D world.

### 🔬 The Science Behind It
This is part of my Master's thesis in Geo-Information Science at Wageningen University. The project bridges the gap between complex meteorological data and intuitive understanding, helping researchers and the public better grasp climate change impacts.

![Weather VFX Flow Diagram](Docs/Images/weather-vfx-flow-diagram.png)

## 🚀 What Makes This Special?

### 🌍 **Real Data, Real Places**
- Uses actual **KNMI weather predictions** (24-hour forecasts)
- Covers real Dutch terrain with accurate geographical coordinates
- Updates in real-time with live weather information

### 🎮 **Interactive 3D Experience**
- **Walk through weather patterns** as if you're really there
- **See wind** as particle effects flowing around buildings and trees
- **Experience rainfall** with realistic 3D precipitation effects
- **Switch between 2D and 3D views** for different perspectives

### 📊 **Smart Data Processing**
- Automatically converts complex meteorological datasets
- Handles massive amounts of spatial-temporal data efficiently
- Provides smooth, real-time visualization performance

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

This project involved cutting-edge work in multiple domains:

### 🎯 **Digital Twin Development**
- Led the design and development of a geospatial digital twin system in Unity/C#
- Created interactive decision support tools for climate change adaptation studies

### 📈 **Data Pipeline Innovation**
- Built an automated data pipeline prototype in Python (xarray, GDAL)
- Processes, interpolates, and streams large-scale KNMI meteorological time-series data
- Ensures simulation's real-time accuracy with efficient data handling

### 🎨 **Visual Translation**
- Successfully translated complex spatiotemporal data into intuitive 3D visual language
- Allows researchers to immersively assess the impact of various rainfall scenarios
- Makes complex climate data accessible to non-experts

![Technical Methods Overview](Docs/Images/methods.png)

## 🏛️ Academic Context

### 📋 **Research Poster**
![Academic Poster](Docs/Poster/poster_Xiaoyu%20Yang.pdf)
*[View full poster](Docs/Poster/poster_Xiaoyu%20Yang.pdf) - Presented at Wageningen University*

### 📝 **Research Survey**
Want to contribute to climate research? [Take our survey](https://tinyurl.com/weathersurveymgi) about weather visualization preferences.

## 🌐 Real-World Applications

### 🏢 **For Researchers**
- Visualize climate scenarios before they happen
- Test different adaptation strategies interactively
- Communicate findings to stakeholders effectively

### 🏫 **For Education**
- Make weather science tangible and engaging
- Help students understand complex meteorological concepts
- Bridge the gap between theory and reality

### 🏛️ **For Policy Makers**
- Experience the impact of different climate scenarios
- Make informed decisions about infrastructure and planning
- Communicate climate risks to the public effectively

## 🔧 System Views

**Unity Development Environment:**
![Unity Scene 2](Docs/Images/unity_scene_2.png)

**2D Analytical View:**
![2D Still View](Docs/Images/2D_view_still.png)

## 🎓 About the Developer

This project represents the culmination of my Master's studies in Geo-Information Science at Wageningen University, focusing on innovative applications of geospatial technology for climate research and public understanding.

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
