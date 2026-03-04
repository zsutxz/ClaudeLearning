using UnityEngine;

namespace Gomoku
{
    /// <summary>
    /// 游戏配置 - 使用 ScriptableObject 存储游戏设置
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Gomoku/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Board Settings")]
        [Tooltip("棋盘大小（15x15）")]
        public int boardSize = 15;

        [Tooltip("格子大小")]
        public float cellSize = 1f;

        [Tooltip("棋子大小")]
        public float pieceScale = 0.4f;

        [Header("Visual Settings")]
        [Tooltip("棋盘背景颜色")]
        public Color boardColor = new Color(0.9f, 0.8f, 0.6f);

        [Tooltip("悬停高亮颜色")]
        public Color hoverColor = new Color(1f, 0.9f, 0.7f);

        [Tooltip("最后落子标记颜色")]
        public Color lastMoveColor = new Color(1f, 0.85f, 0.5f);

        [Tooltip("获胜连线颜色")]
        public Color winningColor = new Color(1f, 0.6f, 0.4f);

        [Header("Game Settings")]
        [Tooltip("默认游戏模式")]
        public GameMode defaultGameMode = GameMode.PvAI;

        [Tooltip("AI 是否先手")]
        public bool aiFirst = false;

        [Header("Audio Settings")]
        [Tooltip("是否启用音效")]
        public bool enableAudio = true;

        [Tooltip("音效音量")]
        [Range(0f, 1f)]
        public float sfxVolume = 0.8f;

        [Header("Animation Settings")]
        [Tooltip("落子动画时长")]
        public float placePieceDuration = 0.2f;

        [Tooltip("获胜动画时长")]
        public float winAnimationDuration = 0.5f;
    }
}
