// We need our utility, and we need our interface
using StellarSurvivors.WorldGen.Utilities;
using StellarSurvivors.WorldGen.Strategies; // Or whatever your INoiseStrategy namespace is
using StellarSurvivors.WorldGen; // For IGenerationStep

namespace StellarSurvivors.WorldGen.Strategies
{
    public class PerlinNoiseStrategy : INoiseStrategy
    {
        private float _scale = 0.4f;
        private int _seed = 12345;

        public void SetSeed(int seed)
        {
            _seed = seed;
            Perlin.Init(_seed);
        }

        public void SetScale(float scale)
        {
            _scale = scale;
        }
        
        public float GetNoise(float x, float y)
        {
            // We call our utility, applying the scale.
            // We cast to double for the Noise method and back to float.
            return (float)Perlin.Noise(x * _scale, y * _scale);
        }
    }
}