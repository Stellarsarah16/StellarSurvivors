namespace StellarSurvivors.Core;

using Raylib_cs;

public class AnimationDefinition
{
    public string Name { get; set; }
    public string TextureName { get; set; } // The key for the AssetManager
    
    // Frame data
    public int FrameCount { get; set; }
    public float FrameDuration { get; set; } // How long (in seconds) each frame lasts
    public bool Loops { get; set; } = true;
    
    public Rectangle[] Frames { get; private set; }
    
    // --- We'll use this helper method in the AnimationManager ---
    
    /// <param name="sheetWidth">Total width of the texture</param>
    /// <param name="frameWidth">Width of a single frame</param>
    /// <param name="frameHeight">Height of a single frame</param>
    /// <param name="startX">Pixel X coordinate where the first frame starts</param>
    /// <param name="startY">Pixel Y coordinate where this animation row starts</param>
    public void GenerateFrames(int frameWidth, int frameHeight, int startX, int startY)
    {
        Frames = new Rectangle[FrameCount];
        for (int i = 0; i < FrameCount; i++)
        {
            int x = startX + (i * frameWidth);
            int y = startY;
            Frames[i] = new Rectangle(x, y, frameWidth, frameHeight);
        }
    }
}
