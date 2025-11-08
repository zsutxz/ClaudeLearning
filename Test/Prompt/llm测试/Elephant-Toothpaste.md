Elephant Toothpaste Test
------------------------

(CC-BY-NC-SA 4.0 by karminski-牙医)

Please use three.js to implement a realistic "Elephant Toothpaste" chemical experiment 3D demonstration. All code (including HTML, CSS, JavaScript) must be encapsulated in a single independent HTML file.

**Scene Setup:**
1.  **Ground Plane:** Create a 1000*1000 gray, smooth horizontal plane that can receive shadows.
2.  **Erlenmeyer Flask:**
    *   **Shape:** Place a transparent glass Erlenmeyer flask at the center of the plane. The flask should have clear contours: a cylindrical neck, a gradually widening conical body, and a flat bottom. Do not use a simple cone as a substitute.
    *   **Material:** The flask material should be highly transparent glass with appropriate high transmission, low roughness, and correct refractive index to simulate glass. Set ```{ color: 0xffffff, transparent: true, opacity: 0.9, roughness: 0.95, metalness: 0.35, clearcoat: 1.0, clearcoatRoughness: 0.03, transmission: 0.95, ior: 1.5, side: THREE.DoubleSide}``` to see background and light refraction effects through the liquid.
    *   **Liquid:** The flask is pre-filled with fluorescent pink liquid to about one-third of its height. The liquid surface should be flat and conform to the inner walls of the flask. Note to reference the Erlenmeyer flask modeling shape - ideally copy part of the conical shape of the Erlenmeyer flask, and don't let the liquid modeling overflow from the flask.
    *   **Note:** The entire Erlenmeyer flask model should be oriented upward.

**"Elephant Toothpaste" Eruption Effect Simulation:**
1.  **Trigger:** The page provides a button in the footer position that starts the eruption when clicked.
2.  **Foam Formation and Texture:**
    *   The ejected foam should consist of **numerous tiny, semi-transparent particles that can merge with each other**, simulating the **dense and fluffy texture** of real foam, avoiding the appearance of isolated small balls. The color should be fluorescent pink, with some brightness variations to add depth.
    *   Consider using a simple **noise texture** or procedural method to add some surface details to foam particles, making them look more like porous foam rather than smooth spheres.
3.  **Jet Dynamics and Fluid Simulation:**
    *   **Initial Eruption:** Foam violently shoots upward from the flask mouth, forming a continuous rising column (dense foam stacked together to form a columnar shape). The height should be at least 3 times the height of the Erlenmeyer flask. The foam column only begins to disperse when it almost reaches maximum height, with high initial jet velocity and pressure.
    *   **Pressure Decay:** The jet intensity (velocity and foam generation rate) should **gradually weaken** over time, causing the foam column height and jet distance to gradually decrease, simulating the process of chemical reactant depletion.
    *   **Particle Motion:** Particle trajectories should simulate **basic fluid dynamics effects**, not just simple parabolic motion. For example, particles should have slight repulsive forces to simulate foam expansion, or random velocity perturbations around the main jet stream. Overall affected by gravity.
4.  **Foam-Environment Interaction and Deformation:**
    *   **Gravity and Accumulation:** Ejected foam particles fall under gravity influence. When foam particles contact the plane or flask exterior walls, they should **simulate compression deformation effects**, such as being flattened in the vertical direction (reduced Y-axis scaling) and slightly expanded horizontally (increased X and Z-axis scaling).
    *   **Accumulation Formation:** Foam falling on the plane and flask should gradually **accumulate to form a certain thickness of coverage**, rather than disappearing. Accumulated foam should also have similar deformation and merging effects.
    *   **Foam Fixation:** After falling onto object surfaces, foam slides a short distance before stopping movement.
5.  **Liquid Reduction:**
    As eruption continues, the liquid level inside the Erlenmeyer flask gradually decreases to demonstrate liquid reduction effects. Note this is not overall liquid shrinkage, but the gradual descent of the liquid's top surface.

**Lighting and Rendering:**
1.  **Light Sources:**
    *   Set up a main light source to produce clear shadows.
    *   Add ambient light to brighten scene dark areas, ensuring no pure black regions.
2.  **Shadows:** Enable renderer shadow mapping. The plane should receive shadows, and the flask and ejected foam should cast shadows.
3.  **Reflection and Refraction:** The flask material should display subtle environmental reflections and light refraction through liquid and glass.

**Camera and Controls:**
*   The camera should view the scene center (Erlenmeyer flask location) from a 45-degree diagonal overhead angle, ensuring clear observation of the entire eruption process and final accumulation effects.