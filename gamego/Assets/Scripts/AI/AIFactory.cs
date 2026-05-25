using UnityEngine;

namespace Gomoku
{
    /// <summary>
    /// AI 工厂类 - 根据类型和难度创建 AI 实例
    /// </summary>
    public static class AIFactory
    {
        /// <summary>
        /// 创建 AI 玩家实例
        /// </summary>
        /// <param name="type">AI 类型</param>
        /// <param name="difficulty">难度等级</param>
        /// <returns>AI 玩家实例</returns>
        public static IAIPlayer CreateAI(AIType type, Difficulty difficulty)
        {
            return type switch
            {
                AIType.Minimax => CreateMinimaxAI(difficulty),
                AIType.Mcts => CreateMctsAI(difficulty),
                _ => CreateSimpleAI(difficulty)
            };
        }

        /// <summary>
        /// 创建 SimpleAI（规则型 AI）
        /// 难度通过随机因子控制
        /// </summary>
        private static IAIPlayer CreateSimpleAI(Difficulty difficulty)
        {
            // 随机因子：Easy 有 50% 随机，Medium 20%，Hard 0%
            float randomFactor = difficulty switch
            {
                Difficulty.Easy => 0.5f,
                Difficulty.Medium => 0.2f,
                Difficulty.Hard => 0.0f,
                _ => 0.3f
            };
            return new SimpleAI(randomFactor);
        }

        /// <summary>
        /// 创建 MinimaxAI（搜索型 AI）
        /// 难度通过搜索深度控制
        /// </summary>
        private static IAIPlayer CreateMinimaxAI(Difficulty difficulty)
        {
            // 搜索深度：Easy=1, Medium=2, Hard=3
            int depth = difficulty switch
            {
                Difficulty.Easy => 1,
                Difficulty.Medium => 2,
                Difficulty.Hard => 3,
                _ => 2
            };
            return new MinimaxAI(depth);
        }

        /// <summary>
        /// 创建 MctsAI（蒙特卡洛树搜索 AI）
        /// 难度通过模拟次数控制
        /// </summary>
        private static IAIPlayer CreateMctsAI(Difficulty difficulty)
        {
            // 模拟次数：Easy=500, Medium=1000, Hard=3000
            int simulations = difficulty switch
            {
                Difficulty.Easy => 500,
                Difficulty.Medium => 1000,
                Difficulty.Hard => 3000,
                _ => 1000
            };
            return new MctsAI(simulations);
        }
    }
}
