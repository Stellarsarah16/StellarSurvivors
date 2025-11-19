namespace StellarSurvivors.Core;
using Raylib_cs;
using System.Drawing;

public class WorldInteractionState
{
    public Point? SelectedTile { get; set; } = null;
    public Point? HoveredTile { get; set; } = null;

    // Visuals
    public Raylib_cs.Color HoveredColor { get; set; } = Raylib_cs.Color.White;
    public Raylib_cs.Color HoveredRedColor { get; set; } = Raylib_cs.Color.Red;
    public Raylib_cs.Color DefaultHoverColor { get; set; } = Raylib_cs.Color.White;
        
    public float FlashDuration { get; set; } = 1.0f;
    public float FlashTimeRemaining { get; private set; } = 0.0f;

    public void StartFlash()
    {
        FlashTimeRemaining = FlashDuration;
    }

    public void UpdateFlashTimer(float deltaTime)
    {
        if (!HoveredTile.HasValue) return;

        if (FlashTimeRemaining > 0)
        {
            FlashTimeRemaining -= deltaTime;
            HoveredColor = FlashTimeRemaining > 0 ? HoveredRedColor : DefaultHoverColor;
        }
    }
}