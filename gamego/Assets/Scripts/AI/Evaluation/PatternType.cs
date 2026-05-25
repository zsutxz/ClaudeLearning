namespace Gomoku
{
    /// <summary>
    /// 五子棋棋型枚举
    /// </summary>
    public enum PatternType
    {
        None,           // 无棋型
        One,            // 单子
        Two,            // 眠二（一端被堵）
        LiveTwo,        // 活二
        Three,          // 眠三（一端被堵）
        LiveThree,      // 活三
        Four,           // 冲四（一端被堵）
        LiveFour,       // 活四（必胜）
        Five            // 五连（胜利）
    }
}
