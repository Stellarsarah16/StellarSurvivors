namespace StellarSurvivors.WorldGen.Steps;
using StellarSurvivors.WorldGen.Strategies;

public class SurfaceGenerationStep : IGenerationStep
{
    private INoiseStrategy _noiseStrategy;

    public SurfaceGenerationStep(INoiseStrategy noiseStrategy)
    {
        _noiseStrategy = noiseStrategy;
    }
    
    public void Process(WorldData worldData, int seed)
    {
        _noiseStrategy.SetSeed(seed);
        _noiseStrategy.SetScale(0.04f);
    
        // --- Define your world's "shape" ---
        int minSurfaceHeight = 50; // How low valleys can go (e.g., y=50)
        int maxSurfaceHeight = 70; // How high hills can go (e.g., y=70)
        // (Assuming y=0 is TOP of screen, y=100 is BOTTOM)
        // ---------------------------------

        for (int x = 0; x < worldData.Width; x++)
        {
            // 1. Get 1D Perlin noise, value from 0.0 to 1.0
            float noise = _noiseStrategy.GetNoise(x, 0); // y=0 is fine, it's just a 1D slice

            // 2. Map the noise to our desired height range
            int range = maxSurfaceHeight - minSurfaceHeight;
            int surfaceY = minSurfaceHeight + (int)(noise * range);
        
            // 3. Store this Y-value
            worldData.SurfaceHeightMap[x] = surfaceY;
        }
    }
}