namespace StellarSurvivors.Core;

using System.Collections.Generic;
using Raylib_cs;

public static class AnimationManager
{
    private static Dictionary<string, AnimationDefinition> _definitions = new Dictionary<string, AnimationDefinition>();
    
    public static void InitializeAnimations()
    {
        Raylib.TraceLog(TraceLogLevel.Info, "[AnimationManager] Initializing animation definitions...");
        
        // In a real game, you would load this from a .json file.
        // For now, we'll hardcode them.
        
        // --- Spaceman Animations ---

        Texture2D spacemanSheet = AssetManager.GetTexture("spaceman_sheet");
        int frameW = 43;
        int frameH = 43;
        
        // Idle
        var spacemanIdle = new AnimationDefinition
        {
            Name = "spaceman_idle",
            TextureName = "spacemanSheet",
            FrameCount = 1,
            FrameDuration = 0.2f,
            Loops = true
        };
        // This animation starts at (0, 0) on the sheet
        spacemanIdle.GenerateFrames(frameW, frameH, 0, 0); 
        _definitions.Add(spacemanIdle.Name, spacemanIdle);
        
        // Walk Animation
        var spacemanWalk = new AnimationDefinition
        {
            Name = "spaceman_walk",
            TextureName = "spacemanSheet",
            FrameCount = 8,
            FrameDuration = 0.1f, 
            Loops = true
        };
        // This animation starts at (0, 0) on the sheet
        spacemanWalk.GenerateFrames(frameW, frameH, 0, frameH * 2); 
        _definitions.Add(spacemanWalk.Name, spacemanWalk);
        
        var spacemanThrust = new AnimationDefinition
        {
            Name = "spaceman_thrust",
            TextureName = "spacemanSheet",
            FrameCount = 3,
            FrameDuration = 0.2f, 
            Loops = false
        };

        spacemanThrust.GenerateFrames(frameW, frameH, 0, frameH * 3); 
        _definitions.Add(spacemanThrust.Name, spacemanThrust);
        
        var spacemanFall = new AnimationDefinition
        {
            Name = "spaceman_fall",
            TextureName = "spacemanSheet",
            FrameCount = 1,
            FrameDuration = 0.2f, 
            Loops = false
        };

        spacemanFall.GenerateFrames(frameW, frameH, 0, frameH * 4); 
        _definitions.Add(spacemanFall.Name, spacemanFall);

    }

    public static AnimationDefinition GetDefinition(string name)
    {
        if (_definitions.TryGetValue(name, out var definition))
        {
            return definition;
        }
        Raylib.TraceLog(TraceLogLevel.Error, $"[AnimationManager] Could not find definition for: {name}");
        return null;
    }
}
