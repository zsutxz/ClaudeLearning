Firecracker Chain Explosion Test
-------------------------------

(CC-BY-NC-SA 4.0 by karminski-牙医)

Please use three.js to implement a realistic "firecracker box" 3D explosion chain reaction demonstration. All code (including HTML, CSS, JavaScript) must be encapsulated in a single, standalone HTML file.

**Scene Setup:**
1. **Ground Plane:** Create a 1000×1000 gray, smooth horizontal plane that can receive shadows. Ground material uses ```{color: 0x808080, roughness: 0.8, metalness: 0.1}```.

2. **Firecracker Modeling:**
   - **Main Body:** Red cylinder, height 1, diameter 0.2, material using ```{color: 0xff0000, roughness: 0.6, metalness: 0.2}```
   - **End Caps:** Small brownish-yellow cylinders at the top and bottom of the main cylinder, height 0.05, material ```{color: 0xdaa520, roughness: 0.8}```
   - **Fuse:** Blue-green thin cylinder extending from the center of the top cap, length 0.5, diameter 0.05, material ```{color: 0x20b2aa, roughness: 0.4}```
   - **Collision Volume:** Firecrackers have collision volumes to prevent model overlap
   - **Burning State:** The fuse tip needs to display a small orange-red glowing sphere to simulate sparks

3. **Transparent Box:**
   - **Dimensions:** 10×10×8 transparent glass box, no collision volume at the top, collision volumes on other faces, material ```{color: 0xffffff, transparent: true, opacity: 0.15, roughness: 0.1, metalness: 0.0}```
   - **Inside the Box:** At animation start, randomly generate firecrackers in groups of 10 from a cluster 5 units above the box top, generating 10 groups total, firecrackers free-fall into the box interior
   - **Box Position:** Placed at the center-back position of the plane

**Explosion & Chain Reaction Simulation:**

1. **Trigger Mechanism:**
   - "Start Fireworks" button at the bottom of the page to trigger animation
   - A pre-lit firecracker falls into the box from above in a parabolic trajectory

2. **Fuse Burning Effect:**
   - **Burning Animation:** Fuse gradually shortens from top to bottom over 2 seconds, burning section shows orange-red glowing effect

3. **Explosion Effect:**
   - **Instant Replacement:** Firecracker disappears instantly upon explosion, replaced by numerous colored paper confetti fragments
   - **Confetti System:** Generate 50-80 small colored square/rectangular fragments in red color
   - **Explosion Shockwave:** Produces spherical expanding transparent shockwave effect, affecting surrounding firecrackers
   - **Fragment Physics:** Confetti has initial outward velocity, affected by gravity, with random rotation animation

4. **Impact Force & Ejection:**
   - **Area of Effect:** Firecrackers within 2-3 unit radius from explosion center receive impact
   - **Ejection Animation:** Impacted firecrackers are thrown in random directions and forces, traveling 1-5 unit distances
   - **Rotation Effect:** Ejected firecrackers have random tumbling rotation animation
   - **Collision Detection:** Ejected firecrackers bounce off box walls and ground

5. **Fire Spread & Chain Reaction:**
   - **Fire Range:** Each explosion produces a spherical fire zone with 3-unit radius, lasting 0.5-1 seconds
   - **Ignition Mechanism:** Unexploded firecrackers' fuses within the fire zone automatically ignite, beginning 1-second burn countdown
   - **Propagation Delay:** Different firecrackers have 0.1-0.3 second random ignition delays to avoid simultaneous explosions
   - **Chain Spread:** Forms realistic domino-style chain reaction

6. **Environmental Interaction Effects:**
   - **Ground Impact:** Fallen firecrackers and fragments produce small impact bounces on the ground
   - **Fragment Accumulation:** Paper confetti eventually scatters around the ground and box, forming pile effects

**Advanced Visual Effects:**

1. **Enhanced Particle Systems:**
   - **Spark Particles:** Bright sparks during fuse burning and explosions, with trailing effects
   - **Smoke Effect:** Gray-white smoke particles after explosions, slowly rising and gradually dissipating
   - **Halo Effect:** Bright halos during explosion moments, illuminating surrounding areas

2. **Dynamic Lighting:**
   - **Explosion Flash:** Create instant point light sources during each explosion to simulate explosion flashes
   - **Fuse Fire Light:** Burning fuses produce small dynamic point light sources
   - **Color Temperature:** Light source colors change from orange-red to bright white

**Lighting & Rendering Settings:**

1. **Light Source Configuration:**
   - **Main Light:** 45-degree angled directional light ```DirectionalLight```, intensity 1.2, capable of casting shadows
   - **Ambient Light:** Ambient light with intensity 0.4, ensuring no pure black areas in scene
   - **Dynamic Light Sources:** Temporary point lights from explosions and burning

2. **Shadow System:**
   - Enable renderer shadow mapping ```renderer.shadowMap.enabled = true```
   - All firecrackers and boxes cast shadows ```castShadow = true```
   - Ground receives shadows ```receiveShadow = true```
   - Dynamic shadow updates to reflect moving objects

3. **Post-Processing Effects:**
   - Temporary screen flash-white effect during explosions
   - Optional addition of slight camera shake effects

**Camera & Controls:**
- Camera views the entire scene from an oblique 45-degree angle, approximately 15 units from the box
- Mouse controllable
- Ensures complete observation of the chain reaction process and fragment scattering effects

**Code Structure Requirements:**
- Create independent ```Firecracker``` class to manage individual firecracker states
- Create ```ExplosionSystem``` class to manage explosion effects and particles
- Create ```ChainReactionManager``` class to control chain reaction logic
- All physics calculations use simplified but realistic motion equations
- Add detailed comments in code, especially for explosion propagation and physics simulation parts
- Performance optimization: reasonably control particle quantities

**Interactive Controls:**
- Provide "Reset Scene" button to restart demonstration
- Optional "Single Detonation" mode, click specific firecrackers for localized explosion testing
- Display statistics of current unexploded firecracker count and explosion occurrence count


