namespace StellarSurvivors.WorldGen;

public interface IGenerationStep
{
    void Process(WorldData data, int seed);
}