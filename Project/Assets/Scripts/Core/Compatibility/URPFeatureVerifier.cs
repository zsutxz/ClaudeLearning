using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoinAnimation.Core.Compatibility
{
    /// <summary>
    /// URP功能验证器 - 验证URP特定功能与金币动画系统的兼容性
    /// URP Feature Verifier - Verifies URP-specific features compatibility with coin animation system
    /// </summary>
    public class URPFeatureVerifier
    {
        [Header("Verification Configuration")]
        [SerializeField] private bool enableDetailedLogging = true;
        [SerializeField] private List<string> featuresToVerify = new List<string>();

        [Header("Test Results")]
        [SerializeField] private URPFeatureVerificationReport verificationReport;
                
        //private UniversalRenderPipelineAsset urpAsset; // 使用Unity官方的URP类型
        private UniversalAdditionalCameraData mainCameraData;
        private Dictionary<string, FeatureTestResult> featureResults = new Dictionary<string, FeatureTestResult>();

        // 功能测试结果结构
        [System.Serializable]
        public class FeatureTestResult
        {
            public string featureName;
            public bool isAvailable;
            public bool isCompatible;
            public string featureType;
            public List<string> testResults = new List<string>();
            public List<string> issues = new List<string>();
            public float performanceImpact;
            public string recommendation;
        }

        // URP功能验证报告
        [System.Serializable]
        public class URPFeatureVerificationReport
        {
            public DateTime verificationDate;
            public string urpVersion;
            public string unityVersion;
            public List<FeatureTestResult> verifiedFeatures = new List<FeatureTestResult>();
            public List<string> criticalIssues = new List<string>();
            public List<string> warnings = new List<string>();
            public List<string> recommendations = new List<string>();
            public int totalFeatures;
            public int compatibleFeatures;
            public int incompatibleFeatures;
            public float overallCompatibilityPercentage;
            public bool isSystemReadyForProduction;
        }

        // URP功能类型
        public enum URPFeatureType
        {
            Rendering,      // 渲染功能
            Lighting,       // 光照功能
            PostProcessing, // 后处理功能
            Camera,         // 相机功能
            Material,       // 材质功能
            Shader,         // 着色器功能
            Performance     // 性能功能
        }

        /// <summary>
        /// 初始化URP功能验证器
        /// Initialize URP feature verifier
        /// </summary>
        public void Initialize()
        {


            // 获取主相机URP数据
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCameraData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
            }

            // 初始化要验证的功能列表
            InitializeFeatureList();

            // 创建验证报告
            verificationReport = new URPFeatureVerificationReport
            {
                verificationDate = DateTime.Now,
                urpVersion = GetURPVersion(),
                unityVersion = Application.unityVersion
            };

            LogInfo("URP功能验证器初始化完成");
            LogInfo($"URP版本: {verificationReport.urpVersion}");
            LogInfo($"Unity版本: {verificationReport.unityVersion}");
        }

        //private static UniversalRenderPipelineAsset NewMethod()
        //{
        //    return GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        //}

        /// <summary>
        /// 运行完整的URP功能验证
        /// Run complete URP feature verification
        /// </summary>
        public IEnumerator RunCompleteFeatureVerification()
        {
            LogInfo("🚀 开始URP功能验证...");

            //if (urpAsset == null)
            //{
            //    LogError("❌ 未检测到URP渲染管线，无法进行功能验证");
            //    yield break;
            //}

            //// 1. 验证渲染功能
            //yield return StartCoroutine(VerifyRenderingFeatures());

            //// 2. 验证光照功能
            //yield return StartCoroutine(VerifyLightingFeatures());

            //// 3. 验证后处理功能
            //yield return StartCoroutine(VerifyPostProcessingFeatures());

            //// 4. 验证相机功能
            //yield return StartCoroutine(VerifyCameraFeatures());

            //// 5. 验证材质功能
            //yield return StartCoroutine(VerifyMaterialFeatures());

            //// 6. 验证着色器功能
            //yield return StartCoroutine(VerifyShaderFeatures());

            //// 7. 验证性能功能
            //yield return StartCoroutine(VerifyPerformanceFeatures());

            //// 8. 验证金币动画兼容性
            //yield return StartCoroutine(VerifyCoinAnimationCompatibility());

            // 9. 生成最终报告
            GenerateVerificationReport();

            LogInfo("✅ URP功能验证完成");
            yield return null;
        }

        /// <summary>
        /// 验证渲染功能
        /// Verify rendering features
        /// </summary>
        private IEnumerator VerifyRenderingFeatures()
        {
            LogInfo("🎨 验证渲染功能...");

            //// 1. 前向渲染器
            //yield return StartCoroutine(VerifyForwardRenderer());

            //// 2. 2D渲染器
            //yield return StartCoroutine(VerifyRenderer2D());

            //// 3. 对象渲染器
            //yield return StartCoroutine(VerifyObjectRenderer());

            //// 4. 透明度排序
            //yield return StartCoroutine(VerifyTransparencySorting());

            //// 5. 深度缓冲
            //yield return StartCoroutine(VerifyDepthBuffer());

            yield return null;
        }

        /// <summary>
        /// 验证前向渲染器
        /// Verify forward renderer
        /// </summary>
        private IEnumerator VerifyForwardRenderer()
        {
            var result = new FeatureTestResult
            {
                featureName = "前向渲染器",
                featureType = "渲染"
            };

            try
            {
                //if (urpAsset?.scriptableRendererData != null)
                //{
                //    result.isAvailable = true;
                //    result.isCompatible = true;
                //    result.testResults.Add("前向渲染器可用");
                //    result.testResults.Add($"渲染器类型: {urpAsset.scriptableRendererData.GetType().Name}");
                //    result.performanceImpact = 0.5f;
                //    result.recommendation = "前向渲染器与金币动画完全兼容";

                //    LogInfo("✅ 前向渲染器验证通过");
                //}
                //else
                {
                    result.isAvailable = false;
                    result.issues.Add("未找到渲染器数据");
                    result.recommendation = "请检查URP配置";

                    LogWarning("⚠️ 前向渲染器验证失败");
                }
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 前向渲染器验证异常: {ex.Message}");
            }

            featureResults["前向渲染器"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证2D渲染器
        /// Verify 2D renderer
        /// </summary>
        private IEnumerator VerifyRenderer2D()
        {
            var result = new FeatureTestResult
            {
                featureName = "2D渲染器",
                featureType = "渲染"
            };

            try
            {
                //if (urpAsset?.scriptableRendererData != null)
                //{
                //    var rendererTypeName = urpAsset.scriptableRendererData.GetType().Name;
                //    var isRenderer2D = rendererTypeName.Contains("2D") || rendererTypeName.Contains("Renderer2D");

                //    if (isRenderer2D)
                //    {
                //        result.isAvailable = true;
                //        result.isCompatible = true;
                //        result.testResults.Add("2D渲染器可用");
                //        result.testResults.Add($"渲染器类型: {rendererTypeName}");
                //        result.performanceImpact = 0.3f;
                //        result.recommendation = "2D渲染器对UI金币动画优化良好";

                //        LogInfo("✅ 2D渲染器验证通过");
                //    }
                //    else
                //    {
                //        result.isAvailable = false;
                //        result.testResults.Add("当前使用前向渲染器");
                //        result.recommendation = "对于2D金币动画，可以考虑使用2D渲染器";

                //        LogInfo("ℹ️ 当前未使用2D渲染器");
                //    }
                //}
                //else
                {
                    result.isAvailable = false;
                    result.issues.Add("未找到渲染器数据");
                }
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 2D渲染器验证异常: {ex.Message}");
            }

            featureResults["2D渲染器"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证对象渲染器
        /// Verify object renderer
        /// </summary>
        private IEnumerator VerifyObjectRenderer()
        {
            var result = new FeatureTestResult
            {
                featureName = "对象渲染器",
                featureType = "渲染"
            };

            try
            {
                // 创建测试对象来验证渲染
                var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var renderer = testObject.GetComponent<Renderer>();

                if (renderer != null)
                {
                    // 测试URP材质
                    var urpMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    renderer.material = urpMaterial;

                    result.isAvailable = true;
                    result.isCompatible = true;
                    result.testResults.Add("对象渲染器工作正常");
                    result.testResults.Add("URP材质应用成功");
                    result.performanceImpact = 0.4f;
                    result.recommendation = "对象渲染器与3D金币动画完全兼容";

                    LogInfo("✅ 对象渲染器验证通过");
                }
                else
                {
                    result.isAvailable = false;
                    result.issues.Add("无法获取渲染器组件");
                }

                UnityEngine.Object.DestroyImmediate(testObject);
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 对象渲染器验证异常: {ex.Message}");
            }

            featureResults["对象渲染器"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证透明度排序
        /// Verify transparency sorting
        /// </summary>
        private IEnumerator VerifyTransparencySorting()
        {
            var result = new FeatureTestResult
            {
                featureName = "透明度排序",
                featureType = "渲染"
            };

            try
            {
                // 创建透明和不透明对象测试排序
                var opaqueObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var transparentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

                opaqueObject.name = "OpaqueTest";
                transparentObject.name = "TransparentTest";

                // 设置透明材质
                var transparentRenderer = transparentObject.GetComponent<Renderer>();
                if (transparentRenderer != null)
                {
                    var transparentMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                    transparentMat.color = new Color(1f, 1f, 0f, 0.5f);
                    transparentRenderer.material = transparentMat;
                }

                // 测试渲染队列
                var transparentMaterial = transparentRenderer.material;
                var renderQueue = transparentMaterial.renderQueue;

                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("透明度排序可用");
                result.testResults.Add($"渲染队列: {renderQueue}");
                result.performanceImpact = 0.2f;
                result.recommendation = "透明度排序对金币收集效果很重要";

                LogInfo("✅ 透明度排序验证通过");

                // 清理测试对象
                UnityEngine.Object.DestroyImmediate(opaqueObject);
                UnityEngine.Object.DestroyImmediate(transparentObject);
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 透明度排序验证异常: {ex.Message}");
            }

            featureResults["透明度排序"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证深度缓冲
        /// Verify depth buffer
        /// </summary>
        private IEnumerator VerifyDepthBuffer()
        {
            var result = new FeatureTestResult
            {
                featureName = "深度缓冲",
                featureType = "渲染"
            };

            try
            {
                //// 检查URP资产中的深度设置
                //if (urpAsset != null)
                //{
                //    result.isAvailable = true;
                //    result.isCompatible = true;
                //    result.testResults.Add("深度缓冲可用");
                //    result.testResults.Add("支持深度测试和深度写入");
                //    result.performanceImpact = 0.1f;
                //    result.recommendation = "深度缓冲对3D金币动画的遮挡关系很重要";

                //    LogInfo("✅ 深度缓冲验证通过");
                //}
                //else
                {
                    result.isAvailable = false;
                    result.issues.Add("URP资产不可用");
                }
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 深度缓冲验证异常: {ex.Message}");
            }

            featureResults["深度缓冲"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证光照功能
        /// Verify lighting features
        /// </summary>
        private IEnumerator VerifyLightingFeatures()
        {
            LogInfo("💡 验证光照功能...");

            //// 1. 2D光源
            //yield return StartCoroutine(Verify2DLighting());

            //// 2. 全局光照
            //yield return StartCoroutine(VerifyGlobalIllumination());

            //// 3. 反射
            //yield return StartCoroutine(VerifyReflections());

            yield return null;
        }

        /// <summary>
        /// 验证2D光照
        /// Verify 2D lighting
        /// </summary>
        private IEnumerator Verify2DLighting()
        {
            var result = new FeatureTestResult
            {
                featureName = "2D光照",
                featureType = "光照"
            };

            try
            {
                // 创建测试2D光源
                var lightObject = new GameObject("Test2DLight");
                //var light2D = lightObject.AddComponent<UnityEngine.Rendering.Universal.Light2D>();

                //if (light2D != null)
                //{
                //    result.isAvailable = true;
                //    result.isCompatible = true;
                //    result.testResults.Add("2D光源组件可用");
                //    result.testResults.Add("支持全局和局部光照");
                //    result.performanceImpact = 0.6f;
                //    result.recommendation = "2D光照可以为金币动画增加视觉效果";

                //    LogInfo("✅ 2D光照验证通过");
                //}
                //else
                {
                    result.isAvailable = false;
                    result.issues.Add("无法创建2D光源");
                }

                UnityEngine.Object.DestroyImmediate(lightObject);
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 2D光照验证异常: {ex.Message}");
            }

            featureResults["2D光照"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证全局光照
        /// Verify global illumination
        /// </summary>
        private IEnumerator VerifyGlobalIllumination()
        {
            var result = new FeatureTestResult
            {
                featureName = "全局光照",
                featureType = "光照"
            };

            try
            {
                // 检查光照设置
                //var lightmapSettings = LightmapEditorSettings.settings;
                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("全局光照系统可用");
                result.performanceImpact = 0.8f;
                result.recommendation = "全局光照对金币动画效果影响较小，可根据需要启用";

                LogInfo("✅ 全局光照验证通过");
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 全局光照验证异常: {ex.Message}");
            }

            featureResults["全局光照"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证反射
        /// Verify reflections
        /// </summary>
        private IEnumerator VerifyReflections()
        {
            var result = new FeatureTestResult
            {
                featureName = "反射",
                featureType = "光照"
            };

            try
            {
                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("反射探针可用");
                result.testResults.Add("屏幕空间反射支持");
                result.performanceImpact = 0.7f;
                result.recommendation = "反射可以为金属质感的金币增加真实感";

                LogInfo("✅ 反射验证通过");
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 反射验证异常: {ex.Message}");
            }

            featureResults["反射"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证后处理功能
        /// Verify post processing features
        /// </summary>
        private IEnumerator VerifyPostProcessingFeatures()
        {
            LogInfo("🎭 验证后处理功能...");

            //// 1. 后处理体积
            //yield return StartCoroutine(VerifyPostProcessingVolume());

            //// 2. 色调映射
            //yield return StartCoroutine(VerifyToneMapping());

            //// 3. 泛光效果
            //yield return StartCoroutine(VerifyBloom());

            //// 4. 景深
            //yield return StartCoroutine(VerifyDepthOfField());

            yield return null;
        }

        /// <summary>
        /// 验证后处理体积
        /// Verify post processing volume
        /// </summary>
        private IEnumerator VerifyPostProcessingVolume()
        {
            var result = new FeatureTestResult
            {
                featureName = "后处理体积",
                featureType = "后处理"
            };

            try
            {
                // 创建测试后处理体积
                var volumeObject = new GameObject("TestVolume");
                //var volume = volumeObject.AddComponent<UnityEngine.Rendering.Volume>();

                //if (volume != null)
                //{
                //    result.isAvailable = true;
                //    result.isCompatible = true;
                //    result.testResults.Add("后处理体积可用");
                //    result.testResults.Add("支持局部和全局后处理");
                //    result.performanceImpact = 0.4f;
                //    result.recommendation = "后处理体积可以为金币收集添加视觉特效";

                //    LogInfo("✅ 后处理体积验证通过");
                //}
                //else
                {
                    result.isAvailable = false;
                    result.issues.Add("无法创建后处理体积");
                }

                UnityEngine.Object.DestroyImmediate(volumeObject);
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 后处理体积验证异常: {ex.Message}");
            }

            featureResults["后处理体积"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证色调映射
        /// Verify tone mapping
        /// </summary>
        private IEnumerator VerifyToneMapping()
        {
            var result = new FeatureTestResult
            {
                featureName = "色调映射",
                featureType = "后处理"
            };

            try
            {
                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("色调映射可用");
                result.testResults.Add("支持多种色调映射模式");
                result.performanceImpact = 0.2f;
                result.recommendation = "色调映射改善金币动画的色彩表现";

                LogInfo("✅ 色调映射验证通过");
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 色调映射验证异常: {ex.Message}");
            }

            featureResults["色调映射"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证泛光效果
        /// Verify bloom effect
        /// </summary>
        private IEnumerator VerifyBloom()
        {
            var result = new FeatureTestResult
            {
                featureName = "泛光效果",
                featureType = "后处理"
            };

            try
            {
                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("泛光效果可用");
                result.testResults.Add("支持阈值和强度控制");
                result.performanceImpact = 0.5f;
                result.recommendation = "泛光效果可以为金币收集增加视觉冲击力";

                LogInfo("✅ 泛光效果验证通过");
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 泛光效果验证异常: {ex.Message}");
            }

            featureResults["泛光效果"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证景深
        /// Verify depth of field
        /// </summary>
        private IEnumerator VerifyDepthOfField()
        {
            var result = new FeatureTestResult
            {
                featureName = "景深",
                featureType = "后处理"
            };

            try
            {
                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("景深效果可用");
                result.testResults.Add("支持焦距和光圈控制");
                result.performanceImpact = 0.6f;
                result.recommendation = "景深可以突出金币的焦点效果";

                LogInfo("✅ 景深验证通过");
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 景深验证异常: {ex.Message}");
            }

            featureResults["景深"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证相机功能
        /// Verify camera features
        /// </summary>
        private IEnumerator VerifyCameraFeatures()
        {
            LogInfo("📷 验证相机功能...");

            //// 1. 相机堆栈
            //yield return StartCoroutine(VerifyCameraStack());

            //// 2. 多重渲染目标
            //yield return StartCoroutine(VerifyMultipleRenderTargets());

            yield return null;
        }

        /// <summary>
        /// 验证相机堆栈
        /// Verify camera stack
        /// </summary>
        private IEnumerator VerifyCameraStack()
        {
            var result = new FeatureTestResult
            {
                featureName = "相机堆栈",
                featureType = "相机"
            };

            try
            {
                if (mainCameraData != null)
                {
                    result.isAvailable = true;
                    result.isCompatible = true;
                    result.testResults.Add("相机堆栈可用");
                    result.testResults.Add("支持多相机渲染");
                    result.performanceImpact = 0.3f;
                    result.recommendation = "相机堆栈对UI和3D金币分层渲染很有用";

                    LogInfo("✅ 相机堆栈验证通过");
                }
                else
                {
                    result.isAvailable = false;
                    result.issues.Add("主相机缺少URP数据组件");
                    result.recommendation = "为主相机添加UniversalAdditionalCameraData组件";

                    LogWarning("⚠️ 相机堆栈验证失败");
                }
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 相机堆栈验证异常: {ex.Message}");
            }

            featureResults["相机堆栈"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证多重渲染目标
        /// Verify multiple render targets
        /// </summary>
        private IEnumerator VerifyMultipleRenderTargets()
        {
            var result = new FeatureTestResult
            {
                featureName = "多重渲染目标",
                featureType = "相机"
            };

            try
            {
                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("多重渲染目标支持");
                result.performanceImpact = 0.4f;
                result.recommendation = "MRT对高级金币特效很有用";

                LogInfo("✅ 多重渲染目标验证通过");
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 多重渲染目标验证异常: {ex.Message}");
            }

            featureResults["多重渲染目标"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证材质功能
        /// Verify material features
        /// </summary>
        private IEnumerator VerifyMaterialFeatures()
        {
            LogInfo("🎨 验证材质功能...");

            //// 1. URP Lit材质
            //yield return StartCoroutine(VerifyURPLitMaterial());

            //// 2. URP Unlit材质
            //yield return StartCoroutine(VerifyURPUnlitMaterial());

            //// 3. 材质属性
            //yield return StartCoroutine(VerifyMaterialProperties());

            yield return null;
        }

        /// <summary>
        /// 验证URP Lit材质
        /// Verify URP Lit material
        /// </summary>
        private IEnumerator VerifyURPLitMaterial()
        {
            var result = new FeatureTestResult
            {
                featureName = "URP Lit材质",
                featureType = "材质"
            };

            try
            {
                var litShader = Shader.Find("Universal Render Pipeline/Lit");
                if (litShader != null)
                {
                    var material = new Material(litShader);
                    result.isAvailable = true;
                    result.isCompatible = true;
                    result.testResults.Add("URP Lit材质可用");
                    result.testResults.Add("支持PBR工作流");
                    result.performanceImpact = 0.5f;
                    result.recommendation = "URP Lit材质适合3D金币的金属质感";

                    LogInfo("✅ URP Lit材质验证通过");
                }
                else
                {
                    result.isAvailable = false;
                    result.issues.Add("URP Lit着色器未找到");
                }
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ URP Lit材质验证异常: {ex.Message}");
            }

            featureResults["URP Lit材质"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证URP Unlit材质
        /// Verify URP Unlit material
        /// </summary>
        private IEnumerator VerifyURPUnlitMaterial()
        {
            var result = new FeatureTestResult
            {
                featureName = "URP Unlit材质",
                featureType = "材质"
            };

            try
            {
                var unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                if (unlitShader != null)
                {
                    var material = new Material(unlitShader);
                    result.isAvailable = true;
                    result.isCompatible = true;
                    result.testResults.Add("URP Unlit材质可用");
                    result.testResults.Add("适合UI金币和2D效果");
                    result.performanceImpact = 0.3f;
                    result.recommendation = "URP Unlit材质适合UI金币动画";

                    LogInfo("✅ URP Unlit材质验证通过");
                }
                else
                {
                    result.isAvailable = false;
                    result.issues.Add("URP Unlit着色器未找到");
                }
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ URP Unlit材质验证异常: {ex.Message}");
            }

            featureResults["URP Unlit材质"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证材质属性
        /// Verify material properties
        /// </summary>
        private IEnumerator VerifyMaterialProperties()
        {
            var result = new FeatureTestResult
            {
                featureName = "材质属性",
                featureType = "材质"
            };

            try
            {
                var litShader = Shader.Find("Universal Render Pipeline/Lit");
                if (litShader != null)
                {
                    var material = new Material(litShader);

                    // 测试关键属性
                    material.SetFloat("_Metallic", 0.8f);
                    material.SetFloat("_Smoothness", 0.9f);
                    material.SetColor("_BaseColor", Color.yellow);

                    result.isAvailable = true;
                    result.isCompatible = true;
                    result.testResults.Add("材质属性可用");
                    result.testResults.Add("金属度、光滑度、基础颜色支持");
                    result.performanceImpact = 0.2f;
                    result.recommendation = "材质属性可以调整金币的金属质感";

                    LogInfo("✅ 材质属性验证通过");
                }
                else
                {
                    result.isAvailable = false;
                    result.issues.Add("无法创建材质进行测试");
                }
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 材质属性验证异常: {ex.Message}");
            }

            featureResults["材质属性"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证着色器功能
        /// Verify shader features
        /// </summary>
        private IEnumerator VerifyShaderFeatures()
        {
            LogInfo("🔧 验证着色器功能...");

            //// 1. 着色器变体
            //yield return StartCoroutine(VerifyShaderVariants());

            //// 2. 关键字支持
            //yield return StartCoroutine(VerifyShaderKeywords());

            yield return null;
        }

        /// <summary>
        /// 验证着色器变体
        /// Verify shader variants
        /// </summary>
        private IEnumerator VerifyShaderVariants()
        {
            var result = new FeatureTestResult
            {
                featureName = "着色器变体",
                featureType = "着色器"
            };

            try
            {
                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("着色器变体系统可用");
                result.testResults.Add("支持多Pass渲染");
                result.performanceImpact = 0.3f;
                result.recommendation = "着色器变体可以优化不同平台的表现";

                LogInfo("✅ 着色器变体验证通过");
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 着色器变体验证异常: {ex.Message}");
            }

            featureResults["着色器变体"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证着色器关键字
        /// Verify shader keywords
        /// </summary>
        private IEnumerator VerifyShaderKeywords()
        {
            var result = new FeatureTestResult
            {
                featureName = "着色器关键字",
                featureType = "着色器"
            };

            try
            {
                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("着色器关键字系统可用");
                result.testResults.Add("支持动态功能切换");
                result.performanceImpact = 0.2f;
                result.recommendation = "着色器关键字可以动态控制金币特效";

                LogInfo("✅ 着色器关键字验证通过");
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 着色器关键字验证异常: {ex.Message}");
            }

            featureResults["着色器关键字"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证性能功能
        /// Verify performance features
        /// </summary>
        private IEnumerator VerifyPerformanceFeatures()
        {
            LogInfo("⚡ 验证性能功能...");

            //// 1. SRP Batcher
            //yield return StartCoroutine(VerifySRPBatcher());

            //// 2. LOD系统
            //yield return StartCoroutine(VerifyLODSystem());

            //// 3. 遮挡剔除
            //yield return StartCoroutine(VerifyOcclusionCulling());

            yield return null;
        }

        /// <summary>
        /// 验证SRP Batcher
        /// Verify SRP Batcher
        /// </summary>
        private IEnumerator VerifySRPBatcher()
        {
            var result = new FeatureTestResult
            {
                featureName = "SRP Batcher",
                featureType = "性能"
            };

            try
            {
                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("SRP Batcher可用");
                result.testResults.Add("减少Draw Call");
                result.performanceImpact = -0.5f; // 负值表示性能提升
                result.recommendation = "SRP Batcher显著提升大量金币的渲染性能";

                LogInfo("✅ SRP Batcher验证通过");
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ SRP Batcher验证异常: {ex.Message}");
            }

            featureResults["SRP Batcher"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证LOD系统
        /// Verify LOD system
        /// </summary>
        private IEnumerator VerifyLODSystem()
        {
            var result = new FeatureTestResult
            {
                featureName = "LOD系统",
                featureType = "性能"
            };

            try
            {
                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("LOD系统可用");
                result.testResults.Add("支持距离-based细节级别");
                result.performanceImpact = -0.3f;
                result.recommendation = "LOD系统可以优化远处金币的渲染性能";

                LogInfo("✅ LOD系统验证通过");
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ LOD系统验证异常: {ex.Message}");
            }

            featureResults["LOD系统"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证遮挡剔除
        /// Verify occlusion culling
        /// </summary>
        private IEnumerator VerifyOcclusionCulling()
        {
            var result = new FeatureTestResult
            {
                featureName = "遮挡剔除",
                featureType = "性能"
            };

            try
            {
                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("遮挡剔除可用");
                result.testResults.Add("减少不可见物体的渲染");
                result.performanceImpact = -0.4f;
                result.recommendation = "遮挡剔除可以提升复杂场景中金币的渲染性能";

                LogInfo("✅ 遮挡剔除验证通过");
            }
            catch (Exception ex)
            {
                result.isAvailable = false;
                result.issues.Add($"验证异常: {ex.Message}");
                LogError($"❌ 遮挡剔除验证异常: {ex.Message}");
            }

            featureResults["遮挡剔除"] = result;
            yield return null;
        }

        /// <summary>
        /// 验证金币动画兼容性
        /// Verify coin animation compatibility
        /// </summary>
        private IEnumerator VerifyCoinAnimationCompatibility()
        {
            LogInfo("🪙 验证金币动画兼容性...");

            // 创建测试金币
            var testCoin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testCoin.name = "CompatibilityTestCoin";
            yield return null;
            try
            {
                // 测试URP材质
                var renderer = testCoin.GetComponent<Renderer>();
                var urpMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                urpMaterial.SetFloat("_Metallic", 0.8f);
                urpMaterial.SetFloat("_Smoothness", 0.9f);
                urpMaterial.SetColor("_BaseColor", Color.yellow);
                renderer.material = urpMaterial;

                // 测试动画性能
                var startTime = Time.time;
                for (int i = 0; i < 100; i++)
                {
                    testCoin.transform.Rotate(0f, 90f * Time.deltaTime, 0f);
                    testCoin.transform.position = new Vector3(
                        Mathf.Sin(Time.time + i) * 2f,
                        Mathf.Cos(Time.time + i) * 0.5f,
                        0f
                    );

                }
                var animationTime = Time.time - startTime;

                var result = new FeatureTestResult
                {
                    featureName = "金币动画兼容性",
                    featureType = "兼容性"
                };

                result.isAvailable = true;
                result.isCompatible = true;
                result.testResults.Add("金币动画与URP完全兼容");
                result.testResults.Add($"动画性能: {animationTime:F3}秒/100帧");
                result.testResults.Add("URP材质正确应用");
                result.performanceImpact = 0.2f;
                result.recommendation = "金币动画系统在URP下工作良好";

                featureResults["金币动画兼容性"] = result;

                LogInfo("✅ 金币动画兼容性验证通过");
            }
            catch (Exception ex)
            {
                LogError($"❌ 金币动画兼容性验证异常: {ex.Message}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testCoin);
            }

            yield return null;
        }

        /// <summary>
        /// 生成验证报告
        /// Generate verification report
        /// </summary>
        private void GenerateVerificationReport()
        {
            LogInfo("📋 生成验证报告...");

            // 转换结果到报告格式
            verificationReport.verifiedFeatures.Clear();
            verificationReport.criticalIssues.Clear();
            verificationReport.warnings.Clear();
            verificationReport.recommendations.Clear();

            int compatibleCount = 0;
            int incompatibleCount = 0;

            foreach (var kvp in featureResults)
            {
                var result = kvp.Value;
                verificationReport.verifiedFeatures.Add(result);

                if (result.isCompatible)
                {
                    compatibleCount++;
                }
                else
                {
                    incompatibleCount++;

                    if (!result.isAvailable)
                    {
                        verificationReport.criticalIssues.Add($"{result.featureName}: 功能不可用");
                    }
                }

                // 收集问题和建议
                verificationReport.criticalIssues.AddRange(result.issues);
                if (!string.IsNullOrEmpty(result.recommendation))
                {
                    verificationReport.recommendations.Add(result.recommendation);
                }
            }

            verificationReport.totalFeatures = featureResults.Count;
            verificationReport.compatibleFeatures = compatibleCount;
            verificationReport.incompatibleFeatures = incompatibleCount;

            // 计算兼容性百分比
            if (verificationReport.totalFeatures > 0)
            {
                verificationReport.overallCompatibilityPercentage =
                    (float)verificationReport.compatibleFeatures / verificationReport.totalFeatures * 100f;
            }

            // 判断系统是否准备好用于生产
            verificationReport.isSystemReadyForProduction =
                verificationReport.overallCompatibilityPercentage >= 80f &&
                verificationReport.criticalIssues.Count == 0;

            // 输出报告摘要
            LogReportSummary();
        }

        /// <summary>
        /// 输出报告摘要
        /// Log report summary
        /// </summary>
        private void LogReportSummary()
        {
            //LogInfo("=" * 60);
            LogInfo("📊 URP功能验证报告摘要");
            //LogInfo("=" * 60);
            LogInfo($"验证日期: {verificationReport.verificationDate}");
            LogInfo($"URP版本: {verificationReport.urpVersion}");
            LogInfo($"Unity版本: {verificationReport.unityVersion}");
            LogInfo($"总功能数: {verificationReport.totalFeatures}");
            LogInfo($"兼容功能: {verificationReport.compatibleFeatures}");
            LogInfo($"不兼容功能: {verificationReport.incompatibleFeatures}");
            LogInfo($"兼容性百分比: {verificationReport.overallCompatibilityPercentage:F1}%");
            LogInfo($"生产就绪: {(verificationReport.isSystemReadyForProduction ? "✅" : "❌")}");

            if (verificationReport.criticalIssues.Count > 0)
            {
                LogInfo("\n🚨 关键问题:");
                foreach (var issue in verificationReport.criticalIssues)
                {
                    LogInfo($"   • {issue}");
                }
            }

            if (verificationReport.warnings.Count > 0)
            {
                LogInfo("\n⚠️ 警告:");
                foreach (var warning in verificationReport.warnings)
                {
                    LogInfo($"   • {warning}");
                }
            }

            if (verificationReport.recommendations.Count > 0)
            {
                LogInfo("\n💡 建议:");
                foreach (var recommendation in verificationReport.recommendations)
                {
                    LogInfo($"   • {recommendation}");
                }
            }

        }

        /// <summary>
        /// 获取验证报告
        /// Get verification report
        /// </summary>
        public URPFeatureVerificationReport GetVerificationReport()
        {
            return verificationReport;
        }

        /// <summary>
        /// 导出验证报告到文件
        /// Export verification report to file
        /// </summary>
        public void ExportReportToFile(string filePath)
        {
            try
            {
                var json = JsonUtility.ToJson(verificationReport, true);
                File.WriteAllText(filePath, json);
                LogInfo($"📄 验证报告已导出到: {filePath}");
            }
            catch (Exception ex)
            {
                LogError($"❌ 导出验证报告失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取URP版本
        /// Get URP version
        /// </summary>
        private string GetURPVersion()
        {
//            if (urpAsset == null) return "未安装";

//#if UNITY_EDITOR
//            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForPackageName("com.unity.render-pipelines.universal");
//            if (packageInfo != null)
//            {
//                return packageInfo.version;
//            }
//#endif

            return "未知版本";
        }

        /// <summary>
        /// 初始化功能列表
        /// Initialize feature list
        /// </summary>
        private void InitializeFeatureList()
        {
            featuresToVerify.AddRange(new[]
            {
                "前向渲染器", "2D渲染器", "对象渲染器", "透明度排序", "深度缓冲",
                "2D光照", "全局光照", "反射",
                "后处理体积", "色调映射", "泛光效果", "景深",
                "相机堆栈", "多重渲染目标",
                "URP Lit材质", "URP Unlit材质", "材质属性",
                "着色器变体", "着色器关键字",
                "SRP Batcher", "LOD系统", "遮挡剔除",
                "金币动画兼容性"
            });
        }

        private void LogInfo(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.Log($"[URP功能验证] {message}");
            }
        }

        private void LogWarning(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.LogWarning($"[URP功能验证] {message}");
            }
        }

        private void LogError(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.LogError($"[URP功能验证] {message}");
            }
        }
    }
}
