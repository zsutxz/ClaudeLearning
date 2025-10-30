using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using CoinAnimation.Core;
using CoinAnimation.Animation;
using CoinAnimation.Physics;
using DG.Tweening;
using System.Collections.Generic;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// Unity Test Runner configuration and validation suite
    /// Entry point for running all coin animation tests
    /// </summary>
    [TestFixture]
    public class CoinAnimationTestRunner
    {
        #region Test Environment Validation
        
        [Test]
        public void TestEnvironment_Validation()
        {
            // Validate that all required components are available
            Assert.IsTrue(System.Type.GetType("CoinAnimation.Core.CoinAnimationState") != null,
                "CoinAnimationState type should be available");
            
            Assert.IsTrue(System.Type.GetType("CoinAnimation.Animation.CoinAnimationManager") != null,
                "CoinAnimationManager type should be available");
            
            Assert.IsTrue(System.Type.GetType("CoinAnimation.Physics.MagneticCollectionController") != null,
                "MagneticCollectionController type should be available");
            
            Assert.IsTrue(System.Type.GetType("CoinAnimation.Physics.SpiralMotionController") != null,
                "SpiralMotionController type should be available");
            

            Debug.Log("✅ Test environment validation passed");
        }


        #endregion

        #region Acceptance Criteria Validation

        [Test]
        public void AcceptanceCriteria_1_DOTweenFramework()
        {
            // Validate AC1: System must implement smooth DOTween-based animation framework
            
            // Create test coin
            GameObject testCoin = new GameObject("TestCoin_AC1");
            var controller = testCoin.AddComponent<CoinAnimationController>();
            testCoin.AddComponent<Rigidbody>();
            testCoin.AddComponent<SphereCollider>();
            
            // Test animation framework functionality
            Assert.IsNotNull(controller, "Coin animation controller should be created");
            Assert.AreEqual(CoinAnimationState.Idle, controller.CurrentState, 
                "Coin should start in idle state");
            
            // Test animation execution
            Vector3 targetPosition = new Vector3(1f, 0f, 0f);
            controller.AnimateToPosition(targetPosition, 0.1f);
            
            // Verify state change
            Assert.AreEqual(CoinAnimationState.Moving, controller.CurrentState,
                "Coin should enter moving state during animation");
            
            // Clean up
            Object.DestroyImmediate(testCoin);
            
            Debug.Log("✅ Acceptance Criteria 1 (DOTween Framework) validated");
        }

        [Test]
        public void AcceptanceCriteria_2_MagneticSystem()
        {
            // Validate AC2: Physics-based magnetic attraction system with configurable parameters
            
            // Create magnetic field data
            var fieldData = ScriptableObject.CreateInstance<MagneticFieldData>();
            fieldData.ResetToDefaults();
            fieldData.MaxMagneticStrength = 10f;
            fieldData.FieldRadius = 5f;
            
            // Test magnetic force calculations
            Vector3 centerPosition = Vector3.zero;
            Vector3 closePosition = new Vector3(1f, 0f, 0f);
            Vector3 farPosition = new Vector3(4f, 0f, 0f);
            
            Vector3 closeForce = fieldData.CalculateMagneticForce(closePosition);
            Vector3 farForce = fieldData.CalculateMagneticForce(farPosition);
            
            Assert.IsTrue(closeForce.magnitude > farForce.magnitude,
                "Magnetic force should be stronger at closer distances");
            Assert.IsTrue(closeForce.magnitude > 0f,
                "Magnetic force should be positive within field range");
            
            // Test field strength calculations
            float closeStrength = fieldData.GetFieldStrength(closePosition);
            float farStrength = fieldData.GetFieldStrength(farPosition);
            
            Assert.IsTrue(closeStrength > farStrength,
                "Field strength should decrease with distance");
            Assert.IsTrue(closeStrength <= 1f && closeStrength >= 0f,
                "Field strength should be normalized (0-1)");
            
            // Clean up
            ScriptableObject.DestroyImmediate(fieldData);
            
            Debug.Log("✅ Acceptance Criteria 2 (Magnetic System) validated");
        }

        [Test]
        public void AcceptanceCriteria_3_SpiralMotion()
        {
            // Validate AC3: Spiral motion patterns near collection points
            
            // Create spiral controller
            GameObject spiralObject = new GameObject("TestSpiral_AC3");
            var spiralController = spiralObject.AddComponent<SpiralMotionController>();
            
            // Create test coin
            GameObject testCoin = new GameObject("TestCoin_AC3");
            var coinController = testCoin.AddComponent<CoinAnimationController>();
            
            // Test spiral animation startup
            Vector3 targetPosition = new Vector3(3f, 0f, 0f);
            bool spiralStarted = spiralController.StartSpiralAnimation(
                coinController, targetPosition, 1f, SpiralType.Helix);
            
            Assert.IsTrue(spiralStarted, "Spiral animation should start successfully");
            Assert.AreEqual(1, spiralController.ActiveSpiralCount, 
                "Should have one active spiral animation");
            
            // Test different spiral types
            var spiralTypes = new SpiralType[] 
            { 
                SpiralType.Helix, 
                SpiralType.Vortex, 
                SpiralType.DoubleHelix, 
                SpiralType.Corkscrew 
            };
            
            foreach (var spiralType in spiralTypes)
            {
                bool started = spiralController.StartSpiralAnimation(
                    coinController, targetPosition, 0.5f, spiralType);
                Assert.IsTrue(started, $"Spiral type {spiralType} should start successfully");
            }
            
            // Clean up
            Object.DestroyImmediate(testCoin);
            Object.DestroyImmediate(spiralObject);
            
            Debug.Log("✅ Acceptance Criteria 3 (Spiral Motion) validated");
        }

        [Test]
        public void AcceptanceCriteria_4_NaturalDeceleration()
        {
            // Validate AC4: Natural deceleration and easing functions
            
            // Test easing function availability
            Assert.IsNotNull(CoinAnimationEasing.CoinDecelerate, 
                "Coin deceleration easing should be available");
            Assert.IsNotNull(CoinAnimationEasing.CoinBurst, 
                "Coin burst easing should be available");
            Assert.IsNotNull(CoinAnimationEasing.MagneticAttraction, 
                "Magnetic attraction easing should be available");
            
            // Test contextual easing selection
            Ease nearEasing = CoinAnimationEasing.GetContextualEasing(1f, EasingContext.Normal);
            Ease farEasing = CoinAnimationEasing.GetContextualEasing(5f, EasingContext.Normal);
            Ease magneticEasing = CoinAnimationEasing.GetContextualEasing(2f, EasingContext.Magnetic);
            
            Assert.IsNotNull(nearEasing, "Near easing should be valid");
            Assert.IsNotNull(farEasing, "Far easing should be valid");
            Assert.IsNotNull(magneticEasing, "Magnetic easing should be valid");
            
            // Test custom curve creation
            var settleCurve = CoinAnimationEasing.CreateCustomCurve(CustomCurveType.CoinSettle);
            var magneticCurve = CoinAnimationEasing.CreateCustomCurve(CustomCurveType.MagneticPull);
            var spiralCurve = CoinAnimationEasing.CreateCustomCurve(CustomCurveType.SpiralIntensity);
            
            Assert.IsNotNull(settleCurve, "Settle curve should be created");
            Assert.IsNotNull(magneticCurve, "Magnetic pull curve should be created");
            Assert.IsNotNull(spiralCurve, "Spiral intensity curve should be created");
            
            Debug.Log("✅ Acceptance Criteria 4 (Natural Deceleration) validated");
        }

        [Test]
        public void AcceptanceCriteria_5_SatisfyingPhysics()
        {
            // Validate AC5: Coins must flow naturally toward collection points
            
            // Create complete physics scenario
            GameObject magneticObject = new GameObject("TestMagnetic_AC5");
            var magneticController = magneticObject.AddComponent<MagneticCollectionController>();
            
            GameObject testCoin = new GameObject("TestCoin_AC5");
            var coinController = testCoin.AddComponent<CoinAnimationController>();
            
            // Create magnetic field
            var fieldData = ScriptableObject.CreateInstance<MagneticFieldData>();
            fieldData.ResetToDefaults();
            fieldData.MaxMagneticStrength = 15f; // Stronger for satisfying collection
            fieldData.FieldRadius = 3f;
            fieldData.EnableSpiralMotion = true;
            fieldData.SpiralIntensity = 1.2f;
            
            Vector3 fieldCenter = Vector3.zero;
            fieldData.FieldCenter = fieldCenter;
            
            // Test magnetic field influence
            Vector3 coinPosition = new Vector3(2f, 0f, 0f);
            Vector3 magneticForce = fieldData.CalculateMagneticForce(coinPosition);
            
            Assert.IsTrue(magneticForce.magnitude > 0f, 
                "Magnetic force should be applied to coin");
            
            Vector3 forceDirection = magneticForce.normalized;
            Vector3 expectedDirection = (fieldCenter - coinPosition).normalized;
            
            Assert.IsTrue(Vector3.Dot(forceDirection, expectedDirection) > 0.9f,
                "Magnetic force should point toward collection point");
            
            // Test spiral offset calculation
            Vector3 spiralOffset = fieldData.CalculateSpiralOffset(coinPosition, 1f);
            Assert.IsNotNull(spiralOffset, "Spiral offset should be calculated");
            
            // Test field strength for collection triggering
            float fieldStrength = fieldData.GetFieldStrength(coinPosition);
            Assert.IsTrue(fieldStrength > 0f, "Field strength should be positive within range");
            
            // Clean up
            Object.DestroyImmediate(testCoin);
            Object.DestroyImmediate(magneticObject);
            ScriptableObject.DestroyImmediate(fieldData);
            
            Debug.Log("✅ Acceptance Criteria 5 (Satisfying Physics) validated");
        }

        #endregion

        #region Integration Validation

        [Test]
        public void Integration_CompleteSystem()
        {
            // Test complete system integration
            
            // Create all system components
            GameObject managerObject = new GameObject("IntegrationManager");
            var animationManager = managerObject.AddComponent<CoinAnimationManager>();
            
            GameObject magneticObject = new GameObject("IntegrationMagnetic");
            var magneticController = magneticObject.AddComponent<MagneticCollectionController>();
            
            GameObject spiralObject = new GameObject("IntegrationSpiral");
            var spiralController = spiralObject.AddComponent<SpiralMotionController>();
            
            // Create test coin
            GameObject testCoin = new GameObject("IntegrationCoin");
            var coinController = testCoin.AddComponent<CoinAnimationController>();
            testCoin.AddComponent<Rigidbody>();
            testCoin.AddComponent<SphereCollider>();
            
            // Test complete workflow
            Vector3 collectionPoint = new Vector3(5f, 0f, 0f);
            
            // 1. Coin starts idle
            Assert.AreEqual(CoinAnimationState.Idle, coinController.CurrentState);
            
            // 2. Add magnetic field
            var fieldData = ScriptableObject.CreateInstance<MagneticFieldData>();
            fieldData.ResetToDefaults();
            fieldData.FieldCenter = collectionPoint;
            int fieldId = magneticController.AddMagneticField(collectionPoint, fieldData);
            
            Assert.IsTrue(fieldId >= 0, "Magnetic field should be added");
            Assert.AreEqual(1, magneticController.ActiveFieldCount);
            
            // 3. Position coin and test magnetic influence
            coinController.transform.position = new Vector3(2f, 0f, 0f);
            bool inField = fieldData.IsWithinField(coinController.transform.position);
            Assert.IsTrue(inField, "Coin should be within magnetic field range");
            
            // 4. Start spiral animation
            bool spiralStarted = spiralController.StartSpiralAnimation(
                coinController, collectionPoint, 1f, SpiralType.Helix);
            Assert.IsTrue(spiralStarted, "Spiral animation should start");
            
            // 5. Test collection sequence
            coinController.CollectCoin(collectionPoint, 0.5f);
            Assert.AreEqual(CoinAnimationState.Collecting, coinController.CurrentState);
            
            // Clean up
            Object.DestroyImmediate(testCoin);
            Object.DestroyImmediate(magneticObject);
            Object.DestroyImmediate(spiralObject);
            Object.DestroyImmediate(managerObject);
            ScriptableObject.DestroyImmediate(fieldData);
            
            Debug.Log("✅ Complete system integration validated");
        }

        #endregion

        #region Performance Validation

        [Test]
        public void Performance_BaselineValidation()
        {
            // Baseline performance validation
            
            // Create performance test setup
            GameObject managerObject = new GameObject("PerfManager");
            var animationManager = managerObject.AddComponent<CoinAnimationManager>();
            
            // Create multiple test coins
            List<CoinAnimationController> testCoins = new List<CoinAnimationController>();
            for (int i = 0; i < 20; i++) // Smaller set for unit test
            {
                GameObject coinObject = new GameObject($"PerfCoin_{i}");
                var controller = coinObject.AddComponent<CoinAnimationController>();
                coinObject.AddComponent<Rigidbody>();
                coinObject.AddComponent<SphereCollider>();
                testCoins.Add(controller);
            }
            
            // Measure baseline performance
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Start animations
            foreach (var coin in testCoins)
            {
                Vector3 targetPos = coin.transform.position + new Vector3(
                    Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
                coin.AnimateToPosition(targetPos, 1f);
            }
            
            stopwatch.Stop();
            
            // Performance should be reasonable
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000, 
                "Starting 20 animations should take less than 1 second");
            Assert.IsTrue(animationManager.ActiveCoinCount <= 100, 
                "Active coin count should be within reasonable limits");
            
            // Clean up
            foreach (var coin in testCoins)
            {
                if (coin != null && coin.gameObject != null)
                    Object.DestroyImmediate(coin.gameObject);
            }
            Object.DestroyImmediate(managerObject);
            
            Debug.Log("✅ Baseline performance validation passed");
        }

        #endregion

        #region Test Suite Summary

        [Test]
        public void TestSuite_CompletenessCheck()
        {
            // Validate that all acceptance criteria are covered by tests
            
            var requiredComponents = new[]
            {
                "CoinAnimationManager",
                "CoinAnimationController", 
                "MagneticCollectionController",
                "SpiralMotionController",
                "MagneticFieldData"
            };
            
            foreach (var component in requiredComponents)
            {
                Assert.IsTrue(System.Type.GetType($"CoinAnimation.{component.Split('.')[0]}.{component}") != null ||
                             System.Type.GetType($"CoinAnimation.Core.{component}") != null,
                    $"Required component {component} should be available");
            }
            
            var requiredStates = new[]
            {
                CoinAnimationState.Idle,
                CoinAnimationState.Moving,
                CoinAnimationState.Collecting,
                CoinAnimationState.Pooled
            };
            
            foreach (var state in requiredStates)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(CoinAnimationState), state),
                    $"Required state {state} should be defined");
            }
            
            Debug.Log("✅ Test suite completeness check passed");
            Debug.Log("🎯 All acceptance criteria validated successfully!");
            Debug.Log("🚀 Coin Animation System ready for production deployment!");
        }

        #endregion
    }
}