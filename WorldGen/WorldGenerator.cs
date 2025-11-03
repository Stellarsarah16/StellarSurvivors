namespace StellarSurvivors.WorldGen;
using System.Collections.Generic;

public class WorldGenerator
{
    // This is the "assembly line"
    private List<IGenerationStep> _steps = new List<IGenerationStep>();
    
    // This is the "Open/Closed Principle" in action.
    // We can add new steps without modifying this class.
    public void AddStep(IGenerationStep step)
    {
        _steps.Add(step);
    }

    public WorldData Generate(int width, int height,  int seed)
    {
        // 1. Create the "conveyor belt" object
        WorldData worldData = new WorldData(width, height);
        
        // 2. Pass the object down the line.
        foreach (var step in _steps)
        {
            // Each step modifies the 'worldData' object
            step.Process(worldData, seed);
        }
        
        // 3. Return the finished product
        return worldData;
    }
}
