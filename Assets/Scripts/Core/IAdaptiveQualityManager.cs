using System;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 自适应质量管理器接口
    /// 避免程序集循环依赖
    /// </summary>
    public interface IAdaptiveQualityManager
    {
        /// <summary>
        /// 设置质量等级
        /// </summary>
        /// <param name="qualityLevel">质量等级</param>
        void SetQualityLevel(int qualityLevel);

        /// <summary>
        /// 启用/禁用自适应质量
        /// </summary>
        /// <param name="enabled">是否启用</param>
        void SetAdaptiveQualityEnabled(bool enabled);

        /// <summary>
        /// 获取性能报告
        /// </summary>
        /// <returns>性能报告</returns>
        object GetPerformanceReport();

        /// <summary>
        /// 启用/禁用自适应质量（属性）
        /// </summary>
        bool enableAdaptiveQuality { get; set; }

        /// <summary>
        /// 启用/禁用自动质量调整
        /// </summary>
        bool autoAdjustQuality { get; set; }

        /// <summary>
        /// 当前质量等级
        /// </summary>
        int CurrentQualityLevel { get; }

        /// <summary>
        /// FPS临界阈值
        /// </summary>
        float fpsCriticalThreshold { get; set; }
    }
}