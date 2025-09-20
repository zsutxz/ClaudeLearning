# Story 1.1: Basic Cascading Coin Collection

## Status
Ready for development

## User Story
As a game player,
I want coins to cascade toward my collection point when I gather them,
so that I feel a sense of flow and continuity in my gameplay.

## Story Context
This story is the first implementation step in adding waterfall effects to the existing coin collection system. It focuses on implementing the basic cascading effect where collecting one coin triggers a ripple effect that pulls nearby coins toward the collection point.

## Acceptance Criteria

### Functional Requirements
1. Collecting a coin triggers a cascade effect that pulls nearby coins toward the collection point
2. Cascade follows a delayed activation sequence with ripple effect
3. Coin movement uses curved paths rather than straight lines
4. Effect works with existing object pooling system
5. Performance remains stable with 10+ simultaneous cascades

### Integration Requirements
1. Existing coin collection functionality remains intact
2. CoinPoolManager continues to function correctly with cascade effects
3. Frame rate remains stable with cascade implementation

### Quality Requirements
1. Code follows existing Unity and DOTween best practices
2. Implementation maintains backward compatibility with existing CoinAnimationSystem
3. Performance monitoring is integrated with existing PerformanceMonitor.cs script

## Technical Notes
- **Integration Approach:** Extend existing CoinAnimationSystem with cascade functionality
- **Existing Pattern Reference:** Follow patterns in Coin.cs, CoinPoolManager.cs, CoinAnimationSystem.cs
- **Key Constraints:** 
  - Must use existing DOTween implementation
  - Must maintain 60+ FPS on target mobile devices
  - Must integrate with existing object pooling system

## Dev Notes
- Implementation should build upon the existing DOTween integration for coin animations
- Curved paths should mimic waterfall flow with varying heights and angles
- Ripple effect timing should create a natural cascading sequence
- All new components should follow existing naming conventions (PascalCase for classes, camelCase for variables)
- Performance should be monitored using the existing PerformanceMonitor.cs script

## Tasks / Subtasks
1. Extend CoinAnimationSystem with cascade trigger method
2. Implement cascade effect logic with delayed activation sequence
3. Create curved path animations using DOTween for natural waterfall flow
4. Integrate with existing CoinPoolManager for object pooling
5. Add performance monitoring hooks to PerformanceMonitor.cs
6. Test backward compatibility with existing coin collection functionality
7. Verify frame rate stability with multiple simultaneous cascades
8. Document new API methods and configuration options

## Definition of Done
- [ ] Functional requirements met
- [ ] Integration requirements verified
- [ ] Existing functionality regression tested
- [ ] Code follows existing patterns and standards
- [ ] Tests pass (existing and new)
- [ ] Documentation updated if applicable

## Risk Assessment
- **Primary Risk:** Performance degradation with complex cascade animations
- **Mitigation:** Implement efficient DOTween sequences and use existing object pooling
- **Rollback:** Disable cascade effects through configuration without affecting core functionality