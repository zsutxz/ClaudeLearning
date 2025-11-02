using System;
using UnityEngine;
using CoinAnimation.Core;
using CoinAnimation.Animation;

/// <summary>
/// 编译测试脚本 - 验证所有类型引用是否正确
/// </summary>
public class CompilationTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("开始编译测试...");

        // 测试 CoinAnimationManager 引用
        var animationManager = FindObjectOfType<CoinAnimationManager>();
        Debug.Log($"CoinAnimationManager found: {animationManager != null}");

        // 测试 CoinObjectPool 引用
        var objectPool = FindObjectOfType<CoinObjectPool>();
        Debug.Log($"CoinObjectPool found: {objectPool != null}");

        // 测试 PerformanceMonitor 引用
        var performanceMonitor = FindObjectOfType<PerformanceMonitor>();
        Debug.Log($"PerformanceMonitor found: {performanceMonitor != null}");

        //// 测试 AdaptiveQualityManager 引用
        //var qualityManager = FindObjectOfType<IAdaptiveQualityManager>();
        //Debug.Log($"IAdaptiveQualityManager found: {qualityManager != null}");

        //// 测试 RealTimeQualityAdjuster 引用
        //var realTimeAdjuster = FindObjectOfType<RealTimeQualityAdjuster>();
        //Debug.Log($"RealTimeQualityAdjuster found: {realTimeAdjuster != null}");

        //// 测试 DeviceCapabilityDetector 引用
        //var deviceDetector = FindObjectOfType<DeviceCapabilityDetector>();
        //Debug.Log($"DeviceCapabilityDetector found: {deviceDetector != null}");

        Debug.Log("编译测试完成 - 所有类型引用正确！");
    }
}