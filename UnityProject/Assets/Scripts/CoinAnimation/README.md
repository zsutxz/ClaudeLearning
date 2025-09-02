# DOTween Installation Instructions

## How to Install DOTween

Since DOTween is not available through the Unity Package Manager, you need to install it manually:

1. Visit the DOTween website: https://dotween.demigiant.com/
2. Download the latest version of DOTween
3. Import the DOTween package into your Unity project:
   - In Unity, go to `Assets > Import Package > Custom Package`
   - Select the DOTween package file you downloaded
   - Import all files

## Alternative Installation Method

You can also install DOTween through the Unity Asset Store:

1. Open the Unity Editor
2. Go to `Window > Asset Store`
3. Search for "DOTween"
4. Click "Download" and then "Import"

## After Installation

After installing DOTween, the CoinAnimationController script should work correctly. The script is already configured to use DOTween for the coin flying animation.

## Testing the Animation

To test the coin flying animation:

1. Create an empty GameObject in your scene
2. Attach the CoinAnimationController script to it
3. Configure the animation parameters in the Inspector
4. Enter Play Mode
5. Call the StartCoinAnimation() method (you can do this through a UI button or by calling it from another script)

The coin should fly from the start position to the end position with rotation and scaling effects.