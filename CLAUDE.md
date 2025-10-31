# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

### 🎯 Project Overview
This is a **Unity 2022.3.5f1** project implementing an ultra-simplified coin animation system using pure Unity coroutines. The system achieves smooth animations with zero external dependencies, optimized for high performance with 50+ concurrent coins through lightweight coroutine-based animations.

### 🏗️ Architecture Overview

#### Minimal Assembly Structure
The project uses a clean, simplified structure with only essential components:

1. **CoinAnimation.Core** - Core interfaces and state definitions
   - `CoinAnimationState` - State machine definitions (Idle, Moving, Collecting, Paused, Pooled)
   - `ICoinAnimationManager` - Basic system interface
   - `PerformanceMetrics` - Performance monitoring data structure

2. **CoinAnimation.Animation** - Pure Unity coroutine animations
   - `CoinAnimationController` - Individual coin controller with coroutine-based movement
   - `CoinAnimationManager` - Simple singleton manager for coin coordination
   - Built-in easing functions for natural movement

3. **Examples** - Demonstration and usage examples
   - `SimpleCoinDemo` - Basic demonstration script
   - `README.md` - Usage documentation

4. **Tests** - Comprehensive test coverage
   - `CoinAnimationTestSuite` - Core functionality tests
   - `PerformanceValidationScenarios` - Performance tests
   - `UnityEnvironmentValidatorTest` - Environment verification

#### Key Architectural Patterns
- **Coroutine-Based Animation**: All animations use Unity coroutines with custom easing functions
- **State Machine**: CoinAnimationState enum for clear lifecycle management
- **Event-Driven Architecture**: Clean event system for state transitions
- **Singleton Pattern**: CoinAnimationManager for centralized coordination
- **Zero Dependencies**: No external plugins or packages required

### ⚙️ Development Commands

#### Environment Validation
```bash
# Unity Menu: Coin Animation > Validate Environment
# Automated validation of Unity version, URP setup, project structure, and dependencies
# Essential for ensuring proper zero-dependency configuration
```

#### Testing Framework
```bash
# Unity Test Runner (Window > General > Test Runner)
# Run all: Ctrl+Shift+T (Windows) / Cmd+Shift+T (Mac)
# Run selected: Ctrl+T (Windows) / Cmd+T (Mac)

# Performance validation tests (30+ concurrent coins)
Unity Test Runner > PerformanceValidationScenarios

# Environment validation tests
Unity Test Runner > UnityEnvironmentValidatorTest

# Core functionality tests
Unity Test Runner > CoinAnimationTestSuite
```

#### Build Configuration
```bash
# Unity Menu: File > Build Settings
# Target Platforms: Windows, macOS, Linux, iOS, Android
# Scripting Backend: IL2CPP for production builds
# API Compatibility Level: .NET Standard 2.1

# Performance Tiers
# Low Quality: 20 coins max, basic rendering
# Medium Quality: 50 coins max, standard effects
# High Quality: 50 coins max, full effects
```

### 🔧 Core Systems Integration

#### Coroutine Animation System
- **Pure Unity Implementation**: Uses only built-in Unity coroutines
- **Custom Easing Functions**: Built-in EaseOutQuad, EaseOutBack, EaseInSine, EaseInBack
- **Smooth Movement**: Vector3.Lerp with mathematical easing
- **Multi-Phase Collection**: Scale up → Move → Scale down animation sequence
- **Rotation Animation**: Continuous rotation during movement

#### Animation Controller
- `AnimateToPosition(targetPosition, duration)` - Move with easing and rotation
- `CollectCoin(collectionPoint, duration)` - Multi-phase collection animation
- `StopCurrentAnimation()` - Stop any running animation
- Built-in state management and event triggering

#### Performance Features
- **Coroutine-Driven**: Lightweight animations with minimal overhead
- **Memory Efficient**: No external library dependencies
- **Adaptive Scaling**: Automatic performance adjustment based on coin count
- **60fps Target**: Optimized for smooth performance

### 📊 Performance Specifications
- **Target Frame Rate**: 60fps sustained performance
- **Concurrent Coins**: 50+ with coroutine-based animation
- **Memory Budget**: <20MB for 50 coins during stress tests
- **Animation Update Rate**: Frame-rate independent using Time.deltaTime
- **Quality Tiers**: Manual adjustment based on performance needs

### 🎯 Development Workflow

#### System Creation
The project follows a simplified development approach:
- **Core System**: Pure Unity coroutine animations
- **Demo Implementation**: Simple demonstration of animation capabilities
- **Testing Strategy**: Comprehensive test coverage with performance validation

#### Code Standards
- Namespace organization: `CoinAnimation.Core`, `CoinAnimation.Animation`, `CoinAnimation.Examples`
- Event-driven architecture for loose coupling
- Comprehensive documentation for all public APIs
- Zero external dependencies for maximum compatibility

#### Testing Strategy
- Performance validation with 30+ concurrent coins
- Environment validation for Unity version compatibility
- Animation flow testing for complete state transitions
- Memory usage validation during stress tests

### 🚀 Build and Deployment
- **Unity Version**: 2022.3.5f1 LTS
- **Target Platforms**: Windows, macOS, Linux, iOS, Android
- **Scripting Backend**: IL2CPP for production builds
- **API Compatibility**: .NET Standard 2.1
- **Zero External Dependencies**: Pure Unity implementation

### 📁 Key Configuration Files
- `Project/Packages/manifest.json` - Core Unity packages only (zero external dependencies)
- `Project/ProjectSettings/` - Unity project configuration
- `docs/stories/` - Development stories with logical progression (Story 1.1 → 1.2 → 1.3)
- `docs/tech-spec-epic-mvp.md` - Complete technical specifications and API reference

### 💡 Usage Guidelines

#### Creating Coin Animations
```csharp
// Get the coin controller
var coinController = coinObject.GetComponent<CoinAnimationController>();

// Animate movement with easing
coinController.AnimateToPosition(targetPosition, 1.0f);

// Collect with multi-phase animation
coinController.CollectCoin(collectionPoint, 1.5f);

// Stop current animation
coinController.StopCurrentAnimation();
```

#### Animation States
1. **Idle** - Coin is not animating
2. **Moving** - Coin is moving to a target position
3. **Collecting** - Coin is being collected with scale effects
4. **Paused** - Animation temporarily suspended
5. **Pooled** - Coin is ready for reuse

#### Multi-Phase Collection Animation
1. **Scale Up** (30% duration): EaseOutBack easing to 1.5x size
2. **Movement** (70% duration): EaseInSine easing to collection point with rotation
3. **Scale Down** (20% duration): EaseInBack easing to 0x size

#### Performance Considerations
- Use coroutine-based animations for best performance
- Limit concurrent coins to maintain 60fps
- Monitor memory usage during extended animations
- Test on target platforms for performance validation

#### UGUI Prefab Creation (Current Task: Story 1.2)
When creating UGUI coin prefabs for the animation system:
1. **Canvas Setup**: Create a Screen Space - Overlay Canvas for UGUI coins
2. **Image Component**: Use Unity UI Image component for visual representation
3. **Component Structure**: Ensure `CoinAnimationController` is attached to the same GameObject
4. **Animation Compatibility**: Verify RectTransform positioning works with coroutine animations
5. **Prefab Standardization**: Create consistent prefab structure for batch spawning

### ⚠️ Important Notes
- **Coroutine-Based**: All animations use Unity's coroutine system with custom mathematical easing
- **Memory Efficient**: Minimal memory footprint for high performance (587 lines of core code)
- **Cross-Platform**: Compatible with all Unity-supported platforms
- **Easy Integration**: Simple API for quick integration into existing projects
- **Zero-Dependency Philosophy**: Demonstrates professional-quality animation without external packages
- **Bilingual Documentation**: English (CLAUDE.md) and Chinese (Assets/Scripts/Examples/README.md) available

### 🔄 Architecture Evolution & Key Insights

#### Historical Context
This project represents a significant architectural simplification journey:
- **Evolved**: Completely removed external dependencies for maximum compatibility
- **Current**: Pure Unity coroutine implementation with custom mathematical easing

#### Key Architectural Insights
1. **Coroutine Power**: Unity coroutines provide sufficient capability for professional animations without external libraries
2. **Mathematical Easing**: Custom easing functions (EaseOutQuad, EaseOutBack, EaseInSine, EaseInBack) deliver visual quality comparable to commercial animation packages
3. **Performance through Simplicity**: 587 lines of core code achieve what typically requires thousands of lines with external dependencies
4. **State Machine Clarity**: Five-state system (Idle, Moving, Collecting, Paused, Pooled) provides robust lifecycle management
5. **Event-Driven Design**: Clean separation between animation controller and manager through event system

#### Development Philosophy Demonstrated
- **Extreme Simplification**: Remove complexity while maintaining functionality
- **Zero-Dependency Strategy**: Maximum compatibility through pure Unity implementation
- **Performance-First Design**: 60fps target with 50+ concurrent coins validated
- **Documentation Excellence**: Comprehensive bilingual documentation for accessibility
- **UGUI Integration**: Current focus on creating standardized UGUI coin prefabs

This codebase serves as an exemplar of how complex functionality (smooth coin animations) can be implemented with minimal complexity through careful architectural decisions and leveraging Unity's built-in capabilities effectively.

### 🎯 Current Development Focus

**Story 1.2 - Task 5**: Creating UGUI coin prefabs to provide standardized visual components for the animation system. This task ensures all coin animations use consistent prefacts with proper UGUI Canvas integration while maintaining the zero-dependency philosophy.
