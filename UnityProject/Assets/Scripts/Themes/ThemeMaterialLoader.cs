using UnityEngine;
using GomokuGame.Themes;

namespace GomokuGame.Themes
{
    public class ThemeMaterialLoader : MonoBehaviour
    {
        public ThemeManager themeManager;
        
        void Start()
        {
            if (themeManager != null)
            {
                AssignMaterialsToThemes();
            }
        }
        
        private void AssignMaterialsToThemes()
        {
            // Classic theme materials
            Material classicBlackMat = Resources.Load<Material>("Materials/Themes/Classic_Black_Mat");
            Material classicWhiteMat = Resources.Load<Material>("Materials/Themes/Classic_White_Mat");
            Material classicBoardMat = Resources.Load<Material>("Materials/Themes/Classic_Board_Mat");
            Material classicPointMat = Resources.Load<Material>("Materials/Themes/Classic_Point_Mat");
            
            if (classicBlackMat != null && classicWhiteMat != null && 
                classicBoardMat != null && classicPointMat != null)
            {
                ThemeSettings classicTheme = themeManager.GetThemeSettings(BoardTheme.Classic);
                if (classicTheme != null)
                {
                    classicTheme.blackPieceMaterial = classicBlackMat;
                    classicTheme.whitePieceMaterial = classicWhiteMat;
                    classicTheme.boardLineMaterial = classicBoardMat;
                    classicTheme.boardPointMaterial = classicPointMat;
                }
            }
            
            // Modern theme materials
            Material modernBlackMat = Resources.Load<Material>("Materials/Themes/Modern_Black_Mat");
            Material modernWhiteMat = Resources.Load<Material>("Materials/Themes/Modern_White_Mat");
            Material modernBoardMat = Resources.Load<Material>("Materials/Themes/Modern_Board_Mat");
            Material modernPointMat = Resources.Load<Material>("Materials/Themes/Modern_Point_Mat");
            
            if (modernBlackMat != null && modernWhiteMat != null && 
                modernBoardMat != null && modernPointMat != null)
            {
                ThemeSettings modernTheme = themeManager.GetThemeSettings(BoardTheme.Modern);
                if (modernTheme != null)
                {
                    modernTheme.blackPieceMaterial = modernBlackMat;
                    modernTheme.whitePieceMaterial = modernWhiteMat;
                    modernTheme.boardLineMaterial = modernBoardMat;
                    modernTheme.boardPointMaterial = modernPointMat;
                }
            }
            
            // Nature theme materials
            Material natureBlackMat = Resources.Load<Material>("Materials/Themes/Nature_Black_Mat");
            Material natureWhiteMat = Resources.Load<Material>("Materials/Themes/Nature_White_Mat");
            Material natureBoardMat = Resources.Load<Material>("Materials/Themes/Nature_Board_Mat");
            Material naturePointMat = Resources.Load<Material>("Materials/Themes/Nature_Point_Mat");
            
            if (natureBlackMat != null && natureWhiteMat != null && 
                natureBoardMat != null && naturePointMat != null)
            {
                ThemeSettings natureTheme = themeManager.GetThemeSettings(BoardTheme.Nature);
                if (natureTheme != null)
                {
                    natureTheme.blackPieceMaterial = natureBlackMat;
                    natureTheme.whitePieceMaterial = natureWhiteMat;
                    natureTheme.boardLineMaterial = natureBoardMat;
                    natureTheme.boardPointMaterial = naturePointMat;
                }
            }
        }
    }
}