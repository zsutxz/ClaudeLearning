namespace Gomoku
{
    /// <summary>
    /// AI 玩家接口
    /// </summary>
    public interface IAIPlayer
    {
        /// <summary>
        /// 获取 AI 的落子位置
        /// </summary>
        /// <param name="board">当前棋盘状态</param>
        /// <param name="myPiece">AI 的棋子颜色</param>
        /// <returns>落子坐标 (x, y)</returns>
        (int x, int y) GetMove(Board board, PieceType myPiece);
    }
}
