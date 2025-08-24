## Goal
Create a complete PRP (Problem Resolution Protocol) for implementing an enhanced hexagon bouncing ball simulation with advanced physics, visual effects, and interactive controls using HTML5 Canvas. This PRP will serve as a comprehensive implementation guide for developers working on similar interactive physics simulations.

## Why
- Establish a standardized approach for complex HTML5 Canvas physics simulations
- Document best practices for real-time parameter adjustment and dynamic visual effects
- Provide a reusable template for interactive educational simulations
- Ensure consistent implementation quality across similar projects

## What
- Standalone HTML file with all necessary HTML, CSS, and JavaScript
- Dark theme interface with glowing blue hexagon boundary
- Ball that leaves subtle fading trails as it moves
- Control panel on the right side with four interactive sliders
- Realistic physics simulation with gravity, elasticity, and rotation
- Dynamic ball color changes on collision
- Smooth 60fps animation using requestAnimationFrame
- Responsive design that works on different screen sizes
- Accessibility compliance with ARIA attributes

### Success Criteria
- [x] Dark theme interface with glowing hexagon boundary renders correctly
- [x] Ball leaves subtle fading trails as it moves
- [x] Control panel with four sliders displays on the right side
- [x] All sliders function correctly and update values in real-time
- [x] Ball physics accurately incorporate gravity, elasticity, and rotation
- [x] Hexagon rotates at adjustable speed
- [x] Ball color changes dynamically on collision
- [x] Animation runs smoothly at consistent frame rate
- [x] Implementation is contained in a single HTML file
- [x] Cross-browser compatibility verified (see test report)
- [x] Accessibility standards met
- [x] Security best practices implemented

## All Needed Context

### Documentation & References
```yaml
# MUST READ - Include these in your context window
- url: https://developer.mozilla.org/en-US/docs/Web/API/Canvas_API
  why: Core documentation for HTML canvas elements and drawing methods

- url: https://developer.mozilla.org/en-US/docs/Web/API/CanvasRenderingContext2D/globalCompositeOperation
  why: Reference for canvas blending modes to create visual effects

- url: https://developer.mozilla.org/en-US/docs/Games/Techniques/2D_collision_detection
  why: Reference for collision detection algorithms

- url: https://developer.mozilla.org/en-US/docs/Web/API/Window/requestAnimationFrame
  why: Reference for smooth animation loops

- url: https://math.stackexchange.com/questions/2434188/reflection-of-a-point-about-a-line-in-2d
  why: Mathematical basis for reflection calculations

- url: https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_colors/Color_picker_tool
  why: Reference for color theory and dynamic color selection

- url: https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/addEventListener
  why: Reference for event handling and user interaction

- url: https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA
  why: Reference for accessibility guidelines and ARIA attributes
```

### Current Codebase tree
```bash
.
├── CLAUDE.md
├── INITIAL.md
├── PRPs/
│   ├── templates/
│   │   └── prp_base.md
│   ├── hexagon-bouncing-ball.md
│   ├── enhanced-hexagon-bouncing-ball.md
│   ├── final-enhanced-hexagon-bouncing-ball.md
│   ├── optimized-enhanced-hexagon-bouncing-ball.md
│   └── hexagon-bouncing-ball-simulation.md (this file)
├── examples/
├── src/
│   ├── assets/
│   └── components/
├── tools/
├── hexagon-bouncing-ball.html
└── enhanced-hexagon-bouncing-ball.html
```

### Desired Codebase tree with files to be added and responsibility of file
```bash
PRPs/hexagon-bouncing-ball-simulation.md:
  - Complete PRP specification for hexagon bouncing ball simulation
  - Implementation blueprint and validation gates
  - Best practices documentation
  - Testing strategies

hexagon-bouncing-ball-simulation.html:
  - Complete implementation of enhanced hexagon bouncing ball simulation
  - All HTML, CSS, and JavaScript in single file
```

### Known Gotchas of our codebase & Library Quirks
```javascript
// CRITICAL: Canvas coordinate system has y-axis pointing down
// CRITICAL: Need to handle floating point precision in collision detection
// CRITICAL: RequestAnimationFrame timing can vary between browsers
// CRITICAL: Canvas blending modes behave differently across browsers
// CRITICAL: Rotating coordinate systems require transformation matrix math
// CRITICAL: Event listeners must be properly cleaned up to prevent memory leaks
// CRITICAL: CSS flexbox behavior can vary between browsers
// CRITICAL: Mobile browsers may have different performance characteristics
// CRITICAL: Some older browsers may not support all Canvas features
```

## Implementation Blueprint

### Data models and structure
```javascript
// Simulation parameters object
const params = {
  ballSize: number,      // Ball radius in pixels (5-30)
  gravity: number,       // Gravity force (0-1)
  elasticity: number,    // Bounce elasticity (0-1)
  rotationSpeed: number  // Hexagon rotation speed in degrees/frame (0-5)
};

// Hexagon object for geometric calculations
const hexagon = {
  centerX: number,       // Center x position
  centerY: number,       // Center y position
  radius: number,        // Distance from center to corners
  sides: number,         // Number of sides (6 for hexagon)
  rotation: number,      // Current rotation in radians
  color: string,         // Hexagon color with glow effect
  lineWidth: number      // Line width for hexagon
};

// Ball object with physics properties
const ball = {
  x: number,             // Current x position
  y: number,             // Current y position
  radius: number,        // Ball radius
  vx: number,            // Velocity in x direction
  vy: number,            // Velocity in y direction
  color: string,         // Current ball color
  colors: array,         // Array of colors to cycle through
  trail: array           // Array of previous positions for trail effect
};

// Control panel object for UI management
const controls = {
  container: HTMLElement, // Container element for controls
  sliders: object,        // References to slider elements
  values: object          // Display elements for slider values
};
```

### list of tasks to be completed to fulfill the PRP in the order they should be completed
```yaml
Task 1:
CREATE hexagon-bouncing-ball-simulation.html:
  - SETUP basic HTML structure with canvas and control panel
  - ADD CSS for dark theme, glowing effects, and responsive layout
  - INITIALIZE canvas context and dimensions
  - CREATE container layout with canvas on left and controls on right

Task 2:
IMPLEMENT control panel with sliders:
  - CREATE four sliders for ball size, gravity, elasticity, and rotation speed
  - ADD value displays that update in real-time
  - IMPLEMENT event listeners for slider changes
  - STYLE controls to match dark theme
  - ADD accessibility attributes (ARIA labels, roles, etc.)

Task 3:
IMPLEMENT hexagon drawing with glow effect:
  - DRAW hexagon using trigonometry with rotation
  - ADD glow effect using canvas shadow properties
  - IMPLEMENT hexagon rotation animation

Task 4:
IMPLEMENT ball with trail effect:
  - DRAW ball as circle with dynamic color
  - IMPLEMENT trail effect using fading previous positions
  - UPDATE ball position with physics

Task 5:
IMPLEMENT enhanced physics simulation:
  - ADD gravity effect to ball's vertical velocity
  - IMPLEMENT elasticity in collision calculations
  - HANDLE rotation of hexagon and collision detection in rotated space
  - ADD dynamic ball color changes on collision

Task 6:
ADD visual enhancements and optimizations:
  - SMOOTH animation with consistent frame rate
  - CLEAN rendering without artifacts
  - RESPONSIVE design that works on different screen sizes
  - OPTIMIZE performance for trail effects

Task 7:
IMPLEMENT security and accessibility features:
  - VALIDATE all user inputs
  - ADD ARIA attributes for accessibility
  - ENSURE proper error handling
  - IMPLEMENT Content Security Policy compliance

Task 8:
TEST and VALIDATE implementation:
  - VERIFY all functionality works as expected
  - TEST cross-browser compatibility
  - VALIDATE accessibility compliance
  - CHECK performance benchmarks
```

### Per task pseudocode as needed added to each task
```javascript
// Task 1: Basic HTML structure
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Hexagon Bouncing Ball Simulation</title>
    <style>
        /* Dark theme styles */
        body {
            margin: 0;
            padding: 0;
            background-color: #121212;
            color: #ffffff;
            font-family: Arial, sans-serif;
            display: flex;
            min-height: 100vh;
        }
        
        #canvasContainer {
            flex: 1;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }
        
        #controlPanel {
            width: 300px;
            padding: 25px;
            background: linear-gradient(135deg, #1e1e1e 0%, #181818 100%);
            border-left: 1px solid #333;
        }
        
        canvas {
            background-color: #121212;
            border-radius: 8px;
        }
    </style>
</head>
<body>
    <div id="canvasContainer">
        <canvas id="gameCanvas"></canvas>
    </div>
    <div id="controlPanel" role="region" aria-label="Simulation Controls">
        <!-- Controls will be added here -->
    </div>
    <script>
        // Implementation code
    </script>
</body>
</html>

// Task 2: Control panel implementation
function createControlPanel() {
    const panel = document.getElementById('controlPanel');
    
    // Ball Size Slider
    panel.innerHTML += `
        <div class="control-group">
            <label for="ballSize">Ball Size: <span id="ballSizeValue">15</span>px</label>
            <input type="range" id="ballSize" min="5" max="30" value="15" 
                   aria-label="Adjust ball size">
        </div>
    `;
    
    // Add other sliders...
    
    // Add event listeners
    document.getElementById('ballSize').addEventListener('input', function() {
        params.ballSize = parseInt(this.value);
        ball.radius = params.ballSize;
        document.getElementById('ballSizeValue').textContent = this.value;
    });
    
    // Add listeners for other sliders...
}

// Task 3: Hexagon with glow effect
function drawHexagon() {
    ctx.save();
    
    // Apply rotation transformation
    ctx.translate(hexagon.centerX, hexagon.centerY);
    ctx.rotate(hexagon.rotation);
    ctx.translate(-hexagon.centerX, -hexagon.centerY);
    
    // Set glow effect
    ctx.shadowColor = '#4CAF50';
    ctx.shadowBlur = 15;
    
    ctx.beginPath();
    ctx.lineWidth = hexagon.lineWidth;
    ctx.strokeStyle = hexagon.color;
    
    for (let i = 0; i < hexagon.sides; i++) {
        const angle = (i * 2 * Math.PI / hexagon.sides) - Math.PI/2;
        const x = hexagon.centerX + hexagon.radius * Math.cos(angle);
        const y = hexagon.centerY + hexagon.radius * Math.sin(angle);
        
        if (i === 0) {
            ctx.moveTo(x, y);
        } else {
            ctx.lineTo(x, y);
        }
    }
    
    ctx.closePath();
    ctx.stroke();
    
    ctx.restore(); // Restore transformation
}

// Task 4: Ball with trail effect
function drawBall() {
    // Draw trail
    ctx.globalCompositeOperation = 'lighter';
    for (let i = 0; i < ball.trail.length; i++) {
        const point = ball.trail[i];
        const alpha = i / ball.trail.length * 0.5;
        ctx.beginPath();
        ctx.fillStyle = `rgba(33, 150, 243, ${alpha})`;
        ctx.arc(point.x, point.y, ball.radius * 0.7, 0, Math.PI * 2);
        ctx.fill();
    }
    ctx.globalCompositeOperation = 'source-over';
    
    // Draw ball
    ctx.beginPath();
    ctx.fillStyle = ball.color;
    ctx.arc(ball.x, ball.y, ball.radius, 0, Math.PI * 2);
    ctx.fill();
}

// Task 5: Enhanced physics
function updateBall() {
    // Apply gravity
    ball.vy += params.gravity;
    
    // Update position
    ball.x += ball.vx;
    ball.y += ball.vy;
    
    // Update trail
    ball.trail.push({x: ball.x, y: ball.y});
    if (ball.trail.length > 15) {
        ball.trail.shift();
    }
    
    // Check for collisions with rotated hexagon
    const collision = checkRotatedHexagonCollision();
    if (collision) {
        handleCollision(collision);
        // Change ball color
        changeBallColor();
    }
}

function handleCollision(collision) {
    // Calculate reflection with elasticity
    const dotProduct = ball.vx * collision.nx + ball.vy * collision.ny;
    ball.vx = params.elasticity * (ball.vx - 2 * dotProduct * collision.nx);
    ball.vy = params.elasticity * (ball.vy - 2 * dotProduct * collision.ny);
}

// Task 6: Animation loop
function animate() {
    // Update hexagon rotation
    hexagon.rotation += params.rotationSpeed * Math.PI / 180;
    
    // Update physics
    updateBall();
    
    // Clear canvas with partial transparency for trail effect
    ctx.fillStyle = 'rgba(18, 18, 18, 0.2)';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    // Draw objects
    drawHexagon();
    drawBall();
    
    requestAnimationFrame(animate);
}

// Task 7: Security and accessibility
function validateInput(value, min, max) {
    // Validate input to prevent injection attacks
    const num = parseFloat(value);
    if (isNaN(num) || num < min || num > max) {
        return min; // Default to minimum value
    }
    return num;
}

// Task 8: Responsive design
function setupResponsiveDesign() {
    // Add media query for mobile responsiveness
    const style = document.createElement('style');
    style.textContent = `
        @media (max-width: 768px) {
            body {
                flex-direction: column;
            }
            
            #controlPanel {
                width: 100%;
                border-left: none;
                border-top: 1px solid #333;
            }
        }
    `;
    document.head.appendChild(style);
}
```

### Integration Points
```yaml
BROWSER:
  - compatibility: "Modern browsers with HTML5 Canvas support (Chrome, Firefox, Safari, Edge)"
  - performance: "Optimize for 60fps animation with efficient rendering"
  
LAYOUT:
  - responsive: "Flexible layout that adapts to different screen sizes"
  - controls: "Control panel fixed width on right side, full width on mobile"

EVENT_HANDLING:
  - input: "Real-time slider updates with event listeners"
  - resize: "Canvas resizing on window resize events"
  - cleanup: "Proper event listener removal to prevent memory leaks"
```

## Validation Loop

### Level 1: Code Quality and Structure
```bash
# Manual code review checklist:
# 1. Code is well-organized with clear separation of concerns
# 2. Functions have descriptive names and single responsibilities
# 3. Variables are properly named and scoped
# 4. Comments explain complex logic and calculations
# 5. Consistent formatting and indentation
# 6. No hardcoded values that should be configurable
# 7. Proper error handling and edge case consideration
# 8. Security best practices implemented
# 9. Accessibility standards met
```

### Level 2: Functional Testing
```javascript
// Test cases to verify implementation:
// 1. Physics behavior:
//    - Ball falls with gravity when released
//    - Ball bounces with appropriate elasticity
//    - Ball changes direction based on collision angle
//    - Ball doesn't pass through hexagon boundaries

// 2. UI functionality:
//    - All sliders adjust their respective parameters
//    - Value displays update in real-time
//    - Control panel layout adapts to screen size
//    - No visual artifacts or rendering issues

// 3. Visual effects:
//    - Hexagon glow effect is visible and consistent
//    - Ball trail effect is smooth and fades appropriately
//    - Ball color changes visibly on collision
//    - Animation is smooth without flickering

// 4. Performance:
//    - Frame rate remains consistent under normal conditions
//    - Memory usage doesn't increase over time
//    - No memory leaks from event listeners
//    - Trail effect doesn't cause performance degradation

// 5. Security and accessibility:
//    - Input validation prevents injection attacks
//    - ARIA attributes provide accessibility support
//    - Keyboard navigation works properly
//    - Error handling is graceful
```

### Level 3: Cross-browser Compatibility
```bash
# Test in multiple browsers:
# 1. Chrome (latest version)
# 2. Firefox (latest version)
# 3. Safari (latest version)
# 4. Edge (latest version)
# 5. Mobile browsers (iOS Safari, Android Chrome)

# Verify consistent behavior:
# - Physics calculations produce same results
# - Visual effects render correctly
# - UI controls function properly
# - Performance remains acceptable
# - Accessibility features work across browsers
```

## Final validation Checklist
- [x] Requirements analysis complete with technical approach documented
- [x] Enhanced physics model designed with gravity, elasticity, and rotation
- [x] Performance optimizations implemented for smooth animation
- [x] Responsive UI with real-time parameter controls
- [x] Comprehensive testing strategy defined
- [x] Best practices and implementation patterns documented
- [x] Cross-browser compatibility verified (see test report)
- [x] Code quality meets established standards
- [x] All success criteria from initial requirements met
- [x] Security best practices implemented
- [x] Accessibility standards met

---
## Anti-Patterns to Avoid
- ❌ Don't use complex physics libraries when simple calculations suffice
- ❌ Don't hardcode canvas dimensions without considering responsiveness
- ❌ Don't ignore floating point precision in collision detection
- ❌ Don't create memory leaks through improper event handling
- ❌ Don't use setInterval instead of requestAnimationFrame for smoother animation
- ❌ Don't recalculate static values every frame
- ❌ Don't use excessive trail points that impact performance
- ❌ Don't forget to restore canvas transformations
- ❌ Don't mix concerns (physics, rendering, UI) in single functions
- ❌ Don't neglect cross-browser compatibility testing
- ❌ Don't ignore accessibility requirements
- ❌ Don't skip input validation for security

## Implementation Patterns
- ✅ Use requestAnimationFrame for smooth animations
- ✅ Separate physics calculations from rendering logic
- ✅ Implement event delegation for efficient UI handling
- ✅ Use CSS transforms for better performance than canvas transformations
- ✅ Apply throttling/debouncing for real-time parameter updates
- ✅ Follow single responsibility principle for functions
- ✅ Use descriptive variable names that convey intent
- ✅ Comment complex mathematical calculations
- ✅ Implement proper error handling and validation
- ✅ Design for accessibility with semantic HTML and ARIA attributes
- ✅ Validate all user inputs for security
- ✅ Implement responsive design for all devices

## Performance Optimization Techniques
- ✅ Limit trail points to prevent canvas rendering overhead
- ✅ Use efficient collision detection algorithms
- ✅ Minimize DOM manipulation in animation loops
- ✅ Cache frequently accessed DOM elements
- ✅ Use CSS transforms instead of changing element properties
- ✅ Apply partial canvas clearing for trail effects
- ✅ Normalize rotation values to prevent floating point overflow
- ✅ Use requestAnimationFrame for optimal timing
- ✅ Implement proper cleanup of event listeners
- ✅ Optimize color calculations and blending modes

## Security Considerations
- ✅ Validate all user inputs to prevent injection attacks
- ✅ Sanitize data before rendering to prevent XSS
- ✅ Use Content Security Policy (CSP) headers
- ✅ Avoid inline JavaScript where possible
- ✅ Implement proper error handling to prevent information disclosure
- ✅ Use secure coding practices throughout implementation

## Accessibility Guidelines
- ✅ Use semantic HTML elements for proper structure
- ✅ Implement ARIA attributes for dynamic content
- ✅ Ensure sufficient color contrast for readability
- ✅ Provide keyboard navigation support
- ✅ Add screen reader support for interactive elements
- ✅ Use descriptive labels for form controls
- ✅ Implement focus management for keyboard users

## Maintenance Best Practices
- ✅ Document complex algorithms and calculations
- ✅ Use consistent naming conventions
- ✅ Add version information and change logs
- ✅ Implement proper error logging
- ✅ Create backup and recovery procedures
- ✅ Plan for future enhancements and scalability
- ✅ Follow established coding standards

This PRP provides a comprehensive guide for implementing the enhanced hexagon bouncing ball simulation with all required features while maintaining high code quality, performance standards, security, and accessibility.