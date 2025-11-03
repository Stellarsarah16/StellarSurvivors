namespace StellarSurvivors.WorldGen.Strategies;

public interface INoiseStrategy
{
    void SetSeed(int seed);
    void SetScale(float scale);
    
    float GetNoise(float x, float y);
}