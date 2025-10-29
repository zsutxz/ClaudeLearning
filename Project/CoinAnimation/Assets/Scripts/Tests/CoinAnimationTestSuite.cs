using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using DG.Tweening;
using CoinAnimation.Core;
using CoinAnimation.Animation;
using CoinAnimation.Physics;

namespace CoinAnimation.Tests
{
    /// <summary>
    /// Comprehensive test suite for coin animation system
    /// Validates all acceptance criteria and performance requirements
    /// </summary>
    public class CoinAnimationTestSuite
    {
        #region Test Setup
        
        private GameObject _testGameObject;
        private CoinAnimationManager _animationManager;
        private CoinAnimationController _coinController;
        private MagneticCollectionController _magneticController;
        private SpiralMotionController _spiralController;
        private MagneticFieldData _testFieldData;

        [SetUp]
        public void SetUp()
        {
            // Initialize DOTween for testing
            DOTween.SetTweensCapacity(200, 50);
            DOTween.logBehaviour = LogBehaviour.ErrorsOnly;
            
            // Create test game objects
            CreateTestEnvironment();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test environment
            CleanupTestEnvironment();
            
            // Kill all DOTween animations
            DOTween.KillAll(false);
        }

        /// <summary>
        /// Create test environment with all required components
        /// </summary>
        private void CreateTestEnvironment()
        {
            // Create animation manager
            _testGameObject = new GameObject("TestAnimationManager");
            _animationManager = _testGameObject.AddComponent<CoinAnimationManager>();
            
            // Create test coin
            GameObject coinObject = new GameObject("TestCoin");
            coinObject.transform.position = Vector3.zero;
            _coinController = coinObject.AddComponent<CoinAnimationController>();
            
            // Add required components for coin
            Rigidbody coinRb = coinObject.AddComponent<Rigidbody>();
            coinRb.useGravity = false;
            coinRb.isKinematic = true;
            
            SphereCollider coinCollider = coinObject.AddComponent<SphereCollider>();
            coinCollider.radius = 0.5f;
            
            // Create magnetic controller
            GameObject magneticObject = new GameObject("TestMagneticController");
            _magneticController = magneticObject.AddComponent<MagneticCollectionController>();
            
            // Create spiral controller
            GameObject spiralObject = new GameObject("TestSpiralController");
            _spiralController = spiralObject.AddComponent<SpiralMotionController>();
            
            // Create test magnetic field data
            _testFieldData = ScriptableObject.CreateInstance<MagneticFieldData>();
            _testFieldData.ResetToDefaults();
            _testFieldData.SetFalloffCurve(MagneticFieldType.Gaussian);
        }

        /// <summary>
        /// Clean up test environment
        /// </summary>
        private void CleanupTestEnvironment()
        {
            if (_coinController != null && _coinController.gameObject != null)
                Object.DestroyImmediate(_coinController.gameObject);
            
            if (_animationManager != null && _animationManager.gameObject != null)
                Object.DestroyImmediate(_animationManager.gameObject);
            
            if (_magneticController != null && _magneticController.gameObject != null)
                Object.DestroyImmediate(_magneticController.gameObject);
            
            if (_spiralController != null && _spiralController.gameObject != null)
                Object.DestroyImmediate(_spiralController.gameObject);
            
            if (_testFieldData != null)
                ScriptableObject.DestroyImmediate(_testFieldData);
        }

        #endregion

        #region Acceptance Criterion 1: DOTween Animation Framework
        
        [Test]
        public void Test_AnimationManager_Initialization()
        {
            // Verify animation manager initializes correctly
            Assert.IsNotNull(_animationManager, "Animation manager should be initialized");
            Assert.AreEqual(0, _animationManager.ActiveCoinCount, "Should start with no active coins");
            Assert.IsFalse(_animationManager.IsAtCapacity, "Should not be at capacity initially");
        }

        [Test]
        public void Test_CoinAnimation_StateTransitions()
        {
            // Verify coin starts in idle state
            Assert.AreEqual(CoinAnimationState.Idle, _coinController.CurrentState, 
                "Coin should start in idle state");
            
            // Animate coin and verify state change
            Vector3 targetPosition = new Vector3(5f, 0f, 0f);
            _coinController.AnimateToPosition(targetPosition, 1f);
            
            Assert.AreEqual(CoinAnimationState.Moving, _coinController.CurrentState, 
                "Coin should enter moving state during animation");
        }

        [Test]
        public void Test_EasingFunctions_ContextualSelection()
        {
            // Test contextual easing selection
            Ease nearEasing = CoinAnimationEasing.GetContextualEasing(1f, EasingContext.Normal);
            Ease farEasing = CoinAnimationEasing.GetContextualEasing(5f, EasingContext.Normal);
            
            Assert.AreEqual(CoinAnimationEasing.CoinBurst, nearEasing, 
                "Near distances should use burst easing");
            Assert.AreEqual(CoinAnimationEasing.CoinDecelerate, farEasing, 
                "Far distances should use decelerate easing");
        }

        [UnityTest]
        public IEnumerator Test_AnimationFramework_SmoothMovement()
        {
            Vector3 startPosition = _coinController.transform.position;
            Vector3 targetPosition = new Vector3(3f, 0f, 0f);
            float duration = 1f;
            
            // Start animation
            _coinController.AnimateToPosition(targetPosition, duration, CoinAnimationEasing.CoinBurst);
            
            // Wait for animation to complete
            yield return new WaitForSeconds(duration + 0.1f);
            
            // Verify coin reached target
            Vector3 finalPosition = _coinController.transform.position;
            Assert.IsTrue(Vector3.Distance(finalPosition, targetPosition) < 0.1f, 
                "Coin should reach target position within tolerance");
            Assert.AreEqual(CoinAnimationState.Idle, _coinController.CurrentState, 
                "Coin should return to idle state after animation");
        }

        #endregion

        #region Acceptance Criterion 2: Magnetic Collection System
        
        [Test]
        public void Test_MagneticField_DataValidation()
        {
            // Test magnetic field data configuration
            _testFieldData.FieldRadius = 5f;
            _testFieldData.MaxMagneticStrength = 10f;
            _testFieldData.ValidateParameters();
            
            Assert.AreEqual(5f, _testFieldData.FieldRadius, "Field radius should be set correctly");
            Assert.AreEqual(10f, _testFieldData.MaxMagneticStrength, "Max strength should be set correctly");
        }

        [Test]
        public void Test_MagneticForce_Calculation()
        {
            Vector3 fieldCenter = Vector3.zero;
            _testFieldData.FieldCenter = fieldCenter;
            _testFieldData.FieldRadius = 5f;
            _testFieldData.MaxMagneticStrength = 10f;
            
            // Test force calculation at different distances
            Vector3 closePosition = new Vector3(1f, 0f, 0f);
            Vector3 farPosition = new Vector3(4f, 0f, 0f);
            Vector3 outsidePosition = new Vector3(6f, 0f, 0f);
            
            Vector3 closeForce = _testFieldData.CalculateMagneticForce(closePosition);
            Vector3 farForce = _testFieldData.CalculateMagneticForce(farPosition);
            Vector3 outsideForce = _testFieldData.CalculateMagneticForce(outsidePosition);
            
            Assert.IsTrue(closeForce.magnitude > farForce.magnitude, 
                "Force should be stronger at closer distances");
            Assert.AreEqual(Vector3.zero, outsideForce, 
                "Force should be zero outside field radius");
        }

        [Test]
        public void Test_MagneticController_FieldManagement()
        {
            // Add magnetic field
            Vector3 fieldPosition = Vector3.zero;
            int fieldId = _magneticController.AddMagneticField(fieldPosition, _testFieldData);
            
            Assert.IsTrue(fieldId >= 0, "Field should be added successfully");
            Assert.AreEqual(1, _magneticController.ActiveFieldCount, "Should have 1 active field");
            
            // Remove magnetic field
            _magneticController.RemoveMagneticField(fieldId);
            Assert.AreEqual(0, _magneticController.ActiveFieldCount, "Should have 0 active fields after removal");
        }

        [UnityTest]
        public IEnumerator Test_MagneticCollection_CoinAttraction()
        {
            // Set up magnetic field
            Vector3 fieldCenter = new Vector3(5f, 0f, 0f);
            int fieldId = _magneticController.AddMagneticField(fieldCenter, _testFieldData);
            
            // Position coin within field range
            _coinController.transform.position = new Vector3(2f, 0f, 0f);
            
            // Wait for physics update
            yield return new WaitForSeconds(0.1f);
            
            // Verify coin is affected by magnetic field
            int affectedCount = _magneticController.AffectedCoinCount;
            Assert.IsTrue(affectedCount > 0, "Coin should be affected by magnetic field");
            
            // Clean up
            _magneticController.RemoveMagneticField(fieldId);
        }

        #endregion

        #region Acceptance Criterion 3: Spiral Motion Patterns
        
        [Test]
        public void Test_SpiralController_CapacityManagement()
        {
            // Test spiral controller capacity
            Assert.IsFalse(_spiralController.IsAtCapacity, "Should not be at capacity initially");
            Assert.AreEqual(0, _spiralController.ActiveSpiralCount, "Should start with no active spirals");
        }

        [Test]
        public void Test_SpiralAnimation_ParameterConfiguration()
        {
            Vector3 targetPosition = new Vector3(5f, 0f, 0f);
            
            // Test different spiral types
            bool helixStarted = _spiralController.StartSpiralAnimation(
                _coinController, targetPosition, 2f, SpiralType.Helix);
            bool vortexStarted = _spiralController.StartSpiralAnimation(
                _coinController, targetPosition, 2f, SpiralType.Vortex);
            
            Assert.IsTrue(helixStarted, "Helix spiral should start successfully");
            Assert.IsTrue(vortexStarted, "Vortex spiral should start successfully");
        }

        [UnityTest]
        public IEnumerator Test_SpiralMotion_VisiblePatterns()
        {
            Vector3 startPosition = Vector3.zero;
            Vector3 targetPosition = new Vector3(5f, 0f, 0f);
            _coinController.transform.position = startPosition;
            
            // Start spiral animation
            bool spiralStarted = _spiralController.StartSpiralAnimation(
                _coinController, targetPosition, 2f, SpiralType.Helix);
            
            Assert.IsTrue(spiralStarted, "Spiral animation should start successfully");
            
            // Monitor spiral path
            List<Vector3> pathPositions = new List<Vector3>();
            float startTime = Time.time;
            float duration = 2f;
            
            while (Time.time - startTime < duration)
            {
                pathPositions.Add(_coinController.transform.position);
                yield return null;
            }
            
            // Verify spiral pattern (should have multiple unique positions)
            Assert.IsTrue(pathPositions.Count > 10, "Should record multiple path positions");
            
            // Verify coin reached target area
            Vector3 finalPosition = _coinController.transform.position;
            Assert.IsTrue(Vector3.Distance(finalPosition, targetPosition) < 0.5f, 
                "Coin should reach target area within tolerance");
        }

        [Test]
        public void Test_SpiralIntensity_DistanceScaling()
        {
            // Test distance-based intensity scaling
            Vector3 closeTarget = new Vector3(2f, 0f, 0f);
            Vector3 farTarget = new Vector3(10f, 0f, 0f);
            
            bool closeSpiral = _spiralController.StartSpiralAnimation(
                _coinController, closeTarget, 1f, SpiralType.Helix);
            bool farSpiral = _spiralController.StartSpiralAnimation(
                _coinController, farTarget, 1f, SpiralType.Helix);
            
            Assert.IsTrue(closeSpiral, "Close spiral should start successfully");
            Assert.IsTrue(farSpiral, "Far spiral should start successfully");
        }

        #endregion

        #region Acceptance Criteria 4 & 5: Natural Movement and Physics
        
        [UnityTest]
        public IEnumerator Test_NaturalDeceleration_Movement()
        {
            Vector3 startPosition = Vector3.zero;
            Vector3 targetPosition = new Vector3(5f, 0f, 0f);
            _coinController.transform.position = startPosition;
            
            // Animate with deceleration easing
            _coinController.AnimateToPosition(targetPosition, 2f, CoinAnimationEasing.CoinDecelerate);
            
            // Track movement speed over time
            List<float> speeds = new List<float>();
            Vector3 lastPosition = startPosition;
            float startTime = Time.time;
            
            while (Time.time - startTime < 2f)
            {
                Vector3 currentPosition = _coinController.transform.position;
                float speed = Vector3.Distance(currentPosition, lastPosition) / Time.deltaTime;
                speeds.Add(speed);
                lastPosition = currentPosition;
                yield return null;
            }
            
            // Verify deceleration (speed should decrease over time)
            Assert.IsTrue(speeds.Count > 10, "Should record multiple speed samples");
            
            float firstHalfAvg = 0f, secondHalfAvg = 0f;
            int halfCount = speeds.Count / 2;
            
            for (int i = 0; i < halfCount; i++)
                firstHalfAvg += speeds[i];
            firstHalfAvg /= halfCount;
            
            for (int i = halfCount; i < speeds.Count; i++)
                secondHalfAvg += speeds[i];
            secondHalfAvg /= (speeds.Count - halfCount);
            
            Assert.IsTrue(firstHalfAvg > secondHalfAvg, 
                "Speed should decrease over time (natural deceleration)");
        }

        [UnityTest]
        public IEnumerator Test_PhysicsBehavior_Satisfaction()
        {
            // Set up complete physics scenario
            Vector3 fieldCenter = new Vector3(3f, 0f, 0f);
            int fieldId = _magneticController.AddMagneticField(fieldCenter, _testFieldData);
            
            _coinController.transform.position = Vector3.zero;
            
            // Wait for magnetic influence
            yield return new WaitForSeconds(0.5f);
            
            // Verify physics-based movement
            Vector3 initialPosition = _coinController.transform.position;
            yield return new WaitForSeconds(0.5f);
            Vector3 laterPosition = _coinController.transform.position;
            
            float movementDistance = Vector3.Distance(initialPosition, laterPosition);
            Assert.IsTrue(movementDistance > 0.1f, "Coin should move under magnetic influence");
            
            // Verify satisfying collection behavior
            _coinController.CollectCoin(fieldCenter, 1f);
            yield return new WaitForSeconds(1.1f);
            
            // Coin should be collected/pooled
            Assert.AreEqual(CoinAnimationState.Pooled, _coinController.CurrentState, 
                "Coin should be pooled after collection");
            
            // Clean up
            _magneticController.RemoveMagneticField(fieldId);
        }

        #endregion

        #region Performance Tests
        
        [UnityTest]
        public IEnumerator Test_Performance_60fpsTarget()
        {
            // Create multiple coins for performance testing
            List<CoinAnimationController> testCoins = new List<CoinAnimationController>();
            int coinCount = 50;
            
            for (int i = 0; i < coinCount; i++)
            {
                GameObject coinObject = new GameObject($"TestCoin_{i}");
                coinObject.transform.position = new Vector3(i * 0.2f, 0f, 0f);
                
                var coinController = coinObject.AddComponent<CoinAnimationController>();
                Rigidbody rb = coinObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
                coinObject.AddComponent<SphereCollider>();
                
                testCoins.Add(coinController);
            }
            
            // Start animations on all coins
            float startTime = Time.time;
            float testDuration = 2f;
            int frameCount = 0;
            
            while (Time.time - startTime < testDuration)
            {
                // Animate random coins
                foreach (var coin in testCoins)
                {
                    if (Random.value < 0.1f) // 10% chance per frame
                    {
                        Vector3 targetPosition = new Vector3(
                            Random.Range(-5f, 5f),
                            Random.Range(0f, 2f),
                            Random.Range(-5f, 5f)
                        );
                        coin.AnimateToPosition(targetPosition, Random.Range(0.5f, 1.5f));
                    }
                }
                
                frameCount++;
                yield return null;
            }
            
            // Calculate FPS
            float averageFPS = frameCount / testDuration;
            
            // Verify performance target
            Assert.IsTrue(averageFPS >= 50f, 
                $"Performance should be close to 60fps target (achieved: {averageFPS:F1}fps)");
            
            // Clean up test coins
            foreach (var coin in testCoins)
            {
                if (coin != null && coin.gameObject != null)
                    Object.DestroyImmediate(coin.gameObject);
            }
        }

        #endregion

        #region Integration Tests
        
        [UnityTest]
        public IEnumerator Test_Integration_CompleteWorkflow()
        {
            // Test complete workflow: idle -> magnetic -> spiral -> collection
            
            // 1. Start with idle coin
            Assert.AreEqual(CoinAnimationState.Idle, _coinController.CurrentState);
            
            // 2. Add magnetic field
            Vector3 fieldCenter = new Vector3(5f, 0f, 0f);
            int fieldId = _magneticController.AddMagneticField(fieldCenter, _testFieldData);
            
            // 3. Position coin for magnetic influence
            _coinController.transform.position = new Vector3(2f, 0f, 0f);
            yield return new WaitForSeconds(0.2f);
            
            // 4. Start spiral animation toward field
            bool spiralStarted = _spiralController.StartSpiralAnimation(
                _coinController, fieldCenter, 1.5f, SpiralType.Helix);
            Assert.IsTrue(spiralStarted, "Spiral animation should start in magnetic field");
            
            // 5. Wait for collection
            yield return new WaitForSeconds(2f);
            
            // 6. Verify complete workflow
            Assert.AreEqual(CoinAnimationState.Pooled, _coinController.CurrentState, 
                "Coin should complete full workflow to pooled state");
            
            // Clean up
            _magneticController.RemoveMagneticField(fieldId);
        }

        #endregion
    }
}