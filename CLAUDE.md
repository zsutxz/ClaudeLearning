# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

### 🎯 Project Overview
This is a **Unity 2022.3.5f1** project implementing a professional coin animation system with physics-based magnetic collection, optimized for 60fps performance with 100+ concurrent coins. The system is designed as a Unity asset package with enterprise-level architecture, comprehensive testing, and adaptive performance scaling.

### 🏗️ Architecture Overview

#### Multi-Assembly Structure
The project uses 4 main assemblies with clean separation of concerns:

1. **CoinAnimation.Core** - Core interfaces, managers, and configuration
   - `ICoinAnimationManager` - Central system interface
   - `CoinAnimationState` - State machine definitions (Idle, Moving, Collecting, Paused, Pooled)
   - `URPConfigurationManager` - Render pipeline management and quality tiers

2. **CoinAnimation.Animation** - DOTween integration and animation utilities
   - `CoinAnimationController` - Individual coin controller with physics-based movement
   - `CoinAnimationEasing` - Custom easing functions for natural coin movement
   - `CoinAnimationUtilities` - Reusable animation patterns (spin, bounce, flip, wobble)
   - `DOTweenManager` - Singleton for DOTween framework management

3. **CoinAnimation.Physics** - Magnetic collection and physics simulation
   - `MagneticCollectionController` - Multi-field magnetic system with spatial optimization
   - `IMagneticCollectionController` - Physics system interface
   - `SpiralMotionController` - Complex 3D spiral path generation for collection

4. **CoinAnimation.Tests** - Comprehensive test coverage using Unity Test Framework
   - `PerformanceValidationScenarios` - 60fps validation with 100+ concurrent coins
   - `UnityEnvironmentValidatorTest` - Environment and package verification
   - `URPConfigurationTest` - Render pipeline testing

#### Key Architectural Patterns
- **Singleton Pattern**: DOTweenManager, CoinAnimationManager for centralized coordination
- **State Machine**: CoinAnimationState enum for coin lifecycle management
- **Event-Driven Architecture**: Extensive use of events for state transitions and system coordination
- **Object Pooling**: Designed for 100+ concurrent coins with memory-efficient lifecycle management
- **Performance Monitoring**: Real-time metrics and automatic quality adjustment

### ⚙️ Development Commands

#### Testing Framework
```bash
# Run all tests in Unity Test Runner
# Unity Menu: Window > General > Test Runner
# Run all: Ctrl+Shift+T (Windows) / Cmd+Shift+T (Mac)
# Run selected: Ctrl+T (Windows) / Cmd+T (Mac)

# Performance validation tests (100+ concurrent coins)
Unity Test Runner > PerformanceValidationScenarios

# Environment validation tests
Unity Test Runner > UnityEnvironmentValidatorTest

# URP configuration tests
Unity Test Runner > URPConfigurationTest
```

#### Build Configuration
```bash
# Unity Menu: File > Build Settings
# Target Platforms: Windows, macOS, Linux
# Scripting Backend: IL2CPP for performance optimization
# API Compatibility Level: .NET Standard 2.1

# Quality Settings (accessed via URPConfigurationManager)
# Low Quality: 20 coins max, basic rendering
# Medium Quality: 50 coins max, standard effects
# High Quality: 100+ coins, full effects and URP features
```

#### Package Management
Key dependencies from `manifest.json`:
- **Unity URP 14.0.8** - Modern rendering pipeline
- **DOTween** - Animation framework (via OpenUPM registry)
- **Unity Test Framework 1.3.9** - Testing infrastructure
- **TextMeshPro 3.0.6** - UI text rendering

DOTween installation via OpenUPM:
```bash
# Unity Package Manager > Add package from git URL
# https://github.com/Demigiant/dotween.git
# Or configure scoped registry in manifest.json (already configured)
```

### 🔧 Core Systems Integration

#### DOTween Integration
- Conditional compilation with `DOTWEEN_INSTALLED` directive
- Fallback movement coroutines when DOTween unavailable
- Custom easing functions in `CoinAnimationEasing.cs`
- Use `DOTweenManager.Instance` for centralized tween management

#### URP Configuration
- Automatic quality adjustment based on performance metrics
- Multi-tier quality settings (Low/Medium/High)
- Use `URPConfigurationManager.Instance` for render pipeline management
- Validation system ensures proper URP installation

#### Physics and Animation
- Magnetic field system with spatial optimization for performance
- Spiral motion patterns for satisfying coin collection
- Event-driven state transitions through `CoinAnimationEventArgs`
- Real-time physics calculations at 60Hz (configurable)

### 📊 Performance Specifications
- **Target Frame Rate**: 60fps sustained performance
- **Concurrent Coins**: 100+ with adaptive scaling (100→50→20 based on performance)
- **Memory Budget**: <50MB for 100 coins during 1-hour stress tests
- **Physics Update Rate**: 60Hz configurable via `MagneticCollectionController`
- **Quality Tiers**: Automatic performance adjustment

### 🎯 Development Workflow

#### Story-Driven Development
The project follows structured story-based development:
- **Story 1.2**: Unity Environment Setup and Configuration (Completed)
- **Story 1.3**: Object Pooling and Memory Management (In Progress)

#### Code Standards
- Namespace organization: `CoinAnimation.Core`, `CoinAnimation.Animation`, `CoinAnimation.Physics`, `CoinAnimation.Tests`
- Assembly definitions enforce clean dependencies
- Event-driven architecture for loose coupling
- Comprehensive XML documentation for all public APIs

#### Testing Strategy
- Performance validation with 100+ concurrent coins
- Environment validation for Unity version and package compatibility
- URP configuration testing for render pipeline functionality
- Memory leak prevention and stability testing

### 🚀 Build and Deployment
- **Unity Version**: 2022.3.5f1c1 LTS
- **Target Platforms**: Windows, macOS, Linux
- **Rendering Pipeline**: Universal Render Pipeline (URP) 14.0.8
- **Scripting Backend**: IL2CPP for production builds
- **API Compatibility**: .NET Standard 2.1

### 📁 Key Configuration Files
- `Project/Packages/manifest.json` - Package dependencies and OpenUPM registry
- `Project/Assets/Scripts/Settings/` - URP configuration and quality settings
- `Project/ProjectSettings/` - Unity project configuration and build settings
- Assembly definition files (.asmdef) - Define dependencies between modules