using System;
using System.Collections;
using UnityEngine;

namespace CoinAnimation.Core.AdaptiveQuality
{
    /// <summary>
    /// 设备能力检测和基准测试系统
    /// Story 2.1 Task 2.1 - 自动检测设备硬件能力并进行性能基准测试
    /// </summary>
    public class DeviceCapabilityDetector : MonoBehaviour
    {
        #region Configuration

        [Header("Detection Settings")]
        [SerializeField] private bool enableDetection = true;
        [SerializeField] private bool runBenchmarkOnStart = true;
        [SerializeField] private float benchmarkDuration = 3.0f;

        [Header("Performance Thresholds")]
        [SerializeField] private float lowEndCPUScore = 30f;
        [SerializeField] private float midRangeCPUScore = 60f;
        [SerializeField] private int lowEndMemoryMB = 8192;  // 8GB
        [SerializeField] private int midRangeMemoryMB = 16384; // 16GB
        [SerializeField] private float lowEndGPUScore = 25f;
        [SerializeField] private float midRangeGPUScore = 50f;

        [Header("Benchmark Settings")]
        [SerializeField] private int benchmarkParticleCount = 1000;
        [SerializeField] private int benchmarkObjectCount = 500;
        [SerializeField] private int benchmarkDrawCalls = 100;

        #endregion

        #region Private Fields

        private DeviceCapabilities _deviceCapabilities;
        private BenchmarkResults _benchmarkResults;
        private bool _detectionComplete = false;
        private bool _benchmarkComplete = false;

        // 系统信息
        private string _deviceModel;
        private string _processorType;
        private int _processorCount;
        private int _systemMemorySize;
        private string _graphicsDeviceName;
        private int _graphicsMemorySize;
        private string _graphicsDeviceVendor;

        #endregion

        #region Properties

        public DeviceCapabilities DeviceCapabilities => _deviceCapabilities;
        public DeviceCapabilities DetectedCapabilities => _deviceCapabilities;
        public BenchmarkResults BenchmarkResults => _benchmarkResults;
        public bool DetectionComplete => _detectionComplete;
        public bool BenchmarkComplete => _benchmarkComplete;
        public DevicePerformanceTier PerformanceTier => _deviceCapabilities?.PerformanceTier ?? DevicePerformanceTier.Unknown;

        #endregion

        #region Events

        public event Action<DeviceCapabilities> OnDetectionComplete;
        public event Action<BenchmarkResults> OnBenchmarkComplete;
        public event Action<DevicePerformanceTier> OnPerformanceTierDetermined;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (enableDetection)
            {
                StartCoroutine(DetectionWorkflow());
            }
        }

        #endregion

        #region Detection Workflow

        private IEnumerator DetectionWorkflow()
        {
            Debug.Log("[DeviceCapabilityDetector] Starting device detection...");

            // 步骤1: 收集系统信息
            CollectSystemInformation();
            yield return null;

            // 步骤2: 分析硬件能力
            AnalyzeHardwareCapabilities();
            yield return null;

            // 步骤3: 运行基准测试 (如果启用)
            if (runBenchmarkOnStart)
            {
                yield return StartCoroutine(RunPerformanceBenchmark());
            }

            // 步骤4: 确定性能等级
            DeterminePerformanceTier();

            // 完成
            _detectionComplete = true;
            OnDetectionComplete?.Invoke(_deviceCapabilities);
            OnPerformanceTierDetermined?.Invoke(_deviceCapabilities.PerformanceTier);

            Debug.Log($"[DeviceCapabilityDetector] Detection complete. Device tier: {_deviceCapabilities.PerformanceTier}");
        }

        #endregion

        #region System Information Collection

        private void CollectSystemInformation()
        {
            try
            {
                // 基本设备信息
                _deviceModel = SystemInfo.deviceModel;
                _processorType = SystemInfo.processorType;
                _processorCount = SystemInfo.processorCount;
                _systemMemorySize = SystemInfo.systemMemorySize;

                // GPU信息
                _graphicsDeviceName = SystemInfo.graphicsDeviceName;
                _graphicsMemorySize = SystemInfo.graphicsMemorySize;
                _graphicsDeviceVendor = SystemInfo.graphicsDeviceVendor;

                Debug.Log($"[DeviceCapabilityDetector] System Info collected:");
                Debug.Log($"  Device: {_deviceModel}");
                Debug.Log($"  CPU: {_processorType} ({_processorCount} cores)");
                Debug.Log($"  Memory: {_systemMemorySize}MB");
                Debug.Log($"  GPU: {_graphicsDeviceName} ({_graphicsMemorySize}MB)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DeviceCapabilityDetector] Error collecting system info: {e.Message}");
            }
        }

        #endregion

        #region Hardware Capability Analysis

        private void AnalyzeHardwareCapabilities()
        {
            _deviceCapabilities = new DeviceCapabilities();

            // CPU能力分析
            _deviceCapabilities.CPUScore = CalculateCPUScore();
            _deviceCapabilities.ProcessorType = _processorType;
            _deviceCapabilities.ProcessorCount = _processorCount;

            // 内存能力分析
            _deviceCapabilities.MemoryScore = CalculateMemoryScore();
            _deviceCapabilities.TotalMemoryMB = _systemMemorySize;
            _deviceCapabilities.AvailableMemoryMB = _systemMemorySize; // 简化处理

            // GPU能力分析
            _deviceCapabilities.GPUScore = CalculateGPUScore();
            _deviceCapabilities.GraphicsDeviceName = _graphicsDeviceName;
            _deviceCapabilities.GraphicsMemorySize = _graphicsMemorySize;
            _deviceCapabilities.GraphicsVendor = _graphicsDeviceVendor;

            // 存储能力分析
            _deviceCapabilities.StorageScore = CalculateStorageScore();

            Debug.Log($"[DeviceCapabilityDetector] Hardware Analysis:");
            Debug.Log($"  CPU Score: {_deviceCapabilities.CPUScore:F1}");
            Debug.Log($"  Memory Score: {_deviceCapabilities.MemoryScore:F1}");
            Debug.Log($"  GPU Score: {_deviceCapabilities.GPUScore:F1}");
            Debug.Log($"  Storage Score: {_deviceCapabilities.StorageScore:F1}");
        }

        private float CalculateCPUScore()
        {
            float score = 0f;

            // 基于处理器核心数
            score += Mathf.Min(_processorCount * 5f, 30f);

            // 基于处理器频率 (简化处理)
            try
            {
                // 从处理器名称中提取频率信息
                var processorInfo = _processorType.ToLower();
                if (processorInfo.Contains("i9") || processorInfo.Contains("ryzen 9"))
                    score += 30f;
                else if (processorInfo.Contains("i7") || processorInfo.Contains("ryzen 7"))
                    score += 25f;
                else if (processorInfo.Contains("i5") || processorInfo.Contains("ryzen 5"))
                    score += 20f;
                else if (processorInfo.Contains("i3") || processorInfo.Contains("ryzen 3"))
                    score += 15f;
                else
                    score += 10f;
            }
            catch
            {
                score += 10f; // 默认分数
            }

            return Mathf.Min(score, 100f);
        }

        private float CalculateMemoryScore()
        {
            if (_systemMemorySize >= 32768) // 32GB+
                return 100f;
            else if (_systemMemorySize >= 16384) // 16GB+
                return 80f;
            else if (_systemMemorySize >= 8192) // 8GB+
                return 60f;
            else if (_systemMemorySize >= 4096) // 4GB+
                return 40f;
            else
                return 20f;
        }

        private float CalculateGPUScore()
        {
            float score = 0f;

            // 基于GPU内存
            if (_graphicsMemorySize >= 8192) // 8GB+
                score += 40f;
            else if (_graphicsMemorySize >= 4096) // 4GB+
                score += 30f;
            else if (_graphicsMemorySize >= 2048) // 2GB+
                score += 20f;
            else
                score += 10f;

            // 基于GPU型号
            try
            {
                var gpuName = _graphicsDeviceName.ToLower();
                if (gpuName.Contains("rtx 40") || gpuName.Contains("rx 7000"))
                    score += 40f;
                else if (gpuName.Contains("rtx 30") || gpuName.Contains("rx 6000"))
                    score += 35f;
                else if (gpuName.Contains("rtx 20") || gpuName.Contains("gtx 16") || gpuName.Contains("rx 5000"))
                    score += 30f;
                else if (gpuName.Contains("gtx 10") || gpuName.Contains("rx 400"))
                    score += 25f;
                else if (gpuName.Contains("gtx 9") || gpuName.Contains("rx 300"))
                    score += 20f;
                else
                    score += 15f;
            }
            catch
            {
                score += 15f; // 默认分数
            }

            return Mathf.Min(score, 100f);
        }

        private float CalculateStorageScore()
        {
            // 简化处理，基于设备类型判断存储性能
            var deviceName = _deviceModel.ToLower();
            
            if (deviceName.Contains("pro") || deviceName.Contains("gaming"))
                return 80f;
            else if (deviceName.Contains("ultra") || deviceName.Contains("max"))
                return 100f;
            else
                return 60f;
        }

        #endregion

        #region Performance Benchmark

        private IEnumerator RunPerformanceBenchmark()
        {
            Debug.Log("[DeviceCapabilityDetector] Starting performance benchmark...");

            _benchmarkResults = new BenchmarkResults
            {
                StartTime = DateTime.UtcNow
            };

            // 基准测试1: CPU计算测试
            yield return StartCoroutine(CPUBenchmarkTest());

            // 基准测试2: GPU渲染测试
            yield return StartCoroutine(GPUBenchmarkTest());

            // 基准测试3: 内存访问测试
            yield return StartCoroutine(MemoryBenchmarkTest());

            // 基准测试4: 综合性能测试
            yield return StartCoroutine(ComprehensiveBenchmarkTest());

            _benchmarkResults.EndTime = DateTime.UtcNow;
            _benchmarkResults.Duration = (_benchmarkResults.EndTime - _benchmarkResults.StartTime).TotalSeconds;
            _benchmarkResults.OverallScore = CalculateOverallBenchmarkScore();

            _benchmarkComplete = true;
            OnBenchmarkComplete?.Invoke(_benchmarkResults);

            Debug.Log($"[DeviceCapabilityDetector] Benchmark complete. Overall score: {_benchmarkResults.OverallScore:F1}");
        }

        private IEnumerator CPUBenchmarkTest()
        {
            var startTime = Time.realtimeSinceStartup;
            int iterations = 100000;
            float result = 0f;

            for (int i = 0; i < iterations; i++)
            {
                result += Mathf.Sqrt(i) * Mathf.Sin(i * 0.1f);
            }

            var duration = Time.realtimeSinceStartup - startTime;
            _benchmarkResults.CPUBenchmarkScore = Mathf.Max(0f, 100f - duration * 10f);
            _benchmarkResults.CPUBenchmarkTime = duration;

            yield return null;
        }

        private IEnumerator GPUBenchmarkTest()
        {
            var startTime = Time.realtimeSinceStartup;

            // 创建临时游戏对象进行GPU测试
            var testObject = new GameObject("BenchmarkTest");
            var renderer = testObject.AddComponent<MeshRenderer>();
            var filter = testObject.AddComponent<MeshFilter>();

            // 创建简单网格
            var mesh = new Mesh();
            var vertices = new Vector3[benchmarkObjectCount];
            var indices = new int[benchmarkObjectCount];

            for (int i = 0; i < benchmarkObjectCount; i++)
            {
                vertices[i] = UnityEngine.Random.insideUnitSphere * 10f;
                indices[i] = i;
            }

            mesh.vertices = vertices;
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            filter.mesh = mesh;

            // 渲染测试
            for (int frame = 0; frame < 60; frame++)
            {
                testObject.transform.Rotate(0f, 1f, 0f);
                yield return null;
            }

            var duration = Time.realtimeSinceStartup - startTime;
            _benchmarkResults.GPUBenchmarkScore = Mathf.Max(0f, 100f - duration * 5f);
            _benchmarkResults.GPUBenchmarkTime = duration;

            // 清理
            if (mesh != null) Destroy(mesh);
            Destroy(testObject);

            yield return null;
        }

        private IEnumerator MemoryBenchmarkTest()
        {
            var startTime = Time.realtimeSinceStartup;
            var testArrays = new int[100][];
            const int arraySize = 1024;

            try
            {
                // 内存分配测试
                for (int i = 0; i < 100; i++)
                {
                    testArrays[i] = new int[arraySize];
                }
            }
            catch (OutOfMemoryException)
            {
                Debug.LogWarning("[DeviceCapabilityDetector] Memory benchmark hit memory limit");
                _benchmarkResults.MemoryBenchmarkScore = 10f;
                yield break;
            }

            // 内存访问测试（移到try-catch外部）
            for (int iteration = 0; iteration < 10; iteration++)
            {
                for (int i = 0; i < 100; i++)
                {
                    for (int j = 0; j < arraySize; j++)
                    {
                        testArrays[i][j] = iteration * i + j;
                    }
                }
                yield return null; // 每帧后让出控制权
            }

            var duration = Time.realtimeSinceStartup - startTime;
            _benchmarkResults.MemoryBenchmarkScore = Mathf.Max(0f, 100f - duration * 8f);
            _benchmarkResults.MemoryBenchmarkTime = duration;

            // 清理
            for (int i = 0; i < testArrays.Length; i++)
            {
                testArrays[i] = null;
            }

            yield return null;
        }

        private IEnumerator ComprehensiveBenchmarkTest()
        {
            var startTime = Time.realtimeSinceStartup;
            var testObjects = new GameObject[50];

            // 创建复杂场景测试综合性能
            for (int i = 0; i < 50; i++)
            {
                testObjects[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                testObjects[i].transform.position = UnityEngine.Random.insideUnitSphere * 20f;
                testObjects[i].AddComponent<Rigidbody>();
            }

            // 运行物理模拟
            for (int frame = 0; frame < 120; frame++) // 2秒测试
            {
                foreach (var obj in testObjects)
                {
                    if (obj != null)
                    {
                        obj.GetComponent<Rigidbody>().AddForce(UnityEngine.Random.insideUnitSphere * 0.1f);
                    }
                }
                yield return null;
            }

            var duration = Time.realtimeSinceStartup - startTime;
            _benchmarkResults.ComprehensiveScore = Mathf.Max(0f, 100f - duration * 3f);
            _benchmarkResults.ComprehensiveTime = duration;

            // 清理
            foreach (var obj in testObjects)
            {
                if (obj != null) Destroy(obj);
            }

            yield return null;
        }

        private float CalculateOverallBenchmarkScore()
        {
            if (_benchmarkResults == null) return 0f;

            return (
                _benchmarkResults.CPUBenchmarkScore * 0.3f +
                _benchmarkResults.GPUBenchmarkScore * 0.3f +
                _benchmarkResults.MemoryBenchmarkScore * 0.2f +
                _benchmarkResults.ComprehensiveScore * 0.2f
            );
        }

        #endregion

        #region Performance Tier Determination

        private void DeterminePerformanceTier()
        {
            var overallScore = _deviceCapabilities.CalculateOverallScore();

            if (overallScore >= 80f)
                _deviceCapabilities.PerformanceTier = DevicePerformanceTier.HighEnd;
            else if (overallScore >= 50f)
                _deviceCapabilities.PerformanceTier = DevicePerformanceTier.MidRange;
            else if (overallScore >= 30f)
                _deviceCapabilities.PerformanceTier = DevicePerformanceTier.LowEnd;
            else
                _deviceCapabilities.PerformanceTier = DevicePerformanceTier.Minimum;

            Debug.Log($"[DeviceCapabilityDetector] Performance tier determined: {_deviceCapabilities.PerformanceTier} (Score: {overallScore:F1})");
        }

        #endregion

        #region Public API

        /// <summary>
        /// 手动重新检测设备能力
        /// </summary>
        public void RedetectDeviceCapabilities()
        {
            _detectionComplete = false;
            _benchmarkComplete = false;
            StartCoroutine(DetectionWorkflow());
        }

        /// <summary>
        /// 获取设备能力报告
        /// </summary>
        public DeviceCapabilityReport GetDeviceReport()
        {
            return new DeviceCapabilityReport
            {
                DeviceCapabilities = _deviceCapabilities,
                BenchmarkResults = _benchmarkResults,
                DetectionTime = DateTime.UtcNow,
                IsDetectionComplete = _detectionComplete,
                IsBenchmarkComplete = _benchmarkComplete
            };
        }

        /// <summary>
        /// 检查设备是否满足最低要求
        /// </summary>
        public bool MeetsMinimumRequirements()
        {
            return _deviceCapabilities != null && 
                   _deviceCapabilities.PerformanceTier >= DevicePerformanceTier.Minimum;
        }

        /// <summary>
        /// 获取推荐的质量设置
        /// </summary>
        public QualitySettings GetRecommendedQualitySettings()
        {
            if (_deviceCapabilities == null)
                return QualitySettings.GetDefaultSettings();

            return _deviceCapabilities.PerformanceTier switch
            {
                DevicePerformanceTier.HighEnd => QualitySettings.GetHighSettings(),
                DevicePerformanceTier.MidRange => QualitySettings.GetMediumSettings(),
                DevicePerformanceTier.LowEnd => QualitySettings.GetLowSettings(),
                _ => QualitySettings.GetMinimumSettings()
            };
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class DeviceCapabilities
    {
        public float CPUScore;
        public float MemoryScore;
        public float GPUScore;
        public float StorageScore;
        public float OverallScore;

        public string ProcessorType;
        public int ProcessorCount;
        public int TotalMemoryMB;
        public int AvailableMemoryMB;
        public string GraphicsDeviceName;
        public int GraphicsMemorySize;
        public string GraphicsVendor;

        public DevicePerformanceTier PerformanceTier;

        public float CalculateOverallScore()
        {
            OverallScore = (CPUScore * 0.3f + MemoryScore * 0.25f + GPUScore * 0.35f + StorageScore * 0.1f);
            return OverallScore;
        }
    }

    [System.Serializable]
    public class BenchmarkResults
    {
        public DateTime StartTime;
        public DateTime EndTime;
        public double Duration;

        public float CPUBenchmarkScore;
        public float GPUBenchmarkScore;
        public float MemoryBenchmarkScore;
        public float ComprehensiveScore;
        public float OverallScore;

        public float CPUBenchmarkTime;
        public float GPUBenchmarkTime;
        public float MemoryBenchmarkTime;
        public float ComprehensiveTime;
    }

    [System.Serializable]
    public class DeviceCapabilityReport
    {
        public DeviceCapabilities DeviceCapabilities;
        public BenchmarkResults BenchmarkResults;
        public DateTime DetectionTime;
        public bool IsDetectionComplete;
        public bool IsBenchmarkComplete;
    }

    public enum DevicePerformanceTier
    {
        Unknown,
        Minimum,
        LowEnd,
        MidRange,
        HighEnd
    }

    #endregion

    #region Quality Settings Helper

    [System.Serializable]
    public class QualitySettings
    {
        public int MaxCoinCount;
        public bool EnableParticleEffects;
        public bool EnableAdvancedShaders;
        public float AnimationQuality;
        public int TextureQuality;
        public bool EnablePostProcessing;

        public static QualitySettings GetMinimumSettings()
        {
            return new QualitySettings
            {
                MaxCoinCount = 15,
                EnableParticleEffects = false,
                EnableAdvancedShaders = false,
                AnimationQuality = 0.3f,
                TextureQuality = 0,
                EnablePostProcessing = false
            };
        }

        public static QualitySettings GetLowSettings()
        {
            return new QualitySettings
            {
                MaxCoinCount = 25,
                EnableParticleEffects = false,
                EnableAdvancedShaders = false,
                AnimationQuality = 0.5f,
                TextureQuality = 1,
                EnablePostProcessing = false
            };
        }

        public static QualitySettings GetMediumSettings()
        {
            return new QualitySettings
            {
                MaxCoinCount = 50,
                EnableParticleEffects = true,
                EnableAdvancedShaders = false,
                AnimationQuality = 0.7f,
                TextureQuality = 2,
                EnablePostProcessing = false
            };
        }

        public static QualitySettings GetHighSettings()
        {
            return new QualitySettings
            {
                MaxCoinCount = 100,
                EnableParticleEffects = true,
                EnableAdvancedShaders = true,
                AnimationQuality = 1.0f,
                TextureQuality = 3,
                EnablePostProcessing = true
            };
        }

        public static QualitySettings GetDefaultSettings()
        {
            return GetMediumSettings();
        }
    }

    #endregion
}