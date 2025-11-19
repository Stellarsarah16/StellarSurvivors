namespace StellarSurvivors.Components;

using StellarSurvivors.Core; // For AnimationDefinition and Manager

/// <summary>
/// The "live state" component that goes on an entity.
/// It tracks which animation is playing and its current progress.
/// </summary>
public class AnimationComponent
{
    public AnimationDefinition CurrentAnimation { get; private set; }
    public int CurrentFrameIndex { get; set; }
    public float FrameTimer { get; set; }
    public bool IsPlaying { get; set; }

    public AnimationComponent() { IsPlaying = false; }

    /// <summary>
    /// A simple polymorphic "Play" message. This tells the
    /// component to switch its "state" to a new animation.
    /// </summary>
    public void Play(string animationName)
    {
        var newAnimation = AnimationManager.GetDefinition(animationName);
        if (newAnimation == null) return;
        
        // Don't restart an animation that's already playing
        if (CurrentAnimation == newAnimation)
        {
            IsPlaying = true; // Ensure it's playing
            return; 
        }

        // Switch to the new animation
        CurrentAnimation = newAnimation;
        CurrentFrameIndex = 0;
        FrameTimer = 0f;
        IsPlaying = true;
    }

    public void Stop()
    {
        IsPlaying = false;
    }

    public void Pause()
    {
        IsPlaying = false; 
        // Note: We don't reset the timer, so "Play" will resume.
    }
}
