using UnityEngine;

namespace Gomoku
{
    /// <summary>
    /// 棋型分数配置 - 使用 ScriptableObject 便于调试调优
    /// </summary>
    [CreateAssetMenu(fileName = "PatternScores", menuName = "Gomoku/Pattern Scores")]
    public class PatternScores : ScriptableObject
    {
        [Header("基础棋型分数")]
        public int five = 1000000;      // 五连
        public int liveFour = 100000;   // 活四（必胜）
        public int four = 10000;        // 冲四
        public int liveThree = 5000;    // 活三
        public int three = 500;         // 眠三
        public int liveTwo = 200;       // 活二
        public int two = 50;            // 眠二
        public int one = 10;            // 单子

        [Header("威胁组合加成")]
        public int doubleThree = 8000;   // 双活三
        public int fourThree = 15000;    // 四三
        public int doubleFour = 200000;  // 双四

        /// <summary>
        /// 获取棋型对应的分数
        /// </summary>
        public int GetScore(PatternType pattern)
        {
            return pattern switch
            {
                PatternType.Five => five,
                PatternType.LiveFour => liveFour,
                PatternType.Four => four,
                PatternType.LiveThree => liveThree,
                PatternType.Three => three,
                PatternType.LiveTwo => liveTwo,
                PatternType.Two => two,
                PatternType.One => one,
                _ => 0
            };
        }

        /// <summary>
        /// 获取默认分数配置
        /// </summary>
        public static PatternScores GetDefault()
        {
            var scores = CreateInstance<PatternScores>();
            return scores;
        }
    }
}
