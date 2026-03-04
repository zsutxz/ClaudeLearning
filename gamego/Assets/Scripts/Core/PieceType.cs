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
        PvP         // 本地双人
    }
}
