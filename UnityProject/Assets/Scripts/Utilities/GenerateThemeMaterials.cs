// This script should be run in Unity to generate the theme materials
// To use: Create a new C# script with this code in Unity, then run it from the Gomoku menu

using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateThemeMaterials : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    static void GenerateMaterials()
    {
        // This method would be called when the game starts to ensure materials exist
        Debug.Log("Theme materials would be generated here in Unity Editor");
    }
}