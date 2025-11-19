using System.Numerics;

namespace StellarSurvivors.Interfaces;
using StellarSurvivors.Core;

public interface IGenerator
{
    string Name { get; }
    double ProductionPerSecond { get; set; }
    Vector2 Position { get; set; }
    
    void Update(float deltaTime, GameData state);
}
