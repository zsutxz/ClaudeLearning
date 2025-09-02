# How to Create the Coin Prefab

Since prefab files (.prefab) are binary files that can't be created directly through code, follow these steps to create the Coin prefab in Unity:

## Steps to Create the Coin Prefab

1. Open the Unity Editor with the project loaded
2. In the Hierarchy window, right-click and select "Create Empty" to create a new GameObject
3. Rename the GameObject to "Coin"
4. With the Coin GameObject selected, in the Inspector window:
   - Click "Add Component"
   - Search for and add a "Sprite Renderer" component
   - Click "Add Component" again
   - Search for and add the "Coin" script (Coin.cs)
5. With the Coin GameObject still selected, drag it from the Hierarchy window to the Project window into the Assets/Prefabs folder
6. This will create the Coin.prefab file
7. Delete the Coin GameObject from the Hierarchy (we'll use the prefab now)
8. You can now use the Coin.prefab in your scenes by dragging it from the Project window to the Hierarchy

## Configuring the Prefab

After creating the prefab, you can configure its default parameters:
1. Select the Coin.prefab in the Project window
2. In the Inspector window, you'll see the Coin component with its parameters
3. You can adjust the default values for:
   - Duration
   - Start Position
   - End Position
   - Enable Rotation
   - Rotation Speed
   - Enable Scaling
   - Start Scale
   - End Scale
   - Ease Type

These default values will be used when coins are spawned, but can be overridden when spawning individual coins through the CoinAnimationSystem.