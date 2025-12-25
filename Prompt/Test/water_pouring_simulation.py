#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
杯子倒水测试 - 二维流体模拟
(CC-BY-NC-SA 4.0 by karminski-牙医)

This program simulates liquid (represented by particles) being poured
from a tilting cup using pygame library.
"""

import pygame
import math
import random
import numpy as np

# Initialize Pygame
pygame.init()

# Constants
SCREEN_WIDTH = 1920
SCREEN_HEIGHT = 1080
BACKGROUND_COLOR = (255, 255, 255)  # White
CUP_COLOR = (0, 0, 0)  # Black
PARTICLE_COLOR = (0, 100, 255)  # Blue
GRAVITY = 0.1  # Gravity acceleration
DAMPING = 0.7  # Restitution coefficient for bounce
REPULSION_FORCE = 0.5  # Force for particle interaction
PARTICLE_RADIUS = 2  # Particle radius in pixels
NUM_PARTICLES = 400  # Number of particles
FPS = 60

class Particle:
    """Particle class to manage individual particle properties and physics"""

    def __init__(self, x, y):
        self.x = x
        self.y = y
        self.vx = random.uniform(-0.5, 0.5)  # Initial velocity with small random variation
        self.vy = 0
        self.radius = PARTICLE_RADIUS
        self.color = PARTICLE_COLOR
        self.mass = 1.0

    def update(self, dt=1.0):
        """Update particle position using Euler integration"""
        # Apply gravity
        self.vy += GRAVITY * dt

        # Update position
        self.x += self.vx * dt
        self.y += self.vy * dt

        # Apply small air resistance
        self.vx *= 0.999
        self.vy *= 0.999

    def apply_force(self, fx, fy):
        """Apply external force to particle"""
        self.vx += fx / self.mass
        self.vy += fy / self.mass

    def check_boundaries(self, width, height):
        """Check collision with screen boundaries"""
        # Left and right boundaries
        if self.x - self.radius < 0:
            self.x = self.radius
            self.vx = -self.vx * DAMPING
        elif self.x + self.radius > width:
            self.x = width - self.radius
            self.vx = -self.vx * DAMPING

        # Top boundary
        if self.y - self.radius < 0:
            self.y = self.radius
            self.vy = -self.vy * DAMPING

        # Bottom boundary (particles should not fall through)
        if self.y + self.radius > height:
            self.y = height - self.radius
            self.vy = -self.vy * DAMPING

            # Apply friction when hitting bottom
            self.vx *= 0.9

    def draw(self, screen):
        """Draw particle on screen"""
        pygame.draw.circle(screen, self.color, (int(self.x), int(self.y)), self.radius)
        # Add small highlight for better visibility
        highlight_pos = (int(self.x - self.radius//3), int(self.y - self.radius//3))
        pygame.draw.circle(screen, (150, 200, 255), highlight_pos, max(1, self.radius//3))

class Flask:
    """Flask class to manage cup shape and rotation"""

    def __init__(self, center_x, center_y):
        self.center_x = center_x
        self.center_y = center_y
        self.rotation_angle = 0  # Rotation angle in radians
        self.max_rotation = math.radians(135)  # Maximum rotation angle (135 degrees)
        self.rotation_speed = 0.005  # Rotation speed in radians per frame

        # Define cup shape as vertices before rotation (cup opening facing up)
        # Shape: bottom-left, top-left, top-right, bottom-right
        self.base_vertices = [
            (-60, 100),   # Bottom-left
            (-60, -50),   # Top-left
            (-40, -100),  # Inner top-left (opening)
            (40, -100),   # Inner top-right (opening)
            (60, -50),    # Top-right
            (60, 100)     # Bottom-right
        ]

        # Define edges as pairs of vertex indices
        self.edges = [
            (0, 1),  # Left side
            (1, 2),  # Top-left to opening
            (2, 3),  # Opening (this is the gap where particles pour out)
            (3, 4),  # Opening to top-right
            (4, 5),  # Right side
            (5, 0)   # Bottom
        ]

        self.vertices = self.base_vertices.copy()
        self.is_tilting = False
        self.particles_added = False

    def update(self):
        """Update flask rotation"""
        if self.is_tilting and self.rotation_angle < self.max_rotation:
            self.rotation_angle += self.rotation_speed
            if self.rotation_angle >= self.max_rotation:
                self.rotation_angle = self.max_rotation

    def start_tilting(self):
        """Start the tilting animation"""
        self.is_tilting = True

    def reset(self):
        """Reset flask to initial position"""
        self.rotation_angle = 0
        self.is_tilting = False
        self.particles_added = False
        self.update_vertices()

    def update_vertices(self):
        """Update vertex positions based on rotation"""
        self.vertices = []
        cos_angle = math.cos(self.rotation_angle)
        sin_angle = math.sin(self.rotation_angle)

        for vx, vy in self.base_vertices:
            # Rotate around center
            new_x = vx * cos_angle - vy * sin_angle + self.center_x
            new_y = vx * sin_angle + vy * cos_angle + self.center_y
            self.vertices.append((new_x, new_y))

    def get_edge_line(self, edge_index):
        """Get line equation coefficients for an edge"""
        if edge_index >= len(self.edges):
            return None

        i, j = self.edges[edge_index]
        x1, y1 = self.vertices[i]
        x2, y2 = self.vertices[j]

        # Line equation: ax + by + c = 0
        a = y2 - y1
        b = x1 - x2
        c = x2 * y1 - x1 * y2

        # Normalize
        length = math.sqrt(a*a + b*b)
        if length > 0:
            a /= length
            b /= length
            c /= length

        return a, b, c, (x1, y1, x2, y2)

    def check_particle_collision(self, particle):
        """Check and handle collision between particle and flask"""
        # Update vertices for current rotation
        self.update_vertices()

        for edge_idx in range(len(self.edges)):
            # Skip the opening edge (index 2)
            if edge_idx == 2:
                continue

            line_data = self.get_edge_line(edge_idx)
            if line_data is None:
                continue

            a, b, c, (x1, y1, x2, y2) = line_data

            # Calculate distance from particle center to line
            distance = abs(a * particle.x + b * particle.y + c)

            if distance < particle.radius + 2:  # Small margin for collision
                # Check if particle is on the correct side of the line (inside the cup)
                line_value = a * particle.x + b * particle.y + c
                center_value = a * self.center_x + b * self.center_y + c

                if line_value * center_value < 0:  # Particle is outside
                    # Calculate normal vector (pointing inward)
                    normal_x = -a if center_value > 0 else a
                    normal_y = -b if center_value > 0 else b

                    # Push particle back inside
                    push_distance = particle.radius + 2 - distance
                    particle.x += normal_x * push_distance
                    particle.y += normal_y * push_distance

                    # Reflect velocity
                    dot_product = particle.vx * normal_x + particle.vy * normal_y
                    particle.vx -= 2 * dot_product * normal_x
                    particle.vy -= 2 * dot_product * normal_y

                    # Apply damping
                    particle.vx *= DAMPING
                    particle.vy *= DAMPING

    def draw(self, screen):
        """Draw flask on screen"""
        # Update vertices for current rotation
        self.update_vertices()

        # Draw flask edges
        for i, (start_idx, end_idx) in enumerate(self.edges):
            # Skip drawing the opening edge (index 2)
            if i == 2:
                continue

            start_pos = self.vertices[start_idx]
            end_pos = self.vertices[end_idx]
            pygame.draw.line(screen, CUP_COLOR, start_pos, end_pos, 3)

        # Draw vertices as small circles for better visibility
        for vertex in self.vertices:
            pygame.draw.circle(screen, CUP_COLOR, (int(vertex[0]), int(vertex[1])), 4)

class WaterSimulation:
    """Main simulation class"""

    def __init__(self):
        self.screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
        pygame.display.set_caption("杯子倒水测试 - 二维流体模拟")
        self.clock = pygame.time.Clock()
        self.running = True

        # Create flask at center of screen
        self.flask = Flask(SCREEN_WIDTH // 2, SCREEN_HEIGHT // 2)

        # Create particles list
        self.particles = []
        self.particle_spawn_timer = 0
        self.particles_spawned = 0

        # Font for UI
        self.font = pygame.font.Font(None, 36)
        self.small_font = pygame.font.Font(None, 24)

    def spawn_particles(self):
        """Spawn particles above the cup opening"""
        if self.particles_spawned >= NUM_PARTICLES:
            return

        # Spawn particles in batches
        spawn_rate = 5  # Particles per frame
        for _ in range(spawn_rate):
            if self.particles_spawned >= NUM_PARTICLES:
                break

            # Spawn position above the cup
            x = self.flask.center_x + random.uniform(-30, 30)
            y = self.flask.center_y - 200 + random.uniform(-20, 20)

            particle = Particle(x, y)
            # Give particles initial downward velocity to help them fall into cup
            particle.vy = random.uniform(0.5, 2.0)
            particle.vx = random.uniform(-1, 1)

            self.particles.append(particle)
            self.particles_spawned += 1

    def apply_particle_interactions(self):
        """Apply repulsion forces between particles to simulate fluid behavior"""
        for i in range(len(self.particles)):
            for j in range(i + 1, len(self.particles)):
                particle1 = self.particles[i]
                particle2 = self.particles[j]

                # Calculate distance between particles
                dx = particle2.x - particle1.x
                dy = particle2.y - particle1.y
                distance = math.sqrt(dx*dx + dy*dy)

                # Check if particles are too close
                min_distance = particle1.radius + particle2.radius + 2

                if distance < min_distance and distance > 0:
                    # Calculate repulsion force
                    force_magnitude = REPULSION_FORCE * (1 - distance / min_distance)

                    # Normalize direction vector
                    dx /= distance
                    dy /= distance

                    # Apply forces
                    force_x = dx * force_magnitude
                    force_y = dy * force_magnitude

                    particle1.apply_force(-force_x, -force_y)
                    particle2.apply_force(force_x, force_y)

                    # Also separate particles if they're overlapping
                    if distance < min_distance * 0.8:
                        separation = (min_distance * 0.8 - distance) / 2
                        particle1.x -= dx * separation
                        particle1.y -= dy * separation
                        particle2.x += dx * separation
                        particle2.y += dy * separation

    def update(self):
        """Update simulation state"""
        # Spawn particles initially
        if self.particles_spawned < NUM_PARTICLES:
            self.particle_spawn_timer += 1
            if self.particle_spawn_timer >= 2:  # Spawn every 2 frames
                self.spawn_particles()
                self.particle_spawn_timer = 0
        else:
            # After all particles are spawned, start tilting after a delay
            if not self.flask.is_tilting and not self.flask.particles_added:
                pygame.time.wait(1000)  # Wait 1 second before tilting
                self.flask.start_tilting()
                self.flask.particles_added = True

        # Update flask
        self.flask.update()

        # Update particles
        for particle in self.particles:
            particle.update()

            # Check collision with flask
            self.flask.check_particle_collision(particle)

            # Check boundaries
            particle.check_boundaries(SCREEN_WIDTH, SCREEN_HEIGHT)

        # Apply particle interactions
        self.apply_particle_interactions()

    def draw(self):
        """Draw everything on screen"""
        self.screen.fill(BACKGROUND_COLOR)

        # Draw flask
        self.flask.draw(self.screen)

        # Draw particles
        for particle in self.particles:
            particle.draw(self.screen)

        # Draw UI text
        self.draw_ui()

        pygame.display.flip()

    def draw_ui(self):
        """Draw user interface elements"""
        # Title
        title_text = self.font.render("杯子倒水测试 - 二维流体模拟", True, (0, 0, 0))
        self.screen.blit(title_text, (20, 20))

        # Instructions
        instructions = [
            "按 SPACE 键重新开始",
            f"粒子数量: {len(self.particles)}/{NUM_PARTICLES}",
            f"杯子倾斜角度: {math.degrees(self.flask.rotation_angle):.1f}°",
            f"倾斜状态: {'进行中' if self.flask.is_tilting else '等待中' if self.flask.rotation_angle < self.flask.max_rotation else '完成'}"
        ]

        y_offset = 70
        for instruction in instructions:
            text = self.small_font.render(instruction, True, (50, 50, 50))
            self.screen.blit(text, (20, y_offset))
            y_offset += 30

        # Physics info
        physics_info = [
            f"重力: {GRAVITY}",
            f"阻尼系数: {DAMPING}",
            f"排斥力: {REPULSION_FORCE}",
            f"FPS: {int(self.clock.get_fps())}"
        ]

        y_offset = 70
        for info in physics_info:
            text = self.small_font.render(info, True, (50, 50, 50))
            self.screen.blit(text, (SCREEN_WIDTH - 200, y_offset))
            y_offset += 30

    def handle_events(self):
        """Handle pygame events"""
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                self.running = False
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_SPACE:
                    # Reset simulation
                    self.reset_simulation()
                elif event.key == pygame.K_ESCAPE:
                    self.running = False

    def reset_simulation(self):
        """Reset the entire simulation"""
        self.particles.clear()
        self.particles_spawned = 0
        self.particle_spawn_timer = 0
        self.flask.reset()

    def run(self):
        """Main simulation loop"""
        while self.running:
            self.handle_events()
            self.update()
            self.draw()
            self.clock.tick(FPS)

        pygame.quit()

def main():
    """Main function"""
    print("杯子倒水测试 - 二维流体模拟")
    print("================================")
    print("控制说明:")
    print("- SPACE 键: 重新开始")
    print("- ESC 键: 退出程序")
    print("================================")
    print("正在启动模拟...")

    simulation = WaterSimulation()
    simulation.run()

    print("模拟已结束")

if __name__ == "__main__":
    main()