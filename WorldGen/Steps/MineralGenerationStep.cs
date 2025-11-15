namespace StellarSurvivors.WorldGen.Steps;

using StellarSurvivors.Core;
using StellarSurvivors.Enums;

public class MineralGenerationStep :  IGenerationStep
{
    private readonly Random _random;
    private readonly double _ironChance;
    private readonly double _goldChance;
    private readonly double _coalChance;
    
    public MineralGenerationStep(Random random, double ironChance = 0.05, double goldChance = 0.01, double coalChance = 0.05)
    {
        _random = random;
        _ironChance = ironChance;
        _goldChance = goldChance;
        _coalChance = coalChance;
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

                    // --- Check for minerals in the TOP third ---
                    if (y < worldData.Height / 3) // <-- FIX #2: Corrected Y-check
                    {
                        // We check for all three minerals, stacking their chances
        
                        // Check for the RAREST mineral first (Gold)
                        if (roll < _goldChance)
                        {
                            worldData.SetTileType(x, y, TileType.Gold);
                        }
                        // <-- FIX #1: All checks are in one "else if" chain
                        else if (roll < _goldChance + _ironChance)
                        {
                            worldData.SetTileType(x, y, TileType.Iron);
                        }
                        else if (roll < _goldChance + _ironChance + _coalChance) // Assumes you have a _coalChance
                        {
                            worldData.SetTileType(x, y, TileType.Coal);
                        }
                        // If it's none of these, it stays Stone
                    }
    
                    // --- Check for minerals in the MIDDLE and BOTTOM thirds ---
                    else 
                    {
                        // Coal doesn't spawn here, so we only check for Gold and Iron
                        if (roll < _goldChance)
                        {
                            worldData.SetTileType(x, y, TileType.Gold);
                        }
                        else if (roll < _goldChance + _ironChance)
                        {
                            worldData.SetTileType(x, y, TileType.Iron);
                        }
                        // If it's neither, it stays Stone
                    }
                }
            }
        }
    }
}