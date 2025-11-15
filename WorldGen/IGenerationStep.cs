namespace StellarSurvivors.WorldGen;
using StellarSurvivors.Core;

public interface IGenerationStep
{
    void Process(WorldData data, int seed);
}