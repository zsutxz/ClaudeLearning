import pygame
import math
import random
import sys

# Initialize Pygame
pygame.init()

# Screen dimensions
WIDTH, HEIGHT = 800, 600
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("2D Fluid Simulation - Pouring Liquid from Tilting Cup")

# Colors
BACKGROUND = (10, 10, 40)
CUP_COLOR = (180, 180, 200)
LIQUID_COLOR = (65, 105, 225)
PARTICLE_COLORS = [
    (65, 105, 225),   # Royal Blue
    (30, 144, 255),   # Dodger Blue
    (0, 191, 255),    # Deep Sky Blue
    (135, 206, 250),  # Light Sky Blue
    (70, 130, 180),   # Steel Blue
]

# Physics constants
GRAVITY = 0.5
FRICTION = 0.99
ELASTICITY = 0.8
PARTICLE_RADIUS = 3
MAX_PARTICLES = 1000

class Particle:
    def __init__(self, x, y):
        self.x = x
        self.y = y
        self.vx = random.uniform(-1, 1)
        self.vy = random.uniform(-1, 1)
        self.radius = PARTICLE_RADIUS
        self.color = random.choice(PARTICLE_COLORS)
        self.life = 255  # For fade-out effect
    
    def update(self):
        # Apply gravity
        self.vy += GRAVITY
        
        # Apply friction
        self.vx *= FRICTION
        self.vy *= FRICTION
        
        # Update position
        self.x += self.vx
        self.y += self.vy
        
        # Boundary collision
        if self.x - self.radius < 0:
            self.x = self.radius
            self.vx = -self.vx * ELASTICITY
        elif self.x + self.radius > WIDTH:
            self.x = WIDTH - self.radius
            self.vx = -self.vx * ELASTICITY
            
        if self.y - self.radius < 0:
            self.y = self.radius
            self.vy = -self.vy * ELASTICITY
        elif self.y + self.radius > HEIGHT:
            self.y = HEIGHT - self.radius
            self.vy = -self.vy * ELASTICITY
            # Apply more friction when on ground
            self.vx *= 0.9
            
        # Reduce life for fade-out effect
        self.life = max(0, self.life - 0.2)
    
    def draw(self, surface):
        # Draw with fade-out effect
        color_with_alpha = (*self.color, int(self.life))
        pygame.draw.circle(surface, self.color, (int(self.x), int(self.y)), self.radius)
        
        # Draw a highlight for 3D effect
        highlight_color = (min(255, self.color[0] + 50), 
                          min(255, self.color[1] + 50), 
                          min(255, self.color[2] + 50))
        pygame.draw.circle(surface, highlight_color, (int(self.x - self.radius/3), int(self.y - self.radius/3)), self.radius/2)

class Cup:
    def __init__(self, x, y):
        self.x = x
        self.y = y
        self.width = 120
        self.height = 80
        self.tilt_angle = 0  # In degrees
        self.tilt_speed = 0.5
        self.max_tilt = 90
        self.pouring = False
        self.pour_timer = 0
        
    def update(self):
        if self.pouring:
            if self.tilt_angle < self.max_tilt:
                self.tilt_angle += self.tilt_speed
        else:
            # Return to upright position
            if self.tilt_angle > 0:
                self.tilt_angle -= self.tilt_speed
                if self.tilt_angle < 0:
                    self.tilt_angle = 0
    
    def get_pouring_point(self):
        # Calculate the point where liquid pours out based on tilt angle
        angle_rad = math.radians(self.tilt_angle)
        pour_x = self.x + (self.width/2) * math.cos(angle_rad)
        pour_y = self.y - (self.width/2) * math.sin(angle_rad)
        return pour_x, pour_y
    
    def draw(self, surface):
        # Create cup polygon points
        angle_rad = math.radians(self.tilt_angle)
        
        # Base points of the cup (without tilt)
        base_points = [
            (self.x - self.width/2, self.y),
            (self.x + self.width/2, self.y),
            (self.x + self.width/2 - 20, self.y - self.height),
            (self.x - self.width/2 + 20, self.y - self.height)
        ]
        
        # Apply rotation to all points
        rotated_points = []
        for point in base_points:
            # Translate to origin
            x_translated = point[0] - self.x
            y_translated = point[1] - self.y
            
            # Rotate
            x_rotated = x_translated * math.cos(angle_rad) - y_translated * math.sin(angle_rad)
            y_rotated = x_translated * math.sin(angle_rad) + y_translated * math.cos(angle_rad)
            
            # Translate back
            rotated_points.append((x_rotated + self.x, y_rotated + self.y))
        
        # Draw cup
        pygame.draw.polygon(surface, CUP_COLOR, rotated_points)
        pygame.draw.polygon(surface, (100, 100, 150), rotated_points, 2)
        
        # Draw cup handle
        handle_center_x = self.x + self.width/2 + 15
        handle_center_y = self.y - self.height/2
        
        # Apply rotation to handle
        handle_offset_x = handle_center_x - self.x
        handle_offset_y = handle_center_y - self.y
        
        handle_rotated_x = handle_offset_x * math.cos(angle_rad) - handle_offset_y * math.sin(angle_rad)
        handle_rotated_y = handle_offset_x * math.sin(angle_rad) + handle_offset_y * math.cos(angle_rad)
        
        handle_x = handle_rotated_x + self.x
        handle_y = handle_rotated_y + self.y
        
        pygame.draw.circle(surface, CUP_COLOR, (int(handle_x), int(handle_y)), 15, 5)

def create_particles(pour_x, pour_y, count):
    particles = []
    for _ in range(count):
        # Create particles with some randomness around the pour point
        particle_x = pour_x + random.uniform(-5, 5)
        particle_y = pour_y + random.uniform(-5, 5)
        particle = Particle(particle_x, particle_y)
        # Give particles an initial velocity based on the pour
        particle.vx += random.uniform(-2, 2)
        particle.vy += random.uniform(-1, 1)
        particles.append(particle)
    return particles

def main():
    clock = pygame.time.Clock()
    particles = []
    cup = Cup(WIDTH // 2, HEIGHT // 2 + 50)
    
    # For controlling particle emission
    particle_emission_rate = 5
    particle_emission_counter = 0
    
    running = True
    while running:
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_SPACE:
                    cup.pouring = not cup.pouring
                elif event.key == pygame.K_ESCAPE:
                    running = False
            elif event.type == pygame.MOUSEBUTTONDOWN:
                if event.button == 1:  # Left mouse button
                    cup.pouring = True
            elif event.type == pygame.MOUSEBUTTONUP:
                if event.button == 1:  # Left mouse button
                    cup.pouring = False
        
        # Update
        cup.update()
        
        # Emit particles if cup is pouring
        if cup.pouring:
            particle_emission_counter += 1
            if particle_emission_counter >= particle_emission_rate:
                pour_x, pour_y = cup.get_pouring_point()
                new_particles = create_particles(pour_x, pour_y, 3)
                particles.extend(new_particles)
                particle_emission_counter = 0
        
        # Limit number of particles
        if len(particles) > MAX_PARTICLES:
            particles = particles[-MAX_PARTICLES:]
        
        # Update particles
        for particle in particles[:]:  # Use slice copy to avoid modification during iteration
            particle.update()
            # Remove dead particles
            if particle.life <= 0:
                particles.remove(particle)
        
        # Draw everything
        screen.fill(BACKGROUND)
        
        # Draw particles
        for particle in particles:
            particle.draw(screen)
        
        # Draw cup
        cup.draw(screen)
        
        # Draw instructions
        font = pygame.font.SysFont(None, 24)
        instructions = [
            "SPACE: Toggle pouring",
            "MOUSE: Hold to pour",
            "ESC: Quit"
        ]
        
        for i, text in enumerate(instructions):
            text_surface = font.render(text, True, (200, 200, 200))
            screen.blit(text_surface, (10, 10 + i * 30))
        
        # Draw particle count
        count_text = font.render(f"Particles: {len(particles)}", True, (200, 200, 200))
        screen.blit(count_text, (WIDTH - 150, 10))
        
        pygame.display.flip()
        clock.tick(60)
    
    pygame.quit()
    sys.exit()

if __name__ == "__main__":
    main()