using UnityEngine;
using System.Collections;
using GomokuGame.Core;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// Performance benchmark tests for game performance across different scenarios
/// Tests frame rate stability, memory usage, and algorithm performance
/// </summary>
public class PerformanceBenchmarkTest : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null)
        {
            StartCoroutine(RunPerformanceBenchmarks());
        }
        else
        {
            Debug.LogError("PerformanceBenchmarkTest: Could not find GameManager");
        }
    }

    private IEnumerator RunPerformanceBenchmarks()
    {
        Debug.Log("Starting Performance Benchmark Tests");

        // Test 1: Frame Rate Stability
        yield return StartCoroutine(TestFrameRateStability());

        // Test 2: Memory Usage
        yield return StartCoroutine(TestMemoryUsage());

        // Test 3: Win Detection Performance
        yield return StartCoroutine(TestWinDetectionPerformance());

        // Test 4: Board Operations Performance
        yield return StartCoroutine(TestBoardOperationsPerformance());

        // Test 5: Extended Gameplay Performance
        yield return StartCoroutine(TestExtendedGameplayPerformance());

        Debug.Log("Performance Benchmark Tests Completed");
    }

    private IEnumerator TestFrameRateStability()
    {
        Debug.Log("Testing Frame Rate Stability");

        bool frameRateStable = true;

        // Test with different board sizes
        int[] testSizes = { 9, 13, 15, 19 };

        foreach (int size in testSizes)
        {
            Debug.Log($"Testing frame rate for board size {size}x{size}");

            gameManager.SetBoardSize(size);
            gameManager.StartNewGame();
            yield return new WaitForSeconds(0.1f);

            // Measure frame rate over 2 seconds
            int frameCount = 0;
            float startTime = Time.realtimeSinceStartup;
            
            while (Time.realtimeSinceStartup - startTime < 2.0f)
            {
                // Simulate game operations
                for (int i = 0; i < 10; i++)
                {
                    gameManager.CheckWin(size / 2, size / 2);
                }
                
                frameCount++;
                yield return null;
            }

            float frameRate = frameCount / 2.0f;
            bool acceptableFrameRate = frameRate >= 30.0f; // Minimum acceptable frame rate
            
            frameRateStable &= acceptableFrameRate;
            Debug.Log($"Board size {size}: Frame rate = {frameRate:F1} FPS, Acceptable = {acceptableFrameRate}");
        }

        Debug.Log($"Frame rate stability across all board sizes: {frameRateStable}");
    }

    private IEnumerator TestMemoryUsage()
    {
        UnityEngine.Debug.Log("Testing Memory Usage");

        bool memoryUsageAcceptable = true;

        // Test memory usage with different board sizes
        int[] testSizes = { 9, 13, 15, 19 };

        foreach (int size in testSizes)
        {
            // Force garbage collection for clean measurement
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            
            long initialMemory = System.GC.GetTotalMemory(false);

            gameManager.SetBoardSize(size);
            gameManager.StartNewGame();
            yield return new WaitForSeconds(0.1f);

            // Fill half the board to simulate gameplay
            int piecesPlaced = 0;
            for (int x = 0; x < size && piecesPlaced < (size * size) / 2; x += 2)
            {
                for (int y = 0; y < size && piecesPlaced < (size * size) / 2; y += 2)
                {
                    gameManager.boardManager.PlacePiece(x, y, gameManager.currentPlayer);
                    gameManager.SwitchPlayer();
                    piecesPlaced++;
                }
            }

            long finalMemory = System.GC.GetTotalMemory(false);
            long memoryUsed = finalMemory - initialMemory;

            bool memoryWithinLimit = memoryUsed < GetMemoryLimitForSize(size);
            memoryUsageAcceptable &= memoryWithinLimit;

            Debug.Log($"Board size {size}: Memory used = {memoryUsed} bytes, Within limit = {memoryWithinLimit}");
        }

        // Test memory growth during extended gameplay
        Debug.Log("Testing memory growth during extended gameplay");
        
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        long extendedInitialMemory = System.GC.GetTotalMemory(false);

        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Simulate extended gameplay with many operations
        for (int cycle = 0; cycle < 10; cycle++)
        {
            for (int i = 0; i < 5; i++)
            {
                int x = Random.Range(0, 15);
                int y = Random.Range(0, 15);
                
                if (gameManager.boardManager.IsPositionEmpty(x, y))
                {
                    gameManager.boardManager.PlacePiece(x, y, gameManager.currentPlayer);
                    gameManager.SwitchPlayer();
                    gameManager.CheckWin(x, y);
                }
            }
            yield return new WaitForSeconds(0.05f);
        }

        long extendedFinalMemory = System.GC.GetTotalMemory(false);
        long extendedMemoryGrowth = extendedFinalMemory - extendedInitialMemory;

        bool growthAcceptable = extendedMemoryGrowth < 5000000; // Less than 5MB growth
        memoryUsageAcceptable &= growthAcceptable;

        Debug.Log($"Extended gameplay memory growth: {extendedMemoryGrowth} bytes, Acceptable = {growthAcceptable}");

        Debug.Log($"All memory usage tests: {memoryUsageAcceptable}");
    }

    private IEnumerator TestWinDetectionPerformance()
    {
        Debug.Log("Testing Win Detection Performance");

        bool winDetectionFast = true;

        // Test win detection speed with different board states
        int[] testSizes = { 9, 13, 15, 19 };

        foreach (int size in testSizes)
        {
            gameManager.SetBoardSize(size);
            gameManager.StartNewGame();
            yield return new WaitForSeconds(0.1f);

            // Test empty board win detection
            Stopwatch emptyBoardTimer = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                gameManager.CheckWin(size / 2, size / 2);
            }
            emptyBoardTimer.Stop();
            
            float emptyBoardTime = emptyBoardTimer.ElapsedMilliseconds / 100.0f;
            bool emptyBoardFast = emptyBoardTime < 1.0f; // Less than 1ms per check

            // Test nearly full board win detection
            // Fill board to 80% capacity
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if ((x + y) % 5 != 0) // Leave some empty spaces
                    {
                        gameManager.boardManager.PlacePiece(x, y, 
                            (x + y) % 2 == 0 ? GameManager.Player.Black : GameManager.Player.White);
                    }
                }
            }

            Stopwatch fullBoardTimer = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                gameManager.CheckWin(size / 2, size / 2);
            }
            fullBoardTimer.Stop();
            
            float fullBoardTime = fullBoardTimer.ElapsedMilliseconds / 100.0f;
            bool fullBoardFast = fullBoardTime < 5.0f; // Less than 5ms per check on full board

            winDetectionFast &= emptyBoardFast && fullBoardFast;
            
            Debug.Log($"Board size {size}: Empty={emptyBoardTime:F3}ms, Full={fullBoardTime:F3}ms, Fast={emptyBoardFast && fullBoardFast}");
        }

        // Test worst-case scenario (large board with complex patterns)
        gameManager.SetBoardSize(19);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        // Create complex pattern that requires checking all directions
        for (int i = 0; i < 18; i++)
        {
            gameManager.boardManager.PlacePiece(i, i, GameManager.Player.Black);
            gameManager.boardManager.PlacePiece(i, 18 - i, GameManager.Player.White);
        }

        Stopwatch worstCaseTimer = Stopwatch.StartNew();
        for (int i = 0; i < 50; i++)
        {
            gameManager.CheckWin(9, 9);
        }
        worstCaseTimer.Stop();
        
        float worstCaseTime = worstCaseTimer.ElapsedMilliseconds / 50.0f;
        bool worstCaseAcceptable = worstCaseTime < 10.0f; // Less than 10ms for worst case
        
        winDetectionFast &= worstCaseAcceptable;
        Debug.Log($"Worst-case win detection: {worstCaseTime:F3}ms, Acceptable={worstCaseAcceptable}");

        Debug.Log($"All win detection performance tests: {winDetectionFast}");
    }

    private IEnumerator TestBoardOperationsPerformance()
    {
        Debug.Log("Testing Board Operations Performance");

        bool boardOperationsFast = true;

        // Test board initialization performance
        int[] testSizes = { 9, 13, 15, 19 };

        foreach (int size in testSizes)
        {
            Stopwatch initTimer = Stopwatch.StartNew();
            gameManager.SetBoardSize(size);
            gameManager.StartNewGame();
            initTimer.Stop();
            
            float initTime = initTimer.ElapsedMilliseconds;
            bool initFast = initTime < 100.0f; // Less than 100ms for initialization
            
            boardOperationsFast &= initFast;
            Debug.Log($"Board size {size}: Initialization time = {initTime}ms, Fast = {initFast}");

            // Test piece placement performance
            Stopwatch placementTimer = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                int x = i % size;
                int y = (i * 2) % size;
                
                if (gameManager.boardManager.IsPositionEmpty(x, y))
                {
                    gameManager.boardManager.PlacePiece(x, y, gameManager.currentPlayer);
                    gameManager.SwitchPlayer();
                }
            }
            placementTimer.Stop();
            
            float placementTime = placementTimer.ElapsedMilliseconds / 100.0f;
            bool placementFast = placementTime < 1.0f; // Less than 1ms per placement
            
            boardOperationsFast &= placementFast;
            Debug.Log($"Board size {size}: Placement time = {placementTime:F3}ms, Fast = {placementFast}");

            // Test board clearing performance
            Stopwatch clearTimer = Stopwatch.StartNew();
            gameManager.boardManager.ClearBoard();
        }

        Debug.Log($"All board operations performance tests: {boardOperationsFast}");
        yield return null;
    }

    private IEnumerator TestExtendedGameplayPerformance()
    {
        Debug.Log("Testing Extended Gameplay Performance");

        bool extendedPerformanceGood = true;

        // Simulate extended gameplay session
        gameManager.SetBoardSize(15);
        gameManager.StartNewGame();
        yield return new WaitForSeconds(0.1f);

        int totalOperations = 0;
        Stopwatch extendedTimer = Stopwatch.StartNew();

        // Simulate 30 seconds of gameplay
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < 5.0f) // Reduced to 5 seconds for testing
        {
            // Perform various game operations
            for (int i = 0; i < 10; i++)
            {
                int x = Random.Range(0, 15);
                int y = Random.Range(0, 15);
                
                if (gameManager.boardManager.IsPositionEmpty(x, y))
                {
                    // Place piece
                    gameManager.boardManager.PlacePiece(x, y, gameManager.currentPlayer);
                    
                    // Check win
                    gameManager.CheckWin(x, y);
                    
                    // Switch player
                    gameManager.SwitchPlayer();
                    
                    totalOperations++;
                }
            }
            
            yield return null;
        }

        extendedTimer.Stop();
        
        float operationsPerSecond = totalOperations / 5.0f;
        bool operationsRateGood = operationsPerSecond >= 10.0f; // At least 10 operations per second
        
        extendedPerformanceGood &= operationsRateGood;
        Debug.Log($"Extended gameplay: {totalOperations} operations in 5s, Rate = {operationsPerSecond:F1} ops/s, Good = {operationsRateGood}");

        // Check memory after extended gameplay
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        long finalMemory = System.GC.GetTotalMemory(false);
        
        bool memoryStable = finalMemory < 100000000; // Less than 100MB total
        extendedPerformanceGood &= memoryStable;
        Debug.Log($"Memory after extended gameplay: {finalMemory} bytes, Stable = {memoryStable}");

        Debug.Log($"Extended gameplay performance: {extendedPerformanceGood}");
    }

    private long GetMemoryLimitForSize(int size)
    {
        // Define reasonable memory limits based on board size
        switch (size)
        {
            case 9: return 1000000;  // 1MB
            case 13: return 2000000; // 2MB
            case 15: return 3000000; // 3MB
            case 19: return 5000000; // 5MB
            default: return 10000000; // 10MB
        }
    }
}