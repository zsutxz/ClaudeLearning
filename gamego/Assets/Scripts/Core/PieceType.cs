namespace Gomoku
{
    /// <summary>
    /// 棋子类型枚举
    /// </summary>
    public enum PieceType
    {
        None = 0,   // 空位
        Black = 1,  // 黑棋
        White = 2   // 白棋
    }

    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        Playing,    // 游戏进行中
        BlackWin,   // 黑棋获胜
        WhiteWin,   // 白棋获胜
        Draw        // 平局
    }

    /// <summary>
    /// 游戏模式枚举
    /// </summary>
    public enum GameMode
    {
        PvAI,       // 人机对战
        PvP,        // 本地双人
        AIvsAI      // AI 自对弈
    }

    /// <summary>
    /// AI 难度枚举
    /// </summary>
    public enum AIDifficulty
    {
        Simple,     // 规则优先级
        Medium,     // Minimax depth=1
        Hard        // Minimax depth=3 + Alpha-Beta 剪枝
    }

    /// <summary>
    /// AI 类型枚举
    /// </summary>
    public enum AIType
    {
        Simple,     // 规则型 AI（快速但简单）
        Minimax,    // Minimax + Alpha-Beta 剪枝
        Mcts        // 蒙特卡洛树搜索
    }

    /// <summary>
    /// AI 难度等级
    /// </summary>
    public enum Difficulty
    {
        Easy,       // 简单
        Medium,     // 中等
        Hard        // 困难
    }
}
