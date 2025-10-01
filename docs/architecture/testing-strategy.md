# Testing Strategy

## Testing Pyramid

```
    Manual Playtesting
         /     \
Integration Tests   Unit Tests
```

## Test Organization

### Frontend Tests

```
Assets/Scripts/Tests/
├── UnitTests/
│   ├── BoardManagerTests.cs
│   ├── WinDetectorTests.cs
│   └── GameManagerTests.cs
└── IntegrationTests/
    ├── GameFlowTests.cs
    └── UISystemTests.cs
```

### Backend Tests

N/A for local game.

### E2E Tests

```
Assets/Scripts/Tests/
└── IntegrationTests/
    ├── FullGameplayTests.cs
    └── SettingsPersistenceTests.cs
```

## Test Examples

### Frontend Component Test

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BoardManagerTests
{
    [Test]
    public void PlacePiece_ValidPosition_PiecePlaced()
    {
        // Arrange
        var boardManager = new BoardManager();
        boardManager.Initialize(15);
        
        // Act
        bool result = boardManager.PlacePiece(7, 7, 1);
        
        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(1, boardManager.GetPieceAt(7, 7));
    }
}
```

### Backend API Test

N/A for local game.

### E2E Test

```csharp
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;

public class FullGameplayTests
{
    [UnityTest]
    public IEnumerator FullGame_WinCondition_MatchResult()
    {
        // Arrange
        // Setup game scene
        
        // Act
        // Simulate full game to win condition
        
        // Assert
        // Verify win is detected correctly
        
        yield return null;
    }
}
```
