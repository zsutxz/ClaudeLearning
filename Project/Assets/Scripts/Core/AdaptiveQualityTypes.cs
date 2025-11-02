using System;

namespace CoinAnimation.Core
{
    /// <summary>
    /// 质量等级枚举
    /// </summary>
    public enum QualityLevel
    {
        Minimum,
        Low,
        Medium,
        High
    }

    /// <summary>
    /// 性能趋势枚举
    /// </summary>
    public enum PerformanceTrend
    {
        Improving,
        Stable,
        Degrading
    }

    /// <summary>
    /// 质量压力等级枚举
    /// </summary>
    public enum QualityPressureLevel
    {
        None,
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// 调整类型枚举
    /// </summary>
    public enum AdjustmentType
    {
        Monitor,           // 仅监控
        Upgrade,           // 升级质量
        ModerateDowngrade, // 适度降级
        Downgrade,         // 降级
        EmergencyDowngrade // 紧急降级
    }
}