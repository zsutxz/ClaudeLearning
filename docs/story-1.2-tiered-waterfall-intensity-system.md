# Story 1.2: Tiered Waterfall Intensity System

## Status
Draft

## User Story
As a game player,
I want the waterfall effects to become more impressive as I collect coins in quick succession,
so that I feel rewarded for my skill and maintain engagement.

## Story Context
This story builds upon the basic cascading coin collection implemented in Story 1.1. It adds a tiered intensity system that enhances the waterfall effects based on the player's combo performance. The system implements four tier levels (Bronze, Silver, Gold, Platinum) that provide increasingly impressive visual and audio feedback.

## Acceptance Criteria

### Functional Requirements
1. System implements four tier levels (Bronze, Silver, Gold, Platinum) based on combo count
2. Visual effects scale with tier level including particle density and screen shake intensity
3. Audio feedback intensifies with tier progression
4. UI clearly displays current combo count and tier level
5. Combo resets after configurable time window without collection

### Integration Requirements
1. Existing ComboManager integration works correctly with new tier system
2. Visual and audio systems properly respond to tier changes
3. Performance benchmarks maintained across all tier levels

### Quality Requirements
1. Code follows existing Unity and DOTween best practices
2. Implementation maintains backward compatibility with existing systems
3. Performance monitoring is integrated with existing PerformanceMonitor.cs script
4. All new components include comprehensive documentation and code comments

## Technical Notes
- **Integration Approach:** Extend existing ComboManager with tier functionality and integrate with visual/audio systems
- **Existing Pattern Reference:** Follow patterns in existing combo system implementation
- **Key Constraints:** 
  - Must maintain 60+ FPS on target mobile devices across all tier levels
  - Must integrate with existing UI canvas systems
  - Audio and visual effects must scale appropriately without performance degradation

## Dev Notes
- Implementation should build upon the existing combo system
- Tier progression should feel rewarding and satisfying to the player
- Visual effects should include particle density changes, screen shake intensity, and lighting variations
- Audio feedback should layer additional sound effects as tiers progress
- UI should clearly indicate current tier level with visual indicators
- All tier configurations should be customizable through inspector parameters
- Combo reset timing should be configurable to balance challenge and accessibility

## Tasks / Subtasks
1. Extend ComboManager with tier level tracking based on combo count
2. Implement tier progression logic (Bronze → Silver → Gold → Platinum)
3. Create visual effect scaling system that responds to tier changes
4. Implement audio feedback intensification that layers with tier progression
5. Design and implement UI elements for combo count and tier level display
6. Add configurable parameters for tier thresholds and effect intensities
7. Integrate with existing PerformanceMonitor.cs for performance tracking
8. Test tier progression across all levels with performance monitoring
9. Verify backward compatibility with existing combo system functionality
10. Document new API methods and configuration options

## Definition of Done
- [ ] Functional requirements met
- [ ] Integration requirements verified
- [ ] Existing functionality regression tested
- [ ] Code follows existing patterns and standards
- [ ] Tests pass (existing and new)
- [ ] Documentation updated if applicable
- [ ] Performance benchmarks maintained across all tier levels

## Risk Assessment
- **Primary Risk:** Performance degradation with high-tier visual and audio effects
- **Mitigation:** Implement LOD system and provide quality settings for different device capabilities
- **Rollback:** Revert to basic cascade effects without tiered intensification