using StellarSurvivors.WorldGen.Strategies;

namespace StellarSurvivors.WorldGen.Steps;
using static StellarSurvivors.WorldGen.Blueprints.TileIDs;

public class MineralGenerationStep :  IGenerationStep
{
    private readonly Random _random;
    private readonly double _ironChance;
    private readonly double _goldChance;
    
    public MineralGenerationStep(Random random, double ironChance = 0.05, double goldChance = 0.01)
    {
        _random = random;
        _ironChance = ironChance;
        _goldChance = goldChance;
    }
    
    public void Process(WorldData worldData, int seed)
    {
        for (int x = 0; x < worldData.Width; x++)
        {
            for (int y = 0; y < worldData.Height; y++)
            {
                
                TileType currentType = worldData.GetTileType(x, y);

                if (currentType == TileType.Stone)
                {
                    // Roll a random number between 0.0 and 1.0
                    double roll = _random.NextDouble();

                    // Check for the RAREST mineral first
                    if (roll < _goldChance)
                    {
                        // e.g., if roll is 0.005 (which is < 0.01)
                        worldData.SetTileType(x, y, TileType.Gold);
                    }
                    // Check for the next mineral. 
                    // The probabilities are "stacked".
                    else if (roll < _goldChance + _ironChance)
                    {
                        // e.g., if roll is 0.03 (which is not < 0.01, but is < 0.06)
                        worldData.SetTileType(x, y, TileType.Iron);
                    }
                }
            }
        }
    }
}