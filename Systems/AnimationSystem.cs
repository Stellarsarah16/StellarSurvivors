namespace StellarSurvivors.Systems;

using StellarSurvivors.Components;
using StellarSurvivors.Core;

public class AnimationSystem : IUpdateSystem
{
    private EntityManager _entityManager;

    public AnimationSystem(EntityManager entityManager)
    {
        _entityManager = entityManager;
    } 
    
    public void Update(Game world, float deltaTime)
    {
        // Find all entities that have both an Animation and a Render component
        foreach (var entityId in _entityManager.Animations.Keys)
        {
            if (!_entityManager.Renderables.ContainsKey(entityId))
            {
                continue; // Skip if it can't be rendered
            }

            var anim = _entityManager.Animations[entityId];
            var ren = _entityManager.Renderables[entityId];

            // 1. Check if we should do any work
            if (!anim.IsPlaying || anim.CurrentAnimation == null) { continue; }

            // 2. Update the frame timer
            anim.FrameTimer += deltaTime;

            // 3. Check if it's time to advance the frame
            if (anim.FrameTimer >= anim.CurrentAnimation.FrameDuration)
            {
                anim.FrameTimer -= anim.CurrentAnimation.FrameDuration;
                anim.CurrentFrameIndex++;

                // 4. Check if we've reached the end of the animation
                if (anim.CurrentFrameIndex >= anim.CurrentAnimation.FrameCount)
                {
                    if (anim.CurrentAnimation.Loops)
                    {
                        anim.CurrentFrameIndex = 0; // Loop back
                    }
                    else
                    {
                        anim.CurrentFrameIndex = anim.CurrentAnimation.FrameCount - 1; // Stay on last frame
                        anim.IsPlaying = false; // Stop playing
                    }
                }
            }

            // 5. THE SYNERGY!
            // Update the RenderComponent's SourceRect to match
            // the animation's *current* frame.
            ren.SourceRect = anim.CurrentAnimation.Frames[anim.CurrentFrameIndex];
        }
    }
}
