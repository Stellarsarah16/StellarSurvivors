namespace StellarSurvivors.WorldGen.Steps;
using StellarSurvivors.WorldGen.Strategies;

public class HeightmapStep : IGenerationStep
{
    private INoiseStrategy _noiseStrategy;

    public HeightmapStep(INoiseStrategy noiseStrategy)
    {
        _noiseStrategy = noiseStrategy;
    }

    public void Process(WorldData data, int seed)
    {
        System.Console.WriteLine("Processing Heightmap...");
        data.TileMap = new int[data.Width, data.Height];
        
        _noiseStrategy.SetScale(0.015f);
        _noiseStrategy.SetSeed(seed);

        for (int y = 0; y < data.Height; y++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                float noiseValue = _noiseStrategy.GetNoise(x, y);
                int biomeId;
                
                if (noiseValue < 0.3f)
                {
                    biomeId = 2; // e.g., Water
                }
                else if (noiseValue < 0.4f)
                {
                    biomeId = 3; // e.g., Sand
                }
                else if (noiseValue < 0.7f)
                {
                    biomeId = 1; // e.g., Grass
                }
                else
                {
                    biomeId = 4; // e.g., Stone
                }

                data.TileMap[x, y] = biomeId;
            }
        }
    }
}