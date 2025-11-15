namespace StellarSurvivors.Interfaces;
using StellarSurvivors.Core;

public interface IGenerator
{
    string Name { get; }
    double ProductionPerSecond { get; set; }
    
    void Update(float deltaTime, GameData state);
}
